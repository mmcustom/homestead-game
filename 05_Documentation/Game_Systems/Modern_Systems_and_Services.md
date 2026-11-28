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
**Mike's answers for slice B1b, the barn feeding builds and taking deliveries (2026-11-27):**
1. **Staged sites:** R at a site within 50 m of a barn loads what the player carries first, then tops up from the barn in the same press.
2. **Drop point:** a standing barn counts as a drop point, so an order can be placed with no Delivery Pad; the pad is only needed when no barn has room.
3. **Overflow:** as much as fits goes into the barn and the rest onto the Delivery Pad (or it waits for a pad if there is none).
4. **More than one barn:** the barn nearest the Delivery Pad (the first one built if there is no pad), spilling over to the next barn when it is full.

**As built: slice B1b (2026-11-27), awaiting Mike's play-test:**
- **What feeds:** the shelves of every finished Pole Barn within 50 m (measured flat). For builds and crafting the distance is from where the player stands; for a site it is from the site. The nearest barn is used first.
- **One-step builds (Build tab):** the materials counts and the "Needs ..." check include the barns, and building pays from the pack first, then the barns. A line under the entry says the counts include a barn within 50 m.
- **Crafting (Craft tab):** the same. The counts, the Craft button and the amount stepper include the barns, and crafting spends the pack first, then the barns. The stepper still judges the weight as if every ingredient came out of the pack; anything made that the pack can't hold goes on the ground in front of you, as before.
- **Staged sites and the Small Cabin site:** R stores what you carry, then tops the current stage up from the barns in the same press. The prompt says "Store ..., then the rest from the barn", or "Load from the barn (...)" when you carry none of it. A Kitchen hearth's R only takes it down when neither the pack nor a barn has anything for it.
- **Deliveries:** a standing barn is a drop point, so Place Order works with no Delivery Pad. An order fills the barn nearest the pad (or the first built), then the next, by weight; whatever doesn't fit goes on the pad. With no pad, the rest waits on the order until there is room or a pad, and a banner says so. The order message says it arrives at your Pole Barn. Pole Barn kits still go onto their own site.
- **Know-how:** "The Pole Barn" (entry 35) has a fourth paragraph on feeding and deliveries; the Build tab help for the barn sites says so too.
- **Code:** BarnRules (FeedRadius, Allocate, the words), DeliveryRules (NoPad, Placed to the barn), WoodManager (BarnsNearestTo, BarnsFeeding, BarnStock, Have, TakeFromBarns, Spend; CanStartBuild and BuildPile use them), Crafting, CraftingScreen, PlacedStructure and WoodPile (R tops sites up), TradingPost (HasDropPoint, DropBarns, IntoBarns) and TradingPostScreen; tests in PoleBarnB1bTests.
**Then:** the phone, bills and the contractor frame (build-order steps 2 and 3), power, water pressure, septic and greywater, and livestock in the barn.

Sources (2026-11-27): HomeGuide, pole barn kit prices and pole barn prices (kit $10 to $30 per sq ft, built $15 to $40, labour $5 to $10); PoleBarnCosts 2026 guides (kit vs contractor; Midwest at the low end); Angeles Millwork and Ricci Lumber delivery pages (flat $45 to $50 delivery, free over a minimum order).

# The phone, bills and contractors (Mike, 2026-11-27)

**Mike's answers (2026-11-27):**
1. **Getting a phone:** the player starts with a smartphone in the pack. It does nothing useful until a service plan is paid.
2. **Opening it:** its own key, P. It should look like a real smartphone held in the player's hands: the first-person hands raise a 3D phone, and its screen shows the apps, the time and the weather when it has service.
3. **The plan:** one plan, $25 a month (unlimited talk, text and data, the real 2026 cheap prepaid price), billed on the 15th.
4. **Apps:** Contractors, Weather, Bills & Plan and Market prices, plus Mike's own app icons on the home screen (Brandy's Bakery, Sweet Swap, Fast Food Frenzy, Starforge and My Tech App) as free advertising.
5. **No service** (no plan yet, or a bill unpaid): the screen still shows the time and date with a "No Service" bar; apps that need service are greyed out except Bills & Plan, to sign up.
6. **The first contractor job is a Drilled Well:** a new structure, separate from the hand-dug Well, at about the real Ohio price ($5,320 for an average 140 ft well, $25 to $55 a foot). A cased well with a hand pump now: clean water that never runs dry. An electric pump comes with Power later.
7. **All hiring is by phone:** the Pole Barn crew moves from the Trading Post counter to the phone's Contractors app; the barn kit is still ordered at the Trading Post.

Built in two parts: P1, the phone, its plan and bills, and the apps (the Pole Barn crew moves into Contractors); P2, the Drilled Well job.

**Mike's answers for slice P2, the Drilled Well (2026-11-27):**
1. **Price by depth at the spot:** the quote depends on how deep the aquifer is where the site is staked, from about $3,500 on low ground near water to about $9,000 on high ground (real Ohio drillers: $25 to $55 a foot). The phone shows the quote before hiring.
2. **A big supply that refills:** about 1,000 L of excellent water, refilling about 40 L an hour, so heavy use can run it low.
3. **Hiring:** mark out a free Drilled Well Site on the Build tab and hire the driller by phone, half now and half when done; the rig comes 3 days after booking and drills for 2.

