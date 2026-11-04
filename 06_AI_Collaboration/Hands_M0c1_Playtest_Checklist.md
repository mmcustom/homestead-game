# Hands M0c-1 Playtest Checklist (Fishing Rod, Cane Pole, Torch, Lantern in hand)

Version 0.1, 2026-11-04. Code baseline 175a8c7. Build is uncommitted until you pass this list.
Suite expected: 1770 of 1770. Console expected: 0 errors, 0 warnings.
Before you start: Saves is untouched (hash copy SavesBeforeM0c1 exists). Continue from slot 4. Do NOT run Seed MarketData. Game tab maximized (Shift+Space).
Pass format: reply "1 - N pass" or list the numbers that failed with a note.

## A. Fishing Rod and Cane Pole in hand

1. Equip the Fishing Rod. The hands come up holding a rod: right hand on the grip, left hand supporting it. Rod points forward and slightly up, not through the screen edge.
2. Equip the Cane Pole. Same idea, a longer, thinner pole. It looks different from the Fishing Rod.
3. Face water and click to cast. The rod pulls back, then whips forward (about 0.6 seconds), and the line flies out from the rod tip.
4. The fishing line starts at the rod TIP (the end of the rod you see), not from a spot in front of your face.
5. While waiting for a bite, the rod rests steady with a slight lean, hands do not float away.
6. When a fish bites, the rod tip dips (about 0.35 seconds) at the same moment the bite sound or signal happens.
7. Reeling: while you hold the reel button, the right hand cranks around smoothly. It stops when you let go.
8. Catch a fish. The rod lifts with a short hook-set motion (about 0.6 seconds) and the fish is landed as before.
9. Miss a fish or let it get away. The rod snaps back or drops briefly (about 0.45 seconds), then returns to rest.
10. Line snap (if you can make it happen). Same short snap pose, no stuck hands afterward.
11. Reel the line in with nothing on it. The line draws back toward the rod tip and the hands return to the ready pose.
12. Do the same cast, bite, catch with the Cane Pole. It behaves the same.

## B. Torch and Lantern in hand

13. Equip the Torch (unlit). The hand holds a plain dark stick or torch head, no flame, no glow.
14. Light the Torch. The hand does a short light-up motion (about 0.45 seconds) and a flame appears at the top. The flame flickers.
15. Snuff the Torch. A short lower-and-snuff motion (about 0.4 seconds), the flame disappears, the light goes out.
16. Equip the Lantern. It hangs or is held in the hand, a little higher than the other tools (raised ready pose).
17. Light the Lantern and snuff it. Same states as the Torch (lit shows glow, unlit shows none).
18. Held Light: while lit and your hands are showing, the light you see comes from the flame, not from beside your face. Walk past a wall at night and look at how the light falls.
19. The light is NOT blown out white on the hands or the wall right next to you. Tell me if it looks too dim or too bright compared with before.
20. Light strength is unchanged: it reaches the same distance as before this build.
21. Put the light away or switch to another tool. The light goes back to its old spot (beside you) and still works normally.

## C. Night, walls, and hotkeys

22. At night, walk around with the Torch lit. The flicker looks natural, not strobing.
23. Walk up to a wall or tree with the Lantern. The tool stays in view and does not pass through the wall in a way that bothers you.
24. Switch between Rod, Torch, Lantern, and an Axe using the real hotkeys. Each change is clean, no leftover rod or flame from the previous tool.
25. Sprint while holding the Torch and while holding the Rod. The hands bob without clipping off screen.

## D. Test prefab menus (Play Mode stopped)

26. Menu Homestead > Art > Create Test Rod Prefab: it creates a test prefab. Enter Play Mode, equip the Rod, and the test prefab shows in hand instead of the primitive rod.
27. Menu Homestead > Art > Remove Test Rod Prefab: it removes it. The primitive rod is back.
28. Menu Homestead > Art > Create Test Torch Prefab: the torch/lantern show the test prefab and the flame sits at the prefab's flame anchor.
29. Menu Homestead > Art > Remove Test Torch Prefab: back to primitives. Afterward, nothing is left under Assets/Resources/Art.

## E. Regression (nothing should have changed)

30. Fishing works exactly as before: same bite timing, same catches, same bait use, same fish sizes.
31. Torch and Lantern fuel or burn behaviour is unchanged.
32. No HUD text changed anywhere (no new words on screen, nothing smaller than before).
33. Axe, Hammer, Shovel, and Pick Axe swings and pull-backs still look the same as the M0b test.
34. Rifle scope still hides the hands.
35. Console shows 0 errors and 0 warnings after the whole session.

## Feel questions (just answer in a line each)

- Do the rod poses (cast, bite, catch, miss) feel right in speed?
- Is the crank speed good?
- Is the flame light position good, or should it sit closer to or farther from the flame?
- Is the Lantern position good where it is?
- Anything that looks wrong at 21:9 or wider?
