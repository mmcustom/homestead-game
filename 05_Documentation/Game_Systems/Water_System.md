# Homestead
## Water System v1.0

Status: Design Draft

---

# Purpose

Water is the most important resource in Homestead.

The Water System influences:

- Survival
- Health
- Livestock
- Agriculture
- Property Development
- Seasonal Planning

Water scarcity can become one of the largest challenges facing the player.

---

# Core Philosophy

Humans can survive:

- Weeks without food
- Days without water

Water takes priority over food.

Water availability heavily influences homestead development decisions.

---

# Water Sources

## Natural Springs

Quality:

Excellent

Advantages:

- Reliable
- Clean
- Low maintenance

Disadvantages:

- May not exist on all properties

---

## Wells

Quality:

Excellent

Advantages:

- Reliable
- Year-round operation

Disadvantages:

- Expensive to construct

Requirements:

- Drilling
- Hand pump
- Future pump systems

---

## Creeks

Quality:

Good

Advantages:

- Common
- Renewable

Disadvantages:

- Usually requires purification

---

## Rivers

Quality:

Good

Advantages:

- Large water volume

Disadvantages:

- Contamination possible

---

## Ponds

Quality:

Questionable to Unsafe

Advantages:

- Storage reservoir

Disadvantages:

- High contamination risk
- Seasonal algae growth

---

## Rain Collection

Quality:

Good

Advantages:

- Renewable
- Low cost

Disadvantages:

- Weather dependent

---

# Water Quality Levels

## Excellent

Examples:

- Springs
- Wells

Illness Risk:

Very Low

**Confirmed 2026-09-25 (Claude Code, Phase 3 — built and tested, not yet committed):** implemented as 0% illness risk per litre, whether drinking straight from the source or from carried water.

---

## Good

Examples:

- Fast-moving creeks
- Rain collection

Illness Risk:

Low

**Confirmed 2026-09-25 (Claude Code, Phase 3 — built and tested, not yet committed):** implemented as 5% illness risk per litre. Empirically verified in Play Mode: 400 drinks from a Good-quality creek produced sickness 15 times, about 4%, close to the 5% target.

---

## Questionable

Examples:

- Slow streams

Illness Risk:

Moderate

Purification recommended.

**Confirmed 2026-09-25 (Claude Code, Phase 3 — built and tested, not yet committed):** implemented as 20% illness risk per litre.

---

## Unsafe

Examples:

- Stagnant ponds
- Floodwater

Illness Risk:

High

Purification strongly recommended.

**Confirmed 2026-09-25 (Claude Code, Phase 3 — built and tested, not yet committed):** implemented as 45% illness risk per litre.

---

# Water Purification

Methods:

## Boiling

Benefits:

- Eliminates most biological contamination

Requirements:

- Fire
- Container

**Confirmed 2026-09-25 (Claude Code, Phase 3 — built and tested, not yet committed):** implemented at a lit campfire — raw water rows in the Inventory screen get a Boil button, gated on carrying the Bucket (see Item_Data.md's Tools table). Boiling turns raw water into a Purified Water item at 0% illness risk, reusing the old, previously-unused generic `water` item id from Item_Data.md's Consumables table rather than adding a new one; it still counts toward the Bucket's capacity. The drink prompt on any non-Excellent water now warns the player before they drink, e.g. "Drink (Good water, slight risk of sickness)." Getting sick from unpurified water runs through the new Sickness mechanic on Health_System.md's Illness System section. Tested: boiling Good water into Purified Water worked via both the dedicated Boil button and a click on the water row in Inventory.

---

## Filtration

Future System

Benefits:

- Improves water quality

---

## Combined Purification

Highest water safety.

---

# Water Transportation

Early Game

## Hand Carrying

Methods:

- Buckets
- Yoke and Buckets

Advantages:

- No construction required
- Available from Day One

Disadvantages:

- Time-consuming
- Limited volume per trip
- Physically demanding

---

## Wagon or Cart Hauling

Methods:

- Barrel on Cart

Advantages:

- Larger volume per trip

Disadvantages:

- Requires a cart
- Requires passable ground

---

# Future Water Transportation

Not Alpha 0.1

Examples:

- Piped Distribution
- Pressure Water Systems
- Powered Pumps

Deferred until the core survival loop is proven fun.

---

# Water Storage

## Cistern

Purpose:

Bulk water storage from rain or hauled water.

Benefits:

- Reduces trips to source
- Buffer against dry spells

---

## Rain Barrel

Purpose:

Small-scale rain collection storage.

Benefits:

- Low cost
- Easy to construct early

---

# Discovery Integration

Water sources must be discovered before they appear on the map.

Journal Records:

- Location
- Water Quality
- Discovery Date

Example:

Discovery Unlocked

Natural Spring

Water Quality: Excellent

Journal Updated

Map Updated

Full discovery behavior is defined in Discovery System documentation.

---

# Season Effects

## Spring

Highest water availability.

Rain and snowmelt replenish most sources.

---

## Summer

Variable availability.

Droughts may occur.

Livestock water needs increase.

---

## Fall

Generally stable availability.

---

## Winter

Surface water may freeze.

Springs and wells remain valuable and may become the only reliable sources.

Detailed seasonal behavior is defined in Season System documentation.

---

# Livestock Integration

Livestock require daily access to clean water.

Water needs increase during:

- Summer
- Drought
- High activity

Full livestock water requirements are defined in Livestock System documentation.

---

# Economy Integration

Water infrastructure is a primary Economic Sink.

Examples:

- Well Drilling
- Cistern Construction

Full economic sink detail is defined in Economy System documentation.

---

# Design Rules

1. Water takes priority over food.

2. Water quality determines illness risk.

3. Purification reduces risk but requires fire and time.

4. Water availability should shape where and how the player builds.

5. Winter should make water noticeably harder to secure.
