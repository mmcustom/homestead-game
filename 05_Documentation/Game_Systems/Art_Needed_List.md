# Homestead
## Art Needed List v0.3

UPDATE 2026-11-07 (v0.3): audited against the current build. Waves 1 to 6 below are unchanged; four new waves (7 to 10), the missing item groups for Wave 4 and a status tracker are in the "Audit and tracker, 2026-11-07" section at the end of this document.

Status: written 2026-11-05 (Mike asked for a proper art list after collecting reference images in the Art folder). Built from Art_Readiness_and_Placeholders.md (every placeholder and swap point in the code) and Mike's 37 reference images. Docs only, no code. v0.2: Mike DECIDED the art style on 2026-11-05: realistic (see "Style choice" below), chosen knowing it is more work. The supplier rules and budgets below were updated to match.

How to use this list: hand it, together with the matching reference images, to whoever makes the art (Mike in Blender, a freelance artist, an asset pack search, or an AI 3D tool). Each row says what the thing is, how big it is, which named anchors the code needs, and which reference image shows the idea. Placeholders in the game stay as they are until a real prefab is dropped at the swap point (Resources/Art/<id>.prefab), so art can arrive one piece at a time and nothing breaks.

---

# Style choice (DECIDED: realistic)

Three common looks, using Mike's own folder as examples:

- Realistic (photo-real): textures look like photographs, shapes are detailed and irregular, real materials (weathered wood, rusted iron). Examples in the folder: the charcoal kiln, workshop and woodshed photos, the hand cart render. Most work per object, needs high-resolution textures and careful lighting. Hardest to keep consistent when assets come from different makers.
- Stylized low-poly (simple, faceted shapes with clean painted colours, little texture detail): the trees and world in the game today look like this, and so does the primitive axe image. Fast to make, forgiving when assets from different sources are mixed, runs well, and reads clearly in first person. Easiest for a one-person project.
- Hand-painted or cartoon: simplified shapes with painted textures and visible outlines or flat colour (like the flat cartoon tool sheet in the folder). Very readable and charming, but it needs a consistent artist and does not match realistic assets at all.

DECISION (Mike, 2026-11-05): realistic. I had recommended stylized low-poly as the cheaper route; Mike chose realistic knowing it is more work. What that means in practice:

- Look: believable proportions and real materials (weathered wood, rusted and forged iron, canvas, stone, clay, leather), photo-based or PBR textures, normal maps for wood grain and stone, visible wear. Lighting comes from HDRP, so materials must be physically based (base colour, normal, mask or metallic and smoothness, optional height).
- Cost: every object needs better modelling and textures than the placeholders, so the full list will take much longer than a stylized one. The order of the waves matters even more: finish what is on screen all day first (hands and tools), then the cabin and main structures.
- Consistency is the main risk. Realistic assets from different sources often clash in lighting, scale, texture sharpness and wear. Rule: pick one texture standard for the whole game (see supplier rules) and have every piece checked in the game, in the same lighting, before it is accepted.
- Where to get it: Mike in Blender with photo-scanned or CC0 PBR texture libraries, a freelance 3D artist for a themed set, licensed asset packs (check the licence allows a sold game), or AI-assisted 3D tools for rough models that a human then cleans up. Mixing sources is fine only if the texture standard and scale rules are followed.
- Mixed look today: the current game world is simple primitives and generated meshes, so a realistic tool in hand against a primitive world will look odd until the structures, trees and terrain catch up. Plan realistic art in waves that still look acceptable together (hands and tools together; cabin with its tools; and so on).
- Reference images: the real photos in the Art folder are now the right kind of reference for shape, material and wear. They are still not usable as textures or models (watermarks, unknown rights). Mike's earlier low-poly examples (the primitive axe image) are no longer the style target.

---

# What every supplier must follow (from the code conventions)

- Scale: 1 Unity unit = 1 metre; real-world sizes for everything. Player 1.8 m tall, eye 1.65 m.
- Pivot at the centre of the footprint on the ground (y = 0). Forward is +Z (the open front or the working side); a prefab's root rotation is zero.
- Delivery: one FBX or GLB per object plus textures, set up as a prefab named by its id, placed at Resources/Art/<id>.prefab. HDRP-compatible PBR materials (Lit shader: base colour, normal, mask map with metallic, occlusion and smoothness). Texture standard (realistic style, subject to adjustment after the first test piece): tools and small items 2048 x 2048, hands 2048 x 2048, structures 2048 to 4096 per material set, texel density about 512 px per metre on things the player touches up close; no baked lighting or shadows in the textures. Triangle budgets for realistic work: hands 15,000, tools 15,000 each, small items 3,000, big structures 60,000, trees 25,000 per tree (with a lower-detail version for distance). The game must still run at the target frame rate, so every budget is checked in the game before a piece is accepted.
- Named anchors: empty child objects with the exact names the code looks for (Anchor_Smoke, Anchor_Mouth, Anchor_Stock, Anchor_Grip_R, Anchor_Grip_L, Anchor_Tip, and others listed per row). Do not rotate or scale anchors. A missing anchor falls back to a built-in position.
- State and effect parts: children with exact names (State_Empty, State_Loaded, State_Firing, State_Done, On_Firing, State_Lit, State_Unlit and so on) that the code switches on and off.
- Roofs and walls that shelter from rain: children named Solid_* (the code gives them colliders). Everything else loses its colliders.
- Tools and hands are drawn on the Viewmodel layer by the code; the author does nothing special. Hanging tools (buckets, pots, traps, Pouch, Canteen) are authored with the grip at the top and the body hanging below along -Y.
- Licence: only original work, or assets with a licence that allows use in a sold game (keep the licence text with the files). Do NOT use watermarked stock images or web photos as textures or as something to trace. See the reference table for which of Mike's images are watermarked.
- The exact anchor and state lists per id live in ArtSwap (Specs and ToolSpecs) and the header comment in ArtSwap.cs. The planned editor tool Homestead/Art/Placeholder List (phase A2) will generate them as a checklist.

---

# Delivery order (priority from Art_Readiness_and_Placeholders.md)

Wave 1: hands and the 30 tools (always on screen). Wave 2: the Small Cabin, shelters and the main structures. Wave 3: workshop and processing stations. Wave 4: world items and item icons. Wave 5: animals. Wave 6: trees, terrain, water, weather and effects. UI icons and map markers alongside wave 4.


---

# Wave 1: first-person hands and the 30 tools (always on screen, highest value)

Hands: one prefab, id hands_player (Resources/Art/hands_player.prefab). Two forearms with sleeves and hands, rigged or posed by code through the named anchors Anchor_Hand_R and Anchor_Hand_L. Keep every renderer on the Viewmodel layer. The code poses the hands (grip, swing, draw, carry, punch), so the art needs open-hand and gripping-hand shapes that survive being moved, not an animation set. Reference: hands.jpg (AI concept sheet, reference only). Real-world size: adult forearm and hand, sleeve to the elbow.

STATUS (2026-11-06, Hands v3 built and imported): done as two prefabs. hands_player is the thumb-up idle hand (empty-handed) and hands_player_grip is the tool-grip hand (fingers wrapped, thumb over them), which PlayerHands shows whenever a tool is held (a missing grip prefab falls back to the idle one). Both are one-piece realistic hands with a plaid sleeve, nails and photo-based skin textures made from Mike's own hand photos (14,560 and 14,696 triangles for the pair). Sources: Art\Blender_Work\hands (hands_player_v3.blend idle, hands_player_grip.blend grip), reference photos and AI 3D hand references in Art\References\Hand_Photos and Hand_3D. Known limits: skin is plain HDRP Lit (subsurface scattering renders the hands black because the viewmodel pass draws after HDRP's SSS pass), no deep palm creases, nails are a flat pink material, the grip hand has no tool-specific variants. The rifle ready pose now aims along the view (Hands v3 fix).

Tools: one prefab each, id tool_<id>. The grip point of every tool sits at Anchor_Grip_R (right hand, required). Two-handed tools also have Anchor_Grip_L. Sizes are real-world overall lengths and are marked "confirm" until Mike or the artist checks the in-game primitive. Hanging items are authored with the grip at the top and the body hanging down along -Y. Realistic style: up to about 15,000 triangles each with 2048 PBR textures (see the supplier rules). Hands and tools are seen from 30 to 60 cm away and fill the screen, so they are where realism shows most; make these first and get them right.

| id | What it is | Approx. real size (m) | Anchors (R = required) | Reference image | Notes |
|---|---|---|---|---|---|
| tool_axe | Felling axe, steel head, hickory handle | 0.9 long (confirm) | Anchor_Grip_R (R), Anchor_Grip_L, Anchor_Tip | primitive axe.png (shape only) | Tip is the cutting edge. |
| tool_primitive_axe | Stone head lashed to a branch handle | 0.7 long (confirm) | Anchor_Grip_R (R), Anchor_Grip_L, Anchor_Tip | primitive axe.png (shape only) | Lashing visible. |
| tool_wood_saw | Hand saw with frame or plain blade | 0.7 long (confirm) | Anchor_Grip_R (R), Anchor_Tip | primitive wood saw.png | Teeth face forward. |
| tool_hammer | Hand hammer, wooden shaft | 0.35 long (confirm) | Anchor_Grip_R (R), Anchor_Tip | primitive hammer.png | Tip is the striking face. |
| tool_shovel | Wooden shovel | 1.1 long (confirm) | Anchor_Grip_R (R), Anchor_Grip_L, Anchor_Tip | none yet | Primitive-age look. |
| tool_iron_shovel | Iron-bladed shovel | 1.1 long (confirm) | Anchor_Grip_R (R), Anchor_Grip_L, Anchor_Tip | none yet | Should read as better than the wooden one. |
| tool_stone_pick_axe | Pick with stone head | 0.8 long (confirm) | Anchor_Grip_R (R), Anchor_Grip_L, Anchor_Tip | primitive pick axe.png | Tip is the point. |
| tool_iron_pick_axe | Pick with forged iron head | 0.9 long (confirm) | Anchor_Grip_R (R), Anchor_Grip_L, Anchor_Tip | primitive pick axe.png | Same silhouette, iron head. |
| tool_fishing_rod | Fishing rod with line | 2.0 long (confirm) | Anchor_Grip_R (R), Anchor_Grip_L, Anchor_Tip, Anchor_Line | none yet | Line leaves from Anchor_Line. |
| tool_cane_pole | Plain cane pole | 2.4 long (confirm) | Anchor_Grip_R (R), Anchor_Grip_L, Anchor_Tip, Anchor_Line | none yet | Simpler than the rod. |
| tool_torch | Pitch torch | 0.7 long (confirm) | Anchor_Grip_R (R), Anchor_Flame; children State_Lit, State_Unlit | primitive torch.png | Flame light comes from the code. |
| tool_lantern | Hand lantern, glass and frame | 0.3 tall (confirm) | Anchor_Grip_R (R), Anchor_Flame; children State_Lit, State_Unlit | none yet | Hanging style, grip at the top bail. |
| tool_primitive_bow | Self bow, straight staff look from behind | 1.5 long (confirm) | Anchor_Grip_L (R), Anchor_Nock, Anchor_Tip | primitive tools.png (flat cartoon, shape only) | Held in the left hand. Anchor_Nock is the string hand, 7 cm below the grip. Lower limb may run off screen. |
| tool_recurve_bow | Recurve bow | 1.3 long (confirm) | Anchor_Grip_L (R), Anchor_Nock, Anchor_Tip | primitive tools.png (shape only) | Same rules as the primitive bow. |
| tool_bolt_action_rifle | Bolt-action rifle | 1.1 long (confirm) | Anchor_Grip_R (R, at the wrist of the stock, about 0.09 behind the receiver), Anchor_Grip_L, Anchor_Muzzle | none yet | Muzzle anchor optional. The placeholder stock is short and bent down 42 degrees so the butt stays out of view (Mike accepted this 2026-11-07 for now). A real model should seat its butt at the right shoulder, out of sight, with the stock on the inside of the right arm; re-tune the Ready pose and RifleGripBack then. |
| tool_bow_drill | Bow drill fire kit | 0.5 long (confirm) | Anchor_Grip_R (R), Anchor_Grip_L, Anchor_Tip | bow drill kit.png | Spindle, bow, hearth board. |
| tool_bucket | Wooden or tin bucket with bail | 0.3 tall (confirm) | Anchor_Grip_R (R) at the top of the bail | primitive bucket.png | Hanging item. |
| tool_sap_bucket | Sap bucket with lid | 0.3 tall (confirm) | Anchor_Grip_R (R) at the top | sap bucket.png | Hanging item. |
| tool_canteen | Canteen on a strap | 0.2 tall (confirm) | Anchor_Grip_R (R) at the top | none yet | Hanging item. |
| tool_spile | Sap spile (tap) | 0.15 long (confirm) | Anchor_Grip_R (R), Anchor_Tip | spile.png | Small; reads best in the carry pose. |
| tool_box_trap | Wooden box trap | 0.5 long (confirm) | Anchor_Grip_R (R) at the top | none yet | Hanging item. |
| tool_fish_trap | Woven fish trap | 0.6 long (confirm) | Anchor_Grip_R (R) at the top | none yet | Hanging item. Sits near the screen bottom edge. |
| tool_rabbit_snare | Wire or cord snare loop | 0.3 (confirm) | Anchor_Grip_R (R) at the top | none yet | Hanging item. |
| tool_knife | Fixed-blade knife | 0.25 long (confirm) | Anchor_Grip_R (R), Anchor_Tip | primitive knife.jpg (watermarked, reference only) | Melee Knife: alternating slash and stab, Anchor_Tip is the point. |
| tool_flint_and_steel | Flint and steel striker | 0.12 (confirm) | Anchor_Grip_R (R), Anchor_Tip | none yet | Tiny; fine detail is wasted. |
| tool_pouch | Leather pouch on a cord | 0.15 tall (confirm) | Anchor_Grip_R (R) at the top | none yet | Hanging item. |
| tool_clay_pot | Clay pot | 0.25 tall (confirm) | Anchor_Grip_R (R) at the top | none yet | Hanging item. Reads as a dark silhouette today. |
| tool_cooking_pot | Iron cooking pot with bail | 0.3 tall (confirm) | Anchor_Grip_R (R) at the top of the bail | cooking tripod.png (pot only) | Hanging item. |
| tool_potash_kettle | Iron kettle | 0.4 tall (confirm) | Anchor_Grip_R (R) at the top | tripod kettle.png | Hanging item. |
| tool_tarp | Folded canvas tarp | 0.5 long (confirm) | Anchor_Grip_R (R), Anchor_Tip | none yet | Carried bundle. |

Not in this list on purpose: Sleeping Bag, Tent and Weather Station are carried-only items with no tool row.

Caution on the table above: anchor lists are a guide written from the design notes. The authoritative anchors per tool are the ToolSpecs rows in ArtSwap.cs (the Placeholder List editor tool in phase A2 will print them). Optional anchors can be left out and the code falls back to a built-in position.

---

# Wave 2: shelter and the main structures

Structure prefabs are the finished building only (staged "Site" plots get no prefab, by decision). Swap point today exists for the Kiln only (structure_kiln); the other kinds get theirs in phases A2 to A4, so art for them can be made now and dropped in once those phases are built. Sizes are the interaction or footprint box from the code (metres, width x height x depth) or "confirm" where the code does not record one. Forward is +Z. Walls and roofs that must shelter from rain are children named Solid_*.

| id | What it is | Size (m) | Anchors and parts | Reference image | Notes |
|---|---|---|---|---|---|
| structure_cabin | Small Cabin: stacked-log walls, thatch roof, stone hearth slab, clay chimney | 4 x 5 footprint (code), height about 3 (confirm) | Anchor_Hearth (where the fire sits), Anchor_Door, Anchor_Chimney; Solid_Roof, Solid_Wall_* | small cabin.jpg, small cabin 2.jpg (AI concept sheets, reference only) | Highest priority structure. Keep door and hearth where the code expects them. |
| structure_tent | A-frame canvas tent | confirm (about 2 x 1.4 x 2.2) | Anchor_Door | none yet | Shelters from rain: Solid_Roof. |
| structure_tarpshelter | Tarp shelter | confirm (about 2.5 x 1.6 x 2.5) | Solid_Roof | none yet | Pairs with tool_tarp. |
| structure_leanto | Branch lean-to | confirm (about 2 x 1.5 x 2) | Solid_Roof | none yet | Open front toward +Z. |
| structure_well | Well: stone ring, cap, water surface | 1.9 x 1.2 x 1.7 | Anchor_Water (surface disc) | none yet | Reference needed. |
| structure_outhouse | Outhouse shack | 1.3 x 2.1 x 1.3 | Anchor_Door | none yet | Reference needed. |
| structure_workshop | Open thatched work shed with bench | shed 2.8 x 2.4 (site), bench 2.0 x 1.1 x 0.7 | Anchor_Bench (work piece and product sit here); Solid_Roof, Solid_Post_*, Solid_Wall | workshop exterior.png, workshop2 exterior.png, workshop interior.png (video-thumbnail badges, reference only) | Open side faces +Z. |
| structure_pottershed | Potter's shed: roofed, shelf for drying and fired pieces | box 2.2 x 1.4 x 0.7, shed 2.4 x 2.0 | Anchor_Shelf slots; Solid_Roof, Solid_Post_* | pottery shop interior 1, 2, 3 | Shelf slot positions are read by the code. |
| structure_kiln | Clay dome kiln (swap point built) | pad 2.4 x 2.4, dome 2.0 x 1.7 | Anchor_Smoke (1.1 m up), Anchor_Mouth (0.95 forward, 0.45 up), Anchor_Stock (x -1.45); State_Empty, State_Loaded, State_Firing, State_Done, On_Firing | charcoal kiln.png (web photo, reference only) | Prefab spec fully documented in Art_Readiness A1. |
| structure_charcoalpit | Earth-covered charcoal mound | 1.7 x 0.9 x 1.7 | Anchor_Smoke; State_Burning, State_Done | none yet | Low priority. |
| structure_bloomery | Clay-and-stone iron bloomery stack | 1.2 x 1.4 x 1.2 | Anchor_Smoke, Anchor_Mouth; On_Smelting | none yet | Reference needed. |
| structure_smithy | Dark sooty smithy shed, hearth with chimney, anvil on stump | box 2.4 x 1.1 x 1.3, shed 3.0 x 2.4 x 2.2 | Anchor_Chimney, Anchor_Mouth, Anchor_Anvil; On_Working, HotPiece; Solid_Roof, Solid_Post_* | none yet | Reference needed. |
| structure_leachhopper | Ash leach hopper: frame and tub | 1.2 x 1.4 x 1.2 | Fill object scaled on Y | ash hopper.png (watermarked, reference only) | Watermark means do not trace or texture from it. |
| structure_kitchen | Roofed kitchen with stone slab hearth and backing wall | about 3.6 square | Anchor_Hearth (KitchenLayout.HearthAt); Solid_Roof (4 posts) | none yet | Hearth uses the Campfire prefab. |
| structure_countertop, structure_grill, structure_slopsink | Kitchen add-ons | 0.88 x 1 x 0.5 | Grill: Anchor_Fire; Sink: Fill water level | none yet | Countertop is the simplest. |

---

# Wave 3: storage, piles and workshop furniture

Count-dependent contents (logs, stones, delivered materials) are the hard part; the plan is to keep the contents as code-drawn primitives and swap only the container, so the container prefab should be empty.

| id | What it is | Size (m) | Anchors and parts | Reference image | Notes |
|---|---|---|---|---|---|
| structure_woodstorage | Wood pile frame or stacking rack | about 1.4 wide (confirm) | Anchor_Stock | woodshed.png (photo, reference only) | Contents stay primitives. |
| structure_rockstorage | Rock pile ring | 1.4 x 0.6 x 1.4 | Anchor_Stock | none yet | Contents stay primitives. |
| structure_waterbarrel, structure_rainbarrel | Barrel | about 0.6 wide x 0.9 tall (confirm) | Fill (water level), Anchor_Top | Barrel.png | Rain barrel has an open top. |
| structure_foodcache | Food cache box | confirm | none | primitive food cache.png | Contents stay primitives. |
| structure_storagebin | Storage bin or trunk | confirm | none | storage trunk.png | Lid optional. |
| structure_toolrack | Tool rack with pegs | confirm | Anchor_Peg_* for hung tools | tool storage rack.png | Low priority. |
| structure_woodshed | Roofed wood shed | confirm | Solid_Roof | woodshed.png | Medium. |
| structure_shelving | Shelving unit | confirm | Anchor_Shelf_* | none yet | Medium. |
| structure_handcart | Hand cart: bed, two wheels, handles | confirm (about 0.8 x 0.7 x 1.6) | Anchor_Handle (E grabs here), Anchor_Load; Wheels as separate children | primitive hand cart.png (faint watermarks, reference only) | Hands pose for the cart is already tuned. |
| structure_tripod | Cooking tripod: three poles, chain | about 1.4 tall (confirm) | Anchor_Hang (vessel hangs here) | cooking tripod.png, tripod campfire.png, tripod kettle.png | Pot or kettle is a separate tool or item prefab. |
| structure_tap | Sap tap on a tree: spile, bucket, sap disc | small | Anchor_Spile; Fill (sap level), State_Sour | spile.png, sap bucket.png | Done 2026-11-15 from tool_spile and tool_sap_bucket (see the structure_tap notes). |
| effect_campfire | Campfire (existing prefab slot, FireManager.campfirePrefab) | about 0.9 across | Anchor_Flame, light, particles | tripod campfire.png | Already prefab-based; needs real fire mesh and ash. |

Note on ids: the structure_<pilekind> ids above follow the ArtSwap convention (lower-case pile kind name). The exact id spelling for each kind is fixed when its Spec row is added in phase A2; the Placeholder List editor tool will then print the final ids and replace this column.

---

# Wave 4: world items, item icons and UI

Items today are one tinted cube sized by weight (DroppedItems.Spawn) and have no icon at all (ItemDefinition has no icon field; Assets/UI is empty). The plan reuses one item_<id> prefab for the dropped object, and later in the hands. About 92 item rows exist in the market list; they do not need art at once.

| id or group | What it is | Approx. size (m) | Notes | Priority |
|---|---|---|---|---|
| item_<id> for logs, branches, firewood, stone, clay, charcoal, ash | Raw materials seen in the world | 0.2 to 0.5 | Do these first; they are dropped and carried constantly. | High |
| item_<id> for iron ore, bars, nails, tools made at the Smithy | Metal goods | 0.1 to 0.4 | After the Smithy exists as art. | Medium |
| item_<id> for hides, pelts, meat, fish, eggs, forage | Food and animal goods | 0.1 to 0.4 | Hides pair with the tanning rack reference (tanning not in code yet). | Medium |
| item_<id> for pottery (pots, jars, bricks) | Fired and unfired clay goods | 0.1 to 0.3 | Dry versus fired colour difference matters. | Medium |
| Item icons (2D) | One icon per item for inventory and market | 128 x 128 px, transparent PNG | Done 2026-11-19 (see Wave 4 item icons below): rendered from the item art, shown in the Inventory rows. | Medium |
| Map icons (2D) | Map markers for structures and places | 64 x 64 px | In Unity 2026-11-20 (ui_map_markers; see its section): the nine discovery markers as 128 px art, used by MapIcons.Marker; the player arrow and the maple droplet and pail stay drawn in code (the HUD tints them). Approved by Mike 2026-11-20. | Low |
| Stump | Tree stump left after felling | trunk radius (confirm) | Pivot at ground, cut face up. | Medium |
| Carcass | Dead animal on the ground | by species | Moved to Wave 5 (Mike, 2026-11-19): there is no separate carcass in code; Animal.Die tips the animal's own model 85 degrees onto its side. Made with each animal. | none |
| Placement ghost | Uses the structure prefabs themselves | as each kind | No separate art. | none |

