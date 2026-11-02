# Workshop a2-2 Playtest Checklist (2026-11-01)

Build: HEAD f8f28ca plus uncommitted a2-2 (4 modified paths, no new files). Suite 1269 of 1269, Console 0 errors, no Play Mode run by Claude Code.
Reply with item numbers and pass or fail. Write down anything in [brackets].

## Setup
Play Mode, Game tab maximized (Shift+Space). Press Shift+\ (a finished Workshop, bench stocked 5 Logs, 8 Branches, 3 Cordage). Then press \ (kit: Hammer, Axe, 10 Branches, 10 Tall Grass, 3 Cordage, and a Wood Pile with 5 Logs). Note which tool is in your hand (write it down for item 14). Walk to the bench (the stocked one).

## The new look
1. Press E. The screen opens. The pack line under the status line reads "You carry: 10 Branches, 3 Cordage" (plus any other of Logs you carry; only kinds you have are listed).
2. Read the whole screen from your normal sitting distance. Is the new text (pack line, stock button, key line at the bottom) easy to read? Say which text, if any, is too small.
3. Nothing is cut off, nothing overlaps, the stock button's words fit on its button (a wrap onto two lines is fine if both fit), and the key line fits on one line.
4. Under the list there is a big button "PUT MY MATERIALS IN THE BENCH" and a line above it.

## The stock button
5. The bench holds 8 Branches (full) and 3 Cordage (cap 4). Click the button: 1 Cordage goes in, a green message says so, the bench rows and pack line update (Cordage 2 left in the pack). [Write down the message.]
6. Click it again: the button is grey and says "The bench is already full of what you carry."
7. Close the screen. Chop a tree with the Axe and pick up the Logs (3 or more). Open the screen: the pack line shows the Logs. The bench holds 5 Logs (cap 6). Click the button: exactly 1 Log goes in and the rest stay in your pack. [Write down the pack count before and after.]
8. With the Hammer still in your pack, the button never dismantles the Workshop (the bench stays standing with its stock).
9. Start a Chair (or any piece) so a job is running, then click the stock button if there is room (use Shift+\ again on a second bench if needed). Stocking while a job runs works.
10. If your pack carries none of Logs, Branches or Cordage at some point: the button is grey and reads "Your pack has none of these materials." [Skip if you cannot easily get an empty pack.]

## The keyboard
11. Press 1, 2, 3, 4: the list choice jumps to Rough Stool, End Table, Chair, Table.
12. Press Up from the Rough Stool: it goes to the Table (wraps). Press Down from the Table: it goes to the Rough Stool.
13. Hold Down for two seconds: the choice moves once, not many times.
14. Close the screen. The tool in your hand is the same as before you opened it (the digits did not change it, and nothing else happened).
15. Choose a piece the bench can make and press Enter once: it starts once (green message, "Making a ..."), no second job and no double message.
16. Choose a piece while a job is running and press Enter: the reason line says why (the bench is busy), nothing starts, no click sound.
17. Click a piece with the mouse, then press Enter: it counts as one start, not a second click on a button.
18. Press R: the bench is stocked from the pack, the same as the button (and it does not dismantle with the Hammer in your pack).
19. Esc and the Close button still close the screen. Opening it with E held down does not start anything by itself.

## Still working in the world
20. With the screen closed, R at the bench without the Hammer stocks it as before. With the Hammer in hand and no job running, R dismantles it (half the materials back, as in a1). Do this last on a spare bench.
21. The Rough Stool, End Table, Chair and Table still start, finish (Ctrl+\\) and collect as in a2-1.
22. Console: any errors? (The known NullReferenceException at PlayerController.cs:217 when Play Mode stops does not count.)

Afterwards: note anything that looked or felt wrong even if it passes. Then I restore Saves from SavesBackup38 and we stage the 4 paths and commit.
