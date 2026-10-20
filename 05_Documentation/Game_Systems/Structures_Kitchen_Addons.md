# Covered Outdoor Kitchen Add-ons (design draft, Claude, not yet confirmed)

Status: SCOPE CONFIRMED by Mike 2026-10-19; rules below are drafts. NOT built. Comes after Slice 1 phase C (the Covered Outdoor Kitchen itself) is playtested and committed. First step is a read-only pass by Claude Code (questions at the end).

## Scope (Mike, 2026-10-19)

Mike's words: "you can add countertops, and cleaning tables for processing harvested plants and animals, a slop sink, shelving and a wood fired grill / griddle. but everything else should wait on the other power systems."

IN scope (this add-on set):
1. **Countertops**
2. **Cleaning (processing) tables** for processing harvested plants and animals
3. **Slop sink**
4. **Shelving**
5. **Wood-fired grill / griddle**

OUT of scope until the power systems exist: LP gas appliances, electric appliances, and anything else that needs fuel supply or power (refrigeration, electric stove or oven, lights on power, and so on). These wait on Power_System and the other power systems.

## Rules that already apply (from Mike's earlier decisions)

- **Build tab = permanent structure, Craft tab = crafted item.** Permanent pieces need the Hammer to build or tear down. Placing the ghost, delivering and taking materials back (before pounding) are free.
- **Heavy builds (over about 15 kg) become staged sites** with Hammer pounding. Light ones stay single-step.
- **No time jumps in any work.** Real time only.
- **Old saves load unchanged.** New PileKinds are appended, never renumbered. No new save keys where the existing stamps can be reused.
- Each piece gets a finished-structure ghost, Setbacks-style named refusal reasons, a Hammer Dismantle (half refund rounded down), and a playtest checklist.

## Draft rules (Claude, not yet confirmed)

**General shape.** Each add-on is a placeable permanent piece that must be placed inside or against a finished Covered Outdoor Kitchen (a short reach from the kitchen's roof footprint), so a kitchen with nothing in it is still the roof, hearth and prep table that phase C built. Whether add-ons snap to fixed slots on the kitchen or are free-placed inside its footprint is a question for the read-only pass (below). The finished kitchen has no interaction box today, so each add-on needs its own interactable.

1. **Countertop.** A work surface. First pass: carpentry cost (Logs and Branches, or planks if the game has them); light enough to be a single-step build. Its job is to give the other processing and cooking actions a place to happen under cover, and to carry the "prepared meal" rules (a meal prepared at a counter, not on the ground). Rule detail depends on what the cooking screen does today (read-only pass).
2. **Cleaning table.** A table to process what you harvest or kill: plants (wash, trim, sort) and animals (clean fish, dress and butcher game, pluck birds). Real basis: fish cleaning tables are the standard way to process a catch; processing at a clean raised surface keeps meat clean, and the real-world rule is that the cleaning surface and tools are washed after each use. First pass: processing at the table needs a Knife for animals (matching the Knife rule for traps), runs in real time (hold click, like digging and pounding, no clock jump), and produces the same cleaned products the game already makes. Whether processing at the table is better than processing in the field (more yield, less spoilage, cleaner food) is a design choice; see decisions.
3. **Slop sink.** A basin for washing and for dumping dirty water. There is no plumbing, so first pass is a basin you pour water into (from a carried Bucket or a Water Barrel), and the dirty water drains out to the ground. Washing produce or hands at it counts as clean prep. Greywater handling (reuse, a drain field) is on the earlier backlog and is NOT part of this set; the slop sink just drains.
4. **Shelving.** Storage for food, jars and kitchen goods, like the Storage Bin but kitchen-scoped and held under the roof so contents are kept dry. It uses the same transfer screen as the other storage. Capacity and item rules to be decided after the read-only pass.
5. **Wood-fired grill / griddle.** A cooking surface that sits at the kitchen hearth or beside it and burns wood like a Campfire (same wet/dry batch rules, same burn times, same Flint and Steel and Bow Drill lighting). Grill = direct heat for meat and fish; griddle = flat surface for flatbread, eggs and vegetables. Whether these are a second cooking station or a recipe-set option at the existing hearth is a question for the read-only pass. If the game has no metal items (iron or steel), the first pass uses stone or clay for the surface and the metal grate version waits.

## What stays untested / unknown

All of it. Nothing is built. Every number above is a first-pass guess to be replaced by the read-only findings.

## Decisions needed from Mike (after the read-only pass)

- Add-ons snap to fixed slots on the kitchen, or are free-placed anywhere inside its footprint?
- Is processing at the cleaning table better than processing in the field (yield, spoilage, illness risk), or just a nicer place to do it?
- Slop sink: a basin you pour water into (no plumbing), drains to the ground. OK for now?
- Grill and griddle: a second cooking station beside the hearth, or extra recipes at the hearth itself?
- Build order inside this set (suggested: countertop and shelving first, then the cleaning table and slop sink, then the grill/griddle).

## Questions for the Claude Code read-only pass

1. **Kitchen and attachment:** how does the finished Kitchen (kinds 22-24, `PlacedStructure`, `WellsAndOuthouses` staged-site data) expose its footprint and its hearth position? What would it take for other structures to be placed inside or attach to it (slots, a footprint test, or a parent link), without new save keys?
2. **Processing today:** how does the game process harvested animals and plants now (dressing game with the Knife, cleaning fish, plucking, butchering, cleaning plants)? Where is it implemented, does it take game time or real time, what does it produce, and what does the Knife rule check? Is there any "cleanliness" or spoilage state on meat, fish or produce that a clean surface could affect?
3. **Cooking:** how does the Cooking screen find a heat source today (Campfire, cabin hearth)? Could a grill or griddle be a heat source of its own, or a recipe filter on the hearth? Which recipes exist that would belong on a grill versus a griddle?
4. **Water for the sink:** how does a Bucket or Water Barrel hand over water, and could a basin accept and drain it? What does washing hands or produce affect today, if anything? Anything existing about dirty water or greywater?
5. **Storage:** what does the Storage Bin and its transfer screen allow, and is there a spoilage or dryness concept that shelving under the roof could use?
6. **Materials:** is there a plank or lumber item, and any iron or steel, in the item list today? What do Logs, Branches, Stone and Clay give, and what are their weights (so each add-on stays at or under the 15 kg single-step line or becomes a staged site)?
7. **Interaction and ghosts:** what does it take to give each add-on a ghost, an interactable and a Dismantle consistent with the existing staged sites? Which existing scripts would change?
8. **Risks:** anything in phase C's finished kitchen (colliders, the hearth, the roof raycasts for shelter and shade, the Bow Drill cover test) that add-ons placed under the roof could break?
