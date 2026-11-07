using System;
using System.Collections.Generic;
using UnityEngine;

// THE SWAP POINT for structure visuals (Art readiness A1, Mike's standing rule 2026-11-04). A prefab at Resources/Art/<id>.prefab replaces the primitive version of a structure;
// with no prefab there nothing changes: StructureVisuals.Build draws the same primitives as before. The one place the swap happens is the first line of StructureVisuals.Build,
// which calls ArtSwap.TryBuild; everything about the swap lives here.
//
// What a prefab author does (no code needed):
//   * Name the prefab structure_<pilekind> (structure_kiln). Pivot at the centre of the footprint on the ground, 1 unit = 1 m, the open front is +Z.
//   * Put the parts that always show under any children you like; a child named "Body" is kept on.
//   * Add the anchors named in the table below (empty children; keep them unrotated and unscaled). A missing anchor is made at today's rule position.
//   * Add State_* children (one is shown at a time) and On_* children (shown while the thing is happening). A missing one is simply never shown.
//   * Name a child Solid_<anything> to make it solid (a box collider fitted to its mesh). Every other collider in the prefab is removed: the interaction box and the footprint come from
//     the builder and the rules classes, never from the art.
//   * A particle system under Anchor_Smoke is switched on and off instead of the built-in smoke.
// Things that change at run time (the Firewood, the dry and fired pieces, the Ash) stay plain primitives, parented to the anchors named in Spec.contentAnchors.
//
// The prefab is instantiated once, as a child named after its id, and kept across PlacedStructure.Rebuild; each Rebuild only switches the named children and rebuilds the content
// primitives. The placement ghost goes through the same Build, so it shows the prefab too (PlacementGhost gives every renderer its green or red material).
// To add the next kind: one more Spec in Specs below, plus a Spec.staticPieces count and a contentSplit if the primitive builder draws changing contents.
public static class ArtSwap
{
    public const string Folder = "Art";
    public const string BodyName = "Body";
    public const string SolidPrefix = "Solid_";

    public const string AnchorSmoke = "Anchor_Smoke";
    public const string AnchorMouth = "Anchor_Mouth";
    public const string AnchorStock = "Anchor_Stock";

    // KilnSmoke.height's default: the vent is at 1.0 and the smoke starts a hand above it. A test pins it against the component.
    public const float KilnSmokeHeight = 1.1f;

    public sealed class AnchorSpec
    {
        public string name;
        public bool required;
        public Vector3 fallback; // where the code puts it when the prefab has none, in the structure's local space (today's rule coordinates)
    }

    // One row per visual kind. Everything the code needs to know about a prefab for that kind.
    public sealed class Spec
    {
        public string id;                 // structure_<pilekind>: the prefab is Resources/Art/<id>.prefab
        public PileKind kind;
        public AnchorSpec[] anchors;
        public string[] states;           // State_* names, one shown at a time (chosen by stateOf)
        public string[] flags;            // On_* names, each shown while flagsOf says so
        public Vector3 expectedSize;      // the art's width, height and depth in metres (the editor list warns when a prefab's bounds differ; nothing is resized)
        public Vector3 boxCenter;         // the interaction box, today's values from the builder (a test pins that they agree)
        public Vector3 boxSize;
        public int staticPieces;          // how many primitives the builder draws before the changing contents
        public string[] contentAnchors;   // where the changing contents are parented, in the order the builder draws them
        public Func<WoodPileState, int[]> contentSplit; // how many contents go to each anchor but the last, which takes the rest
        public Func<WoodPileState, string> stateOf;
        public Func<WoodPileState, string[]> flagsOf;
        public Func<WoodPileState, bool> smokeOn;

        public AnchorSpec Anchor(string anchorName)
        {
            foreach (AnchorSpec a in anchors)
                if (a.name == anchorName)
                    return a;
            return null;
        }
    }

    static AnchorSpec A(string name, bool required, float x, float y, float z) => new AnchorSpec { name = name, required = required, fallback = new Vector3(x, y, z) };

    static readonly string[] NoFlags = new string[0];

