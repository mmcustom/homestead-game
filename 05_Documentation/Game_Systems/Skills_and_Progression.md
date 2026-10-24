# Homestead
## Skills and Progression v0.1

Status: Design Draft (PROPOSED), written 2026-10-24. **Docs only, nothing is built, and nothing should be built until Mike approves it and the Money Phase D2 income measurements are done** (see "Timing"). All numbers are first pass and live in one table so they can be tuned.

# What Mike asked for (2026-10-24, mid-playtest)

Mike wants upgrades for skills and bonuses (more carry weight, fewer dig blows, fewer axe swings or cuts, other minor tweaks) earned by doing an activity a number of times, so the player progresses faster than real time: "once they reach a certain number of times doing something, they get better at it and are rewarded with being able to do it with better efficiency." Claude's recommendation (agreed by Mike): design it now, build it after D2.

# How this fits the existing decisions

- Decisions_Log (2026-09-25) and Crafting_System.md ("Not decided here") defer **mechanics-level skills, stations, durability and recipe discovery** to post-Alpha. This draft proposes lifting that deferral for **gathering efficiency and carry weight only**. It does not add crafting skills, stations, durability or discovery. **If Mike approves, Decisions_Log gets a new entry and the Crafting_System.md line is amended; nothing is changed until then.**
- Core_Survival_System.md has an open question about a skill-based Bow Drill success bonus; this system would be its home.
- Feature_Backlog.md is where deferred items live; the later skills below belong there once approved.
- There is already a work-efficiency value from the survival stats (hunger and thirst slow work). Skills multiply with it, they do not replace it.

# Principles

1. **Skills buy time and stamina, not money.** The Trading Post market already caps what logs, crafted goods and forage earn, so faster chopping gives the player back minutes, not cash. No skill raises a sell price in v1.
2. **Count finished tasks, not swings.** One tree felled, one well dug, one Stone mined counts once. Cancelled or abandoned work counts nothing.
3. **Threshold tiers, not a smooth curve.** Three upgrades per skill. Easy to explain, show, test and save.
4. **Capped bonuses.** About 30 percent less effort at the top tier, never more, so no task becomes trivial.
5. **Reward, not rescue.** The base game must not be made harsh and then fixed by skills. A new player's first hour has to feel fine without them.
6. **Visible.** The player sees progress ("12 of 40 trees") and a message when a tier is reached.
7. **Pure and testable.** The rules live in a pure helper (like `WellWater`) with edit-mode tests; wiring comes later.
8. **Save-safe.** One counter per skill, appended to the save, 0 means untrained, so old saves load exactly as they play today.

# The skills (version 1: four)

| Skill | One use is | What it improves | Today's baseline (from the docs and code) |
|---|---|---|---|
| **Felling** | One tree (not a shrub) felled | Swings needed per tree and stamina per swing | About 10 swings for a broad hardwood, 13 for a tall one (3 for a shrub), about 0.9 s a swing, 4 stamina a swing |
| **Digging** | One well, pit or trench finished (later garden beds) | Blows per dig | Well: 40 blows (Medium tier), 20 Shallow, 70 Deep; each about 0.9 s |
| **Mining** | One Stone broken from the outcrop | Blows per Stone | 4 blows a Stone; the outcrop holds 40 Stone in total |
| **Hauling** | One kilometre walked carrying at least 15 kg | Carry limit and the Encumbered line | Hard cap 45 kg (55 with the Pouch); Encumbered above 30 kg; Stone is 5 kg, a Log 8 kg |

**Later skills (not v1):** Splitting (queued job time), Foraging, Hunting, Fishing, Cooking, Building (Hammer blows), Tanning. Each only joins when its system has something to speed up. See Open question 2.

# Tiers and bonuses (first pass)

Every skill has Untrained plus three tiers: **Novice, Practiced, Skilled**.

