# Money Phase C Playtest Checklist (the loss screen)

Build: code 0f6b002 plus uncommitted Phase C: the loss rule, the Lost state, the loss screen, saving blocked while lost, Restart with the new-game setup, your savings going to the debt on a loss, and a fixed starting weapon by difficulty (no weapon choice).
Report results by item number ("1. pass", "7. fail: ...").

Debug keys: F5 skips 1 day, Shift+F5 skips 10 days, F6 skips to the next season, F12 gives the debug kit plus $50 (every press), and NEW: Insert forces the loss at once (editor and development builds only, while playing).
Payment days: the first payment is due on day 112, then every 28 days (140, 168, 196). A payment is missed 10 days after its due day (day 122, 150, 178, 206). Three missed payments open at once is the loss, so with no payments the loss comes on day 178.

## Before you start

- Back up the Saves folder (copy it somewhere outside the project).
- Keep the Unity Console open and clear it before starting.
- To check the autosave, look at the Date modified of the autosave file in the Saves folder.

## A. Getting to the loss

1. Start a new game: Pioneer, 5 acres, any savings split. There is no weapon choice any more, only the difficulty (see section J). Write down the difficulty, parcel and split you chose, and your cash. Open the pause menu and save to a manual slot (call it Slot A).
2. Do not pay anything. Press F6 six times (day 168). Press L: two payments are missed and open, the third payment is due, and the red warning shows as before. Write down the Loan tab figures (principal balance, late fees owed, interest owed on the open payments). Close L.
3. Press F5 nine times (day 177). The game is still playing, no loss screen. Wait a moment, then note the autosave's Date modified.
4. Press F5 once (day 178). The third miss counts and the loss screen appears.

## B. The loss screen

5. The screen says "Mother Nature beat you." with a line under it: "Three payments on the land went unpaid."
6. The summary shows: days survived, seasons completed, total income, total expenses, cash you had, taken for the debt, land loan still owed (and "Cash left" only if it is above $0.00), and the due days of the three missed payments, one per line.
7. The numbers make sense: 178 days, 6 seasons, and the three due days are Spring 1, Summer 1 and Fall 1 of Year 2. Cash you had is your real cash. Taken for the debt is all of your cash (or all of the debt, if you somehow have more cash than debt). Land loan still owed is everything owed (principal, plus late fees and interest on the open payments, with the third $75 late fee added on day 178) minus what was taken. Check it by hand against the figures you wrote down in item 2. Example from Claude Code's test game: $2,000.00 cash, $48,000 principal, $150 fees, $960 interest, plus the $75 third fee = $49,185.00 owed, minus $2,000.00 = $47,185.00 still owed.
8. The world is frozen. You cannot move or look. Esc does not open the pause menu. T, I, L and M do nothing. The cursor is free.
9. Three buttons are visible and readable: Restart, Load Save and Main Menu.
10. The autosave's Date modified is unchanged from item 3 (the loss day did not write a save).

## C. Main Menu and Continue

11. Click Main Menu. The main menu appears. The autosave's Date modified is still unchanged.
12. Click Continue. It loads the last safe autosave: day 177, two missed payments open and the third overdue. No loss screen. Your cash is the full amount you had on the loss screen (taking the savings for the debt is only a display, not a real loss).
13. Press L and pay everything that is open. The missed count clears. Press F5 once: no loss. Play on normally.

## D. Load Save

14. Press Insert. The loss screen appears at once.
15. Click Load Save. The slot panel opens in load mode. Pick Slot A. It loads cleanly: day 0, a fresh loan, no closed ledger seasons, the clock runs, the cursor is locked, the loss screen is gone, and no old banners are left.
16. Play for a minute. L, the Statement tab, T and I all work.

## E. Restart

