using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemStack
{
    public string itemId;
    public int quantity;

    // In-game day (TimeManager.TotalDays) the stack was acquired. Perishable items only stack with others
    // from the same day, so the Spoilage System can age each batch separately.
    public int acquiredDay;
}

[Serializable]
public class InventoryContainerData
{
    public string id;
    public float maxWeightKg;
    public List<ItemStack> stacks = new List<ItemStack>();
}

// A weight-limited set of item stacks: the player's on-person inventory or one Home Storage structure.
public class InventoryContainer
{
    readonly List<ItemStack> stacks = new List<ItemStack>();

    public InventoryContainer(string id, float maxWeightKg)
    {
        Id = id;
        MaxWeightKg = maxWeightKg;
    }

    public event Action Changed;

    public string Id { get; }

    // Hard limit: items that would push the total past this can't be added.
    public float MaxWeightKg { get; set; }

    public IReadOnlyList<ItemStack> Stacks => stacks;

    public float WeightKg
    {
        get
        {
            float total = 0f;
            foreach (ItemStack stack in stacks)
            {
                ItemDefinition item = ItemDatabase.Get(stack.itemId);
                if (item != null)
                    total += item.WeightKg * stack.quantity;
            }
            return total;
        }
    }

    public float FreeWeightKg => Mathf.Max(0f, MaxWeightKg - WeightKg);

    public int Count(string itemId)
    {
        int total = 0;
        foreach (ItemStack stack in stacks)
        {
            if (stack.itemId == itemId)
                total += stack.quantity;
        }
        return total;
    }

    public bool Has(string itemId, int quantity = 1) => Count(itemId) >= quantity;

    // How many of this item still fit under the weight limit.
    public int SpaceFor(ItemDefinition item)
    {
        if (item.WeightKg <= 0f)
            return int.MaxValue;

        // Small tolerance so float error doesn't reject an item that exactly fits.
        return Mathf.Max(0, Mathf.FloorToInt((MaxWeightKg - WeightKg) / item.WeightKg + 0.0001f));
    }

    // Adds as many as fit and returns how many were added.
    public int Add(ItemDefinition item, int quantity, int acquiredDay)
    {
        if (item == null || quantity <= 0)
            return 0;

        int added = Mathf.Min(quantity, SpaceFor(item));
        if (added <= 0)
            return 0;

        AddStack(item, added, acquiredDay);
        Changed?.Invoke();
        return added;
    }

    // Removes up to quantity, oldest stacks first, and returns how many were removed.
    public int Remove(string itemId, int quantity)
    {
        int removed = TakeOldest(itemId, quantity, null);
        if (removed > 0)
            Changed?.Invoke();
        return removed;
    }

    // Moves up to quantity into another container, keeping each batch's acquired day.
    // Returns how many moved (limited by what this holds and what the target can take).
    public int MoveTo(InventoryContainer target, string itemId, int quantity)
    {
        ItemDefinition item = ItemDatabase.Get(itemId);
        if (item == null || target == null || target == this || quantity <= 0)
            return 0;

        int toMove = Mathf.Min(quantity, Count(itemId), target.SpaceFor(item));
        if (toMove <= 0)
            return 0;

        TakeOldest(itemId, toMove, (day, amount) => target.AddStack(item, amount, day));
        Changed?.Invoke();
        target.Changed?.Invoke();
        return toMove;
    }

    public void Clear()
    {
        if (stacks.Count == 0)
            return;

        stacks.Clear();
        Changed?.Invoke();
    }

    public InventoryContainerData ToData()
    {
        var data = new InventoryContainerData { id = Id, maxWeightKg = MaxWeightKg };
        foreach (ItemStack stack in stacks)
            data.stacks.Add(new ItemStack { itemId = stack.itemId, quantity = stack.quantity, acquiredDay = stack.acquiredDay });
        return data;
    }

    // Replaces the contents. Stacks for items that no longer exist are dropped with a warning.
    // The weight limit isn't enforced here, so a save made under different limits still loads intact.
    public void LoadData(InventoryContainerData data)
    {
        stacks.Clear();
        foreach (ItemStack stack in data.stacks)
        {
            if (stack.quantity <= 0)
                continue;

            if (ItemDatabase.Get(stack.itemId) == null)
            {
                Debug.LogWarning($"[Inventory] Dropping {stack.quantity} x unknown item '{stack.itemId}' from '{Id}'.");
                continue;
            }

            stacks.Add(new ItemStack { itemId = stack.itemId, quantity = stack.quantity, acquiredDay = stack.acquiredDay });
        }
        Changed?.Invoke();
    }

    void AddStack(ItemDefinition item, int quantity, int acquiredDay)
    {
        foreach (ItemStack stack in stacks)
        {
            if (stack.itemId == item.Id && (!item.IsPerishable || stack.acquiredDay == acquiredDay))
            {
                stack.quantity += quantity;
                return;
            }
        }

        stacks.Add(new ItemStack { itemId = item.Id, quantity = quantity, acquiredDay = acquiredDay });
    }

    // Takes from the oldest batches first (so older food gets used before it spoils), reporting each batch taken.
    int TakeOldest(string itemId, int quantity, Action<int, int> onTaken)
    {
        int remaining = quantity;
        while (remaining > 0)
        {
            int index = -1;
            for (int i = 0; i < stacks.Count; i++)
            {
                if (stacks[i].itemId == itemId && (index < 0 || stacks[i].acquiredDay < stacks[index].acquiredDay))
                    index = i;
            }

            if (index < 0)
                break;

            ItemStack stack = stacks[index];
            int taken = Mathf.Min(remaining, stack.quantity);
            stack.quantity -= taken;
            remaining -= taken;
            onTaken?.Invoke(stack.acquiredDay, taken);

            if (stack.quantity <= 0)
                stacks.RemoveAt(index);
        }

        return quantity - remaining;
    }
}
