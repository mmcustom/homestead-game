# Homestead
## Vehicles and Hauling v0.1

Status: Design Draft (PROPOSED), written 2026-10-30. Nothing is built, and nothing here is decided except the answers listed below. Every number is a first pass for Mike to cut down.

# Why this exists

Mike (2026-10-30): "We also need to start thinking about a UTV for getting around faster and carrying more resources." His measuring runs said the same thing in another way: the limit on early income is the player's time ("not enough time in the day", food and water chores eating the Winter), and the Maple run showed a strong first week followed by almost nothing once chores took over. Walking and hauling are part of every chore: logs, stone, clay, water, sap, furs. A vehicle converts money into time. (The Trading Post is a menu with no location, so selling costs no walking.)

## Answered by Mike (2026-10-30)

1. **How the vehicle arrives:** a hand cart first, a UTV later (a late purchase at the Trading Post with fuel and maintenance costs).
2. **Build order:** not decided. The workshop (furniture, pottery wheel) and this vehicle design are all written first, then the order is chosen.

# What the game has today (so nothing is designed twice)

- **Carry limits:** hard cap 45 kg (55 kg with the Pouch), Encumbered above 30 kg (Skills_and_Progression.md). The Hauling skill raises both by 3, 6 and 10 kg at 3, 10 and 30 km walked carrying 15 kg or more.
- **Weights:** Log 8 kg, Stone 5 kg, Clay 3.5 kg, Branch 1 kg, Tall Grass 0.1 kg. A Cabin site trip, a Well or a Kitchen all needed several trips to deliver because of these limits.
- **Storage pattern:** Storage Bin and Food Cache are placed objects with a weight cap, which a cart bed can reuse.
- **Economy hooks:** Economy_System.md already lists "Vehicle Maintenance" as an expense category (a placeholder, not built). The Loan system exists and could finance equipment.
- **Water danger:** Water_System.md and Water_Sustainability_Research.md already quote the real warning that about 2 feet of fast water can carry away a vehicle, and a creek flood zone exists. A vehicle gives the flood zone a real risk.
- **Horses:** Livestock_System.md has "Horses: transportation, work animals" as a placeholder. A horse or mule with a cart is a parallel path to the UTV and is NOT designed here.
- **Not known yet (Claude Code must measure):** the player's walk and sprint speed, how big the walkable world and the owned parcel are, how far the cabin is from the nearest Maples, the creek and the Trading Post, and how the first-person controller would hand over to a vehicle.

# Step 0: a time audit (DONE 2026-10-30, results in the last section)

A vehicle only helps if travel is a real part of a day. Before building, have Claude Code measure, from the code and a short play session, how many game minutes the usual chores take, split into walking, working and waiting. The list: fell and carry a tree's logs to the cabin, a Maple tap round (12 taps), a water trip, a snare round, a Trading Post trip from the cabin, a Well or Cabin delivery trip. If walking is under about 15% of the day, a UTV is a luxury and the hand cart is enough. If it is 40% or more, the UTV is a real fix. This also gives the numbers for the "food and water take too long" problem.

# The three-step path

## Step 1: Hand cart (craftable, early)

- **What it is:** a two-wheeled cart you push by the handles. It has its own load (separate from your pack), you steer it with your own movement, and you leave it where you stop.
- **Parts:** a **Wooden Wheel** (1 Log, 2 h at the Woodworking bench) makes this depend on the workshop (W1). First-pass cart: 2 Wooden Wheels, 3 Logs, 6 Branches, 3 Cordage, built at the workshop (a staged site, a few Hammer blows). About 40 kg.
- **Load:** about 100 kg (12 Logs, or 20 Stone), in the cart, not the pack. Use the Storage Bin pattern: E takes and puts, a weight cap, a prompt showing what is in it.
- **Pushing:** speed about 75% of walking when loaded, 100% empty. You cannot push it up steep slopes, through deep water, or while sprinting. While pushing you cannot use tools. The player does not count as Encumbered for the load in the cart, only for the pack.
- **Why it is worth it:** a full cart is 12 logs in one trip instead of 3. A Bed (32 kg of logs plus branches) and the Cabin's 20 Logs become one or two trips. It does not help with selling, because selling happens in the Trading Post window with no walking.
- **Wheelbarrow variant:** a one-wheel version at half the capacity (50 kg), cheaper (1 Wooden Wheel, 2 Logs, 4 Branches, 2 Cordage), usable on rougher ground. Recommended as the very first step if Mike wants two cart tiers.
- **Size:** roughly 300 to 450 lines (the cart object, push control, storage, save data, tests), plus the Wooden Wheel recipe.

