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

## Wood as Direct Fuel (Sticks, Branches, Logs) - REQUESTED 2026-10-17 (Mike), BUILT and PLAY-TESTED 2026-10-17 (compiles clean, committed as 65c9cb7 and pushed to origin/master)

**BUILT 2026-10-17 (Claude Code, compiled 0 errors / 0 warnings), PLAY-TESTED by Mike 2026-10-17 ("everything plays good, the cap, shift+E, hearth, everything works normally"; one-line report, Console status not stated):** FireManager.cs has plain [SerializeField] tunables hoursPerLog 10, hoursPerBranch 1, hoursPerStick 0.15 (next to hoursPerFirewood 2), BurnHours(itemId), SelectedFuel and CycleFuel() (selection kept in a static, not saved; falls back to the first carried type in the default order if the picked fuel is no longer carried), fuel-aware CanAddFuel/AddFuel, and AddFuelUpTo for Shift+E. A piece goes in only if its whole burn time fits (0.01 h tolerance); otherwise "Not enough room for a Log (needs 10 h)" and nothing is consumed. Campfire.cs: prompts are fuel-aware ("Add Branch (+1 h)  · burning, 4.0 h of fuel"), "· F: change fuel" shows only when more than one kind is carried, a full burning fire reads "Burning (..., no room for a Log)", a burnt-out fire reads "Burnt out - add wood to rebuild it", E adds one, Shift+E adds as many as fit, CycleFuel() flashes "Fuel: Branch (+1 h each)". PlayerController.cs: three lines so **F** while looking at a Campfire cycles fuel (F is free: not in the input actions asset, not a hotbar number, not F2 to F12; the mouse wheel is bound in the asset so it was not used). Shift+E works only with E; R (adds to an out fire) still adds one piece. Cabin dismantle still refunds floor(fuelHours / 2) whole Firewood, so a hearth loaded with Logs refunds Firewood, not Logs (correct arithmetic, not the same items back). InventoryScreen, TradingPost, WoodPile, Axe, scenes, assets, save format, Bow Drill and Flint and Steel checks untouched. **Confirmed by Mike 2026-10-17 after trying it:** 0.15 h per Stick is fine (no longer just a first-pass guess) and F works as the cycle key. Checklist: 06_AI_Collaboration/Wood_Fuel_Playtest_Checklist.md. Note: PlayerController.cs is ALSO changed by the uncommitted save-slot work, so a wood-fuel-only commit needs those hunks separated.

**Requested by Mike 2026-10-17:** "sticks are small and burn quickly, but they still burn. same with branches, they are just bigger sticks, but they still burn. Logs are both large and can burn for long times, but they can still be used to fuel a fire." Today only Firewood fuels a fire (2 in-game hours each); Sticks, Branches and Logs must first be split into Firewood with the Axe (Wood_Gathering_System.md, Chopping Firewood). Wood_Gathering_System.md's own Core Loop already says "burn (Logs, Branches)", so this closes a gap between the doc's intent and the build.

**Decided 2026-10-17 (Mike):** unsplit wood burns for the SAME time as the Firewood it would split into (no penalty for skipping the Axe).

**First-pass numbers (Claude, not yet confirmed except where marked):**

