# Maple B (Taps) Playtest Checklist

Date written: 2026-10-26. Build under test: crafting the Spile and Sap Bucket, placing taps on Maples, sap accumulation, collecting, souring, taking taps down, felling, saves, the Home debug keys (757 edit-mode tests pass, not yet Play Mode tested). Sap is NOT boiled into Syrup yet (that is Maple C).

Reply with the item number and pass or fail (for example "8 fail, no room message").

Before you start: the Saves folder is backed up (SavesBackup25, 22 files, diff -r 0 differences). Use a throwaway new game. Tell Claude Code when you finish so it can restore the Saves folder.

Setup: the 12 nearest Maples (world X, Z) are (29,-107), (-3,-129), (12,-135), (74,-120), (-31,-138), (10,-155), (38,-156), (-28,-147), (68,-157), (-66,-134), (6,-167), (-3,-168). The first is about 67 m from spawn, 40 m past the cabin site. Only large trees count, so look at the tall ones. F11 and F12 give kits. Easiest way to reach a Maple: press Page Down (item 19). Debug keys: Page Down teleports to the nearest Maple (Shift+Page Down goes back), Home toggles "sap always runs", Shift+Home fills every bucket, F5 skips a day, F6 skips to the next season, F7 cycles x1, x10, x60, x360, F8 skips to Winter.

## Crafting

1. Open the Craft tab. "Spile" is under Tools and "Sap Bucket" under Containers and Storage. Craft one of each, then 3 Spiles at once using the stepper. Costs: Spile 3 Sticks; Sap Bucket 1 Log, 3 Branches, 1 Cordage.

## Looking and placing

2. With empty hands look at a Maple within about 3 m. The tool line reads "Sugar Maple, tappable" (and "equip a Spile to tap it" if you carry one). A non-maple hardwood or a shrub shows nothing. With an Axe equipped no maple line appears.
3. Equip the Spile and look at a Maple: the line says "click to tap it". Click. A small tap and bucket appear on the trunk, the Spile and Bucket leave your pack, and you see "Tapped the maple. 1 of 12 taps out."
4. Refusals: click on a non-maple (a message), on an already-tapped maple (message), and try with no Bucket in the pack (message).
5. Look at the tap. It reads like "Check the tap (empty, not flowing)", and R reads "Take down the tap (back: Spile and Sap Bucket)".

## Sap flow

6. Press Home ("Sap always runs: ON"). Press F7 until x60 and let a few game hours pass. The tap turns "flowing" and the bucket fills. Around 4 L you get one "Sap is ready" message, then no more that day.
7. Press Shift+Home to fill every bucket. Press E. You get "Collected 8 L of sap" and 8 Maple Sap in the pack.
8. Nearly full pack: fill a bucket again (Shift+Home), load the pack to just under the 55 kg limit with Stone or Logs, and press E. You get "Took N L ... still in the bucket", and a second E later takes the rest. With the pack completely full you get "No room to carry any sap".
9. Souring: keep Home on and fill a bucket. Press F5 twice (2 days) without collecting. The prompt changes to "souring in N h", then "sour, empty it". E pours it out ("The sap had soured ...") and gives nothing. Press Home again to turn it off.
10. Out of season: turn Home off in Summer (F6 to skip seasons). The tap shows "not flowing" and nothing fills.

## Taking down and felling

11. With a little sap in a bucket press R. You get the Spile and Bucket back, and the message says the sap was lost.
12. Tap a Maple, then chop it down with the Axe. You see "The tapped maple came down ...", the Spile and Bucket arrive in your pack (within 10 m) or as pickups beside the stump, and the tap is gone.
13. (Optional) Storm felling: tap a Maple, walk 15 m away and use the dev windthrow if there is one. The Spile and Bucket should be pickups on the ground by the stump. Skip this if you cannot force it for a chosen tree.

## Saves

14. Tap two Maples, put sap in one, save, then load. Both taps, their sap and their ages come back unchanged. Quit to the menu and load again.
15. Load slot1 (Settler, Spring 2). It loads with no taps, no errors, and the crafting list shows the new recipes.
16. 12-tap limit: craft 12 Spiles and 12 Buckets (36 Sticks, 12 Logs, 36 Branches, 12 Cordage; Shift-click Craft crafts the most allowed) and tap 12 Maples. A thirteenth says "You already have 12 taps out."

## Real weather and Console

17. (Optional) Press F8 to jump to Winter and skip a few days with Home off. The prompts show flowing or not flowing sensibly.
18. No errors or warnings in the Console during any of the above.

## Getting to a Maple (editor and development builds)

19. Use this first: press Page Down in a world with trees. You land about 2 m from a Sugar Maple trunk, facing it with the view slightly downward, and settle on the ground without falling through or sticking. The message names "Maple 1 of N" with a distance and position, and looking at the trunk shows the look prompt. Press Page Down again for Maple 2, then Shift+Page Down to return to Maple 1. Finally walk away and press Page Down: it continues the same order, not restarting from where you now stand.

## Also watch for

- the tap's size and position on a real trunk (too big, floating, wrong side);
- whether the Spile equips as expected;
- the prompt and the tool line fighting each other on the same tree.