## Step 2 (optional, parallel): horse or mule and cart

Not designed here. It fits the Livestock plan and would give a slower, living version of the UTV with feed and care costs. Listed so the UTV is not the only road.

## Step 3: UTV (late purchase)

### Real-world reference (research 2026-10-30)

- ATV.com (published July 29, 2025, before the Ranger 500 went on sale) reported the **2026 Polaris Ranger 500**: set to go on sale in August 2025 at **$9,999**, a 28 hp single-cylinder engine ("essentially a detuned variant of the 570"), **40 mph** top speed, **1,500 pounds towing**, a **300-pound dump bed** (it says it carries 10 bags of mulch and two people), a 2,500-pound winch, 10 inches of ground clearance. Source: https://www.atv.com/atv/polaris-reveals-2026-ranger-500-pitches-value-for-under-10k-45130159
- That price is the announcement price, not a current retail price. The Polaris 2026 model page did not load, so the full-size Ranger 1000 and other model prices, payload and fuel use were not found. Bigger models cost more. I did not find fuel use or tank size, so the fuel numbers below are game balance, not real figures.
- Real UTVs are priced in the $10,000 range new, which is about 11 years of Mike's current Pioneer income ($8 a game day, about 1,250 game days). So the price is a real design problem (see decisions).

### Game version (proposal)

- **Acquire:** buy at the Trading Post, late. Price options (decision V2): realistic about $9,999 new, a "used, rough" one at about $3,500 that breaks down more, or a game-scaled price (for example $1,500) because the economy is much smaller than real life.
- **Financing:** the existing Loan system could finance it as a second, smaller loan (an equipment loan). That fits the tycoon spine: buy the UTV to save time, pay interest, earn more.
- **Drive:** E to enter and exit, WASD to drive, hand brake, headlights. First-person seat view. Horn optional.
- **Speed:** walk is 2.5 m/s and sprint 5 m/s, and the terrain is 480 m square (about 456 m walkable). Proposed top speed about 10 to 12 m/s (about 2 times sprint), slower on rough ground and slopes. A real 40 mph (about 18 m/s) would cross the whole map in about 25 real seconds, which is too fast.
- **Cargo:** a bed of about 136 kg (300 lb) in the cart storage pattern, plus a passenger seat. A trailer (towing 680 kg, 1,500 lb in real life) is a later add-on.
- **Fuel:** gasoline, bought in cans at the Trading Post. First pass: a 20 L tank, about 1 L per game hour of driving. Fuel is how money turns into time: every trip costs a little cash. This keeps driving from becoming free.
- **Upkeep:** Vehicle Maintenance (from Economy_System.md) as a small recurring cost or a condition value that drops with distance and rough ground. A breakdown means a repair at the workshop or the Trading Post. Keep it light at first: condition only, no engine simulation.
- **Hazards:** the creek and the flood zone (a vehicle in 2 feet of fast water is swept away or stuck), steep slopes (tip risk), and trees (collision stops you). Driving is loud and could spook game and animals near the road (a hunting trade-off, optional).
- **Parking:** left where you stop, with a Garage or Equipment Shed as a later structure (shelter from weather and slower wear).
- **Size:** much bigger than the cart. A first version is probably 700 to 1,200 lines plus Unity vehicle physics (wheel colliders or a simple arcade controller), a camera and controller handover, fuel and condition, saves (position, rotation, fuel, condition, cargo) and tests. This is a multi-phase build like the Tripod and Potash.

# How it fits the rest of the economy

- **Workshop and furniture:** furniture and pottery are heavy and log-hungry. The cart makes the workshop practical. The UTV makes it scale.
- **Income runs:** a vehicle does not add a money line by itself. It saves time, which the runs showed is the limit. It should be measured with a Time audit (Step 0) before and after, using the same Pioneer measuring approach.
- **Loan and pressure:** a UTV bought on a second loan adds a payment on top of the land's. That should be a deliberate late-game choice, not a trap, so the Loan UI must show both payments.
- **Immersion:** a vehicle at the start would hurt the primitive feel. Hand cart then UTV keeps the progression: carry, push, drive.