    // The spec table. Add the next kind here.
    static readonly Spec[] Specs =
    {
        new Spec
        {
            id = "structure_kiln",
            kind = PileKind.Kiln,
            anchors = new[]
            {
                A(AnchorSmoke, true, 0f, KilnSmokeHeight, 0f),  // KilnSmoke sits here (a prefab particle system under it is used instead)
                A(AnchorMouth, true, 0f, 0.45f, 0.95f),         // the loaded and the fired pieces, and the Ash, are set out here
                A(AnchorStock, false, -1.45f, 0f, 0f),          // the Firewood stack
            },
            states = new[] { "State_Empty", "State_Loaded", "State_Firing", "State_Done" },
            flags = new[] { "On_Firing" },
            expectedSize = new Vector3(2.4f, 1.1f, 2.4f),       // the pad is 2.4 across; the vent tops out at 1.1
            boxCenter = new Vector3(0f, 0.75f, 0f),
            boxSize = new Vector3(2.2f, 1.5f, 2.2f),
            staticPieces = 9,                                   // pad, dome, five ring stones, vent, mouth
            contentAnchors = new[] { AnchorStock, AnchorMouth },
            contentSplit = s => new[] { KilnFirewoodShown(s), int.MaxValue },
            stateOf = KilnStateOf,
            flagsOf = s => WellsAndOuthouses.KilnPhaseOf(s) == PotteryRules.KilnPhase.Firing ? new[] { "On_Firing" } : NoFlags,
            smokeOn = s => WellsAndOuthouses.KilnPhaseOf(s) == PotteryRules.KilnPhase.Firing,
        },
    };

    // The Firewood pieces the primitive Kiln draws: only while it is neither firing nor done.
    static int KilnFirewoodShown(WoodPileState s)
    {
        PotteryRules.KilnPhase phase = WellsAndOuthouses.KilnPhaseOf(s);
        if (phase == PotteryRules.KilnPhase.Firing || phase == PotteryRules.KilnPhase.Done)
            return 0;
        return Mathf.Min(s.Count(PotteryRules.FirewoodId), PotteryRules.KilnFirewood);
    }

    static string KilnStateOf(WoodPileState s)
    {
        switch (WellsAndOuthouses.KilnPhaseOf(s))
        {
            case PotteryRules.KilnPhase.Firing: return "State_Firing";
            case PotteryRules.KilnPhase.Done: return "State_Done";
            case PotteryRules.KilnPhase.Loading:
            case PotteryRules.KilnPhase.Ready: return "State_Loaded";
            default: return "State_Empty";
        }
    }

    public static IReadOnlyList<Spec> AllSpecs => Specs;

    // The spec for a kind, or null: most kinds have no spec yet and always draw their primitives.
    public static Spec SpecFor(PileKind kind)
    {
        foreach (Spec spec in Specs)
            if (spec.kind == kind)
                return spec;
        return null;
    }

    public static Spec SpecFor(string id)
    {
        foreach (Spec spec in Specs)
            if (spec.id == id)
                return spec;
        return null;
    }

    public static string StructureId(PileKind kind) => "structure_" + kind.ToString().ToLowerInvariant();

    // --- Finding the prefab ---

    // The one place a prefab is loaded from. A test replaces it to supply a prefab without a real asset in Resources.
    public static Func<string, GameObject> Loader = path => Resources.Load<GameObject>(path);

    static readonly Dictionary<string, GameObject> Cache = new Dictionary<string, GameObject>();

    // The prefab for an id, or null. Both answers are remembered, so a missing prefab costs one lookup for the whole run.
    public static GameObject Resolve(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;
        if (Cache.TryGetValue(id, out GameObject found))
            return found;
        GameObject prefab = Loader != null ? Loader(Folder + "/" + id) : null;
        Cache[id] = prefab;
        return prefab;
    }

    public static void ResetCache() => Cache.Clear();

    // Back to the real Resources loader with an empty cache (tests call this when they finish).
    public static void ResetForTests()
    {
        Loader = path => Resources.Load<GameObject>(path);
        Cache.Clear();
    }

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ClearOnEnterPlayMode() => Cache.Clear(); // with domain reload off, a prefab added or removed in Edit Mode is seen on the next Play
#endif

    // --- Building ---

