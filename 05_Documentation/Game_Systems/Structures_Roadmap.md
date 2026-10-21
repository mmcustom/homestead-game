# Homestead
## Structures Roadmap v0.1

UPDATE 2026-10-20: Slice 1 (Well, Outhouse, Wood Shed, Covered Outdoor Kitchen) and kitchen add-ons batch 1 are built, playtested and pushed (commit 493e35c). Mike's vision update (Homestead_Vision_and_Design_Pillars, GDD v2.0 Land Purchase and Debt) adds a bare money system; see step 1b under "Proposed order" and `Money_System.md`. The rest of this document is unchanged.

Status: Design Draft (roadmap only, nothing built). Requested 2026-10-19 (Mike). Everything below is Claude's first pass and is NOT yet confirmed by Mike. No numbers are set yet; this document sorts the structures, shows what each one depends on, and proposes an order. Real-world research and numbers come when each group is detailed.

---

# Purpose

Mike's request (2026-10-19): "some new buildings and structures. Early structures would include a well, an outhouse, covered firewood storage, water storage indoors or water storage structures to prevent freezing, Greenhouses for growing food in the winter time, covered outdoor kitchen and dining areas, composting bins, straw and hay for building and livestock feed and everything else that builds towards our goals of being self sustaining and eventually profiting from the homestead."

This roadmap does three jobs: (1) gathers every structure that has been named anywhere in the docs plus the new ones from that request, (2) shows what each one needs before it can exist, and (3) ties each one to the self-sufficiency and income loop in Economy_System.md. It is not a build spec. Each group gets its own detailed design draft, checklist and Claude Code read-only pass later, same as every system so far.

The GDD's Development Rule held back Stage 2 to 4 design until the core survival loop was proven fun. That gate was met on 2026-10-11 (Winter Preparation playthrough, 25 of 25). Mike's request is treated as the go-ahead to design these structures. Designing is open; building waits for each group's own go-ahead.

---

# The loop every structure should serve

Economy_System.md already sets the order: Personal Use, then Stored Reserves, then Livestock Needs, then Surplus, then Market Sales. The homestead comes first, and the economy "should never become the primary objective." So each structure below should do at least one of these jobs:

1. **Meet a need** (water, sanitation, warmth, shelter, food).
2. **Protect a reserve** (keep firewood dry, keep water from freezing, keep food from spoiling, keep feed from rotting).
3. **Produce a surplus** (food, eggs, milk, hay, firewood, lumber) that can be sold.
4. **Cut a cost** (grow it instead of buying it, make it instead of buying it).

A structure that does none of these is decoration and can wait. The revenue tiers already in Economy_System.md (Early Survival, Early Homestead, Established, Modern) say what gets sold when; the structures here are what let the player reach each tier.

---

# What already exists (so nothing is designed twice)

**Built and playtested:** Tarp Shelter, Tent, Lean-To, Small Cabin (with hearth, Hammer Dismantle), Wood Pile, Rock Pile, Water Barrel (40 L), Rain Barrel (80 L, eave-fed), Food Cache, Storage Bin, Tool Rack, Campfire, Placement Preview (every Build-tab item and every trap), Pond Drying, creek flooding.

**Named in Building_Housing_System.md but design-only, no detail:** Root Cellar, Smokehouse, Storage Shed, Large Cabin (with indoor water and a primitive kitchen), Cottage, Barn, Chicken Coop, Goat Shelter, Rabbit Hutch, Well House, Windmill, Solar Shed, Farmhouse, Workshop, Equipment Buildings. The Cistern is a concept in Water_System.md, and Well Drilling and Cistern Construction are listed as economic sinks in Economy_System.md.

**Mentioned but open on purpose:** indoor plumbing raises the sewage and composting question; Mike's 2026-09-26 instruction was to decide it later. This roadmap is where that comes due.

**Not mentioned anywhere yet:** outhouse, covered firewood storage, covered outdoor kitchen and dining area, composting bins, greenhouse or cold frame, straw and hay as a made-and-stored resource.

---

# What the structures depend on (gaps to design first or alongside)

Several structures cannot do their job until a supporting system exists. These are the real gating items, more than the structures themselves:

| Missing system | What it blocks | Status |
|---|---|---|
| Winter water pressure and surface freezing | Frost-proof water storage, Rain Barrel freezing, Cistern | Wanted later by Mike, not designed |
| Spoilage, smoking and drying | Root Cellar, Smokehouse, Storage Shed benefits | Not built (items record their acquired day, nothing ages them) |
| Gardening / Farming | Greenhouse, composting, garden beds, irrigation | Not designed; GDD Development Rule now open |
| Livestock quantities (feed per animal, winter reserves) | Hay storage, coop, hutch, goat shelter, Barn | Livestock_System.md is a draft with no quantities |
| Hay and straw work loop (cut, dry, stack) | Hay and straw as a resource | Not designed |
| Sanitation hook in Health | Outhouse (what it actually does) | Not designed |
| Firewood moisture or seasoning (if wanted) | Covered firewood storage (what the roof protects against) | Open question, see below |
| Greenhouse temperature model | Winter growing | Would reuse the weather and body-heat work; not designed |
| Staged build sites for multi-part structures | Greenhouse, Barn, Smokehouse, Root Cellar | The Small Cabin's staged CabinSite is the precedent |

