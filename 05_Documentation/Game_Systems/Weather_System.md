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

# Precipitation Type by Temperature

Confirmed 2026-09-24 (Mike, from playtest): he saw Rain at 19°F in Winter, which read as wrong — precipitation type should follow the actual temperature, not just season-based odds. Whenever WeatherManager would have precipitation fall as Light Rain, Heavy Rain, or Thunderstorm and the current temperature is at or below freezing (32°F / 0°C), it should fall as Snow instead, regardless of season. This can happen outside Winter too — a Cold Front cold snap in Spring or Fall, say — not just Winter's own weather odds. This is a correction to which weather type actually gets presented, not a new mechanical effect: Snow's documented gameplay effects (reduced travel speed, surface water freezing, reduced foraging, increased firewood consumption) apply whenever it resolves this way, same as any other time it's Snow. Whether this is implemented as a check at weather-roll time or a resolve-time override, and how Thunderstorm specifically should behave below freezing (still thunder and lightning, just with snow instead of rain falling), are Claude Code's call.

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

Displayed to the player as Fahrenheit (confirmed 2026-09-24) — WeatherManager's internal values can be stored in whatever unit is convenient for the code, but any UI showing temperature (the pause screen's season/year/time/temperature readout, or anything similar later) should convert to °F before display, matching this doc's own freezing-point references above.

---

# Visual Feedback

Confirmed 2026-09-24 (Mike): weather that includes precipitation should render visible particles, not just the cloud cover, fog, and exposure dimming Claude Code already built (2026-09-24 DayNightCycle fix). This is presentation of the gameplay state already defined above, not a new gameplay effect — Design Rule 1 still holds, and none of the mechanical effects per weather type change.

- **Light Rain** — light, sparse rain particles.
- **Heavy Rain** — denser, heavier rain particles, consistent with Heavy Rain's already-documented reduced visibility.
- **Thunderstorm** — same rain particle treatment as Heavy Rain; this doc doesn't define a separate precipitation intensity for storms. Also gets lightning and thunder — see below.
- **Snow** — falling snowflakes, Winter's primary weather condition.
- **Clear, Cloudy, Cold Front, Wind** — no precipitation particles; none of these are precipitation types.

Confirmed 2026-09-24 (Mike): precipitation should also respond to overhead cover — lighter under partial cover (tree canopy) and none at all under full cover (inside a structure or building). Still presentation only, not a new gameplay effect — cover doesn't change WeatherManager's state or any documented mechanical effect, only how much of the particle presentation reaches the player. This should be a general overhead-occlusion check (e.g. a raycast or overlap test above the player), not logic specific to trees, so it also covers Building_Housing_System.md's future buildings once they exist without needing a separate ask later — that system is still a Design Draft with no buildable structures in Alpha 0.1 yet, so today this only affects tree canopy.

Confirmed 2026-09-24 (Mike): snow should also visibly accumulate on the ground while the temperature is below freezing (32°F / 0°C), not just fall as particles. It builds up while it's actively snowing and the temperature is below freezing, keeps lingering once the snow weather itself ends as long as the temperature stays below freezing (matching how real snow persists through Winter rather than vanishing when the weather type changes), and gradually melts away once the temperature rises back above freezing — which can happen mid-Winter during a warm spell, not only at the season change. Accumulation should follow the same overhead-cover logic as falling precipitation above: less builds up under tree canopy, none under full cover, so open pasture reads snowier than the forest floor, consistent with how the snow fell there in the first place. Still presentation only — no new mechanical effect, and none of Snow's documented gameplay effects (reduced travel speed, surface water freezing, reduced foraging, increased firewood consumption) change based on how much is visibly on the ground.

Confirmed 2026-09-24 (Mike, from playtest): snow accumulation should also show on tree tops, not just the ground — right now snowfall visibly builds up on open ground but tree canopies stay bare, which reads wrong once the ground nearby is white. Same presentation-only framing as ground accumulation above: builds up on canopy tops while it's actively snowing and below freezing, and melts on the same terms as ground snow. Technique (a snow-dusted canopy material variant, a decal/overlay pass, shader-based snow blend, etc.) is Claude Code's call — whatever fits how the tree prototypes are currently built (per `OverheadCover.cs`'s existing read of the terrain's tree instance data).

Confirmed 2026-09-24 (Mike): Thunderstorm should also get lightning and thunder, layered on top of its rain particle treatment above. Lightning is a brief bright flash — most strikes read as a distant flash on the horizon with no visible bolt, with an occasional closer strike showing one. Thunder is the matching one-shot sound (`sfx_thunder`, new — see Audio_System.md), timed with a delay after its flash that scales with how far away the strike reads as being: a closer strike gets a sharp crack close behind the flash, a distant strike a longer, duller rumble after a longer delay — the way real lightning and thunder are heard apart. Purely presentation, same as the rest of this section: it doesn't add a new mechanical effect on top of Thunderstorm's documented "dangerous working conditions / poor visibility / livestock stress / risk of storm damage to structures" list above, since those stay abstract gameplay effects until their own systems (Building_Housing_System, a future livestock manager) actually exist to receive them.

Exact particle system choice, density, performance tuning, the occlusion check's implementation (raycast height/radius, sample count, transition smoothing), snow accumulation's technique, buildup/melt rate and max depth, and lightning's flash frequency/intensity/distance-delay curve are Claude Code's call.

---

# Design Rules

1. Weather is a gameplay system, not decoration.

2. Prepared players are less affected by bad weather.

3. Weather should influence daily decision-making.

4. Winter weather should be the most demanding of the year.

5. No two years should feel identical.
