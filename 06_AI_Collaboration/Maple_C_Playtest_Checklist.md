# Maple C (Boil Sap, Sell Syrup) Playtest Checklist

Date written: 2026-10-27. Build under test: boiling Maple Sap into Maple Syrup at a lit fire (10 L per batch, 12 s), the Boil row in Inventory, selling Syrup at the Trading Post (776 edit-mode tests pass, not yet Play Mode tested).

Reply with the item number and pass or fail (for example "9 fail, progress was lost").

Before you start: the Saves folder is backed up (SavesBackup26, 22 files, diff -r 0 differences). Use a throwaway new game. Tell Claude Code when you finish so it can restore the Saves folder.

Setup: a game with a Cooking Pot, Flint and Steel and some Firewood (F11 gives them), standing at a lit fire. Dev key: Delete gives 20 L of Maple Sap into the pack (editor and development builds only). Open Inventory (I) for each boil. Page Down teleports to a Maple, Home forces sap flow, Shift+Home fills buckets (for item 15).

## Starting a boil

1. With less than 10 L of sap, look at the lit fire. The hint must not offer "Cook or boil from Inventory (I)" for sap alone. With 10 L or more it reads "Cook or boil from Inventory (I)".
2. Hold exactly 8 L (drop the rest). The Maple Sap row's Boil is greyed with "needs 10 L of sap, have 8 L". Nothing can be queued.
3. With 10 L or more but no Cooking Pot, the row shows "needs the Cooking Pot" and Boil is greyed. Carrying the Pot enables it.
4. Hold 10 L and click Boil. Progress runs about 12 s with the HUD "Boiling Maple Sap, 1 Syrup to go". At the end the flash reads "Boiled 10 L of Maple Sap into 1 Maple Syrup". You have 0 sap and 1 Syrup, and no Firewood was used.
5. Hold 16 L. The info line reads "16 L -> 1 Syrup, 6 L left over". Boil: you get 1 Syrup and keep 6 L of sap. Boil is greyed again with the need text.
6. Press Delete to get 20 L. Shift-click Boil: two batches run back to back (HUD "2 Syrup to go"), ending with 2 Syrup and 0 sap. (The stepper counts litres; setting it to 20 and clicking Boil also queues 2 batches.)

## Interrupting a boil

7. Cancel: start a batch and click Cancel at about 50%. Nothing is removed (sap intact, no Syrup).
8. Walk away: start a batch and walk more than 3 m from the fire. The HUD says "Boiling paused". Return: it resumes where it was, finishes, and nothing is lost.
9. Fire out: start a batch and put the fire out (R on the fire) at about 50%. The boil pauses with the same message and keeps its progress. Relight it and it finishes. No sap or Syrup is lost.
10. Quit to the menu mid-boil and load again: the queue and its progress are not saved, all the sap is still in the pack, and no Syrup was made.

## Eating and selling

11. Eat a Syrup: +8 food.
12. Trading Post, stepper: the Sell list shows Maple Syrup at $10 with a "n -> $" total. Set the stepper to 2 and click Sell: 2 Syrup sell at a falling price (5% off per unit, floor 25%). Maple Sap is not in the Sell list.
13. Shift-click Sell on the Syrup row: it sells the lot.
14. Press End. In the exports folder, ledger_items_*.csv has a maple_syrup row with the units and dollars you sold, and the season ledger_*.csv shows the same dollars in the Farm produce column.

## Tapped sap and the dev key

15. Tapped sap boils too: tap a Maple (Page Down to teleport, Home to force the flow, Shift+Home to fill buckets). Collect sap with E from two buckets (up to 16 L), then boil it as in item 5.
16. Press Delete with the pack nearly full. The message "Maple Sap given: N of 20 L fit" appears and only what fits is given.

## Console

17. No errors or warnings in the Console during any of the above.

## Also watch for

- whether the "Boil N" label fits the button (same width as Refill, so it should);
- the Boil row and the Drop row sharing one stepper (it counts litres, so Boil makes one Syrup per 10 L shown, at least 1).