---

# The structures, in groups

Each row says what the structure does for the player and what it needs. "Needs" is what must exist first. "Loop role" uses the four jobs above.

## Group 1: Survival comforts that work with systems already built

These need little or nothing new underneath them, so they can be designed in detail and built first.

| Structure | What it does | Needs | Loop role |
|---|---|---|---|
| **Well** (hand-dug first, drilled later) | A steadier water source than a pond or the creek: it does not flood or evaporate away, but a shallow dug well can run low in a long drought; quality set by how well it is protected and how close waste is | Water quality system (built); Shovel and stone/timber lining; Ohio groundwater research | Meet a need; drilled well is a money sink (Economy_System.md) |
| **Outhouse** | Gives waste somewhere to go; keeps the camp and water safe | A sanitation effect in Health (new, small); placement rule keeping it away from the well and water | Meet a need; protects water quality |
| **Covered firewood storage** (wood shed) | A Wood Pile with a roof, so fuel stays dry in rain and snow | Wood Pile (built); a decision on whether wood gets wet or seasons | Protect a reserve; stored firewood is later sellable |
| **Covered outdoor kitchen and dining area** | A roofed cooking and eating space, so a campfire can be used and meals eaten in rain | Cooking and fire (built); do rain and wind affect campfires today (Claude Code to check) | Meet a need; comfort bonus for eating under cover |
| **Water storage that stays out of the cold** (indoor barrel, insulated or buried tank) | Keeps stored water liquid in Winter | Winter water pressure and freezing (NOT built) | Protect a reserve |
| **Cistern** | The big upgrade to the Rain Barrel and Water Barrel | Winter freezing; Building tier; cost sink | Protect a reserve; money sink |

Note: the frost-proof storage and Cistern rows cannot be finished until the Winter water system is designed. Recommended: design Winter water pressure and freezing as its own group right after Group 1, so these two land with it.

## Group 2: Reserves (needs spoilage first)

| Structure | What it does | Needs | Loop role |
|---|---|---|---|
| **Root Cellar** | Cool, stable storage; slows spoilage of produce and preserved food | Spoilage system; staged build site | Protect a reserve |
| **Smokehouse and drying rack** | Long-term meat and fish storage | Smoking and drying system | Protect a reserve; preserved goods are sellable |
| **Storage Shed** | Dry-goods upgrade of the Storage Bin; larger and weatherproof | Staged build; tier | Protect a reserve |
| **Pantry or larder** (inside the cabin) | Cabin-level food storage | Large Cabin tier | Protect a reserve |

## Group 3: Growing

| Structure | What it does | Needs | Loop role |
|---|---|---|---|
| **Garden beds and fence** | The place food is actually grown; the base for everything below | Gardening system (new); seeds; Plants docs; seasons (built) | Produce surplus; cut cost |
| **Composting bins** | Turn kitchen scraps, animal offal, plant waste and later manure into soil amendment | Gardening system; sanitation rules; Hunting_System.md already mentions compost for offal | Cut cost; feeds garden yield |
| **Cold frame, then hoop house, then greenhouse** | Winter and early-spring growing. Stage 1 cheap and low, Stage 3 framed and glazed | Gardening; weather and temperature model; a way to vent in a Heat Wave (the Heat Wave system already exists) | Produce surplus in the hard months; cut cost |
| **Irrigation and rain-fed watering** | Gets Rain Barrel water to the beds | Rain Barrel (built); Gardening | Cut cost; ties rainwater to food |
| **Seed saving** | Keeps next year's seed | Gardening; spoilage | Cut cost |

## Group 4: Livestock and feed

| Structure | What it does | Needs | Loop role |
|---|---|---|---|
| **Chicken coop, rabbit hutch, goat shelter** | Housing for each animal (named in Building_Housing_System.md) | Livestock system and quantities | Produce surplus (eggs, milk, meat) |
| **Hay and straw storage** (covered hay shed or loft) | Keeps hay and straw dry through winter; the reserve that livestock depends on | Hay and straw work loop; Livestock feed quantities | Protect a reserve; meet livestock needs |
| **Hay and straw as a made resource** | Hay as feed; straw as bedding, insulation and building material | A field or meadow, a cutting tool, drying time, storage | Meet livestock needs; hay is sellable |
| **Barn** | Shelter, feed and equipment in one building | Livestock; staged build; tier | Protect a reserve; meet livestock needs |
| **Fencing and pasture** | Keeps animals where they belong, and out of the garden | Livestock; Property_Layout | Meet livestock needs |

