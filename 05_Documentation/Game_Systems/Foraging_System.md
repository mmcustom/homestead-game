# Homestead
## Foraging System v1.0

Status: Design Draft

---

# Purpose

The Foraging System allows players to gather naturally occurring food and resources from the environment.

Foraging is one of the most important Year One survival systems.

It rewards:

- Exploration
- Observation
- Seasonal knowledge
- Land familiarity

Players should become increasingly familiar with their property over time.

---

# Core Philosophy

Knowledge Is Power.

A knowledgeable forager gathers significantly more resources than an inexperienced player.

The player is rewarded for:

- Remembering locations
- Learning seasons
- Tracking harvest windows
- Exploring new areas

Foraging should feel like studying and learning the land.

---

# Resource Categories

## Berries

Examples:

- Blackberries
- Raspberries

Uses:

- Food
- Preservation
- Trade

---

## Fruit

Examples:

- Wild Apples
- Pawpaw

Uses:

- Food
- Preservation

---

## Nuts

Examples:

- Hickory Nuts
- Walnuts
- Chestnuts

Uses:

- High calorie food
- Winter food reserves

---

## Edible Plants

Examples:

- Dandelion
- Wild Onion
- Cattail

Uses:

- Early food source

---

## Mushrooms

Examples:

- Seasonal edible mushrooms

Uses:

- Food
- Trade

Future systems may include poisonous species.

---

# Discovery Integration

Resources must be discovered.

Examples:

Blackberry Patch Found

Journal Updated

Map Updated

---

## Journal Information

Resources record:

- Location
- Season
- Notes
- Discovery Date

---

## Map Information

Resources appear after discovery.

Examples:

🫐 Berry Patch

🌰 Nut Grove

🍎 Fruit Tree

🍄 Mushroom Area

---

# Seasonal Availability

## Spring

Examples:

- Wild Greens
- Mushrooms

---

## Summer

Examples:

- Berries
- Early fruits

Peak gathering season.

---

## Fall

Examples:

- Nuts
- Apples
- Pawpaw

Most important storage season.

---

## Winter

Minimal foraging.

Prepared players rely primarily on stored resources.

---

# Harvesting

Resources regenerate seasonally.

Examples:

Berry Patch

Harvested:
Summer

Available Again:
Next Summer

---

# Property Knowledge

Successful players learn:

- Best berry patches
- Best mushroom locations
- Productive nut groves

Knowledge accumulates over time.

The property becomes more valuable as the player learns it.

---

# Food Preservation

Foraged foods may be:

- Eaten fresh
- Dried
- Stored in root cellar

Proper preservation increases winter survival.

---

# Wildlife Integration

Wildlife and foraging overlap.

Examples:

Nut trees attract squirrels.

Fruit attracts deer.

Berry patches attract wildlife.

Understanding plants also improves hunting success.

---

# Economy Integration

Surplus foraged resources may be:

- Sold
- Traded
- Preserved

Examples:

- Mushrooms
- Nuts
- Fruit

Survival remains the primary purpose.

---

# Design Rules

1. Foraging rewards knowledge.

2. Discovery matters.

3. Seasons matter

---

# Seasonal Forage Frequency: Mike's Feedback 2026-10-10

**Mike (Play Mode playtest):** "forage locations should be more frequent during spring and summer for certain naturally occurring vegetation and other more prominent during other seasons."

**What the doc says today:** the Seasonal Availability section above lists examples per season (Spring greens and mushrooms, Summer berries and early fruit as the peak, Fall nuts, apples and pawpaw, Winter minimal) but gives no numbers, and it does not say how often forage spots appear. Claude does not know how the current build spawns forage, so Claude Code should report that first (see the message).

**First pass (Claude, not yet confirmed):** each forage type has a relative frequency per season. Frequency means how many forage locations of that type are active across the property at once (and how often they respawn), not how much each location gives. 1.0 is the baseline the game uses today. Locations that are out of season simply are not there, rather than being there and empty, so the map and journal stay honest.

| Forage type | Spring | Summer | Fall | Winter |
|---|---|---|---|---|
| Edible plants (dandelion, wild onion, wild greens) | 1.5 (peak) | 1.0 | 0.4 | 0 (under snow) |
| Cattail (also Cordage source) | 1.0 | 1.0 | 1.0 | 0.5 |
| Mushrooms | 1.2 (morels) | 0.5 | 1.5 (peak) | 0 |
| Berries (blackberry, raspberry) | 0.1 (flowers only) | 2.0 (peak) | 0.5 | 0 |
| Fruit (wild apple, pawpaw) | 0 | 0.3 (early) | 1.5 (peak) | 0.1 (windfalls) |
| Nuts (hickory, walnut, chestnut) | 0 | 0 | 2.0 (peak) | 0.4 (leftovers) |

