using System.Collections.Generic;
using UnityEngine;

// A pile or container in the World (WoodManager owns the state): what a felled tree left at its base, or storage the
// player built — Wood_Gathering_System.md's Wood Pile and Rock Pile, Primitive_Storage_System.md's Water Barrel, Food
// Cache, Storage Bin and Tool Rack. Looking at it shows what's in it. Perishable food keeps the day it was acquired
// while stored. With the Axe, swinging at a wood pile splits its Logs (then its Branches) into Firewood where they lie
// (AxeTool). Drawn from simple shapes; built storage shows its frame even when empty.
//
// A felled tree's pile and a Small Cabin's staged CabinSite are quick, incidental piles visited often while gathering:
// E takes everything that fits in one go, lightest first (Logs last), and R stores everything the pile accepts in one
// go — pouring water into the barrel from the Bucket aside, unchanged, bulk, same as always. Every deliberate,
// player-built storage kind (Wood Pile, Rock Pile, Water Barrel, Food Cache, Storage Bin, Tool Rack) instead opens
// StorageTransferScreen on E and on R (Primitive_Storage_System.md's Two-Way Transfer Screen, 2026-10-04 — the take
// side shipped 2026-10-02, the store side the day Mike's playtest showed he wanted it selectable too): the player's
// pack on the left, the structure on the right, click moves one and Shift-click the stack. The old bulk behaviors stay
// there as shortcut buttons — Store All Eligible, Take Everything, and the Water Barrel's "fill from the best quality".
public class WoodPile : MonoBehaviour, IInteractable, ISecondaryInteractable
{
    WoodPileState state;
    Material bark;
    readonly List<GameObject> pieces = new List<GameObject>();
    BoxCollider box;

    public WoodPileState State => state;

    public void Bind(WoodPileState pileState, Material barkMaterial)
    {
        state = pileState;
        bark = barkMaterial;
        box = gameObject.AddComponent<BoxCollider>();
        Rebuild();
    }

    string Name => WoodManager.PileName(state.kind);
    bool IsBarrel => state.kind == PileKind.WaterBarrel;
    static InventoryManager Inventory => InventoryManager.Instance;

    // The deliberate, player-built storage kinds — everything except a felled tree's incidental pile and a Small
    // Cabin's staged CabinSite — get the per-item transfer screen on E instead of an instant take-all.
    bool UsesTransferScreen => state.kind == PileKind.WoodStorage || state.kind == PileKind.RockStorage ||
                                state.kind == PileKind.WaterBarrel || state.kind == PileKind.FoodCache ||
                                state.kind == PileKind.StorageBin || state.kind == PileKind.ToolRack;

    // --- Taking (E) ---

    public string InteractionPrompt
    {
        get
        {
            if (state == null)
                return "";
            if (UsesTransferScreen)
                return $"Open {Name}  ({Describe(state)})";
            if (state.IsEmpty)
                return IsCabinSite ? WithSiteNeeds($"Remove {Name}  (empty — nothing to take back)") : $"{Name}  (empty)";
            string contents = Describe(state);
            string verb = state.kind == PileKind.CabinSite ? "Take Materials" : "Take Wood";
            return WithSiteNeeds(AnythingFits() ? $"{verb}  ({contents})" : $"{Name}  ({contents}) — no room to carry more");
        }
    }

    // A Small Cabin site's look prompt gets a second line with every material still short, quantity first, or the next
    // step once it is complete (Mike's playtest, 2026-10-09: he could not tell what a partly stocked site was missing).
    string WithSiteNeeds(string prompt)
    {
        WoodManager wood = WoodManager.Instance;
        if (!IsCabinSite || wood == null)
            return prompt;
        string needs = wood.SiteNeeds(state);
        string done = WoodManager.HammerEquipped ? "Fully stocked - open Inventory and press Complete Small Cabin"
                                                 : "Fully stocked - equip the Hammer, open Inventory and press Complete Small Cabin";
        return prompt + "\n" + (needs != null ? $"Still needs: {needs}" : done);
    }

    // Always true so the contents show when looked at.
    public bool CanInteract(PlayerController player) => state != null && Inventory != null;

    public void Interact(PlayerController player)
    {
        if (state == null || Inventory == null)
            return;

        // Opens even when empty now — there may be something to put in.
        if (UsesTransferScreen)
        {
            OpenScreen();
            return;
        }

        // An empty site has nothing to take, so E takes the site itself down (a stocked one is emptied first, then this).
        if (state.IsEmpty)
        {
            if (IsCabinSite)
                ToolStatus.Flash(RemoveStructure());
            return;
        }

        int taken = TakeAll();
        if (taken == 0)
        {
            ToolStatus.Flash("Too heavy — no room to carry any of it");
            return;
        }
        if (!state.IsEmpty)
            ToolStatus.Flash($"Left behind: {Describe(state)}");
        if (WoodManager.Instance != null)
            WoodManager.Instance.NotifyChanged(state);
    }

