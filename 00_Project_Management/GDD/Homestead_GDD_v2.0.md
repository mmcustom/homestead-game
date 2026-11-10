# HOMESTEAD
## Master Game Design Document v2.0

### Motto
Survive. Prepare. Build. Prosper.

### Status
In Development (pre-release)

### Revision Note

2026-10-20: Updated v2.0 in place to add the land purchase and debt, the late fee and loss rules, and three new pillars (Money Shapes Every Decision, Modern Tools Are Earned, Everything Connects). The wording of the Executive Summary, Vision Statement, Self-Sufficiency Before Profit, Stage 1 goal, Economy System and Alpha scope was changed to match. See the Homestead Vision and Design Pillars doc.

2026-11-08: v2.1, brought in line with the build. The status, game day length, engine version, starting weapon, locked parcels and tier levers were corrected; difficulty tiers, production chains, tools and combat, art direction, the Know-how journal and the save system were added; the Economy income list and the Stage 1 and 2 notes were updated to what is built; and the Alpha 0.1 scope lock and Development Rule were retired in favour of roadmap-driven scope (Mike, 2026-11-08). The vision now states the move from primitive to modern systems explicitly.

### Project Lead
Mike Mack

### Engine
Unity 6.3 LTS (6000.3.24f1), HDRP

### Modeling Software
Blender

### Source Control
Git

---

# Executive Summary

Homestead is a realistic first-person homesteading life simulator with a tycoon spine, focused on self-sufficiency, land stewardship, debt, and long-term property development. It is not a survival game or a farming simulator: surviving is where the player starts, not the goal.

Players begin by buying an undeveloped parcel of land with borrowed money, with a payment due every month, plus basic tools, camping equipment, limited resources, and possibly some saved money.

Success is not measured by wealth alone.

Success is measured by:

- Reliable water
- Secure shelter
- Adequate food reserves
- Winter preparedness
- Livestock management
- Property improvements
- Long-term self-sufficiency
- Paying off the land debt
- Profit from the land's resources and products

The player gradually transforms wilderness into a thriving homestead.

---

# Vision Statement

The goal of Homestead is to create the most authentic homesteading simulator possible while remaining enjoyable to play.

The game prioritizes:

- Realistic progression
- Knowledge-based gameplay
- Meaningful decision-making
- Seasonal planning
- Long-term development

The player begins as a survivor and eventually becomes a land steward and homestead owner: debt-free, self-sufficient, and profiting from the land's resources and manufactured products.

Modern conveniences and equipment are part of the game. Modern homesteaders use them to become self-sufficient over time, and the player earns them through hard work and discipline. The game progresses from primitive tools and structures to current systems and methods. This move past the primitive level, from hand tools and hand-built shelter to modern systems (pressure water, solar power, powered equipment and a modern farmhouse), is the long arc of every game.

Finances shape most aspects of life, and the game plays on that. See the Land Purchase and Debt section.

---

# Core Design Philosophy

## Water First

Water is the most important survival resource.

Humans can survive much longer without food than without water.

Players must:

- Locate water
- Transport water
- Purify water
- Protect water supplies

Water systems drive many other gameplay systems.

---

## Knowledge Is Power

Information is valuable.

Players learn:

- Wildlife behavior
- Fishing locations
- Plant locations
- Water sources
- Seasonal patterns

Knowledge is stored through:

- Journal entries
- Map markers
- Discovery records

---

## Self-Sufficiency Before Profit

The homestead comes first.

Production order:

Personal Use

↓

Stored Reserves

↓

Livestock Needs

↓

Surplus

↓

Market Sales

The player should never feel forced to sell the reserves the homestead needs to survive. The monthly land payment is the reason to sell surplus, never reserves.

---

## Preparation Beats Panic

Successful players plan ahead.

Systems include:

- Food storage
- Firewood reserves
- Animal feed reserves
- Winter preparation
- Storm preparation

---

## Money Shapes Every Decision

Finances dictate most aspects of life, and the game understands that.

The monthly land payment, the cost of materials and equipment, and the income from surplus all pressure each choice the player makes.

