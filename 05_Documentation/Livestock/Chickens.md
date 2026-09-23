# Homestead
## Livestock: Chickens v1.0

Status: Design Draft

---

# Purpose

Concrete, tunable stats for Chickens, the low-cost/low-space livestock option per Livestock_System.md. This document does not redefine the general Livestock mechanic — that's Game_Systems/Livestock_System.md's job. This is the per-species numbers that mechanic needs to actually run.

Grounded loosely in real backyard-chicken care (see Research.md) but scaled for pacing.

---

# Core Stats

- **Feed consumption:** 1 unit/day (grain, insects, kitchen scraps) — the lowest of the three Alpha 0.1 livestock, per Livestock_System.md's "Low cost" advantage
- **Water consumption:** 1 unit/day, 2 units/day in Summer
- **Shelter requirement:** Coop (per Livestock_System.md)
- **Egg yield:** 1 egg/day per hen once mature — **halved to roughly 1 egg every 2 days in Winter**, confirmed 2026-09-22 (Mike), matching real hens laying less in cold/low-light months. Not gated behind breeding — a hen lays whether or not she's ever bred, matching how real chickens work. Breeding is a separate track used only to grow the flock (see below).
- **Maturation time:** 20 in-game days from chick to egg-laying hen — faster than goats' 30 days, giving chickens the mid-tier maturation speed of the three
- **Breeding/hatching:** Not season-locked, unlike goats — a hen can be set to hatch a clutch at any time. Clutch size: 3 chicks per hatch. Cooldown: 14 days between hatches per hen.
- **Meat:** Adult birds can be culled directly for meat at any time, trading future egg output for an immediate resource. Confirmed 2026-09-22 (Mike): culling carries no downside beyond the lost future egg output — no separate Morale or other penalty.
- **Neglect threshold:** 2+ consecutive days without water, or 3+ without feed, before health/production penalties begin

Chickens sit in the middle of the three: faster and cheaper than goats, but not as fast a breeder as rabbits. Their differentiator is the *unconditional* daily resource (eggs come regardless of breeding), which goats and rabbits don't have.

---

# Economy Reference

Per Livestock_System.md's Economy Integration loop (Eggs → Household Use → Surplus Sales):

- Eggs: low individual value, but steady and unconditional — a reliable low-effort income floor (lower in Winter)
- Meat (culled birds): moderate value, one-time per bird, no additional penalty attached
- Breeding stock (chicks/hens): sellable, per the existing Economy Integration diagram

No specific currency numbers proposed here — that's Economy_System.md's territory.

---

# Design Rules

1. Values on this page are confirmed baselines, not immutable — normal playtesting-driven tuning still applies.

2. Real-world chicken care (Research.md) informs direction and relative scale, not literal numbers.

3. Chickens' defining trait is unconditional daily output (eggs, reduced but not eliminated in Winter) rather than breeding-gated output (goat milk) or fast population growth (rabbits) — future additions to this species should preserve that identity rather than blur the three livestock types together.
