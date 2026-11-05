# Hands M0c-2 Fix Round 2 Re-test (bow look, crosshair, arm length, see-through sleeve)

Version 0.1, 2026-11-05. Code baseline 0d6cdd3 plus the uncommitted M0c-2 build and two fix rounds. Full suite 1817 of 1817 in one run. Console expected 0 errors and 0 warnings. Saves hash-identical to SavesBeforeM0c2Fix2. Bootstrap scene open, Game view on Full HD (built-in). Game tab maximized (Shift+Space).
Pass format: reply "S1 - S16 pass" or list what failed with a note.

## Cause found for the see-through sleeve

World shadows were landing on the hands (a shelter's slats cast dotted stripes on the sleeve). Fix: the hands and held tools no longer receive shadows. It existed since M0a and was only obvious now.

## Bows

S1. Primitive Bow (pale wood) reads as a bow: a grip in the left hand, two curved limbs joined to it, and a string. It sits low on the left.
S2. Recurve Bow (darker) reads as a bow, with the tips flicking forward. Tell me if its dark colour is too hard to see on grass.
S3. At ready, half draw and full draw, neither arm covers the crosshair (both bows).
S4. Draw: the bow stays still in the left hand. Only the right hand moves back and to the right, and the string swings over to it.
S5. Release: the right hand springs forward and settles, bow still. Early let-go eases back.
S6. Known weak spots (tell me if they bother you): the lower limb runs off the bottom of the screen, and the string is a thin line.

## Arm length and hanging items

S7. The arms look a normal length now at bow ready and at full draw, no longer stretched toward the middle. Compare with the Axe at ready.
S8. Bucket: hangs straight down, you can see it, and the arm no longer looks extra long.
S9. Sap Bucket, Canteen, Pouch, Fish Trap, Box Trap, Cooking Pot and Potash Kettle: each hangs below the hand and clears the cuff. Known: Pot and Kettle read as dark silhouettes, and the Fish Trap's lower end may touch the bottom edge.
S10. Dip, drink and trap place-down still read well and the item keeps hanging.

## See-through sleeve

S11. Walk and turn with the Bucket near a shelter's slats: the sleeve is solid, no dotted stripes or world showing through. Also check with the Axe and with empty hands.
S12. Hands under a roof or in the shade do not look odd (they are lit by sun and sky but never shadowed, so they may look a little bright in shade). Tell me if it bothers you.

## Regression

S13. The Bow Drill, Rifle, Axe, Hammer, Rod, Torch and Lantern look and work as before.
S14. Hunting is as before (draw time, damage, arrows, rifle bolt 1.3 s). No HUD text changed.
S15. Game starts normally: open Bootstrap, press Play. Tell me if the Game view size is not what you usually use.
S16. Console shows 0 errors and 0 warnings after the whole session.

## Feel questions (one line each)

- Is the Recurve Bow's dark tone readable on your screen?
- Should the Pot and Kettle be lighter still?
- Does the stripe pattern go away in your real shelter scene? (If it persists, Claude Code would look at Motion Blur next.)
