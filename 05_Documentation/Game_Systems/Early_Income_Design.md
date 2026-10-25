# Homestead
## Early Income Design v0.1

Status: Design Draft (PROPOSED), written 2026-10-25. Docs only. Nothing is built and nothing here is decided. Every number is a first pass for Mike to cut down. This draft supersedes the 2026-10-20 decision in Trading_Post_Market.md that "no new sellable goods are needed for Phase A", because the Spring Year 1 measurement contradicts it.

# Why this exists

Mike (2026-10-24): "there's just not enough ways to generate income when only selling furs and hides." He chose to measure first and then build income. He then set the target and style: **cover the loan payment, about $20 a game day**, using **both passive and active income**, and **include crafted-goods price options**.

This doc takes the measured gap, shows what the game can already pay if the player sells more kinds of goods, proposes new income sources (Ash first), lays out the crafted-goods price options, and ends with open questions. It builds on Workshop_and_Handmade_Goods.md (the workshop, tanning and log-chain proposals) rather than repeating it.

# What was measured (Spring Year 1, first round)

| | Settler (middle tier, parcel not yet confirmed) | Pioneer, 5 acres |
|---|---|---|
| Cash at start | $12,000 | $0 |
| Income for the season | $119.14 | $33.16 |
| Where it came from | game meat $18.10, hides and furs $100.04, crafted $1.00 | hides and furs $30.16, crafted $1.00, stocked sold back $2.00 |
| Expenses | $6,015.00 (down payment $6,000, hunting and fishing $15) | $16 (shelter and bedding $8, cooking, water and fire $8) |
| Loan at end | $54,000 | $60,000 |
| Income per game day | $4.29 | $1.19 |
| Payment per game day (loan payment divided by 28) | about $18.43 (my estimate, not read from the game) | $20.48 |
| Share of the payment covered | about 23% | about 6% |

Read plainly: at the measured rate a Pioneer reaches game day 112 (the first payment) with about $130 against a $573.39 payment, so the loan fails without a change in what the player can earn.

Caveats on the data: the Pioneer session was only 50 real minutes, which undersells that tier; Mike sped time to x60 once camp, shelter, water and snares were set, so "per real hour" figures are meaningless and only per-game-day numbers are used here; the Settler parcel size and loan are still unconfirmed ($54,000 after the $6,000 down payment is what the statement shows).

# Why the gap is so large (findings)

1. **Only two income groups appeared in either statement.** Hides and furs, plus a little game meat. No logs, firewood, stone, fish or forage income shows up. **Mike's answer (2026-10-25): he did not sell them because he needed those materials for building and crafting.** That is the real finding: in the first season the player is a net consumer of logs, firewood and stone, so those market pools exist but are not income the player can spend. The early game has almost nothing to sell that it is not also using.
2. **The goods that are sold are low volume.** Hides and furs come from snares and the odd hunt, so they arrive in a trickle (about $3.60 a game day for Settler).
3. **Crafted goods do not pay for the effort.** Mike: selling handcrafted tools or crafts "doesn't pay enough for the time it takes to make it." The lantern ($4) and pouch ($3) sit in the crafted group (10% drop per unit, floor 0.1), which was set that way on purpose to kill the lantern loop.
4. **Nothing signposts the other pools.** A new player sees furs and hides, and nothing in the game says that a log or a basket of forage also pays.

The game's own model had a Survivor at about $21 a game day, a Casual at about $38 and a Grinder at about $69 to $85, but those assumed a player selling a broad mix every day. The measurement says a real early player sells a narrow mix. The design problem is therefore two problems: make the broad mix easy and obvious, and add sources that pay more per unit of effort than a $1 firewood stick.

# The target

About **$20 a game day** covers a 5-acre payment ($18.43 Settler estimate, $20.48 Pioneer). The Homesteader loan ($112,000, $38.23 a game day) sits well above $20 and is left to the D2 levers at the end of this doc rather than to income alone.

All income figures below come from the market simulation (25% of the price gap recovers each game day; a good's pool drops by the group's percentage per unit sold, down to the floor), run at steady state with seasonal targets ignored. They are balance estimates, not measurements, and the measuring sessions should re-check every one.