**As built: slice P2 (2026-11-27), awaiting Mike's play-test:**
- **The site:** a Drilled Well Site under Other on the Build tab, free. It keeps a well's distances (from the outhouse, water's own zone, floodplain and hollows) but has no depth limit. It remembers the depth to water where it was placed (the same lookup the hand-dug Well's ghost shows). It looks like four stakes with orange flagging round a marked spot; E or its prompt says to hire by phone and shows the quote.
- **The quote:** the drilled depth is 70 ft plus 15 ft for every metre to the water table, between 80 and 250 ft (about 92 ft by the creek, about 205 ft where a hand-dug well is too deep); the price is $30 a foot plus $1,000 for the casing cap, the pump and the permit: about $3,800 to $8,500 (an unknown depth is quoted as the average 140 ft, $5,200).
- **Hiring (phone, Contractors, "Well drilling"):** each site with its quote and a Hire button for half now, or the rig's status once booked. The rig (a yellow truck with its mast up, and a pile of cuttings that grows) shows up 3 days later and drills for 2. When it is done the balance is taken (or a banner says it is due and it is taken as soon as there is the cash), and the site becomes the Drilled Well, full. Both payments are Contractors in the ledger; the job is saved with the crews (CrewJob gained a job type and a price).
- **The Drilled Well:** a concrete pad, the steel casing and its bolted cap, and a cast-iron pitcher pump (structure_drilledwell). E pumps water into the Bucket or a Canteen, or drinks: excellent water (flood water and a privy can't foul it), up to 1,000 L, refilling 40 L an hour, drought or not. It counts as a Well for the outhouse distances and for a garden Hand Pump beside it.
- **Know-how:** "The Drilled Well" (entry 37), unlocked by marking out the site. The Build tab help says all this.
- **Code:** Economy/DrillRules.cs (depth, price, timing, water, words), WoodManager (kinds 95 and 96 appended: DrilledWellSite, DrilledWell), WellsAndOuthouses (setbacks, water quality, numbers, the hourly refill), TradingPost (HireDriller, the driller's hour), PhoneView (the Well drilling section), World/DrilledWellVisuals.cs (the site, the rig and the well), PlacedStructure, WateringFittings, CraftingScreen, DeliveryRules (CrewJob), KnowHowRules; tests in DrillP2Tests.

**As built: slice P1 (2026-11-27), awaiting Mike's play-test:**
- **The phone:** a Smartphone item (0.2 kg) in both starting kits; a save from before it gets one in the pack when it loads. P raises it, P or Esc puts it away (Esc doesn't also open the pause menu). It can't come up while placing a build, pushing the cart or knocked down in the creek; opening any menu puts it away.
- **In the hands:** both first-person hands hold a 3D phone (item_smartphone, graphite with a camera bump) low in front, its screen facing the eye; it slides up as it comes out. The tool in hand is lowered while the phone is up. The world keeps running (it is a phone, not a pause menu), but you stand still and the mouse is free.
- **The screen:** a panel drawn over the 3D screen each frame, fitted to where its corners land on the display, so the text is sharp and big and the mouse clicks it. A status bar (the time, "5G" or "No Service", the battery), and a home bar at the bottom that goes back home.
- **Home:** a big clock, the date, and the temperature and weather (with service) or "No service - open Bills & Plan to start a plan" (without). Nine apps, three across: Contractors, Weather, Bills & Plan, Market, then Brandy's Bakery, Sweet Swap, Fast Food Frenzy, Starforge and My Tech App, with Mike's own icons (copied to Art/References/PhoneApps; Fast Food Frenzy's is its burger on a red tile). Without service every app but Bills & Plan is greyed out and says it needs service.
- **Bills & Plan:** the plan ($25 a month, unlimited talk, text and data, billed on the 15th), its status, the next bill date and the cash; Start the plan (the first month is paid now), Cancel (service runs to the next bill day, then ends with nothing more to pay), Keep my plan, and Pay now when suspended. The last six bills are listed.
- **Bills:** on the 15th the $25 comes out of the cash with a short message; with too little cash the plan is suspended with a banner until it is paid from the app (nothing else is lost, and no more bills pile up meanwhile). They show in the ledger and the statement under a new category, "Bills" (category 21, appended).
- **Weather:** the temperature, conditions and feels-like, precipitation, wind, humidity, tonight's expected low (with "frost likely" at 32 F or below), sunrise and sunset, and the creek. There is no multi-day forecast in the game's weather yet.
- **Market:** the season's note, then what the Trading Post pays for each thing it buys and what it charges for each thing it sells, today, alphabetically.
- **Contractors:** each Pole Barn Site with the crew's price and a Hire button (half now), or the crew's progress once hired. The Trading Post's Deliver panel no longer hires; it says to hire by phone.
- **The ads:** Mike's apps open a page with the icon, the name and a line ("A cozy bakery game by Mike Mack" and so on).
- **Know-how:** "Your Phone" (entry 36), unlocked by having the phone (so at once); an Inventory tip for the phone. The Pole Barn entry and the barn's Build help now say the crew is hired by phone.
- **Code:** Economy/PhoneRules.cs (the plan, bills and suspension, the words, the apps), Managers/PhoneManager.cs (the plan's state, billing on the 15th, saving), UI/PhoneView.cs (P, the 3D phone, the screen and the apps), HandPoseRules (the phone pose), PlayerHands, PlayerController, DifficultyManager (the kit), GameManager, Ledger, TradingPostScreen, BarnRules, KnowHowRules; tests in PhoneP1Tests.
# Dependencies

The calendar decision (seasons and month length), Money Phase D2 (income per game day with the Workshop, pottery and metal in play), `Skills_and_Progression.md` (parked), the Power and Vehicles designs, and art for the phone UI, Delivery Spot, delivery truck and crew.
