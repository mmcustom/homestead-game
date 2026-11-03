# Homestead
## Workshop and Handmade Goods v0.1

Status: Design Draft (PROPOSED), research and first-pass list written 2026-10-24. Nothing is built, and nothing here is decided. Every number is a first pass for Mike to cut down.

# Why this exists

Mike (2026-10-24, while playing the income measuring sessions): "I'm thinking we need to introduce more profitable things to craft and sell in the early stages to make enough money to pay the bills. Perhaps we move forward with a new workshop structure to craft wood, clay, straw, and furs and hides. Research the internet and see what are viable crafts and skills used by beginning homesteaders to generate income, then create a list of things needed to craft or create those things."

This matches the numbers. The Trading Post market model (Trading_Post_Market.md) put a "Survivor" at about $21 a game day against a 5-acre payment of $16 to $20 a game day, and crafted goods were deliberately capped (10% price drop per unit, floor 0.1) to kill the lantern loop. So early money is tight by design. Handmade goods that need real inputs, a station and game time are a way to raise early income without reopening the lantern problem.

# What real beginning homesteaders actually do (research, 2026-10-24)

**What the search found, and what it did not.** General "make money homesteading" lists are dominated by food, plants, eggs, soap, salves, mushrooms and blogs, not by wood, clay, straw or hides. Prices are the authors' local examples and startup costs are mostly missing. I found no solid real-world price for hand-split shakes, straw baskets, hand-made bricks, pottery or leather goods, so the game prices below are balance numbers, not real prices.

**Income ideas from homestead sources that are made things (with the price evidence found):**

- Cold process soap about $8 a bar at a local co-op; melt-and-pour soap needs no lye. Salves, tinctures (up to about $15 an ounce), dried herb bundles, fire starters.
- Wood items named by homesteaders: raised beds, compost bins, trellises, planters, coops, brooder boxes, rustic decor from salvaged wood (beginner to intermediate, scrap wood and basic tools). Shiitake on hardwood logs sells at $8 to $12 a pound and the logs fruit for years.
- Handmade brooms sell for about $21 (small braided whisk) to $65 (hearth broom) and up; makers use broomcorn and plant fibers, cord and wooden handles.
- Straw coil work: a bee skep takes about two days of repetitive work with straw or grass and a cane binding (comb, coil, stitch). No sale price found.
- Fur: 2025 raw pelt prices are weak for common animals (raccoon averages about $5, beaver $10 to $25, coyote $10 to $30, red fox $10 to $25, muskrat about $2) and strong only for scarce species. Raw fur is a poor product; value comes from processing.
- Hides: fleshing a hide takes a beginner over an hour; salt or freeze it to keep it; brain tanning is the classic small-scale method, but do not brain-tan a deer without a chronic wasting disease test.

**Frontier and Ohio history (what settlers made, sold and bought):**

- Western Reserve pioneers built log cabins and furniture, spun and wove cloth, made candles and maple syrup, raised and sold pigs and grain, and bought what they could not make: tools, salt and gunpowder. Towns added shoemakers, gristmills and sawmills.
- **Ashes were the first cash crop for clearing settlers.** Felled hardwood was burned, ashes were leached with hot water into lye, and the lye was boiled down to potash ("black salts"). About 30 cords of wood gave one ton of ashes, and one ton of ashes gave about a sixth of a ton of potash. Home ashes sold for about 12 cents a bushel and field ashes 5 to 8 cents; asheries bought potash and leased kettles for about $1 a month. Lye for soap came from the same ashes (wood ash, water and lard).

**How the clay, straw, wood and hide crafts actually work (this is what the recipes below copy):**

- **Clay pots:** dig clay from creek banks, test it (a wet rope should tie in a knot), mix 1 part sand or crushed quartz temper to 2 parts clay, shape a pinch pot first (coil pots come later), dry it for weeks until bone dry, then fire it in a pit or bonfire to about 1,112 °F (600 °C). Heat it slowly, bury it in the fire, cool completely. Wet clay steams and cracks pots; fast heating or wind cracks them. A fingernail flick should ring.
- **Bricks:** tread and knead the clay, mould it in a wooden mould lined with sand (one brickmaker moulds about 2,000 a day), dry 2 to 3 days flat then on edge for about a week, stack into a clamp with fire tunnels, fire 4 to 5 days. About one cord of wood per 1,000 bricks. Expect about 20% unusable; underfired bricks crumble in rain and overfired ones fuse.
- **Hand-split shakes:** a froe and a mallet split short cedar (or other straight-grained) rounds. Fast and simple once you have the two tools.
- **Spoons and bowls:** straight green branch of basswood, birch, ash or alder; split and rough out with an axe, finish with a knife (a hooked spoon knife helps); dry slowly so it does not crack; finish with food-safe oil.
- **Straw and fibre:** straw or tall grass combed straight, kept supple with water, coiled with a girth and stitched with split cane or similar; brooms bind broomcorn or fibre to a wooden handle.
- **Hides:** salt or keep the green hide; flesh it; remove the hair (ash lye); tan (brain, bark or similar); soften by working it; smoke it to keep it soft when wet. Makes pouches, gloves, moccasins, belts and hats.

**Skills a beginning homesteader leans on (what the game should reward, not as an XP tree):** greenwood carving, riving (splitting) with a froe, coppicing and basket weaving, cordage and rope, fleshing and tanning hides, pinch and coil pottery, brick moulding and clamp firing, lye, soap and candle making.

# What the game already has (so nothing is designed twice)

- **Raw materials in the world:** logs, firewood, sticks, branches, stone, clay (never runs out), tall grass (regrows in 3 days), cattail, deer hide, small furs, sinew, feathers, antlers.
- **Crafting today (Crafting_System.md):** a Craft and Build screen on B, 15 recipes, all instant, no station, no timer, no skill. Crafting_System.md and Decisions_Log (2026-09-25) say stations, skills and durability stay post-Alpha. **A workshop breaks that rule on purpose, so it needs Mike's yes (decision 1 below).**
- **Timed jobs already exist:** cooking, boiling and wood splitting use a queue with progress and a Cancel button. Workshop jobs can reuse that pattern.
- **The Workshop is already on the roadmap** (Structures_Roadmap.md, Group 5, Slice 6): "tool repair and a future crafting space". This draft proposes moving a first version much earlier. The Drying rack in Group 2 is also needed here.
- **What the Trading Post pays today:** deer hide $8, small furs $3, antlers $6, sinew $1, firewood $1, log $3, stone $1, lantern $4, pouch $3, tool crafts $1 to $4. Clay, tall grass, sticks, branches and cordage are not bought. Lumber is sold in the store ($8) and has no use yet.
- **The market (committed 0f8e336):** every item has its own price pool, so selling many different goods earns more than selling one good over and over. Crafted tools, traps and lanterns drop 10% per unit sold with a floor of 0.1; everything recovers 25% of the gap each game day.

# The proposal: a Workshop shed plus yard stations

Real crafters do not do everything under one roof. Hides, kilns and fires stay outside; benches stay dry. So the proposal is one covered building and three outdoor stations, all built with the existing Build tab and placement ghost, all staged so Mike can stop after any step.

| Piece | What it is | Handles | Notes |
|---|---|---|---|
| **Workshop shed** | A roofed work space with a Woodworking bench (shaving horse) and a Fiber bench. A small staged site like the Wood Shed from Slice 1. | Wood and straw goods | The "workshop structure" Mike asked for |
| **Drying rack** | An open rack under a roof edge | Dries clay ware, straw, hides, bowls (drying is a real wait) | Already in Structures_Roadmap Group 2 |
| **Tanning frame and soak tub** | A stretching frame plus a barrel (the existing Water Barrel can be the tub) | Hides and furs | Outdoors, away from the kitchen and water (smell and water safety) |
| **Clay table and clamp kiln** | A wedging table and an earthen mound kiln | Clay ware and bricks | Fire and smoke: keep away from buildings, a placement rule like the outhouse and well |

**Recommended build order (profit for effort):** (W1) Workshop shed with wood and fiber benches, because logs and grass are plentiful and no new gathering is needed; (W2) Tanning frame, because the player already hunts and a hide goes from $8 raw to about $22 tanned, the biggest value-add on the list; (W3) Clay table and kiln, because clay is free but needs fuel, drying days and temper, so it is the slowest and riskiest.

**How crafting would work (proposed):** a workshop recipe uses a station, a tool, ingredients and game time. It runs in a queue like cooking, so the player can walk off and hunt or chop while it works. Ingredients come out of the pack when the job starts, the result is collected at the station. No skill XP and no recipe discovery: the gate is having the station and the tool, which matches the game's existing "tool and place" feel. Clay and bricks add a small failure chance (about 10% to 20% in real life) when the load is rushed or underdried.

# The list: things to craft, and what each one needs

All prices are proposed base prices in game dollars, to be run through the market. "Inputs at store value" uses what the Trading Post pays today (log $3, firewood $1, deer hide $8, small fur $3); free inputs (branch, tall grass, cattail, clay) count as $0. "Time" is game time while the job runs. Nothing here is confirmed.

## Woodworking bench (tools: Knife and Axe; Froe and Maul for splitting)

| Item | Inputs | Time | Base price | Inputs at store value |
|---|---|---|---|---|
| Wooden Spoon | 1 Branch | 20 min | $3 | $0 |
| Wooden Bowl | 1 Log | 1 h, then 1 day to dry | $7 | $3 |
| Cutting Board | 1 Log | 1 h | $8 | $3 |
| Split Shakes (bundle of 10) | 1 Log | 1 h | $9 | $3 |
| Fence Rails (bundle of 4) | 2 Logs | 2 h | $12 | $6 |
| Rough Stool | 2 Logs, 2 Branches | 3 h | $14 | $6 |

Shakes and rails are also building materials (a roof and fence supply that does not use the $8 store Lumber), which fits Slice 5's fencing and pasture. A later cooperage item (staved bucket or barrel) needs iron hoops or split withy bands.

## Fiber bench (tools: Knife; materials: tall grass, cattail, later willow withies and bark)

| Item | Inputs | Time | Base price | Inputs at store value |
|---|---|---|---|---|
| Woven Basket | 6 Cattail (or 8 Willow withies later) | 2 h | $8 | $0 |
| Grass Hat | 8 Tall Grass | 1.5 h | $6 | $0 |
| Sleeping Mat | 12 Tall Grass, 2 Cordage | 2 h | $8 | $0 |
| Twig Broom | 1 Branch, 8 Tall Grass, 1 Cordage | 1 h | $6 | $0 |
| Thick Rope | 3 Cordage | 30 min | $9 | $0 |
| Thatch Bundle | 10 Tall Grass | 30 min | $3 | $0 (building material) |
| Straw Skep (later) | Rye straw, cane or bark, comb and girth | 2 days of work in real life | $8 | needs a grain field and beekeeping |

Tall grass regrows in 3 days, so fiber goods are the cheapest to feed, and the market's per-item pools mean six different goods earn far more than one.

## Tanning frame and soak tub (tools: Knife as a fleshing knife; Ash Lye; Water Barrel)

