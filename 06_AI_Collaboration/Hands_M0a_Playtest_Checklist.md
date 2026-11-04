# Hands and tools M0a (first-person hands): playtest checklist (2026-11-04)

Code baseline ef52c25. Not committed yet. Stop Play Mode before sending anything to Claude Code. Play from your slot 4 save. Maximize the Game tab with Shift+Space. Do not restore any save.

What M0a is: you now see your own two hands at the bottom corners of the screen. Empty-handed they rest low and breathe. When you press a tool key (1 to 9, 0) the hands lower, the tool changes and they rise holding a plain placeholder shape (a long staff for two-handed tools, a short block for one-handed ones, coloured by item type). The hands are drawn on top of the world so they never poke into walls, and the rifle scope does not zoom them. They show only while you are playing and hide behind the Inventory, Journal, Finances, pause and sleep screens. There are no swing poses yet (M0b), no real tool shapes yet (M0b and M0c), and the old line of text under the crosshair is still there (M0d). Tools work exactly as before.

Not expected yet: the hands staying up while placing a structure, pushing the Hand Cart or after being knocked down by the creek (M0c); the hands staying still when you chop (no swing pose until M0b).

Menu items (top menu bar, Homestead > Art): Create Test Hands Prefab and Remove Test Hands Prefab (items 18 and 19 only). Do those with Play Mode stopped.

Reply with the numbers that pass and describe anything that does not. I also want your opinion on the look: size, position, colour of the hands and the placeholder tools, and how the sway feels.

## A. The hands

1. Load slot 4. Two hands with forearms, sleeve cuffs and mittens show at the bottom corners, in a skin tone, relaxed and low, with a slow breathing sway. Nothing flashes or flickers.
2. If you saved with a tool equipped, the hands start already raised holding the placeholder for it.
3. The hands are lit like the world: they look brighter in direct sun, darker in shade, dim at night, and not blown out to near-white in bright sun.
4. Walk: a small step sway. Sprint (Shift): a different, stronger sway. Crouch: the hands sit a little lower.

## B. Equipping

5. Press a two-handed tool key (Axe, Saw, Shovel, Pick Axe, Fishing Rod, a bow or the Rifle if you have one). The hands go down, the tool changes at the bottom, and both hands rise holding a long placeholder staff.
6. Press a one-handed tool key (Hammer, Torch, Lantern or Knife). The left hand is hidden and a short block sits in the right hand.
7. Press the same key again: the tool is put away and both hands rest empty.
8. Switch quickly between several tools: no flicker and no leftover placeholder from the previous tool.
9. Equip a non-tool item such as a Cooking Pot, Clay Pot, Pouch or Canteen: a small block in the item's category colour, no errors.

## C. Walls, the camera and window sizes

10. Walk right up to a tree, a rock and a structure wall. The hands never poke into them.
11. Look up, down and around quickly: the hands stay fixed to the screen. Spin the camera fast: the hands stay sharp, with no trail or shimmer.
12. Stand in the shade right beside a wall while the sun is out: the hands are not strangely dark compared with the ground in front of you.
13. Make the Game tab 16:9, 21:9 and 32:9: the hands stay at the screen edges, not drifting to the middle and not clipped.
14. If you have a Rifle: raise the scope. The world zooms but the hands do not.

## D. Screens, saves and tools

15. Open the Inventory, Journal, Finances screen, pause menu and sleep: the hands are hidden behind each and come back when you close it. Equip a tool from the Inventory and close it: the raise plays then.
16. Save, quit to menu and load: the main menu has no hands; after loading, the hands show with the right tool raised.
17. Chop, dig and build a little: the tools work exactly as before and the text line under the crosshair is unchanged.

## E. Swapping in a prefab (the swap point)

18. Stop Play Mode. Homestead > Art > Create Test Hands Prefab (no error or warning in the Console). Enter Play Mode: a blue right hand and a magenta left hand replace the primitives, still follow the walking and equip poses, and tools still rise into them.
19. Stop Play Mode. Homestead > Art > Remove Test Hands Prefab. The Assets/Resources/Art folder is gone. Enter Play Mode: the primitive hands are back.

## F. Regression

20. The frame rate feels the same as before.
21. Console shows no new errors or warnings (the PlayerController.cs:217 NullReferenceException on stopping Play Mode is the old known one).
22. Git shows nothing under Assets/Resources/Art (Claude Code checks this at commit time).

---

# Re-test after the first playtest fixes (same day)

First playtest: everything passed except two things Mike reported with screenshots: the hands and staff showed inside the rifle scope, and the sleeve cuffs and mittens were not visible. Both are fixed (suite 1671 of 1671). Re-test these, then a quick regression.

R1. Rifle scope: right-click to aim. As the scope comes up the hands slide down out of view, and with the scope overlay showing there are no hands or staff at all. Lower the scope: the hands come back up with the rifle.
R2. Bows are unchanged: draw and release a bow and the hands stay as before (no scope).
R3. Hands at rest, 16:9: each hand shows a mitten (a flat palm with a separate thumb on the inner side), a short bare forearm, then a blue sleeve with a lighter blue cuff band entering from the bottom corner.
R4. Equip a two-handed tool and a one-handed tool: the sleeves and cuffs still show, the tool still sits in the hand (right hand, and the left hand on two-handed tools).
R5. 21:9 and 32:9: the hands are out toward the sides with sleeves visible. (At 32:9 they sit about a third in from each edge; tell me if you want them further out.)
R6. In shade and at night the blue sleeve is still easy to see.
R7. Walk, sprint and crouch still sway; walls still do not clip the hands.
R8. The prefab swap items (18 and 19) and the regression items 20 to 22 above still pass.
