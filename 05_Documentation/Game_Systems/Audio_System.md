# Homestead
## Audio System v1.0

Status: Design Draft, content confirmed — all 22 original Alpha 0.1 files sourced, mapped, and implemented (2026-09-23). sfx_thunder added, sourced, and implemented 2026-09-24 (sliced into five clips — see Sourced Files); not yet confirmed by ear, since Claude Code can't listen.

---

# Purpose

AudioManager.cs exists as an empty template — no audio design and no audio assets exist yet. This document defines what Homestead should sound like and exactly what triggers each sound, so Claude Code has a real spec to hook up rather than an empty script.

This document defines what to play and when. It does not supply the actual audio files. Sourcing or creating those — a licensed library, an asset store pack, field recording, or AI-generated audio — is a separate practical decision, not something a docs pass can produce on its own; see the Sourcing section at the end.

---

# Core Philosophy

Sound should reinforce the same things every other system does: realism over spectacle, and knowledge earned through paying attention. A player should be able to hear weather changing before they see it, learn to recognize an animal by its call the way they'd learn a real deer trail, and hear a tool that sounds like the real tool. Homestead is a quiet game — ambience carries more weight than music.

---

# Mixer Groups

Four groups under one master mixer: Music, Ambient, SFX, UI. This costs nothing to set up now and means an independent-volume Settings menu (not Alpha 0.1 scope, but likely soon after) doesn't need AudioManager rebuilt later to support it.

---

# Alpha 0.1 — Systems That Already Have Code to Hook Into

Everything below ties to a manager or scene element that actually exists right now (TimeManager, WeatherManager, PlayerController, InventoryManager, DiscoveryManager, JournalManager, MainMenu, PauseMenu). Nothing here is blocked on a system that hasn't been built yet.

## Music

| id | Trigger | Notes |
|---|---|---|
| music_main_menu | MainMenu scene | One loop, calm and pastoral — first thing a player hears. |
| music_gameplay_day | Daytime in World | Sparse, ambient-leaning — not a constant score. Low enough in the mix that Ambient carries most of the atmosphere. |
| music_gameplay_night | Nighttime in World | Quieter and sparser than the day track; could even be silence with just a soft pad, given how ambient-heavy this game already is. |

Three tracks is deliberately minimal for Alpha 0.1 — season-variant or tension music is a Future System, not needed to prove the loop.

## Ambient