---

## Modern Tools Are Earned

Modern equipment is allowed, and it is reached through hard work and discipline, not handed over.

Each step from primitive to modern needs the materials, the infrastructure, and the money or the skill to craft it. Modern items can be bought with money, crafted, or both.

---

## Everything Connects

Systems depend on each other.

A gap in one (water, fuel, food, cash) shows up in the others.

---

# Progression Overview

## Stage 1 - Survival

Housing:

- Tent
- Lean-To
- Primitive Shelter

Food:

- Foraging
- Fishing
- Fish Traps
- Hunting
- Trapping

Water:

- Springs
- Creeks
- Rain Collection
- Wells

Primary Goal:

Survive the first year and meet the land payments.

Built so far (Stages 1 and 2 together): tent placement, campfire and Tripod, Well, Outhouse, Wood Shed, Covered Outdoor Kitchen, Workshop, Potter's Shed and Kiln, Charcoal Pit, Bloomery and Smithy, and the Hand Cart. The Small Cabin, Root Cellar, Smokehouse, livestock and the later stages are designed, not built. See Development_Roadmap.md.

---

## Stage 2 - Early Homestead

Housing:

- Small Cabin

Infrastructure:

- Root Cellar
- Smokehouse
- Hand-Pump Well
- Storage Shed

Livestock:

- Goats
- Chickens
- Rabbits

Primary Goal:

Food Security

---

## Stage 3 - Established Homestead

Housing:

- Larger Cabin
- Cottage

Infrastructure:

- Barn
- Orchard
- Windmill
- Small Solar Systems

Livestock:

- Expanded Herds
- Sheep
- Pigs

Primary Goal:

Independence

---

## Stage 4 - Modern Homestead

Housing:

- Farmhouse

Infrastructure:

- Pressure Water
- Solar Arrays
- Battery Banks
- Workshops
- Equipment Buildings

Livestock:

- Cattle
- Horses

Primary Goal:

Prosperity

---

# Land Purchase and Debt

The player begins by buying land with borrowed money. The monthly payment is the constant pressure behind the whole game.

## The Arc

1. Buy the land for a large sum, with a payment due every month.
2. Develop it, starting with primitive tools and structures.
3. Become self-sufficient, so less money leaves the property.
4. Pay off all debt.
5. Profit from the resources and manufactured products of the land.

## Starting Money

The player can start with saved money as well as the debt.

- At the start, the player chooses how to split the savings between a down payment on the land and a cash reserve.
- A bigger down payment means a smaller monthly payment but less cash for tools and materials. A bigger reserve means the opposite.
- The amount of savings is a percentage of the land price, so bigger or pricier land starts with proportionally more, and the percentage scales with the difficulty tier: Settler gets 20%, Homesteader gets 40%, and Pioneer gets none. Savings are capped at $30,000 for every tier, so the biggest parcels do not start with huge cash.
- Pioneer is the zero-savings hard option: the land is fully financed and the player has almost no cash.
- The starting weapon is fixed by tier (decided 2026-10-23): Pioneer starts with the Recurve Bow and 20 arrows; Settler and Homesteader start with the Bolt-Action Rifle and 10 rounds.

## Loan Terms

The loan is a standard fixed-rate loan, chosen to match typical land loans (2026 estimates run about 8 to 10 percent interest and 5 to 15 years for raw land, and lower rates and longer terms for improved land).

- Interest rate: 8% a year, fixed.
- Term: 15 years, which is 180 payments. A payment falls due every 28 days (the length of a season), at the monthly-sized amount, so a game "month" is 28 days. The interest is 8% a year spread over 12 payments. The full loan is about 45 game years, so extra payments are what let a player finish.
- The loan amount is the land price minus the down payment. The monthly payment is fixed for the life of the loan and covers interest and principal.
- Example, for illustration only: on land priced at $200,000, savings are $40,000. With all of it as a down payment the loan is $160,000 and the payment is about $1,529 a month. With none of it as a down payment the loan is $200,000 and the payment is about $1,911 a month, so putting all savings down saves about $382 a month.
- Extra payments: the player can make extra payments at any time, and the game encourages it. Extra money reduces the balance, so the debt is paid off sooner and less interest is paid overall. Extra payments go to principal, the required monthly payment stays the same, there is no prepayment penalty, and an extra payment never covers a future month, so the missed-payment rule is unaffected. This supports the goal of paying off all debt.
- Real raw land lenders often want 30 to 50 percent down, so a 20% limit on the down payment is easier than reality. This is a deliberate game simplification.

