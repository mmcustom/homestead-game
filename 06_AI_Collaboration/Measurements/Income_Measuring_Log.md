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


## Winter Pioneer run, Spring Y1 statement (2026-10-28, Mike's screenshots; item CSV not yet exported)

Build b552bef (Maple, Tripod, Potash, teaching pass in). New Pioneer game, 5 acres, 27.75 game days, 252.9 real minutes (about 9.1 real minutes per game day, so time keys were used; per-real-hour figure is not comparable with the first run).

| Line | Winter-run Spring Y1 | First Pioneer run, Spring Y1 |
|---|---|---|
| Income | $227.09 | $177.16 |
| Income per game day | $8.18 | $6.38 |
| Coverage of the $20.48 payment | about 40% | about 31% |
| Expenses | $152.00 | not recorded here |
| Net / cash at end | $75.09 | not recorded here |
| Hides and furs | $120.03 | $111.20 |
| Firewood, logs and stone (Ash, and Lye or Potash if sold) | $76.70 | $50.13 (Ash) |
| Crafted goods (Kindling Bundles) | $20.36 | $14.83 |
| Farm produce | $10.00 (looks like one Maple Syrup at $10; to confirm) | none |

Expenses: hunting and fishing $92.00, shelter and bedding $15.00, cooking, water, fire $45.00 (fits Cooking Pot $12 + Potash Kettle $25 + Flint and Steel $8; to confirm). Per game day: income $8.18, expenses $5.48, net $2.71. Income per real hour $53.87 (not comparable, see above).

Reading: income is up about 28% on the first Pioneer run ($49.93 more in the same 27.75 days), mostly Ash side ($26.57 more in the Firewood, logs and stone line), Kindling Bundles and a little more fur. The gap to the payment is now about $12.30 a game day (was $14.10). Rough projection only: if Summer and Fall hold about $8 a game day and Winter adds about $9 a day of Maple over 28 days, cash at day 112 is roughly $950 to $1,000 against the $573.39 first payment, so a cushion of roughly $350 to $400 after payment 1; payment 2 (day 140) is probably covered; payment 3 (day 168) is where it runs short unless income keeps growing. To refine with the Summer, Fall and Winter statements and the item CSV (End key) which splits Ash, Lye, Potash and Syrup.


## Winter Pioneer run, Summer Y1 statement (build b552bef income rules; display build a3f5f07)

Source: Mike's two screenshots of the Account Statement, Summer Year 1, 28.00 game days, 65.4 real minutes played.

| Line | Summer Y1 | Spring Y1 (for comparison) |
|---|---|---|
| Firewood, logs and stone (Ash, Lye, Potash included) | $17.96 | $76.70 |
| Hides and furs | $111.74 | $120.03 |
| Crafted goods | $1.00 | $20.36 |
| Farm produce (Maple Syrup) | $0.00 | $10.00 |
| Total income | $130.70 | $227.09 |
| Expenses | $0.00 | $152.00 |
| Net | $130.70 | $75.09 |
| Cash at end | $205.79 | $75.09 |
| Loan at end | $60,000.00 | $60,000.00 |
| Income per game day | $4.67 | $8.18 |
| Net per game day | $4.67 | $2.71 |
| Coverage of $20.48 per game day (Pioneer payment) | 22.8% | 40% |

Game's own comparison: income per game day -$3.51 (-42.9%) against Spring; expenses +$5.48 per day better (nothing bought); net +$1.96 per day (+72.3%) better. Year 1 so far: income $357.79, expenses $152.00, cash $205.79.

Reading: Hides and furs are about 85% of Summer income. Maple is Winter and Spring only, so Summer has no Maple line, and Ash, Lye and Potash money fell from $76.70 to $17.96. Crafted goods almost vanished ($1.00).

Projection to the day 112 payment ($573.39): cash at day 56 is $205.79, so Fall and Winter together must net $367.60, about $6.56 per game day. If Fall repeats Summer ($4.67 per day, about $131), Winter must bring about $237, or $8.45 per day. Without Maple or growth (Fall and Winter both $4.67 per day) cash at day 112 would be about $467, short by about $106. If Winter reaches about $17 per day (Spring rate plus Maple), cash would be about $810 and payment 1 is covered. Later payments (day 140 on) need the full $20.48 per game day on average, which Summer is nowhere near.

