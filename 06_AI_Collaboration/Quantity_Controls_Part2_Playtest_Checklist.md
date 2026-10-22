# Quantity Controls Part 2 Playtest Checklist

Build: code b306150 plus uncommitted Part 2 (Inventory drop, split, cook, boil, lantern refill, Crafting).
Report results by item number ("1. pass", "7. fail: ...").

## Before you start

- Back up the Saves folder (copy it somewhere outside the project).
- Start a new game, or load a save. Press F12 once for the debug kit. Press it only once, because it gives the whole kit every press and will overload you.
- You will need: feathers or another stack to drop, logs and branches (for splitting), raw fish and venison (for cooking), a container of unsafe water and a Cooking Pot (for boiling), a lantern and Lamp Oil (for refilling), and materials for Arrows and Cordage (for crafting).
- Keep the Unity Console open and clear it before starting.

## A. Inventory layout

1. Press I. Each row is now two lines. Top half: name and tags, hotkey button, quantity and weight. Bottom half: stepper, status line, then buttons (Drop n, then Cook / Boil / Split / Refill n, then Cancel when something is queued).
2. Clicking the top half of a row still equips a tool, eats or sleeps as before. Clicking anywhere in the stepper line never equips or eats.
3. With a long list, the Inventory scrolls and every row's stepper still works after scrolling.
4. The help text on the right is fully visible, not cut off.
5. Make the game window small (1024x768). Rows stay readable with no overlap; the list scrolls.

## B. Drop

6. On a stack of 12 feathers (or similar), click plus four times. The button reads "Drop 5". Click it. Exactly 5 drop on the ground and 7 remain. The amount returns to 1. One pickup appears in the world.
7. Shift-click Drop on a stack. The whole stack drops, whatever the stepper says.
8. Click Max, then Drop. The whole carried count drops.
9. Try to drop where there is no room (a tight spot, a slope, or at the edge of water). Note what happens. Nothing should vanish; either it drops or you get a message.

## C. Split firewood

10. With logs in the pack, set the stepper to 3 and click Split. The button reads "Split 3". The row shows progress and 3 logs are queued.
11. Click Cancel while it is running. The split stops and Cancel disappears.
12. Shift-click Split on a stack. All are queued.
13. With Branches (which split in lots), the stepper clamps to what the pack can split and the row shows how many ("4 to split"). Nothing queues beyond that.
14. Without the Axe, Split is not available and the row says why.

## D. Cook and boil

15. Light a real campfire (build it with sticks and light it, not by debug). Stand next to it. With raw fish, set the stepper to 2 and click Cook. The button reads "Cook 2". 2 fish are queued and the row shows progress ("Queued", then a percentage).
16. Let the cook finish. You get cooked fish for the amount queued and the raw count drops to match.
17. Queue a cook, then click Cancel. It stops. The stepper and Cancel do not fight (clicking Cook again queues more, it does not cancel).
18. Shift-click Cook on a stack of venison with the stepper at 1. The whole stack is queued.
19. Queue a cook, then walk away out of the fire's range. Note what happens (the cook should pause or stop sensibly, not vanish items). Walk back and see if it resumes.
20. With unsafe water and the Cooking Pot, set the stepper to 4 and click Boil. "Boil 4" queues 4. Let it finish.
21. Drop or put away the Cooking Pot. Boil is disabled and the row says "needs the Cooking Pot".
22. Queue a cook and quit to the menu, then load. Nothing is duplicated or lost (queues are not saved, so the raw items should still be in your pack).
23. Cook and boil still require a lit fire nearby. With no fire, the buttons are unavailable and say why.

## E. Lantern refill

24. With 5 Lamp Oil and an empty lantern, set the stepper to 5. The button reads "Refill 2" (it clamps to the room in the tank). Click it. The tank is full, 3 oil remain.
25. With a full tank, Refill is disabled or does nothing, and the Lamp Oil row is still droppable.
26. Shift-click Refill with oil in the pack and room in the tank. It pours as many as fit.
27. With a half-full tank, Refill pours only what fits and does not waste a whole bottle.

## F. Typing amounts and hotkeys

28. Click the amount box on an Inventory row, type 3, and click Drop. Exactly 3 drop.
29. Type 999999. It clamps to the carried count. Type letters: ignored. Type 0: becomes 1.
30. Click the amount box, type 2, then press Esc. Typing ends and the Inventory stays open. A second Esc closes it as usual.
31. Click the amount box and type a number. While you are typing, something changes in your pack (for example a cook finishes). The typed number is still there and is not lost.
32. Hover over a tool row and press a digit key (1 to 9) without typing in a box. The hotkey assigns, as before.
33. Click a quantity box, then type a digit. The digit goes into the box and does NOT assign a hotkey.
34. While a box has focus, press T and M. They do not open the Trading Post or Map. After clicking away or pressing Esc, they work again.

## G. Crafting

35. Open Crafting and select Arrows. The x1 / x5 / Max buttons are gone, replaced by a stepper above the Craft button.
36. Click plus twice. The button reads "Craft x3". Craft. You get 3 batches' worth and the materials are used for exactly that. The amount returns to 1.
37. Type 500. It clamps to what your materials allow. Type letters: ignored.
38. Click Max with plenty of materials. It stops at 99. With little material, it stops at what materials allow.
39. Select a single-craft recipe. The stepper is hidden and Craft works as before.
40. Select a different recipe. The amount starts at 1 again.
41. The Craft and Build button and the notice line are fully visible and not overlapping the stepper or help text.

## H. Regression and console

42. Trading Post (T) and Storage still work exactly as in Part 1 (do a quick buy, sell, store and take).
43. Campfire fuel is unchanged: E adds 1 of the selected fuel, Shift+E adds as many as fit.
44. Walking, camera and the pause menu behave normally after using the Inventory and Crafting.
45. Save, quit to menu, load. Cash, pack and storage contents are exactly what they were before saving.
46. Watch the Console the whole time. Claude Code saw "An abnormal situation has occurred: the PlayerLoop internal function has been called recursively" 5 times in one test session (it believes it came from its test harness, not the game). If you see it, note exactly what you were doing and whether the screen went blank. Report any other red errors or yellow warnings.

## Notes for Claude

- Anything that felt awkward (rows too tall, buttons too small, text too small at 1024x768, one stepper feeding several buttons confusing) goes here, even if it passed.
- Claude Code chose Stepper max = carried count on every row, and each button clamps to its own limit (so "Refill 2" shows when the stepper holds 5). Did that feel right, or would you rather the stepper itself stop at the smaller number?
- Is the 84 px row height too tall for the Inventory with many items?


---

## Result

Played by Mike: items 1 to 46 all pass. No notes added.
