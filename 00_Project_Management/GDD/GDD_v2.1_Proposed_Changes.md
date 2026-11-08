# Homestead GDD v2.1: Proposed Changes

**Status:** APPLIED 2026-11-08. Mike approved: C1 retire the Alpha 0.1 lock (agreed), C2 milestones in the roadmap only, C3 vision unchanged but it must state the move from the primitive level to modern systems (added to the Vision Statement and the revision note). The edits are in `Homestead_GDD_v2.0.md` (kept at that filename, revision note marks it v2.1). This file stays as the record of what changed.

**Why:** the GDD was last edited 2026-10-20. Since then the game has grown well past the Alpha 0.1 list, and several statements are now wrong or missing. Source for every change below: the system docs, `Decisions_Log.md` (back-filled 2026-11-08) and `Development_Roadmap.md` (status section of 2026-11-07).

Each item gives where, what is there now, the proposed replacement, and whether it needs Mike's decision. Items marked **DECISION** change direction rather than just correct a fact.

---

## A. Factual corrections (no decision needed)

**A1. Status line.** Now: "Pre-Production". Proposed: "In Development (pre-release)". Add to the Revision Note: "2026-11-08: v2.1, brought in line with the build (see Proposed Changes)."

**A2. Time System.** Now: "Game Day Length: 45-60 Minutes". Proposed: "Game Day Length: 30 real minutes. A season is 28 game days and a year is 112 game days." (Matches `Season_System.md` and the loan timing already in the GDD.)

**A3. Engine line.** Add the version: Unity 6.3 LTS (6000.3.24f1), HDRP.

**A4. Starting equipment.** The GDD says the player starts with "basic tools, camping equipment, limited resources". Add: the starting weapon is fixed by tier (Pioneer: Recurve Bow and 20 arrows; Settler and Homesteader: Bolt-Action Rifle and 10 rounds), decided 2026-10-23.

**A5. Parcels.** The Land Price section says the player chooses among three sizes. Add: in the current build only the 5-acre parcel is selectable; the 10-acre and 40-acre cards show locked ("Needs more income than the early game offers. Unlocks later."), decided 2026-10-25.

**A6. Loan and loss rules.** Already match the build. Add one sentence: the tier table (missed payments before a loss, late fee, interest rate, Trading Post sell share) exists in the code and holds the same values for every tier until Money Phase D2 sets them.

## B. New content to add (describes what is built)

**B1. Difficulty tiers (new short section).** Pioneer (hardest, $0 savings, recurve bow), Settler (20 percent savings, rifle), Homesteader (easiest, 40 percent savings, rifle); savings capped at $30,000. Point to `Difficulty_System.md`.

**B2. Art direction (new short section).** Realistic first-person presentation with physically based materials (decided 2026-11-05). Real art goes in through the art swap point (`Resources/Art/<id>.prefab`); until then everything is primitive placeholders, except the first-person hands. Point to `Art_Needed_List.md`.

**B3. Economy System.** Replace the "Possible income sources" list with what exists, in order of appearance: furs and hides (snares), Ash and Kindling Bundles, Potash and Lye, Maple Syrup, handmade goods from the Workshop (Stool, End Table, Chair, Table), pottery, charcoal and iron goods (Hinges, Hooks, Nails, tools). Keep eggs, milk, wool, honey and the rest under "Later". Add one line on the Trading Post: a simulated market with price drops per group and a sell-surplus control.

**B4. Production chains (new section).** One short paragraph each, with a link to the doc: Ash and Kindling, Maple sugaring, Tripod and Potash, Pottery, Metal (Charcoal Pit, Bloomery, Smithy), Workshop, Wood and Hauling (Wood Saw, Hand Cart, tree regrowth).

**B5. Tools, hands and combat (new short section).** All tools are visible in hand; fists stun small game but never kill; the Knife slashes and stabs; animals cannot hurt the player. Points to `Hands_and_Held_Tools.md` and `Melee_Fists_and_Knife.md`.

**B6. Teaching (extend the Journal System section).** The Journal has a Know-how section: entries unlock as the player meets each production chain, and in-the-moment hints name the next step (decided 2026-10-27 after a playtest showed the Potash chain was undiscoverable).

**B7. Save System (new line).** 10 manual slots plus an autosave; autosave on end of day, sleep, return to menu and quit.

**B8. Stage lists.** Stage 1 and 2 are partly built (Well, Outhouse, Wood Shed, Covered Outdoor Kitchen, Workshop, Potter's Shed, Smithy). Add a one-line "Built so far" under Stage 1 and Stage 2; leave Stage 3 and 4 as designed.

## C. Needs Mike's decision

**C1. DECISION: Alpha 0.1 scope list and Development Rule.** Now: "Nothing beyond this scope should be developed until the survival loop is fun", and the Development Rule blocks Farming, Large livestock operations, Vehicles, Solar arrays and Automation. In practice the loop was passed as fun on 2026-10-11 and a great deal beyond the list has been built, including a Hand Cart (a vehicle in a small sense). Options:
1. **Retire the Alpha 0.1 lock** and replace it with: "Scope is set by the Development Roadmap and Mike's next-step choices. Farming, large livestock operations, powered vehicles, solar arrays and automation remain deferred until Mike chooses them." (Recommended: matches how the project is actually run.)
2. Keep the Alpha list as history under a heading "Original Alpha 0.1 scope (met 2026-10-11)" and still add the replacement rule above.
3. Leave as is (not recommended: a new developer would read it as a rule that the build already breaks).

**C2. DECISION: What is the next milestone called?** The GDD has no milestone after Alpha 0.1. Options: name the current state "Alpha 0.2 (income and economy)" with exit criteria (Money Phase D2 done, early game winnable on every tier), or leave milestones to the roadmap. Mike picks, or says "roadmap only".

**C3. DECISION: Tone of the vision.** The GDD says the goal is "the most authentic homesteading simulator possible while remaining enjoyable". No change proposed; flagged only so Mike can confirm nothing in the vision has shifted since September.

## D. Not changing

The pillars, the loan arc and terms, the account statement, the water, fire, hunger and fatigue sections and the progression stages are unchanged and still match the build.
