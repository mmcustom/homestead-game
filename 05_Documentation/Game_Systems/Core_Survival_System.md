# Homestead
## Core Survival System v1.0

Status: Approved Design Draft

---

# Purpose

The Core Survival System governs all player survival mechanics during the Survival and Early Homestead phases of gameplay.

It is the foundation upon which all future systems depend.

Core Principles:

- Water First
- Fire Enables Survival
- Preparation Beats Panic
- Knowledge Is Power

---

# Core Survival Loop

Water
↓
Fire
↓
Safe Water
↓
Shelter
↓
Food
↓
Preparation
↓
Winter Survival

---

# Survival Attributes

## Health

Represents the player's overall physical condition.

Range:

0-100

Health is affected by:

- Dehydration
- Starvation
- Illness
- Exposure
- Fatigue

At 0 Health:

Player collapses.

(Player death mechanics TBD)

---

## Hydration

Highest priority survival stat.

Range:

0-100

Hydration decreases continuously.

Influencing factors:

- Temperature
- Physical activity
- Illness
- Weather

Effects:

### 75-100

No penalties.

### 50-74

Minor stamina reduction.

### 25-49

Reduced stamina.
Reduced work efficiency.

### 1-24

Severe penalties.
Health loss begins.

### 0

Rapid health loss.

---

# Water Sources (Summary)

Water quality ranges from Excellent (developed springs, wells) down to Unsafe (stagnant ponds, floodwater), with Good and Questionable covering moving water, rain collection, and slow streams in between.

Lower-quality sources carry higher illness risk and should be purified before drinking.

Full source list, exact quality ratings, transportation, and storage are defined in Water System documentation.

---

# Fire System

Fire is a cornerstone survival mechanic.

Functions:

- Water purification
- Food cooking
- Warmth
- Lighting
- Food preservation

Fire requires:

- Fuel
- Ignition source
- Maintenance

**Confirmed 2026-09-26 (Mike):** wants a Warmth meter on the HUD, and confirmed the full scope — real ambient exposure, not just fire proximity. This moves "Advanced temperature systems" out of Future Expansion at the bottom of this doc into current scope; see that section's note. Full attribute definition, decay factors, and effect tiers are now specced in Health_System.md's Exposure System section, which this used to just gesture at with no numbers.

---

# Hunger

Range:

0-100

Represents food energy reserves.

Food Categories:

- Meat
- Fish
- Fruits
- Nuts
- Vegetables
- Preserved Foods

Hunger decreases slower than hydration.

---

# Spoilage System

Purpose:

Encourage food preservation.

Raw food eventually spoils.

## Raw Fish

Short shelf life.

## Raw Meat

Moderate shelf life.

## Produce

Varies by crop.

Methods of preservation:

- Drying
- Smoking
- Root Cellar Storage

---

# Fatigue

Range:

0-100

Represents physical and mental exhaustion.

Affected by:

- Workload
- Sleep
- Nutrition
- Illness

---

# Sleep

Players choose when to sleep.

## Early Sleep

Benefits:

- Faster recovery
- Morale bonus
- Higher productivity

---

## Normal Sleep

Balanced recovery.

---

## Late Sleep

Benefits:

- Extra work completed

Costs:

- Reduced energy
- Increased fatigue

---

## All-Nighter

Emergency use only.

Heavy penalties next day.

---

# Morale (Summary)

Represents player outlook and motivation. Provides small efficiency and recovery modifiers.

Full Morale mechanics, increase and decrease sources, and effect tiers are defined in Health System documentation.

---

# Seasons

All survival systems are affected by seasons.

---

## Spring

Focus:

- Exploration
- Water discovery
- Foraging

Benefits:

- Increased rainfall

---

## Summer

Focus:

- Food gathering
- Property development

Benefits:

- Longer days

Risks:

- Heat
- Dehydration

---

## Fall

Focus:

- Hunting
- Food preservation
- Firewood

Benefits:

- Harvest opportunities

---

## Winter

Focus:

- Survival

Effects:

- Frozen surface water
- Increased firewood use
- Limited foraging

Winter serves as the annual survival test.

---

# Food Preservation

## Drying Rack

Preserves:

- Herbs
- Fruits
- Meat

---

## Smokehouse

Preserves:

- Fish
- Meat

---

## Root Cellar

Stores:

- Potatoes
- Carrots
- Apples
- Preserved goods

---

# Trapping System (Summary)

Passive food and small fur acquisition through placed, baited traps that must be checked and maintained.

Outputs:

- Meat
- Small Furs

Full trap types, placement, bait, and maintenance detail are defined in Trapping System documentation.

---

# Economy Connection

Survival resources are prioritized:

Personal Consumption
↓
Stored Reserves
↓
Livestock Needs
↓
Surplus
↓
Market Sales

---

# Future Expansion

Not Alpha 0.1

- Predator threats
- Animal population management
- Detailed diseases
- Veterinary systems
- ~~Advanced temperature systems~~ — moved into current scope 2026-09-26 (Mike) as the new Warmth attribute; see the Fire System section above and Health_System.md's Exposure System. Clothing as an actual Warmth factor stays Future — no clothing/apparel system exists yet.
- Beekeeping
- Maple syrup production — spec drafted and ready in `Game_Systems/Maple_Sugaring.md` (confirmed 2026-09-24), still deferred until moved into active scope

These features are deferred until the core survival loop is proven fun.

---

# Design Rule

Nothing should be added that undermines the core survival experience.

If Water → Fire → Safe Water → Food is not fun, all future systems must stop until it is.