| Item | Inputs | Time | Base price | Inputs at store value |
|---|---|---|---|---|
| Tanned Deer Hide | 1 Deer Hide, 1 Ash Lye, bark or brains | about 3 days (flesh 1 h of work, soak 1 day, soften 2 h, smoke 1 day) | $22 | $8 |
| Tanned Small Fur | 1 Small Fur, 1 Ash Lye | about 2 days | $7 | $3 |
| Leather Gloves | 2 Tanned Small Fur, 1 Cordage | 2 h | $14 | $6 |
| Fur Hat | 4 Tanned Small Fur | 3 h | $14 | $12 |
| Leather Belt and Sheath | 1 Tanned Deer Hide | 2 h | $10 | $8 |
| Moccasins | 1 Tanned Deer Hide, 1 Sinew | 3 h | $20 | $9 |

Gloves, hat and moccasins also keep the player warm in Winter, so they serve self-sufficiency first and sales second. Tanning brains from deer carry a chronic wasting disease warning; bark tanning (oak bark is common in Ohio) is the safer game default.

## Clay table and clamp kiln (tools: wooden paddle and smooth stone; fuel: Firewood)

| Item | Inputs | Time | Base price | Inputs at store value |
|---|---|---|---|---|
| Clay Cup | 1 Clay, 1 Temper | dry 2 days, then fire | $4 | $0 |
| Clay Bowl | 2 Clay, 1 Temper | dry 3 days, then fire | $6 | $0 |
| Cooking Pot (clay) | 3 Clay, 1 Temper | dry 4 days, then fire | $10 | $0 |
| Water Crock | 4 Clay, 1 Temper | dry 5 days, then fire | $12 | $0 (also stores water) |
| Brick batch (20) | 10 Clay, 1 Temper | mould, dry about 7 days, then fire | $6 | $0 (also hearths, chimneys, kiln) |
| Lantern (existing) | 2 Clay, 1 Cordage | moves to the kiln chain | $4 | $0 |

A kiln firing burns Firewood (proposed 10 for a small load) and takes a game day or more; a rushed or underdried load has about a 10% to 20% chance to lose pieces.

## Soap and candles (optional fifth bench, historically the early cash)

| Item | Inputs | Time | Base price |
|---|---|---|---|
| Ash Lye | 4 Ash, water, leaching tub | 1 day | used in tanning and soap |
| Lye Soap | 1 Ash Lye, 1 Tallow | 2 h, then cure | $5 |
| Tallow Candle | 1 Tallow, 1 Cordage | 30 min | $3 |

# What it needs that the game does not have yet

**New gatherable or by-product items**

- **Ash:** a by-product of burning Firewood in a campfire or hearth (needed for lye, tanning and soap). The one small change to existing fire code.
- **Temper (sand or crushed stone):** for clay. Easiest is crushed Stone (the game has about 520 stones), or sand from the creek bank as a new gatherable.
- **Tallow (animal fat):** from hunted animals, for soap and candles. A new by-product: Hunting_System.md mentions offal (for compost) but not fat.
- **Bark strips (oak or hickory) and willow withies:** for bark tanning and baskets; both are on the property in real life.
- **Rye straw:** only with grain growing (Gardening slice), so straw skeps and straw bedding wait for that. Tall grass covers the early straw goods.

**New tools (all craftable from what exists)**

- **Froe and Maul** (split shakes and rails): wood and a stone or later iron blade. This is the one tool whose blade wants iron, so a stone-and-hardwood first version is needed.
- **Brick mould and paddle and anvil stone** (clay): wood frame and a smooth stone.
- **Antler Awl and bone needle** (leather): uses antlers, which sell for only $6 today.
- **Fleshing beam** (a log with a smooth face) or just the Tanning Frame itself.

**New structures (Build tab, existing placement ghost, staged site like the Small Cabin)**

Workshop shed, Drying rack, Tanning frame, Clay table, Clamp kiln. Placement rules: kiln and tanning frame away from buildings and from the well and water; kiln not under a roof.

**New or extended systems**

- A **workshop job queue** with progress and Cancel, reusing the cooking and splitting queue.
- **Failure chance** for clay firing, with a message that says why (too wet, too fast) so it feels fair.
- **Market entries** for every new good (the market already supports per-item pools and groups); a decision on a new group (decision 4).
- Save data for jobs in progress, same shape as the cooking queue.
- Tests in the Economy assembly for recipe costs and the pricing table, so a price cannot silently break the balance.

# How much money could this add

Using the market simulation (25% daily recovery, crafted group: 10% drop per unit, floor 0.1), a good's steady daily take is about 0.77 times its base price for 1 sold a day, 1.2 for 2, 1.5 for 3, 1.7 for 4 and about 2.1 for 8. So extra income comes from selling several different goods, not from selling many of one.

| Player | What they make each game day | Added income per game day |
|---|---|---|
| Light | 2 kinds of good, 1 of each, average base $8 | about $12 |
| Casual | 3 kinds, 2 of each, average base $8 | about $29 |
| Heavy | 8 kinds, 3 of each, average base $9 | about $107 (not realistic) |

The heavy figure ignores real limits: job time, one job at a time per bench, and scarce inputs (a deer gives one hide, so tanned hides are about one every few days, worth about $5 a day). Realistic early gain is probably about $10 to $25 a game day, which would lift the Survivor from about $21 to about $31 to $46 against a 5-acre payment of $16 to $20.

**Risks to watch**

- Fiber goods use free grass, so they are the likeliest to turn into a new lantern loop. Controls: per-item pools, the market floor, job time, one job at a time, and starting prices on the low side. Test with the measuring sessions.
- Clay is free and endless; fuel, drying days and failure are what limit it. Without those it is the lantern loop again.
- Adding income before D2 measurement muddies the baseline. Keep the current build as the baseline for the Settler, Pioneer and Homesteader sessions, and add the workshop as its own slice after the numbers are in.

# Decisions for Mike

1. **Break the "no stations, instant crafting" rule for workshop goods?** Recommended: yes, only for these new goods; the existing 15 recipes stay instant. If no, the same list can be instant recipes with a bigger cost sink, but you lose the "walk off while it works" feel.
2. **Scope and order:** W1 Workshop shed (wood and fiber), W2 Tanning frame, W3 Clay table and kiln, optional soap and candles. Recommended as listed; stop after any step.
3. **Where it sits in the build order:** before D2 tuning (so D2 tunes the whole economy) or after (so the baseline stays clean)? Recommended: finish the three measuring sessions first, then build W1 and W2, then re-measure and do D2 once.
4. **New market group for handmade goods:** keep them in the existing crafted group (10% drop, floor 0.1, harsh) or give them a milder "Handmade wares" group (about 6% drop, floor 0.25)? Recommended: start in the crafted group and loosen only if measuring shows the income is too thin.
5. **Which items to keep:** the list above is about 30 items. Cut it to a first set (suggested 10 to 12 across the three benches) before any build.
6. **Ash and tallow:** accept the small change to fire and hunting to produce Ash and Tallow, or leave tanning and soap out of the first version.

# Cross-references

- Trading_Post_Market.md: per-item pools, groups, floors and recovery used for the income numbers.
- Money_System.md: the income and Trading Post read-only pass, Phase D and the measuring plan.
- Crafting_System.md: the current Craft and Build screen and the "no stations" decision this draft asks to bend.
- Structures_Roadmap.md: Workshop (Slice 6), Drying rack (Group 2), staged build sites.
- Hunting_System.md and Trapping_System.md: hides, furs and sinew (fat and ash would be new by-products).
- Economy_System.md: Early Survival and Early Homestead revenue tiers.

# Sources (research 2026-10-24)

- Craft Council, "Clean Sweep" (handmade brooms, materials and prices): https://craftcouncil.org/magazine/article/clean-sweep
- Practical Self-Reliance, homestead extra income (soap $8 a bar, tinctures, shiitake, crafts): https://practicalselfreliance.com/homestead-extra-income/
- 104 Homestead, uncommon ways to make money (wood and fibre product ideas): https://104homestead.com/how-to-make-money-homesteading/
- The Free Range Life, 32 ways to earn money on a homestead: https://thefreerangelife.com/32-ways-earn-money-homestead/
- Trapping Today, 2025 fur prices: https://trappingtoday.com/2025-fur-prices-trapping-todays-fur-market-report/
- Outdoor Life, primitive ceramics (clay, temper, firing): https://www.outdoorlife.com/primitive-ceramics-how-to-make-your-own-clay-pots/
- Virginia Places, making bricks (moulding, drying, clamp firing, fuel, losses): https://virginiaplaces.org/architecture/makingbricks.html
- Reynolds, "Making Potash" (ashes to lye to potash, quantities and prices): https://reynoldstonnewyork.org/making-potash/
- National Park Service, Western Reserve pioneers (what Ohio settlers made and bought): https://www.nps.gov/cuva/learn/kidsyouth/western-reserve-pioneers.htm
- Mental Floss, household items every frontier family owned (lye soap from ash and fat): https://www.mentalfloss.com/history/household-items-every-frontier-family-owned
- Modern Farmer, how to build a bee skep (straw coil work): https://modernfarmer.com/2013/05/how-to-build-a-bee-skep/
- Northern Woodlands, greenwood spoon carving (woods, tools, drying): https://northernwoodlands.org/articles/article/greenwood-spoon-carving
- Grit, how to make cedar shakes by hand (froe and mallet): https://www.grit.com/uncategorized/how-to-make-cedar-shakes-by-hand/
- Grit, tanning leather (fleshing, salt, steps, safety note): https://www.grit.com/farm-and-garden/do-it-yourself/tanning-leather-zm0z23ndzols/


---

# Addendum 2026-10-24 (Mike): logs to a lumberyard, a lumber mill, and leather

Mike's two additions: (1) sell logs to a lumberyard for money until the player can build a lumber mill, which then makes boards and planks for building other structures; (2) a hide tanning station that turns hides into leather, used to craft other items. Both fit this draft. The research behind (1) is below.

## What the game already has for logs and lumber

- **Logs already sell at the Trading Post for $3 each** (Trading_Post_Market group "Firewood, logs and stone": 4% price drop per unit, floor 0.25, seasonal target 1.5 in Winter and 0.8 in Summer). So the "lumberyard" exists today as the T window; it is a buyer, not a place.
- **Lumber already exists** (5 kg, buys at the Trading Post for $8, sells back at 40%, nothing consumes it yet). Mike's 2026-09-26 decision stands: a board is a type of lumber, lumber comes from a lumber mill and is bought at the Trading Post until the player can buy or build a mill, boards are used for most buildings, and boards inherit quality from the wood (pine, birch, oak, walnut in rising quality).
- **Not built:** the Lumber Mill, wood species and quality on trees, and any use for Lumber. The Small Cabin deliberately uses logs, branches and tall grass and needs no lumber.
- **Finite trees:** 8,852 trees with no regrowth, so logs are a limited income and also the clearing of the land (settlers cleared trees for fields and burned the rest for ashes).

## What the research found on logs and lumber

