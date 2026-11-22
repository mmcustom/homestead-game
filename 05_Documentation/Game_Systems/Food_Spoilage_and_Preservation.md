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

---

# 6. Root Cellar

**Build (proposed):** a staged build site dug into the ground with the Shovel (like the Well's dig), lined with stone and logs, with an earth roof and a plank door. Proposed cost: 10 Stone, 8 Logs, 4 Lumber, plus the digging.

**How it works (proposed):** a storage container (like the Food Cache, but bigger: 300 kg) that stays at about 4 C (40 F) all year. Everything inside ages at x 0.3 in every season, and never freezes. Raw meat lasts about 10 days, apples about 6 weeks, eggs about 10 weeks, nuts most of a year, and preserved foods (smoked, dried, salted) far longer.

**Later (not in this build):** potatoes, carrots and other crops from farming will be the classic cellar foods.

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
