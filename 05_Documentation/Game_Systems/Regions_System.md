# Homestead
## Regions System v0.1

Status: Design Draft (PROPOSED). Written 2026-10-23 from Mike's request of the same day. Nothing is built, no code has been read yet, and nothing here has been researched except what the earlier Bethel, Ohio land-price work already covers. Every rule marked DECIDED is Mike's. Everything marked PROPOSED is Claude's first pass.

Related: `Money_System.md` (land prices, savings, the loan), `Season_System.md`, `Weather_System.md`, `Water_System.md`, `Wildlife_System.md` and the wildlife tier docs, `Difficulty_System.md`, `Wood_Gathering_System.md`, `Stone_Gathering_System.md`, `Trading_Post_Market.md`, `Homestead_GDD_v2.0.md`.

---

# Purpose

Today the game has one place: Mike's own region around Bethel, Ohio. The player should be able to choose what part of the country the property is in, and the climate, the geography, the resources and the environment should change with that choice. A homestead in the Pacific Northwest, the high desert and the north woods should feel like different games to survive and to farm, and each one should make the land loan feel different (cost of land, length of the growing season, how much the land gives for free).

---

# What Mike decided (2026-10-23)

| Question | Decision |
|---|---|
| How many regions | THREE TO FIVE regions from the start, so there is a real choice at New Game. |
| What changes with the region | ALL FOUR: climate and weather; plants, trees and crops; wildlife and fish; land prices and terrain. |
| How the land differs | GENERATED MAPS: a new map is generated for every new game, seeded with the regional settings (not a hand-built map per region and not one fixed map). |

---

# Proposed regions (PROPOSED, Mike to confirm or change)

Five regions chosen to differ as much as possible from each other, so each one forces different play. Real land prices for the four new regions still have to be researched the same way the Ohio numbers were (sources recorded in the GDD).

| Region | Real-world model | What makes it different |
|---|---|---|
| 1. Ohio Valley (the baseline) | Bethel, Ohio and the surrounding area | The game as it exists today: four clear seasons, hardwood forest, creeks and ponds, white-tailed deer, turkey, bass, bluegill and catfish. Land: $12,000 an acre on the 5-acre parcel (from the Ohio research). |
| 2. Appalachia | Eastern Kentucky and West Virginia | Steep hills and hollows, little flat ground, hardwood and hemlock, many small creeks, black bear, trout, ginseng. Cheaper land, harder building sites, harder farming. |
| 3. Pacific Northwest | Western Oregon and Washington | Mild, very wet winters and dry summers, giant conifers (Douglas fir, cedar), salmon and steelhead, elk and black-tailed deer, mushrooms, huge timber, a long growing season. The most expensive land, the most free resources. |
| 4. High Desert Southwest | Northern New Mexico and Arizona | Arid, big day and night swings, scarce water (wells matter), pinyon and juniper, mule deer, pronghorn and rabbits, summer monsoon with flash floods. Cheap raw land per acre, expensive water, thin forage. |
| 5. North Woods | Northern Minnesota and Wisconsin | Long hard winters and a short growing season, lakes, spruce, pine and birch, ice fishing, walleye and pike, wild rice, heavy firewood demand. Moderate land prices, hardest Winter. |

Not in the first version (candidates for later): Gulf Coast and Deep South, Rocky Mountains and Montana, Great Plains, Northeast and New England, California.

---

# What a region changes (PROPOSED)

A region is a data asset (a `RegionData`, like `MarketData`) that every system reads. A region does not add rules; it changes numbers and lists that already exist.

## Climate and weather

- Average temperature and its swing by season, rain and snow chances, storm and flood risk, wind, fog.
- Frost-free growing season (which days crops and forage can grow) and day length by season.
- The existing Weather system's weights become the region's weights (the Weather weights array in the Bootstrap scene is already on the backlog to check).
- Whether the game year still uses four seasons of 28 days in every region (PROPOSED: yes; the region changes what each season is like, not the calendar).

## Plants, trees and crops

- Which tree species grow, how dense, how big, and what they yield (firewood, logs, branches).
- Which forage, berries, nuts, mushrooms and medicinal plants exist and in which seasons (the forage patch list per region).
- Which crops can be grown and how long they take (the farming system is not built yet, so this is a hook for later).
- Stone and clay amounts and where they sit.

