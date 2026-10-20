# Covered Outdoor Kitchen Add-ons (design draft, Claude, not yet confirmed)

Status: SCOPE CONFIRMED by Mike 2026-10-19; rules below are drafts. NOT built. Comes after Slice 1 phase C (the Covered Outdoor Kitchen itself) is playtested and committed. First step is a read-only pass by Claude Code (questions at the end).

## Scope (Mike, 2026-10-19)

Mike's words: "you can add countertops, and cleaning tables for processing harvested plants and animals, a slop sink, shelving and a wood fired grill / griddle. but everything else should wait on the other power systems."

IN scope (this add-on set):
1. **Countertops**
2. **Cleaning (processing) tables** for processing harvested plants and animals
3. **Slop sink**
4. **Shelving**
5. **Wood-fired grill / griddle**

OUT of scope until the power systems exist: LP gas appliances, electric appliances, and anything else that needs fuel supply or power (refrigeration, electric stove or oven, lights on power, and so on). These wait on Power_System and the other power systems.

## Rules that already apply (from Mike's earlier decisions)

- **Build tab = permanent structure, Craft tab = crafted item.** Permanent pieces need the Hammer to build or tear down. Placing the ghost, delivering and taking materials back (before pounding) are free.
- **Heavy builds (over about 15 kg) become staged sites** with Hammer pounding. Light ones stay single-step.
- **No time jumps in any work.** Real time only.
- **Old saves load unchanged.** New PileKinds are appended, never renumbered. No new save keys where the existing stamps can be reused.
- Each piece gets a finished-structure ghost, Setbacks-style named refusal reasons, a Hammer Dismantle (half refund rounded down), and a playtest checklist.

## Draft rules (Claude, not yet confirmed)

