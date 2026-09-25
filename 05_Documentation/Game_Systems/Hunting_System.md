# Homestead
## Hunting System v1.0

Status: Design Draft

---

# Purpose

Hunting is one of the five primary Year One food sources, alongside Foraging, Fishing, Fish Traps, and Trapping.

Hunting provides:

- High-yield meat
- Hides
- Antlers
- Knowledge progression
- Seasonal gameplay

Hunting should reward observation and patience rather than random chance.

---

# Core Philosophy

A knowledgeable hunter succeeds where an impatient hunter fails.

Players should learn:

- Animal habitat and trails
- Daily activity cycles
- Seasonal behavior
- Wind and scent awareness

Knowledge should outperform luck.

Hunting should never feel like guaranteed food.

---

# Core Hunting Loop

Locate Game
↓
Approach / Stalk
↓
Take Shot
↓
Track (if needed)
↓
Field Dress
↓
Harvest Meat, Hide, Antlers
↓
Transport Home
↓
Preserve or Consume

---

# Early Game Weapons

## Recurve Bow

Purpose:

- Primary early hunting weapon

Requirements:

- Bow
- Arrows

**Sourcing Arrows (confirmed 2026-09-25, Mike):** craftable from Sticks, Stones, Cordage, and Feathers — an ad-hoc recipe like Fire Building's Campfire, not part of a generalized crafting system (see Decisions_Log.md's 2026-09-25 crafting-deferral entry). Feathers are already a Medium Game byproduct (see Huntable Species below). Not yet built — in-game right now, Arrows only come from the F12 debug kit (see Item_Data.md's Materials table). Cordage sourcing is specced in Trapping_System.md.

Advantages:

- Silent
- Low resource cost
- Rewards close-range stalking skill

Disadvantages:

- Short effective range
- Greater risk of a wounding, non-lethal hit

---

## Bolt-Action Rifle

Purpose:

- Effective mid-to-long range hunting

Requirements:

- Rifle
- Ammunition

Advantages:

- Longer effective range
- More reliable takedown on large game

Disadvantages:

- Loud, may disperse nearby wildlife
- Ammunition is a limited, purchasable resource

**Sourcing Rifle Rounds (confirmed 2026-09-25, Mike):** purchased for money from a supply store, once Economy_System.md's shops exist — not built yet, so in-game right now Rifle Rounds only come from the F12 debug kit (see Item_Data.md's Materials table). Later in the game, once a player has a forge and the other tools that implies, rounds become craftable at home — that crafting chain isn't specced yet and is Future System scope, tied to whatever forge/tool-crafting system eventually gets built.

---

# Future Weapons

Not Alpha 0.1

Examples:

- Crossbow
- Shotgun (Turkey, Waterfowl)
- Compound Bow
- Black Powder Muzzleloader

**Confirmed 2026-09-25 (Mike):** the Black Powder Muzzleloader isn't just an arbitrary future addition — it should be one of the very first firearms a player gets, ahead of the Bolt-Action Rifle in the weapon-progression order, not added after it. Exact stats and how it fits alongside the Recurve Bow and Bolt-Action Rifle aren't specced yet.

---

# Huntable Species

Full habitat, activity cycle, and seasonal behavior detail is defined in Wildlife System documentation. This section covers hunting-specific yield only.

## Small Game (Rabbit, Squirrel)

Yield:

- Small amount of meat
- Small Furs

Best Weapon:

- Recurve Bow

---

## Medium Game (Turkey, Waterfowl)

Yield:

- Moderate meat
- Feathers

Best Weapon:

- Recurve Bow, future Shotgun

---

## Large Game (White-Tailed Deer)

Yield:

- Large amount of meat
- Hide (year-round; higher chance during the Fall Rut)
- Antlers (year-round; higher chance during the Fall Rut)

Best Weapon:

- Bolt-Action Rifle, or Recurve Bow at close range

Large game provides the highest single-harvest food value in the game. Full Hide/Antler drop-chance detail and the Fall Rut Window are defined in 05_Documentation/Wildlife/Large_Game.md.