| Skill | Novice | Practiced | Skilled | Effect at each tier |
|---|---|---|---|---|
| **Felling** (trees felled) | 5 | 20 | 60 | Swings needed x 0.90 / 0.80 / 0.70 (rounded up, never below 3); stamina per swing 4 becomes 3.6 / 3.2 / 2.8 |
| **Digging** (digs finished) | 2 | 6 | 15 | Blows x 0.90 / 0.80 / 0.70 (rounded up). A Deep well 70 blows becomes 63 / 56 / 49; Medium 40 becomes 36 / 32 / 28 |
| **Mining** (Stone mined) | 5 | 20 | 60 | Blows per Stone 4 becomes 3.6 / 3.2 / 2.8 (progress accumulates, so no rounding games) |
| **Hauling** (km carried) | 3 | 10 | 30 | Hard cap +3 / +6 / +10 kg; the Encumbered line moves by the same amount (30 becomes 33 / 36 / 40) |

**Counts lowered 2026-10-24 (Mike: the first set felt "still a little hard"):** the first draft had Felling 10 / 40 / 120, Digging 3 / 10 / 25, Mining 10 / 40 / 120 and Hauling 5 / 20 / 60; this table is roughly half of that. Mike can still say faster or slower after playing.

**Why the counts are what they are:** a game day is 30 real minutes and year 1 (to the first payment on day 112) is about 56 real hours. The tiers are meant to be reached across year 1 by an active player, with the first upgrade inside the first hour or two for Felling and Digging. These counts are guesses until we measure how often a real player fells, digs and hauls (see "Build plan", step S5).

**The honest numbers.** Felling is already quick: a broad hardwood is about 10 swings, roughly 9 seconds. At Skilled that is 7 swings, about 6 seconds, which saves only about 3 seconds a tree. The real saving is **stamina**: 40 stamina a tree becomes about 20 (7 swings at 2.8). The big time savings are in the long jobs: a Deep well drops from about a minute to about 45 seconds of digging, and mining the whole outcrop (160 blows) drops from about 144 seconds to about 100. That is why Felling gets a stamina bonus and not only fewer swings.

**Hauling and the well.** With the starting Settler kit (20.4 kg) a player can carry 4 Stone a trip. At Skilled Hauling (55 kg cap) that becomes 6. A Deep well's 16 Stone (80 kg) then takes 3 trips instead of 4, and the Pouch stack (see Open question 5) shortens it further. This is deliberate: the Deep tier near the cabin can stay a little demanding and progression eases it.

# What counts as a use (rules)

- **Completed only.** The counter goes up when the task finishes: the tree falls, the well is lined and finished, the Stone breaks off. Stopping halfway counts nothing.
- **Trees, not shrubs.** Felling a shrub (about 3 swings) does not count; it would be the cheapest way to grind.
- **Digging counts finished digs**, not blows, and each well, pit or trench counts once even if the player dismantles it and rebuilds.
- **Hauling counts distance walked while carrying at least 15 kg**, measured in kilometres. A soft daily cap (first pass: 3 km of hauling counted per game day; the real walking speed and trip lengths were not measured, so the cap is a guess) stops pacing back and forth to farm the skill. See Open question 7.
- **No counting from debug kits.** Items from F11 or F12 count like any items; the debug keys that set counters (below) are separate and for testing only.
- **Skills are per game (per save).** Nothing carries over to a new game.
- **No decay.** A skill never goes down.

# How bonuses combine with the rest of the game

- **Tool tiers** (Primitive, Iron, Steel, already confirmed direction in Wood_Gathering_System.md and Stone_Gathering_System.md) are a separate lever: bought or crafted, not earned. If both apply they multiply, with a floor: **effort never drops below 50 percent of base**, so a Steel tool and a Skilled player cannot make a task nearly free.
- **Hunger and thirst** (the existing work-efficiency value) still slow the work. A skilled player who is starving is still slow.
- **Difficulty tiers** (Pioneer, Settler, Homesteader): the same counts and bonuses on every tier in v1. A faster gain rate on easier tiers, or a head start on Homesteader, is Open question 8.
- **Money:** no direct effect. The income measuring sessions are done with no skills, which is correct: skills change time and stamina, not the cash a market lets a player earn. After skills exist, income sessions should be repeated once to check that the extra free time does not turn into a money loop (for example more stamina leading to far more logs sold; the market cap should hold, but check).