# Step 0: what the game already pays (no new code)

Steady-state income per game day if the player sells that many of one good every day. Prices are today's Trading Post base prices; drops are the committed group percentages.

| Good | Base price | Sold per day | Income per game day |
|---|---|---|---|
| Logs | $3 | 2 | about $4.76 |
| Logs | $3 | 4 | about $7.78 |
| Logs | $3 | 6 | about $9.86 |
| Logs | $3 | 10 | about $12.53 |
| Firewood | $1 | 6 | about $3.29 |
| Stone | $1 | 4 | about $2.59 |
| Fish (one species, 5% drop) | $2 | 3 | about $4.00 |
| Forage (6% drop) | $1 | 6 | about $2.68 |
| Morels | $3 | 2 | about $4.31 |

Adding Mike's measured Settler baseline of $4.29 a day (hides, furs and meat):

- **Broad mix** (4 logs, 6 firewood, 4 stone, 3 fish, 6 forage, about 23 items a day) reaches about **$24.60** a game day.
- **Half mix** (2 logs, 3 firewood, 2 stone, 2 fish, 3 forage, about 12 items a day) reaches about **$17.70**.

So the market as built can cover the payment on paper, but only if the player gathers and sells around 12 to 23 low-value items every game day across five pools. **Mike's answer changes how much of that is real:** logs, firewood and stone are the same goods the player needs to build and craft with, so in the first season they are spent, not sold. Step 0 income is a later-game surplus (after the cabin, shed and tools are built), not early income. It is also a lot of $1 transactions, and trees are finite (8,852 on the map, no regrowth), so selling logs is also selling the land being cleared.

For the first season the usable Step 0 pools are only the goods the player neither builds with nor eats: game meat, hides and furs, which is the measured $4.29 a game day. Mike confirmed the fish and forage he did not eat were kept as a food reserve (2026-10-25), which is good play and should not be pushed into the market, so they are not early income either. Only surplus beyond a comfortable reserve is sellable. New sources that are by-products, not building inputs or food, are therefore what closes the early gap.

Three cheap changes would make Step 0 work better without new items:

- **Signpost the pools.** The T window could show a short "What sells well today" line using the current price multipliers, so a player sees that logs and forage are worth something. A one-line hint about selling surplus could go wherever the game gives early-game guidance.
- **Give logs their own market group** (already proposed in the Workshop doc) so their drop and floor can be tuned alone.
- **Sell-all-surplus button** in the T window that sells everything above a keep-amount per item, so the broad mix is one click, not thirty. Given Mike's answer, the keep-amount matters most: a player who is still building should be able to keep what the next build needs and sell only the rest.

# New income sources (passive and active)

