# Homestead
## Inventory System v1.0

Status: Design Draft

---

# Purpose

The Inventory System governs what the player can carry, store, and manage.

It is the connective tissue between nearly every other system — Foraging, Fishing, Trapping, Hunting, Water, Livestock, Economy, and Building and Housing all produce or consume items through Inventory.

---

# Core Philosophy

Carrying capacity should create real decisions, not just be a formality.

A player should have to choose between:

- Carrying more water
- Carrying more food
- Carrying tools
- Carrying harvested goods

Encumbrance should discourage hoarding everything at once and reward planned trips, consistent with Preparation Beats Panic.

---

# Item Categories

## Resources

Examples: Meat, Fish, Berries, Nuts, Small Furs, Hides, Antlers

---

## Tools

Examples: Axe, Recurve Bow, Bolt-Action Rifle, Fishing Rod, Traps

---

## Consumables

Examples: Cooked food, Water Container contents

---

## Materials

Examples: Lumber, Stone, Cordage

Full item detail — spoilage, sale value, and similar — lives in the relevant Game System doc. This document governs storage and carrying rules only.

---

# Carrying Capacity

The player has a limited carry weight, not slots — weight-based, tied to First Person Controller's movement speed.

Exceeding capacity:

- Prevents picking up more items, or
- Applies a movement speed penalty

Exact limits are a balancing pass, not a design decision, and should be tuned once the core survival loop is playable. First confirmed balancing pass, 2026-09-23 (Mike, after playtesting InventoryManager.cs): encumbered above 30 kg (about 66 lbs) — movement speed penalty begins; hard cap 45 kg (about 99 lbs) — can't carry more. Sprinting is blocked while overloaded. These remain Inspector-tunable, not locked.

---

# Dropping Items

**Gap found 2026-10-03 (Mike), playtesting:** "i think we need to be sable to drop items if we are carrying too much or just don't need them." Nothing in the game let the player shed carried weight anywhere outside the primitive storage piles (Wood Pile, Rock Pile, Water Barrel, Food Cache, Storage Bin, Tool Rack — see Primitive_Storage_System.md), and none of those are a substitute for a general "I don't want to carry this right now" action — they require reaching that one specific container, and each only accepts its own item category.

**Resolved 2026-10-03 (Mike):** dropped items stay in the world as real pickup objects where they're dropped, rather than vanishing — the player can walk back and retrieve them later. This is a general, any-item, any-location action, distinct from and complementary to the primitive storage system, which stays reserved for deliberate offload of specific categories at a specific container.

**First-pass scope (Claude, not yet confirmed) — Claude Code should correct if this doesn't match:**

- Works from the Inventory screen (I key) on any carried item or stack. Same single-item vs. whole-stack split already established by the shared per-item transfer screen (Primitive_Storage_System.md) — drop one, or drop the whole stack.
- Dropped items appear a short distance in front of the player, not inside geometry, as a world object using the same pickup-and-prompt interaction already used for other ground items (felled-tree piles, foraged resources).
- No decay or despawn timer by default — a dropped item simply sits until picked back up. Nothing else in the project treats offloaded goods as destroyed, and an indefinite pickup matches the "stays in world" decision above. Claude Code should flag it if indefinite persistence causes a real problem once autosave/world-state is exercised with many dropped items lying around.
- Whether an equipped Tool can be dropped directly (unequipping it in the same action) or has to be unequipped first from the Inventory screen is an interaction-flow detail left to Claude Code's judgment, same propose-then-confirm pattern as everything else in this doc.

**Built and tested 2026-10-03 (Claude Code):** every inventory row now has a Drop button — click drops one, Shift-click drops the whole stack, matching the transfer screen's convention. Lands about 1.2 m in front of the player on the ground; if something solid is in the way the spot is pulled closer; dropping into water is refused with a message. Becomes a real `ItemPickup` with the usual "Pick up X (n)" prompt — a plain tinted cube for now, since no per-item drop models exist yet. No despawn timer — a dropped pile just sits there until picked up. Equipped Tools can be dropped directly with no need to unequip first; `InventoryManager` already unequips a Tool once the last copy leaves the pack, confirmed with the Axe.

