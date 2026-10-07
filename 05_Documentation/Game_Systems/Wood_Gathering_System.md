# Homestead
## Wood Gathering System v1.0

Status: Built and tested 2026-09-26 (Claude Code) — committed and pushed as part of commit `0563b9e` (with doc updates in `08b2926`). Phase 3 (Eating/Cooking/Water Purification/Sickness) is committed and pushed alongside it; nothing from this doc's scope is left uncommitted.

---

# Purpose

Right now the only source of Firewood is gathering deadfall piles by hand (Fire Building's own implementation), and Logs — required by Building_Housing_System.md's Small Cabin — have no source in the game at all. This system gives the player a real way to cut down trees and turn what they get into Firewood or building material, using the Axe that already exists as a Tool (Item_Data.md, Inventory_System.md).

---

# Core Loop

Equip Axe
↓
Chop down a tree
↓
Sticks + Branches + Logs
↓
Build (Logs) or burn (Logs, Branches) or craft (Sticks)

---

# Chopping Down a Tree

Requires the Axe equipped. Exact interaction (hold-to-chop vs. repeated hits, how many hits/how long per tree, whether smaller trees fall faster than large ones, and whether a felled tree leaves a stump) is Claude Code's call, same propose-then-confirm pattern as the rest of this doc set. A felled tree yields some mix of Sticks, Branches, and Logs — exact yield per tree (and whether it scales with tree size) is also Claude Code's call.

**Built and tested 2026-09-26 (Claude Code, for Mike to judge by feel):** hold left-click on a trunk within reach with the Axe equipped. About one swing every 0.9s, 4 stamina per swing ("Too tired to swing" if stamina runs out); being hungry or thirsty slows the work via the existing work-efficiency value from the survival stats. Swings needed to fell: about 10 for a broad hardwood, about 13 for a tall hardwood, about 3 for a shrub — more for bigger trees. A HUD progress bar tracks it, and looking away for a moment doesn't lose progress. On felling, the tree tips away slowly, then fast, settles with a small bounce, then sinks away after a few seconds; hardwoods leave a stump, shrubs leave nothing. Felled trees stay down across saves and reloads (only the in-game copy of the terrain changes, the terrain asset on disk is untouched) — trees don't grow back yet. Implementation note: trees are part of the terrain rather than separate objects, so if `PropertyTerrainBuilder` ever regenerates the terrain, felled-tree state in existing saves would point at different trees — a known risk, not yet acted on.

Tested in Play Mode on a copy of Mike's save: felling a broad hardwood produced 2 Logs, 5 Branches, and 6 Sticks, with a stump, a wood pile, and one fewer terrain tree; save/reload round trip confirmed the felled tree and pile both persisted.

---

# Yields

## Logs

The largest, most valuable yield. Two uses:

- **Building** — Building_Housing_System.md's Small Cabin already lists "Logs" as a Requirement; this is that Requirement's real source once both systems exist. Building_Housing_System.md itself has no Unity implementation yet (see that doc), so this use is Future until Building actually gets built.
- **Firewood** — a Log can be chopped into Firewood with the Axe (see Chopping Firewood below). Burns the longest of the two fuel-yielding materials.

## Branches

Mid-sized — bigger than a Stick, smaller than a Log. Two uses:

- **Primitive shelter** — Building_Housing_System.md's Lean-To ("built from local materials") is the natural home for this, once Building exists. Future until that system is built.
- **Firewood** — also choppable into Firewood, but burns for less time than a Log's worth.

## Sticks

The smallest yield. Used to craft different types of hand tools when combined with Stone and Cordage. No general crafting system exists yet (deferred post-Alpha per the 2026-09-25 Decisions_Log.md entry — each feature defines its own small ad-hoc recipe as needed, rather than building a generalized system early), so this stays a Future use until a specific hand tool is actually requested and gets its own ad-hoc recipe, the same way Cordage's and Arrows' recipes were handled.

**Built and tested 2026-09-26 (Claude Code, for Mike to judge by feel) — actual yields per tree:**

| Tree | Logs | Branches | Sticks |
|---|---|---|---|
| Broad hardwood | about 2 | about 4 | about 5 |
| Tall hardwood | about 3 | about 3 | about 4 |
| Shrub | 0 | 1 | about 3 |

Amounts scale with the tree's size. Everything lands in a wood pile at the tree's base rather than straight into Inventory — looking at a pile shows what's in it, and E takes whatever fits, lightest items first and Logs last, so a big tree can take more than one trip ("Left behind: 2 Logs" when the pile isn't fully cleared). Piles save with the game and disappear once emptied. Item weights used: Sticks 0.1 kg, Branches 1 kg, Logs 8 kg (Item_Data.md).

