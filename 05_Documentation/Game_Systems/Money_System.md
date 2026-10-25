# Homestead
## Money System v0.1

Status: Design Draft (nothing built). Written 2026-10-20 from Mike's vision decisions of the same day. Every rule marked DECIDED is Mike's. Everything marked PROPOSED is Claude's first pass and is not yet confirmed. No numbers here are final except the ones Mike set.

Related: `00_Project_Management\GDD\Homestead_GDD_v2.0.md` (Land Purchase and Debt section), the Homestead Vision and Design Pillars doc, `Economy_System.md`, `Difficulty_System.md`, `Season_System.md`, `Structures_Roadmap.md` (step 1b).

---

# Purpose

Finances dictate most aspects of life, and the game plays on that. The player starts by buying land with borrowed money and owes a payment on a schedule. The land payment is the constant pressure that makes self-sufficiency matter: every dollar the land does not have to buy is a dollar that goes toward the debt, and the game ends in defeat if the player falls too far behind.

This system adds the land loan, the payments, the late fee and the loss, the seasonal account statement, and a ledger that records where money comes from and goes. It sits on top of the cash and Trading Post that already exist. It does not add any structure or item.

---

# What Mike decided (2026-10-20)

| Rule | Decision |
|---|---|
| Start | The player buys land with borrowed money. A payment is due every 28 days, at the monthly-sized amount (Mike's pick, 2026-10-20; see Open Decision 1). |
| Land price | Depends on the size of the property and its location. Real costs are researched to reflect actual markets. The starting region is Mike's own (Bethel, Ohio area). Numbers not yet set. |
| Saved money at the start | Scaled by difficulty tier (Mike, 2026-10-20): Settler gets 20% of the land price, Homesteader gets 40%, and Pioneer gets none (the zero-savings hard option). Every tier is capped at $30,000 (Mike, 2026-10-20), so Settler gets $12,000, $28,000 and $30,000 on the 5, 10 and 40 acre parcels, and Homesteader gets $24,000, $30,000 and $30,000. The player chooses how to split the savings between a down payment and a cash reserve. |
| Loan | Fixed rate, 8% a year, 15 years (180 payments, one every 28 days). Loan amount is land price minus the down payment. The payment is fixed and covers interest and principal. Chosen from typical land loan figures (Mossy Oak Properties, July 2026: raw land about 8 to 10% and 5 to 15 years). |
| Extra payments | Allowed at any time and encouraged. They go to principal, the required payment stays the same, there is no prepayment penalty, and an extra payment never covers a future payment. |
| Late fee | A payment that is 10 days late gets a $75 late fee, added on top of the regular payment, and counts as missed. |
| Loss | Three missed payments is a fail state: the game is lost. Catching up clears a missed payment (it stays missed until paid with its late fee), so the loss is three open at once (Mike, 2026-10-20). The game offers to restart or load a save, with a banner or text telling the player that mother nature beat them. |
| Account statement | Shown at the end of every season. Positive figures in black, negative figures in red. It shows income and expenses plus numbers compiled over time, as the budget for the property and household. |
| Modern items | Can be bought with money, crafted, or both. |
| Timing | Money enters early. A bare version of the land payment and balance comes in well before the income systems. The first payment falls due at the end of the first year (day 112), then every 28 days (Mike, 2026-10-20, after the read-only pass). |
| Balance | The early game is balanced three ways at once (Mike, 2026-10-20): the first payment is delayed to day 112, the player starts on a smaller and cheaper starter parcel, and early income is raised. Sizes, prices and income numbers are not yet set. |
| Partial payments | Allowed. Any amount can be paid, applied to the late fee first, then interest, then principal. A payment short of the full amount stays open. Cash can never go negative (Mike, 2026-10-20). |
| After a loss | Continue loads the last safe autosave. The loss never overwrites the autosave. The loss screen offers Restart or Load Save (Mike, 2026-10-20). |

---

# What already exists (from the docs, not re-checked in code)

- Cash. Money is saved with the game. F12 (editor and development builds only) gives a food-gathering kit plus $50 for testing. (Difficulty_System.md)
- The Trading Post, a menu tab opened with T: sell gathered goods, buy tools and materials. Buy prices and the 40% sell-back rule are in Difficulty_System.md. (Difficulty_System.md)
- Difficulty tiers chosen at New Game and locked for the save: Homesteader (starts with $50), Settler ($0), Pioneer ($0). (Difficulty_System.md)
- A calendar of 4 seasons of 28 days each (a 112-day year), 30 real minutes per game day. A new game starts at 6:00 AM on Spring day 1. There are no months. (Season_System.md)
- Economy_System.md lists "Debt" as a future system and Property Development, Livestock and Equipment as money sinks. The land loan is the first real debt.

A Claude Code read-only pass must confirm all of this against the code before anything is built (see the question list near the end).

---

# Open decisions for Mike (found while writing this doc)

**1. What is a "month" in this game? RESOLVED 2026-10-20 (Mike): option C, a payment every 28 days at the monthly-sized amount.**

What this means in practice: a game "month" is 28 days (one season), the interest is 8% a year spread over 12 payments (0.6667% per payment), and the loan has 180 payments. That is 180 x 28 = 5,040 game days, about 45 game years or about 2,520 real hours at 30 minutes a day. Only four payments fall in a 112-day game year, so a game year costs the player about $6,116 on the $160,000 example, against about $18,348 for a real year. Without extra payments the loan will not realistically be finished in one game. The extra payments Mike wants encouraged are what let a player finish, and the 10-day late window fits inside a 28-day period. Total interest over the full 180 payments is about $115,227 on the $160,000 example and about $144,034 on the $200,000 example. Mike can change the term or rate later if a loan this long is not what he wants.

The original options, kept for the record (Claude's mistake corrected: option C makes the debt lighter, not heavier):

The calendar has seasons of 28 days and no months. A real 12-payments-a-year schedule does not fit a 112-day year (12 payments would fall every 9.3 days, which is shorter than the 10-day late window). Options:

- **A. One payment per season, every 28 days (Claude recommends).** The payment lines up with the seasonal account statement, the 10-day late window fits inside the period, and each payment equals three real monthly payments. At 8% a year over 15 game years (60 payments) the numbers below apply. The vision's word "monthly" would become "every season" in the player's wording (or the game calls a 28-day season a "month", but that muddles Spring, Summer, Fall and Winter).
- **B. Add months to the calendar.** Seven 16-day months, or any split. This is a larger change to the Season and Time systems and every doc that counts days.
- **C. Pay every 28 days but call it monthly and keep 12-payment math.** This makes the debt about three times lighter per game year than 8% implies, because only four payments fall in a 112-day year. Claude marked it not recommended, and Mike chose it. CHOSEN.

Option A numbers, superseded by Mike's choice of C and kept only for the record. Under A, with the $200,000 land example (illustration only, not a price decision): savings are $40,000. All of it down: loan $160,000, payment about $4,603 each season, total interest about $116,172. None of it down: loan $200,000, payment about $5,754 each season, total interest about $145,216. The real monthly equivalents are $1,529 and $1,911 (the figures in the GDD). A 15-game-year loan is about 840 real hours at 30 minutes a day, so extra payments are what let a player finish. That fits the "encouraged" rule.

**2. How do the 20% savings and the existing difficulty tiers fit together? RESOLVED 2026-10-20 (Mike): scale the savings by tier.** Settler gets 20% of the land price, Pioneer gets none, and Homesteader gets 40%, with every tier capped at $30,000 (set 2026-10-20). The old starting cash ($50, $0, $0) is retired; that part is Claude's assumption and was not asked. The original question follows for the record.

Difficulty_System.md already gives starting cash by tier: Homesteader $50, Settler $0, Pioneer $0. The new rule gives every player savings of 20% of the land price, with a zero-savings hard option. PROPOSED: Homesteader and Settler start with the 20% savings (split between down payment and cash reserve), Pioneer is the zero-savings hard option (loan fully financed, almost no cash), and the old $50 and $0 starting cash are retired. This also needs Mike's yes. If the player's cash reserve is large (20% of a $200,000 parcel is $40,000), Trading Post prices of $4 to $60 become trivial, so the Trading Post prices or the land price may need to be rebalanced together.

**3. Three missed payments: in a row, or in total? RESOLVED 2026-10-20 (Mike): catching up clears a missed payment.** A missed payment stays open until it is paid with its late fee, and three open at once is the loss. The original question follows for the record.

Mike said three missed payments is a loss. It is not decided whether a caught-up payment clears the count (three in a row), or whether the count only grows (three in the whole game). PROPOSED: a missed payment stays on the count until it is paid off together with its late fee, so a player who catches up recovers, and three unpaid missed payments at the same time is the loss.

**4. How is a payment paid?**

PROPOSED: a payment becomes due on its date. The player pays it from a Finances tab (the same window as the Trading Post, no physical bank). If it is still unpaid 10 days after the due date, the $75 late fee is added and the payment counts as missed. A "pay automatically when cash allows" switch is a possible later addition.

**5. Other items not decided:** land price numbers, which parcel or location the player gets, the exact contents and layout of the account statement, whether the statement can be reopened between seasons, and whether taxes and insurance exist later (not in this version).

---

# How it works (PROPOSED, written for the payment schedule Mike chose, option C above)

## New game

1. The player picks a difficulty (existing). The game shows the land: size, location (starting region only for now) and price.
2. Savings are set by the difficulty tier: Settler 20% of the price, Homesteader 40%, Pioneer none, every tier capped at $30,000. The player splits the savings between a down payment and a cash reserve.
3. The game shows the loan summary: amount, 8% a year, 180 payments, one every 28 days, and the total interest if no extra payments are made. The player confirms.
4. The game starts. The first payment falls due at a set point (still to be set; see Open Decision 5).

## The loan (formulas)

Let L be the loan amount, r the interest rate per payment period, and n the number of payments.

```latex
r = 0.08 / 12 \approx 0.006667, \quad n = 180
```

```latex
\text{payment} = L \cdot \frac{r}{1 - (1 + r)^{-n}}
```

- Each period, interest is balance times r, and the rest of the payment reduces principal.
- An extra payment reduces principal immediately. The payment amount does not change, so the loan finishes sooner. The payoff date is recomputed.
- The last payment is smaller if the balance runs out early. When the balance reaches zero the debt is paid off.
- Money is kept in whole cents internally and shown with cents.

## Late fee and loss

- Each payment has a due date. If the payment is still unpaid 10 days after it, the $75 late fee is added to what is owed, and the payment counts as missed.
- A missed payment stays missed until the payment and its late fee are paid (Mike, Open Decision 3).
- When three missed payments are open at once, the game is lost.
- The loss screen shows a banner or text telling the player that mother nature beat them, and offers Restart or Load Save.
- A warning is shown when a payment is nearly due, and another after the first and second missed payments, so a loss never comes without notice.

## Seasonal account statement

Shown at the end of every season (a payment falls due every 28 days, the same length as a season, so the two stay in step if the first payment is placed on a season boundary). Black for positive, red for negative. It shows:

- Income for the season by source (Trading Post sales by category, and later livestock, produce, lumber and so on).
- Expenses for the season by category (loan payments, late fees, extra payments, Trading Post purchases, later livestock feed, equipment and building costs).
- Net for the season.
- Numbers compiled over time, as the budget for the property and household: totals by category since the start, season by season, with the trend.
- SUGGESTED, not decided: cash on hand, loan balance, interest paid this season, interest saved by extra payments, and the projected payoff date.

The statement is built from the ledger below, so nothing is entered twice.

## The ledger

A list of money events. Each record has the game day, an amount (positive in, negative out), a category, and a short label. Every place that moves money writes one record: the Trading Post (buy and sell), the loan (payment, extra payment, late fee, down payment) and, later, anything else that earns or spends. The seasonal totals and the budget view are computed from the ledger.

Categories to start with: Land payment, Extra payment, Late fee, Down payment, Trading Post purchase, Trading Post sale. More are added as systems that earn or spend money are built.

## Saving

The ledger, the loan (original amount, rate, term, balance, next due date, missed count, late fees owed), and the season's running totals are saved with the game. They are new save fields, so an old save has no loan. PROPOSED: an old save loads exactly as it does today with no loan and no statement, and the money system is simply off for it (no forced debt on an existing game).

---

# Build plan (PROPOSED, nothing started)

Same pattern as every other system: a Claude Code read-only pass first, Mike's answers to the open decisions, then phases, each tested and played by Mike before the next.

- **Land prices decided 2026-10-20:** the player chooses a starter parcel at a new game: 5 acres $60,000, 10 acres $140,000, or 40 acres $450,000 (real Bethel, Ohio area numbers, rounded; see the GDD "Land Price" section for the table and sources). Payments at 8%, 180 payments, every 28 days: 5 acres about $459 with 20% down or $573 with none; 10 acres about $1,070 or $1,338; 40 acres about $3,440 or $4,300. Phase A therefore needs a parcel choice on the new game screen (three entries), and the early-income rebalance must cover at least the 5-acre payment. **Decision 2026-10-25 (Mike, see Early_Income_Design.md question 10):** the 10-acre and 40-acre parcels are shown locked at New Game with a reason ("Needs more income than the early game offers. Unlocks later."); only 5 acres can be picked. Per game day the payments are about $37.50 to $47.80 (10 acres) and about $143 to $154 (40 acres) against about $12 to $20 for 5 acres. Editor and development builds keep a way to pick a locked parcel so the 10-acre measuring sessions can still run. **Built and committed 2026-10-25 as `ad2c226`** (LandRules.IsAvailable, dimmed locked cards, Shift-click dev unlock in the Editor and development builds, 655 edit-mode tests, playtested 18/18). Claude Code's read-only check found that the parcel choice does not change the playable land: acres only feed LoanState.acres (shown on the Finances screen), so a bigger parcel currently buys only a bigger loan. When an unlock rule is designed, the larger parcels will need a real benefit (for example more usable land). Still open: Homesteader savings percentage, early income and Trading Post numbers, statement layout, and whether the old $50/$0 starting cash is retired.
- **Early income decided 2026-10-20 (Mike):** the target is about $25 a game day from wild goods by day 112 for a typical player who is also surviving (comfortable: about 1.5 times the 5-acre Settler payment of about $459 per 28 days). It is reached by raising sell prices of goods already in the game and adding a few new sellable goods. The real numbers come from a Claude Code read-only pass (prices, yields, time per action, starting kits, must-buy items); the new price table and new goods list are still to be proposed.
- **Phase A, loan and payments:** land purchase screen with the three-parcel choice, savings split, the loan, due dates, paying, extra payments, late fee, missed count, warnings. Edit-mode tests for the formulas and the missed-payment rules.
- **Phase B, ledger and account statement:** the ledger, wiring the Trading Post into it, the seasonal statement screen with black and red.
- **Phase C, loss:** the loss screen, Restart and Load Save, and interaction with the slot panel and autosave.
- **Phase D, difficulty and rebalance:** fold the 20% savings and zero-savings option into the difficulty tiers, rebalance Trading Post prices against the cash reserve (land prices for the Bethel, Ohio area are already set, see above).

---

# Questions for the Claude Code read-only pass

1. How is money stored today (type, whole dollars or cents), saved and shown? Where does every place that adds or spends money live (Trading Post, F12)?
2. What does TimeManager expose: day number, season, a day-ended or season-ended event? Can it raise an event at a chosen day, and does it survive save and load?
3. How is the difficulty tier stored and applied at New Game, and where would a land purchase step fit in the New Game flow?
4. How would a Finances tab fit next to the Trading Post (the T window)?
5. How does a loss (a fail state) fit with the existing death or collapse handling, the pause menu, the slot panel and autosave? Is there any existing "game over" path?
6. How are saves versioned, and how would an old save load with the money system off?
7. Any place in the code that assumes starting cash is $0 or $50 that would change?

---

# Edit-mode tests to ask for

- The payment formula against known loans (the $160,000 and $200,000 examples at about 0.6667% a period and 180 payments: about $1,529.04 and $1,911.30 a payment).
- Interest and principal split each period; the balance reaches zero on the last payment.
- Extra payments shorten the loan and leave the payment unchanged; the last payment is smaller.
- The 10-day late rule: not late on day 10, late after it; the $75 fee is added once per payment.
- Missed-payment counting and recovery (per Open Decision 3), and the loss at three open missed payments.
- Down payment and reserve split: they add up to the savings; zero savings gives a fully financed loan.
- Save and load round trip; an old save loads with no loan.

---

# Not in this version

Property tax, insurance, interest rate changes, refinancing, other loans (Economy_System.md's "borrow for wells, livestock, buildings" idea), multiple regions and land selection, a physical bank or NPC, and auto-pay. All can be added later without changing the loan rules above.

---

# Cross-references

- GDD v2.0, Land Purchase and Debt (loan terms, late fee, loss, starting money, account statement).
- Homestead Vision and Design Pillars (pillars Money Shapes Every Decision, Modern Tools Are Earned).
- Economy_System.md (revenue, sinks, the Debt placeholder). Its Design Rules 3 and 5 ("never feel forced to sell everything", "wealth is not the victory condition") still stand; the land payment is what makes selling surplus necessary, never selling reserves.
- Difficulty_System.md (starting cash by tier, the Trading Post).
- Season_System.md (28-day seasons, 112-day year).
- Structures_Roadmap.md (step 1b).

---

# Read-only pass results (Claude Code, 2026-10-20)

Claude Code read the code and changed nothing. Everything below comes from its report. Parts of the pasted report were garbled where the text overlapped, so a few lines (marked) are Claude's best reading and should be rechecked in the next pass.

## What the code does today

- **Money:** `TradingPost.money` is a plain whole-dollar int (Managers/TradingPost.cs), read-only to others, with a `MoneyChanged` event. It is saved by TradingPost as `TradingPostSaveData { int money }` under player/money, and shown only in TradingPostScreen as "$" plus the raw integer (no separators, no cents). It changes only through `Buy` (integer division for what is affordable), `Sell`, and `ResetMoney(amount)` (clamped to 0 or more). `GameManager.StartNewGame` calls `ResetMoney(DifficultyManager.StartingCash)`, `LoadSlotRoutine` calls `ResetMoney(0)` before every load, and F12 (`DebugAddMoney(50)`, editor and development builds only) adds $50 and a food-gathering kit. The 8 existing tests do not cover money.
- **Time:** `TimeManager` exposes TotalDays, DayOfSeason, CurrentSeason, Year, DaysPerSeason, TotalHours and the events DayChanged, SeasonChanged, YearChanged, HourChanged and PhaseChanged. Every midnight crossed fires DayChanged once, even in a long skip, because sleep, collapse and debug skips all go through AdvanceMinutes. A new game starts at totalDays 0, 06:00 on Spring 1. TotalDays reaches 28 at 00:00 on Summer 1, about 27.75 game days (about 13.9 real hours) after the start. There is no scheduler. A due day stored as an int and compared with TotalDays survives save and load, but RestoreState fires no events, so checks must be state-based (is today at or past the due day), not only event-based.
- **Difficulty and New Game:** `DifficultyManager` holds the tier (saved as player/difficulty; old saves load as Settler). Starting cash comes only from `DifficultyManager.StartingCash` (Homesteader $50, otherwise $0), read only by `StartNewGame`. The flow is difficulty and weapon panel, then Start, then SlotPanel in NewGame mode, which calls `GameManager.StartNewGame(tier, weapon, slot)`. A land panel fits between Start and the slot picker.
- **Screens:** `GameScreens` hosts the T window as tabs (a Kind enum, ActionNames, KeyHints, names and widths, CreateScreen<T>). A Finances tab needs a `GameScreen` subclass and a new Player/Finances input action. Six tabs already take about 1,070 of 1,500 px, so a seventh needs trimmed tab widths. UiKit (cream text on a dark panel, ScrollList, ScreenBanner) can be reused.
- **Loss:** there is no game over and no death. Health 0 raises `SurvivalManager.Collapsed` and `SleepManager.CollapseRoutine` fades out, skips hours and wakes the player weakened ("death is still TBD" in a code comment). A money loss needs a new game state (such as Lost) and an explicit timeScale of 0.
- **Saves:** `SaveVersion = 1` in meta.json only logs a warning on mismatch. Each system owns an independent section, and a missing section leaves that system at its reset value, so an old save loads with the money system off if MoneyManager resets to "no loan". The legacy player/money int must still load as dollars.

## Problems found

- **Autosave order:** SaveManager autosaves in its own DayChanged handler, which runs before later subscribers (PondManager, WeatherManager). Money logic that runs after it is missing from that autosave. Money processing must run first, or be idempotent by day.
- **Autosave and loss:** ReturnToMainMenu and OnApplicationQuit autosave whenever InGame is true. A lost game must not overwrite the autosave, and the autosave taken on the loss day needs a rule (a lost flag, or the loss check running first and suppressing it), or Continue could load into a lost game or replay the day before.
- **Sleep:** time keeps running during a sleep skip, so a payment can fall due or a loss can trigger mid-sleep. A banner has to wait for the wake.
- **Restart:** the weapon choice is not stored after New Game, so Restart would have to remember it. Loading a save from a non-InGame state inside the World scene has not been exercised and needs a test.
- **Pause menu and T window:** must be blocked or replaced while Lost.
- **First statement:** the first season is 27.75 days, and a first payment on day 28 lands in the same DayChanged and SeasonChanged moment as the first statement, so the order must be fixed: payments first, then the statement.
- **Statement colours:** black text on positive figures will not read on the dark UI theme. The statement needs its own light, paper-style panel, or a theme decision.
- **Cents and size:** store cents as a long and format with invariant-culture N2 (JsonUtility handles long, not decimal). Compute interest in double, round to cents each period, and absorb the residue in the last payment. A per-click ledger could reach tens of thousands of rows, so aggregate by day and category or roll old seasons into totals.
- **Tunables:** rate, term, fee, grace days and the loss count should be fields on the MoneyManager component, not a nested class.
- **Text to update when the $50 and $0 are retired:** `DifficultyManager.Blurb` ("plus $50", "no money"), the class comment in DifficultyManager, and Difficulty_System.md.

## The biggest risk: the game is unwinnable until income exists

The payment is about $1,529 (a $160,000 loan) to $1,911 (a $200,000 loan) every 28 days, and a day of gathering is worth a few dollars at the Trading Post.

- **Pioneer:** $0 savings and a fully financed loan. The first payment falls due on day 28, the first miss counts on day 38, and the third open miss ends the game on day 94. Phase A alone makes Pioneer a guaranteed loss.
- **Settler and Homesteader:** their reserves cover roughly 6 or more game years at four payments a year, but nothing refills the reserve.
- **Economy_System.md Rules 3 and 5** ("never feel forced to sell everything", "wealth is not the victory condition") conflict with a payment this large against income this small.

Phase A needs the land price numbers and either a stub income or a switch that turns the loss off, or it cannot be played honestly. Claude Code also suggests pulling part of Phase D (income and Trading Post price rebalance) forward.

## Rules missing from this doc (open decisions)

1. **"10 days late":** suggested rule: due day D, late when TotalDays is at or past D + 10, and test the boundary.
2. **Partial payments:** allowed or not, and the order a payment is applied in (fee, then interest, then principal)?
3. **Open missed payments:** does an extra payment pay them first?
4. **Insufficient cash:** can the player pay when short? Cash cannot go negative today, and overdraft is not mentioned.
5. **Payoff:** what happens when the loan reaches zero (a win state, a message, nothing)?
6. **Land step:** minimum down payment, and whether the cash reserve may be 0. The first payment date, the Homesteader percentage and the land price are unset.
7. **Loss wording:** "mother nature beat them" was Mike's wording, but it does not fit a debt loss. Kept as Mike wrote it unless he changes it.
8. **Loss and Continue:** what a lost game does to Continue and to the autosave.

## Recommended build order (Claude Code)

1. Foundation: move money to cents (a long), keeping the old dollar save readable. Build the pure loan math (payment, schedule, extra payments, fee and missed rules) in an engine-free asmdef like SlotRules, and the small ledger beside it.
2. MoneyManager: a day-driven manager with save and load, off for old saves, tunables on the component, hooked into the day handling ahead of the autosave.
3. New Game land panel with the savings split.
4. Finances tab: pay, extra pay, warnings.
5. Phase B, the statement.
6. Phase C, the loss.
7. Phase D, the rebalance (part of it pulled forward).

**Phase A size (estimate):** about 4 to 5 new files (loan math, ledger, MoneyManager, land panel, Finances screen) plus 1 asmdef; about 7 to 8 edited files (TradingPost, TradingPostScreen, GameManager, MainMenuController, DifficultyManager, GameScreens, TimeDebugControls, SaveManager); the input actions file and the Bootstrap scene prefab; about 25 to 30 edit-mode tests.

---

# Decisions after the read-only pass (Mike, 2026-10-20)

Mike chose, from options Claude offered:

- **Balance: all three fixes.** (1) Delay the first payment. (2) Smaller, cheaper starter parcels, so the payment is much lower than the $1,529 to $1,911 in the examples; those examples were for a $200,000 parcel and are now illustration only. (3) Raise early income (Trading Post sell prices and early money sources, so a day of work is worth tens of dollars, not a few). Sizes, prices and income numbers are NOT set, and this part of Phase D is pulled forward to run with Phase A.
- **First payment:** end of the first year, day 112 (TotalDays 112 is Spring 1 of Year 2 at 00:00), then every 28 days. With no payments at all, a miss is counted 10 days after each due day: days 122, 150 and 178, so the earliest possible loss is day 178.
- **Partial payments:** allowed, applied to the late fee, then interest, then principal. A payment short of the full amount stays open and still goes missed 10 days after its due day. Cash can never go negative, so a player short of cash simply pays less.
- **After a loss:** Continue loads the last safe autosave. A loss never overwrites the autosave (the loss check runs before the autosave that day, or suppresses it). The loss screen offers Restart or Load Save. Restart must remember the weapon choice, which the game does not store today.

## Claude's defaults for the remaining gaps (PROPOSED, not asked)

- **"10 days late":** due day D, late when TotalDays is at or past D + 10, with the boundary covered by a test.
- **Extra payments and missed payments:** an extra payment first clears open missed payments and their late fees, then goes to principal.
- **Payoff:** a message that the land is paid off, with the game continuing (no win state yet).
- **Land step:** no minimum down payment, and the cash reserve may be 0 (that is how Pioneer starts).
- **Loss wording:** kept as Mike wrote it ("mother nature beat them").
- **Processing order each day:** money first (payments, fees, missed count, loss), then the autosave, then the seasonal statement on SeasonChanged.
- **Statement panel:** a light, paper-style panel so black and red read correctly (the UI theme is dark).

## Still open

Homesteader savings percentage; starter parcel size and price per tier; real land prices for the Bethel, Ohio area; the early income and Trading Post price changes; statement layout; whether to retire the old $50 and $0 starting cash (assumed yes). Next step: update Phase A in the build plan to include the early income balance, then a Claude Code build brief once Mike confirms the defaults above.


---

# Homesteader savings set, with one open issue (2026-10-20)

Mike set Homesteader savings at 40% of the land price, capped at $30,000. Settler stays at 20% and Pioneer at none.

| Parcel | Settler (20%, uncapped) | Homesteader (40%, cap $30,000) | Pioneer |
|---|---|---|---|
| 5 acres, $60,000 | $12,000 | $24,000 | $0 |
| 10 acres, $140,000 | $28,000 | $30,000 (cap, 40% would be $56,000) | $0 |
| 40 acres, $450,000 | $90,000 | $30,000 (cap, 40% would be $180,000) | $0 |

OPEN: with the cap on Homesteader only, Settler would start with more cash than Homesteader on the 40-acre parcel ($90,000 against $30,000), and nearly the same on the 10-acre parcel. Options for Mike: apply the $30,000 cap to every tier (Settler would then get $12,000, $28,000 and $30,000); cap Settler lower than Homesteader; or leave it and accept that the 40-acre parcel is easier on Settler. Claude recommends capping every tier at the same $30,000 so the order Homesteader, Settler, Pioneer always holds. Not yet decided.


RESOLVED (Mike, 2026-10-20): the same $30,000 cap applies to every tier. Settler gets $12,000, $28,000 and $30,000 on the 5, 10 and 40 acre parcels; Homesteader gets $24,000, $30,000 and $30,000; Pioneer gets $0. Homesteader and Settler tie at the cap on the 40-acre parcel. The tier text above that mentions Homesteader-only capping is superseded by this line.


---

# Income and Trading Post read-only pass results (2026-10-20)

Claude Code read code and assets only. Nothing was changed, created or committed, and nothing was played. Items marked UNCONFIRMED were not verified from code.

## Where prices live

- All prices are in `Managers/TradingPost.cs`: the static `Stock` array (what the store sells) and the `Buys` dictionary (what it pays for non-stock goods). One file sets every price.
- They are static readonly constants: not a data asset, not saved, not on a component. Changing a price needs a script edit, so they sit outside the tunables-on-the-component rule.
- `SellPrice()` pays 40% of the buy price (rounded down, minimum $1) for anything in `Stock`. Items stocked under $3 (arrows, rifle rounds, cordage) are never bought back.
- Flat prices: no demand curve, no daily cap, no price drift, no stock limit.

## What the store sells (buy price, sell-back)

Flint and Steel $8 ($3), Knife $4 ($1), Canteen $5 ($2), Bucket $6 ($2), Cooking Pot $12 ($4), Tarp $8 ($3), Sleeping Bag $15 ($6), Tent $25 ($10), Axe $20 ($8), Hammer $10 ($4), Fishing Rod $12 ($4), Recurve Bow $30 ($12), Bolt-Action Rifle $60 ($24), Lamp Oil $3 ($1), Lumber $8 ($3), Weather Station $25 ($10); Arrows $1, Rifle Rounds $1, Cordage $2 (no buyback). Shift-click buys 10.

## What the store pays for (fixed values)

- Hides and furs: deer hide $8, antlers $6, small furs $3, feathers $1, sinew $1.
- Raw meat: venison $3, turkey $2, waterfowl $2, small game $1, chicken $1. Cooked: $4, $3, $3, $2, $2.
- Raw fish: bluegill $1, crappie $1, bass $2, catfish $2. Cooked: $2, $2, $3, $3.
- Berries, fruit, nuts: $1 each. Morel $3; oyster mushroom, dryad's saddle, wild onion, cattail $1 each.
- Chicken eggs $1, goat milk $1 (no livestock yet). Firewood $1, logs $3, stone $1.
- Crafted tools: stone pick axe $4, shovel $4, primitive axe $4, rabbit snare $1, box trap $2, fish trap $3, cane pole $2, pouch $3, torch $1, bow drill $1, lantern $4, primitive bow $2.
- Not bought: dandelion, tall grass, sticks, branches, clay, water, arrows, cordage, rifle rounds.

## Income by source

One game day is 30 real minutes. Selling is done in the T menu from anywhere (no trip to a shop).

| Source | Casual (about 5 real minutes) | Maximum | Notes |
|---|---|---|---|
| Foraging | about $0.60 a day on average | about $133 a year, $1.20 a day | 22 patches, one season each, no Winter forage |
| Fishing | about $55 (Summer), $65 (Spring, Fall), $25 (Winter) | about $330 to $410 (Summer, Spring, Fall) | about $3.9 per catch, no depletion |
| Fish traps | $2 to $3 a day per trap | no limit on traps | wear 10% a day |
| Snares and box traps | about $1.50 a day each | no limit | needs a Knife |
| Hunting | about $15 to $40 (UNCONFIRMED) | $100 or more | deer $31 to $38, turkey $12, duck $8, rabbit $4 |
| Firewood and logs | about $90 to $130 (10 trees) | $500 to $780 | 8,852 trees, no regrowth; logs also needed for building |
| Stone | about $45 | runs out | about 520 stones |
| Crafted-goods loop (lanterns) | $80 to $160 | about $260 (UNCONFIRMED) | clay never runs out, grass regrows in 3 days |

Mixed casual play while keeping fed, warm and sheltered: about $60 to $150 per game day (UNCONFIRMED).

## Starting kits

- Homesteader: $50 plus Tent, Sleeping Bag, Tarp, Cooking Pot, Bucket, Axe, Hammer, Stone Pick Axe, Shovel, Canteen, Flint and Steel, 3 Cordage, Knife, and a Bow with 20 arrows or a Rifle with rounds (about $63 to $75 sell value).
- Settler: $0, same kit without Hammer and Tarp (about $56 with the bow, $68 with the rifle).
- Pioneer: $0, only a Bucket, 3 Cordage and a Knife (about $3). A full Settler kit costs about $145 to $165 at the store.

## Must-buy items (no recipe)

Axe $20 (Primitive Axe is a craftable alternative), Fishing Rod $12, Recurve Bow $30 (Primitive Bow alternative), Rifle $60, Bucket $6, Canteen $5, Cooking Pot $12 (what it gates is UNCONFIRMED), Flint and Steel $8 (Bow Drill alternative), Tarp $8, Sleeping Bag $15, Tent $25. Nothing is hard-gated by a shop check. Building needs only logs, branches, stone, clay and tall grass (Hammer is craftable); Lumber ($8) has no use yet.

## Awkward selling

One click sells one unit and Shift-click sells the stack. One row per item kind, no totals, no "sell all". No confirmation before selling (an equipped tool may be sellable; not checked). No price drop for volume or freshness; whether stale meat can still be sold is UNCONFIRMED. Money shows as a plain $ and integer with no thousands separator.

## What this means for the balance (Claude's reading)

The problem is the opposite of what was assumed. The land payment is far too small for the income, not the other way round. The 5-acre payment is about $16 to $20 per game day against about $60 to $150 of casual income. Over 28 days a casual player earns about $1,700 to $4,200 against a $459 payment. The Trading Post prices (about $1 to $60) are also trivial next to $12,000 to $30,000 of savings.

The cheapest fix named by Claude Code is the crafted-tool buyback (the lantern loop), which also stops fish and firewood from dominating. Firewood, fishing and the lantern loop each earn $50 to $100 or more per game day for about five real minutes.


---

# Phase A decisions (Mike, 2026-10-20)

Made when Phase A was about to start. Code baseline b6f548a (money in cents), docs baseline e193129.

| Question | Decision |
|---|---|
| Old starting cash ($50 Homesteader, $0 Settler and Pioneer) | RETIRED. The only starting money is the savings (Homesteader 40%, Settler 20%, Pioneer 0, every tier capped at $30,000) minus the down payment. Starting kits stay as they are. |
| How a payment is made | A SEPARATE Finances screen on its own key (not a seventh tab in the T window). |
| Saves made before the loan system | Load as today with no loan; the money system is off for them. |
| Three missed payments in Phase A | Counted and warned in red, but no loss yet. The loss screen is Phase C. |

Phase A scope: parcel choice (5, 10 or 40 acres) and savings split at a new game, the loan (8% a year, 180 payments, one every 28 days, first due day 112), the Finances screen (pay the amount due, extra payments), the late fee, the missed count, due-date warnings, saving and loading, and tests. Not in Phase A: the seasonal statement and ledger (Phase B), the loss screen (Phase C), the Trading Post market, any change to prices.

Expected payments (computed from the formula; Claude Code's tests must match to the cent): $48,000 loan $458.71; $60,000 $573.39; $112,000 $1,070.33; $360,000 $3,440.35.


---

# Phase A build report (Claude Code, 2026-10-20), NOT committed, not yet played by Mike

Built and compiled with no console errors; 222 edit-mode tests pass (165 before, 57 new cases from about 30 methods). Claude Code also drove the real New Game flow, a Settler 5-acre game with a $6,000 split ($54,000 loan, payment $516.05), the Finances screen, warnings, paying, extra payments, three missed payments (red warning), save and reload, and an old save (slot1 loaded with no loan and $23.00 cash converted from dollars) in Play Mode. It backed up the whole Saves folder first and restored it afterwards (43 files, hashes match). The pasted report was partly garbled in the test list; the content above is what could be read.

Files created: `Economy/LoanState.cs` (terms, payment formula, due dates, lateness, paying, warnings, `LoanSaveData`), `Economy/LandRules.cs` (three parcels, savings by tier with the $30,000 cap, split rules), `Managers/MoneyManager.cs`, `UI/FinancesScreen.cs`, `UI/LandPurchasePanel.cs`, `Tests/EditMode/LoanTests.cs`. Changed: `Economy/Money.cs` (TryParseCents), `TradingPost.cs` (TrySpend), `GameManager.cs` (StartNewGame takes the land choice; load clears the loan first), `DifficultyManager.cs` (old starting cash replaced by SavingsPercent; three blurbs reworded), `MainMenuController.cs` (flow: difficulty, land, savings split, loan summary, slot pick), `SaveManager.cs` (end-of-day hook), `GameScreens.cs` (T-window keys ignored while Finances is open), `TimeDebugControls.cs` (Shift+F5 skips 10 days), `InputSystem_Actions.inputactions` (Finances action bound to L).

Processing before autosave: three ways, so no save can hold a loan behind the calendar: SaveManager.OnDayChanged calls MoneyManager.ProcessNow() before the autosave request; MoneyManager.Update catches up whenever TotalDays differs from the last processed day (sleep, debug skips, loads); MoneyManager.CaptureState processes first (manual and quit saves). Processing is idempotent.

What Claude Code chose that Mike did not specify (all PROPOSED, accepted unless Mike objects):
- Four warnings, once each per payment: "due in 7 days" (message line), "due today" (banner), "Overdue" the day after the due day (banner, says when the fee lands), "Missed" at due+10 (banner, prefixed WARNING when three are open). A long sleep shows only the latest stage per payment.
- Interest on later payments is computed on the whole balance, including principal sitting in open (missed) payments.
- If every remaining dollar of principal is in open payments, no more payments are created.
- The amount box takes "250", "250.50" and "$1,250.00". The savings split uses an amount box with "All down", "Half and half" and "None down" presets (no slider) and must stay within the savings; odd cents go to the reserve.
- The Finances screen is its own overlay and uses the Menu state, so the clock stops while it is open.
- MoneyManager creates itself at startup (like GroundWoodManager), so no scene was edited. Its loan terms are inspector fields on a component that only exists at runtime, so they cannot be tuned in the editor yet. Claude Code asked whether to add it to the Bootstrap scene.

Not covered: the visual look (screenshots were not viewed; the 40-acre card is unchecked), the real L key press, sleeping over a due date in the real sleep fade, the Pioneer and Homesteader paths in play, and the payoff banner in play (the logic is tested). A missing loan section and an old money save are covered only by the Play Mode run, not by edit-mode tests.

Playtest: `06_AI_Collaboration\Loan_Phase_A_Playtest_Checklist.md` (41 items).


---

# Phase B design: ledger and seasonal account statement (2026-10-22)

Status: design draft. Nothing built. Code baseline 50d01d8, docs baseline 8e86393. Phase A (loan), the Trading Post market and quantity controls are all committed and played.

## What Mike decided (2026-10-22)

| Question | Decision |
|---|---|
| When the statement appears | A banner at each season change says the statement is ready, and the player opens it with a key when they want. Nothing interrupts play. |
| Where it lives | A tab inside the Finances screen (key L). Finances gets two tabs: Loan (what exists today) and Statement. |
| History | A full season-by-season table: every season since the start. |

Carried over from the earlier decisions: positive figures in black, negative in red, on a light paper-style panel (the UI theme is dark); the statement shows income and expenses and numbers compiled over time, as the budget for the property and household; payments are processed before the statement is built.

## Claude's defaults (PROPOSED, accepted unless Mike objects)

- **The ledger stores totals, not clicks.** One record per season per category: money in and money out in cents. A per-click list could reach tens of thousands of rows, and totals are all the statement needs. Seasons are kept for the whole game (at most about 180 seasons for a full 180-payment loan, a few kilobytes).
- **A season is TotalDays divided by 28** (zero-based), labelled "Spring, Year 1" and so on. The first season is 27.75 days long because a new game starts at 6:00 AM.
- **Income categories:** Trading Post sales, split by market group (firewood, logs and stone; fish; game meat; hides and furs; forage; crafted goods; farm produce). Later systems add their own.
- **Expense categories:** Land payment (interest), Land payment (principal), Extra payment (to principal), Late fees, Down payment (in season 1 only), Trading Post purchases (one line for now). Later systems add their own.
- **Not in the ledger:** the F12 debug money. Starting savings are the opening cash, not income.
- **The statement always reconciles:** cash at start of season, plus money in, minus money out, equals cash at end of season. A test checks the ledger against the real cash balance.
- **The table:** one row per season, newest first, with Income, Expenses, Net, Cash at end and Loan balance at end. Totals rows for This year and Since the start. Click a season row to see its detail by category, per game day averages (income and expenses per game day) and the change from the season before.
- **Black and red:** positive net, income and balances in black; negative net and expenses in red. Zero in black.
- **Timing:** on SeasonChanged, payments, fees and missed-payment checks run first, then the ledger closes the season, then the banner appears. The first statement therefore includes any payment due on the same day.
- **The statement works for every save,** with or without a loan. An old save starts its history at the load (no invented past seasons) and its first statement appears at the next season change after loading.
- **Saving:** a new `player/ledger` save section; a missing section means an empty ledger. Old saves load unchanged.

## Questions for the Claude Code read-only pass

1. Every place money moves: TradingPost Buy, Sell (and the market SaleResult), TrySpend, loan payments, extra payments, late fees, the down payment at new game, F12 DebugAddMoney, ResetMoney. Which of them can write one ledger line, and which need a hook?
2. What does LoanState record when a payment is made: can the interest part and the principal part be read for each payment, or only the totals? Do late fees and extra payments have their own record?
3. Order of events on a season change: SeasonChanged against DayChanged against MoneyManager processing and the autosave. Where does closing the ledger go so payments come first and the autosave contains the closed season?
4. How would a tabbed Finances screen fit the current FinancesScreen layout, and how big is the light paper-style panel on a 1024x768 window (canvas scale 0.616)? Any scrolling problems for a table that grows to about 180 rows?
5. What the banner system offers (ScreenBanner) and whether it can carry a "Statement ready, press L" message without being lost during a sleep skip.
6. Which of the pure logic (ledger totals, season index, reconciliation, per game day averages, trend) can live in the engine-free Economy assembly for testing.
7. Size: files created, files changed, number of tests.


---

# Phase B read-only pass results (Claude Code, 2026-10-22)

Claude Code read the code and changed nothing (no files created, no edits, no staging, no Play Mode). Items marked UNCONFIRMED were not verified from code.

## Where money moves

Only a few paths change cash: it is the private field `money` in TradingPost and a grep found no other writer.

| Movement | Where | Ledger line |
|---|---|---|
| Buy | TradingPost.cs:95 | Directly: it knows the item, the count bought and the price. One expense line. |
| Sell | TradingPost.cs:114 | Directly: it has the SaleResult total. SaleResult carries no group, so the income category comes from the item id through the market table (MarketManager.Table, MarketGroup). |
| TrySpend(cents) | TradingPost.cs:124 | No: no category. Only caller is MoneyManager.Pay. |
| Loan payment, extra payment, late fee | MoneyManager.Pay | Hook here. PayResult already splits toFee, toInterest, toPrincipal and toExtra, so one payment can write four expense lines. |
| Late fee assessed | LoanState.Advance | Not cash. Cash leaves only when the fee is paid, so the statement is cash-basis. |
| Down payment | GameManager.StartNewGame, BeginLoan | The player never holds it as cash (cash starts at the reserve). Needs a new hook. |
| ResetMoney | TradingPost.cs:132 | Not a transaction: used for the new-game reserve, every load and the debug money. Must not write a line. |
| F12 DebugAddMoney | TimeDebugControls.cs:157 | Goes through ResetMoney, so it looks like a reset. Compiled out of release builds. |

## Loan records

`Pay` returns a PayResult (applied, toFee, toInterest, toPrincipal, toExtra, payments cleared). LoanState stores only running totals (feesPaid, interestPaid, principalPaid, extraPaid) and the open payments, with no per-payment history, so the ledger must record from PayResult at payment time in MoneyManager.Pay. Nothing in LoanState needs to change. "Loan balance at end" is LoanState.balanceCents when the season closes.

## Event order

TimeManager.StartNewDay: totalDays++, then DayChanged, then SeasonChanged, then YearChanged. SaveManager.OnDayChanged runs MoneyManager.ProcessNow(), then MarketManager.ProcessNow(), then RequestAutoSave. Only WeatherManager listens to SeasonChanged, and the autosave is requested before it fires, so closing the ledger on SeasonChanged would leave the autosave without the closed season.

Recommendation (adopted as the plan): close seasons state-based like the loan and the market. `LedgerManager.ProcessNow()` closes every season whose end day has passed; it calls MoneyManager.ProcessNow() first itself; it runs from SaveManager.OnDayChanged after the two existing calls and before RequestAutoSave, from Update (catch-up after loads and skips) and from CaptureState. A sleep closes at most one season; F5, F6 and F8 debug skips can close several in one call (the intermediate seasons take the same cash and loan balance, which is correct).

## Finances screen and panel

FinancesScreen is its own canvas (sorting 150, 1920x1080 reference, match 0.5), panel 1500x880 reference px. Two tab buttons ("Loan", "Statement") fit at about x=440 to 840, y=18, with the cash label moving to about x=900. The Statement would be its own class mounted in the panel. At 1024x768 (scale 0.616) a paper panel inset by 24 px is about 894x431 px on screen, text at 22 reference px draws at about 13.5 px (small but legible, as in Parts 1 and 2). Six columns plus a detail pane fit. `UiKit.ScrollList` with a scrollbar exists; 180 rows would be built when the tab opens, not on every event. The paper panel needs its own colors (ink, red, paper); UiKit's text colors are dark-theme cream and ScrollList has a 20% black background that would tint the paper.

## Banner

`ToolStatus.Banner` is one static string with a real-time timer, drawn by ToolHud only in the Playing state. A second call overwrites the first and it can expire during sleep or a menu. Payment due dates fall on season starts (day 112, then every 28 days), so a "payment due today" banner and a "statement ready" message always land on the same instant and one would be lost. Recommendation: a small FIFO queue in ToolStatus whose timer starts only when the banner can be drawn, plus an "unread statement" flag in the ledger (saved) that drives a persistent line until the statement is opened. UNCONFIRMED: whether the HUD banner draws during the sleep fade.

## Pure code (Economy assembly)

Season index and labels ("Spring, Year 1"), this-year grouping (index / 4), fractional first season (27.75 days), per-category totals, record and close, net, "This year" and "Since the start" rows, reconciliation (start + in - out = end, any difference shown as "Other"), per-game-day averages (in hours so partial seasons work), change from the previous season (cents and basis points), the black-or-red rule, and the save struct (like MarketSaveData). The recording hooks and LedgerManager's ProcessNow are checked in Play Mode.

## Opening cash and saves

LandRules.SavingsCents sets savings; LandPurchasePanel splits them into downCents and reserveCents; GameManager.StartNewGame calls TradingPost.ResetMoney(reserveCents) and MoneyManager.BeginLoan(acres, price, down). Plan: `LedgerManager.BeginNewGame(savings, down)` records the opening cash as the full savings and the down payment as a season-1 expense line, so season 1 reconciles. An old save with no ledger section starts the ledger at the first ProcessNow after the load, taking current cash as the opening balance.

New `player/ledger` section from a LedgerManager : ISaveable (version, season length, open season, unread flag, array of season records with index, days covered, long[] in and out per category, cash at end and loan balance at end). Category ids are append-only. About 200 to 300 bytes of JSON per season (20 seasons about 6 KB, the 180-season maximum under 60 KB). A missing section loads exactly as before; older builds ignore the unknown section. GameManager gets ResetLedger next to ResetLoan and ResetMarket.

## Size

New: Economy/Ledger.cs (about 350 lines), Managers/LedgerManager.cs (about 250), UI/StatementPanel.cs (about 450), Tests/EditMode/LedgerTests.cs (about 500), plus .meta files. Changed: TradingPost.cs (about 10 lines), MoneyManager.cs (Pay hook, Update and L-key restructure so they work without a loan, about 40), SaveManager.cs (1 call), GameManager.cs (2 calls plus opening data), FinancesScreen.cs (tabs and no-loan handling, about 120), ToolStatus.cs (banner queue, about 30), ToolHud.cs (persistent line, about 20), Bootstrap.unity (adds the component). About 35 to 45 tests, roughly 1,400 to 1,800 lines with tests.

## Risks found

1. "Statement for every save" conflicts with Phase A: the L key and FinancesScreen.Open only work while a loan exists, and Refresh closes the screen if the loan is gone. This must change.
2. "Payments first, then close" is moot: there is no auto-debit, so nothing is paid at the season boundary. Only the loan catch-up must run first so the "Loan balance at end" snapshot is current. The first payment (day 112) is due on the first day of Year 2, so it cannot be in the season-1 statement either way.
3. Banner collision is certain, not occasional (see Banner).
4. The statement is cash-basis: interest and late fees show only when paid, so a season of missed payments looks cheap while debt grows. Suggested memo line "Owed at end". Accrual would need new loan fields.
5. The Stocked group (16 items sold back) is missing from the income list and needs a category.
6. Debug money breaks reconciliation unless allowed for. Suggested: close each season with the actual cash and show any difference as an "Other" line (0 in release builds).
7. The first season is fractional (27.75 days), so averages need hours or hundredths of a day.
8. Season length (daysPerSeason, default 28) is an inspector value, so it is stored in the save to keep labels and averages stable.
9. UNCONFIRMED: whether the HUD banner draws during the sleep fade; whether SleepManager leaves the game in Playing state during sleep; how the 5-seasons-at-once debug skip behaves in the real clock loop (test in Play Mode).
10. Optional better way: split Trading Post purchases by group (almost free, same table lookup as sales) so later systems such as livestock feed are easier to add.

## Proposed build order (Claude Code)

1. Pure core in Economy with tests. 2. LedgerManager and wiring (ProcessNow, record API, save and load, Bootstrap component, GameManager reset and opening-savings hook, SaveManager line before the autosave). 3. Recording hooks (TradingPost Buy and Sell, MoneyManager.Pay, L key and Finances without a loan), then a Play Mode check that the ledger matches real cash, including a skip over several seasons. 4. Banner queue and the unread flag, with a check of a payment banner and a statement message on the same day and through a sleep. 5. The Statement tab. 6. Layout check at 1920x1080 and 1024x768, then the playtest checklist. 7. Docs.

Decisions needed from Mike: items 1, 4 and 5 change the build most (items 6 and 10 as well).


---

# Decisions after the Phase B read-only pass (Mike, 2026-10-22)

| Question | Decision |
|---|---|
| Finances screen on a save with no loan | YES, always openable. L opens Finances on every save. Without a loan the Loan tab says there is no loan, and the Statement tab works normally. (This reverses Phase A's "old saves have no Finances screen".) |
| Unpaid debt on a cash-basis statement | Add a memo line "Owed at end" in red under the net: late fees and open payments still owed at the end of the season. |
| Cash that does not match the ledger | Show an "Other" line so every season reconciles (zero in a normal game and in release builds; F12 debug money lands here in dev builds). |
| Trading Post purchases | Split by group instead of one line. Claude Code proposes about 5 to 7 purchase categories from the Stock array and lists them in its report. |

Adopted from the read-only pass (Claude's defaults, accepted unless Mike objects):

- Seasons close state-based: LedgerManager.ProcessNow() closes every season whose end day has passed, calls MoneyManager.ProcessNow() first, and runs from SaveManager.OnDayChanged before RequestAutoSave, from Update (catch-up) and from CaptureState.
- A small FIFO banner queue in ToolStatus, with its timer starting only when the banner can be drawn, plus a saved "unread statement" flag that drives a persistent HUD line until the statement is opened.
- New income category "Stocked goods sold back" for sell-backs of stocked items.
- Opening cash is recorded as the full savings and the down payment as a season-1 expense, so season 1 reconciles. An old save starts its ledger at the first ProcessNow after loading, taking current cash as the opening balance.
- The season length is stored in the save. Per-day averages use hours, so the 27.75-day first season is correct.
- The statement is built as its own class (StatementPanel) mounted in the Finances panel behind two tabs, Loan and Statement. The paper panel uses its own ink, red and paper colors, not the dark-theme UiKit colors.
- Statement rows are built when the tab opens, not on every event.

Build plan: one build, in the order proposed by Claude Code (pure core and tests; LedgerManager and wiring; recording hooks and the no-loan Finances screen with a Play Mode check that the ledger matches real cash, including a skip over several seasons; banner queue and unread flag; the Statement tab; layout check; then Mike plays a checklist).


---

# Phase B build report (Claude Code, 2026-10-22), NOT committed, not yet played by Mike

Built on code baseline 50d01d8. 427 of 427 edit-mode tests pass (345 existing + 82 new: LedgerTests 71, BannerQueueTests 11). Console 0 errors and 0 warnings before and after. Saves folder backed up and restored (hashes identical). Bootstrap Weather block identical before and after.

What was built:
- `Economy/Ledger.cs` (pure, engine-free): LedgerCategory (append-only ids), per-season per-category totals in long cents, season index and labels, the fractional first season (27.75 days), close, net, reconciliation with an Other line, This year and Since the start, per-game-day averages (hours), change from the season before, the red rule, and the save struct with the season length stored.
- `Managers/LedgerManager.cs`: running ledger, state-based ProcessNow (calls MoneyManager.ProcessNow first), save section player/ledger, an unread-statement flag. Closes seasons from SaveManager.OnDayChanged before the autosave, from Update and from CaptureState.
- Hooks: TradingPost.Buy and Sell, MoneyManager.Pay (four lines from PayResult), GameManager.StartNewGame (opening savings, down payment as a season-1 expense).
- `UI/StatementPanel.cs` and the Loan/Statement tabs in FinancesScreen: season-by-season table newest first (Season, Income, Expenses, Net, Cash at end, Loan balance at end), This year and Since the start rows, click a season for its detail pane (categories, net, "Owed at end", "Other (not in the ledger)" when not zero, per-game-day averages, change from the season before), light paper panel, positive figures black and negative red. Opens on the Statement tab when a statement is unread or there is no loan.
- `Economy/BannerQueue.cs` with ToolStatus and ToolHud: a FIFO banner queue (timer counts only while a banner can be drawn; same message asked again keeps its place; at most 6 wait) and a persistent "Account statement ready: press L" HUD line.
- The L key and the Finances screen now work on every save, with or without a loan.

Purchase categories (6): Tools; Hunting and fishing; Shelter and bedding; Cooking, water, fire; Building materials; Other goods. Income categories: Trading Post sales by market group, plus "Stocked goods sold back".

Limits: "Owed at end" in seasons skipped in one jump (debug skips, long loads) shows the amount owed today. Statement is cash-basis (interest and fees appear when paid). Untested by Claude Code: the real L key, real clicks, a real sleep across a season, a real payment on a season-close day, 180 seasons, the paper panel's contrast on a real monitor, release builds, F12 itself.

Playtest: `06_AI_Collaboration\Money_Phase_B_Playtest_Checklist.md` (42 items).


---

# Phase C design: the loss screen (2026-10-23)

Status: design draft. Nothing built. Code baseline 0f6b002, docs baseline 5487ccc. Phases A and B are committed and played.

## What is already decided (earlier, by Mike)

| Rule | Decision |
|---|---|
| Loss | Three missed payments open at once is a loss (2026-10-20). Catching up clears a missed payment, so the count is open payments, not payments ever missed. |
| Loss wording | A banner or text telling the player that mother nature beat them (Mike's wording, kept). |
| Loss screen | Offers Restart or Load Save. |
| After a loss | Continue loads the last safe autosave. A loss never overwrites the autosave (the loss check runs before the autosave that day, or suppresses it). |
| Restart | Must remember the weapon choice, which the game does not store today. |

## Decisions (Mike, 2026-10-23)

| Question | Decision |
|---|---|
| What Restart does | Same setup, fresh world: starts a new game at once with the same difficulty, weapon, parcel and savings split. |
| What the loss screen shows | A short summary of the run besides the banner. |
| Loss during sleep | The loss screen appears when the player wakes up. |

## Claude's defaults (PROPOSED, accepted unless Mike objects)

- **Wording:** big text "Mother Nature beat you." and a line under it "Three payments on the land went unpaid." (final wording is Mike's to change).
- **Summary:** days survived, seasons completed, total income and total expenses since the start (from the ledger), loan balance still owed, the due days of the three missed payments, and cash at the end.
- **Buttons:** Restart, Load Save (opens the slot panel in load mode; greyed with "no saves" if there are none), and Main Menu. Main Menu is an addition Mike did not ask for.
- **Lost state:** a new game state with the clock stopped. The pause menu and every game window (T, I, L, M and so on) are blocked while it shows. Nothing in the world can be played.
- **No save while lost:** the end-of-day autosave, the sleep autosave, the quit-to-menu autosave and the quit-game autosave are all suppressed while lost, and a lost game is never written to any slot. The last safe autosave stays exactly as it was.
- **What Continue gives back:** the autosave from the day before the loss. The player is back with two missed payments open and the third payment overdue, so they get a real chance to pay it. This follows from "last safe autosave".
- **Loss threshold:** a field on MoneyManager (default 3), next to the existing warning count, so Phase D can tune it per difficulty.
- **Processing order each day:** money first (payments, fees, missed count), then the loss check, then the autosave, then the ledger close. A loss stops the autosave and the ledger close for that day.
- **Sleep:** the lost flag is set at once so no autosave can happen, but the screen waits for the wake.
- **Restart after loading:** the setup (difficulty, weapon, parcel, savings split) is saved so Restart works after a load. An old save without it makes Restart fall back to the New Game screens.
- **Testing:** a debug key (editor and development builds only) forces the loss without waiting 178 days.

## Open questions for the Claude Code build

1. Where best to store the new-game setup so it survives a load (a small save section, or the loan section), and what an old save does.
2. How to suppress every autosave path while lost (SaveManager day hook, SleepManager, ReturnToMainMenu, OnApplicationQuit) with one flag.
3. Loading a save while in the Lost state inside the World scene: has this been exercised, and what must be reset (loan, ledger, market, time scale, input, screens)?
4. Which pure logic can go in the Economy assembly for tests (the loss rule, the run summary, the setup record).


---

# Phase C build report (Claude Code, 2026-10-23), NOT committed, not yet played by Mike

Built on code baseline 0f6b002. 475 of 475 edit-mode tests pass (427 existing + 48 new in LossTests). Console 0 errors and 0 warnings before and after. Saves folder backed up and restored (29 files, hashes identical). Bootstrap.unity untouched.

What was built:
- `Economy/LossRules.cs` (pure, engine-free): the loss rule (open missed payments against a threshold, default 3), the line "Three payments on the land went unpaid.", the RunSummary record, and the NewGameSetup record with its save struct.
- `MoneyManager`: `lossMissedCount` field (default 3) next to the warning count, a Lost flag and LossSummary. The loss check runs after payments, fees and the missed count, and before any save. Cleared by a new loan, a reset or a load.
- `GameManager`: a Lost state with timeScale 0 and pause ignored; moves to Lost only when the player is not asleep; stores the new-game setup (difficulty, weapon, parcel, savings split) in the player.json section "setup"; RestartGame() and MainMenuNote.
- `SaveManager`: one SavingBlocked check guards manual save, autosave, quit save and queued saves while lost. A lost game is never written to any slot and the last safe autosave stays untouched.
- `LedgerManager`: closes and starts no season once the game is lost.
- `UI/LossScreen.cs`: "Mother Nature beat you." with a line under it, a summary (days survived, seasons completed, total income and expenses from the ledger, land loan still owed, cash at the end, the due days of the missed payments one per line), and three buttons: Restart (same difficulty, weapon, parcel and savings split, fresh world, no slot picked), Load Save (slot panel in load mode, greyed when there are no saves), Main Menu (my addition). The loss clears all banners.
- A game saved before Phase C has no setup record, so Restart goes to the New Game screens with "That game did not record how it began, so pick your start again."
- A sleep that crosses the loss sets Lost at once (no save can happen) and shows the screen after the wake.
- Debug key: Insert (editor and development builds, while playing) forces the loss.

Choices: "Land loan still owed" is principal only (late fees and interest on open payments are not included); Continue returns to the autosave from the day before the loss.

Untested by Claude Code: real key presses and clicks, a real 178-day loss, a sleep started by a human, a real application quit, Load Save greyed with no saves, opening the slot panel from the Load Save button.

Playtest: `06_AI_Collaboration\Money_Phase_C_Playtest_Checklist.md` (29 items).


---

# Phase C addition: savings go to the debt on a loss (Mike, 2026-10-23)

Added after the Phase C build, before Mike played it. Mike's rule: on a loss, all the savings (cash) default to any debt that is still unpaid, before the screen totals what is still owed at the end.

| Question | Decision |
|---|---|
| Which debt the cash goes against | Everything owed on the loan: open missed payments and late fees first, then interest, then principal (the same order as a payment, Phase A). |
| How the loss screen shows it | Cash you had, then the amount taken for the debt, then what is left owed. Cash left is shown only when it is above $0.00. |

How it works (PROPOSED details, Claude's reading of the rule):

- It happens when the loss is declared. The cash is applied to a COPY of the loan state with the same rules as a payment (fee, interest, principal, then any rest as extra principal). The real loan, the real cash and the ledger are not changed and nothing is written to a save: a lost game is never saved and Continue still loads the last safe autosave, so the player gets their cash back there.
- "Land loan still owed" on the screen now means everything still owed after the cash was applied (principal plus interest and late fees owed on open payments), no longer principal only. This also answers the open note in the Phase C playtest checklist.
- If the cash is more than everything owed, the debt shows $0.00 and the leftover cash is shown. (A rare case: a player with enough cash to clear the loan would normally have paid.)
- The seizure is not a ledger line, so the "total income and total expenses since the start" numbers on the screen are unchanged.
- Pure logic goes in the Economy assembly with tests (cash 0, cash below the fees, cash between fees and everything, cash above everything).


---

# Phase D design: difficulty and rebalance (2026-10-23)

Status: design draft. Nothing built. Code baseline 08964d8, docs baseline 2049f3c. Phases A, B and C are committed and played. Real income per game day has never been measured; every income number so far comes from Claude's simulation (about $29, $50 and $82 per game day for survivor, casual and grinder play with one price pool per item).

## Decisions (Mike, 2026-10-23)

| Question | Decision |
|---|---|
| Approach | Add a small MEASURING hook first, then tune from real play. Phase D is split into D1 (infrastructure and measuring, no change in behaviour) and D2 (set the numbers from the measurements). |
| The 40-acre parcel | LEAVE IT as the late-game parcel. With the $30,000 savings cap its payment is about $143 a game day, more than wild goods alone can earn. It needs livestock, crops and other income systems that are not built yet, so it stays very hard for now and is tuned when those systems exist. |
| What differs between tiers, besides savings and the starting weapon | ALL FOUR levers: missed payments before a loss, the late fee amount, the interest rate, and the Trading Post sell prices. |

Payments per game day for reference (8% over 180 payments, one every 28 days): 5 acres about $16 a day with 20% down, about $20 with nothing down; 10 acres about $38 with $28,000 down, about $48 with nothing down; 40 acres about $143 a day with $30,000 down.

## D1: what gets built (PROPOSED)

- **A tier rules table** (`TierRules`, pure, in the Economy assembly): per difficulty tier, the missed payments before a loss, the late fee in cents, the interest rate in basis points, and the Trading Post sell price in basis points (10000 = 100%). In D1 every tier holds TODAY'S values (3 missed, $75, 8.00%, 100%), so nothing changes in play. D2 changes only the numbers.
- **Wiring:** a new loan takes its rate and late fee from the tier (the terms are saved with the loan, so a save keeps the terms it started with); the loss check uses the tier's threshold; sells at the Trading Post use the tier's sell price (the live total, the real sale and the ledger line must all match). Buy prices are not changed.
- **Shown to the player:** the loan summary at New Game and the Loan tab show the tier's terms (rate, late fee, missed payments before a loss).
- **Measuring:** the ledger already gives income and expenses per game day. D1 adds real minutes played per season (recorded in the ledger, an optional field so old saves load unchanged), shown on the season's detail pane with income per real hour, and a development-only key that writes the ledger to a CSV file (one row per season) so the numbers can be read and compared.
- **Measuring plan for Mike:** play each tier for a few seasons the way you really play, read the Statement per season, note anything unusual, and export the CSV. A plan sheet (`Income_Measuring_Plan.md`) is written once D1 is built.

## D2: tuning from the data (after the measurements)

- Set the four tier levers. PROPOSED starting point, NOT decided and NOT to be built until the data is in: Homesteader loses at 4 missed payments, $50 late fee, 7% rate, sell prices +10%; Settler 3, $75, 8%, 0% (as today); Pioneer 2 or 3, $100, 9%, -10%. Pioneer has no savings, so its first miss counts on day 122 and a threshold of 2 would end the game on day 150; the measured income decides whether that is fair.
- Check the early game: with real income per game day, does a typical player on each tier reach the first payment (day 112) on a 5-acre parcel?
- Rebalance Trading Post prices and market numbers if the data says wild goods earn too much or too little.
- Revisit the savings table (20%, 40%, none, capped at $30,000) against how long the reserve lasts.
- Update Difficulty_System.md, the GDD and the New Game blurbs with the final numbers.

## Open for D2

The real tier numbers; whether Pioneer's threshold should be 2 or 3; whether the sell price lever stays or is replaced by a price-floor change; and what "winning" the early game looks like in dollars (for example paying the 5-acre loan down by a set amount by the end of Year 3).

## Phase D1 build report (2026-10-23)

**Status:** built, awaiting playtest and commit. No change in behaviour at default values.

**Tier levers (TierRules, pure, Economy assembly).** One row per tier with four values: missed payments before a loss, late fee (cents), interest rate (basis points), Trading Post sell share (basis points, 10000 = 100%, rounded half up on the sale total). All three tiers currently hold 3 / $75 / 800 / 10000.

| Tier | Missed before loss | Late fee | Rate | Sell share |
|---|---|---|---|---|
| Homesteader | 3 | $75 | 8% | 100% |
| Settler | 3 | $75 | 8% | 100% |
| Pioneer | 3 | $75 | 8% | 100% |

**How terms are applied.**
- The loan freezes its rate and late fee when it is created, so an old save keeps the terms it was made under. The loss threshold and the sell share are read from the tier each time, so changing them in D2 affects games already in progress.
- A dev-only "Override Tier Terms" tickbox on MoneyManager lets the old rate / late fee / loss count fields (and the new sell share field) override the table for testing. Off by default.
- Terms are shown to the player on the New Game loan summary and in a "Terms of this game" block on the Loan tab.
- The sell share scales the sale total and the per-unit price shown; market multipliers are untouched. Stepper quote, sale, cash change and ledger line all agree (checked at 90%: 13 firewood 925 cents against 1,028 cents at 100%).

**Measuring tools.**
- Real play time is stored per season in the ledger (milliseconds), counted only while the game is in the Playing state with the clock running; not during menus, Finances, Trading Post, pause, sleep or a loss. Per-frame cap 5 s so editor recompile stalls cannot add time. Clean-window accuracy 99.6%.
- The Statement season detail shows "Real minutes played" and "Income per real hour". Older seasons without a recording show nothing rather than 0.
- CSV export (End key in editor and dev builds) writes `ledger_YYYYMMDD_HHMMSS.csv` to `...\AppData\LocalLow\MM Custom\Homestead\Exports\`. Dollars as 1234.56, oldest season first, columns include Total income, Total expenses and Real minutes; the open season is marked "(so far)".

**Open D2 questions carried forward.**
- The red warning still fires at 3 open payments, so a tier with a lower threshold would never see it before losing.
- Pioneer starts with $0 savings, so its first missed payment counts on day 122; a threshold of 2 would end the game around day 150.
- Real income per game day is still unmeasured; see `06_AI_Collaboration\Income_Measuring_Plan.md`.
- The NOT-decided starting proposal in "Phase D design" stands as a proposal only until measurements are in.

**Tests.** 583 edit-mode tests pass (91 new in TierRulesTests and LedgerMeasuringTests).
