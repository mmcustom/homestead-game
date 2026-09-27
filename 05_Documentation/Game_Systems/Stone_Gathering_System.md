# Homestead
## Stone Gathering System v1.0

Status: Design Draft, requested 2026-09-26 (Mike). Built, tested, committed and pushed 2026-09-26 (Claude Code) — commit `4145888` (`8b3ea6e` carries the doc updates); `master` matches GitHub — see Current_Task_List.md

---

# Purpose

Stone already exists as a Material (Item_Data.md, 5.0 kg, "general building material") and Wood_Gathering_System.md's new Rock Pile is built to hold it, but nothing in the game actually produces Stone — it has no source at all. This system gives the player a real way to pick it up.

---

# Core Loop

Look at a rock on the ground
↓
Hold E to pick it up
↓
Stone (Item_Data.md)

---

# Distribution

Mike's own description (2026-09-26): "Stone should almost copy the same availability as natural resources found throughout the land. Rocks are lightly scattered everywhere, but more densely located around creeks, rivers and cliff side rock deposits throughout the land."

Two layers, not discrete habitat patches like Foraging_System.md's 13 species — this is closer in spirit to Fire Building's 40 scattered deadfall piles, but property-wide rather than woods-only:

- **Base layer:** a light, even scatter of individual pickup-able rocks across the whole property — pasture, forest, trail, everywhere — so Stone is never far away, just not abundant anywhere in particular. Loose "stone laying around," hand pickup, no tool.
- **Water dense zones:** noticeably more rocks along the property's water features — the creek (Spring Hollow to Bass Hole, Property_Layout.md) and Bass Hole's pond banks. Still loose stone, denser but the same hand-pickup rule as the base layer — no tool required here either.
- **The cliff-side rock deposit (South Ridge):** the one true exception. **Confirmed 2026-09-26 (Mike):** unlike the base scatter and the water zones, this is a real rock deposit/outcrop, not loose fieldstone — gathering from it requires the new Stone Pick Axe (Tools section below) equipped. This is what makes the deposit worth the trip: it's the property's only tool-gated Stone source, versus everywhere else being free hand pickup.

Exact rock count, spawn spacing, how much denser the water zones are than the base scatter, and whether the deposit itself looks like a distinct rock face/outcrop the player mines from versus a dense cluster of tool-gated rocks, are Claude Code's call, same propose-then-confirm pattern as the rest of this doc set.

**Built and tested 2026-09-26 (Claude Code):** 250 loose rocks total — 150 scattered lightly across the whole property (about one every 30 m), 90 in a denser band along the creek and pond banks, 10 lying at the foot of the outcrop. Placed via a new editor-only menu, Homestead → Place Stone (not player-facing). The South Ridge outcrop is 8 large, half-buried boulders on the ridge's south face, 16 m below the ridge top and clear of the cabin site — moved once already during testing, off the new-game spawn point where the first pass had put it.

---

# Picking Up Stone

**The base scatter and the water dense zones:** no tool required — picked up by hand, same as a deadfall pile, since real fieldstone doesn't need cutting or breaking to be gathered. Look at a rock and hold E; exact yield per rock (one Stone vs. a small handful) and whether rocks are single pickups or, in the dense zones, small multi-take piles like a felled tree's wood pile, are Claude Code's call. **Built and tested 2026-09-26 (Claude Code):** each rock is a single hold-E pickup worth 1 Stone (5 kg), gone for good once taken — no multi-take piles.

**The cliff-side rock deposit:** requires the Stone Pick Axe equipped (see Tools below) — looking at the deposit without it should explain what's missing, the same way Wood Gathering's Axe-required chopping already does. Exact interaction (hold-to-mine like chopping, or a quicker single action) and yield per attempt are Claude Code's call. `pick-axe.wav` (sfx_pickaxe, Audio_System.md) is the mining sound for this interaction. **Built and tested 2026-09-26 (Claude Code):** hold click to swing like the Axe; every 4 blows breaks off a Stone. The outcrop holds 40 Stone total before it's worked out. Without the Pick Axe equipped, looking at it reads "mine it with a Stone Pick Axe." `pick-axe.wav` plays one of six clean strikes per blow.

