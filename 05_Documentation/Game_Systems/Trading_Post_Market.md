# Homestead
## Trading Post Market v0.1

Status: Design Draft (PROPOSED), written 2026-10-20. Nothing is built.

# Purpose

The Trading Post pays flat prices with no limits. The Income and Trading Post read-only pass (see `Money_System.md`) found casual income of about $60 to $150 per game day against a land payment of $16 to $20 per game day, and three loops that earn $50 to $100 or more a day for about five real minutes: fishing, firewood, and crafting lanterns from free clay and grass. A simulated market fixes this by making prices respond to what the player sells.

Mike's decisions (2026-10-20):

- The Trading Post uses a simulated supply and demand market. The first idea he raised was that prices fluctuate.
- Scope: the full market, meaning selling pressure, seasonal drift and random events.
- Crafted goods are capped. Claude's reading of "cap sales per day": crafted items get a steeper price drop, so the cap is soft instead of a wall. NOT yet confirmed by Mike.

His earlier picks ("comfortable, about $25 a day, raise sell prices and add new sellable goods") assumed income was too low. They are on hold. The $25 a day target may still stand for the 5-acre parcel; it will be re-checked once the market is simulated.

---

# How it works (PROPOSED)

## Price multiplier

Every sellable item has a base price (today's price) and a current multiplier. The price paid is the base price times the multiplier. The multiplier starts at 1.0, never goes below 0.25 or above 2.0, and is saved with the game.

## Selling pressure

Each unit sold multiplies that item's multiplier by (1 minus d). The drop d depends on the group:

| Group | Drop per unit sold (proposed) |
|---|---|
| Firewood, logs, stone | 4% |
| Raw and cooked meat, fish | 5% |
| Hides, furs, feathers, antlers, sinew | 5% |
| Forage (berries, nuts, mushrooms, onion, cattail) | 6% |
| Crafted tools, traps, lanterns | 10% |
| Stocked items sold back (40% of buy price) | 8% |

## Recovery

At the start of each game day, every multiplier moves 25% of the way back toward its target for the season. A glutted item takes about 8 to 10 game days to recover most of the way.

## Seasonal drift

Each group has a seasonal target multiplier (the level the price recovers toward). Examples, all proposed: firewood and logs 1.5 in Winter, 0.8 in Summer; furs and hides 1.3 in Winter; fish 0.9 in Summer; forage 0.85 in the season it is abundant; cooked food a little higher in Winter. The point is a store-or-sell decision across the year, as Economy_System.md already describes.

## Random events

About one event every 10 game days. An event picks a group and sets a temporary modifier for 3 to 7 game days: a glut (0.6 to 0.7) or a shortage and high demand (1.3 to 1.5). A line in the T window tells the player ("Hide buyers are paying more this week"). Only one event is active per group at a time. The combined multiplier is clamped to 0.25 to 2.0.

## Buying

Proposed: buy prices drift only slightly with the season (about plus or minus 10%) and do not react to the player's purchases. Open question below. Stock stays unlimited.

## What it does to income

A rough simulation (3% to 12% drop per unit, 25% to 50% daily recovery) gave these steady-state daily takes for a player who sells the same item every day:

| Case | Flat price | First day with a market | Steady state |
|---|---|---|---|
| Firewood, 50 a day | $50 | about $26 | about $8 to $15 |
| Lanterns, 30 a day | $120 | about $33 to $46 | about $8 to $24 |
| Fish, 15 a day | $30 | about $20 to $23 | about $7 to $16 |

A casual player who sells a handful of items is barely affected. A player who sells a mix of items earns much more than one who sells a single item. These are rough numbers for sizing the idea, not a tuned balance.

---

# Build requirements (PROPOSED)

- Money and prices move to cents (the Money System already needs cents as a long). Prices like $0.40 and $1.25 must be possible, because a $1 item at 0.6 pays $0.60.
- New saved state: multiplier per item (or group), active events, the random state or seed, and the last day processed. An old save loads with every multiplier at 1.0.
- The market must be computed from the day number (TotalDays), not from events alone, because `RestoreState` fires no events and sleeping can pass several days at once. After a sleep, all missed days of recovery and events are processed in order.
- The T window shows each item's current price and a trend arrow (up, down, flat) and the active event line.
- The Trading Post prices move out of code constants into one data asset so they can be tuned without a script edit.
- Tests: pressure and clamps, recovery, seasonal targets, event start, stop and stacking, determinism with the same seed, save and load, and a multi-day sleep.
- The ledger records each sale at the price actually paid.

# Open questions for Mike

1. Do crafted goods get the steeper drop (Claude's reading), or a literal daily sale limit?
2. Should buy prices react to the player's purchases (each purchase raises that item's price a little and it recovers), or only drift by season?
3. How strong should random events be? Proposed: glut 0.6 to 0.7 and high demand 1.3 to 1.5 for 3 to 7 days.
4. Should base prices also be trimmed, and should the $25 a day target for a casual 5-acre player stand? This is settled by simulating the market against the read-only pass numbers.
5. Fish and firewood: stay as they are, or lower the base price as well as adding the market?
6. Where does this sit in the order: before Phase A of the Money System, or alongside it? The market needs cents, which Phase A also needs.

# Cross-references

- `Money_System.md`: land payment, income and Trading Post read-only pass results.
- `Economy_System.md`: surplus management (store or sell), economic sinks.
- `Difficulty_System.md`: starting kits and the Trading Post's original design.
- `Season_System.md`: season lengths used by the seasonal targets.


---

# Simulation results (2026-10-20, Claude's model, NOT played)

Method: a Python model of the market as described above, using the group base prices and units from the Income and Trading Post read-only pass. Three simulated players sell everything they gather every game day. Each result is the average of 60 random three-year runs (year one dropped as warm-up), with the proposed drops, 25% daily recovery, the seasonal targets, and events (10% chance a day, 3 to 7 days, 0.6 to 0.7 or 1.3 to 1.5).

Players modelled (units sold per game day): Survivor = 15 fish, 20 firewood, 4 game goods, a little forage. Casual mixed = 30 fish, 60 firewood, 7 game goods. Grinder = 42 fish, 100 firewood, 8 game goods, 30 lanterns. Fishing slows to 38% in Winter. These are Claude's guesses at behaviour from the read-only pass numbers (UNCONFIRMED in play), and they assume the player sells everything and keeps none to eat or burn, so real income will be somewhat lower.

## Average income per game day

| Player | Flat prices today | Market only | Market, base prices x0.6 | Market, base prices x0.5 |
|---|---|---|---|---|
| Survivor | $51 | $21 | $13 | $11 |
| Casual mixed | $119 | $38 | $23 | $19 |
| Grinder | $297 | $85 | $51 | $42 |

The earlier target was about $25 a game day for a typical player. The market alone puts the Survivor at $21 and the Casual mixed player at $38, so it lands around the target with no base-price trim.

## Against the land payments (market only, base prices unchanged)

A payment is $459 (5 acres, 20% down), $573 (5 acres, no down payment), $1,070 (10 acres) or $3,440 (40 acres) every 28 days. Share of 28-day periods where income falls below the payment:

| Parcel | Survivor | Casual mixed | Grinder |
|---|---|---|---|
| 5 acres, 20% down ($459) | 0% | 0% | 0% |
| 5 acres, no down payment ($573) | 25% | 0% | 0% |
| 10 acres ($1,070) | 100% | 49% | 0% |
| 40 acres ($3,440) | 100% | 100% | 100% |

Median 28-day income: Survivor $601, Casual mixed $1,078, Grinder $2,399.

## Sensitivity (average $ per game day, market on)

| Setting | Survivor | Casual mixed | Grinder |
|---|---|---|---|
| Proposed (25% recovery, drops as listed) | $21 | $38 | $85 |
| Faster recovery (50% a day) | $30 | $49 | $100 |
| Slower recovery (15% a day) | $17 | $33 | $79 |
| Gentler drops (half) | $28 | $45 | $94 |
| Harsher drops (1.5 times) | $19 | $35 | $81 |
| No events, no seasons | $20 | $37 | $84 |

## What the numbers say

- The market alone fixes the imbalance. It cuts the grinder from $297 to $85 and the survivor from $51 to $21, without touching base prices.
- Base-price trims are not needed for the 5-acre parcel. At x0.6 the Survivor drops to $13 a day, below the $16 to $20 a day the 5-acre payment needs.
- The 10-acre parcel is a stretch: only a player earning above Casual mixed covers it every period. The 40-acre parcel cannot be paid from wild goods at all, so it needs livestock, produce and other later income. That matches the intended design.
- Seasonal drift and random events barely move the AVERAGE (the Casual mixed player's 28-day income only ranges from about $1,020 to $1,120 with them, against $900 to $1,110 without). They add flavour and some swings, but as proposed they are too weak to feel. If Mike wants events to matter, they need to be stronger or to hit the groups a player depends on.
- The price floor matters more than the drop rate. The lantern grinder earns about $32 a day at either a 10% or a 15% drop, because the 0.25 floor holds a saturated item at a quarter of base price (30 lanterns at $1 each is about $30). A lower floor (for crafted goods, say 0.1) would shut the loop down harder.
- Recovery speed is the biggest dial: 15% to 50% a day moves the Survivor from $17 to $30.

## Caveats

This is a model, not a playtest. It ignores carry weight (firewood is 1.5 kg a piece, so 100 a day is not carryable in one trip), stamina, the time needed to gather, what the player eats and burns, and learning. Real income will probably be lower than the model for most players and higher for a few skilled ones. The first playtest after the market is built should record actual money earned per game day.


---

# Decisions after the simulation (Mike, 2026-10-20)

These answer open questions 2 to 5 above and supersede the proposed values where they differ.

| Question | Decision |
|---|---|
| Base prices | NO trim. The market alone sets income. Base prices stay as they are today. |
| Crafted goods | Crafted tools, traps, torches and lanterns drop 10% per unit sold and have a price floor of 0.1 of base (every other group keeps the 0.25 floor). |
| Random events | NOT in the first version. Selling pressure and seasonal drift only. Events can be added later. |
| Buy prices | They drift only slightly with the season (about plus or minus 10%) and do not react to the player's purchases. Stock stays unlimited. |

Still open: question 1 (Claude's reading that crafted goods use a steeper drop and a lower floor stands unless Mike says he wants a literal daily sale limit), question 6 (build order), exact seasonal targets, and exact recovery speed (proposed 25% a day).

## Re-run with the decided settings

Claude re-ran the model with a crafted floor of 0.1, no events, no base-price trim, 25% daily recovery. Average income per game day: Survivor $21, Casual mixed $38, Grinder $69 (down from $85). 30 lanterns a day alone earns about $17 a day (against $120 flat and about $32 with the 0.25 floor). Share of 28-day periods short of the payment: 5 acres at $459, none for any player; 5 acres at $573, Survivor 25%; 10 acres at $1,070, Survivor 100%, Casual mixed 50%, Grinder 0%; 40 acres at $3,440, all 100%.

Correction: the chat message that offered this choice said 30 lanterns would earn about $12 a day with the 0.1 floor. The simulation gives about $17, because the multiplier recovers a little each day. The ranking of the options does not change.

## What changes in the Money System

- Early income (the earlier "raise prices and add new sellable goods" picks) is replaced by this market. No new sellable goods are needed for Phase A.
- The target of about $25 a game day for a typical player is met by the market alone in the model (Survivor $21, Casual mixed $38). It must be confirmed in a playtest.
- The Money System Phase A needs cents, and the market needs cents, so they should be built together or cents first (question 6).


---

# More decisions (Mike, 2026-10-20)

- Crafted goods: NO literal daily sale limit. The steeper drop (10% per unit) and the 0.1 floor stand.
- Recovery: 25% of the gap back toward the target each game day. Confirmed.
- Build order: CENTS FIRST. The money type is changed to cents (a long) as its own step, with no gameplay change, before the market and before the rest of the Money System Phase A.
- Seasonal targets: Mike asked what they are, Claude explained them, and Mike APPROVED the table below (2026-10-20). They can still be tuned when the market is built.

Seasonal targets (APPROVED) (price level each group recovers toward; 1.0 is today's price): firewood, logs and stone 1.0 Spring, 0.8 Summer, 1.0 Fall, 1.5 Winter; fish 1.0, 0.9, 1.0, 1.0; hides, furs and game meat 1.0, 1.0, 1.0, 1.3; forage 0.85 in Spring, Summer and Fall (the abundant seasons), nothing sells in Winter; crafted goods and stocked sell-backs 1.0 all year. Buy prices: survival gear (tents, sleeping bags, tarps, lamp oil) about 10% dearer in Winter and 5% cheaper in Summer; everything else unchanged.
