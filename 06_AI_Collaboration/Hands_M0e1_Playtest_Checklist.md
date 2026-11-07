# Hands M0e-1 Playtest Checklist (Fists)

Version 0.1, 2026-11-06. Code baseline dc19c67 (committed, not pushed).
Suite expected: 1860 of 1860. Console expected: 0 errors, 0 warnings (the five known "PlayerLoop internal function has been called recursively" messages may still appear; note them, they are not a fists failure).

Before you start: Saves is untouched (backup copy SavesBeforeHandsV3 exists). Do NOT run Seed MarketData. Game tab maximized (Shift+Space). Start a new or existing test game with NOTHING equipped (empty hands, idle thumb-up hands visible) and a decent amount of stamina.

Pass format: reply "1 - N pass" or list the numbers that failed with a note.

What the fists should do: empty hands plus left click throws alternating punches (right first). One punch every 0.6 s. The fist lands about 0.10 s after the click. Each punch costs 2 stamina. Reach 1.4 m. Fists never kill anything.

## A. Looks and feel

1. With nothing equipped, you see the idle hands (thumb-up). Click once: the RIGHT fist drives forward toward the middle of the screen and comes back to exactly where it started (no drift, no stuck pose).
2. Click again after about a second: the LEFT fist throws (mirror of the right). Third click is right again.
3. Hold the left mouse button down: punches repeat on their own, alternating right, left, right, left, about 0.6 s apart. Not faster, not stuttering.
4. Click-spam faster than the cycle: extra clicks are ignored until the cycle is ready (no double punches, no animation glitching).
5. The fists and sleeves look right in motion: no stretching skin, no sleeve poking through the screen, no hand flashing to the wrong pose.
6. Release the button: hands settle back to the idle pose and stay still.

## B. Timing, sound and stamina

7. The punch sound (and stamina drop) happens at the click, not after the fist lands. The visual just catches up. It should feel instant.
8. Each punch costs 2 stamina: watch the stamina bar drop in small steps while you punch.
9. Keep punching until stamina is low. When you are too tired, the hands droop and you do not punch. Holding the button retries about every 0.5 s. When stamina recovers enough, punching resumes.
10. Stamina regenerates normally afterwards (same as before the fists).

## C. Reach and walls

11. Face a tree, rock or wall at about 1 m and punch: the hit registers (impact sound or reaction, whatever the game gives).
12. Back away to about 2 m and punch: the swing still plays but nothing is hit.
13. Stand with a wall or building between you and an animal or object just behind it, within 1.4 m of you, and punch: you do NOT hit the thing behind the wall.

## D. Animals (find a Rabbit, Squirrel, Turkey or Duck; then a Deer)

14. Walk up to a live Rabbit/Squirrel/Turkey/Duck and punch it: it is stunned for about 1 s (stops, maybe flops), then startles and runs away. It does NOT die and does not drop a carcass.
15. Punch the same kind of animal several times in a row: it never dies, however many punches.
16. Punch a Deer (if you can get that close): it just startles and runs. No stun, no damage, no death.
17. Kill an animal another way (rifle or bow, with a tool equipped), then switch to empty hands and punch the body/carcass: nothing happens (no extra damage, no weird reaction).
18. If you have a trapped animal in a snare: try punching it and note what happens (may not be hittable; just report it, not a failure unless it errors).

## E. Noise

19. Punch near a calm animal (within about 8 m, but not hitting it): it should notice and startle.
20. Punch while animals are far away (more than 8 m): they do not react.
21. The punch noise is clearly quieter than a gunshot: a rifle shot alerts far more animals than a punch does.

## F. Conflicts with other actions

22. Equip any tool (axe, hoe, rifle, etc.): left click does that tool's normal swing or action, NOT a punch, and the grip hands show. Unequip: idle hands and punching return.
23. Enter structure placement mode: left click places (or cancels) the structure and does NOT also throw a punch.
24. Open a menu / inventory / pause the game and click: no punch while the menu is open or the game is paused.
25. Push the cart (if available): no punching while pushing.
26. If you can get knocked down or are otherwise unable to act: no punching then.
27. Click a UI button or world interaction (door, pick-up, crafting bench) with empty hands: the interaction works and you do not also punch (or, if it does punch, note it).

## G. Wrap-up

28. Quit Play Mode. Console has 0 errors and 0 warnings (besides the known PlayerLoop messages).
29. Anything else that felt off (cadence, reach, punch force, animation, anything). Describe it in a line.

Notes for me when you report: the main risks are input conflicts (22 to 27), walls (13), trapped animals (18), and how the stamina cost and cadence feel against snares and the bow. Tell me which of those felt wrong and I will write the tweak message for Claude Code.