## Land Price

The price depends on the size of the property and its location. Real costs are researched so prices reflect actual markets. The starting region is the lead's current region (Bethel, Ohio area), and other regions can be added later. Prices are real-world numbers, rounded (decided 2026-10-20).

At a new game the player chooses the starter parcel from three sizes. The prices are bare rural land near Bethel, Ohio (Clermont County), rounded from 2026 listings. Small parcels cost more per acre than large ones.

| Parcel | Price | Per acre | Loan after 20% down | Payment every 28 days (20% down) | Payment with no down payment |
|---|---|---|---|---|---|
| 5 acres | $60,000 | $12,000 | $48,000 | about $459 | about $573 |
| 10 acres | $140,000 | $14,000 | $112,000 | about $1,070 | about $1,338 |
| 40 acres | $450,000 | $11,250 | $360,000 | about $3,440 | about $4,300 |

Sources behind the numbers: 2026 listings near Bethel ran about $12,400 an acre (5 acres, Bear Creek Rd), $19,000 an acre (5 to 6 acres, Rose Hill Rd and Mount Olive Point Isabel Rd), and about $11,000 an acre for 34 to 40 acres (Antioch Rd and Dean Rd). Ohio cropland averages $10,100 an acre in 2026 (US $6,020). Small lots with a house or road frontage cost far more per acre and are not modelled. Not every listing was confirmed as bare land.

The 5-acre parcel is the intended first game. The 10 and 40 acre parcels are harder to carry and are for players who want more room or a bigger debt. Other regions can be added later with the same method.

In the current build only the 5-acre parcel can be chosen. The 10-acre and 40-acre cards are shown locked ("Needs more income than the early game offers. Unlocks later."), decided 2026-10-25, and early income is tuned for the 5-acre parcel.

Open: early income must be rebalanced so a payment of about $459 to $573 every 28 days is reachable after the first year, and the larger parcels need much more income.

## Seasonal Account Statement

At the end of every season, the game shows the player an account statement. The finances are listed in black when positive and red when negative, as on a real statement. It shows the player where the money went and rewards extra payments by making the progress on the debt visible.

The statement shows income and expenses for the season, and it also shows numbers compiled over time, as the budget for the property and household.

Suggested extra contents, not yet decided: starting and ending cash, payments made and any late fees, extra payments, interest paid, loan balance, and the projected payoff date.

## Payments, Late Fees, and Losing

- A payment is due on the 1st of every month, starting July 1 of Year 1 (day 122). The player can pay less than the full amount (applied to the late fee, then interest, then principal), but a payment still short after the 10 days counts as missed.
- A payment that is 10 days late gets a $75 late fee, added on top of the regular payment, and counts as missed.
- Three missed payments is a fail state: the game is lost. Catching up clears a missed payment (it stays missed until paid with its late fee), so the loss is three open at once.
- On a loss, the game offers to restart or load a save, with a banner or text telling the player that mother nature beat them. A loss never overwrites the autosave, so Continue loads the last safe autosave.
- Difficulty tiers carry four levers in code: missed payments before a loss, the late fee, the interest rate and the Trading Post sell share. Today every tier holds the same values (3 missed, $75, 8 percent, 100 percent sell share). Setting them is Money Phase D2, which waits on income measuring.

## Timing

Money enters early. A bare version of the land payment and balance comes in well before the income systems. Income streams follow later. The first payment falls due on July 1 of Year 1 (day 122), then on the 1st of every month. To keep the early game winnable, the player starts on a smaller and cheaper starter parcel, and early income is raised (sizes, prices and income numbers still to be set). The full design is in Money_System.md.

---

