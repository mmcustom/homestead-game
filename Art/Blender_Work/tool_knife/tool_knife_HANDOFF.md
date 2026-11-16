# tool_knife: handoff to Copilot (Unity side)

Made in Blender by Claude, 2026-11-14. Pilot piece for the Blender art agent. Source folder: `Art\Blender_Work\tool_knife\`.

## What it is
Knapped flint blade, stripped-branch wooden handle and sinew wrap. This matches the primitive in ToolViewPrimitives ("knife": stone blade, wood handle, lashing).
- 7,482 triangles: Knife_Blade 3,006, Knife_Handle 2,558, Knife_Lashing 1,918. Budget is 15,000.
- Overall length 0.33 m, from the handle butt at Z -0.088 to the tip at Z +0.24. The tip matches the primitive's tip (0, 0, 0.24).

## Files
- `tool_knife_model.fbx`: three meshes plus the empties `Anchor_Grip_R` and `Anchor_Tip`.
- `textures\`: per material (flint, wood, sinew), each 2048 x 2048:
  - `_basecolor` (sRGB)
  - `_normal` (tangent space, OpenGL style)
  - `_mask` (HDRP mask: R metallic 0, G occlusion 1, B 0, A smoothness)
  - `_roughness` is an intermediate. Do not import it.
- `tool_knife.blend` is the source. `review\baked_check.png` is the render from baked maps only.

## Steps in Unity
1. Copy the FBX and the 9 basecolor, normal and mask PNGs into `Assets/Resources/Art/`. Do not commit that folder; the ArtSwap test at line 558 still fails while it exists.
2. Set the normal maps' Texture Type to Normal map. Untick sRGB on the mask maps.
3. Make three HDRP/Lit materials: `tool_knife_flint`, `tool_knife_wood` and `tool_knife_sinew`. Assign base colour, normal and mask.
4. Make the prefab `tool_knife.prefab`, wrapping the FBX the same way as the axe and hands. The FBX and the prefab must keep different names.

## Orientation: what to check (the reason for this pilot)
Authored in tool space, with the handle along Blender -Y and the edge along Blender -Z. Exported with forward -Z, up Y, apply scale All and apply transform. Expected in Unity, with no wrapper turn:
- The blade points along +Z. Anchor_Tip is near (0, 0.004, 0.24).
- The cutting edge faces -Y.
- Anchor_Grip_R is at (0, 0, 0), mid-handle.

**X-axis marker:** a small knot sticks out of the handle on Blender's +X side, about 4.5 cm behind the grip (Unity Z about -0.045).

Report:
- Which side of the handle the knot is on in Unity: +X or -X.
- Whether the blade came in along +Z or -Z.

That settles the axis rule for every future tool (the axe could not, being symmetric). If the blade arrives along -Z, use the axe fix: turn the wrapper 180 degrees about Y.

## Check in game
Hold the Knife and check:
- Grip seats in the right hand.
- Slash and stab read correctly, with the point leading on the stab.
- It looks right under daytime and overcast HDRP light.

The holdPitch of -22 degrees in ToolViewPrimitives was tuned for the primitive and may need a small re-tune.
