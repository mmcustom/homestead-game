# Pottery polish batch: playtest checklist (2026-11-03)

Code baseline 782e74f. Four fixes after the P3 playtest: Kiln prompt wording, Kiln smoke, Kiln flashing, creek message. Not committed yet. Play from your slot 4 save. Maximize the Game tab with Shift+Space.

Reply with the numbers that pass and describe anything that does not.

## A. Kiln prompt wording

1. Press the apostrophe key for a finished Kiln. Finish or empty it so it is idle and empty (Ctrl+; finishes a firing; take the pieces out). Carry no Firewood and no dry pieces. Aim at the Kiln: the R line reads "Load dry pieces and Firewood  (you're not carrying any)".
2. Carry some dry pieces and Firewood (Shift+' gives a kit). The R line no longer says "not carrying any" and offers to load.
3. Carry loadable pieces but fill the Kiln so it has no room. Note what the R line says (known: it may still say "you're not carrying any", which is slightly wrong; tell me what you see).

## B. Kiln smoke

4. Press the apostrophe key, light the Kiln. Smoke appears above it while firing.
5. The smoke looks like smoke: soft, grey, translucent, rising slowly and drifting a little. Not glossy and not solid balls.
6. The puffs fade or thin out as they rise. They do not suddenly pop out of existence (note if they do).
7. The smoke is a sensible size and height for the Kiln dome (not tiny, not a huge cloud). Tell me too big or too small, and by how much.
8. Ctrl+; finishes the firing. No new smoke starts; the puffs already up finish rising and fade away.
9. While firing, the smoke keeps going smoothly across game-hour changes (no restart or hiccup every game hour). Speed time up with F7 to check.
10. Optional: while playing, select the "Kiln Smoke" child in the Hierarchy and try puffsPerSecond, startSizeMin, startSizeMax, opacity, riseSpeed. Tell me the values you like and I will make them the defaults.

## C. Kiln flashing

11. While the Kiln fires, walk slowly around the front of it. The tan block and the orange mouth at the front stay steady. No flashing and no change of shape.
12. Look at it from far away and close up, at the front and at the sides. Still steady.
13. The mouth still glows orange while firing and looks normal when idle and when done.

## D. Creek message

14. Make heavy rain with your usual numpad rain debug keys and stay awake until the creek rises. You see "The creek is rising." (not "While you slept").
15. Open the Inventory or pause, then close it. No "While you slept the creek rose." line appears on return.
16. Real sleep: sleep (tent or sleeping bag) through a storm that raises the creek. After waking you see "While you slept the creek rose." (or "...flooded." for a flash flood).
17. If you cannot make the creek rise on demand, say so and I will ask for a dev key.

## E. Regression

18. A full Kiln firing still works: load, fire, 24 game hours, pieces come out fired and sell as before.
19. The Clay Cooking Pot still hangs on the Tripod and cooks.
20. Console shows no new errors (the PlayerController.cs:217 NullReferenceException on stopping Play Mode is the old known one).
