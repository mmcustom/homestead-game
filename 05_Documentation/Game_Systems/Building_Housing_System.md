# Homestead
## Building and Housing System v1.0

Status: Design Draft

---

# Purpose

The Building and Housing System provides shelter, storage, infrastructure, and progression.

Housing is one of the most visible indicators of player progress.

A player's home should tell the story of the homestead.

---

# Core Philosophy

Players improve shelter over time.

Progression should feel earned.

Housing progression:

Tent
↓
Lean-To
↓
Small Cabin
↓
Large Cabin
↓
Cottage
↓
Farmhouse

Older structures remain useful.

Buildings are rarely replaced entirely.

---

# Survival Phase

## Sleeping Bag (no shelter)

**Added 2026-09-26 (Mike) — new bottom tier, below Tent.** Sleeping on the ground with just a Sleeping Bag and nothing over it — the lowest sleep-quality tier in the game, below even a Tent. This is what a player on the hardest Difficulty_System.md tier is sleeping on until they can craft or afford something better. No structure, no weather protection at all.

## Tent

Advantages:

- Fast deployment
- Portable
- Low material cost

Disadvantages:

- Poor weather protection
- Low comfort
- Limited storage

**Note added 2026-09-26 (Mike):** a Tent and a Lean-To are roughly the same sleep-quality tier — one step up from a bare Sleeping Bag — just sourced differently: a Tent is a purchasable Difficulty_System.md starting-kit/Trading Post item, a Lean-To is built from local materials without needing a store. Both are furnished with the Sleeping Bag inside them rather than replacing it.

---

## Lean-To

Advantages:

- Better weather protection
- Built from local materials

Disadvantages:

- Minimal insulation
- Poor winter protection

**Note added 2026-09-26 (Claude, design):** Wood_Gathering_System.md (Design Draft, requested 2026-09-26, not yet built) gives "local materials" a real, specific source — Branches chopped from trees with the Axe. Future use, tied to this system actually getting a Unity implementation.

---

# First Build: Sleep System (Sleeping Bag / Tent / Lean-To)

**Added 2026-09-26 (Mike + Claude, design) — this is the scoped first slice of Building/Housing to actually build.** Right now there is no Sleep mechanic anywhere in the game — Save_Data_Model.md already assumes an auto-save trigger "at Sleep," but nothing produces that trigger, and none of Building/Housing's tiers have any Unity implementation. Rather than attempt the whole six-tier progression at once, this is scoped to just the bottom two Comfort tiers (Sleeping Bag and Tent/Lean-To), which is enough to give players an actual place to sleep and see whether shelter tier should affect anything mechanically. Small Cabin and everything above it (including the stove/fireplace, kitchen, sink, and running water additions already sketched further down this doc) stay Future, unblocked by this but not part of it.

## What This Adds

**Placement, Tent and Lean-To.** Both use the same Inventory → Build pattern already proven for the Campfire, Fish Trap, traps, and this week's Wood Pile/Rock Pile/Water Barrel/Food Cache/Storage Bin — placed in the world, not carried. The Tent is not craftable (it's a Difficulty_System.md starting-kit/Trading Post item, same non-craftable status as the metal cooking pot); the Lean-To is craftable from local materials — Claude Code's first-pass proposal for the recipe, likely Branches (the "local materials" source already established above) plus Sticks and/or Cordage, matching the shape of every other primitive recipe this week.

**No placement for the Sleeping Bag.** Per Mike's framing, sleeping in just a Sleeping Bag with no shelter should work anywhere, no Build step needed — an interaction available directly from carrying it (exact trigger, e.g. an E-hold prompt when not near a placed Tent/Lean-To, is Claude Code's call).

