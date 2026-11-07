# Hands M0e-2 Playtest Checklist (Knife)

Version 0.1, 2026-11-06. Code baseline b5c816a plus the uncommitted Knife build (KnifeRules, KnifeTests, MeleeAttack, PlayerHands, ToolEvents, Animal.Slay). Build is uncommitted until you pass this list.
Suite expected: 1882 of 1882. Console expected: 0 errors, 0 warnings from the game (the "PlayerLoop internal function has been called recursively" messages are known and untraced; they only show up in Play sessions run through the tooling. Note them, they are not a Knife failure).

Before you start: Saves is untouched (backup copy SavesBeforeHandsV3 exists; Claude Code played only slot5). Do NOT run Seed MarketData. Game tab maximized (Shift+Space). You need the Knife in your pack (it is in the starting kit) and a decent amount of stamina.

Pass format: reply "1 - N pass" or list the numbers that failed with a note.

What the Knife should do: equip it and left click alternates SLASH, STAB, SLASH, STAB. The first click after a pause of 1.5 s or more is always a slash. Slash: 0.7 s cycle, 3 stamina, reach 1.5 m. Stab: 0.8 s cycle, 3 stamina, reach 1.6 m. No Sprint+click. Any solid hit kills small game (Rabbit, Squirrel, Turkey, Duck) and leaves a Carcass. A Deer is only startled, never hurt.

## A. Equip and ready pose

1. Equip the Knife: the grip hand holds it, blade up, slightly cocked. The knife is in the right hand only; the left hand is relaxed and not on the knife.
2. Standing still, the hand sits steady in the ready pose (only the normal breathing sway).
3. Switch to another tool and back: the other tool looks and behaves as before, and the Knife returns to its ready pose.
4. Switch to empty hands: idle hands appear and left click punches (the fists still work).

## B. Slash and stab

5. Click once: a SLASH, a diagonal arc across the view.
6. Click again within about a second: a STAB, a straight forward thrust.
7. Click again: SLASH. The pattern keeps alternating slash, stab, slash, stab.
8. Hold the left mouse button: blows repeat on their own, alternating slash and stab, roughly 0.7 s then 0.8 s apart.
9. Click-spam faster than the cycle: extra clicks are ignored (no double blows, no animation glitching).
10. After a STAB, wait about 2 seconds with no click, then click: it starts with a SLASH (not another stab).
11. After a SLASH, click again quickly: it is a STAB (the pause reset does not trigger within 1.5 s).
12. After each blow the hand comes back to the cocked ready pose exactly (no drift, no stuck pose, no hand left out in front).
13. The blade, hand and sleeve look right in motion: no skin stretching, no sleeve poking through the screen, the knife does not detach from the hand. (Stab: say whether the blade pointing up while thrusting looks wrong to you. A blade pointing forward may read better. This is a feel note, not a pass/fail.)

## C. Timing, sound and stamina

14. The blow's sound and the stamina drop happen at the click, not after the picture lands. It should feel instant.
15. Each blow costs 3 stamina (slash and stab the same): watch the bar drop in small steps.
16. Keep swinging until stamina is low. When too tired, the hand droops and no blow happens. Holding the button retries about every 0.5 s. When stamina recovers, blows resume. The tired droop does not count as a slash or stab (the alternation stays where it was).
17. Stamina regenerates normally afterwards.

## D. Reach and walls

18. Face a tree, rock or wall at about 1 m and slash: the hit registers (sound or reaction, whatever the game gives).
19. Back away to about 2 m and slash, then stab: both swing but nothing is hit.
20. Optional: at about 1.55 m the stab (1.6 m reach) can reach something the slash (1.5 m) cannot. Hard to measure, note what you see.
21. Put a wall or building between you and an animal or object just behind it, within about 1.5 m: neither slash nor stab hits the thing behind the wall.

## E. Animals (creep up on one; animals bolt from about 3 m, so this takes patience. Crouch and approach from behind.)

22. Hit a live Rabbit or Squirrel: it dies from one blow and leaves a Carcass. The first blow kills, whether it was a slash or a stab.
23. Hit a live Turkey or Duck: it also dies from one blow and leaves a Carcass.
24. Hit a Deer if you can get close: it only startles and runs. It does not die, is not wounded, and leaves no Carcass.
25. Hit a wounded small animal if you have one (shoot it so it survives, or find one): the blow kills it.
26. Hit a dead animal or Carcass with the Knife: nothing happens (no second Carcass, no weird reaction).
27. With a Knife in your pack, field dress a Carcass from a Knife kill: it works as it does for a rifle kill (same meat and hide).
28. Trapped animal in a snare or box trap: swing at it and report what happens. Claude Code found catches are only text inside the trap system, not a live animal, so nothing is expected to be hit; the swing only makes noise. Not a failure.

## F. Noise

29. Swing near a calm animal (within about 6 m, not hitting it): it notices and startles.
30. Swing while animals are far away (more than about 6 m): they do not react.
31. A Knife swing is clearly quieter than a rifle shot, and a little quieter than the fists (6 m versus 8 m).

## G. Conflicts with other actions

32. With the Knife equipped, left click does the Knife blow and nothing else (no extra punch, no tool action).
33. Axe, hoe, rifle, rods and other tools: their left click behaves exactly as before.
34. Enter structure placement mode with the Knife equipped: left click places or cancels the structure and does NOT also slash or stab.
35. Open a menu or inventory, or pause the game, and click: no blow.
36. Push the cart or be knocked down by the creek: no blow.
37. Click a door, pick-up or crafting bench with the Knife equipped: the interaction works and you do not also swing (or note it if you do).
38. Swap Knife to empty hands and back while a blow is in progress, a few times: no stuck pose, no knife floating on screen.

## H. Wrap-up

39. Quit Play Mode. Console has 0 errors and 0 warnings (besides the known PlayerLoop messages).
40. Anything else that felt off (cadence, reach, the slash or stab animation, sound, anything). One line.

Notes for me when you report: the likely trouble spots are the stab blade orientation (13), the pause reset (10, 11), walls (21), input conflicts (32 to 38), and the sound (Knife has no sound of its own yet; it borrows the item-drop cue). Tell me which felt wrong and I will write the tweak message for Claude Code.
