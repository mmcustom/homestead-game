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

**Correction 2026-09-26 (Mike) — reverses the gating above.** A wooden Bucket can't actually go over a campfire to boil water without burning. Boiling should be gated on carrying a new Tool, a metal cooking pot, instead — not the Bucket. The Bucket's role narrows to transporting water and other liquids (its existing hand-carrying job everywhere else), never boiling. This is a real change to already-shipped, committed code (the current Boil button check), not just a doc update. `water-pouring-a.wav` (Audio_System.md) is meant for this: pouring carried water out of the Bucket into the cooking pot before boiling. Exact capacity, the pour interaction, and whether the cooking pot is placed (like the Campfire) or a carried Tool are Claude Code's call — see Primitive_Storage_System.md and Item_Data.md's new Tools row.

**Built, tested, committed and pushed 2026-09-26 (Claude Code)** as part of commit `4145888` (`8b3ea6e` carries the doc updates). Boiling now refuses outright without the cooking pot: "Boiling water needs a Cooking Pot – the wooden Bucket would burn." The Bucket carries water only. Boiling first pours carried water from the Bucket into the pot, up to 4 L at a time, playing `water-pouring-a.wav`; the pot itself is carried, not placed. **Claude Code's own call, flagged for Mike to confirm:** the pot isn't craftable — nothing on the property can produce metal, so it's in the new-game starting kit alongside the Bucket, with F11 granting one to existing saves that predate it (including Mike's own). A craftable clay pot or similar was offered as a quick change if Mike would rather it be earnable than given. Tested in Play Mode: boiling refused without the pot, worked once equipped.

**Confirmed 2026-09-26 (Mike):** keep the metal cooking pot as a starting-kit item, not craftable — Claude Code's implementation above stands as-is, no code change needed. **Future — Clay Pottery.** Mike likes the idea of clay pots (and eventually cups, plates, bowls, etc.) craftable from clay deposits found around the property, as a later addition — not this batch, not yet designed in any detail (no deposit locations, recipe, or which vessels come first). Logged here so the idea isn't lost; Property_Layout.md would need a new clay-deposit terrain feature (same pattern as Stone_Gathering_System.md's cliff-side rock deposit) whenever this gets picked up.

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