    // The swap point's work. Returns false (and touches nothing) when the kind has no spec or no prefab exists: the caller then draws the primitives. Otherwise the prefab instance is
    // under parent (made the first time, found after that), its State_* and On_* children are set for the state, the changing contents are rebuilt as primitives at the anchors (and
    // added to pieces, which may be null as it is for the placement ghost), and the interaction box is reported.
    public static bool TryBuild(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        boxCenter = Vector3.zero;
        boxSize = Vector3.zero;
        Spec spec = SpecFor(state.kind);
        if (spec == null)
            return false;
        Transform art = Attach(parent, spec);
        if (art == null)
            return false;

        ApplyState(art, spec, state);
        BuildContents(art, spec, state, bark, pieces);
        boxCenter = spec.boxCenter;
        boxSize = spec.boxSize;
        return true;
    }

    // The instance under parent, made if it is not there yet; null when there is no prefab. Every collider in it is removed, then Solid_* children get a box fitted to their mesh.
    public static Transform Attach(Transform parent, Spec spec)
    {
        Transform existing = parent.Find(spec.id);
        if (existing != null)
            return existing;
        GameObject prefab = Resolve(spec.id);
        if (prefab == null)
            return null;

        GameObject instance = UnityEngine.Object.Instantiate(prefab, parent, false);
        instance.name = spec.id;
        instance.SetActive(true);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
            UnityEngine.Object.DestroyImmediate(collider);
        foreach (Transform t in instance.GetComponentsInChildren<Transform>(true))
            if (t != instance.transform && t.name.StartsWith(SolidPrefix))
                AddSolidCollider(t.gameObject);
        return instance.transform;
    }

    static void AddSolidCollider(GameObject solid)
    {
        BoxCollider box = solid.AddComponent<BoxCollider>();
        MeshFilter filter = solid.GetComponent<MeshFilter>();
        if (filter != null && filter.sharedMesh != null)
        {
            box.center = filter.sharedMesh.bounds.center;
            box.size = filter.sharedMesh.bounds.size;
        }
    }

