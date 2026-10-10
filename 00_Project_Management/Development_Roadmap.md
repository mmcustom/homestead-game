# Homestead
## Development Roadmap

Status: Active — refreshed 2026-10-04

---

# Purpose

Tracks where Homestead stands against its own GDD, so design work stays paced ahead of implementation without racing past what's been proven fun.

This roadmap covers design documentation and, as of this refresh, a summary of what has been built. The detailed build history lives in Current_Task_List.md's Log; Unity implementation is done by Claude Code (Copilot has been replaced — see AI_Collaboration_Rules.md).

---

# Current Phase

Alpha 0.1 implemented, awaiting playthrough. Every one of the GDD's 16 Alpha 0.1 Required Systems has a design doc and has been built and tested in Unity. The GDD's Development Rule still applies: nothing beyond Alpha 0.1 should be designed or built until the core survival loop has been proven fun by actually playing it.

**The next step is playtesting, not new design.** The best test is a full run toward Winter Preparation — the Year One climax the GDD names — since it exercises fire, shelter, food, water, storage and light together.

---

# Status Snapshot — 2026-10-07

A one-page view of the whole GDD. Details are in the sections below.

**Built and tested:** all 16 Alpha 0.1 systems, plus hunting, trapping, cooking, wood and stone gathering, seasons, weather, health with Warmth, terrain and property layout, primitive storage with the two-way transfer screen, the Small Cabin with Hammer Dismantle, and Torch/Lantern lighting.

**Designed, not built:**

- Player Collapse ("wake weakened", decided 2026-10-04). The specifics are Claude's first pass and still need Mike's confirmation.
- Power_System, Flashlight and Maple_Sugaring.
- Rainwater Collection and the Lumber Mill.
- Large Cabin and higher housing tiers.
- Later-stage design drafts: Livestock, Mining/Metalworking, Difficulty, and the Plants, Wildlife and Fishing species sheets.

**Known gaps:**

- Livestock feed quantities are not defined.
- Whether the Trading Post buys furs has not been checked.
- Lumber at the Trading Post was confirmed 2026-09-26 but never sent to Claude Code.
- Real mouse and keyboard input (clicks, Shift-click, held keys) is untested, because Unity ignores simulated input when its window is unfocused.

**Playtest findings 2026-10-07 (Mike, Pioneer run from a new game):**

- Sticks and Branches have no source without an Axe, which Pioneer doesn't start with. Accepted for now, and built 2026-10-07 by Claude Code but only compile-checked, not tested in Play Mode, not committed: loose Sticks and Fallen Branches on the ground (Wood_Gathering_System.md). Mike will update after more playtesting.
- Stone and Tall Grass need to be more common. Accepted for now, built and compile-checked but not tested or committed: density increases (Stone_Gathering_System.md, Trapping_System.md). The three placement menus still have to be run in the World scene. To be confirmed after playtest.
- Warmth drops very fast in a cold start. Mike wants it built to real human physiology, so the first idea (a simple exertion bonus) was replaced by a researched Core Temperature Model (Health_System.md, sources in Research/Human_Thermoregulation.md). Claude Code reviewed it against the code and found it buildable for the cold side with corrections (now in the doc), but the hot side first needs weather additions: humidity, sun load, heat waves and heat Health tiers. Claude drafted those as a first pass in Weather_System.md on 2026-10-07 (not confirmed, not built). Claude Code reviewed that draft against the code: mostly buildable, with corrections (sun-load inputs, dew-point humidity, no existing temperature cap, collapse gaps) now applied to the docs. Numbers are still unconfirmed. **Build approved 2026-10-07 (Mike): start building the cold side of the Core Temperature Model now** (a playtest fix, so it goes ahead of the playthrough). The hot side and Player Collapse stay unbuilt until Mike says so. Claude Code built the cold side the same evening (compile-checked and simulated on its own, not Play Mode tested, not committed): new BodyHeat.cs plus SurvivalManager, PlayerController, SurvivalHud and WeatherManager changes; details, numbers and risks in Health_System.md.
- Sticks and Branches still too sparse after placement (only a few sticks and 1 branch found), and no way to light a Campfire without Flint and Steel (2026-10-07, Mike). Accepted: raise ground wood to about 1,200 bundles / 450 branches placed per tree, and add a craftable Bow Drill (2 Sticks + 1 Cordage, unreliable). Built 2026-10-07 by Claude Code, compile-checked only, not Play Mode tested or committed; docs committed in 4193f2c.

- Playtest 2026-10-09 (Mike): Bow Drill, Torch lighting and plentiful sticks/branches/tall grass confirmed. New: Primitive Bow (1 Branch + 1 Cordage) and stone-tipped Arrows (1 Stick + 1 Stone; built 2026-10-09, compile-checked only), a separate Crafting screen (Crafting_System.md, design draft), Clay not findable (dug with the Shovel, nothing marks it, and Pioneer has no Shovel; visible clay patches built 2026-10-09, placer not yet run), and the Tarp has no deploy action (placed Tarp Shelter confirmed by Mike 2026-10-09, built the same day, compile-checked only).
- Small Cabin site never says what is missing when partly stocked (2026-10-09, Mike): the Inventory button hides the right reason behind "already have a site". Fix built 2026-10-09 (Claude Code), compile-checked only, not Play Mode tested.
- Finished Small Cabin renders wrong (two walls rotated across the interior) and cannot be entered; also no "fully stocked" notice (2026-10-09, Mike, screenshot). Cause found in Shelter.cs; fixed and playtest-confirmed by Mike 2026-10-09 (walkable, sleeping inside, under cover); the hearth should move inside the cabin, documented, not built.
- The "winter start" was Spring day 1 with snow flurries. Mike resolved it, and Claude Code confirmed from the code that a new game starts on Spring day 1 at 06:00.