# Risks and things to watch

- **Trip time trivialised:** if the UTV makes every trip instant, the world feels small. Fuel cost, noise and the terrain limits are the controls.
- **Physics cost:** vehicle physics on HDRP terrain, with trees and the creek, can be buggy. A simple arcade controller is much safer than a full wheel simulation for a first version.
- **Money loop:** none expected, because the vehicle sells nothing. The risk is a runaway financing loop, so the second loan should be capped.
- **Save data:** every new field must be additive, `LedgerSaveData.CurrentVersion` stays 1, and a vehicle left in an unloaded area must reload where it was.
- **Controller handover:** entering and leaving a vehicle in a first-person controller is the usual trouble spot (collision, camera, interaction prompts).

# Decisions for Mike

V1. **Cart first:** wheelbarrow (50 kg) then hand cart (100 kg), or just the hand cart? Recommended: just the hand cart first; add the wheelbarrow only if the hand cart feels too large too early.
V2. **UTV price:** realistic (about $9,999), used and rough (about $3,500), or game-scaled (about $1,500)? Recommended: game-scaled, financed by an equipment loan, so it is a real mid-game goal. This one decides how the whole feature feels.
V3. **Fuel and upkeep:** include fuel from the start (money into time), and condition only for upkeep? Recommended: yes.
V4. **Horse path:** keep the horse and cart as a separate later option in Livestock, or drop it? Recommended: keep it listed, not designed.
V5. **Noise and animals:** does driving spook game? Recommended: optional, only after the first version.
V6. **Roads:** use the open ground and any worn tracks, or add a Path/Road build piece so the vehicle has predictable routes? Recommended: open ground first, tracks later.
V7. **Time audit first:** accept a short read-only Time audit by Claude Code before any vehicle plan? Recommended: yes. It decides whether the UTV is worth building before the workshop, after it or at all.

# Read-only checks for Claude Code before any vehicle plan

1. Player walk and sprint speed, and the walkable world and owned parcel sizes.
2. Typical distances from the cabin to the nearest tappable Maples, the creek, the Well site, the nearest tree stand and the Trading Post, and the game time each takes on foot.
3. The Time audit above (walking versus working versus waiting for the chores listed).
4. How the first-person controller could hand over to a vehicle (Rigidbody, CharacterController, camera, interaction prompts, input map).
5. The existing Storage Bin code as the template for a cart's load.
6. The Loan system: how a second loan could attach, and where Vehicle Maintenance appears in the expense categories today.

# Sources (research 2026-10-30)

