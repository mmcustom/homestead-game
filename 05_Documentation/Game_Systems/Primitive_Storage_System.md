# Homestead
## Primitive Storage System v1.0

Status: Design Draft, requested 2026-09-26 (Mike). Built and tested 2026-09-26 (Claude Code) — commit/push in progress, not yet confirmed — see Current_Task_List.md

---

# Purpose

Wood_Gathering_System.md's "Primitive Storage (Wood Pile / Rock Pile)" section already established a working pattern: a buildable-now, ad-hoc storage container that lets the player offload inventory weight before Building_Housing_System.md's real storage structures (Root Cellar, Storage Shed, Barn) exist. Mike asked for that same pattern extended to three more resource categories that don't have anywhere to go yet — water, food, and general dry goods/resources — plus a new carrying Tool (a Pouch or Bag) to bring non-liquid resources to them, parallel to how the Bucket already carries water.

This doc is the new content; Wood Pile and Rock Pile stay documented where they already are, in Wood_Gathering_System.md.

---

# Relationship to Existing Primitive Storage

Wood Pile (holds Sticks/Branches/Logs/Firewood) and Rock Pile (holds Stone) are already built, tested, committed, and pushed. Same core mechanic reused below: a placeable container, deposit/withdraw interaction (R to store, E to take, per the existing pattern), no capacity limit unless Claude Code's build calls for one, persists in the world even empty.

---

# Water Storage — Water Barrel (primitive tier)

Purpose: stockpile carried water beyond what one Bucket trip holds, before Water_System.md's real Cistern/Rain Barrel buildings exist (those stay Future, tied to Building/Housing).

Added 2026-09-26 (Mike) — new primitive storage request. **Proposed (Claude, not confirmed):** a craftable Water Barrel, filled by pouring a carried Bucket into it (same deposit-style interaction as Wood Pile/Rock Pile), holding some multiple of the Bucket's capacity, placed the same way as the Campfire/Wood Pile. Build cost, capacity, and the exact fill interaction are Claude Code's call.

This is separate from the new metal cooking pot in Water_System.md's Boiling correction below — the Water Barrel stores water, the cooking pot boils it. Both use the Bucket to move water into them.

**Built and tested 2026-09-26 (Claude Code):** costs 3 Logs to build, holds up to 40 L of water. R pours carried water in (with the pour sound); E fills the player's Buckets back up, drawing purified water first when both are present. Tested in Play Mode: built it, poured water in, and drew it back out into the Bucket.

---

# Food Storage — Food Cache (primitive tier)

Purpose: mirrors Building_Housing_System.md's Root Cellar (Food storage) but buildable now, before Building/Housing exists.

Added 2026-09-26 (Mike). **Proposed (Claude, not confirmed):** a craftable Food Cache holding Food-category Consumables and Resources, same deposit/withdraw pattern as Wood Pile. Recommend it carry no spoilage-reduction bonus of its own for now — matching Rock Pile's "offload point only, no extra benefit" precedent — with the Root Cellar's real bonus arriving once Building/Housing ships. Whether to add a bonus anyway is Claude Code's call.

**Built and tested 2026-09-26 (Claude Code):** costs 2 Logs + 4 Branches to build, holds Food. No spoilage bonus, per the recommendation above — each stored batch keeps its original acquired day, so simply storing food doesn't reset its age once spoilage exists. Tested in Play Mode: stored and retrieved food with its acquired day intact.

---

# Dry Goods / Other Resources Storage — Storage Bin (primitive tier)

Purpose: catches everything that doesn't already have a home — not Wood/Stone (their own piles), not Water (Water Barrel above), not Food (Food Cache above). Covers things like Cordage, Feathers, Hides, Furs, Arrows/Rifle Rounds, and any other Material/Resource not otherwise piled.

Added 2026-09-26 (Mike). **Proposed (Claude, not confirmed):** a craftable general Storage Bin, same deposit/withdraw pattern as the others.

**Built and tested 2026-09-26 (Claude Code):** costs 6 Branches + 6 Sticks to build, holds everything else that isn't a Tool. **Flagged (Claude Code):** E currently takes everything that fits at once — a proper transfer screen, where the player chooses what to take, would be the natural next step for this and the Food Cache.

---

# Tools: Pouch or Bag

Added 2026-09-26 (Mike) — new craftable Tool, parallel to the Bucket but for non-liquid resources: increases how much the player can carry of something back to whichever storage above (or Wood Pile/Rock Pile) it belongs in, the way the Bucket increases what can be carried of water.

Materials weren't specified by Mike. **Proposed (Claude, not confirmed):** Deer Hide + Cordage — a hide pouch bound with cordage, using two Materials that already exist (Item_Data.md) rather than inventing a new resource. Mike may have had something else in mind and should correct if so.

Exact carry-capacity bonus, and whether it's a flat kg increase or a multiplier, is Claude Code's call, same as how the Bucket's own water capacity was decided.

**Built and tested 2026-09-26 (Claude Code):** recipe is 1 Deer Hide + 1 Cordage. Carried, it adds 10 kg to both the max-carry cap (55 kg) and the Encumbered threshold (40 kg); the limits drop back to normal if it leaves the inventory. The Inventory now shows two rows of three buttons for Build and Craft, each explaining on hover what it needs or why it can't be built there. **Flagged (Claude Code):** Cordage has no production source anywhere in the game — the Pouch, the Stone Pick Axe, and the Primitive Shovel each need one, using up exactly the 3 Cordage a new game starts with, leaving none for snares or the Fish Trap. Worth deciding soon. Tested in Play Mode: crafted the Pouch, confirmed the capacity increase, and confirmed the full save round trip (all three containers persisted).

---

# Cross-References

- Wood_Gathering_System.md — Primitive Storage (Wood Pile / Rock Pile), the established precedent this doc reuses.
- Water_System.md — Boiling / metal cooking pot correction (Bucket narrowed to transport only), and the Water Storage section (Cistern/Rain Barrel, the eventual real upgrade for the Water Barrel).
- Building_Housing_System.md — Storage category (Root Cellar, Storage Shed, Barn), the eventual real upgrades for Food Cache and Storage Bin.
- Item_Data.md — new Tools rows (metal_cooking_pot, pouch_bag).

---

# Design Rules

1. Primitive storage is an offload point, not a bonus — matches Rock Pile's precedent (no spoilage or quality benefit) unless Claude Code's build calls for one.

2. Real storage buildings (Root Cellar, Storage Shed, Barn, Cistern) remain the eventual upgrade once Building/Housing ships — primitive tier isn't replaced, just superseded in usefulness, same relationship Wood Pile/Rock Pile already have with their own future upgrades.

3. Each primitive storage type takes only its own category — Wood Pile stays wood, Rock Pile stays Stone, and so on — so there's never ambiguity about where a resource goes.