**The Sleep interaction itself.** Interacting with a placed Tent or Lean-To (or using the bare Sleeping Bag) triggers Sleep: advances time (to morning, or a player-chosen duration — Claude Code's call), fires the auto-save Save_Data_Model.md already expects at Sleep, and applies whatever recovery/protection effect the shelter tier grants (see below). This is the first thing in the whole game that actually uses "Sleep" as a real action rather than a documented placeholder.

**Shelter tier's mechanical effect — proposed hook into systems that already exist, not a new stat.** Rather than invent a separate Sleep Quality number, propose tying tier into Health_System.md's existing Warmth and Recovery mechanics: a bare Sleeping Bag offers little or no protection from overnight Warmth drain (full exposure to whatever the night's temperature and precipitation are), while a Tent or Lean-To reduces or blocks that overnight Warmth loss (roughly analogous to campfire proximity's existing warming effect, but passive and tied to being inside the structure rather than near a fire). Whether there's also a flat Health/Stamina recovery bonus on waking, and the exact numbers for either effect, are Claude Code's first-pass proposal — Mike confirms by feel in Play Mode, same pattern as every other system this week.

**Comfort ratings note:** this doc's existing Ratings list already has Sleeping Bag = Very Poor, Tent = Poor, Lean-To = Low. Mike's more recent framing (Tent and Lean-To are the same tier, just sourced differently) sits slightly at odds with Tent and Lean-To having different named ratings — that's a labeling detail, not a design conflict, and can be resolved by feel once this is actually built rather than guessed at here.

**Built, tested, committed and pushed 2026-09-26 (Claude Code)** as part of commit `4145888` (`8b3ea6e` carries the doc updates). Sleeping Bag: not craftable, new starting-kit item; click it in the Inventory to sleep right where the player stands, no placement needed, and it won't work while standing in water. Tent: not craftable, also starting-kit (same status as the metal cooking pot); placed from Build using the carried Tent, E sleeps in it, R packs it back up. Lean-To: craftable from 8 Branches, 4 Sticks, 1 Cordage; E sleeps in it, R takes it down and returns 4 Branches (Claude Code's proposed recipe, not locked). At night (from 19:00) sleep runs until 06:00; sleeping during the day is a 3-hour rest instead. During sleep the screen fades out while Hydration, Hunger, Warmth, Health, campfire fuel, weather, and windthrow all keep running in the background; waking refills Stamina and fires the auto-save — the "save at Sleep" trigger this doc and Save_Data_Model.md have been expecting all along. An early-waking safeguard fires rather than letting sleep cause real harm: Warmth below 20, Hydration or Hunger at 0, or Health below 15 wakes the player early (e.g. "You wake shivering — too cold to sleep"). Shelter tier's effect, confirmed working as proposed above (existing Warmth/Health-recovery mechanics, not a new stat): a bare Sleeping Bag cuts overnight Warmth loss by 25% (0%/0% rain/wind block, ×1.5 Health recovery while asleep); a Lean-To cuts it by 50% (80%/60% block, ×2 recovery); a Tent cuts it by 60% (100%/90% block, ×2 recovery); a Sleeping Bag used inside a shelter adds its own 25% on top, and a nearby campfire still warms as usual. Sample numbers at 32°F over 9 hours, Warmth starting at 90 (Health starting at 60): nothing = 31 Warmth/75 Health; Sleeping Bag alone = 46/95; Lean-To = 62/100; Lean-To + bag = 69/100; Tent = 70/100; Tent + bag = 75/100. **Flagged by Claude Code for Mike to judge by feel:** a full night in a shelter healing 60 Health to 100 may be generous — called out as an easy number to lower. **Confirmed 2026-09-26 (Mike):** a full night's sleep healing that much is good as-is — no change needed. Tested in Play Mode on a copy of Mike's save: built a Tent and a Lean-To, slept in the Tent from 21:00 to 06:02 with the auto-save firing, slept on the bag alone and triggered an early cold wake after 1.3 hours, packed both back down, and confirmed a save round trip (cut grass and the placed Tent both persisted). Two visual fixes along the way: the Tent looked like a box from behind, now has a proper triangular gable; the Inventory's right panel was resized (smaller type for long costs) to fit the new third rows of Build/Craft buttons. Mike's own save has no Sleeping Bag or Tent yet — F11 grants both; new games start with both already.

---

# Early Homestead

## Small Cabin

Purpose:

First permanent residence.

Requirements:

- Logs
- Lumber
- Stone
- Labor

**Note added 2026-09-26 (Claude, design):** Wood_Gathering_System.md (Design Draft, requested 2026-09-26, not yet built) gives "Logs" a real source — chopped from trees with the Axe. Future use, tied to this system actually getting a Unity implementation. Stone_Gathering_System.md (Design Draft, requested 2026-09-26) does the same for "Stone" above — hand-picked from the property's scattered rocks, or mined from the new cliff-side rock deposit with the Stone Pick Axe.

