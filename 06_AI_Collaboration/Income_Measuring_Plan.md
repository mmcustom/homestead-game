# Income Measuring Plan (for Money Phase D2)

**Purpose:** find out how much a normal player really earns per game day and per real hour on each tier, so D2 can set the tier numbers (missed payments before a loss, late fee, interest rate, Trading Post sell share) from facts instead of guesses.

**Status:** plan only. Start after the D1 playtest passes and D1 is committed.

## What we need to know

1. How much income per game day does a decent, honest player make in each season of year 1 (and into year 2)?
2. How does that compare to the payment? The payment per game day is the monthly payment divided by 28: about $16.38 a day on a $48,000 loan ($458.71), about $20.48 on $60,000 ($573.39), about $38.23 on $112,000 ($1,070.33). A 40-acre loan with the $30,000 savings cap is about $143 a day.
3. How much is that per real hour? (Tells us if the pacing feels fair, not only the numbers.)
4. Where does the player get stuck? (First payment is due on day 112, the end of year 1, so the first-year build-up matters most.)

## The measuring rule

Play like a real player, not a min-maxer. Do not cheat with the debug kit (no F12). Time skip keys are fine for waiting, but note it when you used them, because skipped time still counts as game days and distorts "per real hour".

## Sessions

Play one new game on each tier, at least through the end of year 1 (day 112), longer if you can:

| Session | Tier | Parcel | Why |
|---|---|---|---|
| 1 | Settler | 5 acres | the middle case, most players' default |
| 2 | Pioneer | 5 acres | hardest tier, $0 savings, the tight case |
| 3 | Homesteader | 5 acres | easiest tier, checks it is not trivially easy |
| 4 (optional) | Settler | 10 acres | the bigger loan ($112,000, $1,070.33 a payment) |

## What to do each season

At the end of every season press L and open that season on the Statement. Write down in the template below: income, expenses, net, cash, loan owed, real minutes played, income per real hour. Also jot short notes on anything unusual: what you sold and what sold best, what you were short of, what blocked you, any bug.

## Template (copy one per session)

| Season | Income | Expenses | Net | Cash | Loan owed | Real min | Income / real hour | Notes |
|---|---|---|---|---|---|---|---|---|
| Spring Y1 | | | | | | | | |
| Summer Y1 | | | | | | | | |
| Autumn Y1 | | | | | | | | |
| Winter Y1 | | | | | | | | |
| Spring Y2 | | | | | | | | |

## At the end of each session

1. Press the End key. The Console and HUD show where the CSV was written (`C:\Users\mmcus\AppData\LocalLow\MM Custom\Homestead\Exports\`).
2. Copy the CSV into `06_AI_Collaboration\Measurements\` (make the folder) and rename it with the tier and parcel, for example `settler_5acre_ledger.csv`.
3. Tell me the session is done. I will read the CSV and your notes, work out income per game day and the coverage (income per day divided by payment per day) for each season, and log the results.

## What I will do with the results (D2)

- Build a table of income per game day by season and tier, and the coverage ratio against the payment.
- Propose the tier numbers (the NOT-decided starting proposal in Money_System.md is only a starting point), including the Pioneer threshold and the red warning question.
- Check the early game: can a careful player reach the first payment on day 112 on every tier, and is the 40-acre parcel hard in the intended way?
- You decide. Nothing changes in the game until you approve the numbers; Claude Code then edits the TierRules table, and Difficulty_System.md, the GDD and the tier blurbs are updated to match.


## Amendment 2026-10-24 (Mike): lighter first round

A full year-1 run per tier is about 56 real hours each (game day 30 real minutes, season 28 days about 14 hours), too much for a first pass. **First round: finish Spring of year 1 on Settler (5 acres) and on Pioneer (5 acres), then send the numbers.** Homesteader and the Settler 10-acre session wait. Mike's observation while playing (2026-10-24): too few ways to earn when only furs and hides sell, so after these numbers the next step is building income sources (Ash first, then the wood and fiber workshop), then measuring again. The rest of this plan (what to record, the End-key CSV, naming, what Claude does with the results) is unchanged. Time-skip keys are fine when idle; say when used. No F12 and no cash keys.


## Amendment 2026-10-25 (Mike): next Pioneer run

- Timing: after the Ash by-product is built (see Early_Income_Design.md), so the run measures Pioneer with Ash and kindling in the game. No separate pre-change Pioneer baseline; the 50-real-minute Spring Y1 statement (income $33.16, $1.19 a game day) stays the only "before" figure and is a weak one.
- Length: full Spring, building at a normal pace and then speeding time up after setup, about 2 to 3 real hours.
- Rules: build only what a new player needs first (shelter, water, fire, food); press End at each season change to export the ledger (the main CSV plus the new ledger_items CSV); copy both to Measurements/ as `<tier>_<parcel>_ledger.csv` and `<tier>_<parcel>_ledger_items.csv`.
- Not required (Mike did not choose them): a ban on selling logs, firewood and stone, and a note of snare count and check pattern. Without a snare note, the snare model can only be checked through the item rows (small_furs units and dollars), not through catch rates.
- Settler and Homesteader sessions remain on hold as in the 2026-10-24 amendment.
