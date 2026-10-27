# Maple D (Maple Finding Aids) Playtest Checklist

Date written: 2026-10-27. Build under test: droplet markers over untapped Maples while a Spile is equipped, pail markers over tapped trees (always on, state colours), and tap pins on the minimap (803 edit-mode tests pass, not yet Play Mode tested).

Reply with the item number and pass or fail (for example "10 fail, the amber does not pulse").

Before you start: the Saves folder is backed up (SavesBackup28, 22 files, diff -r 0 differences). Use a throwaway new game. Tell Claude Code when you finish so it can restore the Saves folder.

Setup: a game with a Spile and Sap Bucket (F11 or F12 gives the kit; craft more if you want several taps). Dev keys (editor and development builds only): Page Down teleports to the nearest Maple (Shift+Page Down goes back), Home forces sap flow, Shift+Home fills every bucket, F5 skips a day, F8 jumps to Winter.

Reference numbers: droplets show out to 80 m and fade from 55 m; pails show out to 100 m and fade from 75 m; both hide under 4 m; at most 30 droplets and 12 pails on screen.

## Droplets over untapped Maples (Spile equipped)

1. Equip the Spile and stand within about 80 m of several Maples. A yellow-green droplet floats over each untapped Maple, full strength inside 55 m and fading out between 55 m and 80 m.
2. Switch to another tool or empty hands: every droplet disappears. Equip the Spile again: they return.
3. Walk to within 4 m of a Maple: its droplet hides and the normal look prompt for the tree takes over. They never overlap.
4. Turn so Maples are behind you: no droplets are drawn for them. A droplet near the crosshair or the screen edge hides instead of sitting on the crosshair or clipping at the edge.
5. Place a tap on a Maple and step back: that tree no longer has a droplet (it now has a pail, see item 7). Other untapped Maples keep theirs.
6. Optional: place 12 taps (the soft cap). With 12 taps down, no droplets show at all.

## Pails over tapped trees (no Spile needed)

7. With a tap placed, put the Spile away and walk about 30 m off. A pail marker shows over the tapped tree even with empty hands.
8. Walk away: the pail is full strength to about 75 m, fades out by 100 m, and is gone beyond that. Walk back to within 4 m: it hides.
9. A new or part-filled bucket shows a cream-grey pail.
10. Press Shift+Home to fill the bucket (or wait out a flow). At 4 L or more the pail turns bright amber and pulses gently.
11. Let a full bucket sit 48 game hours (press F5 twice with Home off) until it sours. The pail turns muted brown-red. A sour bucket is never amber and never pulses amber, even if it holds 4 L or more.
12. Collect with E: the pail goes back to cream-grey (or disappears if the tap is taken down with R).
13. Raise the rifle: the pails stay visible.
14. No tree ever shows both a droplet and a pail.

## Minimap pins

15. Open the minimap (it is on screen by default). Each tap has a pin in the matching colour: cream-grey, amber (slightly larger when ready) and brown-red when sour.
16. Walk more than 80 m from a tap: its pin clamps to the rim of the minimap, a little smaller and fainter. Walk back and it moves to its true position.
17. Tap pins draw above the existing discovery markers and do not hide them.
18. Take a tap down with R, or fell a tapped tree: the pail and the minimap pin both disappear.

## Hiding and layout

19. Open Inventory or any menu, pause, and sleep: all droplets, pails and pins hide while the menu is up and come back after.
20. No clash with the tool line, the look prompt, the clock and weather, or the minimap. Nothing is cut off or overlapping.
21. No hitches or frame drops while walking through a Maple-heavy area (Page Down takes you to one).
22. Known limit, report only if it looks bad: markers can show through hills and other trees because there is no occlusion check.
23. The Console shows no new warning (in particular no "tree height" fallback warning).
24. Save and quit to the menu with taps down, then load: the pails and the pins reappear in the right colours for the loaded bucket levels.
