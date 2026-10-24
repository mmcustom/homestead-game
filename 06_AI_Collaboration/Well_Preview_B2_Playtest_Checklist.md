# Well Preview (B2) Playtest Checklist

**Goal:** prove the Well Site ghost now tells you how deep the water is, what is nearby and whether floods can reach the spot, and that nothing about digging, lining or water amounts changed.
**Before you start:** back up your Saves folder (copy it somewhere outside the project). Play in the editor with the B2 changes uncommitted. Use the F12 full kit if you need tools and materials for the Well Site, and build an Outhouse first for item 8.
**How to report:** reply with the item number and pass or fail ("1. pass, 2. fail - text cut off"). Add a short note on any fail.
**Colours:** green = fine, amber = legal but with a warning, red = refused.

---

## A. The preview text and colours

1. Enter Well Site placement on the **cabin ridge** (near the Cabin Site). The ghost is green, and the HUD shows "Place Well Site here", **"Depth to water: about 8.6 m (Deep)"**, a "Nearby:" line, then the keys line. All lines are visible, none cut off.
2. **Look hard at the ghost colour there.** **Restart the Unity Editor first.** The pink well-shaped object seen at the ridge was a leftover ghost from Claude Code's own earlier test (an editor-only leak, diagnosed 2026-10-24), not a game bug. After the restart the ridge should have no pink object, and the live ghost should look clean green. Say what you see.
3. Walk the ghost slowly from the ridge toward the creek. The depth number falls as you go downhill and the "Nearby: the creek ... m" distance shrinks.
4. Aim at a spot **between 25 m and 40 m from the creek's edge** (about 30 m). The ghost turns **amber**, and the HUD adds "Floods can foul this well here: the creek is about 31 m away and a flash flood reaches 40 m." plus "Higher ground is safer but deeper."
5. Aim at **low ground within about 25 m of the creek**. The ghost is **red** with "This ground floods." (the same words as before).
6. Aim at a **higher bank within 25 m** of the creek if you can find one. It may stay legal and show amber with the flood warning instead of red.
7. Near the **pond** and the **Spring Hollow**: a legal spot just outside their zones shows a Shallow depth (under 3 m) and the "Higher ground is safer but deeper" hint. Inside the zone stays red. (A "Nearby: pond 0 m" line just outside the zone is known and cosmetic: the zone is a bit bigger than the visible water.)

## B. Existing rules still win

8. Build an **Outhouse**, then aim the well within 30 m of it. The ghost is red with the same setback refusal as before, and "Nearby:" lists the outhouse with a distance when the spot is legal.
9. Aim too far away: the "Too far, come within ..." message still shows.

## C. Nothing else changed

10. Place and cancel an **Outhouse, Wood Shed, Small Cabin site and Kitchen**. Their ghosts and HUD look exactly as before (green or red, no amber, no depth lines).
11. **Click to place a real Well Site**, then dig and line it. The dig takes the same **40 blows**, the lining needs **10 Stone**, and the finished well holds **150 L** and refills at the old rate. (B2 only changes the preview, not the well.)
12. E at the finished well still fills a Bucket or Canteen, or drinks if you have neither equipped, as before.
13. Look at the placement HUD at **1920x1080** and at **1024x768**. The multi-line block stays readable and does not overlap anything.
14. The Console is clean after the session (the "WebSocket is not initialised" MCP message is known and fine; no red errors).

---

**When done:** send me your pass or fail list. If everything passes I will check Claude Code's staging list (9 paths expected), write the commit message, and nothing is committed until you say OK.
