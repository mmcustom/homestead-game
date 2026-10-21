# Homestead
## Quantity Controls v0.1

Status: Design Draft (PROPOSED), written 2026-10-21. Nothing is built, and no code has been read yet.

# Purpose

Mike asked for a way to increase and decrease quantities when buying, selling, storing, or removing things from storages or campfires. Today (from the read-only passes so far) the Trading Post moves 1 per click, buys 10 and sells a whole stack with Shift-click. How storages and campfires move items is not yet known and is the subject of a Claude Code read-only pass.

# What Mike decided (2026-10-21)

| Question | Decision |
|---|---|
| Control style | An on-screen stepper plus keyboard shortcuts: each row gets a small  -  [ n ]  +  and Max control, a typed amount works, and the existing click and Shift shortcuts keep working. |
| Where | Everywhere items are moved by count: Trading Post buy and sell, storage containers (store and take out), campfire and cooking (put on and take off), and any other screen that moves items by count. |
| Default amount | Resets to 1 after each move (no remembered amount), so a big stack is never moved by accident. |

# Proposed behaviour (Claude's defaults, not yet confirmed)

- Stepper: minus and plus change the amount by 1; holding Shift changes it by 10; Max sets the largest allowed amount.
- Typed amount: the number can be typed; invalid text is ignored; the amount is clamped to the allowed range (at least 1, at most what is available).
- Limits come from the screen: for selling, how many the player has; for buying, how many the player can afford (and carry, if weight blocks it); for storing, how many the player has and the container has room for; for taking out, how many are in the container and the player can carry; for a campfire or pot, how many it accepts.
- Prices: on the Trading Post the row shows the total for the chosen amount, using the market's falling price (the quote function the market already has), so the player sees what the move is worth before committing. Buying shows the total cost.
- Shortcuts stay: click moves the chosen amount (1 by default); Shift-click keeps its present meaning until Mike chooses otherwise.
- Mouse and keyboard both work; the amount returns to 1 after each move or when the screen closes.
- One shared control component, so every screen looks and behaves the same.

# Open questions

1. Does Shift-click keep its present meaning (Trading Post sells the whole stack, buys 10) once the stepper exists, or should it become "use the Max"?
2. What happens when the player lacks weight capacity for the amount (take everything possible, or refuse)?
3. Should the Trading Post confirm a large sale (for example above a set value)? Today there is no confirmation.
4. Which screens exist that move items by count? (To be answered by the read-only pass.)

# Build plan (PROPOSED)

1. Claude Code read-only pass: every screen that moves items (Trading Post, storage containers, campfire and cooking, anything else): how each moves items today, what clicks and keys they use, and which code is shared.
2. Decisions from Mike on the open questions.
3. A shared quantity control (pure clamping and amount rules in the Economy or a similar engine-free assembly so they can be tested; a UI component in the main assembly), then each screen wired in turn, Trading Post first (it has the quote function).
4. Tests for the clamping rules; a playtest checklist; Mike plays before anything is committed.


---

# Claude Code read-only pass on quantity controls (2026-10-21)

Read only: nothing changed or created, Play Mode not run. Items marked UNCONFIRMED were not verified from code. The market commit was still waiting for Mike's OK when this ran.

## Every place items move by count (today)

