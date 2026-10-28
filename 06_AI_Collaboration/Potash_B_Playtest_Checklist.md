# Potash B (Leach Hopper and Kettle Boil) Playtest Checklist

Date written: 2026-10-27. Build under test: Potash B, the world side of the Potash chain. The Leach Hopper (a staged site you stock and finish with the Hammer), loading Ash and water, a 24 game hour leach, collecting Lye, and the Kettle boil job (Lye and Firewood into Potash) on a tripod. 52 new test cases were added and the full suite passes in Unity (964 of 964, Console clean). Nothing has been seen in Play Mode.

Reply with the item number and pass or fail (for example "20 fail, the queue was lost").

Before you start: the Saves folder is backed up (SavesBackup31, 22 files, 0 differences). Use a throwaway new game. Tell Claude Code when you finish so it can restore the Saves folder.

Setup: for items 1 to 5 carry 3 Logs, 4 Tall Grass and the Hammer. For items 14 to 23 you need a Tripod over a lit fire (F11 gives a Tripod and the starting kit), $25 for the Kettle (PageUp gives $1,000), and Firewood (8 or more). Dev keys (editor and development builds only): Ctrl+Insert stands a finished hopper about 3 m ahead and gives 12 Ash, 6 L Unsafe water and 6 L Good water (press it twice for two hoppers and 24 Ash). Ctrl+Delete finishes every running leach. Shift+Delete gives 12 Lye and 4 Potash. F5 skips a day. WARNING: plain Insert (without Ctrl) forces a game loss, so hold Ctrl.

Important: R loads the hopper only when the Hammer is NOT equipped. With the Hammer equipped, R dismantles. Put the Hammer away for loading.

## Build the hopper

1. Open Build and find "Leach Hopper Site" with its cost. Place it. A green ghost appears on dry ground, and a plot of stakes stands once you click.
2. Carry 3 Logs to the site and press R. The prompt says "Needs 3 of 3 Logs, 0 of 4 Tall Grass".
3. Carry 4 Tall Grass over a second trip and press R. A banner says the site is fully stocked.
4. Press E before hammering. The materials come back to your pack.
5. Deliver again, equip the Hammer and hold click for about 20 blows. "Leach Hopper built" appears, and after the first blow E no longer takes the materials back.

## Load and leach

6. Press Ctrl+Insert. A finished hopper stands ahead and you carry 12 Ash, 6 L Unsafe water and 6 L Good water.
7. Drop 4 Ash in Inventory (leaving 8), put the Hammer away and press R at the hopper. The message reads "Loaded 8 Ash and 8 L of water - 8 Lye in 24 h". Inventory shows 0 Unsafe and 4 L of Good water left, so the worst water went first.
8. Press Ctrl+Insert twice more (for fresh Ash, water and a second hopper) and press R at one hopper. It loads only 12, and you still carry 12 Ash.
9. Press R again on a hopper that is leaching. The message is "still leaching - N h to go", and the Ash and water stay in your pack.
10. Aim at the hopper and press E mid-leach. It says how many hours are left.
11. Sleep through a game day, or skip a day with F5. Aim at the hopper and the prompt reads "Collect Lye (8 ready...)".
12. Press E. "Took 8 Lye" appears and the Lye is in your pack.
13. Load another batch (Ctrl+Insert for more Ash and water, then R) and press Ctrl+Delete. The prompt switches to "Collect Lye" at once.

## The Kettle boil

14. Buy a Potash Kettle for $25 and set up a tripod over a lit fire. Press E at the tripod to hang the Kettle (swap out the Pot if it is hung). The prompt reads "Potash Kettle hung".
15. With the Kettle hung, 12 Lye and 8 Firewood, open Inventory (I) and find the Lye row. The Boil button is enabled and the row reads "12 Lye + 8 Firewood -> 4 Potash".
16. In the same Inventory, check the water and sap rows. They are disabled and read "hang the Cooking Pot (another vessel is on the tripod)".
17. Hang the Pot instead of the Kettle. The Lye row is disabled and reads "hang the Potash Kettle (another vessel is on the tripod)". With no tripod at the fire it reads "needs the Potash Kettle hung on a tripod".
18. Carry only 2 Lye, or only 1 Firewood. The Lye row reads "needs 3 Lye and 2 Firewood (have ...)".
19. Queue Boil 2. The HUD reads "Boiling Lye, 2 Potash to go", the tripod prompt reads "Potash Kettle hung: boiling Lye, 2 Potash to go", and each Potash takes about 18 seconds.
20. Swap the Kettle for the Pot mid-boil. The HUD says "Boiling paused: hang the Potash Kettle on a tripod at this fire". The queue stays, and Lye and Firewood are unchanged. Hang the Kettle again and the batch restarts from the start.
21. Let the fire die, or walk more than 3 m away. The HUD says "Boiling paused: get back to a burning campfire". Come back and it resumes.
22. Check Ash after a boil. No Ash appears in your pack or at the fire, since the Firewood is an ingredient and not fuel.
23. Fill your pack close to its weight limit and boil one batch. The Potash is still made and nothing is lost.

## Dismantle, save and sell

24. Load a hopper, then dismantle it mid-leach with the Hammer (R). You get 1 Log, 2 Tall Grass and the pending Ash back, and the water is lost.
25. Dismantle a hopper that holds ready Lye. The Lye comes back with the half materials.
26. Load a hopper, then save, quit and load. The timer and Ash are kept and the batch finishes on time.
27. Load a save from before Potash B. Everything loads, with no hoppers and no errors.
28. Sell the Potash at the Trading Post. One Potash pays about $16 and the Ledger shows it on the "Firewood, logs and stone" line. The Console is clean throughout.
