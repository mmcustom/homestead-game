# Hands M0c-2 Playtest Checklist (the rest of the tools in hand)

Version 0.1, 2026-11-05. Code baseline 0d6cdd3 (M0c-1). Build is uncommitted until you pass this list.
Suite expected: 1797 of 1797. Console expected: 0 errors, 0 warnings (the Unity MCP WebSocket drop during a suite run is known noise).
Before you start: Saves was NOT copied before this build, but it is hash-identical to SavesBackup50 (29 files). Continue from slot 4. Do NOT run Seed MarketData. Game tab maximized (Shift+Space).
Pass format: reply "1 - N pass" or list the numbers that failed with a note.

## A. Bows and Rifle

1. Equip the Primitive Bow. A simple bent stick with a string, held in the hands. Not the old generic block.
2. Equip the Recurve Bow. It looks different from the Primitive Bow (limb tips curve back).
3. Draw a bow in the real game. The hand and bow pull back toward you as the draw builds, and at full draw they hold with a faint tremble.
4. Release. A short snap springs forward (about 0.25 seconds) and settles. The arrow flight, damage and sound are as before.
5. Let go early (partial draw). The pose eases back, no stuck hands.
6. Equip the Bolt-Action Rifle. Stock, barrel and bolt are visible, held across the body.
7. Raise the scope slowly. The rifle lifts toward the middle of the view, and the hands disappear when the scope comes up (as in M0a).
8. Fire the rifle. A kick, then the bolt is worked (hand pulls back and pushes forward) while it reloads, about 1.2 seconds. The shot, hit and reload time are as before.

## B. Bow Drill

9. Equip the Bow Drill. Spindle, hearth board, bow and cord are visible.
10. Start a fire with it. While it works, the hand saws back and forth about three times a second, easing in and out.
11. Release or cancel partway. The sawing stops right away.
12. Let the attempt run out. The sawing stops at the end. Fire results are as before.

## C. Water, sap and traps

13. Equip the Bucket. It hangs from its handle beside the hand.
14. Fill it at water. A dip motion plays (about 1 second): down, hold, up.
15. Equip the Sap Bucket. A smaller lidded bucket.
16. Equip the Canteen. A flat round leather canteen with a neck and strap.
17. Fill the Canteen at water. Same dip motion.
18. Drink from the Canteen or Bucket. The vessel lifts to the mouth (about 1.1 seconds) and returns.
19. Equip the Spile. A small tapered wooden spout.
20. Tap a tree with the Spile. Two short drives (about 0.7 seconds). The Spile is used up as before, so the hands may lower the tool right after. Tell me if that looks wrong.
21. Equip the Box Trap, Fish Trap and Rabbit Snare. Each looks different (wooden box, wicker cylinder, stake with noose).
22. Set a trap. A place-down motion plays (about 0.7 seconds). Placement works as before.

## D. Knife and the rest

23. Equip the Knife. A short blade with a wrapped handle. Holding only: it has NO slash or stab yet (that is the melee phase).
24. Equip Flint and Steel. Flint block and steel bar.
25. Equip the Pouch, Clay Pot, Cooking Pot, Potash Kettle and Tarp. Each one shows its own simple shape, not the old grey generic block.
26. Sleeping Bag, Tent and Weather Station show whatever they showed before this build.

## E. Hand Cart and special states

27. Push the Hand Cart. The tool drops out of the hands and both hands take a forward pushing grip. They sway as you walk.
28. Stop pushing and let go. The hands come back up with your tool.
29. Placement mode (placing a structure or trap): the hands stay up holding the tool.
30. Creek knock-down (if you can get one): the tool drops out of the hands and the hands go to rest, then come back.

## F. Look, light and feel

31. Hanging items (Bucket, Canteen, pots, traps, Pouch) are recognisable beside the hand, not hidden behind the sleeve. They looked like dark shapes in Claude Code's screenshots; tell me which ones are too hard to read.
32. At night, equip each new tool in turn. Nothing is blown out white or too dark to see.
33. Walk up to a wall or tree with the Bow, Rifle and Bucket. They stay in view and do not push through the wall in a way that bothers you.
34. Switch between many tools with the real hotkeys (Bow, Rifle, Bucket, Axe, Torch, Rod). Each change is clean, no leftover shape from the previous tool.
35. Sprint with the Rifle and with the Bucket. The hands bob without clipping off screen.
36. At 21:9 or wider, nothing looks wrong with the new tools.

## G. Regression (nothing should have changed)

37. Hunting is exactly as before: draw time, damage, range, arrow use, rifle bolt time (1.3 s), scope.
38. Fire starting, water filling and drinking, sap tapping, trap setting, Hand Cart physics all behave as before.
39. No HUD text changed anywhere. The equipped-tool line at the crosshair is still there (it goes in M0d).
40. M0b and M0c-1 tools still look the same: Axe, Hammer, Shovel, Pick Axe, Rod, Torch, Lantern.
41. Nothing under Assets/Resources/Art, and no Captures folder in the project.
42. Console shows 0 errors and 0 warnings after the whole session.

## Feel questions (just answer in a line each)

- Do the bow draw and release feel right in speed and size?
- Is the rifle shot kick and bolt motion good?
- Is the Bow Drill sawing speed good?
- Which hanging items look worst and need a better pose or shape?
- Is the push pose for the Hand Cart good?
