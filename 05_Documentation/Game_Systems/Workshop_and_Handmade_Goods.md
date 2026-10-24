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