Open question for Mike: how time passed in Summer (65.4 real minutes for 28 game days means a speed key or day skips). Per game day is only fair if the days were played, not skipped.


## Winter Pioneer run, Fall Y1 statement

Source: Mike's two screenshots of the Account Statement, Fall Year 1, 28 game days, played at 10x speed all season (Mike's note). Real-minute figures ignored (counter unreliable).

| Line | Fall Y1 | Summer Y1 | Spring Y1 |
|---|---|---|---|
| Firewood, logs and stone | $95.06 | $17.96 | $76.70 |
| Hides and furs | $92.44 | $111.74 | $120.03 |
| Crafted goods | $14.42 | $1.00 | $20.36 |
| Farm produce | $0.00 | $0.00 | $10.00 |
| Total income | $201.92 | $130.70 | $227.09 |
| Expenses (Fall: Bought: tools) | $20.00 | $0.00 | $152.00 |
| Net | $181.92 | $130.70 | $75.09 |
| Cash at end | $387.71 | $205.79 | $75.09 |
| Income per game day | $7.21 | $4.67 | $8.18 |
| Net per game day | $6.50 | $4.67 | $2.71 |
| Coverage of $20.48 per game day | 35.2% | 22.8% | 40% |

Game's own comparison: income +$2.54 per day (+54.3%) over Summer; net +$1.83 per day (+39.1%). Year 1 so far (84 days): income $559.71 ($6.66 per game day, 32.5% of $20.48), expenses $172.00, cash $387.71.

Reading: Firewood, logs and stone jumped back to $95.06 (Ash chain, firewood or logs sold); Hides and furs eased to $92.44. Crafted goods $14.42.

Projection to the day 112 payment ($573.39): cash at day 84 is $387.71, so Winter must net $185.68, about $6.63 per game day. Fall netted $6.50 per day, so if Winter merely repeats Fall (net $181.92) cash at day 112 would be about $569.63, short by about $3.76. Winter Maple on top should cover it with a thin cushion. Payment 2 (day 140) needs another $573.39 from 28 days, about $20.48 per day, against roughly $7 per day earned: this is the real gap.


## Winter Pioneer run, Winter Y1 statement (RUN RESULT: first payment NOT covered)

Source: Mike's two screenshots of the Account Statement, Winter Year 1, 28 game days. Mike's note: 60x speed all season. The game is now in Spring Year 2 (so far $0). The day 111 End export was NOT taken (the season rolled past it at 60x).

| Line | Winter Y1 | Fall Y1 | Summer Y1 | Spring Y1 |
|---|---|---|---|---|
| Firewood, logs and stone | $5.67 | $95.06 | $17.96 | $76.70 |
| Hides and furs | $51.80 | $92.44 | $111.74 | $120.03 |
| Crafted goods | $6.52 | $14.42 | $1.00 | $20.36 |
| Farm produce (Maple Syrup) | $0.00 | $0.00 | $0.00 | $10.00 |
| Total income | $63.99 | $201.92 | $130.70 | $227.09 |
| Expenses | $0.00 | $20.00 | $0.00 | $152.00 |
| Net | $63.99 | $181.92 | $130.70 | $75.09 |
| Cash at end | $451.70 | $387.71 | $205.79 | $75.09 |
| Income per game day | $2.29 | $7.21 | $4.67 | $8.18 |
| Coverage of $20.48 per game day | 11.2% | 35.2% | 22.8% | 40% |
| Time keys | 60x all season | 10x all season | 10x from day 11 | time keys, not noted |

Winter statement shows "Owed at end -$573.39" against cash $451.70 and Loan balance still $60,000.00: the day 112 payment was due and cash was short by $121.69. The game is in Spring Y2 with that payment owed.

Year 1 totals (112 days): income $623.70 ($5.57 per game day, 27.2% of $20.48), expenses $172.00, net and cash $451.70.

