# Hands Sleeve Fix Re-test (see-through arms in interiors)

Version 0.1, 2026-11-05. Code baseline e13ebe4 (M0d) plus the uncommitted sleeve fix. Full suite 1839 of 1839 in one run (1836 plus 3 new). Console expected 0 errors and 0 warnings. Saves hash-identical to SavesBeforeSleeveFix. Editor left on Bootstrap, Play Mode stopped, Game view Full HD. Game tab maximized (Shift+Space).
Pass format: reply "X1 - X10 pass" or list what failed with a note. If any stripes remain, send the exact place, time of day and tool in hand.

Cause (found after a wrong first guess): HDRP's ambient occlusion layer of the world was being multiplied onto the hands, so the dark creases between the cabin logs darkened the sleeves at the same screen spots. Fix: one line in the viewmodel pass gives the hands white (no) occlusion while they are drawn. No HDRP asset, Volume or project setting changed. The earlier receive-shadows change did nothing in HDRP and is now only a comment.

X1. Inside the Small Cabin at about 06:05 with the Rifle: both sleeves are solid and evenly lit, no stripes or bands, and the wall does not show through.
X2. Same spot with the Axe, with empty hands and with the Bucket: solid sleeves and forearms.
X3. Same cabin at midday: solid.
X4. Other places with creases or slats near you: other interiors (Workshop, Smithy and so on), under a shelter roof, beside a log pile or a slat fence, in tree shade. Sleeves are solid in each.
X5. Outdoors in daylight: the hands look the same as before (solid, same colours).
X6. At night: the hands are dark like the world around them and do not glow. A lit Torch or Lantern lights them naturally.
X7. The world itself looks unchanged: corners, under roofs and between logs are as dark as before (the world keeps its ambient occlusion).
X8. The hands do not look flat or odd under the sleeve and cuff. Claude Code could not see a difference outdoors.
X9. Everything else is unchanged: tool poses, the 2.5 s fade-in names, prompts, bows, hanging items.
X10. Console shows 0 errors and 0 warnings after the whole session.

## Feel questions (one line each)

- Are the stripes gone in your cabin?
- Do the hands look worse anywhere without the occlusion shading?