**"Lumber" answered 2026-09-26 (Mike), new Future system surfaced.** Asked how a Board (the Primitive Shovel's newly requested material — see Wood_Gathering_System.md) relates to this Requirement's existing "Lumber," Mike confirmed they're the same thing: "a board is a type of lumber. lumber comes from a lumber mill and can be purchased at the trading post until the player is able to purchase or build one of their own. boards are used for building most buildings. boards will inherit the quality of the lumber it comes from from pine to birch to oak and walnut in levels of quality." This resolves the near-duplicate-material question (Board = the existing `lumber` item, no new material needed) but introduces two things nothing in this project has designed yet: (1) a **Lumber Mill** — a new production building the player can eventually purchase or build, milling Logs into Lumber/Boards, alongside the Trading Post selling Lumber in the meantime; (2) **wood species/quality tiers** on trees themselves (Pine, Birch, Oak, Walnut, ascending in quality per Mike's own ordering) — Wood_Gathering_System.md's tree chopping currently treats every tree identically, with no species tagged on any Log. Both are Future, not designed in detail yet, and out of scope for Small Cabin's own first implementation — noted here so the dependency isn't lost. See Wood_Gathering_System.md's "Tools: Primitive Shovel" section for the resulting open question about whether the shipped Shovel recipe should actually change to Board given this unbuilt chain.

**Site Preparation, added 2026-09-26 (Mike) — Future, tied to this system's first implementation.** Before a structure goes up, the ground under it should be cleared and leveled first. Wood_Gathering_System.md's new Primitive Shovel is the tool for this (it also removes tree stumps, which is buildable now since felled trees already exist) — the ground-clearing/leveling half specifically waits on Building/Housing getting a real Unity implementation, since there's no "place a building" flow yet for a prepared site to matter to. Exact mechanic (a manual clear-and-level interaction vs. an automatic check that a spot is already clear/flat enough) is Claude Code's call whenever this system is actually built.

Benefits:

- Improved rest
- Increased storage
- Better weather protection

**Furnishing added 2026-09-26 (Mike):** a tiny primitive cabin at this tier comes with a primitive stove/fireplace — one step up from the Lean-To's no-fire-indoors reality, giving the player their first indoor heat source (feeds Health_System.md's Warmth recovery once that system's campfire-proximity logic is adapted for an indoor source) and a place to cook without stepping outside. Not the full wood fireplace + stove combo the log home gets below — just one basic unit.

---

# Tools: Hammer

**Added 2026-09-26 (Mike) — new craftable Tool.** A Hammer, for primitive building and repairing — the tool this whole Building/Housing progression will need once it has any Unity implementation at all (Site Preparation, Small Cabin, and everything above it), and per Difficulty_System.md's Item Gaps section, one of the items Mike wants in the easiest difficulty tier's starting kit. Crafted from **Sticks, Cordage, and a Log** (Mike's own materials, all already-existing items — Logs come from Wood_Gathering_System.md's tree chopping) — a heavy wood-and-stone-adjacent build tool bound together the same "sticks and cordage" way as the other primitive tools in this doc set.

**Recipe:** proposing 2 Sticks, 1 Cordage, 1 Log — quantities are Claude Code's call once this is buildable, same propose-then-confirm pattern as the rest of this doc set. **Not yet buildable:** the Hammer's actual use — building and repairing — has nothing to attach to yet, since Building/Housing has zero Unity implementation at any tier (Site Preparation and everything in this doc stays Design Draft). Stays Future until Building itself gets its first real implementation, at which point the Hammer would likely gate the Build interaction for real structures the same way the Axe/Pick Axe/Shovel gate their own gathering actions.

**Confirmed 2026-09-26 (Mike): "i agree with claude code."** Left out of the Primitive Axe/Knife batch as Claude Code recommended, rather than added now as a craftable-but-inert placeholder — a two-minute add (recipe line and item) whenever Building/Housing gets its first real piece, bundled with that work instead of built ahead of it.

---

## Root Cellar

Purpose:

Food storage.

Benefits:

- Reduced spoilage
- Winter food security

---

## Smokehouse

Purpose:

Food preservation.

Benefits:

- Long-term meat storage
- Long-term fish storage

---

# Established Homestead

## Large Cabin

Purpose:

Expanded living space.

Benefits:

- More comfort
- More storage
- Better recovery bonuses

