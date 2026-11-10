# Homestead
## Season System v1.0

Status: Design Draft

UPDATE 2026-11-08: the season length in this doc (28-day seasons, 112-day year, 30-minute day) is being replaced. Mike decided on a real 365-day calendar (12 real-length months, no leap years; Spring March to May, Summer June to August, Fall September to November, Winter December to February). Not built yet. See Calendar_and_Time_Scale_Options.md.

UPDATE 2026-11-10: the real 365-day calendar is BUILT (Phase 1) and playtested 16 of 16. Spring is 92 days (Mar to May), Summer 92 (Jun to Aug), Fall 91 (Sep to Nov), Winter 90 (Dec to Feb); the game year runs March to February; the day is still 30 real minutes. Everywhere this doc says 28 days a season or a 112-day year, read the real lengths above. Per-season effects (weather weights, Maple colour, forage, heat waves) are NOT retuned: Summer is now 92 days, about 1.4 heat waves per Summer at the current weights. See Calendar_and_Time_Scale_Options.md.

---

# Purpose

The Season System is one of the core simulation systems in Homestead.

It drives:

- Wildlife behavior
- Foraging opportunities
- Fishing patterns
- Hunting opportunities
- Water availability
- Food preservation
- Livestock management
- Weather conditions
- Workload planning

Every year is divided into four seasons.

Each season requires different priorities.

---

# Pacing

Confirmed 2026-09-23 (Mike, reviewing Claude Code's TimeManager implementation).

A full in-game day takes 30 real minutes. Each season lasts 28 in-game days, a 112-day year. A new game begins at 6:00 AM on Spring, day 1.

**Resolved 2026-10-07 (Mike): "it could have been spring, just the snow flurries made me think it was winter."** So this is most likely not a mismatch: a Spring day-1 start with flurries and Pioneer's −3°C reads as winter. **Confirmed from the code 2026-10-07 (Claude Code):** `GameManager` calls `TimeManager.ResetClock()` on a new game, which sets day 0, Spring day 1 at 06:00. That is from reading the code, not from a run, but it supports Mike's flurries explanation. Original note, kept for the record:

**Possible mismatch found 2026-10-07 (Mike, Pioneer playtest):** Mike reported that his new game "started in the wintertime when temperatures were lower." This section says a new game starts on Spring day 1, so either the code starts somewhere else (or doesn't use this start date for a new game), or early Spring on Pioneer (−3°C, Difficulty_System.md) simply read as winter. The docs can't tell which. **Needs Claude Code to check:** what season and day `TimeManager` actually starts a new game on, and what the HUD showed. If the code and this section disagree, one of them is stale and Mike should say which is right. Whether a new game should always start in Spring is Mike's call, since a Winter start combined with Pioneer is the hardest possible opening.

Daytime is meant to feel generous rather than strictly realistic — roughly twice as long as night in every season, and never flipping to night-heavy the way a real mid-latitude Winter would. Confirmed day/night split: Summer 17h day / 7h night, Spring and Fall 16h/8h, Winter 14h/10h. Dawn and Dusk each span 2 in-game hours bridging sunrise and sunset into full Day or Night.

These values live in TimeManager.cs as Inspector-tunable defaults; this section records what's currently confirmed, not a permanent lock. See 06_AI_Collaboration/Current_Task_List.md for the review history.

---

# Spring

Primary Focus:

- Exploration
- Water discovery
- Shelter improvement
- Resource gathering

Opportunities:

- New plant growth
- Mushroom foraging
- Wildlife activity increases
- Water availability improves

Challenges:

- Rain
- Mud
- Unpredictable weather

---

# Summer

Primary Focus:

- Food gathering
- Fishing
- Land clearing
- Property development

Opportunities:

- Berry harvests
- Peak fishing
- Long daylight hours

Challenges:

- Heat
- Dehydration
- Drought risk

---

# Fall

Primary Focus:

- Hunting
- Harvesting
- Food preservation
- Firewood production

Opportunities:

- Nut harvest
- Fruit harvest
- Hunting season
- Harvest season

Challenges:

- Shorter days
- Increased workload

Fall is preparation season.

Most winter success is determined during fall.

---

# Winter

Primary Focus:

- Survival

Opportunities:

- Trapping
- Planning
- Tool maintenance

Challenges:

- Frozen water
- Reduced forage
- Increased food consumption
- Increased firewood consumption

Winter is the annual survival test.

---

# Water Availability

## Spring

Highest availability.

---

## Summer

Variable availability.

Droughts may occur.

---

## Fall

Generally stable.

---

## Winter

Surface water may freeze.

Springs remain valuable.

Wells remain valuable.

---

# Wildlife Effects

Animal behavior changes by season.

Examples:

- Deer movement patterns shift.
- Fish activity changes.
- Rabbits have seasonal population changes.
- Waterfowl migrate.

Detailed behavior is defined in Wildlife System documentation.

---

# Foraging Effects

Season determines:

- Availability
- Regrowth
- Harvest windows

Examples:

Spring:
- Mushrooms
- Greens

Summer:
- Berries

Fall:
- Nuts
- Fruit

Winter:
- Minimal foraging

---

# Visual Feedback

Confirmed 2026-09-24 (Mike, from playtest): the property's hardwood forest should visibly change through Fall rather than staying uniformly green year-round — leaf color shifting and leaves falling. This is presentation of the season already defined above, not a new gameplay effect (same framing as Weather_System.md's own Visual Feedback section) — none of Fall's documented Primary Focus, Opportunities, or Challenges change based on how the trees look.

- Canopies shift from green toward Fall color (yellow/orange/red) over the course of the season, progressing across Fall's 28 days rather than snapping instantly on the season boundary.
- Leaves fall over the back half of Fall, thinning the canopy and leaving visible litter on the ground underneath. Existing wind (direction and strength, already tracked by WeatherManager per Weather_System.md) is a natural fit for drift on the way down, but that's an implementation detail, not a requirement.
- By Winter, canopies read bare — consistent with Winter's already-stark character (reduced forage, Design Rule 4's "most demanding season") and with the tree-top snow accumulation already specced in Weather_System.md, which will read more naturally against bare branches than a full green canopy.
- Canopies return to green over Spring, alongside Spring's already-documented "New plant growth" opportunity — the natural complement to Fall's color change and drop, not a separate ask.

Applies to the mixed hardwood forest's canopy trees (the two hardwood prototypes `PropertyTerrainBuilder.cs` already places); whether the understory shrub also changes is a minor call left to Claude Code. Technique (seasonal material/texture variants, a shader blend driven by season progress, a leaf-litter decal or mesh pass, etc.), the exact color progression, and how much of the canopy thins per day are all Claude Code's call.

---

# Future System

Homestead Region Selection

Not Alpha 0.1. The full game may let players choose a real-world region or latitude for their homestead, shaping day-length swing and climate character. Alpha 0.1 uses one fixed, unspecified temperate location instead. Source: DayNightCycle.cs's open latitude question, confirmed with Mike 2026-09-23 — see 06_AI_Collaboration/Current_Task_List.md.

Maple Sugaring

Not Alpha 0.1 (per Core_Survival_System.md's Future Expansion list). Sap season sits right at the Winter/Spring boundary — the same freeze-thaw temperature signal Weather_System.md's precipitation rules use. Full spec is drafted and ready in `Game_Systems/Maple_Sugaring.md`, confirmed 2026-09-24.

---

# Design Rule

The player should naturally change priorities as seasons change.

Successful players plan months ahead.

Unprepared players struggle.