The aim is goods that pay more per unit of effort than raw firewood, and that include some money that arrives while the player is busy elsewhere (Mike's "both passive and active"). The options below are ordered by how little new code they need. Prices are proposed balance numbers.

## 1. Ash (passive by-product, then Lye and Potash)

Ohio history supports this directly (Workshop doc, research section): ashes were the first cash crop for clearing settlers, burned hardwood became ashes, ashes were leached into lye, lye boiled down to potash. Home ashes sold for about 12 cents a bushel and potash went to asheries. The game's version:

- **Ash** is a by-product: burning Firewood in a campfire or hearth leaves Ash (proposed 1 Ash per 3 Firewood burned) in an ash pan that fills while the player does other things. Collecting it is a short interact. This is the passive part: the player already burns firewood for warmth and cooking, so the Ash accrues with no extra work.
- **Sell Ash raw** at about $2 a bushel (its own market group, 6% drop). Selling 4 a day earns about $4.41 a game day.
- **Leaching hopper** (cheap build: a barrel or log trough, straw and water): Ash plus water becomes Lye over a game day. Lye is needed for tanning (W2) and soap, so Ash has uses beyond cash.
- **Potash kettle** (needs a kettle, a Trading Post purchase like the Axe): Lye boiled down with Firewood becomes Potash, proposed at about $12. One Potash every 3 days earns about $3.83 a game day, every 2 days about $5.57. It costs Firewood and attention, so it is the active upgrade to the passive Ash.
- **Needs:** one new item (Ash), a small change to fire code (output on burn), market entries, and later the hopper and kettle stations. Ash alone is small code.

Honest size: Ash plus Potash adds roughly $8 a game day, not $20. It matters because it is passive and historically right, not because it solves the gap alone.

## 1b. Sticks (an unsold by-product Mike already has spare)

Mike (2026-10-25): in the first season the things he had spare were the fish and forage he did not eat, which he saved, and **sticks**. A felled tree gives 2 Logs, 5 Branches and 6 Sticks, and the Trading Post does not buy sticks or branches (Workshop doc, "What the game already has"), so they pile up with no use except building recipes.

- **Kindling Bundle:** a craft or a direct sell of 10 Sticks, priced like firewood. At $1 a bundle, 3 bundles a day earns about $2.14 a game day, 5 a day about $2.97. At $1.50 a bundle, 3 a day earns about $3.21. That is 30 sticks a day, about 5 felled trees' worth, so it is a small supplement, not a solution.
- **Sticks as fuel:** if sticks can be burned, they could also leave a smaller amount of Ash (proposed 1 Ash per 6 Sticks). That feeds the Ash line below instead of selling for pennies. Needs a check of how fires consume fuel today.
- Branches could join the same bundle at a higher count. Both are by-products of clearing, so they cost the player no building material that is not already surplus.

Honest size: about $2 to $3 a game day. Worth doing only if it is a few lines in the market and recipe tables.

## 2. Snares and trapping, used harder

Snares are the one income Mike's statements show working. This section is based on Trapping_System.md and the live code in TrapManager.cs and Trap.cs (read 2026-10-25), not on the doc alone.

**What the code does today (first-pass odds from 2026-09-25, never tuned):**

- A Rabbit Snare costs 1 Cordage (3 Tall Grass), so it is nearly free. **There is no limit on how many a player can place.**
- It rolls once per game hour at 3.5%, multiplied by condition (0.3 to 1.0), spot quality (0.4 to 1.3, best on brush lines and forest edges with 1 to 4 trees within 10 m, plus a bonus near a discovered wildlife trail) and season (Spring 1.1, Summer 1.0, Fall 1.3, Winter 0.6).
- A catch is 70% Successful (1 Small Game Meat and 1 Small Fur), 20% Small (meat only, fur torn) and 10% Damage (no catch, and the snare loses half its condition).
- **A snare holds one catch and stops catching until the player checks it.** Checking needs a Knife in the pack.
- Condition falls 0.1 a day. Repair costs 1 Cordage and is offered below 70%, so a daily-checked snare needs about 0.26 Cordage a day.
- There is no animal population: nothing runs out, so more snares always means more catches, limited only by the player's walking and the market.

**What that works out to (my simulation of those rules, 28-day runs, summer, good spot, repair whenever below 70%):**

| Per snare | Successful catches a day (meat + fur) | Small catches a day |
|---|---|---|
| Checked every day | about 0.39 | about 0.11 |
| Checked every 2 days | about 0.28 | about 0.08 |
| Checked every 3 days | about 0.20 | about 0.06 |

By season (checked daily, good spot): Spring about 0.41, Summer 0.39, Fall 0.46, Winter 0.27.

**What a line of snares earns (fur at $3 a unit, 5% drop; checked daily):**

| Snares | Furs a day | Fur income per game day | Meat a day (food, or about $2 each if sold; meat price assumed, not looked up) |
|---|---|---|---|
| 3 | 1.2 | about $3.00 | 1.5 |
| 5 | 2.0 | about $4.40 | 2.5 |
| 8 | 3.1 | about $6.10 | 4.0 |
| 12 | 4.7 | about $7.80 | 6.0 |
| 20 | 7.8 | about $9.90 | 10.0 |

Income per extra snare shrinks quickly because all furs share one price pool: the first 5 snares earn about $4.40, the next 15 add only about $5.50 more. The market is therefore already a natural cap, and 20 snares is a lot of walking for $10.

**Where the real lever is: the one-catch limit.** Today a snare that is not checked sits full and wastes time, so a player who cannot check daily loses most of its output. Options, in the order I would try them:

1. **Snare holds 2 catches** (the Fish Trap already holds 3). Checked every 3 days, a snare goes from about 0.21 to about 0.35 successful catches a day (about 67% more); checked daily, from about 0.40 to about 0.52. With capacity 3 it is about 0.42 at a 3-day check. This favors the player who sets a line and does something else, which is the passive style Mike asked for.
2. **Raise the base odds from 3.5% to 5% an hour.** About 0.50 a day (daily check, capacity 1) instead of 0.40, or about 0.71 with capacity 2. It is one number but it lifts every snare, including the unattended ones.
3. **Snare bait.** Only Box Traps use bait today. A bait that adds up to 25% (apples best) would give the forage the player already gathers a second use. It costs the player food, so it works against the reserve Mike keeps.
4. **A spacing rule so scaling cannot run away.** Because there is no limit and nothing runs out, a soft rule such as "spot quality drops when another snare is within 15 m" keeps the player spreading a line out along brush edges, which matches the doc's "placement matters" rule, and stops 40 snares on one rabbit trail.
5. **Fur quality.** Small Furs are one price today. If a clean catch (the Knife requirement is already there) gave a better fur than a torn one, the price would follow skill, not volume. This is a bigger change and is left as an idea.

**Size of the gain:** options 1 and 2 together roughly double a snare's output if the player cannot check every day. For a player running 8 snares and checking every 3 days, that is about 1.6 furs a day now (8 x 0.20) against about 3.3 with capacity 2 and 5% odds (8 x about 0.41), worth roughly $3 to $6 more a game day in fur, plus the meat. That is real but still not $20 on its own, so snares help close the early gap and do not replace Ash, kindling or a loan lever.

**Mike's answers (2026-10-25):** he ran 6 snares and checked them several times a day, and he likes all five ideas above (recorded as accepted for the design; nothing built).

**What that means for his play style.** Checking several times a day means the one-catch limit almost never binds, so a 2-catch snare helps him little (about 0.39 to 0.53 full catches per snare per day now, about 0.42 to 0.57 with capacity 2, from my 6-hour-check sim). It matters for the "set a line and walk off" player, which is still worth doing for the passive style. The levers that help him are the odds and the price:

| 6 snares, checked every 6 hours | Furs a day | Fur income per game day (fur $3, 5% drop) |
|---|---|---|
| Today (3.5% odds, 1 catch) | 2.3 to 3.2 (poor to good spots) | about $5.00 to $6.24 |
| 5% odds | 3.3 to 4.4 | about $6.37 to $7.55 |
| 5% odds and 2 catches | 3.5 to 4.9 | about $6.60 to $8.00 |

So the odds change adds only about $1.30 a game day for a 6-snare player, because the shared fur price pool soaks up the extra furs. A market-side change helps more. At 2.7 furs a day: fur at $3 earns about $5.59; fur at $4 about $7.45 (+$1.86); a milder 3% drop instead of 5% about $6.38 (+$0.79); both about $8.51. A price rise needs a reason in the game: real raw pelts for common animals are weak (research in the Workshop doc: raccoon about $5, muskrat about $2), so a better fur price fits best as a **processed fur** (Tanned Small Fur, about $7 in Workshop W2) rather than a bigger raw price.

**Calibration warning.** For 6 snares the model predicts about $5 to $6 a game day in fur alone, but the Settler statement shows only $3.60 a game day for all hides and furs (which may include deer hides; the statement does not split them), and the Pioneer $1.08. So the real result is below the model, and the model's catch rate may be too high or the snares were set late, in poor spots, or the Knife requirement and Cordage repair cost time. This should be checked against a ledger CSV before any snare number is trusted. I also have not verified the Small Game Meat price (assumed $2).

## 3. Maple sugaring (strong Ohio fit, spring only)

The Western Reserve pioneers made maple syrup (Workshop doc, research section), and a Spring Year 1 start lines up with sugaring season. Tap a maple, hang a bucket, and sap collects on its own (passive); boiling it down at a fire takes Firewood and attention (active). In real life it takes roughly 40 gallons of sap to make 1 of syrup (general knowledge, not sourced this round).

- Proposed: Syrup at about $10 a jar, 6% drop. 1 jar a day earns about $8.47 a game day and 2 a day about $14.38, but only for the sugaring weeks.
- **Blocked on trees:** the game has no wood species yet (Workshop doc: species and quality are not built). Maples need to exist as a species with a tap point. This is a larger piece of work than Ash and should be sized before it is promised.

## 4. Shiitake logs and other log-based passives (later)

A hardwood log inoculated with shiitake spawn fruits for years (research: $8 to $12 a pound). One placed log would be a slow passive income that also spends the finite log supply on something that keeps paying. It needs mushroom spawn bought at the Trading Post and a species check, so it is a later option, not Phase A.

## 5. Wood and fiber workshop goods (W1) and leather (W2)

These are already fully drafted in Workshop_and_Handmade_Goods.md (spoon, bowl, shakes, baskets, mats, brooms, tanned hide at about $22, moccasins about $20). The Workshop doc's decisions 1 to 10 are all still unanswered. What this doc adds is the effort and price question below.

# Crafted goods: price options

Mike's complaint is that crafts do not pay for their time. Today the crafted group drops 10% per unit with a floor of 0.1 and most recipes are instant and cheap. The simulation shows the two obvious dials move income less than the base price does.

| Option | What changes | Income, 1 good (base $8) a day | 2 a day | 4 a day |
|---|---|---|---|---|
| A. Keep | 10% drop, floor 0.1 | about $6.15 | about $9.68 | about $13.54 |
| B. Milder group | 6% drop, floor 0.25 ("Handmade wares") | about $6.78 | about $11.50 | about $17.63 |
| C. Price by labor | Keep the group, but set each good's base price from its inputs plus a labor rate per game hour of job time | depends on the rate | | |
| D. Mixed | Option B for goods that need a station and time, Option A for instant recipes (lantern, pouch) | | | |

Reading the table: A versus B changes a 2-a-day player by about $2. Making the good worth more per unit (Option C) changes it much more. For example, the Workshop doc's Wooden Bowl ($7, 1 game hour) and Cutting Board ($8, 1 game hour) are about $7 to $8 an hour of job time but cost almost no real time (a game hour is 1.25 real minutes), which is why Option C needs a decision on **what an hour of game time should be worth**, not only a market percentage.

**Money-loop risk (the lantern problem):** any craft built from free inputs (tall grass, cattail, clay) is the likeliest to become a loop. Controls already in the proposal: per-item pools, the group floor, job time, one job per bench at a time, and starting prices on the low side. Re-measure after any change.

# Putting it together: mixes that reach about $20 a game day

All include the measured Settler baseline of $4.29 a day (snares, hunting) and are steady-state estimates.

| Mix | What the player does each game day | Income per game day |
|---|---|---|
| Early season baseline (measured; snares and hunting, nothing built with, nothing eaten) | Hides, furs and game meat | about $4.29 |
| Early season plus raw Ash (Pioneer, no kettle yet) | Baseline plus 4 Ash a day | about $8.70 |
| Early season plus Ash and Kindling Bundles | Same, plus 3 bundles of 10 Sticks a day | about $10.84 |
| Early season plus Ash, Kindling and Potash | Same, plus one Potash every 3 days | about $14.67 |
| Snares, Ash and Potash only (passive-leaning) | Check snares, collect Ash, one Potash every 3 days | about $12.50 |
| Step 0 half mix | Baseline plus 2 logs, 3 firewood, 2 stone, 2 fish, 3 forage | about $17.70 |
| Step 0 half mix plus Ash and Potash | Same, plus 4 Ash and one Potash every 3 days | about $25.90 |
| Plus workshop goods | Add 2 crafted goods a day (base $8) and a tanned hide every 3 days | about $42.70 |

Reading it: the rows from "Step 0 half mix" down include logs, firewood and stone, so they describe the player after the building rush, not the first season. In the first season, Ash, Kindling Bundles and Potash together reach only about $14.70, about $5 under $20 even with everything running (and the Potash kettle is a purchase a Pioneer cannot afford at first), so early income needs at least one more non-building, non-food source (maple sugaring or better snares) or a D2 lever. After the building phase, Step 0 plus Ash clears the $20 target with room, and the workshop belongs to the Casual and Grinder tiers (Survivor $21, Casual $38 in the market model). Two cautions: the Potash kettle is a Trading Post purchase and a Pioneer starts with $0, so a Pioneer's early version of the Ash line is raw Ash only until the first sales pay for the kettle; and the passive-only row is a ceiling on how much money can arrive while the player is away, not a promise that a player doing nothing covers the loan.

# Loan-term levers (D2) if income alone does not close the gap

D1 gave levers that are equal for all tiers: missed payments before loss (3), late fee ($75), interest rate (8%), and sell share (100%). If the re-measurement after Step 0 and Ash still shows the Pioneer well short, the options are, in order of how little they change the design: a higher missed-payments allowance for Pioneer only, a lower first-year rate for the hard tiers, or a lower principal for Pioneer. This doc does not propose numbers for these until the next measurement is in; the Homesteader's $38.23 a game day is expected to need them regardless.

# Proposed order (each step measured before the next)

1. **Ash first:** Ash by-product from burning Firewood, an ash pan, Ash in the market. It is the one source that costs the player no building material. Re-measure Settler and Pioneer for Spring and Summer Y1.
2. **Snare changes (Mike likes all five ideas):** the trapping pass is done (section 2). 2-catch snare, higher base odds and a spacing rule are small changes in TrapManager.cs. For a player who checks several times a day they add only about $1 to $2 a game day, so check the model against the ledger CSVs first and tune fur price through processed fur.
3. **Then choose** between the Potash kettle and leaching hopper, maple sugaring (size the tree species work first), and Workshop W1 and W2, using what the measurements say.
4. **Later-game market help:** "What sells well today" line, sell-surplus button with a keep-amount, logs in their own group. These matter once the player has a surplus, so they come after the early sources.

This respects Mike's measuring-first rule and keeps the Settler and Pioneer baselines clean after each change.

# Open questions for Mike

1. ANSWERED (2026-10-25): Mike did not sell logs, firewood, stone, fish or forage because he needed the materials for building and crafting. Follow-up answered (2026-10-25): what he did not eat of the fish and forage was saved as surplus, and the other spare item was sticks (see 1b). Final answer (2026-10-25): the fish and forage were kept as a food reserve, so they are not counted as early income; only surplus beyond a reserve is sellable.
2. What was the Settler parcel size and loan? The statement shows $54,000 after the $6,000 down payment.
3. Is $20 a game day the average to hit across a year, or the amount needed by game day 112 for the first payment?
4. Accept the small change to fire so that burning Firewood leaves Ash (proposed 1 Ash per 3 Firewood)?
5. Add the "What sells well today" line and the sell-surplus button to the T window?
6. Crafted goods: Option A, B, C or D above, and if C, what should an hour of game time be worth?
7. Maple sugaring: size the wood species work first, or leave it for a later slice?
8. ANSWERED (2026-10-25): Mike ran 6 snares, checked several times a day, and likes all five ideas (2-catch snare, higher odds, spacing rule, snare bait, fur quality). Still open: do you have the Spring ledger CSVs (End key) so I can check the snare model against real catches, and do you want a better fur price through processed fur (Tanned Small Fur) rather than a higher raw price?
9. How long should the next Pioneer run be? Fifty real minutes undersells that tier.
10. Leave the Homesteader's higher payment to the D2 levers?
11. Confirm this doc supersedes the 2026-10-20 "no new sellable goods needed" decision.

# Cross-references

- Trading_Post_Market.md: price pools, group drop percentages, floors, recovery, seasonal targets and the 2026-10-20 decision this doc supersedes.
- Workshop_and_Handmade_Goods.md: Ash, lye, potash, soap, tanning, W1 to W3, the log chain and leather (decisions 1 to 10 still open).
- Money_System.md and Income_Measuring_Plan.md: Phase D, the measuring plan and the first-round amendment.
- Measurements/Income_Measuring_Log.md: the Spring Year 1 statements this doc is based on.
- Trapping_System.md and Hunting_System.md: snare and hunting income (not re-read for this draft).
- Difficulty_System.md: tiers, loan principal and the D1 levers.
