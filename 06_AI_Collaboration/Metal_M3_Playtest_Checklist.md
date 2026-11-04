# Metal chain M3 (the Smithy and the first goods): playtest checklist (2026-11-04)

Code baseline cc9a8a9. Not committed yet. Play from your slot 4 save. Maximize the Game tab with Shift+Space.

What M3 is: the Smithy is a roofed open-front shed with a stone forge and an anvil. You stock the forge with Iron Bars and Charcoal (R, or the stock button on the screen, up to 8 of each), press E to open the Smithy screen, choose a good and start it, walk away, and take the finished good when it is done. One job at a time, one good per job:
- Door Hinges: 1 Bar + 1 Charcoal, 3 game hours, sells $26
- Hook Set: 1 Bar + 1 Charcoal, 2 game hours, sells $18
- Box of Nails (20): 1 Bar + 1 Charcoal, 2 game hours, sells $16
The goods are Crafted group (10% price drop per unit sold, recovering 25% a day). They are not sold in the store. The Iron Skillet and the iron tools are not in this phase.

Dev keys (editor and dev builds; checked unused):
- Minus gives a Hammer, 3 Clay, 4 Branches and 6 Tall Grass, and stands a Rock Pile with 6 Stone about 3 m ahead on the left and a Wood Pile with 2 Logs about 3 m ahead on the right (the full kit is 61 kg against the 45 kg pack).
- Shift+minus stands a finished Smithy about 4 m ahead stocked with 4 Iron Bars and 6 Charcoal.
- Ctrl+minus finishes every running Smithy job.
- Shift+equals gives 4 Iron Bars. Equals gives 10 Iron Ore and 10 Charcoal.
- PageUp gives $1,000. Numpad 2 skips 1 game hour, Shift+Numpad 2 skips 6.

Ignore the report's last step about restoring a slot: do not restore anything. Reply with the numbers that pass and describe anything that does not.

## A. Building the Smithy

1. Press minus. Check the Hammer, Clay, Branches and Tall Grass are in your pack, and the two piles stand ahead. Take the Stone and then the Logs with E.
2. Open the Build tab, go to Other, and place a Smithy Site. The Build-tab help reads clearly. The mouse wheel turns it.
3. Try placing it about 2 m from a tree: refused ("Too close to a tree: 3 m needed for the forge fire."). Try it next to the Workshop (if you have one) or another structure: refused for spacing. A clear spot works.
4. Deliver the materials in any order with R (6 Stone, 3 Clay, 2 Logs, 4 Branches, 6 Tall Grass). Partial deliveries work and the site shows what is still needed. E takes materials back.
5. Equip the Hammer and hold click for 40 blows: "Smithy built" shows and "The Smithy: Forging Iron Goods" unlocks in the Journal.
6. The Smithy looks like a darker roofed shed with a stone hearth and chimney and an anvil on a stump. Tell me what you think of the look (size, colour, soot).

## B. The screen and stocking

7. Put the Hammer away and press E on the Smithy: the screen opens. Text is easy to read (nothing small), the three goods are listed with hours and what the Trading Post pays now, and Esc and the Close button both close it.
8. With an empty forge, press Enter: it says what is missing (Iron Bars and Charcoal).
9. Press Shift+equals for 4 Iron Bars and equals for Charcoal. Use the stock button "PUT MY IRON BARS AND CHARCOAL IN THE FORGE" (or R at the Smithy). The pack line and the forge line update. Caps are 8 Bars and 8 Charcoal; a ninth is refused with the reason.
10. Keys 1 to 3 pick a good, Up and Down move, Enter or the Start button starts it.

## C. Forging

11. Start Door Hinges: it takes 3 hours. Try to start a second job: it says the forge is busy. A hot piece shows on the anvil, smoke rises from the chimney and a dim ember glows at the hearth. The glow does not flash or change shape and the front of the hearth is steady from every side.
12. Skip time with Numpad 2 and also sleep once: the job finishes, the smoke stops, and the E prompt says the good is ready.
13. Take the good (E or the screen). Repeat for Hook Set (2 hours) and Box of Nails (2 hours). Each takes 1 Bar and 1 Charcoal from the forge stock.
14. Start a job, then press Ctrl+minus: it finishes at once.
15. Rain does not stop a job.

## D. Selling

16. Sell Hinges, Hooks and Nails at the Trading Post: prices $26, $18, $16 at first. Sell two of the same good: the second sells at about 90% of the first. The Ledger shows the sales under Crafted.
17. The goods are not sold in the store, and the Iron Bar is still not sellable.

## E. Dismantle, save, window sizes

18. With a job running, try to dismantle (Hammer plus R): refused with a message. After taking the good, dismantle: half the materials come back and the stock and finished goods return whole.
19. Shift+minus for a ready Smithy: it works the same as a built one.
20. Start a job, save, quit to menu and load: the job and the stock are still there and the job finishes on time. A finished good waiting in the Smithy also survives save and load.
21. Make the Game tab 21:9 and 32:9 (the aspect dropdown above the Game view), open the Smithy screen: the whole panel is visible and nothing clips.

## F. Know-how and regression

22. Open the Journal: "The Smithy: Forging Iron Goods" reads correctly, the Bloomery entry points to it, and the item help for Hinges, Hooks and Nails reads clearly.
23. The Charcoal Pit and the Bloomery still work (a quick check of one). The Workshop and the Potter's Shed screens still open.
24. Console shows no new errors or warnings (the PlayerController.cs:217 NullReferenceException on stopping Play Mode is the old known one).