# Difficulty Tiers

Three tiers, chosen at a new game (see Difficulty_System.md):

- **Pioneer:** the hardest. No savings, the land is fully financed, and the player starts with the Recurve Bow.
- **Settler:** the middle tier. Savings are 20 percent of the land price. Starts with the Bolt-Action Rifle.
- **Homesteader:** the easiest. Savings are 40 percent of the land price. Starts with the Bolt-Action Rifle.

Savings are capped at $30,000 for every tier.

---

# Production Chains

Self-sufficiency and income both come from chains where the player turns raw land resources into goods. Each chain has its own design doc and a Know-how entry. Built so far:

- **Ash and Kindling:** burning wood leaves Ash, which sells, and feeds Lye and Potash. Kindling Bundles are made from sticks. See Early_Income_Design.md.
- **Maple sugaring:** tap Sugar Maples in late winter, boil sap to Syrup. See Maple_Sugaring.md.
- **Tripod and Potash:** a Tripod over a campfire holds the Cooking Pot or the Potash Kettle; a Leach Hopper makes Lye from Ash and water, boiled to Potash.
- **Pottery:** Sand and Clay from the creek banks, shaped and dried in the Potter's Shed, fired in the Kiln.
- **Metal:** Charcoal Pit, Bloomery and Smithy; ore is bought at the Trading Post. First goods are Hinges, Hooks, Nails and iron tools. See Mining_Metalworking_System.md.
- **Workshop:** a bench that makes Stools, End Tables, Chairs and Tables. See Workshop_and_Handmade_Goods.md.
- **Wood and hauling:** Wood Saw, Wooden Wheels, the Hand Cart, and trees that grow back over a game year. See Wood_Gathering_System.md, Vehicles_and_Hauling.md and Tree_Regrowth.md.

---

# Tools, Hands and Combat

Every tool is visible in the player's hands in first person. Fists stun and startle small game but never kill; the Knife slashes and stabs; traps, bows and the rifle kill. Animals cannot hurt the player. See Hands_and_Held_Tools.md and Melee_Fists_and_Knife.md.

---

# Save System

Ten manual save slots plus an autosave. The autosave runs at the end of each day, on sleep, on returning to the main menu and on quit, and it never overwrites a manual save. A lost game is never saved. See Save_System.md.

---

# Art Direction

Realistic first-person presentation with physically based (PBR) materials (decided 2026-11-05). Real art goes in through one swap point: a prefab at `Resources/Art/<id>.prefab` replaces the primitive shape the game draws by default. So far only the first-person hands are real art; every other tool, structure and world object is a primitive placeholder. The art list and build waves are in Art_Needed_List.md.

---

# Year One Philosophy

Year One should focus on survival and learning.

Players are not expected to build large farms during the first year.

Primary Food Sources:

- Foraging
- Fishing
- Fish Traps
- Hunting
- Trapping

Secondary Food Sources:

- Small Gardens

Priority Order:

1. Water
2. Fire
3. Shelter
4. Food
5. Winter Preparation

---

# Core Survival Systems

## Hydration

The most important player need.

Water quality influences health.

Water Sources:

- Spring
- Creek
- Pond
- Rain Collection
- Well

Water Quality Levels:

- Excellent
- Good
- Questionable
- Unsafe

Risks include illness and dehydration.

---

## Fire

Fire enables:

- Water purification
- Cooking
- Warmth
- Light
- Food preservation

Fire is essential during early survival.

---

## Hunger

Food categories:

- Meat
- Fish
- Fruit
- Nuts
- Vegetables
- Preserved Food

Balanced diets improve performance.

---

## Fatigue

Tracks:

- Sleep
- Energy
- Recovery

Players may:

- Sleep early
- Sleep normally
- Stay up late

Working late provides more productivity but less recovery.

---

# Discovery System

The player begins with limited knowledge.

As discoveries occur:

- Journal updates automatically
- Map updates automatically

Examples:

- Spring Found
- Fishing Hole Found
- Deer Trail Found
- Berry Patch Found
- Nut Grove Found

Discovery becomes one of the core gameplay systems.

