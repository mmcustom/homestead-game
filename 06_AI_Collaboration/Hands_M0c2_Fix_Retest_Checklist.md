# Hands M0c-2 Fix Re-test (right hand draws the bow, hanging items hang down)

Version 0.1, 2026-11-05. Code baseline 0d6cdd3 plus the uncommitted M0c-2 build and fix. Suite: 1810 tests; one failing geometry test was fixed and only its class re-ran (40 of 40), so the full suite must be re-run before the commit. Console expected 0 errors and 0 warnings. Saves: SavesBeforeM0c2Fix is hash-identical to Saves. Game tab maximized (Shift+Space). Play Mode stopped to begin.
Pass format: reply "R1 - R20 pass" or list what failed with a note.

## Bows (Primitive Bow and Recurve Bow only)

R1. Equip the Primitive Bow. It is held in the LEFT hand, upright, and you can see it.
R2. Equip the Recurve Bow. Same, and it looks different from the Primitive Bow.
R3. Start a draw. The bow does NOT move or tilt. Only the RIGHT hand moves back toward your face.
R4. The string follows the right hand (it stretches from the limb tips to the hand).
R5. At full draw the right hand holds with a faint tremble, and the bow stays still in the left hand.
R6. Release. The right hand springs forward and settles (about 0.25 s). The bow does not move.
R7. Let go early. The right hand eases back, no stuck hands.
R8. The Bow Drill is exactly as it was (sawing) and the Rifle is exactly as it was (ready, aim, shot, bolt).

## Hanging items

R9. Bucket: the body hangs straight DOWN under the hand, no forward tilt, and you can see it.
R10. Sap Bucket and Fish Trap hang straight down and are visible.
R11. Canteen and Pouch hang straight down. Tell me if they are tucked behind the cuff and too small to see.
R12. Cooking Pot, Potash Kettle and Box Trap hang straight down. Tell me if any is half behind the sleeve.
R13. Walk with each hanging item. A small gentle swing, a bit more when sprinting, and nothing leaves the screen.
R14. Equip a hanging item. A tiny settle swing (about 1 second) and then it is still.
R15. Dip (Bucket, Canteen), drink, and trap place-down still read well and the item keeps hanging.
R16. Lantern, Rabbit Snare, Clay Pot, Tarp, Knife and Flint and Steel are unchanged.

## Regression

R17. Hunting, water, fire, traps and the Hand Cart behave as before. No HUD text changed.
R18. The Hand Cart push pose is still good.
R19. Console shows 0 errors and 0 warnings. No Resources/Art folder and no Captures folder.
R20. Claude Code left the Game view on a custom 1920 by 1080 size called "Mike16x9". Tell me if you want it back on your old size.

## Feel questions (one line each)

- Is the bow's look acceptable for now (thin dark profile, big sleeves), or should it be more visible?
- Should the Canteen and Pouch sit even higher or farther left so they clear the cuff?
- Should the Lantern also get this same carry pose and walk swing, for consistency? (My suggestion: no, it passed M0c-1 and already hangs straight.)