**General shape.** Each add-on is a placeable permanent piece that must be placed inside or against a finished Covered Outdoor Kitchen (a short reach from the kitchen's roof footprint), so a kitchen with nothing in it is still the roof, hearth and prep table that phase C built. Whether add-ons snap to fixed slots on the kitchen or are free-placed inside its footprint is a question for the read-only pass (below). The finished kitchen has no interaction box today, so each add-on needs its own interactable.

1. **Countertop.** A work surface. First pass: carpentry cost (Logs and Branches, or planks if the game has them); light enough to be a single-step build. Its job is to give the other processing and cooking actions a place to happen under cover, and to carry the "prepared meal" rules (a meal prepared at a counter, not on the ground). Rule detail depends on what the cooking screen does today (read-only pass).
2. **Cleaning table.** A table to process what you harvest or kill: plants (wash, trim, sort) and animals (clean fish, dress and butcher game, pluck birds). Real basis: fish cleaning tables are the standard way to process a catch; processing at a clean raised surface keeps meat clean, and the real-world rule is that the cleaning surface and tools are washed after each use. First pass: processing at the table needs a Knife for animals (matching the Knife rule for traps), runs in real time (hold click, like digging and pounding, no clock jump), and produces the same cleaned products the game already makes. Whether processing at the table is better than processing in the field (more yield, less spoilage, cleaner food) is a design choice; see decisions.
3. **Slop sink.** A basin for washing and for dumping dirty water. There is no plumbing, so first pass is a basin you pour water into (from a carried Bucket or a Water Barrel), and the dirty water drains out to the ground. Washing produce or hands at it counts as clean prep. Greywater handling (reuse, a drain field) is on the earlier backlog and is NOT part of this set; the slop sink just drains.
4. **Shelving.** Storage for food, jars and kitchen goods, like the Storage Bin but kitchen-scoped and held under the roof so contents are kept dry. It uses the same transfer screen as the other storage. Capacity and item rules to be decided after the read-only pass.
5. **Wood-fired grill / griddle.** A cooking surface that sits at the kitchen hearth or beside it and burns wood like a Campfire (same wet/dry batch rules, same burn times, same Flint and Steel and Bow Drill lighting). Grill = direct heat for meat and fish; griddle = flat surface for flatbread, eggs and vegetables. Whether these are a second cooking station or a recipe-set option at the existing hearth is a question for the read-only pass. If the game has no metal items (iron or steel), the first pass uses stone or clay for the surface and the metal grate version waits.

## What stays untested / unknown

All of it. Nothing is built. Every number above is a first-pass guess to be replaced by the read-only findings.

## Decisions needed from Mike (after the read-only pass)

- Add-ons snap to fixed slots on the kitchen, or are free-placed anywhere inside its footprint?
- Is processing at the cleaning table better than processing in the field (yield, spoilage, illness risk), or just a nicer place to do it?
- Slop sink: a basin you pour water into (no plumbing), drains to the ground. OK for now?
- Grill and griddle: a second cooking station beside the hearth, or extra recipes at the hearth itself?
- Build order inside this set (suggested: countertop and shelving first, then the cleaning table and slop sink, then the grill/griddle).

## Questions for the Claude Code read-only pass

1. **Kitchen and attachment:** how does the finished Kitchen (kinds 22-24, `PlacedStructure`, `WellsAndOuthouses` staged-site data) expose its footprint and its hearth position? What would it take for other structures to be placed inside or attach to it (slots, a footprint test, or a parent link), without new save keys?
2. **Processing today:** how does the game process harvested animals and plants now (dressing game with the Knife, cleaning fish, plucking, butchering, cleaning plants)? Where is it implemented, does it take game time or real time, what does it produce, and what does the Knife rule check? Is there any "cleanliness" or spoilage state on meat, fish or produce that a clean surface could affect?
3. **Cooking:** how does the Cooking screen find a heat source today (Campfire, cabin hearth)? Could a grill or griddle be a heat source of its own, or a recipe filter on the hearth? Which recipes exist that would belong on a grill versus a griddle?
4. **Water for the sink:** how does a Bucket or Water Barrel hand over water, and could a basin accept and drain it? What does washing hands or produce affect today, if anything? Anything existing about dirty water or greywater?
5. **Storage:** what does the Storage Bin and its transfer screen allow, and is there a spoilage or dryness concept that shelving under the roof could use?
6. **Materials:** is there a plank or lumber item, and any iron or steel, in the item list today? What do Logs, Branches, Stone and Clay give, and what are their weights (so each add-on stays at or under the 15 kg single-step line or becomes a staged site)?
7. **Interaction and ghosts:** what does it take to give each add-on a ghost, an interactable and a Dismantle consistent with the existing staged sites? Which existing scripts would change?
8. **Risks:** anything in phase C's finished kitchen (colliders, the hearth, the roof raycasts for shelter and shade, the Bow Drill cover test) that add-ons placed under the roof could break?


---

# READ-ONLY FINDINGS from Claude Code (2026-10-19)

Claude Code changed no code, assets, scenes or docs and made no commits. These are findings from reading the code; nothing was run in Play Mode. Where these differ from the draft rules above, THESE WIN.

## 1. Kitchen and attachment
- The kitchen is one `WoodPileState` (kind 24 when finished); its position is the centre of the roof, plus yaw. Stage is carried by the kind (22, 23, 24) through `WellsAndOuthouses.FinishedKind`.
- `Setbacks.KitchenLayout` (Survival/Setbacks/Setbacks.cs) holds the only shared layout numbers: HalfWidth 1.7, HalfDepth 1.5, RoofHeight 2.45, HearthLocal (-0.35, 0.14, 0.15). Methods: `UnderRoof(pivot, yaw, point)` and `HearthAt(pivot, yaw)`. `WoodManager` adds `KitchenHearthSpot`, `KitchenHearthOf` (a FireNear at 0.75 m) and `KitchenRoofAt(point)` (the finished kitchen whose footprint contains a point).
- There is NO parent/child link, slot table or registry of contents. The prep table is hard-coded inside `StructureVisuals.BuildKitchen` (World/PlacedStructure.cs) at local (0.95, -1.2), a 1.1 x 0.6 top at 0.85 m, not a constant or method.
- **Attaching without new save keys:** an add-on is its own `WoodPileState` with an appended kind. The parent is found each time by `KitchenRoofAt(addon.position)` and never stored (same pattern as the bin hearth and the Rain Barrel eave). Two ways: (a) fixed slots, a table of local-space spots with a snap in PlacementMode (precedent: `WoodManager.EaveSpots` and `PlacementMode.TrySnapToEave`, using `GroundAt()`); (b) free placement inside the footprint, tested with `UnderRoof`. Reusable fields if an add-on needs state: litres, builtDay, digProgress, contaminatedHours, buildProgress, contents (stacks with day stamps), yaw.
- **Placement rules that would reject an add-on today (`WoodManager.CanBuildPileAt`):** every pile must be at least `minSpacing` (2 m) from every other pile's position, and the kitchen pivot is within about 2.3 m of every point under the roof, so most add-ons would be refused "Too close to another pile". A lit fire within 2 m refuses "Too close to the fire". Precedent for an exemption: the Rain Barrel skips the cabin spacing check.
- **Aim problem:** `PlacementMode.Evaluate` takes the first non-player collider the aim ray hits, and only terrain counts as ground. Aiming at ground under the roof from the side hits planks, posts or the table: "No clear ground there". Slot snapping with `GroundAt` avoids this.
- **Space is tight:** footprint 3.4 x 3.0 m. The hearth slab (1.5 x 1.5 at x -0.35, z 0.15) and the prep table (x about 0.4 to 1.5, z about -1.5 to -0.9) already use it. Free: a strip along the left, the right-front area, and some of the back-left. Claude Code does not think all five add-ons fit inside.
- **Dismantle:** `DismantleKitchen` knows nothing about add-ons, so they would be orphaned. It needs a "remove the add-ons first" refusal, like the lit-hearth refusal.

## 2. Processing today
- The only processing step is field dressing, in `Wildlife/Carcass.cs` (E, Interact). It is instant (a single press; the header says "hold E" but the code is not a hold). It takes no game-clock time, but yield depends on the clock: full meat within 1 hour of death, 75% within 3, 50% within 8, spoiled after that. The carcass rots away after 24 hours and is not saved.
- Output: meat from `WildlifeManager.YieldFor` (venison, turkey_meat, waterfowl_meat, small_game_meat, chicken_meat). Deer add hide and antlers (seasonal odds), sinew from deer and turkey, feathers or fur as the species extra, and a recovered arrow. Meat is stamped today minus spoilageShift (0, 1 or 2 days).
- Knife rule: `Knife.Carried` (Wildlife/Knife.cs) is just `Player.Has("knife")`; `TrapManager` (around line 407) uses the same check for small game in traps. Fish do not need a Knife.
- NOT implemented at all: cleaning fish, plucking, butchering, plant washing or trimming. Fish come out whole (bluegill, crappie, bass, catfish) and go straight to cooking. Forage is edible as picked.
- **Important constraint:** a carcass cannot be carried. It is a world object dressed where it lies, and trap catches stay in the trap until dressed. A table "for processing harvested animals" has nothing to process unless a way to carry a carcass, or a new pre-dressed item, is added.
- **Cleanliness or spoilage: none that does anything.** Perishables keep a batch day stamp and `shelfLifeDays` (ItemDefinition, InventoryContainer), but no code reads them to spoil, expire or rot food ("not built yet" in a DifficultyManager comment). The only live food-risk lever is the fixed `illnessChance` per item (0.3 raw meat, 0.2 raw fish). A clean surface would need a new mechanic or a modifier on that chance.

## 3. Cooking
- `Cooking.FireInReach(player)` calls `FireManager.NearestLit(player position)`. It accepts any lit `CampfireState`, so campfires, the cabin hearth and the kitchen hearth are identical to it. No station type, no recipe filter.
- Cooking is queued from the Inventory screen's Cook and Boil buttons and run by `CampfireCooking` in real time (fish 4 s, small game and chicken 5 s, birds 6 s, venison 8 s, boil 6 s per litre), no extra fuel. Recipes: a static table of 9 (`Cooking.Recipes`): venison, turkey, waterfowl, chicken, small game, bluegill, crappie, bass, catfish. Boiling also needs the Cooking Pot.
- **Grill/griddle as a heat source:** simplest fit is a second `CampfireState` made with `FireManager.BuildFurnished`. It gets fuel, wet/dry burn, Flint and Steel, Bow Drill, cover test, sound, saving and `NearestLit` for free. It must stay more than 0.75 m from the hearth or `KitchenHearthOf` could pick the wrong fire, and `DismantleKitchen` would have to remove it too.
- **Grill vs griddle:** all 9 recipes are meat or fish, so every one is grill-type. Nothing is griddle-type: eggs, goat milk, cattail, onion, mushrooms, nuts and fruit have no recipe, and there are no grain or flour items. A griddle recipe filter would be empty until a recipe set exists.

## 4. Water for the sink
- The Bucket holds 10 L and the Canteen 2 L (tracked as capacity, not a container with contents). Water is one item per litre per quality (water_excellent, good, questionable, unsafe, purified), 1 kg each. Sources (`WaterSource`, the Well's `PlacedStructure.DrawWater`, the barrels) fill an equipped Bucket or Canteen. A Water Barrel is a WoodPile whose contents are water stacks, capped by `CapacityOf` (40 L; the Rain Barrel 80 L); R pours carried water in, and the transfer screen draws it out per quality.
- A basin could accept water by reusing the barrel's pour and cap logic as a new kind (widening `IsBarrel`/`IsWaterBarrel` checks), or as a `PlacedStructure` that stores litres. Draining is just removing litres. There is no drain concept, ground wetting or sound.
- **Washing affects nothing today:** zero hits for wash, rinse, slop, greywater or dirty water; no hygiene stat; nothing edible has an illness chance washing could cut apart from meat, fish and raw-water items. Related existing mechanics: the Outhouse pit fill and sanitary flag, and well `contaminatedHours` / `Setbacks.Fouls`. A drain that fouls a nearby well would be a new rule following that pattern.

## 5. Storage
- Storage Bin (PileKind.StorageBin, 6 Branches + 6 Sticks) accepts IsDryGoods: anything that is not a tool, wood, Stone, water or food (cordage, hides, furs, feathers, arrows, rounds, lumber, clay, lamp oil). Food Cache accepts any food that is not water and keeps each batch's day stamp. Only the Water Barrel has a capacity limit.
- Transfer screen: `StorageTransferScreen` (E or R), typed to WoodPile; click moves one item, Shift-click the stack. Shelving would use it as a WoodPile kind. Wood Shed is the precedent (a roofed WoodPile with `WoodPile.UsesTransferScreen`).
- Only wood has a dryness concept (`WoodMoisture`; `RainOnOpenPiles`). Food spoilage does not exist, so shelving "kept dry" has no mechanical benefit for food today; it could keep day stamps like the Food Cache and nothing more.

## 6. Materials
- Lumber exists (5 kg, a Material), sells at the Trading Post for $8, and nothing consumes it yet. No other planks (the kitchen's planks are visuals built from Logs and Branches).
- NO metal: no iron, steel or metal material. `cooking_pot` and `flint_and_steel` are tool items. DifficultyManager says Iron and Steel do not exist yet, so a metal grate waits.
- Weights (kg): Logs 8, Branches 1, Sticks 0.1, Stone 5, Clay 3.5, Tall Grass 0.1, Cordage 0.3, Firewood 1.5, Lumber 5. Staged-site line `stagedSiteWeightKg` = 15 (on WellsAndOuthouses). Examples: 2 Logs (16 kg) staged; 1 Log + 4 Branches (12 kg) single step; 2 Stone + 2 Clay (17 kg) staged; 4 Stone (20 kg) staged.

## 7. Interaction, ghosts and Dismantle
- **Route A, storage-like (shelving): WoodPile route.** Change: WoodManager (enum, PileName, CostOf, Accepts, Buildable, NeedsHammerToBuild), WoodPile (UsesTransferScreen, a Rebuild visual), PlacementGhost.ForPile, CraftingScreen (PileHelp), PlacementMode.FacingMatters. Dismantle: `WoodPile.RemoveStructure`.
- **Route B, work surfaces (counter, cleaning table, sink, grill): PlacedStructure route.** Change: WoodManager (enum, IsPlacedStructure, PileName, CostOf, Buildable), PlacedStructure (prompt, Interact and SecondaryInteract cases, Rebuild colliders), StructureVisuals.Build, CraftingScreen (PileHelp), PlacementMode.FacingMatters. The ghost comes free for any IsPlacedStructure kind through StructureVisuals.Build. Hammer Dismantle: the generic branch in `PlacedStructure.Dismantle` already does half of CostOf rounded down, so each add-on needs only a CostOf entry. Cleaning-table or sink behaviour would be new code in PlacedStructure or a new component.
- If staged (over 15 kg): add the WellsAndOuthouses data rows (IsStagedSite, FinishedKind, FinishedCost, DeliveryOrder, BuildBlows). Hammer pounding in `AxeTool.UpdateHammering` is generic over IsStagedSite.
- All of them need the spacing and fire exemptions and slot snapping from section 1, in `WoodManager.CanBuildPileAt` and `PlacementMode`.
- Unknown: how big the Craft-tab versus Build-tab listing change is beyond Buildable.

## 8. Risks to the finished kitchen
- **Raycasts count any collider:** `OverheadCover.Roofed` casts straight up from 0.1 m above a point and accepts any non-terrain collider; `SunBlocked` does the same toward the sun. Tall shelving or a hood above the player or the hearth would read as a roof and change shelter, shade and the Bow Drill cover test. Keep add-on colliders below head height, or accept the effect.
- **Hearth lookup:** `FireNear(hearthSpot, 0.75)` and `NearestLit` are position-based. A second fire near the hearth could be picked up as the kitchen's hearth.
- **Aim and interaction:** the kitchen has no interaction box, and the interaction and Hammer rays take the first collider hit. An add-on collider in front of the hearth could block the Campfire interaction; one in front of a staged site could block the Hammer. Add-ons need their own interaction collider, and `PlacedStructure`'s Rebuild only adds "Solid" colliders for kind 24.
- **Walking:** posts are real colliders, so add-on colliders could block the entry. The front (+Z) is open.
- **Sheltered meal:** `ShelteredMeal` uses `UnderRoof` and does not depend on colliders, so add-ons will not break it.
- **Setbacks:** `WellsAndOuthouses.KitchenNeighbours` gives unknown kinds a 0.7 m radius, and `Sites()` ignores kinds it does not list. New add-on kinds would count as neighbours for later kitchens unless excluded.
- Not known: whether the existing campfire collider and a nearby add-on collider interact badly (Campfire.cs not checked).

## Also asked
- **Prep table today:** decor only, several "Solid" pieces (top and four legs) with BoxColliders, built inline in `BuildKitchen`. No interactable; aiming at it shows the kitchen's prompt, and E only flashes the hearth state.
- **Countertop reuse:** only if the table code is extracted into a helper (it is inline and fixed at one spot). Recommendation: build the counter as a new piece from that helper and leave the existing prep table as decor. No save impact either way.
- **Sizes and build order (Claude Code):**
  1. Countertop, small (a placed piece, interactable only if wanted). It also fixes the slot and placement exemptions.
  2. Shelving, medium (Wood Shed route with its transfer screen; no spoilage benefit unless that system exists).
  3. Wood-fired grill, medium (a second furnished fire plus visuals); the griddle has no recipes yet and waits.
  4. Slop sink, medium to large (needs a water-accepting kind, a drain and a pour interaction; washing affects nothing today).
  5. Cleaning table, large, last (needs a way to carry a carcass or new pre-dressed items, and a benefit rule; no cleanliness or spoilage system exists).

## What this means for the draft rules above
- **Countertop:** buildable now. Its "job" is mostly decor and surface until a use exists.
- **Shelving:** buildable now as a roofed WoodPile; benefit is a place to store things under cover.
- **Grill:** buildable now as a second furnished fire (heat source and look). **Griddle:** waits for recipes (no eggs, grain, flour or griddle recipes exist).
- **Slop sink:** can hold and drain water now, but washing affects nothing; a benefit rule is new design.
- **Cleaning table:** needs new design first (carry a carcass or a pre-dressed item, and a benefit such as better yield or lower illness risk). Nothing to process today.
- **Placement:** needs a decision on slots versus free placement, and room is tight (probably not all five fit).


---

# BATCH 1 BUILT - slots, countertop, shelving, wood-fired grill, slop sink (2026-10-19, NOT played, NOT committed)

Claude Code built batch 1. Compiles clean; 146/146 edit-mode tests pass (126 earlier + 20 new). Checklist: Kitchen_Addons_Playtest_Checklist.md (77 items). Nothing committed or pushed; nothing played in Play Mode. The one Console warning was the test harness reporting that the first run did not start in time; a re-run passed. Where this section differs from the draft rules and findings above, this section wins.

**Mike's decisions (2026-10-19):** fixed slots on the kitchen; first batch = countertop, shelving, grill and slop sink; cleaning table DEFERRED until spoilage and hygiene exist; griddle waits for recipes; no metal grate (no iron or steel); nothing needing LP gas, electric or power.

**Slots (Setbacks.KitchenLayout, pure and tested).** Four generic slots, all under the roof, any add-on in any slot. Kitchen-local coordinates, +Z is the open front:
- Slot 1: right wall, front (1.15, 0.78)
- Slot 2: right wall, back, beside the prep table (1.15, -0.20)
- Slot 3: left wall, front (-1.37, 0.80)
- Slot 4: left wall, back (-1.37, -0.12)
Each slot is 0.88 m along the wall and 0.5 m deep, and the item faces the middle. Slots clear the posts, hearth slab and prep table. The open front and the aisle to the hearth stay clear (doorway between the side items about 2 m; tests check it). Every slot is at least 1.06 m from the hearth fire (rule: more than 0.75 m), so the hearth lookup cannot pick a grill's fire. No slots outside the roof (four fit). Lookups: nearest free slot, and which slot a position is in.

**Kinds and attachment.** PileKinds 25 to 28 appended, none renumbered. No new save keys: a slot is just the add-on's position; its kitchen is found each time with `KitchenForSlot`.

**Placement.** An add-on ghost snaps to the nearest free slot of the nearest finished kitchen within 3 m of the aim, and faces the middle. Refusals: "Needs a finished Covered Outdoor Kitchen." (in the Build tab and in placement), "That spot is taken.", "Equip the Hammer to build." Add-ons skip the 2 m pile spacing against their own kitchen and its other add-ons, and skip the lit-fire rule against that kitchen's hearth and grills; both still apply against unrelated piles. Add-ons are excluded from `KitchenNeighbours` (`Sites()` already ignored them).

**Kitchen Dismantle** refuses while any add-on stands in a slot ("remove the add-ons first"). An add-on's own Dismantle is not blocked by the kitchen.

**The four add-ons (first-pass costs are Claude's, tuning fields on WellsAndOuthouses; all single-step with the Hammer)**
1. **Countertop:** 1 Log + 4 Branches (12 kg). 0.9 m work surface with a lower board, a back lip and a chopping board (PlacedStructure). Own box collider and prompt; no function yet. The prep-table code is now a shared `WorkTable` helper; the existing prep table is drawn the same way and stays decor. Dismantle: Hammer, half refund rounded down.
2. **Shelving:** 10 Branches + 6 Sticks (10.6 kg). A WoodPile with the transfer screen, like the Wood Shed. Accepts dry goods and food (everything the Storage Bin takes plus anything edible that is not water); tools, wood, Stone and water are refused. 1.4 m tall, so below head height. Contents keep their day stamps. Dismantle: Hammer, only when empty, from the transfer screen's Remove button.
3. **Wood-Fired Grill:** 2 Stone + 1 Clay (13.5 kg). A real second campfire built through `FireManager.BuildFurnished`: fuel, wet/dry wood, lighting, Bow Drill cover test, sound, saving and cooking (via the nearest-lit-fire lookup) all come with it. No new recipes. Look: a firebox with a stone top and a green-stick rack. Prompts are named "Wood-Fired Grill". Dismantle: Hammer, the fire must be out, half back rounded down, leftover Firewood returned, the fire goes with it.
4. **Slop Sink:** 1 Log + 1 Clay + 2 Branches (13.5 kg). Holds 20 L (`slopSinkLitres`), water by quality stored in the pile's contents with its own pour and draw code in PlacedStructure (`CapacityOf` returns the sink capacity; the barrels' `IsWaterBarrel` logic is untouched). E fills your Buckets and Canteens, best quality first. R: pours carried water in; with nothing to pour, or with the sink full, it drains the sink onto the ground ("Drained the sink."); with it empty, it dismantles with the Hammer. A 1.5 s guard stops the draining press from also dismantling. Washing affects NOTHING and nothing fouls wells (no hygiene system). Dismantle: sink must be empty ("drain the sink first").

**Build tab:** all four are in "Storage and Piles", after the Covered Kitchen Site, with help text.

**Debug key:** Numpad / prints, for the nearest finished kitchen, what stands in each slot, the grill's state and fuel, the sink's litres, the shelving's stacks and items, and the 0.75 m hearth check. 8 s on screen and in the Console as `[KitchenAddons] ...`.

**Changes from the message (Claude Code's calls; flagged for Mike):**
1. Sink draw: E draws best-quality-first in one press, not a per-quality screen (the transfer screen is typed to WoodPile).
2. Sink R does three things (pour, drain, dismantle) chosen by what you carry and what is in the sink. With a half-full sink you cannot drain while holding water. Mike can ask for a separate drain key.
3. Grill fire is drawn at 0.6 of a normal campfire (`FireManager.SetViewScale` plus a hook in `SpawnView`) so it fits the firebox; otherwise a normal fire except its size and the "Wood-Fired Grill:" prompt prefix.
4. Edits outside the add-on files: small changes to FireManager.cs and Campfire.cs (scale and prompt) and to CraftingScreen.cs (add-on help text; the kitchen's help now mentions slots).
5. One unit test dropped ("add-ons are not neighbours for a later kitchen"): a fake WoodManager reads as null to Unity's `== null`, so it could not run. Covered by checklist item 71.

**Untested:** all of it in Play Mode: look of the ghosts and models, slot snapping, colliders and prompts, the grill's fire and its 0.6 scale, cooking at the grill, Bow Drill cover at the grill, the sink's pour, draw and drain, shelving contents and day stamps, saves, regressions. A kitchen on a steep slope and a turned kitchen were not tried.

**Still deferred / out of scope:** cleaning table (needs spoilage, hygiene, a way to carry carcasses), griddle (needs recipes), washing benefit for the sink, well-fouling from a drain, metal grate (no iron or steel), LP gas, electric and anything needing power.


---

## RENAME (Mike, 2026-10-20): names that say where the add-ons go

Mike asked for direct names that reinforce that these only go in a kitchen. Player-facing names, replacing the names used in the "BATCH 1 BUILT" section above (the rest of that section still applies):
- Countertop is now **Kitchen Cabinets** (mapped by Claude from Mike's four names; same piece, still a work surface with no function; Mike can correct this)
- Kitchen Shelving is now **Kitchen Shelves**
- Slop Sink is now **Kitchen Sink**
- Wood-Fired Grill is now **Kitchen Grill** (still a wood-fired second campfire; fire prompt prefix "Kitchen Grill:")

Internal enum and field names are unchanged. This is a text change only (Build tab, help text, prompts, refusals, Dismantle text, Numpad /, checklist).


---

## RENAME CORRECTED (Mike, 2026-10-20): the "Building-Item" naming convention

This section SUPERSEDES the "RENAME" section directly above it (that one mapped the examples to specific items and used spaces; both were wrong).

**Convention (Mike):** add-on names are `<Building>-<Item>`: the building's name, a hyphen, then the item, each part capitalised. The name says where the add-on can be placed. It applies to these add-ons and to any future add-on to another building.

Batch 1 player-facing names:
- Countertop is now **Kitchen-Countertop**
- Kitchen Shelving is now **Kitchen-Shelves**
- Slop Sink is now **Kitchen-Sink**
- Wood-Fired Grill is now **Kitchen-Grill** (still a wood-fired second campfire; fire prompt prefix "Kitchen-Grill:")

"Kitchen-Cabinets" was only an example of the format, not an item. Future add-ons to other buildings follow the same pattern (for example Cabin-... or Shed-...; the prefix for each building is Mike's call when it comes up).

Internal enum and field names are unchanged. Text change only (Build tab, help text, prompts, refusals, Dismantle text, transfer screen title, Numpad /, checklist).