**Next step:** a Winter Preparation playthrough. Stages 2 through 4 stay deferred until the survival loop is proven fun.

**Housekeeping:** the roadmap rewrite, gap check and first Player Collapse draft were committed and pushed 2026-10-07 (`f74dd5f`). The Player Collapse corrections, Core Temperature Model, thermoregulation research and the first sticks/branches/density doc changes were committed and pushed later the same day (`66c5c53`). Those later doc edits (built notes, Season confirmation, Health_System review corrections, research notes, the first hot-side weather draft with the no-ceiling and heat-collapse decisions) were committed and pushed as `d9d6711` (full `d9d67115184c6a4b3ca35617796587bdfe1de04d`); master matches origin/master. Doc edits made after `d9d6711` (Claude Code's hot-side review corrections in Weather_System.md and the Health_System pointer, this roadmap, the task log) are uncommitted. Also uncommitted and untested: the cold-side Core Temperature code (new BodyHeat.cs; SurvivalManager, PlayerController, SurvivalHud and WeatherManager changes), the ground wood, stone and grass code, Bootstrap.unity, and the `ProjectSettings.asset` `runInBackground` change (a local testing setting; Mike does not want it committed).

---

# Alpha 0.1 Required Systems — Status

All 16 are documented and implemented (build details, commits and test notes are in Current_Task_List.md's Log):

- First Person Controller (including Jump) — `Systems/First_Person_Controller.md`
- Inventory (screen, carry weight, Drop Items, tool hotkeys, build/craft notices) — `Systems/Inventory_System.md`
- Hydration and Hunger — `Game_Systems/Core_Survival_System.md`
- Fire Building — `Game_Systems/Core_Survival_System.md`
- Water Collection and Water Purification — `Game_Systems/Water_System.md`
- Foraging — `Game_Systems/Foraging_System.md`
- Fishing and Fish Traps — `Game_Systems/Fishing_System.md`
- Tent Placement and Sleeping (Sleeping Bag, Tent, Lean-To) — `Game_Systems/Building_Housing_System.md`
- Discovery System, Journal, Minimap (plus World Map and compass) — `Game_Systems/Discovery_System.md`
- Save System — `Systems/Save_System.md`

---

# Built Beyond the Alpha 0.1 List

Added at Mike's request, each with its own doc, as support for the Year One food pillars and the homestead's first buildings:

- Hunting, Trapping, Cooking, Eating
- Wood Gathering (tree chopping, windthrow) and Stone Gathering
- Seasons, Weather, Health (with Warmth), and the real terrain and property layout
- Primitive Storage (Wood Pile, Rock Pile, Water Barrel, Food Cache, Storage Bin, Tool Rack) with a shared two-way transfer screen
- Small Cabin (staged build, hearth, sleeping), Hammer Dismantle, cabin-site removal
- Portable Lighting: Torch and Lantern, with Lamp Oil

---

# Designed but Not Built (Future)

- **Power_System.md** — generation, storage and sizing for electricity, researched ahead of need. Not a build task until housing reaches a tier that needs power.
- **Flashlight** — waits on a battery item from Power_System.md.
- **Maple_Sugaring.md** — a Future System spec.
- **Lumber Mill and wood species tiers**, **Rainwater Collection** — noted in their source docs.
- **Large Cabin and every housing tier above it** — documented, not built.

Feature_Backlog.md catalogs every other deferred item in one place.

---

# Open Questions From the 2026-09-22 Design Review

The design review (`claude/homestead-gdd-v2.0-design-review.md` in the Homestead Game project) ranked gaps in the GDD. Several have since been covered — Season, Weather and Health/Morale are now defined, and Spoilage has its own Core_Survival_System.md section. Checked against the docs 2026-10-04:

- **Failure/death state — decided 2026-10-04, designed, not built.** Mike chose "wake weakened": at Health 0 the player fades out, 8 in-game hours pass, and they wake weakened rather than dying. A first-pass spec (wake location, stat floors, a 4-hour Weakened state, no item loss) is in Health_System.md under "0 — Player Collapse", marked as Claude's proposal pending confirmation. Build it after, or alongside, the Winter Preparation playthrough, per the Development Rule.
- **Economy sink — covered.** Economy_System.md has an Economic Sinks section (property development, livestock, equipment, future transportation), and the Trading Post is built as the first place to actually spend money.
- **Trapping's link to the Early income tier — covered in design.** Trapping_System.md's Economy Integration section says Small Furs may be sold, and Economy_System.md lists Small Furs and Deer Hide as Early Survival Revenue. Whether the in-game Trading Post actually buys furs was not checked.
- **Livestock feed loop — partly covered, not built.** Livestock_System.md names feed needs per animal, Feed Reserves for winter, and seasonal effects, but gives no quantities (how much feed per animal per day, how much a winter needs). Livestock is a Design Draft with no implementation, so this can wait until it's scheduled.

---

# Supporting Documentation Folders

Livestock, Plants, Wildlife and Fishing species sheets, Gameplay_Loops (Core_Loops.md), Technical_Design, Unity_Architecture and Research all now contain files; they were listed as "not started" before this refresh. Depth was not re-reviewed.

---

# Stages 2 Through 4 — Explicitly Deferred

Early Homestead, Established Homestead, and Modern Homestead (per the GDD's Progression Overview) are not in scope for design work right now. The GDD's Development Rule blocks Farming, Large Livestock Operations, Vehicles, Solar Arrays, and Automation until the Stage 1 survival loop is proven fun. This roadmap will not add tasks for those stages until that milestone is reached and Mike says so.
