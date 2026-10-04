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

# Portable Lighting: Torch, Lantern, Flashlight

**Gap found 2026-10-03 (Mike):** "i also think we need to add another couple of tools. perhaps a primitive torch, and then a lantern or flashlight for nighttime." Lighting is already listed as one of Fire's functions above, but only the Campfire and a Small Cabin's hearth exist as fixed, stationary light sources — nothing portable the player can carry and use away from a fire goes anywhere in the game yet.

**Resolved 2026-10-03 (Mike):** add portable lighting Tools. Rather than pick one of "lantern or flashlight" as Mike's message left open, proposing both, staged by what the project can actually support right now:

- **Primitive Torch** — buildable now, same primitive-materials tier as every other early Tool.
- **Lantern** — buildable now, a step up in brightness/burn time, primitive-era appropriate (oil/fat-fueled, not electric) so it doesn't need to wait on anything else.
- **Flashlight** — **Future**, deferred until batteries exist as a real item. Rather than invent a standalone battery just for this one Tool, it's tied to the new Power_System.md (added in this same pass, covering electricity generation and storage for the whole homestead) — Flashlight becomes buildable/purchasable once that system has a battery item defined, the same way Small Cabin waited on Clay existing first.

**First-pass scope (Claude, not yet confirmed) — Claude Code should correct if this doesn't match:**

- **Torch:** crafted from Stick + Cordage (wrap) — exact recipe and whether a flammable soak (Tallow, pitch) is needed is Claude Code's call, same propose-then-confirm status as every other primitive recipe. Lit with Flint and Steel, same ignition as the Campfire. Burns down and is consumed over time like Firewood, rather than being refuelable — it's the cheap, disposable tier. Provides a light radius around the player while equipped; exact radius/intensity is a lighting/rendering call for Claude Code.
- **Lantern:** a refillable container, burns a liquid fuel rather than consuming itself — **new item gap:** no oil/fat/wax material exists anywhere in Item_Data.md yet. Claude Code's first-pass call on sourcing, same pattern as Clay's creek-bank dig site — Lamp Oil as a simple Trading Post-purchasable consumable is the most likely first pass (parallel to how Lumber was added to the Trading Post rather than requiring a full production chain on day one), with a renderable Tallow byproduct from Hunting/Livestock butchering as a possible future free source. Brighter and longer-lasting per unit of fuel than the Torch, and doesn't burn itself up — refuel it instead of rebuilding it.
- **Flashlight:** Future. Depends on Power_System.md defining a battery item first. Not sent to Claude Code as a build task yet.

**Built and tested 2026-10-03 (Claude Code), committed `fd5bfe4` (Torch, Lantern and Lamp Oil; confirmed 2026-10-04):** Torch crafts from 1 Sticks + 1 Cordage; equip it and left-click to light or snuff — lighting needs Flint and Steel in the pack (not consumed, same as the Campfire), and without it the player gets "You need Flint and Steel to light the Torch." Burns about 3 in-game hours (~4 real minutes); when it burns out the Torch is removed and auto-unequipped. A partial Torch keeps its remaining time when snuffed or put away, and relighting continues from there rather than resetting — it visibly dims over its last 24 in-game minutes as a low-fuel warning. Lantern crafts from 2 Clay + 1 Cordage (a clay oil lamp with a cord wick) — **Claude Code's own call, not Mike's ask:** burns a new Lamp Oil consumable rather than being disposable like the Torch, sold at the Trading Post for $3/bottle (8 hours of light each), with the Lantern's own tank holding 16 hours (2 bottles). A Refill button appears on the Lamp Oil row whenever a Lantern is carried — pours one bottle, or as many as fit with Shift — and the Lantern itself is never consumed, unlike the Torch. The Lantern lights with no Flint and Steel needed at all (treated as a lit wick rather than a fire to start, for lower friction) — **confirmed 2026-10-03 (Mike): "i'm fine with Lantern lighting without Flint and Steel."** No change needed. Lantern is brighter with a wider light pool than the Torch. Only the currently equipped light can be lit; putting it away or switching tools snuffs it. A HUD status line shows state and time left (e.g. "Torch — burning, 2.7 h left. Click to snuff it."), and the Inventory rows show hours remaining. Torch time left and Lantern fuel both save with the game; a lit light is always off after a reload. New items: `torch` (0.5 kg), `lantern` (1.0 kg), `lamp_oil` (0.5 kg/bottle). Trading Post buys back Torches for $1 and Lanterns for $4.

Tested in Play Mode: crafting both items, lighting with and without Flint and Steel, snuffing and relighting, a Torch burning out, refilling the Lantern and hitting its capacity cap, a save/restore round trip, and night screenshots of both lit. **Not tested: a real physical mouse click** — Unity ignores simulated mouse input when unfocused, so the same underlying code the click calls was exercised directly instead, same caveat as the Tool Hotkeys' untested real key presses. Two Inventory-screen layout bugs were caught and fixed along the way (not asked for, needed): with 12 recipes the last Craft row was overlapping the "Esc to close" hint, and the Lantern's oil readout was getting clipped — both fixed by shrinking the Build/Craft row heights slightly and shortening the hint text. The light's position needed a fix too: it first floated as an orb in front of the player rather than at hand height, moved down to the lower-right edge of view. Mike's slot1 save was backed up and restored at each step; all six files matched throughout. Session note: Claude Code found Unity already in Play Mode from a session it hadn't started partway through this work, asked before stopping it, and it had already ended by the time it did — no impact on the result.

**Flagged (Claude Code) — all items now resolved or accepted as-is:** no weather or sleep interaction — rain doesn't put the Torch out, and neither light burns down while the player sleeps. Neither has a held/world model yet, just the light itself. **Accepted 2026-10-04 (Mike): "weather and sleep is fine the way it is for now"** — deferred, not rejected; revisit if weather/sleep realism becomes a priority. Dropping a half-burnt Torch loses its remaining time — the next one picked up starts fresh. **Accepted 2026-10-04 (Mike): "the dropped torch is fine for now"** — same deferral. Two Lanterns would share one tank (clarified 2026-10-04 by Claude Code — the original sentence was clipped in screenshots: "Dropping a half-burnt torch loses its partial time, and the next torch you pick up starts fresh. Two Lanterns would share one tank."). Lantern fuel is saved once rather than per Lantern, so carrying two means they draw from and refill a single shared tank rather than each holding its own — **confirmed 2026-10-04 (Mike): "its fine if lanterns use a shared fuel source."** No change needed. Wording: the "1 more Sticks" grammar on the crafting readout is unchanged (known quirk, left as-is). Daytime: Claude Code only checked the lights at night — they're meant to be nearly invisible in daylight. **Resolved 2026-10-04 (Mike, playtested): "lighting from torches and lanterns in daylight is fine."** No change needed.

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