---

# Shot Placement & Outcomes

## Clean Kill

Ideal outcome.

Full harvest available.

---

## Wounding Hit

Animal is injured but escapes.

Wounded Animal Tracking is a Future System, defined in Wildlife System documentation (blood trail → tracking → recovery).

In Alpha 0.1, a wounding hit results in a lost or reduced harvest, reinforcing the value of a clean, patient shot.

---

## Miss

No harvest.

May alert nearby wildlife, reducing opportunity for a period of time.

---

# Field Dressing

Meat quality depends on how quickly the animal is field dressed after harvest.

Delayed field dressing:

- Reduces meat quality
- Accelerates spoilage

Prompt field dressing:

- Preserves meat quality
- Extends time before spoilage begins

Field dressing connects directly to the Spoilage System defined in Core Survival System documentation.

---

# Carcasses

**Confirmed 2026-09-25 (Mike):** a carcass left behind (not field dressed / not collected) attracts other predators over time. A carcass can also be collected and added to a future Compost system, producing fertilizer for crops. Neither behavior is built yet — Claude Code's Phase 2 report notes carcasses currently aren't persisted at all; wildlife just respawns. Both halves are Future System scope: predator-attraction needs wildlife AI that reacts to a carcass, and the compost/fertilizer loop needs a Farming/Crops system that doesn't exist yet. Recording the design here so it isn't lost, same pattern as this doc set's other Future System notes.

---

# Hunting Locations

Examples:

- Deer Trails
- Forest Edges
- Meadow Corridors
- Natural Ground Cover

Hunting locations are discovered through exploration, consistent with Wildlife System habitat data.

Future expansion may include constructed ground blinds and tree stands.

---

# Discovery Integration

Successful hunters learn:

- Animal trails
- Bedding and feeding areas
- Dawn and dusk activity windows

Journal updates automatically.

Examples:

Deer Trail Discovered

High Deer Activity — Dawn/Dusk

Map Updated.

---

# Season Effects

## Spring

Limited hunting.

Breeding activity is underway; game is less predictable and less available.

---

## Summer

Limited hunting.

Focus remains on fishing and land development.

---

## Fall

Primary hunting season.

Animals feed heavily in preparation for winter.

Best trail activity and highest success rates of the year.

Most of the player's meat reserves for winter should come from Fall hunting.

---

## Winter

Reduced hunting opportunity.

Lower animal activity and movement.

Trapping becomes the more reliable food source during this period.

---

# Weather Effects

Wind:

- Affects scent detection by game
- Hunting against the wind improves approach success

Rain:

- Reduces visibility
- Dampens sound, aiding stalking

Heavy Weather:

- Reduces animal movement
- Reduces hunting opportunity

Weather should influence hunting decisions, consistent with Weather System documentation.

---

# Preservation Integration

Harvested meat can be:

- Cooked
- Dried
- Smoked

Preservation becomes increasingly important approaching Fall's end and through Winter.

Untreated meat is subject to the Spoilage System defined in Core Survival System documentation.

---

# Economy Integration

Hunting supports:

Food Supply
↓
Hide and Antler Collection
↓
Surplus Income

Deer Hide and Antlers are Early Survival Revenue, as defined in Economy System documentation.

Meat is primarily for personal consumption and stored reserves before it is considered for sale.

---

# Future Expansion

Not Alpha 0.1

- Wounded Animal Tracking (blood trail, tracking, recovery)
- Constructed ground blinds and tree stands
- Scent control mechanics
- Additional weapons (Crossbow, Shotgun, Compound Bow, Muzzleloader)
- Predator hunting (see Wildlife System — Predator Species)

These features are deferred until the core survival loop is proven fun.

---

# Design Rules

1. Hunting is not guaranteed food.

2. Knowledge increases success.

3. Shot placement matters.

4. Prompt field dressing preserves meat quality.

5. Fall is the primary hunting season; other seasons offer limited opportunity.

6. Hunting complements fishing, trapping, and foraging rather than replacing them.
