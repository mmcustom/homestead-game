# Homestead
## Water System v1.0

Status: Design Draft

---

# Purpose

Water is the most important resource in Homestead.

The Water System influences:

- Survival
- Health
- Livestock
- Agriculture
- Property Development
- Seasonal Planning

Water scarcity can become one of the largest challenges facing the player.

---

# Core Philosophy

Humans can survive:

- Weeks without food
- Days without water

Water takes priority over food.

Water availability heavily influences homestead development decisions.

---

# Water Sources

## Natural Springs

Quality:

Excellent

Advantages:

- Reliable
- Clean
- Low maintenance

Disadvantages:

- May not exist on all properties

---

## Wells

Quality:

Excellent

Advantages:

- Reliable
- Year-round operation

Disadvantages:

- Expensive to construct

Requirements:

- Drilling
- Hand pump
- Future pump systems

---

## Creeks

Quality:

Good

Advantages:

- Common
- Renewable

Disadvantages:

- Usually requires purification

---

## Rivers

Quality:

Good

Advantages:

- Large water volume

Disadvantages:

- Contamination possible

---

## Ponds

Quality:

Questionable to Unsafe

Advantages:

- Storage reservoir

Disadvantages:

- High contamination risk
- Seasonal algae growth

---

## Rain Collection

Quality:

Good

Advantages:

- Renewable
- Low cost

Disadvantages:

- Weather dependent

---

# Water Quality Levels

## Excellent

Examples:

- Springs
- Wells

Illness Risk:

Very Low

**Confirmed 2026-09-25 (Claude Code, Phase 3 — built and tested, not yet committed):** implemented as 0% illness risk per litre, whether drinking straight from the source or from carried water.

---

## Good

Examples:

- Fast-moving creeks
- Rain collection

Illness Risk:

Low

**Confirmed 2026-09-25 (Claude Code, Phase 3 — built and tested, not yet committed):** implemented as 5% illness risk per litre. Empirically verified in Play Mode: 400 drinks from a Good-quality creek produced sickness 15 times, about 4%, close to the 5% target.

---

## Questionable

Examples:

- Slow streams

Illness Risk:

Moderate

Purification recommended.

**Confirmed 2026-09-25 (Claude Code, Phase 3 — built and tested, not yet committed):** implemented as 20% illness risk per litre.

---

## Unsafe

Examples:

- Stagnant ponds
- Floodwater

Illness Risk:

High

Purification strongly recommended.

**Confirmed 2026-09-25 (Claude Code, Phase 3 — built and tested, not yet committed):** implemented as 45% illness risk per litre.

---

# Water Purification

Methods:

## Boiling

Benefits:

- Eliminates most biological contamination

Requirements:

- Fire
- Container

**Confirmed 2026-09-25 (Claude Code, Phase 3 — built and tested, not yet committed):** implemented at a lit campfire — raw water rows in the Inventory screen get a Boil button, gated on carrying the Bucket (see Item_Data.md's Tools table). Boiling turns raw water into a Purified Water item at 0% illness risk, reusing the old, previously-unused generic `water` item id from Item_Data.md's Consumables table rather than adding a new one; it still counts toward the Bucket's capacity. The drink prompt on any non-Excellent water now warns the player before they drink, e.g. "Drink (Good water, slight risk of sickness)." Getting sick from unpurified water runs through the new Sickness mechanic on Health_System.md's Illness System section. Tested: boiling Good water into Purified Water worked via both the dedicated Boil button and a click on the water row in Inventory.

**Correction 2026-09-26 (Mike) — reverses the gating above.** A wooden Bucket can't actually go over a campfire to boil water without burning. Boiling should be gated on carrying a new Tool, a metal cooking pot, instead — not the Bucket. The Bucket's role narrows to transporting water and other liquids (its existing hand-carrying job everywhere else), never boiling. This is a real change to already-shipped, committed code (the current Boil button check), not just a doc update. `water-pouring-a.wav` (Audio_System.md) is meant for this: pouring carried water out of the Bucket into the cooking pot before boiling. Exact capacity, the pour interaction, and whether the cooking pot is placed (like the Campfire) or a carried Tool are Claude Code's call — see Primitive_Storage_System.md and Item_Data.md's new Tools row.

**Built, tested, committed and pushed 2026-09-26 (Claude Code)** as part of commit `4145888` (`8b3ea6e` carries the doc updates). Boiling now refuses outright without the cooking pot: "Boiling water needs a Cooking Pot – the wooden Bucket would burn." The Bucket carries water only. Boiling first pours carried water from the Bucket into the pot, up to 4 L at a time, playing `water-pouring-a.wav`; the pot itself is carried, not placed. **Claude Code's own call, flagged for Mike to confirm:** the pot isn't craftable — nothing on the property can produce metal, so it's in the new-game starting kit alongside the Bucket, with F11 granting one to existing saves that predate it (including Mike's own). A craftable clay pot or similar was offered as a quick change if Mike would rather it be earnable than given. Tested in Play Mode: boiling refused without the pot, worked once equipped.

