# Crafting System and Crafting Screen

Status: APPROVED TO BUILD 2026-10-09 (Mike: "think we need to move forward with the separate craft/build screen"). Layout numbers below are still Claude's first pass; scope updated to Craft AND Build on one screen (see Scope).

## Why this exists

**Requested 2026-10-09 (Mike, playtest):** "I'm thinking we are going to need a separate 'crafting' screen by the time we are done adding craft able items." Until now crafting has lived as a grid of buttons at the bottom of the Inventory screen. Decisions_Log.md's 2026-09-25 entry deferred a generalized Crafting system to post-Alpha, and that decision still holds for **mechanics** (no skills, no workbenches, no durability, no discovery). What changes is the **screen**: the Craft grid is already at 13 buttons in five rows, squeezing the Inventory's "Esc to close" hint, and Primitive Bow, Arrows and more are coming.

## Scope

- Moves crafting out of the Inventory screen into its own Crafting screen. The recipe data (Crafting.cs) and the missing-materials notice from Inventory_System.md are reused, not rewritten.
- **Updated 2026-10-09 (Mike, "craft/build screen"):** the new screen has two top tabs, **Craft** and **Build**. The Build tab takes over the Inventory screen's Build area (Lean-To, Tarp Shelter, Tent, Small Cabin site and "Complete Small Cabin", storage and wood piles, and any other PileKind entries), using the same category list, have/need panel and Build button. This replaces the earlier first-pass "Build stays in Inventory" and "Build tab later" text. Build rules do not change: costs, Hammer requirements, placement, the cabin completion flow (R deposits at the site, Hammer completes) and the missing-materials banner all behave exactly as built. Only where the buttons live changes. The Inventory screen keeps items, equipment and stats and loses both the Craft grid and the Build area.
- Build categories (Claude Code assigns each existing PileKind to the nearest): Shelter (Lean-To, Tarp Shelter, Tent, Small Cabin), Storage and Piles (wood piles, storage), Other as needed.
- The in-world look prompts and the R-to-take-down behavior are untouched.
- No new crafting mechanics: crafting stays instant, with no timer, skill or station, matching how every craftable works today.

## First-pass layout (Claude, not yet confirmed)

- **Opens with a dedicated key** (a free key is Claude Code's call, with an Inventory screen button as well) and closes with Esc or the same key. Pausing and cursor behavior match the Inventory screen.
- **Left: category tabs, then a scrolling recipe list.** Categories: Tools (Axe, Pick Axe, Shovel, Hammer, Knife), Weapons and Ammo (Primitive Bow, Arrows), Fire and Light (Bow Drill, Torch, Lantern), Traps and Fishing (Rabbit Snare, Box Trap, Fish Trap), Containers and Storage (Pouch), Materials (Cordage). Claude Code assigns each existing recipe to the nearest category; a recipe the player can make right now is listed first.
- **Right: the selected recipe.** Name, short description, weight of the result, and an ingredient list showing "have / need" per ingredient, green when enough and red when short, with the "Either ... or" alternatives (Cordage) shown as alternatives.
- **Craft button with a quantity** (x1, x5, Max) for stackable results such as Arrows and Cordage. The button is greyed when short, and clicking it anyway shows the existing missing-materials notice naming exactly what is lacking.
- **Counts update live** as materials are crafted. Result goes into the pack, or on the ground as a pickup if the pack is full, as other items already do.
- **All recipes visible from the start**, no discovery or unlocking, since the game has none today.
- **Hover** shows the same detail as the right panel.
- The Inventory screen loses the Craft grid, which frees the layout bug where the last row crowded the hint.

## Current recipe set to migrate

Reported by Claude Code 2026-10-09 from Crafting.cs (15 recipes, including the two new bow recipes). Category is Claude's grouping for the first-pass screen.

| Category | Output | Ingredients | Makes |
|---|---|---|---|
| Tools | stone_pick_axe | 2 Sticks, 1 Cordage, 1 Stone | 1 |
| Tools | shovel | 3 Sticks, 1 Cordage, 1 Stone | 1 |
| Tools | primitive_axe | 2 Sticks, 1 Cordage, 1 Stone | 1 |
| Tools | knife | 1 Stick, 1 Cordage, 1 Stone | 1 |
| Tools | hammer | 2 Sticks, 1 Cordage, 1 Log | 1 |
| Weapons and Ammo | primitive_bow (new, not Play Mode tested) | 1 Branch, 1 Cordage | 1 |
| Weapons and Ammo | arrows (new, not Play Mode tested) | 1 Stick, 1 Stone | 2 |
| Fire and Light | bow_drill | 2 Sticks, 1 Cordage | 1 |
| Fire and Light | torch | 1 Stick, 1 Cordage | 1 |
| Fire and Light | lantern | 2 Clay, 1 Cordage | 1 |
| Traps and Fishing | rabbit_snare | 1 Cordage | 1 |
| Traps and Fishing | box_trap | 3 Firewood | 1 |
| Traps and Fishing | fish_trap | 3 Firewood, 1 Cordage | 1 |
| Containers and Storage | pouch | 1 Deer Hide, 1 Cordage | 1 |
| Materials | cordage | 3 Tall Grass, or 2 Cattail, or 1 Sinew | 1 |

**How the current grid is built (for the migration):** InventoryScreen.cs creates one button per Crafting.Recipes entry, three to a row starting at y=584 with CraftRowHeight, through the Grid() helper. A refresh loop greys out unaffordable buttons, hover shows help text or what is missing, and a click calls Craft(), which runs Crafting.CanCraft and then Crafting.Craft. Recipes now carry an output count (added for Arrows). 15 recipes is exactly five rows, so the grid has no room left on the Inventory screen.

**Key to open the screen:** the input actions asset binds 1, 2, A, C, D, E, I, J, M, R, S, T, W, the arrows, Enter, Shift and Space; number-key hotkeys may be bound in code too. B and K looked unbound everywhere searched. Claude Code suggests B, but must check what C and T are bound to before settling (T opens the Trading Post).
## Open questions

- Which key opens it. Suggested B (unbound); Claude Code to verify C, T and the code-bound number keys first.
- RESOLVED 2026-10-09 (Mike): Build items move to this screen as a second tab in the same pass (Craft and Build together).
- Claude Code to report: the final key, how the Build tab gets its list and affordability from WoodManager.CanBuildPile and CanCompleteCabin, and whether Build placement preview (cursor/aim) needs the screen to close on click.

## Not decided here

Crafting skills, stations (workbench, forge, loom), durability and repair (Feature_Backlog.md), and recipe discovery all stay post-Alpha.