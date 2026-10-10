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

**Note added 2026-10-03 (Mike):** this is the default outcome, not an enforced one — the player can choose to dismantle any built structure, including permanent housing, with the Hammer (see Tools: Hammer below). A structure survives because the player kept it, not because the game won't let them remove it.

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

- Logs (20)
- Branches (10)
- Tall Grass (15, roofing)
- Stone (12, cabin foundation and the stove's hearth)
- Clay (6, new material — see note below)
- Labor

**Built and tested 2026-09-27 (Claude Code), revised the same day on Mike's call.** Quantities above are final. A first pass carried the whole cost in one trip, which forced them down to fit the 45 kg carry limit (2/4/15/2/1) — Mike's correction: that made a first permanent residence cost less than a Lean-To, and gathering for it should take real time and multiple trips instead. Rebuilt as a staged deposit (place an empty site, haul materials into it over as many trips as it takes, complete it once fully stocked with the Hammer equipped — the Wood Pile/Rock Pile pattern, not a new mechanic), which put the original, more substantial numbers back on the table since nothing needs to fit in one carry any more. See the "First Build: Small Cabin" section further down for the full mechanic and everything else that shipped with it (the Hammer gate, the hearth, sleeping in it, Clay's creek-bank source).

**Resolved 2026-09-26 (Mike): "the small cabin shouldn't need lumber, it can be built with logs and branches and long grass for a roof."** This replaces the original Logs/Lumber/Stone/Labor list above — Small Cabin no longer needs Lumber at all, unblocking its first implementation from the whole Lumber Mill/Trading Post question below. Branches and Tall Grass are both already real items (Wood_Gathering_System.md's tree chopping for Branches; Item_Data.md's Foraged Plants for Tall Grass, added for Sleep System's Cordage sourcing) — no new materials needed.

**Resolved 2026-09-26 (Mike): "yes, include stone and extra stone and clay for a stove in a cabin."** Stone stays in the general Requirements (the cabin itself), and the stove/fireplace furnishing below needs its own extra allocation of Stone plus a new material, **Clay**, for the hearth — matching real log-cabin construction (a stone-and-clay hearth under an indoor fire, not wood directly under flame). **New item gap:** Clay doesn't exist anywhere in Item_Data.md yet — no recipe, no gathering source. Left as Claude Code's first-pass call where it comes from (a creek/pond bank dig site is the obvious real-world answer, similar to how bog iron is sourced in Mining_Metalworking_System.md), same propose-then-confirm pattern as every other new material this week.

**Note added 2026-09-26 (Claude, design):** Wood_Gathering_System.md (Design Draft, requested 2026-09-26, not yet built) gives "Logs" and "Branches" a real source — chopped from trees with the Axe. Future use, tied to this system actually getting a Unity implementation. Stone_Gathering_System.md (Design Draft, requested 2026-09-26) does the same for "Stone" if it stays part of this Requirement — hand-picked from the property's scattered rocks, or mined from the new cliff-side rock deposit with the Stone Pick Axe.

**Superseded 2026-09-26 (Mike) — Small Cabin no longer needs Lumber, see Resolved note above.** The paragraph below is kept for its still-relevant Lumber Mill/wood-species design, which other future buildings ("boards are used for building most buildings," per Mike's own words) will still need — it just no longer blocks Small Cabin's own first implementation. Original: Asked how a Board (the Primitive Shovel's newly requested material — see Wood_Gathering_System.md) relates to this Requirement's existing "Lumber," Mike confirmed they're the same thing: "a board is a type of lumber. lumber comes from a lumber mill and can be purchased at the trading post until the player is able to purchase or build one of their own. boards are used for building most buildings. boards will inherit the quality of the lumber it comes from from pine to birch to oak and walnut in levels of quality." This resolves the near-duplicate-material question (Board = the existing `lumber` item, no new material needed) but introduces two things nothing in this project has designed yet: (1) a **Lumber Mill** — a new production building the player can eventually purchase or build, milling Logs into Lumber/Boards, alongside the Trading Post selling Lumber in the meantime; (2) **wood species/quality tiers** on trees themselves (Pine, Birch, Oak, Walnut, ascending in quality per Mike's own ordering) — Wood_Gathering_System.md's tree chopping currently treats every tree identically, with no species tagged on any Log. Both are Future, not designed in detail yet. See Wood_Gathering_System.md's "Tools: Primitive Shovel" section for the resulting open question about whether the shipped Shovel recipe should actually change to Board given this unbuilt chain.

**Confirmed 2026-09-26 (Mike): "yes i agree we should add lumber to the trading post."** Even though Small Cabin itself no longer needs it, Lumber stays a real gap for later buildings — adding it to the Trading Post's existing stock (rather than waiting on the full Lumber Mill) is confirmed as the near-term fix. Not yet sent to Claude Code as its own task; can go whenever, since nothing currently blocks on it.

**Site Preparation, added 2026-09-26 (Mike) — Future, tied to this system's first implementation.** Before a structure goes up, the ground under it should be cleared and leveled first. Wood_Gathering_System.md's new Primitive Shovel is the tool for this (it also removes tree stumps, which is buildable now since felled trees already exist) — the ground-clearing/leveling half specifically waits on Building/Housing getting a real Unity implementation, since there's no "place a building" flow yet for a prepared site to matter to. Exact mechanic (a manual clear-and-level interaction vs. an automatic check that a spot is already clear/flat enough) is Claude Code's call whenever this system is actually built.

Benefits:

- Improved rest
- Increased storage
- Better weather protection

**Furnishing added 2026-09-26 (Mike):** a tiny primitive cabin at this tier comes with a primitive stove/fireplace — one step up from the Lean-To's no-fire-indoors reality, giving the player their first indoor heat source (feeds Health_System.md's Warmth recovery once that system's campfire-proximity logic is adapted for an indoor source) and a place to cook without stepping outside. Not the full wood fireplace + stove combo the log home gets below — just one basic unit.

---

# First Build: Small Cabin (2026-09-26, design — scoped first slice of this tier)

**This is the next system up for Claude Code, following the same "scope the first slice, don't try the whole tier progression at once" approach the Sleep System used.** Small Cabin is the first real permanent structure — a step up from the Tent/Lean-To's Survival Phase — and the thing everything else in this doc (the Hammer's actual function, Site Preparation, wood species tiers, the eventual Lumber Mill) has been waiting on.

**Resolved 2026-09-26 (Mike): Small Cabin doesn't need Lumber at all** — "it can be built with logs and branches and long grass for a roof." This removes the blocking question entirely. Lumber is still confirmed for addition to the Trading Post (Mike: "yes i agree"), but purely for other future buildings — nothing about Small Cabin's own first implementation waits on it anymore. **Stone and Clay resolved too:** Stone stays for the cabin itself, plus extra Stone and a new material, Clay, for the stove's hearth — see the Requirements section above for the full resolution and Clay's new-item-gap note.

**Everything else, proposed as a first-pass design for Claude Code to build against (same propose-then-confirm status as every other system this week):**

- **Requirements:** Logs, Branches, and Tall Grass (all already real items), Stone (already real), and Clay (new — Claude Code's first-pass call on sourcing) — exact quantities left to Claude Code's own first pass, same as the Trading Post's catalog and every recipe so far.
- **The Hammer finally gets its function.** Placing/building the Small Cabin is proposed to require the Hammer equipped, the same "equip the tool that matches the job" pattern already used for the Axe/Pick Axe/Shovel — this is the payoff for the Hammer existing as an inert item since Difficulty_System.md's kit needed one.
- **Site Preparation:** per the existing note above, Claude Code's call whether this is a manual clear-and-level interaction (using the already-buildable Shovel) or an automatic check that the chosen spot is already clear/flat enough.
- **Furnishing:** the primitive stove/fireplace per Mike's note above — proposed to reuse Health_System.md's existing campfire-proximity Warmth logic (adapted for an indoor source) and the existing Cooking mechanic, rather than inventing new systems for either.
- **Placement pattern:** same Inventory → Build flow already proven for the Campfire, Fish Trap, traps, Wood Pile/Rock Pile/Water Barrel, and the Tent/Lean-To — nothing new to invent here.

**Built and tested in Play Mode 2026-09-27 (Claude Code), then revised the same day on Mike's call.** First pass built Small Cabin as one instant `BuildPile` like everything else, carried in one trip. **Mike's correction:** that made a first permanent residence cost less to build than a Lean-To, since fitting inside the 45 kg carry limit forced the quantities down to 2 Logs/4 Branches/15 Tall Grass/2 Stone/1 Clay. His call: gathering for a cabin should take real time and multiple trips, not one 45 kg haul, and the 45 kg limit shouldn't be constraining the design at all.

**Rebuilt as a two-phase, staged-deposit build — the Wood Pile/Rock Pile pattern, not a new mechanic.** Phase one places an empty `PileKind.CabinSite` from the Inventory screen — free, no Hammer needed, but the same bigger clear footprint as the finished building (3.5 m build distance, 3.2 m clearance against other piles, standing trees, and un-dug stumps — Site Preparation's automatic-check option). Phase two is exactly the Wood Pile's own storing/taking (R and E), just capped per material at what Small Cabin still needs (`WoodPileState.Accepts`, `WoodManager.CabinRequired`) instead of taking anything without limit — visibly growing as logs, branches, grass, stone and clay pile up in their own corners of the site (`WoodPile.BuildCabinSite`), and freely reversible: take any of it back out at any point, nothing locked in early. Mike's three specifics all confirmed working live: the pile is visibly growing (not an invisible counter), storing more of a material than's still needed is refused rather than wasted (tested depositing 40 Tall Grass against a 15 need — exactly 15 went in, the other 25 stayed with the player), and the Inventory screen's Small Cabin button does double duty — "Build" while short of materials, switching to "Complete Small Cabin" only once every material's at its required amount **and** the Hammer's equipped (`WoodManager.CanCompleteCabin`) — the Hammer gates completion, not placement, exactly as asked. `CompleteCabin` then consumes the deposited materials and swaps the site in place for the real Shelter/hearth, tested end to end: hauled the full 20/10/15/12/6 across several trips (weight forced the Logs alone into four separate hauls), confirmed the button stayed "Build a site" until fully stocked and without the Hammer, then completed it and got the same working cabin as the first pass — sleepable, hearth lightable, Warmth and Cooking picking it up by proximity.

**Requirements, final: 20 Logs, 10 Branches, 15 Tall Grass, 12 Stone, 6 Clay** — the original, more substantial first pass, back on the table now that nothing needs to fit in a single carry. Labor isn't tracked as its own resource, matching the Lean-To's shipped recipe (materials only) rather than inventing a build timer nothing else in the game has yet — real time is spent making the trips instead.

**Also caught and fixed along the way:** WoodManager lives directly in Bootstrap.unity (not a prefab), so its `[SerializeField]` defaults only apply the moment a field is first serialized — an earlier `force` asset refresh had baked the smaller, since-reverted quantities into the scene, so editing the C# defaults back to 20/10/15/12/6 alone did nothing until the scene's saved values were corrected too. And the very first pass never actually added `CabinSite`/`Cabin` to `WoodManager.Buildable`, so no Inventory button ever existed for it — only found because this revision needed a real button to test against; a Play Mode check that only calls `BuildPile` directly (as the first pass's testing did) doesn't catch a missing UI hookup.

**Site Preparation, resolved as the automatic check** (the option this doc's earlier note left open): `CanBuildPile` rejects a spot with a standing tree or an un-dug stump inside the cabin's footprint ("Clear the trees off the building site first" / "Dig out the stumps on the building site first (Primitive Shovel)"), rather than adding a separate manual clear-and-level interaction. This still ties back to the Primitive Shovel — stumps have to be dug out with it first, same as always — without inventing a new mechanic.

**Furnishing, confirmed working exactly as proposed — zero new Warmth or Cooking code.** Completing a cabin also calls a new `FireManager.BuildFurnished`, which drops an ordinary, unlit, unfuelled `CampfireState` (and its normal Campfire view — the same stone-ring prefab as any other fire) just outside the cabin's front wall. Because Warmth-by-proximity (SurvivalManager) and Cooking both already work off `FireManager.NearestLit` by distance alone, a lit hearth warms and lets the player cook there with no changes to either system — verified live: lighting it with Firewood and Flint and Steel raised the player's Warmth standing next to it, and `Cooking.FireInReach` picked it up immediately. It sits beside the cabin rather than literally inside it — the cabin's collider is one solid shell (matching the Tent/Lean-To's own simplification, not a walk-in interior), so an interior fire's collider would be unreachable by the player's aim raycast from outside.

**Sleeping in it — the best shelter tier yet.** Extended `Shelter`/`SleepManager`'s existing Tent/Lean-To tier system rather than building a separate mechanic: cuts overnight Warmth loss by 75%, blocks all rain and wind (a real enclosed shell, matching the Tent), and gives ×2.5 Health recovery while asleep — one step above the Tent's 60%/100%/90%/×2. A nearby lit hearth stacks on top the same way a campfire already does for any other shelter. No pack-up/take-down prompt shows for it (`SecondaryPrompt` returns empty), since a permanent structure has nothing to pack away.

**Clay, resolved: a creek/pond bank dig site, dug with the Primitive Shovel.** Rather than a placed, exhaustible deposit (like the one South Ridge rock outcrop), the Shovel now also digs Clay directly from dry ground close to the water's edge — checked with `Animal.IsOverWater` in a ring around the aim point (in the water = no; within ~3 m of it on dry ground = yes; further = no), reusing the exact same water-detection primitive the game already had rather than authoring new terrain data. Verified live at an actual water edge: over the water itself `IsCreekBank` was false, 1–3 m out it was true, past 4 m it was false again. Since banks run the whole length of the property's water, unlike the rock outcrop this never runs out. Weight proposed at 3.5 kg (a hand-sized wet lump, lighter than Stone's 5 kg fieldstone).

**The Hammer's recipe, added in the same pass** (Building/Housing's first real implementation, exactly the moment this doc's own Hammer section said it would be bundled in): 2 Sticks, 1 Cordage, 1 Log, matching the quantities already proposed there.

**Feedback 2026-09-27 (Mike): "it looks like the build was successful once claude code reduced the quantities of resources necessary, but i think it should be higher quantities that the player needs to stage at the build site prior to building with the hammer. It should take time gathering the materials and not carry all the necessary materials at one time."** This isn't a request for smaller numbers — it's a rejection of the carry-it-all-in-one-trip framing that forced the quantities down to 35 kg in the first place. The fix is a mechanic change: Small Cabin should work like the Wood Pile/Rock Pile pattern already shipped (Wood_Gathering_System.md's "Primitive Storage" section) — a persistent build site the player deposits materials into across multiple trips, the same way a felled tree's yield piles up and gets drawn from over time, rather than a single `BuildPile` call that needs the whole cost carried at once. The Hammer then completes construction once the site has accumulated the full requirement, instead of gating the initial placement. This removes the 45 kg carry limit as a ceiling on quantities entirely, so the original, more substantial first-pass numbers (20 Logs / 10 Branches / 15 Tall Grass / 12 Stone / 6 Clay, or something in that range) are back on the table — a first permanent residence should cost meaningfully more than a Lean-To's 8 Branches/4 Sticks/1 Cordage. Exact quantities under the new model are Claude Code's first-pass call again, same propose-then-confirm pattern as before.

**Mechanic confirmed 2026-09-27 (Mike), answering two follow-up questions:** the staged materials must be visible at the build site as they accumulate (not an invisible counter), and the player can freely interact with the pile — both add to it and take materials back out — at any point before building; nothing is locked in early. The Hammer-equipped Build action only becomes available once the exact required quantities for the selected building are present at the site; using it then completes construction. This is the same take/add freedom the Wood Pile/Rock Pile already allow, just with a completion gate (the Hammer, at full quantity) that those piles don't have. Ready to send to Claude Code.

---

# Tools: Hammer

**Added 2026-09-26 (Mike) — new craftable Tool.** A Hammer, for primitive building and repairing — the tool this whole Building/Housing progression will need once it has any Unity implementation at all (Site Preparation, Small Cabin, and everything above it), and per Difficulty_System.md's Item Gaps section, one of the items Mike wants in the easiest difficulty tier's starting kit. Crafted from **Sticks, Cordage, and a Log** (Mike's own materials, all already-existing items — Logs come from Wood_Gathering_System.md's tree chopping) — a heavy wood-and-stone-adjacent build tool bound together the same "sticks and cordage" way as the other primitive tools in this doc set.

**Recipe:** proposing 2 Sticks, 1 Cordage, 1 Log — quantities are Claude Code's call once this is buildable, same propose-then-confirm pattern as the rest of this doc set. **Not yet buildable:** the Hammer's actual use — building and repairing — has nothing to attach to yet, since Building/Housing has zero Unity implementation at any tier (Site Preparation and everything in this doc stays Design Draft). Stays Future until Building itself gets its first real implementation, at which point the Hammer would likely gate the Build interaction for real structures the same way the Axe/Pick Axe/Shovel gate their own gathering actions.

**Confirmed 2026-09-26 (Mike): "i agree with claude code."** Left out of the Primitive Axe/Knife batch as Claude Code recommended, rather than added now as a craftable-but-inert placeholder — a two-minute add (recipe line and item) whenever Building/Housing gets its first real piece, bundled with that work instead of built ahead of it.

**Built and tested 2026-09-27 (Claude Code):** buildable now, exactly as this section proposed — 2 Sticks, 1 Cordage, 1 Log. Gates completing a Small Cabin, not placing its building site (Mike's revision, same day) — the same "equip the tool that matches the job" pattern already used for the Axe/Pick Axe/Shovel, just applied to the finishing step of a multi-trip build rather than the first one.

**Gap found 2026-10-03 (Mike): "can we make the hammer remove or dismantle, built things too?"** The Hammer only builds/completes right now. Nothing lets the player remove a structure once it's placed, apart from the Lean-To's own pack-up (R, returns 4 of its 8 Branches) and the Tent's own pack-up (not consumed, just carried again). Small Cabin explicitly has no take-down at all ("a permanent structure has nothing to pack away," see above), and none of the primitive storage piles (Wood Pile, Rock Pile, Water Barrel, Food Cache, Storage Bin, Tool Rack — Primitive_Storage_System.md) or an in-progress build site can be removed once placed either.

**Resolved 2026-10-03 (Mike), asked directly: dismantle should cover everything, including permanent housing** — not just the primitive/undoable tier. This extends to Small Cabin and every future permanent tier above it (Large Cabin, Cottage, Farmhouse). **This is a real, confirmed reversal of this doc's own "buildings are rarely replaced entirely" / "older structures remain useful" philosophy for any structure the player actively chooses to dismantle** — see the notes added to Core Philosophy and Property Storytelling below; it's the player's call, not an automatic loss.

**First-pass scope (Claude, not yet confirmed) — Claude Code should correct if this doesn't match:**

- The Hammer equipped gates a new Dismantle/Remove interaction on any built structure: every primitive storage pile and the Tool Rack, any in-progress build site (`CabinSite`), and completed permanent housing (Small Cabin now, future tiers once built).
- Material return: proposed partial refund, matching the Lean-To's own existing take-down precedent (returns half its Branches cost) rather than a full refund or none — dismantling shouldn't be a free way to stockpile materials, but shouldn't feel punishing either. Exact fraction, and whether it's the same fraction for every structure type or varies, is Claude Code's first-pass call.
- A structure currently holding something — a Tool Rack with tools on it, a Wood Pile with logs, a lit Small Cabin hearth — should refuse dismantle, or require it be emptied first, rather than silently destroying its contents. Exact guard is Claude Code's call, same spirit as Storage Bin's own category checks.
- Dismantling Small Cabin specifically — the most invested structure in the game so far — is worth a confirmation step rather than a single accidental keypress; exact UI (hold-to-confirm, a yes/no prompt) is Claude Code's call, same as every other interaction-flow detail in this doc.

**Gap found 2026-10-04 (Mike), playtesting — a real, current instance of the build-site case above:** "and we also need to be able to remove a site if we accidentally place more than 1 site. i did exactly that and now i cant remove the second small cabin site." Placing a `CabinSite` is free and has no Hammer requirement (see First Build: Small Cabin), nothing stops a second one being placed, and nothing can remove it afterward — so a misclick leaves a permanent, unwanted site in the world. This is the same missing capability the Hammer Dismantle scope above already covers ("any in-progress build site"), but it's now blocking real play, so it shouldn't have to wait for the whole Dismantle feature if that build is still pending.

**First-pass scope (Claude, not yet confirmed) — Claude Code should correct if this doesn't match:**

- **Removing an in-progress site needs no Hammer.** Placing a site is free and Hammer-less, so undoing one should be too — a player who just misplaced a site shouldn't have to craft or equip a tool to fix it. The Hammer requirement stays for dismantling *completed* structures. (If the Hammer Dismantle build already gates sites behind the Hammer, relax that for `CabinSite` specifically.)
- **Anything already deposited comes back in full.** A site's materials were only stored, never consumed (construction consumes them at completion), so removing a stocked site returns everything inside to the player — or drops it as a persistent pile if the player can't carry it all (the Dropping Items behavior) — rather than applying the partial-refund rule meant for completed buildings. Nothing is lost by cleaning up a mistake. An empty site simply disappears.
- **A prompt on the site itself**, same interaction style as the Lean-To's pack-up, so it's discoverable without opening a menu. Exact key/prompt text is Claude Code's call.
- **Guard against the mistake, optional:** consider refusing to place a second `CabinSite` while one already exists, with a message like "You already have a cabin site." Real homesteaders can plausibly want two cabins eventually, so this is a judgment call rather than a rule — flagged to Claude Code and Mike rather than decided here. Removal is the confirmed ask; the placement guard is only a suggestion.
- **Same fix should cover any other stray placed structure** (Wood Pile, Rock Pile, Water Barrel, Food Cache, Storage Bin, Tool Rack) if the Hammer Dismantle build doesn't land soon — the underlying gap is identical.

**Built and tested 2026-10-04 (Claude Code), committed `e7cbe67`.** Answer to the open question: **Hammer Dismantle (resolved 2026-10-03) was never built** — there's no dismantle code anywhere and no commit for it. The only take-downs in the game are the Tent's pack-up and the Lean-To's take-down. So Mike's stuck site wasn't a bug in how Dismantle treats sites; Dismantle simply doesn't exist yet, and the fallback in the scope above applied (the same removal also covers stray storage piles — see Primitive_Storage_System.md, "Take Down Structure").

- **Removing a site (no Hammer):** an empty site — press E; the prompt reads "Remove Small Cabin Site (empty — nothing to take back)". A stocked site — press R while carrying nothing the site takes; the prompt reads "Remove Site (everything stocked comes back to you)". Everything stocked returns in full, and whatever the pack can't carry is left on the ground as pickup piles.
- **Key limits:** E and R were already taken (E takes materials, R deposits). If the player is carrying materials the site still takes, R deposits as before — the player can E to empty the site first, then E again to remove it. This is the trade-off of reusing the existing keys instead of adding a new one.
- **Placing a second site is now refused while one is in progress:** "You already have a Small Cabin site — finish it, or take it down first." A finished cabin doesn't count, so a second site can be started once the first is built. This was Claude Code's call (the optional guard from the scope above); it's one block in `WoodManager.CanBuildPile` if Mike wants it gone. It doesn't affect Mike's existing second site, which can still be removed.

**Tested in Play Mode by calling the interaction code directly, not with real key presses:** the second site was refused 12 m from the first, and the guard lifted once the first was removed. A stocked site with a full pack (4 Logs, 6 Branches, 5 Tall Grass, 8 Stone, 3 Clay) — only the Branches fit in the pack; the ground piles totaled Logs 4, Tall Grass 5, Stone 8, Clay 3, so nothing was lost, and the piles save with the world. An empty site was removed and the take-down logged. **Not tested:** where the overflow piles land on slopes, and the stray-pile button with a real mouse. Mike's slot1 save was backed up and restored, all six files matching.

**Hammer Dismantle — built and tested 2026-10-04 (Claude Code), committed `fc37969`.** The 2026-10-03 spec is now real; the stopgap behavior above is partly superseded (noted per item). Tested in Play Mode by calling the interaction code directly, not with real key presses.

- **Finished Small Cabin:** with the Hammer equipped, looking at the cabin shows "Dismantle Small Cabin (gives back about half its materials)" on R. Without the Hammer there's no prompt, and pressing R anyway says "Can't dismantle the Small Cabin — equip the Hammer." The first R press asks "Press R again to confirm — you'll get about half its materials back"; a second press within 5 seconds dismantles it, and if the window lapses the next press just asks again. The hearth has to be out first, otherwise "put the hearth fire out first"; it's removed with the cabin, and any fuel left in it comes back as Firewood.
- **Storage piles (supersedes the stopgap above):** the "Take Down Structure" button on the transfer screen now **needs the Hammer** — greyed with "equip the Hammer" or "empty it first". It still returns half the build materials, rounded down. The previous turn's no-Hammer stopgap is gone, as the spec intended.
- **In-progress cabin site:** unchanged and still Hammer-free (placing one needs no tool); returns everything stocked, in full.
- **Lean-To and Tent:** keep their own pack-ups.
- **Refunds:** half the materials, rounded down. A cabin returns 10 Logs, 5 Branches, 7 Tall Grass, 6 Stone and 3 Clay, plus the hearth's Firewood. What doesn't fit in the pack lands on the ground as pickup piles, saved with the world, never lost.

**Tested:** cabin gating — the refusal without the Hammer, the refusal with the hearth lit, the confirmation lapsing, and the real dismantle. Overflow — with the pack nearly full, pack and ground totals matched the expected refund exactly, and the hearth and cabin were both gone. A Rock Pile refused without the Hammer and again when it held Stone, then came down once empty with the Hammer; a cabin site still came down with no Hammer in hand. **Not tested:** the button with a real click, and a cabin from an older save (the hearth is found by where it was placed, so older saves should work, but only a cabin built this session was tested).

**Things to know:** equipping the Hammer while the transfer screen is open doesn't update its button — close and reopen it, because the screen only refreshes on its own actions. R without the Hammer next to a cabin shows the "equip the Hammer" message, not nothing. Storage piles have no on-site Dismantle prompt (E and R both open the screen), so the button is the only way in. Larger housing tiers don't exist yet; the cabin path is the template for them. Mike's slot1 save was restored after, all six files matching.

**Still open:** nothing from the 2026-10-03 spec beyond future permanent tiers (Large Cabin and up) once they exist. **Earlier re-confirmation, now historical — 2026-10-04 (Claude Code, asked directly by Mike):** no dismantle code exists, no commit mentions it; the Tent/Lean-To take-downs in `Shelter.cs`, the CabinSite removal (`e7cbe67`) and the storage-pile take-down are stopgaps, not the Dismantle itself. **Mike green-lit building it ("yes, build the real Hammer Dismantle")** — Claude Code is starting from this spec's remaining items: the Hammer gate, refunds for finished buildings, and a confirmation step for the cabin. (Mike's earlier "hammer dismantle worked" was about the site/pile removal, not this.) No build report yet.

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

**Note added 2026-10-03 (Claude, research):** if "equipment" ends up meaning an electric well pump, Large Cabin is the first tier that would need real power — see the new Power_System.md, which sizes generation/storage against exactly this load as its first worked example (~1,500W peak, ~1–2 kWh/day). Not locked to an electric pump specifically; a hand or foot pump stays a valid non-electric alternative Claude Code could choose instead.

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

**Note added 2026-10-03 (Mike/Claude, research):** Windmill and Solar Shed, plus a new Gas Generator option, now have real sizing math behind them — see the new Power_System.md, researched and written in response to Mike's request to work out correct generator/solar/wind/battery sizing ahead of actually needing it. Nothing here is buildable yet; Power_System.md stays Future until a tier that actually needs electricity (Large Cabin's water pump is the first candidate) gets built.

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

**Note added 2026-10-03 (Mike):** describes the default case, not a guarantee — see Tools: Hammer's new dismantle action, which lets the player remove any structure, including permanent housing, if they choose to. A homestead's story is still there to look back on for whoever leaves it standing.

---

# Design Rules

1. Housing progression should feel meaningful.

2. Better shelter improves quality of life.

3. Older structures remain useful.

4. Buildings tell the story of the homestead.

5. Structures should support self-sufficiency rather than wealth accumulation.

6. **Added 2026-10-03 (Mike):** rules 3 and 4 above describe what happens by default, not what's enforced — the player can dismantle any structure, including permanent housing, with the Hammer (Tools: Hammer). Permanence is the player's choice to keep, not the game's choice to impose.

## Playtest 2026-10-09 (Mike): how do we deploy the Tarp?

**Question from Mike:** "How do we deploy the tarp?" **Answer from the code (SleepManager.cs):** there is nothing to deploy. The Tarp is purely passive: if it is anywhere in the pack when the player sleeps outdoors with no shelter, sleep treats it as strung overhead (rain 90%, wind 30%, per Difficulty_System.md). It has no equip, place or pitch action and nothing is visible in the world, so it reads as a dead item.

**Confirmed 2026-10-09 (Mike); numbers are first-pass (Claude, not yet confirmed) - Pitch Tarp as a real placed shelter:**

- A new entry in the Inventory Build list, "Tarp Shelter": needs the Tarp (it is placed and taken back, not used up) + 2 Sticks + 1 Cordage (stakes and a ridgeline). No Hammer.
- Placed like the Lean-To and Tent, with the same site-prep checks but a smaller footprint. Taking it down returns the Tarp and half the Sticks, rounded down, like the other refunds.
- Sleeping under it: Warmth loss cut by about 25%, rain 90%, wind 50% (a pitched, staked tarp should beat one just thrown over the player at the 30% passive figure). A Sleeping Bag stacks on top as it does elsewhere. Still well below a Lean-To (50%) and a Tent (60%), so the shelter tiers stay in order: nothing, Tarp, Lean-To, Tent, Cabin.
- It counts as a roof for the existing OverheadCover check, so it also keeps rain off the player while awake, and so a Bow Drill under it ignores the rain and snow penalty.
- The passive carried-Tarp effect stays as the fallback for sleeping rough without pitching anything.
- **Confirmed 2026-10-09 (Mike): "yes, i think we need to pitch the tarp as you suggested."** Tarp Shelter is a go as specced above; the numbers (25% warmth-loss cut, 90% rain, 50% wind, 2 Sticks + 1 Cordage) are Claude's first pass and Claude Code should correct anything that does not fit the code.

## Playtest 2026-10-09 (Mike): "I couldn't find any clay"

**Cause, from the code (AxeTool.cs, committed 2026-09-27):** Clay is not a placed pickup. It is dug with the Primitive Shovel from dry ground within about 3 m of the water's edge of any creek or pond: hold click and four blows later you get one lump. Nothing in the world marks a bank, so a player who has not equipped a Shovel and aimed at the shore sees nothing at all. The Lantern (2 Clay + 1 Cordage) and the Small Cabin hearth (6 Clay) both need it.

**Proposal (Claude, not yet confirmed):** keep the Shovel dig and the never-runs-out rule, but make clay findable the way sticks, stones and tall grass are now: (1) visible clay patches along the banks, a grey-orange wet-clay look on the ground, placed by a re-runnable menu (Homestead > Place Clay Banks) with stable ids, plentiful along the whole shoreline; (2) digging works on a patch and, as before, anywhere on the bank within 3 m of water; (3) looking at a patch without a Shovel equipped shows "Clay bank - equip a Shovel to dig" (and with no Shovel in the pack, "Clay bank - a Primitive Shovel digs this (3 Sticks, 1 Cordage, 1 Stone)"). First, Claude Code should confirm the existing dig works at a bank in Play Mode, since the problem may be discoverability only.

**Clay patches built 2026-10-09 (Claude Code), compile-checked only, NOT Play Mode tested, NOT committed, and the placer has NOT been run:** new ClayBank.cs marks a patch and shows the prompt (nothing with a Shovel equipped, since the Shovel's own line takes over; "Clay bank - equip a Shovel to dig" with a Shovel in the pack; "Clay bank - a Primitive Shovel digs this (3 Sticks, 1 Cordage, 1 Stone)" with none, the cost read from the recipe). AxeTool.cs now also counts a looked-at patch as bank, and the 3 m rule still works anywhere on the bank. New editor menu Homestead > Place Clay Banks (ClayBankPlacer.cs): samples terrain every 1.5 m for water, treats dry ground within 3 m of water as bank, places a grey-orange glossy mound about 2 m across at least 4 m apart, stable ids from 1, seed 20261009, cap 1,000 patches, slope limit 30 degrees. Patches never run out, so no manager and nothing to save. Mike runs it in the World scene, reads the "[ClayBankPlacer] ... placed N clay patches" Console line, and saves the scene (the menu marks it dirty but does not save). Claude Code did not do the Play Mode dig check to avoid risking the playtest save; reading AxeTool.cs, the Shovel does dig Clay.

**Correction (Claude, 2026-10-09):** Claude Code said the Pioneer kit already includes the Shovel. It does not: Pioneer gets only the Bucket, 3 Cordage and the Knife; Settler and Homesteader carry a Shovel. If Mike was testing on Pioneer, that is the other half of "couldn't find any clay": he had no Shovel, so the bank never offered anything. A Pioneer has to craft one (3 Sticks + 1 Cordage + 1 Stone), which the new prompt now tells them.

**Tarp Shelter built 2026-10-09 (Claude Code), compile-checked only (Unity compiled clean), NOT Play Mode tested, NOT committed:** `PileKind.TarpShelter` added at the end of the enum in WoodManager.cs so existing saves keep their meaning; saves and loads with the world; Build-list entry placed before Tent (order by tier). Cost 1 Tarp + 2 Sticks + 1 Cordage (one TarpShelterSticks constant), no Hammer, same site checks as the Lean-To. Look (Shelter.cs): a blue sheet on a cord ridgeline between two poles at the front, sloping to two stakes at the back, 2.0 m wide by 1.6 m deep (Lean-To 2.4 x 1.8), sheet height 2.0 m front and 0.5 m back, with a collider so OverheadCover.Roofed sees it (this also covers the Bow Drill's rain/snow check, which reads Roofed directly). Press R to "Take Down Tarp Shelter": returns the Tarp and 1 Stick (half of 2) through the existing refund helper, with anything that does not fit dropped on the ground. SleepManager: sleeping under it counts as a shelter tier and a Sleeping Bag stacks as before; the passive carried-Tarp fallback is untouched, and the note "Your Tarp keeps the rain off while you sleep outdoors, or pitch it for a proper shelter." shows on the sleep overlay when sleeping rough with a Tarp in the pack.

Shelter tier numbers as built (warmth-loss cut / rain blocked / wind blocked / Health recovery): nothing 0% / 0% / 0% / 1x; carried Tarp, rough 0% / 90% / 30% / 1x; **Tarp Shelter 25% / 90% / 50% / 1.5x**; Lean-To 50% / 80% / 60% / 2x; Tent 60% / 100% / 90% / 2x; Cabin 75% / 100% / 100% / 2.5x. With a Sleeping Bag on top the Tarp Shelter's cut becomes about 44%.

**Deviations and caveats:** (1) Health recovery 1.5x is Claude Code's pick (the spec did not give one; same as a Sleeping Bag alone). (2) The Tarp Shelter blocks more rain (90%) than the Lean-To (80%), as specced, so the tiers are in order for warmth and wind but not rain. (3) The 2 m spacing between structures is the same as for other piles even though the footprint is smaller. (4) A standing player (1.8 m) only fits at the very front edge; a crouching player (1.1 m) fits under about the front 60% of the sheet, which is where "awake and roofed" works. (5) Pioneer has no Tarp and Settler dropped it too, so only Homesteader starts with one; everyone else buys one at the Trading Post ($8) to pitch it. (6) The Bow Drill under the tarp depends on fire-placement spacing allowing a fire there, so that test is the least certain.

**Gap found 2026-10-09 (Mike):** the Small Cabin site does not tell the player what is still missing when it is partly stocked - see Inventory_System.md ('Gap found 2026-10-09 (Mike, playtest): the Small Cabin site does not say what is missing') for the cause and the proposed fix.


## Playtest 2026-10-09 (Mike, screenshot): the finished Small Cabin is drawn wrong and cannot be entered

**Mike:** "small cabin rendered incorrectly ... it appears that 2 of the walls are lined up in the wrong direction, so you can't enter the cabin. And the site never informed me that everything was there and to equip the hammer to build." The screenshot shows stacked logs sticking out sideways from the cabin and a roof sitting askew over them.

**Cause, from the code (Shelter.cs, BuildCabin, Claude's reading, not yet verified in the editor):**

- **Two walls are rotated wrong.** The two side walls sit at x = plus or minus half the width and should run along the depth (Z), but their logs are rotated (0, 0, 90), which turns the cylinder to run along X instead, 5 m long and centered on the wall, so they poke through the cabin interior and out the far side. The correct rotation for those two walls is (90, 0, 0). The front and back walls (90, 90, 0) are already right.
- **The logs do not touch.** Each log is scaled to a 0.14 m diameter but stacked every 0.28 m (log radius used as the scale instead of the diameter), so the walls are half gaps. They should be scaled to 0.28 m diameter so the courses sit flush.
- **It cannot be entered even once the rotation is fixed.** The "doorway" is a solid dark cube laid on the front wall, and the whole cabin has one big box collider the size of the footprint, so the player cannot walk in at all. Until now the cabin has worked as a solid prop you sleep beside.

**Resolved 2026-10-09 (Mike's expectation: a cabin you can walk into). First-pass scope (Claude, not yet confirmed) - Claude Code should correct if it does not fit the code:**

- Fix the side-wall rotation to (90, 0, 0) and the log diameter, so all four walls are solid stacked logs.
- **A real doorway** in the front wall, about 1.1 m wide and 2.0 m high: skip the log courses inside the opening, add one lintel log across the top, and drop the dark door cube (or make it an open dark frame only).
- **Collision follows the walls, not one big box:** four wall colliders with a gap at the door, a lintel collider above it, a walkable floor, and roof colliders so the OverheadCover.Roofed check still counts the roof. The foundation slab is 0.3 m high, so make sure the player can step onto it at the doorway (a small stone step if the character's step height is lower).
- **Everything that works today keeps working:** looking at the cabin still gives the sleep prompt (from outside and from inside), sleeping inside still counts as the Cabin shelter tier, the hearth still lights, the Hammer dismantle prompt still works, and a cabin saved before this change still loads and is rebuilt with the new geometry.
- The hearth stays where it is for now. **Open question for Mike (not a blocker):** with a walkable interior, should the hearth move inside the cabin?

**Also from the same playtest: no notice when the site was complete.** The look prompt and button text from the 2026-10-09 cabin-site fix should say the site is fully stocked, but Mike never saw it. Possibly he was testing a build from before that fix compiled; if not, the prompt is not showing. Either way, add an active notice: the moment a deposit (R) completes the last missing material, show the large banner "Small Cabin site fully stocked - equip the Hammer, open Inventory and press Complete Small Cabin". And the look-prompt wording should change once the Hammer is already equipped to "open Inventory and press Complete Small Cabin" instead of repeating "equip the Hammer".

**Confirmed 2026-10-09 (Mike, Play Mode playtest of the cabin fix): "I have confirmed the playtest was successful. sleeping in the cabin was good, under cover ... everything seemed to work as planned."** The walls, the walkable doorway, sleeping inside and the roof counting as cover all stand as built (I have not seen Claude Code's own report on the geometry fix, so any deviations it made are not recorded here).

**Resolved 2026-10-09 (Mike), answering the open question above: the hearth belongs INSIDE the cabin.** "the firepit outside the cabin was not expected. i was expecting the fireplace inside the cabin." The furnished hearth currently goes just outside the walls (FireManager.BuildFurnished, placed from Shelter.CabinWidth and CabinDepth). **First-pass scope (Claude, not yet confirmed) - Claude Code should correct if it does not fit the code:**

- Move the hearth inside, against the back wall opposite the doorway (or a back corner), as a stone-and-clay hearth set on a small stone slab, with the fire as a real campfire exactly as now (Firewood to feed it, lit with Flint and Steel or the Bow Drill, warms the sleeper and the room).
- Keep the doorway path and a sleeping spot clear: nothing blocks the door, and the fire is far enough from the log walls that the player can walk around it. No fire damage to the cabin (nothing in the game burns structures).
- Smoke: a simple vent is enough (a gap at the ridge or a short clay chimney); no new smoke simulation.
- A lit fire indoors counts as under cover, so rain never affects it, and the existing fire-warmth radius keeps working for the cabin sleeper.
- A cabin already saved with the hearth outside is moved to the new spot on load (the hearth's saved fuel and lit state carried over), not left behind.
- The earlier hearth quantities (Stone and Clay for the hearth) and the Dismantle refund are unchanged.

**Built 2026-10-09 (Claude Code), not yet compiled or Play Mode tested:** the hearth sits inside, a little right of centre against the back wall (local x +0.45, z -1.45 from the cabin's centre, door on +Z), on a 1.3 x 1.1 m stone slab 0.18 m high, with a clay backing and a clay chimney through the thatch as the vent (visual only; the open gable is a second gap). 0.76 m is left to the right wall and 1.7 m of open floor to the left for the door path and a sleeping spot. The fire is the same campfire as before (no rain effect exists on fires in the game, so rain never affects it; warmth is by planar distance, walls do not block it). A cabin saved with the hearth outside has the fire moved indoors, fuel and lit state intact, within a second of loading (WoodManager.MigrateHearths); the Dismantle refund and quantities are unchanged and dismantling finds the hearth at either spot.
**Bug found 2026-10-10 (Mike, Play Mode, screenshot): loading a save made while standing inside the Small Cabin puts the player ON TOP of the roof.** Everything else about the indoor hearth "works as described" (Mike). The screenshot shows the player standing on the thatch roof with the prompt "[Hold E] Rest in the Small Cabin (3 hours)". Expected: the player reappears at the saved spot inside the cabin. Not yet diagnosed. Likely suspects (Claude's guesses, unconfirmed): the load snaps the player to the ground with a downward ray or ground check from above, and the cabin's roof colliders (added in the geometry fix) are hit first; or the player's saved position is applied before the cabin is rebuilt from its saved state and then re-snapped once the roof exists. Fix goal: a save made inside any enclosed structure (Small Cabin, Tent, Lean-To, Tarp Shelter) restores the player at the saved position inside it, without being lifted onto the roof.

**CONFIRMED 2026-10-10 (Mike, Play Mode): indoor hearth works (lights with Bow Drill and Flint and Steel, lights the room, warms the player, cooking and boiling good, warm overnight, all prompts good) and save-and-load inside the cabin now restores the player inside. Not yet confirmed: old outside-hearth save migration, dismantle with the fire out, Tarp Shelter and slope/creek save-load, older pre-cabin save.**

**Cause found and fix written 2026-10-10 (Claude Code report; NOT compile-checked, NOT Play Mode tested, uncommitted):** the load-on-the-roof bug is in PlayerController.AboveGround, called from RestoreState (PlayerController.cs:537). The old probe cast a ray down from 500 m above the saved position and teleported the player onto the first collider hit if it was above the saved Y; inside the cabin that first hit is the roof. It was written to catch saves from before the terrain was reshaped (underground positions) and was never meant to handle buildings; it could not trigger before the cabin geometry fix because nobody could stand inside the old solid cabin. Fix: AboveGround is now an instance method in two steps. (1) After Physics.SyncTransforms(), if the player's capsule fits at the saved position (no collider overlap), restore it exactly: cabin interior, doorway, hearth slab, under a Tarp. (2) Only if it overlaps, do the downward probe but skip colliders belonging to a Shelter (roof, walls, tarp sheet) and lift only if the ground is above the saved Y. Affected before the fix: Small Cabin (inside, doorway, sleeping) and Tarp Shelter (sheet collider over the sleeper's head, and sleeping triggers an auto-save). Not affected in practice: Tent and Lean-To (solid, can't stand inside; skipped now anyway), Campfire, storage piles and bins. Slopes up to about 30 degrees keep the exact position; steeper ones may report a false overlap, but the fallback probe finds the same height and keeps it; underground saves from before a terrain change still lift to the surface. No other fell-through-the-world safety net exists in the code.

**CONFIRMED 2026-10-10 (Mike, Play Mode, all pass):** old outside-hearth saves migrate the fire indoors (fuel and lit state kept, none left outside); dismantling the cabin with the fire out removes the hearth and returns Firewood; save and load under a Tarp Shelter, on a steep slope, on the creek bank, and from an older pre-cabin save all restore correctly. Together with the earlier confirmation, the indoor hearth and the load-on-the-roof fix are fully confirmed.