**Extended 2026-09-26 (Mike) — this is the "larger log home" tier confirmed as Large Cabin, not a replacement for Cottage/Farmhouse above it.** A real jump up from the Small Cabin's single basic stove/fireplace: a wood fireplace AND a separate stove, a primitive kitchen, a sink, and indoor running water — but the running water is conditional, not automatic. It only works "if water reserves are full and equipment is working properly," meaning it draws from an actual water source (Water_System.md / Primitive_Storage_System.md's Water Barrel, or whatever cistern/well upgrade exists by the time this is built) rather than being an unlimited tap — go without maintaining that reserve or let the equipment fail, and indoor water stops working. Exact mechanic (what "equipment" means here — pipes, a pump, a well — and how failure is triggered/repaired) is Claude Code's call whenever this tier gets built, same as everything else in this doc.

**Flagged 2026-09-26 (Mike) — deferred, not decided:** indoor plumbing raises the sewage/composting question (where does wastewater and waste actually go), and Mike's explicit: this needs to be determined later, not guessed here. No sewage/composting mechanic exists anywhere in the game yet.

---

## Cottage

Purpose:

Family-sized dwelling.

Benefits:

- High comfort
- Increased indoor storage
- Better quality of life

---

## Barn

Purpose:

Livestock shelter
Feed storage
Equipment storage

Benefits:

- Animal protection
- Winter support

---

# Modern Homestead

## Farmhouse

Purpose:

Primary residence.

Represents a mature homestead.

Benefits:

- Maximum comfort
- Maximum recovery
- Large storage capacity

---

## Workshop

Purpose:

Tool maintenance
Equipment repair
Future crafting systems

---

## Equipment Buildings

Purpose:

Storage of:

- Tractors
- Trailers
- Equipment

---

# Building Categories

## Housing

Examples:

- Tent
- Cabin
- Cottage
- Farmhouse

---

## Storage

Examples:

- Root Cellar
- Storage Shed
- Barn

**Note added 2026-09-26 (Mike, design), scope confirmed 2026-09-26 (Mike):** wants a primitive tier below these — an informal wood pile and rock pile the player can drop excess Logs/Firewood/Sticks/Branches/Stone onto to offload inventory weight before any real storage building exists. Originally scoped as Future/sequenced after Building itself, Mike has since confirmed he wants the primitive versions built now, with real storage buildings (Storage Shed etc.) layered in as an upgrade once Building/Housing gets its first Unity implementation (still Design-Draft-only, no code yet). Full spec — build method, what each pile holds, capacity, persistence — is in Wood_Gathering_System.md's new "Primitive Storage (Wood Pile / Rock Pile)" section, and on Current_Task_List.md as a Needs Claude Code entry. Wood Gathering already established the working precedent this reuses — a felled tree drops its yield into a wood pile that persists in the world and can be drawn from over multiple trips.

**Extended 2026-09-26 (Mike)** — three more primitive storage types requested, using this exact same pattern: Water Storage, Food Storage, and a general "other dry goods and resources" storage, plus a new Pouch/Bag carrying Tool to bring non-liquid resources to them. Full spec is in the new Primitive_Storage_System.md — Root Cellar (Food) and this doc's Storage Shed/Barn (dry goods/equipment) remain the eventual real upgrades once Building/Housing ships, same relationship the Wood Pile/Rock Pile already have with Storage Shed/Barn above.

---

## Animal Structures

Examples:

- Chicken Coop
- Goat Shelter
- Rabbit Hutch

---

## Utility Structures

Examples:

- Well House
- Windmill
- Solar Shed

---

# Comfort System

Housing affects:

- Sleep quality
- Recovery
- Morale

Ratings:

Sleeping Bag (no shelter)
= Very Poor

**Added 2026-09-26 (Mike):** new bottom rating, below Tent — see the Survival Phase section above.

Tent
= Poor

Lean-To
= Low

Small Cabin
= Good

Large Cabin
= Very Good

Farmhouse
= Excellent

---

# Expansion Philosophy

Buildings should grow naturally over time.

Example:

Small Cabin
↓
Addition
↓
Large Cabin
↓
Cottage

The homestead develops gradually.

---

# Property Storytelling

Past structures remain visible.

Examples:

- Original tent site
- First cabin
- First garden

Players should be able to look back and remember their journey.

---

# Design Rules

1. Housing progression should feel meaningful.

2. Better shelter improves quality of life.

3. Older structures remain useful.

4. Buildings tell the story of the homestead.

5. Structures should support self-sufficiency rather than wealth accumulation.