using UnityEngine;

// Cooking and boiling at a lit campfire (the Cooking and Water Purification entries, 2026-09-25). Standing within
// reach of a burning campfire, the Inventory screen offers Cook on raw meat and fish — each becomes its cooked
// Consumable, safe to eat, more filling and keeping longer — and Boil on collected raw water, which becomes Purified
// Water. Boiling needs a pot to boil it in: the metal Cooking Pot (Water_System.md's Boiling correction, 2026-09-26 —
// the wooden Bucket only carries water; it would burn over the fire).
// The Pot is no longer carried to the fire: it hangs on a Tripod standing over it (TripodRules), and a boil needs the Pot hung at a lit fire in
// reach. Cooking meat and fish still needs only a lit fire.
// Each piece takes a few seconds (CampfireCooking runs the queue and calls Cook as each one finishes); it doesn't burn
// extra fuel.
public static class Cooking
{
    public const string PurifiedWaterId = "water";
    public const string PotId = "cooking_pot";

    // How close to a lit campfire the player must be to cook (metres).
    public const float Range = 3f;

    // Raw item → cooked item.
    static readonly string[,] Recipes =
    {
        { "venison", "cooked_venison" },
        { "turkey_meat", "cooked_turkey" },
        { "waterfowl_meat", "cooked_waterfowl" },
        { "chicken_meat", "cooked_chicken" },
        { "small_game_meat", "cooked_small_game" },
        { "bluegill", "cooked_bluegill" },
        { "crappie", "cooked_crappie" },
        { "bass", "cooked_bass" },
        { "catfish", "cooked_catfish" },
    };

    // The cooked version of a raw item, or null if it isn't cooked.
    public static string CookedId(string rawId)
    {
        for (int i = 0; i < Recipes.GetLength(0); i++)
            if (Recipes[i, 0] == rawId)
                return Recipes[i, 1];
        return null;
    }

    public static bool IsCookable(string itemId) => CookedId(itemId) != null;
    public static bool IsBoilable(string itemId) => WaterQualities.IsRawWater(itemId);

    // Maple Sap boils down to Maple Syrup in the Cooking Pot (Maple C): a batch of 10 L at a time, not litre by litre.
    public static bool IsSapBoil(string itemId) => itemId == MapleRules.SapId;

    // Lye boils down to Potash in the Potash Kettle (Potash B): 3 Lye and 2 Firewood a batch, so it is queued by batches like sap.
    public static bool IsLyeBoil(string itemId) => itemId == PotashRules.LyeId;

    // The lit campfire the player can cook at, or null.
    public static CampfireState FireInReach(PlayerController player)
    {
        if (player == null || FireManager.Instance == null)
            return null;
        return FireManager.Instance.NearestLit(player.transform.position, Range);
    }

    // The vessel a boil of this item hangs in: the Cooking Pot for water and sap, the Potash Kettle for Lye; null for anything that is not a boil.
    public static string BoilVessel(string itemId) =>
        IsBoilable(itemId) || IsSapBoil(itemId) ? TripodRules.PotId : IsLyeBoil(itemId) ? TripodRules.KettleId : null;

    public static string VesselName(string itemId) => ItemDatabase.Get(itemId)?.DisplayName ?? itemId;

    // The nearest lit fire in reach that has a tripod with this vessel hung on it, or null.
    public static CampfireState VesselFireInReach(PlayerController player, string vesselId)
    {
        FireManager fires = FireManager.Instance;
        if (player == null || fires == null)
            return null;

        CampfireState best = null;
        float bestDistance = Range;
        foreach (CampfireState fire in fires.Fires)
        {
            if (!fire.lit || !fire.tripod || fire.vessel != vesselId)
                continue;
            float distance = Vector3.Distance(fire.position, player.transform.position);
            if (distance <= bestDistance)
            {
                best = fire;
                bestDistance = distance;
            }
        }
        return best;
    }

    // Whether a boil using this vessel can run here, and if not, why (judged by the nearest lit fire in reach).
    public static TripodRules.Gate BoilGate(PlayerController player, string vesselId)
    {
        if (VesselFireInReach(player, vesselId) != null)
            return TripodRules.Gate.Ok;
        CampfireState lit = FireInReach(player);
        return TripodRules.BoilGate(lit != null, lit != null && lit.tripod, lit != null ? lit.vessel : null, vesselId);
    }

    // True when the vessel is hung at a lit fire in reach; otherwise flashes why not (doing is "Boiling water" or "Boiling sap") and returns false.
    public static bool CheckBoilVessel(PlayerController player, string doing, string vesselId = TripodRules.PotId)
    {
        TripodRules.Gate gate = BoilGate(player, vesselId);
        if (gate == TripodRules.Gate.Ok)
            return true;
        ToolStatus.Flash(TripodRules.GateMessage(gate, doing, VesselName(vesselId)));
        return false;
    }

