# Homestead
## Difficulty System v1.0

Status: **Built, tested, and committed 2026-09-26 as `a815628` on master (not yet pushed).** Everything below — the New Game picker, all three starting kits, the Trading Post, and Survival Severity — is in Play Mode and confirmed working on a copy of Mike's save. See the new "Built and Tested" section near the bottom for the full report.

---

# Purpose

A New Game difficulty choice that answers a real question the game has been quietly begging since Phase 3: how much does the player start with, and how much has to be earned from the land? Mike's own framing (2026-09-26): many people who dream of homesteading don't have everything they need on day one, and still have to source things from a retail store until they've developed far enough to craft or produce their own — the arc of the whole game is working toward not needing that store at all. Difficulty is the dial that decides how far from that goal a new game begins.

Confirmed 2026-09-26 (Mike): this is a full difficulty axis, not just a starting-kit toggle — it also scales how forgiving the core survival systems are (Hunger/Hydration/Warmth drain, food spoilage speed, wildlife danger, weather severity), and it's presented as a small set of named presets rather than a custom checklist or slider.

---

# The Three Axes

Each named tier is a bundle of settings across three axes. Exact numbers per tier are unconfirmed first-pass proposals throughout this doc — same status as every other balancing pass in this doc set.

## 1. Starting Kit

What the player has in inventory the moment a new game begins. Ranges from a full practical loadout down to effectively nothing.

## 2. Trading Post Access

Whether the player starts with any cash, and how available the new Trading Post (see below) is. This is the axis that answers "where does money come from on the hardest setting" — see Confirmed note below.

## 3. Survival Severity

How forgiving the existing survival systems are: Health_System.md's Hunger/Hydration/Warmth drain rates, Core_Survival_System.md's Spoilage speed, Weather_System.md's storm/cold severity and frequency, and Wildlife_System.md's animal danger/aggression. This doc doesn't touch those systems' own numbers — it just proposes that difficulty apply a multiplier or tier-shift to whatever numbers those docs already define or will define.

**Built and tested 2026-09-26 (Claude Code), not yet committed** — real first-pass multipliers:

| | Hunger/Hydration/Warmth drain | Temperature | Storm odds | Wildlife alertness |
|---|---|---|---|---|
| Homesteader | ×0.75 | +3°C | ×0.6 | ×0.85 |
| Settler | ×1 | — | ×1 | ×1 |
| Pioneer | ×1.3 | −3°C | ×1.5 | ×1.2 |

**Claude Code's own note on Wildlife:** no animal is dangerous yet (Wildlife_System.md has no aggression mechanic built), so this axis scales how easily game hears the player instead — making hunting itself harder on higher tiers rather than making wildlife more dangerous. **Claude Code's own note on Spoilage:** a stored multiplier exists (0.75/1/1.3, matching the drain column) but nothing reads it yet, since Core_Survival_System.md's Spoilage System isn't built — it's wired up and waiting for that system to exist.

---

# The Trading Post (new mechanic — extends Economy_System.md)

**Confirmed 2026-09-26 (Mike):** the store is really a **Trading Post** — a two-way exchange, not a one-way shop. Players sell gathered/produced resources for money, and spend that money on things they can't yet craft or produce themselves. On the hardest tier there's no starting cash at all: the player has to gather resources and sell them at the Trading Post before they can buy anything, which is the intended pressure, not a bug to solve around.

This is the first real mechanic Economy_System.md has ever had — that doc has been pure Design Draft bullets since it was written, with no currency, shop UI, or purchase flow behind Revenue Sources, Economic Sinks, or Barter. The Trading Post is proposed as the thing that actually implements those three sections at once: selling resources there is Revenue Sources made real, buying tools/materials there is Economic Sinks made real (its own Equipment sink already lists "Axes," "Fishing Equipment," "Traps," and "Garden Tools" as purchase examples — this is exactly that, just finally given a mechanic), and a favorable sell-low/buy-high spread on certain goods could cover what Barter was gesturing at.

**Confirmed 2026-09-26 (Mike):** menu-only for now — the Trading Post opens like the Inventory/Craft screen, no physical location, no NPC, no Property_Layout.md changes needed. Matches there being no town anywhere in the current property layout.

