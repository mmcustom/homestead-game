# Homestead
## Building and Housing System v1.0

Status: Design Draft

---

# Purpose

The Building and Housing System provides shelter, storage, infrastructure, and progression.

Housing is one of the most visible indicators of player progress.

A player's home should tell the story of the homestead.

---

# Core Philosophy

Players improve shelter over time.

Progression should feel earned.

Housing progression:

Tent
↓
Lean-To
↓
Small Cabin
↓
Large Cabin
↓
Cottage
↓
Farmhouse

Older structures remain useful.

Buildings are rarely replaced entirely.

---

# Survival Phase

## Tent

Advantages:

- Fast deployment
- Portable
- Low material cost

Disadvantages:

- Poor weather protection
- Low comfort
- Limited storage

---

## Lean-To

Advantages:

- Better weather protection
- Built from local materials

Disadvantages:

- Minimal insulation
- Poor winter protection

**Note added 2026-09-26 (Claude, design):** Wood_Gathering_System.md (Design Draft, requested 2026-09-26, not yet built) gives "local materials" a real, specific source — Branches chopped from trees with the Axe. Future use, tied to this system actually getting a Unity implementation.

---

# Early Homestead

## Small Cabin

Purpose:

First permanent residence.

Requirements:

- Logs
- Lumber
- Stone
- Labor

**Note added 2026-09-26 (Claude, design):** Wood_Gathering_System.md (Design Draft, requested 2026-09-26, not yet built) gives "Logs" a real source — chopped from trees with the Axe. Future use, tied to this system actually getting a Unity implementation.

Benefits:

- Improved rest
- Increased storage
- Better weather protection

---

## Root Cellar

Purpose:

Food storage.

Benefits:

- Reduced spoilage
- Winter food security

---

## Smokehouse

Purpose:

Food preservation.

Benefits:

- Long-term meat storage
- Long-term fish storage

---

# Established Homestead

## Large Cabin

Purpose:

Expanded living space.

Benefits:

- More comfort
- More storage
- Better recovery bonuses

---

## Cottage

Purpose:

Family-sized dwelling.

Benefits:

- High comfort
- Increased indoor storage
- Better quality of life

---

## Barn

Purpose:

Livestock shelter
Feed storage
Equipment storage

Benefits:

- Animal protection
- Winter support

---

# Modern Homestead

## Farmhouse

Purpose:

Primary residence.

Represents a mature homestead.

Benefits:

- Maximum comfort
- Maximum recovery
- Large storage capacity

---

## Workshop

Purpose:

Tool maintenance
Equipment repair
Future crafting systems

---

## Equipment Buildings

Purpose:

Storage of:

- Tractors
- Trailers
- Equipment

---

# Building Categories

## Housing

Examples:

- Tent
- Cabin
- Cottage
- Farmhouse

---

## Storage

Examples:

- Root Cellar
- Storage Shed
- Barn

**Note added 2026-09-26 (Mike, design), scope confirmed 2026-09-26 (Mike):** wants a primitive tier below these — an informal wood pile and rock pile the player can drop excess Logs/Firewood/Sticks/Branches/Stone onto to offload inventory weight before any real storage building exists. Originally scoped as Future/sequenced after Building itself, Mike has since confirmed he wants the primitive versions built now, with real storage buildings (Storage Shed etc.) layered in as an upgrade once Building/Housing gets its first Unity implementation (still Design-Draft-only, no code yet). Full spec — build method, what each pile holds, capacity, persistence — is in Wood_Gathering_System.md's new "Primitive Storage (Wood Pile / Rock Pile)" section, and on Current_Task_List.md as a Needs Claude Code entry. Wood Gathering already established the working precedent this reuses — a felled tree drops its yield into a wood pile that persists in the world and can be drawn from over multiple trips.

---

## Animal Structures

Examples:

- Chicken Coop
- Goat Shelter
- Rabbit Hutch

---

## Utility Structures

Examples:

- Well House
- Windmill
- Solar Shed

---

# Comfort System

Housing affects:

- Sleep quality
- Recovery
- Morale

Ratings:

Tent
= Poor

Lean-To
= Low

Small Cabin
= Good

Large Cabin
= Very Good

Farmhouse
= Excellent

---

# Expansion Philosophy

Buildings should grow naturally over time.

Example:

Small Cabin
↓
Addition
↓
Large Cabin
↓
Cottage

The homestead develops gradually.

---

# Property Storytelling

Past structures remain visible.

Examples:

- Original tent site
- First cabin
- First garden

Players should be able to look back and remember their journey.

---

# Design Rules

1. Housing progression should feel meaningful.

2. Better shelter improves quality of life.

3. Older structures remain useful.

4. Buildings tell the story of the homestead.

5. Structures should support self-sufficiency rather than wealth accumulation.