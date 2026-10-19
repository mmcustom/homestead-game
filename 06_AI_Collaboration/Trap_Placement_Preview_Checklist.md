# Trap Placement Preview (Placement Preview Phase 6) - Play Mode Checklist

Built 2026-10-19, reworked after Mike's first playtest (ghost shows before clicking, one click sets). Compile-checked and the 62 existing edit-mode tests pass. NOT played since the rework. UNCOMMITTED. No trap rule was changed.

**How it works now:** equip a Rabbit Snare, Box Trap or Fish Trap and a translucent ghost follows where you aim straight away (green = allowed, red = refused with the reason; reach 6 m). ONE left click sets the trap at the ghost. There is no separate mode: nothing to cancel, Esc still opens the pause menu, right mouse does nothing for traps, the wheel does nothing (no trap needs a facing; they face the way you do, as before). The Build tab's Placement Mode is separate and unchanged.

**Setup:** back up `slot1` first (restore and hash-verify after). Have 1+ of each trap in the pack (debug/TimeDebugControls item keys or the Trading Post). Good places: land near trees for snares, a forage patch for the Box Trap, the creek (outside the 14 m spring and 32 m pond zones) and the pond for the Fish Trap.
**Debug keys:** Numpad 8 steps the creek to Flash flood (sweeps creek Fish Traps); Numpad 4/5/6 pond level; F-keys per TimeDebugControls for time skips; Shift+F10 debug collapse.

## A. Equip shows the ghost
1. [ ] Equip a Rabbit Snare (no click yet): a ghost of the snare appears where you aim, and the HUD reads "Place Rabbit Snare here - <excellent/good/fair/poor> spot" when green. Same for Box Trap and Fish Trap.
2. [ ] The old "Click to choose where to set the ..." text is gone.
3. [ ] The ghost is translucent, green where allowed, and follows the aim every frame; it stays on the ground or the water surface.
4. [ ] The ghost has no catch or bait pieces showing (Box Trap, Fish Trap, Rabbit Snare).
5. [ ] You cannot aim at, bump into, or interact with the ghost (no prompt on it, you walk through it).
6. [ ] The spot word matches what the Check prompt says after you set it.

## B. Red reasons (same wording as before)
7. [ ] Snare/Box Trap aimed at water: red, "Can't set a <name> in water".
8. [ ] Snare/Box Trap on a steep slope or a non-terrain surface (a log, rock, cabin): red, "<name> - needs open, level ground".
9. [ ] Fish Trap aimed at dry ground: red, "Fish Trap - aim at water close by".
10. [ ] Fish Trap where the water is too shallow: red, "Too shallow for fish here".
11. [ ] Within 2 m of another trap: red, "Too close to another trap".
12. [ ] More than 6 m away: red, "Too far - come within 6 m."
13. [ ] Aimed at the sky: no ghost, and the hint says to aim at the ground (or water for the Fish Trap).
14. [ ] Left click while red sets nothing, flashes the reason, and the pack is unchanged.

## C. Setting (single click)
15. [ ] One left click on green sets the trap exactly at the ghost, the trap leaves the pack (one), with the usual message ("Set the Rabbit Snare"; the Box Trap says to bait it) and sound (trap-set; Fish Trap splash).
16. [ ] The setting click sets exactly one trap (no second trap from the same click) and swings no tool.
17. [ ] Click again right away: the ghost is now red "Too close to another trap" at that spot (spacing rule), until you aim elsewhere; set several traps in a row.
18. [ ] Esc with a trap equipped opens the pause menu as normal (it is not swallowed); closing it, the ghost comes back.
19. [ ] Right mouse with a trap equipped puts the trap away: it stays in the pack (check the Inventory), nothing is set or spent, the ghost disappears and your hands are free. The number key (or the hotbar) re-equips it and the ghost returns. Right mouse with nothing/another tool equipped behaves as before (Hunting Weapon aim still works).
20. [ ] The mouse wheel does nothing for traps beyond what it did before.
21. [ ] The trap faces the way you were facing when you set it (same as the old behaviour).
22. [ ] With only one trap, set it: the pack is empty, the ghost goes away cleanly.

