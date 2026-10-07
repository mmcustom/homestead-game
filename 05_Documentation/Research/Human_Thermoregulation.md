# Homestead
## Human Thermoregulation Research v1.0

Status: Research notes, 2026-10-07 (Claude, requested by Mike). Not a design spec: this records real-world numbers so the Warmth system can be built as accurately as Mike wants. The design that uses them is in Health_System.md ("Core Temperature Model"). For game use only, not medical advice.

---

# Why This Exists

Mike (2026-10-07), after a Pioneer playtest where Warmth dropped fast: "we are 'humans' and we all have our 'best temperature' ranges that we can tolerate. But on average humans have a minimum and a maximum temperature that we can physically tolerate before our bodies shut down. lets build this as accurate as possible by checking our research on the internet and go from there."

Caveat on sources: most numbers below come from Wikipedia and general safety or physiology pages, not from clinical textbooks, and one source's text and table disagree slightly. Treat each as a good approximation. Where a number is my own arithmetic, it says so.

---

# Core Body Temperature Ranges

- **Normal core temperature:** about 37 °C (98.6 °F). Healthy adults average about 36.8 °C (98.2 °F); the normal range is about 36.5–37.5 °C (97.7–99.5 °F). The body regulates inside a narrow band: shivering starts near the bottom of it (about 36.5 °C) and sweating near the top (about 37.5 °C). ([Wikipedia: Hypothermia](https://en.wikipedia.org/wiki/Hypothermia), [Deranged Physiology](https://derangedphysiology.com/main/cicm-primary-exam/thermoregulation/Chapter-123/normal-temperature-thermoneutral-zone-and-inter-threshold-range), [Wikipedia: Thermoregulation](https://en.wikipedia.org/wiki/Thermoregulation))

## Cold side (hypothermia)

| Stage | Core temp | What happens |
|---|---|---|
| Mild | 32–35 °C (89.6–95 °F) | Shivering, mental confusion, poor coordination (can still walk and talk) |
| Moderate | 28–32 °C (82.4–89.6 °F) | Shivering stops, confusion increases, slurred speech, irrational behavior (including paradoxical undressing) |
| Severe | 20–28 °C (68–82.4 °F) | Unconsciousness, no shivering, increased risk of cardiac arrest |
| Profound | below 20 °C (68 °F) | No vital signs, cardiac arrest |

- Hypothermia is defined as core temperature below 35 °C (95 °F). Ventricular fibrillation is common below 28 °C and asystole below 20 °C. Shivering can fade as glycogen runs out, even before the body is truly cold. ([Wikipedia: Hypothermia](https://en.wikipedia.org/wiki/Hypothermia), [Outdoor Action guide](https://nasdonline.org/1415/d001216/hypothermia-and-cold-weather-injuries-outdoor-action-guide.html))
- The outdoor guide gives slightly different ranges in its text than in its table, so exact boundaries vary by source: about 95–93 °F for moderate and about 92–86 °F for severe.
- A hypothermic person shouldn't be declared dead until rewarmed above about 32 °C (90 °F). ([Wikipedia: Hypothermia](https://en.wikipedia.org/wiki/Hypothermia))

## Hot side (hyperthermia)

- **Heat exhaustion:** core temperature about 37–40 °C (98.6–104 °F). Profuse sweating (1–2 L per hour), weakness, dizziness, headache, nausea, cramps, fast heart rate, low blood pressure. ([Wikipedia: Heat exhaustion](https://en.wikipedia.org/wiki/Heat_exhaustion))
- **Heat stroke:** above 40 °C (104 °F) with central nervous system dysfunction: confusion, seizures, loss of coordination, and sweating stopping. It can lead to organ failure and death if untreated. The page gives no specific lethal temperature. ([Wikipedia: Heat exhaustion](https://en.wikipedia.org/wiki/Heat_exhaustion))
- Humidity matters: the body's main cooling route in heat is sweat evaporation, and humid air reduces it. When air is warmer than skin, evaporation is the only way to shed heat. ([Wikipedia: Thermoregulation](https://en.wikipedia.org/wiki/Thermoregulation))

---

# Comfort Ranges (the "best temperature")

- **Thermoneutral zone** (the air temperature range where the body holds its core temperature using only skin blood flow, with no shivering or sweating): about 28–32 °C (82–90 °F) for an unclothed adult, and about 14.8–24.5 °C (59–76 °F) for a lightly clothed adult. ([Deranged Physiology](https://derangedphysiology.com/main/cicm-primary-exam/thermoregulation/Chapter-123/normal-temperature-thermoneutral-zone-and-inter-threshold-range))
- So "comfortable" depends on clothing. The game's current Warmth comfort line (50 °F feels-like) is well below the lightly clothed zone. That is reasonable for a heavily dressed player, but the game has no clothing yet (Health_System.md says clothing stays Future).

---

# Heat Production (Why Exercise Warms You)

- **1 MET** is resting heat/energy use: 3.5 ml O₂/kg/min, roughly 1 kcal/kg/hour. My arithmetic: about 1.16 W per kg, so about 81 W for a 70 kg person. ([Wikipedia: Metabolic equivalent of task](https://en.wikipedia.org/wiki/Metabolic_equivalent_of_task))
- **Activity levels (MET, a multiple of resting):** walking at 3 mph about 3.5; chopping wood 5; carrying 25–49 lb 5; digging with a shovel 5–6; running at 6 mph 10. ([Top End Sports MET table](https://www.topendsports.com/weight-loss/energy-met.htm), [Wikipedia: MET](https://en.wikipedia.org/wiki/Metabolic_equivalent_of_task))
- Most of that energy ends up as heat, so heavy work produces several times more heat than resting. That is why Mike's idea is physically right: working hard in the cold really does warm you.
- **Shivering** is the body's own involuntary exercise. Peak shivering is about 4.9 times resting metabolic rate on average (about 6.3 METs, roughly 530 W for a 72 kg person by my arithmetic), and it depends on fitness (higher VO₂max helps) and body composition (a higher BMI and older age reduce it). ([PubMed: peak shivering intensity](https://pubmed.ncbi.nlm.nih.gov/11394237/)) Visible shivering can raise heat production by up to about 500% but only for a few hours before glycogen depletion and fatigue. ([Outdoor Action guide](https://nasdonline.org/1415/d001216/hypothermia-and-cold-weather-injuries-outdoor-action-guide.html))
- A 60-minute walking/running test at 0 °C versus 22 °C (lightly clothed) found core temperature held steady in the cold while the runners burned more fat. Exercise at those intensities didn't make anyone cold. ([Frontiers in Physiology](https://www.frontiersin.org/journals/physiology/articles/10.3389/fphys.2013.00099/full))

---

# Heat Loss (What Makes You Cold)

- **Water** conducts heat about 25 times faster than air. **Wet clothing** raises conductive heat loss about 5-fold, and wet feet lose heat about 25 times faster than dry feet. ([Outdoor Action guide](https://nasdonline.org/1415/d001216/hypothermia-and-cold-weather-injuries-outdoor-action-guide.html), [University of Illinois cold stress page](https://www.drs.illinois.edu/Page/SafetyLibrary/ColdStress))
- **Wind** increases convective loss, which is what wind chill describes. Frostbite can happen above freezing because of wind chill. ([Outdoor Action guide](https://nasdonline.org/1415/d001216/hypothermia-and-cold-weather-injuries-outdoor-action-guide.html), [University of Illinois](https://www.drs.illinois.edu/Page/SafetyLibrary/ColdStress))
- **Sweat is a double-edged sword.** Exercise warms you, but sweat that soaks clothing increases heat loss. Hypothermia can occur above 40 °F (4 °C) if a person is chilled by rain, sweat or submersion. ([Outdoor Action guide](https://nasdonline.org/1415/d001216/hypothermia-and-cold-weather-injuries-outdoor-action-guide.html), [University of Illinois](https://www.drs.illinois.edu/Page/SafetyLibrary/ColdStress))
- Heat is also lost through breathing (inhaled air is warmed and exhaled fully humidified) and through evaporation of sweat. Evaporative losses also shrink circulating fluid, which raises dehydration risk and hypothermia susceptibility. ([Outdoor Action guide](https://nasdonline.org/1415/d001216/hypothermia-and-cold-weather-injuries-outdoor-action-guide.html))
- Alcohol dilates blood vessels and increases heat loss (not relevant to the game today).

---

# Not Found / Not Verified

- A reliable number for how fast a clothed person's core temperature falls at a given air temperature and wind speed (needs a heat-balance model with clothing insulation; none of the pages gave one).
- The body's heat capacity (how many watts of imbalance move core temperature by 1 °C per hour). The commonly cited figure is about 3.5 kJ/kg/°C, but I did not fetch a source for it this session. Claude Code or Mike should verify before it goes into a formula.
- Splitting logs' MET (not listed), and MET for carrying a heavy pack.
- A specific lethal temperature on the hot side.
