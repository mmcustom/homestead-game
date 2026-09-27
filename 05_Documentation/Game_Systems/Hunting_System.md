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

# Aiming

**Update 2026-09-25 (Mike, playtest):** neither weapon has any aiming aid right now — Mike found it hard to tell where a shot would actually land at a deer. Wants a scope or sights added for shooting/aiming. Not specced anywhere before this; both weapons currently fire from wherever the camera points with no on-screen reference. Exact treatment per weapon is Claude Code's call — a simple crosshair/reticle would suit the Recurve Bow's instinctive, close-range feel, while the Bolt-Action Rifle could reasonably get real aim-down-sights (iron sights or a scope with a slight zoom), consistent with the doc's existing "longer effective range" framing for the rifle versus the bow's "rewards close-range stalking skill." Whichever reticle/sight picture is shown should read clearly against Shot Placement & Outcomes below, since the whole point is helping the player judge heart/lung placement rather than guessing.

**Resolved 2026-09-25 (Claude Code) — committed and pushed, not yet playtested.** Implemented per the split suggested above: the Recurve Bow gets a spread reticle, the Bolt-Action Rifle gets real aim-down-sights (a scope), both behind a new dedicated Aim action (right mouse button / left trigger on gamepad). Shot-placement feedback uses an amber/green color cue on the reticle/sight picture to signal shot quality against the targeted spot, addressing the doc's own requirement above that it read clearly against Shot Placement & Outcomes. Bundled into the same commit as the fishing line-tension retune (`96ea709`), with the doc edits in a separate commit (`5f68600`); both are pushed to `master` on GitHub. Mike has only confirmed the fishing half in Play Mode so far — this half is still awaiting his own test.

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

**Tool requirement added 2026-09-26 (Mike) — real change to already-shipped behavior.** Field dressing should require a Knife equipped/carried — right now it doesn't gate on any tool at all (Claude Code's already-built-and-tested implementation lets the player field dress with nothing equipped; see Current_Task_List.md's Hunting entry). This is a genuine mechanic change to committed code, not just a doc update: without a Knife, field dressing should presumably be blocked entirely or forced into the "delayed" penalty path (Claude Code's call, propose a first pass) rather than working exactly as it does today. See the new Tools: Knife section below for the item itself.

**Built and tested 2026-09-26 (Claude Code), not yet committed.** Claude Code's call: no Knife **blocks** field dressing entirely rather than sending it down the delayed-penalty path — the carcass instead shows "Deer (fresh) — needs a Knife to field dress" and waits. The meat-quality clock keeps running while it waits, so wandering off without a Knife still costs freshness the same way the existing delay rules already do; the Knife only needs to be **in the pack**, not equipped, unlike the Axe/Pick Axe/Shovel which need to be equipped to use. Tested in Play Mode on a copy of Mike's save: a freshly killed squirrel refused field dressing and showed the new prompt; crafting a Knife and retrying gave Meat and Fur as normal. **Confirmed 2026-09-26 (Mike): "smooth it out."** The Knife joins the new-game starting kit rather than making players craft one before their first hunt — the one-line change Claude Code flagged. F11 should also grant a Knife to existing saves that predate this change, same pattern used for the Tent/Sleeping Bag/Pouch additions.

---

# Tools: Knife

**Added 2026-09-26 (Mike) — new craftable Tool.** A Knife, specifically called out for field dressing animals (see above) — likely also the natural tool for skinning/processing a carcass into Hide/Fur/Meat generally, though Mike only named field dressing explicitly. Trapping_System.md's own catch-checking (Successful Catch → Meat and Fur) would presumably use the same Knife, since it's the same field-dressing-adjacent action on trapped game — cross-referenced there too.

**Recipe:** not specified by Mike. Proposing **Sticks, Cordage, and Stone** (a small stone blade lashed to a stick handle) as a first-pass, matching the same "small stone tool head" construction as the Stone Pick Axe and the new Primitive Axe (Wood_Gathering_System.md) rather than the "log-sized" construction of the new Hammer (Building_Housing_System.md). **Not yet confirmed by Mike** — flag for correction if he had a different construction in mind.

**Built and tested 2026-09-26 (Claude Code), not yet committed.** Recipe shipped as 1 Stick, 1 Cordage, 1 Stone — a smaller blade than the Axe/Pick Axe head, crafted from the Inventory's Craft grid. Confirmed by Claude Code: Trapping_System.md's catch-checking does need the same Knife too, for consistency (see that doc's updated note) — a snare or box trap's catch now refuses to give up its Meat/Fur without one ("A Rabbit in the trap — you need a Knife to dress it," and the catch just stays in the trap), while Fishing_System.md's Fish Trap and regular fishing do **not** need a Knife, since fish come out whole rather than needing dressing. Tested in Play Mode on a copy of Mike's save: a trap refused to give up its rabbit without a Knife equipped; after crafting one, both the trap's catch and a field-dressed squirrel gave their Meat/Fur normally. All 9 Craft buttons (the new Axe and Knife alongside the existing seven) still fit in three rows in the Inventory UI. Save was restored and hash-checked afterward; the project settings file was untouched. **Committed 2026-09-26 as `67d85da` on master, not yet pushed** — bundled with the Primitive Axe (Wood_Gathering_System.md) as one 13-file code commit. This doc's own updates (Claude-primary) are still uncommitted, awaiting a separate docs commit alongside it (see Wood_Gathering_System.md's Primitive Axe section for the full status).

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