**Beyond the proposal (Claude Code's own calls, not asked for but needed):** dropped piles are saved and restored with the world — a new `dropped` block in `world.json`, written by a new `DroppedItems` manager that creates itself at startup — without this, dropped items would have vanished on reload, breaking the "stays in the world" decision above. A dropped perishable keeps the day it was originally acquired rather than refreshing to the drop date (new `InventoryContainer.RemoveBatches` method). Dropping several singles of the same item in a row merges them into one pile instead of littering separate piles; dropping a different item nudges it sideways so the two piles don't overlap.

Tested in Play Mode by calling the drop code directly rather than clicking the real Inventory UI buttons, so the new row layout and status messages haven't been exercised in the actual game yet. That run covered a single drop, a merge into one pile of 20 arrows, dropping the equipped Axe, a full save/restore round trip, and picking everything back up — weight returned to 19.0 kg, same as before the drops. Mike's slot1 save was backed up before the run and restored after; all six save files matched the backup byte-for-byte.

Committed as `1de876f` (code only — the doc edits above were already in progress as this hand-off, so Claude Code left them out of that commit). The pile-overlap nudge was added after that commit and compiles clean, but **hasn't been run in Play Mode yet** — flagged as untested.

**Flagged (Claude Code):** no cap on dropped piles and no despawn timer, same as proposed — fine at normal scale, not tested with hundreds of piles on the ground at once. Other existing world pickups (felled-tree piles, foraged resources) still aren't saved/restored on reload — a pre-existing gap `ItemPickup.cs` already noted, not a new one introduced by this feature.

---

# Storage

## On-Person Inventory

Limited. What the player is actively carrying.

---

## Home Storage

Root Cellar, Storage Shed, Barn — see Building and Housing System documentation.

Larger capacity, not carried, tied to a specific structure.

---

# Inventory Screen (UI)

Confirmed 2026-09-25 (Mike). A dedicated screen, opened and closed with the I key.

Shows the player's on-person inventory — items carried, current weight measured against the Carrying Capacity thresholds above, and whether the player is Encumbered — and lets the player select which Tool is currently equipped (`InventoryManager`'s existing `EquippedTool`).

No buildable structures exist yet in Alpha 0.1 (Building_Housing_System.md is still a Design Draft with nothing built), so Home Storage transfer — moving items between on-person and a Root Cellar/Shed/Barn — isn't needed by this screen yet. `InventoryManager` already supports per-structure storage containers in code (`GetOrCreateStorage`), so wiring an existing container into this same screen should be a small follow-up once a structure actually exists to interact with, not a redesign.

This screen doesn't need to solve eating by itself — no food-producing systems (Foraging, Fishing) exist yet to put food items in the inventory in the first place — but it's the piece Claude Code flagged as missing in its Hydration/Hunger implementation log: Hunger can currently only be refilled with a debug key since there's no inventory UI to consume an item from. This closes that gap for whenever Foraging/Fishing land and real food items exist to select and eat.

Layout (grid vs. list), interaction (drag-and-drop vs. click-to-equip/use), and whether opening it pauses gameplay are Claude Code's call, same as the Minimap and compass were left open.

**As built (Claude Code, updated 2026-10-03):** a scrolling list, one row per kind of item, with columns Item | Qty | Weight and per-row buttons. Opening it puts the game in the Menu state (observed in Play Mode), which is what turns the number-key hotkeys off; whether the world clock itself stops while it's open wasn't checked. Clicking a Tool row equips it (or puts it away if equipped); clicking food or water eats/drinks one. Two per-row buttons were added on 2026-10-03 — see Dropping Items and Tool Hotkeys above/below for behavior:

- **Drop** on every row, at the right edge (click: one; Shift-click: the whole stack).
- **Key** on every Tool row that equips on click — a "Key N" or "Key —" button that cycles the Tool's hotkey slot. Sleeping Bag and Tent don't get one. It sits in the same spot as the Cook/Boil/Split button, which only appears on non-Tool rows, so the two never share a row.

Making room for Drop narrowed the Qty and Weight columns slightly and moved the Cook/Boil/Split button left. The hint text under "Equipped Tool" was shortened to fit the new instructions: "Click a tool to equip it, food or water to use one, the Sleeping Bag to sleep. Drop: Shift = stack. Point at a tool, press 1-9/0 for a hotkey." (plus the campfire line). Status messages from Drop and hotkey assignment go through `ToolStatus.Flash`, the same brief message line the Craft button uses, rather than a label on the screen itself. **Not verified:** that this line is actually visible while the Inventory screen is open — the Craft button already relies on it, but it was never looked at on screen, so if Drop or assignment feedback seems missing in play, that's the first place to look.

---

# Build/Craft Missing-Materials Notice

**Gap found 2026-10-03 (Mike):** "on the inventory / build screen, i think we need to pop open a notice or something that tells the player what they are lacking if they try to build something that they don't have all the resources to build it?" Build and Craft buttons across the Inventory screen already explain on hover what's needed or why a button's greyed out (first shipped with the Pouch/Bag — see Primitive_Storage_System.md), but hover-only feedback is easy to miss: a player who clicks a disabled button, or doesn't linger on it long enough to read the tooltip, gets no clear answer for why nothing happened.

