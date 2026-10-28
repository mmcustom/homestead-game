# Potash A (Lye, Potash, Kettle and Market) Playtest Checklist

Date written: 2026-10-27. Build under test: Potash A, the market side of the Potash chain. Three new items (Lye, Potash, Potash Kettle), a new market group, and ledger lines. There is no hopper and no Kettle boil job yet (those are Potash B). 54 new test cases were added and the full suite passes in Unity (912 of 912, Console clean). Not yet seen in Play Mode.

Reply with the item number and pass or fail (for example "6 fail, the price did not recover").

Before you start: the Saves folder is backed up (SavesBackup30, 22 files, 0 differences). Use a throwaway new game. Tell Claude Code when you finish so it can restore the Saves folder.

Setup: cash for the Kettle (PageUp gives $1,000 in editor and development builds). Dev keys: Shift+Delete gives 12 Lye and 4 Potash (press it again for more), F5 skips a day, End exports the ledger CSVs. The Trading Post is a tab in the Map and Inventory screen.

1. Open the Trading Post Buy tab. The Potash Kettle is listed at $25.00.
2. Buy a Kettle and open the Ledger. The $25 sits under "Bought: cooking, water, fire".
3. Sell the Kettle back. It pays $10.00.
4. Press Shift+Delete, then open the Trading Post Sell tab. Lye shows $3.00 and Potash shows $16.00, and Ash still shows $2.00.
5. Sell 5 Lye. Each unit pays about 5% less than the one before, and the Potash price does not change.
6. Press Shift+Delete again for more Lye, sell down to about 10 sold in total, then skip a day with F5. The Lye price rises about a quarter of the way back toward $3.00.
7. Check the Ledger after the Lye and Potash sales. The money appears on the "Firewood, logs and stone" line. Press End, then open the exported item CSV: lye and potash are listed as separate items.
8. Hang a Kettle on a tripod (E at the tripod), then open Inventory (I). The water and sap Boil rows are disabled and say "hang the Cooking Pot (another vessel is on the tripod)". Cooking meat and fish still works. A hung Kettle does nothing else yet.
9. Load an older save and open the Trading Post. Lye and Potash are at normal price and the Console is clean (no errors or warnings).
