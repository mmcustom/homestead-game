# Homestead
## Winter Preparation Playthrough: Playtest Plan

Status: Proposed 2026-10-10, revised 2026-10-11 with Claude Code's code findings (marked "from the code"). The plan itself is Claude's first pass (not yet confirmed by Mike).

---

# Purpose

The Development Roadmap's next step is a full run toward Winter Preparation, the Year One climax the GDD names. Fire, shelter, food, water, storage, light, crafting and temperature now all exist together, so this run is the first real test of whether the survival loop is fun.

The GDD's Development Rule applies: nothing beyond Alpha 0.1 gets designed or built until this loop has been proven fun by playing it. Fixes the run turns up are the exception.

The run answers two questions: (1) does anything break or block progress, and (2) does Fall preparation make Winter feel tense but manageable, as Gameplay_Loops describes, rather than trivial or impossible?

---

# Setup

- **Build:** master at f3525e5 or later. `ProjectSettings.asset` runInBackground stays a local setting. Unity Console was clean at startup (0 errors, 0 warnings, from Claude Code 2026-10-11).
- **Difficulty:** Pioneer, the hardest start. Kit (from the code): Bucket, 3 Cordage and a Knife; no Axe, no Flint and Steel, no Tarp, no Shovel and $0. Claude's default, not confirmed. A Settler run can come later to compare.
- **Start:** a NEW game from the main menu, not an old save. A new game begins at 06:00 on Spring day 1.
- **Pacing:** a game day is 30 real minutes and each season is 28 days, so Spring through Fall is 84 days, about 42 real hours at normal speed. Winter starts on day 85.
- **Debug keys (from the code; Editor and Development builds, while playing):**
  - F5 skips 1 day. F6 skips to the next season. F7 cycles speed x1, x10, x60, x360 and back to x1 (no key per speed). F8 skips straight to Winter.
  - **Do not press by accident:** F9 refills Hydration, Hunger, Health and Warmth, dries you and cures sickness. F10 drops Hydration, Hunger and Warmth to 20. F11 gives the camping kit and 6 Firewood. F12 gives the hunting and fishing kit and $50. Write down any use.
- **Important (from the code): skips and fast-forward do NOT burn fire fuel, Hunger or other survival timers.** Only real-rate play and sleep do. So F5, F6, F7 and F8 cost the game nothing, and skipping through Fall tests nothing. Play at normal speed, sleep through nights, and use skips only to pass days where nothing needs doing; log each one. Each skipped day writes an auto-save, which is harmless.
- **Keep a run log:** for each play session note the date and time in game, what you did, what went wrong, and any Console errors (copy the first red entry, not only the repeats).

---

# Phase 1: Spring, days 1 to 7 (Water, Fire, Shelter, Food)

The GDD priority order. Goal: survive the first week and learn the basics.

