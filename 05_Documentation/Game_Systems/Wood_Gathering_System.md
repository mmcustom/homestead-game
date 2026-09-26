# Homestead
## Wood Gathering System v1.0

Status: Built and tested 2026-09-26 (Claude Code) — NOT YET committed. Sits on top of the still-uncommitted Phase 3 work.

---

# Purpose

Right now the only source of Firewood is gathering deadfall piles by hand (Fire Building's own implementation), and Logs — required by Building_Housing_System.md's Small Cabin — have no source in the game at all. This system gives the player a real way to cut down trees and turn what they get into Firewood or building material, using the Axe that already exists as a Tool (Item_Data.md, Inventory_System.md).

---

# Core Loop

Equip Axe
↓
Chop down a tree
↓
Sticks + Branches + Logs
↓
Build (Logs) or burn (Logs, Branches) or craft (Sticks)

---

# Chopping Down a Tree

Requires the Axe equipped. Exact interaction (hold-to-chop vs. repeated hits, how many hits/how long per tree, whether smaller trees fall faster than large ones, and whether a felled tree leaves a stump) is Claude Code's call, same propose-then-confirm pattern as the rest of this doc set. A felled tree yields some mix of Sticks, Branches, and Logs — exact yield per tree (and whether it scales with tree size) is also Claude Code's call.

**Built and tested 2026-09-26 (Claude Code, for Mike to judge by feel):** hold left-click on a trunk within reach with the Axe equipped. About one swing every 0.9s, 4 stamina per swing ("Too tired to swing" if stamina runs out); being hungry or thirsty slows the work via the existing work-efficiency value from the survival stats. Swings needed to fell: about 10 for a broad hardwood, about 13 for a tall hardwood, about 3 for a shrub — more for bigger trees. A HUD progress bar tracks it, and looking away for a moment doesn't lose progress. On felling, the tree tips away slowly, then fast, settles with a small bounce, then sinks away after a few seconds; hardwoods leave a stump, shrubs leave nothing. Felled trees stay down across saves and reloads (only the in-game copy of the terrain changes, the terrain asset on disk is untouched) — trees don't grow back yet. Implementation note: trees are part of the terrain rather than separate objects, so if `PropertyTerrainBuilder` ever regenerates the terrain, felled-tree state in existing saves would point at different trees — a known risk, not yet acted on.

Tested in Play Mode on a copy of Mike's save: felling a broad hardwood produced 2 Logs, 5 Branches, and 6 Sticks, with a stump, a wood pile, and one fewer terrain tree; save/reload round trip confirmed the felled tree and pile both persisted.

---

# Yields

## Logs

The largest, most valuable yield. Two uses:

- **Building** — Building_Housing_System.md's Small Cabin already lists "Logs" as a Requirement; this is that Requirement's real source once both systems exist. Building_Housing_System.md itself has no Unity implementation yet (see that doc), so this use is Future until Building actually gets built.
- **Firewood** — a Log can be chopped into Firewood with the Axe (see Chopping Firewood below). Burns the longest of the two fuel-yielding materials.

## Branches

Mid-sized — bigger than a Stick, smaller than a Log. Two uses:

- **Primitive shelter** — Building_Housing_System.md's Lean-To ("built from local materials") is the natural home for this, once Building exists. Future until that system is built.
- **Firewood** — also choppable into Firewood, but burns for less time than a Log's worth.

## Sticks

The smallest yield. Used to craft different types of hand tools when combined with Stone and Cordage. No general crafting system exists yet (deferred post-Alpha per the 2026-09-25 Decisions_Log.md entry — each feature defines its own small ad-hoc recipe as needed, rather than building a generalized system early), so this stays a Future use until a specific hand tool is actually requested and gets its own ad-hoc recipe, the same way Cordage's and Arrows' recipes were handled.

**Built and tested 2026-09-26 (Claude Code, for Mike to judge by feel) — actual yields per tree:**

| Tree | Logs | Branches | Sticks |
|---|---|---|---|
| Broad hardwood | about 2 | about 4 | about 5 |
| Tall hardwood | about 3 | about 3 | about 4 |
| Shrub | 0 | 1 | about 3 |

Amounts scale with the tree's size. Everything lands in a wood pile at the tree's base rather than straight into Inventory — looking at a pile shows what's in it, and E takes whatever fits, lightest items first and Logs last, so a big tree can take more than one trip ("Left behind: 2 Logs" when the pile isn't fully cleared). Piles save with the game and disappear once emptied. Item weights used: Sticks 0.1 kg, Branches 1 kg, Logs 8 kg (Item_Data.md).

---

# Chopping Firewood

