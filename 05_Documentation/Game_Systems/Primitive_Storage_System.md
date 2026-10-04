# Homestead
## Primitive Storage System v1.0

Status: Design Draft, requested 2026-09-26 (Mike). Built, tested, committed and pushed 2026-09-26 (Claude Code) — commit `4145888` (`8b3ea6e` carries the doc updates). Tool Rack and the shared per-item transfer screen added 2026-10-02, committed and pushed as `9c75638` (code) / `5410341` (docs) — `master` matches GitHub — see Current_Task_List.md

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

**Resolved 2026-10-02 (Mike): "i think we need a per-item transfer screen now."** Confirms the deferred fix flagged above (and again on Tool Rack below) as a real ask, not a someday-maybe. **Proposed scope (Claude, not yet confirmed by Mike):** build it once as the shared take/store UI for every primitive storage container — Wood Pile, Rock Pile, Water Barrel, Food Cache, Storage Bin, and Tool Rack alike — rather than a one-off for just Storage Bin/Tool Rack, since they all share the same underlying R/E interaction today and a player would reasonably expect the same selection behavior everywhere. Exact UI (a list with quantity steppers, a grid of tappable stacks, drag-and-drop) is Claude Code's first-pass call, same propose-then-confirm pattern as everything else. If Mike actually meant a narrower scope (e.g., just Tool Rack, where swapping one or two tools is the most common case), flag that back.

**Built and tested 2026-10-02 (Claude Code):** built the shared screen at the proposed broad scope — Wood Pile, Rock Pile, Water Barrel, Food Cache, Storage Bin and Tool Rack all open it on E instead of taking everything at once. It's a new fifth screen inside the existing Inventory/Journal/Map/Trading Post window (`GameScreens`), with no hotkey or tab of its own — looking at a built storage pile and pressing E opens it targeting that specific pile, and the other four tabs stay reachable while it's open. One row per kind of thing stored (the Water Barrel breaks out by quality — Purified, then Excellent→Unsafe — rather than one lump "water" row, since picking a quality is the whole point there); clicking Take picks one, Shift-click takes the whole stack, same convention already used by Cooking's Split and Trading Post's Buy/Sell. A row that can't be taken (too heavy, or the barrel with no Bucket/Canteen room) shows why, greyed out, instead of disappearing. A shortcut button at the top keeps the old one-press behavior available too — "Take Everything" normally, "Fill Bucket (best quality)" for the barrel — so emptying the whole thing in one go is still one click, same as before this change.