Result: a Pioneer on these rules, played this way, did NOT cover the first payment ($573.39): short $121.69. If Winter had merely matched Fall's income ($201.92), cash would have been about $589.63 (covered by about $16). Spring's rate ($8.18 per day, $229) would have given about $617.

Validity warning: Winter is not a clean measurement. There was no Maple Syrup income at all in Winter (the season Maple was meant to add about $9 per day), hides fell 44%, firewood/logs/stone fell 94%, and 60x speed may have cut what Mike could do per game day. Treat Winter as unreliable for rates; the Fall figure ($7.21) is the best recent steady rate. Year-round mean excluding Winter: (227.09 + 130.70 + 201.92) / 84 = $6.66 per game day.

Open: what happened with Maple and chores in Winter; End export to be taken now (ledger CSVs can still be exported after the fact).


## Winter Pioneer run, ledger export (taken after the payment day, not day 111)

Files copied to this folder: pioneer_5acres_winter_ledger.csv and pioneer_5acres_winter_ledger_items.csv (from Exports\ledger_20261029_225756.csv and ledger_items_20261029_225756.csv). The export came after day 112, so the last row is Spring Year 2 (so far, 0.39 days).

What the day 112 payment did: the game took ALL of Mike's cash ($451.70) as a short payment: interest $400.00, principal $51.70. Loan balance now $59,948.30, cash $0.00, Owed at end $121.69 (the rest of the $573.39). Late fees so far $0.00.

Year 1 income by line (112 days, 111.75 game days played): Hides and furs $376.01 (60.3%), Firewood, logs and stone $195.39 (31.3%), Crafted goods $42.30 (6.8%), Farm produce $10.00 (1.6%). Total $623.70 = $5.58 per game day, 27.3% of $20.48. Fish, Game meat, Forage and Stocked goods lines were $0 all year.

Items sold (units, dollars, average price):

| Item | Spring | Summer | Fall | Winter | Year 1 |
|---|---|---|---|---|---|
| Small furs | 56, $120.03 ($2.14) | 53, $111.74 ($2.11) | 40, $90.49 ($2.26) | 17, $51.80 ($3.05) | 166, $374.06 |
| Ash | 18, $29.99 | 10, $17.96 | 16, $25.35 | 3, $5.67 | 47, $78.97 |
| Kindling bundles | 34, $15.36 | 1, $1.00 | 36, $10.42 | 10, $6.52 | 81, $33.30 |
| Potash | 3, $46.71 ($15.57) | 0 | 3, $45.64 ($15.21) | 0 | 6, $92.35 |
| Lye | 0 | 0 | 10, $24.07 ($2.41) | 0 | 10, $24.07 |
| Maple Syrup | 1, $10.00 | 0 | 0 | 0 | 1, $10.00 |
| Other (tools, sinew, bow drill) | $5.00 | 0 | $5.95 | 0 | $10.95 |

Reading:
- Small furs are the Pioneer's engine: 166 furs, about $374 of $624. Winter had only 17 furs (against 40 to 56), and the average price rose to $3.05 because fewer were sold. That is the main reason Winter collapsed, together with no Maple.
- Maple Syrup earned $10.00 in the whole year (one Syrup). Mike's Winter run did not use Maple, so Maple's Winter value (estimated about $9 per game day) is NOT measured by this run.
- Potash: 6 sold for $92.35 over the year, Lye 10 for $24.07. Ash and kindling together about $112. This Ash chain line is small but steady: about $1.00 per game day.
- The $45 "Bought: cooking, water, fire" in Spring is a purchase; the items file lists sales only, so it still cannot be broken down.
- Real minutes column: Spring 252.9, Summer 65.4, Fall 52.7, Winter 21.2 (not reliable, ignore).

Design implication to discuss: even at the best season (Spring $8.18 per day) the Pioneer reaches 40% of the payment; with Fall's $7.21 the year-1 income of about $5.58 per game day is 27%. Furs carry 60% of income; the Ash chain adds about 5%; Maple was not tested in Winter.
