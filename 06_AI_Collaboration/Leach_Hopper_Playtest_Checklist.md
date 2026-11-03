# Leach Hopper separate loading: playtest checklist (2026-11-04)

Code baseline 821a0d4. Not committed yet. Play from your slot 4 save. Maximize the Game tab with Shift+Space.

Dev keys: Ctrl+Insert stands an empty Leach Hopper about 3 m ahead and puts 12 Ash and 12 L of water (6 unsafe, 6 good) in your pack. Ctrl+Delete finishes every running batch now. (Insert alone still loses the game; do not press it.)

Controls: R loads Ash and water (separately, any time). E collects Lye first, otherwise starts a batch. R with the Hammer in hand dismantles.

Reply with the numbers that pass and describe anything that does not.

## A. Prompts and loading

1. Press Ctrl+Insert. Put the Hammer away. Aim at the Hopper: the prompt reads "Leach Hopper  (empty)" and tells you to load with R.
2. Empty your pack of Ash and water first (drink the water in the Inventory with I, put the Ash in a Wood Pile or drop it). Aim at the Hopper: the R line reads "Load Ash and water  (you're not carrying any)".
3. Carry only Ash and press R. It loads the Ash and says there is no water in your pack. The E line reads like "Leach Hopper  (N Ash, 0 L of water - needs water to start)". Press E: it says it needs water.
4. Carry only water (no Ash) and press R. It loads the water and says there is no Ash in your pack. Press E: it says it needs Ash.
5. Carry both and press R. Both are stocked, worst water first. The E line reads like "Start leaching  (N Ash + N L -> N Lye in 24 h)".

## B. Starting, topping up, finishing

6. Press E. A batch of the smaller of the Ash and litres starts. If the numbers were uneven, the leftover shows as waiting ("3 Ash left over").
7. While it leaches, press R with more Ash or water. It loads (up to the caps) and the prompt shows them as waiting. The batch timer does not change and the new material does not join the running batch.
8. Press Ctrl+Delete. The running batch's Ash becomes Lye. The waiting Ash stays Ash.
9. Press E: the Lye is collected first (and goes into your pack).
10. Press E again with Ash and water still waiting: a new batch starts from the stock.
11. Caps: load until you hold 12 Ash (batch, stock and waiting Lye together), then press R with more Ash. It says the Ash is full. Do the same with 12 L of water: it says the water is full.
12. The water you stored does not add weight to anything and does not take room that Ash needs. (Check your pack weight before and after storing water; it should not change from the water you put into the Hopper beyond the water you gave up.)

## C. Looks and help text

13. The Hopper fill level shows the Ash and Lye. When water is stored there is a thin blue-grey layer so you can see it.
14. Open the Know-how entry "Using the Leach Hopper". It reads correctly: R to load Ash and water, E to start, Lye comes out after 24 game hours.
15. Open the Crafting screen help line for the Leach Hopper. It reads correctly for the new steps.

## D. Dismantle and the rest of the chain

16. Equip the Hammer and press R on a Hopper holding Ash, Lye and water. The Ash and Lye come back whole; the water is lost.
17. Lye still goes through the Potash Kettle on a Tripod to make Potash as before (3 Lye plus 2 Firewood, 18 s). Soap and the market prices are unchanged.

## E. Saves and regression

18. Save, quit to menu, load the same slot with a Hopper holding stocked Ash, stored water and a running batch. Everything is still there and the timer still counts down.
19. A Hopper you already had in slot 4 (if there is one) loads and still works.
20. Optional (hard to set up): an old save with a Hopper mid-batch loads and the whole Ash count becomes Lye when it finishes. Claude Code tested this in code only. Skip it if you have no old save to try.
21. Console shows no new errors (the PlayerController.cs:217 NullReferenceException on stopping Play Mode is the old known one).
