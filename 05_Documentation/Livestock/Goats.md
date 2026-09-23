# Homestead
## Livestock: Goats v1.0

Status: Design Draft

---

# Purpose

Concrete, tunable stats for the Goat, the earliest and most-emphasized livestock option per Livestock_System.md's Design Rule #6. This document does not redefine the general Livestock mechanic — that's Game_Systems/Livestock_System.md's job. This is the per-species numbers that mechanic needs to actually run.

Grounded loosely in real goat care (see Research.md) but scaled for pacing, not literal simulation.

---

# Core Stats

- **Feed consumption:** 2 units/day (hay or browse) — drops to 1 unit/day if pasture "Brush Control" access is available (per Livestock_System.md's brush-clearing benefit)
- **Water consumption:** 1 unit/day, 2 units/day in Summer (per Livestock_System.md's seasonal water note)
- **Shelter requirement:** Three-sided shelter or small barn (per Livestock_System.md) — no shelter causes health decline over time, not instant death
- **Milk yield:** 1 unit/day once mature and bred at least once. Missed milking for 3+ consecutive days reduces yield temporarily rather than spoiling or wasting it outright — confirmed 2026-09-22 (Mike): the softer penalty stands, no harsher wastage mechanic.
- **Maturation time:** 30 in-game days from kid to milk-producing adult — confirmed 2026-09-22 (Mike): acceptable pacing, may be revisited later if needed once playtested.
- **Breeding cooldown:** Spring-only breeding window (per Livestock_System.md's Seasonal Effects), one kid per breeding cycle — confirmed 2026-09-22 (Mike). Twins are a possible Stage 2+ addition, not in scope for Alpha 0.1.
- **Neglect threshold:** 2+ consecutive days without water, or 3+ without feed, before health/production penalties begin

Of the three Alpha 0.1 livestock, goats are the slowest breeder (once per year, Spring-only, single kid) — the tradeoff for their high-value daily milk output and land-clearing utility. See Chickens.md and Rabbits.md for how the other two differentiate.

---

# Economy Reference

Per Livestock_System.md's Economy Integration loop (Goat Milk → Cheese → Sale):

- Raw milk: low value, mainly for household Hunger/Nutrition use
- Cheese (once a preservation/crafting step exists): higher value, sellable surplus
- Breeding stock: sellable directly, per the existing Economy Integration diagram

No specific currency numbers proposed here — that's Economy_System.md's territory, not this sheet's.

---

# Design Rules

1. Values on this page are confirmed baselines, not immutable — normal playtesting-driven tuning still applies.

2. Real-world goat care (Research.md) informs direction and relative scale, not literal numbers — this is a game, not a husbandry sim.

3. Goats are the deliberately slow-breeding, high-value member of the Alpha 0.1 livestock set. Any future change that speeds up goat breeding should be weighed against Rabbits.md and Chickens.md's differentiation, not made in isolation.