| Item | Burn time added to a fire | Basis |
|---|---|---|
| Firewood | 2 h (unchanged) | existing |
| Log | 10 h | 1 Log splits into 5 Firewood (Mike's decision: same time) |
| Branch | 1 h | 2 Branches split into 1 Firewood (2 h) |
| Stick | 0.15 h (9 minutes) | Claude, not confirmed: roughly the same hours per kg as Firewood (a 0.1 kg Stick against 1.5 kg for 2 h is about 0.13 h), rounded up a little; Mike only said Sticks "burn quickly" |

- **Where it applies:** every fire that already accepts Firewood: the Campfire and the Small Cabin hearth (and any stove or fireplace that uses the same fuel). Claude Code checks how fuel is added today and mirrors it.
- **Fuel cap:** the existing cap (12 h in the Campfire and hearth, per the Winter playthrough numbers) stays. An item is added only if its whole burn time fits under the cap (with a 0.01 h tolerance, as the code does today); a Log needs 10 h of room. If it does not fit, the player gets a short message ("Not enough room for a Log (needs 10 h)") and nothing is consumed (no wasted wood).
- **Mike's choice of fuel (asked 2026-10-17: "can it give a choice of which 1 to use?"):** the player chooses which wood to burn; it is not picked for them. First-pass design (Claude, not yet confirmed): looking at a fire while carrying more than one kind of fuel, the prompt names the selected fuel and its burn time ("Add Branch (+1 h)"), and one key cycles through the fuel types carried (Firewood, Branches, Sticks, Logs, skipping any not carried). E adds one of the selected fuel; Shift+E adds as many of the selected fuel as fit. The default selection when you walk up is the first carried type in this order: Firewood, Branches, Sticks, Logs (Logs last, so a Log you were saving for building is never the default). The cycling key is Claude Code's call after checking input conflicts (mouse wheel or another free key); the selection is not saved. An Inventory "Add to fire" button is a possible later addition, not part of this build. If only one kind is carried there is nothing to choose and E just works as it does today.
- **Correction (Claude Code read the code, 2026-10-17):** a fire has no multi-add gesture today (E adds exactly one Firewood), so Shift+E is new, not "existing". A fire's fuel is stored only as hours (CampfireState.fuelHours); the Small Cabin hearth is an ordinary Campfire, so there is no separate stove or fireplace. The Bow Drill and Flint and Steel check the fire's fuelHours, not the pack, so they need no change.
- **Bow Drill and "fire needs fuel":** a Stick, Branch or Log counts as fuel. So a Pioneer can build a Campfire, load it with Sticks, and light it without splitting anything.
- **No heat difference in the first pass:** all fuel gives the same warmth; only burn time differs. Kindling versus long-burning fuel is not modelled.
- **Splitting stays useful, but less so:** splitting a Log still turns 8 kg into 7.5 kg of Firewood (about 0.5 kg lighter) and lets you add fuel in smaller steps; it is no longer required to burn it. Mike chose this knowingly ("same burn time").
- **Balance check (Claude):** Sticks are plentiful (about 1,200 bundles of 3), but one 10 h night would need about 67 Sticks (22 bundles), so Sticks do not trivialise Winter firewood pressure; Branches (10 per night) and Logs (1 per night) are the real shortcut and cost carrying weight (1 kg per hour, and 8 kg for 10 h).
- **Rules:** no new save fields (fuel is already stored as hours); put any new tunables on FireManager, not inside a nested serialized class; old saves unaffected.
- **Campfire build recipe: Sticks option (requested 2026-10-17 by Mike: "campfire recipe doesn't include sticks as an option"; Mike chose 6 Sticks).** The Campfire may now be built from EITHER 3 Firewood (unchanged) OR 6 Sticks. 6 is a convenience number, not a weight or burn-time match (matching 3 Firewood would take 40 to 45 Sticks). Firewood is used first if the player has 3, otherwise Sticks (first-pass rule, Claude, not confirmed; Claude Code may choose a different tie-break if the Cordage "Either ... or" pattern in the Build tab does it differently, and reports it). Branches and Logs are NOT added to the recipe in this pass (Mike asked only about Sticks). The Build tab entry shows both options with have/need in green/red, like Cordage's "Either ... or"; the red reason text when neither is affordable names both ("Needs 3 Firewood or 6 Sticks"). Dismantling or picking up a campfire refunds nothing new. The Small Cabin hearth cost is unchanged. **WRITTEN 2026-10-17 (Claude Code), compiled clean (0 errors, 0 warnings; FireManager.BuildPayment and CampfireSticksCost present), committed as 65c9cb7 with the wood-fuel work and pushed to origin/master, PLAY-TESTED by Mike 2026-10-17 (checklist items 29-34 all PASS; Branch 1 h, Log 10 h and the refund behaviour also confirmed):** FireManager.cs has a plain [SerializeField] campfireSticksCost = 6 (property CampfireSticksCost) and BuildPayment(): 3 Firewood if the player has them, otherwise 6 Sticks, otherwise none; CanBuild and Build both use it (the Build tab is the only caller). Neither affordable reads "Needs 3 Firewood or 6 Sticks." (full amounts, not Cordage's shortfall wording). CraftingScreen.cs: the Campfire entry has two alternative sets, so the pane shows "Either 3 Firewood / or 6 Sticks" with green/red have/need, and the list label reads "3 Firewood / 6 Sticks". Tie-break follows the Cordage pattern: first affordable set in listed order is spent, Firewood first. **Decided 2026-10-17 (Mike):** a new Campfire starts with the burn time of what was spent, so a Firewood-built fire starts at 6 h (as before) and a 6-Stick fire starts at 0.9 h, as built. No change needed. Unchanged: Small Cabin hearth, dismantle refund, scenes, assets, save format, F/E/Shift+E fuel work, Branches and Logs not in the recipe.

---

## Bow Drill (primitive fire starter)

**Gap found 2026-10-07 (Mike, playtest):** "There is still no way to light a campfire even after building a primitive axe to collect firewood from chopped down trees. We need to create a new primitive tool - "Bow Drill" for lighting a fire without flint and steel. It should require 2 sticks and 1 cordage to craft, but isn't always successful." The only ignition source in the docs was Flint and Steel, which the Pioneer kit does not include and which costs $8 at the Trading Post on a $0 start, so a Pioneer could gather every ingredient for a Campfire and still not light it.

