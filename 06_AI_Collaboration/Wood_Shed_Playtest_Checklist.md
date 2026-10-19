# Structures Slice 1, Phase B: Wood Shed + Wet/Dry Wood - Play Mode Checklist

Built 2026-10-19. Compile-checked; 113 edit-mode tests pass (22 new in `WoodShedTests.cs`). PLAYTESTED 2026-10-19: all 73 items pass (Mike). UNCOMMITTED. Nothing in this build advances the game clock (only Sleep, rest, collapse and the debug time-skip keys do). All numbers are public fields on `WellsAndOuthouses` (first-pass tuning, Claude, not confirmed).

**Setup:** back up `slot1` first (restore and hash-verify after). Carry: Hammer, Axe, Flint and Steel, 4 Logs, 16 Branches, 10 Sticks, 10 Firewood (debug item keys / felled trees). Build tab: "Wood Shed Site" (Storage category). A Small Cabin (finished) is useful for the 3 m refusal and the regression items; a Campfire for the burn tests. Known weather keys: Numpad 1 forces the next weather for 12 h (Clear, Light Rain, Heavy Rain, Thunderstorm); Numpad 2 skips 1 game hour (Shift: 6) and raises the hour-end that rain wets open piles on.
**Debug keys (new):** Numpad + marks the nearest Wood Shed's wood (within 8 m) WET, or the wood you carry if no shed is in reach (Shift marks it DRY; Ctrl always means the carried wood). Numpad - advances all wood by one season (28 days): every pile, every shed and the wood you carry; wet wood becomes dry. Both flash the result ("Logs 2 wet, Firewood 5 dry").
**Weights (kg):** Logs 8, Branches 1, Sticks 0.1, Firewood 1.5, Wood Shed cost = 2 Logs + 8 Branches = 24 kg (over the 15 kg staged-site cutoff, so a staged site; one trip of 24 kg is under the 30 kg Encumbered line, but the Logs go in a first trip and the Branches in a second to test two trips).
**Numbers:** Hammer blows 25 (`woodShedBuildBlows`, about 23 s). Seasoning 1 season = 28 days = about 14 real hours (`seasoningSeasons` 1). Wet burn 70 percent (`wetBurnFraction`): Firewood 2 h becomes 1.4 h, Branch 1 h to 0.7 h, Stick 0.15 h to 0.105 h, Log 10 h to 7 h. Shed distance from a cabin 3 m. Shed capacity: no limit (same as a Wood Pile, which has none).

