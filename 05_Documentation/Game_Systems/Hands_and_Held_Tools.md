# Homestead
## Hands and Held Tools v0.3

Status: planned 2026-11-04 from Claude Code's read-only plan; phases M0a (73ee84d) and M0b (175a8c7) BUILT, M0c and M0d not built yet. Mike's wish: draw the player's hands holding and using tools on screen, so the permanent on-screen equipped-tool text can go. Rough primitive shapes for the Alpha; real art later, so every visual has one swap point (standing rule in Art_Readiness_and_Placeholders.md). Phase "Hands and tools M0" in the art phases. Depends on A1 (ArtSwap, ef52c25).

---

# How things work today (Claude Code's findings)

- Equip: InventoryManager holds one EquippedTool (ItemDefinition) and fires EquippedToolChanged. Ten hotkey slots (keys 1 to 9, then 0) map to tool ids; a press equips, a second press on the same key puts it away. Any Tool-category item can be equipped except Sleeping Bag, Tent and Weather Station (carried only). Dropping or selling an equipped tool unequips it. There is no tool slots bar on the HUD; hotkeys show only in the Inventory screen.
- 32 tool assets in Resources/Items/Tools: axe, primitive_axe, wood_saw, hammer, shovel, iron_shovel, stone_pick_axe, iron_pick_axe; fishing_rod, cane_pole, bucket, sap_bucket, canteen; torch, lantern, bow_drill, flint_and_steel; primitive_bow, recurve_bow, bolt_action_rifle, knife; box_trap, fish_trap, rabbit_snare; spile, clay_pot, cooking_pot, potash_kettle, pouch, tarp; carried-only: sleeping_bag, tent, weather_station.
- AxeTool (chop, dig, mine, build): hold click, one blow every 0.9 s (swingSeconds); each blow costs stamina and plays a sound. The blow happens at the instant of the click. Iron tools need 0.75 of the blows. Swing() is the single moment to hook.
- FishingRod (Rod and Cane Pole): cast, wait, bite (click within about 1.2 s), reel (hold, ease off on runs). HuntingWeapon: Bow (hold to draw, release), Rifle (right-click raises scope, click fires, bolt delay); the scope changes the main camera's field of view directly. PortableLight (Torch, Lantern): click to light or snuff; a point light "Held Light" is a child of the camera. BowDrill, TrapSetter (ghost, right-click places), WaterSource (Bucket, Canteen), Spile.
- HUD: ToolHud draws ToolStatus.Line under the crosshair, plus a bar, flash message, banners and weapon sights. Each tool script calls ToolStatus.Report(...) every frame it is equipped; most idle strings start with the tool's display name plus a generic hint.
- Carried things: the Hand Cart has a handle (E grabs; CartHandler keeps the cart about 0.45 m ahead). Nothing else carried is drawn (Logs, Stone, water and pouches are inventory weight only). Equippable non-tools (Cooking Pot, Clay Pot, Pouch, Canteen, Potash Kettle) sit in "equipped" with no visual.
- Camera: one "Main Camera" prefab nested under the Player, FOV 60, near clip 0.3, far 1000, HDRP 17.3.0 (HDAdditionalCameraData), no Cinemachine, no head bob (only crouch eye height changes). Layers used: Water (4), UI (5). A global Volume is built in code by DayNightCycle. HDRP has no camera stacking.
- Events today: EquippedToolChanged only; ToolStatus.Flash and Report are text. No public event for a blow, cast, bite or catch.

---

# The plan

Rendering (decision 1): hands on a new Viewmodel layer (layer 6, TagManager.asset change), drawn by a custom pass volume created in code (like the Cart and Volume are, no scene edit): a small pass renders that layer from a hidden second camera with its own field of view and a depth clear, before post-processing so the hands get the same exposure and lights as the world (night, cabin, torch). That layer is removed from the main camera's culling mask. Hands do not cast shadows but receive light. Performance: one extra cull and a few dozen draws; not measured. Fallback (plan B): hands in the world in front of the camera, pulled back by a raycast near walls (simpler, but clips at corners and zooms with the scope). M0a proves the custom pass first.

