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