    // Everything that fits, lightest first, each batch keeping its day.
    int TakeAll()
    {
        var ids = new List<string>();
        foreach (WoodStack stack in state.contents)
            if (!ids.Contains(stack.itemId))
                ids.Add(stack.itemId);
        ids.Sort((a, b) => WeightOf(a).CompareTo(WeightOf(b)));

        int taken = 0;
        foreach (string itemId in ids)
            taken += TakeInto(itemId, state.Count(itemId));
        return taken;
    }

    // --- StorageTransferScreen's per-item take, and its two "old behavior" shortcut buttons ---

    // One unit of a kind stored, or (all) the whole stack — StorageTransferScreen's Take/Shift-click. The barrel
    // draws against Bucket/Canteen room instead of carry weight, same rule DrawWater always used.
    public int TakeOne(string itemId, bool all)
    {
        if (state == null || Inventory == null)
            return 0;
        int available = IsBarrel ? Mathf.Min(BucketRoom(), state.Count(itemId)) : state.Count(itemId);
        int want = all ? available : Mathf.Min(1, available);
        int taken = TakeInto(itemId, want);
        if (taken > 0 && WoodManager.Instance != null)
            WoodManager.Instance.NotifyChanged(state);
        return taken;
    }

    // Whether TakeOne(itemId, ...) could take anything right now, and why not if not.
    public bool CanTakeOne(string itemId, out string reason)
    {
        reason = "";
        if (state == null || Inventory == null)
            return false;
        if (IsBarrel)
        {
            if (BucketRoom() > 0)
                return true;
            reason = WaterQualities.Capacity(Inventory.Player) > 0 ? "water containers full" : "needs a Bucket or Canteen";
            return false;
        }
        ItemDefinition item = ItemDatabase.Get(itemId);
        if (item != null && Inventory.Player.SpaceFor(item) > 0)
            return true;
        reason = "too heavy to carry more";
        return false;
    }

    // The old one-press "take everything"/"fill from the best quality" — now a shortcut inside the transfer screen.
    public string TakeAllToStatus()
    {
        int taken = TakeAll();
        if (taken > 0 && WoodManager.Instance != null)
            WoodManager.Instance.NotifyChanged(state);
        return taken == 0 ? "Too heavy — no room to carry any of it."
             : state.IsEmpty ? "Took everything."
             : $"Took what fit. Left behind: {Describe(state)}";
    }

    public string DrawWaterToStatus()
    {
        int drawn = DrawWater();
        if (drawn > 0 && WoodManager.Instance != null)
            WoodManager.Instance.NotifyChanged(state);
        return drawn > 0 ? $"Filled your Bucket with {drawn} L."
             : WaterQualities.Capacity(Inventory.Player) > 0 ? "Your water containers are full." : "Needs a Bucket or Canteen to carry water.";
    }

    int TakeInto(string itemId, int count)
    {
        ItemDefinition item = ItemDatabase.Get(itemId);
        if (item == null)
            return 0;
        int fits = Mathf.Min(count, Inventory.Player.SpaceFor(item));
        int added = 0;
        foreach (WoodStack batch in state.Take(itemId, fits))
            added += item.IsPerishable ? Inventory.AddToPlayer(itemId, batch.count, batch.day) : Inventory.AddToPlayer(itemId, batch.count);
        return added;
    }

    // The Water Barrel fills the carried Buckets: purified water first, then the best raw water.
    int DrawWater()
    {
        int room = BucketRoom();
        if (room <= 0)
            return 0;
        int drawn = 0;
        string[] order = { Cooking.PurifiedWaterId, "water_excellent", "water_good", "water_questionable", "water_unsafe" };
        foreach (string itemId in order)
        {
            if (room <= 0)
                break;
            int n = TakeInto(itemId, Mathf.Min(room, state.Count(itemId)));
            room -= n;
            drawn += n;
        }
        if (drawn > 0)
            ToolStatus.Flash($"Filled your Bucket with {drawn} L");
        return drawn;
    }

    // Litres the player's Buckets and Canteens can still take.
    static int BucketRoom()
    {
        if (Inventory == null)
            return 0;
        return Mathf.Max(0, WaterQualities.Capacity(Inventory.Player) - WaterQualities.LitresCarried(Inventory.Player));
    }