---

# Chopping Firewood

A separate Axe interaction from felling a tree: chopping a carried (or dropped) Log or Branch converts it into Firewood — the same Firewood item Fire Building already produces from deadfall, so it drops straight into the existing Fire System (Core_Survival_System.md) and campfire fuel loop with no new item needed on that end. Logs should yield more Firewood, or Firewood that burns longer, than Branches — exact conversion numbers are Claude Code's call.

**Built and tested 2026-09-26 (Claude Code, for Mike to judge by feel):** 1 Log makes 5 Firewood (10 hours of burn time at Fire Building's existing 2-hour-per-Firewood rate); 2 Branches make 1 Firewood. Both conversions lighten the load a little. Two ways to split: at a wood pile, swinging the Axe at it splits its Logs first (4 swings each), then its Branches; or, while carrying the Axe, carried Logs and Branches get a Split button in the Inventory screen that works like Cook — about 4s per Log, about 2s per pair of Branches, with chopping sounds, a countdown on the button, Shift-click for the whole stack, click again to stop, and it keeps running while the Inventory screen is open.

Tested in Play Mode: splitting 2 Logs at a pile produced 10 Firewood; splitting from Inventory turned 1 Log into 5 Firewood and 4 Branches into 2 Firewood with the leftover Branch kept (an odd Branch doesn't get wasted). One visual-only issue noted but not re-verified on screen: split Firewood pieces in a pile had been visually merging into long bars — fixed by laying the pieces out across the row instead.

---

# Primitive Storage (Wood Pile / Rock Pile)

**Added 2026-09-26 (Mike) — requesting now, not deferred to Building.** Mike originally scoped a player-placed wood pile and rock pile as a Future ask, sequenced after Building_Housing_System.md's real storage buildings (Root Cellar, Storage Shed, Barn) shipped — see that doc's Storage category. He's since decided the primitive versions should come in now, with better storage upgraded in later once Building exists.

Wood Gathering already produces a working pile mechanic as a side effect of felling — a tree drops its yield into a pile at its base that persists across saves, shows its contents on look, and lets the player take what fits over multiple trips (see Chopping Down a Tree above). A player-placed Wood Pile is the same mechanic made deliberate: build one from Inventory (same "Inventory → Build" pattern as the Campfire, Fish Trap, and traps), place it, and then deposit or draw from it like any felled-tree pile. Holds the game's wood-family Materials — Sticks, Branches, Logs, Firewood.

A Rock Pile is the same idea for Stone (already an existing Material, Item_Data.md) — nothing currently produces or stores Stone at all, so this gives it its first real home.

Exact build cost (if any — could be free to place, or cost a small amount of Cordage/materials, Claude Code's call), capacity (unlimited vs. a soft cap), and the deposit interaction (an Inventory "Store" button mirroring Cook/Split, or a direct drop-into-pile interaction) are Claude Code's call, same propose-then-confirm pattern as the rest of this doc set. Both pile types persist across saves the same way felled-tree wood piles already do.

**Built and tested 2026-09-26 (Claude Code) — committed and pushed (`0563b9e`).** A Wood Pile costs 4 Sticks to build; a Rock Pile is free to place. R stores an item into a pile, E takes from it — the same interaction shape as the felled-tree piles. Neither pile has a capacity limit. Unlike the felled-tree piles, both of these persist across saves even when completely emptied, staying visible in the world as an empty frame (Wood Pile) or a small ring of base stones (Rock Pile) rather than disappearing. Stone itself still has no source anywhere in the game — the Rock Pile exists and works, but nothing currently produces Stone to put in it; that's a known gap for a future rock-gathering mechanic, not something this entry was asked to fix.

**Future, once Building_Housing_System.md ships:** these primitive piles get replaced or supplemented by real storage buildings — larger capacity, weather protection, and (per that doc's own Storage category) a proper Storage Shed as the built upgrade path. The primitive piles aren't meant to be the permanent answer, just the buildable-now one.

---

# Ground Sticks and Fallen Branches (gap found 2026-10-07)

**Gap found 2026-10-07 (Mike, Pioneer playtest, new game from the beginning):** "i'm noticing some difficulty finding any branches or sticks to build my campfire. I think we need to re-evaluate the distribution of some basic resources. Basically, i think rocks, sticks, branches and tall grass should be more readily available."

**What the docs show (Claude, from the Wood, Stone, Trapping and Difficulty docs):** the only way to get Sticks or Branches today is felling a tree with an Axe (Yields above). Deadfall piles give Firewood only, never Sticks or Branches, and windthrow leaves a wood pile only when a tree actually falls. Pioneer starts with no Axe, so on that tier Sticks and Branches have no source at all. That also blocks the way out: the Primitive Axe needs 2 Sticks, so a Pioneer who can't find Sticks can't make the tool that would yield them. This breaks Difficulty_System.md's Design Rule 2 ("every tier must leave a viable path from nothing to self-sufficient"). It applies to every tier in a milder form, since Homesteader and Settler can only get Sticks by chopping too.

**First-pass scope (Claude) — accepted for now 2026-10-07 (Mike: "seems ok, for now"; he'll update after more playtesting). Claude Code should still correct anything that doesn't fit the code:**

- **Loose Sticks:** scattered stick bundles on the ground, picked up by hand with E, no tool. Each bundle gives 3 Sticks (0.3 kg). About 300 across the property, densest under the hardwood canopy and along woodland edges, lighter in meadow and pasture, none on open water.
- **Fallen Branches:** single branches lying on the ground, picked up by hand, no tool. Each gives 1 Branch (1 kg). About 120, concentrated in the woods.
- **Regrowth:** unlike Stone, which is gone for good once taken, loose Sticks and Branches come back. Claude's first pass is 5 in-game days for a stick bundle and 7 for a branch (a forest keeps shedding them), so a Pioneer can never be left without a source. Available in every season, Winter included.
- **Not difficulty-scaled:** the same availability on all three tiers. Pioneer's harshness comes from the drain multipliers and the empty starting kit (Difficulty_System.md), not from a missing source of basic materials.
- **Placement:** an editor-only menu like the Stone and Tall Grass ones (for example Homestead → Place Sticks and Branches), not a player-facing feature. Exact counts, spacing and a look-prompt such as "Sticks" or "Fallen branch" are Claude Code's call.

**Related density changes (Stone and Tall Grass) are in Stone_Gathering_System.md and Trapping_System.md, same date.**

**Built 2026-10-07 (Claude Code) — compile-checked only, not committed, not tested in Play Mode.** `GroundWood` is a hand-pickup component with no tool needed: a stick bundle gives 3 Sticks and a fallen branch gives 1 Branch, with the prompts "Pick up Sticks" and "Pick up Branch". `GroundWoodManager` remembers what was taken and brings it back after 5 in-game days (sticks) or 7 (branches). It saves under the key `groundwood`, resets on a new game, and creates itself at startup, so no scene edit was needed; saves from before this load with nothing taken. The new editor menu Homestead → Place Sticks and Branches places 300 bundles (85% under hardwood canopy or at shrub edges, the rest in the open) and 120 branches (95% in the woods). Both skip water and slopes over 25°, and the shapes are generated low-poly meshes saved under `Assets/Art/Wood`. Same on every difficulty and in Winter. **Not tested:** Play Mode, the placement menu, pickup, regrowth, save and reload, and real input. UnityMCP failed to connect, and the open editor hadn't recompiled. **Still needed (Mike, or Claude Code with MCP):** run the menu, check pickup and prompts, regrowth across days, a save and reload, and that a new Pioneer game can get Sticks, light a fire and craft a Primitive Axe. Claude Code also edited `Bootstrap.unity` on disk while the editor had it open, so Unity may ask to reload.

---

# Natural Tree Fall (Windthrow)

**Added 2026-09-26 (Mike) — confirmed as a real ask, not yet built.** Resolves the open question Audio_System.md raised when `tree-fall.wav` was sourced: besides a tree coming down from being chopped (built above), standing trees should also be able to fall on their own from weather, independent of the player. Mike's own framing: "random throughout the year, primarily after or during heavy thunderstorms or long winters with a lot of snow buildup."

So this is a low, year-round background chance on any standing tree, weighted heavily upward by two conditions already tracked elsewhere in the game: during or shortly after a Thunderstorm (Weather_System.md), and after a long winter's worth of accumulated Snow (Weather_System.md's Snow effects, which already include surface water freezing and reduced foraging as season-long buildup effects) — read as heavy snow load bearing down on branches and trunks over a Winter, not a single snowfall. Outside those two triggers, a windthrow should be rare enough that a player could easily go a full season without seeing one.

A tree that falls this way should behave like a felled tree for everything downstream — it yields the same Sticks/Branches/Logs mix (Yields above) into a wood pile at its base, plays sfx_tree_fall (`tree-fall.wav`, Audio_System.md) for hardwoods same as a chopped fall, and leaves a stump the same way. It just skips the chopping interaction entirely — the tree simply comes down while the player isn't swinging an Axe at it, whether they're nearby to see it or not.

Exact odds (a per-tree-per-day roll vs. a check tied specifically to Thunderstorm/Winter-snow events), how much the two trigger conditions weight the chance versus the year-round baseline, and whether it can happen to trees near the player only or anywhere on the property, are all Claude Code's call, same propose-then-confirm pattern as the rest of this doc set. Worth flagging: this compounds the existing "trees don't regrow" risk already noted in Chopping Down a Tree above — a long enough game with both chopping and windthrow active will thin the property's tree count with no regrowth to offset it, same caveat, not a new one.

**Built and tested 2026-09-26 (Claude Code):** windthrow can hit any standing hardwood anywhere on the property, falling roughly in the wind's current direction; shrubs are never blown down, and nothing falls within 6 m of the player. First-pass odds: about one tree a month on an ordinary day; about one every 8 storm-hours during a Thunderstorm; raised for the 12 hours following a storm; and in late Winter/early Spring, scaling with how much snow fell that Winter, up to about one every day and a half after a very snowy one. Storm and snow state save with the game. Tested in Play Mode: forced a windthrow directly, and let the ordinary-day odds simulate out to about 1 per 30 days.

---

# Tools: Primitive Shovel

**Added 2026-09-26 (Mike) — new craftable Tool, two purposes.** A primitive Shovel, requested for (1) removing tree stumps that hardwood felling leaves behind (Chopping Down a Tree above — right now a stump is permanent, with no way to clear it) and (2) clearing and leveling ground to build on, tying into Building_Housing_System.md.

**Materials:** Mike specified the Stone Pick Axe's materials (Sticks, Cordage, Stone — see Stone_Gathering_System.md) but not the Shovel's. Proposing the same three — Sticks, Cordage, Stone — as a first-pass recipe, on the same "stone tool head bound to a wood handle" logic as the Pick Axe, since a flat stone blade lashed to a stick handle is a reasonable primitive digging tool. **This is a proposal, not confirmed by Mike** — flag it for him to correct if he had different materials in mind, same as any other open first-pass detail in this doc set.

**Recipe update requested 2026-09-26 (Mike) — conflicts with the already-shipped recipe above, still unresolved, now for a different reason.** Mike specified the Primitive Shovel should craft from Sticks, Cordage, and a **Board** — not the Stone above, which was only ever Claude Code's own first-pass guess (never confirmed by Mike, per the note above).

**Confirmed 2026-09-26 (Mike) — Board answered, but it opens a new question:** "a board is a type of lumber. lumber comes from a lumber mill and can be purchased at the trading post until the player is able to purchase or build one of their own. boards are used for building most buildings. boards will inherit the quality of the lumber it comes from from pine to birch to oak and walnut in levels of quality." So Board is not a separate material — it's the existing `lumber` item (Item_Data.md), settling the near-duplicate-material question Claude raised. But this also means Lumber/Boards have a real, defined production chain now, and **none of it exists yet**: a Lumber Mill (a brand-new production building, not designed or built anywhere in this project) or a purchase from the Trading Post (Difficulty_System.md's new mechanic, also not built — no currency or shop UI exists yet). It also implies tree species/wood-quality tiers (Pine → Birch → Oak → Walnut) that Wood_Gathering_System.md's tree chopping has never modeled — every tree currently yields the same generic Logs/Branches/Sticks regardless of species.

**Resolved 2026-09-26 (Mike): "agreed, keep your shovel recipe; and include metal versions for the future upgrades."** The shipped recipe (3 Sticks, 1 Cordage, 1 Stone, commit `4145888`) stays exactly as-is — no code change needed, no Board/Lumber dependency for this tool at all. The Shovel instead joins the Axe/Pick Axe's Primitive → Iron → Steel tiering (see the "Tools: Primitive Axe" section's Tiering paragraph just below): Stone Shovel is its Primitive tier, with Iron and Steel Shovel upgrades planned for later. Claude Code's reading of "metal versions": Iron and Steel, matching the exact two-tier progression Mike already confirmed for the Axe/Pick Axe, rather than inventing a separate naming scheme just for this tool — worth a quick confirm once that's actually relevant. Same flagged gap applies: neither Iron nor Steel exists as a material yet, blocked on the same unscoped Mining/Metalworking system.

**Stump removal** is buildable now, since felled trees and their stumps already exist in the game: equip the Shovel, interact with a stump, and it's removed. Whether it yields anything (a small amount of Firewood-grade wood scrap, or nothing at all) and how long the interaction takes are Claude Code's call. `digging.wav` (sfx_digging, Audio_System.md) is the sound for using the Shovel — covers stump removal now and ground clearing/leveling once that half is built.

**Built and tested 2026-09-26 (Claude Code):** recipe is 3 Sticks, 1 Cordage, 1 Stone. Equip the Shovel and hold click on a stump; after 6 digs the stump is gone for good and the roots yield 1 Firewood. `digging.wav` plays one of 16 clean scrapes per push. Ground clearing/leveling stays Future as scoped. Two bugs found and fixed during testing: stumps had a nearly spherical collider that let aim slide off the rim, now a proper cylinder; and newly felled wood piles were landing close enough to a stump to cover its collision box entirely (this affects Mike's own save specifically, where a pile sits right over a stump) — the Shovel now finds the stump just behind the pile instead. Tested in Play Mode: crafted the Shovel, dug out a stump, confirmed both fixes.

**Clearing and leveling ground to build on** is the Shovel's other half, but Building_Housing_System.md itself is still Design-Draft-only with no Unity implementation at all — there's no actual "place a building" flow yet for a cleared/leveled site to matter to. This half is documented here and cross-referenced into Building_Housing_System.md below as forward-looking spec, not a build-now request: it becomes relevant once Building gets its first real implementation, the same "buildable-now vs. sequenced-after-Building" split already used for Primitive Storage above.

---

# Tools: Primitive Axe

**Added 2026-09-26 (Mike) — new craftable Tool, a hand-tool tier below the existing (metal) Axe.** Mike confirmed the Pick Axe and the (chopping) Axe are two separate tools, and each should have its own hand-craftable primitive version — **Sticks, Cordage, and Stone (Rock)** — following the same "stone/rock head bound to a wood handle with cordage" construction already used for the Stone Pick Axe (Stone_Gathering_System.md) and the Primitive Shovel above. The Primitive Axe does the same job as the existing `axe` item (chopping trees, per Chopping Down a Tree above) — it's a lower, hand-craftable tier that exists alongside it, not a replacement.

**Recipe:** proposing 2 Sticks, 1 Cordage, 1 Stone — matching the Stone Pick Axe's exact recipe, since both are the same "small stone tool head" construction. **Not yet confirmed by Mike** — first-pass proposal, same status as every other unconfirmed recipe in this doc set.

**Built and tested 2026-09-26 (Claude Code), not yet committed.** Recipe shipped exactly as proposed: 2 Sticks, 1 Cordage, 1 Stone, crafted from the Inventory's Craft grid (now 9 buttons, still fitting in three rows). Does everything the metal Axe does — felling trees, splitting wood at piles, splitting a carried Log or Branch from Inventory — but 1.5x slower: measured at 18 blows to fell a broad hardwood versus the Axe's 12, and 6 seconds to split a carried Log versus 4. If the player is carrying both, splitting still uses the (faster) metal Axe's speed. Tested in Play Mode on a copy of Mike's save: crafted the Axe, confirmed it shows equipped, measured both timing comparisons directly, and restored/hash-checked the save afterward. **Batch closed out 2026-09-26 — committed and pushed.** Code as `67d85da`, docs (this file, Hunting_System.md, Trapping_System.md, Item_Data.md, Difficulty_System.md, Mining_Metalworking_System.md, Research.md) as `b6cf76d`; both on `master` and pushed to GitHub. Before pushing, Claude Code ran the Knife starting-kit change in Play Mode two ways per Mike's ask: F11 on a copy of Mike's own save (added the Knife plus the Sleeping Bag/Tent/Cooking Pot his save predated) and a brand-new game (started with Flint and Steel, Bucket, 3 Cordage, the Axe, Cooking Pot, Sleeping Bag, Tent, and the Knife — 12.5 kg total). Both passed; Mike's save was restored and hash-checked afterward, project settings untouched. Nothing left open on this thread — the Hammer stays held for Building/Housing's first implementation.

**Tiering — confirmed direction, not yet buildable.** Mike also confirmed the Axe and Pick Axe should each progress Primitive → Iron → Steel, with the iron and steel tiers presumably crafted rather than store-bought outright (exact crafting-vs.-Trading-Post split per tier is open). **Extended 2026-09-26 (Mike) to the Shovel too** — see the "Tools: Primitive Shovel" section above, which resolved its Board recipe question by keeping the shipped Stone recipe and joining this same Primitive → Iron → Steel progression instead. **Gap now designed, still not buildable, 2026-09-26:** neither Iron nor Steel exists yet as a real material, but the resource chain to produce them is no longer unscoped — see the new Mining_Metalworking_System.md (mining Iron Ore, making Charcoal, smelting a Bloomery, consolidating the bloom with the Hammer and an Anvil, then Forge/Anvil/Workbench blacksmithing into a finished Iron or Steel tool), written 2026-09-26 per Mike's request to research and design the process properly. The existing generic `axe` item can still stand in as an unstated "already metal" top tier for now. This is Future, not buildable today, until that new doc's structures (Bloomery, Forge, Anvil, Workbench) get their first Unity implementation — same status as Building/Housing generally — for all three tools (Axe, Pick Axe, Shovel).

---

# Tools: Wooden Hoe and Stone Sickle (Future)

**Added 2026-09-26 (Mike) — "the same for a wooden hoe and stone sickle for harvesting crops later."** Mike asked for these to follow the same hand-craftable pattern as the Primitive Axe/Pick Axe/Shovel above, explicitly for future crop-harvesting. Both stay Future/not buildable today for the same reason: there is no cultivated-crop or Gardening/Farming system anywhere in this project yet — only wild foraged plants (berries, nuts, wild onion, etc. in Item_Data.md's Consumables) exist today, with nothing to plant, tend, or harvest on a schedule. Mike's own word "later" matches that — this section exists so the naming and rough recipes aren't lost before that system gets scoped.

**Proposed recipes, unconfirmed, reading "the same" as matching each existing tool's own material pattern:**
- **Wooden Hoe** — **Confirmed 2026-09-26 (Mike): "keep it like the stone sickle."** Recipe is now 2 Sticks + 1 Cordage + 1 Stone, matching the Sickle exactly rather than the old Board-based idea (which no longer has a live Shovel recipe to mirror, per the resolution above). Board/Lumber is no longer part of this tool at all.
- **Stone Sickle** — 2 Sticks + 1 Cordage + 1 Stone, following the Primitive Axe/Pick Axe's stone-bladed pattern, since "sickle" implies a cutting edge like the Axe/Pick Axe rather than a Shovel-style dig tool.

Both recipes are now confirmed by Mike (2026-09-26) as far as materials go — still Future/not buildable, since no Gardening/Farming system exists yet for either tool to actually harvest anything from.

---

# Cross-References

- **Building_Housing_System.md** — Small Cabin's "Logs" Requirement and the Lean-To's "local materials" both get a real source here, once Building itself is built. Not a dependency the other way — this system doesn't need Building to exist to be useful, since Logs/Branches can already be burned as Firewood. Storage category also cross-references the Primitive Storage section above. The new Primitive Shovel's ground-clearing/leveling half is cross-referenced there too, for whenever Building gets its first implementation. **Added 2026-09-26 (Mike):** Small Cabin's "Lumber" Requirement now has a confirmed real answer too — Lumber (Boards) comes from a new, unbuilt Lumber Mill or a Trading Post purchase, and Boards inherit a quality tier from the source wood's species (Pine → Birch → Oak → Walnut). This system's tree chopping doesn't model species at all today — every tree yields the same generic Logs/Branches/Sticks — so a real Lumber Mill would need species tagged onto trees here first. See Building_Housing_System.md's Small Cabin section for the fuller note.
- **Difficulty_System.md** — the Trading Post (its new mechanic) is now also confirmed as where Lumber/Boards can be bought before the player has their own Lumber Mill, per Mike's 2026-09-26 note above — one more thing that mechanic will eventually need to sell, once it exists.
- **Mining_Metalworking_System.md (new, 2026-09-26)** — the Axe/Pick Axe/Shovel's Iron and Steel tiers resolve through that doc's full mining-to-blacksmithing chain; the South Ridge cliff-side deposit (Stone_Gathering_System.md) is proposed there as a second Iron Ore source alongside bog iron near water.
- **Core_Survival_System.md (Fire System)** — Firewood produced here is the same Firewood item Fire Building already uses; no change needed to the Fire System itself.
- **Item_Data.md** — needs three new Materials: sticks, branches, logs (non-perishable, weight a first-pass proposal like everything else in that doc). Also needs a new Tools row for the Primitive Shovel (Sticks/Cordage/Stone, proposed).
- **Stone_Gathering_System.md** — the Rock Pile in Primitive Storage above is Stone's first real destination; that doc covers where Stone itself comes from, and its own new Stone Pick Axe uses the same three materials as the Shovel's proposed recipe.
- **Weather_System.md** — the Natural Tree Fall section above ties windthrow to Thunderstorm and Snow, both already-confirmed weather effects.


---

# Design Rules

1. This system covers chopping trees, Logs/Branches into Firewood, the Wood Pile/Rock Pile primitive storage, windthrow (natural tree fall from weather), and the Primitive Shovel's stump-removal half — it does not include building a primitive shelter or crafting other hand tools beyond the Shovel and (Stone_Gathering_System.md's) Pick Axe. Those stay Future, noted here and in Building_Housing_System.md, until their host systems (Building/Housing, a real reason to craft a specific hand tool) actually exist to receive them.

2. Exact chop timing, yield counts, and Firewood-per-Log/Branch conversion are all Claude Code's call — propose a first pass, Mike confirms by feel in Play Mode, same pattern as the rest of this doc set.

3. Firewood produced here is identical to Firewood from Fire Building's deadfall gathering — one item, two sources, no special-casing needed in the Fire System itself.

---

# Starting Kit Note

**2026-09-26 (Claude Code):** the Axe is now in the starting kit for new games, since nothing else in the game previously supplied one — without it, this whole system would be unreachable from a fresh start. F11 also grants an Axe to existing saves that predate this change.
