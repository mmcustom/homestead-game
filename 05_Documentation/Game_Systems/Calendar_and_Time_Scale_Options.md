# Homestead
## Calendar and Time Scale Options v0.1

Status: DECIDED 2026-11-08 (Mike): option D (real 365-day calendar) plus step 0 (consolidate the duplicated constants first) and step 3 (a player-facing time control). Build not started; plan-first request goes to Claude Code, builds wait until Mike finishes the Pioneer re-measure run. Written 2026-11-08 from a read-only code audit by Claude Code and Cowork's analysis. Nothing is changed in the game.

---

# Why this is being looked at

Mike's concern (2026-11-08): a 28-day season feels off, and budgeting would be more realistic with longer seasons (3 months each), possibly a real 365-day year. It also affects farming, winter, and the planned monthly bills (see `Modern_Systems_and_Services.md`).

# Today

One 28-day season, a 112-day year, 30 real minutes a game day (48x real time). The land loan is a 180-payment loan whose payment falls every 28 days at a monthly-sized amount, with interest computed as 1/12 of the annual rate per payment. So only 4 payments fall in a game year, each carrying 1/12 of the year's interest: the debt is about three times lighter than 8 percent implies, and the loan runs about 45 game years, not 15. (Money_System.md calls this the "month = 28-day payment" decision of 2026-10-20, option C.)

## The economic point that decides most of this

At $573 a payment, a real 8 percent, 15-year loan costs $6,876 a year. A 112-day year at about $20 a game day earns only about $2,300, so the game is only winnable today because the loan is deliberately light. With 84-day seasons (336-day year) and a payment every 28 days, 12 payments a year costs $6,876 against about $6,880 earned at $20.48 a game day: the money becomes honest without changing income per game day. So per-game-day measurements (the Pioneer runs) stay valid under options A and D. Option C (change only the loan cadence) cannot reach the same honesty: a shorter cadence at the same per-day pressure would need a payment smaller than the interest.

# Options

| | A. 84-day seasons, 28-day months, same day length | B. A plus a shorter real day | C. Keep calendar, change only the loan cadence | D. Real 365-day calendar, real months, no leap years |
|---|---|---|---|---|
| Year | 336 days | 336 days | 112 days | 365 days (Spring 92, Summer 92, Fall 91, Winter 90) |
| Real hours per year at 30 min/day | about 168 | about 56 to 112 | about 56 | about 182 |
| Loan | 12 payments a year, true 15 game years | same | stays about 45 game years | monthly on a calendar day, true 15 game years |
| Code | about 8 C# files for duplicated constants, 3 to 4 scene values | A plus about 20 hour-based constants in about 12 files | scene values plus loan defaults | about 20 to 25 files, a new CalendarRules class |
| Tests | about 35 to 45 methods if the loan defaults change | A plus playtesting | same as loan part of A | about 80 to 120 methods, plus new calendar tests |
| Saves | ledger needs migration or old saves declared incompatible | same | none (loan terms are frozen per loan) | incompatible without migration |
| Size | small to medium; the risk is tuning | medium to large, mostly balance | smallest | large |
| Real-world value | seasons last long enough for crops and winter | same | none | real frost, planting, sap and hunting dates; bills on the 1st |

# What each option does to feel

- **A:** wood seasoning, regrowth, annual forage and the year boundary all become about 3 times longer in real time. Heat waves per Summer triple. The first payment (day 112 today) would land in Summer unless firstDueDay is moved on purpose.
- **B:** hunger, thirst, fire and torch fuel, smelts and kilns all run faster in real minutes. Hunger drops from 100 to 0 in 90 real minutes today; at a 10-minute day it would take 30. Needs about 20 hour-based constants re-tuned or a single compensation factor. Swing, walking and stamina timers are in real seconds and do not change.
- **D:** same as A for feel, plus uneven months; the first statement is partial and the game starts on March 1. Winter spans the calendar year, so "Year" needs a definition.

# Findings from the audit that matter whichever option is chosen

1. **Duplicated constants.** Ledger, MarketManager, WoodMoisture, RegrowRules and LoanTerms.Default each carry their own 28 or 112. They agree only because every value is currently 28. The scene file (Bootstrap.unity) overrides the C# defaults, so editing a default alone silently does nothing.
2. **Four seasons is baked in** (hard-coded arrays and `% 4`). Fine for A and D; any change to the number of seasons is large.
3. **Tree regrowth is a fixed constant** (sprout 14, step 7, full 112) and does not follow the calendar; its stage math assumes exactly 14 steps of 7 days.
4. **Saves:** only totalDays and minuteOfDay are saved, so season and year are derived and an old save changes meaning. The ledger saves its own season length and index and would disagree with TimeManager after a change.
5. **Loan is flexible:** first due day, interval and grace are separate fields frozen into each loan; payment count and cadence are free parameters. Grace is 10 days, so a cadence under about 10 days would make a payment go missed before the next falls due.
6. **Bills:** no recurring non-loan expense exists. A Bills category is easy to add (append an enum id, bump the count, add a name); a failed bill must only set a service-suspended flag, never count as a missed loan payment. About 3 new files and 25 to 35 tests.
7. **Stamina** recovers per real second, not per game day; trap checks are hourly. Real-time stamina and swing timers would not change if the real day length changed.

