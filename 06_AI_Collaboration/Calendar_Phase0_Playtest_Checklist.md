# Calendar Phase 0 Playtest Checklist

**What changed:** nothing you should see. Phase 0 moved every copy of the calendar numbers (28-day seasons, 112-day year, first payment on day 112, then every 28 days) into one place. **Everything must look and behave exactly as before.** Any difference, even a small one, is a failure worth reporting.

**Before you start:** copy your whole Saves folder somewhere safe (Claude Code already made a hash-checked backup, but a second copy is cheap). Note slot 4's date, cash and loan balance so you can compare after loading it. Unity open, Bootstrap scene, Console cleared.

**Pass format:** reply "1 - 13 pass" or list the failures.

## Steps

1. Press Play from Bootstrap. The main menu appears and the Console shows **no errors**.
2. Start a New Game (Settler, 5 acres). The land purchase screen shows the first payment due **day 112** and a payment **every 28 days**, as before.
3. The game starts on **Spring, day 1, 6:00 AM** (check the pause menu date).
4. Press **F5** once: it is day 2. Press **Shift+F5**: ten more days pass (day 12). No errors.
5. Press **F6** to jump to the next season, four times in a row. You should see Summer, Fall, Winter, then **Spring of Year 2**, each starting on day 1 of the season.
6. Press **L**. The statement lists the seasons that closed (Spring, Summer, Fall, Winter of Year 1) with figures, and a "statement ready" banner appeared at each season change.
7. Open the Finances screen. The next payment shows as due **day 112** (the start of Spring Year 2) and the amount is the same as the land purchase screen showed in step 2.
8. Pay the payment on the Finances screen. It is accepted, and the next payment now shows due **28 days later (day 140)**.
9. Press **F5** (or Shift+F5) to move past the day-140 due date without paying. The warning banners appear in order (due soon, then overdue), and the late fee and missed count appear **10 days after the due date**, as before.
10. Save to an unused slot, quit to the menu, and load it. The date, cash, loan balance and statement are the same as before the save.
11. Load a **copy** of your slot 4 (an old save). It loads with the same date, cash and loan balance you noted. Do not overwrite it.
12. At the Trading Post, sell a few items, press F5 twice, and check the prices recover a little and nothing errors. Then fell a tree, press Shift+F5 (ten days), and check a sprout appears at the stump by about day 14.
13. Look at the Console at the end: **no new errors or warnings** from any of this.

## What to tell me if something is off

The step number, what you saw, and what you expected. For date or payment differences, a screenshot of the Finances screen helps.

## After the playtest

If all pass, I will ask Claude Code to commit these files only (the calendar source, the tests, the edited managers and the Bootstrap scene) and tag the commit `pre-real-calendar`. Nothing is pushed.