    static float WeightOf(string itemId) => ItemDatabase.Get(itemId)?.WeightKg ?? 0f;

    // --- Storing (R) ---

    public string SecondaryPrompt
    {
        get
        {
            if (state == null)
                return "";
            int carried = CarriedStorable(out string what);
            // R stores what the site still takes; with nothing to store it takes the site down, like the Lean-To's
            // pack-up — the one key left over on the site, since E takes materials back.
            if (carried <= 0)
                return IsCabinSite ? "Remove Site  (everything stocked comes back to you)" : "";
            if (IsBarrel)
            {
                int room = WoodManager.Instance != null ? WoodManager.Instance.BarrelLitres - state.Total : 0;
                return room > 0 ? $"Pour In Water  ({Mathf.Min(room, carried)} L)" : "";
            }
            return UsesTransferScreen ? $"Store Items  ({what})" : $"Store {what}";
        }
    }

    public void SecondaryInteract(PlayerController player)
    {
        if (state == null || Inventory == null)
            return;

        // The built storage kinds open the transfer screen on R as well as E (Primitive_Storage_System.md's Two-Way
        // Transfer Screen, 2026-10-04); "Store All Eligible" in there is the old one-press bulk store.
        if (UsesTransferScreen)
        {
            OpenScreen();
            return;
        }

        // A CabinSite with nothing left to store takes itself down (see SecondaryPrompt).
        if (IsCabinSite && CarriedStorable(out _) <= 0)
        {
            ToolStatus.Flash(RemoveStructure());
            return;
        }

        // A felled tree's pile and a CabinSite keep the quick bulk store.
        bool wasComplete = IsCabinSite && WoodManager.Instance != null && WoodManager.Instance.SiteNeeds(state) == null;
        int stored = StoreEligible();
        if (stored <= 0)
        {
            // Nothing carried is still wanted: say how far along the site is, so the cap is not a silent refusal.
            if (IsCabinSite && WoodManager.Instance != null)
                ToolStatus.Flash($"Nothing more to add — {WoodManager.Instance.SiteProgress(state)}", 4f);
            return;
        }
        ToolStatus.Flash(IsBarrel ? $"Poured in {stored} L — the barrel holds {state.Total} L"
                         : IsCabinSite && WoodManager.Instance != null ? $"Stored — {WoodManager.Instance.SiteProgress(state)}"
                         : $"Stored — it now holds {Describe(state)}");
        if (WoodManager.Instance != null)
            WoodManager.Instance.NotifyChanged(state);
        // The deposit that supplied the last missing material: a big notice, so the next step isn't missed.
        if (IsCabinSite && !wasComplete && WoodManager.Instance != null && WoodManager.Instance.SiteNeeds(state) == null)
            ToolStatus.Banner("Small Cabin site fully stocked - equip the Hammer, open Inventory and press Complete Small Cabin", 10f);
    }

    void OpenScreen()
    {
        GameScreens screens = FindAnyObjectByType<GameScreens>();
        if (screens != null)
            screens.OpenStorage(this);
    }

    // --- Dismantling (Building_Housing_System.md, Tools: Hammer) ---
    //
    // The Hammer's Dismantle (specified 2026-10-03, built 2026-10-04) for the structures this component draws:
    //   A storage pile (Wood Pile, Rock Pile, Water Barrel, Food Cache, Storage Bin, Tool Rack) — Hammer equipped, and
    //     only when it's empty, so nothing silently disappears with it. Gives back half its building materials,
    //     rounded down, the Lean-To's precedent. Taken down from the transfer screen's button.
    //   A Small Cabin site — free to place and needs no tool, so none to take it down (Mike placed two by accident,
    //     2026-10-04). Everything stocked in it comes back in full, since it was only stored, never used up. E on an
    //     empty site, or R with nothing to store, takes it down.
    // Whatever the pack can't carry is left on the ground as pickup piles, never lost (StructureRefund). A finished
    // Small Cabin is WoodManager.DismantleCabin's; a felled tree's pile goes away by itself once emptied.

    bool IsCabinSite => state.kind == PileKind.CabinSite;

    // Whether this can be dismantled right now, and why not if not.
    public bool CanRemove(out string reason)
    {
        reason = "";
        if (state == null || Inventory == null)
            return false;
        if (IsCabinSite)
            return true; // no Hammer: placing one doesn't need it
        if (!UsesTransferScreen)
        {
            reason = "can't be taken down";
            return false;
        }
        if (!WoodManager.HammerEquipped)
        {
            reason = "equip the Hammer";
            return false;
        }
        if (!state.IsEmpty)
        {
            reason = "empty it first";
            return false;
        }
        return true;
    }