# Suggested path (Cowork's view, not decided)

1. **Step 0, no behaviour change:** consolidate every duplicate so every system reads one calendar source, and derive regrowth stages from the calendar. This lowers the cost of every option and is useful on its own.
2. **Choose A or D.** A is the smaller and faster step and already makes the money honest; D adds real-world dates for a much larger change. Building A through a central calendar object (step 0) keeps D open for later.
3. **Ship a player-facing time control** (sleep skip, wait, fast-forward). Today the x10 and x60 speeds are debug keys only; a year of 168 to 182 hours makes this essential.
4. **Skip B for now** (day-length retune); revisit only if testing shows 30-minute days drag.
5. Pick `firstDueDay` and the statement period (per season of 3 payments, or monthly) on purpose.

---

# Build plan received 2026-11-08 (Claude Code, read-only)

Three phases, each its own build, commit and playtest checklist.

**Phase 0, one calendar source (no behaviour change, small):** a new pure `Economy/CalendarRules.cs` (today's numbers at first, with an immutable Calendar object and Real and Legacy instances). Every duplicate (TimeManager, Ledger, MarketRules and MarketManager, WoodMoisture, RegrowRules, LoanTerms.Default, MoneyManager, LossRules) reads it; the ledger period is decoupled from season length; the orphaned serialized fields are removed from Bootstrap.unity through Unity. About 12 files edited, 0 existing tests changed, 15 to 20 new tests including a guard test that every consumer equals the calendar. Outhouse pit fill (28 days) stays an independent gameplay tunable.

**Phase 1, the real calendar (medium to large):**
- Game year runs March to February; day 0 is March 1, so no epoch offset: dayOfYear = TotalDays % 365, Year = TotalDays / 365 + 1.
- Months from March: 31, 30, 31, 30, 31, 31, 30, 31, 30, 31, 31, 28 (start days 0, 31, 61, 92, 122, 153, 184, 214, 245, 275, 306, 337). Seasons: Spring 0 (92 days), Summer 92 (92), Fall 184 (91), Winter 275 (90).
- Loan: due on the 1st of each month (dueDayOfMonth; 0 keeps the old interval for legacy loans, `LoanTerms.Legacy` lets existing loan tests keep their logic), grace 10 days, loss at 3 open, 180 payments exactly 15 years, first payment July 1 (day 122).
- Statements stay one per season (three payments each; first period partial); monthly would be a small PeriodOf swap later.
- Tree regrowth: sprout 14, full at 365, step 25 with a clamp; real-time full regrowth about 182 hours.
- Wood seasoning converts from "1 season" to 28 days so it stays about 14 real hours.
- Old saves: option 1, a calendarVersion in meta.json and slotinfo.json; legacy slots are labeled "old calendar" and refuse to load; tag the Phase 0 commit (for example `pre-real-calendar`) so that build can still open them; warn on overwriting a legacy slot. Migration (large) only if Mike names a save he must keep; a one-off converter for one save is the cheaper path.
- Every date string changes to "Mar 1, Year 1" style (pause menu, weather screen, save slot list, journal, finances, land purchase panel, loan banners, loss screen, statement rows and CSV, debug).
- About 20 files edited, 25 to 35 tests changed, 80 to 120 added. Optional split: 1a (calendar, display, regrowth, save gate; loan keeps its old interval) then 1b (monthly loan and ledger periods).
- Oddity noted: "Jan 15, Year 1" comes after "Dec 31, Year 1" because Year N is the March-to-February game year.

**Phase 2, player time control:**
- Finding: the F7 debug speed (x1, x10, x60, x360) advances only the clock. Survival, fire, wood and torch fuel advance from Time.deltaTime and ignore it, so at speed hunger does not drain and fires do not burn; hourly and daily events (weather, traps, taps, loan, market, ledger, autosave) do fire. It is unsafe for players and was a data-quality caveat for the income runs.
- 2a (recommended first, small to medium): Wait 1, 3 or 6 hours, until dawn or dusk, plus the existing sleep, through one shared PassWorld(hours) step loop (0.25-hour steps for survival, fire, wood, torch, then the clock), with a pure TimeControlRules checking stop reasons (low stats, severe weather, flood rise, fire about to go out, trap catch, loan, statement or bill notice, danger nearby) and gating (not in the creek, a menu, or collapsed). New: TimeControlRules.cs, TimeControlManager.cs, WaitPanel.cs and tests.
- 2b (later, medium to large): one frame-hours value from TimeManager so every system shares the same time, enabling a true live fast-forward.

**Bills:** need an absolute month index and a NextMonthDay function so a state-based BillsManager can charge each month crossed in order; recommended bill day is the 15th (the loan is due on the 1st); order of operations loan, then bills, then the ledger close; a bill never touches the missed-payment count.

**Decisions needed from Mike:** first payment date (July 1), bill day (15th), Phase 1 split (1a and 1b), old saves (option 1). Accepted by default unless Mike objects: statements per season, wood seasoning to 28 days, regrowth numbers, outhouse pit stays 28, Phase 2a before 2b.

**Mike's decisions on the plan (2026-11-08):** first land payment July 1 (day 122); bills charged on the 15th; Phase 1 as ONE build (not split into 1a and 1b); old saves: label and refuse, plus a one-off conversion of ONE named save (Mike named slot 4). Accepted by default: statements per season, wood seasoning converted to 28 days, regrowth numbers (sprout 14, full 365, step 25), outhouse pit stays 28, Phase 2a (wait and sleep skip) before 2b. Build order: Phase 0, then Phase 1, then Phase 2a, each with its own playtest checklist; builds start after Mike finishes the Pioneer re-measure run.

**Added to Phase 1 scope (found in the Phase 0 playtest, 2026-11-08):** after the player pays a land payment, the on-screen payment banner (due today, overdue) stays up until its timer runs out. This is existing behaviour (the banners are timed ToolStatus.Banner notices that nothing clears on payment), not a Phase 0 regression. Phase 1 rewrites those banner strings anyway, so also clear the loan payment banners once the payments they refer to are settled, without clearing unrelated banners.

---

**Phase 1 BUILT and playtested (2026-11-10):** the real 365-day calendar is in the game. Claude Code's suite passed 2,077 of 2,077 (up from 1,944) and Mike's 16-step playtest passed 16 of 16 (Calendar_Phase1_Playtest_Checklist.md). Local rollback tags: pre-real-calendar and pre-phase1.

What changed: game year March to February, 365 days, real month lengths, seasons 92/92/91/90 days, dates read "Mar 1, Year 1". The loan is monthly on the 1st from July 1 (day 122), 180 payments, last Jun 1 Year 16. $60,000 is $573.39 a month, $18.85 per game day (was $20.48). Tree regrowth sprouts at 14 days, full at 365, 25-day steps. Wood seasoning is 28 plain days. Saves carry calendarVersion; old-calendar slots are labeled, refuse to load, and are copied to Saves\_old_calendar before anything overwrites them. Slot 4 was converted into a new slot 5 (day 38 became Jul 3, Year 1). Loan reminder banners now clear once a payment is settled. NextMonthDay, MonthDayOnOrAfter and MonthIndexOf are general, ready for Bills on the 15th.

Known and accepted: "Jan 1, Year 1" follows "Dec 31, Year 1". A payment due at midnight on a season's first day shows as owed on the statement that just closed (cash basis). Summer is 92 days, about 1.4 heat waves per Summer at the current weights (not retuned; decide later). Weather, Maple and forage season weights not retuned.

Still to do: Phase 2a (Wait 1/3/6 hours, until dawn or dusk, through one shared PassWorld loop), then the Pioneer re-measure on the final calendar, then Money Phase D2 tier numbers. Bills framework and everything in Modern_Systems_and_Services.md come after.
---

**Phase 2a BUILT and playtested (2026-11-10):** the player-facing Wait is in the game. 2,177 of 2,177 tests pass (+100) and Mike's 16-step playtest passed 16 of 16 (Calendar_Phase2a_Playtest_Checklist.md).

Wait is on the Z key: 1, 3 or 6 hours, until dawn, until dusk. Wait and sleep share one loop (PassWorld, 0.25-hour steps: survival, fire, wood, torch fuel, then the clock). Stop reasons, in order: collapse, danger, a stat under 25 (or core temperature over 38), severe weather arriving, the creek rising or flooding, a fire dying (half an hour of fuel left) or going out, a trap catch, any notice banner. A Wait refuses to start in the water, with a stat already low, in a menu, or while asleep or collapsed. F7 debug speed is unchanged (clock only).

Known: sleep now burns the fuel of a lit torch or lantern (it used to burn none); kept as built. "Danger near" has a rule and a hook (TimeControlManager.DangerSource) but nothing sets it until a predator exists. A Wait runs in one frame with no fade. Phase 2b (a live fast-forward) is not built.

Next: the Pioneer re-measure on the final calendar, then Money Phase D2 tier numbers.