**Resolved 2026-10-03 (Mike):** add an explicit on-screen notice that appears when the player actually attempts to Build or Craft something they're short on materials for, naming exactly what's missing — not just the passive hover tooltip.

**First-pass scope (Claude, not yet confirmed) — Claude Code should correct if this doesn't match:**

- Applies to every Build and Craft action in the Inventory screen — every primitive storage type, Small Cabin's site and its completion, the Hammer, the Pouch/Bag, every other craftable Tool — not a special case for any one of them.
- On an attempted Build/Craft that's short on materials, shows a brief notice naming exactly what's missing and how much (for example, "Need 4 more Logs, 2 more Stone") — reusing the same short on-screen message convention the game already uses elsewhere (`CanBuildPile`'s site-prep refusals, the hotkey system's "Not carrying the Axe"), rather than inventing a new UI element.
- Exact presentation — a toast near the button, a banner at the top of the screen, how long it stays up — is Claude Code's call, matching that existing short-message convention.
- The existing hover tooltip stays as-is; this adds an active notice on attempt, it doesn't replace the passive hover info.

**Playtested 2026-10-04 (Mike): the notice works, but "is small and not easy to read."** Behavior is confirmed; the presentation isn't. (The original small version was committed `a71f6a3`; the enlarged banner below is `b8dcd04`.)

**Readability fix, first-pass scope (Claude, not yet confirmed) — Claude Code should correct if this doesn't match:**

- **Larger text.** Meaningfully bigger than the surrounding Inventory row/hint text, so it reads at a glance without leaning into the screen — it's an error message the player needs to act on, not a footnote.
- **Strong contrast against its background**, ideally on its own solid panel or banner rather than loose text over the busy Inventory list — a dark panel with light text (or a warm warning colour with a readable outline) so it stands out regardless of what's behind it.
- **Placed where the eye already is** — near the Build/Craft button just clicked, or a banner across the top/centre of the Inventory screen — not tucked in a corner or the existing small hint line at the bottom ("Esc to close").
- **One missing material per line**, quantity first ("4 more Logs"), rather than a single run-on sentence — easier to scan when several are short.
- **Stays up long enough to read**, and until the next click or a few seconds after, rather than a brief flash. Exact duration is Claude Code's call.
- Same short-message convention and no new UI concept; this is a sizing/contrast/placement pass on what shipped, not a redesign.

**Built and tested 2026-10-04 (Claude Code), committed `b8dcd04`:** the notice is now a large banner across the top of the Inventory screen — a solid dark-red panel with an amber border and cream text. Checked in screenshots and by calling the button code directly, not with a real mouse click.