## D. Fish Trap at the waterline
23. [ ] Creek (outside the zones): ghost green on water, Fish Trap set works.
24. [ ] Pond: ghost green, set works; the Check prompt and catches behave as before.
25. [ ] The ghost sits at the same height the real trap does.
26. [ ] Where the old rule refused (None water/shallow), the ghost is red; where it allowed, green. Walk the shoreline and compare with a few sets.

## E. The ghost goes away by itself
27. [ ] Switch to another tool (number keys): the ghost disappears at once; switch back, it returns.
28. [ ] Open the inventory, crafting screen, journal or map: the ghost disappears (and a click in the screen sets nothing); close it, the ghost returns. Open the pause menu: same.
29. [ ] Sleep while a trap is equipped: no ghost while asleep.
30. [ ] Collapse (Shift+F10) with a trap equipped: no ghost, nothing set; the ghost returns after waking if the trap is still equipped.
31. [ ] Load a save, or quit to the main menu and start a game: no stray ghost left behind; it appears again only when a trap is equipped in play.

## F. Ghost never counts as a trap
32. [ ] With a ghost showing, save and inspect the world json: no extra trap (compare `traps` to what you set).
33. [ ] Skip time with a ghost showing (Numpad 2): nothing is caught in the ghost; real traps roll as normal.
34. [ ] Numpad 8 (Flash flood) with a Fish Trap equipped: only real creek Fish Traps are rolled for the sweep.
35. [ ] The ghost does not block setting another trap, and the spacing test counts only real traps.

## G. Regression
36. [ ] Build-tab Placement Mode (a pile, the Rain Barrel eave snap, a Tent, a Campfire): works exactly as before, including Esc and right-click cancel.
37. [ ] The wheel still turns Build-tab structures where facing matters.
38. [ ] Trap catches: set a Snare and Box Trap, skip hours, catches appear; Check, Bait, Repair and Pick Up (R) work.
39. [ ] Fish Trap catches in creek and pond; pond level effects (Pond Drying) still change the odds.
40. [ ] Flash flood still sweeps creek Fish Traps (message "A flood swept away a Fish Trap."); pond traps are safe.
41. [ ] Rain Barrel and fishing rod unaffected; a click with a rod still casts.
42. [ ] Collapse during Build-tab placement still cancels it; a collapse with a trap equipped leaves no stuck ghost (item 30), and the next click on a trap after waking works.
43. [ ] Save with traps set, load: traps are back, no new fields (diff the json), ghost absent.
44. [ ] Load an OLD save (made before phase 6): traps and everything else load; setting traps works.

## Notes to fill in
- Is 6 m the right reach for the trap ghost (it was 4 m)?
- Is the ghost visible and readable on all three traps, especially the Rabbit Snare (small) and Fish Trap at the waterline?
- Does the red-click flash feel right, or should a red click do nothing?


---

## RESULTS (Mike, played 2026-10-19)
**44 of 44 PASS.** One pass carries a design comment, not a failure:
- **Item 19 (right mouse with a trap equipped does nothing):** pass, but Mike thinks a right click should cancel. CHANGED 2026-10-19: right click now puts the trap away (Unequip, trap stays in the pack; the number key re-equips it). Item 19 was reworded and needs a re-check.

**Notes answered:**
- 6 m is the right reach for the trap ghost: yes.
- Ghost is visible and readable on all three traps, including the Rabbit Snare and the Fish Trap at the waterline: yes.
- Red-click flash: "flash feels right" (keep it).


## RE-CHECK of item 19 after the right-click change (Mike, 2026-10-19)
**5 of 5 PASS:** right click with a trap equipped puts it away (trap stays in the pack, ghost gone); number key re-equips it; Hunting Weapon aim unaffected; Build-tab right-click cancel unaffected and does not also put away a held trap; Esc still opens the pause menu. Item 19 is now resolved. The checklist stands at 44 of 44 PASS with the right-click change.