    // Dismantles it for good and says what came back. The pack gets what fits; the rest is left as pickup piles where
    // the structure stood.
    public string RemoveStructure()
    {
        if (!CanRemove(out string reason))
            return $"Can't take the {Name} down — {reason}.";

        WoodManager wood = WoodManager.Instance;
        if (wood == null)
            return "Not available here.";

        var refund = new StructureRefund(transform.position, transform);

        // Everything stocked, each batch keeping its day.
        var ids = new List<string>();
        foreach (WoodStack stack in state.contents)
            if (!ids.Contains(stack.itemId))
                ids.Add(stack.itemId);
        foreach (string itemId in ids)
            foreach (WoodStack batch in state.Take(itemId, state.Count(itemId)))
                refund.Return(itemId, batch.count, batch.day);

        // Half the building materials of a storage pile (a site is free to place, so there's nothing to give back).
        if (!IsCabinSite)
            foreach (WoodStack cost in wood.CostOf(state.kind))
                refund.Return(cost.itemId, cost.count / 2);

        string name = Name;
        wood.RemovePile(state);
        return refund.Describe(name);
    }

    // --- Storing: the transfer screen's per-item store (the mirror of TakeOne), and the bulk store behind R ---

    // Litres the barrel can still take; unlimited for anything else.
    int StorageRoom => IsBarrel && WoodManager.Instance != null ? Mathf.Max(0, WoodManager.Instance.BarrelLitres - state.Total) : int.MaxValue;

    // Whether StoreOne(itemId, ...) could store anything right now, and why not if not.
    public bool CanStoreOne(string itemId, out string reason)
    {
        reason = "";
        if (state == null || Inventory == null)
            return false;
        if (!state.Accepts(itemId))
        {
            reason = $"a {Name} doesn't hold that";
            return false;
        }
        if (Inventory.EquippedTool != null && Inventory.EquippedTool.Id == itemId)
        {
            reason = "equipped — put it away first";
            return false;
        }
        if (Inventory.Player.Count(itemId) <= 0)
        {
            reason = "none carried";
            return false;
        }
        if (StorageRoom <= 0)
        {
            reason = "the barrel is full";
            return false;
        }
        return true;
    }

    // One unit of an item from the pack, or (all) the whole stack, into this pile — StorageTransferScreen's
    // Store/Shift-click. Returns how many went in.
    public int StoreOne(string itemId, bool all)
    {
        if (!CanStoreOne(itemId, out _))
            return 0;
        int want = Mathf.Min(all ? Inventory.Player.Count(itemId) : 1, StorageRoom);
        int stored = StoreInto(itemId, want, IsBarrel);
        if (stored > 0 && WoodManager.Instance != null)
            WoodManager.Instance.NotifyChanged(state);
        return stored;
    }

    // Whether Store All Eligible would move anything.
    public bool AnythingToStore()
    {
        foreach (ItemStack stack in Inventory.Player.Stacks)
            if (CanStoreOne(stack.itemId, out _))
                return true;
        return false;
    }

    // The old one-press bulk store — everything carried that this takes, minus the equipped Tool — now a shortcut inside
    // the transfer screen.
    public string StoreAllToStatus()
    {
        int stored = StoreEligible();
        if (stored > 0 && WoodManager.Instance != null)
            WoodManager.Instance.NotifyChanged(state);
        if (stored <= 0)
            return IsBarrel && StorageRoom <= 0 ? "The barrel is full." : "Nothing you're carrying fits in here.";
        return IsBarrel ? $"Poured in {stored} L — the barrel holds {state.Total} L." : $"Stored {stored}.";
    }

    // Stores every kind of item carried that this pile accepts, up to its limits. Returns how many went in.
    int StoreEligible()
    {
        // The barrel's litres are one shared budget across every water type; a CabinSite's cap is per material
        // instead (each stops accepting more once it's got what Small Cabin needs — see WoodPileState.Accepts), so it
        // doesn't share a running total the way the barrel's room does.
        bool isCabinSite = state.kind == PileKind.CabinSite;
        int room = StorageRoom;
        int stored = 0;
        var ids = new List<string>();
        foreach (ItemStack stack in Inventory.Player.Stacks)
            if (state.Accepts(stack.itemId) && !ids.Contains(stack.itemId))
                ids.Add(stack.itemId);

        bool first = true;
        foreach (string itemId in ids)
        {
            if (room <= 0)
                break;
            if (Inventory.EquippedTool != null && Inventory.EquippedTool.Id == itemId)
                continue;

            int cap = isCabinSite && WoodManager.Instance != null
                ? Mathf.Max(0, WoodManager.Instance.CabinRequired(itemId) - state.Count(itemId))
                : room;
            if (cap <= 0)
                continue;

            int removed = StoreInto(itemId, Mathf.Min(cap, Inventory.Player.Count(itemId)), IsBarrel && first);
            first = false;
            stored += removed;
            if (!isCabinSite)
                room -= removed;
        }
        return stored;
    }

