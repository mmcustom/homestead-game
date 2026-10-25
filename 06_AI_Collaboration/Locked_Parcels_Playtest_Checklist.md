# Locked Parcels Playtest Checklist

Date written: 2026-10-25. Build under test: parcel lock (LandRules.IsAvailable, LandPurchasePanel, 655 edit-mode tests pass, not yet Play Mode tested).

Reply with the item number and pass or fail (for example "4 fail, the panel advanced").

Before you start: back up the Saves folder (Claude Code knows where it is, or ask it to). Items 11, 15 and 17 touch save slots, so use a throwaway slot for 11 and 17.

## New Game, Land step

1. Press Play, choose New Game, pick any difficulty, press Start. The Land step opens with 5 acres selected and bright, and Next is enabled.
2. The 10-acre and 40-acre cards are dimmed and read "Locked. Needs more income than the early game offers. Unlocks later."
3. (Editor only) The locked cards also show "(Shift-click: dev unlock)".
4. Click the 10-acre card with no Shift held. The note line at the bottom shows the reason, the selection stays on 5 acres, and the panel does not advance.
5. Click the 40-acre card with no Shift held. Same result as item 4.
6. With 5 acres selected press Next. You reach the Split step (Summary on Pioneer), then Summary, as before.
7. Back and Esc step back as before. On the Land step they return you to the difficulty panel.
8. Go back to the Land step after clicking a locked card. The reason message is cleared.

## Dev unlock (Editor only)

9. Shift-click the 10-acre card. It selects, the note line shows the usual savings note, and Next is enabled.
10. Press Next through Split to Summary. The Summary says "10 acres" with the 10-acre price.
11. Confirm so the slot pick opens, then press Back (do not pick a slot). You return to the Summary of the 10-acre parcel.
12. Press Back from that Summary to the Land step. The 10-acre card is still selected and Next is enabled.
13. Click the 5-acre card. The 10-acre card dims again.
14. Shift-click the 40-acre card. Same behaviour as item 9.

## Existing games

15. Load a save that has a 10-acre or 40-acre loan. It loads and plays as before, and Finances shows its acres.
16. In a dev-unlocked 10-acre game open Finances (Claude Code says F5). It shows "10 acres".
17. In a throwaway 10-acre game press Insert (force loss), then "Start over". It restarts as 10 acres, not 5.

## Console

18. No errors or warnings in the Console during any of the above.
