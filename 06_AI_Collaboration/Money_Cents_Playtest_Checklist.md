# Money in Cents: Playtest Checklist

Status: PLAYTESTED 2026-10-20: all 15 items pass (Mike). Code passes 165 edit-mode tests. COMMITTED b6f548a and pushed to master 2026-10-20 (12 paths).

Note from item 14: each F12 press gives the full debug kit as well as $50, so repeated presses overload carry weight and the player has to sell things to move. Not a money bug; logged as a backlog item (a cash-only debug key).

What changed: money is stored as cents (a long). Nothing should play differently. The only visible change is that money now shows with two decimals and a comma, for example $8.00 and $1,000.00.

How to use: play each item and tell Claude the number and pass or fail. Press T to open the Trading Post. Start a new game for items 1 to 3.

## Display

1. New game on Homesteader: the T window shows $50.00.
2. New game on Settler, then on Pioneer: the T window shows $0.00 each time.
3. The price columns show cents (for example Flint and Steel $8.00, Axe $20.00, Knife $4.00) and nothing looks cut off or overlapping.

## Buying and selling

4. Press F12 once to get the debug kit (+$50). Buy a Knife for $4.00: money drops by exactly $4.00 and the message shows $4.00.
5. Sell the Knife back: money rises by exactly $1.00.
6. Try to buy something you cannot afford (for example the Rifle at $60.00 with less money): it is refused and the message shows the price with cents.
7. Shift-click buy Arrows (10 at a time): money drops by $10.00 and you get 10. With less than $10.00, Shift-click buys only as many as you can afford.
8. Sell a deer hide, a small fur, a fish and a firewood: each pays the same whole-dollar amount as before (hide $8.00, small fur $3.00, bluegill $1.00, firewood $1.00).
9. Arrows, Rifle Rounds and Cordage are still NOT bought back.

## Save and load

10. Note your money, save, quit to the menu and load that slot: the same amount is shown.
11. Load a slot you saved BEFORE this change (an older save with money in it): it loads with the right amount, shown as dollars and .00 (for example $50 shows $50.00).
12. Autosave: let a day pass, quit to the menu, press Continue: money is unchanged.
13. Start a new game after playing: money resets to the new game's starting amount.

## Big numbers and errors

14. Press F12 20 times (each is +$50): the money shows $1,000.00 with the comma, and the T window still looks right.
15. No red errors appear in the Unity console during any of the above.

## Result log (Claude fills this in from Mike's reports)

- Item results: 1 to 15 all PASS (Mike, 2026-10-20). Item 14 passed, with the F12 kit overload noted above.
- Commit: DONE, b6f548a pushed to master (493e35c..b6f548a). Claude Code committed ONLY the money-in-cents files (TradingPost.cs, TradingPostScreen.cs, DifficultyManager.cs, GameManager.cs, the new Scripts/Economy files, MoneyTests.cs, Homestead.Tests.EditMode.asmdef and the new .meta files). The docs (GDD, roadmap, Money_System.md, Trading_Post_Market.md, task list, checklists) go in a separate docs commit.