    // Moves up to want of an item from the pack into the pile, each batch keeping the day it was acquired. Storing
    // isn't dropping: pour plays one pour (the barrel), otherwise it stays quiet. Returns how many moved.
    int StoreInto(string itemId, int want, bool pour)
    {
        if (want <= 0)
            return 0;

        // Note each batch's day before it leaves the inventory, oldest first as Remove takes them.
        var batches = new List<ItemStack>();
        foreach (ItemStack stack in Inventory.Player.Stacks)
            if (stack.itemId == itemId)
                batches.Add(new ItemStack { itemId = itemId, quantity = stack.quantity, acquiredDay = stack.acquiredDay });
        batches.Sort((a, b) => a.acquiredDay.CompareTo(b.acquiredDay));

        ItemDefinition item = ItemDatabase.Get(itemId);
        if (AudioManager.Instance != null)
        {
            if (pour)
                AudioManager.Instance.PlayOnConsume(SoundCue.Pour);
            else
                AudioManager.Instance.SilenceNextRemoval();
        }

        int removed = Inventory.RemoveFromPlayer(itemId, want);
        int left = removed;
        foreach (ItemStack batch in batches)
        {
            if (left <= 0)
                break;
            int n = Mathf.Min(left, batch.quantity);
            state.Add(itemId, n, item != null && item.IsPerishable ? batch.acquiredDay : 0);
            left -= n;
        }
        return removed;
    }

    // How many storable items the player carries (not the equipped tool), and a short description.
    int CarriedStorable(out string what)
    {
        what = "";
        if (Inventory == null)
            return 0;
        var counts = new Dictionary<string, int>();
        foreach (ItemStack stack in Inventory.Player.Stacks)
        {
            if (!state.Accepts(stack.itemId) || (Inventory.EquippedTool != null && Inventory.EquippedTool.Id == stack.itemId))
                continue;
            counts[stack.itemId] = (counts.TryGetValue(stack.itemId, out int n) ? n : 0) + stack.quantity;
        }
        int total = 0;
        var parts = new List<string>();
        foreach (KeyValuePair<string, int> pair in counts)
        {
            total += pair.Value;
            parts.Add($"{pair.Value} {ItemDatabase.Get(pair.Key)?.DisplayName ?? pair.Key}");
        }
        what = Shorten(parts);
        return total;
    }

    bool AnythingFits()
    {
        foreach (WoodStack stack in state.contents)
        {
            ItemDefinition item = ItemDatabase.Get(stack.itemId);
            if (item != null && Inventory.Player.SpaceFor(item) > 0)
                return true;
        }
        return false;
    }

    // e.g. "2 Logs, 4 Branches, 5 Sticks" — water as litres, and long lists cut short.
    public static string Describe(WoodPileState pile)
    {
        if (pile.kind == PileKind.WaterBarrel)
            return pile.IsEmpty ? "empty" : $"{pile.Total} L of water";

        var parts = new List<string>();
        foreach (KeyValuePair<string, int> pair in Contents(pile))
            parts.Add($"{pair.Value} {ItemDatabase.Get(pair.Key)?.DisplayName ?? pair.Key}");
        return parts.Count > 0 ? Shorten(parts) : "empty";
    }

    // Each distinct kind stored, totalled across every batch, in a sensible display order (wood biggest first,
    // everything else in the order it went in) — used by Describe above and StorageTransferScreen's rows. Unlike
    // Describe, this breaks the Water Barrel out by quality too, since the screen needs to offer each one separately.
    public static List<KeyValuePair<string, int>> Contents(WoodPileState pile)
    {
        var counts = new List<KeyValuePair<string, int>>();
        foreach (WoodStack stack in pile.contents)
        {
            int i = counts.FindIndex(p => p.Key == stack.itemId);
            if (i < 0)
                counts.Add(new KeyValuePair<string, int>(stack.itemId, stack.count));
            else
                counts[i] = new KeyValuePair<string, int>(stack.itemId, counts[i].Value + stack.count);
        }
        // Wood biggest first; water best quality first (DrawWater's old priority), matching how a player would want
        // to draw it; everything else in the order it went in.
        string[] order = { WoodManager.LogsId, WoodManager.BranchesId, WoodManager.SticksId, FireManager.FirewoodId,
                            Cooking.PurifiedWaterId, "water_excellent", "water_good", "water_questionable", "water_unsafe" };
        counts.Sort((a, b) => Order(a.Key, order).CompareTo(Order(b.Key, order)));
        return counts;
    }

