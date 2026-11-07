# Hands M0e-3 Playtest Checklist (Melee prompts, Know-how, shared small game, Knife hit sound)

Version 0.1, 2026-11-07. Code baseline 1a25173 (Knife) plus the uncommitted M0e-3 build (MeleeRules, MeleePromptsTests, MeleeAttack, KnowHowRules, KnifeRules, PunchRules, AudioManager, Journal_KnowHow_Text.md and the updated tests). Build is uncommitted until you pass this list.
Suite expected: 1899 of 1899. Console expected: 0 errors, 0 warnings.

Before you start: Saves is untouched (backup copy SavesBeforeHandsV3 exists; Claude Code played only slot5). Do NOT run Seed MarketData. Game tab maximized (Shift+Space). For the Know-how steps, start a NEW game as well as using an existing one.

Pass format: reply "1 - N pass" or list the numbers that failed with a note.

What changed: a prompt appears when you aim at a live animal in reach. Fists: "<Species>: click to punch". Knife: "<Species>: click to slash" or "<Species>: click to stab" (always the NEXT blow). A new Know-how entry "Fighting with your hands and a Knife". Fists now stun Turkey and Duck too (same small-game list as the Knife: Rabbit, Squirrel, Turkey, Duck). The Knife has a thud sound when it hits an animal.

## A. Aimed-target prompts

1. Empty hands, aim at a live Rabbit or Squirrel within about 1.4 m: the prompt "Rabbit: click to punch" (or Squirrel) shows near the middle-bottom of the screen.
2. Same with a Turkey or Duck, and with a Deer: "Turkey: click to punch" and so on. A prompt shows for every live animal species.
3. Aim at the animal from just over 1.4 m (out of reach): no prompt. Step in: the prompt appears.
4. Put a wall or building between you and the animal: no prompt.
5. Equip the Knife and aim at a live animal within reach: the prompt shows "click to slash" the first time.
6. Click once (a slash): the prompt now says "click to stab". Click again: it goes back to "click to slash". The prompt always shows the next blow, never both, and never mentions Sprint.
7. Stand still for 2 seconds after a stab, with the animal still in reach: the prompt goes back to "click to slash" (the pause reset).
8. Aim at a dead animal or Carcass: NO melee prompt. The normal field-dress or harvest prompt still shows and still works (hold or press its key as before) with the Knife in your pack, equipped or not.
9. Equip any other tool (axe, rifle, rod and so on) and aim at an animal: no melee prompt; that tool's own prompts are unchanged.
10. No prompt while placing a structure, in a menu, pushing the cart or knocked down, even if aimed at an animal.
11. The prompt looks fine: readable, not flickering, disappears when you look away or the animal bolts.

## B. Fists and small game (Turkey and Duck now included)

12. Punch a live Rabbit or Squirrel: stunned for about a second, then it bolts. It does not die.
13. Punch a live Turkey, then a Duck: also stunned for about a second, then they flee. They do NOT die, no Carcass. (This is new: before, they only startled.)
14. Punch a Deer: it only startles and runs. No stun, no damage.
15. Fists cadence, stamina (2 per punch), reach and noise feel the same as before.

## C. Knife and small game, and the hit sound

16. Knife on a Rabbit or Squirrel: dies in one blow, leaves a Carcass.
17. Knife on a Turkey or Duck: dies in one blow, leaves a Carcass.
18. Knife on a Deer: only startles, no Carcass.
19. When a Knife blow HITS an animal, you hear a short dull thud on top of the blow's own sound. A swing that misses has only the blow sound. Tell me how the thud sounds (too loud, too long, fine). It is a stand-in, not a real blade sound.
20. Knife blows, stamina (3), reach, alternation and noise feel the same as before.

## D. Know-how entry

21. Start a NEW Settler game: the start message says two know-how entries were added (Tripod and "Fighting with your hands and a Knife"). You get the Melee entry right at the start because the starting kit has a Knife (Mike's decision, 2026-11-07: keep it this way).
22. Open the Journal, Know-how: "Fighting with your hands and a Knife" is there and readable. It says: fists only stun and startle; the Knife kills a Rabbit, Squirrel, Turkey or Duck in one hit and only startles a Deer; clicks alternate slash and stab with a slash after a pause; a punch costs 2 stamina and a slash or stab 3; noise 8 m and 6 m; no hitting through walls; trapped animals cannot be struck.
23. The text has no leftover "hold Sprint" wording and no obvious typos or wrong numbers.
24. Load an older existing save (one made before this build): it loads without errors, and the Melee entry appears once (it has a Knife) without duplicates. Quit and reload: still one entry, no repeat flash.
25. The other Know-how entries (Tripod, Smithy and so on) still open and read fine, none missing.

## E. Regression and wrap-up

26. Fists, Knife slash/stab, tool swings and the cart all still work as in the earlier checklists (a quick spot check of each, no need to repeat every step).
27. Quit Play Mode. Console has 0 errors and 0 warnings.
28. Anything else that felt off (prompt wording or position, sound, stun on Turkey/Duck, anything). One line.

Notes for me when you report: likely trouble spots are the prompt showing at the wrong time (3, 4, 8, 10), the field-dress prompt (8), the Know-how entry (21 to 24), and the hit sound (19). Tell me which felt wrong and I will write the tweak message for Claude Code.
