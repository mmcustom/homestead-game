# Calendar Phase 1 Playtest Checklist

Date written: 2026-11-08
Build: Phase 1 (real 365-day calendar), built and tested by Claude Code, NOT committed.
Automated: 2,077 of 2,077 EditMode tests pass. This list covers what tests cannot: how it reads and feels in the game.

## Before you start

- A backup of all your saves already exists: `08_Backups\Saves_before_Phase1_20261108_131357` (and the older `Saves_pre_real_calendar_phase0`). You do not need to back up again.
- Your slots 1 to 4 and the autosave were written before the calendar version existed, so this build treats them as "old calendar" and will not load them. That is expected.
- `slot5` ("Slot 5 (from Slot 4)") is the converted copy of your old slot 4. It is the only existing save this build can load.
- Debug keys are fine for this playtest (we are checking dates and rules, not measuring income). Use the same money key you used in the Phase 0 banner test.
- Do not measure income in this run. The Pioneer re-measure waits until after Phase 1 and the Wait/Sleep control (Phase 2a).

Reply in the usual form, for example: `1 pass, 2 pass, 3 fail (what you saw), ...`

## Part A: New game and dates

1. Pick Pioneer, 5 acres. On the land purchase panel, check the loan lines. Expected: "180, one a month (the 1st)", first payment "Jul 1, Year 1 (day 122)", payment $573.39 for $60,000. Check no text is cut off or overlapping its label.
2. Start the game. Expected: "Mar 1, Year 1", 6:00, Spring. Check the date on the pause menu and on the weather screen.
3. Open Finances. Expected: payment reads "a month, due on the 1st", and the next payment shows the date plus "in N days" (about 122 days from the start).

## Part B: Seasons and the year

4. Use the debug keys to reach each season start. Expected: Summer starts "Jun 1", Fall "Sep 1", Winter "Dec 1".
5. Cross the end of Year 1. Expected: "Dec 31, Year 1" is followed by "Jan 1, Year 1" (accepted oddity), then "Feb 28, Year 1" is followed by "Mar 1, Year 2" and Spring.
6. Watch the weather and the Maple (or any seasonal look) across Jun 1, Sep 1 and Dec 1. Expected: seasons blend smoothly, no sudden jump on the first day of a season.

## Part C: The land loan

7. Skip to Jul 1 (keep enough cash to pay). Expected: a "payment due today" style banner and an open payment. Pay it. Expected: the loan banner clears, and a statement-ready banner (if one is showing) stays. This is the banner fix from the Phase 0 playtest.
8. On a fresh new game, do NOT pay. Expected: overdue banner, then "MISSED" on Jul 11, with the next payment on Aug 1.
9. Keep not paying. Expected: the game ends on Sep 11, Year 1 (three open missed payments). Check the loss screen date reads correctly.
10. Open a statement at a season change. Expected: seasons of different lengths (92, 92, 91, 90 days), the first period partial, labels like "Spring, Year 1".

## Part D: Saves

11. Open the slot list. Expected: old slots show an amber "old calendar" label with Load disabled, and Continue picks the newest current save, never an old one.
12. Save over one old slot (pick one you do not need; backups exist). Expected: a warning asks first. After confirming, the old file appears copied under `Saves\_old_calendar`.
13. Load slot5. Expected: "Jul 3, Year 1", the $60,000 loan with the first payment due Mar 1, Year 2 (then Apr 1), felled trees partway through regrowth, wood piles still wet or dry as before. Note: because slot5 was converted from a loan not yet due, it has no July payment. Use a new game for the July payment tests.

## Part E: Rules that changed

14. Wood: a wet batch dries in 28 days. Check the Wood Shed hint says "about 28 days".
15. Regrowth: a felled tree sprouts at about 14 days and is not full size until about a year.

## Part F: Regressions

16. Day length is unchanged (still 30 real minutes). Sleep, traps, fires, torches and crafting jobs should feel the same as before.

## Notes to bring back

| Item | What you saw |
| --- | --- |
| Any text cut off or wrong date format | |
| Heat waves in Summer (it is 92 days now, may feel frequent) | |
| Anything that behaved differently from before | |
