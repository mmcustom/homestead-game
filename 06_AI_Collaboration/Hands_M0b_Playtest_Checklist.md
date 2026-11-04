# Hands and tools M0b (tools in hand with swing poses): playtest checklist (2026-11-05)

Code baseline 73ee84d. Not committed yet. Stop Play Mode before sending anything to Claude Code. Play from your slot 4 save. Maximize the Game tab with Shift+Space. Do not restore any save.

What M0b is: eight tools are now drawn in your hands as rough primitive shapes: Axe, Primitive Axe, Saw, Hammer, Shovel, Iron Shovel, Stone Pick Axe and Iron Pick Axe. Stone tools have a grey head, iron and the Axe and Saw have dark steel, handles are wood, and the Hammer is a wooden mallet. Every other tool (Rod, Torch, Bucket, bows and so on) still shows the plain block or staff until M0c. When you swing, the hands swing with you: the blow, its sound and its count still happen at the instant of the click (nothing about timing or gameplay changed), and the picture shows the strike landing a moment later (about 0.15 s), then follows through and recovers by the time the next blow is due, so a held click looks like repeated chopping. Chop, split, mine, dig, saw and hammer each have their own motion; the last blow of a job is a slightly bigger swing; swinging with no stamina gives a droop with no strike. The crosshair text line is unchanged (M0d).

Mike wants to feel how no-wind-up looks, so item 17 is the main question.

You need the tools in your pack. If you have no Iron Shovel or Iron Pick Axe, skip the iron looks or forge them (the Smithy dev keys from M4).

Menu items (top menu bar, Homestead > Art): Create Test Axe Prefab and Remove Test Axe Prefab (items 23 and 24). Do those with Play Mode stopped.

Reply with the numbers that pass and describe anything that does not. Also tell me what you think of the tool shapes and sizes, the colours, and the swings.

## A. Tools in hand

1. Axe: a wooden handle with a steel head, the right hand holding the handle and the left hand steadying it. You can see the head from three-quarter view, not edge-on.
2. Primitive Axe: like the Axe but with a stone-grey head.
3. Saw: a blade with a handle. Both hands show.
4. Hammer: a mallet with a heavy head on a short handle, in the right hand only. The left hand is hidden.
5. Shovel (stone grey blade) and Iron Shovel (dark steel blade): both on a long handle, and clearly different in colour.
6. Stone Pick Axe (grey, pointed head) and Iron Pick Axe (dark steel): clearly different in colour.
7. Other tools (Fishing Rod, Torch, Bucket, a bow and so on) still show the plain placeholder block or staff, no errors.

## B. Swings

8. Chop a tree with the Axe, holding the click. At each blow the sound and the count come at the click and the hands strike a moment later; they follow through and are back in the ready position when the next blow is due, so it chains smoothly with no stuck or jumping pose.
9. The last blow that fells the tree is a slightly bigger swing than the others.
10. Split a Log at a wood pile: it looks like a chop (the same arc).
11. Dig a stump, then Clay or Sand, with a Shovel: a shorter downward push with a lean.
12. Mine stone at the South Ridge outcrop with a Pick Axe: like a chop but shorter and more forward.
13. Cut with the Saw: a short push and pull stroke, no big arc.
14. Build with the Hammer (a site): a short forward tap.
15. Swing until you run out of stamina: the hands do a half raise that droops with no strike, and the "Too tired to swing" message still shows.
16. Click once, then release: one swing, then the hands settle back to ready and do not get stuck.

## C. How it feels

17. Does the strike landing at the click, with the sound and count at the same moment, feel right? Is the strike quick enough, is the recovery too slow or too visible, would you like a visible wind-up before the blow (which would mean the sound and count come later; a gameplay timing change)? Describe it in your own words.

## D. Mixing it with everything else

18. Switch tools in the middle of a swing: no leftover tool, no stuck pose, no error.
19. Walk, sprint and crouch with each tool in hand: the same sways as in M0a, with the tool following the hand.
20. Stand right up against a wall or a tree trunk with the Shovel or the Axe: the tool does not poke through.
21. Rifle scope still hides the hands, and the hands still hide behind the Inventory, Journal, Finances, pause and sleep screens.
22. Make the Game tab 21:9 and 32:9: the tools are in view and not clipped.

## E. Swapping in a prefab (the swap point)

23. Stop Play Mode. Homestead > Art > Create Test Axe Prefab (no error or warning in the Console). Enter Play Mode and equip the Axe: an orange handle with cyan and magenta double heads and small yellow marker cubes replaces the primitive Axe, the left hand sits on the left marker, and the swing still works with it.
24. Stop Play Mode. Homestead > Art > Remove Test Axe Prefab. The Assets/Resources/Art folder is gone. Enter Play Mode: the primitive Axe is back.

## F. Regression

25. The tools work exactly as before: swing counts (for example Stump 6 swings with the stone Shovel, 5 with iron), stamina, sounds, saves.
26. The text line under the crosshair is unchanged.
27. Console shows no new errors or warnings (the PlayerController.cs:217 NullReferenceException on stopping Play Mode is the old known one).
28. Git shows nothing under Assets/Resources/Art (Claude Code checks this at commit time).

---

# Update after your feel feedback (2026-11-05): the pull-back

You asked for a pre-loaded pull-back before the blow for the Axe, Hammer and Pick Axes. Decision: visual only, no delay (the blow, sound, count and stamina still happen at the click; ContactSeconds 0.15 and the 0.9 s cycle are unchanged). How it works: the Axe, Primitive Axe, Hammer, Stone Pick Axe and Iron Pick Axe now rest COCKED, with the tool drawn back and up behind the hand, so a first blow after a pause strikes from a pulled-back hand. Each swing ends on a LOADED pose, a little farther back than cocked, so a held click shows a pull-back before every blow; with no new blow the hands settle from loaded to cocked over a quarter second. The Saw and both Shovels are not cocked and look as before. Suite 1735 of 1735.

Items 8, 9, 15 and 16 above now include this. Re-test these as well:

P1. Equip the Axe, Hammer and a Pick Axe: each rests drawn back, the head up and to the right of the crosshair, never covering the crosshair or the middle of the screen, the left sleeve staying low.
P2. Click once after standing still: the strike comes from the cocked pose with no delay (sound and count at the click), then the hands settle back to cocked and do not stay stuck loaded.
P3. Hold the click on a tree: each blow shows a visible pull-back to the loaded pose before it, and the rhythm stays the same as before (about 0.9 s between blows).
P4. The Saw and both Shovels rest in the plain neutral hold, as before.
P5. The blow that finishes a job is still the bigger swing; swinging with no stamina still droops from the cocked pose with no strike.
P6. Switch tools mid-swing: no stuck pose or leftover tool.
P7. Stand right against a wall with a cocked Axe: the tool is not poking through the wall.
P8. Tell me about the amount of pull-back: too little, too much, or right? Which tool looks best and which worst? (Constants in SwingRules: cock and load pitch and back per tool, SettleSeconds.)