---

# Journal System

The journal stores:

## Water Sources

- Location
- Quality
- Discovery Date

## Plants

- Season
- Uses
- Location

## Wildlife

- Habitat
- Activity Periods
- Notes

## Player Notes

Custom player observations.

## Know-how

A Know-how section teaches the player how each production chain works. Entries unlock as the player meets each chain, and short in-the-moment hints name the next step. Decided 2026-10-27, after a playtest showed the Potash chain could not be worked out without a checklist.

---

# Minimap System

The map begins mostly undiscovered.

Resources appear after discovery.

Examples:

- Springs
- Fishing Holes
- Deer Trails
- Berry Patches
- Buildings
- Pastures

The land should feel learned rather than revealed.

---

# Economy System

The economy is based on surplus, and it has a constant cost: the monthly land payment (see Land Purchase and Debt). Modern items can be bought with money, crafted, or both.

Income sources built so far, roughly in the order a new player meets them:

- Furs and hides (snares and hunting)
- Ash and Kindling Bundles
- Potash and Lye
- Maple Syrup
- Handmade goods from the Workshop (Stool, End Table, Chair, Table)
- Pottery
- Charcoal and iron goods (Hinges, Hooks, Nails, tools)

Planned, not built: Eggs, Goat Milk, Cheese, Produce, Wool, Livestock, Fruit, Lumber and Honey. Fish, forage and game meat are treated first as the homestead's food reserve (Self-Sufficiency Before Profit).

The Trading Post is a simulated market: each item group has a price that drops as the player sells and recovers over time, and a sell-surplus control sells everything above an amount the player chooses to keep. See Money_System.md, Trading_Post_Market.md and Early_Income_Design.md.

---
# Livestock Philosophy

Animals should provide practical value.

## Goats

- Milk
- Cheese
- Brush Control

## Chickens

- Eggs
- Meat
- Insect Control

## Rabbits

- Meat
- Breeding

Future livestock:

- Sheep
- Pigs
- Cattle
- Horses

---

# Time System

Game Day Length:

30 real minutes. The game uses a real 365-day calendar: a game year runs March to February, with 12 real-length months and no leap years. Spring is 92 days (Mar to May), Summer 92 (Jun to Aug), Fall 91 (Sep to Nov) and Winter 90 (Dec to Feb).

Built 2026-11-10 (Phase 1, playtest passed 16 of 16): the game now uses a real 365-day calendar (12 real-length months, no leap years; Spring March to May, Summer June to August, Fall September to November, Winter December to February), and a land payment on the 1st of every month from July 1. Monthly bills (phone, internet, utilities) will be charged on the 15th. A player-facing Wait control (key Z: 1, 3 or 6 hours, until dawn or until dusk) was built 2026-11-10 (Phase 2a, playtest passed 16 of 16). It shares one world-passing loop with sleep, and stops early for a low stat, severe weather, a rising creek, a dying fire, a trap catch or a notice. Dates read "Mar 1, Year 1"; the year is the game year, so "Jan 1, Year 1" follows "Dec 31, Year 1". See Calendar_and_Time_Scale_Options.md.

Design Rule:

The player should always have more work available than can be completed in a single day.

The challenge becomes prioritization.

---

# Sleep System

The player chooses when to rest.

Benefits of Sleeping:

- Energy Recovery
- Health Recovery
- Morale Recovery

Benefits of Staying Up:

- More work completed

Cost:

- Fatigue penalties

---

# Scope and Milestones

Scope is set by the Development Roadmap (`00_Project_Management/Development_Roadmap.md`) and Mike's next-step choices, not by a fixed feature list. Milestones are tracked in the roadmap only.

Farming, large livestock operations, powered vehicles, solar arrays and automation remain deferred until Mike chooses them.

The original Alpha 0.1 required list (first person controller, inventory, hydration, hunger, fire, water collection and purification, foraging, fishing, fish traps, tent placement, sleeping, discovery, journal, minimap, save system and a bare land payment) was met, and the core survival loop was judged fun by Mike on 2026-10-11. See Decisions_Log.md.
