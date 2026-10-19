# Creek Crossing Danger (Flash Floods Phase 5) - Play Mode Checklist

Built 2026-10-19. Compile-checked and edit-mode tested only. NOT played. UNCOMMITTED. All numbers are first-pass (Claude, not yet confirmed).

**Before you start:** back up `slot1` (see the playtest-save rule), then restore and hash-verify after.

**Debug keys:** Numpad 8 steps the creek Normal -> High water -> Flash flood -> Normal. Numpad 9 shows the stage plus `in creek: yes/no, flow <deg>, speed x<factor>` (and `knocked down` or `immune <s>`). Shift+F10 is the debug collapse; F-keys per TimeDebugControls.

**Where to test:** the creek between the Spring Hollow zone (14 m round (-15,115)) and the pond zone (32 m round (-185,-150)). Good wading spots: about (-88,42) and (-118,-30). Wading depth must be ankle (0.15 m) or deeper.

## A. Normal creek is unchanged
1. [ ] At Normal, wade across the creek: walking speed, sprint and stamina feel exactly as before.
2. [ ] Numpad 9 in the creek at Normal: `in creek: yes`, `speed x1.00`.
3. [ ] No toast appears when wading at Normal.
4. [ ] Wading still cools you (wet and cold) as before.

## B. High water: slow and tiring
5. [ ] Numpad 8 once (High water). Step into the creek: toast "The creek is running high." once.
6. [ ] Standing in the creek, Numpad 9 shows `in creek: yes`, `speed x0.85`.
7. [ ] Walking and sprinting in the water are visibly slower than on the bank.
8. [ ] Stamina drains slowly while standing in the water (about half the sprint rate), and does not recover while you stay in.
9. [ ] Stamina never goes below 0, and nothing else happens at 0 (you can still wade out).
10. [ ] No push, no knock-down, however long you stand in the water.
11. [ ] Step out and back in within a second: the toast does not repeat. Leave for 2+ seconds and re-enter: it shows again.
12. [ ] Ankle test: where the water is shallower than the ankle (bank edge), no slow, no drain.

## C. Flash flood: push, knock-down, lockout, immunity
13. [ ] Numpad 8 to Flash flood. Step into the creek: toast "The water is too fast to cross." once per entry.
14. [ ] In the water: `speed x0.60` on Numpad 9, you are pushed downstream steadily (about 1.5 m/s), stamina drains fast.
15. [ ] The push direction follows the creek: check at two or three different bends (Numpad 9 shows the flow angle; it should point toward the pond).
16. [ ] Stay in the water about 1.5 s: toast "The current knocks you off your feet!" and you cannot move, look-turn still works (movement, jump, sprint, crouch, interact, tools are locked) for about 1.5 s.
17. [ ] During the lockout you are carried downstream (about 2.5 m/s) and are not pushed through trees, rocks or the ground.
18. [ ] The knock-down costs a chunk of stamina (25) and 2 Health.
19. [ ] After the lockout you can move again; Numpad 9 shows `immune` with a countdown (about 4 s). Staying in the water during that time does NOT knock you down again.
20. [ ] Walk or sprint out sideways during the immunity: you can reach the bank.
21. [ ] If you stay in after immunity ends, you are knocked down again after about 1.5 s.
22. [ ] Sprinting straight across a narrow stretch (upstream end) can beat the 1.5 s timer; walking across usually cannot. Note how fair this feels.
23. [ ] Wall check: get pushed against a tree or bank: you stop; you are never pushed through it or launched into the air or under the ground.
24. [ ] Fully loaded pack (slow): you can still step sideways out of the water.

## D. Health floor and no collapse
25. [ ] Set Health to about 20 with the debug tool first (100 would take 45 knock-downs). Get knocked down several times in a row (re-enter after each immunity): Health goes 20 -> 18 -> 16 ... and stops at 10. (From full Health the sequence is 100 -> 98 -> 96 and so on.)
26. [ ] At Health 10, a knock-down takes no Health and you do not collapse.
27. [ ] Set Health to about 6 (debug): a knock-down takes no Health, no collapse.
28. [ ] Set Health to 11 (the loss of 2 would cross the floor): a knock-down leaves exactly 10, not 9. (At 12 it would leave exactly 10 with no clamping.)
29. [ ] Stamina at 0 when knocked down: stays at 0, never negative or glitchy.
30. [ ] No item is lost or dropped at any point; no drowning.