- **Ohio sawlogs are priced per board foot by species and grade** (Ohio Timber Price Report, Fall 2025, delivered sawlogs per thousand board feet): black walnut about $2,317 (prime up to about $3,260), white oak about $1,150, hard maple about $825, red oak about $596, cherry about $585, yellow-poplar about $524, hickory about $464. So walnut is worth about four times red oak, white oak about twice red oak, and poplar and hickory slightly less than red oak. That is the same order as Mike's quality ladder and gives realistic price multipliers for species.
- **Grade matters inside a species:** prime logs sell at roughly 1.7 to 2.9 times No. 2 common (the lowest grade listed), for example red oak $780 against $360 and white oak $1,500 against $510. A log's price should depend on its species and its size or quality, not just that it is a log.
- **Sawing alone is a thin margin.** A portable-sawmill article puts simple cutting at about 16 cents a board foot, and kiln drying adds 20 cents to $1. A sawlog is worth roughly 50 to 60 cents a board foot, so a mill adds about a quarter to a third to the value; the real gain is making boards you would otherwise have to buy.
- **Pioneers had no sawn lumber at first.** They built log cabins, split shingles and made puncheon floors from split logs. The first sawn boards came from a two-man pit saw, about 12 boards a day (about 100 linear feet) for two men. Early water-powered sash mills cut about 500 linear feet a day, and water-powered sawing could reach about 200 boards a day. Mills spread quickly on the frontier (the source is about LeRoy, New York, not Ohio: a land agent offered loans in 1802 to settlers who would build sawmills, and the town had five mills by 1825).
- **Settlers often paid the miller with a share of their logs.** One 1806 mill (also in the LeRoy, New York area) sold boards for $7.50 per thousand feet and took logs as payment for sawing.

## Proposal: the log chain (three steps, each optional)

1. **Now: sell logs at the Trading Post, as today.** Two small changes worth considering. Put logs in their own market group, separate from firewood and stone, so their drop, floor and season can be tuned alone. Later add wood species and quality, using the real ratios above (walnut about 4, white oak about 2, red oak 1, poplar and hickory about 0.8 to 0.9) so a better tree is worth chopping. Income from raw logs is capped by the market: with a 4% drop and 25% recovery, selling 4 logs a day earns about $8 a game day, 8 logs about $11 and 12 logs about $13 (the simulation, base $3).
2. **Lumberyard sawing service at the Trading Post (the "until we can build our own mill" bridge).** In the T window the player brings logs and pays a fee in cash or a share of the logs to have them sawn into boards, which matches how pioneers used the local miller. No new building and no tool: it gives boards from day one so the later building slices are not blocked by the $8 store price. Proposed: 2 logs in, 1 board out (the other log is the fee), or $2 cash per log; a game day to collect.
3. **Saw pit, then a water-powered lumber mill.** The Saw Pit is a cheap early station and a whipsaw (steel, so bought at the Trading Post like the Axe): 1 log in, 1 board out, about 1 game hour a board. The Lumber Mill is the later big step: a sash mill on the creek bank (real mills used water power), 1 log in, 2 boards out, about 10 game minutes a board, and it can take other people's logs for a fee. The creek flood zone already exists, so a mill at the water's edge has a real flood risk, which is a good placement rule. A steam or later powered mill is a later tier, not needed now.

**Keeping it from becoming a money loop:** sawing is a thin margin in real life, so the board's sell-back should stay close to the log's price (today $8 Lumber sells back at 40% = $3.20 against a $3 log). The reward for milling is skipping the $8 store price and building more, not selling boards. Boards from better species inherit their price multiplier.

**What boards and planks are for (so the mill has a reason to exist):** most later buildings (Storage Shed, Barn, Chicken Coop, Cottage and up), fence rails and gates, stools and tables, workshop benches, and replacing the kitchen's log-and-branch planks. The Small Cabin, Lean-To and Wood Shed keep their log, branch and grass recipes.

## Leather and the hide tanning station

The Tanning frame and soak tub in W2 above are the hide tanning station. New point from Mike: **Leather becomes a real intermediate material** that other crafts consume, not just something sold.

