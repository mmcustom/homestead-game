# Land Loan Phase A: Playtest Checklist

Status: PLAYTESTED 2026-10-21: all 41 items pass (Mike). Code passes 222 edit-mode tests (165 before, 57 new), re-run after the Bootstrap edit with 0 console errors or warnings. COMMITTED b03ebc3 and pushed to master 2026-10-21 (22 files).

Back up your Saves folder before playing (Claude Code backed up and restored it once, but your own copy is safer).

How to use: play each item and tell Claude the number and pass or fail, and what you saw if it fails. Debug keys (editor and development builds only): F5 skips 1 day, Shift+F5 skips 10 days, F6 jumps to the next season. Day 0 is Spring day 1, so F6 four times from a new game reaches day 112, when the first payment is due. The Finances screen opens with L. T is still the Trading Post.

Worked example used below (Settler, 5 acres, Half and half): price $60,000, savings $12,000, down payment $6,000, reserve (cash) $6,000, loan $54,000, payment $516.05. First payment: interest $360.00, principal $156.05.

## New game flow

1. New game, pick Settler: a land screen shows three parcels (5 acres $60,000, 10 acres $140,000, 40 acres $450,000) with price per acre and a payment preview.
2. Nothing overlaps or runs off the edge on any of the three cards, especially the 40-acre card.
3. Pick 5 acres: the savings screen shows $12,000 of savings. "Half and half" gives a $6,000 down payment and $6,000 reserve.
4. "All down" gives $12,000 down and $0 reserve. "None down" gives $0 down and $12,000 reserve.
5. Type a custom down payment such as 7,500.50: accepted, with the odd cents going to the reserve.
6. Type nonsense (letters) and an amount above your savings: both are refused with a message, and nothing breaks.
7. The loan summary shows the loan ($54,000 for Half and half), 8% a year, 180 payments, one every 28 days, the payment ($516.05) and the total interest.
8. Back out from the slot pick: it returns to the loan summary and you can change the choices.
9. Pick a slot and start: the game begins. The T window shows $6,000.00 (the reserve, not $12,000 and not $50).

## Starting values

10. New game on Homesteader, 5 acres: savings show $24,000 (40%).
11. New game on Homesteader, 10 acres: savings show $30,000 (capped, not $56,000). Same for 40 acres.
12. New game on Settler, 10 acres: savings $28,000. Settler, 40 acres: $30,000 (cap).
13. New game on Pioneer: no savings screen is shown, the loan is the full price (5 acres: $60,000, payment $573.39) and the T window shows $0.00.
14. The three difficulty descriptions read sensibly (they were reworded to remove "$50" and "no money").
15. Starting kits and Trading Post prices are unchanged from before.

## Finances screen

16. Press L in the game (a real key press): the Finances screen opens. Esc, L or Close shuts it.
17. While it is open the clock stops, and the T window keys do nothing.
18. It shows land (acres and price), original loan, balance, payment, next due day (season and day, plus "in N days"), open payments, missed count, late fees owed, payments made, interest paid, and total interest if you make no extra payments.
19. Text is readable and nothing overlaps.
20. Press L on an old save from before the loan system (see item 37): nothing opens.

## Due dates and warnings (new Settler game, 5 acres, Half and half)

21. Go to day 105 (F6 three times to day 84, Shift+F5 twice to day 104, F5 once). A "due in 7 days" message shows once on the small message line.
22. From day 105, press F5 seven times to reach day 112: a "due today" banner shows. The Finances screen shows one open payment of $516.05.
23. Pay what is due (cash $6,000 and nothing sold): cash becomes $5,483.95, open payments clear, balance becomes $53,843.95, payments made is 1.
24. The next due day is 140.

## Paying

25. New game, same setup, go to day 112. Type 100 and pay: the open payment shows $416.05 left, cash drops by $100.00. (The interest is paid first.)
26. Pay what is due: the rest clears. Balance $53,843.95.
27. Type 1,000 as an extra payment: balance drops to $52,843.95 and cash drops by $1,000.00. No future payment is covered.
28. Go to day 140: the payment is still $516.05, with interest $352.29 and principal $163.76 (lower interest because of the extra payment).
29. Try to pay more than your cash: it is refused with a message and nothing changes.
30. Type "250", "250.50" and "$1,250.00": all are accepted as amounts.

## Late, missed and warnings

31. New game, Pioneer, 5 acres (cash $0). Go to day 112 and then to day 113: an "Overdue" banner shows the day after the due day, saying when the fee lands.
32. Go to day 122: a "Missed" banner shows, a $75.00 late fee is added once, and the missed count is 1 (red).
33. Go to day 140 and then 150: a second payment falls due and goes missed. Count is 2. The fee is added once for each.
34. Go to day 168 and then 178: count is 3. A red WARNING shows on the Finances screen and in the banner. The game does NOT end.
35. Give yourself cash (sell things, or F12) and pay: oldest payment clears first (fee, then interest, then principal), the missed count falls as each one clears. Money left after all open ones are cleared goes to principal.

## Save and load

36. Mid-loan, save, quit to the menu, load that slot: balance, due day, open payments, missed count and cash are exactly the same.
37. Load a slot saved BEFORE the loan system (an old save): it loads with no loan, no warnings and L does nothing. The T window money is the old dollar amount shown with cents.
38. Autosave: let a day pass in a loan game, quit to the menu, press Continue: the loan is exactly as it was.
39. Sleep in a bed across a due date: the due message and the open payment appear after waking and nothing is skipped.
40. Start a new game after playing a loan game: the old loan does not carry over.

## Errors

41. No red errors or warnings appear in the Unity console during any of the above.

## Result log (Claude fills this in from Mike's reports)

- Item results: 1 to 41 all PASS (Mike, 2026-10-21).
- Commit: DONE, b03ebc3 pushed to master (e193129..b03ebc3). When all items pass, Mike says so; Claude writes the commit message and Claude Code stages only the Phase A files and shows the list first.
