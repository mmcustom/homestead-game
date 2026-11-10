# Phase 2a Playtest Checklist: Wait (player time control)

Date written: 2026-11-10
Build: Phase 2a, built and tested by Claude Code, NOT committed. Phase 1 is committed (da55162).
Automated: 2,177 of 2,177 EditMode tests pass. This list covers what tests cannot: how Wait feels and whether sleep still behaves as before.

## Before you start

- A Saves backup exists: `08_Backups\Saves_before_Phase2a_20261110_073007`. Use a new game or slot5 (the only older save this build can load).
- Wait is on the Z key. Z or Esc closes the menu. Sleep still works in the bed or Sleeping Bag.
- Debug keys are fine here. F10 lowers your stats to 20.
- "Danger near" cannot be tested: nothing hostile exists yet, so only the rule is built.
- Do not measure income in this run. The Pioneer re-measure comes after Phase 2a passes.

Reply in the usual form, for example: `1 pass, 2 pass, 3 fail (what you saw), ...`

## Part A: The menu

1. Press Z in plain play. Expected: a Wait window with five buttons (1 hour, 3 hours, 6 hours, until dawn, until dusk). Z, Esc and Close all shut it. Z does nothing while the Inventory, Finances, a placement mode or the pause menu is open.

## Part B: Plain waits

2. Wait 1, 3 and 6 hours, one at a time. Expected: the clock moves by that much, a HUD line says "Waited N hours." for about 5 seconds, and Hunger and Hydration drop about as they would over those hours.
3. Dawn and dusk. Press Z at 12:00. Expected: dusk shows about 8 hours and dawn about 18 hours. Run "until dusk" and check the clock lands on sunset. Then, at exactly dawn, check it offers a full day.
4. Wait across midnight on an ordinary day with nothing due. Expected: the Wait does NOT stop. (The autosave and the new-day work happen while you wait. A routine "saved" message must not count as a notice.)

## Part C: When Wait refuses to start

5. Press Z in the creek or a pond. Expected: buttons greyed, "You can't wait in the water."
6. Drop a stat under 25 with F10, then press Z. Expected: refused with a line like "You're too thirsty to wait."

## Part D: When Wait stops early

7. Stat stop: with a stat near 30, wait 6 hours. Expected: it stops when the stat crosses 25 and says so.
8. Weather: wait with a thunderstorm, cold front or snow coming. Expected: it stops on arrival and names it. Waiting while one is already falling runs to the end.
9. Flood: wait during heavy rain (numpad rain controls). Expected: stops at "the creek is rising", and again at "the creek is in flood".
10. Fire: light a campfire with 1 to 2 hours of fuel and wait 3 hours. Expected: stops at "the fire is dying" with about half an hour left. Waiting again does not stop for the same fire until it goes out.
11. Trap: set a trap and wait until it catches. Expected: stops with "a trap has caught something".
12. Notice: use F5 to reach the day before a loan payment, then wait across midnight. Expected: stops on the payment banner. Check a statement banner at a season change too.

## Part E: Torch and sleep

13. Torch: light a torch and wait 2 hours. Expected: it loses about 2 hours of fuel. Then sleep with a torch lit and note what happens. Claude Code changed this on purpose: sleep now burns torch fuel, so a torch left lit all night burns out. Tell me whether you want to keep that.
14. Sleep regression: sleep in a tent, Lean-To and Sleeping Bag, once at night (to 6:00) and once by day (3 hours). Expected: Hunger and Hydration drops, fire burning down, wood drying, traps and the autosave all as you remember. An early wake for cold, thirst or hunger still works.

## Part F: Other

15. F7 debug speed: x1, x10, x60 and x360 still work, and Wait is not affected by them.
16. After a long Wait, save and reload. Expected: date and clock are right and nothing is stuck in a menu state.

## Notes to bring back

| Item | What you saw |
| --- | --- |
| Which routine events (if any) wrongly stopped a Wait | |
| Any HUD line that read oddly or hid behind something | |
| Torch during sleep: keep burning, or snuff at bedtime? | |
| Anything that behaved differently from before | |
