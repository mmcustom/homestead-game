# Homestead
## Development Roadmap

Status: Active — refreshed 2026-10-19 (see the 2026-10-19 status update at the bottom, which supersedes older "not built" lists above)

---

# Purpose

Tracks where Homestead stands against its own GDD, so design work stays paced ahead of implementation without racing past what's been proven fun.

This roadmap covers design documentation and, as of this refresh, a summary of what has been built. The detailed build history lives in Current_Task_List.md's Log; Unity implementation is done by Claude Code (Copilot has been replaced — see AI_Collaboration_Rules.md).

---

# Current Phase

Alpha 0.1 implemented and PLAYTHROUGH PASSED (2026-10-11). Every one of the GDD's 16 Alpha 0.1 Required Systems has a design doc and has been built and tested in Unity. The GDD's Development Rule asked for the core survival loop to be proven fun by actually playing it before anything beyond Alpha 0.1 is designed or built. The full Winter Preparation run (Spring through 14+ Winter days, 25 of 25 checks passed, "playtest was fun") met that gate on 2026-10-11, so post-Alpha 0.1 design is now open at Mike's choice.

**The next step is picking the first post-Alpha 0.1 item** (see "Next step" below). Unrequested new design should still wait for Mike's pick.

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


---

# Status Update — 2026-10-10 (supersedes the 2026-10-07 housekeeping and playtest notes above where they differ)

**Everything below is built, Play Mode tested by Mike, committed and pushed (latest commit b71ace2; master matches origin/master). `ProjectSettings.asset` runInBackground remains a local, uncommitted setting by Mike's choice.**

- Core Temperature Model, cold side (BodyHeat.cs, SurvivalManager): confirmed, with Mike's tuning on 2026-10-10: visible shivering starts at core 35.5 C (full 34.5 C) so dry rests no longer shiver; shivering's stamina penalty follows the visible ramp; a rewarming boost speeds recovery while walking (x1.5), working (x2.0) and sprinting (x2.5) without changing resting levels or cooling. Shiver.wav wired.
- Bow Drill, ground wood, stone and tall grass density, Primitive Bow and Arrows, Clay patches, Tarp Shelter: confirmed.
- Small Cabin: site messaging, geometry and walkable interior confirmed; hearth moved INSIDE (stone slab, clay backing and chimney) and confirmed; old saves migrate; loading a save made inside a cabin or under a Tarp no longer puts the player on the roof.
- Crafting and Build screen (key B, Craft and Build tabs, 15 recipes, have/need panel): confirmed.
- Audio listener fallback and minimap water: confirmed.

**Still designed, not built:** hot side of the temperature model (humidity, sun load, Heat Waves, Overheating tiers; Weather_System.md first pass), Power_System, Flashlight, Maple_Sugaring, Rainwater Collection, Lumber Mill, Large Cabin and higher tiers, Livestock, Mining/Metalworking.

**Known gaps:** Lumber at the Trading Post (confirmed 2026-09-26, never sent to Claude Code), Livestock feed quantities, pond drying in Heat Waves; spoilage and smoking/drying are NOT built (found 2026-10-11 by Claude Code: items record their acquired day but nothing ages them) and surface water never freezes (the Trading Post does buy furs, closing that gap), what is dropped on a fourth collapse, a performance review of the roughly 13,000 placed ground objects in World.unity (46 MB, each with its own collider), and real mouse/keyboard input untested.

**Winter Preparation playthrough DONE 2026-10-11: PASS (25 of 25), "playtest was fun."** Fire, shelter, food, water, storage, light, crafting and temperature work together from Spring through 14+ Winter days, with no bugs found and no Console errors. The Development Rule's gate (core survival loop proven fun) is met for Alpha 0.1 as built, without spoilage, smoking/drying or Winter water freezing.

**Status 2026-10-14: Player Collapse is BUILT and CONFIRMED in Play Mode (cold side and all non-heat causes; two bugs found, fixed and confirmed; clean Console). Commit pending.** The hot side will reuse it with no change.

**Done 2026-10-14:** Player Collapse (3bb16cb) and Lumber at the Trading Post (ac2eb37), both confirmed in Play Mode and pushed.

**Next step (2026-10-14, Mike):** hot side of the temperature model and Heat Waves, approved to build in two phases (Phase 1 the dangerous core; Phase 2 feels-like, wading, wave-ending storm, difficulty weighting, pond drying). Spec: Weather_System.md "Heat Inputs for the Core Temperature Model". After it, candidates (not confirmed): spoilage plus smoking/drying and Winter water pressure (Mike wants these later), Rainwater Collection, Large Cabin, Power_System/Flashlight.


---