# What the player sees

- **A Skills section in the existing Map, Inventory and Journal window** (a new tab, or a block at the top of the Journal; Open question 9). One row per skill: name, tier, a progress bar and the count toward the next tier ("Felling: Novice, 14 of 40 trees"), and the current bonus in plain words ("Chopping takes 10 percent fewer swings and costs less stamina").
- **A message when a tier is reached:** a short banner such as "Felling: Practiced. Chopping now takes 20 percent fewer swings." through the same banner path as other notices. Also written to the Journal.
- **No new world UI.** The swing counter is already visible on the existing progress bar; the bar simply fills faster.

# Save plan

- A small `SkillsState` appended to the save data: three ints (`treesFelled`, `digsFinished`, `stoneMined`) and a float (`kmHauled`), plus `haulKmToday` and the day it belongs to for the soft cap. JsonUtility treats a missing field as 0, so **an old save loads with every skill Untrained and plays exactly as today.**
- **Only the counts are saved, never the tier.** The tier is computed from the count against `SkillRules.Defaults`, so thresholds and bonuses can be retuned after playtests without migrating any save.
- Edit-mode test: an old save loads with the new fields at 0 and the same behaviour (the same pattern as `AnOldPileSaveLoadsWithTheNewFieldsAtZero`).
- Debug keys, for testing only, in the style of the cash keys: one to add to every counter (or set a chosen tier), one to zero them. Exact keys are Claude Code's call and must not clash with F11, F12, Insert, End, PageUp or Shift+PageUp.

# Build plan (proposed, in order; each step can stop)

Nothing starts until Mike approves this doc **and D2 is done.**

1. **S0: this doc and Mike's decisions** (the open questions below), then a Decisions_Log entry lifting the "no skills" deferral for gathering and carry only, and an amendment to Crafting_System.md.
2. **S1: pure helper.** `SkillRules` and a `Skill` struct in a pure assembly (the same pattern as `WellWater`): tiers from a count, an effort factor, a stamina factor, a carry bonus, the 50 percent floor, the daily haul cap. One tuning table. Edit-mode tests for every boundary and the floor.
3. **S2: counters and save.** Count completed tasks, append the save fields, test an old save. Add a read-only readout and the debug keys. **No bonus is applied yet**, so the game plays exactly as today.
4. **S3: wire Felling, Digging and Mining.** Swings, stamina, blows (including `WellWater.digBlows`, multiplied by the factor) and blows per Stone. A tier-up banner. Playtest checklist.
5. **S4: Hauling and the Skills tab.** Touches `InventoryManager` (the 45 kg cap and the 30 kg Encumbered line), `CarryRules` and the felled-tree take screen messages (the limit shown there must follow the bonus). Playtest checklist.
6. **S5: measure and tune.** Log the counters in the ledger CSV or the measuring log, and see how many trees, digs, Stone and kilometres a real player reaches in year 1. Retune the counts. Repeat one income session.

# Risks

- **Grind and exploits.** Mitigated by counting finished tasks only, ignoring shrubs and capping hauling per day. Still the first thing to watch in playtests.
- **Skills feel invisible.** Felling saves only about 3 seconds a tree, so the stamina saving and the banner matter; if players do not notice the bonus, the system is not doing its job.
- **Stacking with tool tiers** could make late tasks too easy. The 50 percent floor is the guard; revisit when Iron and Steel exist.
- **Touching carry weight** affects the felled-tree screen, drop piles, storage and the Stone trips. That is why Hauling is the last step.
- **Early game balance.** Skills must not be the thing that makes year 1 bearable. D2 should set the tier numbers with skills ignored; skills are a bonus on top.
- **More saved state and more tests.** Small (four numbers), but it is a new save field and a new test file.

