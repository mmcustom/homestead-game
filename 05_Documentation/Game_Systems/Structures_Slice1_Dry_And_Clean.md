# Homestead
## Structures, Slice 1: Dry and Clean (design draft)

Status: Design Draft, written 2026-10-19 (Claude). Nothing is built. Slice 1 of Structures_Roadmap.md, confirmed by Mike 2026-10-19 ("Yes, use all seven"). Every number marked "(Claude, not yet confirmed)" is a first pass to tune in Play Mode; real-world figures are sourced at the bottom and converted to the game's metres.

Slice 1 is four structures: a **Well**, an **Outhouse**, a **Wood Shed** (covered firewood storage) and a **Covered Outdoor Kitchen**. They all work with systems that exist today (water quality, Placement Preview, Wood Pile, Cooking and Fire, Weather, Comfort). The frost-proof water storage Mike also asked for waits for Slice 2 (Winter water).

---

# What this slice is for

- **Well:** a steadier water source than a pond or the creek, which can be placed at the homestead instead of walking to the water.
- **Outhouse:** gives the homestead sanitation, and gives the Well a real reason to be placed carefully.
- **Wood Shed:** keeps fuel dry and lets it season, so firewood becomes something to plan for, not just collect.
- **Covered Outdoor Kitchen:** a roofed place to cook and eat in rain and heat.

Together they make the camp feel like a homestead instead of a collection of piles, and each one joins the loops in the roadmap (water, fuel, food).

---

# Shared rules: setbacks (real distances turned into game metres)

Ohio's private water system rule (OAC 3701-28-07, Table 1) sets minimum distances between a well and sources of contamination. A game pit privy leaches into the ground, so the leaching-privy figure applies.

