# Homestead
## Research v1.0

Status: Design Draft

---

# Purpose

Real-world homesteading and survival reference material used to ground Homestead's existing systems in fact, not new game design decisions. Per Current_Task_List.md's confirmed purpose for this folder, this document does not redefine any mechanic — it backs up claims already made in Game_Systems docs with real-world sourcing, and flags anywhere a doc's claim doesn't quite match reality.

Each section below maps to one or more existing system docs and states plainly whether the doc's claim holds up, needs a small caveat, or was already appropriately vague.

---

# Core Philosophy

Homestead's stated goal is realism-grounded survival, not survivalist simulation for its own sake. This document exists so that "realistic" claims already made in design docs (animal activity patterns, trapping mechanics, livestock needs, water safety, foraging seasons) rest on something more than assumption — while leaving all game-balance numbers (yield rates, timers, stat thresholds) as design decisions Claude will not touch here.

---

# Trapping Technique Grounding

Supports: Trapping_System.md, Hunting_System.md

Real-world rabbit snares and box traps work by exploiting an animal's habitual movement rather than active pursuit. Snares are placed directly over established, well-worn rabbit trails or den entrances — rabbits follow the same paths repeatedly and won't avoid a subtle obstruction placed on a route they already trust. The snare loop is sized specifically to the target animal: too small and it won't close in time to catch the animal, too large and the animal simply walks through it. As the animal struggles against the noose, the mechanism self-tightens, which is part of why snares are considered a comparatively humane, low-maintenance trap once set.

This matches Trapping_System.md's existing framing of traps as placement-and-wait tools tied to location choice rather than active player skill at the moment of catch — the real-world mechanic is about where and how the trap is set, not a skill check when the animal arrives. No change needed to the existing design; this is confirmation, not a correction.

---

# Wildlife / Hunting Activity Pattern Grounding

Supports: Wildlife_System.md, Hunting_System.md, Season_System.md

Whitetail deer are crepuscular — most active in roughly the first two hours after sunrise and the last two hours before sunset, moving between bedding and feeding areas during those windows. This directly supports Wildlife_System's existing dawn/dusk activity claims and Hunting_System's early-game weapon design built around those windows.

