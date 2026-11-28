# Homestead
## Power System v1.0

Status: Design Draft — Future, not blocking Alpha 0.1

---

# Purpose

Covers electricity generation, storage, and consumption for the homestead: gas generators, solar, and wind as generation options; battery banks for storage; and the sizing math that ties them to whatever actually needs power.

**Added 2026-10-03 (Mike):** "here's where we need to do some research and math, to figure out the correct sizes of gas generators or solar or wind generators and battery banks to supply correct amounts of energy to use for everything that requires electricity, when we get to that point." Mike's own framing — "when we get to that point" — scopes this as research to have ready, not a build request. Nothing in Alpha 0.1 requires electricity today; this document exists so the sizing is already worked out, grounded in real numbers, by the time a system actually needs power.

---

# Core Philosophy

Electricity is a late-homestead upgrade, not a starting assumption — consistent with Housing progression (Building_Housing_System.md): a Small Cabin has none, a Large Cabin's indoor running water is already explicitly conditional on "equipment working properly" (a likely electric well pump), and a Workshop or Farmhouse is where real loads (power tools, refrigeration, a washing machine) would first make sense.

The player should face the same real trade-off an actual off-grid homesteader does: generation and storage both cost real money/materials and scale with how much is actually being run, not an abstract unlocked tier. Oversizing wastes resources; undersizing means a generator that can't start the well pump, or lights that go out overnight.

Three generation methods, not one, so the player chooses based on their situation — same way Weather_System.md already gives Wind a real mechanical role, Solar depends on clear skies, and a gas generator trades fuel consumption for reliability regardless of weather.

---

# When This Applies

Nothing today needs power. The first real electrical load in the existing design is the Large Cabin's indoor running water pump (Building_Housing_System.md: "it only works if water reserves are full and equipment is working properly" — "equipment" is that pump). Building_Housing_System.md's existing Utility Structures list already names **Well House, Windmill, and Solar Shed** — this document is what gives Windmill and Solar Shed (and a new Gas Generator option alongside them) their actual numbers once Building/Housing reaches that tier. A Workshop ("tool maintenance, equipment repair") would be the next real load (power tools), and a Farmhouse ("maximum comfort... large storage capacity") is where a full appliance set — refrigeration, a washing machine, full interior lighting — would land.

Nothing here is sent to Claude Code as a build task yet. This is design math to have ready.

---

# Load Tiers

Matched to Building_Housing_System.md's existing progression, using real-world off-grid load data as the anchor (see Sources):

| Homestead tier | Real-world equivalent | Daily load | What's drawing power |
|---|---|---|---|
| Large Cabin | Weekend/minimal cabin | ~1–2 kWh/day | Well pump (intermittent, ~1,500W peak), a few interior lights |
| Cottage / Workshop | Year-round cabin | ~6–8 kWh/day | Above, plus Workshop power tools, more lighting, device/Flashlight battery charging |
| Farmhouse | Full homestead | ~15–20 kWh/day | Above, plus refrigeration (~1,500 Wh/day for a compressor fridge), a washing machine, a fuller lighting circuit |

These are starting proposals for Claude Code's balancing pass, not locked numbers — same status as every other first-pass figure in this doc set.

---

# Sizing Methodology

Standard off-grid sizing math (see Sources for the real-world formulas this reuses directly):

**1. Daily load, in watt-hours (Wh):** for every powered item, `watts × hours/day = Wh/day`, summed across everything that might run. Add a 20–25% margin for system losses (inverter standby draw, wiring, battery round-trip inefficiency).

**2. Battery bank capacity:** `daily Wh × days of autonomy ÷ depth of discharge = usable Ah` at the system's battery voltage. 2–3 days of autonomy is the real-world standard for a grid-independent cabin — enough to ride out a cloudy stretch without needing the generator. Example at the Large Cabin tier: 1,500 Wh/day × 2.5 days = 3,750 Wh needed; at a 90% depth of discharge (lithium-type), that's about 4,170 Wh of rated battery capacity.

