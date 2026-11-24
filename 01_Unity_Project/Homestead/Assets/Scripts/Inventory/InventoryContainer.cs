using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemStack
{
    public string itemId;
    public int quantity;

    // The batch's stamp. Perishable food: its freshness stamp, in hours on the freshness clock of where it is (SpoilageRules,
    // Food_Spoilage_and_Preservation.md; before that system it was the in-game day it was acquired). Wood: the day it was last
    // made wet (WoodMoisture). Items that track a batch only stack with others of the same stamp, so each batch ages separately.
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

    // Fired for items entering or leaving through Add/Remove/MoveTo (not Clear or LoadData),
    // e.g. for pickup/drop sounds. Arguments: item id, quantity.
    public event Action<string, int> ItemsAdded;
    public event Action<string, int> ItemsRemoved;

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

    // Tanning slice T4 (Mike, 2026-11-23: "Log becomes Peeled Log"): a Peeled Log counts as a Log wherever Logs are counted or taken from a container,
    // plain Logs first. Rows, drops and moves of the Peeled Log itself go by its own id as usual.
    public const string LogsId = "logs";
    public const string PeeledLogId = "peeled_log";
    // Tanning slice T5b (Mike, 2026-11-24): Rawhide Lashing counts as Cordage the same way, plain Cordage first.
    public const string CordageId = "cordage";
    public const string LashingId = "rawhide_lashing";

    // The item that stands in for this one when it runs short, or null.
    public static string StandInFor(string itemId) => itemId == LogsId ? PeeledLogId : itemId == CordageId ? LashingId : null;

    public int Count(string itemId)
    {
        string standIn = StandInFor(itemId);
        int total = standIn != null ? Count(standIn) : 0;
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

    // The batches held of one item as (acquiredDay, quantity), oldest first: the order Remove and MoveTo take them in.
    public List<(int day, int quantity)> Batches(string itemId)
    {
        var batches = new List<(int day, int quantity)>();
        foreach (ItemStack stack in stacks)
            if (stack.itemId == itemId && stack.quantity > 0)
                batches.Add((stack.acquiredDay, stack.quantity));
        batches.Sort((a, b) => a.day.CompareTo(b.day));
        return batches;
    }

    // Gives the stacks of the items picked out by which a new batch day (map takes the old one), merging stacks of one item that now share
    // a day. Wood marked wet or dry, or seasoned a season on, by the testing keys (WoodMoisture); nothing in play uses it.
    public void MapBatchDays(Func<string, bool> which, Func<int, int> map)
    {
        foreach (ItemStack stack in stacks)
            if (which(stack.itemId))
                stack.acquiredDay = map(stack.acquiredDay);
        for (int i = stacks.Count - 1; i > 0; i--)
        {
            ItemDefinition item = ItemDatabase.Get(stacks[i].itemId);
            if (item == null || !item.TracksBatchDay)
                continue;
            for (int j = 0; j < i; j++)
            {
                if (stacks[j].itemId == stacks[i].itemId && stacks[j].acquiredDay == stacks[i].acquiredDay)
                {
                    stacks[j].quantity += stacks[i].quantity;
                    stacks.RemoveAt(i);
                    break;
                }
            }
        }
        Changed?.Invoke();
    }

    // Spoilage (Food_Spoilage_and_Preservation.md): every stack the test says has spoiled becomes Rot, one for each, kept even past the
    // weight limit (nothing just vanishes). Returns what spoiled as (item id, quantity); fires Changed once when anything did.
    public List<(string itemId, int quantity)> ReplaceSpoiled(Func<ItemDefinition, int, bool> spoiled, ItemDefinition rot)
    {
        var lost = new List<(string itemId, int quantity)>();
        if (spoiled == null || rot == null)
            return lost;
        int rotCount = 0;
        for (int i = stacks.Count - 1; i >= 0; i--)
        {
            ItemDefinition item = ItemDatabase.Get(stacks[i].itemId);
            if (item == null || !item.IsPerishable || stacks[i].quantity <= 0 || !spoiled(item, stacks[i].acquiredDay))
                continue;
            lost.Add((stacks[i].itemId, stacks[i].quantity));
            rotCount += stacks[i].quantity;
            stacks.RemoveAt(i);
        }
        if (rotCount > 0)
        {
            AddStack(rot, rotCount, 0);
            Changed?.Invoke();
        }
        return lost;
    }

    // Gives every perishable stack a new stamp (map gets the item and its old stamp), merging stacks of one item that now share one.
    // Loading a save from before spoilage turns its acquired days into freshness stamps this way (SpoilageManager).
    public void RestampPerishables(Func<ItemDefinition, int, int> map)
    {
        foreach (ItemStack stack in stacks)
        {
            ItemDefinition item = ItemDatabase.Get(stack.itemId);
            if (item != null && item.IsPerishable)
                stack.acquiredDay = map(item, stack.acquiredDay);
        }
        MapBatchDays(_ => false, d => d); // merges stacks that now share a stamp, and fires Changed
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
        ItemsAdded?.Invoke(item.Id, added);
        return added;
    }

    // Removes up to quantity, oldest stacks first, and returns how many were removed.
    public int Remove(string itemId, int quantity)
    {
        int removed = TakeOldest(itemId, quantity, null);
        if (removed > 0)
        {
            Changed?.Invoke();
            ItemsRemoved?.Invoke(itemId, removed);
        }
        string standIn = StandInFor(itemId);
        if (standIn != null && removed < quantity)
            removed += Remove(standIn, quantity - removed);
        return removed;
    }

    // As Remove, but reports each batch taken as (acquiredDay, quantity) so the caller can keep the batch's age —
    // used by dropping, so a dropped perishable doesn't come back fresh.
    public int RemoveBatches(string itemId, int quantity, Action<int, int> onTaken)
    {
        int removed = TakeOldest(itemId, quantity, onTaken);
        if (removed > 0)
        {
            Changed?.Invoke();
            ItemsRemoved?.Invoke(itemId, removed);
        }
        string standIn = StandInFor(itemId);
        if (standIn != null && removed < quantity)
            removed += RemoveBatches(standIn, quantity - removed, onTaken);
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
        ItemsRemoved?.Invoke(itemId, toMove);
        target.ItemsAdded?.Invoke(itemId, toMove);
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
            if (stack.itemId == item.Id && (!item.TracksBatchDay || stack.acquiredDay == acquiredDay))
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
