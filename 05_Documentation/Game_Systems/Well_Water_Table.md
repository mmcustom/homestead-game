# Homestead
## Well Water Table and Placement Preview v0.3

Status: Design Draft (PROPOSED), written 2026-10-24, corrected the same day after Claude Code's read-only pass (see "Corrections from the read-only pass" below). **v0.3 (2026-10-24): Mike reversed the Bucket-only rule (a well has its own bucket attached; see Part A) and chose depth slope 0.5.** **Built and playtested 2026-10-24: B1 (WellWater helper, commit 2d1e866), B2 (placement preview and flood warning, 9842600, with the ghost-lifetime fix) and B3 (dig blows, Stone lining, buffer, refill and drought by depth, saved as `depthToWater`, 64ffc69). Not built: the shallow-well setback factor (about 1.5x, estimated about 60 lines and 8 tests, safe to add later), the water-table overlay (B4), drilled wells, mineral effects.** Builds on Structures_Slice1_Dry_And_Clean.md (Well, Phase A built) and Water_System.md.

# What Mike asked for (2026-10-24, playing Phase A)

1. **Bug or oversight:** pressing E at the well with no Bucket equipped makes the player drink straight from it. **Decision (first, superseded): Bucket only.** **Decision (final, Mike, 2026-10-24): this is not a bug.** A well has its own bucket attached (rope and windlass), so no Bucket is needed at the well. You can drink straight from it, fill an equipped Bucket, or fill an equipped Canteen. That is what the code already does, so rule 6 of the Well in Slice 1 stands unchanged.
2. **Question:** can the player see the water quality a well will give when deciding where to build it? **Decision: preview plus ground that varies.** The ghost shows the expected quality, and the land itself changes the well (how deep the water is, how long the dig takes, how much it yields, how exposed it is).

# Part A: the Bucket rule (DROPPED 2026-10-24)

Mike's final ruling: the well has a bucket attached, so the current behaviour is correct and **no code change is needed.**

- E at a Well with a Bucket or Canteen equipped fills it from the pack's water (as today).
- **Cooking pot (Mike, 2026-10-24, final): NOT filled at the well or any other source.** The pot is filled by pouring water from the Bucket into it, to be built when cooking recipes are worked on. No change now; Claude Code found the pot never holds raw water today, which stays as is until then.
- E with anything else equipped, or nothing, drinks 1 L straight from the well, with the existing quality and illness roll (as today).
- Possible tiny polish, not decided: the prompt `WellPrompt` always says "Draw water"; it could say what E will do ("Fill Bucket", "Fill Canteen", "Drink"). Cosmetic only.
- Cosmetic idea, not decided: show a bucket and rope on the well model so the rule reads visually.
- Creeks, ponds and the spring keep drinking by hand; barrels and the Kitchen Sink never drink.
- No save change.

# Part B: what the research says

