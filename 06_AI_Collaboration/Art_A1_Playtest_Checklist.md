# Art readiness A1 (ArtSwap and the Kiln pilot): playtest checklist (2026-11-04)

Code baseline 948abad. Not committed yet. Stop Play Mode before sending anything to Claude Code. Play from your slot 4 save. Maximize the Game tab with Shift+Space. Do not restore any save.

What A1 is: a swap point for structure art. If a prefab named structure_kiln sits in Assets/Resources/Art, the finished Kiln is drawn from it instead of from code shapes. With no prefab, the Kiln looks and works exactly as before. Nothing about gameplay or saves changed. To test it without an artist, there is a menu that makes a plain test prefab (a slab, four coloured state blocks, an orange glow block and a small marker cube at each anchor) and a menu that removes it again. You need a finished Kiln in slot 4 (use yours, or any way you used before to get one).

What the test prefab looks like: the slab is the body and is always on. Four coloured blocks stand for the Kiln phases (Empty, Loaded, Firing, Done), and only the one that matches the phase shows. The orange block is the firing glow. The marker cubes show where the code attaches things: smoke point (above the slab), mouth (in front, where pieces go) and stock (to the left, where firewood goes).

Menu items (top menu bar, Homestead > Art): Create Test Kiln Prefab and Remove Test Kiln Prefab. Create the prefab while Play Mode is stopped, then enter Play Mode. Remove it while Play Mode is stopped.

Reply with the numbers that pass and describe anything that does not.

## A. Baseline (no prefab)

1. With no test prefab, look at your Kiln empty, loaded, firing and done (skip time with Numpad 2 or Shift+Numpad 2 as needed). It looks and behaves as before: clay dome, vent, mouth ember, KilnSmoke while firing.

## B. With the test prefab

2. Stop Play Mode. Homestead > Art > Create Test Kiln Prefab. The Console shows no error or warning. The prefab appears at Assets/Resources/Art/structure_kiln.prefab.
3. Enter Play Mode, load slot 4 and walk to your Kiln. The dome is gone: you see the slab, one coloured state block (Empty) and the three marker cubes.
4. Put firewood in. The firewood sits at the stock marker cube (to the left), as it sat at the left of the old Kiln.
5. Load pieces. The coloured block changes to the Loaded colour, and the pieces appear at the mouth marker cube.
6. Start firing. The Firing block shows, the orange glow block shows, and smoke rises from just above the smoke marker cube. The Empty and Loaded blocks are hidden.
7. Skip hours until it is done. The glow goes off, the smoke stops, the Done block shows and the fired pieces sit at the mouth marker.
8. Collect the pieces with E. The prompts and the Kiln screen work as before; aiming at the Kiln with the crosshair still highlights it from the same place.
9. You cannot walk through the Kiln and you can still walk up to it: the solid box is the same as before.
10. Save, quit to menu, load. The Kiln shows the right state block for its phase.
11. Dismantle a Kiln with the Hammer and R (if you have one to spare). It works as before.
12. If you can reach the placement ghost of a finished Kiln, it shows the test shapes in translucent green or red and the mouse wheel turns it. (The build menu may only offer the Kiln site, which is not swapped; skip this item if you only see the site.)
13. Look at the Smithy, Bloomery, Workshop and Potter's Shed: unchanged, still the code shapes.
14. Leave Play Mode running through several game hours with the Kiln firing: no stutter and no Console errors.

## C. Remove it

15. Stop Play Mode. Homestead > Art > Remove Test Kiln Prefab. The Assets/Resources/Art folder is gone.
16. Enter Play Mode and look at the Kiln in all four phases: the clay dome is back, exactly as in item 1.

## D. Regression

17. Console shows no new errors or warnings (the PlayerController.cs:217 NullReferenceException on stopping Play Mode is the old known one).
18. After item 15, Git shows nothing under Assets/Resources/Art (Claude Code checks this at commit time).
