# Homestead
## Modern Systems and Services v0.1

Status: Design Draft, nothing built. Ideas from Mike 2026-11-08; the ladder, numbers and build order below are Claude's PROPOSALS, not decisions.

---

# Purpose

Answers one question: what is the clearest path for a player to move from primitive tools and hand-built structures to modern equipment and systems (solar power, water and sewage systems, livestock feeders and waterers, crop production), so the game never feels like a survival game set in primitive times?

This doc covers how modern things are obtained (craft, buy and install, hire), how the player gets access to them (a phone, then internet), what that costs every month, and how it all plugs into the existing money system. The detailed numbers for each modern system live in their own docs (`Power_System.md` and others); this doc is the shared frame they hang on.

---

# Principles

1. **The game is set today.** The start is primitive because the player has no money, not because of the era. Modern items already appear (a rifle, a canteen, a weather station), so a phone, a delivery truck and a contractor crew are natural.
2. **Modern Tools Are Earned** (GDD pillar). Each modern step needs the materials, the infrastructure and the money or the skill. Money alone is never the only gate.
3. **Build the frame once.** One ordering system, one hiring system and one bills system; every modern system is then a catalog entry, not a new mechanic.
4. **Modern life costs money every month.** Recurring bills are how the game shows it. Each bill must buy something the player can see paying for itself.
5. **Only the land loan can end the game.** A bill the player cannot pay suspends the service; it never causes a loss.

---

# Three lanes

| Lane | What it is | Real-life example | Gates |
|---|---|---|---|
| Craft | The player makes it from land resources | furniture, pottery, charcoal, iron hardware, simple tools | materials, stations, know-how |
| Buy and install | The player orders a kit or pieces, then installs it | solar panels, batteries, pumps, tanks, fencing, feeders, appliances | money, delivery access, a prepared site, tools, later skill |
| Hire | A contractor builds the whole system | drilled well, septic system, full electrical hookup, grid connection, a builder crew | money (the highest price), a phone, a staked site, lead time |

Most modern systems offer buy-and-install and hire, and the player picks money-versus-labor. Things nobody makes at home (panels, batteries, pumps) are only ever bought, which is realistic.

---

# Gates in the world (not only price)

A modern system needs some of these before it can be ordered or installed:

- **A place:** a staked site using the existing placement preview (the same one used for builds).
- **A structure:** for example a roofed Solar Shed or battery shed; a Well with the right depth for a pump.
- **A way for it to arrive:** a Delivery Spot (mailbox or driveway pad) that small orders come to, and a road or driveway wide enough for large deliveries. Before the road exists, large items cannot be delivered (the hand cart and the UTV matter here).
- **Tools:** for example a tool set for electrical or plumbing install.
- **Skill:** optional later, if the parked `Skills_and_Progression.md` is picked up.
- **A phone and service:** to call a contractor or order from the catalog (below).

---

# Ordering and delivery (the Order screen)

- The **Trading Post** stays the first store: cash, immediate, existing market rules.
- The **Catalog** is a second store, opened by a phone: the player orders kits and pieces, pays at order time, and the order arrives at the Delivery Spot after a few game days (PROPOSED 2 to 7, depending on item and distance). A delivery fee applies (PROPOSED flat plus weight-based).
- Orders show in the Journal and ledger (a new category, Orders) and in a pending-orders list; a delivered order is announced and waits at the Delivery Spot.
- Large items (a battery bank, a tank, a UTV) need the vehicle-access gate above.
- Only items already in the game's catalog are sold, so each new system adds rows, not code.

---

# Hiring a contractor

- The player calls from the phone, picks a job, and stakes the site with the placement preview. The contractor gives a **quote** (price, lead time, build time).
- Money is taken at booking or on completion (PROPOSED: half at booking, half on completion; Mike to decide).
- The job runs on game days in the background as a **staged site** (the existing system for heavy builds); the player keeps playing and can see progress. No people appear on the property at first; a crew model is a later art item.
- Contractor price is the highest lane (PROPOSED about 2 to 3 times the material cost), and the job can have prerequisites (access road, site prepared).
- First candidate jobs: drilled well, septic system, electrical hookup, solar install, a builder crew for the next cabin tier.

---

# Phone and internet ladder (PROPOSED)

| Step | What the player needs | Unlocks | Monthly bill |
|---|---|---|---|
| 0. No phone | the start | the Trading Post only | none |
| 1. Basic prepaid phone | bought at the Trading Post | call contractors, order from the Catalog | small |
| 2. Smartphone with a data plan | upgrade | weather and market-price lookup, online ordering, more catalog items | medium |
| 3. Home internet | a modem or satellite kit, installed or hired | online selling channels (better prices for some goods), the full catalog | larger |