## Wildlife and fish

- Which game, birds, fish and predators live there, their numbers, seasons, yields and prices.
- The Trading Post market groups and seasonal targets can differ by region (for example firewood is worth more in the North Woods Winter).

## Land prices and terrain

- Real regional land prices for the 5, 10 and 40 acre parcels, and therefore different loan payments. Savings by tier stay the same rule (20%, 40%, 0%, capped at $30,000).
- Terrain character: hills or flat, water features (creek, pond, lake, river, none), soil, ground cover, elevation range, bare rock, how much of the parcel is buildable.
- A region difficulty rating shown at New Game (PROPOSED, not decided).

---

# Generated maps (PROPOSED, built from Mike's decision)

- Every new game generates a new map from a SEED and the region's terrain settings. The same seed and region always produce the same map, so a save stores only the region id, the seed, and the changes the player made, not the map.
- The generator places everything the game already needs on a parcel: the land shape, creeks, ponds and other water, trees and their species, stone, clay, forage patches, the player's start point and the parcel boundary (5, 10 or 40 acres).
- Restart (Phase C) uses the same region, parcel and difficulty with a NEW seed (a fresh world).
- Old saves (the hand-built Bethel world) keep working unchanged. They load as Ohio Valley "classic map" and never regenerate.
- The risk is the biggest in the whole feature: a generator can produce unfair or unbuildable parcels. It needs rules (a minimum amount of flat buildable ground, water within reach, a minimum of trees and stone) and a way to test many seeds automatically.

---

# Staged plan (PROPOSED)

Mike chose three to five regions at once, so the plan is to DESIGN all of them first and BUILD them in stages, so each stage is playable and testable.

0. Claude Code read-only pass on how the world is built today (see the question list below), then Mike's answers.
1. Region foundation: `RegionData`, a Region step at New Game, the region saved with the game, regional land prices and loan payments, regional climate and weather numbers. Ohio Valley only is fully filled in, with the others as stubs that reuse Ohio data. The existing world is still used.
2. Terrain generator, one region: a seeded generator for Ohio Valley that places every gameplay hook, with a test that checks many seeds for fair parcels. Replaces the hand-built map for new games.
3. Region content, one region at a time: terrain settings, trees and plants, wildlife and fish, market tweaks, regional look. Each region is its own build, playtest and commit.
4. Regional polish: the Region step shows climate summaries, price and difficulty, and the Statement and Trading Post reflect regional prices.

Order of the regions after Ohio Valley is open (PROPOSED: Appalachia, then Pacific Northwest, then North Woods, then High Desert, because the desert needs the water system to change the most).

---

# Open questions for Mike

1. Confirm the five regions, or swap any (for example Gulf Coast or Rocky Mountains instead of Appalachia).
2. Does the calendar stay four 28-day seasons everywhere (PROPOSED yes)?
3. Is there a region difficulty rating at New Game, and does it change the difficulty tiers' savings or start (PROPOSED: shown for information only; difficulty stays separate)?
4. Are new species and models per region acceptable to add one region at a time (this is the biggest content and art cost: every region needs its own trees, plants, animals and fish)?
5. Where does this sit against Alpha 0.1 and the rest of the roadmap?

---

# Questions for the Claude Code read-only pass

1. How is the world built today: hand-placed in the scene, generated by script, or both? What is the terrain (a Unity Terrain, a mesh, tiles)? How are trees, stone, clay, the creek, the pond and the property boundary placed, and what are their counts?
2. Which systems are tied to specific places in the current map (the creek crossing, the pond, ground wood, structure placement and Setbacks, the player start, the Trading Post, wildlife spawns, forage patches)?
3. How data-driven are weather, seasons, forage, wildlife and the Trading Post market today (ScriptableObjects, constants in code, scene values)?
4. How are saves tied to the world: what does a save store about trees, stone, structures and the terrain, and what would happen if the world were generated from a seed and region id?
5. What would a region choice have to touch at New Game (LandPurchasePanel, LandRules, DifficultyManager, MainMenuController, GameManager, NewGameSetup)?
6. Performance and tooling: how large is the playable world, what is the HDRP terrain budget, and what can run at load time versus needing a baked step?
7. Size and risk: a rough count of files and the riskiest parts, and a recommendation on whether the generator should be a new terrain or a modification of the existing map.
