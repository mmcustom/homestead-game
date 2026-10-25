# Ledger Item Export Playtest Checklist

Date: 2026-10-25. Change under test: item-level sales in the ledger (units and dollars per item per season) and a second export file, `ledger_items_<stamp>.csv`, written by the End key. Edit-mode tests pass (646, 10 new); Play Mode is untested. Report each item by number as pass or fail.

**Use a throwaway test slot, not a measuring save.** Selling changes cash and the market price pools, which would pollute the Settler and Pioneer measurements. A new game or a copy of a slot is fine.

Exports folder: `C:\Users\mmcus\AppData\LocalLow\MM Custom\Homestead\Exports\`

1. Start a new game (or load a throwaway slot that has a ledger) and enter Play Mode with no errors in the Console.
2. Get some goods to sell (F12 full kit, or hunt and trap). Note your cash before selling.
3. At the Trading Post (T), sell at least 3 units of one item, for example Small Furs. Note the units sold and the cash received.
4. Sell a second, different item (for example Firewood or meat).
5. Sell back one stocked item you bought (for example a Knife or Bucket), if you have one. If not, skip and say so.
6. Selling still works as before: cash goes up by the amount shown, prices look normal, no errors in the Console.
7. Press End. The banner reports the export and names the file.
8. The Exports folder holds two new files with the same timestamp: `ledger_<stamp>.csv` and `ledger_items_<stamp>.csv`.
9. The main CSV keeps its usual columns, and the dollars for the matching income category include what you just sold.
10. The items file header is exactly `Season,Item,Units,Dollars,Average price`.
11. The items file has one row for each item you sold, labelled with the season as "Spring, Year 1 (so far)" (or the current season) while it is open. Units equal what you sold, and Dollars equal the cash you received.
12. Average price equals Dollars divided by Units for each row.
13. Add up the Dollars of all item rows for one season. The sum equals that season's Total income in the main CSV.
14. The stocked sell-back (item 5) appears as its own item row, and its dollars are in the "Stocked goods sold back" income, not mixed into another category.
15. Save the way you normally do, reload that save, and press End again. The item rows from before the reload are still there and the totals still agree with the main CSV.
16. Load an older save from before this change and press End. The Console shows no errors. The items file has only rows for sales made since the load (header only if you sold nothing).
17. No new errors or warnings appear in the Console during any of the above.

Not part of this test: Part 2 (trap counters) is not built.
