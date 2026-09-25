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

Spring Hollow

Water Quality: Excellent

Journal Updated

Map Updated

Updated 2026-09-23 (Claude, matching Claude Code's HUD implementation): the notification uses the site's own name (e.g. "Spring Hollow"), not its generic type ("Natural Spring") — this example originally predated Discovery_Test_Sites.md, back when sites didn't have proper names yet. The site's name is what's in the player's journal, so that's what should be on screen. Updated 2026-09-25 (Claude, matching Claude Code's minimap implementation): step 3 (Minimap Updates) is now live — the confirmation reads "Journal Updated · Map Updated", and the site's marker appears on the minimap at the same moment. Updated 2026-09-25 (Claude): step 4 (World Map) has now been requested by Mike — see the new World Map Integration section below.

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

## Journal Screen (UI)

Confirmed 2026-09-25 (Mike). A dedicated screen, opened and closed with the J key.

Every discovery already creates a journal entry and pops up a "Journal Updated" HUD confirmation, per the Discovery Process above — but there's currently no way to actually open the journal and read what's in it. This screen is that missing piece: browsing `JournalManager`'s entries by the seven sections above (Water Sources, Plants, Wildlife, Fishing, Property Features, History, Notes — exactly `JournalManager.cs`'s existing `JournalSection` enum), newest or oldest first per Claude Code's call. Entries are read-only except the player's own Notes, which can be added and edited directly in the screen, matching `JournalManager`'s existing rule that only player notes can be changed or removed — discovery and milestone entries stay permanent records.

Layout (tabs per section vs. one scrolling list with filters), and whether opening it pauses gameplay, are Claude Code's call, same as the Minimap, compass, and Inventory Screen were left open.

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

## Fog of War

Confirmed 2026-09-25 (Mike). This is separate from, and in addition to, the marker rule above — it governs the minimap's terrain itself, not just the site icons on top of it.

The minimap starts completely blacked out. No terrain, no property features, nothing — the player has explored none of it yet. As the player physically walks the property, the ground they've actually covered is revealed on the minimap (typically as a radius around the player that uncovers permanently, the standard survival/RPG fog-of-war pattern) — everywhere they haven't physically been stays black. This is a second, independent layer on top of the existing marker rule: terrain reveal tracks where the player has walked; a Discovery marker only appears once its specific site has actually been found, even inside already-revealed terrain (walking near a spring without triggering its discovery reveals the ground around it, but not the 💧 Spring icon itself). Reinforces this doc's Design Rule directly — the map should be learned, not revealed, and now that applies to the land's shape as well as what's on it.

Once terrain is revealed it should stay revealed (no re-fogging) — matches the property being a fixed 400-500m homestead the player returns to repeatedly, not something that needs re-scouting. Exact reveal radius, whether it grows with player progression (Knowledge Progression above already implies the player should know more each year), and how the minimap terrain itself is rendered (a live top-down camera render vs. a pre-baked static texture that gets unmasked) are Claude Code's call.

---

# World Map Integration

Confirmed 2026-09-25 (Mike). Step 4 of the Discovery Process above — a full-screen version of the map, opened and closed with the M key.

The World Map shows the whole property at once, rather than the minimap's local view centered on the player. It is not a separate, second thing to build from scratch: it reuses exactly the same fog-of-war terrain reveal and the same discovery markers already tracked for the minimap above, just presented full-screen and zoomed out to fit the whole property instead of a small radius around the player. Terrain the player hasn't physically walked stays just as blacked out here as it does on the minimap, and a site's icon still only appears once that specific site has been discovered — the World Map earns no knowledge the minimap hasn't already earned; it's a bigger window onto the same explored/unexplored state, not a new source of information.

Whether opening the World Map pauses gameplay, exact framing and layout (a simple full-screen version of the minimap's look vs. something more like a field survey map), and whether it shows the player's live position and facing while open are Claude Code's call.

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