## Group 5: Income and scale

| Structure | What it does | Needs | Loop role |
|---|---|---|---|
| **Workshop** | Tool repair and a future crafting space | Tier; tool wear system if wanted | Cut a cost |
| **Lumber Mill** | Turns logs into lumber; sellable | Wood species tiers; already named in source docs | Produce surplus (lumber is Established Homestead revenue) |
| **Sugar Shack** | Maple sugaring (Maple_Sugaring.md already specs it) | Maple trees; Winter | Produce surplus (Modern Homestead revenue) |
| **Dairy and cheese room, apiary** | Value-added goods | Livestock; later systems | Produce surplus (Early Homestead and Modern revenue) |
| **Market or farm stand** | A place to sell surplus; today the Trading Post is the only buyer | Economy; Trading Post | Market sales |

---

# How structures get built (proposal)

The framework already exists: the Craft and Build tab, Placement Mode with its green/red ghost and snapping (phases 1 to 6, all playtested), and Hammer Dismantle for removal and refunds. New structures should reuse all of it.

- **Small, single-piece structures** (outhouse, well, firewood shed, kitchen shelter, compost bin, cold frame, hay shed) use the existing pile-style build: spend materials, place with the ghost, done.
- **Multi-part or large structures** (greenhouse, Barn, Smokehouse, Root Cellar) use the Small Cabin's staged site: place a site, stock materials, complete it. That keeps the placement rules and the save format consistent.
- Every placement rule is reasoned from the real world where it matters: wells and outhouses keep a distance, a greenhouse wants sun and level ground, a firewood shed wants good airflow, and none belongs in the floodplain (the creek flood system already shows where that is).
- Costs avoid Cordage until it has a source (the long-standing shortage flagged in Primitive_Storage_System.md).

---

# Proposed order (Claude, not yet confirmed)

Order follows what each group can stand on, not how exciting it is:

