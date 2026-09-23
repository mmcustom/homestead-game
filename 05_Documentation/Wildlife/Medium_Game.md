# Homestead
## Wildlife: Medium Game v1.0

Status: Design Draft

---

# Purpose

Concrete, tunable stats for Turkey and Waterfowl, the two Medium Game species defined in Wildlife_System.md and Hunting_System.md. This document does not redefine the general Wildlife or Hunting mechanics — it's the per-species numbers those mechanics need to actually run.

---

# Design Role

Medium Game sits between Small Game's reliability and Large Game's high-value difficulty: hunting-only (no trapping path), moderate yield, and more dependent on time-of-day and habitat than Small Game. Turkey and Waterfowl are now differentiated from each other (see below) rather than being interchangeable, per Mike's direction.

---

# Turkey

- **Acquisition:** Hunting only (Recurve Bow at Alpha 0.1; future Shotgun per Hunting_System.md's Future Weapons)
- **Habitat:** Forest openings, mature timber (per Wildlife_System.md)
- **Activity pattern:** High activity at Dawn, per Wildlife_System.md's Daily Activity Cycles (Turkey is explicitly listed alongside Deer as a Dawn-active species) — Dawn-only strong window, requiring the player to actually be in the right forest opening at that time
- **Abundance:** Moderate — requires being in the right place at Dawn specifically, not just showing up anytime
- **Yield:** 4 Meat + Feathers — confirmed 2026-09-22 (Mike): Turkey yields a small amount more than Waterfowl, reflecting a slightly larger bird
- **Season notes:** Wildlife_System.md's Fall "heavy feeding activity... excellent hunting season" applies; no species-specific override beyond the Dawn activity window above

---

# Waterfowl

- **Acquisition:** Hunting only (Recurve Bow at Alpha 0.1; future Shotgun)
- **Habitat:** Wetlands, tied directly to Water_System.md's water body locations — per Wildlife_System.md's Water Dependency note ("Waterfowl require wetlands"), the one Medium Game species whose location is fully determined by discovered water sources rather than generic forest/field habitat
- **Activity pattern:** Its own timing, confirmed 2026-09-22 (Mike) — distinct from Turkey/Deer's Dawn/Dusk gating. Proposed: Waterfowl are available throughout daylight hours rather than a narrow window, since real waterfowl spend most of the day resting and feeding on open water rather than moving in short bursts like Turkey or Deer. The tradeoff is location, not timing — Waterfowl require a discovered wetland, but reward the player anytime they're there.
- **Abundance:** Moderate, concentrated entirely at discovered wetland/pond/river locations
- **Yield:** 3 Meat + Feathers — one tier below Turkey per Mike's direction above
- **Season notes:** Season_System.md establishes that "Waterfowl migrate" — absent in Winter, present the rest of the year while a discovered water source remains available

---

# Design Rules

1. Values on this page are confirmed baselines, not immutable — normal playtesting-driven tuning still applies.

2. Medium Game yield sits below Large Game and above Small Game, maintaining the tiered yield curve across all Wildlife categories — Turkey (4) now sits slightly above Waterfowl (3), both still well under Large Game's 8.

3. Turkey and Waterfowl are differentiated on two axes at once: Turkey trades a narrow Dawn-only time window for slightly higher yield and no location requirement; Waterfowl trades a hard wetland-location requirement for all-day availability and slightly lower yield.

4. Waterfowl's habitat is a hard dependency on Water_System.md's discovered water locations, and its migration follows Season_System.md's existing "Waterfowl migrate" note — keep both in sync if either doc changes.