**3. Solar array sizing:** `daily Wh ÷ (peak sun hours × system efficiency) = panel wattage`. System efficiency is typically ~80% (wiring, inverter, dirty panels, imperfect angle). At 4 peak sun hours (a reasonable mid-latitude average) and the Large Cabin's 1,500 Wh/day: 1,500 ÷ (4 × 0.8) ≈ 470W of panels.

**4. Wind turbine sizing:** rated-power turbines produce roughly their capacity factor (~42% for a modern small turbine) of their rating, continuously. A 1 kW turbine averages ~307 kWh/month (~10 kWh/day) — far more than the Large Cabin tier needs on its own, meaning even a small turbine is oversized for an early homestead and makes more sense once the Farmhouse tier's bigger loads exist, or as a supplement alongside solar rather than the sole source. Wind also depends on the property actually being windy enough — tie turbine output to Weather_System.md's existing Wind value rather than a flat number, so a calm stretch produces less, same as a cloudy stretch cuts solar.

**5. Generator sizing (backup, not primary):** sized to the **peak simultaneous load**, not the daily total — the well pump's 1,500W starting surge matters more than its average draw. A 3–5 kW dual-fuel or propane generator covers every tier in the table above with headroom, used as backup for cloudy/calm stretches rather than running continuously (real-world rule of thumb: 30–60 generator-hours per year for a solar-primary system). This is the option that doesn't depend on weather at all, at the cost of needing fuel (a new consumable — Gasoline or Propane, sourced from the Trading Post, same pattern as Lumber).

**6. Inverter sizing:** sized to the highest load ever running at once (not the daily total), with headroom for a motor's starting surge — a well pump's ~1,500W running draw might briefly surge toward 3,000W on startup, so the inverter needs to cover the surge, not just the steady draw.

---

# Generation Options

## Gas Generator

Reliable regardless of weather, at the cost of a fuel supply chain (Gasoline/Propane, new consumables — not yet in Item_Data.md). Proposed as the default backup for whichever primary source (solar or wind) the player builds, sized per the methodology above (3–5 kW typical, covering every tier through Farmhouse with headroom). Fits the existing Trading Post pattern for a consumable the player can buy rather than produce.

## Solar (Solar Shed)

Already named as a Utility Structure in Building_Housing_System.md — this gives it real numbers. Output scales with Weather_System.md's existing weather state (less on an overcast day, consistent with how Weather already affects other systems) rather than a flat rate. Panel wattage sized per the methodology above for whichever tier the player is building toward.

## Wind (Windmill)

Also already named as a Utility Structure. Output tied to Weather_System.md's existing Wind value rather than a flat rate, same as Solar ties to cloud cover — a calm day produces little, a windy one produces more. Oversized relative to the Large Cabin tier's needs even at a small 1 kW rating, so most useful once Farmhouse-tier loads exist, or as a supplement running alongside Solar rather than the sole source.

---

# Battery Bank