Poses without animation clips: a pure class HandPoseRules (no engine types, easy to test) returns position and rotation from a time in the swing. Idle breathing sway; walking step sway; sprint and crouch lower it; equip rises over about 0.3 s from the bottom of the screen (putting away reverses); chop and mine: raise, strike on the click, recover; dig: shorter downward push with a lean; hammer: short forward tap; cast: back-then-forward flick, reel: small crank while held, bite: quick dip; bow pulls back with the draw fraction; rifle moves to the scoped position as scope goes 0 to 1. The pose follows the existing 0.9 s swing and adds no game time (contact part in the first ~0.15 s, the rest is recovery). A finished job is a slightly bigger final swing; "too tired to swing" is a half-raise that droops.

Hooks: one static ToolEvents class (Swung(toolId, kind, point), Cast, Bite, Landed, Failed, Lit, Drew(power), Shot). Each tool calls one line where it already acts; the only invasive one is a single added line at the success point in AxeTool.Swing.

Swap points (through ArtSwap, A1 reused: loader, cache, ResetCache seam, Named and AnchorOf, state and On_ children by name, collider stripping; it needs a small second table keyed by item id, a TryAttachHands and a TryAttachTool; tool and hand prefabs get no Solid_* colliders):

- Hands: Resources/Art/hands_player.prefab, anchors Anchor_Hand_R and Anchor_Hand_L (a tool's grip is placed on the matching hand anchor); optional Hand_R and Hand_L children the code hides or shows. Code: ArtSwap.TryAttachHands.
- Each tool: Resources/Art/tool_<itemid>.prefab using the item id; anchors Anchor_Grip_R (required), Anchor_Grip_L (optional), Anchor_Tip (working point), Anchor_Flame (Torch, Lantern), Anchor_Line (Rod, Cane Pole, where the line starts), Anchor_Muzzle (Bow, Rifle); State_* children for lit/unlit, drawn/relaxed bow. Code: ArtSwap.TryAttachTool. Primitive default: a new ToolViewPrimitives builds the rough shape per tool (one shape builder with a head colour: stone grey, iron dark steel).
- Stone and iron are separate ids with no fallback: tool_shovel, tool_iron_shovel, tool_stone_pick_axe, tool_iron_pick_axe.
- Hand Cart grip belongs to the Hand Cart's structure swap (A2); in M0c it is a fixed forward pushing pose.

Text: stays: the short fade-in name on switching (new), all aimed-target prompts ("Stump: hold click to dig it out"), progress bars, banners, sights, flash messages, all "Equip the Hammer to build" messages, and the Inventory screen's equipped display. Goes: the idle name-plus-generic-hint line under the crosshair. The fade-in name is part of the same phase as the removal, so a hotkey press always has on-screen confirmation.

---

# Decisions (Claude Code's recommendations; ALL 8 ACCEPTED by Mike, 2026-11-04; on no wind-up, Mike will see how it feels in play and can ask for one later)

1. Draw the hands with a custom pass on a Viewmodel layer (own FOV, depth clear); plan B is raycast pull-back.
2. Hand style: simple forearms with mitten hands, skin-tone tint, sleeve cuff (a raw capsule reads as a robot arm).
3. Empty-handed: both hands drawn, relaxed and low at the screen edges, slight breathing sway.
4. Left hand drawn on two-handed tools (Axe, Saw, Shovels, Pick Axes, Rod, Bows, Rifle) at Anchor_Grip_L; hidden on one-handed (Hammer, Torch, Knife).
5. Swing timing: the pose follows the existing 0.9 s swing and adds no game time (strike first, recovery after); no wind-up delay.
6. Drop the idle equipped line; show the name once as a fade-in for about 2.5 s on switching; keep all aimed-target prompts, bars and messages.
7. Equipping a non-tool item (Cooking Pot, Clay Pot, Pouch, Canteen, Potash Kettle): a small generic held shape in the category colour; real shapes with the item art phase.
8. Hands show only while the world is live (GameState.Playing); hidden behind Inventory, Journal, pause and sleep screens.

---

# Phases (one commit each, each ends with a playtest checklist)

- M0a Hands and viewmodel: two primitive hands at the screen edges that breathe and sway; press a tool key and a placeholder rises; walk against a wall and nothing clips; the rifle scope does not magnify the hands; hands_player prefab appears when dropped in Resources/Art; menu items to make and remove a test hands prefab. New: ViewModelPass, PlayerHands, HandPoseRules; ArtSwap hands method; TagManager.asset. About 20 tests. Medium.
- M0b Tools and swings: Axe, Saw, Hammer, both Shovels and both Pick Axes drawn in hand with chop, strike, mine, dig and build poses on the existing timing; iron looks steel-headed; menu item for a test tool_axe. New: ToolViewPrimitives, ToolEvents; one line in AxeTool.Swing; ArtSwap tool table. About 22 tests. Medium to large.
- M0c Rod, weapons, light, carried: Rod and Cane Pole (cast, wait, bite, reel); Torch and Lantern with flame (Held Light moves to Anchor_Flame); Bow and Rifle (draw, scope); Bow Drill; Bucket and Canteen; traps; Knife; generic shape for the rest; pushing pose on the Hand Cart. Hooks in FishingRod, HuntingWeapon, PortableLight, BowDrill, TrapSetter, WaterSource, CartHandler. About 20 tests. Large; split c1 fishing and light, c2 weapons and the rest.
- M0d Remove the equipped line: idle line gone, name fades in about 2.5 s on switching, aimed-target prompts stay. ToolHud and the idle strings across the tool scripts. About 10 tests. Small to medium.

---

# Risks and not verified

- The custom pass is untested: HDRP 17.3 classes exist (CustomPassUtils.RenderFromCamera, DrawRenderers), but whether the Custom Pass feature is enabled in the HDRP asset in use, and whether a second camera's field of view is honoured as expected, is not confirmed. M0a tests it first; decision 1's fallback is plan B.
- The 47 MB scene file was not read: scene volumes, camera overrides on the nested prefab, cabin interior lights reaching the hands.
- Not checked: whether Player.prefab has a body mesh; each of the Pause, Journal, Inventory and sleep screens' state handling (hands hide there is an assumption); the equipped tool is restored on load so hands should start already raised (needs a test); the torch's Held Light position versus a prefab flame; Hand Cart handle geometry; mouse-wheel build rotation and the placement ghost while a tool is in hand; tool pose conflicts with PlacementMode; performance (a guess).
- Not exhaustive: Claude Code did not search every script for anything that attaches to the player.


---

# BUILT: Hands and tools M0a, the first-person hands (2026-11-04, playtest 1 to 22 and re-test R1 to R8 passed, committed 73ee84d)

Two primitive hands at the bottom corners of the screen, drawn on top of the world. Tools are not drawn yet (a generic placeholder shape rises into the hands); no swing poses; the old equipped line under the crosshair is unchanged. All 8 decisions applied as accepted.

Rendering: a new Viewmodel layer (layer 6, TagManager.asset). ViewmodelPass (Scripts/Player), a custom pass created in code, renders that layer from a hidden camera with its own 60 degree field of view and its own depth buffer, into the main camera's colour buffer before post-processing, so the hands are lit like the world, never clip into walls and are not zoomed by the rifle scope. The layer is taken out of the main camera's culling mask; the pass adds it to its own cull (HDRP culls custom passes separately); hand renderers have very large bounds so the narrowed scope frustum cannot cull them; the hands cast no shadows. The rig is scaled 0.5 toward the eye so it sits inside the player's 0.35 m body radius and a wall cannot shade the hands alone. A width scale (1 to 1.5 by window aspect) keeps the hands at the screen edges on wide windows.

The HDRP lesson: the hands first did not show with the default Lit material. Lit Shader Mode "Both" did not help (tried and reverted; all four HDRP assets stay at supportedLitShaderMode 2, Deferred only, and a test pins it). The real cause: the Lit forward pass tests depth as Equal for opaque objects (_ZTestDepthEqualForOpaque 3), which can never pass against the pass's own cleared depth. Fix: ViewmodelPass draws with a render-state override, depth test LessEqual and depth write on. World comparison pass on versus off: mean difference 0.6 to 1.0 out of 255 (noise). Frame time about 16 ms with and without (rough: editor in the background, 60 fps cap).

Files: HandPoseRules.cs (Scripts/Economy: pure pose math, HeldTools two-handed table, HandRaise, scope rules), PlayerHands.cs (runtime object added in PlayerController.Awake like the Cart handler; no prefab or scene edit), ViewmodelPass.cs, ArtSwap hands methods, ArtSwapMenu test hands items, DroppedItems.Tint made internal (placeholder colour = item category colour), HuntingWeapon.ScopeFraction (one read-only line, the only tool-script change).

Poses: idle breathing sway, step sway, sprint and crouch lower the hands, equip raise over about 0.3 s from the bottom with the tool changing at the bottom (put away reverses), loaded game starts already raised. Left hand shown on two-handed tools (Axe, primitive_axe, Saw, Shovels, Pick Axes, Rod, Cane Pole, bows, Rifle), hidden on one-handed. Hands show only in GameState.Playing; they hide behind Inventory, Journal, Finances, pause, sleep. Rifle scope: the raise is scaled down as the scope rises (ScopeLowering, to 0 at 0.5) and the hands are not drawn at all from 0.5, where the overlay appears (HiddenByScope); the bows read 0.

Look (primitives): skin forearm about 15 cm, lighter blue cuff band thicker than the arm, denim-blue sleeve (the first olive was invisible in shade), flat mitten palm, rounded finger block, separate thumb (mirrored). Skin 0.74, 0.54, 0.42. Held placeholder: a long staff for two-handed tools, a short block for one-handed, in the item's category colour.

Swap point: Resources/Art/hands_player.prefab through ArtSwap.TryAttachHands (PlayerHands.BuildRig is the one caller; primitives only when it returns null). Separate table ViewSpecs keyed by id. Anchor_Hand_R and Anchor_Hand_L (grip, in the palm; a missing one is made at the palm; one found outside its hand is moved under it); optional Hand_R and Hand_L (the objects the code moves and hides; a missing one is made empty). Author at normal size (the rig is scaled 0.5 toward the eye). Colliders removed, layer and no-shadow set, a prefab's own Animator is not driven. Menu Homestead/Art/Create Test Hands Prefab and Remove Test Hands Prefab (a blue right and magenta left hand; never commit Assets/Resources/Art).

Not covered yet: the hands stay up in placement mode, while pushing the Hand Cart and after the creek knock-down (all GameState.Playing; M0c); real rifle aim was seen by Mike, and re-checked only with a manual scope fraction by Claude Code; TAA at slow movement; ultrawide wider than about 21:9 (at 32:9 the hands sit about a third in from each edge).

Save and gameplay: none changed. Files: 14 paths (8 new, 6 modified), 1674 insertions and 6 deletions. Edit-mode suite 1671 of 1671 (62 new tests in all).

Next: M0b, tools drawn in hand with swing poses (Axe, Saw, Hammer, both Shovels, both Pick Axes), ToolEvents hooks (one line in AxeTool.Swing), tool_<itemid> swap points with Anchor_Grip_R/L and Anchor_Tip, ToolViewPrimitives; then M0c (Rod, weapons, light, carried), M0d (remove the idle equipped line).


---

# BUILT: Hands and tools M0b, tools in hand with swing poses (2026-11-05, playtest passed incl. the pull-back re-test P1 to P8, committed 175a8c7)

Eight tools are drawn in the hands as primitives: Axe, Primitive Axe, Saw, Hammer, Shovel, Iron Shovel, Stone Pick Axe, Iron Pick Axe. Every other tool keeps the generic M0a placeholder until M0c. The crosshair text line is unchanged (M0d).

Shapes (ToolViewPrimitives): one shape builder with a head colour: stone grey for the stone tools, dark steel for the iron ones and for the Axe and Saw, wood for handles; the Hammer is a wooden mallet; matt parts (smoothness 0.12, a flat blade facing the sun otherwise blazed white). Tools are rolled 24 to 55 degrees about the handle so heads and blades show three-quarter, not edge-on. The right hand holds the tool at Anchor_Grip_R; the left hand goes to Anchor_Grip_L on two-handed tools (moved nearer the right hand on the Axes and Pick Axes so the left sleeve stays low); the Hammer keeps the left hand hidden.

Swap point: Resources/Art/tool_<itemid>.prefab through ArtSwap.TryAttachTool. A ToolSpecs table keyed by item id sits next to the hands table and apart from the structure table. Ids: tool_axe, tool_primitive_axe, tool_wood_saw, tool_hammer, tool_shovel, tool_iron_shovel, tool_stone_pick_axe, tool_iron_pick_axe; stone and iron are separate ids with no fallback. Anchors: Anchor_Grip_R (required), Anchor_Grip_L (two-handed tools only), Anchor_Tip (working point); a missing one is made at the primitive's own grip. Tool space for a prefab author: the origin where the right hand grips, +Z along the handle toward the working end, -Y the cutting side; authored at normal size (the hands rig scales 0.5 toward the eye). Viewmodel layer, colliders removed, no shadows. PlayerHands.ShowHeld is the one place that decides: prefab first, then the primitive for the eight tools, then the generic placeholder. Menu Homestead/Art/Create Test Axe Prefab and Remove Test Axe Prefab (orange handle, cyan and magenta double heads, marker cubes; never commit Assets/Resources/Art).

Swings (SwingRules, pure; HandPoseRules unchanged except small additions): the blow, its sound, its count and its stamina cost happen at the instant of the click, nothing about gameplay timing changed. The picture shows the strike landing ContactSeconds = 0.15 s later (the one number to change a feel), a DwellSeconds = 0.05 hold, then follow-through; the cycle ends exactly at SwingSeconds = 0.9 s so a held click chains. The hand and tool turn about an elbow ElbowBack = 0.20 m behind the wrist (HandPitchShare 0.35: the hand turns 35% of each pitch change, the tool tilts about its grip for 65%), so the sleeve never sweeps across the screen. Strikes: chop and split 24 degrees and 0.12 m forward; mine 18 and 0.14; dig 16 plus a lean; build (hammer tap) 13; saw a push of 0.11 m and a pull of 0.07 m, no arc. The finishing blow of a job is 1.35 times bigger (FinishingScale); "too tired to swing" is a half raise that droops with no strike (TiredSeconds 0.5, matching the game's retry).

Pull-back (Mike's playtest feel feedback, decision: visual only, no delay): the Axe, Primitive Axe, Hammer, Stone Pick Axe and Iron Pick Axe rest COCKED (tool drawn back and up behind the hand), so even the first blow after a pause strikes from a pulled-back hand. Each swing ends LOADED, a little farther back than cocked, so a held click shows a pull-back before every blow; with no new blow the hands settle from loaded to cocked over SettleSeconds = 0.25. Constants (pitch / back): chop cock -14 degrees / -0.04, load -24 / -0.07; mine -12 / -0.03 and -21 / -0.06; build -10 / -0.02 and -16 / -0.04; CockUp 0.015, LoadUp 0.03, CockYaw 4, LoadYaw 6 (the head turns away from the crosshair). The Saw and both Shovels are not cocked (neutral hold). Mike: "the drawback looks good for now."

Hooks (ToolEvents, static, Scripts/Player): Swung (tool, kind, finishing), Tired, WorkingOn(Split), JobFinished. AxeTool got 11 single-statement hook lines (11 insertions, no behaviour change): one where a blow lands, one for the stamina refusal, one before the blow at a wood pile (so a split is told from a chop), and one at each of eight job-finishing points (fell a tree, split, stump, stone, Clay or Sand, hammer build, Well shaft, Outhouse pit). A finishing blow raises Swung a second time in the same frame with the flag set; the hands treat that as the same swing made bigger. No other tool script mentions ToolEvents (a test pins it).

Diagnostics: PlayerHands.HoldSwing and SwingState.Seek let a screenshot look at one moment of a swing.

Not covered yet: the Fishing Rod, Cane Pole, bows, Rifle, Torch, Lantern, Bow Drill, traps, Bucket, Canteen, Knife and the rest (M0c); the hands staying up in placement mode, while pushing the Hand Cart and after the creek knock-down (M0c); ultrawide, night and TAA with tools were checked on the hands in M0a only.

Save and gameplay: none changed. Files: 14 paths (8 new: SwingRules.cs, ToolEvents.cs, ToolViewPrimitives.cs, ToolsInHandTests.cs plus metas; 6 modified: AxeTool.cs, PlayerHands.cs, ArtSwap.cs, ArtSwapMenu.cs, HandPoseRules.cs, HandsTests.cs), 1887 insertions and 12 deletions. Edit-mode suite 1735 of 1735. SavesBackup49 made.

Next: M0c (Fishing Rod and Cane Pole with cast, wait, bite and reel; Torch and Lantern with flame at Anchor_Flame; Bow and Rifle with draw and scope; Bow Drill; Bucket and Canteen; traps; Knife; generic shape for the rest; pushing pose on the Hand Cart; split c1 fishing and light, c2 weapons and the rest), then M0d (remove the idle equipped line). Idea logged 2026-11-05: melee (fists on an empty-handed click; Knife slash and stab for defense), a read-only plan request first; it uses the M0b swing poses and ToolEvents.