The dollar amounts are placeholders to be researched against real rural prices before anything is built. Phones and plans are generic (no real brand names).

Service rules:

- Bills are charged on a regular day. If cash is short, the service is suspended until paid; nothing is lost and the missed-payment count is untouched.
- The player can cancel and re-subscribe.
- Each step must pay back: for example, online selling raises what some goods earn, and contractors save labor days.

---

# Recurring bills on the statement

A new expense category, Bills, joins the ledger and the seasonal Account Statement: phone, internet, utilities (grid power or propane while there is no solar), and later insurance and property tax (parked in `Money_System.md`). Bills are charged on the 15th of each month (Mike, 2026-11-08; the loan falls due on the 1st), so they sit beside the land payment on the statement and show the player the true cost of running the homestead.

---

# What each modern system looks like

| System | Craft | Buy and install | Hire | Needs first |
|---|---|---|---|---|
| Electric power (solar, wind, generator) | no | panels, batteries, inverter, wiring | full install and grid connection | a roofed Solar Shed, delivery access, phone (see `Power_System.md`) |
| Water (pressure, pump, recycling) | hand pump parts | electric pump, tank, filters | drilled well, plumbing | Well, power for the pump |
| Sewage | outhouse (built) | septic kit | septic system | site, access, digging |
| Livestock feeders and waterers | simple feeders | automatic feeders and waterers | n/a | livestock built, power and water |
| Crop production | tools, beds | irrigation, greenhouse kit | n/a | farming built, water |
| Vehicles and powered tools | hand cart (built) | UTV, chainsaw | n/a | fuel, road access |
| Housing tiers | cabin by hand | materials package | builder crew | Stage design in `Building_Housing_System.md` |

The cells are a starting map, not final design; each system's own doc sets the real recipes and prices.

---

# Fit with the money system

- Bills raise the income the player needs, so the Money Phase D2 measurements must include them or they will under-state the pressure.
- The land loan stays the only loss condition (Principle 5).
- Every modern purchase should be able to pay for itself over time, otherwise the player has no reason to take it. A payback table (cost, monthly bill, monthly gain) belongs in each system's doc.

---

# Suggested build order (not committed)

1. Order and delivery frame with a Delivery Spot, using Trading Post catalog items (no phone yet).
2. Phone, the first plan and the Bills ledger category.
3. Contractor framework with one job (the drilled well is the best fit because the Well already exists).
4. The first modern system on the frame: Electric power.
5. Smartphone and internet steps, then more systems.

---

# Open questions for Mike