    static int Order(string itemId, string[] order)
    {
        int i = System.Array.IndexOf(order, itemId);
        return i < 0 ? order.Length : i;
    }

    static string Shorten(List<string> parts) =>
        parts.Count <= 4 ? string.Join(", ", parts) : string.Join(", ", parts.GetRange(0, 3)) + $" and {parts.Count - 3} more";

    // --- Looks ---

    public void Rebuild()
    {
        foreach (GameObject piece in pieces)
            if (piece != null)
                Destroy(piece);
        pieces.Clear();
        if (state == null)
            return;

        var random = new System.Random(state.id * 7919);
        switch (state.kind)
        {
            case PileKind.RockStorage: BuildRocks(random); return;
            case PileKind.WaterBarrel: BuildBarrel(); return;
            case PileKind.FoodCache: BuildBox(new Vector3(1f, 0.6f, 0.7f), 0.8f, true); return;
            case PileKind.StorageBin: BuildBox(new Vector3(1.1f, 0.55f, 0.75f), 1.15f, false); return;
            case PileKind.ToolRack: BuildRack(); return;
            case PileKind.CabinSite: BuildCabinSite(random); return;
        }

        int logs = Mathf.Min(state.Count(WoodManager.LogsId), 6);
        int branches = Mathf.Min(state.Count(WoodManager.BranchesId), 8);
        int sticks = Mathf.Min(state.Count(WoodManager.SticksId), 10);
        int firewood = Mathf.Min(state.Count(FireManager.FirewoodId), 16);

        // A built pile's frame: two poles on the ground that the wood sits across.
        if (state.IsBuilt)
            foreach (float x in new[] { -0.55f, 0.55f })
                Piece(PrimitiveType.Cylinder, new Vector3(x, 0.03f, 0f), new Vector3(90f, 0f, 0f), new Vector3(0.06f, 0.8f, 0.06f), bark);

        for (int i = 0; i < logs; i++)
        {
            float x = ((i % 3) - 1) * 0.36f, y = 0.17f + (i / 3) * 0.3f;
            Piece(PrimitiveType.Cylinder, new Vector3(x, y, 0f), new Vector3(90f, 0f, 0f), new Vector3(0.32f, 0.8f, 0.32f), bark);
        }
        for (int i = 0; i < branches; i++)
            Piece(PrimitiveType.Cylinder, new Vector3(0.8f + (float)random.NextDouble() * 0.4f, 0.05f + i * 0.04f, (float)random.NextDouble() * 1.2f - 0.6f),
                  new Vector3(90f, (float)random.NextDouble() * 60f - 30f, 0f), new Vector3(0.09f, 0.9f, 0.09f), bark);
        for (int i = 0; i < sticks; i++)
            Piece(PrimitiveType.Cylinder, new Vector3(-0.8f + (float)random.NextDouble() * 0.3f, 0.03f + i * 0.02f, (float)random.NextDouble() * 0.6f - 0.3f),
                  new Vector3(90f, (float)random.NextDouble() * 180f, 0f), new Vector3(0.035f, 0.35f, 0.035f), bark);
        for (int i = 0; i < firewood; i++)
            Piece(PrimitiveType.Cylinder, new Vector3(((i % 4) - 1.5f) * 0.2f, 0.09f + (i / 4) * 0.17f, -0.85f),
                  new Vector3(90f, 0f, 0f), new Vector3(0.17f, 0.2f, 0.17f), bark);

        bool anyWood = logs + branches + sticks + firewood > 0;
        box.center = new Vector3(0f, 0.25f, -0.1f);
        box.size = anyWood ? new Vector3(logs + branches > 0 ? 2.2f : 1.6f, logs > 3 || firewood > 8 ? 0.7f : 0.45f, 1.9f)
                           : new Vector3(1.3f, 0.3f, 1.7f);
    }

