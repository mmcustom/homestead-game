# Build / Craft Materials Display Playtest Checklist

Status: built, 1021 of 1021 tests pass, nothing seen in Play Mode yet, not committed.

What changed: every material line on the Build and Craft tabs now starts with a plain word. HAVE means you have enough. SHORT 2 means you have some but are 2 short. NEED 3 means you have none and need 3. The list rows show the word after the name. Staged sites (Cabin, Well, Hopper and so on) also count what is already delivered at the site, and the prompts you see in the world show how many you carry.

## Before you start

- SavesBackup34 is the backup of your paused Winter Pioneer run (29 files, 0 differences). Do NOT use the paused run for this test.
- Start a throwaway NEW game. When you finish, tell Claude Code so it can restore the Saves folder from SavesBackup34 (not 33).
- Make sure the Game tab is maximized (hover, Shift+Space) so the text is readable.
- Keys: a new Pioneer game starts with an empty pack. F11 gives the starting kit, F12 gives the full kit. To get "SHORT", drop or store part of a stack so you carry some but not enough.

## Reply format

Reply with numbers and pass or fail, for example "1 to 9 pass, 10 fail, the Well line is cut off".

## Checklist

1. Open the Craft tab and the Build tab. Every entry shows a word (HAVE, SHORT or NEED) after its name. At full Game tab size, no row name runs into the grey cost text on the right.
2. Empty pack (new Pioneer game, no keys): the Build entries you cannot afford say NEED. Click one. The heading under the name reads like "Missing: 3 of 3 materials" and every line says NEED with a number.
3. Press F12 for the full kit. Click a simple Build entry such as a Tent or Tarp Shelter. The row says HAVE, the heading says HAVE all (some number) materials, every line says HAVE, and the Build button is bright.
4. Drop part of one stack so you carry some but not enough of one material for that same entry. The row says SHORT, that line reads SHORT with the number you are missing, and the short line sits at the top, above the HAVE lines.
5. Read the Build list and a detail panel without using colour. Each state must be clear from the words alone.
6. Campfire: the panel shows "Either ... or ..." (Firewood or Sticks). The set you are closest to says "closest set", and if one set is covered it says "You can make this with this set".
7. Craft tab, Cordage and Pouch: they show Either / or with the closest set marked, and the same words on each line.
8. Cabin site with no site placed yet: five material lines, each against your pack only (for example Logs 0 / 20).
9. Place a Cabin site near you. The lines switch to "in pack N, at site N, need N". Deliver some materials with R. The numbers update. When your pack alone could finish a line, it says "(your pack covers it)".
10. Repeat step 9 on a Barrel, Food Cache, Wood Shed, Outhouse and Leach Hopper site.
11. Well with no site placed: the Stone line reads "have N, need 6, 10 or 14 by depth". With a Well site placed, it shows that site's own Stone figure instead.
12. Covered Kitchen: it shows Stage 1 (hearth) and Stage 2 (roof) with their own lines. Nothing is cut off at full size, and the Hammer note is still visible.
13. Leach Hopper entry: the note "the worst water goes in first" is there as plain text, with no have/need line.
14. In the world, look at a Cabin site and at another staged site. Each missing material shows a line like "Logs: 2 of 3 delivered, 1 in your pack" (or "none in your pack"). Check that the text on screen is not too long to read.
15. Press R at a Cabin site and at another staged site, with something to deliver and with nothing. The pop-up message uses the same line style, and the numbers are right.
16. Text size: nothing on the Craft or Build screens is smaller than before, nothing is clipped, and the action button and quantity stepper are not covered.
17. Regression: craft one thing and build one thing. Costs, quantities and your pack come out the same as before. Save and reload, and it loads normally. The Console shows no red errors.
