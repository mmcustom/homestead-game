using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

// Hands and tools M0a (Mike, 2026-11-04; Hands_and_Held_Tools.md): the player's first-person hands. Added to the Player at run time (PlayerController.Awake, as the Cart handler is), so no
// prefab or scene is edited. Two hands, drawn on the Viewmodel layer by ViewmodelPass: relaxed and low at the screen edges when empty-handed, rising with a placeholder shape in hand
// when a tool is equipped (EquippedToolChanged), going back down when it is put away. They breathe, sway as the player steps, and sit a little lower in a sprint or a crouch. They show
// only while the world is live (GameState.Playing), so every menu, the pause screen and sleep hide them. Where each hand sits is HandPoseRules; this class only moves objects.
//
// THE SWAP POINT for the hands is BuildRig: it asks ArtSwap.TryAttachHands for Resources/Art/hands_player.prefab and draws the primitive forearms and mittens only when there is none.
// The primitive tree has the same names a prefab uses (hands_player > Hand_R, Hand_L, Anchor_Hand_R, Anchor_Hand_L).
//
// THE SWAP POINT for the held tools (M0b) is ShowHeld: it asks ArtSwap.TryAttachTool for Resources/Art/tool_<itemid>.prefab, draws the ToolViewPrimitives shape when there is none, and keeps the
// generic category-colour placeholder for a tool outside the eight (M0c). The right hand holds the tool at Anchor_Grip_R; on a two-handed tool the left hand goes to Anchor_Grip_L. A blow from
// AxeTool (ToolEvents) plays a swing pose (SwingRules) on the right hand and the tool with it, and the left hand follows the tool.
public class PlayerHands : MonoBehaviour
{
    public static PlayerHands Instance { get; private set; }

    [Header("Viewmodel camera")]
    [Tooltip("The hands' own field of view in degrees, unaffected by the rifle scope or any other change to the main camera.")]
    [SerializeField, Range(30f, 90f)] float viewFieldOfView = 60f;
    [SerializeField, Min(0.001f)] float viewNearClip = 0.02f;
    [SerializeField, Min(0.1f)] float viewFarClip = 3f;

    [Tooltip("The whole hands rig is scaled by this about the camera. The picture is the same (everything shrinks toward the eye together), but the hands then sit inside the player's body radius " +
             "(0.35 m), so a wall can never stand between them and the sun and shade them while the player's own side is lit.")]
    [SerializeField, Range(0.3f, 1f)] float viewScale = 0.5f;

    [Header("Look")]
    [SerializeField] Color skin = new Color(0.74f, 0.54f, 0.42f);
    [SerializeField] Color sleeve = new Color(0.20f, 0.30f, 0.46f);

    // How far out the hands sit sideways, as a multiple of the 16:9 pose: on a wider window they stay at the screen edges instead of drifting toward the middle.
    public float WidthScale { get; set; } = 1f;
    public const float MaxWidthScale = 1.5f;

    // Hands show only while the world is live (Playing). Menu is the Inventory, Journal, Finances and the sleep fade; Paused is the pause menu; Lost, Loading and the main menu have no world.
    public static bool VisibleIn(GameState state) => state == GameState.Playing;

    // The tool id the hands should hold for an equipped item, or null for empty hands.
    public static string WantedToolId(ItemDefinition equipped) => equipped != null ? equipped.Id : null;

    PlayerController player;
    CharacterController body;
    HuntingWeapon weapon; // for the rifle's scope fraction (ScopeFraction, read-only)
    Camera worldCamera;
    Camera viewCamera;
    CustomPassVolume passVolume;
    ViewmodelPass pass;
    GameObject rig;
    Transform art, handR, handL, anchorR, anchorL;
    GameObject held;
    string heldShown;
    Transform heldRoot, gripL;   // the tool in hand (the prefab or the primitive, or the generic placeholder), and where the left hand steadies it
    bool heldTwoHanded;
    Transform heldGripR;
    float heldPitch, heldRoll;
    readonly SwingState swing = new SwingState();
    InventoryManager inventory;
    int layer = -1;
    bool failed;

    readonly HandRaise raise = new HandRaise();
    string wanted;
    float breathTime, stepPhase;