| Where | How it moves today | Limits |
|---|---|---|
| Trading Post, Buy (TradingPostScreen, TradingPost) | Click buys 1, Shift-click buys 10 | Cash (price times count) and carry weight, through the pure `Money.BuyCount` |
| Trading Post, Sell | Click sells 1, Shift-click sells the whole stack, unit by unit at the market's falling price (`TradingPost.Sell(id, count)`) | Carried count and the market floor |
| Storage, Store (StorageTransferScreen, WoodPile.StoreOne) | Click stores 1, Shift-click the whole stack; "Store All Eligible" button | What the pile accepts, carried count, equipped tool excluded, litre room for barrels only |
| Storage, Take (WoodPile.TakeOne) | Click takes 1, Shift-click the whole stack; "Take Everything" and "Fill Bucket (best quality)" buttons | Count in the pile, carry weight (`SpaceFor`); a barrel by Bucket and Canteen litres |
| Campfire fuel (World/Campfire.cs, FireManager.AddFuel) | E adds 1 of the selected fuel, Shift+E adds as many as fit; a key cycles the fuel type. NO screen, an in-world interaction | The fire's fuel-hours cap (`PiecesThatFit` is already pure) |
| Campfire cooking and boiling (InventoryScreen, CampfireCooking.Enqueue) | A Cook button on an Inventory row queues 1, Shift queues the stack; clicking again cancels | Cookable items, a Cooking Pot for boiling, carried count, a lit fire nearby |
| Drop (InventoryScreen) | Click drops 1, Shift-click the stack | Carried count, a clear spot |
| Split firewood (AxeTool.EnqueueSplit) | Click queues 1, Shift queues all; click again cancels | Carried count, the Axe |
| Lantern refill (PortableLight.Refill) | Click pours 1 Lamp Oil, Shift as many as fit | Tank capacity, Lamp Oil carried |
| Craft (CraftingScreen) | An x1 / x5 / Max row beside the Craft button, multi-craft recipes only | Materials, capped at 99 |

Out of scope (not count-driven): building materials, placing structures, eating. Not read: trap baiting, pickups, structure refunds. There is no drag and drop anywhere. Removing fuel from a fire is NOT possible today (fuel becomes burn hours); the only "removal" at a campfire is cancelling a queued cook.

## Shared code and layout

