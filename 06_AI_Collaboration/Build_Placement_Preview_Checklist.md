# Build Placement Preview - Play Mode Checklist

Written 2026-10-18 (Claude). Feature: Placement Mode (Building_Housing_System.md, "Build Placement Preview"). Phases 1 to 5 built and compile-checked (0 errors, 0 warnings), never run in Play Mode. Reply with "1. pass", "7. fail - what happened", and so on.

**Setup.**
- Back up slot 1 first (or use a COPY of a save that has a finished Small Cabin: the slot panel's Copy, then load the copy). Items 30 and 31 save and load, so do those on the copy.
- Have materials for each thing you want to build: Firewood or Sticks (Campfire), Sticks, Logs, Branches, Cordage, a Tarp and a Tent in the pack. Keep an eye on weight (a Log is 8 kg).
- Stop and restart Play Mode first so the old scripts are gone.
- Keys while placing: **left click** builds, **Esc or right click** cancels, **mouse wheel** turns (15 degrees a notch) on the structures that have a front. Green ghost = allowed, red = refused with the reason shown in the line above the keys.

## A. Starting and cancelling

1. Open Craft and Build (B), pick Wood Pile, click Build: the screen closes, a **translucent green** ghost appears where the crosshair meets the ground, and a line at the bottom reads "Place Wood Pile here" with the keys underneath. Nothing is spent yet (check the pack).
2. Walk around while the ghost is up: the game keeps running (time passes, you move, the ghost follows your aim).
3. Press **Esc**: the ghost goes, "Placement cancelled - nothing was spent." shows, **the pause menu does NOT open**, and the pack is unchanged.
4. Start again, press **right mouse**: same as 3.
5. Start again, open another screen (I for Inventory, M for Map): placement ends cleanly, no ghost left behind, the screen opens normally.
6. Start again, then pause (it needs Esc, so use the pause by alt-tabbing or via a screen): placement is gone afterwards. (If you can only pause with Esc, mark this "n/a, Esc cancels first".)
7. Start with the **Axe** equipped and **left click to confirm**: the structure is built and **the Axe does not swing** (no stamina spent, no chop sound, no tree/pile hit). Try the same with the Rod and a Trap equipped if you have them.
8. While placing, press **E** and **R** at a pile, a shelter and a campfire: nothing opens or happens.

## B. Every Build entry (ghost colour, look, building)

For each: start from Craft and Build, check the ghost is green in open flat ground, red with a reason in a bad spot, then click to build and check the structure appears **exactly where the ghost was**, with the materials spent only now.

9. **Wood Pile**: ghost shows the two poles of the frame.
10. **Rock Pile**: ghost is a small heap of stones.
11. **Water Barrel**: ghost is a round barrel; the wheel does nothing.
12. **Rain Barrel**: bigger barrel with a dark rim band (see section C for snapping).
13. **Food Cache**: a low box; the wheel turns it.
14. **Storage Bin**: a low box; the wheel turns it.
15. **Tool Rack**: a standing frame; the wheel turns it.
16. **Tarp Shelter** (needs a Tarp): the tarp sheet on its ridgeline; the wheel turns it.
17. **Tent** (needs a Tent): the A-frame; the wheel turns it.
18. **Lean-To**: the branch lean-to; the wheel turns it.
19. **Small Cabin site** ("Place Site"): four corner posts **plus a thin square outline on the ground** about 3 m a side; the wheel turns it. After building, deposit materials and complete it as before (this path is unchanged), and the finished cabin faces the way the site did.
20. **Campfire**: a ring of stones with crossed logs; the wheel does nothing. After building, light it as before.

## C. Rain Barrel snapping

Needs a finished Small Cabin. The slope is named as seen standing outside the door looking in: "left slope" / "right slope".

21. Aim at the middle of one long wall (the roof side): the ghost **jumps to an eave spot** just outside the roof edge, is green, and the line reads "Place Rain Barrel here - right slope (or left) of the cabin (11.5 m2 of roof)".
22. Slide your aim along that wall: the ghost moves in steps along the wall, never into the wall and never out past the roof ends.
23. Walk round to the **other long wall** and aim: it snaps there too, and the line names the **other** slope.
24. Do 21 from the **door side** and from the **back wall side** of the cabin (standing at each end): the ghost still snaps to a long-wall eave when you aim within about 3 m of one, and does not snap to the door or back wall.
25. Build a barrel, then aim at the next spot along the same wall: it moves to the next free spot (about 1.2 m along). Fill the wall: up to five per side; a full wall shows red with "Too close to another pile." at the nearest spot.
26. Aim at open ground far from any cabin: the ghost stays red **at the aim point** with "A Rain Barrel stands just outside a Small Cabin's long wall, under the roof's edge."
27. Stand more than 6 m from an eave and aim at it: red with "Too far - come within 6 m." Step closer: green.
28. The barrel fills in rain exactly as the Rainwater checklist expects (spot check with Numpad 1 then Numpad 2).

## D. Range, red reasons, rotation

29. Aim at the ground further than 6 m: red "Too far - come within 6 m." Aim at the sky: no ghost, line says "Aim at the ground nearby."
30. Red reasons, one each where you can set them up (they use the same wording as before): water ("No clear ground there." for the lake surface), steep ground ("The ground there is too steep."), within 2 m of another pile or building ("Too close to another pile."), near a lit fire ("Too close to the fire."), a Cabin Site near trees or stumps, a second Cabin Site ("You already have a Small Cabin site..." is shown before placement starts, on the Build button), a Campfire within 2 m of another campfire ("Too close to another campfire."), too few materials (the Build button's banner, as before).
31. Click while the ghost is **red**: nothing is built or spent, the reason flashes, and you stay in Placement Mode.
32. Mouse wheel on Lean-To / Tent / Tarp / Tool Rack / Storage Bin / Food Cache / Cabin Site: turns 15 degrees a notch in both directions; the built structure faces exactly the way the ghost did (door of the Tent / Cabin on the side you chose).
33. Wheel on barrels, piles and the Campfire: nothing turns.

## E. Cleanup and saves

34. During placement press F5 (quick save) if you use it, or save from the pause menu after cancelling: the save works, and **loading it later shows no ghost or phantom pile** where you were aiming.
35. Start placing, then load a save (from the slot panel / main menu path): placement ends, no ghost left in the loaded world.
36. Start placing, then let yourself collapse or sleep (or sleep in a shelter first and note you cannot start placing while asleep): placement ends and the ghost is gone.
37. Start placing and cancel five times in a row: no ghosts accumulate (look around the area, and check the Hierarchy for "Placement Ghost" objects if you are in the editor).
38. Build, then Take Down the structure (R / Take Down): the refund is as before (half the materials), nothing odd about the ghost.

## F. Regression: the old fixed-probe path

39. Nothing else calls the old "just in front of you" placement any more from the screens, but the old methods (CanBuildPile / BuildPile(kind, player) and FireManager.CanBuild / Build(player)) still exist with the same behaviour. If you have a debug or macro that uses them, or after any future code that does, check a build "just in front of you" still gives the old messages ("No clear ground in front of you.").
40. The Rainwater checklist items about placing a Rain Barrel (2 to 8) were written for the old probe: redo them through Placement Mode (the barrel now snaps to the eave, so the "too close to the wall" and "door end" cases show red instead of being refused after a click).

## Known gaps (not bugs to report unless they get in your way)

- A Campfire can still be built on any solid surface the aim ray hits (its old rule took any collider but the player's), including a roof or a rock.
- There is no ring showing the 2 m spacing around the ghost, only the colour.
- Traps, snares and the Fish Trap still use the old click-to-set aiming (not part of this build).
- The Complete Small Cabin step is unchanged.


## RESULTS (Mike, 2026-10-18)

Items 1-40: ALL PASS (reported by Mike in one message). Note: the setup text above says "Items 30 and 31 save and load"; the save/load items are really 34 and 35 (numbering slipped when written). Not covered by this checklist and still untested: traps/snares/Fish Trap placement (phase 6, not built), gamepad placement (not supported), and the known gaps listed above (Campfire on a roof or rock, no 2 m spacing ring, lake message).
