# Well Depth (B3) Playtest Checklist

**Goal:** prove a Well now digs, lines and fills according to how deep the water is where you placed it, and that old wells and every other structure behave exactly as before.
**Before you start:** restart the Unity Editor, then back up your Saves folder (copy it somewhere outside the project). Play in the editor with the B3 changes uncommitted. Use the F12 full kit for tools and Stone, and your time debug keys to skip hours.
**How to report:** reply with the item number and pass or fail ("1. pass, 4. fail - Deep needed 70 blows"). Add a short note on any fail.

**The numbers to expect (first pass, Mike's Deep option 2):**

| Where | Depth | Dig blows | Stone | Buffer | Refill | Refill in a drought |
|---|---|---|---|---|---|---|
| Low ground near the creek or pond | under 3 m (Shallow) | 20 | 6 | 200 L | 8 L/h | 4 L/h |
| Mid-slope | 3 to 6 m (Medium) | 40 | 10 | 150 L | 6 L/h | 3 L/h |
| Cabin ridge | 6 to 9 m (Deep) | 60 | 14 | 120 L | 4.5 L/h | 4.5 L/h |

---

## A. The preview

1. Enter Well Site placement on the **cabin ridge**, a **mid-slope** spot and **low ground** near the creek or pond (a legal spot). The preview now has two new lines, "Dig: N blows, lining N Stone" and "Supply: N L, refills N L/h (N L/h in a drought)", and they match the table above for each spot.
2. The preview is up to eight lines tall now. At **1920x1080** and at **1024x768** it stays readable and does not overlap anything. (Claude Code did not re-check 1024x768 with the longer text.)

## B. Building a well

3. Place a well on each of the three kinds of ground. The staged-site prompt reads "Line it with Stone: 0 of **14 / 10 / 6**" (Deep, Medium, Shallow).
4. Dig each one. Count the blows: about **60 / 40 / 20**. (Known quirk, not a fail: the count can come out one blow over, 61 or 41, from float rounding. It did that before B3 too.) Deep takes about a minute of real digging, Shallow only about 20 seconds.
5. Carry Stone to each site in trips with R. The site accepts only up to its own amount; try giving it extra and the extra stays in your pack.
6. Finish each well with the Hammer. The finished well's prompt shows its own size, like "x of **120 / 150 / 200** L".

## C. Water

7. Draw a well down (fill Buckets), skip about 10 game hours, and check the refill: about **45 L (Deep), 60 L (Medium), 80 L (Shallow)** from empty.
8. In a Summer drought (if you can get one with the debug keys or time): the Deep well refills faster, about 4.5 L/h, than the Medium well at 3 L/h.
9. Filling a Bucket or Canteen at the well and drinking from it work exactly as before, with the same quality and illness behaviour. The "dry well" message appears when you empty it.

## D. Dismantling and saves

10. Dismantle each well with the Hammer. The prompt shows half of that well's own Stone: **7 (Deep), 5 (Medium), 3 (Shallow)**, plus the Log and Branches. Anything that does not fit in your pack is left on the ground, not lost.
11. Save, go to the Main Menu, press Continue. Each well keeps its depth and numbers, and a half-dug well site keeps its depth and still wants only its own Stone.
12. If you have an older save with a well built before B3, load it. The well behaves exactly as it did (40 blows, 10 Stone, 150 L, 6 L/h). If you have no such save, say so; Claude Code tested this by forcing the depth to 0.

## E. Nothing else changed

13. The Outhouse, Kitchen and other structures ask for the same Stone as before. The Build tab's Well Site help now says the Stone is 6, 10 or 14 depending on depth, and that digging and supply depend on depth.
14. The Console is clean after the session (the "WebSocket is not initialised" MCP message is known and fine; no red errors).

---

**When done:** send me your pass or fail list. If everything passes I will check Claude Code's staging list (13 paths expected), write the commit message, and nothing is committed until you say OK.
