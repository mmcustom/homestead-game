# Tree Regrowth Playtest Checklist

Build: Tree Regrowth (uncommitted), 1183 edit-mode tests passing. Code baseline 302445c. Design: 05_Documentation/Game_Systems/Tree_Regrowth.md.
Reply with item numbers and pass or fail (and what you saw if fail). For items 5 and 6 please send the blow counts.

Before you start
- Maximize the Game tab (Shift+Space). Use a throwaway new game. SavesBackup38 is the safe copy of Saves (it also holds your four real slots).
- F12 gives the full kit (Axe, Saw, Shovel and materials). Time keys: F5 skips 1 day, Shift+F5 skips 10 days, F6 skips to the next season, Ctrl+F5 regrows every felled tree now.
- Count days from the day you fell the trees. A tree sprouts on day 14 and is full on day 112.
- Do not worry about the Console. When you finish, Claude Code reads the [Trees] timing lines for me.

Sprouting
1. Near each other, fell 3 tall hardwoods, 1 broad hardwood and 1 shrub with the Axe. Hardwoods leave stumps, the shrub leaves nothing.
2. Press Shift+F5 once (10 days). The stumps are still there and nothing has sprouted.
3. Press F5 four times (day 14). Small sprouts appear on the hardwood stumps and where the shrub stood.
4. Fell a sprout with the Axe. It takes 1 blow and gives Branches and Sticks but no Logs (a seedling gives 0 Logs).

Digging and clearing
5. On a stump that has a sprout (before day 63), equip the Shovel and dig it out. The stump and the sprout go together and you get 1 Firewood. Then skip 112 days (Shift+F5 eleven times plus F5, or F6 four times): that spot stays empty for good. If the Shovel cannot target the stump when a sprout stands there, tell me.

Growth and yields
6. Skip forward in steps of Shift+F5 and watch the other trees grow. At about day 56 an average broad tree takes about 5 Axe blows (3 with the Saw) and a tall one about 7 (4 with the Saw). At about day 98: broad about 10 (6 with the Saw), tall about 13 (8 with the Saw). Send the blows you count, with the day.
7. Around day 63 the stump of a growing tree disappears into the trunk (it is swallowed).
8. A fully regrown tree (day 112) gives what a full tree always did (average tall about 3 Logs, broad about 2).

Blocking
9. Stand on a spot that is due to sprout, then skip a day. Nothing grows while you stand there. Move away and skip another day: it sprouts then, and its year starts from that day.
10. Fell a shrub, then build a Cabin Site within 3 m of that spot (allowed, a shrub leaves no stump). Skip past day 14 and day 112: nothing sprouts within 3.2 m of the Cabin Site.
11. Put a Campfire within 2 m of a spot that is due to sprout: it waits until the fire is gone.

Dev key, saving and your saves
12. Ctrl+F5: every felled tree that can grow back is full size at once, the stumps are gone and you see "Regrew N felled spot(s) to full size". Spots you dug out stay empty.
13. Save and reload while some trees are half grown. They keep their size and keep growing (stumps keep their clocks).
14. Load your real slot1 (it has 13 stumps). It loads with no errors and the stumps are still there. Skip 112 days: they grow into trees. (Saves are restored from SavesBackup38 afterwards, so nothing you do to your real slots is kept.)

Maples, hitches and tools
15. Fell a tapped Maple: the Spile and Sap Bucket come back. Skip 56 days: the sapling shows no tap prompt. Press Ctrl+F5: it is a full Maple and can be tapped again.
16. Sleep through a night, then use Shift+F5 a few times with many felled spots. Any freeze should be a brief hitch at most. Tell me if it stutters.
17. The Hand Cart still stops at trees (try a sapling), and the Wood Saw still fells and splits as before.

Result (2026-10-31): items 1 to 17 all passed. Claude Code's Console readout: 0 errors; tree rebuilds 24 to 50 ms (max 49.0 ms), flat whatever the number of spots changed. Blow counts for items 5 and 6 were not recorded.
