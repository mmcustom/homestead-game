# Homestead
## Difficulty System v1.0

Status: Design Draft — brand-new system, first pass, pending Mike's confirmation on the open questions below before anything goes to Claude Code

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

---

# The Trading Post (new mechanic — extends Economy_System.md)

**Confirmed 2026-09-26 (Mike):** the store is really a **Trading Post** — a two-way exchange, not a one-way shop. Players sell gathered/produced resources for money, and spend that money on things they can't yet craft or produce themselves. On the hardest tier there's no starting cash at all: the player has to gather resources and sell them at the Trading Post before they can buy anything, which is the intended pressure, not a bug to solve around.

This is the first real mechanic Economy_System.md has ever had — that doc has been pure Design Draft bullets since it was written, with no currency, shop UI, or purchase flow behind Revenue Sources, Economic Sinks, or Barter. The Trading Post is proposed as the thing that actually implements those three sections at once: selling resources there is Revenue Sources made real, buying tools/materials there is Economic Sinks made real (its own Equipment sink already lists "Axes," "Fishing Equipment," "Traps," and "Garden Tools" as purchase examples — this is exactly that, just finally given a mechanic), and a favorable sell-low/buy-high spread on certain goods could cover what Barter was gesturing at.

**Confirmed 2026-09-26 (Mike):** menu-only for now — the Trading Post opens like the Inventory/Craft screen, no physical location, no NPC, no Property_Layout.md changes needed. Matches there being no town anywhere in the current property layout.

**Confirmed 2026-09-26 (Mike):** the actual catalog and prices are Claude Code's first-pass proposal to make, same pattern as the crafting recipes so far (Stone Pick Axe, Primitive Shovel, Pouch/Bag) — Mike confirms by feel once it's built and playtested rather than specifying the list up front.

---

# Item Gaps This Surfaces

Mike's described "basic supplies" list — tent, sleeping bag, metal pot for boiling, a bucket, a metal axe, hammer, pick axe, shovel, canteen, bow and arrows or rifle — checked against Item_Data.md's actual current items:

Already real: Bucket, metal cooking pot, recurve bow + arrows, bolt-action rifle + rounds, the generic `axe`.

Don't exist yet, at any tier: **tent**, **sleeping bag**, **tarp**, and **canteen**. None of these are in Item_Data.md today. Tent Placement and Sleeping has been sitting as a known future feature this whole project without being built — a difficulty system that starts easier tiers with a tent/sleeping bag would be the reason to finally build that, not a side effect of it. Canteen (a small carried water container, lighter than the Bucket) doesn't exist either.

**Superseded 2026-09-26 (Mike):** Hammer is no longer just a gap — it has a real Design Draft spec now, in Building_Housing_System.md's "Tools: Hammer" section (proposed recipe: 2 Sticks + 1 Cordage + 1 Log), added the same day as Mike's tool-tier request below. Still not buildable in Unity, same reason as before: Building/Housing has zero implementation at any tier for it to attach to. A new **Knife** tool also entered the project the same day (Hunting_System.md's "Tools: Knife" section, for field dressing) — it wasn't part of Mike's original "basic supplies" list here. **Resolved 2026-09-26 (Mike): "smooth it out."** The Knife is now a new-game starting-kit item on every tier, not something removed going up in difficulty like the Design Rules' "one full kit, items removed" pattern — it's cheap enough (1 Stick/1 Cordage/1 Stone) and gates a core system (Field Dressing, Trapping's catch-processing) closely enough that Claude Code's fix was to guarantee it everywhere rather than treat it as a kit-tier variable. F11 grants one to existing saves, same pattern as the Tent/Sleeping Bag/Pouch additions. Built and tested alongside the Primitive Axe (Wood_Gathering_System.md) — **committed 2026-09-26 as `67d85da` on master, not yet pushed.**

**Added 2026-09-26 (Mike):** Cordage and a Tarp belong in the easiest tier's kit too, "as well" as the original list — Cordage already exists as a Material (Trapping_System.md), Tarp is new. Framing: the easiest tier's full kit is meant to feel like someone who planned and packed for a week-long camping trip — Axe, Hammer, Pick Axe, Shovel, Canteen, Bow or Rifle, Cordage, Tarp, Sleeping Bag, Tent, metal cooking pot, and Bucket, all together. Each harder tier takes items away from that full kit rather than the tiers being built up independently — confirms the "take things away the higher the difficulty" direction for the Named Tiers sketch below.