## E. Zones: Spring Hollow and the pond are unaffected
31. [ ] In Flash flood, wade in the creek inside the 14 m Spring Hollow zone: no slow, no drain, no push, no toast (Numpad 9: `in creek: no`).
32. [ ] Wade in the creek inside the 32 m pond zone near the pond: same, nothing happens.
33. [ ] Wade in the pond itself: nothing happens.
34. [ ] Walk from the spring downstream: the effects begin just outside the spring zone and end when you enter the pond zone.
35. [ ] Standing on the bank beside the creek (not in the water): nothing happens, even at the channel edge.

## F. Safe cases
36. [ ] Placement Mode: start placing something while standing in the flooded creek: no push, no slow, no knock-down, and the placement is NOT cancelled.
37. [ ] Leave Placement Mode in the water: the effects resume.
38. [ ] Open the pause menu, inventory or another screen while in the flooded creek: nothing happens to you meanwhile (no knock-down appears after closing from time spent in the menu).
39. [ ] Sleeping: sleep in a shelter near the creek during a flood: no push, drain or knock-down.
40. [ ] Collapse (Shift+F10) while in the flooded creek: no push; you wake as normal.
41. [ ] Knocked down, then open the pause menu: the lockout does not run out while paused; resuming continues it.
42. [ ] Load a save while knocked down or immune: you are not locked or immune after loading.
43. [ ] Start a New Game from the menu after a flood: no leftover lockout, immunity or toast.
44. [ ] Stage drops back to Normal while you are in the water: push and slowdown stop at once.

## G. Save and load
45. [ ] Save in the middle of a flood, quit, continue: the stage is as saved (existing behavior); the crossing itself has no saved state.
46. [ ] Save files contain no new fields (diff the player and world json against a pre-build save).
47. [ ] A pre-build save loads and plays normally.

## H. Regression
48. [ ] Fishing: creek casting still refuses in a flash flood; pond fishing unaffected.
49. [ ] Traps: a flash flood still rolls to sweep creek Fish Traps; pond traps safe.
50. [ ] Pond Drying: levels, Numpad 4/5/6, murk and the state text unchanged.
51. [ ] Rain Barrel: fills in rain, fill and draw work as before.
52. [ ] Flood warnings: "The creek is rising.", the "in flood" banner, the Weather screen Creek row and Watch text, and the wake-up summary still appear.
53. [ ] Numpad 7 and Numpad 9 overlays still show the stage text, and Numpad 9 now adds the crossing text.
54. [ ] Jumping, crouching, sprinting on dry land, interactions and tool use behave as before outside the creek.
55. [ ] Placement Preview unaffected.

## Notes to fill in
- Does 1.5 s feel right for the knock-down? Does the 4 s immunity let you escape?
- Is the push strong enough to be felt but never a trap?
- Is the edge of the channel (width plus 1 m margin) where the water actually ends?


---

## RESULTS (Mike, played 2026-10-19)
**55 of 55 PASS.** Two passes carry comments that are design feedback, not failures:
- **Item 18 (knock-down costs 25 stamina and 5 Health):** pass, but Mike disagrees with the Health loss. CHANGED 2026-10-19: the field is now `floodKnockDownHealthLoss`, default 2 (Mike's working value, not yet confirmed). Items 18, 25, 27 and 28 above were reworded for it and need a re-check.
- **Item 52 (flood warnings, wake-up summary):** pass, but Mike's note about the "while you slept, the creek flooded" message was cut off before it said what is wrong. OPEN: need the rest of the comment.

**Notes answered:**
- 1.5 s knock-down feels right: yes. 4 s immunity lets you escape: yes.
- Push strong enough to be felt but never a trap: yes.
- Channel edge (width plus 1 m margin) matches where the water ends: yes.

The "feel stuck" risk (items 22, 23, 24: narrow-stretch sprint, wall pinning, fully loaded pack) did not show up in play.