# Open questions for Mike

1. **Lift the post-Alpha deferral** for gathering efficiency and carry weight only (crafting skills, stations, durability and discovery stay deferred)? Recommended: yes, but build after D2. Mike asked what the deferral is (he did not remember it); it is the Decisions_Log entry of 2026-09-25, explained in chat. **Mike (2026-10-24): "yes, defer that decision."** Read as: the decision to lift the deferral is parked until after D2. Decisions_Log and Crafting_System.md are NOT changed, and this doc stays a parked Design Draft. Revisit after the D2 numbers are set.
2. **Four skills in v1** (Felling, Digging, Mining, Hauling), or fewer, or add Splitting now? Recommended: the four, others later as their systems mature.
3. **Counts: ANSWERED** (Mike, 2026-10-24: "still a little hard"). Lowered to roughly half: Felling 5 / 20 / 60 trees, Digging 2 / 6 / 15 digs, Mining 5 / 20 / 60 Stone, Hauling 3 / 10 / 30 km. Still guesses until measured (step S5).
4. **Bonus size:** 10 / 20 / 30 percent less effort and +3 / +6 / +10 kg carry, with a 50 percent effort floor? Recommended: yes.
5. **Pouch and Hauling stacking: DECIDED yes** (Mike, 2026-10-24). The Pouch (+10 kg) and Skilled Hauling (+10 kg) stack for a 65 kg hard cap.
6. **Felling stamina bonus** (4 becomes 2.8 per swing at Skilled) on top of fewer swings. Recommended: yes, because that is where the felt benefit is.
7. **Daily soft cap on Hauling** (3 km counted per game day) to stop pacing exploits? Recommended: yes. Same idea for other skills (a few finished tasks per day count) is not proposed; ask only if grinding shows up.
8. **Difficulty tiers:** same skill gains on every tier (recommended for v1), or faster on Homesteader, or a head start (Novice in Felling and Digging) for the easiest tier?
9. **Where the player sees it:** a new Skills tab in the Map, Inventory and Journal window (recommended) or a block in the Journal?
10. **Decay:** none (recommended). A skill you do not use should not fade in a homesteading sim that is already about long-term upkeep.
11. **Later skills:** which do you want first (Foraging, Hunting, Fishing, Cooking, Building, Splitting, Tanning)? These would also be where non-effort bonuses (a better chance of a good catch or forage find, never higher prices) could appear.
12. **Measuring:** are you happy for the income sessions to note roughly how many trees you fell, wells you dig and Stone you mine, so S5 can tune the counts?

# Cross-references

- Decisions_Log.md (2026-09-25 deferral, 2026-10-09 Crafting screen) and Crafting_System.md "Not decided here".
- Wood_Gathering_System.md (felling numbers, tool tiers), Stone_Gathering_System.md (mining, outcrop 40 Stone, tool tiers), Inventory_System.md (carry limits 30 and 45 kg), Primitive_Storage_System.md (Pouch).
- Well_Water_Table.md (`WellWater` blows per tier; Hauling and the Deep well's 16 Stone).
- Money_System.md and Income_Measuring_Plan.md (skills must not distort D2 measuring).
- Core_Survival_System.md (work-efficiency value, Bow Drill skill-bonus question).
- Feature_Backlog.md (deferred items).

# Sources and limits

No outside research was done for this draft. The design leans on how other games are generally understood to handle use-based skills (practice raises a skill, effects are capped); that comparison was from general knowledge, not looked up. All baseline numbers come from the project docs and Claude Code's reports (felling swings and stamina and mining blows from Wood_Gathering_System.md and Stone_Gathering_System.md; carry limits from Inventory_System.md; well blows from Well_Water_Table.md), and the time estimates assume about 0.9 seconds a swing or blow.