- Find drinkable water and purify it (Water System). Note how long it took.
- **Fire (from the code):** a Pioneer has no way to light a fire at the start. Either build a Bow Drill (2 Sticks + 1 Cordage, unreliable) or sell something at the Trading Post and buy Flint and Steel ($8). Note which you chose and how many attempts it took.
- **Shelter:** with $0 you can buy nothing on day 1 (Tarp $8, Axe $20). Make a Lean-To or other shelter from what you can gather, or sell something first and buy a Tarp for the Tarp Shelter.
- Eat something every day: forage first (Spring: dandelion, wild onion, cattail, morels, dryad's saddle, oyster mushrooms). Raw meat and fish can make you sick (from the code).
- Survive the first cold nights. Note the Warmth readout, when shivering starts, and what restored Warmth fastest.
- Craft the basics (key B): Cordage, Shovel (a craft item, not sold), Primitive Bow and Arrows.

**Questions:** Was it clear what to do on day 1 without the docs? Was anything impossible with the Pioneer kit? Did the first night feel dangerous or just annoying?

---

# Phase 2: Late Spring and Summer, days 8 to 56 (Food supply and the cabin)

Goal: stop living day to day and build reserves.

- Run all five Year One food sources: forage (Summer berries), fishing, fish traps, hunting, trapping. Note which ones actually feed you and which are a waste of time.
- Gather materials for the Small Cabin: logs, stone, clay (Clay Banks, Shovel). Build it in stages with the Hammer and use the site messages to see what is missing.
- Place a Wood Pile, Food Cache and other storage; use the two-way transfer screen. Looking at a Wood Pile shows its contents.
- **Spoilage (from the code): not built.** Items record the day acquired but nothing ages or removes them, so raw and cooked meat never spoil. Note whether that makes food storage feel trivial. There is no "days of food" readout, so keep your own tally of food on hand.
- **Trading Post (from the code):** it buys small furs ($3), deer hide ($8), antlers, meat, fish, forage, firewood, logs and stone, and buys back a crafted Shovel for $4. Sell something, buy something, and note whether prices feel fair and whether income is enough to matter.
- Save and load several times, including inside the cabin.

**Questions:** Does Summer feel like a real preparation season or just waiting? Is there enough to do each day, or too much? Do you ever feel forced to sell everything?

---

# Phase 3: Fall, days 57 to 84 (the Preparation Season)

Season_System calls Fall the season where most winter success is decided. Goal: enter Winter with enough of everything.

- **Firewood, with real numbers (from the code):** every Firewood burns 2 hours (0.5 per hour). A fire holds at most 12 hours of fuel (6 Firewood). A lit fire's prompt reads "Burning (X.X h of fuel)", in hours, not pieces. The cabin hearth is an ordinary campfire with no Winter or cabin modifier and starts unlit and empty. Claude's arithmetic from those rates (not confirmed): a 10-hour Winter night costs about 5 Firewood, a full 24 hours about 12, so the 28-day Winter is roughly 140 Firewood if you only burn at night and up to about 336 if the fire never goes out. Write down your Wood Pile count on day 84.
- **Food:** hunt at dawn and dusk. **Smoking and drying do not exist (from the code)**, so there is no preservation step; note what you do with surplus meat and fish instead (eat, sell, store).
- **Late foraging:** apples, pawpaw and nuts (Fall patches only; spring and summer patches should be gone and greyed on the map).
- **Building:** finish the cabin and hearth before Winter.
- **Trapping:** set traps and deadfalls now, since trapping is the main Winter food source. Winter also slows fishing (bites about 2.5 times longer, chance about 40%).
- **Clothing and warmth:** check what you are wearing against the seasonal clothing values and how Warmth behaves on cold, wet nights.
- **Light:** make sure Torch or Lantern and Lamp Oil are stocked for the long Winter nights (10 hours).

**Questions:** Did Fall feel like a rush against a deadline, or was there nothing pressing? Could you tell what you were short of before Winter started? Was there a clear way to under-prepare?

---

# Phase 4: Winter, day 85 onward (the Annual Survival Test)

Goal: see whether preparation paid off. Play at least 14 game days of Winter at normal speed (skips do not consume fuel or food, so skipping proves nothing).

- Record each morning: Firewood left, food on hand, Warmth, Health.
- Food: does trapping (plus stored food) keep you fed? Note the day you run short of anything.
- **Water (from the code):** surface water does NOT freeze in this build, so there is nothing to observe there. Just note whether water stays easy to reach.
- Fire: how much fuel does the cabin hearth burn per night against the 5 Firewood estimate, and does sleeping beside it keep Warmth up? One night needs a refill or overnight top-up because the hearth holds 12 hours at most.
- Exposure: leave the cabin in a storm or soaked and see how long until it is dangerous, and whether there is a way out of it.
- Try to fail on purpose once: let Hunger or Warmth reach 0 away from the fire (F10 drops Hydration, Hunger and Warmth to 20 as a shortcut; log its use). Player Collapse is designed but NOT built, so Warmth 0 and soaked players can currently fall to a core temperature of 24 C with no rescue. Report what you see; do not count it as a surprise bug.

**Questions:** Was Winter tense but manageable? Did a well-prepared run feel rewarded and a poor one punished? Did anything make you want to quit?

---

# Numbers to Write Down

- Real time and game days for each phase.
- Days to first fire, first shelter, first cabin finished, and how you got the first fire.
- Firewood and food on hand on day 84 and on day 85 plus 14.
- Firewood used per Winter night.
- Count of near-deaths and any debug keys used.

---

# Not Bugs (known and decided)

- Winter has no forage at all (decided 2026-10-10).
- Out-of-season forage patches are hidden and greyed on the map, minimap and journal.
- Spoilage and smoking or drying are not built; food never spoils (from the code).
- No "days of food" readout; the Wood Pile and lit fires do show their contents.
- Surface water does not freeze; Winter only slows fishing (from the code).
- No Player Collapse yet (so no rescue from Warmth 0 or Health 0).
- No hot-side temperature, Heat Waves, livestock, power, Rainwater Collection or Large Cabin.
- Lumber is not sold at the Trading Post yet.
- Trees are placeholder low-poly, and a bare Winter forest still shelters snowfall like a leafy one.
- Fires do not burn during debug skips or fast-forward.
- A Pioneer cannot light a fire until they build a Bow Drill or sell something and buy Flint and Steel.

---

# Design Questions This Run Should Answer (do not build yet)

Claude Code's findings mean two pillars of the GDD's "Preparation Beats Panic" are not in the build: food spoilage and preservation (smoking, drying), and Winter water pressure (freezing). The run should tell Mike whether the loop is already fun without them or whether Fall feels too easy. Decide after the run, per the Development Rule:

- Is Fall tense without spoilage and preservation, or does food stockpiling feel free?
- Does firewood (about 140 to 336 for Winter) create enough pressure by itself?
- Is the Trading Post income enough to buy the first tools, or too slow?

---

# Reporting Back

Send Claude the run log in whatever shape is easiest: a pass or fail per phase, anything that blocked you, anything that felt unfair or boring, and Console text for any red errors. Claude sorts it into Current_Task_List.md (bugs and fixes), the Roadmap (what to design next) and the Decisions Log, and drafts the Claude Code message for anything that needs building.


---

# RESULT: Run 1, 2026-10-11 (Mike)

PASS on all 25 pass/fail checks: Spring week 1 (1-6), Summer and cabin (7-12), Fall prep (13-15), Winter (16-20), Console and stability (21-22), and the three design questions (23-25). Mike: "playtest was fun." No bugs, no red Console errors (the AudioManager NullReferenceException did not return). Design questions answered pass, read by Claude as: the loop is fun and tense enough as built, so spoilage, smoking/drying and Winter water freezing are not needed to call Alpha 0.1 proven. Run numbers (days to first fire, shelter, cabin; Firewood at Winter start) were not recorded. Follow-ups are logged in Current_Task_List.md, Development_Roadmap.md and Decisions_Log.md (2026-10-11).