- **Leather** (new material): 1 Deer Hide becomes 1 Leather (about 3 game days: flesh, soak with ash lye, soften, smoke). Small Furs become Tanned Fur. Both are sold or used.
- **Things made from leather** (all at the Fiber bench or a Leather bench, with an Antler Awl and Cordage or Sinew): Leather Pouch (upgrade of today's raw-hide Pouch), Leather Satchel (a real carry-capacity step), Moccasins or boots (warmth and a sprain or wet-feet effect if wanted), Gloves, Belt and Sheath, Quiver, Waterskin (a Canteen alternative), Fur Hat and Fur Lining, and rawhide strips as a sinew or cordage substitute (rawhide needs no tanning, only drying).
- **Why it matters for survival, not just money:** a deer already gives meat, hide, sinew and antlers. Leather finishes that chain, so Winter clothing and carry gear can be made, not bought. Pioneer difficulty starts with almost no kit, so leather goods are its way to catch up without the Trading Post.
- **Real-world note:** brain tanning takes days, bark (oak) tanning takes weeks to months in real life, and the game compresses either to a few days. This timing is general knowledge, not sourced this round. A deer-brain tan carries a chronic wasting disease warning, so bark or a plain "tanning solution" is the safer default.

## What changes in the decisions list

Add to the six decisions above: **7. Log chain:** keep selling logs at the Trading Post, plus the Lumberyard sawing service as the bridge, then the Saw Pit and Lumber Mill. Recommended yes, in that order. **8. Log market group:** give logs their own group and add species and quality later. Recommended yes. **9. Leather as a material:** yes, with the satchel, boots and waterskin as its first uses. Recommended yes. **10. Order against the workshop:** the lumberyard sawing service is mostly Trading Post and market work (small, no new structure), so it could come before the Workshop shed.

# Sources added in the addendum

- Ohio Timber Price Report, January 2026 (Fall 2025 sawlog and stumpage prices): https://woodlandstewards.osu.edu/sites/woodlands/files/imce/TPR%20January%202026.pdf
- Wikipedia, Saw pit (two-man whipsaw, about 12 boards a day, water-powered up to 200): https://en.wikipedia.org/wiki/Saw_pit
- LeRoy Historical Society, sawn lumber (pioneer pit saw, early mills, boards at $7.50 per thousand, paying with logs): https://leroyhistoricalsociety.org/Articles/2011/110611%20-%20Sawn%20Lumber.pdf
- Mother Earth News, portable sawmill business (cutting about 16 cents a board foot, value added by drying): https://www.motherearthnews.com/homesteading-and-livestock/portable-sawmill-business-zmaz02djzgoe



---

# Addendum 2026-10-30 (Mike): furniture, a pottery wheel, and why the workshop matters now

Status: Design Draft (PROPOSED). Nothing here is built or decided except the four answers below. Every number is a first pass for Mike to cut down.

## What Mike asked for

"I think we need to move forward with the wood workshop to start making furniture like stools, tables, chairs, beds, end tables and other wood products and maybe a pottery wheel to utilize more clay products. We also need to start thinking about a UTV for getting around faster and carrying more resources." The UTV and hand cart are in their own draft: Vehicles_and_Hauling.md.

## Answered by Mike (2026-10-30)

1. **What furniture does:** cabin use and sale. You place it in your cabin for comfort and function, and you can sell spares.
2. **What the first furniture is made from:** logs and branches first (with Cordage and Tall Grass where needed), boards later once the Lumber Mill chain exists.
3. **Vehicles:** hand cart first, UTV later (see Vehicles_and_Hauling.md).
4. **Build order:** not decided yet. All three designs are written first, then the order is chosen.

## Why now (what the measuring runs said)

- The Pioneer runs earned about $8 a game day at best (Spring $8.18, Fall $7.21, Winter with Maple $8.11 underlying) against the $20.48 a game day the 5-acre payment needs. Furs were 60% of income. Maple added only about $3.28 a game day because food and water chores ate Mike's time.
- **A workshop job runs while the player walks off.** That is the one income source in the whole design that does not cost the player's own hands while it works. Furniture is the first product where one hour of hands-on work (felling and carrying logs, loading the bench) turns into many game hours of job time. That is why the workshop is the right answer to "not enough time in the day".
- **Furniture roughly triples the value of a log.** A log sells for $3. A Rough Stool uses 2 Logs ($6 at store value) and sells for about $14. A Table uses 3 Logs and sells for about $28 (about $9 a log). A Rope Bed uses 4 Logs and sells for about $40 (about $10 a log).
- **But logs are heavy and trees are finite.** A Log is 8 kg, the Encumbered line is 30 kg and the hard cap is 45 kg (55 with the Pouch), so one pack-load is 3 logs without being slowed. There are 8,852 trees with no regrowth. So the workshop pushes the player to haul a lot of wood, which is exactly what the hand cart and later the UTV are for.

## What exists in the game today that furniture can hook into (so no new stat is needed)

- **Sleep (built 2026-09-26):** Sleeping Bag, Tent and Lean-To change overnight Warmth loss and Health recovery while asleep (Tent: 60% less Warmth loss and double recovery; Lean-To: 50% and double; bag alone: 25% and 1.5 times). Nothing in the code is a Comfort stat; "Comfort" ratings in Building_Housing_System.md are labels only.
- **Sheltered meal (Covered Outdoor Kitchen):** eating a Hunger-restoring item under the roof gives 25% faster Health recovery for 4 game hours (no Comfort or Morale stat exists).
- **Storage:** Storage Bin and Food Cache already hold items (a placed pile with a weight cap).
- **Placement:** the Build tab with its placement ghost handles placed objects, staged sites (deliver materials with R, then Hammer blows) and queued jobs (cooking, boiling and wood splitting use a queue with progress and Cancel).
- **Unknown today (Claude Code must read the code before any build):** whether the Small Cabin has an interior that the placement ghost can place objects inside, whether the player can sit, and what the Cabin's sleep bonus is today.

## Furniture: first set (logs and branches)

All made at the **Woodworking bench** in the Workshop shed (W1), tools Knife and Axe. Prices are proposed base prices in game dollars, "inputs at store value" uses a log at $3, a Branch and Tall Grass free, Cordage free. Weights are first-pass guesses.

| Item | Inputs | Job time | Weight | Base price | What it does in the cabin |
|---|---|---|---|---|---|
| Rough Stool | 2 Logs, 2 Branches | 3 h | about 10 kg | $14 | A seat. Sit and rest (needs a Sit action, see decisions). |
| Bench | 3 Logs, 3 Branches | 4 h | about 20 kg | $20 | A seat for two, fits a table. |
| Chair | 2 Logs, 4 Branches, 1 Cordage | 4 h | about 12 kg | $18 | A seat with a back. |
| Table | 3 Logs, 4 Branches, 2 Cordage | 5 h | about 24 kg | $28 | With a seat within 2 m under a roof, a meal counts as a "sit-down meal" (a better sheltered meal). |
| End Table | 1 Log, 3 Branches | 2 h | about 8 kg | $12 | Small storage (about 10 kg) beside the bed. |
| Rope Bed (frame) | 4 Logs, 6 Branches, 4 Cordage | 8 h | about 32 kg | $40 | Sleep on it with a Sleeping Mat (12 Tall Grass + 2 Cordage, already on the Fiber bench list at $8) for a better night than a Tent. |
| Storage Chest | 3 Logs, 4 Branches, 2 Cordage | 5 h | about 22 kg | $26 | Storage (about 60 kg, the Storage Bin pattern). |
| Shelf | 2 Logs, 3 Branches | 3 h | about 14 kg | $18 | Wall storage (about 30 kg). |

Board versions (planed table, framed chair, paneled chest) come when the Lumber Mill or the lumberyard sawing service exists, as a second row of the same items at a higher price. Furniture made from boards inherits the wood's quality multiplier from the lumber chain.

## What furniture does mechanically (proposal, no new Comfort stat)

First pass, sizes are Claude Code's call after reading the sleep and meal code:

- **Bed:** sleeping on a Rope Bed with a Mat inside a roofed cabin adds a bit on top of the shelter's own effect, for example a few percent more insulation and recovery 2.8 times instead of 2.5 times. The code says the Cabin already gives 2.5 times Health recovery and the best insulation (0.75), so the Rope Bed is an upgrade to a Cabin, not a way to beat a Tent. It must not make a night heal more than the 100 Health cap.
- **Sit-down meal:** a Table with a Bench, Chair or Stool within 2 m, under a roof, upgrades the sheltered meal from 25% faster Health recovery for 4 game hours to about 35% for 6 game hours.
- **Seats alone:** a Sit action (E on a seat) that makes Stamina recover faster than standing (for example double the rate), and lets the player pass 15 minutes of game time sitting. This is optional and needs a controller check (see decisions).
- **Storage pieces:** the Chest, Shelf and End Table are the Storage Bin pattern with a different weight cap, so they cost almost no new code.
- **Selling:** every piece can be sold in the Trading Post window (T) from the pack. The Trading Post has no physical location in the game, so selling costs no walking and no game time. A piece is heavy (24 kg Table, 32 kg Bed) but it only has to be carried from the bench to wherever the player stands to sell.

## How the workshop would deliver the materials (important because the pieces are heavy)

A Bed needs 4 Logs (32 kg) plus Branches and Cordage, over the 30 kg Encumbered line in one carry. Two options:

- **A. Bench stock (recommended):** the Woodworking bench takes materials by delivery like the Leach Hopper and the Cabin site (R stores what you carry, in several trips), then starts the job when everything is there. Same pattern as already-built staged sites.
- **B. Carry it all:** ingredients come out of the pack when the job starts. Simple, but the Bed and Table are awkward to start without a cart.

## How much money could furniture add

The market formula from the first draft still applies (25% daily recovery, 10% drop per unit, floor 0.1 in the crafted group): about 0.77 times base price for 1 sold a day and about 1.2 for 2. Examples at the first-pass prices:

| Player | What they sell each game day | Added income per game day |
|---|---|---|
| Light | 1 Stool and 1 Chair (base $14 and $18) | about $25 |
| Casual | 1 Table, 1 Bed, 1 Chair (base $28, $40, $18) | about $66 before limits |
| Heavy | all 8 kinds, 1 of each (average base $22) | about $135, not realistic |

Real limits: one bench job at a time, job time (a Bed is 8 hours), wood supply (a Casual day uses about 9 logs, 72 kg), and hauling. Realistic uplift for a Pioneer is probably $10 to $25 a game day, similar to the first draft, but it costs far less player time because the bench works while the player does other things. It would still not close the whole gap to $20.48 on its own, so it is one leg of several (furs, Maple, Ash chain, furniture, pottery).

Risks: furniture from free branches and cordage may turn into the lantern loop again; per-item pools, the floor, job time, a one-job-at-a-time bench and the finite trees are the controls. Measure with the same Pioneer runs before tuning.

## Pottery wheel (the clay chain upgraded)

The first draft's clay chain was a Clay table and a Clamp kiln with hand-built ware (pinch and coil pots). A **kick wheel** is the upgrade: a foot-driven wheel with a heavy flywheel that spins the clay while the potter shapes it. Real throwing steps (Sheffield Pottery's beginner guide): wedge the clay (remove air bubbles) in 1 to 2 lb portions, center it on the wheel, open it with a base about 3/8 inch thick, pull the walls up, and trim at the leather-hard stage. Drying and firing times were not on that page (the first draft's drying and firing rules stand: weeks of drying in real life compressed to game days, fire in a kiln around 1,100 degrees F, rushed or underdried loads crack).

**Chain, game version:** Clay table (wedge, add Temper) then **Kick Wheel** (throw) then Drying rack (2 to 5 game days) then Kiln (fire with Firewood, 10 per load of about 12 pieces) then collect. Hand-built ware from the first draft stays as the cheap early path.

**Kick Wheel (build, staged site like the Hopper):** first pass 3 Logs, 4 Branches, 2 Stone (the flywheel), 2 Cordage, about 40 kg in total over several trips, 20 Hammer blows. Placed under a roof (the Workshop shed or a lean-to) because wet clay and a wooden frame do not mix.

**Why the wheel is worth building over hand-built ware (proposal):**

- Wheel-thrown ware sells for about 60% to 80% more than hand-built, because walls are thin and even.
- A throw takes 20 to 40 game minutes instead of 1 to 2 game hours.
- The failure chance is lower, about 5% to 10% instead of 10% to 20%.
- It makes shapes hand-building can't: Plate, Pitcher, Storage Jar with Lid.

| Item | Inputs | Throw time | Base price (wheel) | Hand-built price (first draft) | Use |
|---|---|---|---|---|---|
| Clay Cup | 1 Clay, 1 Temper | 20 min | $7 | $4 | Sells |
| Clay Plate | 1 Clay, 1 Temper | 20 min | $9 | none | Sells |
| Clay Bowl | 2 Clay, 1 Temper | 25 min | $10 | $6 | Sells |
| Flower Pot | 2 Clay, 1 Temper | 25 min | $8 | none | Sells; later holds seedlings |
| Pitcher | 3 Clay, 1 Temper | 30 min | $15 | none | Sells; holds a few litres of water |
| Storage Jar with Lid | 4 Clay, 1 Temper | 35 min | $18 | none | Sells; later food storage |
| Water Crock | 5 Clay, 1 Temper | 40 min | $22 | $12 | Sells; stores 20 L of water in the cabin |
| Clay Cooking Pot | 3 Clay, 1 Temper | 30 min | $14 | $10 | Works as a Cooking Pot on the Tripod (see decisions) |
| Brick batch (20) | 10 Clay, 1 Temper | mould, no wheel | $6 | $6 | Hearths, chimneys and the kiln itself |

Clay is 3.5 kg a lump (a Water Crock is 17.5 kg of clay before shrinkage), so pottery is also a hauling job.

**Links to systems already built:** kiln firings burn Firewood and could drop Ash like any fire (about 3 Ash for a 10 Firewood load, if Ash comes from kilns too, a Claude Code check), feeding the Lye and Potash chain. A Clay Cooking Pot could be a free alternative to the $12 bought Pot, which would take the Pioneer's early purchase list down (the Spring run bought "cooking, water, fire" gear for $45).

## Build order inside the workshop (replaces the first draft's W1 to W3 order)

- **W1 Workshop shed with the Woodworking bench:** furniture, shakes, fence rails, bowls. First because logs, branches and cordage are already on the land, and the bench is the base of everything else.
- **W2 Tanning frame:** hides to leather.
- **W3a Clay table, Drying rack and Clamp kiln:** hand-built ware, bricks, Clay Cooking Pot.
- **W3b Kick Wheel:** the upgrade to wheel-thrown ware.
- **Fiber bench** can come with W1 (Sleeping Mat is needed for the Bed).

Any step can be stopped after, and each is staged so Mike can see it work.

## Decisions for Mike (new, numbered after the first draft's 1 to 10)

11. **Furniture list:** the 8 pieces above are the proposed first set. Cut, add or rename any. Recommended: start with Stool, Table, Chair, Rope Bed, Storage Chest, then add the rest.
12. **Materials delivery:** bench stock by delivery (A) or carry everything (B)? Recommended: A.
13. **Sit action:** add a Sit on stools, chairs and benches (faster Stamina recovery, pass 15 minutes), or keep seats as sale and sit-down-meal pieces only? Recommended: a small Sit action, if the controller allows it easily (Claude Code check).
14. **Indoor placement:** can the Small Cabin hold placed furniture (floor, collision, ghost snapping)? Claude Code reads the code first. If not, the first build also has to add it, which could make W1 bigger.
15. **Cabin effects:** accept the bed and sit-down meal bonuses as first-pass sizes and tune by feel in Play Mode? Recommended: yes.
16. **Clay Cooking Pot counts as a Cooking Pot for the Tripod:** recommended yes. It gives the Pioneer a free route to a pot. Check how it interacts with the Maple and boiling rules (a clay pot is still a "vessel, no litre capacity").
17. **Wheel before or after the kiln chain:** recommended hand-built first (W3a), the Kick Wheel as W3b.
18. **Handmade wares group (first draft decision 4):** now more important, because furniture and pottery both go there. Keep the crafted group (10% drop, floor 0.1) at first and loosen only if measuring says income is too thin, or start with a milder group (6% drop, floor 0.25)? Recommended: start harsh, loosen after measuring.
19. **Wood from the land:** with 8,852 trees and no regrowth, should furniture need a "wood quality" system now or wait for the Lumber Mill chain? Recommended: wait, quality arrives with boards.

## Read-only checks for Claude Code before any workshop plan

1. Can the Small Cabin interior hold placed pieces? Floor, collision, ghost snapping.
2. What is the Cabin's sleep effect today, and how would a bed layer on it?
3. Does the controller have any seated or crouch-at-seat state? How hard would a Sit be?
4. How do the existing queued jobs (cooking, boiling, splitting) store progress, so the workshop reuses them?
5. How big is the staged-site code for a station like the Hopper, as a template for the Workshop shed and the Kick Wheel?
6. What would be the kiln's Ash behavior, and how much Firewood is realistic?



## What the code says (Claude Code read-only report, 2026-10-30)

This answers the six read-only checks above. No code was changed. Sizes are Claude Code's estimates.

1. **Furniture inside the Small Cabin:** the shell is solid and the player already stands and sleeps inside (interior about 4 m wide and 5 m deep, walls 2.2 m, a floor collider with its top 0.3 m up, a door 1.1 m by 2.0 m, the hearth slab on one side leaving about 1.7 m of open floor). But placement refuses the whole interior: `CanBuildPileAt` rejects any spot within 3.2 m of a cabin's centre ("Too close to another pile or building"), and the interior is only 2 m by 2.5 m each way of centre. **Nothing can be placed inside the Cabin today.** The Kitchen add-ons already snap to fixed slots (`PlacementMode.SnapToSlot`, `WoodManager.AddonSlots`), which is the template. Positions and yaw already save, so furniture needs no new save shape. A rule is needed for dismantling a cabin that holds furniture. **Size: about 250 to 450 lines in 1 to 2 phases** (cabin-local bounds exempt from the 3.2 m rule, floor-hit accepted, optional slots, dismantle rule).
2. **Cabin sleep today:** the Cabin is already the best tier: insulation 0.75, full rain and wind block, and **Health recovery 2.5 times** (not the 2 times the first draft assumed). A carried Sleeping Bag adds to it, and the Cabin hearth is a real campfire that warms a sleeper. A bed is one more term in the same block, like `SleepRecoveryBonus`. **Size: about 40 to 80 lines plus tests**, once a bed can exist inside the Cabin.
3. **Sit:** there is no seated state (`MovementState` is Idle, Walking, Sprinting, Crouching, Jumping). Locked-input precedents exist (knocked down, creek crossing, placement mode, sleeping). A seat could be an interactable that snaps the player, lowers eye height, locks movement and applies a stamina recovery multiplier, and "pass 15 minutes" could copy the sleep fade. **Size: about 120 to 200 lines, 1 phase.**
4. **Queued jobs:** cooking, boiling and wood splitting keep progress in memory only, are not saved, and pause when the player walks away. So the workshop must NOT reuse them. The **Leach Hopper pattern** is the right one: progress saves as `leachHours` on the placed object and ticks each game hour (`OnHour`), so it runs while the player is away and through sleep. A bench would add additive fields (job id, job hours) and a static rules class like `PotashRules`. **Size: about 150 to 250 lines plus tests**, bench-specific, not a generic queue.
5. **Staged-site template:** a station is a row in each of four data methods (`FinishedKind`, `FinishedCost`, `DeliveryOrder`, `RequiresDig`) plus a `PileKind` (saved as an integer, so new kinds are appended after the Leach Hopper) and a Hammer-blow row. Delivery and blows are shared code in `PlacedStructure`. **Size: about 150 to 250 lines each for the Workshop shed and the Kick Wheel** before any job logic, and the two share nearly everything.
6. **Bench takes materials by R, then runs a job:** copy the Hopper load flow: store carried items up to a cap over several trips, start the job when the stock covers a recipe, E collects. Choosing a recipe needs a small screen (`StorageTransferScreen` or `CraftingScreen` are the templates), otherwise the bench runs "the one recipe the stock matches". **Size: about 250 to 400 lines for the bench logic and about 200 for a recipe screen.**
7. **Kiln and Ash:** no kiln exists, and the word does not appear in the code. Ash today comes only from campfires and hearths (1 per 3 Firewood). A kiln firing could call the same Ash rules when it burns fuel (10 Firewood is about 3.3 Ash). **Size: about 40 to 80 lines on top of the kiln station.**

**Total picture for W1 (shed, bench, furniture recipes):** interior placement 250 to 450, shed site 150 to 250, bench job and delivery 400 to 650, recipe screen about 200, bed 40 to 80, Sit 120 to 200 (optional). That is about 1,000 to 1,800 lines in several phases, the size of the Tripod plus Potash work together.

## What this changes in the plan

- **The Trading Post is a menu, not a place.** There is no physical Trading Post (TradingPost.cs, Difficulty_System.md). Selling needs no walking. A cart or UTV helps with hauling materials to the bench, not with selling.
- **Furniture in the Cabin needs a prerequisite:** interior placement. It is a separate, smaller first phase that also helps other things (shelves, a bed, a table in the Kitchen add-on idea). Furniture can still be made and SOLD before it can be placed, which keeps the income value independent of the interior work.
- **A bed layered on the Cabin is a small step up** (2.5 times to about 2.8 times recovery), so the Rope Bed's main job is to be a good product to sell, with a modest cabin effect.
- **Phases for W1 (proposal):** (a) Workshop shed site and a Woodworking bench with delivery and a job that runs while away, making 3 to 4 sellable pieces; (b) the full furniture list and the recipe screen; (c) interior placement for the Cabin and the bed, sit-down meal and Sit effects. Mike can stop after (a) and still have an income source.


## Addendum 2026-10-31: Wood Saw, Wooden Wheels recipe, chainsaw (decided, not built yet)

Mike's direction: Wooden Wheels are crafted from Logs only (1 Log makes 1 wheel, instant, no tool, no Cordage); a Wood Saw is sold at the Trading Post and does both jobs (faster felling now, the tool for boards at the workshop later); a chainsaw comes late, with fuel and upkeep.

Code facts (Claude Code read-only report): felling is a hold-to-swing, one blow every 0.9 s, 4 stamina a blow; blows = round(base x size x tool factor) with base 10 broad, 13 tall, 3 shrub; Axe factor 1.0, Primitive Axe 1.5; no tool tier field, no tool wear, tools are not per-instance (one-off state such as fuel is saved by the owning component); the Lantern tank (8 h per Lamp Oil, 16 h capacity) is the pattern for chainsaw fuel; a recipe cannot require a tool today (about 15-20 lines plus 3 tests to add; not needed now because the workshop station can check Has("wood_saw")). An average tree is about 12.7 Axe blows, 11.4 s, about 2.7 Logs, about 51 of 100 stamina, so stamina limits a session to about two trees before a rest.

The saw does not raise income. Logs sell at base $3 in the WoodStone group (4% drop per unit, floor 25%, recovery 25% of the gap a day); from 100% the floor is reached after 34 Logs, and selling about 14 or more a day settles at the floor ($0.75 a Log). A steady 12 Logs a day pays $13.44 in Spring or Fall, $11.27 in Summer, $20.15 in Winter, and even the Primitive Axe fells more than the market pays for. The saw's value is time and stamina (12 Logs of swinging: Primitive Axe 76 s, Axe 50 s, Saw at 0.65 about 33 s), plus boards later.

Proposed saw (to be confirmed by playtest): id wood_saw, Tool category, Trading Post $30 (1.5 times the Axe), sells back $12, 2 kg, sawFactor 0.65 (about 35 percent faster than the Axe), equipped to fell, in the pack for Inventory splitting and later boards, fastest carried tool sets the splitting speed. Chainsaw (design note only): late purchase about $250-400, felling factor about 0.15-0.25, bought Chainsaw Fuel (about $3, Lantern-style tank, about 8 trees a fuel), condition-only wear restored by a bought file or chain, fuel cost kept near 10-15 percent of a Log's base price per Log.


## Addendum 2026-10-31: Workshop phase W1(a) plan, decisions (Claude Code read-only plan, Mike's answers)

Plan: one placed Workshop structure (a roofed shed with the Woodworking bench built in, no interior placement needed), staged site then finished structure (PileKind WorkshopSite = 33, Workshop = 34, appended after HandCart = 32; three pinned enum-count tests change from 33 to 35). Built as a PlacedStructure like the Leach Hopper. Delivery by R over several trips (StoreMaterials, shared code), then the finished Workshop takes bench stock by R up to a cap; E starts a job; the job runs in saved game hours ticked each hour (Hopper pattern, so it runs while away and through sleep and fast-forward; per-hour tick so a job can finish up to an hour early, accepted); E collects, a full pack leaves the piece in the bench. One job at a time; a finished piece waits until collected; stock can be topped up while a job runs; dismantle refused while a job runs. Saved data additive only: WoodPileState gets string jobItem and float jobHours; stock and product live in contents; no version bump. Sales go through the existing Crafted market group (10% drop, floor 0.1, 100% target all seasons) and the ledger line "Crafted goods" (SalesCrafted); no new ledger category (an id 19 income category would be counted as an expense because IsIncome is (int)category <= LastIncomeId 7). Furniture is an item of the Material category.

Decisions: tool gate = any felling tool in the pack (Axe, Primitive Axe or Wood Saw via AxeRules.FastestCarried), checked when the job starts, no Knife; chooser = single recipe in a1, Workshop tab on the Craft and Build screen in a2 (Option A); site cost 5 Logs, 10 Branches, 10 Tall Grass, 3 Cordage (about 52 kg) and 40 Hammer blows; stock cap 6 Logs, 8 Branches, 4 Cordage (about 57 kg); first piece Rough Stool (2 Logs, 2 Branches, 3 h, about $14, 10 kg); a2 adds End Table (1 Log, 3 Branches, 2 h, $12, 8 kg), Chair (2 Logs, 4 Branches, 1 Cordage, 4 h, $18, 12 kg), Table (3 Logs, 4 Branches, 2 Cordage, 5 h, $28, 24 kg); cart feeding the bench (a3) after a2's playtest; no batch quantity; dev keys backslash (Workshop kit), Shift+backslash (finished Workshop with stock for two Tables), Ctrl+backslash (finish every job now). Moved into a1 from a3 on review (recommended, not yet confirmed by Mike): the Workshop's own clearance against a parked cart (the cart's spacing exemption would otherwise let it be placed over a cart) and a Workshop radius for tree sprouts (SproutBlocked only treats a built structure as a 1.5 m blocker, narrower than a 3 m shed).

Economy flag (market formula run on a copy): selling at steady state, Rough Stool $10.77 a day at 1 a day, $16.95 at 2; End Table $9.23 and $14.53; Chair $13.85 and $21.79; Table $21.55 and $33.90; one of each every day $55.40; one of each every other day $31.90. A single Table a day ($21.55) already equals the loan payment ($20.48), and furniture is worth $5 to $9 per Log against $1.12 per Log for selling 12 Logs a day, so the bench could out-earn everything else. Limits: job starts (each needs the player at the bench; the full set is 14 job hours of 24), wood (the set uses about 8 Logs, 14 Branches and 3 Cordage a day, about 3 trees; Branches are probably the binding supply). Keep the doc prices for the first build, measure with a Pioneer run, and re-check prices before a2 adds the Table; base prices are one table of constants, so lowering them is a small change.


## BUILT: Workshop W1(a1) (2026-11-01, playtest 1 to 21 passed, committed 7262c74)

Built to the plan. A placed Workshop (a roofed thatch shed about 3 m by 2.4 m, open at the front, a Woodworking bench against the back wall, solid posts and back wall, interaction box on the bench) built as a staged site: Build tab "Workshop Site" (category Other), mark out free, deliver 5 Logs, 10 Branches, 10 Tall Grass, 3 Cordage (about 52 kg) with R over several trips, 40 Hammer blows. PileKinds WorkshopSite = 33 and Workshop = 34 (appended after HandCart = 32; none of the existing numbers moved). The finished Workshop is a PlacedStructure: R stocks the bench up to 6 Logs, 8 Branches, 4 Cordage (wood keeps its wet or dry day); E starts a Rough Stool (2 Logs, 2 Branches, 3 game hours, 10 kg, $14, Material item sold in the Crafted group under "Crafted goods") when a felling tool (Axe, Primitive Axe or Wood Saw) is in the pack, no job is running and no finished piece waits; the job's hours are saved on the bench (WoodPileState.jobItem and jobHours, additive, no version bump) and ticked once per game hour like the Leach Hopper, so it runs away from the bench and through sleep and fast-forward (a 3 h job can finish up to an hour early); E collects, a piece that does not fit stays in the bench; Hammer plus R dismantles for half the materials plus the stock and any finished piece whole, refused while a job runs.

Placement: dry ground (flood band refused), 3 m from a Cabin (Wood Shed rule, CanPlaceShed), a tree trunk within 2 m of the centre refused, 3 m from another pile, 2.6 m from a parked Hand Cart both ways (the cart's own spacing exemption no longer lets the two overlap); stumps are not checked by the Workshop (the Cabin rule still does). Tree regrowth keeps sprouts out of a 2.9 m radius (RegrowRules.WorkshopMetres), and the Kitchen's fire-safety gap sees the Workshop (1.9 m). Dev keys: backslash (the site kit and a Wood Pile with 5 Logs), Shift+backslash (a finished Workshop with stock for two stools), Ctrl+backslash (finish every job now). Tests 1234 (51 new in WorkshopTests); three enum-count assertions went from 33 to 35 and the pinned pile field list gained jobItem and jobHours.

Not recorded in the playtest: job hours, stool prices, whether a stump pokes through the shed (item 19), the sprout check at 2 m (item 20). Next: re-measure with a Pioneer run (stools made and sold a day, Logs and Branches used, income against $20.48), then a2 (End Table, Chair, Table, the Workshop tab on the Craft and Build screen with "in bench" wording, and a price re-check before the Table), then a3 (the cart feeding the bench).


## BUILT: Workshop a2-1 (2026-11-01, playtest 1 to 25 passed, committed deb8658)

Mike asked for a way to choose what the Workshop makes, and chose its own separate screen (not a tab of Craft and Build), all four pieces, and the doc prices with a measurement afterwards. Built as a2-1; the pack line, stock button and keyboard keys are a2-2.

What changed. E at the bench opens the Workshop screen (WorkshopScreen: own canvas, 1920 by 1080 scaler, sort order 150, static Open(bench), Close and IsOpen, like the Finances screen); a finished piece still collects with E. While a job runs, E opens the same screen read-only ("Making a Chair, 2 h to go"). One start path, PlacedStructure.TryStartJob(Recipe, out string message), works the gate out again on the live bench at the click, then takes the stock and sets jobItem and jobHours; the old E-start code and its prompts are gone. Esc and the Close button close the screen, GameScreens.Update does not leak hotkeys while it is open, it closes on any state change away from Menu (load, main menu), and the last piece chosen is remembered for the session only (not saved).

The four pieces (all Material items sold in the Crafted group under "Crafted goods"; market rows 79 to 82, Crafted group 14 to 17):

| Piece | Logs | Branches | Cordage | Hours | Weight | Base price |
|---|---|---|---|---|---|---|
| Rough Stool | 2 | 2 | 0 | 3 | 10 kg | $14 |
| End Table | 1 | 3 | 0 | 2 | 8 kg | $12 |
| Chair | 2 | 4 | 1 | 4 | 12 kg | $18 |
| Table | 3 | 4 | 2 | 5 | 24 kg | $28 |

The screen. Left, four rows (name, then hours and the current Trading Post price from MarketManager.SellPrice; a piece the bench cannot cover yet is dimmed but can be chosen). Right, the chosen piece: name, hours and weight and base price, then each need against what the bench holds ("Logs - need 2 - bench has 4 - enough" or "short by 3", short rows orange), a reason line and the Start button (START CHAIR, SHORT OF MATERIALS, NEED A TOOL, BENCH IS BUSY naming the piece being made, COLLECT THE PIECE FIRST). A status line across the top says whether the bench is idle, working or has a piece ready. Text sizes at the 1920 by 1080 reference (a test enforces nothing below 26): title 36, status, list hours and price, detail line, needs, reason and Close 28, list names 32 bold, detail name 40 bold, Start 36 bold, Esc hint 26; rows 104 px high with 12 px gaps, panel 1500 by 880. The bench draws each finished piece: the stool's round seat, the End Table's square top with a low shelf, the Chair's seat with a back, the Table's long top on four legs.

Other changes. FinishJob drops a job item this build does not know, so it cannot sit in a bench with nothing to collect it. Save format unchanged (PileKind 35 entries, WoodPileState 14 fields, LedgerSaveData.CurrentVersion 1, SaveVersion 1). The Craft and Build help text lists the four pieces. Dev keys: Shift+backslash now stocks a finished Workshop with 5 Logs, 8 Branches and 3 Cordage (one Table plus one Chair); backslash and Ctrl+backslash as before. Tests 1259 (25 new).

Not recorded in the playtest: the prices shown in the list, the sale prices, the clock hours across sleep. Not yet measured: income with a Pioneer run. A single Table a day ($21.55 steady state) already matches the $20.48 payment, so the Table price is the one to watch.

Next: a2-2 (a pack line on the screen, a "Put my materials in the bench" button sharing the R stocking code, keyboard 1 to 4, Up, Down, Enter; no Collect button), then a Pioneer re-measure with all four pieces and a price re-check, then a3 (the cart feeds the bench from about 4 m).


## BUILT: Workshop a2-2 (2026-11-02, playtest 1 to 22 passed, committed 6871540)

The Workshop screen now saves Mike the walk out to the world's R key, and answers the keyboard. Four existing files changed (WorkshopScreen.cs, WorkshopRules.cs, PlacedStructure.cs, WorkshopTests.cs); no new files, no save, recipe, price or market change (PileKind, WoodPileState, LedgerSaveData.CurrentVersion and SaveVersion all unchanged).

Pack line. Under the status line the screen shows what the pack carries of the three stock materials, only the kinds carried ("You carry: 4 Logs, 6 Branches, 1 Cordage", or "You carry: nothing the bench uses"). It counts by kind whatever the wood's wet or dry day, the same count the R key uses (Inventory.Player.Count), and it refreshes whenever the screen does (open, Start, Stock, choosing a piece).

Stock button. "PUT MY MATERIALS IN THE BENCH" calls PlacedStructure.StockFromPack, the one method the R key at the bench now calls too (R's old code was moved into it, not copied). It stocks up to the bench caps (6 Logs, 8 Branches, 4 Cordage), keeps the wood's wet or dry day, and works while a job is running. When there is nothing to do it is greyed with a reason from a pure rule (WorkshopRules.StockState): "Your pack has none of these materials." or "The bench is already full of what you carry." After a click the line above the button shows the same message R flashes, in green. The screen never dismantles; Hammer plus R at the bench in the world still does.

Keyboard. 1 to 4 choose the Rough Stool, End Table, Chair and Table; Up and Down move the choice and wrap at the ends; Enter starts the chosen piece through TryStartJob (when it cannot start, the reason shows in the reason line with no sound); R stocks the bench; Esc closes. WorkshopRules.Resolve(key, selected, pieces) is the pure key-to-action mapping. Keys are read from the new Input System (Keyboard.current wasPressedThisFrame in the screen's Update), so one press is one action and holding a key does nothing more. The Menu state stops the player and tools, so the digits cannot change the equipped tool, Enter's Attack binding reaches nothing, and R is not the world's second interaction while the screen is open. The screen clears the event system's selected button every frame so Enter after a mouse click cannot press that button twice.

Layout and text. Panel 1500 by 920 (was 880), left column 700 wide (was 560), list moved down to make room for the pack line, rows still 104 high with 12 gaps. New text: pack line 28, stock button 32 bold (88 px high), stock reason 28, key hint line 28 in full cream colour (it replaced the 26 "Esc to close" hint; the smallest text on the screen is now 28, and a test requires at least 26 everywhere). Tests 1269 (10 new). Dev keys unchanged.

Not recorded in the playtest: the stock messages, pack counts before and after, readability notes.

Next: a Pioneer run with all four pieces to measure income against the $20.48 payment and re-check the Table price (a single Table a day at steady state is $21.55); then a3 (the cart feeds the bench from about 4 m); pottery (kiln chain, Kick Wheel); UTV last.


## Decisions 2026-11-02 (Mike): prices stay, no automatic cart feeding

Prices. The four Workshop prices stay as they are for now (Rough Stool $14, End Table $12, Chair $18, Table $28; Crafted market group). No Pioneer re-measure or price cut is planned before more is built. If income later looks too easy, the single table of constants in WorkshopRules (baseCents) is the one place to change them, and the Crafted group's 10% drop per unit already limits repeat sales.

Hauling. Materials reach the bench by hand only: R at the bench, or the screen's "Put my materials in the bench" button and the R key on the screen. The planned a3 (a parked Hand Cart within about 4 m feeding the bench) is dropped, not postponed. The Hand Cart stays a way to carry heavy loads to the Workshop, nothing more. The Workshop plan's phases (b) and (c) remain open and are unchanged by this.


## Addendum 2026-11-02: Pottery, decisions (Mike's answers; plan from Claude Code still to come)

Mike chose pottery as the next build and answered four questions (these settle doc decisions 16 and 17 and the shape of W3a):

- **Structures: a Potter's Shed and a Kiln, hand-built first.** Two placed structures like the Workshop. The Potter's Shed is a roofed shed where pieces are shaped and then dry on its own shelf; the Kiln is a separate outdoor structure where a batch is fired with Firewood. The Kick Wheel (W3b) later upgrades the same shed (wheel-thrown ware, Plate, Pitcher, Jar). Not the doc's three stations (Clay table, Drying rack, Clamp kiln) and not one structure doing everything.
- **Drying takes a few game days (1 to 3).** Proposed: Cup 1 day, Bowl 2, Cooking Pot 3, Water Crock 3 (Brick batch also about 3). Drying runs on game time like the Workshop jobs, through sleep and away time. A piece cannot be fired until it is dry, so there is no random cracking in the first build.
- **A Clay Cooking Pot works as a Cooking Pot on the Tripod** (decision 16: yes), giving a Pioneer a free route to a pot. Claude Code checks how the Tripod, boiling and Maple rules recognise a pot before it is built.
- **First pieces: Clay Cup, Clay Bowl, Clay Cooking Pot, Water Crock and Brick batch.** Hand-built base prices from the first draft ($4, $6, $10, $12, $6), sold in the Crafted group like the furniture (so the Handmade wares group of decision 18 is still not built). The Water Crock only sells in the first build; storing water in it comes later. Bricks sell only until a hearth, chimney or kiln upgrade uses them.

Still open for the plan: where Temper comes from (crushed Stone or sand dug from the creek bank), how dry pieces are carried to the kiln as items, the build costs and placement rules (the kiln away from buildings, wells and water, not under a roof; Ash from firings), and how the work splits into commit-sized builds.


## Pottery plan (Claude Code read-only report, 2026-11-02) and Mike's answers

Claude Code read the code and proposed; Mike answered the open points. What was settled:

**Temper is Sand, dug with the Shovel** (Stone is finite: one outcrop of 40 plus loose rocks). A new Material item `sand`, 2 kg (Clay is 3.5, Stone 5). Rule (corrected 2026-11-02): Mike chose 'Clay patches give Clay, other bank spots give Sand', but a diagnosis found NO Clay patches exist in any scene (the Place Clay Banks tool made the empty root and meshes and placed nothing, so the old 'dig Clay anywhere within 3 m of water' rule was the only Clay source in the game). The rule is therefore a distance band instead: a ClayBank patch (if any are ever placed) always gives Clay; ground within about 2 m of the water's edge (the wet bank) gives Clay; the creek or pond bed in shallow water (up to 0.3 m deep, PotteryRules.WadeDigDepthMetres) gives Clay; deeper water cannot be dug; ground from the wet bank out to 3 m (the dry bank) gives Sand; the HUD line names which. All water questions go through one function (AxeTool.WaterAt) and the rule is pure (PotteryRules.DigFor), so a future change to visual water levels only needs WaterAt re-pointed. Clay stays available at the water's edge on every creek and pond bank, Sand sits just up the bank. The Hand Cart's list does not change.

**Items.** One asset per piece, no tag on ItemStack (a tag would touch about ten files). Wet pieces are never items: they sit on the Shed's shelf as stamped stacks (WoodStack.day = the absolute game hour the piece becomes dry; Take takes the soonest dry first; days left = ceil((stamp - now) / 24)). Dry carried items (no market row, so unsellable): Dry Clay Cup, Dry Clay Bowl, Dry Clay Cooking Pot, Dry Water Crock, Dry Brick Batch. Fired items (P2, sold in the Crafted group, market rows 82 to 87, Crafted 17 to 22): Clay Cup, Clay Bowl, Clay Cooking Pot, Water Crock, Brick Batch.

**Saved state.** PileKinds appended after 34: PotterSite 35 and PotterShed 36 (P1), KilnSite 37 and Kiln 38 (P2). The shaping job reuses jobItem and jobHours; Mike chose jobs that make 1 to 4 pieces at once, so one additive count field is needed (0 or missing means 1). SaveVersion and LedgerSaveData.CurrentVersion stay 1.

**Numbers (named constants, one table):**

| Piece | Clay | Sand | Shape h each | Dry days | Space | Weight dry / fired | Base price |
|---|---|---|---|---|---|---|---|
| Clay Cup | 1 | 1 | 1 | 1 | 1 | 0.3 / 0.3 kg | $4 |
| Clay Bowl | 2 | 1 | 1 | 2 | 1 | 0.6 / 0.6 kg | $6 |
| Clay Cooking Pot | 3 | 1 | 2 | 3 | 2 | 2.0 / 1.8 kg | $10 |
| Water Crock | 4 | 1 | 2 | 3 | 2 | 6.0 / 5.5 kg | $12 |
| Brick Batch (20) | 10 | 1 | 2 | 3 | 4 | 16 / 14 kg | $6 |

Potter's Shed (staged site, about 35 kg): 3 Logs, 8 Branches, 8 Tall Grass, 2 Cordage, 24 Hammer blows; about 2.4 m by 2.0 m; one job at a time; shelf 12 space units (Mike chose 12); no tool gate on shaping (the Shovel is already needed to dig); dismantling refused while a job runs or any wet piece is on the shelf (dry pieces return whole). Shaping a batch of n takes n times the piece's hours; inputs are taken at the start; all n go on the shelf together at the end. Kiln (P2, staged, about 51 kg, so the Hand Cart carries it): 6 Stone, 6 Clay, 30 Hammer blows, open air, 3 m from structures and trees (a lit Kiln also counts as a neighbour for later placements); loads 12 space units (R from the pack, dry pieces then Firewood); fixed 10 Firewood per firing; fires 24 game hours (Mike chose 24); E starts the firing when at least one piece and 10 Firewood are in, otherwise collects; Ash 3 whole per load; prompts only, no Kiln screen unless the prompt text proves too small; rain does not stop a firing; no Know-how journal entries.

**Income (my arithmetic from the market rules, not measured).** Steady state per pool = base times 2.5 times (1 - 0.9^n) / (1 - 0.75 times 0.9^n) with n sold a day; each pool tops out at 2.5 times its base. The shelf (12 space, space times drying days) is the binding limit: 2 Cups, 2 Bowls and 1 Crock a day is about $21; the kiln alone (2 Cups, 2 Bowls, 2 Pots, 2 Crocks, 12 space) about $39. Digging (about 3.6 s a Clay), shaping (6 h a day), Firewood (10 is 2 Logs) and kiln hours do not bind. Brick Batch is a poor earner (4 space for 3 days, about $4.60 a day) and is kept only for later uses (hearth, chimney, kiln upgrade).

**Build order.** P1: Sand, the Potter's Shed, shaping and drying, the Potter screen, the five dry items (nothing sells yet). P2: the Kiln, firing, the five fired items, market rows. P3: the Clay Cooking Pot works as a Cooking Pot on the Tripod (a TripodRules.Serves matcher for the exact-id checks; a Tool item like cooking_pot; the bought iron Pot keeps its starting-kit and ledger rules). Dev keys: ; pottery site kit, Shift+; finished stocked Shed, ' finished Kiln (P2), Ctrl+; finish every pottery job.


## BUILT: Pottery P1 (2026-11-03, playtest 1 to 27 passed, committed 99b739a)

Built to the plan plus one fix found in the playtest. What exists: Sand (Material, 2 kg), five dry items (dry_clay_cup, dry_clay_bowl, dry_clay_pot, dry_water_crock, dry_brick_batch; no market rows, so they are unsellable and do not appear on the Trading Post sell list), the Potter's Shed (PileKinds PotterSite 35 and PotterShed 36), the Potter screen, and the new Clay and Sand digging rule. Nothing sells yet: fired pieces and market rows come in P2 with the Kiln.

The Potter's Shed. A staged site like the Workshop: 3 Logs, 8 Branches, 8 Tall Grass, 2 Cordage (33.4 kg), 24 Hammer blows, Build tab category Other, about 2.4 m by 2.0 m. Placement: dry ground, 3 m from a Cabin, Well or Outhouse, a tree trunk within 2 m refused, 3 m from other piles, 2.6 m from a parked Hand Cart, a 2.4 m regrowth blocker (RegrowRules PotteryMetres). Stock caps Clay 16 (56 kg) and Sand 8 (16 kg). R without the Hammer stocks the shed from the pack (one shared method with the screen's button); E collects any dry pieces first, otherwise opens the Potter screen (read-only for starting while a job runs); Hammer plus R dismantles (refused while a job runs or any piece is wet; dry pieces and stock come back whole). Shaping reuses jobItem and jobHours (ticked each game hour like the Workshop); a job makes 1 to 4 pieces of one kind (one additive saved int, WoodPileState.jobCount, 0 or missing means 1; the pinned field list is now 15), takes n times the piece's hours, takes the inputs at the start and puts all n pieces on the shelf together at the end. The shelf holds 12 space units as stamped stacks of dry-item ids (WoodStack.day = the absolute game hour the piece becomes dry, so sleep and away time just work; wood-moisture code leaves them alone). Table in the pottery plan section above (recipes, space, drying days).

The Potter screen (PotteryScreen, PotteryRules.Key and Resolve, own key mapping so the Workshop's tests did not change): five rows, count control 1 to 4 (large minus and plus buttons), needs rows, total shaping time, reason line, Start, a shelf block ("Drying: 2 Cups, 1 day left", "Ready: 3 Bowls", space used out of 12), pack line, stock button "PUT MY CLAY AND SAND IN THE SHED". Keys: 1 to 5, Up and Down (wrap), Left and Right (count), Enter, R, Esc. Text sizes all 26 or more (body 28, list names 32 bold, piece name 40 bold, Start 36 bold, stock 32 bold, count 36 bold, plus and minus 48 bold); rows 96 high with 12 gaps; panel 1500 by 1000 on the 1920 by 1080 scaler (40 px margin at 16:9; it clips about 65 px on a 21:9 monitor; if that is ever a problem set this screen's scaler to match height). Dev keys: ; pottery site kit (Hammer, Shovel, 8 Branches, 8 Tall Grass, 2 Cordage, 12 Clay, 6 Sand, a Wood Pile with 3 Logs), Shift+; a finished stocked Shed (2 wet Bowls, 3 dry Cups), Ctrl+; finish every shaping job and dry every piece.

The Clay fix (found in the playtest: Mike could not find any Clay). The first P1 build made Clay patch-only, assuming Clay patches existed; a read-only diagnosis found none in the game (see Building_Housing_System.md update 2026-11-03). The rule is now one pure function, PotteryRules.DigFor(onClayPatch, overWater, depthMetres, waterlineMetres): a patch gives Clay; shallow water up to WadeDigDepthMetres 0.3 gives Clay ("Creek bed"); deeper water nothing ("Water too deep to dig - try the shallows or the wet bank"); dry ground within WetBankMetres 2.0 of water gives Clay ("Wet clay bank"); out to BankReachMetres 3.0 gives Sand ("Dry creek bank"); else nothing. All water questions go through AxeTool.WaterAt (the only Animal.IsOverWater call in AxeTool, pinned by a test), so future visual water levels only need that one function re-pointed (a replacement water plane needs a non-trigger collider on the Water layer that a ray from 30 m above can hit). Probes: rings of 8 at 1, 2 and 3 m, recomputed when the aim point moves 25 cm or after 1 s. Nothing moves the water surface today (StreamManager stage and PondManager level change quality and amounts only). No waterfalls exist; a steep water quad over shallow ground would still count as water (an optional hit.normal.y check was not built).

Tests 1329 (60 new in PotteryTests across P1). Save format: PileKind 37 entries (0 to 36), WoodPileState 15 fields, SaveVersion and LedgerSaveData.CurrentVersion still 1. Not recorded in the playtest: Clay and Sand strip widths, HUD flicker, readability and monitor shape.

Next: P2, the Kiln and firing (PileKinds KilnSite 37 and Kiln 38; 6 Stone, 6 Clay, 30 Hammer blows; loads 12 space units; fixed 10 Firewood; 24 game hours; Ash 3 a load; the five fired pieces with market rows 82 to 87 and Crafted 17 to 22 added by hand). Then P3, the Clay Cooking Pot as a Cooking Pot on the Tripod.


## BUILT: Pottery P2 (2026-11-03, playtest 1 to 26 passed, committed dcb43bf)

Pottery now sells. What exists: the Kiln (PileKinds KilnSite 37 and Kiln 38; 39 kinds in all), firing, five fired items sold in the Crafted group, and Ash from firings. Tests 1366 (37 new in KilnTests). No new saved field (WoodPileState stays at 15 fields); SaveVersion and LedgerSaveData.CurrentVersion still 1.

The Kiln. A staged site like the others: 6 Stone and 6 Clay (about 51 kg, so the Hand Cart carries it; CartRules.Carried already lists Stone, Clay and Firewood), 30 Hammer blows, Build tab category Other. Open air: dry ground, not under a roof (OverheadCover.Roofed, "A kiln needs open sky"), its dome (radius 1.2 m) 3 m clear of every other structure and every tree, and the clearance holds both ways: a Kiln that stands keeps every later placement (Cabin, Workshop, Potter's Shed, Kitchen and the rest) 3 m away; a parked Hand Cart is exempt. A regrowth blocker of 1.8 m. Setbacks.cs carries the kiln figures as literals (its assembly does not reference PotteryRules); a test pins that they agree.

Using it (prompts only, no screen; the HUD prompt label is font size 24, best fit off, and the longest Kiln line is about 61 characters; Mike found them readable). R from the pack, Hammer not in hand: loads dry pieces first, then Firewood, up to 12 space units (Cup 1, Bowl 1, Cooking Pot 2, Crock 2, Brick Batch 4) and 10 Firewood; refused while firing and while fired pieces wait. E: with at least one piece and 10 Firewood it lights the kiln; otherwise it collects fired pieces and Ash to the pack (what does not fit stays); while firing it shows the hours left. Hammer plus R takes it down only when empty and not firing (half the Stone and Clay back, 3 each); a kiln holding only Ash must be emptied first. Firing: jobItem "kiln_firing" marks it and jobHours counts down in WellsAndOuthouses.OnHour (through sleep and away time); 24 game hours; fixed 10 Firewood whatever the load; no random failure, no rain rule. When it ends each dry stack becomes its fired stack and 3 Ash (AshRules.Whole(Owed("firewood", 10))) are added. Looks: a dark clay dome with an orange mouth, different when empty, loaded, firing (ember and a column of smoke spheres, placeholder) and done (fired pieces and an ash heap).

The five fired items (Crafted group: 10% drop per unit, floor 0.1, 25% daily recovery, 100% target all seasons, each its own pool; rows 83 to 87, total 87 rows, Crafted group 22, Stocked count stays 21; added by hand to MarketData.asset, never seeded): Clay Cup $4 (0.3 kg), Clay Bowl $6 (0.6 kg), Clay Cooking Pot $10 (1.8 kg), Water Crock $12 (5.5 kg), Brick Batch $6 (14 kg). The Clay Cooking Pot (clay_pot) is a Tool item (Resources/Items/Tools), so P3 only adds the Tripod logic. The Trading Post sell list shows a carried item only if the market can sell it, so the five fired pieces appear and Sand and the dry pieces still do not. Dev keys: ' a finished Kiln 4 m ahead loaded with 4 Cups, 2 Bowls, a Cooking Pot, a Crock (10 of 12 space) and 10 Firewood; Shift+' a Kiln site kit (Hammer, 6 Stone, 6 Clay, 10 Firewood, 3 dry Cups, 2 dry Bowls, about 62 kg so some may not fit the pack); Ctrl+; now also finishes every running firing.

Economy as built (my arithmetic, not measured): the shelf (12 space units times drying days) still binds a Pioneer to about $21 a day (2 Cups, 2 Bowls, 1 Crock a day); the Kiln allows up to about $39 a day if the shelf were not the limit. A full 12-space load is worth at most $72 at base prices (6 Water Crocks) before the 10% drops per sale, but each piece type has its own pool, so a mixed load sells for far more per piece; the 10 Firewood (2 Logs) is the only fuel cost, so pottery stays cheap in player time (Clay and Sand dig in seconds) and the daily income is limited by the shelf and the pools. Brick Batch remains a poor earner. Not measured in a Pioneer run.

Not recorded in the playtest: prices after repeated sales, readability of the prompts at size 24 beyond "readable", the hotkey-slot behaviour of the Clay Cooking Pot as a Tool (Mike checked it; nothing odd reported), the 3 Logs that did not fit from the overweight Shift+' kit.

Next: P3, the Clay Cooking Pot works as a Cooking Pot on the Tripod (TripodRules.Serves matcher for the exact-id checks; NextStep needs "carries any pot"; the hung id stays specific so the Tripod draws a clay pot; the bought iron Pot keeps its starting-kit, ledger and journal rules; check the boiling and Maple rules for litres and heat).


## BUILT: Pottery P3 (2026-11-03, playtest passed, committed 782e74f)

The Clay Cooking Pot now works as a Cooking Pot on the Tripod. One helper, TripodRules.IsCookingPot(id), is true for cooking_pot (the bought iron Pot) and clay_pot. One matcher, TripodRules.Serves(hungId, neededId), replaces the exact-id checks. The id of the hung pot stays specific, so the Tripod draws the pot that was hung and gives back that same pot when it is taken down. When the player carries both pots, the iron pot hangs first (TripodRules.PotToHang). The old NextStep overload is kept so older callers and tests still work.

Same 4 L, same cooking and boiling behaviour, no breakage. The clay pot is drawn clay brown (0.52, 0.34, 0.22) with a paler rim; the iron pot looks as before. The Journal "pot hung" note fires for either pot. Lye still needs the Potash Kettle. Nothing changed in the starting kits, the Ledger or the market. The iron pot sells back for $4.00 (40% rounded down to whole dollars); the clay pot sells for $10 in the Crafted group and is not sold in the store, so there is no buy and sell loop.

Files: 7 modified (KnowHowRules.cs, TripodRules.cs, FireManager.cs, CampfireCooking.cs, Cooking.cs, Campfire.cs, Tripod.cs) and 1 new test file (ClayPotTests.cs, 16 new tests). Edit-mode suite 1382 of 1382. No save changes: SaveVersion and LedgerSaveData.CurrentVersion stay at 1.

Playtest: Mike reported "the clay cooking pot looks good and works as planned".

Playtest feedback that is not a P3 regression (queued, see Current_Task_List.md):
1. The Kiln R prompt says "Load dry pieces and Firewood (carry some)"; it should say the player is not carrying any. Check the Potter's Shed prompts for the same phrase.
2. The Kiln smoke is three grey glossy spheres (placeholder); it should look like smoke.
3. The tan block at the front of the Kiln base and the dark orange shape at the mouth appear to flash or change shape while firing. Cause unknown; the flicker should change colour and intensity only.
4. "While you slept the creek rose" shows in daytime heavy rain when the player is awake; it should say "The creek is rising" when awake.
5. Sand and Clay bank spots look the same (same grey bank terrain); Mike wants them to look different. Needs a read-only plan first.
6. The Leach Hopper only accepts Ash and water together; Mike wants to load Ash and water separately and top up later, like the Workshop bench and the Potter's Shed. Needs a read-only plan first.

Pottery is now complete as planned (P1 Potter's Shed and shaping, P2 Kiln and firing, P3 Clay Cooking Pot). Later option: the Kick Wheel as an upgrade of the same shed.


## BUILT: Pottery polish batch (2026-11-03, playtest 1 to 20 passed, committed 821a0d4)

Four fixes from the P3 playtest. No save change (SaveVersion and LedgerSaveData.CurrentVersion stay at 1). Edit-mode suite 1391 of 1391.

1. Kiln prompt. When nothing can be loaded the Kiln R line reads "Load dry pieces and Firewood  (you're not carrying any)" (was "carry some"). It was the only "carry some" in the code; the Kiln and Potter's Shed prompts were checked and left alone otherwise. The same line also shows when loadable pieces are carried but the Kiln has no room (slightly off; Mike checked it in the playtest and passed it).
2. Kiln smoke. The project had no smoke, and the old Kiln smoke was three grey default spheres. New component KilnSmoke (Scripts/World/KilnSmoke.cs) is a persistent child of the Kiln (PlacedStructure.Rebuild redraws the shapes every game hour, which would restart a normal particle system). It makes soft, translucent, slowly rising, drifting puffs from a round texture made in code, with the HDRP Unlit transparent material Resources/KilnSmoke.mat (Snow material's settings, nothing glossy). It emits only while a firing runs; puffs already up finish rising when it ends. Every number is a named public field on the "Kiln Smoke" child (puffsPerSecond, lifetimeSeconds, maxPuffs, height, spreadRadius, riseSpeed, driftStrength, driftFrequency, startSizeMin, startSizeMax, growthMultiplier, opacity, tint, fadeInFraction, fadeOutFraction), re-applied every frame in the Editor so it can be tuned in the Inspector while playing. Mike judged the look in the playtest and it passed as built.
3. Kiln flashing. Cause: z-fighting. The mouth block's front face and the front ring stone's front face were in the same plane (both z 1.07), tan stone against ember-orange mouth. Fix: the mouth moved from z 0.92 to 0.95, so its front face stands 3 cm clear. The Kiln has no flicker code and nothing rebuilds each frame; tests pin the position and gap and check the Kiln drawing has no per-frame work.
4. Creek message. Cause: StreamManager.TrackAway treated any non-Playing game state (pause, menu, a screen, a load) as "away", and recorded every rise as the away peak, even ones shown live. The return summary therefore always said "While you slept". Now a rise is kept for the summary only while it was out of sight, a new awaySlept flag is set only if the player was really asleep or collapsed (SleepManager.IsSleeping), and the text comes from a pure function StreamManager.AwaySummaryText(peak, slept): high water gives "While you slept the creek rose." or "The creek is rising."; flash flood gives "While you slept the creek flooded." or "The creek is in flood." No other flood or pond warning uses this code.

Files: 4 modified (PotteryRules.cs, StreamManager.cs, PlacedStructure.cs, KilnTests.cs) and 6 new (KilnSmoke.cs, KilnSmoke.mat, PotteryPolishTests.cs, each with its .meta); 10 paths, 516 insertions, 11 deletions.

Still open from the P3 playtest feedback, each needing a read-only plan first: (5) Sand and Clay bank spots look the same; (6) Leach Hopper loading Ash and water separately and topping up.


## BUILT: Leach Hopper separate loading (2026-11-04, playtest 1 to 21 passed, committed 33ede69)

Item 6 of the P3 playtest feedback (not a Workshop or Pottery feature, tracked here with the other feedback). Mike wanted to load Ash and water separately and top up later, like the Workshop bench and the Potter's Shed. Read-only plan first (Claude Code), then built with these decisions (Mike accepted the recommendations): E starts the batch (not automatic); material added while a batch runs waits for the next batch; caps stay 12 Ash in total (running batch plus stock plus waiting Lye) and 12 L of stored water, so throughput does not change; E collects Lye first, otherwise starts a batch of the smaller of the Ash stock and the stored litres (up to 12) with the leftover staying as stock; two new named saved ints instead of reusing litres and jobCount.

As built: R (without the Hammer) loads whatever Ash and water the player carries, at any time, worst water first (raw or boiled; once stored, water is a plain litre count). E collects Lye first (even while a batch leaches), else starts a batch, else says what is missing. Batch timing is unchanged (24 game hours, 1 Ash and 1 L per Lye, Lye out one for one). The hour tick turns only the running batch's Ash into Lye; waiting Ash stays Ash. PotashRules.PlanStock replaces PlanLoad; PotashRules.AshStock separates the batch's Ash from the waiting Ash; PotashRules.ActionFor decides what E does. Dismantling refunds the Ash and Lye whole and loses the stored water as before. Stored water adds no weight and takes no room that Ash needs. The Hopper shows a thin blue-grey layer when water is stored.

Prompts: empty "Leach Hopper  (empty)"; carrying neither "Load Ash and water  (you're not carrying any)"; one of the two loads that and names what is missing; full names which one is full; stocked "Start leaching  (5 Ash + 5 L -> 5 Lye in 24 h; 3 Ash left over)"; leaching "Leach Hopper  (leaching 5 Lye, 17 h to go; 3 Ash and 2 L of water waiting)"; Lye prompts unchanged. The Know-how entry "Using the Leach Hopper", the Crafting screen Hopper line and the "Hopper built" message were rewritten (Journal_KnowHow_Text.md updated to match; KnowHowTests compares it with the code).

Save: two new named additive ints on WoodPileState, hopperWater (stored litres) and hopperBatch (Lye in the running batch); the saved pile is now 17 fields; both default to 0. An old save with leachHours above 0 and hopperBatch 0 treats the whole Ash count as the running batch and finishes as before. A new save loaded by old code would not break (unknown fields ignored), but un-started stock Ash would be turned into Lye by the old finish. SaveVersion and LedgerSaveData.CurrentVersion stay at 1.

Dev keys: Ctrl+Insert stands an empty Hopper about 3 m ahead and gives the pack 12 Ash and 12 L of water (6 unsafe, 6 good); Ctrl+Delete finishes every running batch.

Files: 16 paths (7 modified code: PotashRules, PlacedStructure, WellsAndOuthouses, WoodManager, KnowHowRules, CraftingScreen, TimeDebugControls; 6 modified tests: PotashHopperTests, KnowHowTests, CartTests, KilnTests, PotteryTests, WorkshopTests; 2 new: HopperStockTests.cs plus meta; plus Journal_KnowHow_Text.md), 641 insertions and 136 deletions. Edit-mode suite 1413 of 1413 (22 new tests).

Still open from the P3 playtest feedback: (5) Sand and Clay bank spots look the same. The read-only plan recommends painting two new terrain layers (Wet Clay and Creek Sand) with a separate Editor tool; before building, Claude Code must check World/SeasonalGround.cs, which paints snow and leaf litter onto a runtime copy of the terrain (snow would cover the paint in winter; the layer total is higher than first counted).
