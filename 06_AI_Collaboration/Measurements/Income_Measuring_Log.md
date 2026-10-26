# Income Measuring Log

Started 2026-10-24. Plan: `06_AI_Collaboration\Income_Measuring_Plan.md`. Code baseline 70bc5a0 (D1 tier levers and measuring tools).

Rules for every session: new game, play like a real player, no F12, no PageUp debug cash, time skips allowed but note them. At the end of each season press L and read that season on the Statement. At the end of the session press End and put the CSV in this folder as `<tier>_<parcel>_ledger.csv`.

Payment per game day for reference: $48,000 loan $16.38, $60,000 loan $20.48, $112,000 loan $38.23.

---

## Session 1: Settler, 5 acres ($48,000 loan, $458.71 a payment)

Time skips used:

| Season | Income | Expenses | Net | Cash | Loan owed | Real min | Income / real hour | Notes |
|---|---|---|---|---|---|---|---|---|
| Spring Y1 | | | | | | | | |
| Summer Y1 | | | | | | | | |
| Autumn Y1 | | | | | | | | |
| Winter Y1 | | | | | | | | |
| Spring Y2 | | | | | | | | |

## Session 2: Pioneer, 5 acres ($60,000 loan, $573.39 a payment)

Time skips used:

| Season | Income | Expenses | Net | Cash | Loan owed | Real min | Income / real hour | Notes |
|---|---|---|---|---|---|---|---|---|
| Spring Y1 | | | | | | | | |
| Summer Y1 | | | | | | | | |
| Autumn Y1 | | | | | | | | |
| Winter Y1 | | | | | | | | |
| Spring Y2 | | | | | | | | |

## Session 3: Homesteader, 5 acres

Time skips used:

| Season | Income | Expenses | Net | Cash | Loan owed | Real min | Income / real hour | Notes |
|---|---|---|---|---|---|---|---|---|
| Spring Y1 | | | | | | | | |
| Summer Y1 | | | | | | | | |
| Autumn Y1 | | | | | | | | |
| Winter Y1 | | | | | | | | |
| Spring Y2 | | | | | | | | |

## Session 4 (optional): Settler, 10 acres ($112,000 loan, $1,070.33 a payment)

Time skips used:

| Season | Income | Expenses | Net | Cash | Loan owed | Real min | Income / real hour | Notes |
|---|---|---|---|---|---|---|---|---|
| Spring Y1 | | | | | | | | |
| Summer Y1 | | | | | | | | |
| Autumn Y1 | | | | | | | | |
| Winter Y1 | | | | | | | | |
| Spring Y2 | | | | | | | | |

---

## Results so far

(Claude fills this in from the sessions: income per game day by season and tier, and coverage against the payment.)


### First round, Spring Year 1 (2026-10-25, from Mike's Statement screenshots; code baseline 64ffc69)

Mike sent two Spring Y1 statements. Both seasons are 27.75 game days. **Which tier and parcel the first one is has not been confirmed** (its loan is $54,000 after a $6,000 down payment, which does not match the Settler 5-acre $48,000 in this log); the second is Pioneer 5 acres ($0 savings, $60,000 loan). Time skips used: not yet reported, but the real minutes (362.9 and 50.1 against about 833 for 27.75 days at normal pace) show heavy skipping or sleeping in both, so income per real hour is not comparable between them.

| | Statement 1 (tier to confirm) | Statement 2 (Pioneer 5 acres) |
|---|---|---|
| Cash at start | $12,000.00 | $0.00 |
| Income | $119.14 | $33.16 |
| of which game meat | $18.10 | none |
| of which hides and furs | $100.04 | $30.16 |
| of which crafted goods | $1.00 | $1.00 |
| of which stocked goods sold back | none | $2.00 |
| Expenses | -$6,015.00 (down payment -$6,000, hunting and fishing -$15) | -$16.00 (shelter and bedding -$8, cooking, water and fire -$8) |
| Net | -$5,895.86 | +$17.16 |
| Cash at end | $6,104.14 | $17.16 |
| Loan at end | $54,000.00 | $60,000.00 |
| Real minutes played | 362.9 | 50.1 |
| Income per real hour | $19.70 | $39.72 |
| Income per game day | $4.29 | $1.19 |

**Coverage (income per game day against the loan payment per game day):** Statement 2: $1.19 against $20.48 is about 6%. Statement 1: $4.29 against about $18.43 (my estimate: payments are linear in the loan, $9.5565 per $1,000 a month, so $54,000 is about $516.05 a month) is about 23%. In both, no income source beyond hunting and a token $1.00 of crafting appeared. At these rates the Pioneer on $0 cannot make the first payment on day 112 by a wide margin ($573.39 due, about $100 more income expected by then at this pace); the first statement's cash ($6,104) would fund about 11 payments with no income at all.

Pending: tier of statement 1, time skips used, and Mike's notes.

**Mike's answers (2026-10-25):** (1) Statement 1 was the **middle tier (Settler)**; parcel and loan not confirmed (the screen shows $54,000 after a $6,000 down payment, not the $48,000 expected for Settler 5 acres); statement 2 was **Pioneer**. (2) Time sped up **x60** once a campfire and shelter were set up by excellent water and rabbit snares were built for food and furs to sell; several hours played across both tiers. (3) Sold hides and furs once a small surplus built up. **"In the beginning it doesn't seem to have enough ways to make money other than furs and hides because selling any handcrafted tools or crafts doesn't pay enough for the time it takes to make it."**