- **Heading:** bold and clearly larger, naming what was attempted, e.g. "Can't build Lean-To", followed by "You still need:".
- **One line per material, quantity first:** the Lean-To case shows "8 more Branches", "4 more Sticks" and "1 more Cordage" as three separate large lines. Cordage shows "1 more Sinew", then "or 3 Tall Grass, or 2 Cattail" (alternative sources). Cabin completion changes the sub-heading to "You still need, at the cabin site:". Refusals not about materials, such as "No clear ground in front of you", show as a single large line.
- **Duration:** stays up 6 seconds plus 1.5 per line (the Cordage case ran 9 seconds, the Lean-To about 10). Dismissed when the screen closes or when a later Build/Craft click succeeds; confirmed along with the refused-then-succeeded sequence. It doesn't block clicks.
- **Placement call (Claude Code's):** top banner rather than a panel beside the clicked button, since a panel near the buttons would cover the grid the player is about to click again. **Tradeoff:** the banner covers the top few rows of the item list while showing. The small hover text under the Build buttons is unchanged.
- **Beyond the proposal:** Drop and hotkey messages keep the small status line, but a refused drop or a full Lantern tank refill refusal now also uses the banner.
- **How it was built:** the banner splits the existing "Needs 4 more Logs, 2 more Stone." sentence into lines rather than reading structured data, so if someone later rewords those messages the split could stop matching and fall back to showing one line (still readable, just not split).
- **Not tested:** a real mouse click on the buttons. A screenshot-tool glitch sometimes returned the world instead of the screen, so Claude Code waited a moment before capturing. Mike's save had changed again, so a fresh backup was taken first and restored afterward, all six files matching.

---

# Tool Hotkeys

**Gap found 2026-10-03 (Mike):** "what about hot keys for frequently used items? like numbers 1 thru 0 can be assigned a tool from inventory?" Equipping a Tool today means opening the Inventory screen (I key) and selecting it there — nothing lets the player switch Tools without pausing to open that screen.

**Resolved 2026-10-03 (Mike):** add number-key hotkeys — 1 through 0, ten slots — each assignable to a specific Tool from the Inventory screen. Pressing an assigned number equips that Tool directly, without opening Inventory.

**First-pass scope (Claude, not yet confirmed) — Claude Code should correct if this doesn't match:**

- Ten slots, keys 1–9 and 0, each assignable to one Tool the player is carrying. Assigned from the Inventory screen — exact assignment gesture (hold the number while hovering/selecting a Tool row, a right-click menu, drag-to-slot, or similar) is Claude Code's call.
- Pressing an assigned slot's number sets that Tool as `EquippedTool`, the same effect as selecting it from Inventory — works any time the Inventory screen isn't open, no pause needed.
- Pressing the number for the Tool that's already equipped — toggles it back to unequipped, or does nothing — is Claude Code's call, same as every other interaction-flow detail in this doc.
- If a slot's assigned Tool isn't currently carried (dropped, stored, lost), pressing it should do nothing or show a brief "not carrying" message rather than error. Claude Code's call on which.
- Slot assignments should persist in the save the same way other player state does — losing the whole hotbar setup on every reload would defeat the point.
- Scope stays Tools only, matching what Mike asked for — not Consumables or other item categories. Can extend to other categories later if asked.

**Built and tested 2026-10-03 (Claude Code):** ten slots, keys 1–9 then 0; a Tool sits on at most one slot, and assigning it to a new key moves it off the old one (replacing whatever was already on the target key). Two ways to assign: each carried Tool's row in the Inventory screen has a "Key —" button that cycles 1 through 9, then 0, then off on each click; or point the mouse at a Tool row and press a number key to assign it directly. Pressing an assigned key equips that Tool, same as picking it in Inventory; pressing it again while that Tool is equipped puts it away — the toggle-off behavior was Claude Code's call, resolved as toggle. A Tool that's been dropped, stored, or lost shows a brief "Not carrying the Axe" when its key is pressed, but keeps its slot assignment so the key works again once the Tool's back (checked by dropping the Axe and picking it back up). An empty slot shows "Nothing on key N — assign a Tool from the Inventory screen (I)." Hotkeys only work while the world is live (not while Inventory or any other screen is open, and not while paused). Slot assignments save in the inventory block of the save file; old saves without hotkey data load with every slot empty.

Sleeping Bag and Tent are both Tools by category but aren't equippable (clicking one sleeps, or does nothing) — Claude Code left the hotkey button off both, keeping scope to Tools that actually equip, matching "Tools only" above. The Unity project's default input bindings already use keys 1 and 2 for Previous/Next — Claude Code read the number keys directly for hotkeys instead of adding new input actions, so there's no conflict.

Tested in Play Mode: assigning and moving slots, equip, toggle-off, a Tool that isn't carried, an empty slot, a save/reload round trip, and loading an old save with no hotkey data. Most of this was done by calling `InventoryManager`'s hotkey methods directly; the Inventory screen itself was opened in Play Mode and its Key buttons were clicked programmatically (button built on each Tool row, click cycles through slots and off), but nothing was clicked with a real mouse. **Not yet tested: real physical number-key presses** — Unity ignores simulated keyboard input when the window isn't focused, so only the underlying press-handling code was exercised directly; worth confirming the actual keys work once this is in hand. Also worth a glance once in-game: the new "Key —" button's layout on each row, and the hint text at the bottom of the screen (About three lines tall — check it doesn't overflow). Mike's slot1 save was backed up before testing and restored after; all six save files matched the backup byte-for-byte. Committed as `2fb66d2`.

---

# Spoilage Interaction

Raw food spoils whether carried or stored, per the Spoilage System defined in Core Survival System documentation.

Storage structures like the Root Cellar and Smokehouse slow or prevent spoilage; on-person inventory does not.

---

# Tool Durability

Future System

Not Alpha 0.1.

Tools may eventually degrade with use and require maintenance or repair.

---

# Economy Integration

Sellable items — Small Furs, Hides, Antlers, surplus produce, and similar — must be in Inventory or Home Storage to be sold.

Full sale detail is defined in Economy System documentation.

---

# Design Rules

1. Capacity should create real tradeoffs, not just be a number the player ignores.

2. Home storage should always outperform on-person carrying, to reward building infrastructure.

3. Inventory does not define what items do — that belongs to the system that produces them.

4. Exact capacity numbers are a balancing pass, not fixed by this document.

5. Nothing here should be added that isn't required to support the Alpha 0.1 systems already in scope.
