# Homestead
## Property Layout v1.0

Status: Implemented (2026-09-24) — built in World.unity by `Assets/Scripts/Editor/PropertyTerrainBuilder.cs`; awaiting Mike's in-game walkthrough (Design Rule 4)

---

# Purpose

World.unity is still a flat 200m placeholder with no real terrain — every one of the five Discovery_Test_Sites.md sites sits on that flat ground waiting to be repositioned, a gap that doc itself flagged from the start. This document proposes the property's physical shape (size, elevation, water, ground cover) so Claude Code has a real target to build in Unity's Terrain tools, the same way Item_Data.md and Discovery_Test_Sites.md gave content specs before their implementation passes.

---

# Core Philosophy

The GDD opens with "an undeveloped parcel of land" — this should read as real, unimproved land a homesteader would realistically choose (water access, some buildable high ground, a mix of forest and open pasture potential), not a curated layout. Terrain also serves the discovery loop directly: not everything should be visible from the cabin, and walking to a site should feel like it earns the discovery.

---

# Property Size and Shape

Proposed scale: roughly 400m to 500m across, larger than the current 200m flat placeholder — Discovery_Test_Sites.md already described Bass Hole as "further out" than the other four sites, which the current 200m footprint doesn't really give room for. Still small enough to cross on foot in a normal play session, not open-world scale.

Boundary: natural rather than a hard wall — forest thickening at the property's edges discourages wandering off without needing an invisible barrier. Real-world acreage conversion isn't needed for Alpha 0.1; this is a Unity-meters scale proposal, and the exact number within this range is Claude Code's call.

---

# Terrain Character

Elevation: gently rolling, not flat and not mountainous. One clear high point — South Ridge — a modest rise in the south-central area, matching the cabin site's existing name and giving it the well-drained soil Discovery_Test_Sites.md already assigned it.

Water: Spring Hollow (north) is a small spring-fed hollow that becomes the head of the property's creek. The creek runs south and west, widening into the larger body of water Discovery_Test_Sites.md implied Bass Hole needs ("a fishing hole implies a larger body of water than the spring itself"). The spring and the fishing hole should read as one connected water system, not two unrelated features — consistent with Water_System.md's Springs-vs-Creeks-vs-Ponds distinctions.

Vegetation: mixed hardwood forest over roughly half to two-thirds of the property — enough for Briar Patch, the deer trail, and future foraging/hunting content to feel earned rather than sparse. Open, pasture-able ground concentrates near the cabin site and along the Old Fence Line Trail (whose name already implies former cleared land), tying into Livestock_System.md's framing of converting undeveloped land into productive pasture.

Ground surface tagging: Claude Code's `GroundSurface` component (Grass/Dirt/Gravel) already drives footstep audio — this terrain pass should texture to match rather than needing a second pass later. Grass as the default majority cover; Dirt along the Old Fence Line Trail and any worn paths between sites; Gravel at the creek and pond banks near Spring Hollow and Bass Hole.

---

# Site Placement

Refines Discovery_Test_Sites.md's relative layout now that a terrain shape is proposed — still not exact coordinates, per that doc's own Design Rule 3.

- **Spring Hollow** — north, in a low hollow where the spring emerges and becomes the creek's head.
- **Old Fence Line Trail** — just south of Spring Hollow, along the tree line bordering the pasture near the cabin.
- **Briar Patch** — east, at the forest edge.
- **Bass Hole** — west or southwest, farther from the cabin than the other four, where the creek widens into the property's main pond or slow water.
- **South Ridge Cabin Site** — central-south, on the high ground, with sightlines toward the pasture and reasonable walking distance to all four other sites.

Exact Transform coordinates, terrain mesh work, and Unity Terrain tool specifics remain Claude Code's implementation call. This section only firms up the shape enough for a ridge, a creek, and a tree line to mean something specific.

---

# Not Covered Here (Future System)

- Fencing or property-boundary markers as a buildable feature.
- Roads or paths as a buildable feature, versus natural terrain paths.
- Multiple properties or property expansion, per Development_Roadmap.md's later Stages.
- Real-world acreage conversion.

---

# Design Rules

1. Terrain should read as real, unimproved land — irregular tree lines and natural water shapes, not symmetry.

2. Every proposed feature ties to an already-confirmed doc or system (Water_System.md's source types, Livestock_System.md's pasture framing, the GroundSurface audio tags) — nothing here invents new mechanics.

3. Exact coordinates, mesh work, and Terrain tool specifics are Claude Code's implementation call — this doc fixes shape and intent, not Transform values.

4. This is a first pass sized to unblock terrain work now, the same role Discovery_Test_Sites.md played for site content — expect revision once Claude Code has real ground built and once Mike has walked it in-game.