Rocks gathered this way don't respawn for Alpha 0.1, consistent with how felled trees and deadfall piles currently work — a future resource-regeneration pass (if any) would apply to all three the same way, not to Stone alone.

---

# Tools: Stone Pick Axe

**Added 2026-09-26 (Mike) — new craftable Tool.** A primitive Stone Pick Axe, crafted from **Sticks, Cordage, and Stone** (Mike's own materials, all already-existing items) — a stone head bound to a wood handle with cordage, same basic construction logic as the existing hand-tool concept Wood_Gathering_System.md originally deferred to Future. Its purpose is narrow and specific: it's what lets the player gather from the cliff-side rock deposit above. The base scatter and water-zone stones stay hand-pickup either way, with or without the Pick Axe equipped.

Since this is a genuinely new craftable Tool and not just an ingredient list, it needs an actual crafting interaction — most likely an Inventory "Craft" action along the lines of the existing Cook/Split buttons, rather than the world-placed "Build" pattern used for the Campfire/Wood Pile/Rock Pile (a Tool is carried and equipped, not placed in the world). Exact quantities of Sticks/Cordage/Stone required, and the craft interaction itself, are Claude Code's call, same propose-then-confirm pattern as the rest of this doc set. Since Stone itself has no source until this whole system is built, the very first Pick Axe a player makes will need Stone from the base scatter or water zones (both hand-pickup, no chicken-and-egg problem) before the cliff-side deposit becomes reachable.

**Built and tested 2026-09-26 (Claude Code):** recipe is 2 Sticks, 1 Cordage, 1 Stone, crafted from a new Craft row in the Inventory. Tested in Play Mode on a copy of Mike's save: picked up a stone, crafted the Pick Axe, mined the outcrop, and confirmed the save round trip (both gathered stone and the outcrop's remaining count persisted). **Flagged (Claude Code):** Cordage has no production source anywhere in the game — the Pick Axe, the new Primitive Shovel, and the new Pouch (Primitive_Storage_System.md) each need one, using up exactly the 3 Cordage a new game starts with, leaving none for snares or the Fish Trap. Worth deciding soon.

**Tiering — confirmed direction, not yet buildable, added 2026-09-26 (Mike).** This Stone Pick Axe is confirmed as the Pick Axe's Primitive tier; Mike wants Iron and Steel tiers above it (same progression as the new Primitive Axe and the Shovel, Wood_Gathering_System.md). **Gap now designed, still not buildable, 2026-09-26:** the new Mining_Metalworking_System.md lays out the full chain (mining Iron Ore — this outcrop is one of its two proposed sources — through Charcoal, a Bloomery, and Forge/Anvil/Workbench blacksmithing) needed to produce Iron and Steel; nothing in that chain is buildable yet, same "waits on Building's first implementation" status as the rest of that doc. Difficulty_System.md's earlier "metal Pick Axe" gap is superseded by this — it's really two tiers (Iron, Steel), not one generic "metal" tier.

---

# Design Rules

1. Distribution follows Mike's own framing: light and even everywhere, denser near water and the new cliff-side deposit — not a small number of fixed "Stone patches" like Foraging's discrete sites.

2. Exact counts, spacing, and pickup yield are Claude Code's call — propose a first pass, Mike confirms by feel in Play Mode.

3. No tool requirement for loose stone (base scatter and water zones) — stone there is surface fieldstone, not quarrying. The cliff-side rock deposit is the one exception, gated behind the new Stone Pick Axe.

---

# Cross-References

- **Item_Data.md** — Stone already exists as a Material (5.0 kg); this system is its first real source. Source column should be updated to point here once built. Needs a new Tools row for the Stone Pick Axe (Sticks/Cordage/Stone).
- **Wood_Gathering_System.md** — the Rock Pile in its Primitive Storage section is Stone's first real destination/use.
- **Property_Layout.md** — the creek/Bass Hole water features and the new cliff-side rock deposit (added to Terrain Character) are this system's dense zones.
- **Foraging_System.md** — a useful contrast, not a template: Foraging uses discrete named habitat patches, while Stone uses a continuous scatter with density zones instead.
- **Mining_Metalworking_System.md (new, 2026-09-26)** — proposes the South Ridge outcrop as a secondary Iron Ore source (a rare drop alongside Stone on the same mining interaction), feeding the Pick Axe's Iron/Steel tiers.
