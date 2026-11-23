# Homestead
## Food Spoilage and Preservation

Status: Design Draft v0.1, 2026-11-22 (Claude, from Mike's decisions the same day). Nothing built yet. Tanning for hides and furs comes after this, in its own doc.

Mike's decisions (2026-11-22):
- Food that goes past its shelf life turns into **Rot**. Rot goes into a **compost container** with straw or tall grass to make **fertilizer** for future farming and crops.
- **Temperature matters:** heat spoils food faster, cold slows it, freezing nearly stops it.
- First build includes all four ways of keeping food: **Smokehouse, Drying Rack, Root Cellar and Salt curing.**

Everything marked "proposed" below is Claude's suggestion for Mike to confirm or change.

---

# What exists today

- Every food item already has a shelf life in days (ItemDefinition.shelfLifeDays; values in Item_Data.md): fish 2, milk 2, raw meat 3, berries 3, mushrooms 4 to 5, cooked meat and fish 4 to 5, apples 14, eggs 21, chestnuts 30, hickory nuts and walnuts 60.
- Every stack of perishable food already keeps the day it was acquired, in the pack, in storage (Food Cache, piles) and when dropped. Nothing ages it yet.
- Late field dressing already takes days off the meat's life (Carcass.cs).
- DifficultyManager.SpoilageMultiplier already exists: Settler 0.75, Pioneer 1.0, Homesteader 1.3 (unused).
- Food Cache: an offload point with no spoilage bonus, on purpose.

---

# 1. Spoilage (the core)

**How food ages (proposed).** Each batch has a freshness clock. It runs at a rate set by the air temperature where the food is, times the difficulty multiplier. The shelf life in Item_Data.md is the life at a mild 10 to 20 C (50 to 68 F).

| Air temperature | Rate | Example: raw venison (3 days) |
|---|---|---|
| Below 0 C (32 F), frozen | x 0.1 | about a month |
| 0 to 4 C (32 to 40 F) | x 0.3 | about 10 days |
| 4 to 10 C (40 to 50 F) | x 0.6 | 5 days |
| 10 to 20 C (50 to 68 F) | x 1.0 | 3 days |
| 20 to 27 C (68 to 80 F) | x 1.5 | 2 days |
| Above 27 C (80 F) | x 2.0 | 1.5 days |

The rate blends smoothly between the rows, so there is no sudden jump at a line. Food in the pack, in the Food Cache, in piles and dropped on the ground all use the outdoor temperature. The Root Cellar has its own (see 6).

**Freshness shown to the player (proposed).** Every perishable stack shows how long it has left in plain, large text, for example "Venison x3 - 2 days left". The colour changes as it ages: normal while fresh, amber in its last half ("aging"), red in its last fifth ("going off"). Looking at a storage container shows the same.

**Spoiling.** When a batch reaches the end of its life it turns into Rot, one Rot for each unit, wherever it is. A short message says what spoiled ("3 Venison spoiled and became Rot"). Eating food just before it spoils has no penalty in this first version.

**Rot (new item).** Not edible, does not spoil further, 0.3 kg each (proposed), sells for nothing. Its only use is compost.

**Technical note.** Because the rate changes with temperature, a single "acquired day" is not enough. Proposed: one freshness clock for the outdoors that the game advances every hour by the current rate, and one for the Root Cellar. A batch records the clock reading when it was made fresh. Moving food between the outdoors and the cellar converts the reading so the food keeps exactly the life it had left. Old saves: every existing batch is converted using its acquired day, so nothing spoils the moment the update loads.

---

# 2. Compost container and fertilizer

**Compost Bin (new build, proposed):** a slatted box of sticks and cordage (6 Sticks, 2 Cordage). Holds up to 40 units.

**Making fertilizer (proposed):** 4 Rot plus 2 Tall Grass makes 1 Fertilizer. When straw exists (hay and straw are not built yet) it can stand in for the Tall Grass. Compost takes about 7 days (Mike 2026-11-22: shortened from 14, "maybe down to a week") at mild temperatures, slower when it is cold, and stops while it is frozen. The bin shows its progress.

**Fertilizer (new item):** 1 kg, does not spoil. Until farming exists it can be sold at the Trading Post for a small amount (proposed $0.50 each), so compost has a use from day one.

---

# 3. Drying Rack (early, cheap)

**Build (proposed):** 4 Sticks, 2 Branches, 2 Cordage. Stands outdoors and holds 12 pieces.

**How it works:** food hung on the rack dries in the open air over about 2 days for berries, apple slices and mushrooms, and 3 days for meat and fish strips. Rain or snow pauses it, so does night, and while drying the food is still ageing (slower than in the pack: x 0.5), so fish hung in a heat wave can still be lost. A full dry turns each piece into its dried form.

**Dried foods (new items, proposed):**

| Dried item | Made from | Shelf life | Notes |
|---|---|---|---|
| Dried Berries | Blackberries, Raspberries | 120 days | |
| Dried Apples | Wild Apples, Pawpaw | 120 days | |
| Dried Mushrooms | Morel, Oyster, Dryad's Saddle | 180 days | |
| Dried Meat (jerky) | any raw meat | 90 days | edible without cooking |
| Dried Fish | any raw fish | 90 days | edible without cooking |

Dried food is lighter than fresh (most of the water is gone), so it is easier to carry, and it sells at the Trading Post.

---

# 4. Salt curing

**Salt (new item):** sold at the Trading Post, 1 kg bags (proposed $2 each). Does not spoil.

**Salting Crock (proposed):** made at the Potter's Shed and fired in the Kiln, like the other pottery. Holds up to 10 kg of meat or fish.

**How it works (proposed):** 1 Salt for every 2 kg of raw meat or fish. Curing takes 4 days at any temperature above freezing. The result is Salt Meat or Salt Fish, which keeps for 180 days and is edible as it is.

---

# 5. Smokehouse (the main meat and fish keeper)

**Build (proposed):** a staged build site like the Small Cabin: a small plank-and-log house over a fire pit, about 2 x 2 m. Proposed cost: 12 Logs, 6 Lumber, 10 Stone, 4 Clay.

**How it works (proposed):** load up to 20 pieces of raw meat and fish, add firewood, light it. A load takes 1 day of smoking and uses 6 Firewood. The fire must stay lit; if it runs out of wood, smoking pauses and the meat starts ageing again. Smoking works in any weather (it has a roof).

**Smoked foods (new items, proposed):** Smoked Meat (from any raw meat) and Smoked Fish (from any raw fish), 180 days each, edible as they are. They are the best-selling preserved food at the Trading Post.

**Smoke:** the smokehouse puffs smoke while it works, like the Kiln and Charcoal Pit.

**Slice 5 as built (2026-11-22).** Mike's decisions: a staged build like the Kiln ("Staged build"); smoked food is the top of the line ("Top of the line"); the fire works as proposed ("As proposed"). Checked by Mike in Play Mode 2026-11-23 ("everything looks great").

- **SmokeRules** (Scripts/Economy, pure) and the hourly burn in **SpoilageManager.SmokingTick / BurnHour**. PileKind gains SmokehouseSite (47) and Smokehouse (48), appended.
- **Build:** a staged site (Build tab, Other): 10 Stone, 4 Clay, 12 Logs and 6 Lumber in any order (first built Stone and Clay first; changed 2026-11-23 when Mike's Lumber was refused), about 190 kg; 48 Hammer blows. Open sky, 3 m clear of other buildings and trees both ways (the Kiln's rule, reach 1.2 m).
- **Use:** R loads raw meat and fish (up to 20 pieces, only between loads) and Firewood (up to 12, any time); E lights it (one Firewood is enough to light; the prompt warns when there are fewer than 6). A load smokes 24 game hours; one Firewood burns 4 hours, so a load burns 6. Out of wood, the smoking stops with its hours left and the meat ages again; loading Firewood starts it again from the embers. While the fire burns the meat does not age (its stamp moves on with the outdoor clock).
- **Yield and price:** 1 Smoked Meat or Smoked Fish per dollar the raw piece is worth cooked (Venison 4, Bass 3, Bluegill 2), $3.00 each, so smoked pays more than salted, dried, cooked or raw even after the Firewood. Smoked food keeps 180 days and is eaten as it is (Smoked Meat 0.15 kg, hunger 6; Smoked Fish 0.1 kg, hunger 3.5).
- E collects the smoked food (with its freshness) before lighting again. The Hammer takes it down for half the materials, with the meat, Firewood and smoked food whole, but not while a load is in.
- **Know-how 16:** "Smoking Meat and Fish", unlocked by holding smoked food or marking out a Smokehouse Site.
- **Market:** rows 106 and 107 (Smoked Meat in GameMeat, Smoked Fish in Fish), Bought.
- **Art:** structure_smokehouse (with smoke from its roof vent and a glow at the fire pit), item_smoked_meat, item_smoked_fish, one at a time.

---

# 6. Root Cellar

**Build (proposed):** a staged build site dug into the ground with the Shovel (like the Well's dig), lined with stone and logs, with an earth roof and a plank door. Proposed cost: 10 Stone, 8 Logs, 4 Lumber, plus the digging.

**How it works (proposed):** a storage container (like the Food Cache, but bigger: 300 kg) that stays at about 4 C (40 F) all year. Everything inside ages at x 0.3 in every season, and never freezes. Raw meat lasts about 10 days, apples about 6 weeks, eggs about 10 weeks, nuts most of a year, and preserved foods (smoked, dried, salted) far longer.

**Later (not in this build):** potatoes, carrots and other crops from farming will be the classic cellar foods.

**Slice 6 as built (2026-11-23).** Mike's decisions: dug out with the Shovel, then built ("Dig, then build"); a room you walk into ("Walk inside"); 300 kg ("300 kg"). Awaiting Mike's check in Play Mode.

- **RootCellarRules** (Scripts/Economy, pure). PileKind gains RootCellarSite (49) and RootCellar (50), appended.
- **Build:** a staged site (Build tab, Storage and Piles). Mark it out, dig it out with the Shovel (30 blows, like the Well's shaft), then deliver 10 Stone, 8 Logs and 4 Lumber in any order and hammer it up (40 blows). Dry ground out of hollows, no trees on it, spaced like a wide shed.
- **The room:** an earth-covered room at ground level (a bermed cellar: the dug earth heaped over its log roof), 2 m wide and 2.6 m deep inside, 2.3 m to the roof, entered through a 1.1 by 2.05 m doorway in its stone front with a swinging plank door (SwingDoor). Its walls, roof and front are solid; the room counts as under a roof.
- **Use:** E at the shelves on the back wall, from inside the room or its doorway, opens the storage screen; from outside the prompt says to walk in. Food only, up to 300 kg by weight; the screen shows "N of 300 kg". Everything in it ages on the Cellar clock, a steady x 0.3 all year (never freezing); stamps convert on the way in and out, as for the Food Cache. Stored food shows as boxes on the shelves.
- **Know-how 17:** "The Root Cellar", unlocked by marking out a Root Cellar Site.
- **Doors (Mike, 2026-11-23):** "doors that swing open when we walk into them". SwingDoor (Scripts/World) swings away from the player as they come up to a doorway, in or out, and shuts behind them; its collider closes the doorway only while it is shut. On the Small Cabin, the Smokehouse (its baked-open door taken out of the art) and the Root Cellar.
- **Art:** structure_rootcellar and its build icons next.

---

# 7. Trading Post (proposed)

- Buys: Smoked Meat, Smoked Fish, Dried Meat, Dried Fish, Salt Meat, Salt Fish, Dried Berries, Dried Apples, Dried Mushrooms, Fertilizer. Preserved meat and fish pay more per kg than fresh.
- Sells: Salt.
- Rot: not bought.

---

# 8. Know-how journal

New entries: Spoilage and Rot, Compost, Drying, Salt Curing, Smoking, Root Cellar. Plain language, large text.

---

# 9. Art needed (Wave 9 rows move into the build)

Structures: Compost Bin, Drying Rack (empty and loaded), Smokehouse (with smoke), Root Cellar (door and earth mound). Items: Rot, Fertilizer, Salt, Salting Crock (raw and fired), Dried Berries, Dried Apples, Dried Mushrooms, Dried Meat, Dried Fish, Smoked Meat, Smoked Fish, Salt Meat, Salt Fish, each with its dropped art and inventory icon.

---

# 10. Build order (proposed)

1. **Spoilage core:** the freshness clocks, temperature rates, difficulty multiplier, Rot, freshness shown in large text, old-save conversion, tests.
2. **Compost Bin and Fertilizer.**
3. **Drying Rack** and the dried foods.
4. **Salt and the Salting Crock.**
5. **Smokehouse.**
6. **Root Cellar.**
7. Then **Tanning** (its own design doc).

Each slice is played and approved by Mike before the next starts, with its art made in Blender as part of the slice.

---

# Slice 1 built (2026-11-22, Claude): the spoilage core

Approved by Mike in Play Mode 2026-11-22 ("the spoilage and food storage cache times are good"). Code:

- **SpoilageRules** (Scripts/Economy, pure): the rate table (blended between -1, 2, 7, 15, 23.5 and 30 C at x0.1, 0.3, 0.6, 1.0, 1.5, 2.0), the three clocks (Outdoor; Cache x0.8; Cellar a steady x0.3), stamps (whole clock hours rounded to 6, never 0), Convert between clocks, the stages (fresh, aging past half, going off past 80%), the sickness chance (0 up to half its life, rising to 15% at the end, combined with the food's own chance), the "N days left" text and the old-save conversion.
- **SpoilageManager** (Scripts/Managers, created on startup like DroppedItems, saved in the world file as "spoilage"): advances the clocks every game hour by WeatherManager.TemperatureC and DifficultyManager.SpoilageMultiplier, then sweeps the pack, every storage container, every pile and structure (the Food Cache on its own clock) and every dropped pile. Spoiled batches become Rot (one each); the pack says so on screen.
- **Stamps:** ItemStack.acquiredDay and WoodStack.day now hold the freshness stamp for perishables (clock hours) and the wet day for wood, as before. InventoryManager.AddTo stamps new food from the outdoor clock; the Carcass takes late-dressing days off as hours; the Food Cache converts stamps on the way in and out (WoodPile.ToPile/FromPile, and when it is taken down); StructureRefund keeps a stamp on food left on the ground and gives unstamped food back fresh.
- **Old saves:** SaveManager.Loaded (new, fired after every section has loaded) lets SpoilageManager convert a save with no spoilage section: each batch keeps its age, capped at half its life.
- **Eating:** Food.Consume takes the oldest batch and adds its age risk; the message says "(it was going off)" when that made the player ill.
- **Display:** the Inventory screen and the storage screen show the oldest batch's time left in green, amber or red (size 20).
- **Rot:** new item (Resources/Items/Resources/rot.asset, 0.3 kg, Resource, kept in the Storage Bin until the Compost Bin exists), art item_rot (a slumped dark heap with half-rotted lumps and mould, 3,248 tris, Art/Blender_Work/item_rot) and its icon.
- **Tests:** SpoilageTests (rules, migration, a container turning spoiled food into Rot).

# Answers from Mike (2026-11-22)

1. Temperature rates and shelf lives: sound good; Mike confirms after playing.
2. Eating old food: **a slight chance of feeling sick, rising the closer the food is to spoiling.** Proposed: no risk while fresh, rising from 0% at half its life to about 15% just before it spoils (a per-bite roll). Feeling sick uses the existing sickness from unsafe water if it fits, otherwise a short mild "upset stomach" (some lost food and water over a few hours).
3. Food Cache: **yes, a little preservation.** Proposed x 0.8 of the outdoor rate (shaded and off the ground).
4. Costs and times: fine as starting values, tuned in play.


# Slice 3 built (2026-11-22, Claude): the Drying Rack

Approved by Mike in Play Mode 2026-11-22 ("looks great"). Numbers are the doc's proposals unless noted.

- **DryingRules** (Scripts/Economy, pure): what dries into what (Blackberries and Raspberries to Dried Berries; Wild Apples and Pawpaw to Dried Apples; Morel, Oyster Mushroom and Dryad's Saddle to Dried Mushrooms; Venison, Turkey, Small Game, Waterfowl and Chicken Meat to Dried Meat; Bass, Bluegill, Catfish and Crappie to Dried Fish). Fruit and mushrooms need 24 dry daylight hours (about 2 days), meat and fish 36 (about 3). Only hours with the sun up and no rain or snow count. The rack holds 12 fresh pieces at once.
- **Yield:** fruit and mushrooms dry one for one. Meat and fish are cut into strips, two for every dollar the piece sells for cooked (a Venison gives 8, a Bass or Turkey 6, a Small Game Meat, Chicken Meat or Bluegill 4). Changed from one per 0.3 kg the same day, after Mike's "make salted meat and dried meat more profitable than just cooked meat and even more than fresh raw meat": by weight, small pieces were worth less dried than raw.
- **Ageing on the rack:** a fourth freshness clock, the Rack, runs at x 0.5 of the outdoor rate (saved with the others; an older save starts it at the outdoor clock). Food moved on or off converts its stamp, as with the Food Cache.
- **Progress:** each batch on the rack keeps its own drying hours (WoodStack.dried, appended). Fresh food hung later starts its own batch; food taken off early comes back fresh and loses its progress.
- **Dried foods** (Resources/Items/Consumables, all edible without cooking): Dried Berries 0.1 kg, 120 days, hunger 6; Dried Apples 0.12 kg, 120 days, hunger 8; Dried Mushrooms 0.06 kg, 180 days, hunger 4; Dried Meat 0.08 kg a strip, 90 days, hunger 3; Dried Fish 0.06 kg a strip, 90 days, hunger 1.5.
- **Trading Post** (proposed): Dried Berries $1.50, Dried Apples $1.50, Dried Mushrooms $2.00 (Forage group); Dried Meat $0.80 a strip (Game Meat); Dried Fish $0.80 a strip (Fish). Every meat and fish is worth more dried than cooked, and cooked more than raw, even after the price drops 5% with each strip sold (a test checks it). Five rows appended to MarketData; no other price touched.
- **Build:** 4 Sticks, 2 Branches, 2 Cordage, one click with the Hammer, like the Compost Bin. PileKind.DryingRack (46, appended).
- **Screens:** the rack's screen says "6 of 12 drying" and why drying is paused (night, rain, snow); each fresh row says how many daylight hours it still needs. Know-how entry 14, "Drying Food on a Rack" (Claude's draft text), unlocks with the first dried food held.
- **Art:** structure_dryingrack and its build icon, and the five dried foods with their icons, each approved by Mike.
- **Tests:** SpoilageTests (slice 3 section), and the kind, row and Know-how counts updated.

# Slice 4 built (2026-11-22, Claude): salt curing

Awaiting Mike's check in Play Mode. Mike's decisions: the crock is used from the pack, like boiling sap; the crock is the Crock Pot already made at the Potter's Shed and fired in the Kiln (a new Salting Crock would not fit the Potter screen's five large rows); salting must pay more than cooking or selling raw.

Renamed 2026-11-22 (Mike): the Water Crock is now called the **Crock Pot** everywhere players see it (it still holds water and is the salting crock). Item ids stay water_crock and dry_water_crock so saves keep working.

- **SaltRules** (Scripts/Economy, pure) and **Salting** (Scripts/Survival): carrying a Crock Pot, raw meat and fish rows in the Inventory get a Salt button when no fire is in reach (at a fire the Cook button takes the place). Each raw piece takes 1 Salt for every 2 kg (at least 1; a Venison takes 2) and is cut into Salt Meat or Salt Fish at once, one for every dollar it sells for cooked (a Venison gives 4, a Bass or Turkey 3, a Small Game Meat or Bluegill 2). The crock is not used up. Salting is instant, so the doc's 4-day cure is dropped (Mike can ask for it back).
- **Items:** Salt (1 kg bag, does not spoil); Salt Meat (0.18 kg, 180 days, hunger 6) and Salt Fish (0.12 kg, 180 days, hunger 3.5), both eaten as they are.
- **Trading Post** (proposed): sells Salt for $2; buys Salt Meat and Salt Fish at $2.50 each. Salted meat and fish pay more than cooked, even after the Salt and the 5% drop per piece sold (a test checks it). Three rows appended to MarketData.
- **Know-how** entry 15, "Salting Meat and Fish" (Claude's draft text), unlocks with the first Salt held.
- **Art:** item_salt done; item_salt_meat and item_salt_fish next, one at a time. No crock art (the Crock Pot has its own).