    // For tests and the editor: the hands as built.
    public bool IsBuilt => rig != null;
    public GameObject Rig => rig;
    public Transform ArtRoot => art;
    public Transform RightHand => handR;
    public Transform LeftHand => handL;
    public Transform RightAnchor => anchorR;
    public Transform LeftAnchor => anchorL;
    public Camera ViewCamera => viewCamera;
    public ViewmodelPass Pass => pass;
    public string HeldShown => heldShown;
    public HandRaise Raise => raise;
    public int Layer => layer;
    public SwingState Swing => swing;

    // Holds the swing at whatever moment it is at (a diagnostic: with SwingState.Seek it lets a screenshot look at one pose).
    public bool HoldSwing { get; set; }
    public Transform HeldRoot => heldRoot;
    public Transform LeftGrip => gripL;

    void Awake()
    {
        Instance = this;
        player = GetComponent<PlayerController>();
        body = GetComponent<CharacterController>();
        weapon = GetComponent<HuntingWeapon>();
        ToolEvents.Swung += OnSwung;
        ToolEvents.Tired += OnTired;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
        if (inventory != null)
            inventory.EquippedToolChanged -= OnEquippedChanged;
        ToolEvents.Swung -= OnSwung;
        ToolEvents.Tired -= OnTired;
    }

    // A blow landed with this tool (ToolEvents): the hands swing. Ignored when the hands are holding something else (mid-switch). A finishing flag that arrives with the swing it belongs to makes
    // that swing the bigger one; otherwise a new blow replaces the one in progress, so a held click chains swings.
    public void OnSwung(string toolId, WorkKind kind, bool finishing)
    {
        if (toolId != raise.Shown)
            return;
        if (finishing && swing.Active && !swing.Tired && swing.Kind == kind && swing.Time < 0.05f)
            swing.MakeFinishing();
        else
            swing.Start(kind, finishing);
    }

    // A blow was refused for lack of stamina: the hands droop.
    public void OnTired(string toolId)
    {
        if (toolId == raise.Shown)
            swing.StartTired();
    }

    // Builds the whole thing for a camera: the layer, the hidden camera, the pass and the hands. False when the project has no Viewmodel layer (nothing is drawn, and it says so once).
    public bool Build(Camera main)
    {
        if (rig != null)
            return true;
        if (main == null)
            return false;

        layer = LayerMask.NameToLayer(ArtSwap.ViewmodelLayerName);
        if (layer < 0)
        {
            if (!failed)
                Debug.LogWarning($"[Hands] The project has no '{ArtSwap.ViewmodelLayerName}' layer (Project Settings > Tags and Layers), so the hands are not drawn.");
            failed = true;
            return false;
        }

        worldCamera = main;
        worldCamera.cullingMask = HandPoseRules.MaskWithout(worldCamera.cullingMask, layer);

        // The hidden camera sees only the Viewmodel layer. It never renders by itself: ViewmodelPass asks for its view.
        var cameraObject = new GameObject("Viewmodel Camera");
        cameraObject.transform.SetParent(worldCamera.transform, false);
        viewCamera = cameraObject.AddComponent<Camera>();
        viewCamera.enabled = false;
        viewCamera.cullingMask = 1 << layer;
        viewCamera.clearFlags = CameraClearFlags.Nothing;
        viewCamera.nearClipPlane = viewNearClip;
        viewCamera.farClipPlane = viewFarClip;
        viewCamera.fieldOfView = viewFieldOfView;
        viewCamera.aspect = worldCamera.aspect;

        var volumeObject = new GameObject("Viewmodel Pass");
        volumeObject.transform.SetParent(transform, false);
        passVolume = volumeObject.AddComponent<CustomPassVolume>();
        passVolume.isGlobal = true;
        passVolume.injectionPoint = CustomPassInjectionPoint.BeforePostProcess;
        pass = (ViewmodelPass)passVolume.AddPassOfType(typeof(ViewmodelPass));
        pass.name = "Viewmodel Hands";
        pass.viewCamera = viewCamera;
        pass.worldCamera = worldCamera;
        pass.layerMask = 1 << layer;
        pass.draw = false;

        BuildRig(worldCamera.transform);
        return true;
    }

    // THE SWAP POINT: a hands_player prefab if there is one, the primitive hands if not.
    void BuildRig(Transform cameraTransform)
    {
        rig = new GameObject("Hands");
        rig.layer = layer;
        rig.transform.SetParent(cameraTransform, false);
        rig.transform.localScale = Vector3.one * viewScale; // a prefab is authored at normal size; this brings it in to the eye (see viewScale)

        Transform prefab = ArtSwap.TryAttachHands(rig.transform, layer);
        art = prefab != null ? prefab : BuildPrimitiveHands(rig.transform);

        handR = ArtSwap.HandRoot(art, HandSide.Right);
        handL = ArtSwap.HandRoot(art, HandSide.Left);
        anchorR = ArtSwap.HandAnchor(art, HandSide.Right);
        anchorL = ArtSwap.HandAnchor(art, HandSide.Left);
        Apply(null, 1f, 0f, 0f, HandMotion.Idle);
    }

