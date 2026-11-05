# Hands M0d Playtest Checklist (equipped-tool line becomes a 2.5 s fade-in)

Version 0.1, 2026-11-05. Code baseline c97f731 (M0c-2). Build is uncommitted until you pass this list.
Suite expected: 1836 of 1836 (1824 plus 12 new). Console expected: 0 errors, 0 warnings.
Before you start: Saves is hash-identical to SavesBeforeM0d. Continue from slot 4. Do NOT run Seed MarketData. Open the Bootstrap scene, Game tab maximized (Shift+Space).
Pass format: reply "1 - N pass" or list the numbers that failed with a note.

What changed: the permanent line at the crosshair that said what you were holding ("Recurve Bow - hold to draw, release to shoot  Arrows: 10") is gone. The same text now fades in for about 2.5 seconds when you pick up a tool, then disappears. It is size 28 (the old line was 22). Prompts, bars and in-use lines are unchanged.

## A. The fade-in

1. Empty hands: no text at the crosshair.
2. Equip the Axe. Its text ("Axe - hold click on a tree trunk to chop it down, or on a wood pile to split Firewood") fades in, stays, and fades out in about 2.5 seconds.
3. Three seconds after equipping, nothing is on screen from the tool.
4. The text is easy to read (size 28), in the same place and style as before.
5. Switch tools quickly. Each switch restarts the fade with the new tool's text.
6. Check these tools each show their own text on equip: Pick Axe, Shovel, Hammer, Rod, Cane Pole, Spile, a trap, Bow Drill, Primitive Bow, Recurve Bow, Rifle.
7. Torch and Lantern: on equip they show their line (lit or unlit, hours left, "Click to light it") for 2.5 seconds and then it goes.
8. Tools that never had a line (Bucket, Canteen, Pouch, Knife, Saw, Flint and Steel, Cooking Pot, Clay Pot, Tarp): they now show their NAME for 2.5 seconds on equip. Tell me if you want that removed.

## B. Counts show again when they change

9. Bow: shoot an arrow. The line comes back for 2.5 seconds with the new Arrows count.
10. Rifle: fire a round. The line comes back with the new Rounds count.
11. Set a trap. The line comes back (the number of that trap changed).

## C. Prompts and in-use lines still there

12. Bow: while drawing you still see "Drawing... Arrows: N" with its bar, and "Full draw - release to shoot" with its bar. They are not covered by the fade-in.
13. Rifle: "Working the bolt... Rounds: N" with its bar, and "Rounds: N" while aiming in the scope.
14. Rod: aim at water and you see "Click to cast", then "Waiting for a bite...", "Bite!", and the reeling line, as before. Aim at too-shallow or fast water: the refusal text still shows. (Claude Code could not check the waiting and bite lines itself, so look closely.)
15. Axe on a tree trunk and on a wood pile, Pick Axe at the outcrop, Hammer at a building site, Shovel digging (including the "too deep" text): the same prompts as before.
16. Spile at a Sugar Maple, Bow Drill at a Campfire ("hold click to drill", "Drilling..."), Bucket or Canteen at water ("Fill..."), pushing the Hand Cart ("pushing..."): the same prompts as before.

## D. Nothing else changed

17. Compass, minimap, status bars, teaching prompts and all other text look and read the same.
18. Hunting, fishing, water, fire, traps, Hand Cart and swings behave exactly as before. The hands and tools look the same as in the last build.
19. Console shows 0 errors and 0 warnings after the whole session.

## Feel questions (one line each)

- Is 2.5 seconds the right time to read the text, or should it stay longer or shorter?
- Is the bigger size (28) OK, or too big next to the 22 prompts?
- Torch and Lantern: the hours-left line used to be permanent and is now only shown on equip. Do you miss seeing the hours while it burns? Should lighting or snuffing show it again?
- Are you OK with the name-only text for tools that had no line before (Bucket, Knife and so on)?