    // Every transform called name below root (not root itself).
    public static List<Transform> Named(Transform root, string name)
    {
        var found = new List<Transform>();
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t != root && t.name == name)
                found.Add(t);
        return found;
    }

    // The anchor called name: the prefab's own, or an empty child made at the rule position so the code always has somewhere to attach.
    public static Transform AnchorOf(Transform art, Spec spec, string name)
    {
        List<Transform> own = Named(art, name);
        if (own.Count > 0)
            return own[0];
        var made = new GameObject(name);
        made.transform.SetParent(art, false);
        AnchorSpec anchor = spec.Anchor(name);
        made.transform.localPosition = anchor != null ? anchor.fallback : Vector3.zero;
        return made.transform;
    }

    // --- Hands (Hands and tools M0a) ---
    //
    // THE SWAP POINT for the first-person hands: a prefab at Resources/Art/hands_player.prefab replaces the primitive forearms and mittens PlayerHands builds. PlayerHands.BuildRig calls
    // TryAttachHands first and draws the primitives only when it returns null. The table is separate from the structure table above because a hands prefab has no PileKind, no state
    // and no changing contents; it is keyed by id. What a hands-prefab author does:
    //   * Name it hands_player. Origin at the camera, 1 unit = 1 m, x right, y up, z forward; it is parented to the camera and drawn by the viewmodel camera, so the world and the scope's
    //     zoom do not change how it looks. Author it at normal size: PlayerHands scales the whole rig toward the eye (viewScale, 0.5) so it stays inside the player's body radius. Every renderer is put on the Viewmodel layer and stops casting shadows; every collider is removed.
    //   * Hand_R and Hand_L: the objects the code moves for each hand (the arm and mitten under each). The left one is hidden on a one-handed tool. A missing one is made empty at the
    //     origin, so the code still has something to move, but then the art under the prefab root does not follow the pose.
    //   * Anchor_Hand_R and Anchor_Hand_L: a child of each hand where a held tool's grip is placed (the palm). Keep them unscaled. A missing one is made under its hand at the palm.
    //   * A prefab's own Animator is not driven yet.
    public const string HandsId = "hands_player";
    public const string AnchorHandR = "Anchor_Hand_R";
    public const string AnchorHandL = "Anchor_Hand_L";
    public const string HandRName = "Hand_R";
    public const string HandLName = "Hand_L";
    public const string ViewmodelLayerName = "Viewmodel";

    // Where a made-up palm anchor goes, in the hand's own space: a little ahead of the wrist.
    public static readonly Vector3 PalmFallback = new Vector3(0f, 0f, 0.07f);

    public sealed class ViewSpec
    {
        public string id;
        public AnchorSpec[] anchors;
        public string[] optionalChildren; // shown or hidden by the code, never required

        public AnchorSpec Anchor(string anchorName)
        {
            foreach (AnchorSpec a in anchors)
                if (a.name == anchorName)
                    return a;
            return null;
        }
    }

    static readonly ViewSpec[] ViewSpecs =
    {
        new ViewSpec
        {
            id = HandsId,
            anchors = new[]
            {
                A(AnchorHandR, true, PalmFallback.x, PalmFallback.y, PalmFallback.z),
                A(AnchorHandL, true, PalmFallback.x, PalmFallback.y, PalmFallback.z),
            },
            optionalChildren = new[] { HandRName, HandLName },
        },
    };

    public static IReadOnlyList<ViewSpec> AllViewSpecs => ViewSpecs;

    public static ViewSpec ViewSpecFor(string id)
    {
        foreach (ViewSpec spec in ViewSpecs)
            if (spec.id == id)
                return spec;
        return null;
    }

    // The hands prefab instance under parent (made the first time, found after that), or null when there is no prefab: PlayerHands then builds its primitives. Colliders are removed, every
    // renderer goes on the given layer and stops casting shadows.
    public static Transform TryAttachHands(Transform parent, int layer) => AttachHandsPrefab(parent, layer, HandsId);

    // Hands v3: the second pose of the same swap point, Resources/Art/hands_player_grip.prefab (fingers wrapped round a handle). Same names and anchors as hands_player; PlayerHands shows it
    // whenever a tool is in hand and the thumb-up hands_player otherwise. Null when there is no such prefab, and then hands_player is shown for both.
    public const string HandsGripId = "hands_player_grip";
    public static Transform TryAttachHandsGrip(Transform parent, int layer) => AttachHandsPrefab(parent, layer, HandsGripId);

    static Transform AttachHandsPrefab(Transform parent, int layer, string id)
    {
        Transform existing = parent.Find(id);
        if (existing != null)
            return existing;
        GameObject prefab = Resolve(id);
        if (prefab == null)
            return null;

        GameObject instance = UnityEngine.Object.Instantiate(prefab, parent, false);
        instance.name = id;
        instance.SetActive(true);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
            UnityEngine.Object.DestroyImmediate(collider);
        SetViewLayer(instance, layer);
        return instance.transform;
    }

    // Puts a whole object on a layer and stops its renderers casting shadows (the hands receive light but throw none).
    public static void SetViewLayer(GameObject root, int layer)
    {
        if (layer < 0)
            return;
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
        foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            // M0c-2 fix 2: nothing in the viewmodel receives the world's shadows. NOTE: HDRP ignores this flag (turning it on or off changed nothing in a real cabin), so it is only a statement of intent.
            // The see-through sleeve was really the world's ambient occlusion texture: ViewmodelPass hands the hands a white one while it draws them.
            r.receiveShadows = false;
            // The pass is culled with the main camera's frustum, which the rifle scope narrows; bounds this large keep the hands from being culled away then.
            r.localBounds = new Bounds(Vector3.zero, Vector3.one * ViewBoundsSize);
        }
    }

    // Local-space size of the bounds given to every viewmodel renderer (the hands are small and scaled down, so this still comes to a few metres).
    public const float ViewBoundsSize = 1000f;

    // The object the code moves for one hand: the prefab's Hand_R or Hand_L, or an empty one made at the origin.
    public static Transform HandRoot(Transform art, HandSide side)
    {
        string name = side == HandSide.Right ? HandRName : HandLName;
        List<Transform> own = Named(art, name);
        if (own.Count > 0)
            return own[0];
        var made = new GameObject(name);
        made.layer = art.gameObject.layer;
        made.transform.SetParent(art, false);
        return made.transform;
    }

    // Where a held tool's grip is placed: the prefab's Anchor_Hand_R or _L, or an empty one made under the hand at the palm. One found outside its hand is moved under it.
    public static Transform HandAnchor(Transform art, HandSide side)
    {
        string name = side == HandSide.Right ? AnchorHandR : AnchorHandL;
        Transform hand = HandRoot(art, side);
        List<Transform> own = Named(art, name);
        if (own.Count > 0)
        {
            if (!own[0].IsChildOf(hand))
                own[0].SetParent(hand, true);
            return own[0];
        }
        var made = new GameObject(name);
        made.layer = art.gameObject.layer;
        made.transform.SetParent(hand, false);
        AnchorSpec anchor = ViewSpecFor(HandsId).Anchor(name);
        made.transform.localPosition = anchor != null ? anchor.fallback : Vector3.zero;
        return made.transform;
    }

    // --- Tools in hand (Hands and tools M0b) ---
    //
    // THE SWAP POINT for the tools the hands hold: a prefab at Resources/Art/tool_<itemid>.prefab replaces the primitive shape ToolViewPrimitives builds. PlayerHands.ShowHeld is the one place
    // that decides: it calls TryAttachTool first and draws the primitive only when it returns null (and the generic placeholder for a tool outside the table). The table below is keyed by item
    // id, next to the hands table and separate from the structure table. M0b has eight rows: axe, primitive_axe, wood_saw, hammer, shovel, iron_shovel, stone_pick_axe, iron_pick_axe. Stone and
    // iron are separate ids with no fallback between them. What a tool-prefab author does:
    //   * Name it tool_<itemid> (tool_axe). Author it at normal size, in the tool's own space: the origin where the right hand grips, +Z along the handle toward the working end, -Y the cutting
    //     side. The hands rig scales it toward the eye. Every renderer goes on the Viewmodel layer and stops casting shadows; every collider is removed.
    //   * Anchor_Grip_R (required): where the right hand holds it. Anchor_Grip_L (optional): where the left hand steadies a two-handed tool. Anchor_Tip (optional): the working point.
    //   * A missing anchor is made at the primitive's own grip (ToolViewPrimitives.LayoutFor), so the tool still sits in the hand.
    //   * M0c-2 adds eighteen more rows, so every tool the hands can hold has one: primitive_bow and recurve_bow (Anchor_Nock: where an arrow sits on the string), bolt_action_rifle (Anchor_Muzzle),
    //     bow_drill, bucket, sap_bucket, canteen, spile, box_trap, fish_trap, rabbit_snare, knife, flint_and_steel, pouch, clay_pot, cooking_pot, potash_kettle and tarp. Only the grips and the tip
    //     are needed for those; Anchor_Muzzle is not attached to yet, it is named so a muzzle flash has a place (Anchor_Nock is now where the bows' right hand goes, see below).
    //   * M0c-2 fix, the BOWS (primitive_bow, recurve_bow) are held by the LEFT hand and drawn by the right: the bow sits at Anchor_Grip_L (REQUIRED: the handle, where the left hand holds it, and the bow
    //     never moves during a draw), and the right hand goes to Anchor_Nock (the string's middle: it rests there and is pulled back toward the face as the draw builds). Anchor_Grip_R is no longer used
    //     for the bows. A missing Anchor_Grip_L or Anchor_Nock still falls back to the primitive's coordinates. The primitive string follows the hand (String Upper, String Lower); a prefab's own string
    //     is left to the prefab.
    //   * M0c-2 fix, everything CARRIED BY A HANDLE, BAIL, STRAP OR CORD (bucket, sap_bucket, canteen, cooking_pot, potash_kettle, pouch, fish_trap, box_trap) hangs straight down in the view from
    //     Anchor_Grip_R: author it with the grip at the top (the bail, the handle, the cord) and the body below it along -Y. The code keeps it hanging by gravity whatever the hand does and swings it a little.
    //   * M0c-1 adds four rows: fishing_rod and cane_pole (Anchor_Grip_L for the left hand, Anchor_Line: where the fishing line starts; the world line is drawn from it while the rod is in hand) and torch and lantern
    //     (one-handed; Anchor_Flame: where the flame sits, and the held point light is moved to it while the light burns). A light's State_Lit and State_Unlit children are shown to match
    //     PortableLight's lit state (a missing one is simply never shown); the primitive flame and glow live under State_Lit. A ParticleSystem under Anchor_Flame is played while lit and stopped otherwise.
    public const string ToolPrefix = "tool_";
    public const string AnchorGripR = "Anchor_Grip_R";
    public const string AnchorGripL = "Anchor_Grip_L";
    public const string AnchorTip = "Anchor_Tip";
    public const string AnchorLine = "Anchor_Line";   // the rods: where the fishing line starts
    public const string AnchorFlame = "Anchor_Flame"; // the Torch and Lantern: where the flame and its light sit
    public const string AnchorNock = "Anchor_Nock";   // the bows: where an arrow is nocked on the string
    public const string AnchorMuzzle = "Anchor_Muzzle"; // the rifle: the end of the barrel
    public const string StateLit = "State_Lit";       // shown while the light burns (the primitive flame and glow are here)
    public const string StateUnlit = "State_Unlit";   // shown while it does not

    public sealed class ToolSpec
    {
        public string itemId;
        public string id;             // tool_<itemid>: the prefab is Resources/Art/<id>.prefab
        public bool twoHanded;
        public float holdPitch;       // degrees the tool leans up from the hand's forward
        public float holdRoll;        // degrees it is turned about its own handle
        public AnchorSpec[] anchors;

        public AnchorSpec Anchor(string anchorName)
        {
            foreach (AnchorSpec a in anchors)
                if (a.name == anchorName)
                    return a;
            return null;
        }
    }

    static readonly ToolSpec[] ToolSpecs = BuildToolSpecs();

    static ToolSpec[] BuildToolSpecs()
    {
        string[] ids = ToolWork.ViewToolIds;
        var specs = new ToolSpec[ids.Length];
        for (int i = 0; i < ids.Length; i++)
        {
            ToolViewPrimitives.Layout layout = ToolViewPrimitives.LayoutFor(ids[i]);
            specs[i] = new ToolSpec
            {
                itemId = ids[i],
                id = ToolPrefix + ids[i],
                twoHanded = layout.twoHanded,
                holdPitch = layout.holdPitch,
                holdRoll = layout.holdRoll,
                anchors = ToolAnchors(layout),
            };
        }
        return specs;
    }

    // The anchors a tool's row names: the grips and the tip for every tool, the line start on the rods and the flame on the lights.
    static AnchorSpec[] ToolAnchors(ToolViewPrimitives.Layout layout)
    {
        var anchors = new List<AnchorSpec>
        {
            A(AnchorGripR, !layout.hasNock, layout.gripRight.x, layout.gripRight.y, layout.gripRight.z),   // the right hand's grip, required on every tool but the bows
            A(AnchorGripL, layout.hasNock, layout.gripLeft.x, layout.gripLeft.y, layout.gripLeft.z), // the left hand: optional steadying on a two-handed tool, required on the bows (the handle)
            A(AnchorTip, false, layout.tip.x, layout.tip.y, layout.tip.z),
        };
        if (layout.hasLine)
            anchors.Add(A(AnchorLine, false, layout.line.x, layout.line.y, layout.line.z));
        if (layout.hasFlame)
            anchors.Add(A(AnchorFlame, false, layout.flame.x, layout.flame.y, layout.flame.z));
        if (layout.hasNock)
            anchors.Add(A(AnchorNock, false, layout.nock.x, layout.nock.y, layout.nock.z));
        if (layout.hasMuzzle)
            anchors.Add(A(AnchorMuzzle, false, layout.muzzle.x, layout.muzzle.y, layout.muzzle.z));
        return anchors.ToArray();
    }

    public static IReadOnlyList<ToolSpec> AllToolSpecs => ToolSpecs;

    // The row for a tool's item id, or null: a tool outside the table keeps the generic placeholder.
    public static ToolSpec ToolSpecFor(string itemId)
    {
        if (itemId == null)
            return null;
        foreach (ToolSpec spec in ToolSpecs)
            if (spec.itemId == itemId)
                return spec;
        return null;
    }

    public static string ToolId(string itemId) => ToolPrefix + itemId;

    // The tool's prefab instance under parent (a new one each time: PlayerHands frees the old one when the tool changes), or null when there is no prefab or the tool is not in the table.
    // Colliders are removed, every renderer goes on the layer and stops casting shadows. Its pose is left to PlayerHands (it is aligned to the grip).
    public static Transform TryAttachTool(Transform parent, string itemId, int layer)
    {
        ToolSpec spec = ToolSpecFor(itemId);
        if (spec == null)
            return null;
        GameObject prefab = Resolve(spec.id);
        if (prefab == null)
            return null;

        GameObject instance = UnityEngine.Object.Instantiate(prefab, parent, false);
        instance.name = spec.id;
        instance.SetActive(true);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
            UnityEngine.Object.DestroyImmediate(collider);
        SetViewLayer(instance, layer);
        return instance.transform;
    }

    // The tool's anchor called name: its own, or an empty one made at the primitive's grip. Anchor_Grip_L on a one-handed tool is nobody's business: null.
    public static Transform ToolAnchor(Transform tool, ToolSpec spec, string name)
    {
        if (name == AnchorGripL && !spec.twoHanded)
            return null;
        List<Transform> own = Named(tool, name);
        if (own.Count > 0)
            return own[0];
        var made = new GameObject(name);
        made.layer = tool.gameObject.layer;
        made.transform.SetParent(tool, false);
        AnchorSpec anchor = spec.Anchor(name);
        made.transform.localPosition = anchor != null ? anchor.fallback : Vector3.zero;
        return made.transform;
    }

    // Body stays on; exactly one of the spec's State_* children is shown; each On_* child follows its flag. Names the prefab does not have are skipped.
    public static void ApplyState(Transform art, Spec spec, WoodPileState state)
    {
        foreach (Transform body in Named(art, BodyName))
            body.gameObject.SetActive(true);

        string shown = spec.stateOf != null ? spec.stateOf(state) : null;
        foreach (string stateName in spec.states)
            foreach (Transform t in Named(art, stateName))
                t.gameObject.SetActive(stateName == shown);

        string[] on = spec.flagsOf != null ? spec.flagsOf(state) : NoFlags;
        foreach (string flag in spec.flags)
            foreach (Transform t in Named(art, flag))
                t.gameObject.SetActive(Array.IndexOf(on, flag) >= 0);
    }

    // Draws the primitive version once into a scratch object, throws away the fixed shapes (the prefab is those) and moves the changing contents to their anchors at the same
    // spots they have today: a content piece sits at (its old position - the anchor's rule position) under the anchor, so with an anchor at its rule position nothing moves.
    static void BuildContents(Transform art, Spec spec, WoodPileState state, Material bark, List<GameObject> pieces)
    {
        if (spec.contentAnchors == null || spec.contentAnchors.Length == 0)
            return;
        var scratch = new GameObject("Art Scratch");
        var made = new List<GameObject>();
        StructureVisuals.BuildPrimitive(scratch.transform, state, bark, made, out _, out _);

        int[] split = spec.contentSplit != null ? spec.contentSplit(state) : new int[spec.contentAnchors.Length];
        int slot = 0, used = 0;
        for (int i = 0; i < made.Count; i++)
        {
            GameObject piece = made[i];
            if (i < spec.staticPieces)
            {
                UnityEngine.Object.DestroyImmediate(piece);
                continue;
            }
            while (slot < spec.contentAnchors.Length - 1 && used >= split[slot])
            {
                slot++;
                used = 0;
            }
            used++;

            string anchorName = spec.contentAnchors[slot];
            Transform anchor = AnchorOf(art, spec, anchorName);
            Vector3 rule = spec.Anchor(anchorName).fallback;
            Vector3 local = piece.transform.localPosition - rule;
            piece.transform.SetParent(anchor, false);
            piece.transform.localPosition = local;
            pieces?.Add(piece);
        }
        UnityEngine.Object.DestroyImmediate(scratch);
    }

    // The smoke of a structure with a prefab: a particle system the prefab put under Anchor_Smoke is played and stopped; with none, a KilnSmoke is made once at the anchor (height 0,
    // so it sits exactly on it) and switched. Returns false when the structure has no prefab instance, so PlacedStructure.Rebuild keeps its own smoke.
    public static bool DriveSmoke(Transform structure, WoodPileState state, ref KilnSmoke smoke)
    {
        Spec spec = SpecFor(state.kind);
        if (spec == null || spec.smokeOn == null)
            return false;
        Transform art = structure.Find(spec.id);
        if (art == null)
            return false;

        Transform anchor = AnchorOf(art, spec, AnchorSmoke);
        bool on = spec.smokeOn(state);

        bool ownSystem = false;
        foreach (ParticleSystem system in anchor.GetComponentsInChildren<ParticleSystem>(true))
        {
            if (system.GetComponentInParent<KilnSmoke>() != null)
                continue; // ours, not the prefab's
            ownSystem = true;
            if (on && !system.isPlaying)
                system.Play(true);
            else if (!on && system.isPlaying)
                system.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        if (ownSystem)
            return true;

        if (smoke == null)
        {
            smoke = KilnSmoke.Create(anchor);
            smoke.height = 0f;
            smoke.Apply();
        }
        smoke.SetFiring(on);
        return true;
    }
}