**Scope calls made along the way:** (1) storing (R) stays exactly as it was — bulk, everything carried that the pile accepts (minus whatever's equipped) — since Mike's own phrasing was about picking what comes back out, not what goes in **(superseded 2026-10-04 — Mike's playtest showed he wanted selectable storing too; see "Two-Way Transfer Screen" below)**; (2) a felled tree's wood pile and a Small Cabin's staged CabinSite keep the old instant take-all on E rather than opening the screen — they're incidental piles visited often and quickly while gathering, not the deliberate, built storage this request was about, and a modal per visit would be a real downgrade there. Both are flaggable if Mike wants them included too. Tested in Play Mode on a copy of Mike's save: built a Storage Bin, Tool Rack and Water Barrel, stored a mix of goods/tools/water qualities into each, then for each one opened the screen via the real E interaction, took a single item, Shift-took a whole water-quality stack, used both shortcut buttons, switched the open window from one pile straight to another, closed it back to normal play, confirmed R still bulk-stores in the World unchanged, and confirmed a full save/load round trip with every container's contents intact. One real bug caught and fixed before shipping: the screen's live-refresh had been double-firing on every take (it was both calling its own refresh and separately reacting to the inventory-changed event that same take fired), which briefly left duplicate stale rows in the hierarchy for a frame — fixed by dropping the redundant event subscription, since nothing else can touch a pile while the screen's own Menu state has gameplay paused anyway. Save restored and hash-verified after; nothing of Mike's was left changed. **Committed and pushed `9c75638` (code) / `5410341` (docs).**

---

# Tool Storage — Tool Rack (primitive tier)

Purpose: catches what Storage Bin explicitly doesn't — Tools (Axe, Rifle, Bow, Fishing Rod, Cane Pole, Bucket, Fish Trap, Flint and Steel, Recurve Bow, Hammer, and so on).

**Gap found 2026-10-02 (Mike), playtesting.** Hit the carry-weight limit trying to gather Hammer and Small Cabin materials while still hauling a full hunting/fishing/survival toolkit, and went looking for somewhere to stash the tools he didn't need for that trip. None of the shipped primitive storage takes a Tool: Storage Bin's own spec above explicitly says "holds everything else that isn't a Tool." There's currently no way anywhere in the game to shed a Tool's weight except not carrying it at all — a real gap, not a missing-UI question.

**Resolved 2026-10-02 (Mike): "yes, separate tool storage and other resource storages."** Confirmed as its own dedicated container rather than folding Tools into Storage Bin — a Tool Rack (or similar), parallel to how Wood Pile/Rock Pile/Water Barrel/Food Cache/Storage Bin each already stay scoped to one category (Design Rule 3 below already set this precedent, just hadn't been extended to Tools yet). Build cost, capacity, and placement are Claude Code's first-pass call, same propose-then-confirm pattern as every other primitive storage type.

**Related, pre-existing, not re-opened by this:** Storage Bin's own "everything that fits at once" take interaction (no partial-selection transfer screen) was already flagged by Claude Code as a known gap when it shipped. Worth keeping in mind for the Tool Rack's own design — an all-or-nothing take would be especially awkward for a tool stash the player wants to swap pieces of — but Mike hasn't asked for the selective-transfer fix itself yet, so that stays open rather than bundled into this.

**Built and tested 2026-10-02 (Claude Code):** costs 4 Branches + 4 Sticks to build, holds any Tool (Axe, Rifle, Bow, Fishing Rod, Cane Pole, Bucket, Fish Trap, Flint and Steel, Recurve Bow, Hammer, Pouch, and so on) — anything of `ItemCategory.Tool`, the same category check Storage Bin already uses to exclude them. R stores every Tool carried that isn't currently equipped; E takes back whatever fits. Looks like a standing wooden rack: a frame with one peg per kind of Tool stored (up to 6, more just stack on the nearest peg). Tested in Play Mode on a copy of Mike's own save — the exact overloaded toolkit from his playtest report (Flint and Steel, Bucket, Fishing Rod, Cane Pole, Recurve Bow, Bolt-Action Rifle, Fish Trap, plus the equipped Axe, at 44.95/45 kg carried): built the rack, stored all seven unequipped Tools at once (the Axe stayed equipped and in hand, correctly skipped), took them all back out, and confirmed a save/load round trip with the rack's contents intact. Save restored and hash-verified after; nothing of Mike's was left changed. **Committed and pushed `9c75638` (code) / `5410341` (docs).**

**Update 2026-10-02 (Claude Code):** the all-or-nothing take flagged above is resolved — see the Storage Bin section's "Built and tested" entry above for the shared per-item transfer screen, which the Tool Rack uses too. Swapping one or two specific tools (the exact case this flag called out) now works as intended. **Committed and pushed `9c75638` (code) / `5410341` (docs).**

---

# Two-Way Transfer Screen (store side + side-by-side layout)

**Gap found 2026-10-04 (Mike), playtesting:** "i'm having trouble moving items into and out of the structures. it doesn't allow me to select which tools to put in the tool storage, it just transferrs all tools into the storage. I think we need to have an individual inventory screen for each structure that we can put items in individually and remove individually. maybe when working with one of the structures open 2 seperate inventory windows. 1 for the player and 1 for the structure that we can move items around as necessary?"

This is a real gap in what shipped 2026-10-02, not a bug: the shared transfer screen only covers the **take** direction. Scope call (1) in the Storage Bin entry above deliberately left storing (R) as bulk — "everything carried that the pile accepts (minus whatever's equipped)" — because Mike's original phrasing ("a per-item transfer screen") was read as being about what comes back out. Mike's playtest shows he meant both directions. That earlier read is now superseded: **storing must be selectable too**, and the screen should show the player's side and the structure's side together.

**First-pass scope (Claude, not yet confirmed by Mike) — Claude Code should correct if this doesn't match:**

- **One shared screen, targeting the specific structure — not a different screen per structure type.** "An individual inventory screen for each structure" is met by the screen opening on whichever pile/rack/barrel/bin the player is looking at, titled and filled with that structure's own contents. Building six separate screens would duplicate the same layout for no player-visible gain. Reuses the existing fifth `GameScreens` screen rather than replacing it.
- **Two panels side by side:** left = the player's carried items, right = the structure's contents. Mike suggested "2 separate inventory windows"; two panels inside the one screen is the proposed way to deliver that (same window, so the other four tabs stay reachable and nothing floats over gameplay). Claude Code's call if two literal windows turn out to be easier or clearer.
- **Same click convention already in use:** click moves one item (or one batch) across, Shift-click moves the whole stack. A click on the player's side stores; a click on the structure's side takes. Matches Cooking's Split and Trading Post's Buy/Sell.
- **Player panel shows only what this structure accepts**, or shows everything with non-accepted rows greyed and a reason on hover ("Tool Rack only holds Tools") — whichever reads better; Claude Code's call. Whatever is currently equipped is greyed with its own reason ("equipped — put it away first"), same exclusion R already applies today.
- **Greyed-with-a-reason, never silently missing:** an item that can't move (too heavy to carry, structure full, Water Barrel at its 40 L cap, no Bucket/Canteen room) stays visible, greyed, and says why — same pattern the take side already uses.
- **Water Barrel keeps its per-quality rows** (Purified, then Excellent→Unsafe) on the structure side; the player side shows carried Bucket/Canteen water by quality so the player picks which quality to pour in, not just "all water."
- **Capacity readouts on both sides:** player panel shows carried weight against the cap (already tracked by the carry-weight system); structure panel shows capacity where one exists (the Water Barrel's 40 L; others have none today).
- **One-click shortcuts stay, one per direction:** "Take Everything" / "Fill Bucket (best quality)" already exist on the structure side; add a matching "Store All Eligible" on the player side so the old one-press bulk store survives as an option rather than the only behavior.
- **R and E:** proposed that both open this screen, with R no longer instantly bulk-storing. If Mike would rather keep R as a one-press quick-store (so a fast drop-off trip doesn't need a screen), that's a reasonable alternative — flag it back. Claude Code's call on the default; this is the main behavior change from what shipped.
- **Incidental piles unchanged:** a felled tree's wood pile and a Small Cabin's staged CabinSite keep their instant take-all on E, same carve-out as the 2026-10-02 build — nobody wants a modal while chopping.

**Related:** Mike's wording says "moving items into **and out of**" — the take side already shipped as a per-item screen on 2026-10-02, so out should already work item by item. Worth Claude Code confirming during its test pass that it does, in case something about the take flow also felt wrong in playtest and hasn't been reported yet.

**Built and tested 2026-10-04 (Claude Code):** built at the proposed scope. **Committed `ffa9fca`** (confirmed 2026-10-04); the separate CabinSite-removal work from the same session is `e7cbe67`.

- **Opening:** E or R on a Wood Pile, Rock Pile, Water Barrel, Food Cache, Storage Bin or Tool Rack opens the screen on that specific structure. It opens even when the structure is empty, so items can be put in.
- **Left panel (what you carry that this structure holds):** carried weight against the cap, e.g. "10.3 / 45 kg". An empty left panel says what the structure does hold. Chose the "only what this structure accepts" option from the proposal over showing everything greyed.
- **Right panel (structure contents):** a capacity readout where one exists — "35 / 40 L" for the barrel — and "3 stored · no limit" for the rest.
- **Moving items:** click moves one, Shift-click moves the stack, with a status line at the bottom. Rows that can't move stay greyed with the reason: "equipped — put it away first", "the barrel is full", "too heavy to carry more".
- **Water Barrel:** per-quality rows on both sides, and the left readout also shows water container capacity, e.g. "6 / 10 L".
- **Shortcuts:** "Store All Eligible" heads the left panel; "Take Everything" or "Fill Bucket (best quality)" heads the right. Store All Eligible disables itself when the barrel is full.
- **R stays a screen opener, not a quick-store** — Claude Code's call, since Store All Eligible is one click once the screen is open. R's prompt now reads "Store Items (…)". Flaggable if Mike wants the old one-press R back.
- **Felled-tree piles and CabinSite unchanged:** they keep instant take-all on E and bulk deposit on R, confirmed on a real CabinSite.

**Tested in Play Mode** (by calling the same code the buttons call and checking layout in screenshots — nothing clicked with a real mouse): storing the Knife alone left the Bucket and Flint in the pack with only the Knife going into the rack; the Storage Bin offered only Arrows and Cordage; a plain click moved 1 Arrow, the whole-stack path moved the other 24, and Store All Eligible moved the Cordage and left the Tools. **Taking by item — the one Mike asked Claude Code to confirm — works:** taking the Bucket alone left the rest in the rack. Water Barrel per-quality rows work both ways, clamping at 40 L with greyed "the barrel is full" rows. Contents round-tripped through a save identically; a Food Cache kept the acquired days of two cooked-venison batches (day 1 and day 4) through a store and a take. **Not tested: Shift as held during a real click** — the whole-stack path was called directly.

**Two problems found and handled along the way:** (1) the first layout stacked both panels on the right half, because the layout helper moves the rect it's called on rather than making a new one — caught in a screenshot and fixed; (2) rows from the previously open structure linger for the rest of the frame in which the screen switches or refreshes — harmless to the player, only matters when reading the screen programmatically in the same frame. Mike's slot1 save had changed since the last backup (he played that morning), so a fresh backup was taken and restored after, all six files matching. Two Play Mode sessions broke from Unity reloading scripts and were discarded without touching the save.

---

# Take Down Structure (stray piles)

**Added 2026-10-04 (Claude Code), building on the CabinSite-removal request (see Building_Housing_System.md, Tools: Hammer):** Hammer Dismantle was never built, so the spec's fallback applied — the same removal covers stray storage piles for now. A **"Take Down Structure"** button sits at the bottom right of the transfer screen for all six kinds (Wood Pile, Rock Pile, Water Barrel, Food Cache, Storage Bin, Tool Rack). It's offered only when the structure is empty — otherwise it's greyed with "Take Down — empty it first" — so nothing disappears with it. It takes two clicks, and returns **half the build materials, rounded down**, following the Lean-To's take-down precedent and the spec's partial-refund idea; the exact fraction is Claude Code's first-pass call. A Rock Pile is free to build, so it returns nothing. Piles have no separate on-site prompt, since E and R both open the screen. Felled-tree piles are unaffected and vanish when emptied, as before. Tested: the button stayed greyed while a Tool Rack held a Knife; after the Knife was taken back, the two-click confirm closed the screen and refunded +2 Branches and +2 Sticks. Not tested: the button with a real mouse. **Superseded later 2026-10-04 by the real Hammer Dismantle (`fc37969`):** the button now needs the Hammer equipped (greyed "equip the Hammer" or "empty it first"), still empty-only and still half the build materials rounded down. Equipping the Hammer while the screen is open doesn't update the button — close and reopen the screen. See Building_Housing_System.md, Tools: Hammer.

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
- Inventory_System.md — Dropping Items (2026-10-03), the general any-item/any-location offload action. Distinct from this doc's containers: dropping works anywhere on any item with no category restriction, while every primitive storage type here stays a specific container that only takes its own category.
- Building_Housing_System.md — Tools: Hammer's new Dismantle/Remove action (2026-10-03), which will apply to every container in this doc (Wood Pile, Rock Pile, Water Barrel, Food Cache, Storage Bin, Tool Rack) as well as permanent housing — removing the pile itself, not just taking what's stored in it.

---

# Design Rules

1. Primitive storage is an offload point, not a bonus — matches Rock Pile's precedent (no spoilage or quality benefit) unless Claude Code's build calls for one.

2. Real storage buildings (Root Cellar, Storage Shed, Barn, Cistern) remain the eventual upgrade once Building/Housing ships — primitive tier isn't replaced, just superseded in usefulness, same relationship Wood Pile/Rock Pile already have with their own future upgrades.

3. Each primitive storage type takes only its own category — Wood Pile stays wood, Rock Pile stays Stone, Tool Rack stays Tools, and so on — so there's never ambiguity about where a resource goes.

4. Moving items between the player and a structure is selectable in both directions (2026-10-04) — the player picks what goes in and what comes out; bulk "Store All Eligible" / "Take Everything" exist as shortcuts, not as the only behavior.