1. Is the phone an item the player buys (Trading Post) or something they start with but cannot use until they pay for service?
2. A phone screen: a key that opens a Phone UI, or a menu entry only?
3. Where do orders arrive: a mailbox or a driveway pad, or both?
4. RESOLVED 2026-11-27 (Mike): half at booking, the other half on completion.
5. Do contractor crews ever appear on the property (art cost), or is it a staged site only?
6. Which bills exist at first: phone and internet only, or utilities too?
7. RESOLVED 2026-11-08: the calendar change lands first, option D (a real 365-day calendar). Options that were considered: (A) 84-day seasons with 28-day months, (B) A plus a shorter real day, (C) keep the calendar and change only the loan cadence, (D) a real 365-day calendar with real month lengths, no leap years, seasons starting March, June, September and December (Mike's idea 2026-11-08). Bills and contractor lead times are easier to set once the calendar is final.

---

# Deliveries and the Pole Barn (Mike, 2026-11-27)

Mike's play-test note: the Trading Post will not sell more than the player can carry, which gets in the way of big builds (60 Lumber, 110 Glass Panes). It led to a broader idea: delivery of large orders, a Pole Barn to store everything the homestead needs and to take deliveries, and contractors for the self-sufficiency systems. Discussed 2026-11-27.

**Mike's answers (2026-11-27):**
1. **Delivery comes first, before the phone.** It is step 1 of the build order above.
2. **The Pole Barn feeds nearby buildings:** building sites and builds near it draw their materials from it, so the player does not haul from the barn to the site.
3. **Different sizes, but fit-outs (stalls, loft and so on) only for the largest.**
4. **Contractors are paid half at booking and half on completion** (open question 4 above).

## Proposal (Claude, 2026-11-27; approved by Mike 2026-11-27 with the radius set to 50 m)

**Slice D1: delivery from the Trading Post**
- At the Trading Post, an order can be **carried** (as now, up to what the pack holds) or **delivered**. A delivered order has no weight limit.
- Real lumber yards charge a flat fee of about $25 to $75 a load, and many deliver free over a set order size. PROPOSED: $50 a delivery, free on orders of $1,000 or more.
- It arrives the next morning at the **Delivery Spot**: a gravel pad by the road where the player first arrives (a placed structure, free; the player can move it later). The goods wait there as a storage pile the player takes from, like a Wood Pile.
- Orders show in the ledger as purchases (the existing categories) and the delivery fee as its own line.

**Slice B1: the Pole Barn (shell and storage)**
- Real post-frame barns are sold as kits (posts, trusses, steel roofing and siding, hardware) or built by a crew. Prices in 2026 run about $10 to $30 a square foot for a kit and $15 to $40 built; Midwest prices sit at the low end. The game's land is priced at real Bethel prices, so the barn is too (low Midwest end):

| Size | Footprint | Kit (deliver and build it yourself) | Built by the crew (half at booking, half when done) | Storage (*proposed*) |
|---|---|---|---|---|
| Small | 24 x 32 ft (768 sq ft) | $7,500 | $13,000 | 5,000 kg |
| Medium | 30 x 40 ft (1,200 sq ft) | $12,000 | $20,000 | 10,000 kg |
| Large | 40 x 60 ft (2,400 sq ft) | $22,000 | $36,000, plus fit-outs | 20,000 kg |

- **Ordering:** at the Trading Post counter (the lumber yard arranges kit and crew, as real ones do), so it does not wait for the phone. The kit comes by delivery (the order arrives on the staked site).
- **Building it yourself:** a staged site like the Hoop House, finished with the Hammer over several game days of work. **The crew:** books a start date a few days out and finishes in about a week of game days, while the player keeps playing; the second half is paid on completion.
- **Storage:** R stores what you carry and E takes it, like the other storage, with a much larger limit by weight.
- **Feeding nearby builds (Mike's answer 2):** a building site, a one-step build or a craft station within 50 m of a barn (Mike, 2026-11-27) draws any material it needs from the barn when the player delivers or builds there, after what the player carries. The build ghost and site prompts show "from the barn" so the player sees it.
- **Deliveries:** once a barn stands, delivered orders go into the barn (if it has room) instead of the Delivery Spot.
- **Fit-outs (Large barn only; Mike's answer 3), later slices:** livestock stalls and pens (with Livestock_System.md), a hay loft, an equipment bay for the UTV and tractor (Vehicles_and_Hauling.md), and the roof as a mount for solar panels (Power_System.md).

**Mike's answers for slice D1 (2026-11-27):**
1. **Ordering:** an order list at the Trading Post. Store rows can be added to it; it shows the items, the $50 fee (free at $1,000 or more) and a Place Order button, and everything is paid at once.
2. **Where it arrives:** a Delivery Pad the player builds, free, from the Build tab, anywhere; one at a time (move it by taking it up and building it again). No pad, no delivery.
3. **When:** 1 to 3 days by the order's weight (*proposed*: up to 500 kg the next morning, up to 2,000 kg the day after, more than that 3 days), at 8 a.m.
4. **On the pad:** kept like a store with no limit: E takes, R puts back. Food and seed age as in any storage.

**As built: slice D1 (2026-11-27), awaiting Mike's play-test:**
- **Carry or Deliver:** a button at the top of the Trading Post's store list switches modes. In Deliver mode each row's button is "Add": it puts that many on the order, with no weight limit (money is checked when the order is placed).
- **The order:** the right-hand column becomes "Delivery order" (selling waits until the switch is back on Carry). Each line shows the count, the item and its price, with a Remove button; then the goods, the fee and the total, the weight and when it arrives, and Place Order and Clear. A red line says when there is no Delivery Pad, and Place Order is greyed out. The order list itself is not saved.
- **Place Order:** pays for everything at once at today's prices: $50, or free when the goods come to $1,000 or more. Each item goes in the ledger under its usual purchase category, and the fee under a new category, "Delivery fees" (category 19, appended).
- **Arrival:** 8 a.m. the next day for up to 500 kg, the day after for up to 2,000 kg, otherwise in 3 days, with a banner. If the pad has been taken up, the order waits and a banner says to build one. Orders on their way are saved with the money (an older save has none).
- **The Delivery Pad:** under Other on the Build tab, free, no Hammer; one at a time. A 3 m gravel square with a timber edge; a tarped pallet shows while anything is on it. It is a store with no limit: E opens the transfer screen, R stores. Take it up with the Hammer once it is empty (from the transfer screen).
- **Know-how:** "Deliveries from the Trading Post" (entry 34), unlocked by building the pad.
- **Code:** Economy/DeliveryRules.cs (every number and the words, and the saved DeliveryLine and DeliveryOrder), TradingPost (the order, PlaceOrder, the arrivals, saving), TradingPostScreen (the switch and the order panel), WoodManager (kind 88 appended: DeliveryPad), WoodPile (the pad as a store, and its look), GameManager (no orders carried into a new game or a load), Ledger; tests in DeliveryD1Tests.

**Mike's answers for slice B1, the Pole Barn (2026-11-27):**
1. **Building it yourself: real parts, staged.** The kit is posts, trusses and steel sheets; they are stocked into the site in stages (the posts, then the trusses, then the roof and walls), and each stage is hammered.
2. **The crew:** starts 3 days after booking and builds for about a week (*proposed*: Small 5 days, Medium 7, Large 10); the site shows its progress, and the second half is paid when it is done.
3. **What a barn within 50 m feeds:** staged sites (delivering to them), one-step builds (the Build tab counts the barn's stock) and crafting (the Craft tab counts it too).
4. **A walk-in building** with a big sliding door and a storage bay inside (E at the shelves opens the store), with room for the later fit-outs and vehicles.

Built in two parts: B1a, the barn (parts, the staged site, the crew, the walk-in shell and its storage); B1b, the barn feeding nearby builds and crafting, and deliveries going into it.

**As built: slice B1a (2026-11-27), awaiting Mike's play-test:**
- **The site:** Small, Medium and Large Pole Barn Sites under Other on the Build tab, free (24 x 32, 30 x 40 and 40 x 60 ft; placed like a house, facing matters: the doorway is the front). The site shows corner stakes and string lines, the parts waiting on it, and the work as far as it has got.
- **Ordering:** at the Trading Post in Deliver mode, the top of the order panel lists each barn site you have marked out with two buttons. **Order** buys the kit at once (Small $7,500, Medium $12,000, Large $22,000; delivery is free) and it arrives on the site itself in the usual 1 to 3 days, not on the Delivery Pad. **Hire** books the crew (Small $13,000, Medium $20,000, Large $36,000) and takes half now. With no site marked out, a line says to mark one first.
- **The parts:** Barn Posts (40 kg), Barn Trusses (70 kg) and Steel Sheets (15 kg) are items with icons and Inventory tips; they are not sold on their own and not bought back. Small 10 / 9 / 60, Medium 12 / 11 / 75, Large 16 / 16 / 107.
- **Building it yourself:** three stages, each hammered with the stage's parts on the site: set the posts (8 blows a post), raise the trusses (10 a truss), put on the roof and walls (2 a sheet). Each stage ends with a banner naming the next; the last one turns the site into the barn. The parts are locked in once the first blow lands.
- **The crew:** starts 3 days after booking and works 5, 7 or 10 days; the site shows their progress (posts, then trusses, then the steel going on). The Hammer at a crew site says the crew has it. When they finish, the balance is paid; if the money is not there, a banner says it is due and it is taken as soon as you have it. The payments are in the ledger under a new category, "Contractors" (category 20, appended). Crews are saved with the money (an older save has none).
- **The barn:** red ribbed steel walls on treated posts, a galvalume roof, white trim, a gravel floor, a red sliding door slid open beside the big doorway, and three-tier shelves along the back. Walk in through the doorway; at the shelves E opens the transfer screen and R stores. It holds anything but water, up to 5,000, 10,000 or 20,000 kg by weight. Nothing else can be built inside it.
- **Know-how:** "The Pole Barn" (entry 35), unlocked by marking out a site.
- **Art:** structure_polebarnsmall / _medium / _large (about 4,000 to 6,500 triangles) and item_barn_post, item_barn_truss and item_steel_sheet; Build and item icons rendered from them.
- **Code:** Economy/BarnRules.cs (every number and the words), WoodManager (kinds 89 to 94 appended), WellsAndOuthouses (the staged site in three stages), TradingPost and TradingPostScreen (the kit, the crew and their saving), World/PoleBarnVisuals.cs (the site and the barn), PlacedStructure, WoodPile, StorageTransferScreen, CraftingScreen, PlacementMode, AxeTool, Ledger, KnowHowRules; tests in PoleBarnB1Tests.
**Then:** the phone, bills and the contractor frame (build-order steps 2 and 3), power, water pressure, septic and greywater, and livestock in the barn.

Sources (2026-11-27): HomeGuide, pole barn kit prices and pole barn prices (kit $10 to $30 per sq ft, built $15 to $40, labour $5 to $10); PoleBarnCosts 2026 guides (kit vs contractor; Midwest at the low end); Angeles Millwork and Ricci Lumber delivery pages (flat $45 to $50 delivery, free over a minimum order).

# Dependencies

The calendar decision (seasons and month length), Money Phase D2 (income per game day with the Workshop, pottery and metal in play), `Skills_and_Progression.md` (parked), the Power and Vehicles designs, and art for the phone UI, Delivery Spot, delivery truck and crew.