    // A heap of grey stones: a ring of base stones always, more piled on as Stone is stored.
    void BuildRocks(System.Random random)
    {
        int stones = Mathf.Min(state.Count(WoodManager.StoneId), 20);
        for (int i = 0; i < 6; i++)
        {
            float a = i / 6f * Mathf.PI * 2f;
            Stone(new Vector3(Mathf.Cos(a) * 0.55f, 0.06f, Mathf.Sin(a) * 0.55f), 0.18f, random);
        }
        for (int i = 0; i < stones; i++)
        {
            float a = (float)random.NextDouble() * Mathf.PI * 2f;
            float r = (float)random.NextDouble() * 0.4f * (1f - i / 24f);
            Stone(new Vector3(Mathf.Cos(a) * r, 0.1f + i * 0.025f, Mathf.Sin(a) * r), 0.26f, random);
        }
        box.center = new Vector3(0f, 0.2f, 0f);
        box.size = new Vector3(1.4f, stones > 8 ? 0.6f : 0.35f, 1.4f);
    }

    // A Small Cabin build site: four corner stakes mark the plot even empty, then each of the five materials stacks
    // up in its own corner as it's deposited — visibly growing over however many trips it takes, same as any other
    // pile (Building_Housing_System.md's staged-deposit build).
    void BuildCabinSite(System.Random random)
    {
        var clayColor = new Color(0.45f, 0.3f, 0.2f);
        var grassColor = new Color(0.62f, 0.55f, 0.28f);

        foreach (Vector2 corner in new[] { new Vector2(-1.5f, -1.5f), new Vector2(1.5f, -1.5f), new Vector2(-1.5f, 1.5f), new Vector2(1.5f, 1.5f) })
            Piece(PrimitiveType.Cylinder, new Vector3(corner.x, 0.25f, corner.y), Vector3.zero, new Vector3(0.04f, 0.25f, 0.04f), bark);

        int logs = Mathf.Min(state.Count(WoodManager.LogsId), 20);
        int branches = Mathf.Min(state.Count(WoodManager.BranchesId), 14);
        int grass = Mathf.Min(state.Count(WoodManager.TallGrassId), 15);
        int stone = Mathf.Min(state.Count(WoodManager.StoneId), 16);
        int clay = Mathf.Min(state.Count(WoodManager.ClayId), 10);

        for (int i = 0; i < logs; i++)
        {
            float x = -1.3f + (i % 5) * 0.3f, y = 0.17f + (i / 5) * 0.3f;
            Piece(PrimitiveType.Cylinder, new Vector3(x, y, -1.1f), new Vector3(90f, 0f, 0f), new Vector3(0.32f, 0.85f, 0.32f), bark);
        }
        for (int i = 0; i < branches; i++)
            Piece(PrimitiveType.Cylinder, new Vector3(1.1f + (float)random.NextDouble() * 0.5f, 0.05f + i * 0.035f, -1.3f + (float)random.NextDouble() * 0.7f),
                  new Vector3(90f, (float)random.NextDouble() * 40f - 20f, 0f), new Vector3(0.09f, 0.75f, 0.09f), bark);
        for (int i = 0; i < stone; i++)
        {
            float a = (float)random.NextDouble() * Mathf.PI * 2f, r = (float)random.NextDouble() * 0.4f;
            Stone(new Vector3(-1.2f + Mathf.Cos(a) * r, 0.1f + i * 0.02f, 1.2f + Mathf.Sin(a) * r), 0.24f, random);
        }
        for (int i = 0; i < clay; i++)
            Tinted(Piece(PrimitiveType.Sphere, new Vector3(-0.4f + (i % 3) * 0.3f, 0.08f, 1.3f + (i / 3) * 0.25f), Vector3.zero,
                        new Vector3(0.22f, 0.13f, 0.22f), null), clayColor);
        for (int i = 0; i < grass; i++)
            Tinted(Piece(PrimitiveType.Cylinder, new Vector3(0.9f + (float)random.NextDouble() * 0.5f, 0.02f + i * 0.015f, 0.9f + (float)random.NextDouble() * 0.5f),
                        new Vector3((float)random.NextDouble() * 20f, (float)random.NextDouble() * 360f, 80f), new Vector3(0.02f, 0.35f, 0.02f), null), grassColor);

        box.center = new Vector3(0f, 0.35f, 0f);
        box.size = new Vector3(3.4f, 0.9f, 3.4f);
    }

