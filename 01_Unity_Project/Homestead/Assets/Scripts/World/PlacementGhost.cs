using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

// The translucent preview Placement Mode shows where a structure will go: green where it can be built, red where it can't.
//
// It is drawn by the structures' own builders (WoodPile.Bind / Shelter.Bind) run on a throwaway WoodPileState that is never
// added to WoodManager's piles, so it is not saved, not counted, not found by anything that looks piles up, and cannot feed
// a Rain Barrel. Neither Bind registers anything: they only add a collider and build child meshes, and neither class has an
// Awake, Start, OnEnable or event subscription. Right after Bind every collider is destroyed (so nothing can aim at, bump
// into or interact with it) and so is the WoodPile / Shelter component itself, leaving plain meshes on one GameObject,
// which Destroy removes with everything under it. A Campfire (a prefab with lights and fire) is drawn from a few
// primitives instead, and a Small Cabin site also gets a ground outline of its footprint.
public sealed class PlacementGhost
{
    static readonly Color Valid = new Color(0.25f, 0.9f, 0.3f, 0.45f);
    static readonly Color Invalid = new Color(0.95f, 0.25f, 0.2f, 0.45f);
    static readonly Color Warn = new Color(1f, 0.65f, 0.1f, 0.45f);

    readonly GameObject root;
    readonly List<Renderer> renderers = new List<Renderer>();
    Material validMaterial, invalidMaterial, warnMaterial;
    int shown = -1; // 1 valid, 2 valid with a warning (amber), 0 invalid, -1 not yet painted

    public GameObject Root => root;

    // A ghost of one Build-tab pile or shelter kind.
    public static PlacementGhost ForPile(PileKind kind)
    {
        var ghost = new PlacementGhost("Placement Ghost: " + WoodManager.PileName(kind));
        // A staged site (Well, Outhouse, Water Barrel, Rain Barrel, Food Cache) aims with the outline of the structure it will become;
        // once placed the site draws its plot (PlacedStructure) until it is completed. Every other kind aims as itself.
        PileKind drawn = WellsAndOuthouses.IsStagedSite(kind) ? WellsAndOuthouses.FinalKind(kind) : kind;
        var state = new WoodPileState { id = -1, kind = drawn, builtDay = WellsAndOuthouses.Today }; // id -1: never a real pile
        Material bark = WoodManager.Instance != null ? WoodManager.Instance.SharedBark : null;

        if (state.IsPlacedStructure)
        {
            // The Well and the Outhouse are plain meshes from StructureVisuals (no component, no interaction box, no water-disc script),
            // and Finish() strips colliders and tints, so the ghost registers with nothing and cannot be aimed at or collided with.
            StructureVisuals.Build(ghost.root.transform, state, bark, null, out _, out _);
            ghost.Finish();
            return ghost;
        }

        if (state.IsShelter)
        {
            var shelter = ghost.root.AddComponent<Shelter>();
            shelter.Bind(state, bark);
        }
        else
        {
            var pile = ghost.root.AddComponent<WoodPile>();
            pile.Bind(state, bark);
        }

        if (kind == PileKind.CabinSite)
            ghost.AddFootprint(3.0f);
        ghost.Finish();
        return ghost;
    }

    // A ghost of a trap, built from the same prefab TrapManager places. The Trap component is never Bound (so it is not tied to any
    // TrapState or to TrapManager) and is destroyed with every collider right away, leaving plain meshes; the catch and bait
    // pieces (named Catch and Bait in the prefabs) are hidden. Nothing here is saved, rolled, swept or interactable.
    public static PlacementGhost ForTrap(Trap prefab, string name)
    {
        var ghost = new PlacementGhost("Placement Ghost: " + name);
        if (prefab != null)
        {
            GameObject copy = Object.Instantiate(prefab.gameObject, ghost.root.transform);
            copy.transform.localPosition = Vector3.zero;
            copy.transform.localRotation = Quaternion.identity;
            foreach (Transform piece in copy.GetComponentsInChildren<Transform>(true))
            {
                if (piece != copy.transform && (piece.name == "Catch" || piece.name == "Bait"))
                    piece.gameObject.SetActive(false);
            }
        }
        ghost.Finish();
        return ghost;
    }

    // A ghost of a Campfire: a ring of stones around crossed logs.
    public static PlacementGhost ForCampfire()
    {
        var ghost = new PlacementGhost("Placement Ghost: Campfire");
        for (int i = 0; i < 8; i++)
        {
            float a = i / 8f * Mathf.PI * 2f;
            ghost.Primitive(PrimitiveType.Sphere, new Vector3(Mathf.Cos(a) * 0.5f, 0.1f, Mathf.Sin(a) * 0.5f), Vector3.zero, new Vector3(0.22f, 0.18f, 0.22f));
        }
        foreach (float turn in new[] { 0f, 60f, 120f })
            ghost.Primitive(PrimitiveType.Cylinder, new Vector3(0f, 0.12f, 0f), new Vector3(90f, turn, 0f), new Vector3(0.1f, 0.4f, 0.1f));
        ghost.Finish();
        return ghost;
    }

    // A ghost of a Tripod: three poles leaning in to a point above the fire it will stand over.
    public static PlacementGhost ForTripod()
    {
        var ghost = new PlacementGhost("Placement Ghost: Tripod");
        var top = new Vector3(0f, 1.6f, 0f);
        for (int i = 0; i < 3; i++)
        {
            float a = i * 120f * Mathf.Deg2Rad;
            Vector3 bottom = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 0.6f;
            Vector3 along = top - bottom;
            ghost.Primitive(PrimitiveType.Cylinder, (top + bottom) * 0.5f, Quaternion.FromToRotation(Vector3.up, along).eulerAngles,
                            new Vector3(0.07f, along.magnitude * 0.5f, 0.07f));
        }
        ghost.Finish();
        return ghost;
    }

