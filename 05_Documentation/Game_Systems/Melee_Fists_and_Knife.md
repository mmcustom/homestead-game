# Homestead
## Melee: Fists and Knife v0.1 (plan)

Status: planned 2026-11-05 from Claude Code's read-only plan (nothing built yet). Mike's idea: left click with nothing in your hands attacks with fists; with a Knife it slashes and stabs, for defense. Working name: Hands M0e. Builds on Hands M0a (hands), M0b (tools in hand, swing poses, ToolEvents, cocked and loaded pull-back; see Hands_and_Held_Tools.md). Standing rule: every visual has one swap point for a prefab (Art_Readiness_and_Placeholders.md).

---

# What exists today (Claude Code's findings)

- No health or damage system on any animal. Animal has no hit points, only a state (idle, walk, flee, dead), a "wounded and will die" flag and a tier (Small, Medium, Large). Shots go through HuntingWeapon.Fire into Animal.OnShot: small game dies from any solid hit; bigger game needs a vitals hit and a roll against a clean-kill chance, otherwise it is wounded, flees, and 30% or 60% of the time dies after a short run leaving a reduced Carcass. A kill becomes a Carcass (not persisted), field dressed with a Knife in the pack.
- Five species, all passive: Deer, Turkey, Duck, Rabbit, Squirrel. They flee when they hear you, scaled by crouch and sprint, wind and rain; every animal bolts from about 3 m even if you creep. WildlifeManager.Alert(position, radius) startles everything near a point (the rifle's gunshot and an arrow miss use it).
- No enemy attacks the player. No predators in code; the wildlife doc lists Fox, Coyote and Bobcat as "future expansion, deferred". The player's only external damage source is SurvivalManager.Injure(amount), used once for the creek flood knock-down; it never takes Health below the injury floor (10) and does nothing when collapsed.
- Input: nothing reads the left click when the hands are empty. The attack action (Player/Attack) is read by AxeTool, FishingRod, HuntingWeapon, PortableLight, BowDrill, TrapSetter and TapManager, each only while its tool is equipped. PlacementMode reads the left button directly and the right button to cancel (the right button is also rifle aim). PlayerController.CanUseTools is already the gate (no tools while placing, knocked down by the creek, or pushing the cart). E and R are separate actions.
- Knife.cs is nine lines (an id and a "carried" test). Crafted from 1 Stick, 1 Cordage and 1 Stone and in the starting kit. It only needs to be in the pack for field dressing and for taking game out of snares and box traps. Equippable (Tool category) but does nothing equipped; shows the generic placeholder in hand.

---

# The plan

Fists (empty hands, left click): alternating right, left, right punches. Cycle about 0.6 s (faster than the 0.9 s tool swing), strike lands about 0.10 s after the click, about 2 stamina a punch (tool swing is 4), reach about 1.4 m, found with the same aim raycast the tools use (player.AimRaycast). No tool: the primitive hands themselves punch from rest, no cocked pose.

Knife (equipped): left click slash (cycle about 0.7 s, about 3 stamina, reach 1.5 m, strike at about 0.12 s, a diagonal arc across the view); Sprint plus click stab (cycle about 0.8 s, about 3 stamina, reach 1.6 m, strike at about 0.10 s, a straight forward thrust with a slight lean, a slightly higher kill chance on small game and a small chance to wound a larger animal, which then flees). The Knife uses the M0b "visual only, no delay" approach: the blow, sound, count, stamina and hit test happen at the click; a cocked ready pose and a loaded pose; the strike is drawn about 0.10 to 0.15 s later.

Hits: small game dies from any solid hit (fists: a small animal that has not noticed you dies, otherwise it is startled; a fist hit on a Deer only startles it). A melee hit calls WildlifeManager.Alert with a small radius (about 8 m for fists, 6 m for the Knife), far less than a gunshot. No retaliation (nothing hostile exists). The hit resolves at the click, as the axe does. Melee is a way to dispatch a trapped, wounded or surprised small animal, not a reliable way to hunt.

Hands: new WorkKinds Punch, Slash, Stab in the pure SwingRules; the right and left hands need their own swing state because fists alternate (today one SwingState for the right hand only: the main structural change). New ToolEvents event for a melee blow (which hand or which attack); the existing Swung and the AxeTool hooks do not change. A new MeleeAttack script (the click when empty-handed or Knife-equipped, behind PlayerController.CanUseTools).

Swap points: the Knife: Resources/Art/tool_knife.prefab through ArtSwap.TryAttachTool (add knife to ToolWork.ViewToolIds; anchors Anchor_Grip_R, optional Anchor_Tip, no left grip), the shape in ToolViewPrimitives (a short blade and a handle, stone-grey blade); PlayerHands.ShowHeld stays the one deciding place. Fists: no new swap point; the hands are hands_player.prefab through ArtSwap.TryAttachHands (an optional State_Fist child on each hand is the natural extension, not built now).

Text: only an aimed-target prompt (for example "Rabbit: click to punch", "Rabbit: click to slash, hold Sprint to stab") while aiming at an animal in reach; no permanent equipped line (M0d); the fade-in name covers a Knife switch. One Know-how entry ("Fighting with your hands and a Knife"), shown when you first have a Knife or first punch; no journal entry. No save field expected (Carcasses are not persisted, animals respawn, melee and ready-pose state is momentary).

---

# Phases (order, Mike 2026-11-05: M0c first, then M0d, then melee)

- M0e-1 Fists: alternating punches that hit small game in reach (a trapped, wounded or dead one); hands punch from rest. New MeleeAttack, SwingRules (Punch), PlayerHands (second swing state), ToolEvents, a small Animal method. About 22 tests. Medium.
- M0e-2 Knife: slash and Sprint-click stab with their own poses, the cocked and loaded ready pose, the Knife shape and tool_knife swap point. ToolViewPrimitives, SwingRules, ToolWork table. About 20 tests. Medium.
- M0e-3 Animal reactions and teaching: a hit animal flees or drops, leaves a Carcass, the small noise scare, aimed-target prompts, the Know-how entry. Animal, WildlifeManager, KnowHowRules. About 16 tests. Small to medium.

---

# Decisions (Claude Code's 6 recommendations; Mike's answers, 2026-11-05: 1, 2, 5, 6 accepted as recommended; 3 and 4 CHANGED, see below)

1. What melee can hit: small game only (Rabbit, Squirrel, a Turkey or Duck if it lets you close in) plus a dead or wounded animal; Medium and Large game (Deer) get startled and flee, never hurt.
2. Animals hurting the player: no (nothing hostile exists; predators are deferred in the design doc; SurvivalManager.Injure exists for a later predator phase).
3. Stab input: CHANGED by Mike: the Knife alternates slash then stab on each click, with no extra key (Claude Code recommended Sprint plus click). The first click after a pause starts with a slash. Taught by the Know-how entry and the aimed-target prompt.
4. Fists on small game: CHANGED by Mike: fists NEVER kill; a fist hit stuns and startles (Claude Code recommended a chance to kill an unaware small animal). Only the Knife, traps, bows and the rifle kill. Open for the build message: what a fist hit does to an already wounded or trapped small animal (Claude Code to propose; default: stun and startle only).
5. Pull-back for the Knife and fists: the same visual-only, no-delay approach; fists have no cocked pose.
6. One Know-how entry; no journal entry.

---

# Risks and not verified

- Input conflicts: PlacementMode reads the left button directly and must stay ahead of melee (the CanUseTools gate covers it); other UI screens' click handling not checked.
- Hit detection: the aim raycast stops at the first collider, so a punch cannot reach through a wall (verify with a test wall).
- Balance: fists and the Knife on small game could undercut snares and the bow; the kill chance and the "not alert" gate are the knobs.
- A trapped animal sits inside TrapManager (not a live Animal), so "hit a trapped animal" may need its own path; whether TrapManager catches can be struck is not checked.
- Not verified: Play Mode behaviour, feel, the stamina numbers and cadence.
