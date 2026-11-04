# Homestead
## Mining & Metalworking System v1.0

Status: Design Draft — brand-new system, first pass, not yet reviewed against Unity feasibility. Written 2026-09-26 to answer the Iron/Steel material gap flagged across Wood_Gathering_System.md, Stone_Gathering_System.md, and Difficulty_System.md's tool-tiering requests, per Mike's ask to "research the metallurgy and mining as well as smelting and blacksmithing with a forge and workbench and anvil to keep this answer accurate."

---

# Purpose

Every Iron and Steel tool tier confirmed so far (Axe, Pick Axe, Shovel — Wood_Gathering_System.md, Stone_Gathering_System.md) has been flagged as blocked on "no mining or smelting mechanic exists." This doc is the first pass at that mechanic: a real, historically-grounded chain from ore in the ground to a finished metal tool in hand, sized to one homesteader working alone rather than an industrial operation. See Research.md's new "Metallurgy & Mining Grounding" section for the real-world sourcing behind each stage below.

---

# The Chain, End to End

Iron Ore + Charcoal → (Bloomery) → Bloom → (Hammer + Anvil, heat) → Iron → (Forge + Anvil + Workbench) → Iron Tool

Iron + extra Charcoal → (Forge, carburizing) → Steel → (Forge + Anvil, quench + temper) → Steel Tool

Five real stages, matching what actual pre-industrial ironworking required: mining the ore, making the fuel (charcoal), smelting (ore + fuel into bloom), consolidating (bloom into a workable iron bar), and smithing (iron/steel into a finished tool). None of this is buildable today — it's Future, same status as Building/Housing — but it's designed in enough real detail now that it won't need re-deriving later.

---

# Stage 1: Iron Ore

Two sources proposed, mirroring Stone_Gathering_System.md's own two-layer distribution (Mike's confirmed pattern for that system) rather than inventing a new one:

- **Bog iron — surface, hand-pickup, no tool.** Real bog iron forms as nodules/concretions in wetlands and slow water, and was the ore early Ohio ironworks (including the historic charcoal furnaces of Southern Ohio's Hanging Rock Iron Region) started from before moving to mined ore. Proposed: a rare find in the property's existing water-dense zones (the creek and Bass Hole, same locations Stone's water zones already use) — hand-pickup, but scarce, so it's a slow, real early-game path rather than a reliable one.
- **Iron ore vein — the South Ridge cliff-side deposit, tool-gated.** Real hematite/iron ore is commonly found associated with rock outcrops, the same kind of geology Stone_Gathering_System.md's South Ridge deposit already represents. Proposed: Iron Ore becomes a rarer secondary yield from that same outcrop (already mined with the Stone Pick Axe) — most swings give Stone, an occasional swing gives Iron Ore instead, so no new terrain or location needs building, just a new drop chance on an interaction that already exists.

**Confirmed 2026-09-26 (Mike): both sources.** Bog iron near water and the South Ridge vein both exist in the property — not an either/or choice. Exact rarity/yield for each is still Claude Code's call once this is actually buildable, same propose-then-confirm pattern as the rest of this doc set.

---

# Stage 2: Charcoal

Real bloomery smelting runs roughly a 1:1 ratio of charcoal to ore by weight — charcoal isn't just fuel here, it's the actual reducing agent that turns ore into metal, which is why ordinary Firewood can't substitute for it. Charcoal itself is made by burning wood slowly with limited air (historically a charcoal pit or mound, biomass cooking down instead of burning to ash).

Proposed: a new **Charcoal Pit** structure (Future, same "needs Building's first implementation" status as the Bloomery/Forge below) that converts Logs into Charcoal over time — slower than lighting a campfire, no active player input needed once it's burning, similar in spirit to how a placed campfire already just keeps running. Exact Logs-to-Charcoal ratio and burn time are Claude Code's call once this is actually built.

---

# Stage 3: Smelting (Bloomery)

A **Bloomery** — a small clay/stone furnace, built rather than crafted-and-carried like the hand tools — takes Iron Ore and Charcoal (roughly 1:1) and produces a **Bloom**: a spongy, slag-filled lump of raw iron, not yet usable for anything. This matches how real bloomeries worked — the ore never fully melts, it's chemically reduced by carbon monoxide from the charcoal at a lower temperature than true melting, leaving behind a solid, impure mass rather than poured liquid metal.

A Bloomery is a genuinely new placed structure, Future until Building_Housing_System.md's first implementation exists (the same dependency the Hammer and Wooden Hoe already carry).

---

# Stage 4: Consolidating the Bloom (this is where the Hammer comes back in)

A real bloom is not iron you can use yet — it has to be reheated and beaten with heavy hammer blows, repeatedly, to squeeze out the trapped slag and weld the remaining iron into a solid, workable bar. This is real, physical hammer-and-anvil work, done at high heat.

This is a natural second job for the Hammer Mike already requested (Building_Housing_System.md) — "primitive building and repairing" doesn't have to be its only use. Proposed: consolidating a Bloom into usable Iron requires the Hammer equipped, at an **Anvil** (a new placed structure, see Stage 5), with the Bloomery or Forge nearby for reheating between blows — a multi-step interaction closer to the Primitive Shovel's stump-removal (repeated hits, several passes) than a single craft click.

---

# Stage 5: Blacksmithing (Forge + Anvil + Workbench)

Mike named all three of these structures directly — here's what each one actually does, grounded in the real division of labor:

- **Forge** — a charcoal-fired hearth that reheats metal to working temperature. Used for the initial bloom consolidation above, for carburizing Iron into Steel, and for reheating a tool between hammer blows while shaping it.
- **Anvil** — the hard, stable work surface metal is hammered against. Without one, there's nothing to strike the hot metal on — real blacksmithing is Forge (heat it) → Anvil (shape it) → Forge (reheat) → Anvil (shape more), back and forth.
- **Workbench** — the general assembly/finishing station: fitting a shaped Iron or Steel tool head onto a handle (Sticks, same as the primitive tools), the equivalent of the Inventory's existing Craft action but for metal tools that need a forge-and-anvil step first rather than a simple ingredient list.

Proposed flow for an Iron tool: Iron (from Stage 4) + Sticks + Cordage, worked at Forge/Anvil, assembled at the Workbench — same "stone/metal head + wood handle + cordage" logic the primitive tools already use, just with a real heating-and-shaping step in between instead of a single craft click.

---

# Stage 6: Steel

Real steel is iron with a small, controlled amount of carbon added — historically done by packing iron in charcoal and reheating it repeatedly (carburizing), then hardened by heating to a specific temperature (bright cherry-red, roughly 1,450-1,550°F) and quenching fast in water or oil, then tempering — a second, gentler reheat (300-600°F) that trades a little hardness back for toughness so the tool doesn't shatter. Skipping tempering is a real, dangerous failure mode for a blacksmith, not just flavor.

Proposed: Steel = Iron + extra Charcoal, reheated at the Forge (carburizing), then a **quench** step (a water trough, possibly just the Bucket/Water_System.md's existing water) and a **temper** step (a shorter, cooler reheat) before the tool is finished.

**Confirmed 2026-09-26 (Mike): quench is a real risk, not an automatic pass-through.** "Quench can fail or produce worse quality." So the Steel step actually needs to check something — real quenching failure comes from cooling too fast/unevenly (cracking) or getting the pre-quench heat wrong (glowing the wrong shade of red), so a reasonable first-pass mechanic: a timing or heat-reading interaction at the quench step, with a bad result either producing a lower-quality Steel tool (weaker stats than a clean quench) or, on a real miss, wasting the Iron/Charcoal that went into it entirely. Exact odds, what "worse quality" actually changes on the tool, and whether there's a full-failure outcome at all versus just a quality range, are Claude Code's call once this is buildable — this confirms the *direction* (quenching is a skill moment with a downside), not the numbers.

---

# What's Confirmed vs. Open

**Confirmed direction (Mike, 2026-09-26):** Axe, Pick Axe, and Shovel all progress Primitive → Iron → Steel; the process should use a Forge, Workbench, and Anvil; this whole chain should be researched/grounded in reality rather than invented from nothing; both proposed Iron Ore sources are in (bog iron near water, and the South Ridge vein); quenching Steel is a real skill moment that can fail or produce worse quality, not an automatic pass-through.

**Open, Claude Code's call once this is actually built:** exact ore/charcoal/tool quantities and craft times; whether Bloom is its own visible inventory item or an invisible intermediate step; the exact rarity/yield split between the two confirmed Iron Ore sources; the exact quench mechanic (timing-based, heat-reading, or something else), its odds, and what a "worse quality" Steel tool actually loses versus a clean one; and the exact Building/Housing dependency timing, since Bloomery/Forge/Anvil/Workbench are all placed structures that need Building's first real implementation to exist at all, same as the Hammer.

**Not addressed here:** the Trading Post's role in this chain (buying raw Iron/Steel, or finished metal tools, before the player can produce their own — same pattern Lumber/Boards already established in Building_Housing_System.md) is a natural fit but a separate decision for whoever builds the Trading Post catalog.

---

# Cross-References

- **Wood_Gathering_System.md** — the Primitive Axe, Pick Axe (via Stone_Gathering_System.md), and Shovel's Iron/Steel tiers all resolve through this doc's chain.
- **Stone_Gathering_System.md** — the South Ridge cliff-side deposit is this doc's proposed second Iron Ore source, reusing the existing Stone Pick Axe mining interaction rather than adding a new one.
- **Building_Housing_System.md** — the Hammer gets a second real job here (bloom consolidation); Bloomery/Forge/Anvil/Workbench are all new placed structures with the same "waits on Building's first implementation" status the Hammer already has.
- **Difficulty_System.md** — Iron/Steel tools and raw materials are natural Trading Post catalog items once that mechanic exists, same role Lumber/Boards already have.
- **Item_Data.md** — needs new Materials (iron_ore, charcoal, iron, steel, and possibly bloom as a visible intermediate) once quantities are proposed; none added yet, this doc is design-only.
- **Research.md** — see the new "Metallurgy & Mining Grounding" section for the real-world sourcing behind every stage above.

---

# Design Rules

1. Every stage here mirrors a real step in pre-industrial ironworking — mining, charcoal, smelting, consolidating, smithing, hardening — not a simplified "ore to tool" shortcut. Where a step gets simplified for playability later, that's a deliberate departure from this doc, not an oversight.
2. This doc is design-only — no numbers are locked, no Unity work is implied. It exists so the Iron/Steel tiers already promised across three other docs have somewhere real to point to.
3. Every new structure here (Charcoal Pit, Bloomery, Forge, Anvil, Workbench) is Future, blocked on Building_Housing_System.md's first real implementation, same as the Hammer and Site Preparation.


---

# Addendum 2026-11-04: v1 decisions and build plan (Mike, Claude Code read-only plan)

This addendum supersedes the ore sources and the "Future" status above for the first build. Everything above stays as the long-term picture.

## Decisions (Mike)

- Iron ore is BOUGHT at the Trading Post ($4 each, sells back $1, Stocked group). There is nothing to mine or gather on the property, so no mining, no bog iron and no South Ridge vein in v1. Real terrain digging is a separate, later feature and is not part of this.
- First version is iron only, but built so more metals (copper, tin, bronze) can be added later as rows (a MetalRules table keyed by metal) with no new kind or saved field.
- Accepted all eight of Claude Code's recommendations: one combined roofed Smithy with Forge and Anvil (like the Workshop); iron Pick Axe and Shovel are better than the stone ones (25% fewer blows; Axe and Saw unchanged); Charcoal is the only fuel (Bloomery and Forge); a smelt takes 10 game hours for up to 2 Bars; first goods are Door Hinges, Hooks and Nails to sell plus the Iron Shovel and Pick Axe for own use; no separate bloom step in v1 (the Bloomery makes Iron Bars directly); no per-visit ore limit (the price and carry weight limit it); Charcoal is sellable at $1.
- Iron's payoff beyond cash (the goods net only about $5 a day): plan a later phase M5 with building uses for iron (for example Door Hinges for a Cabin door, Cart wheel tyres, a hinged Workshop chest) after the Smithy is played.

## The chain as planned (numbers to be confirmed when built)

- Charcoal Pit: 4 Clay + 6 Tall Grass, 20 Hammer blows; open air; up to 6 Logs; 24 game hours; makes 4 Charcoal (1 kg each). 3 Logs per Iron Bar.
- Bloomery: 8 Stone + 6 Clay, 36 blows (about 61 kg); open air; up to 4 Ore + 4 Charcoal; 10 game hours; 2 Iron Bars (2 kg each). Recipe: 2 Ore + 2 Charcoal make 1 Bar.
- Smithy (Forge and Anvil, roofed shed): 6 Stone, 3 Clay, 2 Logs, 4 Branches, 6 Tall Grass, 40 blows (about 66 kg); 1 to 2 Charcoal per job; 2 to 4 game hours per job.
- Items: Iron Ore $4 (3 kg, Stocked); Charcoal $1 (1 kg, WoodStone); Iron Bar (2 kg, not sold in v1); Box of Nails (20) $16 and Hook Set $18 (1 Bar, 2 h, 1 Charcoal); Door Hinges $26 (1 Bar, 3 h, 1 Charcoal); Iron Skillet $48 (2 Bars, 4 h, 2 Charcoal, optional); Iron Shovel and Iron Pick Axe (2 Bars + sticks and Cordage, 3 h, own use). Goods are Crafted group (10% drop per unit, floor 0.1). A forged Cooking Pot is not worth making (the store sells it for $12).
- Economy (Claude Code's arithmetic, not measured): one batch a day (2 Bars) of Hinges plus Hooks earns about $34 for $16 of ore, about +$18 cash and about +$5 once the Logs burned for charcoal are valued at market; the ceiling is about $26 a day in cash for about 20 Logs a day, near the pottery ceiling of about $21 and above the $20.48 land payment, so watch it alongside the Workshop and pottery. Ore sells back for less than it costs and goods cannot be bought, so there is no buy and sell loop.
- Stations: Charcoal Pit and Bloomery are prompt-only like the Kiln (R loads, E lights or starts, E collects, Hammer dismantles); the Smithy has a screen like the Workshop. Reuse KilnSmoke and the Kiln glow. New PileKinds 39 to 44 (CharcoalPitSite 39, CharcoalPit 40, BloomerySite 41, Bloomery 42, SmithySite 43, Smithy 44; 45 kinds in total). No new saved fields (jobItem, jobHours, jobCount and contents). SaveVersion and LedgerSaveData.CurrentVersion stay at 1. No rain or wind rule in v1.

## Phases

- M1 (medium): metal rules table, Ore and Charcoal items and market rows (added by hand to MarketData.asset, never seeded), the Charcoal Pit, know-how entries, dev key. Playtest: buy Ore, build the Pit, burn Logs, collect Charcoal, sell Charcoal.
- M2 (medium): the Bloomery and the Iron Bar. Playtest a full smelt.
- M3 (large): the Smithy, its screen and the four goods. Playtest forging, selling and the income.
- M4 (small): Iron Shovel and Pick Axe with AxeTool recognising them (25% fewer blows).
- M5 (later, after the Smithy is played): building uses for iron (Cabin door hinges, Cart tyres, hinged Workshop chest). Also later: copper, tin, bronze.

Status: planned 2026-11-04, nothing built.


## BUILT: Metal M1, Iron Ore, Charcoal and the Charcoal Pit (2026-11-04, playtest 1 to 25 passed, committed e4e806f)

First phase of the iron chain. Not built yet: the Bloomery and Iron Bar (M2), the Smithy and goods (M3), the iron tools (M4).

Items: Iron Ore is bought at the Trading Post for $4 (3 kg, Stocked group, sells back $1) and is the 22nd stocked item; no per-visit limit. Charcoal is made at the Pit (1 kg, WoodStone group, sells for $1, never sold in the store; 6 Logs worth $18 make 4 Charcoal worth $4, so there is no buy loop). Market rows were added by hand to the catalog and MarketData.asset (89 rows in total, sell-back items 19; a test checks they agree); Seed MarketData was never run. Ledger: Ore files under "Bought: other goods" (one line added in Ledger.cs; "Bought: building materials", where Lumber and Cordage sit, would be a possible alternative), Charcoal sales under "Firewood, logs and stone", Ore sold back under "Stocked goods sold back".

Rules: MetalRules (Economy) is a table keyed by metal with the iron row only (ore id iron_ore, fuel id charcoal, $4, 3 kg); copper and tin are later rows. CharcoalRules holds every Pit number (build cost, blows, Logs cap, burn hours, yield, clearances, smoke).

Charcoal Pit: new PileKinds CharcoalPitSite 39 and CharcoalPit 40 (41 kinds in total; 41 to 44 stay reserved for the Bloomery and Smithy). Staged site: 4 Clay and 6 Tall Grass in any order delivered with R (14.6 kg, just under the 15 kg staging line), 20 Hammer blows, no Shovel stage. Placement follows the Kiln: open air (not under a roof), its edge 3 m clear of every structure and tree, keeps later buildings 3 m clear, no tree sprouts within 1.4 m, a parked Hand Cart is exempt; one line in Setbacks.cs now uses a structure's own radius when it has one (Kiln results identical); the mouse wheel turns the site. Prompt-only use: R (without the Hammer) loads Logs from the pack up to 6; E lights it at 3 or more Logs ("Light the pit  (6 Logs -> 4 Charcoal in 24 h)"); it burns 24 game hours one hour at a time (sleeping, being away and skipping days count; rain does not stop it); yield is Logs x 2/3 rounded down (3 gives 2, 4 gives 2, 5 gives 3, 6 gives 4); E collects as much as the pack holds and the rest stays; loading is blocked while Charcoal waits. The Hammer with R dismantles it (half the Clay and Tall Grass back, Logs and Charcoal whole), refused while it burns. Look: a clay-coloured earth mound 1.5 m across and 0.8 m high on a ring of trodden earth, with a vent on top and a draught hole at the front (8 cm clear of the mound, no z-fighting); KilnSmoke is reused with smaller settings only while a burn runs, a dim ember glow (no light, no flicker) shows at the hole, and the mound darkens while it burns. Mike's playtest passed all 25 items.

Save: no new saved field. Logs and Charcoal are ordinary stacks in contents; a burn is jobItem charcoal_burn, jobCount (the Charcoal it will make, fixed when lit) and jobHours (hours left). The pile still has 17 fields; the Hopper's hopperWater and hopperBatch are untouched. Kinds 39 and 40 appear only in new saves. SaveVersion and LedgerSaveData.CurrentVersion stay at 1.

Teaching: Know-how entries "Charcoal: Burning Logs Slowly" (unlocks on holding Charcoal or placing a Pit site) and "Iron Ore at the Trading Post" (unlocks on holding Ore), in code and in Journal_KnowHow_Text.md (a test compares them); item help lines for Ore and Charcoal; Build-tab help for the site and the finished Pit.

Dev keys: comma gives a Hammer, 4 Clay and 6 Tall Grass and stands a Wood Pile with 6 Logs 3 m ahead (6 Logs weigh 48 kg and the pack holds 45 kg, so a full load takes two trips); Shift+comma stands a finished Pit 4 m ahead with 6 Logs ready to light; Ctrl+comma finishes every running burn; equals gives 10 Iron Ore and 10 Charcoal (40 kg).

Files: 32 paths (13 modified code and data including MarketData.asset, 8 modified tests, 10 new: MetalRules.cs, CharcoalRules.cs, CharcoalPitTests.cs, iron_ore.asset, charcoal.asset plus metas, and the docs file Journal_KnowHow_Text.md), 1558 insertions and 43 deletions. Edit-mode suite 1484 of 1484 (36 new tests).

Next: M2, the Bloomery and the Iron Bar (8 Stone + 6 Clay, 36 blows, about 61 kg, open air; up to 4 Ore + 4 Charcoal; 10 game hours; 2 Iron Bars; PileKinds BloomerySite 41 and Bloomery 42).