| Rule | Real figure | Game figure (Claude, not yet confirmed) |
|---|---|---|
| Well to a leaching pit privy | 100 ft (Ohio) | 30 m |
| Well to a watertight vault privy (later upgrade) | 50 ft (Ohio) | 15 m |
| Well to animal pens, stables, manure piles | 50 ft (Ohio) | 15 m (used once Livestock exists) |
| Well to a building foundation | 10 ft (Ohio) | 3 m |
| Privy to a lake, river, stream or dug well or spring | 30 m / 98 ft (Newfoundland pit privy guide) | 30 m (matches Ohio's 100 ft) |
| Privy to a building | 7.5 m / 25 ft (Newfoundland guide) | 7.5 m |
| Well sited up-gradient of contamination sources | Ohio paragraph (B) | The well must stand on higher ground than any outhouse or animal pen near it |
| Rain runs away from the well | EPA well siting advice | Not on the lowest ground; refuse a spot in a hollow |

**Where the rules apply in the game:** they are checked by Placement Mode (the green/red ghost), so a bad spot shows red with the reason ("Too close to the outhouse: 30 m needed"). Whichever structure is placed second is the one that gets refused, and the message names the other one. The existing creek, pond and Spring Hollow zones count as water for the privy rule. Water bodies are treated as the creek and ponds (the 14 m Spring Hollow zone and 32 m pond zone are already known to the code).

**Real-world note, for the Journal flavour line:** in Ohio, installing a well needs a permit from the local health district, and the health district advises testing for bacteria once a year (Greene County Public Health). The game does not model permits.

---

# 1. Well

**What it is.** A hand-dug, stone-lined well, the first step on the water path. A drilled well comes later as a money sink (Economy_System.md lists Well Drilling as one). This version is the hand-dug one only.

**Real basis (EPA).** Dug wells are typically 10 to 30 ft deep (3 to 9 m), lined with stone, brick or tile, and not continuously cased. They are the shallowest type and the most exposed to surface contamination. Drilled wells are the safest because of their depth and continuous casing. Driven wells (30 to 50 ft) are also easily contaminated. A well cap keeps out debris, insects and small animals, and the well should be sited so rain runs away from it.

**Rules (Claude, not yet confirmed):**
1. **Build.** Needs the Shovel and materials: a stone lining and a timber cover with a roller or hand windlass. First pass 10 Stone + 2 Logs + 4 Branches, no Cordage (the Cordage shortage is still open). Claude Code checks Stone's weight so one trip stays under the 40 kg Encumbered threshold (Logs 8 kg, Branches 1 kg; max carry 45 kg).
2. **Digging takes work.** Several in-game hours of digging with the Shovel, with stamina cost, split into stages so it can be interrupted. First pass about 6 in-game hours total. Placeholder.
3. **Quality.** Good (5% illness per litre, same as Rain Collection) when the well is clear of every setback and covered. Questionable if a setback is broken (placement refuses that, but a structure built later can be too close, for example an outhouse put too near an old well). A flood that reaches the well makes its water Unsafe for a while, because surface water enters a dug well. It can be boiled like any other source.
4. **Supply.** Not unlimited. A well holds a buffer of water that recovers over time, so drawing a lot in a short time leaves it temporarily low. First pass buffer 150 L, recovering about 20 L per game hour (Claude, not yet confirmed). In a drought (the existing dry-days counter) recovery halves. Real dug wells can run low in a long dry spell, so this is the honest version.
5. **Winter.** No freezing in version 1. The Winter water system (Slice 2) decides whether a well cover or windlass can ice up. A buried pitless connection is the real-world answer and belongs there.
6. **Use.** E fills Bucket, Canteen and cooking pot like any other source. It joins the existing water source code so quality, Boiling and the quality rows work without new rules.
7. **Placement.** Placement Mode, with the setbacks above. Not inside the Spring Hollow or pond zones, not on water, slope limits as for other piles. The floodplain: refused near the creek channel (the 30 m privy-to-stream rule covers the outhouse; for the well, a low spot flooded by the creek is refused with "This ground floods").
8. **Dismantle.** Hammer Dismantle returns half the build materials, same as every other structure. Filling in a well is not modeled.
9. **Save.** Position, buffer level, and flood-contamination timer. Old saves have no wells.
10. **Not in version 1:** well pump and plumbing (Large Cabin tier), drilled well, water table varying by place, well testing as an item.

---

# 2. Outhouse

**What it is.** A pit privy: a hole in the ground with a small shack over it. Cheap, and for most homesteaders the first sanitation structure.

**Real basis.** Newfoundland's pit privy guide: pit at least 1 m by 1 m and 1.5 m deep; bottom at least 1 m above the water table or bedrock; avoid boggy ground; 30 m from dug wells, springs and streams; 7.5 m from buildings; when the contents rise within 45 cm (18 in) of the surface, dig a new pit, lime the old one and fill it with earth; a screened vent duct above the roof and a screened window. Ohio treats a leaching privy as needing 100 ft from a well and a watertight vault privy as needing 50 ft.

**Rules (Claude, not yet confirmed):**
1. **Build.** A Shovel for the pit and a small shack. First pass 2 Logs + 6 Branches + 3 Tall Grass (roof), about 22 kg of Logs and Branches; Tall Grass weight to check. No Cordage.
2. **Placement.** Placement Mode with the shared setbacks (30 m from the Well, creek, ponds and spring; 7.5 m from the cabin and other buildings), not on a slope above the well, not in a hollow or a flooded spot. The ghost's reason names the nearby thing it is too close to. The mouse wheel turns it so the door faces the way you want.
3. **Effect on the player (small but real, per the confirmed decision).** Three things, all first-pass:
   - **Sanitation state.** A homestead with a working outhouse within about 60 m of where the player sleeps is "sanitary"; without one, or with a full one, it is not. (60 m is a placeholder.)
   - **Comfort.** A sanitary homestead adds a small bonus to the Comfort ratings in Building_Housing_System.md (sleep quality, recovery, morale). Size is Claude Code's first-pass call after reading the Comfort code.
   - **Illness risk.** A non-sanitary homestead gives a small extra illness chance from poor water and spoiled food (exact hook depends on what Health exposes; Claude Code to report). No bladder or waste meter is added; the player does not have to "use" it, and no new need is introduced.
4. **It fills and must be moved.** The pit fills with use over time. First pass: it lasts about one in-game season of one player's use before it is "full" (Claude, not yet confirmed; real pits last years, game time is compressed). When full it stops counting as sanitary, and the prompt says "The pit is full." The fix is to dig a new pit nearby and move the shack (Shovel, a few in-game hours, and the old pit is limed and filled, as the real guide says). Hammer Dismantle still works and refunds half the materials.
5. **Breaking the rules has a cost, not just a red ghost.** Placement refuses a spot that breaks a setback, whichever of the two is built second. If a well and an outhouse are already too close anyway (for example in a save made before this rule existed), the well's water is one tier worse.
6. **Vault upgrade (later).** A watertight vault privy halves the setback to 50 ft (15 m) and never "leaches". Not in version 1.
7. **Humanure composting.** Mike's 2026-09-26 instruction was to decide sewage and composting later. This slice does not decide it. A pit privy plus the composting bins in Slice 4 is the point where it comes due.
8. **Save.** Position, facing, fill level. Old saves have none.
9. **Not in version 1:** a bladder or hygiene need, flies and smell, a privy for animals, a sink, a septic system.

---

# 3. Wood Shed (covered firewood storage)

**What it is.** A roofed, open-sided rack that holds cut wood off the ground, so it stays dry and seasons. It is the Wood Pile's upgrade.

**Real basis.**
- Seasoned firewood should be under 20 percent moisture (Illinois Extension); Northern Woodlands gives 20 to 25 percent. Green wood can be over 50 percent water.
- Seasoning takes about 6 to 9 months (Illinois Extension), 6 months to a year (Northern Woodlands), and about a year to be fully seasoned (Firewood for Life).
- Burning green wood wastes about 30 percent of its BTUs driving off moisture and leaves creosote in the flue (Northern Woodlands). Wetter wood makes more smoke and less heat; creosote is what causes chimney fires (Illinois Extension).
- Wood acts like a sponge and re-absorbs moisture in wet weather, so it needs a roof, but full covering without airflow slows drying. The best setup is a shed that fully covers the wood and is open on at least two sides, with the wood off the ground (Northern Woodlands). The roof should extend past the pile so drips do not reach it, and slatted sides with 2 to 4 inch gaps let wind through (Firewood for Life).

**Rules (Claude, not yet confirmed):**
1. **Build.** Pile-style build with the placement ghost. First pass 3 Logs + 6 Branches + 4 Tall Grass (roof), about 30 kg of Logs and Branches, under the 40 kg Encumbered threshold; Tall Grass weight to check. No Cordage.
2. **Wood has a moisture state (the light version, per the confirmed decision).** Fuel is either **dry** or **wet/green**. Freshly felled and chopped wood, and any wood rained on in an open pile, is wet. Wet wood burns at about **70 percent of its normal fuel time** (the "about 30 percent of BTUs wasted" figure from Northern Woodlands; first pass, Claude, not yet confirmed). Dry wood burns as it does today.
3. **Seasoning.** Wet wood in a Wood Shed becomes dry over time. Real time is 6 to 12 months; the game scales this to its calendar, first pass about one in-game season (Claude Code to scale it against the day and season length). Wood in an open Wood Pile also seasons, but it is re-wetted by rain and does not finish while it keeps getting wet. Items already record the day they were acquired, so this can reuse that stamp if the Wood Pile keeps batches (Claude Code to check).
4. **Protection.** A shed holds wood dry through rain and snow. This is the shed's real job: stored firewood that is ready to burn when Winter arrives.
5. **Capacity.** At least a Wood Pile's, probably more per tile; Claude Code reads the Wood Pile's actual limits first.
6. **Use.** Same transfer screen as the other storage (R and E open it): store Sticks, Branches, Logs and Firewood, take them out with a click or Shift-click. A wet or dry label shows on the rows.
7. **Fuel.** Fires and the hearth already burn Sticks, Branches, Logs and Firewood directly. Wet or dry only changes burn time. No chimney or creosote simulation yet (the real reason dry wood matters); that can wait for a hearth and chimney upgrade.
8. **Placement.** Placement Mode; rotation with the wheel (open front faces away from the wind, as the real guides say; the shed shows which way it opens). Not on water or too steep, not inside the floodplain, at least a short distance from the cabin (real fire practice; first pass 3 m).
9. **Dismantle and save.** Hammer Dismantle (must be empty, refunds half rounded down), saved like the other piles. Old saves have none.
10. **Future:** a splitting block, kindling, selling firewood at the Trading Post (Established Homestead revenue).

---

# 4. Covered Outdoor Kitchen

**What it is.** A roofed, open-sided cooking and eating area with a hearth, for use in rain and heat. Cooking in the open is already possible; this is the sheltered version.

**Real basis.** The only clearance figures I found for covered outdoor cooking were for gas appliances (IRC and manufacturer specs), which do not carry over to a wood fire. For wood, the design relies on common sense and the shed research: an open-sided shelter so smoke escapes, a roof well clear of the fire, and a noncombustible hearth. This is not sourced; Claude Code and Mike should treat the numbers as game tuning.

**Rules (Claude, not yet confirmed):**
1. **Build.** Pile-style build with the ghost. A roof on posts over a stone hearth, with a small prep table. First pass 3 Logs + 6 Branches + 4 Tall Grass + 4 Stone + 2 Clay (about 30 kg of Logs and Branches plus the hearth); reuses the cabin's hearth parts (stone slab, clay backing). Claude Code checks weights so a build trip stays under the Encumbered threshold. No Cordage.
2. **Hearth.** Works as a Campfire (same fuel types and burn rules) placed under the roof, so Cooking, boiling water and the Cooking screen all work. Whether rain stops an open fire today is unknown and is a Claude Code read-only question; if it does, the shelter lets you cook through rain. If it does not, the shelter is a comfort and shade structure only.
3. **Roof material.** Not thatch, because a thatched roof over an open fire is a real fire risk. First pass uses a plank-or-slab roof: Logs and Branches for the frame, Clay or bark for cover. Fire spread is not modeled in version 1, so this is a look and cost choice, not a mechanic.
4. **Comfort.** Eating a meal while standing under the roof counts as a "sheltered meal": a small morale and comfort bonus (size is Claude Code's call after reading the Comfort and Health code; first pass small). Dining: a bench or table is part of the structure (decor, no extra build step in version 1).
5. **Weather.** Rain, Snow and Thunderstorm do not wet the player under the roof (reuses whatever "sheltered" test the Tent, Lean-To and Tarp Shelter already use; Claude Code to check). In a Heat Wave the roof gives shade, which reduces sun load (the heat model already has a sun term; Claude Code to check whether shade can be read).
6. **Placement.** Placement Mode; at least 3 m from other structures and from trees (fire safety, first pass), not on water, not in the floodplain, slope limits as other piles.
7. **Dismantle and save.** Hammer Dismantle (hearth must be out, same rule as the cabin; refunds half). Saved with the world. Old saves none.
8. **Not in version 1:** a cooking-speed bonus, a stove or oven, food drying over the fire, a cook-and-serve group meal, seating for companions.

---

# Build plan (proposal, Claude, not yet confirmed)

Build order, each with its own playtest checklist, after a Claude Code read-only pass:

1. **A. Setbacks helper plus Well plus Outhouse together.** They share the distance rules, the water-quality hook and the "too close to X" ghost reasons. This is the biggest piece.
2. **B. Wood Shed plus the wet/dry wood state.** Smallest and independent, but touches fuel burn time everywhere wood is burned, so it gets its own checklist and regression items (every fire and the cabin hearth).
3. **C. Covered Outdoor Kitchen.** Depends on the read-only findings about rain and fire.

Mike chose "all at once" for Placement Preview (phases 1 to 5), so a similar choice is open here: build A, B and C in one go, or one at a time. Recommended: A first, because it is the largest, plays together as a pair, and settles the shared helpers, then B and C.

---

# Questions for the Claude Code read-only pass

1. **Water sources:** how are water sources built and rated (types, quality, save data, the `WaterSource.QualityAt` path, Spring Hollow and pond zones)? Could a Well be a new source kind that joins the same quality, Boiling and quality-row code? How does the flood code (`StreamManager`, `CreekFlow`) expose a flooded-area test?
2. **Setbacks:** is there an existing way to measure the distance from a point to the creek channel, ponds and spring zones? Could a shared helper give "nearest water" and "ground higher or lower than" for the up-gradient rule?
3. **Health and Comfort:** what does Health expose that a sanitation effect can hook into (illness risk, morale)? What are the Comfort ratings and how are they computed?
4. **Wood:** how does the Wood Pile store wood (counts or batches with acquired days)? What are its capacity limits? How do fires and the cabin hearth read fuel (burn time per item), and where would a wet/dry multiplier go?
5. **Weather and fire:** do rain or wind affect a lit campfire or cooking today? What "sheltered" test do the Tent, Lean-To and Tarp Shelter use, and could a roofed structure reuse it? Can the heat model read shade?
6. **Placement:** can new pile kinds (the Rain Barrel used `PileKind` 12) and their ghost shapes, rotation and refusal reasons be added the same way? How does Hammer Dismantle choose and refund by kind?
7. **Tools and weights:** does the Shovel exist, and what does it do today (Primitive Shovel digs Clay)? What do Stone and Tall Grass weigh, so build trips stay under 40 kg?
8. **Time scale:** how long are a day and a season, so "about one in-game season" for seasoning and a full pit turns into real numbers?

---

# Decisions for Mike (Claude's recommendations in bold)

**CONFIRMED 2026-10-19 (Mike: "Yes, use all six"). All six below stand as written. Numbers stay first-pass. Next step: Claude Code read-only pass (questions above), then the build in the order below.**

1. **Well supply:** a limited buffer that recovers over time and runs slower in a drought (150 L, about 20 L per game hour, halved in drought; placeholders), not an endless tap. **Recommended.**
2. **Setbacks:** use the real Ohio distances (30 m to a leaching privy, 15 m to animal pens, 3 m to a building) and refuse bad spots with a named reason at placement. **Recommended.**
3. **Outhouse effect:** a sanitary or not-sanitary homestead state that gives a small comfort bonus and a small illness-risk change, with no bladder meter, and a pit that fills about once a season and must be moved. **Recommended.**
4. **Wet and dry wood:** wet or green wood burns about 70 percent as long; a Wood Shed keeps wood dry and seasons it over about a season. **Recommended.**
5. **Outdoor Kitchen:** a roofed hearth that cooks like a Campfire under cover, with a small sheltered-meal comfort bonus and shade in a Heat Wave; no fire-spread simulation yet. **Recommended.**
6. **Build order:** Well and Outhouse together first, then Wood Shed, then Outdoor Kitchen. **Recommended.**

---

# Sources

- Ohio Administrative Code 3701-28-07 (location of private water systems, isolation distances, Table 1): https://codes.ohio.gov/ohio-administrative-code/rule-3701-28-07
- US EPA, types of private wells (dug, driven, drilled), well caps, siting: https://19january2025snapshot.epa.gov/privatewells/learn-about-private-water-wells
- Greene County Public Health, private water systems (permits, annual bacteria testing, disinfection): https://www.greenecophoh.gov/environmental-health/private-water-systems
- Newfoundland and Labrador Department of Health and Community Services, pit privies guidance (2009): https://www.gov.nl.ca/hcs/files/publications-pitprivies2009.pdf
- University of Illinois Extension, firewood burning safety (under 20 percent moisture, 6 to 9 months, creosote): https://extension.illinois.edu/firewood/burning-safety
- Northern Woodlands, Tricks of the Trade (seasoning, 30 percent BTU loss, woodshed layout): https://northernwoodlands.org/articles/article/tricks-of-the-trade2
- Firewood for Life, firewood storage shed (roof, airflow, raised floor, about a year to season): https://www.firewood-for-life.com/firewood-storage-shed.html
- Covered outdoor cooking clearances: only gas-appliance figures were found (JLC, Clearances for Outdoor Cookers), which do not apply to wood fires, so nothing is cited for the wood-fired kitchen.

**Gaps (not found):** Ohio rules for a pit privy specifically (the Ohio page lists leaching and vault privies in its well table, not construction rules); Ohio groundwater depth near Bethel; how long a pit lasts in real use (the Newfoundland guide gives only the 45 cm fill rule); wood-fire clearances for covered outdoor kitchens; the real drought behaviour of dug wells (the EPA page gives depth and casing but not recovery rates); the game-scale numbers above (all tuning).


---

# Claude Code read-only findings and build plan (2026-10-19)

**Status: CONFIRMED 2026-10-19 (Mike: "Yes, use all"). Revised numbers and build order A, B, C stand; numbers remain first-pass tuning. Nothing built yet. Where this section disagrees with the sections above, this section wins.**

**Note on the report:** the version pasted into chat started partway through ("Proposed build plan"), so the answers to the eight questions above and the first part of the report were not seen. The plan and the "numbers that look wrong" list below are complete as pasted. If the top half held anything that changes the plan, it needs to be pasted too.

## Errors in my first draft (owned)

- **Encumbered threshold is 30 kg, not 40 kg.** My draft assumed 40 kg and said "build trips stay under 40 kg". Claude Code read the code: Encumbered starts at 30 kg (max carry 45 kg). Every carry figure in this document and in earlier messages that used 40 kg is superseded.
- **Well at 70 kg is impossible to carry.** 10 Stone + 2 Logs + 4 Branches is 70 kg (Stone works out to 5 kg each: 50 + 16 + 4).
- **Covered Outdoor Kitchen at 57.4 kg** is far over the cap.
- **Wood Shed at 3 Logs + 6 Branches** is 30 kg (3 x 8 + 6), right at the threshold.
- **Well recovery of 20 L/h** against a 150 L buffer would never run low, so the drought rule would never matter.
- **"Dig about 6 in-game hours"** is only about 7.5 real minutes of clock, so it is not a real wait.

## Revised first-pass numbers (Claude, not yet confirmed)

Rule: a single-trip build costs about 25 kg or less; anything heavier becomes a staged site (like the Small Cabin) with each delivery about 25 kg or less.

| Structure | Revised cost / method | Why |
|---|---|---|
| Well | Staged site, three stages. Stage 1 dig: Shovel blows plus a clock skip, no materials. Stage 2 lining: 10 Stone delivered in trips of 5 (25 kg each). Stage 3 cap and windlass: 2 Logs + 4 Branches (20 kg). | 70 kg total is fine across trips; no single trip over 25 kg. |
| Outhouse | 2 Logs + 6 Branches + 3 Tall Grass (about 22 kg plus grass; Tall Grass weight still to confirm) | Single trip. |
| Wood Shed | 2 Logs + 8 Branches (24 kg) | Was 30 kg. |
| Covered Outdoor Kitchen | Staged, two stages. Hearth: 4 Stone + 2 Clay (about 20 kg plus clay). Roof: 2 Logs + 6 Branches + 4 Tall Grass (about 22 kg plus grass). | Was 57.4 kg in one go. Clay and Tall Grass weights to confirm. |

**Well supply:** 150 L buffer, about 6 L per game hour (3 L/h in a drought). The drought only bites when someone is filling barrels, which is the intent. (Draft figures; tuning.)

**Digging pace:** each dig stage is a number of Shovel blows plus a clock skip (so the stage costs game time without a real-time wait). Blow count and skip length are tuning fields on the component.

**Season length:** one in-game season is about 14 real hours at a plain clock. Seasoning wood and a pit that "lasts a season" are therefore very long waits. Both get tuning fields and a debug key, and need an explicit test (see the debug keys).

**Refund rule:** Dismantle gives back half of each cost line, rounded down, so a cost-1 line refunds nothing. Open choice: accept it, or round cost-1 lines up. Low impact; Claude's lean is to accept it and note it in the Dismantle text.

## Build plan from Claude Code (three phases)

**A: Setbacks helper + Well + Outhouse (together)**
- A new pure setbacks and floodplain-test class in its own new assembly (asmdef), following the `Homestead.CreekCrossing` precedent so it can be edit-mode tested. Inputs are positions, zones, height samples and a creek-distance function (no scene dependencies).
- Edits: `WoodManager` (new kinds, pile name, cost, buildable, can-build-at, spawn, tick hooks), `WoodPile.cs` (or new `Well.cs` and `Outhouse.cs` components), `PlacementMode.cs` (facing matters for the Outhouse), `PlacementGhost.cs`, `CraftingScreen.cs`, `AxeTool.cs` (a dig target for the Shovel), `SurvivalManager.cs` (illness-roll multiplier, small sleep-recovery bump).
- Save: append fields to `WoodPileState` (litres, contaminatedHours, digProgress, builtDay for the pit). `JsonUtility` ignores missing fields, so old saves load. New `PileKind` values are appended at the end, never renumbered.
- Debug keys: the free keys are Numpad 0, ., +, -, * , / and Enter (Numpad 1-9 and F2-F12 are taken). Numpad 0 sets the nearest Well to 0 L (Shift fills, Ctrl contaminates). Numpad . fills the nearest pit (Shift empties).

**B: Wood Shed + wet/dry wood**
- Edits: `ItemDefinition` and `InventoryContainer` (a `keepsBatchDay` flag), the five batch-carry sites, `WoodManager` (kind, Accepts, Fell stamping, daily seasoning pass), `FireManager.AddFuelPieces`, the build payment and cabin refund (read batch moisture from Take with the day callback, `InventoryContainer.cs` lines 117 and 222), the `AxeTool` split (carry the day onto Firewood), `WoodPile` (UsesTransferScreen, wet/dry row label).
- Save: no new keys (`ItemStack.acquiredDay` and `WoodStack.day` are already saved).
- Debug keys: Numpad + marks the nearest shed or held wood wet (Shift dry); Numpad - advances seasoning by a season (could reuse the F-key day skip).

**C: Covered Outdoor Kitchen**
- A hearth-under-roof variant reusing `BuildFurnished`, `HearthOf` and the hearth migration. The roof needs a real collider, because sun and rain shelter come free from `OverheadCover`.
- Edits: `WoodManager`, `FireManager`, `WoodPile` or Shelter visuals. The hearth is already saved as `CampfireState`; needs a pile kind and a ghost; costs staged (above).
- Debug key: Numpad * prints the roof, shade and rain-sheltered flags.

## What stays untested until built

Everything. The setbacks numbers, well recovery, pit fill time, seasoning time, wet/dry burn time and the kitchen comfort bonus are all first-pass tuning and need a playtest checklist after each phase.

## Decision needed from Mike

Use all of the revised numbers above (staged Well and Kitchen, 6 L/h Well recovery, Wood Shed 2 Logs + 8 Branches, dig by blows plus clock skip, accept the refund-of-1 rule) and build in order A, then B, then C?


---

# PHASE A BUILT (2026-10-19): setbacks helper + Well + Outhouse

**Status: BUILT, compiles with no console errors, Edit Mode suite 75/75 (62 existing + 13 new in `SetbacksTests.cs`). NOT played in Play Mode, NOT committed, NOT pushed. Playtest checklist written: `06_AI_Collaboration/Well_Outhouse_Playtest_Checklist.md` (43 items). All numbers are public fields on the new self-creating `WellsAndOuthouses` component (first-pass tuning, Claude, not yet confirmed).**

One new test failed on its first run because of a mistake in the test data (85.5 m is 14.5 m from the pen, not 15.5 m). The test was fixed and rerun; the game code was not at fault.

## What was built

- **Setbacks helper:** a pure class (`Setbacks.cs`) in a new `Homestead.Setbacks` assembly. Covers well and outhouse distances, the uphill rule, the "fouled well" check, and floodplain and hollow tests. Refusals read like the trap ghost, for example "Too close to the outhouse: 30 m needed."
- **Well Site (staged):** the Shovel digs the shaft blow by blow (40 blows, 360 game minutes in total, spread across the blows). R stores 10 Stone for the lining, then 2 Logs and 4 Branches for the cap. The last delivery completes the Well automatically, in place.
- **Well:** 150 L buffer, 6 L/h recovery (3 in a drought). Good water when clear; Questionable if an outhouse is too close or uphill; Unsafe for 48 game hours after a flash flood reaches it. It plugs into the existing water items, so Boiling and barrels work unchanged.
- **Outhouse:** 2 Logs + 6 Branches + 3 Tall Grass, with a ghost the wheel turns. The pit fills over 28 game days. When full, E with the Shovel moves the structure 2 m to the side onto a fresh pit (rechecked against the same rules).
- **Sanitary homestead:** a working outhouse within 60 m of where you sleep gives a 0.85 illness-chance multiplier (`SurvivalManager.RollIllness`) and +0.15 on the sleep recovery multiplier. A full pit loses both.
- **Dismantle:** Hammer returns half of each cost line, rounded down; the prompt says a single item refunds nothing (the Outhouse has no cost-1 lines).
- **Save:** `WoodPileState` gains litres, contaminatedHours, digProgress, builtDay. New `PileKind` values: WellSite 13, Well 14, Outhouse 15; nothing renumbered. A test confirms old pile JSON loads with the new fields at zero.
- **Debug keys:** Numpad 0 sets the nearest Well to 0 L (Shift fills it, Ctrl makes it Unsafe). Numpad . fills the nearest pit (Shift empties it).

## Answers to my questions

- Staged site accepts partial deliveries: yes. `WellsAndOuthouses.WellSiteAccepts` caps each material at what the Well needs and opens each stage only when the previous one is done. A test delivers Stone in two trips of 5 and checks each stage.
- **Weights:** Tall Grass 0.1 kg, Clay 3.5 kg, Stone 5 kg, Log 8 kg, Branch 1 kg. Trips: Stone trip of 5 = 25 kg, Well cap = 20 kg, Outhouse = 22.3 kg. Every phase A trip is at or under 25 kg (a test checks this); Encumbered starts at 30 kg.

## Deviations from the plan (all reasonable; Mike can overrule after playtest)

1. **Not built on CabinSite.** The Well Site is its own component, `PlacedStructure` (shared by Well and Outhouse), because the cabin's cap-per-material logic does not fit stage order. It completes on the last delivery: no Inventory-screen button and no Hammer needed. Shovel digging is a new dig target in `AxeTool`. Small hooks in `WoodManager`, `PlacementGhost`, `PlacementMode`, `CraftingScreen`, `SleepManager`.
2. **Unsanitary multiplier is 1.0**, not a penalty, so a save with no outhouse is not punished. Only the sanitary side changes things (0.85). Both are fields; setting the unsanitary one to 1.25 gives the draft's penalty.
3. **"Where you sleep":** `SleepManager` records the last sleep spot (not saved). After a load it uses the nearest finished shelter.
4. **Dig skip is per blow**, not one lump per stage: each blow advances the clock by `wellDigSkipMinutes / wellDigBlows` and runs survival and fires like the Bow Drill. A half-dug shaft has cost half the time.
5. **"New pit" moves the shack 2 m sideways** onto a fresh pit and checks the same rules; it is not a separate dig-in-place action.
6. Cost-1 lines: the Outhouse has none, so "refunds nothing" never applies there.

## Note for phase C (Kitchen)

With real weights the Kitchen hearth stage (4 Stone + 2 Clay) is 20 + 7 = 27 kg, under the 30 kg Encumbered line but over my 25 kg rule. The roof stage (2 Logs + 6 Branches + 4 Tall Grass) is 22.4 kg. Options when phase C comes: 3 Stone + 2 Clay (22 kg), or accept 27 kg. Decide then. Whole Kitchen is 57.4 kg, matching Claude Code's earlier figure.

## Untested (everything in Play Mode)

Ghost and structure looks; the real water-surface and creek tests, including floodplain, hollow and zone rules on the real map (they read the live scene); digging feel and how long a shaft takes; Well and Outhouse interaction; sleep-spot detection and the sanitary effect in play; flood contamination; Dismantle and refunds; save and load of the new fields on a real save.

## Files

New: `Survival/Setbacks/Homestead.Setbacks.asmdef`, `Survival/Setbacks/Setbacks.cs`, `Managers/WellsAndOuthouses.cs`, `World/PlacedStructure.cs`, `Tests/EditMode/SetbacksTests.cs`, `06_AI_Collaboration/Well_Outhouse_Playtest_Checklist.md`. Edited: `WoodManager.cs`, `PlacementGhost.cs`, `PlacementMode.cs`, `CraftingScreen.cs`, `AxeTool.cs`, `SurvivalManager.cs`, `SleepManager.cs`, `RainDebugControls.cs`, `Homestead.Tests.EditMode.asmdef`.


---

# DESIGN RULE FROM MIKE (2026-10-19): the Hammer builds and destroys every structure

**Mike: "a hammer should be equipped to build or destroy any built structure."** This applies to everything built, not only phase A.

- **Building:** delivering materials to a staged site, advancing a stage, and finishing a structure all require the Hammer to be equipped. Placing a single-trip structure from the Build tab is covered too (what it does today is being checked).
- **Destroying:** Dismantle stays a Hammer action. Nothing is dismantled without the Hammer equipped.
- **Phase A change:** the Well Site currently completes itself on the last delivery with no Hammer (Claude Code deviation 1). That is wrong under this rule. Stored deliveries and the final "finish" step need the Hammer equipped.
- **Assumption (Claude, not yet confirmed):** the Shovel still does the digging (Well shaft, moving the full Outhouse pit), because that is digging, not building. Only the build, deliver and dismantle steps need the Hammer.
- **Open for Claude Code to report:** which existing structures (Small Cabin, Rain Barrel, Wood Pile, Campfire and others) already require the Hammer and which do not, so the rule can be applied consistently.
- **Status:** rule recorded, not yet built. Phase A playtest should wait for this change so the checklist tests the Hammer behaviour.


---

# HAMMER RULE BUILT for Well Site, Well and Outhouse (2026-10-19)

**Status: BUILT, compiles with no console errors, Edit Mode 76/76 (75 + 1 new). NOT played, NOT committed, NOT pushed.** Playtest checklist `Well_Outhouse_Playtest_Checklist.md` gained a Hammer section, items 44 to 53 (now 53 items).

## What changed

- **Well Site placement:** needs the Hammer equipped. Without it the ghost goes red with "Equip the Hammer to build." This is one rule in `CanStartBuild`, so the ghost and the real build agree. It also applies if the Hammer is put away mid-placement.
- **Well Site deliveries (R):** need the Hammer. Without it the prompt reads "Equip the Hammer to build" when carrying storable material, and pressing R stores nothing and shows that line.
- **Finishing the Well:** the last delivery still completes it, so that also needs the Hammer.
- **Taking an empty site down (E):** needs the Hammer ("Equip the Hammer to take the site down").
- **Taking materials back (E):** stays free of the Hammer (treated as undoing a delivery, not destroying the structure).
- **Outhouse placement:** needs the Hammer. The Build tab shows "Equip the Hammer to build it", the way the cabin entry does.
- **Dismantle:** unchanged and Hammer-only. Every path that calls `RemovePile` for these three structures was checked; none can remove them without the Hammer.
- **Shovel:** still digs the shaft and moves a full Outhouse pit (digging, not building). Claude Code agreed.
- **Decision recorded:** placing the Well Site needs the Hammer (follows Mike's general rule; the Small Cabin site is free to place today). Flipping it back is one condition.
- **Files changed:** `WellsAndOuthouses.cs` (rule helper and message), `WoodManager.cs` (`CanStartBuild`), `PlacedStructure.cs`, `CraftingScreen.cs` (Build tab note, help text), `SetbacksTests.cs`, `Well_Outhouse_Playtest_Checklist.md`.
- **Test:** checks that placing the Outhouse and Well Site needs the Hammer, that every existing build kind is unchanged, and the message text. Edit Mode cannot run the real Hammer-in-hand checks.

## Where the Hammer is required today (Claude Code audit)

| Structure | Place | Deliver | Finish | Dismantle | Hammer today |
|---|---|---|---|---|---|
| Small Cabin site | free | R stores, free | Complete Small Cabin: Hammer | Site: E or R, free. Finished cabin: Hammer, hearth out, R twice | Finish and finished-cabin dismantle only |
| Wood Pile, Rock Pile, Water Barrel, Rain Barrel, Food Cache, Storage Bin, Tool Rack | free (`CanStartBuild` checks materials only) | store and take, free | single step | Hammer, empty pile, half back (`WoodPile.CanRemove`) | Dismantle only |
| Tent | free | n/a | n/a | R packs up, free | No |
| Lean-To | free | n/a | n/a | R takes down, free | No |
| Tarp Shelter | free (help text says so) | n/a | n/a | R, free | No |
| Campfire | free | fuel by E or R, free | n/a | none (goes with a cabin) | No |
| Traps (Snare, Box, Fish) | free | n/a | n/a | R picks up, free | No |
| Hearth | comes with cabin | n/a | n/a | goes with cabin dismantle | Via cabin |

**Existing structures NOT following Mike's rule (unchanged, awaiting Mike's decision):** building with no Hammer: every pile and barrel, Tent, Lean-To, Tarp Shelter, Campfire, all traps; Cabin site placing and delivering are free; destroying with no Hammer: Tent, Lean-To and Tarp Shelter pack-up, trap pick-up, removing an empty Cabin site. Already following it: finishing the cabin, dismantling the finished cabin, dismantling storage piles.

## Untested (Play Mode)

Red ghost and Build tab note; equipping and unequipping mid-placement; R prompts and the empty-site refusal; completing the Well with and without the Hammer; that nothing removes these structures without the Hammer.


---

# HAMMER RULE CLARIFIED BY MIKE (2026-10-19): supersedes the two Hammer sections above where they differ

**Mike: "the hammer isn't required to deliver materials to the build site. the hammer should be equipped to build or tear down any permanent structure. 'Crafting' and 'Building' differ by needing a hammer to build, but not to craft."**

My first reading of the rule was too broad (I told Claude Code the Hammer was needed to deliver materials). Claude Code built that reading into the Well Site, and the delivery part now has to come back out.

## The rule (Mike's words, my reading of the details marked as assumptions)

- **Building a permanent structure needs the Hammer equipped.** This means placing it and, for staged sites, finishing it (the cabin already works this way: "Complete Small Cabin" needs the Hammer).
- **Tearing down a permanent structure needs the Hammer equipped** (Dismantle, and taking down an empty site).
- **Delivering or taking back materials at a build site is free of the Hammer.** Revert the Well Site delivery rule from the previous section (R stores with no Hammer; the "Equip the Hammer to build" R prompt goes away). Taking materials back (E) stays free, as it already is.
- **Crafting does not need the Hammer.** Crafting is the Craft tab. Building is the Build tab. Items that are crafted, not built, stay tool-free.
- **Assumption (Claude, not yet confirmed):** the line between "permanent" and "not" follows the Build tab versus the Craft tab. Claude Code reports which structures sit on which tab; the likely result is that piles, barrels, bins, racks, the Cabin site, the Well Site and the Outhouse are built (Hammer), while the Tent, Tarp Shelter, Campfire and traps are crafted or camp gear (no Hammer). The Lean-To is the one that could go either way; it follows its tab.
- **Assumption:** placing a staged site (Cabin site, Well Site) counts as building, so it needs the Hammer too. Delivery is the only part that is free.
- **Shovel:** unchanged; it still does the digging.

## Effect on what was built

- Well Site and Outhouse placement need the Hammer: stays.
- Well Site finishing needs the Hammer: stays (the last delivery completes the Well, so the finish now needs a clear step under this rule; Claude Code to say how it works, since "finishing" and "delivering" are the same action today).
- Well Site delivery needs the Hammer: REMOVED.
- Empty-site take-down needs the Hammer: stays.
- Existing permanent structures that are free to place today (piles, barrels, bins, racks, Cabin site) need the Hammer to place: TO BUILD.
- Existing permanent structures that are free to take down (empty Cabin site): need the Hammer: TO BUILD.

**Open design point for Mike (not yet raised):** the Well Site completes on its last delivery, so "deliver is free, finish needs the Hammer" is a conflict. The cabin solves this with a separate "Complete Small Cabin" step. The simplest answer is the same for the Well: the last delivery stores the materials, and a separate Hammer step completes the Well. Claude's lean: do this, so the Well and Cabin behave alike.

**Status:** rule recorded, change not yet built. The playtest checklist Hammer section (items 44 to 53) must be revised after the change.


---

# HAMMER AND SHOVEL RULE, FINAL WORDING FROM MIKE (2026-10-19): supersedes all earlier Hammer sections

**Mike's three points:**
1. "Permanent means the build screen and crafted is craft tab."
2. "We should be able to place the ghost without the hammer and add to the build site without the hammer, but can't construct the structure without the hammer."
3. "If digging is needed for construction, the shovel is needed to complete that step and the hammer is needed for the construction part."

## The rule

- **Permanent = Build screen. Crafted = Craft tab.** Craft-tab items (Tent, Tarp Shelter, Campfire, traps, and the Lean-To if it is on the Craft tab) never need the Hammer.
- **Ghost and build site are free.** You can bring up the ghost, place a staged build site, and deliver or take back materials with no Hammer.
- **Construction needs the Hammer.** The step that actually builds the structure (completing a staged site, or the click that builds a single-step structure) needs the Hammer equipped. Tearing down a permanent structure (Dismantle, taking down an empty site) also needs it.
- **Digging needs the Shovel.** Where a build includes digging, the dig step is done with the Shovel (no Hammer), and the construction part is done with the Hammer.
- **This matches the Small Cabin today:** the site is free to place, delivery is free, "Complete Small Cabin" needs the Hammer.

## How it applies (my reading, marked where it is an assumption; Claude, not yet confirmed)

- **Well Site (staged):** placing the site is free (this reverses the "Well Site placement needs the Hammer" change). Dig stage: Shovel, no Hammer. Deliveries (Stone for the lining, Logs and Branches for the cap): free. Finishing: a separate Hammer step, "Complete Well", like the cabin. The last delivery no longer completes it automatically. Assumption: one Hammer step at the end, not one per stage.
- **Outhouse and other single-step structures (piles, barrels, bins, racks):** the ghost shows without the Hammer, but it is red with "Equip the Hammer to build" and the click does nothing until the Hammer is equipped. Assumption: for single-step structures the click that places them is the construction.
- **Taking down:** Dismantle needs the Hammer. Taking down an empty site needs the Hammer. Taking materials back from a site is free.
- **Outhouse pit move:** Shovel only (digging). Assumption.
- **Cabin site:** already matches the rule; nothing to change except the empty-site take-down, which needs the Hammer.

## What changes in the build already done (Hammer build earlier today)

- Well Site placement needs the Hammer: REVERT (placing the site is free).
- Well Site delivery needs the Hammer: REMOVED (already planned).
- Well Site completes on the last delivery: CHANGE to a separate Hammer "Complete Well" step.
- Outhouse placement needs the Hammer: stays (single-step construction).
- Empty-site take-down needs the Hammer: stays.
- Existing single-step Build-tab structures (piles, barrels, bins, racks): need the Hammer to place: TO BUILD.
- Cabin site: delivery and placing stay free; empty-site take-down needs the Hammer: TO BUILD.

**Status:** rule final, change not yet built. Checklist Hammer items 44 to 53 need revising after the change.


---

# HAMMER AND SHOVEL RULE BUILT (2026-10-19): final version

**Status: BUILT, compiles with no errors (the only console line is MCP WebSocket noise from a reconnect), Edit Mode 77/77 (76 + 1). NOT played, NOT committed, NOT pushed. This section supersedes the earlier "HAMMER RULE BUILT" section.** Checklist: Hammer section rewritten as items 44 to 57; items 11, 19, 20 and 40 reworded.

## Build tab vs Craft tab (Claude Code report)

| Structure | Tab | Hammer after this change |
|---|---|---|
| Small Cabin | Build ("Small Cabin Site", then "Complete Small Cabin") | Site and delivery free (unchanged); Complete needs the Hammer (unchanged); NEW: taking the site down needs the Hammer |
| Wood Pile, Rock Pile, Water Barrel, Rain Barrel, Food Cache, Storage Bin, Tool Rack | Build ("Storage and Piles") | NEW: ghost shows, red "Equip the Hammer to build." until equipped; Dismantle already needed the Hammer |
| Tent | Build ("Shelter"), built from the Tent item you carry | No Hammer, unchanged |
| Lean-To | Build ("Shelter") | No Hammer, unchanged (see flag) |
| Tarp Shelter | Build ("Shelter"), built from the Tarp you carry | No Hammer, unchanged |
| Campfire | Build ("Other") | No Hammer, unchanged; it has no teardown at all |
| Traps (Snare, Box, Fish) | Craft ("Traps" category); setting one is the trap ghost, not the Build tab | Never the Hammer, unchanged |
| Well Site / Well | Build ("Well Site", then new "Complete Well") | See below |
| Outhouse | Build | NEW: ghost shows, click needs the Hammer |

## Well Site (final)

- Placing the site: no Hammer (reverted). Ghost green on a good spot, click places it.
- Delivering (R): no Hammer; the "Equip the Hammer to build" prompt is gone.
- Digging the shaft: Shovel only.
- Finishing: the last delivery no longer completes the Well. It shows a "fully stocked" banner, and a separate Hammer step ("Complete Well") appears on the Build tab.
- Copied from the cabin (fit cleanly): `SiteInReach` (6 m, tunable `wellCompleteReachMetres`), `SiteNeeds` (`WellSiteNeeds`: "the shaft dug (Shovel)", "5 Stone, 2 Logs, 4 Branches", or null), `CanCompleteCabin`/`CompleteCabin` (now `CanCompleteWell`/`CompleteWell`: stocked, Hammer, same reason wording), the Build-tab completion entry and `DoComplete...`, the "Fully stocked - equip the Hammer, open ... and press Complete ..." prompt, and the banner on the delivery that fills it.
- One difference from the cabin: "Complete Well" is its own entry beside "Well Site" (placing another stays possible); the cabin entry swaps in place.
- Taking an empty site down: needs the Hammer. Taking materials back (E): free.

## Outhouse, single-step piles, Cabin site

- The Hammer check is in `WoodManager.CanBuildPileAt`, last, so a bad spot still shows its own reason first. It no longer blocks the Build tab from starting Placement Mode. The kinds that need the Hammer (`NeedsHammerToBuild`) are the seven storage piles plus the Outhouse.
- Outhouse pit move: Shovel only.
- Cabin site: placing and delivering unchanged. Taking the site down now needs the Hammer. This also covers R with nothing to store on a stocked site, because that tears the site down and refunds everything. Claude (decision, tell me if wrong): that is tearing down, so it needs the Hammer.
- Craft tab: nothing changed. Saves: unchanged (the rule only gates actions).

## Files and tests

Edited: `WoodManager.cs`, `WellsAndOuthouses.cs`, `PlacedStructure.cs`, `WoodPile.cs`, `CraftingScreen.cs`, `SetbacksTests.cs`, `Well_Outhouse_Playtest_Checklist.md`. 77 tests, 0 failed. New or changed tests: exactly which kinds need the Hammer to build; what a Well Site still needs stage by stage, and that a fully stocked site no longer completes by itself.

## Open point for Mike

**Tent, Lean-To, Tarp Shelter and Campfire are all on the Build screen**, so by "permanent means the build screen" they would need the Hammer. I told Claude Code to leave them alone, so today they need none. The Lean-To is the odd one: Build tab, but it behaves like the portable Tent and Tarp (needs Cordage, takes down free, returns only 4 Branches, not half). Mike to decide.

## Untested (Play Mode)

The red ghost and its Hammer-equip toggling; the Build-tab "Complete Well" entry (appearance, reach, refusal); the "fully stocked" banner; the new cabin-site Hammer prompts; the Rain Barrel eave snapping under the new red reason.


**DECISION (Mike, 2026-10-19): Tent, Lean-To, Tarp Shelter and Campfire stay tool-free ("None of the four").** They are treated as camp gear even though they sit on the Build screen. No code change needed. The Hammer rule therefore covers: the Cabin, the seven storage piles and barrels, the Well, and the Outhouse (and later Slice structures). The open point above is closed. Next: playtest checklist (57 items), then the commit decision.


---

# DESIGN CHANGE FROM MIKE (2026-10-19): heavy builds become staged sites

**Trigger:** while playtesting the Outhouse, Mike had to offload almost everything from his pack to carry the 22.3 kg of materials, because a single-step build needs every material in the pack at the same moment. Mike suggested changing how multi-material structures are built (ghost with the Hammer, placeholder without). Claude's answer: the placeholder already exists as the staged build site the Cabin and Well use, so heavy builds should use it.

**CONFIRMED (Mike: "Yes, over about 15 kg"):**
- **Any build whose materials weigh more than about 15 kg becomes a staged site**, like the Small Cabin and the Well Site. The 15 kg cutoff is a first-pass tuning field (Claude, not yet confirmed); Claude Code reports each build's real total weight first so the cutoff can be set from numbers.
- **Staged site flow:** place the site with no Hammer (the placeholder), deliver materials in as many trips as needed with no Hammer (taking them back is free), then the construction step needs the Hammer ("Complete <name>", same pattern as "Complete Small Cabin" and "Complete Well"). Taking an empty site down needs the Hammer. Dismantling the finished structure needs the Hammer.
- **Builds at or under about 15 kg stay single-step** (the ghost shows, red with "Equip the Hammer to build." until the Hammer is equipped).
- **The Outhouse (22.3 kg) is the first to change.** The planned Wood Shed (24 kg) and Covered Outdoor Kitchen (hearth and roof stages, 57.4 kg in total) use the same staged-site pattern in phases B and C.
- **Not decided (Claude's note, only if Mike asks):** a pit-dig step with the Shovel for the Outhouse, in the same way as the Well shaft. Not part of this change.

**Status:** decision recorded, not yet built. Checklist items for the Outhouse (44 to 47, and 53) will need revising after the change.


---

# OUTHOUSE IS NOW A STAGED SITE (built 2026-10-19)

**Status: BUILT, compile-checked, Edit Mode 81/81 (77 + 4 new). NOT played, NOT committed, NOT pushed.** Saves folder untouched; the AudioManager.cs null guard, ProjectSettings.asset change, Homestead.slnx and 08_Backups/ untouched. This implements the "heavy builds become staged sites" decision above.

## Weight table (from the item assets; cutoff 15 kg, tuning field `stagedSiteWeightKg` on `WellsAndOuthouses`, first-pass, not confirmed)

| Build-tab structure | Materials | kg | Status |
|---|---|---|---|
| Wood Pile | 4 Sticks | 0.4 | under |
| Rock Pile | free | 0 | under |
| Water Barrel | 3 Logs | 24 | OVER, still single-step |
| Rain Barrel | 3 Logs, 2 Branches | 26 | OVER, still single-step |
| Food Cache | 2 Logs, 4 Branches | 20 | OVER, still single-step |
| Storage Bin | 6 Branches, 6 Sticks | 6.6 | under |
| Tool Rack | 4 Branches, 4 Sticks | 4.4 | under |
| Tarp Shelter | Tarp, 2 Sticks, 1 Cordage | 1.3 | under |
| Tent | Tent item | 4.0 | under |
| Lean-To | 8 Branches, 4 Sticks, 1 Cordage | 8.7 | under |
| Campfire | 3 Firewood, 6 Sticks | about 5.1 | under (1.5 kg per Firewood taken from an earlier session, not re-checked) |
| Small Cabin | 20 Logs, 10 Branches, 15 Tall Grass, 12 Stone, 6 Clay | 252.5 | already staged |
| Well | 10 Stone, 2 Logs, 4 Branches | 70 | already staged |
| Outhouse | 2 Logs, 6 Branches, 3 Tall Grass | 22.3 | NOW staged |

Nothing is within a few kilograms of the line (Lean-To 8.7 is the closest below, Food Cache 20 the closest above).

## What was reused and built

- **One staged-site mechanism, not two.** The Outhouse uses `PlacedStructure` and the Well Site mechanism. The Well-only code was generalized into a small data list in `WellsAndOuthouses`: `IsStagedSite`, `FinishedKind` (site to finished structure), `FinishedCost`, `RequiresDig` (true only for the Well), `DeliveryOrder` (groups of materials; a group opens once every earlier group is full). Generic functions: `SiteNeeds`, `SiteAccepts`, `SiteInReach`, `CanCompleteSite`, `CompleteSite`. The Well-only names (`WellSiteNeeds`, `WellSiteAccepts`, `CompleteWell`, and so on) are gone. Delivery loop, take-back, prompts, the "fully stocked" banner and the Build-tab "Complete <name>" entry live in `PlacedStructure` and `CraftingScreen`.
- **Adding the Wood Shed or Kitchen later:** add a `PileKind`, a row in each of the four data methods, and a visual. The Wood Shed's 24 kg counts as over the cutoff.
- **New:** `OuthouseSite` (PileKind 16) and its visual (four stakes round a pit-sized square, with delivered materials beside it). Placement and rotation use the Outhouse's setback rules, so a planned outhouse keeps its distances, and the door still turns with the wheel.
- **Flow:** the site places with no Hammer; R delivers and E takes back (no Hammer); "Complete Outhouse" needs the Hammer; taking an empty site down needs the Hammer; dismantling the finished Outhouse needs the Hammer. No Shovel dig stage.

## Saves

OuthouseSite appended as 16, nothing renumbered. A finished Outhouse in an old save is still kind 15 and loads as a finished Outhouse (tested). A half-delivered site saves its materials in `contents` and round-trips (tested). No new saved fields. A finished Outhouse's pit start day is now set when the site is completed, not when it is placed.

## Things that did not match the plan

- Tunable renamed: `wellCompleteReachMetres` is now `siteCompleteReachMetres` (only a default on a self-created component, so nothing is lost).
- The Outhouse's old direct-build path is gone. Only an old save or a completed site produces a finished Outhouse.
- The Numpad . pit debug key only finds finished Outhouses, so it does nothing to a site.

## Tests and checklist

Four new tests: the Outhouse over the cutoff at 22.3 kg, the cutoff defaulting to 15, and no dig stage; materials accepted in any order and capped at the cost, with two trips of 16 kg and 6.3 kg; the half-delivered round trip and the old finished-Outhouse save; and the Hammer list (Outhouse and OuthouseSite now off it for placing). Four existing tests were changed to match (renamed generic methods, the enum test with OuthouseSite = 16, the Hammer list). Checklist items 44 to 47 and 53 rewritten for the staged flow; items 46 and 47 check a 22.3 kg Outhouse goes in as 2 Logs (16 kg) then 6 Branches and 3 Tall Grass (6.3 kg) without going Encumbered. Items 1, 11, 36 and 38 and the setup and weights lines were touched up.

## Untested (Play Mode)

The staged Outhouse flow end to end (ghost colour, delivery over two real trips, the banner, the "Complete Outhouse" entry's reach and Hammer refusal, empty-site take-down prompts); the new site visual and its interaction box; actual Encumbered behaviour (Edit Mode only checks the arithmetic); loading an old save in the editor.

## Open for Mike: three barrels and the cache are over the cutoff but not converted

Water Barrel (24 kg), Rain Barrel (26 kg) and Food Cache (20 kg) still need all their Logs in the pack at once. Under the confirmed rule ("over about 15 kg becomes a staged site") they would convert. The Rain Barrel is the awkward one because it snaps to a cabin eave. The Claude Code prompt box held an unsubmitted "Convert the Water Barrel, Rain Barrel and Food Cache too"; that is a suggestion, not a decision, and nothing has been converted.


**DECISION (Mike, 2026-10-19): "Convert all three."** Water Barrel (24 kg), Rain Barrel (26 kg) and Food Cache (20 kg) become staged sites under the same rule as the Outhouse: place the site with no Hammer, deliver Logs and the rest in trips with no Hammer, Hammer to "Complete <name>", Hammer to take down an empty site or dismantle. The Rain Barrel's cabin-eave snapping has to carry over to its site and to the finished barrel. The Wood Pile, Rock Pile, Storage Bin and Tool Rack stay single-step (all under 15 kg). Existing finished barrels and caches in saves load unchanged. Not yet built. Checklist items 54 and 55 need revising after the change.


---

# WATER BARREL, RAIN BARREL AND FOOD CACHE NOW STAGED SITES (built 2026-10-19)

**Status: BUILT, compile-checked, Edit Mode 85/85 (81 + 4 new, 3 existing tests edited). NOT played, NOT committed, NOT pushed.** Console shows only MCP WebSocket noise. Saves folder untouched, Play Mode not used. Implements Mike's "Convert all three" decision.

## What was added

- **New kinds, appended:** WaterBarrelSite = 17, RainBarrelSite = 18, FoodCacheSite = 19. Nothing renumbered.
- **One row per structure** in the data methods (`IsStagedSite`, `FinishedKind`, `FinishedCost`, `DeliveryOrder`). `RequiresDig` is false for all three (no Shovel stage).
- **Materials:** Water Barrel 3 Logs; Rain Barrel 3 Logs + 2 Branches; Food Cache 2 Logs + 4 Branches. Logs and Branches in any order. Costs are read from `WoodManager.CostOf`, the same figures Dismantle halves (one source).
- **Visual:** the Outhouse Site plot (four stakes, a patch of earth, delivered wood beside it) is now a general plot builder with a size parameter, used by all four sites.
- **Wiring:** `Buildable` lists the three sites in place of the finished kinds; sites have names, help texts, a "Place Site" entry with "needs in total" notes; sites accept materials through the shared `SiteAccepts`; the Food Cache Site turns with the wheel as the Food Cache did; the existing "Complete <name>" Build-tab entries pick the new sites up automatically.
- **Direct building blocked:** `BuildPile` refuses to build a finished Water Barrel, Rain Barrel or Food Cache directly. Old saves and completed sites are the only way to get one.
- **Hammer list:** `NeedsHammerToBuild` now covers only Wood Pile, Rock Pile, Storage Bin and Tool Rack (single-step). Craft-tab items, Tent, Lean-To, Tarp Shelter and Campfire untouched.
- **Two further mechanism changes:** (1) view swap on completion: finished barrels and caches are `WoodPile` objects, not `PlacedStructure` objects, so `WoodManager.Respawn` (modelled on how `CompleteCabin` swaps its view) is called by `CompleteSite` when the finished kind is not a placed structure; the site keeps its position and facing. (2) `FinishedCost` falls back to hard-coded defaults when `WoodManager` is not running, so Edit Mode tests can read it.

## Flow

Place the site with no Hammer; deliver with no Hammer; E takes materials back with no Hammer; "Complete <name>" needs the Hammer; taking an empty site down needs the Hammer; dismantling the finished structure needs the Hammer and refunds half, rounded down. Same code path as the Well and Outhouse.

## Rain Barrel eave handling

- `WoodManager.IsRainBarrel(kind)` is true for both the Rain Barrel and its site. It drives the eave snap and "not at an eave" refusal in `PlacementMode`, the eave check in `CanBuildPileAt`, and the 1.2 m spacing between a site and a barrel. `TrySnapToEave` evaluates the site's own kind. All other rules and refusal reasons are unchanged and run first.
- The finished barrel stays where the site was (converted in place; position and facing carry over). A Rain Barrel Site does not collect rain; collection counts finished barrels only.
- **Open point 1 (Mike):** the eave is not re-checked at completion. If the cabin is dismantled before the site is finished, the barrel still completes where it stood but no longer fills from the roof. Claude Code did not block completion because the request was for it to end up where it was snapped. Say if completion should be refused in that case.
- **Open point 2 (Mike, a look choice):** the Rain Barrel Site is a small plot about 1 m across, a little smaller than a Rain Barrel's footprint, drawn like the other sites, not as a barrel outline. Easy to change.

## Saves

Finished barrels and caches: old kinds 3, 4 and 12 are unchanged and load as finished WoodPiles with their contents (water level, rain collected, stored food); one test covers all three. Half-delivered sites save and load with their deliveries; one test covers all three kinds. No new saved fields.

## Files and tests

Changed: `WellsAndOuthouses.cs`, `WoodManager.cs`, `PlacedStructure.cs`, `PlacementMode.cs`, `CraftingScreen.cs`, `SetbacksTests.cs`, `Well_Outhouse_Playtest_Checklist.md` (items 54 and 55 rewritten, new items 54a for the four single-step structures and 55a for the old-save check; 54 has a two-trip delivery for each of the three; 55 is the Rain Barrel eave check; weights, settings and setup lines updated). Four new tests: costs, weights (24, 26, 20 kg), staged status and finished kinds; partial and two-trip delivery capped at the cost; half-delivered round trips and finished barrels and caches from old kind numbers keeping contents; sites counting as Rain Barrels for eave rules. Edited three: the enum-number test (adds 17 to 19 and the old numbers), the Hammer list, the kinds that are not built directly.

**Did not match:** debug keys and the Rain Barrel debug controls act on finished barrels only, so they ignore sites. Not changed.

## Untested (Play Mode)

The three site ghosts and visuals; Rain Barrel Site snapping and the completed barrel's position; the view swap on completion; Build-tab "Complete" entries' reach and Hammer refusal; two-trip weight in practice; loading a real old save.


---

# SITE GHOSTS SHOW THE FINISHED STRUCTURE (built 2026-10-19)

**Mike's decisions:** the Rain Barrel Site aims as the barrel ("I'd rather see the barrel ghost"), then "All four" for the other staged sites. Rain Barrel eave check at completion: fine as built (no re-check).

**Status: BUILT, compile-checked, Edit Mode 85/85. NOT played, NOT committed, NOT pushed.**

- One edit in `PlacementGhost.ForPile`: the ghost is built as `FinishedKind(kind)` for any staged site (the same data method completion uses). Well Site aims as the Well, Outhouse Site as the Outhouse, Water Barrel Site as the Water Barrel, Food Cache Site as the Food Cache, Rain Barrel Site as the Rain Barrel (including its eave snapping).
- A placed site is drawn by `PlacedStructure` and keeps its plot look (stakes, earth, delivered wood) until completed.
- Wheel: unchanged (`FacingMatters`); the Food Cache Site and Outhouse Site turn, the Well Site and barrels do not.
- Rules, green/red colouring, refusal reasons, spacing and setbacks come from `CanBuildPileAt` and the setbacks helper on the site's own kind, unchanged. The ghost registers with nothing; `Finish()` strips colliders and tints.
- Well ghost: the interaction box is made only by `PlacedStructure.Bind` (a ghost never gets it) and the water disc is a flat mesh at the shaft bottom, so aiming is not blocked. **Look in Play Mode:** the Well ghost is about 1.9 m across with a roof at 1.6 m, much bigger than the old plot, so it may obscure the aim spot.
- Checklist: item 55 updated; short ghost notes on items 45, 48 and 54; new item 54b is a ghost check across all five sites, including aiming a second Well. No ghost test (needs a scene).
- **Untested (Play Mode):** the five ghosts as finished structures, their colouring, the Well ghost's refusals and size, the wheel on the Food Cache and Outhouse ghosts, and the placed plot after the click.


---

# DESIGN CHANGE FROM MIKE (2026-10-19): build by pounding with the Hammer at the site, not a Build-tab "Complete" entry

**Mike (after playtesting a Rain Barrel Site):** "it confuses me to have to open the build tab and find the 'complete' item to complete the build. Once we pick the site using the ghost and bring all the needed materials to the site, we should equip the hammer and pound until it's complete, like digging or chopping."

**The new flow for every staged site (Claude's reading, numbers first-pass, not yet confirmed):**
1. Pick the Build-tab "<name> Site" entry, aim the ghost (the finished structure), click to place the site. No Hammer.
2. Bring the materials and deliver them with R. No Hammer. (Unchanged.)
3. When the site is fully stocked, equip the Hammer, aim at the site and hold click. Each blow lands, a progress bar fills, the clock advances a little per blow and stamina drains, exactly like the Shovel digging the Well shaft. When the bar is full, the site becomes the finished structure in place.
4. No Build-tab "Complete <name>" entries any more. The Build tab keeps only the "<name> Site" (Place Site) entries.

**Details:**
- **Prompt:** fully stocked and Hammer equipped: "Hold click with the Hammer to build (0%)". Fully stocked and no Hammer: "Fully stocked - equip the Hammer to build". Not yet stocked: the existing "n of m" delivery prompt.
- **The Well:** the Shovel digs the shaft first, the stone lining, logs and branches are delivered, then the Hammer pounds it complete. The Well keeps its Shovel stage.
- **Pacing (first-pass, tuning fields, Claude Code proposes the numbers from the digging and chopping pacing and reports them):** more blows for heavier structures (Well and Small Cabin most, barrels and cache least). Each blow advances the clock by a small amount and runs survival and fires like the Shovel blows do.
- **Progress is saved** with the site (an appended field; old saves load with zero progress). Stopping and coming back keeps the progress.
- **Once pounding has begun, delivered materials are locked in** (E no longer takes them back), because they are being built in. Taking an EMPTY site down still needs the Hammer.
- **Applies to:** Well Site, Outhouse Site, Water Barrel Site, Rain Barrel Site, Food Cache Site. **Small Cabin: assumed to follow the same flow ("Complete Small Cabin" is the same confusing step);** Claude Code reports first if the cabin does not fit cleanly instead of changing it blindly.
- **Hammer rule unchanged otherwise:** placing a site and delivering stay free; the Hammer is needed for the construction step (now pounding), taking an empty site down, and Dismantle.

**Supersedes:** the "Complete <name>" Build-tab flow described in the Hammer, Outhouse and barrel sections above, and checklist items 19, 47, 51, 54 and 56.

**Status:** decision recorded, not yet built.


---

# AMENDMENT FROM MIKE (2026-10-19): no time jump per swing, for pounding or for digging the Well

**Mike: "i don't like the time jump per swing or for the well digging. everything else is fine."**

- **Hammer pounding (new flow above):** no clock advance per blow. Blows take real time only, with the normal clock running as usual; stamina still drains per blow and the progress bar still fills. The "clock advance per blow" and "survival and fires run per blow" lines in the pounding design are withdrawn.
- **Well shaft digging (built in phase A):** the per-blow clock skip is removed (it was about 9 game minutes a blow, 40 blows, 360 game minutes in total, split by `wellDigSkipMinutes / wellDigBlows`). Digging now takes real time only. The blow count (40) stays unless Mike changes it; with no clock skip, the blow count and stamina cost are the pacing.
- **Not changed, flagged for Mike:** moving a full Outhouse pit still spends 3 game hours in one lump (checklist item 33). Mike's amendment named swings and Well digging only, so this was left alone. Mike to say if it should go too.
- **Superseded:** checklist item 13 (the clock moves about 9 game minutes a blow), the "Each blow advances the clock" pacing lines in the pounding design, and the Dig skip deviation (number 4) in the Phase A report.
- **Status:** decision recorded, not yet built.


**DECISION (Mike, 2026-10-19): "yes, get rid of the time jumps when working."** The Outhouse full-pit move (3 game hours in one lump, checklist item 33) loses its time jump too, so no structure work spends a lump of game time: Well digging, Hammer pounding and the pit move all take real time only, with stamina and the progress bar as the cost. Claude's reading of scope (not yet confirmed): this covers the structure work in this slice. Other existing work actions in the game may also jump the clock (for example the Bow Drill advances the clock per blow, and chopping or digging stumps and Clay may too); Claude Code reports which ones do, and none of them is changed until Mike decides. Checklist item 33 to be revised. Not yet built.


---

# TIME JUMPS REMOVED FROM STRUCTURE WORK (built 2026-10-19)

**Status: BUILT, compiles with no errors, Edit Mode 90/90 (89 + 1 new). NOT played, NOT committed, NOT pushed.** Saves folder untouched. Implements Mike's "get rid of the time jumps when working".

**Note on the report trail:** this report says "Hammer pounding: no clock advance, as in the previous amendment", and the test count went 85 to 89 to 90, so the Hammer-pounding build (replacing the Build-tab "Complete" entries) appears to have been done in an earlier Claude Code turn. I did not receive that report, so its details (pacing numbers, how the cabin was handled, the exact prompts) are NOT yet in these docs. Mike to paste it, so it can be reconciled and the checklist items 19, 47, 51, 54 and 56 checked against what was actually built.

## What changed

- **Outhouse full-pit move:** it was an instant E press that spent 20 stamina and jumped the clock 180 game minutes. Now it is hold-click with the Shovel on the outhouse, with the dig sound, a progress bar and a percentage in the prompt. At 100% the shack moves 2 m to the side onto a fresh pit and is 0% full. It is blocked with "No room to dig a new pit here" if the setbacks or ground allow no new spot, so no blow lands. Progress is saved on the Outhouse and survives walking away and save/load. No new saved field: it reuses `digProgress`, which an Outhouse never used. E on a full pit now only tells you to hold click with the Shovel.
- **Well digging and Hammer pounding:** no clock advance.
- **Dead code:** `SkipTime` had no remaining caller and was deleted. The old time and stamina fields are deleted, not zeroed.
- **Test:** one new test checks the old time and stamina fields and `SkipTime` are gone, `outhousePitDigBlows` defaults to 10, and the pit finishes on the 10th blow.

## New pit-move numbers (first-pass, tuning field `outhousePitDigBlows` on `WellsAndOuthouses`)

10 blows, about 0.9 s a blow, so roughly 9 seconds of real time. Stamina: the usual swing cost of 4 per blow, 40 in total (the old one-off cost was 20).

## Other work actions that advance the game clock (report only, nothing changed)

| Action | Where | How much | Runs survival and fires? |
|---|---|---|---|
| Bow Drill | `BowDrill.cs` line 218, `time.AdvanceMinutes(minutesPerAttempt)` | 10 game minutes per attempt (`FireManager.minutesPerAttempt`) | Yes: survival, fires and wood |
| Sleep (and rest) | `SleepManager.cs` line 200, a 15-minute loop calling `AdvanceMinutes` | Up to the morning hour, or `restHours` in the daytime | Yes: survival, fires and wood |
| Collapse (exhaustion, heat) | `SleepManager.cs` line 309, the same loop | The collapse plan's `skipHours`, or on to dusk or dawn for a heat collapse | Fires and wood only (the player's own stats are deliberately not run) |
| Debug keys (Numpad 2, skip day/season) | `RainDebugControls.cs` line 58, `TimeManager.DebugSkip*` | 1 or 6 hours; days or seasons | No, just the clock |

`TimeManager.AdvanceToHour` exists but nothing calls it. No clock skip was found in chopping, felling, splitting, stump digging, Clay digging, fishing or crafting (the Axe swing is real time; splitting from the Inventory runs on real seconds; crafting is instant by design). **The Bow Drill is the only work action left with a time jump.** Sleep, rest, collapse and the debug keys are legitimate time skips.

## Files

`PlacedStructure.cs`, `AxeTool.cs`, `WellsAndOuthouses.cs`, `SetbacksTests.cs`, `Well_Outhouse_Playtest_Checklist.md` (items 32 to 35 and 47 revised for the hold-click pit dig with no time jump; header line says no work action in this build advances the clock).

## Untested (Play Mode)

The hold-click pit dig (blow feel, bar, percentage, dig sound); the move onto the fresh pit; the no-room refusal; half-dug progress kept across save and load; that the clock really stays steady through all of it.

## Open for Mike: Bow Drill

The Claude Code prompt box held "change the Bow Drill to real time too". That is a suggestion, not a decision. Mike said "get rid of the time jumps when working" and the Bow Drill is the one work action left, but it is an existing, tested fire-starting system, so it needs Mike's yes first.


---

# HAMMER POUNDING IS BUILT (correction to the "report gap" note above, 2026-10-19)

**Correction:** the section above said a Claude Code report for the Hammer-pounding build was missing. I should not have asked Mike for it as if it existed: I inferred it from the test counts (85 to 89 to 90). What I can confirm from `Well_Outhouse_Playtest_Checklist.md` (which Claude Code updated) is that the pounding flow IS built. The details below are read from that checklist, not from a Claude Code report, so treat the numbers as "per the checklist" until Claude Code's own summary says otherwise.

- **Flow (as built):** Place Site (no Hammer), deliver (no Hammer), then with the site fully stocked and the Hammer equipped, hold click on the site: a progress bar fills and at 100% the site turns into the finished structure in place. No Build-tab "Complete <name>" entries remain (the Build tab keeps only the "<name> Site" entries). No Hammer: the prompt is "Fully stocked - equip the Hammer to build". Hammer equipped: "Hold click with the Hammer to build (0%)". Real time only, no clock jump per blow.
- **Blow counts (first-pass tuning, per the checklist):** Outhouse about 20, Well about 30 (after the Shovel shaft, lining and cap), Water Barrel about 15, Rain Barrel about 16, Food Cache about 12, **Small Cabin about 60** (the cabin and its hearth, as before). So the Small Cabin was converted too.
- **Locking:** after the first blow the materials are locked ("The materials are built in now"); before it, E takes materials back with no Hammer. E on an EMPTY site takes it down only with the Hammer. On the cabin site, R with nothing to store no longer tears the site down once pounding has begun.
- **Saves:** progress persists across walking away and save/load (per the checklist). Exact saved fields not seen.
- **Checklist items 19, 47, 51, 54 and 56 already describe this flow.** Item 19 and its Build-tab "Complete Well" wording should be rechecked: line 30 of the checklist says "Hammer and hold click on it to build it", so it appears to be updated too.
- **Still not seen:** how the Shovel dig code was reused, any deviations Claude Code noted, and the test changes for this step. Not needed for the playtest; ask Claude Code for a short summary if wanted.


---

## PHASE A PLAYTESTED - all 58 checklist items pass (2026-10-19)

Mike played Well_Outhouse_Playtest_Checklist.md in Play Mode: items 1-43 and 44-58 all pass, no notes. This covers setbacks and ghost refusals, the Well (site, digging, lining, cap, pounding, quality, drought, flash flood), the Outhouse (staged site, pit fill, pit move, sanitary effect), Dismantle, the Hammer and Shovel rule, all five staged sites with pounding and finished-structure ghosts, no time jumps, Bow Drill real time, and old-save loading.

Status of phase A: BUILT and PLAYTESTED. Code and docs remain UNCOMMITTED and UNPUSHED (since 9b42d4c) until Mike decides.

Not covered by the checklist (tuning later): natural 28-day pit fill, a full season of Well recharge, and feel of first-pass blow counts and the 15 kg cutoff. Unsanitary multiplier stays 1.0 unless Mike sets 1.25.

Next: phase B (Wood Shed + wet/dry wood), then phase C (Covered Outdoor Kitchen), each as a staged site with Hammer pounding.


---

## PHASE B BUILT - Wood Shed + wet/dry wood (2026-10-19, NOT played, NOT committed)

Phase A was committed and pushed first (code f7f7787, docs d772648). Phase B is built on top of it. 113/113 edit-mode tests pass (22 new, WoodShedTests.cs). Checklist: Wood_Shed_Playtest_Checklist.md (73 items). Where this section differs from section 3 (Wood Shed) above, this section wins.

**Final rules as built**
- **Wood Shed is a staged site**, not a single-step build. PileKind WoodShedSite 20 and WoodShed 21 (appended). Cost 2 Logs + 8 Branches (24 kg). Free: ghost, placing the site, delivering, taking materials back. Hammer: pounding (25 blows, tuning field), taking an empty site down, Dismantle (shed must be empty; half refund rounded down = 1 Log + 4 Branches, and the refund is dry). Ghost shows the finished shed.
- **Placement:** not on water, not steep, not in the floodplain, at least 3 m from the Small Cabin. Wheel rotates; the open front starts facing away from the wind and the prompt says which way it opens and whether that is into the wind. Facing into the wind is allowed (advice only).
- **Capacity:** no limit, same as the Wood Pile (the Wood Pile has no cap in code).
- **Wet/dry:** the batch day means "the day this batch was last made wet". A batch is dry once one season (28 days, about 14 real hours, `seasoningSeasons`) has passed since that day; day 0 means dry. Wet wood burns 0.7 as long (`wetBurnFraction`); dry as before. Felling and windthrow stamp wood wet; each hour of rain or snow re-wets wood in open Wood Piles; wood in a finished shed is protected, so it dries. The campfire burns the driest batch first; splitting carries the wettest source batch's day onto the Firewood.
- **No daily seasoning pass:** seasoning is the stamp compared with today's date. (This replaces item 3 and the "daily seasoning pass" in the phase B plan above.)
- **Old saves:** wood in old piles has day 0 and reads dry; wood already in a pack carries its pickup day and reads wet until a season passes.
- **No new save keys.** `keepsBatchDay` flag on logs, branches, sticks and firewood.
- **Debug keys:** Numpad + wet (Shift dry), nearest shed first then carried wood, Ctrl forces carried wood; Numpad - advances all wood one season.
- **No time jumps** in any shed work.

**Answer for phase C:** rain does not affect a lit fire (no weather check in FireManager.PassHours); weather changes only the chance of catching when lighting, and a covered fire ignores that penalty. The Kitchen's gain is therefore lighting reliability, shelter from rain and shade, and the sheltered-meal comfort bonus.

**Untested:** all of it in Play Mode (shed look and facing, labels, real rain re-wetting, burn times against the clock, the five carry sites, saves).


---

## PHASE B PLAYTESTED - all 73 checklist items pass (2026-10-19)

Mike played Wood_Shed_Playtest_Checklist.md in Play Mode: items 1-73 all pass, no notes. This covers the Wood Shed ghost and refusals, staged delivery, Hammer pounding, the finished shed and transfer screen, wet/dry labels, seasoning, rain and snow re-wetting of open piles, shed protection, wet vs dry burn time, Dismantle, the wet/dry day at every carry place, splitting, regression for every fire and the cabin, and saves.

Status of phase B: BUILT and PLAYTESTED. Code and docs remain UNCOMMITTED and UNPUSHED (baseline d772648).

Not covered (tuning later): natural-time seasoning (28 game days tested via Numpad - and day skips), natural-weather re-wetting over a real season, a real winter of fuel use from a stocked shed, and the feel of the first-pass numbers (25 blows, wet burn 0.7, one-season seasoning, no capacity limit, wind-facing as advice only).

Next: phase C (Covered Outdoor Kitchen), staged in two stages with Hammer pounding.
