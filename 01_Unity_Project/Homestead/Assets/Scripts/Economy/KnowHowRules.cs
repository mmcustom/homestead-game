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
    public const string Charcoal = "charcoal";
    public const string IronOre = "ironore";
    public const string Smelting = "smelting";
    public const string Smithy = "smithy";
    public const string Melee = "melee"; // Hands M0e-3: fists and the Knife. Only the id is saved, in the journal's existing know-how list (no new save field).

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
        new Entry(Hopper, "Using the Leach Hopper", "A Leach Hopper turns Ash into Lye. Build it from the Build tab: place the site, deliver 3 Logs and 4 Tall Grass by hand with R (the site holds them between trips), then equip the Hammer and hold click for about 20 blows.\n\nTo use it, put the Hammer away. Press R at the hopper to store the Ash and water you carry; you can load Ash alone or water alone, and top up either one later. It holds 12 Ash (with any Lye waiting) and 12 L of water, and the poorest water goes in first. Press E to start a batch: it turns as much Ash as there is water into Lye, 1 Ash and 1 L per Lye, and anything left over waits for the next batch. A batch leaches for 24 game hours, and sleeping counts. Ash and water you add while a batch runs wait for the next one. E collects the Lye when it is ready. With the Hammer equipped, R dismantles the hopper instead and gives back half the materials, with any Ash or Lye inside (the stored water is lost)."),
        new Entry(Potash, "From Ash to Potash", "Potash sells for far more than the Ash it comes from: about $5 of Potash per Ash, against $2 for the raw Ash. The chain has three steps.\n\n1. Leach. Build a Leach Hopper (see \"Using the Leach Hopper\"), load Ash and water, and wait 24 game hours.\n2. Collect. Hold E at the hopper to take the Lye. Lye also sells by itself for about $3, but it is worth more as Potash.\n3. Boil. Buy a Potash Kettle at the Trading Post ($25). Set up a tripod over a lit fire and hold E at it to hang the Kettle. Open Inventory (I) and press Boil on the Lye row: 3 Lye and 2 Firewood make 1 Potash in 18 seconds. The Firewood is used up and leaves no Ash.\n\nSell the Potash at the Trading Post (about $16 each)."),
        new Entry(Charcoal, "Charcoal: Burning Logs Slowly", "Charcoal is Logs burned slowly under earth. It burns far hotter than Firewood and is the fuel iron smelting needs in a Bloomery (see \"Smelting Iron in the Bloomery\"). You make it in a Charcoal Pit.\n\n1. Open Build, then Other, and place a Charcoal Pit Site on open dry ground, 3 m clear of other buildings and trees, and not under a roof.\n2. Deliver 4 Clay and 6 Tall Grass with R over as many trips as you like. Then equip the Hammer and hold click on the site to build the pit.\n3. Put the Hammer away. Press R at the pit to load the Logs you carry (up to 6 in all; a Log weighs 8 kg, so a full pit may take two loads).\n4. Press E to light it once it holds 3 Logs. It burns for 24 game hours, even while you sleep or are away, and the Logs are used up. 3 or 4 Logs make 2 Charcoal, 5 make 3 and 6 make 4.\n5. Press E again to collect the Charcoal.\n\nCharcoal sells at the Trading Post for about $1. With the Hammer equipped, R takes the pit down, but not while it burns; you get back half the Clay and Tall Grass and all the Logs and Charcoal inside."),
        new Entry(IronOre, "Iron Ore at the Trading Post", "There is no iron to dig on your land yet, so iron ore is bought. The Trading Post sells Iron Ore for $4 each (3 kg). It buys it back for only $1, so buy only what you will use.\n\nSmelting iron needs Charcoal as well (see \"Charcoal: Burning Logs Slowly\") and a furnace, the Bloomery (see \"Smelting Iron in the Bloomery\")."),
        new Entry(Smelting, "Smelting Iron in the Bloomery", "Iron ore is smelted in a Bloomery, a small furnace of Stone and Clay that burns Charcoal.\n\n1. Buy Iron Ore at the Trading Post and make Charcoal in a Charcoal Pit (see those two entries).\n2. Open Build, then Other, and place a Bloomery Site on open dry ground, 3 m clear of other buildings and trees, and not under a roof.\n3. Deliver 8 Stone and 6 Clay with R over as many trips as you like (about 61 kg in all, so a Hand Cart helps). Then equip the Hammer and hold click on the site to build the bloomery.\n4. Put the Hammer away. Press R at the bloomery to load the Ore and Charcoal you carry, up to 4 of each. You can load one without the other and top up either later.\n5. Press E to start a smelt once it holds 2 Ore and 2 Charcoal. Each 2 Ore and 2 Charcoal make 1 Iron Bar, up to 2 Bars at a time, and the smelt takes 10 game hours, even while you sleep or are away. Ore and Charcoal you add while it smelts wait for the next smelt.\n6. Press E again to collect the Iron Bars.\n\nNobody buys Iron Bars; forge them into iron goods at a Smithy (see \"The Smithy: Forging Iron Goods\"). With the Hammer equipped, R takes the bloomery down, but not while it smelts; you get back half the Stone and Clay and all the Ore, Charcoal and Bars inside."),
        new Entry(Smithy, "The Smithy: Forging Iron Goods", "A Smithy turns Iron Bars and Charcoal into iron goods that sell at the Trading Post, and into an iron Shovel and Pick Axe for your own use. Its forge and anvil stand under a roof.\n\n1. Make Iron Bars first (see \"Smelting Iron in the Bloomery\").\n2. Open Build, then Other, and place a Smithy Site on dry ground, 3 m clear of other buildings and, for the forge fire, 3 m clear of trees. Turn it with the mouse wheel so the open front faces where you want to work.\n3. Deliver 6 Stone, 3 Clay, 2 Logs, 4 Branches and 6 Tall Grass with R over as many trips as you like (about 61 kg in all, so a Hand Cart helps). Then equip the Hammer and hold click on the site to build it.\n4. Put the Hammer away. Press R at the Smithy to store the Iron Bars, Charcoal, Sticks and Cordage you carry (it holds 8 Iron Bars, 8 Charcoal, 6 Sticks and 4 Cordage).\n5. Press E to open the Smithy screen and choose what to forge. Door Hinges take 3 game hours; a Hook Set and a Box of Nails take 2. Each uses 1 Iron Bar and 1 Charcoal. An Iron Shovel (2 Iron Bars, 2 Charcoal, 3 Sticks and 1 Cordage) and an Iron Pick Axe (2 Iron Bars, 2 Charcoal, 2 Sticks and 1 Cordage) take 3 game hours each. A job runs even while you sleep or are away.\n6. Press E at the Smithy to collect the finished item. One job runs at a time, and the next cannot start until the item is collected.\n\nDoor Hinges sell for about $26, a Hook Set for about $18 and a Box of Nails for about $16, and each price drops a little with every one you sell, then recovers over the next days. The iron Shovel and Pick Axe cannot be sold. They work like the stone ones but need a quarter fewer blows for everything: a Stone from the outcrop, a stump, Clay and Sand, a Well shaft and a new Outhouse pit. The stone tools stay as they are. With the Hammer equipped, R takes the Smithy down, but not while something is being forged; you get back half the materials and all the Bars, Charcoal, Sticks, Cordage and finished items inside."),
        new Entry(Melee, "Fighting with your hands and a Knife", "Click with empty hands to punch. Fists only stun and startle: a small animal (a Rabbit, Squirrel, Turkey or Duck) stops for about a second and then bolts, a Deer just bolts, and nothing is ever hurt.\n\nEquip a Knife and click to slash, then click again to stab. The clicks alternate on their own, and the first click after a short pause is always a slash. Both reach about 1.5 m. A Knife kills small game (a Rabbit, Squirrel, Turkey or Duck) in one solid hit and leaves a Carcass to field dress, but a Deer is only startled, never hurt.\n\nA punch costs 2 Stamina and a slash or stab 3. Every blow makes noise: animals within 8 m of a punch and 6 m of a Knife blow bolt, so you will rarely catch one that has already noticed you. A blow cannot reach through a wall. A trapped animal cannot be struck; take it out of the trap with a Knife in your pack (it does not have to be equipped)."),
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
    public enum Happening { PotHung, HopperSitePlaced, PitSitePlaced, BloomerySitePlaced, SmithySitePlaced, Punched }

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
            case MetalRules.CharcoalId: return new[] { Charcoal };
            case MetalRules.IronOreId: return new[] { IronOre };
            case MetalRules.IronBarId: return new[] { Smelting, Smithy };
            case KnifeRules.ToolId: return new[] { Melee };
            default: return Array.Empty<string>();
        }
    }

    public static string[] ForHappening(Happening what)
    {
        switch (what)
        {
            case Happening.PotHung: return new[] { Tripod };
            case Happening.HopperSitePlaced: return new[] { Hopper, Potash };
            case Happening.PitSitePlaced: return new[] { Charcoal };
            case Happening.BloomerySitePlaced: return new[] { Smelting };
            case Happening.SmithySitePlaced: return new[] { Smithy };
            case Happening.Punched: return new[] { Melee };
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
        MapleRules.SapId, PotashRules.LyeId, TripodRules.KettleId, MetalRules.CharcoalId, MetalRules.IronOreId, MetalRules.IronBarId, KnifeRules.ToolId,
    };

    // The entries a loaded game has already earned: what the pack holds, and what it has built (a hopper site, a tripod, a tap). An old save
    // from before Know-how existed gets its help this way.
    public static List<string> Backfill(Func<string, bool> holds, bool hopperPlaced, bool tripodStands, bool tapsOut, bool pitPlaced = false, bool bloomeryPlaced = false, bool smithyPlaced = false)
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
        if (pitPlaced)
            triggered.AddRange(ForHappening(Happening.PitSitePlaced));
        if (bloomeryPlaced)
            triggered.AddRange(ForHappening(Happening.BloomerySitePlaced));
        if (smithyPlaced)
            triggered.AddRange(ForHappening(Happening.SmithySitePlaced));
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
            case TripodRules.ClayPotId: return "Hang it on a tripod at a fire (hold E) to boil water or sap, like the iron Cooking Pot.";
            case TripodRules.TripodId: return "Set it up on a fire from Build, then hang a Pot or Kettle (hold E).";
            case MapleRules.SpileId: return "Equip it and click a Sugar Maple to tap it; needs a Sap Bucket.";
            case MapleRules.SapBucketId: return "Hangs under a Spile. Hold E at the tap to collect sap; boil it in the Pot.";
            case MapleRules.SapId: return "10 L boils to 1 Syrup in the Pot on a tripod at a fire.";
            case AshRules.KindlingItemId: return "Sell it at the Trading Post.";
            case MetalRules.IronOreId: return "Smelt it with Charcoal in a Bloomery. Sells back for $1.";
            case MetalRules.IronBarId: return "Smelted in a Bloomery. Forge it into goods at a Smithy; not sold itself.";
            case SmithyRules.HingesId: return "Forged at a Smithy. Sells at the Trading Post for about $26.";
            case SmithyRules.HooksId: return "Forged at a Smithy. Sells at the Trading Post for about $18.";
            case SmithyRules.NailsId: return "Forged at a Smithy. Sells at the Trading Post for about $16.";
            case ToolRules.IronShovelId: return "Made at a Smithy. Digs like the Shovel in a quarter fewer blows; not sold.";
            case ToolRules.IronPickAxeId: return "Made at a Smithy. Mines like the Pick Axe in a quarter fewer blows; not sold.";
            case MetalRules.CharcoalId: return "Made from Logs in a Charcoal Pit. Sells for $1; iron smelting burns it.";
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
            case TripodRules.PotId:
            case TripodRules.ClayPotId: return " Open Inventory (I) to Boil water or sap.";
            case TripodRules.KettleId: return " Open Inventory (I) to Boil Lye.";
            default: return "";
        }
    }

    public const string HopperBuilt = "Leach Hopper built. Put the Hammer away, press R to load Ash and water, then E to start leaching.";
    public const string LyeTaken = "Boil 3 Lye + 2 Firewood into Potash in a Potash Kettle at a fire.";
    public const string SiteBuildInfo = "Leaches Ash into Lye for Potash. Free to place, no Hammer; deliver the materials at the site (R).";

    // The hopper's lines when it is empty. The Hammer in hand makes R dismantle, so the player is told to put it away.
    public static string HopperEmptyPrompt(string name, bool hammerEquipped) =>
        hammerEquipped ? $"{name}  (empty). Put the Hammer away to load Ash and water"
                       : $"{name}  (empty)";

    public static string HopperLoadHelp() => "Load Ash and water  (you're not carrying any)";

    // The Tap's prompt, with what the sap is for while the pack holds too little for a Syrup.
    public static string TapBoilHint(int sapCarriedLitres) =>
        sapCarriedLitres < MapleRules.SapPerSyrup ? "  ·  10 L boils to 1 Syrup" : "";
}
