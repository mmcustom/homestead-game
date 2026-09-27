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
