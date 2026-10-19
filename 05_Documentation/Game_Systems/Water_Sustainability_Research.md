# Homestead
## Water Sustainability Research v1.0

Status: Research notes (2026-10-18). NOT a design. Nothing here is built or decided.

Purpose: real-world grounding for aquaponics, koi/garden ponds, farm-pond fish, rainwater collection, greywater reuse and livestock water, before we design pond drying and Rainwater Collection. Companion to Water_System.md (which already lists Rain Collection, Rain Barrel, Cistern and Ponds as concepts).

Rules for this doc:
- "Source says" numbers come from the pages listed at the bottom. Where a number came from a single page, it is marked "(one source)".
- Anything labelled "(Claude, not yet confirmed)" is my first-pass idea for how the number could map to the game. It is not a decision until Mike confirms.
- Gaps and disagreements between sources are listed, not smoothed over.

---

# 1. Rainwater Collection

## Real-world numbers

- Yield formula: 1 inch of rain on 1 sq ft of roof = 0.623 gallons (Midwest Permaculture article, one source on this page; it is the standard conversion).
- Example from that article: a 1,625 sq ft roof gives about 1,012 gallons per inch of rain. The article applies NO loss factor, so real yield is lower. (Claude: a 10-20% loss for splash, evaporation and first flush is common practice, but I did not verify a figure this session, so treat as unverified.)
- First-flush diverter: minimum 0.0125 gal per sq ft of roof (12.5 gal per 1,000 sq ft); up to 0.05 gal per sq ft where the roof is heavily contaminated, such as under trees (one source).
- The same article flags its own inconsistencies (roof area used in two calculations differs), so use the formula, not its totals.
- Barrel overflow is fast: the author reports two 55-gallon barrels overflowing in under 2 minutes in a summer rain (anecdote).
- Ohio rainfall (NCEI 1991-2020 averages via CurrentResults): Cincinnati 45.3 in/yr, Columbus 41.6, Dayton 41.7, Cleveland 41.0. Bethel is not listed; Cincinnati is the nearest entry and is not a Bethel figure. Monthly breakdown not obtained.

## Worked example (Claude's arithmetic, using the formula above)

Roof 400 sq ft (small cabin) x 0.623 = about 249 gal per inch of rain. At 45 in/yr that is roughly 11,200 gal/yr before losses. A 1-inch rain gives roughly 250 gal, which fills about four 55-gal barrels.

## Water quality note

Water_System.md already rates Rain Collection "Good" (5% illness risk per litre as built). Roof runoff carries bird droppings and debris, which is why first-flush diverters exist.

## Game mapping (Claude, not yet confirmed)

- Collection scales with roof area, which fits the Large Cabin and existing cabin tiers: bigger roof, more water per rain.
- Rain event size (inches) could drive fill, with overflow wasted if storage is full. This rewards building more barrels or a cistern.
- First-flush as an optional upgrade that moves quality from Good toward Excellent.
- Winter: barrels and gutters freeze, so collection pauses in Winter (fits Water_System design rule 5, "Winter should make water noticeably harder to secure").

---

# 2. Livestock Water Needs

## Real-world numbers (two sources, they do not fully agree)

Ontario (litres/day, typical):

- Beef cow with calf: 55 (range 43-67). Beef dry cow/bull: 38. Milking dairy cow: 115. Dry dairy cow: 41.
- Sheep: gestating ewe 5.25; lactating ewe with offspring about 10. (Goats not listed on that page.)
- Pigs: weaner 2; feeder 36-70 kg 4.5; feeder 70-110 kg 9; gestating sow/boar 15; lactating sow 20.
- Horse: medium (1,000 lb) 32.5; large (1,500 lb) 49.
- Poultry per 1,000 birds/day: laying hens 250 L, so about 0.25 L per hen per day.

Clemson (gallons/day at 90 F):

- Lactating beef cow 20; dairy cow 30-45; horse 10-15; sheep 2-3; goats 2-3. Pigs and chickens not covered.

