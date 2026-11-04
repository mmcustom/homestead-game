# Metal chain M4 (iron Shovel and iron Pick Axe): playtest checklist (2026-11-04)

Code baseline 9354b31. Not committed yet. Stop Play Mode before sending anything to Claude Code. Play from your slot 4 save. Maximize the Game tab with Shift+Space.

What M4 is: the Smithy now also forges two tools for your own use: the Iron Shovel (2 Iron Bars + 2 Charcoal + 3 Sticks + 1 Cordage, 3 game hours) and the Iron Pick Axe (2 Bars + 2 Charcoal + 2 Sticks + 1 Cordage, 3 game hours). They are not sold anywhere. The forge now also stocks Sticks (cap 6) and Cordage (cap 4) as well as Iron Bars and Charcoal (cap 8 each). Iron tools need 25 percent fewer swings than the stone ones. The stone Shovel and Stone Pick Axe stay as they are. (No tool is drawn in your hand in this game, so there is no iron head to see; a finished tool lies on the anvil.)

Swings, stone tool then iron tool: stone mined 4 then 3; stump dug 6 then 5 (4.5 rounded up); Clay or Sand 4 then 3; well shaft shallow, medium, deep 20, 40, 60 then 15, 30, 45; new Outhouse pit 10 then 8.

Dev keys (editor and dev builds):
- Shift+minus stands a finished Smithy 4 m ahead stocked with 4 Iron Bars, 6 Charcoal, 6 Sticks and 2 Cordage (exactly enough for one Shovel and one Pick Axe).
- Minus gives the Smithy site kit. Ctrl+minus finishes every running Smithy job.
- Shift+equals gives 4 Iron Bars. Equals gives 10 Iron Ore and 10 Charcoal.
- Numpad 2 skips 1 game hour, Shift+Numpad 2 skips 6. PageUp gives $1,000.

Do not restore any save. Reply with the numbers that pass and describe anything that does not.

## A. The screen and stocking

1. Press Shift+minus for a stocked Smithy, press E: the screen opens. It lists five entries (Door Hinges, Hook Set, Box of Nails, Iron Shovel, Iron Pick Axe), all text easy to read, nothing clipped. The two tools say "own use".
2. The status and pack lines show four materials (Bars, Charcoal, Sticks, Cordage). The stock button reads "PUT MY IRON, CHARCOAL, STICKS AND CORDAGE IN THE FORGE".
3. Close it, gather some Sticks and Cordage (or any you have) and press R at the Smithy or use the stock button. Caps are 6 Sticks and 4 Cordage; a seventh Stick is refused with the reason.
4. With the forge short of something, pick a tool: it says what is missing.

## B. Forging the tools

5. Press 4 then Enter for the Iron Shovel. Press Enter again: "busy". Pick another entry: the same refusal.
6. A hot piece shows on the anvil, smoke rises from the chimney and a dim ember glows at the hearth. The glow does not flash or change shape and the front of the hearth is steady.
7. Skip time (Numpad 2) or sleep for 3 hours. The E prompt says "Collect Iron Shovel", Start is off ("COLLECT THE ITEM FIRST"), and the Shovel lies on the anvil.
8. Take it with E: the message says it is yours to use and cannot be sold. Start the Iron Pick Axe (key 5, Enter), wait 3 hours and take it.
9. Ctrl+minus finishes a running tool job at once.
10. The old goods still work: forge a Door Hinges or Hook Set (1 Bar + 1 Charcoal) and check it matches before.

## C. Using the tools (compare with the stone ones)

11. Equip the Iron Shovel from your tool slots. Dig a stump: 5 swings against 6 with the stone Shovel.
12. Dig Clay and Sand at the creek bank: 3 swings against 4. The Shovel prompts and the "Wet clay bank" and "Sandy bank" wording are unchanged.
13. Equip the Iron Pick Axe and mine stone at the South Ridge outcrop: 3 swings against 4 with the Stone Pick Axe.
14. If you have them: a well shaft (iron saves a quarter of the swings) and a new Outhouse pit (8 against 10).
15. Tell me how the saving feels. The saving is small (about one second per swing set); this is the part I want your opinion on.
16. The stone Shovel and Stone Pick Axe still work exactly as before (same swings, same prompts).

## D. Selling, help, dismantle, save

17. The Trading Post sell list does not show the iron tools. The stone Shovel still sells for $4.
18. Open the Know-how entry "The Smithy: Forging Iron Goods": it now covers the tools, the stock caps and the 25 percent saving. The item help for both tools reads clearly (one line each).
19. Start a tool job and try the Hammer plus R: refused. After taking the tool, leave the Smithy stocked and dismantle: half the materials come back and the stock, Sticks and Cordage return whole.
20. Start a tool job, save, quit to menu and load: the job and the stock survive and the job finishes on time.
21. Make the Game tab 21:9 and 32:9 and open the Smithy screen: five rows fit and nothing clips.

## E. Regression

22. The Charcoal Pit, Bloomery, Kiln and Hopper still work (a quick check of one). The Workshop and Potter's Shed screens still open.
23. Console shows no new errors or warnings (the PlayerController.cs:217 NullReferenceException on stopping Play Mode is the old known one).
