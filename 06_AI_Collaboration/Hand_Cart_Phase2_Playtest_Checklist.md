# Hand Cart Phase 2 Playtest Checklist (pushing the cart)

Build: Phase 2 (uncommitted at time of writing). Code baseline 6aada5a, 1095 edit-mode tests passing.
Reply with item numbers and pass or fail (and what you saw if fail).

Before you start
- Maximize the Game tab (Shift+Space) so the text is large.
- Use a throwaway new game. SavesBackup37 is the safe copy of the current Saves.
- Dev keys: [ gives the cart kit and Hammer, ] stands a loaded cart (77 kg) 3 m ahead, Shift+] stands a Wood Pile with 12 Logs (96 kg) 3 m ahead.

Grab and park
1. Press ] on open ground. A loaded cart appears ahead. Look at the bed: its load screen opens as before.
2. Walk behind the cart and look at the handles. The prompt reads "Push Hand Cart  (77 / 100 kg)" with one [E] key shown by the game (not two).
3. Press E at the handles. The cart is pulled in front of you and the status line reads "Pushing the Hand Cart  (77 / 100 kg)  -  E to park".
4. Press E again anywhere. You see "Parked the Hand Cart." and the cart stays where it was. Look at the bed: the load screen opens again.
5. Crouch, look at the handles, press E. You see "Stand up to push the cart." and nothing moves.
6. Optional: if you can arrange a tree or wall right where the cart would be pulled to, E shows "No room to pull the cart here." (write S for skipped if you cannot set it up).

Feel
7. While pushing, the cart rides about 1.7 m ahead of you and swings round on turns instead of snapping. Walk a figure of eight and say if the swing feels too slow, too fast or right.
8. While pushing, none of these work: sprint (Shift), jump, crouch, tools, R. Looking at objects shows no prompts.
9. Speed: push the loaded cart a short way, then park it, empty it into your pack or a pile, and push it empty. The empty cart is clearly faster (walking pace); the full one is slower (about three quarters).

Obstacles and terrain
10. Push into a tree. It stops. After about half a second of trying you see "The cart is stuck - back up or turn." Backing up and turning both work.
11. Push into a rock or the Cabin wall: same result. You are never stuck for good.
12. Try to push the cart through the Cabin door. It cannot go through (expected: carts stay outside).
13. Push the loaded cart up a steep slope. It stops with "Too steep to push the cart." Pushing it downhill works. (Empty, it should climb somewhat steeper ground than loaded.)
14. Push toward the creek. The cart stops at deep water with "Too deep to push the cart through." Backing out works.

Auto-release (each shows "You let go of the cart.")
15. Sleep while holding the cart (or wait for sleep). You are not holding it afterwards.
16. Press Page Down to teleport to the Maple while holding it. The cart is left where it stood.
17. Wade into the creek yourself while holding the cart. It is let go.
18. Open Build mode placement while holding it. It is let go.
19. Quit to the menu while holding it, then load. You are not holding it.

Save and flood
20. Grab the cart, push it a few metres, save, quit to menu, load. The cart stands where it was, you are not holding it, contents unchanged.
21. Push the cart into the flood band near the creek. You see "The cart is in the flood zone - this ground floods. Park it higher up." once as it goes in, and the cart and its load are untouched. Park it there, wait a bit: it is still there.

Loading
22. Park the cart next to a Shift+] pile. Take logs from the pile and put them in the cart (kg shown, 100 kg cap). Looks and works like Phase 1.
23. Phase 1 regression: the item reads "Wooden Wheels" in the Craft list, Build cost line ("2 Wooden Wheels, 3 Logs, 6 Branches, 3 Cordage") and the inventory. "Take Down Cart" still returns half the materials and the whole load.

Timing test: 12 Logs from a stand about 50 m away (the real test of whether the cart saves time)
Rules: take clock times from the game HH:MM display. Menus should not advance the clock; if they do, tell me. Do each run as one go without detours.
Setup: pick a start point S (a tree or landmark). Press Shift+] at S so there is a "home pile" there. Do not touch its logs.
24. By hand. Note the clock at S. Walk about 50 m out to a spot P. Press Shift+] there (this is the stand; pressing it costs no time). Take 3 Logs (24 kg, not Encumbered) at a time, walk back to S, put them in the home pile, and repeat until all 12 Logs from the stand are in the home pile. Note the finish clock and the number of trips.
25. With the cart. Press ] at S, then grab the cart. Note the clock. Push it empty about 50 m out to a new spot P2 in another direction. Press Shift+] there, park, load all 12 Logs, grab, push back to S, park, note the finish clock. (If the home pile refuses logs because it is full, tell me; the cart's finish is "12 Logs parked at S on the cart".)
Report: start and finish clock for both runs, number of trips for run 24, and whether the cart felt fiddly. My estimate was about 128 game minutes by hand and about 38 with the cart, roughly 3 to 3.5 times faster.

Result (2026-10-31): items 1 to 25 all passed. Mike: the timing runs (24, 25) show the cart is worth it, and he likes the hand cart. Clock numbers for the two runs were not recorded.
