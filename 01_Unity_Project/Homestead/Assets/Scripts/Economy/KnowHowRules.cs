using System;
using System.Collections.Generic;

// Journal "Know-how" (Journal_KnowHow_Text.md, decided by Mike 2026-10-27): short step-by-step entries that unlock the first time the player
// holds the key item or first builds the structure, so a player who finds Ash, a Kettle or a hopper site is told how the chain works. Pure and
// engine-free so edit-mode tests cover it. The text below is Mike's, word for word; JournalManager keeps only the ids that have unlocked.
//
// Also the short hints the Inventory rows, prompts and flashes show (the next step in the chain), kept here so their lengths are tested.
public static class KnowHowRules
{
    // The entry ids (what a save keeps).
    public const string Ash = "ash";
    public const string Kindling = "kindling";
    public const string Tripod = "tripod";
    public const string Tapping = "tapping";
    public const string Syrup = "syrup";
    public const string Hopper = "hopper";
    public const string Potash = "potash";

    public readonly struct Entry
    {
        public readonly string id, title, body;

        public Entry(string id, string title, string body)
        {
            this.id = id;
            this.title = title;
            this.body = body;
        }
    }

    // In Mike's order. Titles and bodies are the text file's.
    public static readonly Entry[] Entries =
    {
        new Entry(Ash, "Ash: Collect and Sell", "Burning wood in a campfire or hearth leaves Ash behind. At the fire, press G to collect it (this works whether the fire is lit or out). Firewood leaves 1 Ash for every 3 burned, and Sticks, Branches and Logs leave some too.\n\nAsh sells at the Trading Post for about $2 each, and the price drops a little with every one you sell, then recovers over the next days. You can also leach Ash into Lye, which is worth much more as Potash (see \"From Ash to Potash\")."),
        new Entry(Kindling, "Kindling Bundles", "Felling trees leaves you plenty of Sticks. In the Craft tab, 10 Sticks make 1 Kindling Bundle, which sells for about $1. Use the amount control to craft several at once.\n\nSticks also burn and leave a little Ash (1 Ash for every 12 Sticks)."),
        new Entry(Tripod, "The Tripod and Boiling Water", "Boiling needs the Cooking Pot hung on a tripod over a lit fire. Carrying the Pot is not enough.\n\n1. Craft a Tripod (Craft tab, Fire and Light: 3 Branches and 1 Cordage).\n2. Open Build, then Other, and click Set Up. Aim at a campfire or hearth within 3 m and click when the ghost is green. The Grill cannot take a tripod.\n3. Aim at the tripod and hold E to hang the Cooking Pot from your pack.\n4. Stand within 3 m of the lit fire, open Inventory (I) and press Boil on the water.\n\nOne vessel hangs at a time, and holding E swaps or takes it down. To dismantle the tripod, put the fire out, equip the Hammer and hold R; the Tripod and the Pot come back to you."),
        new Entry(Tapping, "Tapping Maples", "Maples run sap only in Winter and Spring, on days that thaw after a freezing night.\n\n1. Craft a Spile (3 Sticks) and a Sap Bucket (1 Log, 3 Branches and 1 Cordage) for each tap.\n2. Equip the Spile. While you hold it, a green droplet floats over each Maple within 80 m that you can tap.\n3. Walk up and click the Maple. One tap per tree, up to 12.\n\nYour taps show a pail marker, visible from far away and on the minimap: grey while filling, amber when 4 L or more is ready, brown-red when the sap has soured. Hold E at a tap to collect, and R to take the tap down. Sap sours after 48 hours in the bucket, so check your taps about once a day."),
        new Entry(Syrup, "Sap to Syrup", "10 L of sap boils down to 1 Maple Syrup in about 12 seconds. You need the Cooking Pot hung on a tripod over a lit fire, and you must stand within 3 m of it. Open Inventory (I), find the Maple Sap row and press Boil. It uses no extra Firewood.\n\nCarried sap does not spoil. Syrup sells for about $10 at the Trading Post, and you can eat it too."),
        new Entry(Hopper, "Using the Leach Hopper", "A Leach Hopper turns Ash into Lye. Build it from the Build tab: place the site, deliver 3 Logs and 4 Tall Grass by hand with R (the site holds them between trips), then equip the Hammer and hold click for about 20 blows.\n\nTo load it, put the Hammer away and press R at the hopper. Each Lye takes 1 Ash and 1 L of water (the poorest water goes in first), up to 12 at a time. It leaches for 24 game hours, and sleeping counts. Hold E at the hopper to collect the Lye. With the Hammer equipped, R dismantles the hopper instead and gives back half the materials, with any Ash or Lye inside."),
        new Entry(Potash, "From Ash to Potash", "Potash sells for far more than the Ash it comes from: about $5 of Potash per Ash, against $2 for the raw Ash. The chain has three steps.\n\n1. Leach. Build a Leach Hopper (see \"Using the Leach Hopper\"), load Ash and water, and wait 24 game hours.\n2. Collect. Hold E at the hopper to take the Lye. Lye also sells by itself for about $3, but it is worth more as Potash.\n3. Boil. Buy a Potash Kettle at the Trading Post ($25). Set up a tripod over a lit fire and hold E at it to hang the Kettle. Open Inventory (I) and press Boil on the Lye row: 3 Lye and 2 Firewood make 1 Potash in 18 seconds. The Firewood is used up and leaves no Ash.\n\nSell the Potash at the Trading Post (about $16 each)."),
    };

