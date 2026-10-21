# Trading Post Market: Playtest Checklist

Status: PLAYTESTED 2026-10-21: items 1 to 32 all reported "pass" (Mike). Code passes 289 edit-mode tests (222 before, 67 new cases from about 45 methods). COMMITTED 0f8e336 and pushed to master 2026-10-21 (23 files, 7cdadb1..0f8e336).

Note: items 29 to 32 are notes, not pass or fail. Mike answered "pass" to each and recorded no income figures, so real money per game day is still unmeasured. Mike's next request (a quantity control for buying, selling and moving items in storage and campfires) is logged in the task list.

Back up your Saves folder before playing.

How to use: play each item and tell Claude the number and pass or fail, and what you saw if it fails. Items 31 and 32 are notes, not pass or fail.

Debug keys (editor and development builds only): F5 skips 1 day, Shift+F5 skips 10 days, F6 jumps to the next season. Day 0 is Spring day 1: Summer starts at day 28, Fall at 56, Winter at 84. F12 gives the debug kit plus $50 (every press). T opens the Trading Post, L the Finances screen.

What the market does: each item has its own price level. Selling a unit lowers that item's price (firewood 4% per unit, fish and game meat 5%, hides and furs 5%, forage 6%, crafted goods 10%, farm goods 5%, items sold back 8%). Each game day the price recovers 25% of the way back toward its seasonal target. Cooked meat and fish share the price level of their raw version.

Start each section with a new game (Settler, 5 acres, any split) unless it says otherwise.

## Display (new game, Spring day 1)

1. T window opens. Each Sell row has a marker (green +, red -, or dim =), a price, and a second small line. Forage rows say "85% of normal"; rows at 100% show no "% of normal" note.
2. The market note line under the header is empty in Spring (nothing differs from normal by 10% or more).
3. Rows with a stack of 2 or more show a "Shift: all N for about $X.XX" line. Nothing overlaps or is cut off, and the rows are not too tall.
4. The Sell list is ordered by base price (highest first), then name, and does NOT reorder while you sell.
5. The Buy list shows the same stocked prices as before (Tent $25.00, Axe $20.00, Knife $4.00).

## Selling and pressure (Spring, day 1)

6. Chop and split enough firewood for 17 or more. Sell ONE at a time and note the prices: $1.00, $0.96, $0.92, $0.88, $0.85. Cash rises by exactly each amount.
7. Sell the rest of a stack with Shift-click: the cash rise equals the total shown in the status text, and matches the "Shift: all N for about..." quote taken just before.
8. After selling about 17 firewood in total, the firewood row shows about "50% of normal" with a red - marker. Logs and stone still show 100% (each item has its own price level).
9. Press F5 once: firewood shows about 62% of normal. After another F5: about 72%. After another: about 79%. The marker is a green + while it recovers.
10. Sell raw fish (any kind): that fish's price falls, and the COOKED version of the same fish falls with it. Other fish kinds are unaffected.
11. Sell 3 crafted tools of the same kind (for example 3 stone pick axes): $4.00, $3.60, $3.24. A different crafted tool (for example a shovel) is still at 100%.
12. Sell back a stocked item (for example a Knife): it pays about $1.00 at 100%, and arrows, rifle rounds and cordage cannot be sold back.
13. Buy 10 Arrows with Shift-click: the price stays $1.00 for every unit (buying does not move any price).

## Seasons (new game; use F6)

14. F6 to Summer (day 28): the market note reads "Summer prices: Firewood, logs and stone down 20%, Fish down 10%." Tent $23.75, Sleeping Bag $14.25, Tarp $7.60, Lamp Oil $2.85.
15. In Summer, firewood shows a red - marker. Press F5 a few times: it falls toward 80% (about 95%, 91%, 88%...).
16. F6 to Fall (day 56): the market note is empty again, prices recover toward normal, and the gear prices return to normal.
17. F6 to Winter (day 84): the market note says firewood, logs and stone pay about 50% more and hides, furs and game meat about 30% more. Tent $27.50, Sleeping Bag $16.50, Tarp $8.80, Lamp Oil $3.30.
18. In Winter, firewood shows a green + marker and climbs each day (about 112%, 118%, 123%...).
19. Forage rows stay at "85% of normal" with a flat = marker in every season, and you can sell stored forage in Winter.

## Save and load

20. Sell a stack (firewood), note the percent, save, quit to the menu, load that slot: the same percent and marker are shown.
21. Sell a stack, let one day pass (F5), quit to the menu and press Continue: the price keeps recovering from where it was (it is not reset to 100%).
22. Load a slot saved BEFORE the market existed: it loads with no crash, every row shows 100% (including forage), and prices start moving toward the season's targets day by day (forage drifts toward 85%).
23. Start a new game after playing a market game: every item starts at its season price (forage 85%, the rest 100%), not the old game's prices.
24. Sleep in a bed for a long night or across several days: prices recover for each day slept.

## Other systems

25. The land loan still works: L opens the Finances screen, a payment can be made, and money shows with cents everywhere.
26. Money after selling shows with cents and a comma for large amounts, and the loan and T window agree on your cash.
27. Starting kits, new game flow, and the Trading Post Buy list work as before.
28. No red errors or warnings appear in the Unity console during any of the above.

## Feel and income (notes, not pass or fail)

29. The second line of each Sell row is small (13 px). Is it readable? Should it be larger?
30. Does the market feel fair? Selling 20 firewood at once drops the price a lot, then recovers over days. Too harsh, too gentle, or about right?
31. Play a normal session of about 7 game days (about 3 and a half real hours) doing what you would normally do, and keep selling surplus. Note your cash at the start and end of the week, minus anything you spent. We want real money per game day. The model said about $29 for a Survivor and $50 for a casual mixed player.
32. Anything that looked wrong or confusing in the T window.

## Result log (Claude fills this in from Mike's reports)

- Item results: 1 to 28 PASS; 29 to 32 (notes) answered "pass" with no figures (Mike, 2026-10-21).
- Commit: NOT committed. When all pass or notes are settled, Mike says so; Claude writes the commit message and Claude Code stages only the market files and shows the list first.