    // --- The primitive hands (the default) ---

    Transform BuildPrimitiveHands(Transform parent)
    {
        var root = new GameObject(ArtSwap.HandsId);
        root.layer = layer;
        root.transform.SetParent(parent, false);
        BuildPrimitiveHand(root.transform, HandSide.Right);
        BuildPrimitiveHand(root.transform, HandSide.Left);
        ArtSwap.SetViewLayer(root, layer);
        return root.transform;
    }

    // One hand, wrist at its origin, fingers along +Z: a forearm running back toward the camera, a sleeve cuff at its far end, a mitten with a thumb, and the palm anchor.
    void BuildPrimitiveHand(Transform parent, HandSide side)
    {
        float mirror = side == HandSide.Right ? 1f : -1f;
        var hand = new GameObject(side == HandSide.Right ? ArtSwap.HandRName : ArtSwap.HandLName).transform;
        hand.SetParent(parent, false);

        // Along the arm from the wrist (z = 0) back toward the camera: a short bare forearm, then a lighter cuff band thicker than the arm, then the sleeve running on out of the picture.
        // The sleeve starts close to the wrist so its cuff is on screen where the arm comes in from the bottom corner (a hand at rest shows about 0.2 m of arm).
        Part(hand, PrimitiveType.Capsule, "Forearm", new Vector3(0f, 0f, -0.05f), new Vector3(0.068f, 0.075f, 0.068f), new Vector3(90f, 0f, 0f), skin);
        Part(hand, PrimitiveType.Cylinder, "Sleeve Cuff", new Vector3(0f, 0f, -0.115f), new Vector3(0.108f, 0.022f, 0.108f), new Vector3(90f, 0f, 0f), Color.Lerp(sleeve, Color.white, 0.40f));
        Part(hand, PrimitiveType.Cylinder, "Sleeve", new Vector3(0f, 0f, -0.385f), new Vector3(0.096f, 0.25f, 0.096f), new Vector3(90f, 0f, 0f), sleeve);

        // The mitten: a flat palm block, a rounded finger block across its end (one lump, no separate fingers), and a thumb standing out from the inner side.
        Part(hand, PrimitiveType.Cube, "Mitten", new Vector3(0f, 0f, 0.045f), new Vector3(0.092f, 0.036f, 0.090f), Vector3.zero, skin);
        Part(hand, PrimitiveType.Capsule, "Fingers", new Vector3(0f, 0.002f, 0.108f), new Vector3(0.040f, 0.046f, 0.040f), new Vector3(0f, 0f, 90f), skin);
        Part(hand, PrimitiveType.Capsule, "Thumb", new Vector3(-0.052f * mirror, 0.004f, 0.050f), new Vector3(0.030f, 0.032f, 0.030f), new Vector3(90f, -32f * mirror, 0f), skin);

        var anchor = new GameObject(side == HandSide.Right ? ArtSwap.AnchorHandR : ArtSwap.AnchorHandL).transform;
        anchor.SetParent(hand, false);
        anchor.localPosition = ArtSwap.PalmFallback;
    }

    GameObject Part(Transform parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Vector3 euler, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        Collider collider = go.GetComponent<Collider>();
        if (collider != null)
            DestroyImmediate(collider);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localRotation = Quaternion.Euler(euler);
        go.transform.localScale = scale;
        Tint(go, color);
        return go;
    }

    static void Tint(GameObject go, Color color)
    {
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        block.SetColor("_Color", color);
        go.GetComponent<Renderer>().SetPropertyBlock(block);
    }

    // --- The placeholder in hand (M0b replaces it with the real tool through ArtSwap's tool table) ---