1. **Slice 1, Dry and clean (Group 1 without the freeze part):** Well, Outhouse, covered firewood storage, covered outdoor kitchen and dining area. All work with systems that are built today. Needs a short Claude Code read-only pass first (do rain and wind affect campfires and cooking today, is there any wet-wood idea in the code, how the Wood Pile would take a roof).
1b. **Money system, bare version (added 2026-10-20, proposed placement, not yet confirmed):** the land payment, the loan balance, the extra payment and the seasonal account statement, with the late fee and the three-missed-payments loss. It needs no new structures, so it can land right after Slice 1 and before Winter water. Detail is in `Money_System.md`. Mike's vision (2026-10-20) has the land payment due from the start, so it should not wait for Slice 6.
2. **Slice 2, Winter water:** design Winter water pressure and surface freezing, then frost-proof storage, Rain Barrel freezing and the Cistern. This also finishes the open "Rain Barrel freezing with the Winter water system" item.
3. **Slice 3, Reserves:** spoilage and smoking/drying (already on Mike's later list), then Root Cellar, Smokehouse and Storage Shed so they have something to do.
4. **Slice 4, Growing:** Gardening basics designed first, then garden beds, composting, cold frame to hoop house to greenhouse, and irrigation from the Rain Barrel.
5. **Slice 5, Livestock and feed:** livestock feed quantities, the hay and straw work loop, then coop, hutch, goat shelter, hay storage and the Barn.
6. **Slice 6, Income and scale:** Lumber Mill, Sugar Shack, Workshop, dairy, apiary, market stand, Large Cabin and up.

Why this order: Slice 1 is cheap and fixes real gaps today (dry fuel, safe water, a roofed cooking space, somewhere for waste). Slice 2 closes a known open item and unlocks the freeze-proof water Mike asked for. Slices 3 to 5 each need a missing system designed first, so they are designed together with that system and not as stand-alone buildings. Slice 6 is the profit end of the loop and only works after the earlier slices produce a surplus.

---

# The self-sufficiency and profit thread

How the structures connect to each other, so the homestead feels like one machine and not a list of buildings:

- **Water loop:** Rain Barrel and Cistern (rain) plus Well (groundwater) give water; frost-proof storage protects it; irrigation sends it to the garden; the outhouse keeps it clean.
- **Fuel loop:** trees to logs to covered firewood storage to the hearth, stove and kitchen; surplus firewood is Established Homestead revenue.
- **Food loop:** garden and greenhouse produce food; composting returns scraps and waste to the garden; Root Cellar and Smokehouse hold it through winter; surplus produce is Early Homestead revenue.
- **Livestock loop:** hay and straw (made and stored) feed and bed the animals; manure feeds compost; eggs, milk and meat feed the household and the market.
- **Money loop:** surplus sells at the Trading Post (later a market stand), and the money pays for the sinks already listed (Well Drilling, Cistern Construction, building materials, livestock, equipment).

The design check for any new structure: which loop does it join, and what does it make, protect or save?

---

# Open questions for Claude Code read-only passes

For Slice 1:
1. Do rain and wind affect a lit campfire or cooking today? Is there any "wet" state on wood, firewood or fuel?
2. How would the Wood Pile take a roof: a variant of the same type or a new type, and what would the roof protect against in the code?
3. Does anything read "sheltered" for the player or for a fire (the Tent, Lean-To and Tarp Shelter already give cover)? Could a roofed kitchen reuse that?
4. How are Water sources built (types, quality, save data), so a Well can join the same system and never dry out?
5. What does Health have today that a sanitation effect could attach to (illness risk, morale, comfort)?
6. Placement rules for new structures: terrain slope, floodplain avoidance (the creek flood zone), distance rules between wells and outhouses.

For later slices (not yet needed): how crops and plants are modeled today, what the Plants docs already say about gardening, how Livestock is stored, and what the weather and temperature code would need to give a greenhouse its own inside temperature.

---

# Research to do when a group is detailed

Real-world facts and sources should back each group's numbers. Not yet researched:

- **Well:** hand-dug versus drilled wells in Ohio, typical depth around Bethel, water quality from shallow wells, required distance from septic and outhouse.
- **Outhouse:** Ohio rules and sensible setback distances from wells and water, vault versus pit, how a privy affects water safety.
- **Firewood:** how long cut wood takes to season, why a roof and airflow matter, and what burning wet wood does.
- **Outdoor kitchen:** what a roofed, vented cooking space needs; safe fire clearances.
- **Frost-proofing water:** Ohio frost depth, how deep to bury a tank, how insulated or indoor storage behaves in a cold snap.
- **Greenhouse:** unheated and low-tech winter growing in Ohio (cold frames, hoop houses), which crops tolerate cold, plastic versus glass, summer overheating and venting.
- **Composting:** carbon and nitrogen balance, time to finished compost, what should not go in a bin, pests, and what happens with manure.
- **Hay and straw:** hay and straw yield per acre, how much a goat, rabbit or chicken needs per winter (also closes the open Livestock feed quantities gap), curing and storage, fire and mold risk.
- **Economy:** what small homesteads actually sell and what it earns, to set realistic prices in the Trading Post.

---

# Decisions for Mike (Claude's recommendations in bold)

**CONFIRMED 2026-10-19 (Mike: "Yes, use all seven"). All seven below stand as written. Next step: the Slice 1 design draft (`Structures_Slice1_Dry_And_Clean.md`, written 2026-10-19), then a Claude Code read-only pass.** Correction made the same day: the Well row above first said a well "never dries"; real dug wells (10 to 30 ft deep) can run low in a long drought, so it now says that. Detail is in the Slice 1 draft.

1. **Order:** start with Slice 1 (Dry and clean), then Winter water, as above. **Recommended.**
2. **Well path:** a hand-dug well first (labor, Shovel, stone and timber lining), with a drilled well as a later money sink, since Economy_System.md already lists Well Drilling as one. **Recommended.**
3. **Outhouse effect:** a real but small sanitation effect (it keeps water and the camp safer, and having one improves comfort), not just a decoration. How big is for the Slice 1 design draft. **Recommended.**
4. **Firewood and wet wood:** decide whether rained-on and unseasoned wood burns worse. If yes, the covered firewood shed has a real job; if no, it is a comfort and capacity structure. **Recommended: yes, a light version (wet wood burns shorter or needs seasoning time), to be checked against what Claude Code finds.**
5. **Multi-part builds:** big structures use the Small Cabin's staged-site method. **Recommended.**
6. **Greenhouse:** staged as cold frame, then hoop house, then a framed greenhouse, with summer overheating tied to the Heat Wave system. **Recommended.**
7. **Hay and straw:** treated as a real work loop (a field, a cutting tool, drying time, covered storage), designed together with Livestock feed quantities, not as an item that just appears. **Recommended.**

---

# Cross-References

- Building_Housing_System.md: tiers, Storage, Animal and Utility structures, Large Cabin and the deferred sewage and composting note.
- Primitive_Storage_System.md: the pile-style containers the new storage structures upgrade.
- Water_System.md and Water_Sustainability_Research.md: Rain Barrel, Pond Drying, creek floods, Cistern, aquaponics greenhouse research.
- Economy_System.md: the production order, revenue tiers and economic sinks.
- Livestock_System.md and the Livestock sheets: feed needs and shelter requirements.
- Power_System.md: Well House, Windmill, Solar Shed and the Large Cabin well pump.
- Development_Roadmap.md: status and next steps.