The rut (roughly late October through mid-November in most temperate regions, which lines up with the game's Fall season) is the one major exception to that pattern: bucks that are normally cautious and nocturnal-leaning begin moving throughout the day while searching for does, making them far more visible and huntable during normal daylight hours than at any other time of year. This is a real-world justification for Hunting_System's and Season_System's framing of Fall as the primary hunting season — the rut isn't just a lore reason to make Fall busy, it's a documented, dramatic real shift in deer daytime visibility that would be reasonable to reflect mechanically (e.g., a temporary daytime activity bonus during a Fall rut window) if Copilot or Mike ever want to model it. That would be a new design decision, not something this document is proposing — flagging it as a real-world hook only.

---

# Livestock Care Grounding

Supports: Livestock_System.md, Core_Survival_System.md (feed/water dependencies)

Real-world basic care for the three animals most likely to fit Homestead's early-game scale:

- **Chickens** — need a secure, predator-proof coop, layer feed plus grit, and constant fresh water; checked morning and evening.
- **Goats** — need a dry, draft-free shelter, continuous hay access plus minerals, and daily feeding/milking; fencing quality matters more than for the other two, since goats are notorious escape artists.
- **Rabbits** — need secure, ventilated hutches, pellet feed plus hay, and water refreshed twice daily; care burden is lower per-animal than goats or chickens.

This supports Livestock_System.md's general framing of livestock as a daily-commitment, feed/water/shelter-dependent system rather than a passive resource. It does not by itself justify any specific numeric value (feed consumption rate, milk yield, etc.) — those remain undecided and belong in the still-unstarted `05_Documentation/Livestock` per-species sheets, not here. One useful real-world data point for whoever drafts those sheets later: rabbits are consistently the lowest-maintenance of the three, which is a reasonable basis for making them the cheapest/easiest livestock entry point if that hasn't already been decided.

---

# Water Sourcing & Purification Grounding

Supports: Water_System.md

Real-world backcountry guidance (NPS) frames safe water sourcing as a two-step process: collect from moving water or the surface layer of a lake rather than stagnant water, then purify by boiling or chemical/filter treatment before drinking — untreated natural water can carry pathogens (giardia, cryptosporidium) regardless of how clean it looks.

This matches Water_System.md's existing structure almost exactly: a Water Quality Levels system tied to source type, and a separate Water Purification step required before safe consumption. The one thing worth flagging for Mike or Copilot: this session's search did not turn up a precise, source-confirmed boiling time/temperature standard (the commonly cited "rolling boil for one minute, three at high altitude" figure appears widely across secondary sources but wasn't confirmed from a primary source in this pass) — if a Purification mechanic ever needs a displayed "boil time," that specific figure should get a second look before being treated as authoritative rather than folded in from this research pass.

---

# Foraging Seasonal Grounding

Supports: Foraging_System.md, Season_System.md

Per a Southeast Ohio seasonal wild-edibles schedule, the general seasonal shape looks like this:

- **Spring** — shoots, greens, and roots: ramps, dandelion greens, nettle, watercress, wild carrot; sap collection early in the season.
- **Summer** — berries and fruit peak (raspberries, mulberries, strawberries), plus herb leaves and early seed heads.
- **Fall** — nuts and hardier fruit take over: walnuts, hickories, acorns, hazelnuts, pawpaws, persimmons, plus late roots like burdock.
- **Winter** — the source used for this pass didn't break out a distinct winter category; real-world foraging in an Ohio-like climate is genuinely sparse in winter outside of stored nuts and a handful of bark/root exceptions, which is consistent with Foraging_System.md treating Winter as a low-yield season rather than needing its own detailed list.

This lines up with Foraging_System's existing seasonal availability framing and Season_System's Fall-as-preparation-season design — real-world foraging genuinely shifts from greens to calorie-dense nuts right as the game's Fall stockpiling push happens, which is good realism support for a design choice already made rather than a new one.

---

# Fish Species Behavior Grounding

Supports: Fishing_System.md, the still-unstarted `05_Documentation/Fishing` per-species sheets

Real-world behavior for the four species Fishing_System.md already names:

- **Bluegill** — smaller-bodied (roughly 5–8 inches, under a pound), stays shallow near shoreline vegetation, and is an aggressive, opportunistic feeder active throughout the day. This matches Fishing_System's existing "Common / Easy to catch" framing closely — no daylight gating needed, it's the reliable, low-skill entry point.
- **Crappie** — noticeably larger-bodied than bluegill (roughly 8–14 inches, up to 2+ lbs), prefers deeper water near submerged structure (brush piles, drop-offs), and hunts small baitfish most actively at dawn, dusk, and night. Crappie are also a schooling spawner: in spring they move into shallow water in large numbers to spawn, becoming dramatically easier to locate and catch than at any other time of year — a real, documented seasonal shift, not just flavor text. This is a real-world hook for Fishing_System's existing "Crappie — Seasonal" tag, similar to how the Deer rut window worked for Large_Game.md: worth modeling mechanically (a Spring shallow-water abundance/catch-rate bonus) if Mike wants it, but that would be a new design decision, not something decided here.
- **Largemouth Bass** — a sight-hunting ambush predator that stalks visible prey and feeds primarily during daylight, holding near cover (logs, weed lines, fallen trees). This supports Fishing_System's existing "Desirable catch" framing — bass respond to active presentation (lure movement) rather than passive scent, which is a real-world reason Bass might not be a strong Fish Trap candidate the way a bait-scavenging species would be.
- **Catfish** — a scent/barbel-based bottom scavenger rather than a sight hunter, comfortable feeding in low light or murky water, and commonly fished at night specifically because of this. Catfish also grow considerably larger than the other three species, which supports Fishing_System's existing "Large food yield" tag. Their bottom-scavenging, scent-driven feeding style is a real-world reason Catfish would be a strong fit for the existing passive Fish Trap method, unlike Bass.

None of the above is a numeric proposal — catch rates, yield amounts, and any seasonal/time-of-day mechanic remain design decisions for the per-species Fishing sheets, same as every other category in this document.

---

# Maple Sap Tapping Grounding

Supports: Maple_Sugaring.md, Season_System.md, Core_Survival_System.md's "Maple syrup production" Future Expansion item

Real maple sap only flows during freeze-thaw cycles — nights reliably below 32°F followed by days that climb back above freezing — which builds the internal pressure that pushes sap out through a tap hole. In most temperate regions this window falls in late Winter into early Spring, roughly a 4–6 week season, not Fall as might be assumed from Fall being the property's other big harvest season. This directly informed Maple_Sugaring.md's timing, keyed to the same freeze-thaw temperature signal WeatherManager already tracks (and the same 32°F threshold the new Precipitation Type by Temperature rule in Weather_System.md uses) rather than a fixed calendar date.

Only mature trees should be tapped — real guidance calls for at least 10–12 inches of trunk diameter (roughly 30–40 years old) before a tree can support a tap without harm, with larger trees able to support a second tap. Sugar maple has the highest sugar content and is the standard tapping species, but other maples (red, black, silver) and even birch can be tapped at a lower sugar content, meaning more sap is needed per unit of syrup.

The real sap-to-syrup ratio is commonly cited around 40:1 by volume for sugar maple (lower-sugar species need more), which is why syrup production is slow and labor/fuel-intensive relative to the sap collected — boiling off that much water takes sustained heat over time, not a quick pass over a fire. Raw sap is also near-water in appearance and spoils quickly if left too long before boiling, unlike the syrup it becomes once properly reduced.

This is real-world grounding for Maple_Sugaring.md's numbers; exact in-game accumulation rate, spoilage window, and sap-to-syrup ratio (a simplified version of 40:1 for playability) remain Claude Code's implementation call, same as every other numeric value in this document.

---

# Design Rules

1. This document supports existing system claims with real-world sourcing. It does not introduce new mechanics, numbers, or species-specific stats — those belong in the system doc itself or in the still-unstarted per-species sheets (`05_Documentation/Livestock`, `Plants`, `Wildlife`, `Fishing`).

2. Where research surfaces a genuine gap or an unconfirmed number (see Water Purification above), it's flagged here rather than silently assumed — Mike or Copilot should treat those flags as open items, not settled facts.

3. This is a living document. When a new system doc makes a realism claim worth checking, add a section here rather than editing the system doc directly.

---

# Sources

- [Rabbit snare methods (survival forum at permies)](https://permies.com/t/175216/Rabbit-snare-methods)
- [The 3 Basic Rabbit Snare Traps — Survival Sullivan](https://www.survivalsullivan.com/top-3-snares-rabbits/)
- [How to Make a Rabbit Trap: Easy Step-by-Step DIY Guide — Battlbox](https://www.battlbox.com/blogs/outdoors/how-to-make-a-rabbit-trap-a-comprehensive-guide-for-outdoor-enthusiasts)
- [Setting A Rabbit Snare - An Easy Rabbit Trap](https://www.trap-anything.com/rabbit-snare.html)
- [HuntWise | Best Time to Hunt Whitetail Deer](https://huntwise.com/field-guide/deer/best-times-to-hunt-whitetail-deer)
- [Understanding Whitetail Deer Behavior During Pre-Rut and Rut Phases — AFM Real Estate](https://www.afmrealestate.com/land-blog/understanding-whitetail-deer-behavior-during-pre-rut-and-rut-phases)
- [Seasonal deer movement: when whitetails are most active — Code of Silence](https://www.codeofsilence.com/blogs/learn/seasonal-deer-movement-understanding-how-deer-adapt-throughout-the-year)
- [Top 7 Best Low-Maintenance Homestead Animals for Beginners — The Grounded Homestead](https://thegroundedhomestead.com/post/best-low-maintenance-homestead-animals-for-beginners)
- [A Guide to Raising Goats For Beginners on a Homestead](https://homesteadingplace.com/raising-goats-for-beginners/)
- [Homestead Animals: Raising Chickens, Ducks, Rabbits & More Naturally](https://homelyhens.com/homestead-animals/)
- [Two Ways to Purify Water — National Park Service](https://www.nps.gov/articles/2wayspurifywater.htm)
- [Tutorial: Methods to purify backcountry water — Andrew Skurka](https://andrewskurka.com/tutorial-how-to-purify-water-backcountry-methods-pros-cons/)
- [Survival Skills: 10 Ways to Purify Water — Outdoor Life](https://www.outdoorlife.com/survival-skills-ways-to-purify-water/)
- [Seasonal Schedule of Edible Wild Plants in Southeastern Ohio — Ohio University](https://www.ohio.edu/cas/plant-biology/research/facilities-laboratories/edible-wild-plants-se-ohio/seasonal)
- [Midwest Foraging Calendar: Year-Round Guide to Wild Edibles](https://homesteadingsuburbia.com/foraging-calendar/)
- [Wild Edible and Medicinal Plant Harvest Calendar for Ohio — Element Bushcraft & Survival](https://elementbushcraft.com/wild-edibles-medicinal-plants-harvest-calendar-ohio/)
- [Crappie vs. Bluegill: Key Differences and Fishing Tips — FishUSA](https://www.fishusa.com/learn/crappie-vs-bluegill/)
- [Balancing Catfish and Bass in Your Pond for Optimal Fishing — Pond King](https://blog.pondking.com/catfish-and-bass-keeping-both-in-the-same-pond)
- [How to Tap and Make Maple Syrup, What Trees Can Be Tapped — Minnesota DNR](https://files.dnr.state.mn.us/destinations/state_parks/maplesyrup_how.pdf)
- [Bulletin #7036, How to Tap Maple Trees and Make Maple Syrup — University of Maine Cooperative Extension](https://extension.umaine.edu/publications/7036e/)
- [Maple Syrup Season: When to Tap Trees, How It's Made & Health Benefits — The Old Farmer's Almanac](https://www.almanac.com/making-maple-syrup-answering-common-questions)