    public static bool TryGet(string id, out Entry entry)
    {
        foreach (Entry e in Entries)
        {
            if (e.id == id)
            {
                entry = e;
                return true;
            }
        }
        entry = default;
        return false;
    }

    // --- Triggers ---

    // Things that happen in the world, besides an item entering the pack.
    public enum Happening { PotHung, HopperSitePlaced }

    // Entries a first-held item unlocks (none for most items). Ash opens both "Ash" and "From Ash to Potash".
    public static string[] ForItem(string itemId)
    {
        switch (itemId)
        {
            case AshRules.ItemId: return new[] { Ash, Potash };
            case AshRules.KindlingItemId: return new[] { Kindling };
            case TripodRules.TripodId: return new[] { Tripod };
            case MapleRules.SpileId:
            case MapleRules.SapBucketId: return new[] { Tapping };
            case MapleRules.SapId: return new[] { Syrup };
            case PotashRules.LyeId:
            case TripodRules.KettleId: return new[] { Potash };
            default: return Array.Empty<string>();
        }
    }

    public static string[] ForHappening(Happening what)
    {
        switch (what)
        {
            case Happening.PotHung: return new[] { Tripod };
            case Happening.HopperSitePlaced: return new[] { Hopper, Potash };
            default: return Array.Empty<string>();
        }
    }

    // The ids in triggered that are real entries and not unlocked yet, each once, in the order given.
    public static List<string> NewUnlocks(IEnumerable<string> unlocked, IEnumerable<string> triggered)
    {
        var have = new HashSet<string>(unlocked ?? Array.Empty<string>());
        var fresh = new List<string>();
        if (triggered == null)
            return fresh;
        foreach (string id in triggered)
        {
            if (id != null && TryGet(id, out _) && have.Add(id))
                fresh.Add(id);
        }
        return fresh;
    }

    // Every item that can open an entry, in the order the pack is scanned.
    static readonly string[] TriggerItems =
    {
        AshRules.ItemId, AshRules.KindlingItemId, TripodRules.TripodId, MapleRules.SpileId, MapleRules.SapBucketId,
        MapleRules.SapId, PotashRules.LyeId, TripodRules.KettleId,
    };

    // The entries a loaded game has already earned: what the pack holds, and what it has built (a hopper site, a tripod, a tap). An old save
    // from before Know-how existed gets its help this way.
    public static List<string> Backfill(Func<string, bool> holds, bool hopperPlaced, bool tripodStands, bool tapsOut)
    {
        var triggered = new List<string>();
        foreach (string itemId in TriggerItems)
        {
            if (holds != null && holds(itemId))
                triggered.AddRange(ForItem(itemId));
        }
        if (hopperPlaced)
            triggered.AddRange(ForHappening(Happening.HopperSitePlaced));
        if (tripodStands)
            triggered.AddRange(ForHappening(Happening.PotHung));
        if (tapsOut)
            triggered.Add(Tapping);
        return NewUnlocks(null, triggered);
    }

