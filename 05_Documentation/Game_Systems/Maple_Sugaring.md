# Homestead
## Maple Sugaring v1.0

Status: Future System — Not Alpha 0.1. Design ready per Core_Survival_System.md's existing "Maple syrup production" deferral; not a current "Needs Claude Code" build item. Confirmed 2026-09-24 (Mike) — spec drafted now so it's ready to build once the core survival loop is proven and this is pulled into scope.

---

# Purpose

Maple Sugaring is a seasonal, multi-step production chain: tapping select mature hardwood trees for sap, then boiling that sap down into syrup. It's one of Homestead's higher-value, labor-intensive homestead products — a real distinctive sweetener/trade good rather than another savory foraged or hunted staple, and it lands in the property's leanest stretch of the year, right as Winter's stores are lowest and before Spring's other foraging kicks in.

---

# Core Philosophy

Real maple sugaring is slow, weather-dependent, and unglamorous — days of checking taps for a small amount of concentrated syrup at the end. That's the point: Maple Sugaring should read as a distinctive, patient homestead skill, not a quick or easy source of calories. Design Rule 1 from Foraging_System.md ("Foraging rewards knowledge") applies here too — knowing which trees qualify and when the freeze-thaw window is open is itself part of the payoff.

---

# Sap Season

Confirmed 2026-09-24 (Mike, botanically accurate): sap only flows during freeze-thaw cycles — nights reliably below 32°F/0°C followed by days climbing back above freezing — the same threshold Weather_System.md's Precipitation Type by Temperature rule uses. This puts the window in late Winter into early Spring, not Fall, matching real sugar maple tapping. It should key off WeatherManager's existing temperature tracking directly (a rolling check for the freeze-thaw pattern) rather than a fixed calendar date, so a warm stretch can end the season early or a cold snap can extend it, the same way the game's other temperature-driven systems already behave. Exact detection window (how many consecutive freeze-thaw days trigger/end the season) is Claude Code's call.

---

# Tappable Trees

A subset of the property's existing mixed hardwood forest — not every tree, matching how real sugar maple needs roughly 10–12"+ trunk diameter (about 30–40 years old) before it can support a tap without harm. Property_Layout.md's forest was built generically ("mixed hardwood," two low-poly hardwood prototypes with no species distinction), so this is the first system that needs specific trees identified as tappable Maple within it — whether that's tagging a subset of the existing hardwood instances, checking a size/age proxy already available from the terrain's tree data, or something else, is Claude Code's implementation call.

Real-world note: sugar maple has the highest sap sugar content and is the standard tapping species; other maples and even birch can be tapped too, at a lower sugar content (more sap needed per unit of syrup). Alpha-1-and-beyond scope for Homestead almost certainly only needs one tappable "Maple" category, not species variety — multiple tappable species would be a future refinement, not part of this spec.

---

# Equipment and Collection

- **Tap / Spile** (new Tool item) — used on a qualifying tree to open a tap.
- **Sap Bucket** (or similar collection container) — hung at an active tap, gathering sap gradually over time rather than in one action, closer to how Water_System's or Livestock_System's over-time resources work than a single Foraging pick.

Sap left too long at a full or aging tap should spoil faster than most raw foraged goods — real sap is near-water and turns quickly once collected, which is part of why real sugaring means checking taps daily. This should push players to visit taps regularly rather than set-and-forget them, similar to how Trapping_System.md's traps need checking. Exact accumulation rate and spoilage window are Claude Code's call.

---

# Processing: Sap to Syrup

Raw Sap is not directly consumable or sellable — it must be boiled down over a heat source (the existing Fire System from Core_Survival_System.md) into Maple Syrup. Real sap-to-syrup ratio runs around 40:1 by volume for sugar maple; Homestead should use a simplified ratio for playability rather than the literal real number, but it should still read as meaningfully lossy — boiling down a large volume of Sap for a comparatively small amount of Syrup — so Syrup stays a low-yield, high-value good rather than a cheap bulk calorie source. Processing should take meaningful in-game time at the fire, not resolve instantly. Exact ratio and processing duration are Claude Code's call.

---

# Items

- **Raw Sap** — Resource. Low individual value, perishable (spoils faster than most raw foraged goods per above). Not directly useful until processed.
- **Maple Syrup** — Consumable / tradeable. High value relative to weight, long shelf life once properly processed (real syrup preserves far longer than raw sap). A genuine sweetener among Homestead's otherwise savory/plain Alpha food categories.

Both are new entries for whoever next updates Item_Data.md once this moves into active scope — not added there yet, since this document stays Future System for now.

---

# Uses

- **Food** — a caloric/sugar source distinct from Homestead's existing meat/fish/produce categories.
- **Economy** — a distinctive high-value trade good, per Economy_System.md's existing surplus/trade framing.
- **Timing payoff** — collected right as Winter reserves are lowest and before Spring's other foraging (Dandelion, Wild Onion, Cattail per Edible_Plants.md) comes in, giving the lean early-Spring stretch its own dedicated activity.

---

# Cross-References

- **Season_System.md** — sap season sits at the Winter/Spring boundary; worth a short pointer there once this moves into active scope, the same way other systems cross-reference each other.
- **Weather_System.md** — shares the 32°F freeze-thaw threshold with the new Precipitation Type by Temperature rule.
- **Core_Survival_System.md** — "Maple syrup production" is already listed under Future Expansion; this document is that item's ready-to-build spec, not a scope change by itself.
- **Foraging_System.md** — thematically adjacent (seasonal, knowledge-rewarding harvest) but kept as its own document rather than folded in, since sap tapping's equipment-and-processing loop is closer to Trapping_System.md's placement-and-check pattern than to simple Foraging picking.

---

# Design Rules

1. Sap season is temperature-driven (the same 32°F freeze-thaw signal used elsewhere), not a fixed calendar date.

2. Syrup should stay low-yield and high-value — the ratio and processing time should make it feel earned, not a cheap bulk food source.

3. This document is a ready spec, not a scope change. Core_Survival_System.md's Future Expansion deferral stands until Mike moves this into active build scope.
