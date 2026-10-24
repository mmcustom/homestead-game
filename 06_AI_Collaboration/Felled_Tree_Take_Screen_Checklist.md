# Felled Tree Take Screen Playtest Checklist

**Goal:** prove E on a chopped-down tree opens a take-what-you-want screen, and that nothing else about picking up wood changed.
**Before you start:** back up your Saves folder (copy it somewhere outside the project). Play in the editor with the changes uncommitted.
**How to report:** reply with the item number and pass or fail ("1. pass, 2. fail - the title is cut off"). Add a short note on any fail.
**Handy:** the carry limit is 45 kg (55 kg with the Pouch). A log weighs 8 kg, so five logs is already too heavy. Claude Code could not physically chop a tree in its own check, so items 1 to 3 are the most important ones to try for real.

---

## A. The screen

1. Chop a tree down with the axe. Walk to the fallen pile. The prompt reads "Choose what to take" (not "Pick up everything").
2. Press E. A screen titled **Fallen Tree** opens with a list headed "In the pile" (logs, branches and so on, with counts and weights). There is no pack panel on the other side.
3. The mouse is free while it is open, and Escape or the close button shuts it. The game is not left paused or stuck afterwards.
4. The quantity stepper starts at 1 on every row. Plus and minus change it, and it cannot go above what is in the pile.

## B. Taking

5. Click Take on a light item (branches). You get the stepper amount, the pile count drops, and the stepper goes back to 1.
6. Set the stepper to 2 on logs and click Take. You get 2 logs.
7. Shift-click Take on a stack. You take the whole stack (if it fits).
8. Take part of the pile, close the screen, press E again. The pile still holds exactly what you left.
9. Save, go to the Main Menu, press Continue. The part-taken pile is still there with the same contents.

## C. Too heavy (warn and refuse)

10. Carry some wood until you have less than about 20 kg of room. Try to take a stack of logs that will not fit. A red or status message reads like "Too heavy: ... Nothing was taken.", and the pile and your pack are unchanged.
11. Fill the pack to the limit and click Take on anything. The message says "No room: you are already carrying the most you can (45 kg). Nothing was taken."
12. After a refusal the stepper keeps the amount you chose, so you can lower it and try again without retyping.
13. **Take All with plenty of room:** it takes everything, the screen shows an empty list, and the pile disappears from the world when you close it.
14. **Take All when the pile is too heavy for you:** it refuses everything and takes nothing (this is by design, see the question below).

## D. Other things must not change

15. The Small Cabin building site still takes materials with an instant E as before (no new screen there).
16. Leftovers dropped from a full pack (a drop pile) still behave sensibly: E on them opens the same take screen and you can take what you want.
17. Storage chests and the barrel still open their normal two-sided screen with both panels.
18. Look at the screen at 1920x1080 and at 1024x768. Nothing is cut off or overlapping.
19. The Console is clean after the session (the "WebSocket is not initialised" MCP message is known and fine; no red errors).

---

## One question for you (answer after item 14)

Take All refuses everything when the whole pile is too heavy, because you chose "warn and refuse". With heavy logs (8 kg each, 45 kg limit) a 27.5 kg pile will not fit in 21.6 kg of room, so Take All does nothing and you have to take it piece by piece. Which do you prefer?

- **Keep it as is:** Take All is all-or-nothing, consistent with your rule.
- **Take All takes what fits:** only the Take All button, lightest first, and tells you what was left. Individual Take buttons still refuse.

**When done:** send me your pass or fail list and your answer. If everything passes I will check Claude Code's staging list (six paths expected), write the commit message, and nothing is committed until you say OK.