**Superseded 2026-09-26 (Mike):** the paragraph below's generic "metal Pick Axe and metal Shovel, parallel to the existing metal Axe" language is now refined into a real three-tier structure, and the Shovel's own recipe question is fully resolved. Mike confirmed Axe, Pick Axe, and (as of "agreed, keep your shovel recipe; and include metal versions for the future upgrades") the Shovel too are all on the same Primitive → Iron → Steel progression: Primitive Pick Axe (Stone Pick Axe, shipped, commit `4145888`), the new Primitive Axe (Wood_Gathering_System.md, Design Draft), and the shipped Primitive Shovel (Stone, unchanged, commit `4145888`) are each tool's Primitive tier; Iron and Steel tiers above them are confirmed direction but not buildable. **Gap now designed, still not buildable, 2026-09-26:** per Mike's request to research this properly, the new Mining_Metalworking_System.md lays out the full real-world-grounded chain — mining Iron Ore, making Charcoal, smelting a Bloomery, consolidating with the Hammer at an Anvil, then Forge/Anvil/Workbench blacksmithing into Iron or Steel tools (see Wood_Gathering_System.md's and Stone_Gathering_System.md's Tiering notes for the tool-specific detail). The earlier "Board" recipe idea for the Shovel is dropped entirely — it never shipped and Mike opted to keep Stone instead. Original paragraph, now superseded: Pick Axe and Shovel currently only exist as the Stone Pick Axe (Sticks + Cordage + Stone) and Primitive Shovel (Sticks + Cordage + Stone) built this week; new metal-tier versions were wanted as their own items, separate from the primitive-crafted ones. These all still need adding to Item_Data.md's Tools table (weight, and Trading Post price once Claude Code proposes the catalog) alongside the tent/sleeping bag/canteen gap above, once Iron/Steel actually exist as materials.

---

# Named Tiers (first-pass sketch, not locked — count, names, and exact contents are all open)

A rough shape to react to, not a finished spec:

1. **Full "packed for a week of camping" kit, forgiving survival, Trading Post funded from day one.** Everything: Tent, Sleeping Bag, Tarp, metal cooking pot, Bucket, metal Axe, Hammer, metal Pick Axe, metal Shovel, Canteen, Cordage, and Bow or Rifle — plus enough starting cash to smooth over anything missed. Mildest Hunger/Hydration/Warmth drain, slowest spoilage, calmest weather and wildlife.

2. **Partial kit, normal survival, Trading Post open but no cushion.** Some tools present, others (the pick axe/shovel, maybe the rifle) left out to be crafted or bought; little or no starting cash, so the first few in-game days are about generating something sellable. Survival dials at whatever Health/Weather/Wildlife end up defining as their own baseline.

3. **Minimal kit, harsh survival, Trading Post is the only way to get metal tools at all.** Starts with barely more than the clothes on the player's back and maybe the Bucket; everything else — Cordage's raw materials, Stone tools, and any store-bought item — has to be earned. Faster drain, faster spoilage, harsher weather, more dangerous wildlife.

Whether this ends up 3 tiers or 4, and what each is actually called, is entirely open — these three are a starting point for reacting to, not a proposal to lock in.

---

# What This Doc Doesn't Decide

- Exact Trading Post price list or what's for sale — Confirmed 2026-09-26 (Mike): Claude Code's first-pass proposal, same as recipes so far, not something to guess here.
- Exact Survival Severity multipliers — that belongs to Health_System.md/Weather_System.md/Wildlife_System.md once each is ready to define its own baseline numbers difficulty can scale.
- Whether difficulty is locked at New Game or adjustable mid-save.
- Exact tier count, names, and per-tier contents beyond the 3-tier sketch above.

---

# Design Rules

1. The Trading Post exists on every difficulty tier — what changes between tiers is how much cash you start with and how much you're leaning on it versus the land, never whether it's available at all.
2. Harder is not "unwinnable" — every tier must leave a viable path from nothing to self-sufficient, even if the easiest early move on the hardest tier is just gathering something small enough to sell.
3. Self-sufficiency is the shared destination regardless of tier — difficulty changes how far from it you start, not where the game is trying to take you.
4. **Added 2026-09-26 (Mike):** one full kit, items removed going up in difficulty. The easiest tier is the whole "packed for a week of camping" list — every harder tier is defined as that same list with items taken away, not as separate kits built up independently. Keeps every tier's contents traceable back to one source list instead of drifting out of sync with each other.