    void ShowHeld(string toolId)
    {
        if (toolId == heldShown && (held != null) == (toolId != null))
            return;
        if (held != null)
        {
            if (Application.isPlaying)
            {
                held.SetActive(false); // gone this frame, freed at its end
                Destroy(held);
            }
            else
                DestroyImmediate(held); // edit-mode tests
        }
        held = null;
        heldRoot = null;
        gripL = null;
        heldTwoHanded = false;
        heldGripR = null;
        heldShown = toolId;
        // The tool in hand decides the ready pose: the Axes, Pick Axes and Hammer are held cocked (drawn back), everything else neutral. This also drops any swing in progress.
        swing.Configure(ToolWork.CockKind(toolId, out WorkKind cockKind), cockKind);
        if (toolId == null || anchorR == null)
            return;

        // THE swap point for tools: the prefab if there is one, else the primitive shape, for the eight tools M0b draws.
        ArtSwap.ToolSpec spec = ArtSwap.ToolSpecFor(toolId);
        if (spec != null)
        {
            Transform tool = ArtSwap.TryAttachTool(anchorR, toolId, layer);
            if (tool == null)
                tool = ToolViewPrimitives.Build(toolId, anchorR, layer).transform;
            Transform gripR = ArtSwap.ToolAnchor(tool, spec, ArtSwap.AnchorGripR);
            gripL = ArtSwap.ToolAnchor(tool, spec, ArtSwap.AnchorGripL);
            ArtSwap.ToolAnchor(tool, spec, ArtSwap.AnchorTip);
            heldGripR = gripR; heldPitch = spec.holdPitch; heldRoll = spec.holdRoll;
            AlignToGrip(tool, gripR, heldPitch, heldRoll);
            held = tool.gameObject;
            heldRoot = tool;
            heldTwoHanded = spec.twoHanded;
            return;
        }

        // Every other tool keeps the generic placeholder in its category colour until M0c.
        ItemDefinition item = ItemDatabase.Get(toolId);
        Color color = DroppedItems.Tint(item != null ? item.Category : ItemCategory.Tool);
        bool twoHanded = HeldTools.IsTwoHanded(toolId);

        // A long stick leaning forward for a two-handed tool, a short block for the rest.
        Vector3 size = twoHanded ? new Vector3(0.045f, 0.045f, 0.62f) : new Vector3(0.07f, 0.07f, 0.2f);
        Vector3 center = twoHanded ? new Vector3(0f, 0f, 0.22f) : new Vector3(0f, 0f, 0.07f);
        held = Part(anchorR, PrimitiveType.Cube, "Held Placeholder", center, size, new Vector3(twoHanded ? -38f : -12f, 0f, 0f), color);
        ArtSwap.SetViewLayer(held, layer);
        heldRoot = held.transform;
    }

    // Puts the tool in the right hand: its Anchor_Grip_R sits on the palm anchor, and the whole tool leans up by holdPitch from the hand's forward.
    static void AlignToGrip(Transform tool, Transform gripR, float holdPitch, float holdRoll)
    {
        Quaternion gripRotation = Quaternion.Inverse(tool.rotation) * gripR.rotation;
        Vector3 gripPosition = tool.InverseTransformPoint(gripR.position);
        Quaternion rotation = Quaternion.Euler(holdPitch, 0f, holdRoll) * Quaternion.Inverse(gripRotation);
        tool.localRotation = rotation;
        tool.localPosition = -(rotation * gripPosition);
    }

    // The left hand goes to the tool's left grip: its palm anchor on the grip. It keeps its own pose angles, so its arm still comes in from the lower left corner instead of lying along the handle.
    void PlaceLeftOnGrip()
    {
        handL.position += gripL.position - anchorL.position;
    }

    // --- Each frame ---

    void OnEquippedChanged(ItemDefinition tool) => wanted = WantedToolId(tool);

