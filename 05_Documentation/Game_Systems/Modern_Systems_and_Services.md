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
4. Contractor payment: all at booking, half and half, or on completion?
5. Do contractor crews ever appear on the property (art cost), or is it a staged site only?
6. Which bills exist at first: phone and internet only, or utilities too?
7. RESOLVED 2026-11-08: the calendar change lands first, option D (a real 365-day calendar). Options that were considered: (A) 84-day seasons with 28-day months, (B) A plus a shorter real day, (C) keep the calendar and change only the loan cadence, (D) a real 365-day calendar with real month lengths, no leap years, seasons starting March, June, September and December (Mike's idea 2026-11-08). Bills and contractor lead times are easier to set once the calendar is final.

---

# Dependencies

The calendar decision (seasons and month length), Money Phase D2 (income per game day with the Workshop, pottery and metal in play), `Skills_and_Progression.md` (parked), the Power and Vehicles designs, and art for the phone UI, Delivery Spot, delivery truck and crew.