| id | Trigger | Notes |
|---|---|---|
| ambient_day | Daytime, clear weather | Birds, insects, light wind. Loudness scales gently by season — Summer fullest, Winter sparsest (per Season_System.md's existing seasonal character). |
| ambient_night | Nighttime, clear weather | Crickets, occasional owl, quieter wind. |
| ambient_rain_light | WeatherManager: light Rain | |
| ambient_rain_heavy | WeatherManager: heavy Rain | |
| ambient_wind | WeatherManager: Wind (its weather-type role, per Weather_System.md's confirmed dual-role) | Layers under whatever else is playing rather than replacing it — Wind was confirmed to also be a standing property, so this should scale with wind strength, not just switch on/off. |
| ambient_snow | WeatherManager: Snow | Quiet, muffled — snow should sound like it's dampening the world, not adding to it. |
| ambient_water_proximity | Player near a discovered Water Source site (Spring Hollow, etc.) | Soft trickle/flow, distance-faded. Ties discovery to a lasting sensory reward — you don't just see the marker, the place sounds different once you know it. |

**Confirmed 2026-09-24 (Mike, from playtest):** ambient_rain_light, ambient_rain_heavy, and ambient_snow should get quieter under overhead cover, the same way the falling particles already do — Mike checked in-game and the particles visibly thin out under a tree crown, but the rain/snow sound stayed exactly as loud. `OverheadCover.cs` already computes this per-frame for `Precipitation.cs`; AudioManager should read the same value and pull these ambient layers down under partial cover, quieter still (or off) under full cover. Mike described the target as "a little bit softer," not necessarily matching the particles' exact 30%/0% curve — the precise attenuation amount is Claude Code's call.

| sfx_thunder | WeatherManager: Thunderstorm, timed with a lightning flash per Weather_System.md's Visual Feedback section (2026-09-24) | Implemented 2026-09-24 (Claude Code). `Thunderstorm.wav` turned out to be a 4½-minute storm recording rather than a single one-shot, so AudioManager holds five hand-picked slices cut from it (7s, 45.8s, 92.8s, 198.8s, 232.2s; 8–12s each), chosen by loudness/low-frequency analysis as the clearest thunderclaps standing above the rain bed. Each strike plays a random slice with a fade-out, never the same one twice in a row — more variety than the single-clip plan assumed. Routed to the Ambient mixer group; distant strikes play through a low-pass filter (~600Hz) for the rumble, close strikes unfiltered for the crack. Import changed from 96kHz/uncompressed to Compressed In Memory / Vorbis / 48kHz so slices can seek without loading the whole file. **Not yet confirmed by ear** — Claude Code picked the slices from measurements, not listening; Mike should confirm they actually sound like thunder and not just louder rain. |

## Player SFX

| id | Trigger | Notes |
|---|---|---|
| sfx_footstep_grass / _dirt / _gravel | PlayerController movement, by ground material | Needs a surface-tag lookup if one doesn't exist yet — flag to Claude Code as a small prerequisite. **Bug (2026-09-24, Mike's playtest):** grass footsteps play at almost double the cadence of dirt and gravel. Step rate should be driven by the player's actual movement speed/stride, the same for every surface — only the sound played each step should change with ground material. Likely cause is the three source clips being pre-baked footstep-sequence loops of different lengths/step-spacing (per the Sourced Files table: `footsteps grass loop.wav` / `footsteps dirt loop.wav` / `footsteps gravel loop.mp3`), so looping each at its own native length produces a different apparent cadence per surface instead of one driven by the player. Worth checking whether footsteps are event-triggered per stride (correct) or just looping a clip that already has its own internal step timing (the likely bug). |
| sfx_sprint_breathing | PlayerController: sprinting, Stamina draining | Per First_Person_Controller.md's confirmed Stamina numbers. |
| sfx_encumbered_breathing | InventoryManager: at/above 30 kg | Distinct from sprint breathing — heavier, slower. |
| sfx_item_pickup / sfx_item_drop | InventoryManager: item added/removed | |

## UI / System SFX

| id | Trigger | Notes |
|---|---|---|
| sfx_ui_click / sfx_ui_back | MainMenu, PauseMenu navigation | Standard menu feedback. |
| sfx_discovery_chime | DiscoveryManager.Discovered — pairs with the new HUD popup | A distinct, pleasant "ding," not a generic UI click — Discovery_System.md's Core Philosophy calls this a rewarded moment ("Knowledge Is Power"), the sound should say so too. |
| sfx_journal_updated | Same event, paired with the HUD's "Journal Updated" line | Subtler than the discovery chime — a confirmation, not the reward itself. |
| sfx_milestone | A History "first found" entry (first-of-category or a system's own first, e.g. First Successful Deer Harvest) | Slightly more prominent than the discovery chime — these are rarer, bigger moments. |

---

# Future System — Documented for Later, No Manager Exists Yet

These need real content eventually but don't have anything in the Unity project to hook into right now (no Hunting/Fishing/Trapping/Livestock/Wildlife managers, no interior scenes). Recording them here so the design isn't lost, per this doc set's usual Future System pattern — not asking Claude Code to build any of this yet.

- **Wildlife SFX** — per-species calls (Turkey gobble, Waterfowl calls; Deer and rabbits are mostly silent/skittish per their own docs), triggered near a discovered Wildlife site or on a Hunting_System.md encounter.
- **Livestock SFX** — Goat bleat, Chicken cluck/egg-lay chirp, Rabbit (quiet, occasional thump), looping near pens once Livestock husbandry gameplay actually exists in Unity, not just in the Livestock docs.
- **Tool-specific SFX** — bow draw/release, rifle shot, fishing cast/reel, snare/trap set — these need Hunting_System.md, Fishing_System.md, and Trapping_System.md's mechanics actually implemented first.
- **Indoor/Cabin ambient** — once building/interiors exist (Stage 2+ per Development_Roadmap.md).

---

# Sourcing

The Alpha 0.1 list above is 22 files (counting the 3 footstep surfaces and 2 UI click sounds individually) — small enough to be a realistic first pass. Unlike Item_Data.md and Discovery_Test_Sites.md, this isn't something a docs pass can fully deliver: those were numbers and facts I could specify outright, but actual playable audio needs real files from somewhere. Three practical routes, roughly cheapest to most control:

1. **Royalty-free/CC0 libraries** — freesound.org, Pixabay Audio, or similar. Free, but takes time to find matching-quality clips and confirm license terms per file.
2. **A Unity Asset Store pack** — several "Farm," "Nature Ambience," and "Footsteps/Fantasy SFX" packs exist bundled and pre-mixed for consistency. Costs money but saves the search-and-vet time above.
3. **AI-generated audio** — fastest if Mike wants to try it, but quality and consistency across all 22 files would need spot-checking before committing to the mixer.

This is Mike's call, not a design decision — flagging it rather than picking one, since it involves cost/time tradeoffs I can't weigh for him.

**Resolved (2026-09-23):** Mike sourced all 22 files from free libraries. See "Sourced Files" below for the exact filename-to-id mapping.

**Resolved (2026-09-24):** Mike already had a thunder sound sitting in the repo root's `Xtra Sound files/` folder (`Thunderstorm.wav`) — moved into `Assets/Audio/SFX/Thunderstorm.wav` alongside the other sourced SFX. Only one clip for now, not the 2–3 suggested above; fine as a first pass, more variations can be added later if repetition becomes noticeable. See the Sourced Files table below for the filename-to-id mapping.

## Folder and Naming (2026-09-23)

Unity_Architecture_Plan.md already reserves `Assets/Audio` as the top-level folder for this content. Save sourced files under `01_Unity_Project/Homestead/Assets/Audio/`, in subfolders matching the four mixer groups above: `Music/`, `Ambient/`, `SFX/`, `UI/`. Name each file after its id from the tables above (e.g. `music_main_menu.mp3`, `sfx_discovery_chime.wav`) — that's not required for Unity to import them, but it means Claude Code can wire up triggers by filename instead of having to ask which file is which.

## Search Terms (2026-09-23, for sourcing)

Loop vs. one-shot matters when searching — a loop needs to be seamless at the edit point, a one-shot doesn't. Noted per item.

**Music (loops)** — music_main_menu: "acoustic pastoral folk loop", "calm farm theme instrumental" · music_gameplay_day: "peaceful ambient pad loop", "calm acoustic background loop" · music_gameplay_night: "soft night ambient pad", "quiet minimal loop"

**Ambient (loops)** — ambient_day: "birds chirping forest loop", "daytime nature ambience" · ambient_night: "crickets night ambience loop", "owl night forest ambience" · ambient_rain_light: "light rain loop", "gentle rain ambience" · ambient_rain_heavy: "heavy rain loop", "rainstorm ambience" · ambient_wind: "wind loop layered", "howling wind loop" (look for a set with multiple intensities if possible) · ambient_snow: "snow ambience muffled", "winter quiet wind loop" · ambient_water_proximity: "stream trickle loop", "small creek water loop", "spring water flowing loop"

**Player SFX** — footsteps (loops or short step sequences): "footsteps grass loop" / "footsteps dirt loop" / "footsteps gravel loop" · sfx_sprint_breathing (loop): "heavy breathing loop running", "out of breath loop" · sfx_encumbered_breathing (loop): "heavy labored breathing loop", "strained breathing loop" · sfx_item_pickup (one-shot): "inventory pickup sound", "item pickup click" · sfx_item_drop (one-shot): "item drop thud", "object drop sound"

**UI / System SFX (all one-shots)** — sfx_ui_click: "UI click sound", "menu button click" · sfx_ui_back: "UI back sound", "menu cancel click" · sfx_discovery_chime: "achievement chime", "discovery bell ding", "soft reward chime" · sfx_journal_updated: "soft notification chime", "subtle confirm chime", "page turn sound" · sfx_milestone: "success fanfare short", "achievement unlock sound"

## Sourced Files (2026-09-23)

All 22 files are in place under `Assets/Audio/`, verified by folder listing. Files weren't renamed to match their ids exactly, so this table is the filename-to-id mapping Claude Code needs to wire triggers correctly.

**Music/** — `Main Menu theme.wav` → music_main_menu · `Gameplay Day.wav` → music_gameplay_day · `Gameplay Night.wav` → music_gameplay_night

**Ambient/** — `Day.wav` → ambient_day · `Night.wav` → ambient_night · `Rain, light.wav` → ambient_rain_light · `Rain, heavy.wav` → ambient_rain_heavy · `Wind.flac` → ambient_wind · `Snow.wav` → ambient_snow · `Water.wav` → ambient_water_proximity

**SFX/** — `footsteps grass loop.wav` → sfx_footstep_grass · `footsteps dirt loop.wav` → sfx_footstep_dirt · `footsteps gravel loop.mp3` → sfx_footstep_gravel · `Sprint breathing (2).wav` → sfx_sprint_breathing · `strained breathing loop.wav` → sfx_encumbered_breathing · `inventory pickup sound.wav` → sfx_item_pickup · `object drop sound.wav` → sfx_item_drop

**UI/** — `menu button click.wav` → sfx_ui_click · `menu cancel click.wav` → sfx_ui_back · `discovery bell ding.wav` → sfx_discovery_chime · `page turn sound.wav` → sfx_journal_updated · `success fanfare short.wav` → sfx_milestone

Not part of the 22-file spec: `SFX/footsteps rain.wav`. Mike kept this on purpose as a possible future rain-footstep variant — leave it unwired for now, it isn't tied to any documented trigger.

**Added 2026-09-24:** `SFX/Thunderstorm.wav` → sfx_thunder. Turned out to be a 4½-minute storm recording rather than a single clip — see the Ambient table's sfx_thunder row above for how Claude Code sliced it into five reusable thunderclaps.

**Decided (2026-09-23):** `menu button click.wav` and `menu cancel click.wav` turned out to be byte-for-byte identical. Rather than sourcing a distinct file, Mike confirmed reusing the same sound for both sfx_ui_click and sfx_ui_back — they don't play at the same time, so there's no doubling issue like the Day.wav/Gameplay Day.wav pair below. No replacement needed for this pair; `menu cancel click.wav` can stay as-is (redundant but harmless) or be dropped in favor of pointing both ids at `menu button click.wav` — implementation detail, Claude Code's call.

**Resolved (2026-09-23):** `Ambient/Day.wav` and `Music/Gameplay Day.wav` were also byte-for-byte identical — Mike sourced a new recording and replaced `Ambient/Day.wav`'s content in place (same filename, mapping above still applies). All 22 files are now distinct; nothing left blocking the commit.

---

# Design Rules

1. Ambience carries more weight than music — Homestead is a quiet game, not a scored one. When in doubt, cut a music cue before cutting an ambient layer.

2. Every Alpha 0.1 sound listed above ties to a manager or scene element that already exists in the Unity project. If Claude Code finds a listed trigger doesn't actually exist yet (e.g. no surface-tag system for footsteps), that's a prerequisite to flag back, not something to skip silently.

3. The Future System list exists so this design isn't lost, not as a request to build it now — those wait for their respective managers (Hunting, Fishing, Trapping, Livestock, Wildlife, interiors) to exist first.

4. This is a first pass, not locked — the same balancing-pass status as every other content doc in this set. Expect additions once playtesting reveals silence where a sound would help, or noise where one doesn't.
