
# Pioneer Re-measure Run (Money Phase D2)

UPDATE 2026-11-10: rewritten for the real 365-day calendar (Phase 1) and the Wait control (Phase 2a). The old version used the 28-day seasons and the day 112 payment; both are gone. The calendar and payment numbers below are the current ones.

**Purpose:** the last full-year Pioneer run (2026-10-28 to 10-29, old calendar) earned about $5.58 a game day against the payment, and missed the first payment. Since then the Workshop (Rough Stool, End Table, Chair, Table), pottery and metalwork were built. This run measures what a Pioneer earns NOW, on the final calendar, so the tier numbers (missed payments before a loss, late fee, interest rate, sell share) can be set from facts.

**Not a pass/fail test.** It is a data run. Nothing here can "fail"; I just need the exports and a few notes.

Plan and rules: `Income_Measuring_Plan.md`. Results go into `Measurements\Income_Measuring_Log.md`.

## The numbers to keep in mind

- Pioneer, 5 acres: $60,000 loan, **$573.39 a month**, due on the 1st. That is about **$18.85 per game day**. Early income target is about $25 a day.
- **First payment: Jul 1, Year 1 (day 122)**, then Aug 1, Sep 1 and so on. Grace is 10 days (missed on the 11th). Late fee $75. Three open misses lose the game (a Pioneer who never pays loses Sep 11).
- Seasons: Spring Mar 1 to May 31 (92 days), Summer Jun 1 to Aug 31 (92), Fall Sep 1 to Nov 30 (91), Winter Dec 1 to Feb 28 (90). A game day is still 30 real minutes.
- Day numbers for reference: May 31 is day 91, Jun 30 is day 121, Jul 1 is day 122, Aug 31 is day 183.

## Rules

- New game: **Pioneer, 5 acres, $0 savings**. Any weapon.
- Play like a real player. **No F12, no cash keys, no F11 starting kit, no F8 or F5 day skips**: the days must be played.
- **Skipping time:** use **Wait (Z)** and **sleep**. They run hunger, thirst, fires, wood and torches properly. F7 speed (x10, x60, x360) advances only the clock, so a fire does not burn and you are never pressed to eat while it is on. If you use F7, use **x10 only** and only when nothing is burning and nothing needs feeding, and write down which days used it.
- Wait stops by itself for a low stat, a storm, a rising creek, a dying fire, a trap catch or a notice, so it is safe to use between chores.
- Drop to 1x or Wait in small steps from about **Jun 20** so you do not roll past Jun 30 without the export.
- Build what a new player needs first (shelter, water, fire, food), then earn however you like. Use the Workshop, pottery, Smithy, Maple, Potash and snares as you would in a real game. You do not have to use all of them.
- Do not sell the starting gear back to the Trading Post just to pad income (it counted as income last time and is not repeatable).

## Run steps

1. Start a new game, Pioneer, 5 acres. Note the real clock time you start.
2. **Press End at each of these points** (the HUD shows where the CSV is written):
   - **May 31** (end of Spring, day 91)
   - **Jun 30** (day 121), the day before the first payment. This is the one we missed last time.
   - **Aug 31** (end of Summer, day 183)
3. Pay Jul 1 (and Aug 1) if you can. Note your cash on each due day and whether you could pay in full. After Aug 31, press L to read the Summer statement.
4. **Optional:** keep playing into Fall (End on Nov 30, day 274) and Winter (End on Feb 28, day 364). The required run stops at Aug 31, about 183 game days.

## Please jot down as you go (short is fine)

| What | Why I need it |
|---|---|
| The date your first snares went down, and how many | Fur income is the biggest line; I need catch rate per snare |
| The date the Workshop was built, and which pieces you made | Shows how fast the new income starts |
| The date pottery and the Smithy started, and what you sold from them | Same |
| Whether you used Maple (date started, number of taps) | Maple was never measured properly |
| How you skipped time: Wait, sleep, or F7 (and which days at what speed) | Needed to judge how many chores you could do |
| Anything you ran out of or got stuck on (stone, clay, iron, tools, time) | Tells me what blocks a Pioneer |
| Cash on Jul 1 and Aug 1, and whether you could pay in full | The key result |
| Anything that felt unfair or confusing about money | Design feedback |

## At the end

1. Copy the exports from `C:\Users\mmcus\AppData\LocalLow\MM Custom\Homestead\Exports\` into `06_AI_Collaboration\Measurements\`. There are two files per press (`ledger_...csv` and `ledger_items_...csv`). You do not need to rename them; I will label them.
2. Send me your notes and tell me the run is done. A screenshot of each statement (L key) is a good backup if an export is missed.

## What I do with it

I work out income per game day by season, the coverage against the $18.85-a-day payment, and what sold best. Then I propose the D2 tier numbers (including whether the Pioneer loss threshold is 2 or 3), and you decide. Nothing in the game changes until you approve.

Build for this run: master after the Phase 2a commit (real calendar and Wait control); docs commit follows it.
