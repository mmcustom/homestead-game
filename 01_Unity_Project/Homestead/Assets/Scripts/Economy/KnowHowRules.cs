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
    public const string Spoilage = "spoilage"; // Food_Spoilage_and_Preservation.md: Claude's draft text, for Mike to change as he likes
    public const string Drying = "drying";     // Food_Spoilage_and_Preservation.md 3: Claude's draft text, for Mike to change as he likes
    public const string Salting = "salting";   // Food_Spoilage_and_Preservation.md 4: Claude's draft text, for Mike to change as he likes
    public const string Smoking = "smoking";
    public const string Cellar = "cellar";
    public const string Hides = "hides";
    public const string Garden = "garden";    // Gardening_Basics.md (slice GB1): Claude's draft text, for Mike to change as he likes
    public const string Sawmill = "sawmill";  // Saw_Pit_Sawmill_and_Salt_Barrel.md 2: Claude's draft text, for Mike to change as he likes
    public const string BarkTanning = "bark_tanning"; // Tanning_Hides_and_Furs.md 8 (slice T4): Claude's draft text, for Mike to change as he likes
    public const string BrainTanning = "brain_tanning"; // Tanning_Hides_and_Furs.md 8 (slice T3): Claude's draft text, for Mike to change as he likes
    public const string HideFrame = "hide_frame"; // Tanning_Hides_and_Furs.md 8 (slice T2): Claude's draft text, for Mike to change as he likes     // Tanning_Hides_and_Furs.md 8 (slice T1): Claude's draft text, for Mike to change as he likes
    public const string Sawing = "sawing";     // Saw_Pit_Sawmill_and_Salt_Barrel.md 1: Claude's draft text, for Mike to change as he likes     // Food_Spoilage_and_Preservation.md 6: Claude's draft text, for Mike to change as he likes   // Food_Spoilage_and_Preservation.md 5: Claude's draft text, for Mike to change as he likes

    // Gardening GB2 to GB4 and Greenhouses G1 to G6 (2026-11-27): Claude's draft text, for Mike to change as he likes
    public const string BetterBeds = "better_beds";
    public const string SeedSaving = "seed_saving";
    public const string Fences = "fences";
    public const string ColdFrames = "cold_frames";
    public const string HoopHouses = "hoop_houses";
    public const string GlassHouses = "glass_houses";
    public const string GreenhouseHeat = "greenhouse_heat";
    public const string Watering = "watering";
    public const string Venting = "venting";
    public const string Power = "power"; // slice E1 (2026-11-28): Claude's draft text, for Mike to change as he likes
    public const string DrilledWell = "drilled_well"; // slice P2 (2026-11-27): Claude's draft text, for Mike to change as he likes
    public const string Phone = "phone"; // slice P1 (2026-11-27): Claude's draft text, for Mike to change as he likes
    public const string Barn = "pole_barn"; // slice B1a (2026-11-27): Claude's draft text, for Mike to change as he likes
    public const string Deliveries = "deliveries"; // slice D1 (2026-11-27): Claude's draft text, for Mike to change as he likes

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
        new Entry(Smithy, "The Smithy: Forging Iron Goods", "A Smithy turns Iron Bars and Charcoal into iron goods that sell at the Trading Post, and into an iron Shovel and Pick Axe for your own use. Its forge and anvil stand under a roof.\n\n1. Make Iron Bars first (see \"Smelting Iron in the Bloomery\").\n2. Open Build, then Other, and place a Smithy Site on dry ground, 3 m clear of other buildings and, for the forge fire, 3 m clear of trees. Turn it with the mouse wheel so the open front faces where you want to work.\n3. Deliver 6 Stone, 3 Clay, 2 Logs, 4 Branches and 6 Tall Grass with R over as many trips as you like (about 61 kg in all, so a Hand Cart helps). Then equip the Hammer and hold click on the site to build it.\n4. Put the Hammer away. Press R at the Smithy to store the Iron Bars, Charcoal, Sticks and Cordage you carry (it holds 8 Iron Bars, 8 Charcoal, 6 Sticks and 4 Cordage).\n5. Press E to open the Smithy screen and choose what to forge. Door Hinges take 3 game hours; a Hook Set and a Box of Nails take 2. Each uses 1 Iron Bar and 1 Charcoal. An Iron Shovel (2 Iron Bars, 2 Charcoal, 3 Sticks and 1 Cordage) and an Iron Pick Axe (2 Iron Bars, 2 Charcoal, 2 Sticks and 1 Cordage) take 3 game hours each. A job runs even while you sleep or are away.\n6. Press E at the Smithy to collect the finished item. One job runs at a time, and the next cannot start until the item is collected.\n\nDoor Hinges sell for about $26, a Hook Set for about $18 and a Box of Nails for about $16, and each price drops a little with every one you sell, then recovers over the next days. The Trading Post also stocks the iron Shovel ($12) and Pick Axe ($15). They work like the stone ones but need a quarter fewer blows for everything: a Stone from the outcrop, a stump, Clay and Sand, a Well shaft and a new Outhouse pit. The stone tools stay as they are. With the Hammer equipped, R takes the Smithy down, but not while something is being forged; you get back half the materials and all the Bars, Charcoal, Sticks, Cordage and finished items inside."),
        new Entry(Melee, "Fighting with your hands and a Knife", "Click with empty hands to punch. Fists only stun and startle: a small animal (a Rabbit, Squirrel, Turkey or Duck) stops for about a second and then bolts, a Deer just bolts, and nothing is ever hurt.\n\nEquip a Knife and click to slash, then click again to stab. The clicks alternate on their own, and the first click after a short pause is always a slash. Both reach about 1.5 m. A Knife kills small game (a Rabbit, Squirrel, Turkey or Duck) in one solid hit and leaves a Carcass to field dress, but a Deer is only startled, never hurt.\n\nA punch costs 2 Stamina and a slash or stab 3. Every blow makes noise: animals within 8 m of a punch and 6 m of a Knife blow bolt, so you will rarely catch one that has already noticed you. A blow cannot reach through a wall. A trapped animal cannot be struck; take it out of the trap with a Knife in your pack (it does not have to be equipped)."),
        new Entry(Spoilage, "Food Spoils: Rot and Compost", "Fresh food does not keep. The Inventory and every storage screen show how long each food has left: green while fresh, amber once it is past half its life, red when it is about to spoil. Heat spoils food faster, cold slows it, and freezing weather nearly stops it. A Food Cache keeps food a little longer than your pack.\n\nEating food that is amber or red can make you sick, and the closer it is to spoiling, the more likely.\n\nFood that spoils turns into Rot. Build a Compost Bin (Build tab: 6 Sticks and 2 Cordage, with the Hammer), then press R at it to put in Rot and Tall Grass. Every 4 Rot and 2 Tall Grass become 1 Fertilizer in about 7 days, slower in the cold and not at all while frozen. Press E to take the Fertilizer out. The Trading Post buys it until you have crops to use it on."),
        new Entry(Drying, "Drying Food on a Rack", "A Drying Rack turns fresh food into dried food that is light to carry and keeps for months. Build it from the Build tab: 4 Sticks, 2 Branches and 2 Cordage, with the Hammer.\n\nPress R at the rack to hang Blackberries, Raspberries, Wild Apples, Pawpaws, mushrooms, raw meat or raw fish, up to 12 pieces at a time. They dry only in daylight while it is not raining or snowing: fruit and mushrooms need about 2 days, meat and fish about 3. Food on the rack still ages while it dries, but at half the usual rate.\n\nFruit and mushrooms dry one for one. Meat and fish are cut into strips: the more a piece is worth cooked, the more strips, from 4 for a Small Game Meat or a Bluegill to 8 for a Venison. Dried Meat and Dried Fish are safe to eat without cooking. Press E to take the dried food off; the Trading Post pays more for it than for fresh or cooked food."),
        new Entry(Salting, "Salting Meat and Fish", "Salt keeps raw meat and fish for half a year. Buy Salt at the Trading Post ($2 a bag) and carry a Crock Pot (shaped at the Potter's Shed and fired in the Kiln).\n\nAway from a fire, open Inventory (I): raw meat and fish rows show a Salt button. Each piece takes 1 Salt for every 2 kg it weighs (a Venison takes 2) and is cut into Salt Meat or Salt Fish straight away, one for every dollar it is worth cooked: 4 from a Venison, 2 from a Bluegill. The crock is not used up.\n\nSalt Meat and Salt Fish keep for 180 days and are eaten as they are, no cooking needed. The Trading Post pays more for them than for the same meat fresh or cooked, even after the Salt."),
        new Entry(Smoking, "Smoking Meat and Fish", "A Smokehouse turns raw meat and fish into Smoked Meat and Smoked Fish, the best-paying food the Trading Post buys. Build it from the Build tab: mark out the site, deliver 10 Stone and 4 Clay for the fire pit, then 12 Logs and 6 Lumber, and finish it with the Hammer.\n\nPut the Hammer away and press R at the smokehouse to load raw meat and fish (up to 20 pieces) and Firewood (up to 12), then press E to light the fire. A load smokes for a day and burns 6 Firewood. While the fire burns, the meat does not age. If the Firewood runs out, the smoking stops and the meat starts to age again until you load more Firewood.\n\nEach piece gives one Smoked Meat or Smoked Fish for every dollar it is worth cooked: 4 from a Venison, 2 from a Bluegill. Smoked food keeps for 180 days and is eaten as it is. Press E to collect it; the Trading Post pays $3 a piece, more than for salted, dried or cooked meat."),
        new Entry(Cellar, "The Root Cellar", "A Root Cellar is an earth-covered room for keeping food. Food on its shelves ages a third as fast as outdoors, in every season, and it never freezes: raw meat keeps about 10 days, apples about 6 weeks, and smoked, dried and salted food far longer.\n\nBuild it from the Build tab. Mark out the site, dig it out with the Shovel, then deliver 10 Stone, 8 Logs and 4 Lumber in any order and finish it with the Hammer. Pick dry ground out of hollows, or rain will run in.\n\nWalk in through the door and press E at the shelves at the back to store or take food. The shelves hold up to 300 kg of food."),
        new Entry(Sawing, "Sawing Lumber in a Saw Pit", "A Saw Pit turns Logs into Lumber, so you need not buy it at the Trading Post. Build it from the Build tab: mark out the site, dig the trench with the Shovel, then deliver 6 Logs and 4 Branches in any order and finish it with the Hammer.\n\nPut the Hammer away and press R at the pit to lay Logs across it (up to 6). Then equip the Wood Saw and hold click on the log. Every 2 Logs take 20 strokes and make 3 Lumber, stacked beside the pit. Press E to take the Lumber."),
        new Entry(Hides, "Hides Spoil: Salt or Tan Them", "A fresh Deer Hide or Small Furs will not keep. A Deer Hide spoils in about a week and Small Furs in about five days, faster in summer heat and slower in the cold, and then they turn to Rot. The Inventory shows how long each has left, as it does for food.\n\nTo keep a hide, salt it. Buy Salt at the Trading Post ($2 a bag), then open Inventory (I): a raw hide row shows a Salt button. A Deer Hide takes 2 Salt and Small Furs take 1. A salted hide keeps for about a year and sells for the same as a fresh one."),
        new Entry(HideFrame, "The Hide Frame: Scraping and Rawhide", "A Hide Frame is a square pole frame for working hides, built in one go with the Hammer: 4 Sticks, 2 Branches and 2 Cordage. Find it under Other in the build list.\n\nLace a hide into it with R: a Deer Hide, or up to 4 Small Furs, raw or salted. Then equip the Knife and hold click on the hide to scrape it clean: 20 strokes for a Deer Hide, 8 for furs. Scraping takes off the flesh and fat, and the hair from a Deer Hide; furs keep their hair.\n\nThe scraped hide then dries on the frame in daylight while it is not raining, about 12 hours. A dried Deer Hide is Rawhide, stiff and lasting, which the Trading Post buys for $12. Dried Furs fetch $4 each. Press E to take them off the frame."),
        new Entry(BrainTanning, "Brain Tanning: Buckskin and Furs", "Brain tanning makes a hide soft and lasting. Lace Rawhide or Dried Furs into the Hide Frame with R. With a Crock Pot and 1 litre of water in your pack (a Canteen or Bucket), equip the Knife and hold click on the hide to work the brain-and-water mix into it: 30 strokes for a hide, 10 for furs. Each animal's brains are enough for its own hide.\n\nThe worked hide is a Soft Hide or Soft Furs, white and floppy. Take it off with E and smoke it in the Smokehouse with the meat (R loads it). A Soft Hide takes the room of 2 pieces of meat and Soft Furs 1, and it smokes for the same day on the same Firewood.\n\nIt comes out as Buckskin, golden brown and soft, or Tanned Furs. The Trading Post buys Buckskin for $24 and Tanned Furs for $6 each."),
        new Entry(BarkTanning, "Bark Tanning: Leather", "Bark tanning makes Leather, the strongest hide of all, but it takes a week. You need Rawhide from the Hide Frame, Tanbark and water.\n\nTo get Tanbark you need a Bark Spud, a short iron blade for peeling bark: forge one at the Smithy (1 Iron Bar, 1 Charcoal and a Stick) or buy one at the Trading Post. With it in your pack, the Inventory's Logs row has a Strip button (unless an axe is in your hand). Each Log gives 2 Tanbark and becomes a Peeled Log, which still works anywhere a Log does.\n\nBuild a Tanning Vat (3 Logs and 2 Clay, one click with the Hammer, under Other). Load it with R: Rawhide, up to 4, with 4 Tanbark and 20 litres of water for each hide. Press E to start the soak. After 7 days the hides are Leather, which the Trading Post buys for $30 each. E takes it out."),
        new Entry(Sawmill, "The Sawmill", "A Sawmill saws Logs into Lumber on its own, with a water wheel turned by the creek. It is the next step after the Saw Pit: you need Lumber from the pit to build it.\n\nBuild it from the Build tab, under Other. Mark the site out on the creek bank, within 4 m of the water but out of the creek itself; the wheel goes on the creek side. Deliver 12 Logs, 10 Lumber, 2 Iron Bars, 4 Cordage and 6 Stone with R, in any order, then hammer it up.\n\nLoad Logs with R, up to 20. Every hour the wheel saws 2 Logs into 3 Lumber, even while you are away or asleep, and E takes the Lumber. It stops during a flash flood and starts again by itself when the creek goes down."),
        new Entry(Garden, "The Kitchen Garden", "A kitchen garden feeds you and earns money. Mark out a Garden Bed from the Build tab, under Other: each bed is a 4 by 4 ft square, and beds can stand side by side. Dig each one with the Shovel (hold click).\n\nBuy seed at the Trading Post: seed packets, tomato plants, and potatoes for planting. With the Hoe in hand, aim at a dug bed and press E to plant; R picks which seed. Crops grow day by day with the warmth: lettuce in about 50 days, tomatoes 75, potatoes 100. Tender crops (corn, beans, tomatoes, squash) die in any frost, so plant them after the last spring frost, around the middle of May; lettuce, carrots, potatoes and kale can go in from April.\n\nRain waters the beds. In a dry spell, fill a Watering Can or Bucket and press E on a bed to water it. When a crop is ready, E harvests it; tomatoes, beans and kale keep giving pickings for weeks. A Shovel's R fills an empty bed back in."),
        new Entry(BetterBeds, "Better Beds: Soil, Raised Beds and Frost", "Every crop taken to its end wears a bed's soil. A rich bed grows about 3 crops before it is spent, and a spent bed gives three quarters of a full harvest; beans put a little back. The bed's prompt shows its soil: rich, good, worn or spent. When there is nothing else to do at a bed, E spreads one Fertilizer, and 4 bring a spent bed back to rich. Make Fertilizer in a Compost Bin, or buy it at the Trading Post ($1.25).\n\nA Raised Bed is warmer and drains. Place a Raised Bed Site from the Build tab (under Other), deliver 2 Lumber with R, hammer the frame up, then fill it with the Shovel. Or hold the Hammer at a dug bed and press R with 2 Lumber in your pack. Its crops are about a week ahead in spring, and heavy rain never waterlogs it.\n\nA Row Cover ($6 at the Trading Post) keeps 3 C of frost off a crop. R lays it over a bed and R takes it off; it is used again and again. Above 27 C, a crop under it grows a little slower.\n\nAt 6 pm, if tonight's low could hurt a crop, a message says how many beds are at risk, and whether a Row Cover will be enough."),
        new Entry(SeedSaving, "Saving Your Own Seed", "Seed you save costs nothing. With the Hoe in hand, press R on a growing square before its first pick to mark it saved for seed (R again unmarks it). A saved square is never picked: it stays in the ground until the seed is ripe, then E collects seed for 4 squares, with some food for most crops.\n\nBeans, tomatoes, lettuce, both corns and squash ripen their seed the same summer. Corn needs at least 2 saved squares of the same corn standing together, or it sets no good seed. Potatoes are their own seed: keep some from the harvest. Carrots and kale need a winter in the ground first: after about 6 weeks of cold they grow again, and the seed is ripe the next summer. E with 4 Tall Grass mulches a saved carrot or kale square against hard frost.\n\nSaved Tomato Seed sown straight into a bed starts about a month behind a bought Tomato Plant, unless you start it on a seedling bench in a Glass Greenhouse. Seed keeps 2 to 4 years; old seed comes up poorly, and the Inventory shows its age once it is a year old."),
        new Entry(Fences, "Fences and Garden Raiders", "At dawn and dusk, rabbits and deer come to nibble open garden beds, and over a season a bed can lose about a quarter of its crop. If you are nearby you will see the animal walk in and eat: come close or shoot near it and it bolts, and the crop is spared.\n\nA closed ring of fence keeps them out. Build fences from the Build tab, under Other, with the Hammer:\n- Wattle Fence: 4 Sticks and 6 Branches a section, 1 m high. It keeps rabbits out; deer jump it.\n- Deer Fence: 1 Log and 1 Wire Mesh ($15 at the Trading Post) a section, 7 ft high. It keeps deer and rabbits out.\n- Gate: 2 Lumber and 1 Cordage. It swings open as you walk into it and shuts behind you.\n\nA new section snaps to the end of the fence near where you aim, and one that would close a gap is cut to fit. Only a closed ring protects, and a planted bed says whether it is fenced, and against what. R with the Hammer takes a section down."),
        new Entry(ColdFrames, "Cold Frames", "A Cold Frame is a low box with a glass lid that keeps a few garden squares warm. Build one from the Build tab (under Other) with the Hammer: 2 Window Sashes ($8 each at the Trading Post) and 2 Lumber for each square, in Small (1 square), Medium (2) or Large (3). Its beds come with it, already dug.\n\nShut, the lid keeps the inside about 5 C warmer at night, and the sun can heat it far more by day. Propped open, it stays close to the outside air. E on the front board props the lid or shuts it, and the frame shows the inside temperature. Shut it on frosty nights and open it on sunny days, or tender greens cook. A Vent Opener does this for you (see \"Vent Openers\").\n\nLettuce, kale, carrots and beans fit under the lid. No rain gets in with the lid shut, so water the beds; deer and rabbits cannot reach them. For extra warmth, E with 2 Fertilizer digs a hotbed into a frame's bed: it warms the soil about 8 C for 4 weeks. R with the Hammer takes the frame down once its beds are empty."),
        new Entry(HoopHouses, "Hoop Houses", "A Hoop House is a walk-in tunnel of plastic over steel hoops, with its beds inside. Place a Hoop House Site from the Build tab (under Other) on dry, fairly level ground, then deliver Lumber, Greenhouse Hoops, Greenhouse Plastic and Cordage with R. A Small house (8 squares) takes 8 Lumber, 6 Hoops, 1 Plastic and 2 Cordage; the Medium (16 squares) and Large (36) take more. Hoops are $12 at the Trading Post or forged at a Smithy (1 Iron Bar and 1 Charcoal), and Plastic is $20 a sheet. Then hold click with the Hammer.\n\nWith the sides down the house keeps about 5 C of warmth at night and gains a lot in the sun; E at the door rolls the sides up on warm days. No rain falls inside, so water the beds.\n\nSnow piles on the plastic while it snows. With 2 cm or more on it, E at the door sweeps it off, an hour's work. If it reaches 30 cm the plastic tears, and plastic wears out after 4 years anyway. A torn house gives no cover until E fits new sheets from your pack."),
        new Entry(GlassHouses, "Glass Greenhouses and Seedling Benches", "A Glass Greenhouse is the warmest cover, and the glass lasts for good. Place a Glass Greenhouse Site from the Build tab (under Other), deliver Lumber, Stone, Glass Panes ($3 each at the Trading Post) and Boxes of Nails with R, then hold click with the Hammer. A Small one (4 squares and 1 bench) takes 16 Lumber, 10 Stone, 24 Panes and 1 Box of Nails; the Medium (9 squares) and Large (30) take more. No Iron Bars are needed.\n\nWith the vents shut it keeps about 7 C of warmth at night and heats up fast in the sun; E at the door opens or shuts the vents. No rain falls inside, so water the beds.\n\nThe seedling benches along the back start transplants. With seed in your pack, E sows a tray (R picks which seed). When it is ready, E takes up the transplants: 1 Tomato Seed gives 12 Tomato Plants in 30 days, and 1 Lettuce, Kale or Squash Seed gives 3 Starts. Plant them with the Hoe like seed; they start well grown, and wilt if left unplanted for a week."),
        new Entry(GreenhouseHeat, "Heating a Greenhouse", "Each greenhouse has a heater spot and a barrel spot inside, and the build ghost snaps to them. Build these with the Hammer from the Build tab:\n- Wood Stove: 1 Greenhouse Stove ($40 at the Trading Post) and 2 Stone. R loads Firewood (up to 8) and E lights it. It burns one Firewood an hour, only while the house would be below 8 C.\n- Propane Heater: 1 Propane Heater ($35). R loads Propane Tanks ($12, up to 2) and E turns it on. It keeps the house at 5 C or above and uses propane only for the warmth it adds.\n- Water Barrels: 4 Logs. They soak up the sun by day and give the warmth back at night, with no fuel.\n\nA banner says when a stove or heater runs out. The Small Hoop House has no room for a heater, and the Small Glass Greenhouse none for barrels. R with the Hammer takes them out, with any fuel inside, and a greenhouse with heat in it cannot be taken down until they are out."),
        new Entry(Watering, "Watering Without the Can", "Fittings water many beds at once. Each draws from the nearest Water Barrel, Rain Barrel or pump within 15 m that has water. Build them with the Hammer:\n- Hose Bib: 1 Garden Hose ($8 at the Trading Post). It is fitted inside a greenhouse beside the door and waters that greenhouse's beds.\n- Hose Reel: 1 Garden Hose and 1 Lumber, outdoors. It waters the open garden beds within 12 m.\n- Hand Pump: 1 Hand Pump ($30) and 2 Lumber, within 5 m of the creek or a pond, or beside a Well. E pumps 20 L into its 200 L tank.\n- Fuel Pump: 1 Fuel Pump ($90) and 2 Lumber, beside the creek or a pond. R loads Gasoline ($3 a can, up to 4); each can pumps about 400 L.\n\nE at a bib or reel soaks all its beds at once. With a Drip Kit ($15) in your pack, E fits it, and from then on it waters by itself every morning at 6. When it can't, a banner that morning and the fitting's prompt say why."),
        new Entry(Venting, "Vent Openers", "A Vent Opener ($35 at the Trading Post) opens and shuts a cover for you, with no power. Its wax swells as it warms and pushes the lid, the sides or the vents open. Build it with the Hammer on the inside of a Cold Frame's back wall, or inside a Hoop House or Glass Greenhouse beside the door; the build ghost snaps to the spot.\n\nEvery hour it opens the cover when the inside would reach 24 C (75 F) shut, and shuts it when it would stay at 18 C (64 F) or below. In between it leaves it as it is. You can still open or shut the cover by hand with E, but the opener has its say again the next hour.\n\nE at the opener says what it is doing. R with the Hammer takes it off, and a frame or greenhouse with one fitted cannot be taken down until it is off."),
        new Entry(Deliveries, "Deliveries from the Trading Post", "The Trading Post sells only what you can carry, unless it delivers. Build a Delivery Pad first: it is free, from the Build tab (under Other), and you can have one at a time.\n\nAt the Trading Post (T), press the button at the top of the store's list to switch from Carry to Deliver. Each store row then adds to the order instead of your pack, with no weight limit. The order on the right shows the goods, the delivery fee and the weight. Place Order pays for it all at once: $50 a delivery, or free when the goods come to $1,000 or more.\n\nThe order arrives on the pad at 8 a.m.: the next day for up to 500 kg, in 2 days for up to 2,000 kg, and in 3 days for more. A banner says when it is there. Take what you want with E; the pad keeps the rest, and R puts things back. Take the pad up with the Hammer once it is empty, and build it again wherever you want deliveries."),
        new Entry(Barn, "The Pole Barn", "A Pole Barn is a big post-frame building for storing everything the homestead needs. It comes in three sizes: Small (24 x 32 ft, holds 5,000 kg), Medium (30 x 40 ft, 10,000 kg) and Large (40 x 60 ft, 20,000 kg). Mark out a Pole Barn Site from the Build tab (under Other); it is free and needs fairly flat, open ground.\n\nThen, at the Trading Post (T), switch to Deliver and order the kit ($7,500, $12,000 or $22,000) and it is delivered onto the site: posts, trusses and steel sheets. Build it yourself in three stages with the Hammer: set the posts, raise the trusses, then put on the roof and walls. Or hire the crew by phone (P, Contractors; $13,000, $20,000 or $36,000): half is paid now and half when they finish. They start 3 days later and take about a week.\n\nWalk in through the big door and press E at the shelves along the back to store things or take them. It holds anything but water.\n\nAnything you build or craft within 50 m of a barn can use what is on its shelves: the Build and Craft tabs count it, and R at a building site loads what you carry, then the rest from the barn. Trading Post deliveries go into the barn nearest the Delivery Pad when it has room, so you can order without a pad; what does not fit goes on the pad."),
        new Entry(Phone, "Your Phone", "Everyone starts with a smartphone. Press P to raise it in your hands; P or Esc puts it away. While it is up you stand still and the mouse is free, so click the apps.\n\nWith no service it still shows the time and the date. Open Bills & Plan to start the plan: unlimited talk, text and data for $25 a month, paid when you start it and then on the 15th of every month. If a bill comes when you are short of cash, the service is suspended until you pay it from Bills & Plan; nothing else is lost. You can cancel at any time and the service runs to the next bill day.\n\nWith service: Weather shows the conditions now and tonight's low, Market shows what the Trading Post pays and charges today, and Contractors hires crews, such as the Pole Barn crew. The phone bill shows in the Ledger as Bills."),
        new Entry(DrilledWell, "The Drilled Well", "A drilled well goes down to the deep water a shovel can't reach. Mark out a Drilled Well Site from the Build tab (under Other); it is free, and it keeps a well's distances from the outhouse and the water, but it has no depth limit.\n\nThen hire the driller by phone (P, Contractors). The quote is by the depth where you staked it: about $3,800 on low ground near water up to about $8,500 on high ground. Half is paid when you book; the rig comes 3 days later and drills for 2, and the rest is paid when it is done.\n\nThe finished well has a cast-iron hand pump: press E to fill your Bucket or Canteen, or to drink. Its water is excellent and safe, and flood water and the outhouse can't foul it. It holds up to 1,000 L and refills about 40 L an hour, even in a drought, so only heavy use runs it low. A garden Hand Pump beside it can draw from it too."),
        new Entry(Power, "Power: Solar, Batteries and the Generator", "Power comes in pieces you buy at the Trading Post and add up. Build a Power Shed (Build tab, Other; 12 Lumber and an Inverter, with the Hammer) and press R there to install Battery Modules (5 kWh each, up to 8). Build Solar Panel Racks on open ground (6 Lumber each) and press R to mount Solar Panels (400 W each, four to a rack). Trees within about 12 m shade a rack and cut what it makes.\n\nEvery hour the panels make power by the sun's height and the sky: most at midday in summer, little on a grey winter day, nothing at night. The batteries store it, and they are never run below 10%. A Generator (set it down from the Build tab) runs on Gasoline: load cans with R. It starts by itself when the batteries fall below 20% and stops at 80%, burning about half a gallon an hour.\n\nTo wire a building, carry Buried Cable (sold by the foot), press R at the Power Shed and then E on the building; the run takes a foot of cable for every foot of distance. Wired cabins, barns, greenhouses and workshops get electric lights that come on at dusk. E at the Power Shed (or the Power app on your phone) shows the charge, what the sun is making, what is being used and the generator's fuel."),
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
    public enum Happening { PotHung, HopperSitePlaced, PitSitePlaced, BloomerySitePlaced, SmithySitePlaced, Punched, SmokehouseSitePlaced, RootCellarSitePlaced, SawPitSitePlaced, HideFramePlaced, TanningVatPlaced, SawmillSitePlaced, GardenBedPlaced, RaisedBedPlaced, FencePlaced, ColdFramePlaced, HoopSitePlaced, GlassSitePlaced, HeatPlaced, WateringPlaced, VentOpenerPlaced, DeliveryPadPlaced, BarnPlaced, DrilledWellPlaced, PowerShedPlaced }

    // Entries a first-held item unlocks (none for most items). Ash opens both "Ash" and "From Ash to Potash".
    public static string[] ForItem(string itemId)
    {
        switch (itemId)
        {
            case AshRules.ItemId: return new[] { Ash, Potash };
            case AshRules.KindlingItemId: return new[] { Kindling };
            case TripodRules.TripodId: return new[] { Tripod };
            case PhoneRules.PhoneId: return new[] { Phone };
            case MapleRules.SpileId:
            case MapleRules.SapBucketId: return new[] { Tapping };
            case MapleRules.SapId: return new[] { Syrup };
            case PotashRules.LyeId:
            case TripodRules.KettleId: return new[] { Potash };
            case MetalRules.CharcoalId: return new[] { Charcoal };
            case MetalRules.IronOreId: return new[] { IronOre };
            case MetalRules.IronBarId: return new[] { Smelting, Smithy };
            case KnifeRules.ToolId: return new[] { Melee };
            case SpoilageRules.RotId: return new[] { Spoilage };
            case DryingRules.DriedBerriesId:
            case DryingRules.DriedApplesId:
            case DryingRules.DriedMushroomsId:
            case DryingRules.DriedMeatId:
            case DryingRules.DriedFishId: return new[] { Drying };
            case SaltRules.SaltId:
            case SaltRules.SaltMeatId:
            case SaltRules.SaltFishId: return new[] { Salting };
            case SmokeRules.SmokedMeatId:
            case SmokeRules.SmokedFishId: return new[] { Smoking };
            case HideRules.DeerHideId:
            case HideRules.SmallFursId:
            case HideRules.SaltedDeerHideId:
            case HideRules.SaltedFursId: return new[] { Hides };
            case HideFrameRules.RawhideId:
            case HideFrameRules.DriedFursId: return new[] { HideFrame };
            case SmokeRules.SoftHideId:
            case SmokeRules.SoftFursId:
            case SmokeRules.BuckskinId:
            case SmokeRules.TannedFursId: return new[] { BrainTanning };
            case TanningVatRules.TanbarkId:
            case TanningVatRules.LeatherId: return new[] { BarkTanning };
            case GardenRules.HoeId: return new[] { Garden };
            // Gardening GB2 to GB4 and Greenhouses G1 to G6 (2026-11-27).
            case "row_cover":
            case CompostRules.FertilizerId: return new[] { BetterBeds };
            case GardenRules.TomatoSeedId: return new[] { SeedSaving };
            case "wire_mesh": return new[] { Fences };
            case ColdFrameRules.SashId: return new[] { ColdFrames };
            case HoopHouseRules.HoopId:
            case HoopHouseRules.PlasticId: return new[] { HoopHouses };
            case GlassHouseRules.PaneId:
            case "starts_lettuce":
            case "starts_kale":
            case "starts_squash": return new[] { GlassHouses };
            case GreenhouseHeatRules.StoveItemId:
            case GreenhouseHeatRules.HeaterItemId:
            case GreenhouseHeatRules.TankId: return new[] { GreenhouseHeat };
            case WateringRules.HoseId:
            case WateringRules.DripKitId:
            case WateringRules.HandPumpId:
            case WateringRules.FuelPumpId:
            case WateringRules.GasolineId: return new[] { Watering };
            case VentRules.ItemId: return new[] { Venting };
            default:
                // The first crop harvested (anything the garden grows, but not the bought seed potatoes) opens seed saving.
                if (itemId != GardenRules.PotatoesId && Array.IndexOf(GardenRules.ProduceIds, itemId) >= 0)
                    return new[] { SeedSaving };
                return Array.Empty<string>();
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
            case Happening.SmokehouseSitePlaced: return new[] { Smoking };
            case Happening.RootCellarSitePlaced: return new[] { Cellar };
            case Happening.SawPitSitePlaced: return new[] { Sawing };
            case Happening.HideFramePlaced: return new[] { HideFrame };
            case Happening.TanningVatPlaced: return new[] { BarkTanning };
            case Happening.SawmillSitePlaced: return new[] { Sawmill };
            case Happening.GardenBedPlaced: return new[] { Garden };
            case Happening.RaisedBedPlaced: return new[] { BetterBeds };
            case Happening.FencePlaced: return new[] { Fences };
            case Happening.ColdFramePlaced: return new[] { ColdFrames };
            case Happening.HoopSitePlaced: return new[] { HoopHouses };
            case Happening.GlassSitePlaced: return new[] { GlassHouses };
            case Happening.HeatPlaced: return new[] { GreenhouseHeat };
            case Happening.WateringPlaced: return new[] { Watering };
            case Happening.VentOpenerPlaced: return new[] { Venting };
            case Happening.DeliveryPadPlaced: return new[] { Deliveries };
            case Happening.BarnPlaced: return new[] { Barn };
            case Happening.DrilledWellPlaced: return new[] { DrilledWell };
            case Happening.PowerShedPlaced: return new[] { Power };
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
        MapleRules.SapId, PotashRules.LyeId, TripodRules.KettleId, MetalRules.CharcoalId, MetalRules.IronOreId, MetalRules.IronBarId, KnifeRules.ToolId, SpoilageRules.RotId,
        DryingRules.DriedBerriesId, DryingRules.DriedApplesId, DryingRules.DriedMushroomsId, DryingRules.DriedMeatId, DryingRules.DriedFishId,
        SaltRules.SaltId, SaltRules.SaltMeatId, SaltRules.SaltFishId,
        SmokeRules.SmokedMeatId, SmokeRules.SmokedFishId,
        HideRules.DeerHideId, HideRules.SmallFursId, HideRules.SaltedDeerHideId, HideRules.SaltedFursId,
        HideFrameRules.RawhideId, HideFrameRules.DriedFursId,
        SmokeRules.SoftHideId, SmokeRules.SoftFursId, SmokeRules.BuckskinId, SmokeRules.TannedFursId,
        TanningVatRules.TanbarkId, TanningVatRules.LeatherId,
        "row_cover", CompostRules.FertilizerId, GardenRules.TomatoSeedId, "wire_mesh", ColdFrameRules.SashId, HoopHouseRules.HoopId, HoopHouseRules.PlasticId,
        GlassHouseRules.PaneId, "starts_lettuce", "starts_kale", "starts_squash", GreenhouseHeatRules.StoveItemId, GreenhouseHeatRules.HeaterItemId, GreenhouseHeatRules.TankId,
        WateringRules.HoseId, WateringRules.DripKitId, WateringRules.HandPumpId, WateringRules.FuelPumpId, WateringRules.GasolineId, VentRules.ItemId,
        "field_corn", "sweet_corn", GardenRules.GreenBeansId, GardenRules.DryBeansId, "tomato", "lettuce", "carrot", "squash", "kale",
    };

    // The entries a loaded game has already earned: what the pack holds, and what it has built (a hopper site, a tripod, a tap). An old save
    // from before Know-how existed gets its help this way.
    public static List<string> Backfill(Func<string, bool> holds, bool hopperPlaced, bool tripodStands, bool tapsOut, bool pitPlaced = false, bool bloomeryPlaced = false, bool smithyPlaced = false,
                                        IEnumerable<Happening> built = null)
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
        // Anything else standing in the world that opens an entry (the garden and greenhouse builds, 2026-11-27).
        if (built != null)
            foreach (Happening h in built)
                triggered.AddRange(ForHappening(h));
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
            case MetalRules.IronBarId: return "Smelted in a Bloomery or bought ($20). Forge it at a Smithy; not bought back.";
            case SmithyRules.HingesId: return "Forged at a Smithy. Sells at the Trading Post for about $26.";
            case SmithyRules.HooksId: return "Forged at a Smithy. Sells at the Trading Post for about $18.";
            case SmithyRules.NailsId: return "Forged at a Smithy. Sells at the Trading Post for about $16.";
            case ToolRules.IronShovelId: return "Made at a Smithy or bought at the Trading Post; digs in a quarter fewer blows.";
            case ToolRules.IronPickAxeId: return "Made at a Smithy or bought at the Trading Post; mines in a quarter fewer blows.";
            case MetalRules.CharcoalId: return "Made from Logs in a Charcoal Pit. Sells for $1; iron smelting burns it.";
            // The Pole Barn kit parts (2026-11-27).
            case BarnRules.PostId: return "Part of a Pole Barn kit. Load it on the barn site, then Hammer.";
            case BarnRules.TrussId: return "Part of a Pole Barn kit. Goes up after the posts; Hammer at the site.";
            case BarnRules.SheetId: return "Part of a Pole Barn kit. Clads the roof and walls last.";
            case PhoneRules.PhoneId: return "Press P to use it. Start a $25/month plan in Bills & Plan.";
            // The power system (slice E1).
            case PowerRules.PanelId: return "400 W. Mount it on a Solar Panel Rack with R; four to a rack.";
            case PowerRules.BatteryId: return "5 kWh. Install it in the Power Shed with R; up to eight.";
            case PowerRules.InverterId: return "The heart of a Power Shed: building one takes it and 12 Lumber.";
            case PowerRules.GeneratorId: return "Set it down from the Build tab; load Gasoline with R. Starts itself.";
            case PowerRules.CableId: return "1 ft each. R at the Power Shed, then E on a building, wires it.";
            // The garden and the greenhouses (2026-11-27).
            case GardenRules.HoeId: return "E plants a dug bed (R picks the seed); R saves a growing square for seed.";
            case GardenRules.WateringCanId: return "Fill it at water, then press E at a garden bed to water it.";
            case "seed_field_corn":
            case "seed_sweet_corn":
            case "seed_beans":
            case "seed_lettuce":
            case "seed_carrots":
            case "seed_squash":
            case "seed_kale": return "Plant it with the Hoe at a dug bed (E). Seed keeps 2 to 4 years.";
            case GardenRules.TomatoSeedId: return "Saved seed. Sow it with the Hoe, or on a greenhouse's seedling bench.";
            case "tomato_plant":
            case "starts_lettuce":
            case "starts_kale":
            case "starts_squash": return "Plant it with the Hoe (E) within a week, or it wilts. It starts well grown.";
            case GardenRules.PotatoesId: return "Plant them with the Hoe at a dug bed. Keep some from the harvest as seed.";
            case "row_cover": return "R lays it over a garden bed against 3 C of frost; R takes it off again.";
            case "wire_mesh": return "A roll and a Log make a Deer Fence section (Build tab, Other).";
            case CompostRules.FertilizerId: return "E at a garden bed rebuilds worn soil; 2 dig a hotbed in a Cold Frame.";
            case ColdFrameRules.SashId: return "2 Sashes and 2 Lumber make each square of a Cold Frame (Build tab).";
            case HoopHouseRules.HoopId: return "Deliver to a Hoop House Site (Build tab). Also forged at a Smithy.";
            case HoopHouseRules.PlasticId: return "Deliver to a Hoop House Site, or E at a torn hoop house to refit it.";
            case GlassHouseRules.PaneId: return "For a Glass Greenhouse Site (Build tab), with Lumber, Stone and Nails.";
            case GreenhouseHeatRules.StoveItemId: return "Build it with 2 Stone in a greenhouse's heater spot; it burns Firewood.";
            case GreenhouseHeatRules.HeaterItemId: return "Build it in a greenhouse's heater spot; R loads Propane Tanks.";
            case GreenhouseHeatRules.TankId: return "R loads it into a Propane Heater in a greenhouse.";
            case WateringRules.HoseId: return "Makes a Hose Bib in a greenhouse or a Hose Reel outdoors (Build tab).";
            case WateringRules.DripKitId: return "E at a Hose Bib or Reel fits it: it waters the beds at 6 every morning.";
            case WateringRules.HandPumpId: return "Build it by the creek, a pond or a Well; E pumps water for the hoses.";
            case WateringRules.FuelPumpId: return "Build it by the creek or a pond; R loads Gasoline and it feeds the hoses.";
            case WateringRules.GasolineId: return "R loads it into a Fuel Pump; one can pumps about 400 L.";
            case VentRules.ItemId: return "Fit it to a Cold Frame or greenhouse; it opens and shuts the cover itself.";
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
