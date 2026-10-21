# Quantity Controls Part 1 Playtest Checklist

Build: market commit 0f8e336 plus uncommitted Part 1 quantity controls.
Covers: Trading Post (buy and sell), Storage (store and take), typing behaviour, hotkeys, layout.
Report results by item number ("1. pass", "7. fail: ...").

## Before you start

- Back up the Saves folder (copy it somewhere outside the project).
- Start a new game, or load a save with items in the pack. Press F12 once for the debug kit (it also gives $50). Press it only once; it gives the whole kit every press.
- Keep the Unity Console open and clear it before starting.

## A. Trading Post: selling

1. Press T near the Trading Post. The screen opens. Each row is two lines: name, trend and price on line one; stepper, live total and button on line two.
2. On a sell row with more than 1 in the pack, click plus three times. The amount reads 4 and the live total changes to match.
3. Click Sell. Exactly 4 are sold. Cash rises by the total that was shown. The row's amount returns to 1.
4. Click minus until the amount is 1. Minus does not go below 1.
5. Click Max on a sell row. The amount equals what you hold. Click Sell. The row sells the lot.
6. Hold Shift and click plus. The amount jumps by 10 (or clamps at your count). Hold Shift and click minus. It drops by 10, not below 1.
7. Shift-click the Sell button on a row with a stack of 2 or more. The whole stack sells, no matter what the stepper says.
8. After selling several of one item, the price trend and price drop as before (the market still works). The live total for the next sale reflects the lower price.

## B. Trading Post: buying

9. Click plus on a buy row until the amount is 5 and note the live total. Click Buy. You get 5, cash falls by exactly the total shown, amount returns to 1.
10. Shift-click Buy on a cheap item such as arrows. You buy 10 and pay for 10.
11. Click Max on a buy row. The amount is the most you can afford and carry. It never exceeds your cash or your weight limit.
12. Try to buy something you cannot afford. The row is greyed or the button is disabled, and nothing is bought. No error appears in the Console.
13. Load the pack until nearly full. A heavy buy row greys out with "too heavy to carry more". A lighter item that still fits stays usable, and Max respects the weight left.

## C. Typing amounts

14. Click the amount box on a row. The box clears and shows a greyed placeholder.
15. Type 3 and click Buy or Sell. Exactly 3 move.
16. Type 999999. The amount clamps to the largest allowed amount (your max), not 999999.
17. Type letters ("abc"). Nothing appears; the box stays empty or keeps the old value.
18. Type 0. The amount becomes 1.
19. Click the amount box, type 2, then press Esc. Typing ends and the screen stays open (Esc must not close the screen or open the pause menu on the same press). A second Esc then closes the screen as it always did.
20. Click the amount box, type 2, then click away on empty space. Typing ends and the value stays.
21. While the box is in typing mode, press T, I and M. The letters should not trigger the Trading Post, Inventory or Map. After you click away or press Esc, T, I and M work again.
22. Close the Trading Post with a typed amount sitting in a row, then reopen it. Every row is back to 1.
23. Make a purchase or sale while a different row has a typed amount. Note what happens to that other row (a refresh while typing may lose the typed text; that is a known gap, so just report what you see).

## D. Storage (woodpile, barrel, other stores)

24. Open a woodpile. Rows are two lines with steppers, like the Trading Post. The heading hint reads "Shift-click: the lot".
25. Set the stepper to 4 on a Store row and click Store. Exactly 4 move into storage. The "n of max" total updates.
26. Set the stepper to 3 on a Take row and click Take. Exactly 3 come back into the pack.
27. Shift-click Store on a stack. The whole stack goes in. Shift-click Take on a stack. The whole stack comes out.
28. Click Max on a Take row when your pack is nearly full. Max stops at what you can carry (weight), not at what is in storage.
29. Type a number larger than you hold in a Store row (for example 50 when you hold 5). It clamps to what you hold.
30. If a storage is full or a take would be too heavy, the row greys out with the reason on line one, and nothing moves.
31. Open a water barrel or any water row if you can find one. Store and take water with the stepper. (Claude Code did not test these in Play Mode, so note anything odd.)

## E. Layout and feel

32. At your normal window size, nothing overlaps. Each row shows both lines clearly, plus and minus buttons are visible and clickable.
33. Make the game window small (or play at 1024x768). Rows stay readable and the list scrolls instead of overflowing.
34. With a long list, scroll with the mouse wheel. Rows scroll smoothly and the stepper on any row still works after scrolling.
35. Heading hints read "Shift-click: 10" on buy and "Shift-click: the lot" on sell and storage.

## F. Regression checks

36. Inventory (I) still opens and closes normally. Hotkey assignment on the Inventory screen still works when you are not typing in a quantity box.
37. Walking, camera and the pause menu behave normally after using any quantity screen.
38. Save, quit to menu, load. Cash, pack and storage contents are exactly what they were before saving.
39. Console stays clean: no red errors and no yellow warnings during any of the above (ignore warnings you saw before this build and note them).

## Notes for Claude

- Anything that felt awkward (too many clicks, a button too small, a box that was hard to click) goes here, even if it passed.
- Which items, if any, did you want a quantity box for that did not have one?


---

## Result

Played by Mike: items 1 to 39 all pass. No notes added.
