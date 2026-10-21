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


---

# Claude Code read-only pass on the market build (2026-10-21)

Read only: nothing changed, committed or created, and Play Mode was not run. Items marked UNCONFIRMED were not verified from code. Code baseline b03ebc3.

## Findings that shape the build

- The `Homestead.Economy` assembly has no engine access (noEngineReferences). It can hold plain C# and [Serializable] structs (like `LoanSaveData`) but not a ScriptableObject or JsonUtility.
- The edit-mode test assembly references only Economy, SlotSwap, CreekCrossing and Setbacks. It cannot see TradingPost or any MonoBehaviour, so every rule worth testing must live in Economy.
- `SellPrice`, `Catalog`, `Buy` and `Sell` are called only from `TradingPostScreen.cs`. No existing test touches TradingPost (MoneyTests covers `Money.SellBackCents` and `TradingPostSaveData`). Leaving those two alone keeps all 222 tests valid.

## Group mapping

- Stock (19 items): 16 sell back at 40% (flint_and_steel, knife, canteen, bucket, cooking_pot, tarp, sleeping_bag, tent, axe, hammer, fishing_rod, recurve_bow, bolt_action_rifle, lamp_oil, lumber, weather_station). Arrows, rifle_rounds and cordage never sell back (under $3).
- Buys (55 items): firewood/logs/stone (3); meat and fish (18: raw and cooked venison, turkey, waterfowl, small game, chicken; raw and cooked bluegill, crappie, bass, catfish); hides, furs and the like (5: deer_hide, deer_antlers, small_furs, feathers, sinew); forage (12); crafted (12: stone_pick_axe, shovel, primitive_axe, rabbit_snare, box_trap, fish_trap, cane_pole, pouch, torch, bow_drill, lantern, primitive_bow); and no group (2: chicken_eggs, goat_milk). No id is in both lists. The existing ItemCategory is too coarse to derive groups, so groups must be explicit data.

## Build plan

- Data: one data asset (ScriptableObject `MarketData` in the main assembly, Resources/Market/) holds rows {itemId, group, pool, baseCents, kind} and group rows {drop, floor, four seasonal targets}; the plain structs live in Economy. An editor menu command seeds it from today's constants so no price is retyped. Scalars (recovery 25, sell-back 40, buy drift Winter +10% and Summer -5%) go flat on a new `MarketManager` component in Bootstrap, like MoneyManager. TradingPost stays about cash, buying and selling.
- Save: a multiplier per pool as an integer in basis points (10000 = 1.0) plus the last day processed, in a new section `player/market` owned by MarketManager. TradingPostSaveData does not change. Old saves have no section, so every multiplier is 1.0 and the first catch-up sets the last day to today. New games start each pool at its current-season target. `GameManager` needs a `ResetMarket()` next to `ResetLoan()` in both the New Game and Load paths.
- Time: a pure `Advance(state, fromDay, toDay, daysPerSeason)` moves each pool toward the season target one day at a time, in integer basis points with half-up rounding and a snap when the gap is tiny. One call over N days must equal N single-day calls. Same three triggers as MoneyManager (Update catch-up, a line in SaveManager.OnDayChanged before the autosave, CaptureState). The market runs whether or not there is a loan. Buy-side seasonal drift is stateless.
- Selling: unit by unit at the falling price. Price per unit = (base cents x multiplier + half) / 10000 rounded half up; new multiplier = max(floor, multiplier x (1 - drop)) rounded the same way. A `SaleResult` (count, total, first and last unit price, new multiplier) is also the "price actually paid" hook for the Phase B ledger. A non-mutating quote lets the T window show "Shift: all 40 for about $18.20". Sell-back is `Money.SellBackCents` of the base buy price times the multiplier, so at 1.0 it equals today's value.
- No-profit invariant: stocked targets are 1.0 and the cheapest seasonal buy is 0.95 of base, so a test over every stocked item in every season should prove sell-back is below buy.
- T window: a narrow trend column (a coloured + or - if the arrow glyphs do not render; UNCONFIRMED), a small "62% of normal" note in the name cell, a one-line market line under the header (pushes lists down about 28 px), a `MarketManager.Changed` event subscribed in OnShow and OnHide. The Sell list must stop sorting by current price (rows would jump under the cursor); sort by base price or name.
- Tests: about 35 to 45 new, taking 222 to roughly 260 to 270. New files about 6 code files, 1 data asset, 1 test file. Edited: TradingPost.cs, TradingPostScreen.cs, GameManager.cs (2 spots), SaveManager.cs (1 line), Bootstrap.unity.
- Recommended order: settle decisions; pure core in Economy with tests; data asset and seeder with completeness checks; MarketManager, Bootstrap, GameManager and SaveManager hooks; TradingPost Buy and Sell with SaleResult and quotes; T window; a short checklist and Mike's playtest recording money per game day; docs.

