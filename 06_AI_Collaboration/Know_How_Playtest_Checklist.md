# Know-how (Journal Teaching Pass) Playtest Checklist

Date written: 2026-10-27. Build under test: the teaching pass. A new Know-how section in the Journal (J) with 7 step-by-step entries that unlock the first time you hold or build the key thing, plus new hints on item rows, prompts and flashes that name the next step, plus three Inventory text fixes. 34 new test cases; the full suite passes in Unity (998 of 998, Console clean). Not yet seen in Play Mode.

Reply with the item number and pass or fail (for example "10 fail, the prompt still says Dismantle").

Before you start: the Saves folder is backed up (SavesBackup32, 22 files, 0 differences). Use throwaway new games. Tell Claude Code when you finish so it can restore the Saves folder. For item 17 you need an older save that you keep a copy of.

Setup: F11 gives the starting kit, F12 the full kit. Dev keys (editor and development builds only): Ctrl+Insert stands a finished hopper with Ash and water, Shift+Delete gives 12 Lye and 4 Potash, Delete gives 20 L of sap, Ctrl+Delete finishes every leach.

## New game start

1. Start a new Settler (or Homesteader) game. On the first moments of play a flash reads "Journal: new know-how, The Tripod and Boiling Water" for about 4 seconds, a sound plays, and the Know-how tab (J) shows a count of 1 with an unread dot. Nothing else flashes at the same time.

## Ash and the Journal (use a new Pioneer game from here)

2. Collect Ash with G for the first time (burn some wood first). The flash adds "Sell it, or leach it into Lye in a Leach Hopper (Build)." After it clears, "Journal: 2 know-how entries added" flashes and a sound plays.
3. Press J and open the Know-how tab (the last tab). It shows a count of 2 and an unread dot.
4. Select each entry. The dot clears, the text matches the file Journal_KnowHow_Text.md, and it cannot be edited.

## Unlocks

5. Pick up (or craft) a Tripod, a Spile, a Sap Bucket and a Kindling Bundle. Each one flashes its own Know-how message once. Picking up a second Spile or Bucket flashes nothing. (If you already have the Tripod entry from a Settler start, the Tripod does not flash again.)
6. Aim at the tripod and hold E to hang the Pot. The message ends "Open Inventory (I) to Boil water or sap." The Tripod entry does not flash a second time.
7. Swap in the Kettle. The message ends "Open Inventory (I) to Boil Lye."
8. Place a Leach Hopper site. "Using the Leach Hopper" unlocks, and the Build entry's info line reads "Leaches Ash into Lye for Potash...".

## Hopper prompts and flashes

9. Finish the hopper with the Hammer. The flash reads "Leach Hopper built. Put the Hammer away, then hold R to load Ash and water."
10. Aim at the empty hopper with the Hammer equipped. The prompt says "Put the Hammer away to load Ash and water", and the R line is Dismantle.
11. Put the Hammer away and aim at it. The R line reads "Load Ash and water (1 Ash and 1 L each, up to 12...)".
12. Load it, finish the leach (Ctrl+Delete) and collect Lye. The flash ends "Boil 3 Lye + 2 Firewood into Potash in a Potash Kettle at a fire."

## Inventory text and the tap hint

13. Open Inventory (I) at a lit fire with Lye, Firewood, a Water row and an Equipped Tool selected. The Lye leftover text is fully visible, the Water text is clear of its Boil button, and the Equipped Tool help on the right is not cut off. Ash, Lye and the Kettle rows show a short tip.
14. Aim at a tap that holds sap while carrying under 10 L. The prompt ends "10 L boils to 1 Syrup", and that part disappears once you carry 10 L.
15. Check the keys: holding E (and R) at a tripod, a tap and a hopper works as the entries and prompts say.

## Saves and a new game

16. Save, quit and load. The unlocks and which ones are read are kept.
17. Load a save from before this build while holding Ash or a Kettle. One flash reads "Journal: N know-how entries added", the Console is clean, and the entries are unread.
18. Start a new Pioneer game. The Know-how tab is empty and the Console is clean.
