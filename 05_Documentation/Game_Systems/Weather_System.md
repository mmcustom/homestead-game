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

**Added 2026-09-26 (Mike):** Thunderstorms are now one of the two main triggers for natural tree fall (windthrow) — a standing tree coming down on its own, independent of the player chopping it. See Wood_Gathering_System.md's new Natural Tree Fall section for the full spec; exact odds are Claude Code's call.

---

## Cold Front

Effects:

- Sharp temperature drop
- Increased exposure risk
- Wildlife movement changes

Cold Fronts are referenced in Fishing System documentation as a factor in fish activity.

---

## Heat Wave

**Added 2026-10-07 (Claude, first pass, not yet confirmed):** the warm-season mirror of Cold Front. A multi-day stretch of unusually hot, humid weather that makes heat a real survival concern instead of just a number on the pause screen. Full spec in "Heat Inputs for the Core Temperature Model" below.

Effects:

- Sharp rise in temperature and humidity that lasts several days
- Warm nights, so the body gets little chance to recover overnight
- Much higher overheating risk for anyone working in the sun
- Faster Hydration loss from sweating
- Wildlife movement changes (animals seek shade and water; Wildlife and Fishing tuning is not designed yet)

Heat Waves are referenced in Health System documentation as the main driver of heat exhaustion and heat stroke.

---

## Snow

Effects:

- Reduced travel speed
- Surface water freezing
- Reduced foraging
- Increased firewood consumption

Winter's primary weather condition.

**Added 2026-09-26 (Mike):** a long winter's worth of accumulated Snow is the other main trigger for natural tree fall (windthrow) — read as snow load bearing down on branches and trunks over the season, not any single snowfall. See Wood_Gathering_System.md's new Natural Tree Fall section.

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

# Heat Inputs for the Core Temperature Model

**Added 2026-10-07 (Claude, first pass, not yet confirmed).** Mike chose to build the Core Temperature Model (Health_System.md) for cold and hot together. Claude Code's review found the hot side unbuildable because the weather only has a temperature (about 34 °C at most). This section designs what the weather has to add. Real-world numbers and sources are in Research/Human_Thermoregulation.md ("Hot-Side Weather Inputs"). Every number below is a starting point for tuning, not a decision.

Four additions: humidity, sun load, Heat Waves, and Health tiers for overheating. The first three are weather inputs and live here; the fourth is a Health consequence and is summarized here and specified in Health_System.md.

## 1. Humidity

A new standing value, relative humidity from 0 to 100 percent, present at all times the way Wind is. It is a weather input only. It does nothing by itself; it changes how well sweat cools the player.

- **Daily shape:** humidity is lowest in mid-afternoon (when it is hottest) and highest near dawn, the reverse of temperature. Real air does this and it makes dawn and dusk feel better to work in.
- **Weather drives it:** Clear roughly 35–65 percent; Cloudy 55–80; Light Rain 75–95; Heavy Rain and Thunderstorm 85–100; Snow and Cold Front are dry-to-moderate. After rain it stays high for a day, then falls.
- **Season:** Summer is the most humid season (Ohio summers are humid), Winter and early Spring the driest outdoors-felt. Exact curves are Claude Code's call.
- **Heat Waves raise it** by about 15 points over the day's normal.

How it affects the body: sweat only cools when it evaporates, and humid air slows evaporation. First pass for the **evaporation factor** (the fraction of sweat that actually cools you): 1.0 at 30 percent humidity or below, about 0.65 at 60, about 0.3 at 90, about 0.2 at 100. Wind raises it toward 1.0. Dehydration lowers how much sweat there is to evaporate (see Health_System.md).

Player-facing: the pause screen can show a **feels-like temperature (heat index)** in °F when it is above about 80 °F, computed from temperature and humidity. It is a summary for the player only; the body model uses temperature, humidity, sun and activity directly, not the heat index. For orientation, the standard National Weather Service bands are: Caution 80–90 °F, Extreme Caution 90–105 °F, Danger 105–130 °F, Extreme Danger 130 °F and up.

## 2. Sun Load

The sun heats a person who stands in it. An older (1940s) field study put the extra heat load on a standing, clothed person in full sun at roughly 270 W (about 3.4 times resting heat production), and white clothing roughly halves it while dark clothing raises it. That one number is larger than a person's whole resting heat production, which is why shade matters so much on a hot day.

A new **solar exposure** value from 0 to 1, then multiplied into heat gain:

- **Sun height:** zero at night, small near dawn and dusk, highest at midday. Summer midday is the maximum; Winter midday sun is low and gives roughly a third to a half of that.
- **Cloud:** Clear 1.0, Cloudy about 0.4, Light Rain about 0.25, Heavy Rain, Thunderstorm and Snow about 0.15.
- **Overhead cover:** reuse the existing overhead-occlusion check (`OverheadCover.cs`). Open sky 1.0, partial canopy in between, and full cover (inside a structure) drops direct sun to zero. Real shade still leaves some sky and ground radiation; first-pass placeholder is about 50 W in canopy shade at midday, which Claude Code can tune.
- **Heat gain** in watts = solar exposure × about 270 W.

This applies in cold weather too. A sunny winter noon is a small gain that offsets some cold loss, which is real and makes shelter, clearings and timing matter in Winter. Claude Code should include it when tuning the cold baseline.

## 3. Heat Waves

A Heat Wave is a weather type, the mirror of Cold Front (see Weather Types above). First-pass behavior:

- **When:** Summer only. Average about one per Summer; some Summers have none and some have two, so no two years feel the same (Design Rule 5).
- **Length:** about 3–6 in-game days, with about a day to build and a day to ease off.
- **Temperature:** about +6 °C (+11 °F) above that day's normal for an ordinary Heat Wave, with **no design ceiling** (decided 2026-10-07, Mike: some regions go well past 100 °F, so a 100 °F cap is wrong). Stronger Heat Waves should be possible and rarer, for example +10 °C or more. The current weather caps near 34 °C (93 °F); that cap is lifted for Heat Waves. Claude Code may keep an Inspector-tunable safety limit in code (first-pass default about 46 °C / 115 °F; real-world extremes run higher still) so a bad roll cannot produce nonsense. That is a code guard, not a design limit, and it can be raised.
- **Nights stay warm:** overnight cooling is roughly halved, so the body does not recover during sleep. This is what makes real heat waves dangerous.
- **Humidity:** +15 points, per above.
- **Sky:** mostly Clear. A Heat Wave usually ends with a Thunderstorm or Cold Front, which also gives the player relief and fits existing weather behavior.
- **No warning:** forecasting is not in Alpha 0.1 (see Forecasting above), so a Heat Wave arrives as it happens. The player notices it by feel: the sun load, the sweat, and the HUD line.
- **Water link:** a Heat Wave should raise Hydration use and could dry out ponds and shallow water. The pond effect is not designed; it belongs to Water_System.md and is an open question below.

## 4. Overheating Health Tiers (summary)

Specified in full in Health_System.md. Core temperature tiers from the research: sweating starts near 37.5 °C; heat exhaustion starts near 38 °C (Claude Code's number; the sources say 37–40); heat stroke is above 40 °C, where sweating stops and the brain is affected.

| Core temperature | State | Effect (first pass) |
|---|---|---|
| Up to 37.5 °C | Normal | None |
| 37.5–38.0 °C | Hot | Sweating; HUD line "Hot"; no penalty |
| 38.0–39.0 °C | Heat exhaustion | Stamina recovery cut to about 70 percent; HUD line |
| 39.0–40.0 °C | Severe heat exhaustion | Stamina recovery cut to about 40 percent, sprinting not allowed, mild screen effect, slow Health loss |
| Above 40 °C | Heat stroke | Sweating stops (no cooling), Health drains steadily, strong screen effect |

**Decided 2026-10-07 (Mike): heat is handled like cold.** Overheating does not kill outright. When heat drains Health to 0 the player passes out and wakes weakened, with penalties that grow with repeated collapses, using the same Player Collapse system as cold (Health_System.md). The tiers above are the warning steps on the way there.

## Counterplay (Design Rule 2: prepared players are less affected)

- **Shade and shelter:** the biggest lever. Canopy, a Lean-To, a Tent or a Cabin cuts or removes sun load.
- **Timing:** work at dawn and dusk, rest through the midday peak, and sleep through hot nights where there is shade. The schedule the player keeps becomes a survival choice.
- **Water:** drinking keeps Hydration up, which keeps sweating working. This is the main reason dehydration is the real heat-stroke driver.
- **Getting wet:** water conducts heat about 25 times faster than air, so wading into a pond or creek is a fast, real way to cool. How this works with the damp rule is for Claude Code and Water_System.md.
- **Wind:** raises the evaporation factor.
- **Easing the work:** lower activity lowers heat production directly. Resting in shade cools fastest.

## Tuning targets (Claude's estimates, for Claude Code to check against)

These describe what a tuned model should roughly produce. They are not formulas.

| Situation | Expected result |
|---|---|
| 75 °F, 50 percent humidity, working in shade | Comfortable; core stays near 37 °C; Hydration use only slightly up |
| 90 °F, 60 percent humidity, resting in shade, with water | Core stays about 37.0–37.3 °C indefinitely |
| 90 °F, 60 percent humidity, hard work in full sun, with water | Hot within roughly the first game hour, heat exhaustion within a few hours without a rest in shade |
| Heat Wave, about 100 °F, 70 percent humidity, resting in shade, with water | Slowly climbing; heat exhaustion over many hours, avoidable by staying cool and wet |
| Heat Wave, hard work in full sun, no water | Heat stroke within a few hours; the dangerous case |
| Severe Heat Wave, 110 °F or more | Dangerous even resting in shade without water; shade, wading, water and resting through midday are the way through |

## Dependencies and open questions (Claude, not yet confirmed)

- **Seasonal clothing assumption (important). Confirmed 2026-10-07 (Mike): OK for now, until clothing exists.** Claude Code's review set the cold baseline at about 2 clo of insulation. That much clothing in a Summer heat wave would trap heat and make the hot side unfair, and the game has no clothing yet. First-pass fix: until clothing exists, the model assumes the player dresses for the season automatically, from about 2 clo in Winter down to about 0.5 clo in Summer, interpolated by season or air temperature. When clothing is built, it replaces this assumption.
- **HUD for overheating.** Warmth stays a cold-side meter pegged at 100 above 37.0 °C. First pass: a separate overheating indicator appears only when core temperature is above 37.5 °C, and Warmth is not made two-sided. Not confirmed.
- **Pond drying in Heat Waves** is not designed (Water_System.md).
- **Heat Wave odds and strength** are first-pass values, to be tuned after the cold side is tested. The 38 °C ceiling was withdrawn 2026-10-07 (Mike); see Heat Waves above.
- **Wildlife and Livestock** reactions to heat are not designed; Livestock heat stress is a likely future link.
- **Difficulty:** the Pioneer penalty applies in the harmful direction only (hotter and sunnier for heat). See Health_System.md review corrections.

Per the GDD Development Rule, none of this is built until the core survival loop is proven fun. Mike chose cold and hot together as the build scope, so this section is what the hot side is waiting on.

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

Heat and occasional thunderstorms. Heat Waves (first pass, see "Heat Inputs for the Core Temperature Model").

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
