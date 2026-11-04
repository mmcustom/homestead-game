using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

// Hands and tools M0a: the custom pass that draws the first-person hands. HDRP has no camera stacking, so the hands (on the Viewmodel layer, which the main camera does not draw) are rendered by
// a hidden second camera with its own field of view into the main camera's colour buffer, against a depth buffer of their own that is cleared first. So they are lit like the world
// (same lights, same exposure: the pass runs before post-processing), never cut by a wall (the world's depth is not used), and not magnified when the rifle scope narrows the main camera's
// field of view (the hidden camera's field of view is its own). The pass is created in code by PlayerHands, so no scene or asset is edited.
[System.Serializable]
public class ViewmodelPass : CustomPass
{
    // Set by PlayerHands: the hidden camera, the main camera this pass belongs to, and whether there is anything to draw this frame.
    public Camera viewCamera;
    public Camera worldCamera;
    public LayerMask layerMask;
    public bool draw;

    RTHandle depth;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        depth = RTHandles.Alloc(Vector2.one, TextureXR.slices, DepthBits.Depth32, dimension: TextureXR.dimension, useDynamicScale: true, name: "Viewmodel Depth");
    }

    // How many times Execute was called, and how many of those drew: a diagnostic for the tests and the playtest (PlayerHands shows them in the Editor).
    public int calls, draws;

    // The custom passes are culled separately from the main camera, whose mask leaves the Viewmodel layer out on purpose: this puts it back for the pass alone.
    protected override void AggregateCullingParameters(ref ScriptableCullingParameters cullingParameters, HDCamera hdCamera)
    {
        cullingParameters.cullingMask |= (uint)layerMask.value;
    }

    protected override void Execute(CustomPassContext ctx)
    {
        calls++;
        if (!draw || viewCamera == null || worldCamera == null || ctx.hdCamera.camera != worldCamera)
            return;

        draws++;
        // HDRP's Lit forward pass tests depth as Equal for opaque objects (it expects a depth prepass to have run, material value _ZTestDepthEqualForOpaque), and this pass clears its own
        // depth first, so nothing would ever pass. LessEqual with depth write on makes the hands draw (and sort among themselves) as ordinary opaque objects.
        var depthOverride = new RenderStateBlock(RenderStateMask.Depth) { depthState = new DepthState(true, CompareFunction.LessEqual) };
        CustomPassUtils.RenderFromCamera(ctx, viewCamera, ctx.cameraColorBuffer, depth, ClearFlag.Depth, layerMask, overrideRenderState: depthOverride);
    }

    protected override void Cleanup()
    {
        depth?.Release();
        depth = null;
    }
}