A separate Axe interaction from felling a tree: chopping a carried (or dropped) Log or Branch converts it into Firewood — the same Firewood item Fire Building already produces from deadfall, so it drops straight into the existing Fire System (Core_Survival_System.md) and campfire fuel loop with no new item needed on that end. Logs should yield more Firewood, or Firewood that burns longer, than Branches — exact conversion numbers are Claude Code's call.

**Built and tested 2026-09-26 (Claude Code, for Mike to judge by feel):** 1 Log makes 5 Firewood (10 hours of burn time at Fire Building's existing 2-hour-per-Firewood rate); 2 Branches make 1 Firewood. Both conversions lighten the load a little. Two ways to split: at a wood pile, swinging the Axe at it splits its Logs first (4 swings each), then its Branches; or, while carrying the Axe, carried Logs and Branches get a Split button in the Inventory screen that works like Cook — about 4s per Log, about 2s per pair of Branches, with chopping sounds, a countdown on the button, Shift-click for the whole stack, click again to stop, and it keeps running while the Inventory screen is open.

Tested in Play Mode: splitting 2 Logs at a pile produced 10 Firewood; splitting from Inventory turned 1 Log into 5 Firewood and 4 Branches into 2 Firewood with the leftover Branch kept (an odd Branch doesn't get wasted). One visual-only issue noted but not re-verified on screen: split Firewood pieces in a pile had been visually merging into long bars — fixed by laying the pieces out across the row instead.

---

# Primitive Storage (Wood Pile / Rock Pile)

**Added 2026-09-26 (Mike) — requesting now, not deferred to Building.** Mike originally scoped a player-placed wood pile and rock pile as a Future ask, sequenced after Building_Housing_System.md's real storage buildings (Root Cellar, Storage Shed, Barn) shipped — see that doc's Storage category. He's since decided the primitive versions should come in now, with better storage upgraded in later once Building exists.

Wood Gathering already produces a working pile mechanic as a side effect of felling — a tree drops its yield into a pile at its base that persists across saves, shows its contents on look, and lets the player take what fits over multiple trips (see Chopping Down a Tree above). A player-placed Wood Pile is the same mechanic made deliberate: build one from Inventory (same "Inventory → Build" pattern as the Campfire, Fish Trap, and traps), place it, and then deposit or draw from it like any felled-tree pile. Holds the game's wood-family Materials — Sticks, Branches, Logs, Firewood.

A Rock Pile is the same idea for Stone (already an existing Material, Item_Data.md) — nothing currently produces or stores Stone at all, so this gives it its first real home.

Exact build cost (if any — could be free to place, or cost a small amount of Cordage/materials, Claude Code's call), capacity (unlimited vs. a soft cap), and the deposit interaction (an Inventory "Store" button mirroring Cook/Split, or a direct drop-into-pile interaction) are Claude Code's call, same propose-then-confirm pattern as the rest of this doc set. Both pile types persist across saves the same way felled-tree wood piles already do.

**Future, once Building_Housing_System.md ships:** these primitive piles get replaced or supplemented by real storage buildings — larger capacity, weather protection, and (per that doc's own Storage category) a proper Storage Shed as the built upgrade path. The primitive piles aren't meant to be the permanent answer, just the buildable-now one.

---

# Cross-References

- **Building_Housing_System.md** — Small Cabin's "Logs" Requirement and the Lean-To's "local materials" both get a real source here, once Building itself is built. Not a dependency the other way — this system doesn't need Building to exist to be useful, since Logs/Branches can already be burned as Firewood. Storage category also cross-references the Primitive Storage section above.
- **Core_Survival_System.md (Fire System)** — Firewood produced here is the same Firewood item Fire Building already uses; no change needed to the Fire System itself.
- **Item_Data.md** — needs three new Materials: sticks, branches, logs (non-perishable, weight a first-pass proposal like everything else in that doc).


---

# Design Rules

1. This system covers chopping trees and Logs/Branches into Firewood only — it does not include building a primitive shelter or crafting hand tools. Those stay Future, noted here and in Building_Housing_System.md, until their host systems (Building/Housing, a real reason to craft a specific hand tool) actually exist to receive them.

2. Exact chop timing, yield counts, and Firewood-per-Log/Branch conversion are all Claude Code's call — propose a first pass, Mike confirms by feel in Play Mode, same pattern as the rest of this doc set.

3. Firewood produced here is identical to Firewood from Fire Building's deadfall gathering — one item, two sources, no special-casing needed in the Fire System itself.

---

# Starting Kit Note

**2026-09-26 (Claude Code):** the Axe is now in the starting kit for new games, since nothing else in the game previously supplied one — without it, this whole system would be unreachable from a fresh start. F11 also grants an Axe to existing saves that predate this change.
