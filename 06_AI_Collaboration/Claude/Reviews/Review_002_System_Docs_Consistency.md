# Homestead — System Docs Consistency Check

Reviewed: all 14 files in `05_Documentation/Game_Systems/` (Building_Housing, Core_Survival, Discovery, Economy, Fishing, Foraging, Health, Hunting, Livestock, Season, Trapping, Water, Weather, Wildlife) as of this pass.

---

## Critical — two files are truncated mid-write

**Water_System.md** cuts off mid-heading, with no closing content:

> `# Water Transportation`
> `Early Game`

Everything past that point is missing — the rest of Water Transportation, and (comparing to every other system doc) the Discovery Integration, Season Effects, Economy Integration, and Design Rules sections a doc this size should end with. This is the same kind of gap Hunting_System.md had before this session — worth finishing next.

**Weather_System.md** cuts off mid-bullet, inside the Thunderstorm entry:

> `## Thunderstorm`
> `Effects:`
> `- Dangerous working conditions`
> `- Poor`

Only three weather types are defined (Clear, Cloudy, Light Rain, Heavy Rain, and a half-written Thunderstorm) — no Cold Front, Snow, or Wind entry, even though other docs already reference weather conditions that aren't defined here: Fishing_System.md's Season Effects mentions "Cold Fronts," and Hunting_System.md (just added) references wind direction and heavy weather. The file also ends with no Discovery Integration, Season Effects, or Design Rules section.

This one has a concrete downstream effect: Health_System.md's Exposure System (Mild/Moderate/Severe, triggered by "extreme cold, storms, long-term wet conditions") has nothing to hook into yet, because Weather_System.md never reaches a weather type that would trigger it. Finishing Weather_System.md is what would actually connect those two docs.

---

## Structural risk — Core_Survival_System.md duplicates two dedicated docs instead of summarizing them

Core_Survival_System.md was written first and reads as the foundational reference, which is the right role for it — but for Water and Trapping specifically, it doesn't summarize the dedicated docs, it re-states them almost verbatim (full Water Sources list, full Water Quality Ratings, a full Trapping System section with its own loop diagram and outputs). Health and Morale, by contrast, get a short summary in Core_Survival_System.md with the full detail living only in Health_System.md — that's the pattern the other two sections should probably follow, because the duplication has already drifted out of sync in two places:

- **Pond water quality disagrees between the two docs.** Core_Survival_System.md rates Pond flatly as "Unsafe." Water_System.md rates Ponds "Questionable to Unsafe." Not a big gap, but it's the kind of thing that causes confusion later — which one does an implementer follow?
- **Rivers are missing from Core_Survival_System.md's water source list**, even though both Water_System.md and Discovery_System.md include Rivers as a source.
- **Core_Survival_System.md's Trapping summary lists "Hide" as a trapping output**, but Trapping_System.md's own Harvestable Materials section only lists Meat and Fur for Rabbit and Squirrel — Hide never appears as a trapping product anywhere else. Hide is a Hunting output (Deer Hide, per Economy_System.md's Early Survival Revenue list and the new Hunting_System.md), so this looks like it got attributed to the wrong system when it was summarized into Core_Survival_System.md.

None of this is a design problem — it's a single-source-of-truth problem. Recommend trimming Core_Survival_System.md's Water Sources and Trapping System sections down to short summaries with a pointer to the dedicated doc (the way it already does for Health/Morale), and fixing the Pond rating and the Hide attribution while doing it.

---

## Minor / worth a quick confirm

**Morale is fully defined twice** — once in Core_Survival_System.md, once in Health_System.md — and the two lists of what lowers Morale don't quite match (Core_Survival says "severe weather"; Health_System says "storm damage" and "livestock loss," with no general weather trigger). Same root cause as above: pick one doc as the source of truth for Morale's mechanics and have the other reference it.

**Core_Survival_System.md's status field reads "Approved Design Draft"** while every other doc in the folder says "Design Draft." This is probably intentional — it's the foundational doc — but worth a quick confirm that it wasn't a copy-paste slip.

---

## Resolved since the last review — no action needed, noted for the record

Everything flagged in `Review_001_GDD_v2.md` has been closed:

- Season System now exists and is well-connected to Wildlife, Foraging, Water, and Livestock.
- Health and Morale are now formalized stats with defined loss/recovery sources (see the Morale duplication note above for the one loose end).
- Trapping is fully fleshed out and feeds Economy's Early tier.
- Spoilage System and water contamination sources are both defined.
- Livestock feed loop is closed (Feed, Feed Reserves, Shelter Requirements all present).
- Economy now has defined sinks (Property Development, Livestock, Equipment, future Transportation).
- Honey/Maple Syrup are still listed under Economy's Modern-tier revenue, but Core_Survival_System.md's Future Expansion list now explicitly defers "Beekeeping" and "Maple syrup production" — the earlier contradiction (income listed with no supporting system) is resolved.

---

## Priority order

1. Finish Water_System.md (Water Transportation onward, plus the missing integration/Design Rules sections).
2. Finish Weather_System.md (remaining weather types, plus integration/Design Rules sections) — this also unblocks Health_System.md's Exposure System.
3. Trim Core_Survival_System.md's Water Sources and Trapping System sections to summaries-with-pointers; fix the Pond rating mismatch and the Hide attribution while there.
4. Reconcile the two Morale definitions into one source of truth.
5. Confirm Core_Survival_System.md's "Approved" status is intentional.