A new storage structure (parallel to the existing primitive storage piles' "offload point" pattern, but for stored energy rather than physical goods), sized per the Sizing Methodology's battery formula above. Chemistry (lead-acid/AGM vs. lithium/LiFePO4) is a game-balance call for Claude Code when this is actually built — real lithium is lighter, more efficient, and more expensive, which is a natural fit for this project's existing tiered-quality pattern (the same ascending-quality shape as wood species: Pine → Birch → Oak → Walnut, or water quality Excellent → Unsafe) rather than a single fixed spec.

---

# What Draws Power (first-pass list)

Not locked — update as the systems they belong to get built:

- Large Cabin / Large Cabin+ indoor running water pump (Building_Housing_System.md) — the first real load, ~1,500W peak, intermittent.
- Interior lighting, once Building/Housing has a real interior (vs. the current single-collider-shell simplification noted for Small Cabin).
- Flashlight battery charging (Core_Survival_System.md's new Portable Lighting section — Flashlight is Future, gated on this doc defining a battery item).
- Workshop power tools, once Workshop is built.
- Refrigeration (an Icebox/Root Cellar upgrade), a washing machine, and similar Farmhouse-tier appliances.

---

# Design Rules

1. Nothing in Alpha 0.1 requires power — this system stays Future until Building/Housing reaches a tier (Large Cabin or above) that actually needs it.

2. Generation sizing always follows real consumption, not the other way around — figure out the load first (Sizing Methodology step 1), then size generation and storage to match, same as a real off-grid system.

3. Weather-dependent sources (Solar, Wind) tie into Weather_System.md's existing weather/wind state rather than producing a flat rate — a calm, overcast stretch should visibly affect output, giving the generator (or stored battery capacity) a real reason to exist.

4. A gas generator is backup, not a primary source, in every tier this doc covers — matches the real-world rule of thumb this research is grounded in (30–60 generator-hours/year for a solar-primary system).

5. Figures in this doc (load tiers, panel/battery/generator sizes) are a first-pass research baseline, not locked numbers — same propose-then-confirm status as every other number in this doc set, to be tuned once there's something in Unity to playtest against.

---

# Cross-References

- Building_Housing_System.md — Large Cabin's conditional indoor running water (the first real load), and the existing Utility Structures list (Well House, Windmill, Solar Shed) this doc gives real numbers to.
- Weather_System.md — Wind value (Windmill output) and cloud/weather state (Solar output).
- Core_Survival_System.md — Portable Lighting (Flashlight is gated on this doc defining a battery item).
- Item_Data.md — Gasoline/Propane and battery items are new-item gaps, not yet added (nothing buildable yet to need them).

---

# Sources

Real-world off-grid sizing data and formulas this document's numbers are grounded in:

- [Off-Grid Solar for Homesteads and Cabins: What You Actually Need (2026 Guide)](https://www.howtogosolar.org/off-grid-solar-homestead/)
- [Complete Off-Grid Cabin Power Setup (2026): Three Full Builds](https://www.offgridbenchmark.com/guides/complete-off-grid-cabin-power-setup/)
- [Off-Grid Daily Energy Needs: How to Calculate](https://voltcalcs.com/blog/off-grid-load-calculation-guide)
- [Small Wind Turbine Size by Power Rating (With Charts)](https://www.attainablehome.com/small-wind-turbine-size-by-power-rating/)

---

# Mike's answers (2026-11-28): the first power system

1. **What it runs first:** the Greenhouse Thermostat Controller, an electric pump on the Drilled Well, electric lights, Workshop power tools, and battery-powered (cordless) hand tools: a Chainsaw, an Impact Driver, a Circular Saw and a Work Light (a generic brand, no real names).
2. **Sources:** solar panels and a battery bank, with a gas generator as backup (Gasoline is already in the game). Wind comes in a later slice.
3. **Depth:** a real energy budget. Every game hour the panels make watt-hours by the sun and the cloud, the batteries store them, each powered thing draws its real watts, and the generator starts by itself when the batteries run low.
4. **Getting it:** buy the pieces and install them yourself, or hire an electrician by phone to do it all (more money, less work).
5. **Wiring:** run it yourself. Buried outdoor cable, bought by the foot: with a Wire Spool in hand, click the Power Shed, then the thing to power; a shallow trench line shows, and a longer run takes more cable.
6. **Panels:** ground racks on open, sunny ground (trees that shade them cut their output). Roof mounts on the Pole Barn come later.
7. **Buying:** pieces that add up, not kits: 400 W panels, 5 kWh battery modules, one inverter and a generator, sized to what is run; the Power panel shows whether it is enough.
8. **Cordless tools:** they take battery packs, which recharge in a Charger wired to the system (it shows as a load).
9. **Watching it:** both a Power app on the phone and a panel at the Power Shed (E): battery charge, what is making power and what is using it, and the generator's fuel.

# Real prices (checked 2026-11-28)

| Piece | Real price | Notes |
|---|---|---|
| 400 W solar panel | $200 to $350 each for small DIY jobs ($0.80 to $1.40 a watt) | game: $260 |
| 5.12 kWh LiFePO4 rack battery | $212 to $253 a kWh; one well-known 48 V 100 Ah module $1,199 | game: $1,200 |
| Off-grid inverter/charger | 3 kW about $700; 6 kW (8 kW of solar input) about $1,550 | game: one 6 kW, $1,550 |
| 10/2 UF-B buried cable | $1.20 to $2.71 a foot | game: $1.50 a foot |
| 5,000 W gasoline generator | about $1,100 to $1,800; 0.4 to 0.7 gal an hour at half load | game: $1,200, 0.55 gal (2.1 L) an hour at half load |
| Installed solar (an electrician) | small systems $2.50 to $3.50 a watt installed | the hire price is built from this |

# Slices (proposed)

- **E1, the power system:** the Power Shed (the inverter, with battery modules stored in it), Solar Panel Racks, the Generator, the Wire Spool and buried runs, the hourly energy budget, the Power app and the shed's panel, and the first load: electric lights in the cabin, the Pole Barn and the greenhouses, on from dusk.
- **E2, powered upgrades:** the Greenhouse Thermostat Controller and the Drilled Well's electric pump.
- **E3, power tools:** the Charger and battery packs, the cordless Chainsaw, Impact Driver, Circular Saw and Work Light, and Workshop power tools.
- **E4, the electrician:** hire the whole system by phone.
# As built: slice E1 (2026-11-28, awaiting Mike's play-test)

**The parts (Trading Post, Stocked, at the real prices above):** Solar Panel (400 W, $260), Battery Module (5.12 kWh, $1,200), Power Inverter (6,000 W, $1,550), Generator (5,000 W, $1,200) and Buried Cable ($1.50 a foot). Each has an Inventory tip, and buying them is booked under "Bought: building materials" in the ledger.

**The three structures (Build tab, under Other):**
- **Power Shed:** 1 Power Inverter + 12 Lumber, needs the Hammer. R installs Battery Modules from the pack (up to 8). E opens the shed's power panel. R with nothing to install starts or stops laying cable from it.
- **Solar Rack:** 6 Lumber, needs the Hammer. R mounts Solar Panels (up to 4; the rack shows each one). Trees within 12 m shade it, 15% a tree, at most 90%.
- **Generator:** placed from 1 Generator. R loads Gasoline cans (up to 5). It starts by itself below 20% charge and stops above 80%, burning 0.25 gal an hour idling up to 0.85 gal at full load.
- Dismantling returns what is inside, and the Inverter and Generator come back whole.

**Wiring:** start at the Power Shed (R), walk to a building and press E. The run takes Buried Cable from the pack by the foot (metres x 3.28, rounded up; 150 m at most), and a trench line is drawn. These buildings can be wired: the Cabin, the Pole Barns, the walk-in greenhouses, the Workshop, the Potter's Shed, the Smithy and the Kitchen.

**The hourly budget (PowerRules.Hour):** panels make panels x 400 W x 0.8 x sun height^2 x clear sky x (1 - shade) watt-hours. Batteries are kept above 10%. No inverter, or more than 6,000 W of load, means no power. When the batteries can't carry the load, the loads go off and whatever comes in still charges them.

**The first load, lights:** every wired building gets electric lights from dusk to 10:30 pm and from 5:30 am to sunrise, while the system has power.

**Watching it:** the phone's new Power app (after Market) and the shed's panel both show charge, solar, generator, load and fuel.

**Know-how:** entry 38, "Power: Solar, Batteries and the Generator", unlocked by placing a Power Shed.

**Art:** structure_powershed, structure_solarrack (Panel_1 to Panel_4), structure_generator, and the five item models; build and item icons rendered. The icon renderer now keeps the long Solar Rack in the usual front view (ItemIconRenderer.SeenAsBuilding).

**Tests:** PowerE1Tests (11), and the count tests: 100 PileKinds ending with Generator, 38 know-how entries, 170 market rows, 99 stocked, 91 sell back, 62 in the Stocked group, and the phone's ten apps.

**Not in E1 (the later slices):** the Thermostat Controller and well pump (E2), the cordless tools and Charger (E3), and hiring an electrician (E4).