- **The water table follows the land.** Groundwater recharges in uplands and discharges toward lower ground such as streams, lakes and wetlands, so water is closer to the surface in valleys and near water and deeper under hills. One extension example shows 20 ft to water at one site and 50 ft at another.
- **A well has to reach the water table.** A 25 ft well was dry where the table was at 50 ft. A well that only just reaches the water table can go dry in a drought or a dry season; a deeper one is less likely to.
- **Shallow wells feel the neighbourhood.** They draw mostly from nearby land and respond quickly to local activity, so they need the full distance from privies, pens and septic. Deeper wells draw from farther uphill and dilute spills.
- **Deeper is not simply better.** Deeper water tends to be harder (more calcium and magnesium) and to carry more iron and manganese because there is less oxygen. Shallow groundwater can be more corrosive.
- **Ohio well siting rules (OAC 3701-28-07):** a well must be uphill of known contamination where possible and as far from it as practical. Minimum distances include 100 ft from a leaching privy, 50 ft from a septic system, a vault privy or animal pens and manure piles, and 25 ft from a stream, lake or pond. New wells may not go in a floodplain or floodway. (The game's 30 m is about 100 ft, matching the leaching privy rule.)
- **Southwest Ohio geology (Ohio EPA fact sheet):** shale-rich carbonate bedrock generally yields under 5 gpm, and carbonate water is high in calcium, magnesium and sulfate. So a typical home well here is a modest, hard-water well, not a gusher. (Bethel is in this region; the exact local geology was not checked.)
- **What was not found:** no usable numbers for depth to water by landscape position in Ohio, so the depth ranges below are game numbers shaped by the principle, not measurements.

# Part B: the proposal

## One new number: depth to water

For any spot on the property the game computes a **depth to water** (metres to the water table):

- Start from the height of the nearest permanent water (creek, pond or the Spring Hollow) and how far the spot is above it. Low ground near water is shallow; high ground far from water is deep.
- First pass: depth = 1.5 m at the water's edge, plus a slope in metres of depth per metre the ground sits above the nearest permanent water, capped at 9 m (about 30 ft). **The slope is now an open decision:** 0.8 as first written, or 0.5 to 0.6 (see "Corrections" for the measured effect on the real terrain).
- **Too deep to hand-dig:** past the cap the site is refused with "The water is too deep to dig by hand here (a drilled well needs a rig)". That is a natural hook for the drilled well and Well Drilling money sink in Economy_System.md.
- The value is computed when the Well Site is placed and saved with it (one appended float, `depthToWater`; 0 means an old well), so later terrain changes never move an existing well. Old saves keep the old fixed behaviour (150 L buffer, 6 L per hour, 40 blows).

## What depth changes (all first pass)

| Depth to water | Dig effort | Stone lining | Yield (buffer and recovery) | Exposure and quality |
|---|---|---|---|---|
| Shallow, under 3 m (low ground near water) | Quick, about 20 blows | Light, about 6 Stone | Best, about 200 L and 8 L per hour, but the table can drop in a drought | Most exposed: needs the full setbacks and a bigger setback (about 1.5 times) from pens and privies; higher flood risk |
| Medium, 3 to 6 m | The current 40 blows, 10 Stone | The current amount | The current 150 L and 6 L per hour | As built today |
| Deep, 6 to 9 m (high ground) | **Decided 2026-10-24 (Mike, option 2): 60 blows (about 54 s of real digging)**; first draft was 70 | **14 Stone (70 kg, 3 to 4 trips)**; first draft was 16 (80 kg) | **120 L and 4.5 L per hour, drought 4.5 L per hour** (keeps working in a drought, better than today's 3); first draft was 100 L and 3.5 | Safest from surface contamination; harder and iron-tasting water as a label |

The trade-off is the real one: low ground is easy and generous but exposed; high ground is hard work and stingy but safe and drought-resistant. Mineral character (Hard, Iron taste) is a descriptive tag in version 1 and has no health effect; it can feed boiling and later water treatment.

## The placement preview

While the Well Site ghost is in Placement Mode, an info line or small panel shows, for the spot under the ghost:

- **Expected quality:** Good, plus the reason or the nearest hazards. A new well can never be "Questionable" today, because `CanPlaceWell` already refuses those spots; only old saves can show it. So the useful preview is depth, supply, exposure and the flood warning, not a quality grade.
- **Depth to water** and what it means in play ("about 4 m: 40 blows, 10 Stone").
- **Expected supply:** Poor, Fair or Good, with the buffer and recovery numbers.
- **Risks:** the nearest hazards with distances (outhouse, pens, creek or pond, flood ground), and whether the spot is likely to flood.
- **A true flood warning.** Flash floods contaminate a well within 40 m of the channel edge, but placement refuses only within 25 m. A spot at 30 m is legal and still floods; the preview should say "Floods can foul this well" there. This is the one real gap the preview closes.
- **A one-line hint** when the player might want to move: "Higher ground is safer but deeper."
- The ghost colour follows the result (green, amber, red). Hard refusals (broken setbacks, "This ground floods", too deep) stay red and keep their reasons, as in Phase A.
- **Exact, not guessed:** the preview shows the true values for that spot, so the decision is fair. An optional later version could show a range until the player digs a test hole.

Optional second stage: a faint water-table colour overlay on the ground near the player while placing (blue for shallow, brown for deep), so the player can scan the land instead of probing one spot at a time.

## Regions hook (parked plan)

Regions_System.md already plans regions with their own terrain and resources. A region should set the groundwater profile: base depth, yield multiplier and mineral profile (Southwest Ohio: low yield, hard and sulfate-rich; a river valley region: shallow, generous, flood-prone). Version 1 uses one profile (this property) but should keep the profile in one place so a region can replace it.

# Build plan (proposed, in order; each step can stop)

1. **Claude Code read-only pass: DONE 2026-10-24.** Findings are in "Corrections from the read-only pass" below.
2. **A: Bucket rule. DROPPED** (Mike: the well has its own bucket). Build starts at B1.
3. **B1: `WellWater` helper.** A pure static class in the `Homestead.Setbacks` assembly (no references there, so it is edit-mode testable). Inputs: ground height at the spot, height of the nearest permanent water, per-well setback factor. Returns depth, yield tier, exposure tier, dig blows, lining Stone, buffer, recovery. Tests for the ladder, the cap, "too deep" and old-save defaults (0 means old numbers).
4. **B2: ghost preview.** Needs public wrappers for the private Floods, InHollow and Sites, a third (amber) material on `PlacementGhost`, and a multi-line `ToolStatus.Report` label. Shows depth, supply, risks and the true flood-reach warning (40 m flood reach against the 25 m refusal). Reasons for hard refusals still come from `Setbacks.CanPlaceWell`.
5. **B3: effects, in this order:** per-well dig blows first (easy), then buffer and recovery (moderate), and Stone lining last (hardest: FinishedCost, NeedOf, SiteNeeds, SiteAccepts, StoreMaterials and the Dismantle refund all key on kind, and 16 Stone is 80 kg with no carry check). Per-well setback factor needs `SiteInfo`. Save: append one float `depthToWater` to `WoodPileState`; 0 means an old well with the old fixed numbers. Test: an old pile save loads with the new field at 0.
6. **B4 (optional): overlay** of the water table near the player in Placement Mode.
7. Playtest checklist after A, then after B2, then after B3.

# Decided tier numbers (Mike, 2026-10-24: "i'll take your recommendation, 2")

Slope 0.5 puts about 81% of the land near the cabin in the Deep tier, so Deep was softened slightly from the first draft. Final first-pass numbers (still tunable in `WellWaterRules.Defaults`):

| Tier | Blows | Stone | Buffer | Recovery | Drought recovery |
|---|---|---|---|---|---|
| Shallow (under 3 m) | 20 | 6 | 200 L | 8 L/h | 4 L/h |
| Medium (3 to 6 m) | 40 | 10 | 150 L | 6 L/h | 3 L/h |
| Deep (6 to 9 m) | 60 | 14 | 120 L | 4.5 L/h | 4.5 L/h |
| Legacy (old wells, depth 0) | 40 | 10 | 150 L | 6 L/h | 3 L/h |

Not checked: how much water a player or livestock use per day, which decides whether 4.5 L/h is enough; B3 includes a report-only check.

# Open questions for Mike

1. **Depth range and trade-off:** is the low-ground-easy-but-exposed versus high-ground-hard-but-safe trade-off what you want? Numbers are first pass.
2. **Depth slope: DECIDED 0.5** (Mike, 2026-10-24). Nothing near the cabin is refused. Side effect to check: with 0.5 about 81% of the land within 60 m of the cabin is in the deep tier (6 to 9 m), so most players near the cabin would get the stingy deep-tier well (about 100 L, 3 to 4 L per hour) instead of today's 150 L and 6 L per hour. B1 should report the real tier split and the tier numbers may need recentring.
3. **Canteen at the well: DECIDED** (Mike): no Bucket is needed at a well; you can drink, fill an equipped Bucket, or fill an equipped Canteen. The cooking pot is not filled at sources; it is filled by pouring from the Bucket when cooking recipes are built. The Bucket rule is dropped.
4. **Too-deep sites:** refuse hand-digging past the cap and point to a drilled well later (recommended), or let any spot be dug with a much longer dig?
5. **Minerals in version 1:** descriptive tags only (Hard, Iron taste) with no health effect (recommended), or a small real effect such as taste lowering water enjoyment?
6. **Overlay:** build the water-table overlay (B4) in the first pass, or keep the exact numbers on the ghost only?
7. **Old wells:** keep wells already built in saves exactly as they are (recommended; one appended field `depthToWater`, 0 means old numbers), or recompute them from where they stand?

# Corrections from the read-only pass (2026-10-24)

What Claude Code's read of the code changed in this draft:

1. **Digging is real time, not game hours.** The draft said a dig takes "about 5 hours". The clock skips no longer exist; each blow is about 0.9 s, so 40 blows is about 36 s and the deep tier (70 blows) is about a minute. Effort is shown in blows and seconds.
2. **"Questionable" quality cannot happen for a new well.** `CanPlaceWell` already refuses those spots. The preview therefore shows depth, supply, exposure and the flood warning instead of a quality grade.
3. **The 0.8 slope is too harsh on this terrain.** Height above the nearest permanent water across the property: min 0.5 m, p10 3.0, median 5.2, p90 8.8, max 14.5. The cabin site is 14.2 m above the creek and 141 m from it.

   | Slope (m of depth per m above water) | Whole property: under 3 m / 3 to 6 m / 6 to 9 m / refused | Within 60 m of the cabin: under 3 m / 3 to 6 m / 6 to 9 m / refused |
   |---|---|---|
   | 0.8 (first draft) | 1% / 57% / 35% / 7% | 77% refused (rest of the split not reported) |
   | 0.6 | not measured | 0% / 9% / 58% / 33% |
   | 0.5 | not measured | 0% / 19% / 81% / 0% |

   The shallow tier (under 3 m) is a sliver at any slope, so it is mostly a rare lowland bonus, not a common choice.
4. **Lining of 16 Stone is 80 kg** and nothing checks the trip weight today, so the deep tier's lining is the riskiest change; it goes last.
5. **A per-well setback factor** (the 1.5 times for shallow wells) needs `SiteInfo` to carry it, which is more plumbing than the other effects.
6. **Flood reach is wider than the refusal** (40 m against 25 m), so the preview has a real warning to give.
7. **Unverified by Claude Code:** the water-layer surface heights (about 1 m either way), the size of the HUD label, `IsOverWater` at the pond and spring centres, and Stone supply and trip weights. B1 and B2 should check these.
8. **No persistent water table exists.** Drought is `WeatherManager.IsDrought` (Summer only, 5 or more dry days). Per-depth drought behaviour is a rule applied on top, not a simulation.

# Cross-references

- Structures_Slice1_Dry_And_Clean.md: Well rules 1 to 10 and the Phase A build (setbacks, staged dig, 150 L buffer, flood contamination).
- Water_System.md and Water_Sustainability_Research.md: sources, quality tiers, drought and pond drying.
- Economy_System.md: Well Drilling as a money sink.
- Regions_System.md: groundwater profile per region.
- Wood_Gathering_System.md and Crafting_System.md: the Shovel and the Bucket.

# Sources (research 2026-10-24)

- University of Wisconsin Extension, do deeper wells mean better water (depth, contamination, hard water, iron, recharge uplands and discharge lowlands): https://burnett.extension.wisc.edu/files/2010/09/DodeeperwellsmeanbetterWater.pdf
- Ohio Administrative Code 3701-28-07, private water system location and isolation distances: https://codes.ohio.gov/ohio-administrative-code/rule-3701-28-07
- Ohio EPA, major aquifers and groundwater quality fact sheet (carbonate yields and chemistry, iron and manganese in low-oxygen water): https://dam.assets.ohio.gov/image/upload/epa.ohio.gov/Portals/28/documents/gwqcp/FS_Final_Major%20Aquifers%20and%20GWQ_11-2015.pdf
- EPA dug-well description is already cited in Structures_Slice1_Dry_And_Clean.md.
