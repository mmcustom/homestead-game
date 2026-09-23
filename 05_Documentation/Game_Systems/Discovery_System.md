# Homestead
## Discovery System v1.0

Status: Design Draft

---

# Purpose

The Discovery System is one of the core gameplay systems of Homestead.

Players begin with little knowledge about their property.

Knowledge is gained through exploration and observation.

The Discovery System rewards players for learning the land instead of simply revealing information automatically.

---

# Core Philosophy

Knowledge Is Power.

A player who understands the property should have an advantage over a player who does not.

Examples:

- Knowing where springs are located
- Knowing which berry patches produce well
- Knowing deer travel routes
- Knowing the best fishing locations

Knowledge becomes a resource.

---

# Discovery Categories

## Water Sources

Examples:

- Natural Springs
- Creeks
- Ponds
- Rivers
- Wells

Information Recorded:

- Location
- Water Quality
- Discovery Date

---

## Plants

Examples:

- Blackberry Patches
- Raspberry Patches
- Pawpaw Trees
- Hickory Groves
- Walnut Groves

Information Recorded:

- Harvest Season
- Location
- Notes

---

## Wildlife

Examples:

- Deer Trails
- Rabbit Habitats
- Turkey Roosts
- Waterfowl Areas

Information Recorded:

- Activity Periods
- Season
- Observations

---

## Fishing Locations

Examples:

- Bass Hole
- Catfish Area
- Productive Creek Run

Information Recorded:

- Species Found
- Seasonal Activity
- Notes

---

## Property Features

Examples:

- Suitable Cabin Sites
- Orchard Locations
- Grazing Areas
- Pasture Areas

Information Recorded:

- Buildability
- Soil Conditions
- Water Access

---

# Discovery Process

Players must physically locate resources.

When discovered:

1. Discovery Notification Appears
2. Journal Updates
3. Minimap Updates
4. World Map Updates

Example:

Discovery Unlocked

Natural Spring

Water Quality: Excellent

Journal Updated

Map Updated

---

# Journal Integration

Every discovery creates a journal entry.

Journal Categories:

- Water Sources
- Plants
- Wildlife
- Fishing
- Property Features
- History — permanent milestone entries, one per category's first discovery (e.g. "First Water Source Found") plus other systems' own firsts (e.g. First Successful Deer Harvest). Mirrors this doc's Historical Records section below.
- Notes — player-written notes not tied to any specific discovered site.

History and Notes confirmed 2026-09-23 (Mike), added by JournalManager.cs beyond the five categories above.

The journal becomes the player's field notebook.

---

# Minimap Integration

Resources do not appear automatically.

Map markers are only added after discovery.

Examples:

💧 Spring

🫐 Berry Patch

🦌 Deer Trail

🎣 Fishing Hole

🏠 Cabin Site

---

# Knowledge Progression

The player should know more about the property each year.

Example:

Year 1

- Basic resource locations

Year 3

- Wildlife behavior patterns
- Seasonal harvest locations

Year 5

- Deep understanding of the land

The property becomes increasingly familiar.

---

# Historical Records

Important discoveries remain permanently recorded.

Examples:

- First Spring Found
- First Cabin Site
- First Orchard
- First Successful Deer Harvest

The player's history becomes attached to the land.

---

# Design Rule

The map should be learned, not revealed.

Player knowledge should feel earned.