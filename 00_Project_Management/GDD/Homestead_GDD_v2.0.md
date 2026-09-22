# HOMESTEAD
## Master Game Design Document v2.0

### Motto
Survive. Prepare. Build. Prosper.

### Status
Pre-Production

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

Homestead is a realistic first-person homesteading simulator focused on survival, self-sufficiency, land stewardship, and long-term property development.

Players begin with an undeveloped parcel of land, basic tools, camping equipment, and limited resources.

Success is not measured by wealth alone.

Success is measured by:

- Reliable water
- Secure shelter
- Adequate food reserves
- Winter preparedness
- Livestock management
- Property improvements
- Long-term self-sufficiency

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

The player begins as a survivor and eventually becomes a land steward and homestead owner.

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

The player should never feel forced to sell all resources for money.

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

Survive the first year.

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

The economy is based on surplus.

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