**Requested by Mike:** new primitive Tool, Bow Drill. Crafted from 2 Sticks + 1 Cordage. Lights a fire without Flint and Steel. Not always successful.

**First-pass scope (Claude, not yet confirmed) - Claude Code should correct if this does not fit the code:**

- **Item:** `bow_drill`, 0.4 kg, a Tool in the Craft grid next to the Torch and the primitive tools. Reusable - it is never consumed by use in the first pass (no durability), because the point is that it replaces a reusable item, Flint and Steel, for a player who has none.
- **Use:** equip it, face a built Campfire that has fuel in it, and hold left-click to run a drilling attempt. Each attempt takes about 6 real seconds with a progress bar, costs stamina (about 15 points, enough to matter but not to exhaust a rested player), and takes real time only (the clock keeps running; nothing is skipped, changed 2026-10-19). On success the Campfire lights exactly as it would with Flint and Steel. On failure the player gets a message such as "The ember died. Try again." and nothing is lost except the stamina and time.
- **Chance of success:** about 45% per attempt in calm, dry weather. Modifiers by current weather: Wind about 35%, Light Rain or Snow about 20%, Heavy Rain or Thunderstorm about 10%. A Campfire under cover (a built shelter or roof) ignores the rain and snow penalty. Cold Front and Cloudy use the calm figure. These are tuning numbers, not rules, and should be Inspector fields.
- **Frustration guard:** each failed attempt at the same Campfire adds about 10 percentage points to the next attempt's chance (a growing ember), capped at +30, and the bonus resets on success or after about an in-game hour away. This keeps "not always successful" from turning into ten minutes of clicking a coin flip, while still failing often enough that Flint and Steel is clearly the better tool.
- **Flint and Steel stays the reliable tier:** it never fails, takes no stamina and no time. The Bow Drill is the free, crude, unreliable alternative, in the same way the Torch is the cheap disposable light and the Lantern is the better one.
- **What it lights:** Campfire only in the first pass. The Torch still needs Flint and Steel by default; **optional, Claude Code's call:** let a Torch be lit from a burning Campfire by standing next to the fire and clicking, so a Bow Drill player is not locked out of portable light. The Lantern already lights with nothing.
- **Pioneer loop this closes:** Sticks from the ground (see Wood_Gathering_System.md) + the kit's Cordage -> Bow Drill; Sticks and Firewood -> Campfire; Bow Drill -> fire. No Axe and no purchase is required.
- **Messages:** needs-fuel ("The fire needs fuel before you can start it."), attempt in progress, success, and failure text, plus the HUD status line and tooltip. When the Campfire is already lit, the Bow Drill does nothing.
- **Trading Post:** buys back a Bow Drill for $1, like the Torch. It is not stocked for sale, since it is meant to be crafted.

**Open questions for Mike (none are blockers):** does about 45% with the growing-ember bonus feel right, or should failure be rarer or tougher? Should the Bow Drill eventually get a durability or a skill-based success bonus?

---
**Built 2026-10-07 (Claude Code), compile-checked only, NOT Play Mode tested, NOT committed:** the first-pass numbers above went in unchanged as Inspector fields on FireManager (drill 6 s, 15 stamina, no clock advance since 2026-10-19, reach 3 m; chance 45% calm/cloudy/cold front, 35% wind, 20% light rain/snow, 10% heavy rain/thunderstorm; +10 points per failure, cap +30, forgotten after 1 in-game hour). Recipe `bow_drill` = 2 Sticks + 1 Cordage added in Crafting.cs. Details and deviations: the roof test uses the existing OverheadCover.Roofed check, so tree canopy alone does not count as cover; the ember bonus is not saved (resets on reload); after a finished attempt the player must release and click again, so a held button does not chain attempts; **the optional Torch ignition was built:** with a Torch equipped, stand by a lit Campfire and click to light it, and away from the fire it refuses. The Craft grid now has 13 recipes and a fifth row, so the craft row height was shrunk from 36 to 32 to keep clear of the "Esc to close" hint - not checked visually. Mike's Play Mode test list: craft the Bow Drill from ground Sticks + Cordage on a Pioneer new game with no Flint and Steel; build a Campfire, equip the Bow Drill, face the fire and hold click (bar fills over about 6 s, real time only, no clock jump since 2026-10-19); failure shows "The ember died" and the chance climbs after each failure; success lights the fire; no fuel gives the message and an already-lit fire does nothing; in rain the chance shows 20% or 10% (45% under a roof); Torch lights from a lit Campfire and refuses away from it; Flint and Steel still lights instantly; save and reload with a lit fire.
**Confirmed 2026-10-09 (Mike, Play Mode playtest): "The bow drill works great!"** The Bow Drill stands as built. **Also confirmed in the same playtest:** the Torch lights only with Flint and Steel or from an already-lit Campfire, as built.
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