**Reading:** the income was almost all passive (snare furs and hides) plus a little meat. Crafting did not pay for the gathering time it needs (lantern $4, pouch $3, crafted group has the hardest market drop). x60 time means the snares ran while time skipped, so income per game day is the fair number, not per real hour.


## Pioneer 5 acres, Spring Year 1, WITH Ash and kindling (measured 2026-10-26)

Files: `pioneer_5acres_ledger.csv` and `pioneer_5acres_ledger_items.csv` (copied from the End-key export `ledger_20261026_174132`; the first run with the item-level export and with Ash in the game). Build at the time: e0e36b9.

Mike's notes: played several hours; time sped up x10 from game day 6 (earlier runs used x60). Crafted in this order: hammer, primitive axe, primitive shovel, bow drill, wood pile, rock pile, storage bin, tool storage, food cache, then rabbit snares. 6 rabbit snares, each checked about once a day. Mike remembered "roughly 12 Ash" sold; the item file says 32. "Everything went smooth."

| | Pioneer, Spring Y1 (with Ash) | Pioneer, Spring Y1 (earlier, no Ash) |
|---|---|---|
| Days covered | 27.75 | 27.75 |
| Real minutes (counter) | 113.5 | 50.1 |
| Income | $177.16 | $33.16 |
| Expenses | $48.00 (shelter and bedding $40, cooking, water and fire $8) | $16.00 |
| Net / cash at end | $129.16 | $17.16 |
| Loan at end | $60,000.00 | $60,000.00 |
| Income per game day | $6.38 | $1.19 |
| Expenses per game day | $1.73 | $0.58 |

Where the $177.16 came from (item file), per game day over 27.75 days:

| Item | Units | Dollars | Average price | Share | Per game day | Per day it sold |
|---|---|---|---|---|---|---|
| small_furs (snares) | 48 | $111.20 | $2.32 (base $3) | 62.8% | $4.01 | 1.73 furs |
| ash | 32 | $50.13 | $1.57 (base $2) | 28.3% | $1.81 | 1.15 Ash |
| kindling_bundle | 25 | $14.83 | $0.59 (base $1) | 8.4% | $0.53 | 0.9 bundles (250 Sticks) |
| bow_drill | 1 | $1.00 | $1.00 | 0.6% | $0.04 | |

Nothing else sold: no logs, firewood or stone, no fish, forage or meat (the category CSV shows $0 for all of those). All of "Firewood, logs and stone" ($50.13) is Ash.

**Coverage:** $6.38 against the $20.48 a game day payment is about 31%; against the $20 target about 32%. The gap is about $13.62 a game day.

**Against the earlier predictions:**
- Ash: predicted $4.41 a game day (4 a day); got $1.81 (1.15 a day sold, at an average of $1.57 because it was sold in lots). About 41% of the prediction, but above Claude Code's own spring estimate of 0.7 to 1 Ash a day, so sticks, branches or a busy fire contributed.
- Kindling Bundle: predicted $2.14 (3 a day at $1); got $0.53 (0.9 a day, average $0.59). About 25%.
- Snares: 6 snares checked daily; the model says about 0.39 successful catches per snare per game day, so 2.3 a day. The run sold 1.73 furs a day averaged over the whole season. The snares were built last in the crafting order, so they were not out for all 27.75 days; if they ran about 20 days that is about 2.4 a day, in line with the model. **Not confirmed until Mike says which game day the snares went down.**
- Income per real hour ($93.64 on the statement) is not usable: 10x time skip from day 6, and the 113.5-minute counter is well below "several hours", so the counter may not include everything Mike played. Per game day is the fair number.

**What it means for day 112 (rough, assumes the Spring rate holds, ignores season changes in snare luck and any winter costs):** cash $129 plus about 84 more game days at about $6.38 gives about $665 against the first payment of $573.39, so the first payment is probably met with about $90 to spare. The second payment 28 days later needs $573 again against about $270 earned in 28 days, so it is short by roughly $300, and each later one too. At this income a Pioneer reaches the first payment and then falls behind; the loss rule is 3 missed payments.

**Open:** which game day the snares went down; whether Ash volume was mostly Firewood, Sticks or Branches; what the real-minutes counter does while the game is paused or in menus.


**Update 2026-10-26 (Mike): the 6 snares went down on game day 4.** That leaves about 24.75 active days of 27.75. The 48 small furs are then about 1.94 a day, or about 0.32 per snare per day, against the model's 0.39 successful catches per snare per day for a daily check: the model overpredicted by roughly 15 to 20%, close enough to use. Fur income per ACTIVE day is about $4.49 ($111.20 over 24.75 days), not the $4.01 spread over the whole season. Snare changes (2-catch snare, 5% odds, spacing, bait, fur quality) can be sized with the model as it stands, trimmed by about 15%. Still open: Ash volume by fuel type; what the real-minutes counter includes.