Tanning rack and tanning tools (references saved: tanning rack.png, tanning tools.png) are future features; no code or ids exist yet, so they are not listed as rows.

---

# Wave 5: animals and traps

Animals are prefabs already (Prefabs/Wildlife/*.prefab, built from capsules and spheres), so these swap through the existing prefab slots, not ArtSwap. Real art needs the shape and a simple rig or at least separate head, body and legs so the code can bob, turn and stun. Stunned and startled states exist for fists (M0e-1), so the animal needs a clear "stunned" pose reading from a single body tilt, not a full animation.

### Animal art conventions (Mike, 2026-11-19: more animals will come later for other regions and climates, so every animal follows one pattern)
- What the code does today: animals are not animated. Animal moves the whole object; it reads only its serialized `model` (tipped 85 degrees on death), `vitals` (heart-lung point for clean kills) and `antlers` (shown on bucks). The prefabs are built by Phase2Builder from primitives.
- One art hook for every species (planned with the deer): Animal loads Resources/Art/animal_<speciesId>. When it exists, the primitive renderers under Model are hidden, the art goes under Model, Vitals moves to the art's `Vitals` empty and the Body capsule is fitted to the art. A new species then needs only its art and its Animal prefab or builder entry; no per-species code.
- Inside every animal_<id> prefab: `Pose_Alive` (standing) and `Pose_Dead` (lying, the carcass) as siblings, a `Vitals` empty, and optional parts by name inside each pose (`Antlers` for deer; others later, e.g. horns or a winter coat) that the code switches by name. On death the hook shows Pose_Dead instead of tipping the model.
- Sources: one Art/Blender_Work/animal_<id> folder each. Bodies come from a joint table (position and thickness per joint) through Blender's Skin modifier and two levels of smoothing, so a new four-legged animal is mostly a new joint table plus its coat colours, and the dead pose is the same joints re-posed. Ears, eyes and antlers are added on and seated on the body by casting onto its surface.
- Real-world size (a white-tailed deer is about 1.0 m at the shoulder; the primitive deer is oversized). Front faces Blender -Y (Unity +Z), ground at z = 0, budget about 15,000 tris per pose.
- Regional variants (e.g. a mule deer, an elk, a desert or northern subspecies) should reuse the nearest joint table with changed proportions and coat, not start from scratch.
- Shared helpers (2026-11-19): Art/Blender_Work/animal_util.py has skin_body (joint table to smooth body), seat (put a part on the real surface), build_dead / dead_point (the carcass deformation, set per species by a small table: centre height, resting radius, belly and neck heights, head drop, and an optional tail_y / tail_rest that lays a flat tail down), the coat-region shader helpers and export_animal. A species script is its joint table, its parts and its coat.
- Tests: AnimalArtTests.ASpeciesWithArtShowsItAndFallsAsItsCarcass runs for every species listed in it; add each new animal's prefab name there.
- Male and female (Mike, 2026-11-19): every Animal is male or female (IsMale: a deer's antlers decide it, any other species is an even chance). A species drawn differently by sex puts each look in a `Male` and a `Female` group inside each pose (`Dead_Male` and `Dead_Female` in the carcass); the hook shows only the animal's own and fits the body capsule to it. Species without the groups are unchanged. export_animal writes the groups when given {"Male": {...}, "Female": {...}}.
- Import rule: Blender's FBX export gives meshes inside two levels of grouping an extra 90-degree tilt; animal meshes are authored in place, so the prefab build resets every mesh's local transform.

| Animal | Approx. size (m) | Notes | Priority |
|---|---|---|---|
| Deer (white-tailed) | 1.7 long x 1.0 shoulder | Approved 2026-11-19 (animal_deer, standing and carcass; see its section). Head-down grazing pose later needs a code change. | Medium |
| Turkey | 0.9 long x 0.6 tall | Approved 2026-11-19 (animal_turkey, standing and carcass; see its section). | Medium |
| Rabbit (cottontail) | 0.4 long | Approved 2026-11-19 (animal_rabbit, crouched and carcass; see its section). Hops by moving the whole animal (Animal.hops). | Medium |
| Squirrel (grey or fox) | 0.45 long incl. tail | Approved 2026-11-19 (animal_squirrel, on all fours with the tail up, and carcass; see its section). | Low |
| Duck (mallard) | 0.55 long | Approved 2026-11-19 (animal_waterfowl, drake and hen, floating and carcass; see its section). | Low |
| Fish (creek species) | 0.2 to 0.5 long | Art exists (2026-11-19 check): the four species (bluegill, crappie, bass, catfish) are the Wave 4 item_ fish, and the Fish Trap's art has fish in its Catch. The rod shows no fish on the line today: a catch goes straight to the pack (FishingRod). Showing the landed fish is a code feature, not art. Counted done by Mike, 2026-11-19 (no fish on the line for now). | Low |
| Bobber | 0.07 | Approved by Mike 2026-11-20 (fishing_bobber; see its section). | Low |
| Carcass (each species) | as the animal | Moved from Wave 4 (Mike, 2026-11-19). Today Animal.Die tips the live model 85 degrees on its side (Carcass.cs has the dressing logic and no visuals). Make the dead pose from each animal's own model so the body that falls matches the live animal; a field-dressed look is optional (Carcass has a dressed flag, nothing visual yet). | with each animal |
| Rabbit Snare, Box Trap, Fish Trap (world versions) | see tools table | Done 2026-11-15 from the tool_ art (see the trap_ notes). | Low |

---

# Wave 6: trees, terrain, water, weather and effects

These set the look of every screen, so they decide whether the realistic style holds together; do them in step with the structures. Trees have a special constraint: SeasonalTrees recolours leaves per season from per-vertex data baked into the leaf faces and snow caps. A replacement tree mesh without that data stays green all year. Ask the maker for a leaf material that the code can recolour, or plan a small code change when the first real tree arrives.

| Group | What | Notes | Reference image | Priority |
|---|---|---|---|---|
| Trees | Hardwood Broad, Hardwood Tall, Understory Shrub, plus tall and short fall-colour variants | Sugar maple, sycamore, oak, hickory, beech; up to about 25,000 triangles each in the realistic style (leaf cards count); also a low-detail version later. FallingTree tips the same prefab over. | fall sugar maple, sugar maple, winter sugar maple, sycamore (reference only) | High. All three in Unity 2026-11-19 (Hardwood Broad = sugar maple, Hardwood Tall = tulip poplar, Understory Shrub = viburnum), approved by Mike in Play Mode 2026-11-20 (see world_trees). Fall variety done 2026-11-19: a share of each kind turns a second fall colour (golden-orange maples, russet poplars, purple viburnums), chosen per tree in the leaf shader. |
| Rocks | Rock A, B, C and the South Ridge outcrop | Approved 2026-11-19 (world_rocks: weathered limestone; see its section). | none (procedural) | Medium |
| Sticks and branches | Ground wood variants | Approved 2026-11-19 (world_ground_wood: three stick bundles and three fallen branches; see its section). | none (procedural) | Medium |
| Tall grass | Grass clump | Approved 2026-11-19 (world_tall_grass; see its section). Sways in the wind (see Wind sway). | none (procedural) | Medium |
| Forage | One simple shape and one material per species | Done 2026-11-19 (world_forage): one shape per crop in Resources/Art/forage_crop_model.fbx, used by ForagePlacer; fruit and berries now sit on the art trees' real leaves. Approved by Mike 2026-11-20. | none yet | Low |
| Clay patches | Wet clay surface decals | Done 2026-11-19 (world_ground): Resources/Art/world_clay_patch.prefab, an HDRP decal of wet clay, ready to place; none placed (the creek banks already show clay through the terrain layer). | none yet | Low |
| Water | Creek and pond surface material | Approved 2026-11-19 (world_water: see-through, flowing, shore foam; see its section). | none (procedural) | High |
| Terrain | Three terrain layers: Dirt, Grass, Gravel (albedo, normal, mask) | Approved 2026-11-19 (world_terrain; see its section). Runtime snow, leaf litter, wet clay and creek sand layers have art 2026-11-19 (world_ground), approved by Mike 2026-11-20. | none (procedural) | High |
| Weather | Rain, snow, lightning particle sprites | Done 2026-11-19 (world_fx): new Raindrop and Snowflake sprites in place, a glowing Lightning Bolt texture. Approved by Mike 2026-11-20. | none yet | Medium |
| Smoke and fire | KilnSmoke material and texture, campfire flame sprites | Done 2026-11-19 (world_fx): a 16-frame flame sheet on the campfire's Flames and Core, a billowy kiln smoke puff. Approved by Mike 2026-11-20. | none yet | Medium |
| Sky | Skybox or HDRI | Done 2026-11-19 (world_sky): HDRP Physically Based Sky (sun, dawn, dusk) plus stars and a faint Milky Way at night and a textured moon that waxes and wanes over 29.5 days; clouds stay HDRP's cloud layer driven by DayNightCycle. Approved by Mike 2026-11-20. | none yet | Medium |

---

# Reference image inventory (root Art folder, 37 files, checked 2026-11-05)

All 37 are flat 2D pictures, not 3D models. None is usable directly as game art. They are shape and mood references for whoever makes the real models. Keep the Art folder OUTSIDE Assets and out of git commits (it is a root folder, untracked on purpose). Rights column: "AI" means an AI concept sheet (rights unclear, reference only); "stock" means a watermarked stock image (never use as texture, never trace); "web" means a web photo or render with unknown rights (reference only).

| File | Subject | Matches id | Source type and rights | Use |
|---|---|---|---|---|
| hands.jpg | First-person hands concept sheet | hands_player | AI | Reference |
| primitive axe.png | Low-poly stone axe render | tool_primitive_axe, tool_axe | web render | Shape and lashing idea only (style is now realistic) |
| primitive hammer.png | Hammer | tool_hammer | web | Reference |
| primitive pick axe.png | Pick axe | tool_stone_pick_axe, tool_iron_pick_axe | web | Reference |
| primitive knife.jpg | Knife | tool_knife | stock (Shutterstock watermark) | Shape only |
| primitive torch.png | Torch | tool_torch | web | Reference |
| primitive wood saw.png | Wood saw | tool_wood_saw | web | Reference |
| primitive bucket.png | Bucket | tool_bucket | web | Reference |
| sap bucket.png | Sap bucket | tool_sap_bucket, structure_tap | web | Reference |
| spile.png | Spile | tool_spile, structure_tap | web | Reference |
| bow drill kit.png | Bow drill kit | tool_bow_drill | web | Reference |
| primitive tools.png | Flat cartoon sheet of bows and axes | tool_primitive_bow, tool_recurve_bow | stock (Adobe Stock watermark) | Shape only |
| small cabin.jpg | Small cabin concept | structure_cabin | AI | Reference |
| small cabin 2.jpg | Small cabin concept, second view | structure_cabin | AI | Reference |
| workshop exterior.png | Workshop from outside | structure_workshop | web (video thumbnail badge) | Reference |
| workshop2 exterior.png | Workshop, second view | structure_workshop | web (video thumbnail badge) | Reference |
| workshop interior.png | Workshop inside | structure_workshop | web | Reference |
| pottery shop interior.png | Potter's shed inside | structure_pottershed | web | Reference |
| pottery shop2 interior.png | Potter's shed, second view | structure_pottershed | web | Reference |
| pottery shop3 interior.png | Potter's shed, third view | structure_pottershed | web | Reference |
| charcoal kiln.png | Charcoal kiln | structure_kiln, structure_charcoalpit | web photo | Reference |
| ash hopper.png | Ash leach hopper | structure_leachhopper | stock (Shutterstock watermark) | Shape only |
| primitive food cache.png | Food cache | structure_foodcache | web | Reference |
| woodshed.png | Wood shed | structure_woodshed, structure_woodstorage | web photo | Reference |
| tool storage rack.png | Tool rack | structure_toolrack | web | Reference |
| storage trunk.png | Storage trunk | structure_storagebin | web | Reference |
| Barrel.png | Barrel | structure_waterbarrel, structure_rainbarrel | web | Reference |
| primitive hand cart.png | Hand cart | structure_handcart | web (faint watermarks) | Shape only |
| cooking tripod.png | Cooking tripod with pot | structure_tripod, tool_cooking_pot | web | Reference |
| tripod campfire.png | Tripod over a campfire | structure_tripod, effect_campfire | web | Reference |
| tripod kettle.png | Tripod with kettle | structure_tripod, tool_potash_kettle | web | Reference |
| tanning rack.png | Hide tanning rack | none yet (tanning not in code) | web | Future |
| tanning tools.png | Tanning tools | none yet (tanning not in code) | web | Future |
| fall sugar maple.png | Sugar maple in autumn | Trees (seasonal) | web | Reference |
| sugar maple.png | Sugar maple in summer | Trees (seasonal) | web | Reference |
| winter sugar maple.png | Sugar maple in winter | Trees (seasonal) | web | Reference |
| sycamore trees.png | Sycamores | Trees | web | Reference |

Still no references for: Well, Outhouse, Smithy, Bloomery, Kitchen and its add-ons, Tent, Tarp, Lean-To, Shelving, animals, rocks, grass, water, terrain, UI icons, sky. Those rows say "none yet" above; collecting a few images for each (same way as before) is the next useful step for Mike when time allows.

Next step (style is decided: realistic): choose who makes the Wave 1 art (hands plus the tools), because that is what is on screen every minute of play. Suggested first step: one test piece (tool_axe or the hands) built to the realistic texture standard, dropped in at Resources/Art/ and judged in the game under the real lighting, before ordering the rest. A test piece shows the real cost and whether the look fits the current world.


---

# Blender to Unity notes (from the tool_axe test, 2026-11-05)

First pipeline test: Blender (built through the Blender MCP connector) to FBX to a test prefab at Resources/Art/tool_axe.prefab to the hands in Play Mode. It worked end to end. The axe is a blockout (about 770 triangles, plain colours), a pipeline proof and not final art. The test files live in Assets/Resources/Art (uncommitted, never commit that folder) and the sources in Art/Blender_Work/tool_axe/.

What was found:

- Export settings used: FBX, selected objects, empties and meshes, apply scale All, forward -Z, up Y, apply transform on, no textures. Unity imported it at 1 unit = 1 m and the axe measured 0.93 m (as built).
- Orientation: an axe built in Blender with the handle along +Y arrived in Unity with the handle along -Z (Blender's forward is -Y, so +Y is "backward"). The cutting side (Blender -Z) correctly became -Y. Fix applied for this piece: the prefab wrapper turns the meshes 180 degrees about Y and negates Z on the anchors. For the next pieces, author the tool with the handle along Blender -Y so no wrapper turn is needed. This is EXPECTED but not yet confirmed. The X direction could not be checked because the axe is symmetric left to right: next piece should include a tiny asymmetric marker (a notch on the +X side) so the first import settles it.
- Anchors survived import as named children. Unity local values: Anchor_Grip_R (0, 0, 0), Anchor_Grip_L (0, -0.0016, -0.170 in the unfixed import, so +0.17 behind after the wrapper fix, as built), Anchor_Tip (0, -0.1796, +0.530). The tip sits 0.18 m toward the cutting edge (the head is 0.156 m deep).
- Materials: the FBX carries no usable HDRP materials. Each piece needs its HDRP/Lit materials made in Unity (wood: base 0.30, 0.17, 0.075, smoothness 0.38; steel: base 0.30, 0.31, 0.33, metallic 1, smoothness 0.58). Real art needs baked PBR texture sets (base colour, normal, mask map) assigned in the same way.
- In hand: ArtSwap put every renderer on the Viewmodel layer, turned off shadow casting and removed colliders, as documented. The rig scales the tool to 0.5, so the 0.93 m axe shows as about 0.47 m. No clipping or lighting problem. Left palm lands exactly on Anchor_Grip_L, which sits just below the bottom edge of the screen at rest. Head reads dark and glossy, wood plain brown, which fits plain colours and is where textures will help.
- One ArtSwap test fails while Resources/Art exists: TheEditorMenuMakesAndRemovesTheTestKilnPrefabAndRefusesToTouchAnyOther asserts that Assets/Resources/Art does not exist (line 558). Fix before any real art is committed: make that test use only its own test prefab path. Until then, move the axe test files out of Resources/Art before running the full suite for a commit.
- Security: the Blender connector runs any Python Claude sends. Use scratch scenes and copies; the .blend sources stay outside Assets and out of git.

Texture pass on tool_axe (2026-11-05, second pass, not yet in Unity):

- Head remodelled: lofted superellipse sections from a rounded poll through an eye block to a curved, flared cutting edge (612 triangles; handle stays 516). New Anchor_Tip in Blender is (0, 0.514, -0.1836) before the wrapper turn, which is (0, -0.1836, +0.514) in the prefab's own space; the prefab's anchors were edited by hand in the first import, so they must be updated by hand (the FBX anchors do not carry over).
- Materials built procedurally in Blender (Cycles): hickory handle with fine grain along the length, growth bands and tone drift; forged iron head with brushed grinding marks, fine rust pitting and bright polished edges from a bevel-based wear mask.
- Baked at 2048 x 2048 to Art/Blender_Work/tool_axe/textures: basecolor (sRGB), normal (tangent space, OpenGL style), and a packed HDRP mask map per material (R metallic, G occlusion = 1, B = 0, A smoothness). Wood: tool_axe_wood_basecolor/normal/mask.png. Steel: tool_axe_steel_basecolor/normal/mask.png. The separate roughness and metallic PNGs are intermediates.
- Bake lesson: a DIFFUSE bake of a fully metallic surface comes out black. Bake the base colour of metal through a temporary Emission shader (done for the steel). The wood is not metallic, so a normal diffuse bake worked.
- Bake lesson: Blender tool calls over about 60 seconds report a timeout even if they finish on the PC. Keep bakes to a few passes per call and check the output files.
- UV coverage is low (long thin handle islands); resolution is not used well. Improve with a manual UV layout when real art starts.
- Verdict from the Blender render: reads as a real felling axe in the hand. Still a modelled-from-scratch piece with procedural textures; it will not match hand-sculpted and photo-textured work, but it is a credible base for the style decision.


---

# Art tools on Mike's PC (checked and set up 2026-11-06)

Hardware: NVIDIA RTX 3060 12 GB, 32 GB RAM, Intel i5-9400F, about 290 GB free on C. Python 3.12, git, node, uv present.

Blender 5.2.2 LTS with the official Blender Lab MCP add-on 1.0.3 (Preferences, Add-ons, MCP; localhost:9876, server must show "running"). Claude (desktop app) reaches it through the Blender connector. Claude Code was connected on 2026-11-06 with: claude mcp add --scope user blender -- "C:\Users\mmcus\AppData\Roaming\Claude\Claude Extensions\ant.dir.gh.blender.blender-mcp\.venv\Scripts\blender-mcp.exe" (reported Connected). Claude Code needs a new session to pick it up. Blender must be open with the add-on running for either to work.

ComfyUI 0.36.0 at C:\Users\mmcus\ComfyUI (own venv, torch 2.14 cu126, no custom nodes). It was last used 2026-09-21. Models present: five SD 1.5 checkpoints (dreamshaper_8, realisticSD15_v10, perfectrsbmixSD15_v33, reweikUni021_v0214, an anime one), pixarsRendermanInspo_mk1, svd_xt (video), and hunyuan3d-dit-v2_fp16 (shape only, no textures). Nothing newer. Start it with: C:\Users\mmcus\ComfyUI\venv\Scripts\python.exe main.py --listen 127.0.0.1 --port 8188 (run from C:\Users\mmcus\ComfyUI; loopback only, never use a public address), then it answers at http://127.0.0.1:8188.

ComfyUI MCP: the official Comfy-Org comfy-mcp (beta, licence AGPL-3.0-or-later or commercial; fine as a local tool, not shipped in the game) installed in its own folder C:\Users\mmcus\comfy-mcp-env (comfy-mcp as of 2026-11-06 and comfy-cli 1.25.0, kept apart from ComfyUI's own venv so torch is not touched). comfy-cli default workspace set to C:\Users\mmcus\ComfyUI. Registered for Claude Code (user scope, Connected) and added to the Claude desktop config (needs a desktop app restart before Claude sees it). Backups made before any edit: C:\Users\mmcus\.claude.json.bak-before-mcp-setup-2026-11-06 and %APPDATA%\Claude\claude_desktop_config.json.bak-before-comfy-2026-11-06. The server does not start ComfyUI by itself in this setup; start ComfyUI first (command above).

Not done, needs Mike's approval first (multi-GB downloads): image-to-3D models (TRELLIS.2 is now built into ComfyUI 0.34 and later; blog says MIT, no non-commercial dependencies; Pixal3D licence unverified; VRAM need on a 12 GB card unverified) and a modern image model for textures and concept art (the SD 1.5 models are old). Check each model's licence for a sold game and its size before download.

Realistic plan for art: Blender (Claude can model, UV, bake and export), ComfyUI for texture and concept images, and possibly image-to-3D for rough bases that Blender cleans up. All generated assets need a licence check before they go into the shipped game.


## Realistic hands: Blender to Unity findings (2026-11-06)

- The hands FBX did not arrive the way the axe test suggested. With the same export settings, the fingers pointed -Y, the arm ran toward +Y and the back of the hand was +Z, and every mesh node carried a 90 degree X rotation. The fix lives in the prefab wrapper, not the FBX: each hand has a Model_R / Model_L child with local rotation (-90, 0, 0), which turns fingers to +Z and the back of the hand to +Y. The wrapper sits under Hand_R / Hand_L because PlayerHands overwrites each hand's local rotation every frame.
- Rule for every future piece: author in Blender, import, then check orientation in Unity and correct it in the prefab wrapper. Do not rely on the export axis settings alone.
- Naming rule: the FBX and the prefab must not share a name, or Resources.Load returns the FBX and skips the wrapper. FBX files are named <id>_model.fbx (hands_player_model.fbx, tool_axe_model.fbx). tool_axe.fbx still needs renaming.
- Right thumb is on -X, left on +X, both confirmed in game. Anchor_Hand_R / _L sit under each hand at (0, -0.04, 0.07) after the wrapper rotation; only their position is used.
- Hands v2 (2026-11-06, Blender): skin forearm trimmed where it is hidden inside the sleeve, sleeve lengthened to 0.81 m and closed at the far end so the open tube no longer shows on two-handed tools. Total 14,964 triangles (skin 6,512 per hand, nails 420, sleeve 550), inside the 15,000 budget. Files: Art\Blender_Work\hands\hands_player_model.fbx, textures\ (sleeve maps rebaked). The first version is kept as hands_player_v1_old.fbx.
- Known to improve later: nails slightly large and pale, no finger animation (hands stay in the curled pose), cuff can look hollow close up, skin tone is tinted in Unity (0.88, 0.72, 0.62) and can be tuned.


## Realistic hands v3 (2026-11-06)
- Reference photos of Mike's own hand: Art\Reference_Photos\Hands\ (14 photos). Skin pores/hair/freckle detail tiles were derived from them.
- v3 export set: Art\Blender_Work\hands\v3_export\ (hands_player_model.fbx plus hands_skin_basecolor/normal/mask and hands_sleeve_basecolor/normal, named to overwrite the existing Unity files).
- Idle pose is now thumb-up (mesh rolled -90 degrees about Y in Blender). Always verify orientation in Unity and fix in the prefab wrapper (Model_R/Model_L).
- Known improvements for later: real hair cards on the back of the hand and forearm, HDRP Subsurface Scattering skin material with a diffusion profile, separate cuff geometry on the sleeve (sleeve is 800 tris per arm so the silhouette is slightly faceted), palm creases.
- Blender GPU bake gives black images for DIFFUSE/AO here; bake on CPU.


## tool_knife pilot: axis rule confirmed (2026-11-14)

The first piece made end to end by Claude: Blender (Blender MCP) to Unity (MCP for Unity). Copilot is no longer on the project. Claude now does the Unity import too, and the per-piece workflow is the homestead-blender-art skill.

- What it is: a knapped flint blade, a stripped-branch wooden handle and a sinew wrap, matching the stone-blade primitive in ToolViewPrimitives.
  - 7,482 triangles: blade 3,006, handle 2,558, wrap 1,918.
  - Three materials, each with 2048 basecolor, normal and HDRP mask maps.
  - Sources: Art\Blender_Work\tool_knife\ (tool_knife.blend, tool_knife_model.fbx, textures\, review\).
- AXIS RULE (confirmed with an asymmetric marker, a knot authored on the handle's Blender +X side). With the FBX settings used (forward -Z, up Y, apply scale All, apply transform):
  - Blender -Y becomes Unity +Z.
  - Blender -Z becomes Unity -Y.
  - Blender +X becomes Unity -X: X is mirrored, and the knot landed on Unity -X.
  - Nodes import with zero rotation.
  - Author tools with the working end along Blender -Y and the cutting side along Blender -Z, and no wrapper turn is needed. The axe needed its 180-degree turn only because it was built along +Y.
  - For anything left/right-specific, author it on Blender +X to get Unity -X.
- In Unity:
  - tool_knife.prefab is flat, like tool_axe: root, Anchor_Grip_R (0, 0, 0), Anchor_Tip (0, 0.0042, 0.24), and Knife_Blade, Knife_Handle, Knife_Lashing.
  - The meshes still reference tool_knife_model.fbx, so a re-export updates them. Anchors are NOT updated by a re-export.
  - Materials tool_knife_flint, wood and sinew are HDRP/Lit (keywords _NORMALMAP and _MASKMAP).
  - Normal maps are imported as Normal map; masks with sRGB off.
  - ArtSwap.ToolSpecFor("knife") resolves the prefab, and the ArtSwap EditMode tests pass 27/27 with the art present. The earlier Resources/Art test problem (line 558) was already fixed.
- In-game check: approved by Mike 2026-11-14 ("looks good for now"). Seats in the right hand at the existing holdPitch of -22 degrees; no changes needed. Possible later polish: the blade reads dark and thin at a distance in daylight.



## tool_torch (2026-11-14): flames on the Viewmodel layer

- What it is: a pitch torch. A weathered branch handle (bark patches, a knot), a head of pitch-soaked cloth wraps bound with three twisted hemp ropes, and a cord binding where the head meets the stick. Reference: primitive torch.png.
  - 11,748 visible triangles.
  - Five materials: wood, pitch, pitch_lit (with an ember emissive map), rope, binding. Rope is 1024 and binding 512; the rest are 2048.
  - Sources: Art\Blender_Work\tool_torch\ (tool_torch.blend, tool_torch_model.fbx, textures\, review\, make_torch_flame_sheet.py).
- Axis rule confirmed a second time: the FBX imported with no wrapper turn.
- Lit and unlit:
  - State_Unlit holds Torch_Head; State_Lit holds Torch_Head_Lit (the same mesh, pitch_lit material, emissive color 6, like the campfire's flames) and Torch_Flame.
  - Anchor_Flame stays at (0, 0, 0.58), where the code moves the held light.
- THE FLAME, and two lessons for every future held effect:
  1. Particle systems do NOT work on the Viewmodel layer. The main camera never sees that layer, so Unity pauses them (culling) and stops rebuilding their geometry. The flame froze where it was lit, and AlwaysSimulate did not fix the drawing.
     - Fix: Assets/Scripts/World/ViewFlame.cs (new code, commit it). Three upright mesh cards step through a 4 x 4 flipbook (tool_torch_flame_sheet.png, generated by make_torch_flame_sheet.py, original work) with a small flicker.
     - The cards play together, a frame or two apart, and mirror together.
     - Material tool_torch_flame is HDRP/Unlit, transparent, premultiplied, copied from the campfire's Flame Outer.
  2. ViewmodelPass drew everything with depth write on, so a transparent card hid the cards behind it.
     - Fix (Assets/Scripts/Player/ViewmodelPass.cs, commit it): opaque renderers first, as before; then transparent ones with depth write off.
     - The 247 hands, tools and ArtSwap EditMode tests pass.
- Approved in game by Mike 2026-11-14: follows the torch, looks right. Possible later polish: sparks, done as cards (particles will not work here).

## tool_bucket (2026-11-14): hanging by gravity, and water in hand

- What it is: a wooden staved bucket. 14 slightly uneven pale staves, tapered (outer 0.17 m across at the top and 0.15 m at the bottom, 0.17 m deep), a real inside with a darker water-stained wall and floor, two forged iron hoops with rust and a rivet, iron ears, and a wire bail through a worn wooden grip. Reference: primitive bucket.png.
  - 11,104 triangles.
  - Two materials: wood (the inside is darkened through a per-vertex "inner" attribute) and iron (metal; its base colour was baked through Emission, and its metallic value comes from the mask's R channel).
  - Sources: Art\Blender_Work\tool_bucket\.
- Hanging rule confirmed: authored grip at the origin, body down Blender -Z. It imported hanging along Unity -Y, with Anchor_Tip at (0, -0.22, 0) and no wrapper.
- NEW CODE 1, hanging by gravity (Mike's request):
  - Hanging items used to hang "straight down in the view", so they tilted when looking up or down.
  - Assets/Scripts/Player/HangPendulum.cs (new) is a damped world-space pendulum: gravity, plus the hand's own acceleration, 45-degree cap, settles in about a second.
  - PlayerHands.AlignHangingByGravity uses it for every hanging item: bucket, sap bucket, canteen, cooking pot, kettle, pouch and traps. The rifle and bows keep the view-locked AlignHanging.
  - Tests: Assets/Tests/EditMode/HangPendulumTests.cs (6).
- NEW CODE 2, water in a container in hand:
  - Convention: a prefab child named Fill (the water surface, authored at the full level) and an empty named Fill_Bottom (the floor level; its localScale.x is the surface size there, for the taper).
  - PlayerHands.UpdateFill shows the surface while water is carried and raises it by FillQuery: litres carried over the capacity of every Bucket and Canteen, since water is one shared pool.
  - Bucket assets: tool_bucket_water_disc.asset (unit disc) and tool_bucket_water.mat (dark glossy HDRP/Lit).
  - The sap bucket and canteen can use the same two children.
- Approved in game by Mike 2026-11-14: look, lighting, filling, drinking, hanging and the water level all right.

## tool_hammer (2026-11-14): wooden mallet

- What it is: a wooden mallet. A turned barrel head with a ringed band, worn grey-green paint over pale wood (more wear toward the striking ends, end grain on the faces, a faded red band) and a dark streaky oak handle, slightly faceted and tapering, polished at the grip. Reference: primitive hammer.png.
  - 8,252 triangles.
  - Two materials: head and handle, each with 2048 basecolor, normal and mask maps.
  - Sources: Art\Blender_Work\tool_hammer\.
- Design change from the primitive: the primitive laid the head sideways (0.16 m along X). The real mallet's barrel stands across the swing (along Unity Y), so a flat end strikes on the downswing.
  - Anchor_Tip is the centre of the striking face, (0, -0.076, 0.31).
  - No Anchor_Grip_L (one-handed).
  - holdPitch stays -24 degrees.
- Art only, no code changes. Approved in game by Mike 2026-11-14: grip, swing and lighting all right.

## tool_shovel (2026-11-14): Primitive Shovel, the first two-handed tool

- What it is, per the code (not the list's "wooden shovel"): the item "shovel" is the Primitive Shovel, crafted from 3 sticks, 1 cordage and 1 stone.
  - A flat dark slate blade, spade-shaped with a rounded point, about 2.5 cm thick in the middle and thin at the edges.
  - Two tight cord lashings hold it under the end of the handle.
  - A sapling handle with bark patches, a knot and a slight bow, and a crossbar through the butt for the T-grip.
  - 10,416 triangles. Three materials: stone, wood (handle and crossbar) and cord (1024).
  - Sources: Art\Blender_Work\tool_shovel\.
- Anchors: Anchor_Grip_R (0, 0, 0), Anchor_Grip_L (0, 0, 0.42) as the code's gripLeft, Anchor_Tip (0, -0.029, 0.975) at the blade point. holdPitch stays -14 degrees.
- The iron shovel (tool_iron_shovel) is a separate prefab id and still uses its primitive.
- Art only, no code changes. Approved in game by Mike 2026-11-14: both hands, digging pose, size and lighting all right.

---

# Audit and tracker, 2026-11-07 (v0.3)

How this audit was done: Waves 1 to 6 were read against the current build (the Scripts/UI folder, the scenes, the particle and sky code, the Resources folders, Phase 2 item data and the task log up to 2026-11-07). The existing rows hold up: every structure, tool, animal and world group that exists in the game has a row. What was missing is everything that is not a 3D world object (screens and UI, screen effects, the sky, branding), several item groups, and art for systems that are designed but not built yet. Those are added below as Waves 7 to 10 and Wave 4 additions. Nothing in the project's code or art folders changed; this is a list.

Project folders checked: 02_Blender_Assets, 03_Concept_Art, 04_Audio and 07_Marketing are all empty (0 files). There is no logo, no app icon, no title or splash art and no concept art anywhere in the project. The only art on disk is the root Art folder (Blender_Work, References, Reference_Photos and two ComfyUI output folders) and Assets/Resources/Art (the hands).


## tool_iron_shovel (2026-11-14): Iron Shovel, forged blade on a stick handle

- What it is, per the recipe (SmithyRules: 2 iron bars, 2 charcoal, 3 sticks, 1 cordage, made at the Smithy): a forged iron spade blade with a socket the stick handle fits into, rolled treads on the top edge, a rivet through the socket, and a T-grip crossbar held on by a cord binding. Same layout as the Primitive Shovel (twoHanded, holdPitch -14, gripLeft 0.42, tip 0.92).
- Size: 1.15 m overall (crossbar to point), blade about 0.20 wide and 0.25 long, sheet iron about 3.5 mm, slightly dished.
- Tris: 10,416 (budget 15,000).
- Materials: tool_iron_shovel_iron (metal: base colour baked through emission, metallic baked into mask R; worn bright edge, scale and light rust), tool_iron_shovel_wood (handle and crossbar, one object), tool_iron_shovel_cord. All 2048.
  - Sources: Art\Blender_Work\tool_iron_shovel\.
- Anchors: Anchor_Grip_R (0, 0, 0), Anchor_Grip_L (0, 0, 0.42), Anchor_Tip (0, -0.022, 0.985) at the blade point.
- Unity: Assets/Resources/Art/tool_iron_shovel.prefab, ArtSwap.ToolSpecFor("iron_shovel") resolves, ArtSwapTests 27/27 passed.
- Status: approved in game by Mike 2026-11-14 (bought at the Trading Post): both hands, look and lighting right.

## tool_stone_pick_axe (2026-11-14): Stone Pick Axe, a ground stone point lashed across a stick

- What it is, per the recipe (Crafting: 2 sticks, 1 cordage, 1 stone): a long stone, ground to a point at the working end and blunt at the butt, lashed across the top of a stick haft with cordage: tight wraps on the haft above and below the head and an X binding over the joint. Layout from ToolViewPrimitives (twoHanded, holdPitch -14, holdRoll -24, gripLeft 0.22, tip (0, -0.24, 0.68)).
- Size: haft 0.85 m (Grip_R to the end, plus 0.14 behind the grip), head 0.39 long, about 5 cm wide.
- Tris: 11,134 (budget 15,000). The cord was decimated to 55% to stay in budget.
- Materials: tool_stone_pick_axe_stone, _wood and _cord, copied from tool_shovel's procedural set. All 2048. The head uses Smart UV Project (its loft caps are filled).
  - Sources: Art\Blender_Work\tool_stone_pick_axe\. The lashing hugs the surfaces by ray-casting (no floating loops).
- Anchors: Anchor_Grip_R (0, 0, 0), Anchor_Grip_L (0, 0, 0.22), Anchor_Tip (0, -0.24, 0.655) at the stone point (the head sits 0.655 along the haft; the haft runs on to 0.705).
- Unity: Assets/Resources/Art/tool_stone_pick_axe.prefab, ArtSwap.ToolSpecFor("stone_pick_axe") resolves, ArtSwapTests 27/27 passed.
- Status: approved in game by Mike 2026-11-14: grip, look and lighting right.

## tool_iron_pick_axe (2026-11-14): Iron Pick Axe, a forged head on a stick haft

- What it is, per the recipe (SmithyRules: 2 iron bars, 2 charcoal, 2 sticks, 1 cordage, made at the Smithy): a forged iron head with an eye the haft passes through (an iron wedge in the top), a square point down and a flat chisel end up, on the same kind of stick haft as the Stone Pick Axe. The cordage is a tight wrap on the haft just under the eye. Same layout as the stone one.
- Size: haft 0.84 m, head 0.39 long.
- Tris: 8,624 (budget 15,000).
- Materials: tool_iron_pick_axe_iron (copied from the iron shovel; the worn bright edge moved to the point and the chisel edge), _wood and _cord. All 2048. Head UVs by Smart UV Project.
  - Sources: Art\Blender_Work\tool_iron_pick_axe\.
- Anchors: Anchor_Grip_R (0, 0, 0), Anchor_Grip_L (0, 0, 0.22), Anchor_Tip (0, -0.24, 0.655) at the point.
- Unity: Assets/Resources/Art/tool_iron_pick_axe.prefab, ArtSwap.ToolSpecFor("iron_pick_axe") resolves, ArtSwapTests 27/27 passed.
- Pipeline note: the bake_util text block was not kept in the earlier .blend files (a text with no users is dropped on save). It is now saved with a fake user and also as Art\Blender_Work\bake_util.py (with the metallic pass).
- Status: approved in game by Mike 2026-11-14: grip, look and lighting right.

## tool_fishing_rod (2026-11-14): Fishing Rod, a store-bought spinning rod

- What it is (a Trading Post item, $12): a two-piece-look dark brown varnished blank tapering to the tip, cork rear and fore grips with a metal reel seat between, a small spinning reel hanging under the seat with its crank on the player's left (Unity -X), four ring guides under the rod plus a tip-top, and red thread wraps over every guide foot and a winding check above the fore grip.
- Size: 1.59 m overall (butt cap at -0.155 to the tip at 1.459), blank 7.8 mm at the butt to 1.5 mm at the tip.
- Tris: 7,620 (budget 15,000).
- Materials: tool_fishing_rod_cork, _blank, _wrap (1024) and _metal (metal: metallic baked into mask R). Metal, cork and wraps use Smart UV Project; the blank keeps its loft UVs.
  - Sources: Art\Blender_Work\tool_fishing_rod\.
- Anchors: Anchor_Grip_R (0, 0, 0) on the rear cork, Anchor_Grip_L (0, 0, 0.30) on the fore cork, Anchor_Tip (0, 0, 1.45), Anchor_Line (0, 0, 1.47) just past the tip-top. The line itself is the bobber's LineRenderer, drawn from Anchor_Line (FishingRod.cs / PlayerHands.LineAnchor), so nothing to model for it.
- Unity: Assets/Resources/Art/tool_fishing_rod.prefab, ArtSwap.ToolSpecFor("fishing_rod") resolves, ArtSwapTests and RodsAndLightsInHandTests 62/62 passed.
- Status: approved in game by Mike 2026-11-14: grip, reel, line from the tip and lighting right.

## tool_cane_pole (2026-11-14): Cane Pole, a natural cane with the line tied on

- What it is: one natural cane with 8 nodes (closer together toward the tip), tapering from 29 mm at the butt to about 6 mm, with a slight bow and a drooping tip. The line is tied on with tight turns at the tip and a short tag end; no rings. The bobber's line is drawn from Anchor_Line by the game.
- Size: 1.74 m (butt at -0.165 to the tip at 1.575), following the code layout (tip 1.55, line 1.56), not the list's 2.4 m guess.
- Tris: 10,552 (budget 15,000).
- Materials: tool_cane_pole_cane (2048: straw-tan cane with lengthwise streaks, mottling and darker nodes) and tool_cane_pole_cord (1024, from tool_shovel's cord). Loft UVs.
  - Sources: Art\Blender_Work\tool_cane_pole\.
- Anchors: Anchor_Grip_R (0, 0, 0), Anchor_Grip_L (-0.008, -0.003, 0.32), Anchor_Tip (-0.007, -0.034, 1.55) and Anchor_Line (-0.007, -0.034, 1.56), on the drooping tip.
- Unity: Assets/Resources/Art/tool_cane_pole.prefab, ArtSwap.ToolSpecFor("cane_pole") resolves, ArtSwapTests and RodsAndLightsInHandTests 62/62 passed.
- Status: approved in game by Mike 2026-11-14 (length kept at 1.74 m).

## tool_lantern (2026-11-14): Lantern, a red hurricane lantern

- What it is: a cold-blast hurricane lantern in worn red paint over tin (chips to bare tin, a little rust): wire bail with a wooden grip roll, vented chimney cap, glass globe in four guard wires, two side tubes from the fount to the cap, burner collar with a wick knob at the front, and a round fuel fount with a filler cap.
- Size: 0.27 m from the grip to the base; glass globe 9.4 cm wide.
- Tris: 7,276.
- Materials: tool_lantern_tin (metal, 2048), tool_lantern_wood (1024, from tool_shovel's wood), and two Unity-only glass materials: tool_lantern_glass (clear, transparent, double-sided) under State_Unlit and tool_lantern_glass_lit (warm tint with a faint orange emissive glow) under State_Lit.
- The flame: State_Lit/Lantern_Flame, two small cards from the torch's flipbook (tool_torch_flame.mat), driven by ViewFlame with flicker 0.05 at 14 fps, standing on the wick at (0, -0.189, 0). The point light comes from the code at Anchor_Flame.
  - Sources: Art\Blender_Work\tool_lantern\.
- Anchors: Anchor_Grip_R (0, 0, 0) on the bail grip, Anchor_Flame and Anchor_Tip (0, -0.17, 0) in the globe.
- Code: PlayerHands now hangs the Lantern by gravity like the bucket (it stays out of HangingIds, so its ready pose and light sway are unchanged). Also, the pendulum no longer steps outside Play Mode, which fixes a flaky RestOfToolsInHandTests case (the editor's deltaTime made a posed hanging item swing by chance). New test: HangPendulumTests.TheLanternHangsByGravityLikeTheBucket.
- Unity: Assets/Resources/Art/tool_lantern.prefab, ArtSwap.ToolSpecFor("lantern") resolves; ArtSwapTests, RodsAndLightsInHandTests, HangPendulumTests and RestOfToolsInHandTests pass.
- Status: approved in game by Mike 2026-11-14: hangs and swings by gravity, clear glass unlit, warm glass and flame on the wick lit. (A solid cyan globe seen once on first equip is the Editor's placeholder while it compiles the new transparent shader variant; it does not happen in a build.)

## tool_primitive_bow (2026-11-14): Primitive Bow, a self bow from one branch

- What it is, per the recipe (Crafting: 1 branch, 1 cordage): a carved self bow, pale wood with grain and a few knots, flat wide limbs (3.6 cm across at the fades to 1.9 cm at the tips, 1.4 cm thick) on a rounder handle wrapped tight with cordage, and a few turns of cord at each tip where the string is looped on. Seen side-on the limbs arc back to the tips, 3 cm behind the grip (the game's brace), with no forward bulge.
- Size: 0.74 m tip to tip, matching ToolViewPrimitives.BowTip(false) (0, +/-0.37, -0.03).
- Tris: 12,808 (stave 6,436, cord 6,312), plus the two string halves.
- Materials: tool_primitive_bow_wood (2048) and tool_primitive_bow_cord (1024, from tool_shovel's cord; the string uses it too).
  - Sources: Art\Blender_Work\tool_primitive_bow\.
- The string: two children of the prefab, "String Upper" and "String Lower", Unity's built-in cylinder (height 2) at 3.2 mm thick, set at rest from each tip to the nock. PlayerHands.UpdateString stretches them from the tips to the right hand while drawing.
- Anchors: Anchor_Grip_L (0, 0, 0) on the wrapped handle (the left hand holds it), Anchor_Nock (0, -0.07, -0.03) on the string, Anchor_Tip (0, 0.37, -0.03).
- Code: PlayerHands now looks up the string halves on a prefab too (before, only the primitive's string followed the hand). New test: HangPendulumTests.APrefabBowsStringHalvesFollowTheDrawingHand.
- Unity: Assets/Resources/Art/tool_primitive_bow.prefab, ArtSwap.ToolSpecFor("primitive_bow") resolves; ArtSwapTests, HangPendulumTests and RestOfToolsInHandTests pass.
- Status: approved in game by Mike 2026-11-14: looks and draws right.

## tool_recurve_bow (2026-11-14): Recurve Bow, a store-bought laminated recurve

- What it is (a Trading Post item, $30): a laminated wood recurve. A deep, shaped riser of dark figured wood (its laminations show where it fades into the limbs) with a pebbled leather grip sleeve and a small arrow shelf on the archer's left, and glossy reddish-brown limbs that sweep back toward the archer and recurve forward over the last third. A dark waxed string.
- Size: 0.76 m tip to tip, matching ToolViewPrimitives.BowTip(true) (0, +/-0.38, -0.035).
- Tris: 12,152 (bow 10,712, grip 1,440), plus the two string halves.
- Materials: tool_recurve_bow_wood (2048; Smart UV Project), tool_recurve_bow_leather (1024), and tool_recurve_bow_string (Unity-only, plain dark, no texture).
  - Sources: Art\Blender_Work\tool_recurve_bow\.
- The string: "String Upper" and "String Lower", Unity's built-in cylinder at 2.8 mm, the same as the Primitive Bow; PlayerHands.UpdateString draws them to the right hand.
- Anchors: Anchor_Grip_L (0, 0, 0) in the leather grip, Anchor_Nock (0, -0.07, -0.035), Anchor_Tip (0, 0.38, -0.035).
- Unity: Assets/Resources/Art/tool_recurve_bow.prefab, ArtSwap.ToolSpecFor("recurve_bow") resolves; ArtSwapTests and HangPendulumTests 36/36 passed.
- Status: approved in game by Mike 2026-11-14.

## tool_bolt_action_rifle (2026-11-14): Bolt-Action Rifle, a scoped sporter

- What it is (a Trading Post item, $60): an oiled walnut sporter stock with a free-floating blued barrel (front sight post at the muzzle), a round receiver with the bolt shroud at the back and the bolt handle out to the right (Unity +X) with a round knob, a trigger and trigger guard, a magazine floorplate, and a scope on two rings and bases: ocular bell at the back, larger objective bell at the front, turrets on top and right.
- The butt: as the placeholder did (ToolViewPrimitives.RifleStockDrop), the stock behind the wrist drops 42 degrees and 0.24 m toward the shoulder, so the butt stays below the bottom of the hands' view rather than coming at the camera. Only the wrist and the start of the drop are ever in view, under the right hand.
- Size: muzzle at 0.79 m in front of the receiver; fore-end to 0.556.
- Tris: 8,132 (wood 5,540, steel 2,592).
- Materials: tool_bolt_action_rifle_wood (walnut, 2048) and tool_bolt_action_rifle_steel (blued, metallic, 2048). Both Smart UV Project; the steel is shaded smooth by angle (40 degrees).
  - Sources: Art\Blender_Work\tool_bolt_action_rifle\.
- Anchors: Anchor_Grip_R (0, 0, -0.09) on the wrist, Anchor_Grip_L (0, -0.02, 0.40) under the fore-end, Anchor_Tip (0, 0.01, 0.78), Anchor_Muzzle (0, 0.01, 0.79).
- Unity: Assets/Resources/Art/tool_bolt_action_rifle.prefab, ArtSwap.ToolSpecFor("bolt_action_rifle") resolves; ArtSwapTests, HandsTests and RestOfToolsInHandTests 145/145 passed.
- Status: approved by Mike 2026-11-14 as "ok for now". Revisit later: in the ready pose the rifle sits angled across the view and the right hand reads as over the action/scope rather than on the wrist; a nearer, straighter hold or a lower scope may read better.

## tool_bow_drill (2026-11-15): Bow Drill, a stick bow with the spindle in its cord

- What it is, per the recipe (Crafting: 2 sticks, 1 cordage): a curved stick bow, held at its back end in the right hand and bowing out to the right, the spindle on its LEFT (mirrored at Mike's request, 2026-11-15), with the cord tied on at both ends and wrapped once round an upright spindle (pointed foot scorched black from drilling, rounded top). No hearth board: the recipe has none (the fire is lit on a built Campfire).
- Size: bow 0.375 m (from 5 cm behind the grip to the tip at 0.325), spindle 0.205 m.
- Tris: 4,848.
- Materials: tool_bow_drill_wood (bow and spindle in one object with split UVs, the tool_shovel wood with a scorch below Blender z -0.07) and tool_bow_drill_cord (1024, from tool_shovel's cord).
  - Sources: Art\Blender_Work\tool_bow_drill\.
- Anchors: Anchor_Grip_R (0, 0, 0) on the bow's back end, Anchor_Grip_L (-0.02, 0.112, 0.15) on top of the spindle, Anchor_Tip (0, 0, 0.32).
- Code (2026-11-15): the Bow Drill is now two-handed (HandPoseRules.TwoHandedIds; ToolViewPrimitives layout gripLeft on the spindle top), so the left hand presses on the spindle. The left hand rides on the kit, so it moves with the sawing stroke too. HandsTests' one-handed list no longer has it.
- Unity: Assets/Resources/Art/tool_bow_drill.prefab, ArtSwap.ToolSpecFor("bow_drill") resolves; ArtSwapTests and HangPendulumTests 36/36 passed.
- Status: approved in game by Mike 2026-11-15 after the mirror ("much better"): spindle on the left under the left hand, bow in the right.


## tool_sap_bucket (2026-11-15): Sap Bucket, a small lidded wooden bucket lashed with cord
- Built from tool_bucket.blend scaled to 0.78. The iron hoops and bail became cord (the recipe is 1 log, 3 branches, 1 cordage: no iron). A two-plank lid with a cleat sits in the rim.
- No Fill child: the sap bucket in hand is always empty, since sap is collected at the tap. It hangs by gravity (HangingIds).
- Meshes: Sap_Wood 7,716 tris, Sap_Lashing 3,388, Sap_Lid 1,148 (12,252 total). Anchors: Anchor_Grip_R at the bail grip (origin), Anchor_Tip (0, -0.16, 0).
- Materials: tool_sap_bucket_wood (2048), tool_sap_bucket_lid (1024), tool_sap_bucket_lashing (1024, cord).
  - Sources: Art\Blender_Work\tool_sap_bucket\.
- Unity: Assets/Resources/Art/tool_sap_bucket.prefab, ArtSwap.ToolSpecFor("sap_bucket") resolves; ArtSwapTests and HangPendulumTests 36/36 passed. Approved in game by Mike 2026-11-15.
## Status tracker (summary)

| Wave | What | Rows | Done | Partly | Not started |
|---|---|---|---|---|---|
| 1 | First-person hands | 1 (2 prefabs) | 1 (idle and tool-grip, Hands v3) | polish backlog below | 0 |
| 1 | The 30 tools | 30 | 30 (tool_knife, tool_torch, tool_bucket, tool_hammer, tool_shovel, tool_iron_shovel, tool_stone_pick_axe, tool_iron_pick_axe, tool_fishing_rod, tool_cane_pole, tool_lantern, tool_primitive_bow, tool_recurve_bow, tool_bolt_action_rifle, tool_bow_drill, tool_sap_bucket, tool_canteen, tool_spile, tool_box_trap, tool_fish_trap, tool_rabbit_snare, tool_primitive_axe, tool_wood_saw, tool_flint_and_steel, tool_pouch, tool_clay_pot, tool_cooking_pot, tool_potash_kettle, tool_tarp and tool_axe (rebuilt), realistic, approved in game by Mike 2026-11-14/15; the rifle "ok for now") | 0 | 0 |
| 2 | Shelter and main structures | 15 | 15 (structure_kiln, structure_cabin, structure_tent, structure_tarpshelter, structure_leanto approved 2026-11-15; structure_well, structure_outhouse, structure_workshop, structure_pottershed, structure_charcoalpit, structure_bloomery, structure_smithy, structure_leachhopper, structure_kitchen and the kitchen add-ons approved 2026-11-16; WAVE 2 COMPLETE) | 0 | 0 |
| 3 | Storage, piles, workshop furniture, hand cart, tripod, tap, campfire | 12 | 12 (structure_tap 2026-11-15; wood pile, rock pile, both barrels, food cache, storage bin, tool rack, shelving, wood shed, hand cart, campfire, tripod approved 2026-11-16) | 0 | 0 |
| 4 | World items, icons, stump, carcass | 9 groups | 7 (raw materials, metal goods, food and animal goods, pottery, item icons, stump: approved by Mike 2026-11-16 to 2026-11-19, every item in the database has dropped art and an icon; map icons in Unity 2026-11-20, awaiting Mike's check) | 0 | 0 (the other 2 need no art: the carcass moved to Wave 5 and was made with each animal; the placement ghost uses the structure prefabs) |
| 5 | Animals and traps | 8 | 8 (deer, turkey, duck, rabbit, squirrel, each with its carcass, approved by Mike 2026-11-19; fish counted done by Mike 2026-11-19: the Wave 4 item fish and the Fish Trap's catch; traps done 2026-11-15; fishing_bobber approved by Mike 2026-11-20) | 0 | 0 |
| 6 | Trees, terrain, water, weather, effects | 11 groups | 11 (all approved by Mike 2026-11-20 in Play Mode: trees with fall variety, rocks, sticks, tall grass, forage, clay patch decal, water, terrain and the runtime ground layers, weather, smoke and fire, sky) | 0 | 0 |
| 7 | UI and screens (new) | 8 groups | 8 (Ledger theme, fonts, HUD, menus, Loss and Loading screens, management screens, map art, warning banners: approved by Mike 2026-11-20; WAVE 7 COMPLETE) | 0 | 0 |
| 8 | Screen effects, particles, sky (new) | 8 groups | 8 (all approved by Mike in Play Mode 2026-11-20, see wave8_fx below; the sky itself was done in Wave 6; WAVE 8 COMPLETE) | 0 | 0 |
| 9 | Art for designed-but-not-built systems (new) | see below | 0 | 0 | all |
| 10 | Branding, store and marketing (new) | 6 | 6 (logo, app icon, title and loading art approved 2026-11-20; Steam store art, teaser, press kit approved 2026-11-21; WAVE 10 COMPLETE) | 0 | 0 |

Plain reading: in-game art so far is the hands and nothing else. The tool, structure and world art waves are all still primitive shapes. The swap-point system (ArtSwap) exists so each piece can arrive one at a time.


## tool_canteen (2026-11-15): Canteen, a round wool-covered tin drum canteen on a leather strap
- 17.5 cm across and 7.5 cm thick (about 1.8 L, close to the game's 2 L per canteen). Tin seam band around the edge, a tin spout with a cork, two tin lugs the strap runs through. Faces front and back (Unity Z), as the primitive did.
- No Fill child: the canteen is closed, so its water level isn't visible. It hangs by gravity (HangingIds) from the top of the strap.
- Meshes: Canteen_Body (wool cover) 3,968 tris, Canteen_Tin (band, spout, lugs) 1,570, Canteen_Strap 644, Canteen_Cork 178 (6,360 total). Anchors: Anchor_Grip_R at the top of the strap (origin), Anchor_Tip (0, -0.15, 0) at the body centre (the code's fallback is -0.10).
- Materials: tool_canteen_cover (2048, wool), tool_canteen_tin (1024, metallic 1 in the mask), tool_canteen_strap (1024, leather), tool_canteen_cork (512).
  - Sources: Art\Blender_Work\tool_canteen\.
- Unity: Assets/Resources/Art/tool_canteen.prefab, ArtSwap.ToolSpecFor("canteen") resolves; ArtSwapTests and HangPendulumTests 36/36 passed. Approved in game by Mike 2026-11-15 (hang, drinking and filling all fine).

## tool_spile (2026-11-15): Spile, a carved wooden sap spout
- 19 cm long, 2.6 cm across, bored through (1 cm bore). The outer end (in the hand) is carved open on top into a sap trough; the middle keeps its bark; the inner end tapers to 1.4 cm to drive into the tree. Made from sticks, per the recipe (3 Sticks).
- One mesh, Spile_Wood, 5,526 tris (the trough cut is a boolean; edges over 40 degrees split for crisp cut faces). Anchors: Anchor_Grip_R at the trough end (origin), Anchor_Tip (0, 0, 0.155) at the tapered point. The trough opens to Unity +Y.
- Material: tool_spile_wood (1024, wood with a bark band, one material).
  - Sources: Art\Blender_Work\tool_spile\.
- Unity: Assets/Resources/Art/tool_spile.prefab, ArtSwap.ToolSpecFor("spile") resolves; ArtSwapTests and HangPendulumTests 36/36 passed. Approved in game by Mike 2026-11-15 (tapping still works).

## structure_tap (2026-11-15): the tap on a maple, built from the approved Spile and Sap Bucket
- No new model. Tap.cs (Assets/Scripts/World) now instantiates Art/tool_spile and Art/tool_sap_bucket instead of three primitives (it falls back to the primitives if the art is missing).
  - Fix after Mike's first look: the tap's origin is 2 cm outside the trunk's capsule radius, but the trunk mesh is a tapering octagon, so at 1 m up the bark is 4 to 7 cm behind the origin and the Spile floated. Everything is set back by Tap.BarkInset (6 cm).
  - Spile: grip end at (0, 0, 0.045), Euler (-8, 180, 0), so its tip reaches 11 cm behind the origin (well into the bark) and the trough end slopes down and out.
  - Sap Bucket: its bail grip rests on the Spile at (0, 0.008, 0.06), with Sap_Lid switched off (lid off on the tree, on in the hand: Mike).
  - Sap: a disc inside the bucket rises from the floor (-0.140 below the grip) to just under the rim (-0.035) with litres / 8 L, a watery amber, olive-brown once sour.
- MapleTests.TheTapOnTheTreeUsesTheSpileAndSapBucketArtWithTheLidOffAndTheSapRises; MapleTests 31/31 and the Maple, ArtSwap and KnowHow suites (142) passed. Approved in game by Mike 2026-11-15 (after the bark fix).

## tool_box_trap (2026-11-15): Box Trap, a board box with a drop door, carried by a dowel handle
- 40 cm long (Unity Z, door at +Z), 13 cm wide, 15 cm tall; the primitive was 20 x 10 x 15, the list said about 0.5 long. Made of rough boards per the recipe (3 Firewood): bottom, top, two sides, three slats and a batten at the back, a drop door shut between two guides at the front.
- Carried hanging from a dowel handle across Unity X on top (two uprights), like the bucket's bail; the box hangs below it.
- One mesh, Trap_Wood (boards and handle joined), 720 tris, flat-shaded boards with a smooth dowel. Anchors: Anchor_Grip_R at the dowel (origin), Anchor_Tip (0, -0.10, 0) at the body centre.
- Material: tool_box_trap_wood (2048, weathered board grain along the length).
  - Sources: Art\Blender_Work\tool_box_trap\.
- Unity: Assets/Resources/Art/tool_box_trap.prefab, ArtSwap.ToolSpecFor("box_trap") resolves; ArtSwapTests and HangPendulumTests 36/36 passed. Approved in game by Mike 2026-11-15 (placing still works).

## tool_fish_trap (2026-11-15): Fish Trap, a splint basket with a funnel mouth, carried by a cord loop
- 15 cm across, 28 cm tall, hanging straight down: 18 split-wood staves gathered and bound with cord at the top, three woven bands and a rolled rim, and a 14-stave funnel inside the open bottom narrowing to a 5 cm throat (the primitive was 10 x 17 cm). Made of firewood splints and cordage, per the recipe.
- Carried by a cord loop round the grip (loop in the Unity Y-Z plane, round the fingers).
- Meshes: Trap_Splints 7,616 tris, Trap_Cord 1,276 (8,892 total). Anchors: Anchor_Grip_R at the loop (origin), Anchor_Tip (0, -0.20, 0) at the funnel throat.
- Materials: tool_fish_trap_splint (2048), tool_fish_trap_cord (512).
  - Sources: Art\Blender_Work\tool_fish_trap\.
- Unity: Assets/Resources/Art/tool_fish_trap.prefab, ArtSwap.ToolSpecFor("fish_trap") resolves; ArtSwapTests and HangPendulumTests 36/36 passed. Approved in game by Mike 2026-11-15 (setting at the water still works).

## tool_rabbit_snare (2026-11-15): Rabbit Snare, a pointed stake with a cord noose
- Held by the stake's blunt (notched) end, point forward: a 24 cm bark-on stick, 2.5 cm thick, whittled to a point at the front. Cord lashed round it behind the point runs forward to a 9 cm noose with a slip knot, standing upright in front (Unity X-Y plane). Not hung (the code holds it by the stake). Recipe: 1 Cordage.
- Meshes: Snare_Stake 742 tris, Snare_Cord 2,468 (3,210 total). Anchors: Anchor_Grip_R at the stake's blunt end (origin), Anchor_Tip (0, 0, 0.32) at the noose, as in the code's layout.
- Materials: tool_rabbit_snare_wood (1024, bark with the whittled point and cut end bare), tool_rabbit_snare_cord (512).
  - Sources: Art\Blender_Work\tool_rabbit_snare\.
- Unity: Assets/Resources/Art/tool_rabbit_snare.prefab, ArtSwap.ToolSpecFor("rabbit_snare") resolves; ArtSwapTests and HangPendulumTests 36/36 passed. Approved in game by Mike 2026-11-15 (setting still works).

## trap_box_trap, trap_fish_trap, trap_rabbit_snare (2026-11-15): the traps set out in the world, from the hand art
- Trap.cs (Assets/Scripts/World) now loads Resources/Art/trap_<item id> when a trap is placed or loaded and swaps out the prefab's primitive Body. Art children found by name take over: Body (tilts when broken), Catch, Bait and State_Set (shown while set, hidden with a catch). The interaction box is fitted round the art. Without the art the old primitives stay.
- trap_box_trap: the hand Box Trap split in Blender (Art\Blender_Work\trap_box_trap) into Trap_Body and its door, standing on the ground: State_Set is the door raised 14 cm in its guides, Catch is the door shut, Bait is the prefab's apple, smaller, at the back inside.
- trap_fish_trap: the hand Fish Trap laid on its side, half in the water (centre 5 cm under the surface, mouth to -Z), with the prefab's marker pole and flag moved in beside it and two smaller fish inside as Catch.
- trap_rabbit_snare: the stake driven point-down (4 cm in), its lashing near the ground, a new cord running out to the upright noose 25 cm to the side at rabbit-head height (facing the run, +Z). The prefab's rabbit is kept as the catch.
- No new textures: the hand materials are reused. ArtSwapTests.ASetTrapUsesItsArtByNameAndKeepsItsPrimitivesWithoutIt; ArtSwapTests and KnifeTests 50/50 passed. Approved in game by Mike 2026-11-15.

## tool_primitive_axe (2026-11-15): Primitive Axe, a ground stone celt lashed into a branch haft
- Per the recipe (2 sticks, 1 cordage, 1 stone): a 15 cm ground-stone celt, edge down (Unity -Y), poll standing 3.5 cm above the haft, set across a 64 cm branch haft (Grip_R to the end, plus 12 cm behind). Lashed with cord: tight wraps on the haft behind and in front of the head, an X over each side of the stone and a loop round the poll, all ray-cast onto the surfaces (as the Stone Pick Axe).
- Layout from ToolViewPrimitives (twoHanded, holdPitch -14, holdRoll -28). Anchors: Anchor_Grip_R (0, 0, 0), Anchor_Grip_L (0, 0, 0.18), Anchor_Tip (0, -0.08, 0.47).
- Meshes: PAxe_Haft, PAxe_Head, PAxe_Cord (7,992 tris in all).
- Materials: tool_primitive_axe_wood, _stone and _cord (2048), copied from the Stone Pick Axe's set so the two stone tools match.
  - Sources: Art\Blender_Work\tool_primitive_axe\.
- Unity: Assets/Resources/Art/tool_primitive_axe.prefab, ArtSwap.ToolSpecFor("primitive_axe") resolves; ArtSwapTests and HangPendulumTests 37/37 passed. Approved in game by Mike 2026-11-15 (chopping works).

## tool_wood_saw (2026-11-15): Wood Saw, a broad toothed steel blade on a turned wooden handle
- Shape after the general idea of primitive wood saw.png (not traced): a 21 cm oval wooden handle with a hanging-hole end, an iron ferrule, riveted iron cheek plates holding the blade, and a 50 cm steel blade (6 cm deep at the handle, 8.5 cm at the square tip) with real teeth along the lower edge (made coarser at Mike's ask: 9.5 mm pitch, 7.8 mm deep) (Unity -Y). Worn steel with rust spots, brighter along the teeth.
- ONE-HANDED since Mike's first look (2026-11-15: "this type of saw is meant to be 1 handed"): HeldTools.TwoHandedIds and the ToolViewPrimitives layout no longer have it two-handed, and the prefab has no Anchor_Grip_L. Layout: holdPitch -8, holdRoll -55. Anchors: Anchor_Grip_R (0, 0, 0), Anchor_Tip (0, -0.07, 0.62).
- Meshes: Saw_Handle 1,156 tris, Saw_Iron 644, Saw_Blade 484 (2,284 total). The blade's faces are ear-clipped (mathutils tessellate_polygon): Blender's own ngon fill overlapped across the teeth and showed a black band.
- Materials: tool_wood_saw_wood (1024), _iron (1024, metallic), _steel (2048, metallic).
  - Sources: Art\Blender_Work\tool_wood_saw\.
- Unity: Assets/Resources/Art/tool_wood_saw.prefab, ArtSwap.ToolSpecFor("wood_saw") resolves; ArtSwapTests and HangPendulumTests 37/37 passed. Approved in game by Mike 2026-11-15 (one-handed, coarser teeth).

## tool_flint_and_steel (2026-11-15): Flint and Steel, a C-striker and a knapped flint, both in the right hand
- Laid out as the primitive (one-handed, holdPitch -10): a 4.5 cm knapped flint shard (flat facets, black-grey with a little tan cortex) held at the grip, and below it a forged C-shaped steel striker, 9.5 cm across Unity X, its two ends curled up into scrolls. The striking face (the underside of the bar) is worn bright.
- Meshes: FS_Steel 2,324 tris, FS_Flint 432 (2,756 total). Anchors: Anchor_Grip_R (0, 0, 0), Anchor_Tip (0, 0, 0.07).
- Materials: tool_flint_and_steel_steel (1024, metallic), tool_flint_and_steel_flint (512).
  - Sources: Art\Blender_Work\tool_flint_and_steel\.
- Unity: Assets/Resources/Art/tool_flint_and_steel.prefab, ArtSwap.ToolSpecFor("flint_and_steel") resolves; ArtSwapTests and HangPendulumTests 37/37 passed. Approved in game by Mike 2026-11-15.

## tool_pouch (2026-11-15): Pouch, a gathered leather drawstring bag on a cord loop
- 12 cm across, 16 cm tall, hanging (HangingIds): a round leather bag gathered into a frilled neck, tied twice round with cord, two knotted tails, and a cord loop up round the grip (as the Fish Trap's).
- Meshes: Pouch_Leather 3,456 tris, Pouch_Cord 2,104 (5,560 total). Anchors: Anchor_Grip_R at the loop (origin), Anchor_Tip (0, -0.12, 0).
- Materials: tool_pouch_leather (1024, dark tan with creases), tool_pouch_cord (512).
  - Sources: Art\Blender_Work\tool_pouch\.
- Unity: Assets/Resources/Art/tool_pouch.prefab, ArtSwap.ToolSpecFor("pouch") resolves; ArtSwapTests and HangPendulumTests 37/37 passed. Approved in game by Mike 2026-11-15.

## tool_clay_pot (2026-11-15): Clay Cooking Pot, a fired terracotta pot with a strap handle
- Held up in the palm as the primitive (not hung; holdPitch -6): a 13 cm round-bellied pot, 13 cm tall, short neck and flared rim, open with a sooty inside, and a strap handle at the back (toward the hand, near the grip). Terracotta with dark fire-clouding.
- Meshes: Pot_Outer (wall, lip and handle) 1,912 tris, Pot_Inner 912. Anchors: Anchor_Grip_R (0, 0, 0), Anchor_Tip (0, 0.04, 0.10).
- Materials: tool_clay_pot_clay (1024), tool_clay_pot_inside (512).
  - Sources: Art\Blender_Work\tool_clay_pot\.
- Unity: Assets/Resources/Art/tool_clay_pot.prefab, ArtSwap.ToolSpecFor("clay_pot") resolves. ArtSwapTests and HangPendulumTests 37/37 passed. Approved in game by Mike 2026-11-15.

## tool_cooking_pot (2026-11-15): Cooking Pot, a cast-iron bellied pot on a wire bail
- Hanging (HangingIds): rim 17 cm across, belly 19.4 cm, 15 cm deep plus three stub legs; rolled lip, two cast ears and a wire bail whose top is the grip. Dark cast iron with a little rust, sooty inside. Built with Art\Blender_Work\vessel_util.py (shared with the kettle).
- Meshes: V_Iron (body, lip, ears, bail, legs) 2,524 tris, V_Inside 616. Anchors: Anchor_Grip_R at the bail top (origin), Anchor_Tip (0, -0.14, 0).
- Materials: tool_cooking_pot_iron (1024, metallic), tool_cooking_pot_inside (512).
  - Sources: Art\Blender_Work\tool_cooking_pot\.
- Unity: Assets/Resources/Art/tool_cooking_pot.prefab, ArtSwap.ToolSpecFor("cooking_pot") resolves. ArtSwapTests and HangPendulumTests 37/37 passed. Approved in game by Mike 2026-11-15.

## tool_potash_kettle (2026-11-15): Potash Kettle, a lidded cast-iron kettle with a spout
- Hanging (HangingIds): rim 20 cm, belly 24.4 cm, 17 cm deep plus legs, a pouring spout to the front (Unity +Z, as the primitive's), a lid with a knob (kept below the bail top so it clears the hand), ears and a wire bail. Same builder and iron as the Cooking Pot.
- Meshes: V_Iron 3,516 tris, V_Inside 616. Anchors: Anchor_Grip_R at the bail top (origin), Anchor_Tip (0, -0.16, 0).
- Materials: tool_potash_kettle_iron (1024, metallic), tool_potash_kettle_inside (512).
  - Sources: Art\Blender_Work\tool_potash_kettle\.
- Unity: Assets/Resources/Art/tool_potash_kettle.prefab, ArtSwap.ToolSpecFor("potash_kettle") resolves. ArtSwapTests and HangPendulumTests 37/37 passed. Approved in game by Mike 2026-11-15.

## tool_tarp (2026-11-15): Tarp, a folded canvas bundle tied with cord
- Carried as the primitive (one-handed, holdPitch -8): 20 x 7 x 15 cm, four folded layers of oiled canvas (olive-tan, woven texture) tied round twice with cord (at Unity z 0.06 and 0.14), knots on top.
- Meshes: Tarp_Canvas 10,800 tris, Tarp_Cord 2,840 (13,640 total, inside the 15,000 tool budget). Anchors: Anchor_Grip_R (0, 0, 0), Anchor_Tip (0, 0, 0.14).
- Materials: tool_tarp_canvas (2048), tool_tarp_cord (512).
  - Sources: Art\Blender_Work\tool_tarp\.
- Unity: Assets/Resources/Art/tool_tarp.prefab, ArtSwap.ToolSpecFor("tarp") resolves. ArtSwapTests and HangPendulumTests 37/37 passed. Approved in game by Mike 2026-11-15.

## tool_axe (2026-11-15): Felling Axe, rebuilt for real (replaces the 2026-11-05 blockout)
- An American-pattern felling head (forged steel, dark with a polished bevel along the edge, a little rust): poll 4.6 cm above the handle, bit flaring to an 11.4 cm curved edge 15 cm below it, on a 73 cm hickory handle with a gentle S-curve, a thinner throat under the head, a swelled knob at the butt and grip grime toward the hand. The handle end shows through the eye.
- Layout from ToolViewPrimitives (twoHanded, holdPitch -14, holdRoll -28). Anchors: Anchor_Grip_R (0, 0, 0), Anchor_Grip_L (0, 0, 0.20) (the blockout had it behind the grip at -0.17; it now follows the code, as the Primitive Axe), Anchor_Tip (0, -0.14, 0.56) at the edge.
- Meshes: Axe_Handle 2,396 tris, Axe_Head 764.
- Materials: tool_axe_hickory (2048), tool_axe_head (1024, metallic). tool_axe_wood.mat and tool_axe_steel.mat are kept: every tool's material is copied from tool_axe_wood.mat.
  - Sources: Art\Blender_Work\tool_axe\tool_axe_v2.blend (the blockout's tool_axe.blend is kept).
- Unity: Assets/Resources/Art/tool_axe.prefab replaced. ArtSwapTests and HangPendulumTests 37/37 passed. Approved in game by Mike 2026-11-15.
## Hands polish backlog (Wave 1 follow-up, from Hands v3)

Hands v4, pass 1 (2026-11-21): skin, nails and cuff. In Unity, awaiting Mike's in-game check.
- Skin: basecolor lighter and warmer (mean sRGB 186,132,112 to 214,162,134) with the red-orange blotches toned down. Files: textures\hands_skin9_basecolor.png and hands_grip_skin9_basecolor.png, copied over Resources/Art/hands_skin_basecolor.png and hands_grip_skin_basecolor.png. Normal and mask maps unchanged.
- Nails: rebuilt as curved plates that follow the fingertip and tuck in at the cuticle (5 x 5 top grid plus a rim, 64 tris each), 10 to 13.5 mm long. New textures hands_nails_basecolor/normal/mask (512): pink bed, pale lunula, cream free edge with a curved smile line, faint lengthwise ridges, smoothness about 0.68. hands_nails.mat now uses the maps with _BaseColor white.
- Cuff: new Cuff_R and Cuff_L meshes (a band with a rolled lip and a short inner wall that hides the open sleeve end), using hands_sleeve with UVs on the plaid panel. 28 segments on the idle hands and 22 on the grip hands. Added under Model_R and Model_L in both prefabs, with the same transform as the sleeve.
- Triangles: idle 14,960 (skin 6,080 x 2, nails 320 x 2, sleeve 800 x 2, cuff 280 x 2); grip 14,976 (skin 6,148 x 2, cuff 220 x 2).
- Sources: Art\Blender_Work\hands\hands_player_v4.blend and hands_player_grip_v4.blend (the v3 files are untouched); exports in hands\v4_export; the helper code is in the polish_util text block. The v3 Resources files are backed up in the Unity project's Temp\HandsBackup_v3.
- Tests: HandsTests and ArtSwapTests 99/99.
- Mike approved the skin tone (2026-11-21).
- Rifle hold (code, 2026-11-21, rounds 2 to 4, from Mike's photo of a man shooting): the right hand is on the right side of the stock's wrist, turned 90 degrees from palm-down, so the palm faces the wood and the back of the hand faces out. The fingers wrap under to the trigger and only the thumb goes over the top. PlayerHands.SeatHandUnderStock rolls the wrist ToolActionRules.RifleHandRoll = 90 degrees about the forearm and moves the hand RifleHandShiftX/Y/Z = 0.2 cm right, 1.8 cm down and 0.5 cm forward in the view; the rifle itself does not move. The thumb: PlayerHands.ShowRifleThumb swaps the grip hand's Skin_R and Nails_R meshes for Resources/Art/hands_grip_rifle_skin_R and hands_grip_rifle_nails_R (the thumb bent 60 degrees up over the stock, weighted along the thumb from its tip; the source is Art\Blender_Work\hands\thumb_weights.txt) only while the rifle is held. All values were tuned live in Play Mode. RestOfToolsInHandTests checks the palm's offset from the grip point.
- Finger skeleton (2026-11-21): Art\Blender_Work\hands\hands_player_grip_rig.blend has an armature, HandRig_R, on the grip hand's right Skin_R and Nails_R. The bones are forearm, palm (the wrist), and thumb1-3, index1-3, middle1-3, ring1-3 and little1-3; the joint positions are in rig_joints.json. Skin weights are Blender's automatic weights (bone heat), and the nails copy the weights of the nearest skin vertex. Bone local X is the finger's bending axis. The text block 'posing' has helpers (reset, rot_world, move_world, curl), and 'views' renders the review views. The tool-specific grips can now be posed the same way.
- Rifle grip from the skeleton (Mike: palm on the right side of the stock, thumb across the top, fingers barely visible under it, index on the trigger): the whole arm turns 35 degrees about the rifle's side axis around the stock's wrist and moves 2.8 cm forward, 0.6 cm down and 2.5 cm right (hand units). The index finger has IK to the trigger (target T_index), the thumb has IK over the top (T_thumb), and the middle, ring and little fingers curl a further 20/12/8 degrees. It is baked to Resources/Art/hands_grip_rifle_skin_R, _nails_R, _sleeve_R and _cuff_R (the sleeve and cuff are moved rigidly with the forearm). PlayerHands.ShowRifleThumb swaps all four in while the rifle is held and puts the plain meshes back for every other tool. SeatHandUnderStock (roll 25 degrees, shift) still runs before it.
- Rifle grip, round 2 (2026-11-21) was tried (the fingers coming up from under the stock on the player's side, saved in hands_player_grip_rig_pose2.blend), but Mike preferred round 1 on the straight stock, so the game uses round 1 again: the pose described above, baked to the four hands_grip_rifle_*_R meshes. Mike: 'that's the best right hand yet'.
- The rifle stock is straight (2026-11-21, Mike: "should not be angled or split, it should run directly back"): the 42-degree dropped butt is replaced by a straight lofted butt 0.415 m behind the receiver (Art\Blender_Work\tool_bolt_action_rifle\tool_bolt_action_rifle_straight.blend). The wood was re-unwrapped and re-baked (the textures are named *_v2), and the FBX and wood maps in Resources/Art were replaced. The old bent-stock files are backed up in the Unity project's Temp\RifleBackup_bent. RestOfToolsInHandTests and ArtSwapTests pass 93/93; the butt is still out of view.
- Still to do: palm creases, hair cards, and tool-specific grips. SSS stays blocked.

Lighter and warmer skin tone (the hands read dark in overcast light), nail texture (nails are a flat pink material), deep palm creases, real hair cards on the back of the hand and forearm, a Subsurface Scattering skin material (blocked today: the viewmodel pass draws after HDRP's SSS pass, so SSS renders black), a separate cuff on the sleeve (the sleeve silhouette is slightly faceted), and tool-specific grip variants (the grip hand is one shape for every tool). Photo references for this: Art\References\Hand_Photos (6 photos, 3 added 2026-11-07).

## Wave 4 additions: item groups the first list left out

Each needs an item_<id> prefab for the dropped object and, later, an icon. Sizes are rough and marked to confirm.

| Group | Examples | Notes | Priority |
|---|---|---|---|
| Handmade furniture goods | Rough Stool, End Table, Chair, Table | Workshop output; also sold at the Trading Post. 0.4 to 1.4 m. | Medium |
| Maple goods | Sap (in a bucket), Maple Syrup, Maple Sugar items | Syrup is a bottle or jar; the sap bucket is already in Wave 1. | Medium |
| Ash, potash and lye goods | Ash, Kindling Bundle, Lye, Potash | Ash and Kindling Bundle are small and carried constantly. | Medium |
| Lumber | Boards, planks, split rails (as sold at the Trading Post) | Stacked-looking, 0.2 to 2.4 m. | Medium |
| Crafted parts | Cordage, Arrows, Wooden Wheels, Door Hinges, Hook Set, Box of Nails | Small; Nails and Hinges are Smithy goods. | Low |
| Carried-only gear | Sleeping Bag, Tent (rolled), Weather Station, Lamp Oil | These have no tool row on purpose; the dropped and carried look still needs art. | Low |
| Ammunition and hunting kit | Rifle rounds, field-dressed meat, Furs and Hides | Pair with the Carcass row. | Low |
| Forage items | Berries, nuts, fruit, mushrooms, edible plants | One shape and one material per species; see Plants/*.md for the species. | Low |
| Hand cart load | The cart's carried contents | Contents stay code-drawn primitives; no art needed beyond the cart. | none |

## Wave 7: UI and screens (new)

The game has about 30 UI scripts and no UI art at all: Assets/UI is empty, there is no item icon field, and every screen is built from code with UiKit. For a realistic-style game the UI should get a single theme so it stops looking like a debug overlay. Mike has trouble reading small on-screen text (2026-11-04), so legibility is the first rule for every item below.

| Group | What | Notes | Priority |
|---|---|---|---|
| UI theme and kit | Panel, button, tab, slider, bar, scroll, checkbox, tooltip and toast styles; the colour palette; the border and paper or wood look | Mike chose the "Ledger" look 2026-11-20 (parchment in a dark wood frame, brown ink). Screens approved by Mike 2026-11-20. HUD, Pause Menu, Main Menu and toasts approved by Mike 2026-11-20 (see ui_ledger below). | High |
| Fonts | One body font and one display font, with a licence that allows a sold game | Alegreya Sans (body, regular and bold) and Alegreya Bold (titles), SIL Open Font Licence (sold games allowed; no Reserved Font Name). In Unity 2026-11-20 with the OFL files beside them. | High |
| HUD | Survival bars (Health, Stamina, Body temperature, Hydration, Hunger, Load), compass strip, minimap frame and north marker, crosshair, the interaction prompt line, the tool status line, the equipped-tool name fade, the weather HUD line, discovery toasts and screen banners | These are on screen all day. Crosshair and the status line matter most. | High |
| Item icons | One 128 x 128 icon per item (already a Wave 4 row) | Render from the item prefab when possible so the style matches. | Medium |
| Menus and screens | Main Menu, Pause Menu, New Game (difficulty and land picker, Land Purchase panel), Save and Load slot panel, Loss screen ("Mother Nature beat you" banner and restart or load choice) | The Loss screen needs its own art. | Medium |
| Management screens | Inventory, Crafting and Build, Storage Transfer, Trading Post, Workshop, Smithy, Potter's screen, Finances and Statement, Journal (Know-how and discovery pages), Weather screen, Quantity Stepper | Share the UI kit; Journal and Finances can have paper or ledger styling. | Medium |
| Map art | World Map, Minimap terrain colours and map icons (64 x 64) | MapIcons.cs exists; the map icons row in Wave 4 covers the sprites. | Low |
| Scenes | Bootstrap, Loading, MainMenu and World scenes: the Loading screen and Main Menu background art | Needs a title picture (see Wave 10). | Medium |

## ui_ledger (2026-11-20, Wave 7 theme)

- Mike picked direction A, "Ledger", from three mockups (Ledger, Field Kit, Woodsmoke) on 2026-11-20.
- Sources: Art/Blender_Work/ui_ledger holds gen_ui_kit.py (makes the sprites with numpy and PIL, no Blender), mockups.py and the three mockup images. Sprites: Assets/Resources/Art/ui_panel.png (1024 px, parchment in a 26 px wood frame, 9-slice border 72, drawn at 1/1.4 so the frame is about 19 px), ui_card.png (96 px, border 14), ui_button.png (96 x 64, border 16), ui_track.png (64 x 32, border 10; not used yet). Card, button and track are near-white and tinted by UiKit's colours.
- Fonts: Assets/Resources/Fonts/AlegreyaSans-Regular.ttf, AlegreyaSans-Bold.ttf, Alegreya-Bold.ttf (a static 700 instance of the variable Alegreya), OFL_alegreya.txt, OFL_alegreyasans.txt. The digits in all three were remapped to Alegreya's lining tabular figures (one.tf and so on), because the default old-style figures sit low and are harder to read; the OFL allows the change (no Reserved Font Name).
- UiKit (code): Ink, InkMuted, InkAccent (green ink), InkWarning (red ink) and HeadingColor for text on the parchment; Cream, Muted, Accent and Warning kept for text over the world. Images given PanelColor get the framed parchment and Raised/Hover/Selected the paper card; new helpers Heading, Card, LabelColor. Text under 26 is raised 12% (never past 26); 26 and up were already agreed with Mike and are left alone. Bold text uses the real bold face.
- Screens switched to ink (20 scripts): GameScreens, Inventory, Crafting, Journal, Trading Post, Weather, World Map (map labels stay cream, now clipped to the map), Storage, Finances, Workshop, Smithy, Potter, Wait, Loss, Slot panel, Land Purchase, the difficulty panel of the Main Menu, Quantity Stepper, Hover Tip. Rich-text colours in the screens moved to ink equivalents; unavailable recipes dim to 72% instead of 45%.
- Tests: UiThemeTests (17). Full EditMode suite 2286/2286 passed 2026-11-20.
- HUD and menus (2026-11-20, after Mike approved the screens): the scene-built UI is restyled by Editor/LedgerUiApplier.cs (menu Homestead/UI/Apply Ledger Look, safe to re-run; it saved World and MainMenu). HUD text keeps light letters on dark-brown paper plates (ui_card tinted), the bars have rounded tracks (ui_track), the compass is a wood strip (new ui_wood.png, 512 x 64, border 16) with its letters in Alegreya (CompassHud uses UiKit's fonts), the minimap rim is wood-coloured, discovery and Journal toasts are paper notes in ink with an Alegreya Bold heading. Pause Menu and Main Menu: paper buttons with bold ink labels, Alegreya Bold titles, the New Game confirm dialog on the parchment panel, a dark-brown menu background. The "Account statement ready" line moved under the compass (ToolHud, Mike's report). UiThemeTests now 19 (adds a scene check); full EditMode 2288/2288.
- Finished 2026-11-20 (Mike asked for Wave 7 then Wave 10):
  - Loss screen: Resources/Art/loss_background.png (1920 x 1080), the player's own homestead in a snowy winter dusk, graded from an in-game 4K capture (the Small Cabin left, snow added). LossScreen shows it behind a right-hand parchment panel; the title is Alegreya Bold.
  - Warning banners (ScreenBanner and the ToolHud notice): a parchment slip with a red-ink rim and red heading (UiKit.NoticeSlip, NoticeRed, NoticePaper). The HUD notice now sits under the compass instead of over it.
  - Loading scene: it was empty (black). New LoadingScreen.cs, added to Loading.unity by the applier: the title picture, the logo, and a parchment slip with "Loading your homestead..." and a bar on GameManager.LoadProgress.
- Wave 7 is complete.

## Wave 8: screen effects, particles and sky (new)

Wave 6 lists rain, snow, lightning, smoke and fire sprites. These are the effects and sky items it did not cover. Code exists today only for campfire flame, kiln smoke and precipitation particles; the sky comes from DayNightCycle with no sky asset.

| Group | What | Notes | Priority |
|---|---|---|---|
| Sky | Sun, moon, stars, cloud layers, a sky material or HDRI that works with DayNightCycle and the weather states (Clear, Overcast, Rain, Snow, Thunderstorm, Cold Front, Heat Wave) | It sets the mood of every screen. Confirm how the sky is drawn today before ordering. | High |
| Weather screen effects | Fog or mist for cold fronts, heat haze for heat waves, rain on the camera optional | Fog and heat haze can be post-process, not sprites. | Medium |
| Body-state overlays | Shiver and cold vignette, overheating haze, the Player Collapse fade to black and "wake weakened" look, low-stamina and tired visual | Mostly post-process and screen-space textures. | Medium |
| Water effects | Splash and ripple sprites for wading, fish jumps and the bobber, creek flood foam and flow, pond shore edge | Pair with the Wave 6 water surface. | Medium |
| Action particles | Wood chips and dust from the Axe, stone chips from the Pick Axe, dirt from the Shovel, sparks at the Smithy, hammer hit flash, steam from boiling sap and the Potash Kettle, bellows or kiln glow | Small sprite sheets; audio already exists for several of these. | Medium |
| Footsteps and ground | Footprints in snow and mud, leaf litter and snow cover (the terrain layers are painted by code today) | Low until terrain art lands. | Low |
| Breath and body | Cold breath puffs, sweat | Optional polish. | Low |
| Fire and ash | Ash and ember sprites in the campfire, kitchen hearth, Bloomery and Smithy; the Ash pile left after a fire | Pair with effect_campfire. | Low |

## Wave 9: art for designed-but-not-built systems (new, placeholder list for later)

None of these exist in the game yet, so nothing needs to be made now. They are listed so the total scope is visible and so each is designed with a swap point from the start (the standing art rule). Source docs are named in each row.

| Group | What | Source doc | Notes |
|---|---|---|---|
| Livestock | Chickens, goats, rabbits (and later sheep, pigs, cattle, horses) | Livestock_System.md, Livestock/*.md | Coop, Rabbit Hutch, Goat Shelter, feeders and troughs, hay and straw. |
| Stage 2 buildings | Root Cellar, Smokehouse, Storage Shed, Greenhouse or cold frame, Hand-Pump Well, composting bins | Structures_Roadmap.md, Building_Housing_System.md | Need spoilage and gardening first. |
| Stage 3 and 4 buildings | Large Cabin, Cottage, Barn, Orchard trees, Windmill, Farmhouse, Equipment buildings, Well House | Building_Housing_System.md | Housing tiers. |
| Power | Solar panels, battery bank, wiring, lights and the Flashlight item | Power_System.md | Waits on the system. |
| Farming and gardens | Garden beds, crops and their growth stages, tools such as a hoe, a scythe, hay | Structures_Roadmap.md | Not designed yet. |
| Vehicles | UTV and trailer (the hand cart is already in Wave 3) | Vehicles_and_Hauling.md | Last in the build order. |
| Tanning | Tanning rack and tools (references saved: tanning rack.png, tanning tools.png) | none yet | Not in code. |
| Fences, paths and markers | Fence posts and rails, gates, property boundary markers, the dirt track already visible in the world | none yet | Confirm what the land purchase needs to show. |
| Predators and more wildlife | Fox, Coyote, Bobcat | Wildlife_System.md, Feature_Backlog.md | Deferred. |
| More hunting and trapping gear | Crossbow, shotgun, compound bow, muzzleloader, cage and deadfall traps, ground blinds and tree stands | Hunting_System.md, Trapping_System.md | Deferred. |
| Regions | Per-region terrain, plants, trees and wildlife looks | Regions_System.md | Parked until after the money phases. |
| Digging and mining | Dug terrain, ore deposits, mine or pit features | Mike's idea 2026-11-04 | Only where realistic; ore is bought otherwise. |

## Wave 10: branding, store and marketing art (new)

Nothing exists. The 07_Marketing and 03_Concept_Art folders are empty. This wave is only needed when the game is shown to others, but the logo and icon are cheap to decide early, and the title picture is what the Main Menu and Loading screen need.

| Item | What | Notes |
|---|---|---|
| Game logo | "Homestead" wordmark, plus the motto "Survive. Prepare. Build. Prosper." as an optional lockup | Done 2026-11-20 (brand_logo below): a round wood-rimmed badge with an ink log cabin, pines, chimney smoke and a rising sun, plus a lockup with HOMESTEAD in Alegreya Bold and the motto. Original work. Approved by Mike 2026-11-20. |
| App icon | Windows executable icon in the standard sizes (16 to 256 px) | Done 2026-11-20: the badge as homestead_icon.ico (16 to 256) and Assets/Art/Branding/app_icon.png, set as the default icon in Player Settings. |
| Title and Loading art | A key painting or a rendered view of the finished homestead for the Main Menu and the Loading scene | Done 2026-11-20: an in-game 4K render of the homestead at autumn dusk, graded warm (title_background; menu_background is the same with the left side darkened for the logo and buttons). Redo from a new capture when the structures get their final art. |
| Store page art | Capsule and header images, a short and a long description, screenshots (needs a screenshot-friendly lit scene) | Made 2026-11-20 for Steam (Mike's choice): every store and library capsule, hero, logo, page background, icons, 7 screenshots and the store text, in 07_Marketing/Steam and Press_Kit. Approved by Mike 2026-11-21. |
| Trailer | A capture-based trailer | A 26 s teaser made 2026-11-20 (07_Marketing/Trailer): five in-game camera moves with the motto, the logo end card, the Main Menu theme. Remake with MarketingCamera and make_teaser.py when the world looks better. Approved by Mike 2026-11-21. |
| Press kit | Logo files, screenshots, fact sheet | Fact sheet and descriptions drafted 2026-11-20 (07_Marketing/Press_Kit/Store_Page_and_Press_Kit.md); release date, price, website and contact are TBA. Approved by Mike 2026-11-21. |

## Audio note

Audio is a separate list (Audio_System.md): the 22 original files plus thunder are sourced and wired. Known audio gaps: the Knife has no sound of its own (the blow borrows the item-drop cue, a hit plays the first 0.35 s of the Bow Release recording), and the fists have only their original punch sound.

## brand_logo (2026-11-20, Wave 10)

- Sources in Art/Blender_Work/brand_logo: gen_title_art.py (numpy and PIL; draws the badge and lockup with the Alegreya fonts and grades the two captures), title_raw.png and loss_raw2.png (4K captures from the World scene with a temporary camera; the time, weather and snow were set for the shot, then the player's saves were restored from a backup), and every output.
- Outputs: logo_emblem.png (1024), logo_lockup.png (2000 x 780, cream on transparent with a soft shadow), homestead_icon.ico, app_icon.png, title_background.png, menu_background.png, loss_background.png.
- In Unity: Resources/Art/title_background, menu_background (Sprite), loss_background, logo_lockup (Sprite); Assets/Art/Branding/app_icon.png (Player Settings default icon). All 1920-class textures use BC7 (CompressedHQ) to keep the dusk sky free of banding.
- Main Menu (LedgerUiApplier): the picture fills the screen (envelope fit), the logo and the buttons stand in a column on the darkened left, the cabin sits on the right; the old title text is kept, hidden.
- Tests: UiThemeTests 24 (adds the title art, the Main Menu logo and the Loading screen). Full EditMode 2293/2293.

## wave8_fx (2026-11-20): action particles, body-state screen effects, water, breath, fire glow, footprints (approved by Mike 2026-11-20)

- Code: World/WorldFx.cs (one hidden "World FX" object made by PlayerAudio.Awake; holds the particle systems and footprint decals), World/BodyStateFx.cs (a priority-20 global volume on the same object: Vignette, Color Adjustments, Lens Distortion), World/StructureGlow.cs (a persistent "Fire Glow" child switched by PlacedStructure.Rebuild).
- Hooks (one line each): AxeTool.Swing -> WorldFx.Blow; CampfireCooking.Update -> WorldFx.Steam; PlayerAudio stride -> WorldFx.Footstep; FishingRod cast, bite, reeling and while the bobber floats -> Splash / Ripple; PlacedStructure.Rebuild -> StructureGlow.Drive for the Kiln, Charcoal Pit, Bloomery and Smithy.
- Action particles: axes throw wood chips (lit cubes that tumble, bounce and shrink); the pick throws stone chips and dust, with sparks and a flash half the time; the shovel throws dirt clods and dust; the hammer gives a small flash, chips and dust.
- Steam over a boiling pot (and a wisp over cooking), drifting with the wind.
- Body states: cold = blue tint, less colour, a blue-edged vignette that throbs with shivering; heat = warm tint and a slow lens waver; low stamina (under 30%) = darker edges and drained colour in time with breathing; after a collapse = grey, dark, swimming view clearing over 30 s; weakened = slightly grey.
- Water: drops and a ripple ring when wading (every stride in water), at the cast, at a bite and while a fish thrashes; rings from the floating bobber; a fish jumps on deep water near the player every 20-60 s.
- Breath: puffs in front of the face below 4 C, more and bigger the colder it is, quicker when sprinting or tired.
- Footprints: HDRP decals (left and right) in snow (snow cover 0.3 and up) and in mud (Dirt on a rainy or same-day-wet ground), 48 at most, fading over 3 minutes.
- Fire glow: a flickering point light (Kiln 90 cd, Bloomery 120, Smithy 110, Charcoal Pit 30; the Campfire is 150) and rising embers at the mouth (Anchor_Mouth of the art); the Bloomery and Smithy swell and puff sparks every 3.5-7 s like bellows.
- Textures: Resources/Art/fx_ripple, fx_spark, fx_droplet, fx_footprint (gen_fx_wave8.py in Art/Blender_Work/world_fx); dust, steam and breath reuse fx_smoke_puff.
- Tests: Wave8EffectsTests (13).
- Note from testing: with snow at dusk the night is nearly black even near a lit fire (the auto exposure stops at its minimum); that is the existing Wave 6 lighting, not this wave, and worth a look later.

## store_marketing (2026-11-20, Wave 10 finish): Steam art, teaser, press kit (approved by Mike 2026-11-21)

- Mike's choices: Steam, a short teaser now, developer and publisher MM Custom.
- Captures: Assets/Scripts/Debug/MarketingCamera.cs (Editor only, Play Mode): sets day, hour, weather, snow and the fire glows per shot, its own camera with the HUD hidden and the hands not drawn; stills at 4K, teaser shots as one MJPEG file per shot. The game autosaves while it runs: the saves were backed up first and restored after (Temp/SaveBackup_W8).
- Steam (07_Marketing/Steam): header 920x430, small 462x174, main 1232x706, vertical 748x896, library capsule 600x900, library hero 3840x1240 (no text), library logo 1280x720, page background 1438x810, community icon 184x184 JPG, client icon 32x32, 7 screenshots 1920x1080. Capsules carry only the emblem and HOMESTEAD (Steam allows no taglines), from the autumn-dusk homestead capture graded like the title art. Source: Art/Blender_Work/brand_logo/gen_store_art.py.
- Teaser (07_Marketing/Trailer/homestead_teaser_1080p.mp4, 26.5 s, 1080p30, H.264 with AAC): spring "Survive.", fall "Prepare.", winter yard "Build.", snowfall at dusk "Endure.", autumn dusk "Prosper.", then the logo, "In development for Windows PC" and MM Custom; the Main Menu theme under it. A chopping close-up was recorded but dropped (the chips hid inside the trunk). Source: make_teaser.py.
- Store text and fact sheet: 07_Marketing/Press_Kit/Store_Page_and_Press_Kit.md (short description 255 characters, long description, features, tags, fact sheet, the asset checklist). Release date, price, website, press contact and PC specs are TBA.
- Noticed while capturing: seen from above, the spring pool's water reads as a black disc; the "SHORT" tag on the crafting screen is a pale amber that is hard to read on parchment.

## Small fixes (2026-11-21)

- Crafting screen: the SHORT marker and its lines moved from pale amber #8A5A06 to burnt orange #6B3500 (readable on parchment).
- Water (HomesteadWater.shadergraph and Art/Terrain/Water.mat): alpha now lerp(0.15, 0.82) over Clarity x 1.5 (was 0.93 over x 0.6), Clarity 1.6, Deep colour (0.06, 0.13, 0.12). A knee-deep pool read as a solid black disc; now a dark teal with the bed showing. Applies to every creek and pond.
- Dark snowy nights: not reproduced in clear or snowing nights on 2026-11-21; the earlier black frames were the auto exposure catching up after jumping the clock.
- Small Cabin: the hearth light shows through the gaps between the wall logs and onto the snow at night. Mike, 2026-11-21: keep it, for the Small Cabin (it reads as a lived-in cabin). Larger houses later should block their interior lights properly.

## Suggested order from here (updated 2026-11-20)

Waves 1 to 7 are complete, and Wave 10's logo, icon and title art are done. What is left:

1. Wave 8 effects (the sky itself was finished in Wave 6): action particles (chips, dirt, sparks, steam), body-state screen overlays (cold, heat, collapse, tired), water splashes and ripples, cold breath, fire ash and embers, footprints.
2. Hands polish backlog (Wave 1 follow-up).
3. Re-render the Main Menu, Loading and Loss pictures whenever the homestead looks notably better.
4. Wave 10 store art, trailer and press kit when the game is shown to others.
5. Wave 9 only as each system is built.

## structure_kiln (2026-11-15, Wave 2 pilot)

- Clay beehive dome on a 2.4 m flagstone pad, a ring of footing stones, an arched mouth framed with stones, a vent collar topping out at about 1.08 m. 22,208 tris (the dome mesh is used four times, once per state).
- Parts: Body (Kiln_Base: pad and stones; Kiln_Dark: mouth recess and vent hole); State_Empty and State_Loaded (clay dome), State_Firing (same dome, soot-tinted material), State_Done (ash-pale material, normal and mask maps kept); On_Firing (Kiln_Ember in the mouth and Kiln_VentGlow, HDRP Lit emissive, no shadows).
- Anchors as built and imported: Anchor_Smoke (0, 1.1, 0), Anchor_Mouth (0, 0.45, 0.95), Anchor_Stock (-1.45, 0, 0). Front faces +Z.
- Materials: structure_kiln_clay, _clay_soot, _clay_ash, _stone, _dark, _ember. Textures 2048: clay and stone basecolor, normal, mask. Sources in Art/Blender_Work/structure_kiln/.
- Unity: Assets/Resources/Art/structure_kiln.prefab (FBX structure_kiln_model.fbx). ArtSwapTests and KilnTests 65/65 passed. Approved in game by Mike 2026-11-15.
- Possible follow-up: the ember is fairly dim in full daylight; it reads at dusk and night. Easy to raise if wanted.

## structure_cabin (2026-11-15)

- Stacked round-log Small Cabin with saddle-notched corners overhanging 0.26 m, log gables, a thick thatch roof with a ridge roll, a ridge pole and five rafter pairs inside, hewn door jambs, a fieldstone footing and step, and the hearth: stone slab, stone-and-clay backing, clay chimney with a stone cap. 20,872 tris. Built to the exact primitive sizes (4 x 5 m, 8 courses of 0.28 m logs, wall top 2.54, ridge 3.4, door 1.1 x 1.96, HearthLocal (0.45, 0.48, -1.45)), so the existing colliders and RainCatchment still match.
- Code: Shelter.UseArt (called at the end of Bind for every shelter kind since the Tent, Tarp and Lean-To run; first written as UseCabinArt) loads Resources/Art/ + ArtSwap.StructureId(kind). With it, the primitives stop drawing (MeshRenderer.enabled = false) but keep every collider (floor, step, walls, lintel, hearth slab, both roof panels); the art's own colliders are removed. Without it, nothing changes. The hearth Campfire is FireManager's own object and is untouched. Test: ArtSwapTests.TheSmallCabinDrawsItsArtByNameAndKeepsThePrimitiveCollidersForShelterAndRain.
- Parts: Body (Cabin_Logs, Cabin_Thatch, Cabin_Stone, Cabin_Clay). Anchors (not read by code yet): Anchor_Hearth (0.45, 0.48, -1.45), Anchor_Door (0, 0.3, 2.5), Anchor_Chimney (0.45, 3.9, -2.19). Door faces +Z.
- Materials: structure_cabin_log (UV-procedural, shared side band plus an end-grain square), _thatch (UV), _stone, _clay. Textures 2048 (clay 1024). Sources in Art/Blender_Work/structure_cabin/.
- Tests after the change: ArtSwap, Cart, Setbacks, Regrow, WoodShed, Kitchen 221/221. Approved in game by Mike 2026-11-15.

## structure_tent, structure_tarpshelter, structure_leanto (2026-11-15, one run)

- Code: the Cabin's hook became Shelter.UseArt for every shelter kind (Resources/Art/ + ArtSwap.StructureId(kind): structure_cabin, structure_tent, structure_tarpshelter, structure_leanto), called at the end of Bind. Same rule: the primitives stop drawing and keep their colliders (the Tent's and Lean-To's BoxCollider on the Shelter, the Tarp's sheet collider). Test renamed to EveryShelterDrawsItsArtByNameAndKeepsThePrimitiveCollidersForShelterAndRain. ArtSwap, Cart, Setbacks, Regrow, WoodShed, Kitchen 221/221.
- structure_tent: canvas A-frame 1.6 x 1.25 x 2.2 (the primitive's size), sagging slightly between the poles, closed back, front flaps tied back, ridge pole and two end poles, corner stakes, guy lines fore and aft. 3,764 tris. structure_tent_canvas (2048, UV-procedural: weave, stains, dirt toward the ground); poles use structure_cabin_log; ropes structure_shelter_rope (plain hemp colour, no textures). Anchor_Door (0, 0, 1.1).
- structure_tarpshelter: blue poly tarp from the 2.0 m ridgeline at the front to 0.5 m at the back over 1.6 m, folded over the ridgeline, sagging in the middle, darker hem and steel grommets every 0.3 m; front poles, back stakes, ridgeline and guy lines to front stakes. 2,544 tris. structure_tarpshelter_tarp (2048); poles structure_cabin_log; ropes structure_shelter_rope.
- structure_leanto: two forked uprights with a ridge pole at 1.5 m, 15 laid branches with cut twig stubs down to the ground behind (2.4 x 1.8), covered in about 126 overlapping bough clumps. 13,076 tris. structure_leanto_boughs (2048, needles with dead brown patches); wood structure_cabin_log.
- All three: Unity prefabs at Assets/Resources/Art/<id>.prefab, sources in Art/Blender_Work/<id>/. Approved in game by Mike 2026-11-15 (looks and works).

## Wave 2 batch: structure_well, structure_outhouse, structure_workshop, structure_pottershed, structure_charcoalpit, structure_bloomery (2026-11-15, one run)

- Code (ArtSwap.cs): six new Spec rows after the Kiln's, each copying its builder's box, fixed-piece count and phases. The changing contents (water disc, work piece and product, shelf pieces, Clay, Sand, Logs, Branches, Charcoal, Ore, bars) stay primitives under one anchor at the origin (Anchor_Water, Anchor_Bench, Anchor_Shelf, Anchor_Stock), so they sit exactly where the builder draws them. New constants AnchorWater, AnchorBench, AnchorShelf. Charcoal Pit and Bloomery smoke stays PlacedStructure.Rebuild's own.
- Code (PlacedStructure.cs): BuildWell draws its water disc last (after the bucket) so a Well with art keeps it live. No other builder changed.
- Tests: ArtSwapTests.EachWaveTwoRowAgreesWithItsPrimitiveBuilder (box, fixed-piece count, water drawn last); AKindWithNoRowAndAnUnknownIdHaveNoSpec now expects 7 rows; BloomeryTests and CharcoalPitTests build with StructureVisuals.BuildPrimitive (they test the primitive look, which the art now replaces in Build). ArtSwap, Bloomery, CharcoalPit, Kiln, Pottery, PotteryPolish, WellDepth, WellPreview, WellWater, Workshop, Setbacks, WoodShed 375/375.
- Shared materials: structure_plank (new, UV-procedural weathered boards in 16 strips, Art/Blender_Work/structure_shared), structure_cabin_log, structure_cabin_thatch, structure_shelter_rope, structure_kiln_dark, structure_kiln_ember, structure_iron (new, plain dark metal).
- structure_well: fieldstone ring (three courses and a cap course) with a shaft lining down to -0.6 m, two posts, windlass with crank, pitched plank roof, stave bucket with iron bands on a rope. 6,420 tris. structure_well_stone (2048). Water disc stays the game's.
- structure_outhouse: board-and-batten shack 1.2 x 1.9 m on a plank floor, slanted plank roof, door ajar 35 degrees with a crescent moon, a seat inside. 686 tris. Planks only. The primitive's darkening as the pit fills is not drawn by the art (could become states later).
- structure_workshop: four log posts (Solid_Post_1-4), log beams, horizontal-board back wall (Solid_Wall), thatch roof higher at the open front (Solid_Roof), plank bench at the builder's spot (top 0.85), peg rail, chopping block, packed-earth floor. 3,048 tris. structure_workshop_earth (1024).
- structure_pottershed: the same build at 2.4 x 2.0 (Solid_Post_1-4, Solid_Wall, Solid_Roof), shaping table and two shelf boards with uprights at the builder's spots. 2,368 tris. structure_pottershed_earth (1024).
- structure_charcoalpit: earth-and-turf mound with sod lumps round the foot (clear in front), leaning sticks, vent, arched draught hole. State_Idle / State_Burning (soot-tinted material) / State_Done (ash-tinted), On_Burning ember in the hole and the vent. structure_charcoalpit_earth (2048) plus _soot and _ash tints.
- structure_bloomery: tapering clay stack to a chimney lip at 1.25 m, stone footing ring and four footing stones (none in front), a clay bellows pipe toward the back left, arched mouth. State_Idle / State_Smelting (soot tint), On_Smelting ember in the mouth and chimney. structure_bloomery_clay (2048, soot tint variant), structure_bloomery_stone (1024).
- All six: Assets/Resources/Art/<id>.prefab, sources in Art/Blender_Work/<id>/. Approved in game by Mike 2026-11-16.

## structure_smithy (2026-11-16)

- The Workshop's frame (Solid_Post_1-4, Solid_Wall, Solid_Roof, same footprint) with a sooty thatch (structure_smithy_thatch, a darker tint of structure_cabin_thatch) and a darkened plank wall (structure_smithy_plank). A dry-stone forge hearth at the builder's spot (0.9 x 0.8 x 0.7 at x -0.8, z -0.62) with a fire bed and mouth, and a stone chimney at SmithyRules.ChimneyX/Z rising to 3.0 m through the roof. Anvil (structure_iron) on a log stump at (0.55, z 0.15) with the horn toward +X, a quench tub, a rail of hanging tongs. 18,396 tris. structure_smithy_stone (2048). Floor reuses structure_workshop_earth (same pad).
- On_Working: the coals on the fire bed and the mouth glow (structure_kiln_ember) while a job runs (jobItem set and jobHours > 0, as the builder's glow). Smoke stays PlacedStructure.Rebuild's own chimney smoke.
- Code: ArtSwap row structure_smithy (staticPieces 21; the hot piece or finished good, the Iron Bars and the Charcoal stay primitives at Anchor_Stock at the origin). Tests: the Smithy added to EachWaveTwoRowAgreesWithItsPrimitiveBuilder, AllSpecs 8; SmithyTests and SmithyToolTests build with StructureVisuals.BuildPrimitive. ArtSwap, Smithy, SmithyTool 95/95. Approved in game by Mike 2026-11-16.

## structure_leachhopper (2026-11-16)

- A board box (slatted floor at 0.92 m, two-board sides to 1.35 m, structure_plank) on four log posts with cross braces (structure_cabin_log), a small spout trough under the middle, and a clay catch crock beneath (structure_clay_plain, a plain fired-clay material, no textures). 1,308 tris. Original design: the watermarked ash hopper.png was used for the general idea only.
- Code: BuildLeachHopper now draws the catch crock with the fixed pieces (before the water, Ash or Lye fill and the drip), so ArtSwap row structure_leachhopper keeps those three live at Anchor_Fill at the origin (staticPieces 10). Test rows: LeachHopper added to EachWaveTwoRowAgreesWithItsPrimitiveBuilder, AllSpecs 9. ArtSwap, HopperStock, PotashWorld 90/90. Approved in game by Mike 2026-11-16.

## structure_kitchen (2026-11-16)

- Covered Outdoor Kitchen at Setbacks.KitchenLayout's size (posts at +/-1.55, +/-1.35, roof at 2.45): four log posts (Solid_Post_1-4), log beams and three purlins, a board roof sloping up to the open front with battens (Solid_Roof), the prep table at the builder's spot with a low shelf (Solid_Table), a packed-earth floor (structure_workshop_earth reused). Hearth at HearthLocal (-0.35, 0.14, 0.15): a 1.5 m slab of flags, a dry-stone back wall over a clay core (structure_kitchen_clay, plain), cap stones, and a row of fire-ring stones at the front. 6,476 tris. structure_kitchen_stone (2048; _BaseColor 0.5 in Unity, the bake read too pale in game light). The fire is FireManager's own Campfire, untouched.
- Code: ArtSwap row structure_kitchen (staticPieces 31, no changing contents; boxSize unused as before, since PlacedStructure turns the Kitchen's box off and the Solid_* parts are what is aimed at and bumped into). Test: Kitchen added to EachWaveTwoRowAgreesWithItsPrimitiveBuilder, AllSpecs 10. ArtSwap, Kitchen, KitchenAddons 63/63. Approved in game by Mike 2026-11-16.

## Kitchen add-ons: structure_countertop, structure_grill, structure_slopsink (2026-11-16)

- structure_countertop: plank work surface at 0.9 m on four legs, a lower shelf with a clay crock, a back lip and a chopping board. 392 tris. structure_plank, structure_clay_plain.
- structure_grill: a dry-stone firebox over a clay core (0.88 x 0.45 x 0.5), flat slabs either side, foot stones, an ash bed in the middle where the grill's Campfire stands (WoodManager.GrillFireSpot, untouched), and a rack of five green sticks on two forked posts. structure_grill_stone (1024, _BaseColor 0.5), structure_kitchen_clay, structure_cabin_log, structure_greenwood (a green tint of structure_cabin_log). Anchor_Fire at (0, 0.45, 0).
- structure_slopsink: work-height table with a clay basin sunk in a board frame and rim, a drain spout at the front. structure_plank, structure_clay_plain. The water stays the game's (Anchor_Fill).
- Code: BuildSlopSink draws the drain spout before the water (staticPieces 11). ArtSwap rows structure_countertop (8), structure_grill (14), structure_slopsink (11, water at Anchor_Fill); new constant AnchorFire. Test rows added, AllSpecs 13. ArtSwap, Kitchen, KitchenAddons 63/63. Approved in game by Mike 2026-11-16.

## Wave 3 batch (2026-11-16, one run, Mike checks all at the end)

Sources: one shared file, Art/Blender_Work/structure_wave3/structure_wave3.blend (objects named <kind>_<part>), one FBX per id. Materials reused: structure_plank, structure_cabin_log, structure_iron, structure_shelter_rope, structure_clay_plain; new: structure_ash (plain dark grey), structure_charred (dark tint of structure_cabin_log), structure_rockstorage_stone and effect_campfire_stone (1024 bakes, _BaseColor 0.55).

- Code, WoodPile.cs: Rebuild now draws the primitives (BuildPrimitives, the old body) and then UseArt. When Resources/Art/structure_<kind> exists, the kind's first FramePieces(pile) primitives (the container) stop drawing and the art is shown (made once, kept across Rebuild, its colliders removed); everything drawn after them (stored Logs, Stone, water level, shelf boxes, tool pegs, the cart's load) stays the primitives. FramePieces: WoodStorage 2 (built only), RockStorage 6, WaterBarrel 1, RainBarrel 2, FoodCache 2, StorageBin 2, ToolRack 3, WoodShed 18, Shelving 7, HandCart 10. BuildBarrel now draws the Rain Barrel's rim before the water.
- Code, Tripod.cs: UseFrameArt (Resources/Art/structure_tripod replaces the drawn poles, lashing and chain; the poles keep their colliders) and UseVesselArt (the hung pot, clay pot or kettle shows its own tool art, Resources/Art/tool_<id>, hung by its grip from the chain's end at PoleHeight - 0.62; any Fill child is hidden; the primitive keeps its collider).
- Code, Campfire.cs: UseArt in Awake. Resources/Art/effect_campfire replaces the Stones ring; its Logs and Ash children take over the logs and ash fields the script switches, so burning down and going out work as before. Flames, embers, light and sound unchanged. This also applies to the kitchen hearth and grill fires (same prefab).
- Tests (ArtSwapTests): StorageAndPilesHideOnlyTheirContainerPiecesForTheirArt, TheTripodFrameAndItsVesselUseTheirArtAndKeepTheirColliders, TheCampfireArtTakesOverTheLogsAndAshItSwitches. ArtSwap, Tripod, TripodWorld, Cart, WoodShed, KitchenAddons, Setbacks, ClayPot: 248/249; the one failure, ClayPotTests.NoBuyAndSellLoopExistsBetweenTheTwoPots, comes from the 2026-11-15 market change (the store now sells the Clay Cooking Pot), not from the art; left for Mike to decide.
- structure_woodstorage: two ground rails on two sleepers, four corner stakes. 760 tris.
- structure_rockstorage: a border of 11 flat edging stones round a 0.6 m circle. 880 tris.
- structure_waterbarrel, structure_rainbarrel: open-topped stave barrels (0.67 x 0.95, 0.86 x 1.1) with four iron hoops; the rain barrel's top hoop is heavier (the rim). The water level stays the game's. 2,218 and 2,370 tris.
- structure_foodcache: lidded plank chest 1.0 x 0.6 x 0.7 with corner posts, a battened lid and rope handles. structure_storagebin: open plank bin 1.1 x 0.55 x 0.75 with corner posts.
- structure_toolrack: two posts on cross feet, rails at the peg heights (0.88 and 0.48), a top board; the pegs for stored tools stay the game's.
- structure_shelving: two uprights, a boarded back and four shelves at the builder's heights (centre z -0.08). The shelf boxes stay the game's.
- structure_woodshed: deck of boards on log skids, tall front and short back log posts with beams, slatted back and left side, board roof sloping down to the back. 1,404 tris.
- structure_handcart: plank bed with two-board sides and front, cross battens, an axle and two spoked wheels (12 felloes, 8 spokes, hub), handles to the back, a rest leg. 1,556 tris. The handle collider (CartHandle) stays the code's.
- structure_tripod: three poles crossing at 1.6 m, a rope lashing, an iron chain of alternating links to a hook at the chain's end. The hung vessel uses the approved tool art.
- effect_campfire: Stones (10 stones, r 0.55), Ash (a low lumpy bed), Logs (a teepee of five charred logs). 1,876 tris.
- All: Assets/Resources/Art/<id>.prefab. Approved in game by Mike.

## Wave 3 fixes after Mike's check (2026-11-16)

- Mike: everything looked good except the wood lying outside the Wood Pile and the Clay Cooking Pot looking too small on the tripod.
- structure_woodstorage rebuilt as a wide bed (3.0 x 2.1 m): four log sleepers from x -1.3 to 1.6, edge rails and four corner stakes, so the Logs (middle), Branches (+X), Sticks (-X) and Firewood (back) the game draws all lie on it.
- Tripod.UseVesselArt: a hung vessel is grown to the drawn vessel's width (about 0.4 m), capped so its bottom stays at or above 0.55 m (the fire's logs reach about 0.5 m), and hung by its top from the chain's end (the clay pot's grip is at its middle, so it had sat up on the chain). Measured in the editor: iron pot x2.06 (bottom 0.55), clay pot x2.58 (bottom 0.63), kettle x1.23 (bottom 0.70). ArtSwap, Tripod, TripodWorld 88/88. Awaiting Mike's re-check.

- Re-check 2026-11-16: the wood pile approved by Mike. The clay pot's size was better but it hung off to one side (its grip is at its handle); Tripod.UseVesselArt now also centres the vessel under the chain (measured offset 0.000 for all three vessels). ArtSwap, Tripod, TripodWorld 88/88.
- Approved 2026-11-16: Mike re-checked the tripod in game ("looks better now"). Wave 3 complete, 12/12.

## Wave 4 batch 1: raw materials (2026-11-16, one run, Mike checks all at the end)

- Pieces (Art\Blender_Work\item_wave4a\item_wave4a.blend, one file, one FBX per item): item_logs (one 8 kg log, 0.77 x 0.17 m, tapered with two knot bumps, sawn ends with growth rings), item_branches (a loose bundle of five dry sticks about 1.1 m long with side twigs), item_firewood (one split quarter, 0.43 m, bark on the round side, split grain on the flat faces), item_stone (a 5 kg fieldstone, 0.21 x 0.16 x 0.11 m), item_clay (a 3.5 kg slumped wet lump with small finger presses), item_charcoal (a heap of 15 snapped lumps showing the wood grain), item_ash (a small grey heap with dark specks). Tris 520 to 1,280 each.
- Materials: one per real material, baked at 1024 (bark, cut wood, stone, clay, charcoal, ash; 9 in all, no metal). Cut wood rings are centred on the log's axis, so the firewood's split faces show straight grain.
- Code, DroppedItems.cs: Spawn calls UseArt(go, item). When Resources/Art/item_<id> exists the tinted cube's renderer is hidden, the art stands on the cube's bottom face at real size (the cube's scale is undone) and the cube's box collider is fitted to the art, so the whole model can be looked at and picked up. The cube stays as the pickup and the saved position, so saves are unchanged. Items with no art keep their cube.
- Test (ArtSwapTests): ADroppedPileShowsItsItemArtStandingOnTheGroundInsideItsCollider. ArtSwapTests 34/34, HandsTests 62/62.
- Not covered yet: an item held in the hands is still the tinted placeholder (PlayerHands); a pile of 10 logs still shows one log. Both are later steps.
- All: Assets/Resources/Art/item_<id>.prefab. Approved in game by Mike.

## Dropped tools use their hand art (2026-11-16)

- Code, DroppedItems.UseArt: an item with no item_<id> art but a Wave 1 tool row (ArtSwap.ToolSpecFor) shows Resources/Art/tool_<id> on the ground. Carried containers (ToolActionRules.HangingIds, plus clay_pot and lantern) stand upright as authored; every other tool is laid whichever way is lower (rolled 90 degrees onto its side, or left as authored: the shovels' blades and the folded tarp already lie flat). Lights are dropped unlit (State_Lit hidden) and containers empty (Fill hidden). The art sits under an "Art" holder that undoes the cube's scale, so a rolled tool is never squashed. All 30 tools checked in a preview render.
- Test: the dropped-pile test also covers axe, fishing_rod, shovel, tarp (lying), bucket and lantern (upright, unlit, empty).

## Wave 4 batch 2: sticks, kindling, lumber, sand and the first Smithy goods (2026-11-16, one run)

- Pieces (Art\Blender_Work\item_wave4b\item_wave4b.blend, one FBX per item): item_sticks (three small dry sticks about 0.4 m), item_kindling_bundle (about 30 thin split sticks, 0.42 m, tied twice with cord), item_lumber (three rough-sawn boards 1.2 m x 0.14 x 0.022, stacked a little askew), item_sand (a tan heap with dark grains), item_iron_ore (a 3 kg rusty brown-black pitted lump), item_iron_bar (a hammered wrought-iron bar 0.42 x 0.026 m, scale and rust), item_nails_box (an open pine box with 34 nails inside and three lying on top). Tris 36 to 1,540 each.
- Materials: 9, baked at 1024; item_iron_bar_iron and item_nails_box_iron are metal (mask R = 1, _Metallic 1).
- Shared helpers now in Art\Blender_Work\item_util.py (shapes, the bark, grain and lump materials, a bake that handles objects with more than one material, masks, per-item FBX export, review renders).
- ArtSwapTests 34/34 (the dropped-pile test covers all 14 items). Approved in game by Mike.
- Approved 2026-11-16: Mike checked batches 1 and 2 and the dropped tools in game ("look good, carry on").

## Wave 4 batch 3: pottery (raw and fired) and the last Smithy goods (2026-11-16, one run)

- Pieces (Art\Blender_Work\item_wave4c\item_wave4c.blend, one FBX per item): fired item_clay_cup (9 cm with a handle), item_clay_bowl (17 cm), item_water_crock (32 cm, shouldered, rolled rim, two lug handles), item_brick_batch (six bricks in two crossed layers); the same four raw as item_dry_clay_cup, item_dry_clay_bowl, item_dry_water_crock, item_dry_brick_batch (same shapes, pale unfired clay); item_dry_clay_pot (the approved tool_clay_pot model itself, imported from its FBX and given raw clay, so the raw and fired pots match); item_door_hinges (a pair of forged strap hinges with knuckle, pintle plate and nail studs); item_hook_set (four forged S and J hooks tied with twine). Tris 264 to 2,824 each.
- Materials: fired terracotta (bricks rougher and redder), raw clay pale grey-tan and matte, forged iron with rust (metal) and twine; 12 in all, baked at 1024.
- Note: tool_clay_pot is authored upright (mouth up, Unity +Y); its extra length along Z is the strap handle. DroppedItems stands it upright as authored, which is right.
- item_util.py gained tube() (one seamless tube along a path, for handles, hooks and wire) and revolve() (a lathe from a profile, for pottery).
- ArtSwapTests 34/34 (the dropped-pile test covers all 25 items). Approved in game by Mike.
- Approved 2026-11-16: Mike checked batch 3 in game ("looks great, continue").

## world_stump (2026-11-16): the stump a felled hardwood leaves

- Piece (Art\Blender_Work\world_stump\world_stump.blend): a hardwood stump authored for a 0.30 m trunk, pivot at ground level, cut about 0.41 m up. An axe-felled top: a flat cut, the back-cut side a little lower, a ridge of torn fibres across the middle (hinge); a fresh cut face with growth rings, darker heartwood and a dark bark rim; furrowed bark up the sides; six root flares spreading into the ground (to 0.09 m below it). 4,544 tris. Two materials baked at 2048 (world_stump_bark, world_stump_wood).
- The bark is tinted to the trees' current placeholder bark (RGB 0.27, 0.20, 0.14) so stump and trunk don't clash; revisit when the trees get real art (Wave 6).
- Code, WoodManager.SpawnStump calls UseStumpArt: the cylinder's renderer is hidden and the art sits at ground level, widened by trunkRadius / 0.30 (so a sapling's stump is small and a big hardwood's wide) but never taller. The cylinder stays the MeshCollider the Shovel digs at; swallowing and digging out are unchanged (they destroy the whole stump).
- Test: ArtSwapTests.AStumpShowsItsArtWidenedWithTheTrunkOnTheGround (radii 0.12, 0.27, 0.5). ArtSwap, Regrow, Setbacks, Workshop 204/204.
- Approved in game by Mike.

## Wave 4 batch 4: fish and meat, raw and cooked (2026-11-16, one run)

- Pieces (Art\Blender_Work\item_wave4d\item_wave4d.blend, one FBX per item, 18 in all; materials built by item_wave4d\mats.py): four fish lying on their sides with tail, dorsal, anal and pectoral fins, an eye and a gill line: item_bass (33 cm, olive with a dark blotchy side stripe), item_bluegill (21 cm, deep-bodied, dark bars, orange chest, dark ear flap), item_catfish (40 cm, flat head, four barbels, blue-grey with dark speckles), item_crappie (27 cm, silvery with black spots); item_chicken_meat and item_waterfowl_meat (whole plucked trussed birds, goose-pimpled skin), item_turkey_meat (two big drumsticks with the bone ends showing), item_small_game_meat (two skinned rabbit hind legs, red with silvery membrane), item_venison (a 2.5 kg roast, dark red with membrane and fat). Each has a cooked twin of the same shape: cooked fish browned with faint grill marks and a white cooked eye; roast chicken golden, roast duck mahogany, turkey legs golden with browned bone ends, rabbit legs and venison seared dark brown.
- 22 materials (non-metal), baked at 1024. Tris 556 to 2,160 each.
- The meats are portions, not whole animals: whole carcasses stay with the Carcass row.
- ArtSwapTests 35/35 (the dropped-pile test covers all 43 items). Approved in game by Mike.

## Wave 4 batch 5: animal goods (2026-11-16, one run)

- Pieces (Art\Blender_Work\item_wave4e\item_wave4e.blend, one FBX per item): item_chicken_eggs (two brown eggs lying down), item_feathers (seven barred brown and cream feathers with quills), item_deer_hide (a rolled hide, 0.46 m, fur outside, leather at the ends, tied with two thongs), item_small_furs (two flat pelts with leg flaps, grey-brown fur, leather underneath), item_deer_antlers (a shed pair, three tines each, grooved bone paling to polished tips), item_sinew (a looped hank of amber strands, tied), item_goat_milk (a salt-glazed stoneware jar with the milk showing at the neck). Tris 320 to 4,832 each.
- 13 materials (non-metal), baked at 1024. Material helpers for fur and feathers are in item_wave4e (fur stretched hair, barred vanes).
- Watch in game: the rolled deer hide may read more like a tan tube than fur; darken or rough it up if so.
- ArtSwapTests 35/35 (the dropped-pile test covers all 50 items). Approved in game by Mike.

### Wave 4 batch 6: forage (approved in game by Mike, 2026-11-19)
- Ids: blackberries, raspberries, chestnuts, hickory_nuts, walnuts, wild_apples, pawpaw, morel, oyster_mushroom, dryads_saddle, dandelion, wild_onion, cattail, tall_grass.
- Sources: Art\Blender_Work\item_wave4f (geo.py helpers). 14 FBX, 24 materials, 1024 px bakes.
- Unity: Assets/Resources/Art/item_<id>.prefab, shown on dropped piles by DroppedItems.UseArt. Added to the ArtSwapTests dropped-pile test (35/35 pass).
- Long items (cattail 1.0 m, tall_grass 0.8 m) lie flat on the ground.

### Wave 4 batch 7: furniture and crafted parts (approved in game by Mike, 2026-11-19)
- Ids and what each is, per its recipe:
  - rough_stool: split slab seat (0.38 x 0.29) on three splayed bark branch legs, tenons showing through the top, 0.47 tall.
  - end_table: three-plank top 0.46 square on cleats, four branch legs, a low two-plank shelf on stretchers, 0.60 tall.
  - chair: three-plank seat at 0.45, branch legs and back posts to 0.98, two back slats and the seat joints lashed with cordage, rungs.
  - table: five-plank top 1.40 x 0.80 at 0.75 on cleats, four log legs, branch stretchers lashed with cordage (2 cordage).
  - wooden_wheel: two solid wheels sawn from one log (0.48 across, 6.5 cm thick, bark rim, axle hole, growth rings and checks), stacked.
  - arrows: three 0.74 m stick shafts with knapped flint points bound on with cord; no fletching, since the recipe is 1 stick and 1 stone.
  - cordage: a coiled hank of two-ply twisted grass cord with a loose tail (the twist is a per-vertex 'rope' attribute read by the shader).
  - tripod: three 1.6 m branch poles bundled and lashed near the top, as carried; the placed tripod is structure_tripod.
- Sources: Art\Blender_Work\item_wave4g (geo7.py shapes, mats7.py materials). 8 FBX, 18 materials, 1024 bakes (the table 2048). 350 to 5,000 tris each.
- Unity: Assets/Resources/Art/item_<id>.prefab. Item art is placed as authored (only tool art is rolled), so the furniture stands upright. Added to the ArtSwapTests dropped-pile test (35/35 pass).

### Wave 4 batch 8: liquids and supplies (approved in game by Mike, 2026-11-19)
- Ids and what each is:
  - water, water_excellent, water_good, water_questionable, water_unsafe, maple_sap: one buff clay jar (13 x 16 cm, about 1 L) shared by all six (same mesh and UVs, one material item_water_jar_clay). The open jars show the liquid inside: dark and clear (Excellent), clear (Good), cloudy green (Questionable), muddy brown (Unsafe), pale and watery (sap). Purified water is the same jar with a cork in it.
  - maple_syrup: a small two-tone stoneware jug (dark brown dipped top, tan bottom) with a handle and a cork.
  - lye: a grey salt-glazed crock with a wooden lid tied down with cord.
  - lamp_oil: a painted red oil tin with a capped spout and a wire handle, a little worn.
  - potash: a small pile of grey-white lumps.
  - rifle_rounds: four loose brass cartridges with copper bullets.
  - sleeping_bag: an olive canvas roll with two leather straps and buckles; the ends show the rolled layers.
  - tent: a tan canvas roll tied with cord, the two poles alongside.
  - weather_station: a walnut board with a brass barometer (ticked dial, needle) and a thermometer with its red column.
- Sources: Art\Blender_Work\item_wave4h (geo8.py, mats8.py). 14 FBX, 30 materials (the six water surfaces are plain baked colours), 1024 bakes, 700 to 3,400 tris.
- Test change: the last part of ADroppedPileShowsItsItemArtStandingOnTheGroundInsideItsCollider looked for an item with no art to check it keeps its cube. There are none now, so it builds a made-up ItemDefinition (test_item_without_art) instead. ArtSwapTests 35/35.
- With this batch every one of the 116 items in the database has dropped art.

### Wave 4 item icons (approved in game by Mike, 2026-11-19)
- All 116 items have a 128 x 128 transparent icon at Assets/Resources/Icons/item_<id>.png (Sprite, uncompressed, no mipmaps), rendered from the same art a dropped pile shows (item_<id>, or the tool_<id> art unlit and empty for a Tool with no item art), so icons always match the world.
- Renderer: Assets/Scripts/Editor/ItemIconRenderer.cs, menu Homestead > Art > Render Item Icons (re-run after changing any item art; about 1 s per icon). It uses a preview scene, so the open scene is never touched.
  - Lighting: three directional lights (key 3.2, fill 1.1, rim 1.6 lux) that turn with the camera, fixed exposure EV -1.2, and a volume on layer 29 (unused; 31 is Unity's preview layer) with no sky, tonemapping, bloom, vignette or fog. Without "no sky" the open scene's sky ambient lit everything far brighter than the lights and the icons changed with its time of day.
  - HDRP needs HDAdditionalLightData added by hand on lights made in a preview scene.
  - Transparency: each icon is rendered on black and on white; alpha comes from the difference (HDRP's colour buffer keeps no alpha). Rendered at 512 and box-filtered to 128.
  - Poses: anything more than 2.6 times longer than wide (tools, arrows, poles, rolls, fish) lies across the icon on a diagonal with its broad side to the camera; the rest is a three-quarter view from the front-left at 34 degrees (flat piles 50, open containers of liquid 58 so the water grades show).
- Game code: Assets/Scripts/Inventory/ItemIcons.cs, ItemIcons.For(id) loads and caches Icons/item_<id> (null when there is none). InventoryScreen.AddRow shows it as a 72 px picture down the left of each row; the name line, the stepper and the info text start after it.
- Test: ArtSwapTests.EveryItemHasAnIconTheInventoryCanFind (every item in ItemDatabase has a 128 px icon; unknown ids give null). ArtSwapTests 36/36, ClayPotTests 16/16.
- Hover (Mike, 2026-11-19): hovering an icon shows the item's name beside the pointer (Assets/Scripts/UI/HoverTip.cs, a reusable component: HoverTip.Add(graphic, text, onClick)). One shared label per canvas, 22 px cream on a dark panel, drawn on top and kept on screen; a click on the icon still does the row's action; scrolling passes through. Test: HoverTipTests (the test assembly now references UnityEngine.UI).
- Other screens (Mike, 2026-11-19): the Trading Post (buy and sell rows) and Storage rows show the icon with its hover name through QuantityRowUi.SetIcon(itemId, name) (72 px, same layout as the Inventory); the Craft and Build list rows show a 48 px icon for every entry that is an item (every Craft recipe and the Tripod; the Campfire and the piles have none). A click on an icon does what clicking its row does. Test: HoverTipTests.ATradeOrStorageRowShowsTheItemsIconAndNameOnHover. Approved in game by Mike 2026-11-19.
- Build tab (Mike, 2026-11-19): 46 icons at Icons/build_<key> (ItemIconRenderer.RenderBuild, also run by the menu item): every structure kind from its art, each ... Site showing the building it becomes, the Campfire from effect_campfire and the Tripod standing (structure_tripod). The Lean-To and Tarp Shelter are seen from 58 degrees so their roofs show. ItemIcons.ForBuild(entry key) finds them. The Craft and Build list rows became two lines (74 px: name and marker on top, the cost below, wrapping) with a 62 px icon, which also fixed long Build names running into the cost text. Test: ArtSwapTests.EveryBuildEntryHasAnIcon. ArtSwapTests and HoverTipTests 39/39; Cart, CharcoalPit, Smithy and SmithyTool 175/175. Approved in game by Mike 2026-11-19 (with the Trading Post and Storage icons).

## animal_deer (2026-11-19): white-tailed buck and its carcass, the first Wave 5 animal (approved in game by Mike, 2026-11-19)
- Real size: about 1.0 m at the shoulder, 1.29 m to the top of the head, 1.74 m nose to tail (the primitive deer was oversized). A grey-brown coat with a darker spine, white belly line, inner legs, throat patch, chin and under the tail, lighter lower legs, black nose and hooves, glossy eyes; a 10-point rack.
- Mike's shaping note (2026-11-19): a bigger rump and upper hind legs so the rear doesn't come to a point; the tail hangs down.
- Pose_Alive: Body (8,160 tris), Ears (with the eyes), Antlers. Pose_Dead: the same three, made by deforming the standing deer (rolled onto its side, legs sagged to the ground, neck and head laid down), so it shares the standing UVs and textures. Vitals empty low in the chest behind the front leg (0.66 m up).
- Sources: Art\Blender_Work\animal_deer (deer_geo.py: the joint table, skin_body, seat, dead_point; deer_mats.py: the coat by body region and facing). Materials animal_deer_coat (2048), _ear, _eye, _antler (1024).
- Code (Animal.cs): UseArt() in Awake loads Art/animal_<speciesId> for any species; hides the primitive renderers, puts the art under Model, moves Vitals, makes the art's Antlers the buck/doe switch and fits the body capsule to the standing Body. Die shows Pose_Dead (antlers matching) and refits the capsule to the carcass; a species with no art still tips over as before.
- Tests: AnimalArtTests (3); Fists, Knife, MeleePrompts, CreekBank and KnowHow 131/131.
- To check in game: a buck and a doe at grazing distance and up close, a clean kill (the vitals are now in the real chest), and the carcass (aim at it and field dress).

## animal_turkey (2026-11-19): wild turkey toms and hens and their carcasses (approved by Mike, 2026-11-19)
- Mike, 2026-11-19: male and female models; toms carry the big fan like the traditional turkey picture.
- Tom (Male): about 0.93 m to the head, the fan standing up and tilted back behind him (0.91 m across; finely barred, a dark band near the edge, chestnut and buff tips, dark lines between feathers), dark bronze plumage in fine scales, bare red head with a pale blue face, snood, wattle, beard, barred wing feathers.
- Hen (Female): 86% of a tom, duller brown feathers with pale tips, a greyish partly feathered head, small snood, no beard, the folded tail with its chestnut tip.
- Pose_Alive/Male: Body, Wings, Fan, Parts; Pose_Alive/Female: Hen_Body, Hen_Wings, Hen_Tail, Hen_Parts. Pose_Dead/Dead_Male and Dead_Female: the same through the shared carcass deformation (tail laid flat; the tom's whole fan laid flat by lay_fan_flat). Vitals in the chest, 0.51 m up.
- Materials (14): animal_turkey_coat, _wing, _fan, _skin, _beard, _eye, _leg and the hen's _hen_coat, _hen_wing, _tail, _hen_skin, _hen_beard, _hen_eye, _hen_leg. Bodies 2048, the rest 1024.
- Sources: Art\Blender_Work\animal_turkey (turkey_geo.py). Code: Animal.IsMale and ShowSex (general, for any species).
- Tests: AnimalArtTests 7/7 (a tom and a hen each show their own look and carcass); Fists, Knife, MeleePrompts pass.
- To check in game: toms and hens at distance and close, shooting each and its carcass.
## animal_waterfowl (2026-11-19): mallard drakes and hens and their carcasses (approved by Mike, 2026-11-19)
- The Duck prefab's species id is "waterfowl", so the art is Art/animal_waterfowl (not animal_duck).
- Drake (Male): about 0.58 m bill to tail, glossy green head, white collar, chestnut breast, grey flanks with fine wavy lines, brown-grey back, black rump with the two curled tail feathers, pale tail tip, yellow bill, orange feet.
- Hen (Female): 93% of a drake, mottled brown with buff-edged feathers, buff face with a dark crown and eye line, orange-brown bill. Both show the blue speculum edged in white on the folded wing.
- Floats: the origin is the water line (the game keeps the duck's root on the water). Pose_Alive is lowered 0.035 m in the prefab so the hull sits in the water; feet hang under it, paddling.
- Pose_Alive/Male: Drake_Body, Drake_Wings, Drake_Parts (bill, eyes, feet, curl); Pose_Alive/Female: Hen_Body, Hen_Wings, Hen_Parts. Pose_Dead/Dead_Male and Dead_Female: the shared carcass deformation (on its side, neck laid down, feet out), then slumped to 78% height. Vitals in the chest, 0.09 m up.
- Materials (12, all 1024): animal_waterfowl_drake_coat, _drake_wing, _drake_bill, _drake_eye, _drake_foot, _drake_curl and the hen's _hen_coat, _hen_wing, _hen_bill, _hen_eye, _hen_foot, _hen_curl. About 6,300 tris per sex.
- Sources: Art\Blender_Work\animal_waterfowl (duck_geo.py: build_all() builds, unwraps, makes materials and carcasses). Note for later animals: in skin_body keep joints further apart than their radii, or the skin folds over itself.
- Tests: AnimalArtTests (Duck added to the species-with-art cases; the no-art case is now the rabbit, read by its species id) and ArtSwapTests, 45/45.
- To check in game: drakes and hens on the pond, how deep they sit, shooting one and its carcass at the bank.

## animal_rabbit (2026-11-19): eastern cottontail and its carcass (approved by Mike, 2026-11-19)
- One model: bucks and does look alike, so there are no Male/Female groups (IsMale is still set at random; nothing shows by it).
- Crouched, ready to hop: about 0.42 m nose to tail, 0.29 m to the ear tips. Big haunches folded under, long hind feet flat on the ground, short front legs.
- Coat: agouti grey-brown in fine streaks lying back, pale belly, chin and feet, a rusty nape patch, a pale ring round the eye. Long cupped ears with paler insides and dark tips; the cotton tail is white beneath, brown on top.
- Pose_Alive: Body, Legs, Parts (ears, eyes, tail). Pose_Dead: Dead_Body, Dead_Legs, Dead_Parts by the shared carcass deformation, with the ears folded back and draped onto the ground (lay_ears_back). Vitals in the chest, 0.11 m up.
- Materials (5, all 1024): animal_rabbit_coat, _legs, _ear, _eye, _tail. About 5,900 tris.
- Sources: Art\Blender_Work\animal_rabbit (rabbit_geo.py: build_all()).
- Tests: AnimalArtTests (Rabbit added to the species-with-art cases; the no-art case is now the squirrel) and ArtSwapTests, 46/46.
- To check in game: rabbits hopping at distance and close, shooting one and its carcass.

## animal_squirrel (2026-11-19): eastern gray squirrel and its carcass (approved by Mike, 2026-11-19)
- One model: males and females look alike.
- On all fours, alert, the bushy tail rising in an S over the back: about 0.25 m nose to rump, the tail to 0.31 m. Small rounded ears, big dark eyes.
- Coat: grizzled grey with a brownish wash on the back and head, white belly, chin and eye ring. Tail: long grizzled hairs, dark inside, frosted pale at the edges.
- Pose_Alive: Body, Legs, Tail, Parts (ears, eyes). Pose_Dead: Dead_Body, Dead_Legs, Dead_Tail, Dead_Parts by the shared carcass deformation; the tail is swung back to trail out behind on the ground (lay_tail_back). Vitals in the chest, 0.09 m up.
- Materials (5, all 1024): animal_squirrel_coat, _legs, _tail, _ear, _eye. About 8,000 tris (the tail is half).
- Sources: Art\Blender_Work\animal_squirrel (squirrel_geo.py: build_all()).
- Every wildlife prefab now has art. The no-art test gives a squirrel a made-up species id, so it keeps covering an animal added before its art.
- To check in game: squirrels moving about, shooting one and its carcass.

## fishing_bobber (2026-11-19): the round float on the fishing line (approved by Mike 2026-11-20)
- The classic round bobber, 6.6 cm across: glossy red top, white bottom with a slight seam lip, a white spring button with a wire clip on top and a wire line hook underneath.
- Root on the water line like the Bobber prefab (FishingRod puts the bobber's root on the water surface): the ball's centre is 8 mm up, so most of the white half sits under water.
- One mesh, materials fishing_bobber_red, _white, _wire (metal), 1024. About 2,400 tris.
- Code: FishingRod.UseBobberArt (called right after the bobber is cast) hides the prefab's primitive spheres and puts the art under the bobber; the Line stays the prefab's own. Without the art nothing changes.
- Sources: Art\Blender_Work\fishing_bobber (bobber_geo.py). Tests: FishingArtTests (new); EquipNameFade and RodsAndLightsInHand still pass.
- To check in game: casting, the bobber floating and dipping on a bite, at day and night.

## world_rocks (2026-11-19): Rock A, B, C for every loose stone and the South Ridge outcrop (approved by Mike, 2026-11-19)
- How the game uses rocks: StonePlacer (Homestead > Place Stone) put 490 loose stones and the outcrop's 8 boulders into the World scene, all sharing three mesh assets (Assets/Art/Rocks/Rock A, B, C) and one material (Stone.mat), scaled from 0.25 m stones up to 3.2 m boulders sunk 30% into the slope.
- The art: three weathered grey limestone rocks about 1 unit across, flat bottom at -0.17, top near 0.5 like the old meshes; flat broken faces along bedding and joints, rounded weathering, thin bedding lines, a buff tint in places, grey-green lichen on top. About 820 tris each.
- All three share one 2048 texture set (unwrapped and baked together), so the single Stone material still covers every stone. Material instanced.
- Unity: Resources/Art/world_rock_a, _b, _c (FBX) and world_rock_stone.mat. StonePlacer copies the art into Rock A, B, C and Stone.mat in place (new menu Homestead > Art > Apply Rock Art, already run), so the placed stones changed without moving and their save ids are untouched; a later Place Stone keeps the art. The boulders' mesh colliders follow the new shapes.
- Sources: Art\Blender_Work\world_rocks (rocks_geo.py: build, stone_mat, unwrap_together, bake_together, raise_to_game_height).
- Sandstone variant not made (the list said limestone and sandstone, confirm): one set of rocks covers the property today.
- Tests: RockArtTests (new), with ArtSwapTests 41/41.
- To check in game: loose stones close up and at distance, and the outcrop boulders on South Ridge.

## world_ground_wood (2026-11-19): the ground stick bundles and fallen branches (approved by Mike, 2026-11-19)
- How the game uses them: GroundWoodPlacer (Homestead > Place Sticks and Branches) put hand-pickup GroundWood pieces under the hardwoods and in the meadow, sharing six mesh assets (Assets/Art/Wood/Stick Bundle A-C, Fallen Branch A-C) and one material (Ground Wood.mat).
- The art keeps the old sizes (chunkier than life on purpose, so sticks show from a few metres): a bundle is six bent, knobbly sticks 3-4 cm thick, about 1 m across, crossed and stacked; a branch lies along X (its pickup box runs that way), 1.6-2.2 m, tapering, with 2-3 side twigs, some forked. 460-720 tris each.
- Material: weathered fallen wood, mid grey-brown rough bark, peeled in places to pale silvered wood, pale broken ends, a little moss on top; lighter than the leaf litter so it still stands out. One 2048 texture set for all six (baked together), instanced.
- Unity: Resources/Art/world_stick_bundle_a..c, world_fallen_branch_a..c (FBX) and world_ground_wood.mat. GroundWoodPlacer copies them into the six mesh assets and Ground Wood.mat in place (new menu Homestead > Art > Apply Ground Wood Art, already run): pieces already placed changed without moving, save ids untouched; a re-run keeps the art.
- Sources: Art\Blender_Work\world_ground_wood (ground_wood_geo.py). Tests: GroundWoodArtTests (new); with RockArtTests and ArtSwapTests 48/48.
- To check in game: bundles and branches under the trees and in the meadow, close and from a few metres.

## world_tall_grass (2026-11-19): the tall meadow grass clump cut for Cordage (approved by Mike, 2026-11-19)
- How the game uses it: GrassPlacer (Homestead > Place Tall Grass) put about 200 TallGrass clumps in the open meadow and pasture, sharing one mesh (Assets/Art/Grass/Tall Grass Clump) and one material (Tall Grass.mat).
- The art: 70 thin curved ribbon blades from a 0.25 m tuft, arching outward, some drooping at the tip, 0.75-1.35 m tall, plus 9 thinner stems carrying timothy-like seed heads. About 1,360 tris.
- Texture painted in code (not baked): 14 colour strips for blades, green at the base fading to straw at the tips, some greener and some drier all the way down, fine lengthwise streaks and a paler midrib; a brown strip for the seed heads. The normal map folds each blade down its middle. 256 x 1024.
- Material: double-sided (each blade is one ribbon), instanced. Unity: Resources/Art/world_tall_grass_model (FBX) and world_tall_grass.mat; GrassPlacer copies them into the clump mesh and Tall Grass.mat in place (new menu Homestead > Art > Apply Tall Grass Art, already run), so clumps already placed changed without moving; a re-run keeps the art.
- Sways in the wind since 2026-11-19 (see Wind sway below).
- Sources: Art\Blender_Work\world_tall_grass (grass_geo.py: build, paint_textures). Tests: TallGrassArtTests (new); with the rock, ground wood and art-swap tests 50/50.
- To check in game: clumps in the meadow from close and from across a field, at day and dusk.

## Wind sway (2026-11-19): the tall grass and the trees move in the wind (approved by Mike, 2026-11-19)
- Mike, 2026-11-19: "it would be really neato if the tall grass and trees swayed in the wind".
- Shader: Assets/Shaders/HomesteadWindLit.shadergraph (shader Homestead/HomesteadWindLit), an HDRP Lit shader graph with alpha clipping, built from Unity's FoliageWind sample setup. It bends each plant downwind in object space, more with height (height squared, so the base stays planted), in slow gusting waves whose phase changes across the land so neighbours don't move in step, leaning harder and moving faster as the wind rises; leaves add a small quick flutter.
- Same property names as HDRP/Lit (_BaseColor, _BaseColorMap, _NormalMap, _MaskMap, _AlphaCutoff, _Smoothness), so switching a material kept its values and SeasonalTrees still recolours leaves and thins leaves and snow by alpha cutoff. Per material: _WindSway (metres at _WindHeight), _WindHeight, _WindFlutter, _UseMask (0 for materials without a mask map).
- On: Tall Grass.mat and world_tall_grass.mat (height 1.3, sway 0.18, flutter 0.015); Bark, Leaves, Shrub Leaves, Tree Snow (one bend for every tree part, height 15, sway 0.45, so leaves, snow and branches stay together; flutter 0.04 on leaves and crown snow, none on bark). Shrubs move mostly by flutter (they are short against the 15 m bend).
- Driver: Assets/Scripts/World/WindShaderDriver.cs sets the global _HomesteadWind every frame from WeatherManager.WindDirection (the bearing it blows FROM, 0 = north = +Z) and WindStrength, eased over about 4 s so a change of weather builds up; a light westerly breeze (0.25) with no weather and in the editor.
- Builders: PropertyTerrainBuilder.UseWind keeps the tree materials on the wind shader if the trees are rebuilt; GrassPlacer already copies the shader from the grass art material.
- Not covered: any other plant (forage, crops) is still plain HDRP/Lit; the terrain's grass texture doesn't move.
- Tests: WindSwayTests (new; the shader compiles with the Lit names, every plant material sways, tree parts bend together and keep alpha clipping, the wind direction and easing); with the art tests 59/59.
- To check in game: calm and windy weather (the debug weather controls), trees near and far, grass, and Fall and Winter (leaf colour, leaf drop and snow still right).

## world_terrain (2026-11-19): the Grass, Dirt and Gravel terrain layers (approved by Mike, 2026-11-19)
- Before: each layer was a flat 512 colour texture with no normal or mask map.
- Now: tileable 1024 colour, normal and mask maps for each layer, generated in code (Art\Blender_Work\world_terrain\gen_terrain_textures.py: periodic noise, wrapped Voronoi, blades and pebbles drawn round the edges, so nothing seams). Mask: R metallic 0, G AO, B height (for blending), A smoothness.
- Grass (4 m tile): short turf drawn blade by blade, greener and drier patches, dark soil between. Dirt (3 m tile): brown silt-loam with clods, drier high spots, a few small pebbles and dark organic specks. Gravel (2 m tile): rounded limestone-grey pebbles with some buff and a few rusty ones, soil between.
- Unity: Assets/Art/Terrain/Textures/terrain_<layer>_basecolor, _normal, _mask on Grass, Dirt and Gravel.terrainlayer (tile sizes kept). Property Terrain.mat: normal and mask maps on, and height-based blending (transition 0.15), so grass and dirt meet along the textures' bumps instead of a soft smear. PropertyTerrainBuilder.MakeLayer uses these maps when they exist, so a rebuild keeps them.
- Not changed: the runtime layers SeasonalGround (snow, leaf litter) and CreekBanks (clay, sand) paint from their own code-made textures; they could get the same treatment later.
- Tests: TerrainArtTests (new); with the wind and art tests 63/63, CreekBankTests 35/35.
- To check in game: the meadow, a path or the cabin dirt, gravel areas, close and far (watch for the tile repeating), and snow and Fall leaf litter on top.

## world_water (2026-11-19): the spring, creek and pond surface (approved by Mike, 2026-11-19)
- Before: one flat mesh with an opaque, dark, glossy HDRP/Lit material; nothing seen through it, nothing moving. HDRP's own water system is off in this project and was left off (heavy, and built for oceans and large rivers).
- Shader: Assets/Shaders/HomesteadWater.shadergraph (Homestead/HomesteadWater), a transparent HDRP Lit shader graph. It reads the scene depth behind the surface, so the water is clear in the shallows (the creek bed shows) and darker and more opaque with depth; ripples from a tileable normal map are carried downstream by the flow with two-phase flow mapping (no stretching), plus a slower cross ripple that drifts on still water; foam gathers where the water thins against the bank. Normal output in world space (the water mesh has no tangents).
- Flow: stored in the water mesh's UV1 (x, z = direction on the ground, z = speed m/s) by PropertyTerrainBuilder.WaterFlow: the spring wells gently outward (0.12), the pond barely drifts (0.03), the creek runs downstream (0.5, gathering pace below the spring).
- Material Water.mat (tunable): Shallow Colour, Deep Colour, Clarity (m, depth to full colour, 0.9), Foam Width (m, 0.12), Ripple Scale (m, 1.6), Ripple Strength (0.35), Flow Speed (1), Smoothness (0.95). Textures Assets/Art/Terrain/Textures/water_ripple_normal and water_foam (512, tileable; source Art\Blender_Work\world_water).
- Builder: PropertyTerrainBuilder.WaterMesh writes the flow and UseWater puts the shader on (transparent) on a rebuild; new menu Homestead > Art > Apply Water Art (already run) did both for the water in the scene. Collider, Water layer and WaterSource unchanged.
- Not covered: looking up from under the surface (it's one-sided, as before); no refraction or reflections beyond HDRP's sky and screen-space ones; floods don't raise the surface (they never did).
- Tests: WaterArtTests (new); with the terrain, wind, creek bank and art-swap tests 87/87.
- To check in game: the spring, the creek close and from the bank, the pond, at day, dusk and night, in rain; how clear the shallows are and how much foam.


## world_trees (2026-11-19): sugar maple (Hardwood Broad), tulip poplar (Hardwood Tall), viburnum (Understory Shrub) (approved by Mike in Play Mode 2026-11-20)
- Source: `Art/Blender_Work/world_trees/` (tree_geo.py generator, gen_tree_textures.py, world_trees.blend, textures/, review/).
- One branching skeleton (trunk with flare, 5 scaffold limbs, ~60 branches, ~220 twigs) meshed at three detail levels:
  LOD0 10,140 tris, LOD1 2,830, LOD2 770 (old tree 432). Screen-height hand-overs 0.25 / 0.08 / 0.004 (about 40 m, 120 m).
  Middle and far levels use every other ring on branches (same skeleton, so the levels match).
- Material slots kept for SeasonalTrees: 0 bark (world_tree_maple_bark: tileable grey ridged bark, normal, mask),
  1 leaves (world_tree_maple_leaves: 2x2 atlas of maple-leaf clusters, alpha-clipped, normal map), 2 crown snow
  (world_tree_maple_snow_leaves, same atlas, white), 3 branch snow (the existing Tree Snow, foliage-mask texels on upward bark faces).
- Faces are written in slot order (bark, leaves + crown snow, branch snow last): Unity orders submeshes by each material's first face.
- Leaf drop: each leaf card's random value is in UV1.x. HomesteadWindLit has a new `_UseFaceValue` switch (off by default,
  so grass and the old trees are unchanged): when on, the clip alpha is the card's value where the texture shows a leaf, -1 elsewhere.
- Unity: Resources/Art/world_tree_maple_model.fbx + textures + 3 materials. PropertyTerrainBuilder builds the prefab from the
  art (LOD children); menu Homestead > Art > Apply Tree Art rebuilds Hardwood Broad in place (same prefab, so all 4,213 placed
  trees changed). SeasonalTrees now copies materials across every renderer in the tree, per slot. Season colours x1.3 for the art
  tree (its leaf texture is mid-toned). Trunk collider unchanged (r 0.3, h 6.5).
- Tests: TreeArtTests (4). Full EditMode suite 2,242/2,242.
- Known: leaf cards seen edge-on from directly underneath show as streaks.
- Hardwood Tall = tulip poplar (2026-11-19): species "poplar" in tree_geo.py (clear trunk to about 6.8 m, 10 limbs that
  shorten toward the top via scaf_taper, narrow crown, 17.2 m tall, 8.8 m wide). LOD0 14,108 tris, LOD1 4,038, LOD2 1,166;
  hand-overs 0.35 / 0.11 / 0.004 (about 42 m, 135 m, per-kind TreeLodHeights). Four-lobed square-tipped leaf atlas and
  interlacing-ridge bark from gen_tree_textures.py (now per species: `python gen_tree_textures.py <species> [leaves|bark]`).
  Golden fall colour kept. Trunk collider unchanged (r 0.27, h 9.5).
- Season colours on art trees are brightened in RGB only (alpha stays 1: it is the leaves' clip value).
- Understory Shrub = viburnum (2026-11-19): species "viburnum" in tree_geo.py, multi-stem mode (7 stems from the ground, no
  trunk; `stems` setting), a rounded mound 3.5 x 3.9 m, 3.3 m tall. LOD0 5,150 tris, LOD1 1,480, LOD2 460; hand-overs
  0.12 / 0.03 / 0.006 (about 20 m, 80 m, gone past about 400 m). Broad oval finely toothed leaves (36 small leaves per
  cluster), smooth grey-brown stems. Red-purple fall colour kept. Still no collider (walk-through).
- The generator draws its random numbers in the same order as before for single-trunk trees, so the maple and poplar
  regenerate identically.
- Tests: TreeArtTests covers all three (9 tests). Full EditMode suite 2,247/2,247.

## world_ground (2026-11-19): the runtime ground layers: snow, leaf litter, wet clay, creek sand (approved by Mike 2026-11-20)
- Source: `Art/Blender_Work/world_ground/` (gen_ground_textures.py, textures/). Tileable 1024 maps per layer: basecolor, normal,
  mask (R 0, G AO, B height, A smoothness), in Resources/Art as ground_<id>_basecolor/_normal/_mask.
- snow: soft drifts, faint blue in the hollows, a little sparkle; the highest layer (mask height about 0.55 to 0.95), so it sits
  over everything in the terrain's height blend. leaf_litter: about 900 fallen maple, poplar and viburnum leaves (the tree
  leaf shapes) in browns, rust and dull orange over dark damp soil. wet_clay: red-brown and grey mottled clay with a wet
  sheen and a few pressed-in pebbles. creek_sand: fine tan grains, faint water ripples, damp patches, small pebbles.
- Code: GroundLayerArt (Assets/Scripts/World) puts the art on a TerrainLayer by id and keeps the caller's name and tile
  size. SeasonalGround (snow, litter) and CreekBanks (clay, sand) use it, and fall back to their generated textures without
  art; neither destroys the art textures. CreekBanks' tile sizes still apply; its colour, smoothness and noise fields now
  only shape the fallback texture.
- Tests: GroundLayerArtTests (4).
- To check in Play Mode: snow (TimeDebugControls Ctrl+Home sets the snow), late-Fall leaf litter, and the creek banks.

## ui_map_markers (2026-11-20): the minimap and world map discovery markers (approved by Mike 2026-11-20)
- Source: `Art/Blender_Work/ui_map_markers/` (gen_map_markers.py). Nine 128 px PNGs in Resources/Art: map_marker_WaterSource,
  _Plant (berries), _Wildlife (hoof print), _Fishing, _PropertyFeature (cabin), _Plant_fruit, _Plant_nut, _Plant_greens,
  _Plant_mushroom. Same colours and layout as MapIcons drew them (category colour disc, dark rim, cream glyph), with light
  shading, a rim highlight, a soft glyph shadow and glyph detail (fish eye and fin, cabin door, window and chimney,
  mushroom spots, leaf veins, berry and apple leaves).
- MapIcons.Marker uses the art when present (an icon without its own art falls back to its category's), at the drawn
  markers' size (pixels per unit scaled so 128 px art = the 64 px sprites). The arrow, droplet and pail stay drawn in code.
- Tests: MapMarkerArtTests (11).

## world_fx (2026-11-19): rain, snow, lightning, campfire flames, kiln smoke (approved by Mike 2026-11-20)
- Source: `Art/Blender_Work/world_fx/` (gen_fx_textures.py, textures/). All white or grey, tinted by their materials.
- Raindrop.png (32x128, a tapered streak) and Snowflake.png (64x64, a soft clump) replaced in place (same files, so Rain.mat,
  Snow.mat and Precipitation are unchanged). LightningBolt.png on Lightning Bolt.mat (now transparent, premultiplied): the
  LineRenderer's cross-section, a hot core with a glow.
- Flame Sheet.png (Assets/Art/Fire, 4x4 frames, premultiplied alpha): the campfire's Flames and Core particle systems play it
  (texture sheet animation 4x4, random start frame, sprites a little larger). Its importer keeps "alpha is transparency" OFF
  and both flame materials use it as their emissive map too: either mistake draws a square around every flame.
- fx_smoke_puff.png (Resources/Art): KilnSmoke's puff when present (the generated disc otherwise).
- Tests: Wave6FinishTests.
- Fix 2026-11-20: rain had vanished. The new Raindrop drew a thin streak (alpha mean 0.06), below a pixel on the 2 cm rain particles; it now fills about half the sprite's width like the original (alpha mean about 0.25), and Wave6FinishTests checks its coverage.

## world_forage (2026-11-19): forage crop shapes (approved by Mike 2026-11-20)
- Source: `Art/Blender_Work/world_forage/` (forage_geo.py, world_forage.blend). 15 meshes in Resources/Art/forage_crop_model.fbx,
  each named for the ForagePlacer shape it replaces and fitted to that primitive's box (sphere 1x1x1, cylinder 1x2x1):
  blackberries, raspberries, wild_apples, pawpaw, hickory_nuts, walnuts, chestnuts (spiky burr), dandelion (flower head),
  Dandelion Leaves (toothed rosette), wild_onion (flower cluster), cattail (head), morel (ridged cap), dryads_saddle and
  oyster_mushroom (shelves), Fallen Log (with the maple bark texture).
- ForagePlacer.Shape uses the art mesh when one exists. Fruit and berries go on the plant's real leaf clusters (OnFoliage:
  the old ellipsoid point moved to the nearest LOD0 leaf card, then set a little outward), with the extra random numbers
  from their own sequence so every patch keeps its position. Homestead > Place Forage Patches re-run and the World scene
  saved 2026-11-19: all 21 patch ids and positions unchanged.

## world_sky (2026-11-19): stars and the moon (approved by Mike 2026-11-20)
- Source: `Art/Blender_Work/world_sky/` (gen_sky_textures.py, textures/). Assets/Art/Sky: sky_stars.png (equirect 4096x2048,
  imported as a cubemap: about 7,000 stars and a faint Milky Way) and sky_moon_surface.png (maria and craters).
- WorldSkyAndFog.asset: Physically Based Sky space emission = the stars (daylight hides them; checked).
- World scene's Moon light: drawn in the sky (interactsWithSky), 1.2 degrees across (larger than life so it reads), surface
  texture, tint 0.001 (a sunlit full moon is otherwise a white glare at night exposure), earthshine 0.08, Manual shading.
  DayNightCycle sets its phase from the calendar (MoonPhase: full on day 0, new halfway, lunarCycleDays 29.5).

## Fall variety (2026-11-19)
- HomesteadWindLit: _FallAltColor and _FallMix. Each tree picks a random value from where it stands; past 0.35 its leaves
  turn toward the second colour by _FallMix, which SeasonalTrees raises with the Fall colour change (0 outside Fall).
- SeasonalFoliage.fallAltColor / fallVariety: golden-orange maples, russet poplars, purple viburnums (set by PropertyTerrainBuilder).
