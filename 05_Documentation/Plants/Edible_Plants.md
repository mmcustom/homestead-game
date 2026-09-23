# Homestead
## Plants: Edible Plants v1.0

Status: Design Draft

---

# Purpose

Concrete, tunable stats for Dandelion, Wild Onion, and Cattail, the three Edible Plants species defined in Foraging_System.md. This document does not redefine the general Foraging mechanic — it's the per-species numbers that mechanic needs to actually run.

---

# Design Role

Edible Plants are Spring's "Wild Greens" per Foraging_System.md, explicitly called an "Early food source." Their value isn't calorie density (Nuts own that) — it's being available earliest in the year, when stored Winter reserves are running low and nothing else has come in yet.

---

# Dandelion

- **Season:** Spring
- **Yield per harvest:** 2 units per patch
- **Regrowth:** Fast — confirmed 2026-09-22 (Mike) at roughly 3–5 in-game days rather than an annual cycle, so a patch can be leaned on repeatedly through Spring rather than harvested once per year like Berries/Fruit/Nuts
- **Uses:** Early food source
- **Nutritional role:** Light, low-calorie — its value is timing (available when little else is), not quantity

---

# Wild Onion

- **Season:** Spring, same window
- **Yield per harvest:** 2 units per patch
- **Regrowth:** Fast, same as Dandelion
- **Uses:** Early food source — real wild onion is also a flavoring/seasoning ingredient, which could matter once a cooking/recipe system exists (a future hook, not a decision here)
- **Nutritional role:** Same tier as Dandelion

---

# Cattail

- **Season:** Spring, same window
- **Yield per harvest:** 2 units per patch
- **Regrowth:** Fast, same as the other two
- **Uses:** Early food source
- **Habitat/location:** Tied to Water_System.md's discovered water sources — confirmed 2026-09-22 (Mike), the same cross-system dependency Waterfowl has in Medium_Game.md. Cattail only appears at wetland/pond/river locations the player has already found, unlike Dandelion and Wild Onion which aren't location-gated.
- **Nutritional role:** Same tier as Dandelion/Wild Onion

---

# Design Rules

1. Values on this page are confirmed baselines, not immutable — normal playtesting-driven tuning still applies.

2. Edible Plants' defining trait is early-Spring availability and fast regrowth, not yield size — keep them the lowest-yield, fastest-regrowth category if anything is added later.

3. Cattail's wetland tie should stay in sync with Water_System.md's discovered water source list — if that list changes, Cattail's availability should be re-checked, the same way Medium_Game.md's Waterfowl entry needs to be.
