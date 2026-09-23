# Homestead
## Plants: Nuts v1.0

Status: Design Draft

---

# Purpose

Concrete, tunable stats for Hickory Nuts, Walnuts, and Chestnuts, the three Nuts species defined in Foraging_System.md. This document does not redefine the general Foraging mechanic — it's the per-species numbers that mechanic needs to actually run.

---

# Design Role

Nuts are Fall's highest-value storage resource per Foraging_System.md ("High calorie food," "Winter food reserves"). Unlike Berries or Fruit, Nuts have the longest natural shelf life of any Foraging category — and, per Mike's confirmation below, still benefit from active drying/storage on top of that, the same as other perishables.

---

# Hickory Nuts

- **Season:** Fall
- **Yield per harvest:** 5 units per grove
- **Regrowth:** Annual
- **Uses:** High-calorie food, Winter reserves
- **Shelf life:** Long even unpreserved — confirmed 2026-09-22 (Mike) as fine to keep functionally identical to Walnuts for Alpha 0.1

---

# Walnuts

- **Season:** Fall, same as Hickory Nuts
- **Yield per harvest:** 5 units per grove
- **Regrowth:** Annual
- **Uses:** High-calorie food, Winter reserves
- **Shelf life:** Long, same as Hickory Nuts — confirmed 2026-09-22 (Mike): intentionally identical to Hickory Nuts, no differentiation needed for now

---

# Chestnuts

- **Season:** Fall, same window
- **Yield per harvest:** 4 units per grove
- **Regrowth:** Annual
- **Uses:** High-calorie food, Winter reserves
- **Shelf life:** Moderate rather than long — confirmed 2026-09-22 (Mike): keeping this one differentiator, reflecting real chestnuts' lower fat/higher starch content storing less indefinitely than hickory or walnut

---

# Preservation

Confirmed 2026-09-22 (Mike): unlike the original proposal, Nuts are **not** exempt from Preservation — active drying/storage still extends shelf life further, the same as Fruit and Meat. The difference from those categories is baseline, not mechanic: Nuts start with a long natural shelf life even unpreserved, so the Preservation step matters less urgently than it does for something like Pawpaw (Fruit.md), but it still helps and should use the same Preservation System rather than being skipped.

---

# Design Rules

1. Values on this page are confirmed baselines, not immutable — normal playtesting-driven tuning still applies.

2. Nuts are the long-natural-shelf-life Foraging category, not a preservation-exempt one — drying/storage still extends life further, consistent with how Preservation works everywhere else in the game.

3. Hickory Nuts and Walnuts are intentionally identical; Chestnuts is the one deliberate exception with a shorter shelf life. If either Hickory or Walnut is ever differentiated later, update both together so they don't drift apart silently.

4. Annual regrowth follows Berries.md's and Fruit.md's precedent.
