# Homestead
## Season System v1.0

Status: Design Draft

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

# Future System

Homestead Region Selection

Not Alpha 0.1. The full game may let players choose a real-world region or latitude for their homestead, shaping day-length swing and climate character. Alpha 0.1 uses one fixed, unspecified temperate location instead. Source: DayNightCycle.cs's open latitude question, confirmed with Mike 2026-09-23 — see 06_AI_Collaboration/Current_Task_List.md.

---

# Design Rule

The player should naturally change priorities as seasons change.

Successful players plan months ahead.

Unprepared players struggle.
