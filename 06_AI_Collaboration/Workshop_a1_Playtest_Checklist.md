# Workshop a1 Playtest Checklist (Workshop and the Rough Stool)

Build: Workshop W1(a1) (uncommitted), 1234 edit-mode tests passing. Code baseline 79eedb3. Design: Workshop_and_Handmade_Goods.md (addendum 2026-10-31).
Reply with item numbers and pass or fail (and what you saw if fail). For items 10, 12 and 20 please send the numbers.

Before you start
- Maximize the Game tab (Shift+Space). Use a throwaway new game. SavesBackup38 is the safe copy of Saves (it also holds your four real slots).
- Dev keys: backslash gives the Workshop site's materials (a Hammer, an Axe, 10 Branches, 10 Tall Grass, 3 Cordage) and stands a Wood Pile with 5 Logs 3 m ahead. Shift+backslash stands a finished Workshop 4 m ahead with the bench stocked for two Rough Stools. Ctrl+backslash finishes every running job now. PageUp gives $1,000. ] stands a loaded Hand Cart. F7 changes the speed, Shift+F5 skips 10 days.

Building the Workshop
1. Press backslash. You see the kit message, a Wood Pile with 5 Logs is ahead, and your pack holds a Hammer, an Axe, 10 Branches, 10 Tall Grass and 3 Cordage.
2. Build tab: "Workshop Site" is listed (category Other) with its help text. The HAVE / SHORT / NEED lines show the 5 Logs, 10 Branches, 10 Tall Grass and 3 Cordage.
3. In the placement ghost the shed footprint shows and turns with the mouse wheel. Placement is refused with a message: on flood ground, within 3 m of a Cabin or Cabin Site, within 3 m of another pile, and with a standing tree trunk in the footprint ("Clear the trees off the building site first."). Tell me which of these you could test.
4. Cart clearance: press ] to stand a Hand Cart. A Workshop site is refused within about 2.6 m of it, and a cart cannot be placed onto the site's footprint either.
5. Mark the site out, take the Logs from the pile and deliver with R over several trips (5 Logs is 40 kg, over your 30 kg Encumbered line). The prompt shows progress such as "Logs: 2 of 5 delivered, 3 in your pack", and E takes materials back.
6. Equip the Hammer and hold click: about 40 blows. The Workshop appears as a roofed shed with a thatch roof, posts and a bench, and you see "Workshop built" style message.
7. Walk round it: the posts and back wall are solid, you can aim at the bench from the open front, and the roof keeps rain off you if it is raining.

Making a stool
8. Carry 4 Logs and 4 Branches (take them from a pile) and press R at the bench: it stores them ("Stored ... the bench holds ...") up to 6 Logs, 8 Branches and 4 Cordage. With more than that, R says nothing you carry fits.
9. With no felling tool in your pack (sell the Axe at the Trading Post for the test), E says it needs an Axe, Primitive Axe or Wood Saw. Buy the Axe back or keep it for the rest.
10. With an Axe in your pack and 2 Logs and 2 Branches in the bench, E starts a Rough Stool. The prompt shows the hours to go (3 h job). Walk away and sleep or use F7: the job finishes. Send roughly how many in-game hours it took (it can be 2.5 to 3 h because of the hourly tick).
11. When it is done the prompt reads "Collect Rough Stool". E puts it in your pack (10 kg). With a nearly full pack E says "Too heavy - no room to carry it" and leaves it in the bench.
12. Sell the stool in the Trading Post (T). Send the price you got for the first one and for a second one sold right after (expected about $14, then about 10 percent lower). The Finances statement shows it under Crafted goods.

Rules while working and dismantling
13. While a stool is being made, E shows the hours left and R with the Hammer refuses to take the Workshop down.
14. Ctrl+backslash finishes the running job at once and the piece is ready.
15. When idle, with stock and a finished piece in the bench, equip the Hammer and press R: half the materials come back plus the stock and the piece whole, dropped around you.
16. Shift+backslash: a finished Workshop appears 4 m ahead, bench stocked for two Rough Stools, and you can make two stools back to back.

Saves, sprouts, nothing broken
17. Save and reload in the middle of a job: the job keeps going with the same hours left and the stock is the same.
18. Load your real slot1, slot2 and slot4 with this build: they load with no errors and play normally (Saves are restored from SavesBackup38 afterwards).
19. Fell a hardwood and leave its stump. Try to place a Workshop site over the stump. Tell me what happens (it is allowed by the rules as built; I want to see whether the stump pokes through the shed).
20. Fell a tree about 2 m from where you will build, build the Workshop there, then skip 112 days (Shift+F5 eleven times plus F5). Send whether a sprout ever comes up inside the shed footprint (it should not, within 2.9 m).
21. Hand Cart, Wood Saw and tree regrowth still work as before; the Console has no errors and nothing stutters.

Result (2026-11-01): items 1 to 21 all passed. The numbers asked for in items 10, 12 and 20 and the stump observation in item 19 were not recorded.