    // A hollowed-log barrel with the water showing at its level.
    void BuildBarrel()
    {
        const float height = 0.95f;
        Piece(PrimitiveType.Cylinder, new Vector3(0f, height / 2f, 0f), Vector3.zero, new Vector3(0.7f, height / 2f, 0.7f), bark);
        float fill = WoodManager.Instance != null ? Mathf.Clamp01(state.Total / (float)WoodManager.Instance.BarrelLitres) : 0f;
        if (fill > 0f)
            Tinted(Piece(PrimitiveType.Cylinder, new Vector3(0f, 0.08f + (height - 0.1f) * fill, 0f), Vector3.zero, new Vector3(0.62f, 0.005f, 0.62f), null),
                   new Color(0.22f, 0.3f, 0.34f));
        box.center = new Vector3(0f, height / 2f, 0f);
        box.size = new Vector3(0.75f, height, 0.75f);
    }

    // A standing frame with a peg for each kind of Tool stored (up to 6), filling left to right, top to bottom.
    void BuildRack()
    {
        const float height = 1.1f, width = 0.9f;
        Piece(PrimitiveType.Cube, new Vector3(0f, height / 2f, 0f), Vector3.zero, new Vector3(width, height, 0.06f), bark);
        foreach (Vector3 post in new[] { new Vector3(-width / 2f, 0f, 0f), new Vector3(width / 2f, 0f, 0f) })
            Piece(PrimitiveType.Cylinder, post + new Vector3(0f, 0.02f, 0.03f), Vector3.zero, new Vector3(0.05f, 0.02f, 0.05f), bark);

        int kinds = 0;
        foreach (WoodStack stack in state.contents)
            if (stack.count > 0)
                kinds++;
        kinds = Mathf.Min(kinds, 6);
        for (int i = 0; i < kinds; i++)
        {
            float x = -width / 2f + 0.18f + (i % 3) * (width - 0.36f) / 2f;
            float y = height - 0.22f - (i / 3) * 0.4f;
            Piece(PrimitiveType.Cylinder, new Vector3(x, y, 0.09f), new Vector3(90f, 0f, 0f), new Vector3(0.035f, 0.16f, 0.035f), bark);
        }
        box.center = new Vector3(0f, height / 2f, 0f);
        box.size = new Vector3(width + 0.3f, height + 0.1f, 0.5f);
    }

    // A lidded box of split wood (the Food Cache) or an open, lighter-coloured bin (the Storage Bin).
    void BuildBox(Vector3 size, float tint, bool lid)
    {
        GameObject body = Piece(PrimitiveType.Cube, new Vector3(0f, size.y / 2f, 0f), Vector3.zero, size, bark);
        if (tint != 1f)
            Tinted(body, new Color(0.27f * tint, 0.2f * tint, 0.14f * tint));
        if (lid)
            Piece(PrimitiveType.Cube, new Vector3(0f, size.y + 0.03f, 0f), new Vector3(0f, 0f, 2f), new Vector3(size.x + 0.08f, 0.06f, size.z + 0.08f), bark);
        else
            Tinted(Piece(PrimitiveType.Cube, new Vector3(0f, size.y - 0.01f, 0f), Vector3.zero, new Vector3(size.x - 0.1f, 0.02f, size.z - 0.1f), null),
                   new Color(0.08f, 0.06f, 0.05f)); // the dark inside of an open bin
        box.center = new Vector3(0f, size.y / 2f, 0f);
        box.size = new Vector3(size.x + 0.1f, size.y + 0.1f, size.z + 0.1f);
    }

    void Stone(Vector3 position, float size, System.Random random)
    {
        GameObject stone = Piece(PrimitiveType.Sphere, position,
                                 new Vector3((float)random.NextDouble() * 360f, (float)random.NextDouble() * 360f, 0f),
                                 new Vector3(size * (0.8f + (float)random.NextDouble() * 0.5f), size * 0.6f, size), null);
        float g = 0.38f + (float)random.NextDouble() * 0.12f;
        Tinted(stone, new Color(g, g * 0.98f, g * 0.95f));
    }

    static void Tinted(GameObject piece, Color color)
    {
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        piece.GetComponent<MeshRenderer>().SetPropertyBlock(block);
    }

    GameObject Piece(PrimitiveType shape, Vector3 position, Vector3 euler, Vector3 scale, Material material)
    {
        GameObject piece = GameObject.CreatePrimitive(shape);
        Destroy(piece.GetComponent<Collider>());
        piece.transform.SetParent(transform, false);
        piece.transform.localPosition = position;
        piece.transform.localRotation = Quaternion.Euler(euler);
        piece.transform.localScale = scale;
        if (material != null)
            piece.GetComponent<MeshRenderer>().sharedMaterial = material;
        pieces.Add(piece);
        return piece;
    }
}