    // Turns up to count of an item into its cooked (or boiled) version straight away. Returns how many were done.
    public static int Cook(ItemDefinition raw, int count, PlayerController player)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null || raw == null || count <= 0 || FireInReach(player) == null)
            return 0;
        if (IsSapBoil(raw.Id))
            return BoilSap(count, inventory, player);
        if (IsLyeBoil(raw.Id))
            return BoilLye(count, inventory, player);

        bool boil = IsBoilable(raw.Id);
        string outputId = boil ? PurifiedWaterId : CookedId(raw.Id);
        ItemDefinition output = outputId != null ? ItemDatabase.Get(outputId) : null;
        if (output == null)
            return 0;
        if (boil && !CheckBoilVessel(player, "Boiling water"))
            return 0;

        // Swapping raw for cooked isn't dropping anything; the cooked item arriving plays the pickup sound.
        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval();
        int done = inventory.RemoveFromPlayer(raw.Id, count);
        if (done <= 0)
            return 0;
        inventory.AddToPlayer(outputId, done); // cooked food is dated today: its shelf life starts now

        string what = done > 1 ? $"{done} × {raw.DisplayName}" : raw.DisplayName;
        ToolStatus.Flash(boil ? $"Boiled {done} L of {raw.DisplayName} — now {output.DisplayName}" : $"Cooked {what}");
        return done;
    }

    // Boils batches of sap into Syrup straight away (the queue calls it once a batch has run its 12 s). Returns the Syrup made.
    static int BoilSap(int batches, InventoryManager inventory, PlayerController player)
    {
        if (!CheckBoilVessel(player, "Boiling sap"))
            return 0;

        int carried = inventory.Player.Count(MapleRules.SapId);
        if (MapleRules.SyrupFrom(carried) <= 0)
        {
            ToolStatus.Flash("Boiling " + MapleRules.NeedMessage(carried));
            return 0;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval();
        int made = BoilSapInto(inventory.Player, batches, TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0);
        if (made <= 0)
        {
            ToolStatus.Flash("No room to carry the Syrup — the sap is kept");
            return 0;
        }

        ToolStatus.Flash(MapleRules.BoiledMessage(made, made * MapleRules.SapPerSyrup));
        return made;
    }

    // Boils batches of Lye into Potash straight away (the queue calls it once a batch has run its 18 s). Returns the Potash made.
    static int BoilLye(int batches, InventoryManager inventory, PlayerController player)
    {
        if (!CheckBoilVessel(player, "Boiling lye", TripodRules.KettleId))
            return 0;

        int lye = inventory.Player.Count(PotashRules.LyeId), firewood = inventory.Player.Count(AshRules.FirewoodId);
        if (PotashRules.PotashBatches(lye, firewood) <= 0)
        {
            ToolStatus.Flash("Boiling lye " + PotashRules.NeedMessage(lye, firewood));
            return 0;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval();
        int made = BoilLyeInto(inventory.Player, batches, TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0);
        if (made <= 0)
        {
            ToolStatus.Flash("No room to carry the Potash — the Lye and Firewood are kept");
            return 0;
        }

        ToolStatus.Flash(PotashRules.BoiledMessage(made));
        return made;
    }

    // The swap itself: the Lye and Firewood come out (the Firewood is an ingredient, burned away in the boil, so it leaves no Ash) and the Potash
    // goes in. If the Potash cannot all be added, everything is put back exactly, the Firewood with the day it had (nothing is ever lost).
    // Returns the Potash made, 0 if nothing changed.
    public static int BoilLyeInto(InventoryContainer pack, int batches, int day)
    {
        ItemDefinition lye = ItemDatabase.Get(PotashRules.LyeId), firewood = ItemDatabase.Get(AshRules.FirewoodId), potash = ItemDatabase.Get(PotashRules.PotashId);
        if (pack == null || lye == null || firewood == null || potash == null || batches <= 0)
            return 0;

        int make = System.Math.Min(batches, PotashRules.PotashBatches(pack.Count(PotashRules.LyeId), pack.Count(AshRules.FirewoodId)));
        if (make <= 0)
            return 0;

        int lyeTaken = pack.Remove(PotashRules.LyeId, PotashRules.LyeFor(make));
        var days = new System.Collections.Generic.List<(int day, int count)>();
        int firewoodTaken = pack.RemoveBatches(AshRules.FirewoodId, PotashRules.FirewoodFor(make), (d, n) => days.Add((d, n)));
        bool took = lyeTaken == PotashRules.LyeFor(make) && firewoodTaken == PotashRules.FirewoodFor(make);
        int made = took ? pack.Add(potash, make, day) : 0;
        if (made == make)
            return made;

        if (made > 0)
            pack.Remove(PotashRules.PotashId, made);
        if (lyeTaken > 0)
            pack.Add(lye, lyeTaken, 0);
        foreach ((int d, int n) in days)
            pack.Add(firewood, n, d);
        return 0;
    }

    // The swap itself: the sap comes out and the Syrup goes in, and if the Syrup cannot all be added the sap is put back exactly (nothing is
    // ever lost). Returns the Syrup made, 0 if nothing changed.
    public static int BoilSapInto(InventoryContainer pack, int batches, int day)
    {
        ItemDefinition sap = ItemDatabase.Get(MapleRules.SapId), syrup = ItemDatabase.Get(MapleRules.SyrupId);
        if (pack == null || sap == null || syrup == null)
            return 0;

        MapleRules.Boiled plan = MapleRules.Boil(pack.Count(MapleRules.SapId), batches);
        if (plan.syrup <= 0)
            return 0;

        int taken = pack.Remove(MapleRules.SapId, plan.litresUsed);
        int made = taken == plan.litresUsed ? pack.Add(syrup, plan.syrup, day) : 0;
        if (made == plan.syrup)
            return made;

        if (made > 0)
            pack.Remove(MapleRules.SyrupId, made);
        if (taken > 0)
            pack.Add(sap, taken, day);
        return 0;
    }
}