**Confirmed 2026-09-26 (Mike):** keep the metal cooking pot as a starting-kit item, not craftable — Claude Code's implementation above stands as-is, no code change needed. **Future — Clay Pottery.** Mike likes the idea of clay pots (and eventually cups, plates, bowls, etc.) craftable from clay deposits found around the property, as a later addition — not this batch, not yet designed in any detail (no deposit locations, recipe, or which vessels come first). Logged here so the idea isn't lost; Property_Layout.md would need a new clay-deposit terrain feature (same pattern as Stone_Gathering_System.md's cliff-side rock deposit) whenever this gets picked up.

---

## Filtration

Future System

Benefits:

- Improves water quality

---

## Combined Purification

Highest water safety.

---

# Water Transportation

Early Game

## Hand Carrying

Methods:

- Buckets
- Yoke and Buckets

Advantages:

- No construction required
- Available from Day One

Disadvantages:

- Time-consuming
- Limited volume per trip
- Physically demanding

---

## Wagon or Cart Hauling

Methods:

- Barrel on Cart

Advantages:

- Larger volume per trip

Disadvantages:

- Requires a cart
- Requires passable ground

---

# Future Water Transportation

Not Alpha 0.1

Examples:

- Piped Distribution
- Pressure Water Systems
- Powered Pumps

Deferred until the core survival loop is proven fun.

---

# Water Storage

## Cistern

Purpose:

Bulk water storage from rain or hauled water.

Benefits:

- Reduces trips to source
- Buffer against dry spells

---

## Rain Barrel

Purpose:

Small-scale rain collection storage.

Benefits:

- Low cost
- Easy to construct early

---

# Discovery Integration

Water sources must be discovered before they appear on the map.

Journal Records:

- Location
- Water Quality
- Discovery Date

Example:

Discovery Unlocked

Natural Spring

Water Quality: Excellent

Journal Updated

Map Updated

Full discovery behavior is defined in Discovery System documentation.

---

# Season Effects

## Spring

Highest water availability.

Rain and snowmelt replenish most sources.

---

## Summer

Variable availability.

Droughts may occur.

Livestock water needs increase.

---

## Fall

Generally stable availability.

---

## Winter

Surface water may freeze.

Springs and wells remain valuable and may become the only reliable sources.

Detailed seasonal behavior is defined in Season System documentation.

---

# Livestock Integration

Livestock require daily access to clean water.

Water needs increase during:

- Summer
- Drought
- High activity

Full livestock water requirements are defined in Livestock System documentation.

---

# Economy Integration

Water infrastructure is a primary Economic Sink.

Examples:

- Well Drilling
- Cistern Construction

Full economic sink detail is defined in Economy System documentation.

---

# Design Rules

1. Water takes priority over food.

2. Water quality determines illness risk.

3. Purification reduces risk but requires fire and time.

4. Water availability should shape where and how the player builds.

5. Winter should make water noticeably harder to secure.



---

# Rainwater Collection and Pond Drying (design draft)

**Added 2026-10-18 (Claude, first pass, not yet confirmed).** Mike chose these two together (the pond dries out in a dry summer, the Rain Barrel is the player's counter), after the sustainability research in Water_Sustainability_Research.md. Nothing here is built. Every number is a starting point for tuning. Build order: Rainwater Collection first, Pond Drying second, each with its own Play Mode checklist, both after a Claude Code read-only pass (questions at the end of this section).

## What exists today (from the docs; Claude Code to confirm in code)

- Rain Collection and Rain Barrel/Cistern are listed above as concepts only. Weather_System.md already says Heavy Rain "refills ponds" and "refills cisterns", but nothing reads it.
- The primitive Water Barrel exists (Primitive_Storage_System.md): 3 Logs, 40 L, per-quality rows, Bucket pours in, E draws out. It is hand-filled only.
- Water quality is per litre: Excellent 0%, Good 5%, Questionable 20%, Unsafe 45% (Water_System.md, confirmed). Rain Collection is rated Good.
- Weather has Light Rain, Heavy Rain and Thunderstorm. Precipitation falls as Snow at or below 32 F. WeatherManager tracks a rain-recently timer and dry days; `IsDrought` exists but nothing reads it. (Corrected 2026-10-18 by Claude Code: the code has a `heatWaveCountsAsDry` flag, default off, so the dry-day counter freezes during a Heat Wave instead of racing to the 5-day threshold.)
- Ponds: Property_Layout.md puts the main pond at Bass Hole, where the creek widens (spring-fed creek, one connected water system). Water_System.md rates ponds Questionable to Unsafe, and stagnant ponds Unsafe.

## Real-world basis (sources in Water_Sustainability_Research.md and below)

- One inch of rain on one square foot of roof is 0.623 gallons. In metric that is 1 litre per millimetre per square metre, which makes the game arithmetic easy: litres = roof m2 x mm of rain x efficiency.
- Rain intensity (Fox Weather): light is up to about 0.10 in/h (2.5 mm/h), moderate 0.11 to 0.30 in/h (2.8 to 7.6 mm/h), heavy 0.30 in/h and over (7.6 mm/h and over). The mm figures are my unit conversions (1 inch = 25.4 mm), not stated in the article.
- Pond evaporation (Mississippi State Extension via Farm Progress, ONE source, Mississippi, an upper bound): in summer heat a pond can lose up to 1 inch of water per day, several feet over a summer. Falling levels are often first noticed in August. Heavy downpours can refill a pond, but dry summers lose water faster than it comes back. The article gives no per-month total and no oxygen or fish figures.
- Kentucky Fish and Wildlife: a farm pond wants at least 5 ft of water before stocking, so it does not freeze out in winter.
- A rain-barrel overflows fast: two 55-gallon barrels filled in under 2 minutes in one summer rain (anecdote, Midwest Permaculture).
- Not found: Ohio pond evaporation by month, and how fast a shrinking pond turns bad. The doc marks those as game tuning.

## Part 1: Rainwater Collection

### Design in one paragraph

A **Rain Barrel** stands under the edge of a roof and fills by itself while it rains, in proportion to the roof's area and how hard it is raining. The player draws from it with the Bucket exactly like the Water Barrel. Because rain falls in bursts, the barrel fills far faster than the player drinks, so the limit is storage, not supply: more barrels, or later a Cistern, means more water banked for a dry spell. In a dry summer or a frozen winter the barrel stays empty, which is what makes it a buffer and not a free source.

### Rules (first pass)

1. **Roof-fed.** A Rain Barrel works only when placed within about 2 m of a roofed structure's eave. First target: the Small Cabin. Lean-To and Tarp Shelter are not roofs for this (too small; keeps the first build simple). Decision 1.
2. **Fill rate.** Per game hour of rain: litres = roof area in m2 x rain mm/h x 0.8 efficiency (0.8 is Claude's placeholder for splash and evaporation losses; no figure was sourced). Game rain mapped from the intensity bands above: Light Rain 1.5 mm/h, Heavy Rain 8 mm/h, Thunderstorm 12 mm/h. Decision 3.
3. **Worked example (SUPERSEDED 2026-10-18 by the real roof figures in "Claude Code read-only findings" below: one slope is about 11.5 m2, the whole roof about 23.1 m2, and a barrel catches its own slope).** Old placeholder: 20 m2 roof, Heavy Rain 128 L/h, an 80 L barrel filling in about 40 minutes. That is the intended feel: a good storm tops the barrel up, and a dry month empties the player's reserve.
4. **Capacity.** Rain Barrel 80 L, twice the hand-filled Water Barrel (real rain barrels are about 200 L, so this is a game-scale compromise). Build cost is Claude Code's call from existing materials; the Cordage shortage flagged in Primitive_Storage_System.md means it should not need Cordage. Several barrels can stand at one eave and fill in turn. A larger Cistern (about 4 to 5 times the Rain Barrel) stays a later Building/Housing tier. Decision 2.
5. **Quality.** Rain Barrel water is Good (5% illness per litre), the same as the existing Rain Collection rating, and counts as water from a water source for Boiling. It does not change after storage in version 1: no spoilage, algae or mosquitoes yet. Those belong to the later spoilage system; when it arrives, a lid and a first-flush diverter (research: about 0.0125 gallons per sq ft of roof) are the natural upgrades. Decision 4.
6. **Drawing and moving water.** Identical to the Water Barrel: Bucket or Canteen fills at the barrel, quality rows, the shared two-way transfer screen. Open question for Claude Code: whether to extend the existing Water Barrel code with a roof-fed flag or add a sibling type.
7. **Snow and cold.** No collection when precipitation is Snow. Barrel contents do not freeze in version 1 because surface-water freezing and Winter water pressure are not built; when they are, an exposed barrel is an obvious thing to freeze.
8. **Offline and sleep.** Rain falling while the player sleeps or time-skips (collapse) still fills the barrel, using the same hourly step the weather uses, so skipped time does not lose rain.
9. **Save.** Stored litres per barrel, saved like the Water Barrel's quality rows. Old saves have no Rain Barrels, so nothing to migrate.
10. **Not in version 1:** gutters and downspouts as separate parts, first-flush, lid, Cistern, rain-fed livestock troughs, greywater.

### Why this is fun (design intent)

- Gives the Small Cabin a second purpose, so roofing matters beyond warmth.
- Gives the player something to do about a dry spell besides walking to the creek.
- Teaches a real homestead skill: plan storage for the dry weeks, not for the average week.

## Part 2: Pond Drying

### Design in one paragraph

Each pond has a **Pond Level** from 0 to 1 that falls on hot, dry days and rises with rain. The level changes what the pond is worth: shallower water is worse to drink, harder to fish and shallower to wade in. The main pond at Bass Hole is fed by the spring and creek, so it shrinks but never dries out completely; a smaller pond with no spring would be able to dry up fully. The Rain Barrel is the counter: the player who banked water in spring does not need to haul from a failing pond in August.

### Rules (first pass)

1. **Level.** 1.0 is full, 0 is dry. Each game day the level falls by evaporation and rises by rain. Decision 5.
2. **Evaporation.** Seasonal, strongest in July and August (Farm Progress). First-pass daily loss of the pond's full depth, as a fraction of a nominal usable depth of about 1.5 m: Spring 1 percent, Summer 3 percent, Fall 1 percent, Winter 0. For scale, the Mississippi source's extreme of "up to 1 inch per day" (about 25 mm) on a 1.5 m pond is about 1.7 percent per day. The 3 percent Summer figure is deliberately higher than that real extreme so a player notices the change within one game Summer (a game Summer is far shorter than a real one); it is a tuning number, not a measured one. A Heat Wave multiplies evaporation by about 2. Claude Code can tune.
3. **Rain.** Light Rain +2 percent per hour, Heavy Rain +6, Thunderstorm +10 (Claude's guesses; ponds collect runoff from a catchment, so they gain more than the rain that falls on the surface). Snow adds nothing until it melts (melt is not built; skip for now).
4. **Spring and creek feed.** The Bass Hole pond has a floor of about 35 percent in version 1, since the creek and spring keep water flowing, and the property's creek and spring themselves never dry. A pond with no feed (if one is ever placed) has a floor of 0.
5. **Effects by level (first pass):**
   - Above 0.66: normal.
   - 0.33 to 0.66: "low water". Water quality steps one tier worse (Questionable becomes Unsafe). Fishing catch odds fall (Claude Code to read Fishing_System numbers and pick a multiplier). Wading submersion is shallower (it follows the water surface, so this should come for free if the surface moves).
   - Below 0.33: "very low". Unsafe water. Fishing poor. Optional fish die-off is NOT in version 1 (the sources I read give no oxygen figures).
6. **Look.** The water surface drops with the level where the terrain bowl allows it. Claude Code must report how ponds are built (a water plane at a fixed height, a mesh, a shader) before this is promised. If lowering the surface is not practical, fall back to a UI-only effect (a pond-level note in the interaction prompt, a darker "low" tint).
7. **Pond level on the map and prompt.** The interaction prompt at a pond says the state ("Pond water (low)"), so the player can read it without a number. No percent shown without a gauge item (a later thing).
8. **Save.** One float per pond. Old saves load at full.
9. **Drought interaction.** `IsDrought` stays as is. Pond Level is read directly from rain and heat, not from `IsDrought`, so a Heat Wave does not need to be reclassified.
10. **Not in version 1:** fish die-off, deepening a pond with the Primitive Shovel, pond liners, livestock drinking from ponds, aquaponics, koi or stocked ponds, snowmelt.

### Counterplay

- Bank water early: Rain Barrels topped up in Spring.
- Use the spring and creek (never dry) for drinking in August, boiled.
- Fish earlier in the year, while the level is high.
- Later: deepen the pond, add a Cistern, stock the pond.

## Later sustainability systems (research only, not designed)

Greywater (plant-only, per Ohio rules in the research doc), a stocked farm pond with channel catfish and hybrid sunfish, koi/garden pond as a beauty or morale feature, and aquaponics (needs Power_System). Livestock must never be given greywater to drink (no source supports it). These wait for Livestock, Gardening/Farming and Power_System.

## Decisions for Mike (Claude's recommendations in bold)

**CONFIRMED 2026-10-18 (Mike: "Yes, use all six"). All six below stand as written. The numbers are still first-pass and tuned by feel in Play Mode. Next step: Claude Code read-only pass, then Rainwater Collection build.**

1. Roof-fed Rain Barrel, Small Cabin only to start. **Recommended.**
2. Rain Barrel 80 L; Cistern stays a later tier. **Recommended.**
3. Rain rates Light 1.5, Heavy 8, Thunderstorm 12 mm/h, 0.8 collection efficiency. **Recommended.**
4. Stored rainwater stays Good and does not spoil in version 1. **Recommended.**
5. Bass Hole pond floors at about 35 percent (never fully dry); effects are water quality, fishing odds and wading depth, no fish die-off yet. **Recommended.**
6. Build Rainwater Collection first, Pond Drying second, each with its own checklist, after a Claude Code read-only pass. **Recommended.**

## Questions for the Claude Code read-only pass (nothing changes)

1. Small Cabin: roof footprint in m2 and the eave positions (where could a barrel stand, and can the barrel know which roof it is under).
2. How Water Barrel is built (class, save data, quality rows) and whether a roof-fed flag fits or a sibling class is cleaner.
3. How rain is produced: per-hour hook that gives weather type, any existing rainfall amount per hour, how hourly steps run during sleep and collapse time skips.
4. Ponds and water bodies: which exist in World.unity (Bass Hole, creek, spring), how each water surface is built (plane, mesh, shader), what the Water layer is used for (wading, bucket filling, minimap, fishing), and what quality each one is rated.
5. Fishing: how catch odds are computed and where a pond-level multiplier would go.
6. Where `IsDrought` and the dry-days counter live and what reads them.
7. Debug key suggestions for rain (force rain, fill/empty a barrel) and for Pond Level (set to full / low / very low).


## Claude Code read-only findings (2026-10-18) and corrections

**Read-only pass by Claude Code (nothing edited, committed, compiled or run). The design holds; these are the code facts and the corrections they force. Claude's arithmetic below was re-checked.**

### Small Cabin roof (Shelter.cs, WoodManager.cs)

- The cabin is 4 m wide (X) by 5 m deep (Z), door on the +Z end wall. Eave line about 2.54 m (8 log courses), ridge 3.4 m running along Z. Two thatch panels, one per long side, about 23 degrees from horizontal.
- Rain falls on the plan-view area, not the sloped surface: about 23.1 m2 for the whole roof (4.28 m x 5.4 m), about 11.5 m2 per slope. (Sloped surface is about 26.8 m2, which is not what collects rain.)
- Eaves are the two long sides (local x = plus or minus 2.14 m, z from -2.7 to +2.7). The roof is flush with the outer wall, so a barrel stands just outside the wall, about 2.4 to 3.2 m from the cabin centre line. The door end has no eave.
- Placement conflict: `WoodManager.CanBuildPile` refuses any pile within 3.2 m (`cabinFootprint`) of a cabin centre. A barrel at the eave is about 2.6 m out, so a Rain Barrel needs its own placement branch.
- No saved link to the roof: each hour the barrel finds the nearest cabin in `WoodManager.Piles`, converts its own position into cabin space and tests the eave band. A dismantled cabin means the barrel simply stops filling.

### Water Barrel structure

- Not its own class: `PileKind.WaterBarrel` (enum value 3) on `WoodPileState`, drawn by `WoodPile.BuildBarrel`, opened through `StorageTransferScreen`. Litres are contents stacks keyed by item id (water_excellent, water_good, water_questionable, water_unsafe, purified). Cap is `WoodManager.barrelLitres` (40 L).
- Recommendation: add `PileKind.RainBarrel` as value 12 (appended, kinds are saved as numbers), not a roof-fed flag and not a sibling class. About 15 places test `kind == WaterBarrel`; they switch to a shared `IsWaterBarrel` helper plus a `CapacityOf(kind)` lookup. Old saves have no rain barrels, so no migration.

### Rain

- `WeatherManager.Current` is the type; `PrecipitationWeather` and `IsRaining` are derived. Light and Heavy Rain become Snow at 0 C or below; a cold Thunderstorm counts as snow for `PrecipitationWeather`.
- Nothing in the code has a rainfall amount per hour, so the mm/h values are new tunables.
- Hourly hook: `TimeManager.HourChanged` (fires for every whole hour crossed, including sleep, collapse skips and debug F5/F6/F8 skips). Do not use `WoodManager.PassHours` (fractional 0.25 h steps from sleep and collapse; debug day skips never call it).
- Ordering: `WeatherManager.OnHourChanged` counts the hour that just ended before it rolls new weather, so a barrel subscribed to `HourChanged` could run on either side of the roll. Add a `WeatherManager.HourEnded(WeatherType)` event fired at the top of that handler so it is deterministic.

### Water bodies

- ONE object, "Water (Placeholder)", on the Water layer (layer 4): one baked mesh asset (PropertyWater.asset) holding the spring disc (5.2 m radius), the creek ribbon and the pond disc, one `WaterSource` and one `MeshCollider`. Material is plain HDRP Lit (smoothness 0.95, no water shader). The object is fully static, so at runtime it is part of a batched combined mesh and cannot be edited.
- Qualities come from `WaterSource` zones: Spring Hollow (radius 14 m) Excellent; Creek (default) Good; Bass Hole pond (radius 32 m) Questionable.
- Bass Hole is centred at (-185, -150), nominal radius 24 m, bowl about 3.05 m deep at the centre, surface 0.25 m above the rim floor. The "nominal usable depth 1.5 m" in the Pond Drying section is only half the real depth; the percent-based level still fits inside the bowl.
- Water layer uses: wading (`SurvivalManager.Submersion` takes depth from `Animal.IsOverWater`, a raycast to the surface, and `PlayerController` excludes the layer from collisions), building and animal avoidance (same raycast), Bucket, drinking, fishing and traps (`WaterSource` from the collider), `FireManager`, and the minimap (`MinimapTerrain.StampWater` paints the mesh once and caches it, so a shrinking pond would not show unless the cache is cleared).
- Lowering the pond: wading follows a lowered surface automatically; the shoreline recedes naturally (a 1 m drop moves it about 3.3 m); but the live mesh cannot be moved, so the pond needs its own non-static object, and the creek ribbon's last vertices overlap the pond disc and would hang above a lowered surface, so the ribbon needs trimming at the pond rim. Doable; ship Pond Drying with level, effects and prompt text first, then prototype the lowered surface as its own step (a one-off editor tool, not a terrain rebuild). The tint and prompt fallback stays.

### Fishing

- There is no single "catch odds": it is `FishingManager.BiteWait` (seconds to a bite, by time, weather and gear), `PickSpecies` weights (per species, pond or creek, time, season, cover) and the hook chance (`hookRod`/`hookCane`). Pond water is identified through `FishingManager.WaterAt` (zone name contains "pond"), so a pond-level multiplier fits there: a water kind and level parameter on `BiteWait`, optionally a pond-level weight for Bass and Crappie. Fish Traps use `TrapHourlyChance` with `TrapManager.FishWaterAt`.

### Drought

- `dryDays` and `IsDrought` live in `WeatherManager`: `dryDays` rises each day unless it rained (saved, exposed as `DryDays`); `IsDrought` is Summer and `dryDays` at least `droughtDryDays` (5). Nothing outside `WeatherManager` reads either. `heatWaveCountsAsDry` defaults to off.

### Corrections to the draft above

1. Heat Wave and dry days: corrected inline (counter freezes during a wave).
2. Roof area: whole roof about 23.1 m2, one slope about 11.5 m2. Per slope (what a barrel catches), with 0.8 efficiency: Light Rain about 14 L/h, Heavy Rain about 74 L/h, Thunderstorm about 110 L/h. An 80 L barrel fills in about 65 minutes of Heavy Rain (about 32 minutes if it caught the whole roof).
3. Eave placement "within about 2 m" collides with the 3.2 m cabin clearance, so the Rain Barrel gets its own placement branch.
4. Pond depth: real bowl about 3 m, not 1.5 m.
5. Fishing: "catch odds" is bite wait plus species weights plus hook chance.
6. Hourly step: `TimeManager.HourChanged` (plus the new `HourEnded` event for ordering).

### Build plan (Claude Code's, Phase 1 Rainwater Collection)

1. `WeatherManager.cs`: `HourEnded` event; `RainMmPerHour(type)`; top-level tunables `lightRainMm` 1.5, `heavyRainMm` 8, `thunderstormMm` 12 (not inside the settings class); a debug force-weather method.
2. `WoodManager.cs`: `PileKind.RainBarrel` = 12 (appended); tunables on the component (`rainBarrelLitres` 80, `rainEfficiency` 0.8, eave band distances, build cost); subscribe to `HourEnded`; add the barrel to Buildable, `CostOf` and `PileName`; an eave placement branch in `CanBuildPile` with a reason like "stand beside a cabin's long wall"; `CollectRain` splits a slope's litres across its barrels in id order and loses any overflow; `CapacityOf(kind)`.
3. `RainCatchment.cs` (new, Scripts/World/): roof area, eave test and litres maths as pure functions (own asmdef like SlotSwap if EditMode tests are wanted).
4. `Shelter.cs`: promote the local roof constants to public so the roof area has one source.
5. `WoodPile.cs`, `StorageTransferScreen.cs`, `CraftingScreen.cs`: swap the barrel checks for the shared helper and capacity lookup, a larger barrel look, a crafting description.
6. `RainDebugControls.cs` (new, editor and development builds only, TimeDebugControls pattern). Keypad keys (nothing else uses them): Numpad 1 cycles forced weather (Clear, Light Rain, Heavy Rain, Thunderstorm, Clear); Numpad 2 / Shift+Numpad 2 advance 1 h / 6 h; Numpad 3 / Shift+Numpad 3 fill / empty the nearest Rain Barrel within 6 m; Numpad 4 / 5 / 6 set Pond Level full / 0.5 / 0.2 (Phase 2); Numpad 7 shows barrel litres, dry days and pond level.
- No new save fields. A Rain Barrel is a `WoodPileState` with a new kind number and litres in contents; tunables are plain fields that existing scene objects pick up with their defaults, so no scene edit.
- Phase 2 outline: a new `PondManager` saving one float, a reset in `GameManager.ResetManagers`, `WaterSource.QualityAt` and its prompt reading the level; fishing multipliers and weather hooks; the lowered surface last.

### Five questions Claude Code asked

**CONFIRMED 2026-10-18 (Mike: "Yes, use all five"). Rainwater Collection Phase 1 is APPROVED TO BUILD with the build plan above. All numbers stay first-pass and tuned by feel in Play Mode.**

1. Roof catchment: a barrel catches its own slope (about 11.5 m2) rather than the whole roof. **Recommended** (more physical; barrels on both sides each work).
2. Placement: a Rain Barrel can only be placed at an eave, with a clear prompt. **Recommended.**
3. Spacing: barrels at one eave may stand closer than the 2 m pile spacing, at 1.2 m, so three fit per side. **Recommended.**
4. Build cost: 5 Logs, no Cordage. **SUPERSEDED 2026-10-18: see "Cost correction" at the end of this doc (3 Logs + 2 Branches).**
5. Pond surface: the lowered surface comes only after the first Pond Drying version works. **Recommended.**


## RAINWATER COLLECTION PHASE 1 BUILT 2026-10-18 (Claude Code), compile-checked in the editor (0 errors, 0 warnings), NOT Play Mode tested, UNCOMMITTED

Claude Code never entered Play Mode. It tested the fill logic in edit mode with temporary objects, nothing saved. The build plan above was followed; what shipped:

- **WeatherManager.cs:** `HourEnded(WeatherType)` event fired at the top of the hour handler before weather can roll; `RainMmPerHour(type)` backed by top-level fields `lightRainMm` 1.5, `heavyRainMm` 8, `thunderstormMm` 12 (0 for Snow); debug-only `DebugForceWeather`.
- **WoodManager.cs:** `PileKind.RainBarrel` = 12; `WoodPileState.IsWaterBarrel` shared helper; `CapacityOf(kind)`; Rain Barrel in the build list; tunables on the component: `rainBarrelLitres` 80, `rainBarrelLogs` 5, `rainEfficiency` 0.8, `eaveMinOutside` 0.45, `eaveMaxOutside` 1.2, `rainBarrelSpacing` 1.2; the eave-only placement branch (refusal text "A Rain Barrel stands just outside a Small Cabin's long wall, under the roof's edge."); `AtAnEave`, `CollectRain`, debug helpers.
- **RainCatchment.cs (new):** litres-per-hour and eave-test functions. **Shelter.cs:** roof constants made public and used by `BuildCabin`, so the catchment is the roof that is drawn.
- **WoodPile.cs, StorageTransferScreen.cs, CraftingScreen.cs:** shared helper and capacity lookup; bigger barrel with a dark rim band; transfer-screen text; crafting description.
- **RainDebugControls.cs (new, editor and development builds only):** Numpad 1 forces 12 h of the next weather in the cycle Clear, Light Rain, Heavy Rain, Thunderstorm, Clear; Numpad 2 / Shift+Numpad 2 advance 1 h / 6 h; Numpad 3 / Shift+Numpad 3 fill / empty the nearest Rain Barrel within 6 m; Numpad 7 shows barrel litres, dry days (and pond level in Phase 2). Pond keys 4/5/6 skipped.
- No new save fields; old saves load unchanged.

**Real numbers.** Slope plan area 11.54 m2. At 0.8 efficiency, litres per hour per slope: Light Rain 13.9, Heavy Rain 73.9, Thunderstorm 110.8. An 80 L barrel fills in about 65 minutes of Heavy Rain. Barrels on one slope fill in id order and overflow to the next; rain past the last full barrel is lost; fractions of a litre carry to the next hour. Stored water is `water_good` and never ages. A dismantled cabin means its barrels simply stop filling (the roof is found fresh each hour).

**Changes from the spec (Claude Code's calls).**
1. **Eave band:** a barrel's centre must be 0.45 to 1.2 m outside the roof edge (not 2.4 to 3.2 m from the cabin centre line as the findings said). Barrels are 0.9 m wide and the walls are flush with the roof edge, so 0.45 m is the closest that clears the wall.
2. **Barrels per side: five, not three.** The roof is 5.4 m deep, so five barrels at 1.2 m spacing fit along one side (by arithmetic; not yet seen in play).
3. The visual refresh goes through `NotifyChanged` because nothing listens to `PileChanged`.
4. Numpad 1 forces 12 h of weather; Numpad 7 also shows dry days.
5. Claude Code did not edit this doc; the notes here are Claude's.

**Tested without Play Mode (edit-mode checks, temporary objects):** roof maths and the eave test (left slope, right slope, door end, too close, too far); `CollectRain` overflow (two barrels on one slope: 1 h Heavy Rain gave 73/0, 2 h gave 80/67, 4 more hours of Thunderstorm gave 80/80); the other slope fills independently; a barrel away from the cabin gets nothing; Snow and Clear add nothing; 10 h of Light Rain fills and carries fractions correctly; with the cabin removed the barrels stop.

**NOT tested (Play Mode checklist: 06_AI_Collaboration/Rainwater_Playtest_Checklist.md):** placing a barrel from the build screen (the 1.8 m probe, prompt text, 5-Log cost, the look), the transfer screen and drawing water with the Bucket, the hourly fill during real sleep, collapse and F5 skips, the numpad keys, and saving and loading a barrel with its litres.

Phase 2 (Pond Drying) is not started and waits for this to pass.


### Cost correction 2026-10-18 (Mike, Play Mode checklist item 2)

Mike: "i can't carry 5 logs at 1 time." A Log weighs 8 kg (Wood_Gathering_System.md), so 5 Logs is 40 kg, the entire Encumbered threshold (max carry 45 kg, tools count against it). Claude (confirming Claude Code's recommendation) did not check the Log weight; that was a design miss, not a code bug. The Water Barrel's 3 Logs (24 kg) was always buildable.

**DECIDED 2026-10-18 (Mike, taking Claude's recommendation): Rain Barrel costs 3 Logs + 2 Branches (26 kg), still no Cordage.** Build refund on Take Down is half rounded down: 1 Log and 1 Branch. Only the `rainBarrelLogs` tunable and a new Branches cost change; nothing else in the build moves. Considered and not chosen: exactly 3 Logs; building from a nearby Wood Pile (a bigger build-system change that would help every heavy build, kept as a possible later idea).

Rule going forward (Claude): check the carry weight of a proposed build cost against the Encumbered threshold (40 kg) before proposing it.

**Cost change BUILT 2026-10-18 (Claude Code), compile-checked (0 errors, 0 warnings), not Play Mode tested, UNCOMMITTED.** Only `WoodManager.cs` changed: `rainBarrelLogs` is now 3 and there is a new `rainBarrelBranches` of 2, both Inspector tunables on the component. `CostOf(PileKind.RainBarrel)` returns the Logs and Branches stacks (no Cordage). The Craft and Build rows, the placement affordability checks and Take Down Structure all read `CostOf`, so none needed its own edit; the refund works out to 1 Log and 1 Branch (half of each stack, rounded down) plus any water inside. A runtime check of the cost failed to execute, so this rests on reading the code. For Mike to see in Play Mode: the Rain Barrel entry in Craft and Build reads 3 Logs and 2 Branches, and Take Down returns 1 Log and 1 Branch.

**Cost fix and two screen fixes BUILT 2026-10-18 (Claude Code), compile-checked, NOT Play Mode tested, UNCOMMITTED.** Mike's Craft and Build screen still showed "5 Logs, 2 Branches" after the first cost change.
- **Cause (Claude Code's finding; Claude's earlier guess about a scene value on disk was only half right):** WoodManager lives in Bootstrap.unity, not World.unity, and the file on disk has NO rainBarrel*, rainEfficiency, eave* or rainBarrelSpacing values saved; no prefab or asset has them either. The 5 came from the open scene in memory: after the first compile Unity serialized `rainBarrelLogs = 5` into the loaded WoodManager, and a stored value outranks a changed default. It would have reached the scene file at the next scene save. The other tunables (80 L, 0.8, 0.45 to 1.2 m, 1.2 m spacing) are all code defaults and match this doc.
- **Fix:** the field is renamed `rainBarrelLogCount` (default 3), so a stored `rainBarrelLogs` no longer applies; no scene file edit. Only reference was in `CostOf`. Cost is 3 Logs + 2 Branches. The old `rainBarrelLogs: 5` may linger as an unused value in the scene until it is saved.
- **Footer reason:** not stale text. The detail pane shows the entry under the mouse, but the footer reason was taken from the CLICKED entry (the Wood Pile, "Needs 4 more Sticks"). `CraftingScreen.UpdateDetail` now uses the same entry for both, so hovering the Rain Barrel shows the barrel's own reason.
- **Description:** shortened to fit: "stand it under a Small Cabin's eave; it fills from the roof in rain (E draws water, R pours in). Holds 80 L."
- Files changed: `WoodManager.cs`, `CraftingScreen.cs`.
- **Console note:** Claude Code saw repeated NullReferenceExceptions from `SeasonalGround.PaintRows` (SeasonalGround.cs:197, called from Update). It did not touch that file and thinks it comes from the editor reloading scripts while a scene was running. Mike to clear the Console and see whether it comes back; if it does it is a separate bug to chase.
- **Rule going forward:** changing the default of a tunable that has already been serialized (even only in the open scene) does not change its value. Rename the field or edit the scene.


### PLAYTEST RESULT 2026-10-18 (Mike)

Rainwater Collection Phase 1: Rainwater_Playtest_Checklist.md all 26 items PASS in Play Mode. Phase 1 is playtested, still uncommitted. Pond Drying (Phase 2) is cleared to build.


## POND DRYING (WATER PHASE 2) BUILT 2026-10-18 (Claude Code report, reconciled by Claude)

Compile-checked, 0 errors, 0 warnings. NOT Play Mode tested. UNCOMMITTED. ProjectSettings.asset, AudioManager.cs and the Saves folder left alone. Numbers are first-pass (Claude, not yet confirmed).

**Files.** New: `Managers/PondManager.cs`, `06_AI_Collaboration/Pond_Drying_Playtest_Checklist.md` (36 items). Edited: `Survival/WaterSource.cs`, `Managers/FishingManager.cs`, `Managers/TrapManager.cs`, `Fishing/FishingRod.cs`, `Managers/GameManager.cs`, `Debug/RainDebugControls.cs`.

**How it works.**
- PondManager is a self-creating component (like PlacementMode), so Bootstrap.unity was not touched. Tunables are public fields, editable in the Inspector at runtime. One level float per pond keyed by zone name; a zone is a pond if its name contains "pond" (the same test as `FishingManager.WaterAt`), so a second pond gets a level automatically. Reset is wired into `GameManager.ResetManagers`.
- Daily drying once per `TimeManager.DayChanged` using that day's season and `IsHeatWave`. `IsDrought` and `dryDays` are never read.
- Rain gain hourly from `WeatherManager.HourEnded`, clamped 0 to 1.
- Floor: drying never takes a pond below its floor; a pond already below it (after debug key 6) is lifted to the floor at the next daily tick.
- Save: keyed ponds in world.json as two parallel arrays (names and levels). Old saves have no section, so every pond loads at 1.0.

**Tunables (new field names).** evaporationPerDaySpring / Summer / Fall / Winter 0.01 / 0.03 / 0.01 / 0; heatWaveEvapMultiplier 2; rainGainPerHourLight / Heavy / Thunderstorm 0.02 / 0.06 / 0.10; pondFloorNames / pondFloorLevels "Bass Hole pond" / 0.35; pondFloorOtherPonds 0; normalAbove 0.66; veryLowBelow 0.33; biteWaitMultiplierLow / VeryLow 1.5 / 2.5; trapChanceMultiplierLow / VeryLow 0.7 / 0.4. **State rule:** above 0.66 Normal; 0.33 to 0.66 inclusive Low; below 0.33 Very Low (exactly 0.66 is Low).

**Quality shift.** `WaterSource.QualityAt` records the zone and returns `PondManager.Adjust(zone, quality)`: Low = one tier worse, capped at Unsafe; Very Low = Unsafe. Spring Hollow, the creek and default water are unchanged. `QualityAt` is the only place quality is read: the interaction prompt, Bucket and Canteen fills (turned into an item id via `WaterQualities.ItemId`), and the illness roll when drinking. Boiling and filters work on the item id, so a pond fill taken while Low is already an Unsafe item. **Water already carried keeps the quality it was collected at (by design; not yet confirmed by Mike).** `FillableLitres` uses only the default quality to check pack space; left alone.

**Fishing and traps.** `FishingManager.BiteWait(canePole, pondName = null)` multiplies the wait by the Low or Very Low factor; `FishingRod.StartWaiting` passes the pond zone for pond casts, creek casts unchanged. `TrapHourlyChance(base, pondName = null)` scales the base chance first, then the existing seasonal factor; `TrapManager.RollFish` gets the name from a new `PondNameAt(position)`. Species weights, hook chances and fish die-off untouched.

**Prompt and debug keys.** Drink and Fill prompts get a suffix: "Drink (Unsafe water, high risk of sickness) - Pond water (low)" or "(very low)"; nothing for Normal or non-ponds. Numpad 4 lowers the nearest pond 10 points, 5 raises 10, 6 sets it to 0.2 (below the Bass Hole floor on purpose); Numpad 7 overlay also shows the nearest pond's level, state and floor.

**Touched outside the list.** `WaterSource.Zones` (new read-only accessor), `TrapManager.PondNameAt` (new), one line in `GameManager.ResetManagers`.

**Edit-mode checks (all passed).** Drying per season without / with Heat Wave: Spring 0.01 / 0.02, Summer 0.03 / 0.06, Fall 0.01 / 0.02, Winter 0 / 0. Rain gain Light 0.02, Heavy 0.06, Thunderstorm 0.10; Clear, Snow, Heat Wave 0. Floor (Bass Hole stops at 0.35, no-floor pond reaches 0, 0.2 returns to 0.35 at next tick, rain clamps at 1). Thresholds (0.67 Normal; 0.66 and 0.33 Low; 0.329 Very Low). Quality shift. Save round trip (0.42 and 0.9; fresh manager reads 1.0).

**NOT tested.** Everything in Play Mode (real day and hour events, HUD prompt text, debug keys, actual save and load, fishing and trap feel). The static `State` helper was checked only with no instance (returns Normal).

**Known gaps.** Shoreline, water surface and minimap do not move. No tint (one shared static mesh). Level is visible only through the debug overlay (prompt shows just the Low / very low note). Lowered surface remains a separate later step.

**Reminder (Claude's note).** With the Bass Hole floor at 0.35 the "very low" band is unreachable there in normal play; use debug key 6 to test it.

Checklist: `06_AI_Collaboration/Pond_Drying_Playtest_Checklist.md` (36 items). Back up slot 1 and restart Play Mode first.


## Flash Floods, Rising Creek and Pond Overflow (design draft, 2026-10-18)

**Added at Mike's request:** "if we're doing pond drying, shouldn't we do overflowing creeks and ponds with heavy rainfall or flash flooding?" Claude's answer: yes, it is the other half of the same water cycle, and it shares the rain hook and PondManager with Pond Drying, so it is built AFTER Pond Drying passes its checklist. Numbers marked (Claude, not yet confirmed) are first-pass.

### Real-world basis (sources in the response and research doc addendum)

- A flash flood is a quick rise in a stream that begins within about six hours of the cause, usually intense rain (NWS glossary). A short hard storm is enough; it does not take days of rain.
- Six inches of fast-moving floodwater can knock an adult off their feet; two feet of rushing water can carry away a vehicle (Army/NWS "Turn Around, Don't Drown" message). So even a shallow creek crossing becomes dangerous, not only deep water.
- Ponds: floating debris can clog overflow structures, spillway or dam damage and erosion follow big overflow events (Oklahoma State Extension).
- Fish: floods can wash young fish downstream, adults can leave a pond in moving water, and heavy rain after drought swings the water chemistry fast enough to stress or kill fish (Bass Resource). Fish die-off stays OUT of this version, as with Pond Drying.
- I did not find sourced Ohio-specific flood-season statistics in this pass; none are used in the numbers.

### Design

1. **Stream Stage (a leaky bucket).** One number, in millimetres of runoff, that rises with the rain each hour and drains with a time constant of about 6 hours (Claude, not yet confirmed). It reuses the existing rain rates (Light 1.5, Heavy 8, Thunderstorm 12 mm/h) and the `WeatherManager.HourEnded` hook. Saved as one float; old saves load at 0.
2. **States.** Normal below 15 mm. **High water** from 15 mm. **Flash flood** from 25 mm. Worked check (leaky bucket, 6 h constant): Heavy Rain alone reaches High water after about 2.3 hours and a flash flood after about 4.4 hours; a Thunderstorm reaches a flash flood after about 2.6 hours; steady Light Rain never does (it levels off near 9 mm). After a thunderstorm ends the stage falls back below flood in about 40 minutes and below High water in about 3 hours.
3. **Creek crossing danger.** High water: wading in the creek is slower and drains stamina faster. Flash flood: the player is pushed downstream and can be knocked down (stamina loss, wet and cold, minor injury). A prompt warns "The water is too fast to cross." No item loss in version 1.
4. **Water quality.** Creek water is one tier worse at High water and Unsafe in a flash flood, and stays one tier worse for about 12 game hours after (runoff). A pond that fills to 100 percent while it is still raining is "overflowing": one tier worse for about 12 hours. Spring Hollow is unchanged in version 1. Combines with the Pond Drying Low-water shift, capped at Unsafe.
5. **Fishing.** Creek: bite wait doubles at High water and the Rod and Cane Pole cannot be cast in a flash flood ("The water is too high and fast to fish."). Ponds: unchanged by floods in version 1. **Fish Traps set in the creek have a 25 percent chance of being swept away, with their contents, once per flash flood** (a trap in a pond is safe). First-pass numbers.
6. **Warnings.** The Weather panel and the Weather Station show the creek state ("Creek: Normal / High water / Flash flood") and a short toast fires at each change ("The creek is rising." / "The creek is in flood."). A Flash Flood Watch line appears when a Heavy Rain or Thunderstorm has started and the stage is climbing.
7. **No visual change in version 1.** The water is one static baked mesh (see "Water bodies"), so the creek cannot visibly rise or spill over its banks yet. Floods ship as state, prompts, danger, quality, fishing and traps first; the rising surface and overflow visuals are the same separate later step as the lowered pond surface (its own non-static object).
8. **Rain Barrel and Pond Drying interplay.** A full Rain Barrel already wastes overflow. Heavy rain still refills ponds through the Pond Drying rain gain; flooding only adds the overflow state at 100 percent.
9. **Save.** One float for the stream stage, plus the hours-of-murk counter if needed. Old saves load calm.
10. **Not in version 1:** structures damaged by flood (a "floodplain" warning on the Placement Preview for low ground near the creek is a good later idea), erosion, debris, livestock effects, fish die-off or fish washed away, snowmelt, visible flooding, a spillway or dam to build.

### Decisions for Mike (Claude's recommendations in bold)

1. Version 1 ships effects only (state, prompts, danger, quality, fishing, traps); the rising water visual waits for the non-static water step. **Recommended.**
2. Stream Stage as a leaky bucket (6 h drain), High water at 15 mm and flash flood at 25 mm, so about 2.5 hours of Thunderstorm is a flash flood and Light Rain never is. **Recommended.**
3. Creek crossing: slower and tiring at High water; pushed downstream and knocked down in a flash flood; no item loss. **Recommended.**
4. Quality: creek one tier worse at High water, Unsafe in a flash flood and for about 12 hours after; an overflowing pond one tier worse for about 12 hours; Spring Hollow unchanged. **Recommended.**
5. Fishing: creek bite wait doubles at High water and no casting in a flash flood; creek Fish Traps have a 25 percent chance of being swept away per flash flood; ponds unaffected. **Recommended.**
6. Warnings in the Weather panel, Weather Station and a toast at each change, built after Pond Drying passes its checklist. **Recommended.**

### Questions for a Claude Code read-only pass (nothing changes)

1. Creek crossing: where is the creek wadable, how does `SurvivalManager.Submersion` and the movement code work, and is there a flow direction (from the creek ribbon) to push along? Can the player be knocked down and recover safely?
2. How does `WaterSource.QualityAt` tell creek, pond and Spring Hollow apart (zone names, default zone) so the flood shift only touches the creek and an overflowing pond?
3. Where do the Weather panel, Weather Station and HUD toasts get their lines, and what is the cheapest way to add a creek-state line and a toast?
4. Fish Traps: how does `TrapManager` know a trap is in creek water, and how can a trap and its contents be removed safely in a way that survives save and load?
5. Where does the Rod and Cane Pole check whether a cast is allowed, so a "too high and fast" refusal can be added?
6. Should the stage live in PondManager or a small new manager, and does anything about the hourly hook (`HourEnded`) or the day-change hook need care for a sleep or day skip?
7. Debug keys: which numpad keys are free for "set stage" and "show stage"? (4, 5, 6, 7 are taken by Pond Drying.)

**CONFIRMED 2026-10-18 (Mike: "Yes, use all six"). All six flood decisions above stand as written. Numbers stay first-pass (Claude, not yet confirmed). Next step: Claude Code read-only pass (the seven questions above), then build AFTER Pond Drying passes its checklist.**

## Claude Code read-only findings for Flash Floods (2026-10-18), reconciled by Claude

Nothing was changed, compiled or committed. Verdict: floods are buildable as designed, with one new system (knock-down), two conflicts to settle (full-pond overflow rule, Weather panel height) and small caveats. (The pasted report had a repeated "Murk counter" paragraph from a paste glitch; its content is recorded once below.)

### 1. Creek crossing
- **Wadable everywhere in the channel.** `PropertyTerrainBuilder.CarveCreek` cuts the bed 0.12 m plus up to 0.45 m (upstream) or 0.8 m (downstream) below the surface: ankle-deep at the banks, about 0.6-0.9 m (knee to waist) mid-channel. Channel half-width 0.9 m upstream to 1.8 m downstream, so a crossing is about 2 to 4 m. The Water layer is excluded from the player's CharacterController (`PlayerController.Awake`), so there is no swimming or blocking.
- **Submersion.** `SurvivalManager.Submersion(feet)` uses `Animal.IsOverWater` (raycast to the Water layer) for surface minus feet height, mapped through ankle 0.15 m, knee 0.5 m, waist 0.9 m. It feeds only the body-heat model (water cooling, wet-skin tail), so "wet and cold" needs no new code.
- **Movement.** Wading changes nothing about movement today. "Slower and tiring at High water" needs a new speed factor and stamina drain in `PlayerController.Move` and `UpdateStamina`.
- **Flow direction is not available at runtime.** The creek centreline is a `CreekControl` array inside the Editor-only `PropertyTerrainBuilder`, baked into the static mesh. Cheapest route: a small runtime table copying the control points (spring, (-35,92), (-62,70), (-88,42), (-104,8), (-118,-30), (-136,-68), (-155,-102), (-172,-128), pond centre). Flow = direction of the nearest segment, downstream toward the pond.
- **Knock-down does not exist** (no ragdoll or stumble, no input lockout, no public damage method: `SurvivalManager.SetHealth` is private, health changes only by hourly loss, sleep or DebugSet; no stamina-drain API, only `TrySpendStamina`). To build: a short input lockout in `Move` (about 1.5 s), a stamina hit, and a new public `SurvivalManager.Injure(amount)` whose health floor stops it reaching 0.
- **Why the floor matters.** A collapse in the water wakes the player up to 16 hours later at the same spot (`CollapseRoutine` only teleports to a nearby shelter), so a flood must never cause a collapse.

### 2. Telling creek, pond and Spring Hollow apart
- `WaterSource.ZoneAt` returns "Spring Hollow" (radius 14 m), "Bass Hole pond" (radius 32 m) or null. Null = default creek water; `FishingManager.WaterAt` already treats null as `FishWater.Creek`. So creek = ZoneAt null; pond = zone name containing "pond".
- **Overlap gap.** The 32 m pond zone and 14 m spring zone overlap the creek ribbon. Creek water within 32 m of the pond or 14 m of the spring is not "creek" to this code, so floods do not affect it (quality, sweep or push). The player-push test must use the same definition.
- **Hook.** `WaterSource.QualityAt` is the single choke point; the last line `return PondManager.Adjust(found, quality)` is where the flood shift goes. Add tier shifts from both sources and cap at Unsafe: `min(base + lowShift + floodShift, Unsafe)`. A Very Low pond is Unsafe anyway, so the cap handles stacking.
- Caveat: `QualityAt` takes the player's position, not the aimed point (existing behavior; applies to the prompt too).

### 3. Weather panel, Weather Station, toasts
- `WeatherScreen.cs` has 7 free rows built in `OnShow` plus 5 Weather Station rows while the item is in the pack. One free "Creek" row filled in `OnShow` covers both the panel and the Station. Row reads "Normal", "High water" or "Flash flood".
- "Flash Flood Watch" can be derived on demand (precipitation is Heavy Rain or Thunderstorm, stage below flood, and rate x 6 > stage); no event needed. Appended to the same row or a second row.
- **Conflict (row height).** 13 rows at 52 px plus gaps plus the top gap come to 740 px, exactly the content height; 14 rows overflow. Dropping `RowHeight` to 48 fixes either case (688 px for 13 rows).
- Toasts: `ToolStatus.Flash(msg, seconds)` (one-slot toast; used by Placement Mode, catches, sleep) for "The creek is rising."; `ToolStatus.Banner(msg, 8 s)` for "The creek is in flood." Do not toast while `SleepManager.IsSleeping` or when the game state is not Playing; add a one-line summary at wake. Do not toast on load; set the previous state from the restored stage. Optional later: `WeatherHudLine` suffix "- Flood".

### 4. Fish Traps
- `TrapManager.FishWaterAt(position)` runs `FishingManager.WaterAt`: Creek for a null zone. A trap in a pond, or within 3 m of the pond centre, is "pond" and safe. A creek version of `PondNameAt` is a few lines.
- Safe removal: the `traps` list in TrapManager is the only source of truth (`CaptureState` writes it, `SpawnViews` rebuilds from it). `PickUp` already does the removal minus the inventory (traps.Remove, destroy view, views.Remove). A new `Sweep(TrapState)` with the same body is enough; contents (`catchItem`, `catchCount`) go with it.
- Checking a trap is instant (`Trap.Interact` -> `Check` -> flash), so no open screen can hold a stale reference. Save and load: a swept trap is simply absent from the save.
- **Roll once per flash flood, on the rising edge into the Flood state.** The state comes from the saved stage, so a load never rolls again. Player message: "A flood swept away a Fish Trap."

### 5. Rod and Cane Pole
- One class, `FishingRod`. `UpdateIdle` computes `kind = FishingManager.WaterAt(source, hit.point)` and handles None ("Too shallow for fish here") before casting. Add right after: kind == Creek and state is Flood -> `ToolStatus.Report("The water is too high and fast to fish.")` and return. `UpdateWaiting` should reel in if a flood starts mid-cast.
- `FishingManager.BiteWait(canePole, pondName)` (from Pond Drying) needs one more parameter, or a water kind, for the creek's x2 at High water.

### 6. Where the Stream Stage lives
- A separate small self-creating `StreamManager` (same pattern as PondManager and PlacementMode): own tunables, own save key (`stream` in the world file), own reset line in `GameManager.ResetManagers`. PondManager is per-pond; the stage is global.
- Both managers subscribe independently to `HourEnded`, so order is undefined. If the overflow rule needs the stage, `StreamManager` should call into `PondManager` after it updates.
- Formula: `S' = S * e^(-1/6) + 6 * r * (1 - e^(-1/6))` per hour with r from `WeatherManager.RainMmPerHour` (1.5, 8, 12). Reproduces the design: Heavy Rain reaches 15 mm after 2.25 h and 25 mm after 4.4 h; Thunderstorm reaches 25 mm after 2.6 h; Light Rain levels off at 9 mm.
- Sleep, day skips and collapse all go through `TimeManager.AdvanceMinutes`, which crosses hours one at a time, so each hour fires `HourEnded` exactly once. `HourEnded` fires before the weather re-rolls. No `DayChanged` hook needed. `ResetClock` and `ResetWeather` fire no events and loads restore silently, so recompute the state after a load.
- Murk counter: one more float on the same stream save. Per-pond overflow hours would need an extra array in the pond save. Old saves load without it.

### 7. Free debug keys
Used: Numpad 1-7 (Rain and Pond Drying), F2-F12 (TimeDebugControls). Free: Numpad 0, 8, 9, ., +, -, *, /, Enter, F1. **Numpad 8** cycles the stage Normal (0) / High water (18 mm) / Flash flood (30 mm); **Numpad 9** shows stage, state and murk hours (Numpad 7's overlay could also include the stage). Not checked: whether any Player or UI input action binds the numpad.

### 8. Conflicts, cheaper options, risks
**Conflicts.**
1. **Pond overflow at 100 percent while raining.** Bass Hole starts at 1.0 and Pond Drying clamps there, so every Light Rain would count as overflow and murk the pond. Cheaper rule that fits the intent: overflow only when the pond is at 1.0 AND the creek stage is High water or worse. (Needs Mike's confirmation.)
2. The Weather panel is full (section 3): drop `RowHeight` to 48.
3. No knock-down exists (section 1): the one genuinely new system.
4. Zone overlap (section 2): creek water near the pond or spring is excluded.

**Cheaper options.** One shared "creek" definition (zone null) for quality, fishing, traps and danger; derived Watch and Weather rows with no event; a flow table copied from the control points instead of reading the mesh.

**Risks.** Rain Barrel: none. Placement Preview: none (`CanStartBuild` and `CanBuildAt` untouched; a floodplain warning is the later idea). **Pond Drying: floods would modify the same files (`WaterSource.QualityAt`, `FishingManager.BiteWait` and `TrapHourlyChance`, `RainDebugControls`, `GameManager`, the pond save struct). Pond Drying is uncommitted and unplaytested; Claude Code recommends playtesting and committing it first so a floods bug cannot be confused with a drying bug.** Other: the 25 percent sweep silently destroys player gear, so the swept-trap message matters; murk persists through sleep and the counter is saved, so loading right after a flood stays murky.

### Proposed build plan (Claude Code)
1. StreamManager core: stage, states, tunables, hourly leaky bucket, save and reset, state-change event, debug keys 8 and 9, edit-mode checks (15, 25 and 9 mm thresholds, save round trip).
2. Water quality: creek shift at High water, Unsafe at Flood, 12 h murk, pond overflow in PondManager, stacking and cap in `QualityAt`, prompt notes.
3. Warnings: Creek and Watch rows with smaller `RowHeight`, toasts and banner, sleep suppression and wake summary.
4. Fishing and traps: creek bite x2, the flood cast refusal and mid-cast reel-in, the trap sweep with its roll and message.
5. Crossing danger: flow table, High-water slow and stamina drain, Flood push, `Injure` with a health floor, knock-down lockout. Riskiest, so last, could ship separately.
6. Playtest checklist and docs.
Every tunable goes on the components as new public fields.

**CONFIRMED 2026-10-18 (Mike: "Yes, use all five"). (1) Pond overflow only when the pond is at 1.0 AND the creek stage is High water or worse. (2) Weather panel `RowHeight` 52 -> 48 for the Creek row. (3) Crossing danger is its own last phase (phase 5) and may ship separately. (4) `SurvivalManager.Injure` has a health floor so a flood can never cause a collapse. (5) Pond Drying is playtested and committed BEFORE floods are built. Flood build message to be written after Pond Drying passes its 36-item checklist. Numbers stay first-pass (Claude, not yet confirmed).**


### PLAYTEST RESULT 2026-10-19 (Mike)

Pond Drying (Water Phase 2): Pond_Drying_Playtest_Checklist.md all 36 items PASS in Play Mode. Playtested; commit is Mike's call in Claude Code. Flash Floods are now unblocked once Pond Drying is committed (confirmed order, 2026-10-18).


## FLASH FLOODS PHASES 1-4 BUILT 2026-10-19 (Claude Code report, reconciled by Claude)

Phases 1-4 of the Flash Floods plan: StreamManager core, water quality, warnings, fishing and traps. NOT Play Mode tested. UNCOMMITTED (the last commit, 5f3762a, holds Rainwater, Placement Preview and Pond Drying). Phase 5 (creek crossing danger) is NOT built, so the creek is walkable at every stage. Numbers are first-pass (Claude, not yet confirmed).

**What I have from the report.** The pasted report showed the diff summary (13 files changed, +156 -23, as far as the screenshot showed) and the second half of Claude Code's summary (Phase 4, touched-outside-the-list, edit-mode results, not tested, notes). The top of the report (Phase 1-3 summaries, the tunable list with defaults, and the checklist path) was scrolled out of the paste, so it is NOT recorded here yet. Files visible in the diff list: `Debug/RainDebugControls.cs`, `Fishing/FishingRod.cs`, `Managers/FishingManager.cs`, `Managers/GameManager.cs`, `Managers/PondManager.cs`, `Managers/StreamManager.cs` (new) with its .meta, `Managers/TrapManager.cs`, plus 5 more not shown.

**Phase 4 (fishing and traps).**
- `FishingManager.BiteWait(canePole, pondName, creekWater)` multiplies the wait by `creekBiteWaitMultiplier` for creek water at High water or worse. Ponds unchanged.
- `FishingRod.UpdateIdle` refuses a creek cast in a flash flood with "The water is too high and fast to fish." `UpdateWaiting` reels in on a flood.
- `StreamManager.FlashFloodStarted` is a static event raised only on a rise into Flash flood. `TrapManager` rolls `trapSweepChance` for each Fish Trap where `FishWaterAt` is Creek (ponds are safe). The new `Sweep(TrapState)` is `PickUp` minus the inventory. Message: "A flood swept away a Fish Trap." (or a count), held for the wake summary if the player is asleep.
- Debug keys: Numpad 8 steps the stage through 0, 18 and 28 mm and back (28 = flash flood threshold 25 + a small margin), announcing rises and rolling traps as a real hour would. Numpad 9 and the Numpad 7 overlay show stage, state and murk hours.

**Touched outside the list.** `GameManager.ResetManagers` (one reset line). `PondManager` lost its own `HourEnded` subscription (it is now called from StreamManager, so event order never matters) and gained the overflow code. `PondSaveData` gained one new array (overflow). `RainDebugControls` keys 8 and 9.

**Edit-mode results.**
- Crossings: Heavy Rain reaches High water at hour 3 and flood at hour 5 (continuous 2.25 h and 4.41 h). A Thunderstorm reaches High water at hour 2 and flood at hour 3 (continuous 2.56 h to flood). Light Rain levels off at 9.0 mm.
- Decay: from 28.3 mm (what a Thunderstorm reaches after about 3 hours) the stage falls below 25 mm in about 44 minutes and below 15 mm in about **3.8 hours**. **The design said about 3 hours; that was my arithmetic slip** (the correct figure is 6 x ln(28.3 / 15) = about 3.8 h). Corrected here; the design draft line "below High water in about 3 hours" should read about 3.8 hours.
- Murk holds at 12 while in flood and reaches 0 about 12 hours after the flood ends.
- Rising edge: a flood event fires once per rise and never on a load.
- Overflow: a pond at full level during High water overflows; a pond at 0.9 does not. A full pond stays murky after the flood and clears when it drops.
- Saves: round trips for both stream and pond data. An old pond save has no overflow; the stream defaults to 0.

**NOT tested.** Everything in Play Mode: the real hourly events, sleep, collapse and load suppression; the prompt, banner and Weather row text, and whether 13 rows look right; cast refusal, mid-cast reel-in, and the sweep and its message; the "creek" zone boundaries on the real map (which water counts as creek, and whether the null-zone stretch is easy to find). If no WaterSource exists, `FishWaterAt` falls back to Creek, but that only happens outside the World scene.

**Notes.** Pond Drying's rain hook changed (now called from StreamManager), so it needs a regression pass; the checklist is said to include one. Several files in this build were part of commit 5f3762a, so the next diff is clean.


### PLAYTEST RESULT 2026-10-19 (Mike)

Flash Floods phases 1-4: Flood_Playtest_Checklist.md all 55 items PASS in Play Mode (including the Pond Drying, Rain Barrel and Placement Preview regression items 51-54). Playtested; commit is Mike's call in Claude Code. Phase 5 (creek crossing danger) remains the next flood item; the water surface still does not move.


## FLASH FLOODS PHASE 5: CREEK CROSSING DANGER (build spec, 2026-10-19)

Design decision 3 was CONFIRMED earlier (2026-10-18, "Yes, use all six"): creek crossing is slower and tiring at High water; in a flash flood the player is pushed downstream and can be knocked down; no item loss; a flood must never cause a collapse (Claude Code recommendation, confirmed). Claude Code's read-only findings (section "1. Creek crossing") supply the code plan. Mike chose to build it next ("lets go with Creek crossing danger"). Numbers below are first-pass (Claude, not yet confirmed) and every one is a public tunable.

**Real-world basis.** About 6 inches (0.15 m) of fast-moving water can knock an adult down (Army / NWS "Turn Around, Don't Drown"). In the game that is the ankle threshold (0.15 m), so a flash flood is dangerous at ANY depth in the channel, not only deep water.

**Rules.**
1. **Where it applies.** Only water that counts as creek (`WaterSource.ZoneAt == null`), the same definition as quality, fishing and traps. The 14 m spring zone and 32 m pond zone do not apply.
2. **High water.** While wading at ankle depth or deeper in creek water: movement speed x0.85 and extra stamina drain (about half the sprint drain rate). No push, no knock-down. A one-time flash line when first entering: "The creek is running high."
3. **Flash flood.** While wading at ankle depth or deeper in creek water: movement speed x0.6, strong extra stamina drain, and a steady push downstream (about 1.5 m/s along the flow direction). Staying in the water for about 1.5 s (tunable) knocks the player down: input locked for about 1.5 s, carried downstream during the lockout, stamina hit, a small injury, "The current knocks you off your feet!" Then a short immunity (about 4 s) so the player can scramble out instead of being chain-knocked. A one-time warning line when entering: "The water is too fast to cross."
4. **Injury.** New public `SurvivalManager.Injure(amount)` (about 5 health per knock-down). It has a health FLOOR (default 10, tunable): it never takes health below the floor, and does nothing if health is already below it. A flood can therefore never cause a collapse.
5. **No item loss, no drowning, no new save fields.** Lockout, immunity and warnings are transient. "Wet and cold" already comes from the existing wading cold model.
6. **Safe cases.** No push or knock-down while sleeping, collapsed, in a menu or screen, in Placement Mode (do not cancel it), or when the game state is not Playing. Do not push the player through terrain or out of the world (use `CharacterController.Move` with the push vector).
7. **Flow direction.** A small runtime table copied from the `CreekControl` control points in `PropertyTerrainBuilder` (spring, (-35,92), (-62,70), (-88,42), (-104,8), (-118,-30), (-136,-68), (-155,-102), (-172,-128), pond centre (-185,-150)); flow = direction of the nearest segment, downstream toward the pond. If the player's position is not within the channel width (0.9 m upstream to 1.8 m downstream, plus a margin), they are not "in the creek".
8. **Debug.** Numpad 8 already sets the stream stage; no new keys needed. A debug toggle to see "in creek / flow direction / state" in the Numpad 9 overlay is welcome.
9. **Not in this phase:** visible water rising, items swept away, drowning, boats, bridges, stepping stones, animals swept away, rescue mechanics.


---

## FLASH FLOODS PHASE 5 BUILT 2026-10-19 (Creek Crossing Danger)

**Status: BUILT, edit-mode tested only. UNCOMMITTED. Never played.** Claude Code reported compile clean and all 62 edit-mode tests passing (13 of them new). Nothing has been run in Play Mode yet.

### Files
- New: `Homestead.CreekCrossing.asmdef`, `CreekFlow.cs` (flow table built from the CreekControl control points, curved Catmull-Rom like the terrain builder, channel width widening near the pond), `CreekCrossingTimer.cs`, `Scripts/Managers/CreekCrossing.cs` (self-creating, holds the tunables), `CreekCrossingTests.cs`.
- Edited: `PlayerController.cs` (speed factor, stamina drain, push, input lockout in Move, `LoseStamina`, CanUseTools, Teleport, crouch and interact gating), `SurvivalManager.cs` (`Injure`, health floor), `RainDebugControls.cs`, `Homestead.Tests.EditMode.asmdef`, `GameManager.cs` (ResetManagers).
- Checklist: `06_AI_Collaboration/Creek_Crossing_Playtest_Checklist.md` (55 items).

### Tunables (all first-pass numbers, Claude, not yet confirmed)
| Tunable | Value |
|---|---|
| creekEffectMinDepthMetres | 0.15 |
| creekEffectChannelMarginMetres | 1 |
| highWaterWadeSpeedFactor | 0.85 |
| highWaterWadeDrainShareOfSprint | 0.5 (sprint drain is 15/s) |
| floodWadeSpeedFactor | 0.6 |
| floodWadeDrainShareOfSprint | 1.2 |
| floodPushMetresPerSecond | 1.5 |
| floodKnockDownAfterSeconds | 1.5 |
| floodLockoutSeconds | 1.5 |
| floodLockoutCarryMetresPerSecond | 2.5 |
| floodImmunitySeconds | 4 |
| floodKnockDownStaminaLoss | 25 |
| floodKnockDownInjury | 5 |
| creekLeaveGraceSeconds | 1 |
| creekMessageSeconds | 3 |
| SurvivalManager.injuryHealthFloor | 10 (minimum 1) |

### How it works
- Push is added to the `controller.Move` vector, so colliders (trees, banks) stop it.
- Lockout blocks movement, jump, sprint, crouch, interaction and tools. Looking still works.
- `SurvivalManager.Injure` never takes health below the floor, so a flood never causes a collapse.
- Sleeping, collapsed, menu, Placement Mode and non-Playing states reset all clocks.
- Reset points: `PlayerController.Teleport` and `GameManager.ResetManagers`.

### Claude Code's choices to check
- Curved flow path (Catmull-Rom) rather than straight segments.
- "In creek" depth test uses `Animal.IsOverWater`.
- Placement Mode resets the clocks, so placing makes a flood crossing unhindered.
- Immunity (4 s after a knock-down) still applies push and slowdown; only the knock-down is suppressed.
- Lockout scope (everything except looking).
- Numpad 9 overlay shows `in creek: yes/no, flow <deg>, speed x<factor>`.

### Not tested yet
Feel and fairness of the 1.5 s knock-down; whether channel width plus margin matches the real water edge; toast text; live sleep, collapse, save/load and placement behaviour; pinning against trees or banks; and whether the carved banks are climbable under flood slowdown. That last one is the one remaining "feel stuck" risk Claude Code could not rule out.

### Next
Mike runs `Creek_Crossing_Playtest_Checklist.md` in Play Mode (back up `slot1` first, restart Play Mode, clear the Console), then reports pass/fail.


**PLAYTEST RESULT (Flash Floods phase 5, creek crossing danger, 2026-10-19): 55/55 PASS** (Mike, Play Mode). First-pass numbers confirmed by play: 1.5 s knock-down timer feels right, 4 s immunity lets you escape, push is felt but never a trap, and the channel edge matches the visible water. Two open comments: (1) Mike disagrees with the 5 Health lost per knock-down (item 18); the stamina loss (25) was not objected to. (2) Item 52, his note on the "while you slept, the creek flooded" wake-up message was cut off, so what he wants changed is not yet known. Flood phases 1-5 are all still UNCOMMITTED.

**DECISION (2026-10-19, Mike):** flood knock-down Health loss gets SMALLER (not removed, not kept at 5). Mike did not give a number, so the working value is 2 (Claude, not yet confirmed); he can change it. Stamina loss stays 25, lockout stays 1.5 s, Health floor stays 10. Field to be renamed (floodKnockDownInjury to floodKnockDownHealthLoss) because the old one is already serialized. Checklist items 18 and 25-28 need their numbers updated. Not built yet.


**BUILT 2026-10-19: flood knock-down Health loss reduced to 2.** Field `floodKnockDownInjury` (default 5) renamed to `floodKnockDownHealthLoss` (default 2, Claude, not yet confirmed; Mike gave no number). Renamed rather than re-defaulted because the old field was already serialized. Only reader was the `Injure(...)` call in `CreekCrossing.cs`. Unchanged: stamina loss 25, lockout 1.5 s, immunity 4 s, `SurvivalManager.injuryHealthFloor` 10. Compile clean, 62 of 62 edit-mode tests pass (same count: three assertions added inside `HealthAfterInjuryNeverGoesBelowTheFloor`: 100 to 98, 11 to 10 clamp, 12 to 10). Files touched: `CreekCrossing.cs`, `CreekCrossingTests.cs`, `Creek_Crossing_Playtest_Checklist.md`. Checklist items 18, 25 (now starts from Health about 20), 28 (now uses Health 11) updated; item 27 unchanged. **Items 18, 25, 27, 28 need a Play Mode re-check.** UNCOMMITTED. Item 52 (wake-up message comment) still open.


**COMMITTED AND PUSHED 2026-10-19: Flash Floods phases 1-5 in commit c3d3e0f** ("Add flash floods: rising creek, pond overflow, creek crossing danger", 28 files, +1736 -30). Playtested in Play Mode before commit: flood phases 1-4 55/55, creek crossing 55/55 (including the 2-Health knock-down re-check). 62 edit-mode tests pass. Two flood edits not named in the plan went in because they are flood work: `Survival/WaterSource.cs` (creek quality hook) and `UI/WeatherScreen.cs` (Creek row, row height 48). Earlier commit 5f3762a was reworded (message only, tree identical) and is now 6fd820a ("Rain Barrel, Placement Preview and Pond Drying"). origin/master and local master are both at c3d3e0f. Still uncommitted by Mike's choice: AudioManager.cs null guard, ProjectSettings.asset (runInBackground), Homestead.slnx, 08_Backups/.
**OPEN:** Mike thinks the "while you slept, the creek flooded" wake-up message is not needed. Not removed; waiting for his firm yes.