## Decisions needed before building

1. What a multiplier belongs to: one pool per item (selling venison would not depress turkey), or shared pools. The earlier Python model used group-wide pools. With a pool per item, a player who spreads sales over many items earns more than the model says, and a crafted-tool player can craft 12 different tools and dodge the pressure.
2. Eggs, goat milk and chicken meat fit no group (and chicken meat is farm meat, not game).
3. "Nothing sells in Winter" for forage: price 0, or just nothing in season.
4. Whether Sell-back is 40% of the base buy price (Claude Code's reading) or of the seasonal buy price.
5. Whether cooked meat and fish follow their raw item's curve and pool.
6. Whether a data asset table is acceptable under Mike's rule that tunables live on a component and never in nested serialized classes (Claude Code could not confirm what exactly that rule guards against).

Claude's defaults and recommendations are in the next section of this document once Mike has answered.


---

# Decisions on the build questions (Mike, 2026-10-21)

| Question | Decision |
|---|---|
| Pools | ONE POOL PER ITEM (Claude had recommended shared pools). Selling venison does not lower the price of turkey, and each crafted tool has its own pool. The pool is a field on each data row, so items can be moved into shared pools later by editing data. |
| Eggs, goat milk, chicken meat | A new FARM PRODUCE group: 5% drop, floor 0.25, target 1.0 all year (nothing seasonal, no livestock yet). |
| Forage in Winter | Price target stays 0.85 in Winter; a Winter shortage comes from nothing being in the world, not from a price of zero. Players can sell stored forage in Winter. |
| Data asset | Acceptable: scalars (recovery 25%, sell-back 40%, buy drift) sit directly on MarketManager in Bootstrap; the item and group table sits in one data asset. |

Claude's defaults (not asked, accepted unless Mike objects): sell-back is 40% of the BASE buy price (not the seasonal buy price) times the item's multiplier; cooked meat and fish share the pool and seasonal curve of their raw species (otherwise a player could sell raw and cooked fish as two separate price tracks); chicken meat uses the farm produce settings; the Sell list is sorted by base price or name, not current price; trend marker is a coloured + or - if the arrow glyphs do not render; new games start each pool at its current-season target.

## Simulation re-run for one pool per item (Claude's model, not played)

Same player types as before, but fish split over four species pools (45% bluegill, 31% bass, 18% catfish, 5% crappie), game goods over six item pools (venison, small game, small furs, turkey meat, feathers, deer hide), firewood one pool, forage one pool, crafted goods each in their own pool with a 10% drop and a 0.1 floor. No random events. Approved seasonal targets and 25% daily recovery.

| Player | Shared pools (decided earlier) | One pool per item |
|---|---|---|
| Survivor | $21 a day | $29 a day |
| Casual mixed | $38 a day | $50 a day |
| Grinder, lanterns only | $69 a day | $82 a day |
| Grinder, four crafted tools (30 a day split four ways) | not modelled | $99 a day |

Share of 28-day periods below each payment with one pool per item: the 5-acre payments ($459 and $573) are covered every period by every player; the 10-acre payment ($1,070) is short 100% of the time for the Survivor and 0% for the Casual mixed and Grinder (it was short half the time for the Casual player with shared pools); the 40-acre payment ($3,440) is still not covered by wild goods (the Grinder's median 28-day income is about $2,300 to $2,800).

What this means: one pool per item lifts income about 30 to 40 percent over the shared-pool model, which makes the 10-acre parcel comfortable for a casual player and the 5-acre parcel very comfortable. Crafting several different tools adds about 20 percent for the Grinder. The earlier target of about $25 a day for a typical player is now exceeded (Survivor $29, Casual $50). Because the pool is a data field, income can be pulled back by sharing pools (crafted tools in one pool, for instance) after the first playtest records real money per game day, without any code change.


---

# Market build report (Claude Code, 2026-10-21), NOT committed, not yet played by Mike

Built and compiled; 289 edit-mode tests pass (222 old, 67 new cases from about 45 methods, re-run after Play Mode); Play Mode check on a backed-up Saves folder (restored, 50 files, hashes identical) with 0 console errors or warnings. Parts of the pasted report were garbled by overlapping text.

Files created: `Economy/MarketCatalog.cs` (enums, row and group structs, SaleResult, the default catalog as pure data, MarketTable lookup and checks), `Economy/MarketRules.cs`, `Economy/MarketState.cs` (also `MarketSaveData`), `Managers/MarketData.cs` (ScriptableObject), `Managers/MarketManager.cs`, `Editor/MarketDataSeeder.cs` (menu: Homestead > Market > Seed MarketData), `Tests/EditMode/MarketTests.cs`, `Resources/Market/MarketData.asset`. Changed: `Money.cs` (SellBackCents(price, percent) overload; the old one-argument version calls it with 40), `TradingPost.cs` (price tables removed, asks MarketManager, Sell returns a SaleResult), `TradingPostScreen.cs`, `GameManager.cs` (ResetMarket(true) in New Game, ResetMarket(false) in Load), `SaveManager.cs` (OnDayChanged processes the market before the autosave request), `Bootstrap.unity` (+19 lines, additions only, edited with another scene loaded so Unity never rewrote it). Untouched: ProjectSettings.asset, AudioManager.cs, Homestead.slnx, 08_Backups, TradingPostSaveData, the loan code.

Data asset: 71 rows (52 bought, 19 stocked, 16 of which sell back) in 8 groups, 59 pools (71 rows, minus 3 never sold back, minus 9 cooked rows that share a raw pool). Firewood, logs and stone 3 rows; fish 8; game meat 8; hides and furs 5; forage 12; crafted 12; farm produce 4; stocked 19. Group settings as decided. Scalars on MarketManager in Bootstrap: recovery 25, sell-back 40, winter buy +10, summer buy -5, flat-trend band 3 (values read back after reload).

Base-price proof: the 19 stocked and 52 bought prices parsed from TradingPost.cs at commit b03ebc3 match the seeded asset to the cent (71 ids, 0 mismatches, 0 extra). A permanent test pins the totals ($255.00 stocked, $110.00 bought) and individual prices; another test requires the asset file to equal the defaults; another proves sell-back at 1.0 equals the old rule for all 19 stocked items. Correction to the earlier read-only report: the Buys dictionary has 52 items, not 55.

Play Mode (driven through TradingPost.Sell and the TimeManager methods behind F5 and F6, not key presses): new game starts at season targets (forage 0.85, rest 1.0); 17 firewood quoted $12.49, sold for $12.49, multiplier 10000 to 4995; 180 more units reached exactly 0.25 and then paid $0.25 each; recovery from the floor 2500, 4375, 5781, then 9437 at day 9; Summer shows firewood target 0.8 (trend down), tent $23.75, the Summer note; Winter (day 84) 11250, tent $27.50; save and load round-trip (last day 84, three multipliers, cash); an old slot loaded with all 59 pools at 1.0, last day set to today, no loan, then moved to 9500 after one day.

Claude Code's choices (all accepted unless Mike objects): the seeder fills from the pure MarketCatalog and the constants moved out of TradingPost.cs; a `seasonalBuy` flag marks the four survival-gear rows; recovery rounds half up toward positive infinity (a gap of -10 moves 2 basis points); the 3% trend band is 300 basis points absolute; "% of normal" shows only when not 100% and the Shift line only for stacks of 2 or more; flat marker is a dim =; markers are ASCII + - =; a saved game with a gap in its pools defaults them to 1.0; the market note is empty in Spring and Fall; the Buy list shows only stocked rows with the seasonal price; a bad MarketData asset falls back to defaults with a console warning; the 2.0 ceiling and the floors apply on load. Rows are 52 px instead of 44; the second line is 13 px.

Not covered: the Shift button was not clicked in the UI (the sale was checked through TradingPost.Sell and the quote text read from a screenshot); real income per game day was not measured; a hand-edited save with a stocked item above 1.0 could sell back above its buy price (normal play cannot).

Playtest: `06_AI_Collaboration\Market_Playtest_Checklist.md` (28 pass-or-fail items plus 4 notes).
