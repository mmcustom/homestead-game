# Money Phase D1 Playtest Checklist

**Goal:** prove D1 changed NOTHING about how the game plays at default values, and that the new measuring tools work.
**Before you start:** back up your Saves folder (copy it somewhere outside the project). Play in the editor with the D1 changes uncommitted.
**How to report:** reply with the item number and pass or fail ("1. pass, 2. pass, 3. fail - the line is cut off"). Add a short note on any fail.
**Handy:** use your usual time debug keys to skip ahead. Insert still forces a loss.

---

## A. Same terms on every tier (no behaviour change)

1. New Game, pick Settler and 5 acres. The loan summary shows the tier terms (8% interest, $75 late fee, game lost after 3 missed payments) and a monthly payment of **$458.71** on a $48,000 loan.
2. Same screen with Pioneer and 5 acres: same terms line, payment **$573.39** on a $60,000 loan.
3. Settler and 10 acres: same terms line, payment **$1,070.33** on a $112,000 loan.
4. Homesteader on any parcel: same terms line (8%, $75, 3 missed). The payment is whatever the loan amount gives; it just has to look sensible.
5. Start a game and press L, open the Loan tab. A "Terms of this game" block shows the same values as the New Game screen. Nothing overlaps the open-payments list or the summary rows.
6. Skip ahead past the first due day (112) without paying. The $75 late fee lands 10 days after the due date and the payment counts as missed, same as before D1.
7. The red warning still appears at 3 open payments, as before.
8. Keep skipping with no payments. The game is lost on the third missed payment, the loss screen appears, and "Land loan still owed" looks right.
9. Repeat 6 and 8 on a different tier (Pioneer or Homesteader). Same fee, same loss at 3.

## B. Saves

10. Load a save made BEFORE D1 (one of your older saves). It loads without error, and the Loan tab terms read 8%, $75, 3 missed with the same payment as before.
11. Save a new game now, go to the Main Menu, press Continue. The terms and the loan balance are unchanged.

## C. Trading Post sell prices (must be unchanged at 100%)

12. Open the Trading Post and sell one item. The quote on the stepper, the cash you actually receive, and the Statement income line all match.
13. Sell a bigger amount with the stepper (for example 13 of something). Quote, cash change and Statement all match again.
14. Buy prices and sell prices look the same as in earlier playtests (nothing jumped or dropped).

## D. Real minutes played

15. Play about 3 minutes with no menus. Press L, open the current season. "Real minutes played" is close to your real time, and "Income per real hour" shows (or shows nothing sensible if you earned nothing yet, with no error).
16. Spend a minute with the Finances screen open, a minute in the Trading Post, and a minute paused. Real minutes does not grow during any of them.
17. Sleep through a night. Real minutes does not grow while sleeping.
18. Save, go to the Main Menu, press Continue. Real minutes played kept its value and keeps growing from there.
19. In a save made before D1, older seasons show no real minutes (blank, not 0) and no income per real hour line, and nothing errors.
20. The two new lines sit directly under the days line on the Statement detail and do not overlap anything.

## E. CSV export (End key)

21. Press the End key in game. A message shows on the HUD, and the Console shows the full path of the file.
22. The file exists in `C:\Users\mmcus\AppData\LocalLow\MM Custom\Homestead\Exports\` and is named like `ledger_20261023_143000.csv`.
23. Open it in Excel. It opens cleanly, oldest season first, dollars like 1234.56, category names with commas (such as "Firewood, logs and stone") stay in one cell, and the open season is marked "(so far)".
24. Numbers in the CSV match the Statement for the same seasons (income, expenses, net, cash, loan).
25. Press End twice. Two separate files, no error.

## F. Layout and regression

26. Look at the Statement, the Loan tab and the New Game loan summary at 1920x1080. Nothing cut off or overlapping.
27. Same at 1024x768.
28. Quantity steppers still work on the Trading Post, Storage, Inventory and Crafting (plain click moves the stepper amount, Shift-click keeps its old meaning).
29. The "Account statement ready: press L" line, the banner, and the L key still work.
30. Loss screen buttons still work: Restart (same setup, fresh world), Load Save, Main Menu.
31. Console is clean after the session (the "WebSocket is not initialised" MCP message is known and fine; no red errors).

---

**When done:** send me your pass or fail list. If everything passes I will write the D1 commit message, Claude Code shows the staging list, and nothing is committed until you say OK.