17. Press Insert, then click Restart. A new game starts straight away with the same difficulty, parcel and savings split as in item 1, in a fresh world (day 0, a fresh loan, starting kit only). The weapon is the one that belongs to that difficulty.
18. Press L: the loan and down payment match item 1. The pause menu works and saves to a slot. Press F5 once and check that the autosave's Date modified updates (a restarted game starts with no slot picked, so check that saving works).
19. In the restarted game, save to a slot (Slot B), go to the main menu and load Slot B. Press Insert, then Restart. It restarts with the same setup again (the setup travels with the save).
20. Load an older save from before Phase C (a Phase A or Phase B save). Press Insert, then Restart. It goes to the New Game screens with the line "That game did not record how it began, so pick your start again."

## F. Sleep

21. Start a new game (or load Slot A) and do not pay. Press F6 six times and F5 nine times (day 177). Note the autosave's Date modified. Go to bed and sleep so that day 178 happens during the sleep. The sleep fade finishes normally, nothing pops up over it, and the loss screen appears only after you wake up. The autosave's Date modified is unchanged.

## G. Catching up

22. Start a new game and do not pay at first. Press F6 five times (day 140) and Shift+F5 once (day 150): two payments are missed. Pay only the oldest on the Loan tab. Skip to day 178 (Shift+F5 and F5): two are open and there is NO loss. Skip to day 206: three are open and the loss screen appears.

## H. Quitting

23. From the loss screen, stop Play Mode (or close the game). Start again and click Continue. It loads the safe autosave as before, and the autosave's Date modified is unchanged by the quit.

## I. Layout and regression

24. At your normal window size the loss screen is readable and nothing overlaps.
25. Make the game window small (1024x768). The loss screen is still readable, including the three due-date lines.
26. After Continue (item 12), open L and the Statement tab. The seasons shown are the ones from that autosave, with no row for the loss day.
27. A normal game is unaffected: pay on time and no loss, no loss screen, ordinary days, banners and warnings as before.
28. After Restart and Load Save, the Trading Post, Inventory, Crafting and the quantity steppers all work normally.
29. Watch the Console the whole time. Claude Code saw "PlayerLoop internal function has been called recursively" 5 times again after a screenshot (it believes it is the screenshot tool, not the game). If you see it, note what you were doing and whether the screen went blank. Report any other red errors or yellow warnings.

## J. Weapon by difficulty and savings to the debt (added changes)

30. New Game: the difficulty panel has no weapon choice (no Bow and Rifle buttons, no "Your weapon" row). The difficulty descriptions say what weapon that tier starts with, and the panel looks fine with no gap or leftover strip.
31. Start a Pioneer game: you carry the bow and 20 arrows, and no rifle. The rest of the kit is only a bucket, 3 cordage and a knife.
32. Start a Settler game: you carry the rifle and 10 rounds, no bow, and the camping kit (tent and so on, without the hammer and tarp).
33. Start a Homesteader game: you carry the rifle and 10 rounds, no bow, and the camping kit with the hammer and tarp.
34. Restart after a loss (Insert, then Restart) on each of the three difficulties. Each one gives the right weapon for that tier.
35. A loss with almost no cash: spend your cash at the Trading Post first, then reach a loss (Insert). "Taken for the debt" is small (all the cash you had), "Cash left" is not shown, and "Land loan still owed" is nearly everything owed.
36. The 1024x768 check of the New Game difficulty panel: the long Pioneer description fits and the Back and Start buttons are inside the panel.

## Notes for Claude

- Is "Mother Nature beat you." and the line under it the wording you want?
- Is the summary enough? Anything you would add (for example total interest paid, the best season) or remove?
- Is a Main Menu button on the loss screen fine, or do you want only Restart and Load Save?
- The savings are taken from the oldest open payment first (its fee, interest and principal) before the next one, the same as a real payment. The totals are the same as taking all the fees first. Is that fine?
- If you have no saves at all, does Load Save look greyed with "no saves"? (Claude Code could not test this.)
- With no weapon choice, is the bow and 20 arrows for Pioneer and the rifle with 10 rounds for the other two how you want it to feel?


---

## Result

Played by Mike: items 1 to 36 all pass. Notes answered: wording acceptable, summary enough, Main Menu button stays, cash taken from the oldest open payment first is fine, Load Save yes, weapon feel yes.
