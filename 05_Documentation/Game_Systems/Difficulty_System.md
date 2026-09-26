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

Don't exist yet, at any tier: **tent**, **sleeping bag**, **tarp**, **canteen**, and **hammer**. None of these are in Item_Data.md today. Tent Placement and Sleeping has been sitting as a known future feature this whole project without being built — a difficulty system that starts easier tiers with a tent/sleeping bag would be the reason to finally build that, not a side effect of it. Canteen (a small carried water container, lighter than the Bucket) doesn't exist either. Hammer has never come up outside this request — presumably tied to whatever Building/Housing eventually needs it for, which also has no implementation yet.

**Added 2026-09-26 (Mike):** Cordage and a Tarp belong in the easiest tier's kit too, "as well" as the original list — Cordage already exists as a Material (Trapping_System.md), Tarp is new. Framing: the easiest tier's full kit is meant to feel like someone who planned and packed for a week-long camping trip — Axe, Hammer, Pick Axe, Shovel, Canteen, Bow or Rifle, Cordage, Tarp, Sleeping Bag, Tent, metal cooking pot, and Bucket, all together. Each harder tier takes items away from that full kit rather than the tiers being built up independently — confirms the "take things away the higher the difficulty" direction for the Named Tiers sketch below.

Only exist at the **primitive tier**, not a metal/purchasable tier: Pick Axe and Shovel currently only exist as the Stone Pick Axe (Sticks + Cordage + Stone) and Primitive Shovel (Sticks + Cordage + Stone) built this week. **Confirmed 2026-09-26 (Mike):** new metal-tier versions — a metal Pick Axe and metal Shovel, parallel to the existing metal Axe — are wanted as their own items, separate from the primitive-crafted ones. These need adding to Item_Data.md's Tools table (weight, and Trading Post price once Claude Code proposes the catalog) alongside the tent/sleeping bag/canteen/hammer gap above.

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