**Confirmed 2026-09-26 (Mike):** the actual catalog and prices are Claude Code's first-pass proposal to make, same pattern as the crafting recipes so far (Stone Pick Axe, Primitive Shovel, Pouch/Bag) — Mike confirms by feel once it's built and playtested rather than specifying the list up front.

**Built and tested 2026-09-26 (Claude Code).** The Trading Post is a fourth tab in the Map/Inventory/Journal window, opened with T: the store's stock and Buy buttons on the left, what the player carries and Sell buttons on the right, money and current difficulty tier shown at the top. Shift-click buys 10 at once or sells the whole stack. Money is saved with the game.

**Buy prices (first-pass, Claude Code's own):** Flint and Steel $8, Knife $4, Canteen $5, Bucket $6, Cooking Pot $12, Tarp $8, Sleeping Bag $15, Tent $25, Axe $20, Hammer $10, Fishing Rod $12, Recurve Bow $30, Arrows $1 each, Bolt-Action Rifle $60, Rifle Rounds $1 each, Cordage $2.

**Sell prices:** anything the store stocks sells back at 40% of its buy price, except ammo and Cordage. Hide $8, antlers $6, furs $3, venison $3; other meat/fish $1–3 (more for cooked); foraged plants $1 (morels $3); firewood $1, logs $3, stone $1. **Claude Code's own no-profit-loop fix:** testing caught arrows and rifle rounds buying and selling back at the same $1 each, a free-money loop — the store no longer buys back anything priced under $3. Tested end to end on Pioneer: sold 6 Stone and 3 berries for $9, bought a Flint and Steel with it.

---

# Item Gaps This Surfaces

Mike's described "basic supplies" list — tent, sleeping bag, metal pot for boiling, a bucket, a metal axe, hammer, pick axe, shovel, canteen, bow and arrows or rifle — checked against Item_Data.md's actual current items:

Already real: Bucket, metal cooking pot, recurve bow + arrows, bolt-action rifle + rounds, the generic `axe`.

Tent and Sleeping Bag were already built for Building_Housing_System.md's Sleep System. **Tarp and Canteen built 2026-09-26 (Claude Code), not yet committed** — see Item_Data.md and the "Built and Tested" section below for the real specs.

**Superseded 2026-09-26 (Mike):** Hammer is no longer just a gap — it has a real Design Draft spec now, in Building_Housing_System.md's "Tools: Hammer" section (proposed recipe: 2 Sticks + 1 Cordage + 1 Log), added the same day as Mike's tool-tier request below. Still not buildable in Unity, same reason as before: Building/Housing has zero implementation at any tier for it to attach to. A new **Knife** tool also entered the project the same day (Hunting_System.md's "Tools: Knife" section, for field dressing) — it wasn't part of Mike's original "basic supplies" list here. **Resolved 2026-09-26 (Mike): "smooth it out."** The Knife is now a new-game starting-kit item on every tier, not something removed going up in difficulty like the Design Rules' "one full kit, items removed" pattern — it's cheap enough (1 Stick/1 Cordage/1 Stone) and gates a core system (Field Dressing, Trapping's catch-processing) closely enough that Claude Code's fix was to guarantee it everywhere rather than treat it as a kit-tier variable. F11 grants one to existing saves, same pattern as the Tent/Sleeping Bag/Pouch additions. Built and tested alongside the Primitive Axe (Wood_Gathering_System.md) — **committed and pushed 2026-09-26** (code `67d85da`, docs `b6cf76d`, both on `master`).

**Added 2026-09-26 (Mike):** Cordage and a Tarp belong in the easiest tier's kit too, "as well" as the original list — Cordage already exists as a Material (Trapping_System.md), Tarp is new. Framing: the easiest tier's full kit is meant to feel like someone who planned and packed for a week-long camping trip — Axe, Hammer, Pick Axe, Shovel, Canteen, Bow or Rifle, Cordage, Tarp, Sleeping Bag, Tent, metal cooking pot, and Bucket, all together. Each harder tier takes items away from that full kit rather than the tiers being built up independently — confirms the "take things away the higher the difficulty" direction for the Named Tiers sketch below.

**Superseded 2026-09-26 (Mike):** the paragraph below's generic "metal Pick Axe and metal Shovel, parallel to the existing metal Axe" language is now refined into a real three-tier structure, and the Shovel's own recipe question is fully resolved. Mike confirmed Axe, Pick Axe, and (as of "agreed, keep your shovel recipe; and include metal versions for the future upgrades") the Shovel too are all on the same Primitive → Iron → Steel progression: Primitive Pick Axe (Stone Pick Axe, shipped, commit `4145888`), the new Primitive Axe (Wood_Gathering_System.md, Design Draft), and the shipped Primitive Shovel (Stone, unchanged, commit `4145888`) are each tool's Primitive tier; Iron and Steel tiers above them are confirmed direction but not buildable. **Gap now designed, still not buildable, 2026-09-26:** per Mike's request to research this properly, the new Mining_Metalworking_System.md lays out the full real-world-grounded chain — mining Iron Ore, making Charcoal, smelting a Bloomery, consolidating with the Hammer at an Anvil, then Forge/Anvil/Workbench blacksmithing into Iron or Steel tools (see Wood_Gathering_System.md's and Stone_Gathering_System.md's Tiering notes for the tool-specific detail). The earlier "Board" recipe idea for the Shovel is dropped entirely — it never shipped and Mike opted to keep Stone instead. Original paragraph, now superseded: Pick Axe and Shovel currently only exist as the Stone Pick Axe (Sticks + Cordage + Stone) and Primitive Shovel (Sticks + Cordage + Stone) built this week; new metal-tier versions were wanted as their own items, separate from the primitive-crafted ones. These all still need adding to Item_Data.md's Tools table (weight, and Trading Post price once Claude Code proposes the catalog) alongside the tent/sleeping bag/canteen gap above, once Iron/Steel actually exist as materials.

---

# Named Tiers (refined proposal 2026-09-26, not yet confirmed by Mike)

Three tiers, each the full kit with items removed per the Design Rules' "one full kit, items removed" pattern. The Knife isn't listed per tier — it's guaranteed on every tier regardless of difficulty (confirmed 2026-09-26, see Item Gaps above), the one exception to "removed going up."

**Confirmed 2026-09-26 (Mike):** "i like the 3 tiers" — Homesteader, Settler, and Pioneer stand as proposed, no tiers added or moved. **Also confirmed:** difficulty is locked in at New Game, not adjustable mid-save.

**Built and tested 2026-09-26 (Claude Code), not yet committed.** Real, shipped starting kits, one small deviation from the proposal noted below:

1. **"Homesteader" (easiest).** Tent, Sleeping Bag, Tarp, Cooking Pot, Bucket, Axe, Hammer, Pick Axe, Shovel, Canteen, Flint and Steel, 3 Cordage, Knife, and the player's choice of Bow + 20 arrows or Rifle + 10 rounds — plus $50. Total weight was 23.1 kg with the Tarp at its original 1.5 kg; **the Tarp's weight is now confirmed at 0.8 kg** (see Item_Data.md), which brings this down to about 22.4 kg — not re-verified in Play Mode since the change.

2. **"Settler" (normal, the baseline).** The same kit without the Hammer and the Tarp, $0 cash. An existing save (including Mike's own) loads as Settler with $0 — nothing about it changes, since it's already the game's default behavior.

3. **"Pioneer" (hardest).** Bucket, 3 Cordage, and the guaranteed Knife — $0 cash.

**Gap found 2026-10-07 (Mike, Pioneer playtest, new game from the beginning):** Pioneer's kit has no Axe, and the only source of Sticks and Branches in the docs is felling a tree with one (Wood_Gathering_System.md). So a Pioneer can't get the Sticks to build a campfire or craft the Primitive Axe, and can't buy an Axe either, because the player starts with $0 and nothing sellable until they gather something. This breaks Design Rule 2 below. First-pass fix: loose Sticks and Fallen Branches on the ground, hand pickup, no tool, on every tier — see Wood_Gathering_System.md's "Ground Sticks and Fallen Branches". Stone and Tall Grass density also go up (Stone_Gathering_System.md, Trapping_System.md). A cold-start Warmth fix (the researched Core Temperature Model, which replaced a first "Exertion Warmth" idea) is in Health_System.md. The starting season turned out to be most likely fine (Spring day 1 with snow flurries; Mike, 2026-10-07): see Season_System.md's Pacing section.

**Claude Code's own addition, not on the original list:** Flint and Steel is in the Homesteader and Settler kits (and buyable on Pioneer) — without it, even the easiest tiers couldn't light a fire, so Pioneer has to buy one before its first campfire.

**Note:** the Pick Axe and Shovel granted are the Stone Pick Axe and Primitive Shovel — the only versions that exist until Iron/Steel are buildable (Mining_Metalworking_System.md).

---

# What This Doc Doesn't Decide

Nothing left — the tier count/names/contents question and the New-Game-lock question are confirmed (Named Tiers section), and the Trading Post pricing and Survival Severity multipliers Claude Code was always meant to propose are now built and tested (see those sections above). Mike confirms any of it by feel in Play Mode, same as every other first-pass number in this doc set.

---

# Built and Tested (2026-09-26, Claude Code — not yet committed)

**New Game flow.** After the New Game overwrite confirmation, a picker opens showing Homesteader, Settler, and Pioneer with a description of each, plus a Bow-or-Rifle choice for the two tiers that pack a weapon. "Locked in for this save" is shown on screen. The picker is drawn fully opaque — testing caught the main menu's title showing faintly through it, now fixed. The chosen tier is saved with the game and can't change mid-save. An existing save, including Mike's own, loads as Settler with $0 — the baseline, so nothing about existing saves changes.

**New items.** **Tarp** (carried, not equipped, 0.8 kg): sleeping on just the Sleeping Bag with a Tarp over it blocks 90% of rain and 30% of wind — tested on a heavy-rain night, ending at Warmth 81 with the Tarp versus Warmth 40 (soaked) without it. **Real bug found and fixed in the same test:** Warmth drying stopped completely the moment any rain reached the player at all, so a trickle getting past a Tarp or tree canopy soaked someone overnight regardless of the Tarp; drying now never fully stops. **Canteen**: a 2-litre water container (0.4 kg) — equipping it fills at any water source, the same as the Bucket. **Hammer**: ships as a real, purchasable inventory item (Homesteader kit item, and for sale at the Trading Post) but is still non-functional exactly as agreed — it does nothing until Building/Housing exists to attach it to.

**Debug keys:** F11 now also grants a Canteen (in addition to its existing grants); F12 adds $50.

**Testing, on a copy of Mike's save:** the old save continued unchanged as Settler with $0. A new Pioneer game started with the correct three-item kit, $0, and ×1.3 drain, then sold Stone and berries at the Trading Post and bought a Flint and Steel with the proceeds — tier and money both survived a save and reload. A new Homesteader game with the Rifle had the right kit, $50, and ×0.75 drain; the Canteen filled at a creek and the Tarp kept the player dry overnight. Mike's save was restored and hash-checked afterward; the file is unchanged.

**Committed 2026-09-26 as `a815628` on master, not yet pushed.** Mike picked the Tarp's weight (0.5–1 kg range) after the fact; Claude Code set it to 0.8 kg and committed, but didn't re-run Play Mode after that one-line change, so the new ~22.4 kg Homesteader total isn't independently re-verified. Docs (this file, Hunting_System.md, Trapping_System.md, Wood_Gathering_System.md, Item_Data.md, Current_Task_List.md) are Claude-primary and still need their own commit before both get pushed together.

---

# Design Rules

1. The Trading Post exists on every difficulty tier — what changes between tiers is how much cash you start with and how much you're leaning on it versus the land, never whether it's available at all.
2. Harder is not "unwinnable" — every tier must leave a viable path from nothing to self-sufficient, even if the easiest early move on the hardest tier is just gathering something small enough to sell.
3. Self-sufficiency is the shared destination regardless of tier — difficulty changes how far from it you start, not where the game is trying to take you.
4. **Added 2026-09-26 (Mike):** one full kit, items removed going up in difficulty. The easiest tier is the whole "packed for a week of camping" list — every harder tier is defined as that same list with items taken away, not as separate kits built up independently. Keeps every tier's contents traceable back to one source list instead of drifting out of sync with each other.


**Pioneer fire gap found and resolved 2026-10-07 (Mike, playtest):** Pioneer could gather Sticks and Firewood but had no ignition source (Flint and Steel is $8 at the Trading Post on a $0 start). A craftable Bow Drill (2 Sticks + 1 Cordage, unreliable) is added in Core_Survival_System.md so the hardest tier has a free path to fire. The kit itself is unchanged.