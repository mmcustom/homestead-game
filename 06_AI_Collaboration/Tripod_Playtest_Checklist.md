# Tripod (Hanging Cooking Pot) Playtest Checklist

Date written: 2026-10-27. Build under test: commit T, the Tripod. The Cooking Pot no longer needs to be carried: it hangs on a Tripod set up over a fire, and water and Maple sap boiling now need the Pot hung at a lit fire in reach. 55 new test cases were written and the full suite passes in Unity (858 of 858, Console clean). Nothing has been seen in Play Mode.

Reply with the item number and pass or fail (for example "13 fail, the queue was lost").

## Step 0: tests (done)

Once the Unity bridge was reconnected, Claude Code forced a full recompile and ran the whole edit-mode suite: 858 of 858 passed (803 plus 55 new, including the 11 world tests), the Console showed 0 errors and 0 warnings before and after. Nothing has been seen in Play Mode yet, so the items below are the first real look.

## Before you start

The Saves folder is backed up (SavesBackup29, 22 files, diff -r 0 differences). Use a throwaway new game. Tell Claude Code when you finish so it can restore the Saves folder.

Setup: a Homesteader game has the Hammer, Tripod and Pot at the start. If you start with another tier, F11 gives the starting kit (including a Tripod) and F12 gives the full kit. Items 1 and 2 need 3 Branches and 1 Cordage (fell a tree if you have no Branches). Dev keys (editor and development builds only): Delete gives 20 L of Maple Sap, F5 skips a day.

## Craft and set up

1. Open Craft > Fire and Light with 3 Branches and 1 Cordage and craft a Tripod. A 3 kg Tripod is in the pack.
2. Try to craft one without the materials. The banner says what is missing.
3. Open Build > Other with no Tripod and click Set Up. It refuses with a message starting "Needs a Tripod".
4. Carry a Tripod, build a Campfire, and click Set Up. The screen closes and a green ghost sits on the nearest fire within 3 m.
5. Aim more than 3 m from any fire. No ghost shows and the HUD says "Aim at a fire within 3 m of you."
6. Left-click the green ghost. Poles stand over the fire, the Tripod leaves the pack, and the message tells you to press E to hang the Pot.
7. Try to set up a second tripod on the same fire. The ghost is red with "This fire already has a tripod."
8. Try the Grill's fire. The ghost is red with "The Grill's firebox is too small for a tripod."

## Hang the Pot and boil

9. Aim at the poles with the Pot in the pack and press E. The prompt reads "Hang the Cooking Pot on the tripod", the Pot appears hanging over the fire, and it leaves the pack.
10. Aim at the fire ring, not the poles, and press E. The fire still lights or takes fuel as before.
11. With the fire lit and raw water in the pack, open Inventory (I) and use Boil. The row is enabled and boiling gives Purified Water with the progress bar.
12. Press Delete for 20 L of sap and Boil 2. About 24 seconds later you have 2 Maple Syrup.
13. During a boil, take the Pot down with E at the tripod. The HUD says "Boiling paused: hang the Cooking Pot on a tripod at this fire" and the queue stays in Inventory.
14. Hang the Pot again. The boil restarts from the beginning of the batch, and no sap or water was used up meanwhile.
15. At a lit fire with no tripod, open Inventory. The Boil row is disabled and says "needs a tripod with the Cooking Pot hung".
16. At that same fire, queue Cook on raw venison. It still cooks (cooking meat and fish does not need the tripod).
17. Walk more than 3 m from the fire during a boil. The HUD says "Boiling paused: get back to a burning campfire". Return and it resumes with its progress kept.

## Take it down

18. Look at the tripod while the fire is lit. The R prompt says to put the fire out first. With the fire out and no Hammer equipped it says it needs the Hammer.
19. With the fire out and the Hammer equipped, press R at the tripod. The Tripod and the Pot return whole to the pack, and the fire stays.
20. Dismantle the whole Campfire with a tripod and Pot up. The prompt's "back:" line lists the Tripod and Pot, and both arrive in the pack.
21. Put a tripod on a Cabin hearth or Kitchen hearth, hang the Pot and boil. It works. Dismantling the Kitchen or Cabin returns them and shows "Took down the tripod".

## Saves, kits and Console

22. Save, quit and load with the Pot hung. The tripod and Pot are still there and boiling works.
23. Load an older save that has a Pot in the pack and no tripod. The Pot stays in the pack and boiling says it needs a tripod.
24. Start new games on all three tiers. Settler and Homesteader carry a Tripod and the Pot, the Pioneer carries neither. The Console is clean throughout.