- **Why these numbers:** they follow the doc's own seasons (Summer berries as the peak, Fall nuts and fruit as the storage season, Winter minimal) and roughly the southern Ohio calendar the property is based on (morels in spring, blackberries in midsummer, pawpaw and nuts in fall). The values are Claude's, not researched against a source.
- **Winter stays hard on purpose:** only cattail roots, a few windfalls and leftover nuts, so prepared players rely on stored food, as the Foraging doc says.
- **Journal and map:** a location discovered in one season stays remembered and shows greyed out when its season is not active, so players learn when to return ("Berry Patch, Summer").
- **Tunable:** the table lives as Inspector (or data) values so Mike can adjust any cell by feel.

Mike confirms or changes the numbers in a playthrough.

## How Forage Works Today (Claude Code report, 2026-10-10)

- There is no runtime spawning. The editor tool Homestead > Place Forage Patches (Editor/ForagePlacer.cs) ran once and wrote 13 fixed patches into World.unity. Each patch is a ForageNode on a DiscoverySite; ForagingManager keeps the picked state per patch.
- Species today: blackberries (Briar Patch site) and raspberries (Summer); wild apples and pawpaw (Fall); hickory, walnut and chestnut (Fall); dandelion and wild onion (Spring); cattail (Spring); morel, dryad's saddle and oyster (Spring).
- Each species has one season (optionally one part). Out of season the patch still exists but gives nothing ("bears in Summer"). Annual patches return next year. Plants, cattail, dryad's saddle and oyster regrow in 3-4 in-game days while in season.
- Journal and map discovery happens on the first harvest and is permanent. Nothing greys out today. Winter has no forage at all.

## DECISION 2026-10-10 (Mike): Option A (pool of patches, seasonal activation)

Mike chose Option A. Design (Claude's wording of Claude Code's option; numbers remain Claude, not confirmed):

- The world gets a pool of about 2-3 patches per forage type (more than the 13 today).
- A data table holds one multiplier per type per season (the first-pass table above).
- At each season change a fixed per-save seed decides which pool patches are active this season: about multiplier x base count, and stable for the whole season. The same save always shows the same patches in the same season.
- Inactive patches are hidden (no collider, no prompt, no mesh), not shown empty.
- Patches the player has discovered stay in the journal and map greyed out, with the season they return ("Berry Patch, Summer"). This is new UI work.
- Patch ids stay the same, so existing saves keep their picked state.
- Build changes: ForageSpecies gets a per-season table; ForagingManager gets the activation roll at season change; ForageNode shows and hides; ForagePlacer places the extra pool; Mike re-runs Homestead > Place Forage Patches and saves World.unity. World.unity is already 46.4 MB, so the extra pool should stay small.

### Open question: species per season (Claude's default, not confirmed)

Because every species today has exactly one season, the type table only works if a pool patch can yield different species in different seasons. First pass (Claude, not confirmed): reuse EXISTING species only, with no new species invented. A pool patch of a type yields whichever of that type's species are in season at its location, using each species' own season as it is today. A table cell with no existing species behind it counts as 0 until a species is added. Effect on the first-pass table:

| Forage type | Spring | Summer | Fall | Winter |
|---|---|---|---|---|
| Edible plants (dandelion, wild onion) | 1.5 | 0 (no species) | 0 (no species) | 0 |
| Cattail | 1.0 | 0 (no species) | 0 (no species) | 0 (no species) |
| Mushrooms (morel, dryad's saddle, oyster) | 1.2 | 0 (no species) | 0 (no species) | 0 |
| Berries (blackberry, raspberry) | 0 | 2.0 | 0 | 0 |
| Fruit (wild apple, pawpaw) | 0 | 0 | 1.5 | 0 |
| Nuts (hickory, walnut, chestnut) | 0 | 0 | 2.0 | 0 |

The cells that would need new species (or an extra season on an existing one) to match the first-pass table: Edible plants in Summer, Cattail in Summer, Fall and Winter, Mushrooms in Fall (a fall oyster or similar), Berries in Fall, Fruit in Summer and Winter, Nuts in Winter. Adding a species needs a species sheet in the Plants docs, which does not exist yet. Mike decides whether to add any.

## BUILT 2026-10-10 (Claude Code report, reconciled by Claude): seasonal forage, Option A

Status: BUILT and PLAYTEST-CONFIRMED 2026-10-10 (Mike: "everything worked as described, forage patches gone in the winter"), existing species only. Mike re-ran Homestead > Place Forage Patches with World.unity open (the first attempt failed because the World scene was not open). CORRECTION 2026-10-10 (Claude Code): World.unity on disk did NOT contain the nine new pool patches (file unchanged since Oct 9 20:32, 46,447,628 bytes, identical to HEAD, no forage_*_2 ids), so the placer result was probably never saved with Ctrl+S; the Play Mode test ran on the unsaved in-editor scene. Mike then re-ran and saved, and the work was committed and pushed as 24beaf2 (2026-10-10). World.unity went from 46,447,628 bytes (46.4 MB, 1,660,336 lines) to 47,111,294 bytes (47.1 MB, 1,683,506 lines, +0.66 MB), under GitHub's 50 MB warning. **FIXED 2026-10-10: Mike re-ran the placer after the cattail anchor move (Console: "Placed 22 forage patches", no warnings), saved, and it was committed and pushed as f3525e5. World.unity is now 47,184,432 bytes (47.2 MB, 1,686,053 lines), all nine forage_*_2 ids present, 22 patches in total (the original blackberry patch uses the Briar Patch's own id, so a string count of forage_* shows 21).** History: the first commit (24beaf2) had only EIGHT of the nine new patches placed (21 in total, not 22): forage_cattail_2 is missing because the placer found no shore spot for it (expect a "No spot found for Cattail Marsh" Console warning). Effect is minor (Spring cattail still has the original patch). Mike asked Claude Code to move the second cattail's anchor (Spec anchor Vector2(130, -60), Habitat.Shore, in ForagePlacer.cs) so it places; that needs another placer run, Ctrl+S and a small second commit. Also committed: the 13 Resources/Forage species assets (empty alsoBearsIn field added by the placer re-run). Counts elsewhere in this doc saying 22 patches / nine new describe the design; the saved scene has 21 until the cattail is fixed. Claude Code could not compile-check in Unity (UnityMCP down); Mike's Play Mode run implies it compiled. After Mike runs Homestead > Place Forage Patches and saves World.unity, send Claude Code the new World.unity size.

- **Pool:** 22 patches = the original 13 plus 9 new (one extra each of dandelion, cattail, oyster mushroom, blackberries, raspberries, wild apples, hickory, walnuts and chestnuts). New ids look like forage_<item>_2. The original 13 keep their ids and random placement.
- **Table:** ForagingManager has a "Seasonal Frequency" table in the Inspector: per type a Base Count and a multiplier per season. Base counts (Claude Code's pick, not confirmed): Plants 2, Cattail 1, Mushrooms 3, Berries 2, Fruit 2, Nuts 3. Multipliers are the first-pass values above.
- **Activation:** at each season change, per type, the patches whose species bear that season are put in a seeded order; the first (multiplier x base count) are active, and the fractional part is rolled with the same seed. The seed is saved with the forage state; a new game gets a fresh seed. Same save, season and year always show the same patches. An old save has no seed and uses a fixed seed 0.
- **Inactive patches:** plant and crop hidden, colliders off, no mesh, no prompt. This replaces the old "bears in Summer" empty patch.
- **Picked state:** now kept per season, so a patch that bears in several seasons starts fresh each season. Older picked states still match their year.
- **ForageSpecies "Also Bears In":** a new season list, empty for every species today. Adding a season there turns the matching table cell on with no code change.
- **Morel:** its narrow window still applies (present all Spring but only gives within the window).
- **UI:** world map greyed marker and legend row "(returns Summer)"; minimap greyed marker; journal list and detail show "out of season, returns Summer".

**Table cells that are 0 in practice (existing species, each in one season):** Plants Summer and Fall; Cattail Summer, Fall and Winter; Mushrooms Summer and Fall; Berries Spring and Fall; Fruit Summer and Winter; Nuts Winter. Live cells: Plants, Cattail and Mushrooms in Spring; Berries in Summer; Fruit and Nuts in Fall. **Every Winter cell is 0, so Winter has no forage at all.** Old-save players will see their patches vanish in Winter (intended by the build, but it conflicts with the first-pass table, which wanted Winter cattail, windfalls and leftover nuts). **DECIDED 2026-10-10 (Mike): "no winter forage is fine."** Winter has no forage; this overrides the first-pass Winter values (cattail 0.5, fruit 0.1, nuts 0.4) in the table above. The other empty Summer/Fall cells stay as built (existing species only) unless Mike asks for more species later; the "Also Bears In" list is the hook for that.

Claude Code's Play Mode checks (10): 22 patches under Forage with the original 13 ids unchanged; Spring about 3 plant, 1 cattail and 3-4 mushroom patches and no berries, fruit or nuts; Summer 4 berry patches only; Fall 3 fruit and 6 nut patches only; Winter none; same patches after save and reload; discovered patch stays greyed on map, minimap and journal when out of season; old save loads with picked state and discoveries intact; harvesting and yields unchanged in season; a harvested patch is empty until its season returns.

(Claude Code's own build note, already merged into the BUILT section above: ForageSpecies also has a derived Type; inactive patches hide their Plant and Crop children and root colliders; the code is uncommitted until Claude Code reports a commit hash.)