    // --- The words ---

    // The flash when entries unlock: one by name, several as a count.
    public static string UnlockedMessage(IList<string> ids)
    {
        if (ids == null || ids.Count == 0)
            return "";
        if (ids.Count == 1 && TryGet(ids[0], out Entry only))
            return $"Journal: new know-how, {only.title}";
        return $"Journal: {ids.Count} know-how entries added";
    }

    // The flash for entries a loaded or new game already earned: one by name (a new Settler's Tripod), several as a count.
    public static string BackfillFlash(IList<string> ids) =>
        ids != null && ids.Count == 1 ? UnlockedMessage(ids) : BackfilledMessage(ids != null ? ids.Count : 0);

    public static string BackfilledMessage(int count) =>
        count == 1 ? "Journal: 1 know-how entry added" : $"Journal: {count} know-how entries added";

    // Row tips in the Inventory, for items that lead somewhere. Empty for any other item.
    public static string ItemTip(string itemId)
    {
        switch (itemId)
        {
            case AshRules.ItemId: return "Sell it, or leach it with water in a Leach Hopper to make Lye.";
            case PotashRules.LyeId: return "Boil 3 Lye + 2 Firewood into Potash in the Kettle at a fire.";
            case PotashRules.PotashId: return "Sells for far more than the Ash it came from.";
            case TripodRules.KettleId: return "Hang it on a tripod at a lit fire (hold E), then Boil Lye (I).";
            case TripodRules.PotId: return "Hang it on a tripod at a fire (hold E) to boil water or sap.";
            case TripodRules.TripodId: return "Set it up on a fire from Build, then hang a Pot or Kettle (hold E).";
            case MapleRules.SpileId: return "Equip it and click a Sugar Maple to tap it; needs a Sap Bucket.";
            case MapleRules.SapBucketId: return "Hangs under a Spile. Hold E at the tap to collect sap; boil it in the Pot.";
            case MapleRules.SapId: return "10 L boils to 1 Syrup in the Pot on a tripod at a fire.";
            case AshRules.KindlingItemId: return "Sell it at the Trading Post.";
            default: return "";
        }
    }

    // The Ash flash. The first time, it says where Ash goes next.
    public static string AshCollected(int taken, int ready, bool first)
    {
        string note = taken < ready ? $"Collected {taken} Ash — no room for the other {ready - taken}" : $"Collected {taken} Ash";
        return first ? note + ". Sell it, or leach it into Lye in a Leach Hopper (Build)." : note;
    }

    // What follows "Hung the ... on the tripod." for each vessel.
    public static string AfterHang(string vesselId)
    {
        switch (vesselId)
        {
            case TripodRules.PotId: return " Open Inventory (I) to Boil water or sap.";
            case TripodRules.KettleId: return " Open Inventory (I) to Boil Lye.";
            default: return "";
        }
    }

    public const string HopperBuilt = "Leach Hopper built. Put the Hammer away, then hold R to load Ash and water.";
    public const string LyeTaken = "Boil 3 Lye + 2 Firewood into Potash in a Potash Kettle at a fire.";
    public const string SiteBuildInfo = "Leaches Ash into Lye for Potash. Free to place, no Hammer; deliver the materials at the site (R).";

    // The hopper's lines when it is empty. The Hammer in hand makes R dismantle, so the player is told to put it away.
    public static string HopperEmptyPrompt(string name, bool hammerEquipped) =>
        hammerEquipped ? $"{name}  (empty). Put the Hammer away to load Ash and water"
                       : $"{name}  (empty)";

    public static string HopperLoadHelp() =>
        "Load Ash and water  (1 Ash and 1 L each, up to 12; the worst water goes in first). Equip the Hammer to dismantle";

    // The Tap's prompt, with what the sap is for while the pack holds too little for a Syrup.
    public static string TapBoilHint(int sapCarriedLitres) =>
        sapCarriedLitres < MapleRules.SapPerSyrup ? "  ·  10 L boils to 1 Syrup" : "";
}
