# Homestead
## Trapping System v1.0

Status: Design Draft

---

# Purpose

The Trapping System provides passive food acquisition and material gathering.

Trapping is one of the primary Year One food sources.

The system rewards:

- Knowledge
- Planning
- Observation
- Daily maintenance

Trapping should never feel like free food.

Success depends on placement and understanding wildlife behavior.

---

# Core Philosophy

A trapper works smarter, not harder.

The player should:

Discover Animal Activity
↓
Place Trap
↓
Bait Trap
↓
Wait
↓
Check Trap
↓
Harvest Resources

---

# Early Game Trap Types

## Rabbit Snare

Purpose:

- Meat
- Small Furs

Requirements:

- Cordage
- Suitable location

**Sourcing Cordage (confirmed 2026-09-25, Mike):** early game, craftable from animal sinew/tendons, tall grasses, or cattail stalks — an ad-hoc recipe like Fire Building's Campfire, not part of a generalized crafting system (see Decisions_Log.md's 2026-09-25 crafting-deferral entry). Cattails are already a Foraging resource (Item_Data.md). Later in the game, Cordage also becomes craftable at a Spinning Wheel from Cotton. Neither path is built yet — in-game right now, Cordage only comes from the F12 debug kit and the New Game starting kit's 3 units (see Item_Data.md's Materials table). Hunting_System.md's Recurve Bow arrows also use Cordage, so this recipe covers both.

**Update 2026-09-26 (Mike) — now an active build ask, not just Future.** Reconfirmed the same natural methods (tall grasses, cattail stalks, animal tendons, "and other natural methods" — leaves room for a fourth source if one comes up), and added a new path: Cordage should also be purchasable from a store with money. The natural-crafting side is a real gap worth closing now — the new Stone Pick Axe, Primitive Shovel, and Pouch (Primitive_Storage_System.md) each consume one Cordage to craft, using up all 3 a new game starts with and leaving none for snares or the Fish Trap. Requesting: at least one gatherable/craftable source built now (which raw material(s) yield Cordage, and in what quantity, are Claude Code's call — propose a first pass). The store-purchase path stays Future for now, since Economy_System.md has no actual store/purchasing mechanic built yet — no currency, no shop UI, nothing to hook a purchase into. Animal Tendons doesn't exist as an item yet either; if Claude Code uses it as one of the Cordage sources, it'll need adding to Item_Data.md's Resources table (likely a Hunting/Trapping byproduct alongside Small Furs/Hide/Antlers). See Current_Task_List.md's new "Feature: Cordage Sourcing" entry.

**Built and tested 2026-09-26 (Claude Code) — commit/push in progress, not yet confirmed.** A single Cordage craft button uses whichever fibre the player is carrying: 3 Tall Grass, 2 Cattail, or 1 Sinew. Tall Grass is a brand-new Foraged Plant, 90 clumps placed across open meadow and pasture via a new editor-only "Homestead → Place Tall Grass" menu; cutting a clump with E gives 3 Tall Grass and the clump regrows after 4 in-game days, with dry stalks usable in Winter too. Sinew is also new — Claude Code's choice over a standalone "Animal Tendons" item — a field-dressing byproduct: 2 per deer, 1 per turkey. Cattail is the existing forage item, but its single marsh patch only bears in Spring, so Tall Grass and Sinew are the main day-to-day sources. The store-purchase path stays Future, per above. Both new items (`tall_grass`, `sinew`) are now added to Item_Data.md's Resources tables. Tested in Play Mode: cut a grass clump (+3), crafted Cordage from each of the three sources, got a clear "Needs…" message with none, and confirmed the clump regrows after 4 days.

Best Placement:

- Rabbit trails
- Brush edges

---

## Box Trap

Purpose:

- Small game

Requirements:

- Wood
- Bait

Best Placement:

- Near food sources

---

# Future Trap Types

Not Alpha 0.1

Examples:

- Cage Traps
- Deadfall Traps
- Water Traps
- Advanced Fur Traps

---

# Trap Placement

Trap success depends on location.

Factors:

- Animal traffic
- Seasons
- Nearby food
- Weather

Poor placement greatly reduces catches.

---

# Bait System

Some traps require bait.

Examples:

- Apples
- Berries
- Vegetables
- Grain

Bait quality affects success rates.

---

# Trap Maintenance

Traps require periodic inspection.

Players must:

- Check traps
- Repair traps
- Re-bait traps

Neglected traps become less effective.

---

# Catch Results

Possible outcomes:

No Catch

Small Catch

Successful Catch

Trap Damage

Bait Consumed

Each check should feel somewhat unpredictable.

---

# Harvestable Materials

## Rabbit

Resources:

- Meat
- Small Furs

---

## Squirrel

Resources:

- Meat
- Small Furs

---

## Future Wildlife

Resources vary by species.

---

# Seasonal Effects

## Spring

Increased animal movement.

---

## Summer

Good trapping opportunities.

---

## Fall

Excellent trapping opportunities.

Animals are actively feeding.

---

## Winter

Lower activity.

Certain trap lines become more important.

---

# Discovery Integration

Successful trappers learn:

- Animal trails
- Feeding areas
- Seasonal patterns

Journal updates automatically.

Examples:

Rabbit Trail Discovered

Squirrel Feeding Area Discovered

---

# Economy Integration

Trapping supports:

Food Supply
↓
Small Furs Collection
↓
Surplus Income

Small Furs may be sold.

Meat may be consumed or preserved.

---

# Preservation Integration

Trapped animals provide:

- Fresh Meat
- Fur

Meat can be:

- Cooked
- Dried
- Smoked

Preservation becomes increasingly important approaching winter.

---

# Design Rules

1. Trapping is not guaranteed food.

2. Knowledge increases success.

3. Trap placement matters.

4. Daily maintenance matters.

5. Trapping complements hunting and fishing rather than replacing them.