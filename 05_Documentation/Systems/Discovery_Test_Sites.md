# Homestead
## Discovery Test Sites v1.0

Status: Design Draft — a small temporary test set for Alpha 0.1, not a final property layout

---

# Purpose

DiscoveryManager.cs needs real discoverable sites in the World scene to test against, and none exist yet. This is a deliberately small set — one site per Discovery_System.md category — sized to prove the discovery loop works (approach, discover, log to Journal), not to lay out the finished homestead property.

The World scene is still a flat 200m placeholder with no real terrain, so exact Transform coordinates aren't proposed here — that's Claude Code's call once there's actual ground to place things on. What this document fixes is the content of each site: what it is, what facts it should report, and roughly where it sits relative to the others, grounded in the already-confirmed Plants/Wildlife/Fishing/Water docs.

---

# Site 1: Spring Hollow (Water Sources)

| Field | Value |
|---|---|
| Type | Natural Spring |
| Water Quality | Excellent |
| Notes | Reliable, clean, low maintenance — per Water_System.md's Natural Spring entry, the best water source type in the game. Good anchor point for the rest of the test set since two other sites (Wildlife, Property) reference it. |

---

# Site 2: Briar Patch (Plants)

| Field | Value |
|---|---|
| Type | Blackberry Patch |
| Harvest Season | Summer |
| Notes | Yields 3 Blackberries per harvest per Berries.md; patch regrows the following Summer. Good first-discovery site since Blackberries are one of the earliest, lowest-effort foraging targets in the game. |

---

# Site 3: Old Fence Line Trail (Wildlife)

| Field | Value |
|---|---|
| Type | Deer Trail |
| Activity Periods | Dawn and Dusk normally; all-day during the Fall Rut Window |
| Best Season | Fall, especially the back half (Rut) per Large_Game.md |
| Observations | Blank at discovery — fills in from the player's own logged sightings over time, per Discovery_System.md's Wildlife tracking behavior |
| Notes | South of Spring Hollow — deer trails running near a reliable water source is realistic siting, not just convenient test layout. |

---

# Site 4: Bass Hole (Fishing Locations)

| Field | Value |
|---|---|
| Type | Fishing Hole |
| Species Found | Largemouth Bass |
| Seasonal Activity | Daylight, cover-oriented (fallen timber); slower after cold fronts per Bass.md |
| Notes | Reuses the exact name Weather_System.md already references as a journal-note example ("Bass Hole — Slower After Cold Fronts"), so the two docs stay consistent rather than naming two different bass spots. Best fished with Rod and Reel; poor candidate for a Fish Trap per Fishing_System.md. |

---

# Site 5: South Ridge Cabin Site (Property Features)

| Field | Value |
|---|---|
| Type | Buildable Homesite |
| Buildability | Good |
| Soil Conditions | Well-drained |
| Water Access | Near Spring Hollow |
| Notes | Central to the test set on purpose — a cabin site benefits from being reachable from the water, forage, and hunting sites around it, the same logic a real homesteader would use. |

---

# Placement Guidance

Relative layout only, for Claude Code to translate into real Transform coordinates once the World scene has actual terrain: Spring Hollow roughly north, Briar Patch roughly east, Old Fence Line Trail roughly south (below/near the spring, per its own note), Bass Hole further out — a fishing hole implies a larger body of water than the spring itself — and South Ridge Cabin Site central, within reasonable walking distance of the other four. None of this is a locked layout; it's just enough spatial logic that the five sites don't feel randomly scattered for this first test pass.

---

# Design Rules

1. This is a test set, not the finished homestead — five sites, one per Discovery_System.md category, sized to prove the discovery loop works end to end. The real property layout is a separate, later pass once the World scene has real terrain.

2. Every fact here traces back to an already-confirmed doc (Water_System.md, Berries.md, Large_Game.md, Bass.md, Fishing_System.md, Weather_System.md) — this file doesn't invent new species behavior or water/soil rules, only picks specific instances of already-confirmed facts and gives them a place in the world.

3. Exact coordinates are intentionally not specified — Claude Code places these once real terrain exists, using the relative layout above as a guide, not a requirement.

4. Discoverable content beyond these five sites (more of each category, or new categories) is a later pass — this file exists to unblock DiscoveryManager testing now, not to be the last word on world content.
