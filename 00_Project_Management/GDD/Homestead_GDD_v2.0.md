# HOMESTEAD
## Master Game Design Document v2.0

### Motto
Survive. Prepare. Build. Prosper.

### Status
Pre-Production

### Revision Note

2026-10-20: Updated v2.0 in place to add the land purchase and debt, the late fee and loss rules, and three new pillars (Money Shapes Every Decision, Modern Tools Are Earned, Everything Connects). The wording of the Executive Summary, Vision Statement, Self-Sufficiency Before Profit, Stage 1 goal, Economy System and Alpha scope was changed to match. See the Homestead Vision and Design Pillars doc.

### Project Lead
Mike Mack

### Engine
Unity 6 HDRP

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

Modern conveniences and equipment are part of the game. Modern homesteaders use them to become self-sufficient over time, and the player earns them through hard work and discipline. The game progresses from primitive tools and structures to current systems and methods.

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

Open: early income must be rebalanced so a payment of about $459 to $573 every 28 days is reachable after the first year, and the larger parcels need much more income.

## Seasonal Account Statement

At the end of every season, the game shows the player an account statement. The finances are listed in black when positive and red when negative, as on a real statement. It shows the player where the money went and rewards extra payments by making the progress on the debt visible.

The statement shows income and expenses for the season, and it also shows numbers compiled over time, as the budget for the property and household.

Suggested extra contents, not yet decided: starting and ending cash, payments made and any late fees, extra payments, interest paid, loan balance, and the projected payoff date.

## Payments, Late Fees, and Losing

- A payment is due every 28 days, at the monthly-sized amount, starting at the end of the first year (day 112). The player can pay less than the full amount (applied to the late fee, then interest, then principal), but a payment still short after the 10 days counts as missed.
- A payment that is 10 days late gets a $75 late fee, added on top of the regular payment, and counts as missed.
- Three missed payments is a fail state: the game is lost. Catching up clears a missed payment (it stays missed until paid with its late fee), so the loss is three open at once.
- On a loss, the game offers to restart or load a save, with a banner or text telling the player that mother nature beat them. A loss never overwrites the autosave, so Continue loads the last safe autosave.

## Timing

Money enters early. A bare version of the land payment and balance comes in well before the income systems. Income streams follow later. The first payment falls due at the end of the first year (day 112), then every 28 days. To keep the early game winnable, the player starts on a smaller and cheaper starter parcel, and early income is raised (sizes, prices and income numbers still to be set). The full design is in Money_System.md.

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

Possible income sources:

Early:

- Furs
- Hides
- Mushrooms
- Nuts
- Berries

Homestead:

- Eggs
- Goat Milk
- Cheese
- Produce

Advanced:

- Wool
- Livestock
- Fruit
- Lumber
- Honey
- Maple Syrup

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

45-60 Minutes

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

# Current Alpha 0.1 Scope

Required Systems:

- First Person Controller
- Inventory
- Hydration
- Hunger
- Fire Building
- Water Collection
- Water Purification
- Foraging
- Fishing
- Fish Traps
- Tent Placement
- Sleeping
- Discovery System
- Journal
- Minimap
- Save System
- Land Payment and Balance (bare version, added 2026-10-20, confirm Alpha placement)

Nothing beyond this scope should be developed until the survival loop is fun.

---

# Development Rule

Do not add major systems such as:

- Farming
- Large livestock operations
- Vehicles
- Solar arrays
- Automation

until the core survival loop has been proven enjoyable and stable.