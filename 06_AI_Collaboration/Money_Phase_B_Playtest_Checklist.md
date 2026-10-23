# Money Phase B Playtest Checklist (ledger and seasonal statement)

Build: code 50d01d8 plus uncommitted Phase B (ledger, Statement tab in Finances, banner queue, L key without a loan).
Report results by item number ("1. pass", "7. fail: ...").

Debug keys: F5 skips 1 day, Shift+F5 skips 10 days, F6 skips to the next season, F12 gives the debug kit plus $50 (every press).
Reminder: the first loan payment is due on day 112 (Spring day 1 of Year 2). F6 four times from a new game gets you there.

## Before you start

- Back up the Saves folder (copy it somewhere outside the project).
- Keep the Unity Console open and clear it before starting.
- Do NOT press F12 until item 15 says to. The statement should reconcile to the cent until then.

## A. A new game and the Finances screen

1. Start a new game: Settler, 5 acres, and a savings split of your choice (for example Half and half). Note your down payment and your starting cash.
2. Press L. The Finances screen opens. It has two tabs: Loan and Statement. Both tabs are clickable.
3. The Loan tab looks and works as it did before (loan amount, payment, due date, pay and extra payment).
4. Click the Statement tab. The panel is light, paper-style, with dark ink. A row marked "(so far)" is at the top for the season in progress.
5. Click the "(so far)" row. The detail pane shows the opening cash and a down payment expense equal to what you noted in item 1. The down payment is shown in red.
6. Press L again (or Esc). The screen closes and the game continues normally.

## B. Recording money

7. Press T. Sell 3 or more different things (for example firewood, fish, a hide, a crafted item). Note the total you earned.
8. Buy a few things: a tool (Knife or Axe), arrows, a Tent, Lamp Oil. Note the total you spent.
9. Press L, Statement tab, click the "(so far)" row. Income appears under the right groups (firewood, fish, hides and furs, crafted goods) and adds up to what you noted in item 7.
10. Purchases appear split by group, not one line: Tools, Hunting and fishing, Shelter and bedding, Cooking water fire (Lamp Oil, Bucket, Canteen, Cooking Pot, Flint and Steel), Building materials, Other goods. The amounts match what you spent in item 8.
11. Sell back something you bought from the store (a stocked item). It shows as "Stocked goods sold back" income, separate from the other groups.
12. On the Loan tab, make an extra payment of $100 (before the first payment is due). On the Statement tab the "(so far)" detail shows an extra payment expense of $100.
13. The cash shown on the screen equals: cash at start, plus income, minus expenses. The "Other" line is not shown (it only appears when it is not zero).
14. Positive figures (income, net, balances) are black, negative figures (expenses and a negative net) are red. Zero is black.
15. Now press F12 once. Check the "(so far)" detail again: an "Other (not in the ledger)" line appears for the $50, and the numbers still add up to your real cash.

## C. Season change and the banner

16. Press F6 to jump to the next season. A banner says the account statement is ready (press L). A persistent "Account statement ready: press L" line stays on the HUD.
17. Walk around for a minute without opening L. The HUD line stays until you open the statement.
18. Press L. The Finances screen opens on the Statement tab. The HUD line disappears once you have opened the statement.
19. The table now has a closed season row (the one you just finished) at the top, plus a new "(so far)" row for the new season. Rows are newest first.
20. Click the closed season. The detail pane shows income by category, expenses by category, the net, cash at end, loan balance at end, and per game day averages. The "change from the season before" line explains it is per game day.
21. Totals rows "This year" and "Since the start" are present and include the season in progress.
22. The tab opens on the newest closed season each time you open it, not on an old one.

## D. Banners on a payment day, and sleep

23. Press F6 until you reach day 112 (Spring, Year 2). You should have a payment-due banner AND the statement-ready message on the same day. Both show, one after the other. Neither is lost or shown over the other.
24. Go to bed and sleep across a season change (use F5 to get to the last day of a season, then sleep). During the sleep the banners do not draw. After you wake, the queued messages show in order and the "statement ready" HUD line is there.
25. Trigger the same banner twice (for example open and close the same warning). It does not pile up; the same message waits only once.

## E. Missed payments and "Owed at end"

26. Do not pay the day-112 payment. Press F5 ten or more times (or Shift+F5) past day 122. The payment is missed and the late fee is added (this is Phase A behaviour, as before).
27. Open the Statement tab and the detail for the season that just closed. The "Owed at end" memo line is shown in red with the late fee and open payment still owed. It does NOT appear as an expense (it is a memo; the statement is cash-basis).
28. Pay the open payment on the Loan tab. In the "(so far)" detail the payment shows as late fee, interest and principal lines (and an extra line if you overpay), and "Owed at end" returns to zero (black) after the next season closes.

## F. Skipping several seasons

29. Press Shift+F5 about 10 times (or F6 repeatedly) so several seasons pass at once. Every skipped season gets its own row.
30. Each skipped season shows the same cash and the same loan balance, and 28.00 days. (Claude Code noted the "Owed at end" in skipped seasons shows the amount owed today, because the loan caught up in one go. That is a known limit of debug jumps, so just report if anything else looks wrong.)
31. With 20 or more rows, the table scrolls with the mouse wheel and the scrollbar. Scroll to the oldest season and click it. The detail pane works.

## G. Saving and loading

32. Save, quit to the main menu, then load the same save. The statement is identical: same rows, same totals, and the unread HUD line is still there if you had not opened the statement.
33. Load one of your old saves from before the loan system (an old slot with no loan, if you still have one). It loads. Press L. Finances opens on the Statement tab. The Loan tab says there is no land loan. The statement starts at your current cash with no invented past seasons.
34. Load an old save that has a loan but no ledger (a Phase A save). The statement starts empty at the load, and the loan tab works as before.

## H. Layout and look

35. At your normal window size, nothing overlaps on either tab. The paper panel is readable, with good contrast on your monitor (black ink on light paper, red clearly red).
36. Make the game window small (1024x768). The table, the detail pane and both tabs still fit and scroll, and the text is small but legible.
37. The detail pane is wide enough that no line is cut off (for example "Bought: cooking, water, fire").

## I. Regression and console

38. The Trading Post (T) still works, including the quantity stepper from Parts 1 and 2. Buying and selling still change your cash correctly.
39. Inventory, Crafting and Storage still work, and the other HUD messages (tool messages, weather warnings) still appear, one after another.
40. The L key opens and closes Finances normally in a game with a loan and a game without one. While Finances is open, the T window keys are ignored as before.
41. Walking, camera and the pause menu behave normally after using Finances.
42. Watch the Console the whole time. Claude Code saw "PlayerLoop internal function has been called recursively" 5 times in a Play Mode session twice (it believes it is the screenshot tool, not the game; it also happened before the ledger existed). If you see it, note what you were doing and whether the screen went blank. Report any other red errors or yellow warnings.

## Notes for Claude

- Is the paper-style panel easy to read? Any colours that do not work on your monitor?
- Does the table give you the picture you wanted of the property and household budget? What would you add or remove (for example a chart, a running average, a yearly view)?
- Is anything about the banners annoying now that messages queue (too many in a row, too slow)?
- Do the six purchase groups make sense? Should any item sit in another group?


---

## Result

Played by Mike: items 1 to 42 all pass. No notes added.
