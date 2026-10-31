# Hand Cart, Phase 1: Playtest Checklist

Status: built, 1072 of 1072 tests pass, Console clean. Nothing has been seen in Play Mode yet. Not committed.

What Phase 1 is: you can craft Wooden Wheels, mark out a Hand Cart Site, deliver the materials, build the cart with the Hammer, and use it as a parked 100 kg stash (Logs, Branches, Firewood, Stone, Clay, Tall Grass). You cannot push it yet. That is Phase 2.

## Before you start

- SavesBackup36 is the backup of your Saves folder (29 files, 0 differences). Use a throwaway NEW game. When you finish, tell Claude Code so it can restore the Saves folder from SavesBackup36.
- Maximize the Game tab (hover, Shift+Space) so the text is readable.
- Dev keys (editor only): [ gives 2 Wooden Wheels, 3 Logs, 6 Branches, 3 Cordage and a Hammer. ] stands a finished Hand Cart about 3 m ahead with 4 Logs, 6 Stone and 10 Firewood in it (77 kg). Numpad keys do nothing.
- For items 1 and 2 you need a Log and Cordage for each wheel. Fell a tree, or use F11 or F12.

## Reply format

Reply with numbers and pass or fail, for example "1 to 12 pass, 13 fail, the Clay line is cut off".

## Checklist

1. Craft tab, Materials: there is a Wooden Wheel row. It costs 1 Log and 1 Cordage, needs no tool, and crafts instantly. The row shows HAVE, SHORT or NEED. The wheel weighs 3 kg.
2. Craft two Wooden Wheels. Both are in your pack.
3. Build tab: a Hand Cart Site row is there, with HAVE, SHORT or NEED. Read its cost line and help text. Does anything look wrong or confusing? (The cost line says "2 Wooden Wheel", singular. That is a known wording issue.)
4. Place the site. The ghost is green on dry ground and red with "This ground floods." in the flood band. The mouse wheel turns it. Near a cabin, the 3.2 m clearance still applies.
5. Look at the site. The prompt lists what it needs, and one line per material.
6. Deliver the materials with R over several trips (or use the [ key). A third wheel or a fourth Log is refused. E takes materials back before you strike the first blow. The wheels you deliver show beside the site.
7. Equip the Hammer and hold click on the site, about 12 blows. The materials lock in after the first blow. A built message appears and the site becomes the cart.
8. Look at the finished cart. It should be a two-wheeled cart with handles at the back. Tell me what you see.
9. Open the cart (E or R). The screen shows "x / 100 kg" and the status line says what it holds.
10. Fill it with Logs. It stops at 12 Logs (96 kg). With 12 Logs in, 4 Branches still fit, but a 13th Log is refused with a reason showing the room left in kg.
11. Empty cart limits: Stone stops at 20, Clay at 28 and Firewood at 66. (Use the ] key's cart or your own.)
12. Carry some food, water and a tool. They are not listed on the store side. With only those carried, a line says the cart holds only building goods, and not food, water or tools.
13. A heavy cart load does not make you Encumbered. Your pack weight is separate.
14. Press E on the cart's contents to take things out again. It works both ways.
15. Spacing: with a parked cart, you can place another pile about 1.5 m away, but not on top of it (inside about 1.2 m). Near a cabin, the 3.2 m clearance still applies.
16. Take down without the Hammer: the button is greyed, with the reason "equip the Hammer".
17. Take down with the Hammer and a loaded cart: it asks twice ("Click again to take it down"). Half the materials come back, plus the whole load. What the pack can't carry is left on the ground as pickup piles. Nothing is lost.
18. Save and reload with a loaded cart. Its position, facing and every item match, with nothing lost or duplicated.
19. Load a save from before this change (or any older save). It loads normally.
20. Regression, with a parked cart nearby: deliver to a Small Cabin site, a Well site, a Leach Hopper site and a Kitchen site. A Kitchen site keeps 3 m from every structure, and the cart is not exempt from that. If a cart within 3 m of a planned Kitchen is refused, tell me what the message says.
21. The [ and ] keys work. Nothing happens on the Numpad.
22. Text is readable at full Game tab size, and the Console shows no red errors.