## A. Placement ghost and refusals
1. [ ] Build tab lists "Wood Shed Site", "free to mark out; the Wood Shed then takes 2 Logs, 8 Branches over several trips". The help text mentions the open front and the wind.
2. [ ] Placement Mode shows the FINISHED shed as the ghost (posts, slatted back and left side, sloping roof highest at the open front), green where valid, red where not. It does NOT show the plot stakes yet.
3. [ ] The ghost turns with the mouse wheel; the prompt line says "Place Wood Shed Site here - opens toward the south-east, away from the north-west wind" (or whichever way the wind is blowing from; the word after "away from" is the wind's source).
4. [ ] When the ghost starts, its open front already faces away from the wind. Turn it into the wind: the line changes to "... into the north-west wind (turn it with the wheel)". Placing is still allowed either way.
5. [ ] Within 3 m of the finished Small Cabin or its site: red "Too close to the Small Cabin: 3 m needed." (a spot 3.0 to 3.2 m away may instead say "Too close to another pile").
6. [ ] On water (creek, pond, spring zone): red "Can't build that in water." Too steep: red "The ground there is too steep."
7. [ ] Low ground beside the creek (the floodplain): red "This ground floods."
8. [ ] Within 2 m of another pile: red "Too close to another pile."; near a lit fire: "Too close to the fire."
9. [ ] A valid spot: one click places the site with NO Hammer in hand, spending nothing. The pack is unchanged.

## B. Staged site delivery (two trips)
10. [ ] The placed site shows the plot look (four stakes round bare earth), not the finished shed. Prompt: "Wood Shed Site / Needs 0 of 2 Logs, 0 of 8 Branches (R stores what you carry)".
11. [ ] Trip one: carry only 2 Logs, R stores them. Prompt "2 of 2 Logs, 0 of 8 Branches"; the Logs show beside the plot. Sticks, Firewood, Stone and Tall Grass are refused.
12. [ ] Trip two: R with 8 Branches stores them. A banner says "Wood Shed Site fully stocked - equip the Hammer and hold click on it to build it". The shed is NOT built yet.
13. [ ] Carrying more than needed (3 Logs): R takes only what the site still needs.
14. [ ] E on the stocked site takes the materials back (as far as the pack allows), no Hammer needed. Restock it.
15. [ ] Save and load with the site half stocked (only the Logs in): the Logs are still there and the site still wants the Branches.

## C. Pounding with the Hammer, and taking an empty site down
16. [ ] Without the Hammer the prompt reads "Fully stocked - equip the Hammer to build (0%)". Clicking does nothing.
17. [ ] Hammer equipped, hold click on the site: about 25 blows (one every 0.9 s, so roughly 23 s), the progress bar fills, the chop sound plays, stamina drops 4 per blow. Prompt "Hold click with the Hammer to build (n%)".
18. [ ] After the first blow the prompt says the materials are built in; E refuses to take them back ("The materials are built in now - keep hammering to finish it").
19. [ ] Releasing and coming back keeps the percentage (saved on the site).
20. [ ] The last blow finishes it in place: "Wood Shed built.", the plot becomes the finished shed (same spot, same facing), the delivered wood is used up and the shed starts empty.
21. [ ] The game clock does not jump while hammering (watch the clock; it runs at its normal rate).
22. [ ] Taking an EMPTY, unstocked site down: without the Hammer E says "Equip the Hammer to take the site down."; with the Hammer E removes it and says "Took down the wood shed site". Nothing is refunded because nothing was spent.
23. [ ] A site with materials in it cannot be removed (E takes the materials first, then the empty site comes down).

## D. The finished shed and its transfer screen
24. [ ] The finished shed's open front faces the way the ghost showed (toward the downwind side), the roof is highest at the front.
25. [ ] Looking at it: "Open Wood Shed (nothing...)" style prompt; E and R both open the same transfer screen as the Wood Pile (pack on the left, shed on the right).
26. [ ] It stores Sticks, Branches, Logs and Firewood. Stone, food, tools and water are refused ("A Wood Shed only holds wood.").
27. [ ] The right side reads "N stored - no limit". Store 30+ pieces: nothing stops you (same as the Wood Pile, which has no cap).
28. [ ] Fresh felled wood stored: the rows read "x3 · wet". Numpad + (Shift) on the shed: "Wood Shed wood marked dry"; the rows now read "· dry". Mixed: carry some wet and some dry Logs, store them: "· 2 wet, 3 dry".
29. [ ] The pack side of the screen shows the same wet/dry label for carried wood (wood felled just now is wet; wood marked dry, or stored in a pile before this build, is dry; wood already in the pack in an older save carries the day it was picked up, so it reads wet until a season has passed).
30. [ ] Take (click) and Shift-take work; Take takes the driest batch first. Store All Eligible and Take Everything work.
31. [ ] The shed fills visibly: Firewood stacks in the left bay, Logs and Branches in the right, Sticks at the front.
32. [ ] With the Axe in hand, hold click on a Log in the shed: it splits into Firewood where it lies (shed works like a pile for this).

## E. Seasoning, and rain on an open pile
33. [ ] Fell a tree (or take freshly cut wood) and carry it: the transfer screen shows it as wet. Store it in the shed. Numpad - once: "Seasoned all wood forward 28 days"; open the shed: the rows read "· dry".
34. [ ] Without the key: the same wet wood in the shed is still wet after a few game days (skip a day with F-keys or Numpad 2 x24) and dry after 28 days of game time (about 14 real hours; use Numpad - instead).
35. [ ] A wet batch and a dry batch in the same shed stay as two batches, not merged: the row says "2 wet, 3 dry" and the Take button takes the dry ones first.
36. [ ] Wood in an open Wood Pile (the kind you build with 4 Sticks) also seasons: Numpad - turns wet wood in it dry.
37. [ ] Rain re-wets an open pile: put wood in an open Wood Pile and season it dry with Numpad - (check the rows read dry), then Numpad 1 to Light Rain and Numpad 2 to skip an hour: the pile's wood reads wet again.
38. [ ] The same rain does NOT wet the wood in the Wood Shed: shed rows stay dry through Heavy Rain and Thunderstorm hours.
39. [ ] A pile that keeps getting rained on never finishes: wet it, Numpad 1 and skip hours; each rainy hour restamps it. It dries only after 28 dry days (or Numpad -).
40. [ ] Snow wets an open pile too (Winter).
41. [ ] Wood carried in the pack does not get wet or dry by itself (its label stays as it was); only Numpad + / - change it.

## F. Wet and dry burn time (campfire)
42. [ ] Carry 4 dry Logs and 4 wet Logs (Numpad + with no shed in reach marks all carried wood wet; mark some dry: store the dry ones in a shed and take them again). Look at a Campfire: the add prompt reads "Add dry Log (+10 h)" or "Add wet Log (+7 h)"; cycling fuel (the fuel key) flashes "Fuel: dry Log (+10 h each)".
43. [ ] A dry Log is added first (driest first). The fire's fuel rises by 10 h; the next wet Log adds 7 h. (Cap is 12 h: one dry Log fits on an empty fire; a second does not.)
44. [ ] Burn it down in real time: 1 wet Firewood lasts 1.4 game hours, 1 dry Firewood 2 hours (watch the fuel number or the flame).
45. [ ] Shift adds as many as fit under the 12 h cap, counting each batch's own time: e.g. an empty fire with 2 dry + 6 wet Firewood takes 2 dry (4 h) + 5 wet (7 h) = 11 h.
46. [ ] Build a Campfire paid in Firewood: 3 dry Firewood give 6 h of fuel to start, 3 wet give 4.2 h. Built from 6 Sticks: dry 0.9 h, wet 0.63 h.
47. [ ] Branches, Sticks and Logs each burn dry or wet by the same 70 percent.
48. [ ] The Small Cabin hearth takes wet and dry the same way (the hearth is a campfire).
49. [ ] A lit fire is NOT put out or slowed by rain (it never was). Light Rain/Heavy Rain only changes the chance of catching when lighting. Phase C should know this.

## G. Dismantle
50. [ ] An empty shed with the Hammer equipped: transfer screen "Take Down Structure" works (click twice). Refund: half the cost rounded down = 1 Log + 4 Branches back (dry), the shed disappears.
51. [ ] Without the Hammer the button reads "Take Down - equip the Hammer". With wood in the shed it reads "empty it first".
52. [ ] A full pack: the surplus is left as pickup piles where the shed stood (nothing lost). Pick them up: the wood is dry.

## H. The five places wood is carried keep its wet or dry day
53. [ ] Pack to pile: store a wet Log and a dry Log in a Wood Pile and in the shed; the labels survive.
54. [ ] Pile to pack: take them back; the labels survive (the pack side shows "2 wet" etc.).
55. [ ] Drop: drop a wet Log from the Inventory screen (two piles if mixed), pick it up again: still wet; a dry one is still dry.
56. [ ] Structure refund: take down a Tool Rack or Storage Bin (Sticks, Branches): the refund comes back dry. With a full pack the leftovers on the ground are dry when picked up.
57. [ ] Staged site: put wet Branches into the Wood Shed Site, then E takes them back: still wet (the site keeps the day). Dry Branches stay dry.
58. [ ] Collapse (stamina 0) drops a random carried item as a pile: if it is wood, it keeps its day.

## I. Splitting keeps the day
59. [ ] Split a wet Log (Inventory Split button with the Axe): the 5 Firewood are wet. Split a dry Log: dry Firewood.
60. [ ] Split a Log lying in a pile with the Axe: the Firewood has the Log's day.
61. [ ] Branches: 2 Branches (one wet, one dry) make 1 wet Firewood (a mix is as wet as its wettest part).

## J. Regression: every fire and the cabin
62. [ ] Every kind of fire (Campfire, Small Cabin hearth) still burns Sticks, Branches, Logs and Firewood: add each, the fuel hours go up, Light with Flint and Steel works, the fire burns down and goes out as before.
63. [ ] Old wood with no day burns at the full time: load a save from before this build (or old Wood Pile contents): the wood reads dry and burns for its normal time. No errors in the Console.
64. [ ] Build payment still works: a Campfire from 3 Firewood or 6 Sticks; a Wood Pile (4 Sticks), Tool Rack (4 Branches + 4 Sticks), Storage Bin (6 Branches + 6 Sticks), Lean-To (8 Branches, 4 Sticks, Cordage) all take wet or dry wood.
65. [ ] The Small Cabin site still takes Logs and Branches (wet or dry) and the cabin builds; Dismantle the cabin: half its Logs/Branches back (dry), the hearth Firewood back.
66. [ ] Crafting recipes that use Sticks, Branches or Logs still work with wet or dry stacks.
67. [ ] Inventory screen: a pack holding wet and dry Logs shows ONE Logs row with the total, and the weight is right.
68. [ ] A Rock Pile, Food Cache, Water Barrel, Storage Bin and Tool Rack behave exactly as before (no wet/dry labels on them).
69. [ ] The other staged sites (Well, Outhouse, barrels, Food Cache) still deliver, pound and finish as in the phase A checklist.

## K. Saves
70. [ ] Save with a shed holding wet and dry wood, an open pile with wet wood and a half-stocked shed site. Load: all three come back as they were, labels intact, the shed's facing intact.
71. [ ] Load a save made before this build (Wood Piles with Logs, no Wood Shed): loads unchanged, no errors, no Wood Shed anywhere.
72. [ ] Save, quit to the main menu, continue: a finished shed's wood is still seasoning (the date, not a timer, decides): after Numpad - it reads dry.
73. [ ] The Console is free of errors and warnings through all of the above.
