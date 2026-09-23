# Homestead
## Livestock: Rabbits v1.0

Status: Design Draft

---

# Purpose

Concrete, tunable stats for Rabbits, the fast-breeding, small-footprint livestock option per Livestock_System.md. This document does not redefine the general Livestock mechanic — that's Game_Systems/Livestock_System.md's job. This is the per-species numbers that mechanic needs to actually run.

Grounded loosely in real rabbit husbandry (see Research.md) but scaled for pacing.

Design direction confirmed 2026-09-22 (Mike): rabbits should reproduce faster than the other Alpha 0.1 livestock — each animal type should have a clear advantage/drawback identity rather than overlapping. Rabbits are the fast-population-growth option, trading off against Goats (slow, high-value milk) and Chickens (mid-speed, unconditional eggs).

---

# Core Stats

- **Feed consumption:** 1 unit/day (hay, greens) — tied with Chickens for lowest of the three, per Livestock_System.md's "efficient feed conversion" advantage
- **Water consumption:** 1 unit/day, minimal seasonal increase (smallest animal of the three, least affected by heat)
- **Shelter requirement:** Hutches (per Livestock_System.md)
- **Maturation time:** 15 in-game days from kit to breeding-eligible adult — the fastest of the three (vs. Chickens' 20, Goats' 30)
- **Breeding cooldown:** Not season-locked, unlike Goats. A doe can be rebred as soon as her current litter is weaned, for a full kit-to-next-litter cycle of roughly 20 days — the fastest full reproductive cycle of the three Alpha 0.1 animals by design.
- **Litter size:** 4 kits per litter — the largest single-cycle output of the three (vs. Goats' 1, Chickens' 3 per hatch), balanced against rabbits having the lowest per-animal value (no milk, no daily egg-equivalent)
- **Output:** Meat + **Small Furs** per culled/mature rabbit. Confirmed 2026-09-22 (Mike): livestock rabbit fur reuses the same "Small Furs" resource that Trapping_System.md defines for trapped wild rabbits/squirrels, rather than a separate resource — keeps the economy simpler.
- **Population growth:** No hard cap (e.g., no hutch-capacity limit). Confirmed 2026-09-22 (Mike): left as an open-ended management problem for the player, per Livestock_System.md's existing Design Rule that "uncontrolled breeding can create feed shortages" — that pressure is the intended check on rabbit population, not a built-in cap.
- **Neglect threshold:** 2+ consecutive days without water, or 3+ without feed, before health/production penalties begin

This makes rabbits the highest population-growth-rate animal by a wide margin — a well-tended, unmanaged rabbit hutch can plausibly out-populate goats or chickens within a single season and outstrip the player's feed reserves if left unchecked, which is the intended tradeoff for having no ongoing daily resource (no milk, no eggs) between litters.

---

# Economy Reference

Per Livestock_System.md's Economy Integration loop (Breeding Stock → Market Sales) — rabbits lean more heavily on this loop than Goats or Chickens do, since they don't have Goats' milk or Chickens' eggs as an ongoing income floor. Their value proposition is volume: meat, Small Furs, and surplus breeding stock in larger quantities, more often.

- Meat: per-animal value similar to a culled chicken, but available in greater volume due to faster breeding
- Small Furs: shares the same resource and market as trapped wild rabbit/squirrel fur (see Trapping_System.md)
- Breeding stock: highest-volume sellable output of the three species, per the differentiation above

No specific currency numbers proposed here — that's Economy_System.md's territory.

---

# Design Rules

1. Values on this page are confirmed baselines, not immutable — normal playtesting-driven tuning still applies.

2. Real-world rabbit husbandry (Research.md) informs direction and relative scale, not literal numbers.

3. Rabbits' defining trait is reproduction speed and volume, not per-animal value — future additions should preserve that identity rather than making rabbits competitive with Goats' milk or Chickens' eggs on ongoing daily output.

4. Rabbit population is intentionally uncapped. Feed-shortage pressure from Livestock_System.md's existing design is the mechanism that keeps it in check — any future change here should work through that pressure, not add a separate hard limit.