- ATV.com, "Polaris Reveals 2026 Ranger 500, Pitches Value for Under $10K" (July 29, 2025): https://www.atv.com/atv/polaris-reveals-2026-ranger-500-pitches-value-for-under-10k-45130159
- The Polaris 2026 Ranger models page (https://www.polaris.com/en-us/off-road/ranger/2026/models/) could not be read in this research, so no Polaris list prices beyond the article's announcement price were used.
- Sheffield Pottery, "How to use a pottery wheel for beginners" (used in the Workshop doc for the wheel-throwing steps): https://www.sheffield-pottery.com/blogs/pottery-wheels/how-to-use-a-pottery-wheel-for-beginners



# Code facts and time audit (Claude Code read-only report, 2026-10-30)

No code was changed. The only editor action was one Unity query that loaded the terrain's tree list.

**Facts from the code**

- **The Trading Post is menu-only.** `TradingPost.cs` and Difficulty_System.md say it has "no physical location". A Trading Post trip costs 0 m of walking and no game time. A cart or UTV therefore helps with hauling and gathering, not with selling.
- **Speeds and stamina (`PlayerController`):** walk 2.5 m/s, sprint 5 m/s, crouch 1.3 m/s. Speed falls linearly to 0.5 times at full encumbrance and sprinting is impossible then. Stamina max 100, sprint costs 15 a second, recovery 25 a second after 1 second, you need 15 to start a sprint. An axe swing costs 4 per 0.9 s. Claude Code's arithmetic gives a practical sprint pace of about 3.9 m/s (sprint about 33 m, then recover about 5 s). That is an estimate, not a measurement.
- **World:** terrain 480 by 480 m (about 57 acres); invisible walls 12 m inside the edge leave about 456 m square walkable (about 51 acres). The 5 acres you own are about 142 m square, but **no code fences the parcel**, so the whole map is walkable. 8,852 trees (7,312 non-shrub, 289 tappable Maples).
- **Clock:** 30 real minutes a game day, so 1 real second is 0.8 game minutes. 100 m on foot is about 32 game minutes. Menu screens stop the clock.
- **Distances from the builder's default cabin site (18, -68)** (the player can place the cabin anywhere, so these shift): nearest Maple 40 m (9 within 100 m, 26 within 150 m); nearest tree 34 m (55 within 60 m, 252 within 100 m); nearest dense stand 44 m; creek about 140 m; spring about 190 m; pond about 219 m. A typical Well site was not measured (setbacks are 3 m from a building and 15 m from pens).
- **Controller handover:** `PlayerController` uses a `CharacterController` with no Rigidbody. Its Update is gated by `CanAct` and `CanUseTools`, and there is a precedent for locked input (knocked down, creek crossing, placement mode, sleeping). A vehicle could parent the camera to a seat anchor and disable the `CharacterController`. Interaction uses `IInteractable` through the parent, so E to enter comes almost free. The input map has Move, Look, Sprint, Crouch, Jump, Interact, InteractAlt, so only a brake needs adding.
- **Cart load:** the Storage Bin only accepts dry goods (not tools, wood, stone, food or water), so a cart needs a new `PileKind` that accepts Logs and Stone. Wood Pile and Rock Pile already hold those. The cart's state position must follow the cart. **Size: about 150 to 250 lines on top of the cart physics.**
- **Loan:** `MoneyManager` holds exactly one `LoanState` ("the one loan of the current game"). A second loan means a list or a loan kind and touches `MoneyManager`, the loan save data, `FinancesScreen` (must show two payments), `LedgerManager`, the loss threshold and the tests. **Size: about 400 to 700 lines in 2 to 3 phases.** Ledger categories are append-only integers 0 to 18 (count 19), so an equipment loan or fuel needs new ids from 19 up; `LedgerSaveData.CurrentVersion` can stay 1.
- **Vehicle Maintenance exists only in the docs** (Economy_System.md, Feature_Backlog.md). No .cs file mentions it. Fuel would land in `BuyOtherGoods` (18) unless it gets its own category.

**Sizes (Claude Code's estimates):** hand cart about 300 to 450 lines in 2 phases (the draft's figure holds); UTV about 700 to 1,200 lines in several phases, with tree collisions on the terrain unchecked; second loan about 400 to 700 lines.

**Time audit (game minutes, round trip; speeds, blows and clock are from the code, distances and click times marked "est.")**

| Chore | Walking | Working | Waiting |
|---|---|---|---|
| Fell 1 tree and carry 3 logs (24 kg) from a stand 40 to 60 m away | 26 to 38 | 7 to 9 (10 to 13 blows) | 0 |
| Maple round, 12 taps | est. 90 (about a 300 m loop) | est. 20 | the sap boil is a separate wait |
| Water trip to the creek (140 m) | about 90 | est. 5 to 10 | 0 |
| Snare round | not estimated | not estimated | not estimated |
| Trading Post trip | 0 | 0 | 0 |
| Cabin or Well delivery trip, 3 logs from a stand | 26 to 38 per load | est. 2 | 0 |

What the audit says:

- **Walking is about 75% to 90% of every chore that involves a trip.** That is far over the 40% line this draft set, but it comes mostly from the pack limit (3 logs a load without being Encumbered), not from distance alone. A 20-log cabin is about 7 loads.
- **A creek water trip of about 90 game minutes is very likely why food and water ate the Winter in the Maple run.** A Well or a Rain Barrel near the cabin removes that trip with no vehicle at all. This is probably the biggest single time fix in the game, bigger than any vehicle.
- **Only 9 Maples are within 100 m of the default cabin site,** so a 12-tap round is a long loop (about 90 game minutes of walking) unless the cabin is placed near a Maple stand. Where the cabin goes matters a lot.
- **Trees do not regrow,** so the walk to the next stand grows as the near trees are cleared.

**What this changes in the plan**

- A **hand cart** attacks the real problem (the 3-log load, a 4 times cut in delivery trips) at a much lower cost than a UTV. A full cart is 12 logs (about 96 kg), so the 7 loads for a Cabin become 2.
- A **UTV's** benefit is mostly the same hauling plus speed on long hauls. On a 456 m map, a 10 to 12 m/s vehicle crosses it in about 40 seconds against about 3 minutes of walking at sprint pace, so it is a luxury more than a fix. It is also the biggest build (700 to 1,200 lines plus a second loan of 400 to 700). Recommended: late, after the cart, the workshop and a Well near the cabin have been felt.
- **Placement matters more than vehicles:** teaching or nudging the player to put the cabin near water and Maples would cut time more than anything here. That belongs in a Know-how entry or a placement hint, not a vehicle.

# Added decisions

V8. **Wooden Wheel as a Craft recipe, not a workshop part:** the first draft makes the Wooden Wheel at the Woodworking bench (so the cart waits for the workshop). Recommended: make it a normal instant Craft recipe (for example 1 Log, 1 Cordage, Knife) so the hand cart can be built before the workshop exists. The bench can make a better wheel later.
V9. **A cart load kind:** a new `PileKind` for the cart load that accepts Logs, Stone, Clay and Firewood (not food or water). Recommended yes.
V10. **Cabin placement hint:** add a Know-how entry or placement hint about putting the cabin near water and Maples, as the cheapest time saver. Recommended yes, as a small separate task.
V11. **UTV priority:** accept that the UTV is late, after the cart, the workshop and the water-near-cabin hint. Recommended yes.


# Answered by Mike after the code report (2026-10-30)

- **Build order:** hand cart first, then the workshop in three phases, then pottery (kiln chain, then Kick Wheel), UTV last.
- **V8 Wooden Wheel:** a normal instant Craft recipe (not a workshop part), so the cart does not wait for the workshop.
- **V10 Cabin placement hint:** not now.
- Mike's water in the Winter runs was a mix of creek trips and a Well or Rain Barrel.
- Still open: V1 (wheelbarrow tier as well as the hand cart), V2 (UTV price), V3 to V7, V9, V11.


# BUILT: Hand Cart Phase 1 (2026-10-31, playtest 1 to 22 passed)

Built by Claude Code as a staged site that becomes a parked, loaded cart. Pushing is Phase 2 and is not built.

- **Wooden Wheel:** an instant Craft recipe in the Materials category, 1 Log + 1 Cordage, no tool, a 3 kg item. It has no market entry, so it cannot be sold and does not appear in the Sell tab.
- **Hand Cart Site:** a staged site (new PileKind HandCartSite, appended after LeachHopper): 2 Wooden Wheels, 3 Logs, 6 Branches, 3 Cordage delivered with R over several trips, about 12 Hammer blows. It stands on any dry ground (the flood band is refused). The Build and Craft HAVE / SHORT / NEED display works on it.
- **Finished Hand Cart:** a WoodPile (PileKind HandCart) with a 100 kg cap by weight (12 Logs, 20 Stone, 28 Clay, 66 Firewood in an empty cart), carrying Logs, Branches, Firewood, Stone, Clay and Tall Grass only. The load does not count toward the pack's Encumbered line. The transfer screen shows "x / 100 kg".
- **Spacing:** a parked cart is exempt from the 2 m building spacing (CartRules.SpacingMetres instead), but not from a cabin's 3.2 m clearance or the Kitchen's 3 m setback.
- **Take down:** the Hammer and a "Take Down Cart" button on the transfer screen return half the materials plus the whole load; what the pack cannot carry is left on the ground as pickup piles.
- **Saves:** additive only (WoodPileState, no new fields), LedgerSaveData.CurrentVersion stays 1, old saves load unchanged.
- **Code:** pure rules in Economy/CartRules.cs; 51 new edit-mode tests (suite 1021 to 1072). One existing test (PotashWorldTests) changed from "exactly 31 PileKinds" to "at least 31".
- **Dev keys (editor only):** [ gives the cart kit and a Hammer, ] stands a loaded cart (4 Logs, 6 Stone, 10 Firewood).
- **Known follow-ups:** the site's cost line reads "2 Wooden Wheel" because the item name is singular (rename to "Wooden Wheels" like Logs and Branches). The game's prompts use "[Hold E]", so the Phase 2 grab should follow the same hold-E convention rather than a tap.