    PlacementGhost(string name)
    {
        root = new GameObject(name);
        root.hideFlags = HideFlags.DontSave;
    }

    public void SetPose(Vector3 position, float yaw) =>
        root.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));

    public void SetVisible(bool visible)
    {
        if (root != null && root.activeSelf != visible)
            root.SetActive(visible);
    }

    // Green or red; or, for a legal spot that has a warning, amber (warn is off by default, so every other ghost is unchanged). Cheap to
    // call every frame: it only repaints when the answer changes. The amber material is made the first time it is wanted.
    public void SetValid(bool valid, bool warn = false)
    {
        int want = valid ? (warn ? 2 : 1) : 0;
        if (want == shown)
            return;
        shown = want;
        if (want == 2 && warnMaterial == null)
            warnMaterial = MakeMaterial(Warn);
        Material material = want == 2 ? warnMaterial : valid ? validMaterial : invalidMaterial;
        Color tint = want == 2 ? Warn : valid ? Valid : Invalid;
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;
            if (material != null)
            {
                renderer.sharedMaterial = material;
            }
            else
            {
                // No transparent material available: an opaque tint on whatever the piece already uses.
                var block = new MaterialPropertyBlock();
                block.SetColor("_BaseColor", tint);
                renderer.SetPropertyBlock(block);
            }
        }
    }

    public void Destroy() => Release(false);

    // Gone this instant, with its materials. For when Play Mode is ending: a plain Destroy never runs while it tears down, and a ghost made
    // with HideFlags.DontSave then outlives it in the editor with its materials destroyed (solid magenta).
    public void DestroyNow() => Release(true);

    void Release(bool immediately)
    {
        if (root != null)
            Dispose(root, immediately);
        if (validMaterial != null)
            Dispose(validMaterial, immediately);
        if (invalidMaterial != null)
            Dispose(invalidMaterial, immediately);
        if (warnMaterial != null)
            Dispose(warnMaterial, immediately);
        renderers.Clear();
    }

    static void Dispose(Object target, bool immediately)
    {
        if (immediately)
            Object.DestroyImmediate(target);
        else
            Object.Destroy(target);
    }

    // --- Building the look ---

    GameObject Primitive(PrimitiveType shape, Vector3 position, Vector3 euler, Vector3 scale)
    {
        GameObject piece = GameObject.CreatePrimitive(shape);
        piece.transform.SetParent(root.transform, false);
        piece.transform.localPosition = position;
        piece.transform.localRotation = Quaternion.Euler(euler);
        piece.transform.localScale = scale;
        return piece;
    }

    // A thin square on the ground, size metres a side, so a Small Cabin site shows how much room it takes.
    void AddFootprint(float size)
    {
        float half = size / 2f;
        Primitive(PrimitiveType.Cube, new Vector3(0f, 0.03f, -half), Vector3.zero, new Vector3(size, 0.03f, 0.08f));
        Primitive(PrimitiveType.Cube, new Vector3(0f, 0.03f, half), Vector3.zero, new Vector3(size, 0.03f, 0.08f));
        Primitive(PrimitiveType.Cube, new Vector3(-half, 0.03f, 0f), Vector3.zero, new Vector3(0.08f, 0.03f, size));
        Primitive(PrimitiveType.Cube, new Vector3(half, 0.03f, 0f), Vector3.zero, new Vector3(0.08f, 0.03f, size));
    }

    // Strips everything that could make it more than a picture, then gathers the meshes to paint.
    void Finish()
    {
        foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
            Object.DestroyImmediate(collider);
        // The builder components themselves go too (this is also what keeps their interaction prompts off a ghost).
        WoodPile pile = root.GetComponent<WoodPile>();
        if (pile != null)
            Object.DestroyImmediate(pile);
        Shelter shelter = root.GetComponent<Shelter>();
        if (shelter != null)
            Object.DestroyImmediate(shelter);
        foreach (Trap trap in root.GetComponentsInChildren<Trap>(true))
            Object.DestroyImmediate(trap); // a trap ghost: never bound, never interactable

        validMaterial = MakeMaterial(Valid);
        invalidMaterial = MakeMaterial(Invalid);
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.SetPropertyBlock(null); // pieces tinted with a property block (thatch, water) would override the ghost colour
            renderers.Add(renderer);
        }
    }

    // A see-through HDRP Lit. Resources/PlacementGhost.mat (made once in the Editor) is preferred, because a material that is
    // in the project has its shader variants built into a player; failing that one is made here, which is fine in the Editor.
    static Material MakeMaterial(Color color)
    {
        Material template = Resources.Load<Material>("PlacementGhost");
        Material material;
        if (template != null)
        {
            material = new Material(template);
        }
        else
        {
            Shader shader = Shader.Find("HDRP/Lit");
            if (shader == null)
                return null;
            material = new Material(shader);
            material.SetFloat("_SurfaceType", 1f);      // transparent
            material.SetFloat("_BlendMode", 0f);        // alpha
            material.SetFloat("_RenderQueueType", 5f);  // HDRP's "Transparent" queue
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_AlphaCutoffEnable", 0f);
            material.SetFloat("_Smoothness", 0.1f);
            material.SetFloat("_Metallic", 0f);
            HDMaterial.ValidateMaterial(material);
        }
        material.SetColor("_BaseColor", color);
        return material;
    }
}
