# Homestead
## Wildlife: Small Game v1.0

Status: Design Draft

---

# Purpose

Concrete, tunable stats for Rabbit and Squirrel, the two Small Game species defined in Wildlife_System.md and Hunting_System.md. This document does not redefine the general Wildlife or Hunting mechanics — it's the per-species numbers those mechanics need to actually run.

Grouped as one file (matching Wildlife_System.md's own Small Game category), each species broken out individually below.

---

# Design Role

Small Game is the reliable, low-risk, low-yield tier — available via Trapping (primary, per Trapping_System.md) or close-range bow hunting, with minimal weather/time-of-day gating compared to Medium and Large Game. The tradeoff for that reliability is the smallest per-catch yield of the three tiers.

---

# Rabbit

- **Acquisition:** Trapping (primary — snares/box traps per Trapping_System.md) or Recurve Bow hunting
- **Habitat:** Brush lines, field edges, dense cover (per Wildlife_System.md)
- **Activity pattern:** No strong dawn/dusk gating, unlike Deer/Turkey — active throughout daylight hours, making it the most consistently available species
- **Abundance:** Common. Local population ceiling around a given trapping/hunting ground is high enough that a well-placed trapline rarely comes up empty, low enough that overtrapping a single spot noticeably thins it out (ties to Wildlife_System.md's Future Animal Population System — "Overtrapping rabbits may reduce catches")
- **Yield:** 1 Meat (small) + 1 Small Furs per catch, matching Hunting_System.md's existing "small amount of meat + fur" and Trapping_System.md's Small Furs terminology
- **Season notes:** Wildlife_System.md's general seasonal pattern applies (Spring population increase, Fall heavy feeding, Winter reduced movement) — no species-specific override

---

# Squirrel

- **Acquisition:** Trapping or Recurve Bow hunting
- **Habitat:** General small-game habitat, same as Rabbit. Confirmed 2026-09-22 (Mike): not gated behind discovering nearby nut groves — that cross-system dependency (per Foraging_System.md's "Nut trees attract squirrels" flavor note) is more detail than Alpha 0.1 needs. Squirrel functions as a second Rabbit-tier catch for now.
- **Activity pattern:** Same as Rabbit — no strong dawn/dusk gating
- **Abundance:** Common, same as Rabbit
- **Yield:** 1 Meat (small) + 1 Small Furs per catch, confirmed 2026-09-22 (Mike) as intentionally identical to Rabbit — no differentiation planned for Alpha 0.1
- **Season notes:** Same as Rabbit — no species-specific override

---

# Design Rules

1. Values on this page are confirmed baselines, not immutable — normal playtesting-driven tuning still applies.

2. Small Game is the low-risk, low-yield, most-available tier of the three Wildlife categories — any future change should preserve that relative to Medium_Game.md and Large_Game.md.

3. Rabbit and Squirrel are intentionally interchangeable at Alpha 0.1 — same yield, same habitat, same availability. If either is ever differentiated (e.g., reviving the nut-grove tie for Squirrel), update both entries together so they don't drift apart silently.