    void LateUpdate()
    {
        if (!IsBuilt)
        {
            Camera main = player != null && player.CameraTransform != null ? player.CameraTransform.GetComponent<Camera>() : null;
            if (!Build(main))
                return;
        }

        if (inventory == null && InventoryManager.Instance != null)
        {
            inventory = InventoryManager.Instance;
            inventory.EquippedToolChanged += OnEquippedChanged;
            wanted = WantedToolId(inventory.EquippedTool);
            raise.Snap(wanted);
        }

        // Another script could widen the main camera's mask; keep the hands out of it.
        if (HandPoseRules.MaskHas(worldCamera.cullingMask, layer))
            worldCamera.cullingMask = HandPoseRules.MaskWithout(worldCamera.cullingMask, layer);
        viewCamera.aspect = worldCamera.aspect;
        viewCamera.fieldOfView = viewFieldOfView;
        WidthScale = Mathf.Clamp(worldCamera.aspect / (16f / 9f), 1f, MaxWidthScale);

        GameManager game = GameManager.Instance;
        bool live = game == null || VisibleIn(game.State);

        // The rifle's scope: the hands slide down out of view as it comes up and are not drawn at all while the overlay shows. Zero for everything but the rifle.
        if (weapon == null)
            weapon = GetComponent<HuntingWeapon>();
        float scope = weapon != null ? weapon.ScopeFraction : 0f;
        bool shown = live && !HandPoseRules.HiddenByScope(scope);
        rig.SetActive(shown);
        pass.draw = shown;
        if (!live)
        {
            // Behind a loading screen or the main menu the hands follow the equipped tool at once, so a loaded game starts with it already up.
            // In a menu or the pause screen they hold still and finish their move when the world is back.
            if (game != null && !game.InGame)
                raise.Snap(wanted);
            return;
        }

        float dt = Time.deltaTime;
        raise.Tick(dt, wanted);
        if (!HoldSwing)
            swing.Tick(dt);
        breathTime += dt;

        bool moving = body != null && new Vector2(body.velocity.x, body.velocity.z).sqrMagnitude > 0.09f && body.isGrounded;
        HandMotion motion = HandPoseRules.MotionFor(moving, player != null && player.IsSprinting, player != null && player.IsCrouching);
        stepPhase += HandPoseRules.StepRate(motion) * dt;
        Apply(raise.Shown, raise.Raise * HandPoseRules.ScopeLowering(scope), breathTime, stepPhase, motion);
    }

    // Puts the hands where the rules say, for the tool shown (null = empty hands) and how far up it is. Public so a test can pose them without a player.
    public void Apply(string shownTool, float raiseAmount, float breath, float steps, HandMotion motion)
    {
        if (!IsBuilt)
            return;
        ShowHeld(shownTool);

        bool rightHolds = shownTool != null;
        bool leftShown = HeldTools.LeftHandShown(shownTool);
        HandPose right = HandPoseRules.Pose(HandSide.Right, rightHolds, raiseAmount, breath, steps, motion);
        if (rightHolds)
        {
            // The ready pose (cocked or neutral) and a blow in progress: the hand turns about the elbow by only a share of the pitch (so the sleeve stays out of the view); the tool takes the rest about its grip.
            HandPose offset = swing.Offset;
            float toolPitch = offset.Pitch * (1f - SwingRules.HandPitchShare);
            offset.Pitch *= SwingRules.HandPitchShare;
            PlaceHandSwinging(handR, right, offset);
            if (heldRoot != null && heldGripR != null)
                AlignToGrip(heldRoot, heldGripR, heldPitch + toolPitch, heldRoll);
        }
        else
            PlaceHand(handR, right);
        PlaceHand(handL, HandPoseRules.Pose(HandSide.Left, HeldTools.LeftHandHolds(shownTool), raiseAmount, breath, steps, motion));
        handR.gameObject.SetActive(true);
        handL.gameObject.SetActive(leftShown);
        if (leftShown && heldTwoHanded && gripL != null && heldRoot != null)
            PlaceLeftOnGrip(); // on a two-handed tool the left hand follows the tool's left grip
    }

    // The swing: the hand turns by the offset's angles about the elbow (SwingRules.ElbowBack behind the wrist), so the forearm stays near where it was and the hand arcs. The offset's own X, Y, Z
    // then move the hand a little more.
    void PlaceHandSwinging(Transform hand, HandPose pose, HandPose offset)
    {
        Quaternion before = Quaternion.Euler(pose.Pitch, pose.Yaw, pose.Roll);
        Quaternion after = Quaternion.Euler(pose.Pitch + offset.Pitch, pose.Yaw + offset.Yaw, pose.Roll + offset.Roll);
        Vector3 wrist = new Vector3(pose.X * WidthScale, pose.Y, pose.Z);
        Vector3 back = new Vector3(0f, 0f, -SwingRules.ElbowBack);
        Vector3 elbow = wrist + before * back;
        hand.localPosition = elbow - after * back + new Vector3(offset.X * WidthScale, offset.Y, offset.Z);
        hand.localRotation = after;
    }

    void PlaceHand(Transform hand, HandPose pose)
    {
        hand.localPosition = new Vector3(pose.X * WidthScale, pose.Y, pose.Z);
        hand.localRotation = Quaternion.Euler(pose.Pitch, pose.Yaw, pose.Roll);
    }
}
