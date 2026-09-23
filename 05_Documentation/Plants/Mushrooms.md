# Homestead
## Plants: Mushrooms v1.0

Status: Design Draft

---

# Purpose

Concrete, tunable stats for Mushrooms, defined generically ("Seasonal edible mushrooms") in Foraging_System.md. This document does not redefine the general Foraging mechanic — it's the per-species numbers that mechanic needs to actually run.

Confirmed 2026-09-22 (Mike): name real species rather than staying generic, with three species for variety.

---

# Design Role

Mushrooms are listed alongside Wild Greens as a Spring resource in Foraging_System.md's Seasonal Availability. Foraging_System.md also explicitly flags a Future System: "poisonous species" — misidentification risk is meant to eventually be part of this category's identity. Unlike Edible_Plants.md's reliable, repeatable Spring greens, real mushroom foraging is patchy and unpredictable — this category should feel more like a hunt than a harvest.

---

# Morel

The prized, unpredictable find — and the intended seed for Foraging_System.md's future poisonous-species system, since its real-world toxic lookalike (False Morel) is well-documented and visually similar enough to be a fair, well-known lookalike rather than an invented one.

- **Season:** Early-to-mid Spring, a narrow window rather than all of Spring
- **Yield per harvest:** 2 units per patch — confirmed 2026-09-22 (Mike)
- **Regrowth:** Annual, **not** fast-repeat — confirmed 2026-09-22 (Mike): matches how real morels actually fruit, briefly and unpredictably each year, rather than being a renewable patch like Dandelion. Once a season's flush is picked, that's it until next Spring.
- **Habitat:** Ground-growing, tied to specific host trees (real morels favor dead/dying elm, ash, and old apple trees) — distinct from the other two species below, which grow on wood
- **Uses:** Food, Trade
- **Identification/Poisonous species:** Not modeled in Alpha 0.1, per Foraging_System.md's Future System note — flagged as the natural future hook once that system is scoped

---

# Dryad's Saddle

A common, reliable, easy-to-identify Spring mushroom, added for variety per Mike's direction — the "you can actually count on finding this" counterpart to Morel's rarity.

- **Season:** Spring, same general window as Morel but not tied to the same narrow flush
- **Yield per harvest:** 2 units per patch
- **Regrowth:** Fast-repeat within Spring (a few in-game days), unlike Morel — real Dryad's Saddle fruits repeatedly from the same dead/dying tree through the season
- **Habitat:** Grows on dead or dying wood, not the ground — a genuinely different search pattern from Morel
- **Uses:** Food, Trade
- **Identification/Poisonous species:** No dangerous real-world lookalikes — this species stays simple even after a future misidentification system exists

---

# Oyster Mushroom

The third species, also wood-growing, rounding out a "two reliable, one rare" Mushroom category.

- **Season:** Spring flush, same general window
- **Yield per harvest:** 2 units per patch
- **Regrowth:** Fast-repeat, same as Dryad's Saddle
- **Habitat:** Dead or dying wood, same category as Dryad's Saddle — the two could reasonably share a habitat marker distinct from Morel's ground/host-tree marker
- **Uses:** Food, Trade
- **Identification/Poisonous species:** Low real-world lookalike risk, similar to Dryad's Saddle

---

# Design Rules

1. Values on this page are confirmed baselines, not immutable — normal playtesting-driven tuning still applies.

2. Morel is deliberately the rare, annual, ground-growing outlier; Dryad's Saddle and Oyster Mushroom are the common, fast-regrowing, wood-growing pair. Don't blur that distinction by making Morel repeatable or the other two rare — the contrast is the point.

3. Morel's real-world False Morel lookalike is the intended seed for Foraging_System.md's future poisonous-species system — don't pick a different eventual lookalike without revisiting this note. Dryad's Saddle and Oyster Mushroom have no meaningful real-world danger and should stay simple even after that system exists.