# Status Update — 2026-10-17 (supersedes the 2026-10-10 "Next step" above)

**Heat Wave Phase 1 and Phase 2 are built, Play Mode tested by Mike, committed and pushed (Phase 1 0dfad80; Phase 2 54a9a33; master matches origin/master). `ProjectSettings.asset` runInBackground and the AudioManager.cs null guard remain local and uncommitted by choice.**

- **Phase 1 (hot side of the temperature model), confirmed 2026-10-16:** HeatWave weather type (Summer only), dew point and humidity, sun load, sweat and wet-bulb model, Overheating tiers (Hot 37.5 C, Heat exhaustion 38, Severe 39, Stroke above 40), heat collapse reusing Player Collapse, too-hot sleep wake-up, two-sided Body temperature meter replacing the Warmth bar. Debug keys F3 (body readout) and F4 / Shift+F4 (start or end a wave).
- **Phase 2, confirmed 2026-10-17 (checklist 22 of 23 items passed; item 11 deferred as an untested edge case):**
  - Feels-like (NWS heat index) with NWS band names on the pause date line, from 80 F. Display only.
  - Wading cooling: submersion from the Water layer, up to 250 W, scaled by a water-temperature stand-in (24 h smoothed air minus 4 C), a 30-minute evaporative tail after leaving the water, and a core-temperature taper (full at 37.0 C and above, none at 36.5 C and below) so wading cannot drive core temperature down without limit. Wet/damp insulation penalty removed only in air above 28 C (ramp 28 to 30 C); cold-side behaviour unchanged.
  - Relief storm: a wave that runs out its natural length forces a Thunderstorm (70%) or Cold Front (30%). Not forced on F4 debug end or at the end of Summer. F2 jumps a wave to its natural end.
  - Difficulty weighting: Heat Wave chance now follows DifficultyManager.StormMultiplier (Homesteader x0.6, Settler x1, Pioneer x1.5).
  - Player-facing weather text shows "Clear" during a Heat Wave (Mike's decision, 2026-10-17): the player works it out from the heat and the feels-like line. Debug readouts keep the name.
  - Two bugs found in testing and fixed: the taper bounds loaded as 0 from a nested serialized class (moved onto SurvivalManager), and the evaporative tail counted while still in the water.
- **Rule going forward:** put new tunables on the component itself (SurvivalManager, WeatherManager, SleepManager), not inside a nested serialized class that is already saved in the scene.

**Built, play-tested, committed and pushed 2026-10-17 (after Weather Station Phase 2):**
- **Wood as direct fuel plus Campfire from 6 Sticks** (65c9cb7, docs 4f90439): Sticks (0.15 h), Branches (1 h) and Logs (10 h) burn directly in any fire like Firewood (2 h); F cycles the fuel type, E adds one, Shift+E adds as many as fit under the 12 h cap; the Campfire builds from 3 Firewood or 6 Sticks. Design: Core_Survival_System.md ("Wood as Direct Fuel"); checklist: Wood_Fuel_Playtest_Checklist.md (all 34 items pass).
- **Save slots step 1** (979ee12): 10 manual slots plus a separate autosave slot, slot panel (Save / Load / New Game pick), Copy and Delete, Continue loads the newest save, pause-menu "Save to Slot N" label. Design: Save_System.md; checklist: Save_Slots_Step1_Playtest_Checklist.md (all items pass except optional item 24, not run). Step 2 (whole-slot atomic write via temp folder swap, SlotSwap.cs plus 49 EditMode tests) was BUILT and play-tested 2026-10-18 (all 13 checklist items pass); not yet committed.
- Never committed on purpose: ProjectSettings.asset (runInBackground) and the AudioManager.cs null guard.

**Still designed, not built:** Pond drying in Heat Waves (Water_System.md, not yet designed), the Weather Information and Instruments panel (Phase 1, the free HUD line and K Weather tab, BUILT and play-tested 2026-10-17; Phase 2, the Weather Station item with raw readings, BUILT, play-tested, committed and pushed 2026-10-17 (2c149ce); original design: free Weather panel with wind direction in words, a Weather Station item for raw numbers; it must also show "Clear" during a Heat Wave), Power_System, Flashlight, Maple_Sugaring, Rainwater Collection, Lumber Mill, Large Cabin and higher tiers, Livestock, Mining/Metalworking.

**Known gaps and loose ends:** item 11 (overheated wade in air under 28 C) untested; Weather Station checklist item 22 (old save with the station equipped or hotkeyed) not yet reported; humidity-vs-difficulty hot-weather rebalance (raw dew point against shifted air temperature makes Pioneer drier and Homesteader muggier; deliberate later rebalance); wind chill now possible (WindSpeedMph exists); spoilage and smoking/drying not built; Winter water pressure and surface freezing; Livestock feed quantities; a performance review of the placed ground objects in World.unity; real mouse/keyboard input untested; optional untested Player Collapse items (starvation collapse, forgiveness count); contents of commit cc5f555 not reviewed.

**Next step (Mike to pick):** candidates are pond drying (design first), spoilage plus smoking/drying with Winter water pressure (Mike wants these later), Rainwater Collection, Large Cabin, Power_System/Flashlight.



---

# Status Update — 2026-10-19 (supersedes the 2026-10-17 "Still designed, not built" and "Next step" above where they differ)

**Rainwater Collection, Build Placement Preview (phases 1-5), Pond Drying and Flash Floods (phases 1-5) are built, Play Mode tested by Mike, committed and pushed. Master matches origin/master at c3d3e0f.** Save slots step 2 is also committed and pushed (903c72d), so the 2026-10-17 note "not yet committed" no longer applies. Never committed on purpose: `ProjectSettings.asset` (runInBackground), the AudioManager.cs null guard, Homestead.slnx and 08_Backups/.

- **Rainwater Collection, Phase 1: PASS 26/26.** Rain Barrel (80 L, 3 Logs + 2 Branches), eave-only placement, 11.54 m2 catchment per roof slope, water stored as Good. Water_System.md; Rainwater_Playtest_Checklist.md.
- **Build Placement Preview, phases 1-5: PASS 40/40.** Placement Mode with a translucent green/red ghost, 6 m aim ray, left click confirms, Esc or right mouse cancels, mouse wheel rotates 15 degrees, Rain Barrel eave snapping. Shared rules in `CanBuildPileAt` and friends. Building_Housing_System.md; Build_Placement_Preview_Checklist.md. Phase 6 (traps) is NOT built.
- **Pond Drying (Water phase 2): PASS 36/36.** Pond level 0-1 per pond; drying by season, doubled in Heat Waves; rain refills; Low and Very Low states change water quality, fishing bite waits and trap catch rates; Bass Hole never goes below 0.35. This closes the old "pond drying in Heat Waves" gap. Pond_Drying_Playtest_Checklist.md.
- **Flash Floods, phases 1-4: PASS 55/55.** Creek stream state (Normal, High water, Flash flood) from a leaky-bucket model of rainfall; creek water quality and fishing effects; no casting in a flash flood; creek Fish Traps can be swept; pond overflow; Weather screen Creek row; toasts. Flood_Playtest_Checklist.md.
- **Flash Floods, phase 5 (Creek Crossing Danger): PASS 55/55** (re-checked after the knock-down Health loss was reduced from 5 to 2). Wading a high creek slows and tires you; a flash flood pushes you downstream, knocks you down after 1.5 s, locks you out for 1.5 s, then gives 4 s of immunity; Health never drops below 10 from a flood. Creek_Crossing_Playtest_Checklist.md.
- **Rule going forward (unchanged):** put new tunables on the component itself, and rename a field rather than changing the default of an already-serialized one.

**Still designed, not built:** Placement Preview phase 6 (traps/snares/Fish Trap onto the shared placement helper), lowered pond surface and rising creek visuals (needs non-static water objects), Rain Barrel freezing (waits on the Winter water system), floodplain placement warning, greywater, koi/garden pond, stocked farm pond, aquaponics (needs Power_System), first-flush/lid/Cistern, Power_System, Flashlight, Maple_Sugaring, Lumber Mill, Large Cabin and higher tiers, Livestock, Mining/Metalworking.

**Known gaps and loose ends:** the "while you slept, the creek flooded" wake-up message (Mike leans toward removing it; waiting for a firm yes); Campfire can still be placed on a roof or rock, no spacing ring on Placement Preview, no gamepad; spoilage and smoking/drying; Winter water pressure and surface freezing; Livestock feed quantities; humidity-vs-difficulty hot-weather rebalance; wind chill; a performance review of the placed ground objects in World.unity; real mouse/keyboard input untested; Power_System and Flashlight.

**Next step (Mike, 2026-10-19):** Placement Preview phase 6 (traps), then Mike picks from the "still designed, not built" list.


**Update 2026-10-19 (later):** Placement Preview phase 6 (traps) is built, playtested (44/44, plus a 5/5 re-check of right-click-puts-the-trap-away) and pushed as 9b42d4c, so Placement Preview phases 1-6 are all done. Rabbit Snare, Box Trap and Fish Trap show a live ghost as soon as they are equipped, one click sets the trap, and a right click puts it away. The "Next step" line above is superseded: Mike picks the next item from the "still designed, not built" list (placement gaps remain: Campfire on roof or rock, no spacing ring, lake wording, no gamepad).