Disagreement to note: Clemson's horse figure (10-15 gal, about 38-57 L) is higher than Ontario's medium horse (32.5 L, about 8.6 gal). Heat explains part of it (Clemson's table is at 90 F). Use a range, not one number.

Heat and season: Clemson says livestock drink much more in summer. Ontario says poultry intake can rise about 50% above 30 C (86 F), rabbits drink about twice as much at 30 C as at 10 C.

Drinking-water quality (Ontario): total soluble salts under 1,000 mg/L suits all livestock; above 3,000 not recommended for poultry; above 5,000 not recommended for lactating animals; above 7,000 avoid. No bacterial limit given.

## Game mapping (Claude, not yet confirmed)

- Per-animal daily water by species, multiplied up in Summer / Heat Wave (matches Water_System "Livestock water needs increase in Summer") and lowest in Winter, with the trough freezing in Winter.
- Rough ladder for an Ohio homestead herd: chickens tiny, goats and sheep about 2-3 gal each, pigs 1-5 gal, cattle and horses 8-20+ gal. Cattle and horses are what make water hauling painful, which is a good reason for rain storage and a pond.

---

# 3. Greywater

## Ohio rules (Ohio Administrative Code 3701-29-17, plus OSU factsheet AEX-GW1)

- Greywater means bathing and laundry water. It excludes kitchen sink and dishwasher water, toilet waste, diaper washing, toxic or hazardous products, cleaning chemicals other than soap, and water-softener backwash.
- Four system types:
  - Type 1: up to 60 gal/day, subsurface irrigation only. A board of health MAY issue a permit.
  - Type 2: 60 to 1,000 gal/day, subsurface only. Board of health SHALL issue a permit; soil evaluation required.
  - Type 3: up to 1,000 gal/day, into a soil absorption system; hand-carried greywater poured into a sump is allowed (OSU names campgrounds, Amish homes and remote vacation homes as typical users).
  - Type 4: treated and disinfected (NSF/ANSI 350), allows surface or subsurface irrigation of gardens, lawns, landscape and food crops, green roofs, indoor plants.
- Food crops (types 1 and 2): allowed only if not root crops, or if the edible part never touches the greywater; apply at least 25 ft from exposed edible parts. No spraying onto fruit or edibles.
- Storage: types 1 and 2 may not hold greywater in a tank over 24 hours; OSU says disperse within a few hours because it goes anaerobic and smells.
- Application limits: subsurface with at least 4 in of cover; no ponding or runoff; not on slopes over 20%; not onto frozen or saturated ground; at least 12 in vertical separation above limiting conditions.
- Pipes purple or labelled "Caution: Non-potable water, do not drink". Diversion valve to the sewer or septic required, and failures must be diverted immediately.
- Must be used within the property boundary.
- The rule "does not address livestock" (my read of the rule text via the page summary).

Other guidance (University of Arizona, Backyards): use stored greywater within 72 hours; soaps and detergents matter (plants dislike salts; buildup harms plants in compacted soil); nitrogen and phosphorus in greywater can feed plants; drip or flood only, no spraying. NOTE: Arizona's 72 hours conflicts with Ohio's 24-hour limit. Ohio's rule is the one that governs Ohio.

## Greywater for livestock: what I could and could not verify

- I found NO source that says greywater is safe for livestock to drink. Ohio's rule does not cover livestock.
- The California water-reuse expert panel (NWRI) looked only at disinfected tertiary recycled water, not greywater, and said it could not conclude even that is uniformly safe for non-dairy livestock drinking. It recommended UV disinfection (5-log) plus a chlorine residual and excluded dairy animals. It names pathogens of concern (Salmonella, Campylobacter, Giardia, Cryptosporidium and others).
- Conclusion I'd stand behind: raw greywater as animal DRINKING water is not supported by anything I found and should not be a game feature. Using greywater to irrigate fodder or pasture plants is a separate question I did not find a source for.

## Game mapping (Claude, not yet confirmed)

- Greywater as a free, limited, plant-only water source: hand-carry washing and bathing water to a mulch basin or trench at a garden, not onto leaves or root crops. Matches Ohio's own Type 3 hand-carried allowance.
- Rules the game could enforce for flavour: use it fast (a few in-game hours), do not put it on root crops, do not use it on frozen ground (Winter).
- Livestock drinking water stays on clean sources: well, spring, rain storage, filtered or boiled pond or creek water.
- Soap is a plant-hazard variable: only plain soap ("soap-only" greywater), which fits the game's existing crafted-soap loop if there is one (I have not checked).

---

# 4. Garden / Koi Ponds

## Real-world numbers

- Minimum depth about 3 ft in general; colder climates need deeper. Depths over 10 ft risk low oxygen near the bottom (Next Day Koi).
- Aquascape says keep at least 2 ft for winter, deeper is more stable, fish rest at the deepest spot in winter. (Two sources give 2 ft and 3 ft as minimums; neither gives an Ohio frost-line figure.)
- Ponds freeze from the top down and can form 6 in or more of ice (Next Day Koi).
- Stocking: minimum 1,000 gal; 10 sq ft of surface per koi; 10 gal per inch of fish (Next Day Koi). Example: 10 koi of 20-24 in total about 2,400 gal.
- Volume formulas: rectangular L x W x average depth x 7.5 = gallons; round diameter squared x average depth x 5.9 = gallons.
- Winter care: stop feeding when water falls to about 50 F (Aquascape, Pond Informer's second figure) or 55 F sustained for over 7 days (Pond Informer's first figure). Pond Informer gives both numbers without reconciling them. Use "about 50-55 F".
- Keep a hole open in the ice with a floating de-icer plus an air pump. Never break ice by force because shock can injure fish. Switch off waterfalls (they cool the deep water). In freezing climates drain and store external filters and pumps.

## Game mapping (Claude, not yet confirmed)

- A dug garden pond with a depth tier; the Winter ice thickness vs depth decides whether fish survive.
- Fish stop eating below the 50-55 F range, so Winter feed need drops to zero but oxygen becomes the risk.
- Ice-hole / de-icer maintenance as a Winter chore for a koi pond. This may be too fiddly for the game; consider whether koi are only ornamental (a morale or beauty bonus) rather than food.

---

# 5. Farm Pond Fish (food and pond drying)

## Real-world numbers (Kentucky Fish and Wildlife)

- At least 5 ft of water before stocking so it does not freeze solid.
- Per surface acre, new pond, fingerlings: bass 120, bluegill 400 (bass-bluegill); add catfish 50; replace 25-30% of bluegill with redear if used. Sub-adult stocking: bass 30, bluegill 100.
- Ponds under 1/2 acre are hard to balance with several species; suggest hybrid sunfish or channel catfish only. Channel catfish only: 100 sub-adults per acre.
- No fishing in year 1. Bluegill harvestable the second summer. Bass minimum about 15 in.
- Harvest 40-50% of annual production by weight. Fertile 1-acre pond: bass harvest 25-35 lb/yr, bluegill 100-150 lb/yr. Productivity 50 lb/acre (infertile) to 400+ lb/acre (fertile). Harvest rule: for every 1 lb of bass removed, remove 4-5 lb bluegill.
- This is a Kentucky agency page. Ohio DNR's own stocking guidance was not fetched; I'd confirm with Ohio DNR before quoting Ohio numbers.

## Why this matters for pond drying (Claude, not yet confirmed)

Your open "Pond drying: not yet designed" item can lean on this: a pond that is too shallow dries or freezes out and loses its fish, at least 5 ft is the "safe" depth, and a small pond supports only a small harvest (about 4-5 lb of bass per year for a 1/4-acre infertile pond). A game pond is probably well under 1/2 acre, so channel catfish or hybrid sunfish only is the realistic stock.

---

# 6. Aquaponics

## Real-world numbers (FarmHub small-scale aquaponics, one source)

- Fish density: up to 20 kg per 1,000 L (recommended 10-20); tanks under 500 L: 1 kg per 100 L. Do not grow fish for eating in tanks under 500 L.
- Feed rate: 40-50 g per sq m of grow bed per day for leafy greens, 50-80 g for fruiting vegetables. Fish feeding 1-2% of body weight per day.
- Grow bed: about 4 sq m of bed per 1,000 L of tank. Feed example: 200 g/day for a 1,000 L system.
- Plants: 20-25 per sq m leafy, 4-8 per sq m fruiting. Fruiting vegetables need about one-third more nutrients.
- Pump flow about 2 tank volumes per hour. Media-bed biofilter: 1 L of volcanic gravel per g of daily feed.
- Nitrate under 5 mg/L: raise feed slowly; over 150 mg/L: exchange water.
- Tilapia example: about 50 g at stocking, 500 g at harvest.
- Not stated by that page: bed depth and daily water top-up.

## Cold-climate evidence (SARE farmer report FW13-142, one source)

- Tilapia optimal 65-80 F, lowest survivable 60 F; winter nights in the greenhouse dropped below 60 F, which was the main obstacle, solved with aquarium heaters. No heating cost figures given.
- Larger fish survived cold better. One fish lost over winter. Eight fish reached about 1.5 lb. Summer overheating also needed shade and cooling.
- Not verified: whether tilapia may legally be kept in Ohio. Check before using tilapia in the design.

## Game mapping (Claude, not yet confirmed)

- Aquaponics fits as a late, high-investment greenhouse system: a fish tank, a grow bed, a pump and a heater, tied to a power system (your Power_System is still unbuilt) or a manual-labor version.
- It is the highest-yield sustainability option on a small footprint but also the most fragile in Winter (heat and electricity), so a good late-game goal with real tradeoffs.
- Because power does not exist yet, it should come AFTER Power_System, not before.

---

# 7. Open questions and gaps

1. Ohio frost depth / ice thickness numbers for pond depth: not found.
2. Ohio DNR stocking numbers (I used Kentucky): not found.
3. Tilapia legality and heating cost in Ohio: not verified.
4. Greywater on livestock FORAGE or fodder: no source.
5. Roof loss factor for rain collection: not verified.
6. Bethel-specific rainfall and monthly distribution: not found.
7. Pond drying mechanics (evaporation per day, drought drawdown): not researched yet.

---

# Sources

- FarmHub, Small-scale aquaponic food production, component calculations and ratios: https://learn.farmhub.ag/resources/small-scale-aquaponic-food-production/8-management-and-troubleshooting/component-calculations-and-ratios/
- SARE, Final Report FW13-142 (tilapia aquaponics in a cold greenhouse): https://projects.sare.org/?p=479278
- Next Day Koi, How deep should your koi pond be: https://nextdaykoi.com/pond-maintenance/how-deep-should-your-koi-pond-be/
- Pond Informer, Koi pond winter care guide: https://pondinformer.com/koi-pond-winter-care-guide/
- Aquascape, Koi and pond fish over the winter: https://support.aquascapeinc.com/hc/en-us/articles/203285075-What-do-I-do-with-my-Koi-and-Pond-Fish-over-the-winter
- Kentucky Fish and Wildlife, Farm pond management, stocking and harvesting: https://fw.ky.gov/Fish/Pages/Farm-Pond-Management-Stocking-and-Harvesting.aspx
- Ohio Administrative Code 3701-29-17: https://codes.ohio.gov/ohio-administrative-code/rule-3701-29-17
- OSU Extension, Gray water factsheet AEX-GW1: https://ohioline.osu.edu/factsheet/aex-gw1
- University of Arizona, Backyards (greywater): https://cales.arizona.edu/backyards/sites/cals.arizona.edu.backyards/files/p11-12_1.pdf
- NWRI Independent Advisory Panel, recycled water for livestock watering: https://watereuse.org/wp-content/uploads/2021/03/NWRI.Panel_.Livestock.Watering.FINAL_.pdf
- Ontario, Water requirements of livestock: https://www.ontario.ca/page/water-requirements-livestock
- Clemson Extension, Planning for livestock water needs: https://blogs.clemson.edu/livestock/planning-for-livestock-water-needs/
- Midwest Permaculture, rainwater project: https://midwestpermaculture.com/?p=17482
- CurrentResults, Average annual precipitation for Ohio: https://www.currentresults.com/Weather/Ohio/average-yearly-precipitation.php

---

# Addendum 2026-10-18 (found while drafting the Rainwater and Pond Drying design)

- Pond evaporation: a pond can lose up to 1 inch of water per day in summer heat, several feet over a summer; falling levels are often first noticed in August; heavy downpours can refill a pond but dry summers lose water faster than it returns (Mississippi State Extension via Farm Progress, ONE source, Mississippi, upper bound). No Ohio figure and no oxygen or fish-kill figure found.
- Rain intensity: light up to 0.10 in/h, moderate 0.11 to 0.30 in/h, heavy 0.30 in/h and over (Fox Weather). Metric conversions (about 2.5, 2.8-7.6 and 7.6+ mm/h) are mine.
- Metric shortcut: 1 mm of rain on 1 m2 is 1 litre.
- Sources: https://www.farmprogress.com/management/weather-patterns-impact-pond-levels and https://foxweather.com/learn/how-heavy-is-it-really-going-to-rain
- The design that uses these numbers is the "Rainwater Collection and Pond Drying (design draft)" section at the end of Water_System.md.


## Addendum: flash floods and pond overflow (2026-10-18)

- Flash flood: a quick rise in a stream that begins within about six hours of the cause (intense rain, dam failure, ice jam). NWS glossary: https://forecast.weather.gov/glossary.php?word=flash
- Danger depth: about 6 inches of fast-moving water can knock an adult down; about 2 feet can carry away a vehicle. "Turn Around, Don't Drown" (Army article): https://www.army.mil/article/151521/during_heavy_rains_turn_around_dont_drown (the article gives the six-inch and two-foot figures; it does not give a separate figure for floating a car).
- Pond overflow and spillway: floating debris can clog overflow pipes; failed spillways can lead to overtopping and dam damage; erosion in channels. Oklahoma State Extension: https://extension.okstate.edu/articles/2022/pond-cleanup-2022.html (does not address muddy water or fish loss).
- Fish and floods: floods swept young sunfish downstream and some adults left ponds; rapid pH swings after heavy rain following drought stressed or killed fish in reported cases. Bass Resource: https://bassresource.com/fish_biology/floods-droughts.html (does not discuss turbidity or bite behavior).
- Gaps: no sourced Ohio or Clermont County flood-season statistics gathered in this pass; no sourced figure for how long runoff contamination persists (the 12-hour value in the design is a game-tuning guess).
