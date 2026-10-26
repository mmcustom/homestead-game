# Ash, Craft Amount and Campfire Removal Playtest Checklist

Date written: 2026-10-25, merged 2026-10-25. Build under test: Ash by-product of fires (G to collect), Ash market group, Kindling Bundle craft, craft amount stepper, Campfire removal with the Hammer (703 edit-mode tests pass, not yet Play Mode tested).

Reply with the item number and pass or fail (for example "7 fail, the third burn gave 0").

Before you start: the Saves folder is backed up (SavesBackup23, 64 files, diff -r 0 differences). Use a throwaway new game, any difficulty, 5 acres. Tell Claude Code when you finish so it can restore the Saves folder.

Tips: a game day is 30 real minutes, so one Firewood (2 hours of burn) takes 2.5 real minutes at normal speed. Sleeping also burns fuel. F11 gives the starting kit and F12 the full kit, which saves gathering. Homesteader starts with a Hammer; Settler and Pioneer buy one for $10 at the Trading Post or craft one (2 Sticks, 1 Cordage, 1 Log).

## Crafting a Kindling Bundle

1. Start a new game and open the crafting screen (B). "Kindling Bundle" is listed and needs 10 Sticks.
2. Craft one with 10 Sticks. The 10 Sticks go, 1 Kindling Bundle arrives (1 kg), and the help line for the bundle reads sensibly.

## Collecting Ash

3. Build a Campfire (3 Firewood) and light it. Look at it: the prompt has no "Collect Ash" yet.
4. Add Firewood until the fire holds 12 hours (6 Firewood; F cycles the fuel type), then wait about 7.5 real minutes (6 game hours) with the fire still lit. About 3 Firewood have burned.
5. Look at the fire: the prompt ends "G: Collect Ash (1)" while the fire is still lit.
6. Press G. The message says "Collected 1 Ash", the Ash is in the pack (0.3 kg each), and the "Collect Ash" part of the prompt is gone.

## The carry (fractions add up)

7. Start from a fire that has no Ash waiting. Add 1 Firewood, let it burn out, press G (nothing to collect). Do this three times in total: the third burn gives 1 Ash.
8. Burn Sticks: 12 Sticks give 1 Ash (not 6).
9. Burn Branches and a Log: 6 Branches give 1 Ash. 1 Log gives 1 Ash and leaves two thirds in the fire for the next burn (so a second Log gives 2, not 1).

## Keys and fire behaviour

10. With NO Hammer equipped: R puts the fire out while it is lit, and adds fuel while it is out (with Flint and Steel). (With the Hammer equipped and the fire out, R dismantles instead; that is item 29.)
11. Put the fire out with fuel left and Ash waiting. G still collects while the fire is out.

## Selling Ash and the Bundle

12. Trading Post (T): sell Ash. The price starts at $2 and falls about 6% a unit. After about 20 sold it stops falling at 50 cents. Sell a Kindling Bundle at about $1.
13. Press End (Editor). In ledger_items, ash and kindling_bundle are their own rows. In the category CSV, Ash is under "Firewood, logs and stone" and the bundle under "Crafted goods".

## Dismantling a hearth or Grill with Ash

14. Dismantle a Kitchen hearth or Grill that holds uncollected Ash. The message lists the Ash coming back, and the Ash is in the pack.

## Saves (Ash)

15. Save, quit and Continue: the Ash waiting in a fire is still there.
16. Load one of your older saves: its fires work as before. Fuel already in them earns no Ash, but fuel added afterwards does.

## Craft amount

17. Open the Craft tab (B). Every recipe now shows the amount stepper, not just Arrows and Cordage.
18. Pick Kindling Bundle with about 35 Sticks. The stepper's maximum is 3, and the button reads "Craft x3" after you step up.
19. Set 2 and click Craft: 2 Bundles are made, 20 Sticks used, and the stepper resets to 1.
20. Shift-click Craft: it makes the most allowed (3 with 35 Sticks).
21. Choose another recipe and come back: the amount you last chose for the first recipe is remembered. Close and reopen the screen: it starts at 1 again.
22. Carry limit: fill the pack near its limit, then pick a recipe that makes more weight than it uses (Lantern from Clay). The stepper maximum drops to what the room allows, never above it.
23. With the pack completely full and enough Clay for one Lantern, a single craft still works: it is made and the excess is left on the ground with the message.
24. Cordage with mixed Tall Grass, Cattail and Sinew: the maximum counts all three.
25. With too few materials, the stepper is greyed and Craft explains what is missing, as before.

## Campfire removal

26. Build a plain Campfire from 3 Firewood. Look at it with no Hammer in hand while it is out: "Equip a Hammer to dismantle" appears at the end of the prompt.
27. Light it: the hint is gone while it is lit, and R reads Put Out.
28. Put it out, then equip the Hammer: the R line reads "Dismantle the Campfire (back: ...)" and only then. E still adds fuel.
29. Add Firewood, burn some so it has Ash and fuel, put it out, then dismantle with R and the Hammer: you get 1 Firewood for the build plus the unspent fuel as Firewood, plus the whole Ash. The message lists them, and the fire disappears.
30. Build a fire from 6 Sticks (no Firewood). Dismantle it: 3 Sticks come back, no Firewood.
31. With a nearly full pack, dismantle a fire: what does not fit lies on the ground as pickups.
32. Save, quit and Continue after removing a fire: it stays gone. Load an older save with campfires: they load as before and dismantle with the Hammer for 1 Firewood.
33. Try a Kitchen hearth, a cabin hearth and a Grill fire with the Hammer: none offers the Campfire dismantle line. Each keeps its own structure Dismantle.

## Console

34. No errors or warnings in the Console during any of the above.
