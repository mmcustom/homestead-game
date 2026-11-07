# Homestead
## Art Needed List v0.2

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
| tool_bolt_action_rifle | Bolt-action rifle | 1.1 long (confirm) | Anchor_Grip_R (R), Anchor_Grip_L, Anchor_Muzzle | none yet | Muzzle anchor optional. |
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
| structure_tap | Sap tap on a tree: spile, bucket, sap disc | small | Anchor_Spile; Fill (sap level), State_Sour | spile.png, sap bucket.png | Low priority. |
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
| Item icons (2D) | One icon per item for inventory and market | 128 x 128 px, transparent PNG | Not built in code yet; needs an icon field added first. Render from the item prefab when possible so style matches. | Medium |
| Map icons (2D) | Map markers for structures and places | 64 x 64 px | Built in code today (UI/MapIcons.cs). | Low |
| Stump | Tree stump left after felling | trunk radius (confirm) | Pivot at ground, cut face up. | Medium |
| Carcass | Dead animal on the ground | by species | Carcass visuals not yet reviewed in code. | Low |
| Placement ghost | Uses the structure prefabs themselves | as each kind | No separate art. | none |

Tanning rack and tanning tools (references saved: tanning rack.png, tanning tools.png) are future features; no code or ids exist yet, so they are not listed as rows.

---

# Wave 5: animals and traps

Animals are prefabs already (Prefabs/Wildlife/*.prefab, built from capsules and spheres), so these swap through the existing prefab slots, not ArtSwap. Real art needs the shape and a simple rig or at least separate head, body and legs so the code can bob, turn and stun. Stunned and startled states exist for fists (M0e-1), so the animal needs a clear "stunned" pose reading from a single body tilt, not a full animation.

| Animal | Approx. size (m) | Notes | Priority |
|---|---|---|---|
| Deer (white-tailed) | 1.7 long x 1.0 shoulder | Head up and head down poses; large target, must read at 40 m. | Medium |
| Turkey | 0.9 long x 0.6 tall | Wild turkey. | Medium |
| Rabbit (cottontail) | 0.4 long | Hops. | Medium |
| Squirrel (grey or fox) | 0.45 long incl. tail | Small, fast. | Low |
| Duck (mallard) | 0.55 long | Floats on water. | Low |
| Fish (creek species) | 0.2 to 0.5 long | Seen on the line and in the Fish Trap. | Low |
| Bobber | 0.04 | Floats; part of the fishing rod. | Low |
| Rabbit Snare, Box Trap, Fish Trap (world versions) | see tools table | Use the same shapes as the tool_ versions with a set and sprung state. | Low |

---

# Wave 6: trees, terrain, water, weather and effects

These set the look of every screen, so they decide whether the realistic style holds together; do them in step with the structures. Trees have a special constraint: SeasonalTrees recolours leaves per season from per-vertex data baked into the leaf faces and snow caps. A replacement tree mesh without that data stays green all year. Ask the maker for a leaf material that the code can recolour, or plan a small code change when the first real tree arrives.

| Group | What | Notes | Reference image | Priority |
|---|---|---|---|---|
| Trees | Hardwood Broad, Hardwood Tall, Understory Shrub, plus tall and short fall-colour variants | Sugar maple, sycamore, oak, hickory, beech; up to about 25,000 triangles each in the realistic style (leaf cards count); also a low-detail version later. FallingTree tips the same prefab over. | fall sugar maple, sugar maple, winter sugar maple, sycamore (reference only) | High |
| Rocks | Rock A, B, C and the South Ridge outcrop | Weathered limestone and sandstone for southern Ohio (confirm). | none yet | Medium |
| Sticks and branches | Ground wood variants | Several sizes. | none yet | Medium |
| Tall grass | Grass clump | Tapered blades, sways. | none yet | Medium |
| Forage | One simple shape and one material per species | Low priority until foraging is expanded. | none yet | Low |
| Clay patches | Wet clay surface decals | Optional; none placed yet. | none yet | Low |
| Water | Creek and pond surface material | One flat mesh today; needs flow, shore foam, and clear shallow edge. | none yet | High |
| Terrain | Three terrain layers: Dirt, Grass, Gravel (albedo, normal, mask) | Plus runtime snow, leaf litter and creek banks painted by code. | none yet | High |
| Weather | Rain, snow, lightning particle sprites | Small PNG sprites. | none yet | Medium |
| Smoke and fire | KilnSmoke material and texture, campfire flame sprites | Particles built in code; art is just the sprite sheet. | none yet | Medium |
| Sky | Skybox or HDRI | Not listed in the code inventory; confirm what the project uses. | none yet | Medium |

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
