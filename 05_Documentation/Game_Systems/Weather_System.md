# Homestead
## Weather System v1.0

Status: Design Draft

---

# Purpose

The Weather System creates environmental challenges and opportunities.

Weather should influence:

- Survival
- Water availability
- Wildlife behavior
- Fishing
- Hunting
- Livestock management
- Building projects
- Daily workload decisions

Weather helps ensure that no two years feel exactly the same.

---

# Core Philosophy

Weather is not a visual effect.

Weather is a gameplay system.

Players should consider weather when planning:

- Work schedules
- Food harvesting
- Hunting trips
- Building projects
- Livestock care

Prepared players handle bad weather better.

---

# Weather Types

## Clear

Effects:

- Best visibility
- Normal travel
- Ideal building conditions

---

## Cloudy

Effects:

- Lower visibility
- Neutral gameplay impact

---

## Light Rain

Effects:

- Reduced comfort
- Fire starting becomes harder

Benefits:

- Water replenishment

---

## Heavy Rain

Effects:

- Fire maintenance becomes difficult
- Reduced visibility
- Muddy terrain

Benefits:

- Refills ponds
- Refills cisterns
- Improves soil moisture

---

## Thunderstorm

Effects:

- Dangerous working conditions
- Poor visibility
- Livestock stress
- Risk of storm damage to structures

Players should seek shelter rather than work through a thunderstorm.

---

## Cold Front

Effects:

- Sharp temperature drop
- Increased exposure risk
- Wildlife movement changes

Cold Fronts are referenced in Fishing System documentation as a factor in fish activity.

---

## Snow

Effects:

- Reduced travel speed
- Surface water freezing
- Reduced foraging
- Increased firewood consumption

Winter's primary weather condition.

---

## Wind

Wind exists two ways: as a standing direction and strength present at all times (even during Clear weather), and as its own weather type when wind effects become dominant enough to be the defining condition. Confirmed 2026-09-23 (Mike) — both roles stay.

Effects:

- Affects scent detection during hunting
- Reduces fire efficiency
- Increases exposure risk in cold conditions

Wind direction is a factor in Hunting System documentation.

---

# Exposure Connection

Sustained bad weather without adequate shelter contributes to the Exposure System.

Examples:

Thunderstorm, Cold Front, or Snow
↓
Insufficient Shelter
↓
Exposure Risk

Full exposure effects are defined in Health System documentation.

---

# Discovery Integration

Weather patterns are not discovered like locations, but their effects on discovered resources are recorded.

Example:

Journal Note

Bass Hole — Slower After Cold Fronts

---

# Forecasting

Not Alpha 0.1. Players get no advance weather information by default — conditions arrive as they happen. Confirmed 2026-09-23 (Mike): a future Radio (or similar item) should unlock forecasting once it exists. No such item is designed yet; this is a confirmed direction, not a built system. See Feature_Backlog.md.

---

# Season Effects

## Spring

Frequent rain.

Unpredictable conditions.

---

## Summer

Heat and occasional thunderstorms.

Drought risk in dry stretches.

---

## Fall

Cooling trends.

Increasing wind.

Light snow can occasionally occur late in the season. Confirmed 2026-09-23 (Mike).

---

## Winter

Snow and cold fronts dominate.

Weather becomes a central survival concern.

Detailed seasonal behavior is defined in Season System documentation.

Exact per-season weather odds, temperature ranges, and duration numbers are implemented and Inspector-tunable in WeatherManager.cs; Mike has accepted Claude Code's real-world-grounded first pass (confirmed 2026-09-23) rather than hand-setting each value here.

---

# Visual Feedback

Confirmed 2026-09-24 (Mike): weather that includes precipitation should render visible particles, not just the cloud cover, fog, and exposure dimming Claude Code already built (2026-09-24 DayNightCycle fix). This is presentation of the gameplay state already defined above, not a new gameplay effect — Design Rule 1 still holds, and none of the mechanical effects per weather type change.

- **Light Rain** — light, sparse rain particles.
- **Heavy Rain** — denser, heavier rain particles, consistent with Heavy Rain's already-documented reduced visibility.
- **Thunderstorm** — same rain particle treatment as Heavy Rain; this doc doesn't define a separate precipitation intensity for storms.
- **Snow** — falling snowflakes, Winter's primary weather condition.
- **Clear, Cloudy, Cold Front, Wind** — no precipitation particles; none of these are precipitation types.

Exact particle system choice, density, and performance tuning are Claude Code's implementation call.

---

# Design Rules

1. Weather is a gameplay system, not decoration.

2. Prepared players are less affected by bad weather.

3. Weather should influence daily decision-making.

4. Winter weather should be the most demanding of the year.

5. No two years should feel identical.