- Shared: the `GameScreen` base class and `UiKit` factories (Button, Text, Rect, Image, ScrollList, Clear, Height, Input). `UiKit.Input` builds a legacy InputField (Finances, LandPurchasePanel and Journal already use one). There is no common row or slot component: four separate row builders (TradingPostScreen.Row, StorageTransferScreen.Row, InventoryScreen.AddRow, Crafting's list), ShiftHeld copied three times, no prefabs.
- Every screen rebuilds all its rows on each Refresh, so a row's amount cannot live in the row; each screen needs a dictionary of amounts keyed by item, reset to 1 after a move and cleared in OnHide.
- The Trading Post Sell row is already full in a column about 490 px wide. A stepper of about 170 px does not fit: it needs a taller two-line row or a narrower name and price area (the biggest layout risk, unchecked without running the screen).

## What can be pure code

A small `QuantityRules` class in the Economy assembly (integers only): Clamp (at least 1, at most max, 0 when max is 0), Step (plus or minus 1 or 10), TryParse (digits only, strip leading zeros, cap length, ignore invalid text), MaxOf (minimum of available, affordable, carryable, room). About 25 tests. Weight maths stays in the main assembly (`InventoryContainer.SpaceFor`, `MaxStore` and `MaxTake` to be added on WoodPile, fire fuel hours); each screen passes its own limit in. Trading Post totals: sell uses `MarketManager.Quote(itemId, n).totalCents` (exact, since the clock is stopped in the Menu state), buy uses `Money.Mul(price, n)` and `Money.BuyCount`.

## Risks

- Shift-click already means all (or 10 for Buy); the stepper uses Shift for plus or minus 10. They are different buttons, so no mechanical clash; the question is what Shift-click on the action button does.
- Typing: a legacy InputField stays focused after a click and blocks the T, I, M hotkeys until focus is released; `InventoryScreen.PollHotkeyAssign` reads digit keys while the pointer is over a tool row, so typing "5" would assign hotkey 5 (needs a `!GameScreens.IsTyping` guard); whether Esc while typing closes the screen is UNCONFIRMED.
- Mouse-only play cannot type or use Shift for 10, so the minus, plus and Max buttons must be enough; there is no hold-to-repeat on a Button. Every button already has Navigation.None, so controllers cannot use these screens today.
- Cook and Split are queues that run over time and cancel on a second click: "Cook n" needs a separate Cancel control or the cancel toggle and the stepper fight. Moves take the oldest batch first (the player cannot choose wet or dry wood). Water is in litres. No existing test touches these screens; the 289 tests should be unaffected. No saves are involved.

## Size and order

About 3 new code files (QuantityRules, QuantityStepper, tests of about 25) plus edits to TradingPostScreen, StorageTransferScreen, WoodPile (amount overloads, old bool versions kept as wrappers), InventoryScreen, PortableLight (count overload), CraftingScreen (optional consolidation) and UiKit; roughly 600 to 900 lines. Build order: QuantityRules and tests; the shared QuantityStepper; Trading Post with live totals (then a playtest); Storage; Inventory actions (drop, split, cook, lantern); Crafting; campfire fuel last, once its UI is decided.


---

# Decisions after the read-only pass (Mike, 2026-10-21)

| Question | Decision |
|---|---|
| Shift-click on Buy, Sell, Store and Take | KEEP the old meaning. Plain click moves the amount shown in the stepper (1 by default); Shift-click still means the whole stack, or 10 when buying. |
| Campfire | LEAVE THE FIRE AS IT IS: no fuel panel. E and Shift+E keep adding fuel. The stepper applies to cooking and boiling (the Cook button in the Inventory) only. |
| Row layout | TALLER TWO-LINE ROWS: line one is the name, trend and price; line two is the stepper, the live total and the action button. Same on every screen. |

Claude's defaults (accepted unless Mike objects): when weight or room limits the amount, the maximum is clamped to what fits (nothing is refused); no confirmation for large sales (the live total is shown before the click); "Cook n" gets a separate Cancel button so the stepper and the cancel toggle do not fight; amounts for water are in litres; Esc while typing in the amount box only ends the typing; Crafting's x1 / x5 / Max row is replaced by the stepper in a later step.

## Build in two parts

- Part 1: `QuantityRules` and tests, the shared stepper, the Trading Post (buy and sell) with live totals, and the Storage screen (store and take). Then Mike plays a checklist.
- Part 2: the Inventory actions (drop, split firewood, cook and boil, lantern refill) and Crafting. Then Mike plays a second checklist.
- Not built: a campfire fuel panel (decided against), building materials, placing structures, eating.


---

## Part 1 build report (Trading Post and Storage)

Status: built, tests pass, awaiting Mike's playtest. Not committed.

What was built:
- `QuantityRules` (Economy, engine-free): Clamp, Step, TryParse, MaxOf, ParseAndClamp. MaxAmount 999,999. 46 new test cases; 335 edit-mode tests pass in total.
- `QuantityStepper` and `QuantityRowUi`: two-line rows, 84 px. Line one: name, trend, price (or reason the row is unavailable). Line two: stepper (minus, amount box, plus, Max), live total, action button.
- Trading Post buy and sell rows and Storage store/take rows use the stepper. Plain click moves the stepper amount. Shift-click keeps the old meaning (sell whole stack, buy 10, store or take whole stack). Shift on plus/minus steps by 10.
- The amount resets to 1 after a move (also after a failed move) and clears when the screen hides.
- `WoodPile` gained Store(id, count), Take(id, count), MaxStore(id), MaxTake(id); StoreOne and TakeOne are wrappers.
- Typing: digits only; click-to-type clears the box and shows a greyed placeholder; values over the max clamp to the max; 0 becomes 1. Esc while typing ends typing only (one-frame flag on GameScreens, checked in PlayerController). Hotkey assignment on the Inventory screen is blocked while typing.
- Storage total reads "n of max". Weight-limited rows grey out with "too heavy to carry more". Heading hints read "Shift-click: 10" and "Shift-click: the lot".

Known gaps: real key presses and mouse clicks not tested by Claude Code; water and barrel rows not tested in Play Mode; a screen refresh while typing loses the typed text.

Next: Mike plays Quantity_Controls_Part1_Playtest_Checklist.md, then commit. Part 2 follows (Inventory drop, split firewood, cook/boil with separate Cancel, lantern refill, Crafting).


---

## Part 1 status

Played 39 of 39 by Mike. Committed and pushed as b306150 (13 files). Part 2 is next.
