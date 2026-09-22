# Homestead GDD v2.0 — Senior Game Design Consultant Review

Reviewed against the uploaded `Homestead_GDD_v2.0.md`. Role: identify weaknesses, missing systems, balance concerns, and realism issues; suggest additions that support the existing vision; avoid feature creep; respect established design decisions. Focus areas: survival systems, hunting, fishing, trapping, foraging, livestock, economy, seasonal progression.

## Overall assessment

The four core pillars (Water First, Knowledge Is Power, Self-Sufficiency Before Profit, Preparation Beats Panic) are internally consistent and align with the locked Alpha 0.1 scope. Nothing below proposes new pillars or systems outside what "Water, Fire, Shelter, Food, Winter Preparation" already implies — everything is either an inconsistency between systems already named in the GDD, or a gap in a system the GDD commits to but doesn't finish defining.

## 1. Weaknesses

- **Seasons are referenced everywhere but never defined as a system.** "Winter Preparation," "survive the first year," and the Journal's seasonal plant notes all assume a season mechanic, but the GDD never states season count, length (relative to the 45–60 min day), or transition signaling. This is the most load-bearing missing piece — fishing runs, foraging windows, hunting patterns, and water freezing all need a season clock to hang off of.
- **Health and Morale are used as Sleep System recovery outputs before being defined as stats.** Only water-quality-driven illness is named as a health-loss source anywhere in the document. Morale appears exactly once, undefined.
- **Trapping is the weakest-specified of the five Year One food pillars.** Foraging, Fishing, and Hunting each get supporting detail elsewhere (discovery examples, journal categories); Trapping is only a bullet in two lists, despite Economy naming Furs/Hides as early income — presumably trapping's output.

## 2. Missing systems

- **Food spoilage / preservation.** Root Cellar and Smokehouse (Stage 2 infrastructure) and "Preserved Food" (hunger category) have no decay mechanic to make them necessary.
- **Water contamination sources.** The four quality tiers (Excellent/Good/Questionable/Unsafe) have no stated determinants (stagnant vs. flowing, proximity to livestock/outhouse, etc.).
- **Livestock feed loop.** "Animal Feed Reserves" is already named under Preparation Beats Panic, but feed sourcing/consumption for goats/chickens/rabbits isn't described.
- **Economy sink.** Income tiers are fully specified; nothing describes what money is spent on. "Self-Sufficiency Before Profit" needs a temptation to resist to have mechanical teeth.
- **Failure/death state.** What happens when hydration/hunger/health hits zero is unspecified.

## 3. Balance concerns

- **Stay Up Late vs. Sleep Early** needs an explicit penalty curve — it's a primary pacing lever given the Time System's "always more work than time" rule, and is currently undertuned on paper (risk of being either trivial or punishing).
- **Economy "Advanced" tier** includes Honey and Maple Syrup, implying beekeeping/sugaring systems absent from Stage 3/4 infrastructure — a contradiction with the GDD's own anti-feature-creep Development Rule.
- **Fishing vs. Fish Traps** are two separate Alpha 0.1 systems with no stated relationship — if traps passively out-produce active fishing there's no reason to fish; if not, traps are pointless busywork.

## 4. Realism issues

- **Hunting has no wounded-animal/tracking mechanic** — a natural extension of the existing Discovery/deer-trail system that would reinforce the knowledge-based hunting fantasy instead of "walk up, click, done."
- **No animal population or patch-regeneration logic** for hunting, trapping, foraging (Berry Patch, Nut Grove) — ties back to the missing season system and to "the land should feel learned rather than revealed."
- **No ice/freezing effects** on water sources in winter, despite winter being the Year One climax — another downstream consequence of not having a season system defined.

## 5. Suggested additions (completions of existing systems, not new scope)

- Season clock (defined count/length) driving Foraging windows, Fishing runs, Hunting/Trapping activity, water freezing.
- Spoilage timer on raw Meat/Fish/Produce, mitigated by the already-named Smokehouse/Root Cellar.
- Stated water contamination factors for the existing quality tiers.
- Trap-line loop (place, bait, check cadence, catch → fur/hide) feeding Economy's Early tier.
- Explicit numbers for the Sleep/Stay-Up-Late fatigue penalty curve.

## 6–7. Feature creep / respecting existing decisions

Recommend trimming Honey/Maple Syrup from Economy (or giving them a Stage 3/4 home later) as the one place the document has already drifted past its own Development Rule. Predator threats to livestock, fenced pasture/grazing capacity, and tool degradation are real and worth tracking but correctly belong after the survival loop is proven fun, per the existing Development Rule — kept lowest priority for that reason.

## Priority-ordered recommendations

1. Define the Season System (length, count, dependent subsystems) — highest priority, most-referenced-but-undefined concept.
2. Define food spoilage/preservation rules.
3. Define water contamination sources for existing quality tiers.
4. Flesh out Trapping and connect it to Economy's Early tier.
5. Define Health as a stat (loss sources); formalize or remove Morale.
6. Reconcile Economy's Advanced tier with Stage 3/4 infrastructure (cut or place Honey/Maple Syrup).
7. Define an economy sink.
8. Close the livestock feed loop to "Animal Feed Reserves."
9. Specify a failure/death state; clarify exposure/temperature vs. Health.
10. (Post-Alpha) Predator threat to livestock; pasture/grazing capacity.