# Brief for Claude Code: bring tool_knife into Unity

Hand this whole file to Claude Code, started in the repo root: `C:\Users\mmcus\OneDrive\Documents\Homestead Game`.

The knife was made in Blender by Claude (chat) on 2026-11-14 and is the pilot piece for the Blender art pipeline. Copilot is no longer on this project. Claude Code now does all Unity Editor and C# work for the art pipeline; Claude (chat) makes the art in Blender.

## Background to read first
- `Art_Needed_List.md` (find it under 05_Documentation or the repo). Read "What every supplier must follow", "Blender to Unity notes" and "Realistic hands: Blender to Unity findings".
- `01_Unity_Project\Homestead\Assets\Scripts\World\ArtSwap.cs`: the tool swap point (ToolSpecs, TryAttachTool, ToolAnchor). Read the header comment above `ToolPrefix`.
- `01_Unity_Project\Homestead\Assets\Scripts\Player\ToolViewPrimitives.cs`: `LayoutFor("knife")` gives holdPitch -22 and tip (0, 0, 0.24).
- `Assets\Resources\Art\tool_axe.prefab` and the hands prefabs: how a wrapper prefab around an FBX is set up today. Copy that pattern.

## What was delivered (in `Art\Blender_Work\tool_knife\`)
- `tool_knife_model.fbx` contains:
  - Meshes Knife_Blade (3,006 tris), Knife_Handle (2,558) and Knife_Lashing (1,918). Total 7,482; the budget is 15,000.
  - Empties Anchor_Grip_R and Anchor_Tip.
- `textures\`, 2048 x 2048, one set per material (flint, wood, sinew):
  - `tool_knife_<mat>_basecolor.png` (sRGB)
  - `tool_knife_<mat>_normal.png` (tangent space, OpenGL style)
  - `tool_knife_<mat>_mask.png` (HDRP mask: R metallic 0, G occlusion 1, B 0, A smoothness)
  - `*_roughness.png` are intermediates. Do NOT import them.
- `review\baked_check.png`: how it should look. `tool_knife.blend` is the source; do not edit it.

## Tasks
1. **Fix the ArtSwap test first.** `TheEditorMenuMakesAndRemovesTheTestKilnPrefabAndRefusesToTouchAnyOther` (around line 558 of the ArtSwap tests) asserts that `Assets/Resources/Art` does not exist, so it fails whenever real art is present. Make the test check only its own test prefab path. Run the EditMode tests and confirm they pass with the art present.
2. **Import:** copy the FBX and the 9 basecolor, normal and mask PNGs into `Assets/Resources/Art/`. Then set the import settings:
   - Normal maps: Texture Type = Normal map.
   - Mask maps: sRGB off.
   - FBX: same import settings as `tool_axe_model.fbx` (check its .meta).
3. **Materials:** make three HDRP/Lit materials, `tool_knife_flint`, `tool_knife_wood` and `tool_knife_sinew`, with base colour, normal and mask maps assigned. Assign them to the three meshes.
4. **Prefab:** `Assets/Resources/Art/tool_knife.prefab`, wrapping the FBX the way tool_axe does. The prefab and the FBX must keep different names (the FBX is `tool_knife_model`), or Resources.Load returns the FBX. The prefab root has zero rotation. Its anchors must keep their names.
   - Prefab work needs the Unity Editor. Either write a small Editor script with a menu item for Mike to run, or run Unity in batch mode with `-executeMethod`. Batch mode only works while the project is closed in the Editor, so ask Mike which.
5. **Orientation check:** this is the main reason for the pilot. Expected with no wrapper turn:
   - Blade along +Z.
   - Anchor_Tip at about (0, 0.004, 0.24).
   - Cutting edge toward -Y.
   - Anchor_Grip_R at (0, 0, 0).

   **X marker:** a knot on the handle was authored on Blender's +X side, about 4.5 cm behind the grip (Unity Z about -0.045). Record which Unity side it lands on (+X or -X) and whether the blade came in along +Z or -Z. If it is along -Z, fix it in the wrapper (turn 180 degrees about Y), as for the axe.
6. **In game:** Mike holds the Knife in Play Mode and checks:
   - Grip seats in the right hand.
   - Slash and stab read right, with the point leading on the stab.
   - It looks right in daytime and overcast light.

   Re-tune holdPitch only if needed.

## Rules
- Do NOT commit `Assets/Resources/Art` or anything in `Art\`. They stay out of git on purpose. Commit only the test fix and any Editor script, and only when Mike asks.
- Ask Mike before any step that cannot be undone.

## Report back (Mike passes this to Claude in chat)
Add the result to the "Blender to Unity notes" in Art_Needed_List.md:
- Knot side, +X or -X.
- Blade direction.
- Any wrapper fix used.
- Final anchor positions in the prefab.
- Anything that looked wrong in game.

The confirmed axis rule then goes into the homestead-blender-art skill, so later pieces need no fixing.
