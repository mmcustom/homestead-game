# Homestead
## Wildlife: Large Game v1.0

Status: Design Draft

---

# Purpose

Concrete, tunable stats for White-Tailed Deer, the sole Alpha 0.1 Large Game species defined in Wildlife_System.md and Hunting_System.md. This document does not redefine the general Wildlife or Hunting mechanics — it's the per-species numbers those mechanics need to actually run.

---

# Design Role

Large Game is the highest-value, highest-effort tier: best yield in the game per Hunting_System.md ("Large game provides the highest single-harvest food value"), but the most demanding in terms of weapon choice, approach, and shot placement, and the least forgiving of a Wounding Hit.

---

# White-Tailed Deer

- **Acquisition:** Hunting — Bolt-Action Rifle preferred, Recurve Bow viable only at close range (per Hunting_System.md)
- **Habitat:** Forest edges, meadows, water corridors (per Wildlife_System.md)
- **Activity pattern:** High activity at both Dawn and Dusk (per Wildlife_System.md's Daily Activity Cycles), extending to all-day during the Fall Rut Window (see below) — the only species confirmed active at both twilight windows, and the only one with an all-day mode at all
- **Abundance:** Least common of the three tiers to actually close on — deer are more alert and require more careful approach/stalk than Small or Medium Game
- **Yield:** 8 Meat (large) — confirmed 2026-09-22 (Mike) as the right ceiling relative to Small Game's 1 and Medium Game's 3–4
- **Hide:** Available year-round, confirmed 2026-09-22 (Mike) — not gated to Fall/Rut, but at a lower drop chance outside the Rut Window
- **Antlers:** Available year-round, confirmed 2026-09-22 (Mike) — same treatment as Hide, at a lower chance outside the Rut Window, with the highest chance during it. This revises Hunting_System.md's current "Antlers (seasonal)" wording, which read as Fall-only; the intent is year-round-but-rarer, not gated. Hunting_System.md's Huntable Species entry should be updated to match (see note below).

---

# Fall Rut Window — Confirmed

Confirmed 2026-09-22 (Mike): during the rut, Deer activity runs throughout the day rather than being limited to Dawn/Dusk.

Season_System.md doesn't break Fall into specific day ranges, so rather than invent exact dates, this is placed relatively: the Rut Window covers **the second half of Fall**, matching the real-world rut's late-in-the-season timing (Research.md: roughly late October–mid November) without needing a day-count system that doesn't exist yet elsewhere in the docs. If Season_System.md is ever given explicit day numbers per season, this window should be converted to match.

- **Outside the Rut Window** (first half of Fall, and the rest of the year): standard Dawn/Dusk activity pattern. Hide and Antlers can still drop, just less often.
- **During the Rut Window** (second half of Fall): activity extends through midday — Deer are the one species with all-day visibility during this stretch. Hide and Antler drop chance is at its highest here, on top of Fall's existing "excellent hunting season" feeding-activity boost from Wildlife_System.md/Season_System.md.
- This makes the back half of Fall the single best hunting window in the game for Deer specifically, reinforcing Hunting_System.md's existing framing that most of a player's winter meat should come from Fall hunting.

---

# Note: Hunting_System.md Needs a Small Wording Update

Hunting_System.md's Huntable Species section currently lists Large Game yield as "Meat, Hide, Antlers (seasonal)." With Antlers and Hide now confirmed year-round-but-rarer rather than Fall-gated, that line should change to something like "Meat, Hide, Antlers (year-round; higher chance during the Fall Rut)" so the two docs don't drift out of sync the way Water_System/Core_Survival_System did before Review_002 caught it. Flagging rather than silently editing another doc without you seeing the change first — say the word and I'll make that edit.

---

# Design Rules

1. Values on this page are confirmed baselines, not immutable — normal playtesting-driven tuning still applies.

2. Large Game sits at the top of the three-tier yield curve (Small 1 → Medium 3–4 → Large 8) established across Small_Game.md, Medium_Game.md, and this document.

3. The Rut Window (second half of Fall, all-day Deer activity, peak Hide/Antler chance) is confirmed and should carry over into any future Season_System.md revision that adds explicit day ranges.

4. Hide and Antlers are year-round-but-rarer, not Fall-gated — Hunting_System.md's wording should be kept in sync with this (see note above).
