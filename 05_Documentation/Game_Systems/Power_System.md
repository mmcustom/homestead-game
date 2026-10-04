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
