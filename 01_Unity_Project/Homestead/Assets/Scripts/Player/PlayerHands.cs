using System.Collections.Generic;
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
    Transform art, handR, handL, anchorR, anchorL;   // art = the hands tree being shown: artIdle (thumb up, empty hands) or artGrip (fingers wrapped, a tool in hand)
    Transform artIdle, artGrip;
    GameObject held;
    string heldShown;
    Transform heldRoot, gripL;   // the tool in hand (the prefab or the primitive, or the generic placeholder), and where the left hand steadies it
    bool heldTwoHanded;
    Transform heldGripR;    // where the hand that carries the tool holds it: Anchor_Grip_R, or on a bow Anchor_Grip_L (the left hand's handle)
    Transform nockAnchor;   // a bow's Anchor_Nock, where the right hand goes
    Transform[] stringHalves;
    bool bowStance, hanging;
    float shownFor;         // seconds the tool in hand has been shown (the settle of a hanging item)
    float heldPitch, heldRoll;
    readonly SwingState swing = new SwingState();
    readonly PunchState punch = new PunchState(); // the two fists, one state each (Hands M0e-1); the tool swing above is for the right hand's tools only
    public PunchState Punch => punch;
    readonly KnifeState knife = new KnifeState(); // the Knife's slash and stab, right hand only (Hands M0e-2)
    public KnifeState Knife => knife;
    readonly ToolActionState action = new ToolActionState(); // the rods and lights in hand (M0c-1)
    Transform lineAnchor, flameAnchor, stateLit, stateUnlit, flameShape;
    Vector3 flameShapeScale;
    bool heldIsPrimitive, lightMoved;
    bool pushing;
    public bool ForcePushing { get; set; } // a diagnostic: show the push pose without a Hand Cart
    public HandMotion? ForceMotion { get; set; } // a diagnostic: hold the hands at one motion and one step phase (ForceStepPhase) so a screenshot can look at mid-stride
    public float ForceStepPhase { get; set; }
    public bool Pushing { get => pushing; set => pushing = value; } // the Hand Cart is being pushed: both hands take the push pose
    ParticleSystem[] flameParticles;
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
    public ToolActionState Action => action;
    public Transform FlameAnchor => flameAnchor;

    // Where a rod in hand starts its fishing line: its Anchor_Line while the hands are showing it, else null (FishingRod then draws the line from where it always did).
    public Transform LineAnchor => lineAnchor != null && rig != null && rig.activeInHierarchy && ToolActionRules.IsRod(heldShown) ? lineAnchor : null;

    // Whether the Torch or Lantern with this item id is burning. PortableLight answers it; a test replaces it.
    public System.Func<string, bool> LitQuery = id => PortableLight.Instance != null && PortableLight.Instance.IsLit(id);

    void Awake()
    {
        Instance = this;
        player = GetComponent<PlayerController>();
        body = GetComponent<CharacterController>();
        weapon = GetComponent<HuntingWeapon>();
        ToolEvents.Swung += OnSwung;
        ToolEvents.Tired += OnTired;
        ToolEvents.Punched += OnPunched;
        ToolEvents.Knifed += OnKnifed;
        ToolEvents.Cast += OnCast; ToolEvents.Bite += OnBite; ToolEvents.Landed += OnLanded; ToolEvents.Failed += OnFailed;
        ToolEvents.ReelStart += OnReelStart; ToolEvents.ReelStop += OnReelStop; ToolEvents.LineIn += OnLineIn;
        ToolEvents.Lit += OnLit; ToolEvents.Snuffed += OnSnuffed;
        ToolEvents.Draw += OnDraw; ToolEvents.Shot += OnShot; ToolEvents.Working += OnWorking;
        ToolEvents.Filled += OnFilled; ToolEvents.Drank += OnDrank; ToolEvents.Tapped += OnTapped; ToolEvents.Placed += OnPlaced;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
        if (inventory != null)
            inventory.EquippedToolChanged -= OnEquippedChanged;
        ToolEvents.Swung -= OnSwung;
        ToolEvents.Tired -= OnTired;
        ToolEvents.Punched -= OnPunched;
        ToolEvents.Knifed -= OnKnifed;
        ToolEvents.Cast -= OnCast; ToolEvents.Bite -= OnBite; ToolEvents.Landed -= OnLanded; ToolEvents.Failed -= OnFailed;
        ToolEvents.ReelStart -= OnReelStart; ToolEvents.ReelStop -= OnReelStop; ToolEvents.LineIn -= OnLineIn;
        ToolEvents.Lit -= OnLit; ToolEvents.Snuffed -= OnSnuffed;
        ToolEvents.Draw -= OnDraw; ToolEvents.Shot -= OnShot; ToolEvents.Working -= OnWorking;
        ToolEvents.Filled -= OnFilled; ToolEvents.Drank -= OnDrank; ToolEvents.Tapped -= OnTapped; ToolEvents.Placed -= OnPlaced;
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

    // The rods and lights (ToolEvents, M0c-1). Each is ignored when the hands are holding something else.
    public void OnCast(string toolId) { if (toolId == raise.Shown) { action.SetFishing(true); action.Start(ToolAction.Cast); } }
    public void OnBite(string toolId) { if (toolId == raise.Shown) action.Start(ToolAction.Bite); }
    public void OnLanded(string toolId) { if (toolId == raise.Shown) action.Start(ToolAction.Catch); }
    public void OnFailed(string toolId) { if (toolId == raise.Shown) action.Start(ToolAction.Miss); }
    public void OnReelStart(string toolId) { if (toolId == raise.Shown) action.SetReeling(true); }
    public void OnReelStop(string toolId) { if (toolId == raise.Shown) action.SetReeling(false); }
    public void OnLineIn(string toolId) { if (toolId == raise.Shown) action.SetFishing(false); }
    public void OnLit(string toolId) { if (toolId == raise.Shown) action.Start(ToolAction.Light); }
    public void OnSnuffed(string toolId) { if (toolId == raise.Shown) action.Start(ToolAction.Snuff); }

    // M0c-2 (ToolEvents): the bows and rifle, the Bow Drill, a vessel at the water, the Spile and the traps.
    public void OnDraw(string toolId, float fraction) { if (toolId == raise.Shown) action.SetDraw(fraction); }
    public void OnShot(string toolId)
    {
        if (toolId != raise.Shown)
            return;
        if (ToolActionRules.IsBow(toolId))
        {
            action.SetDraw(0f);
            action.Start(ToolAction.Release);
        }
        else
            action.Start(ToolAction.Shot);
    }
    public void OnWorking(string toolId, bool on) { if (toolId == raise.Shown) action.SetWorking(on); }
    public void OnFilled() { if (ToolActionRules.IsContainer(raise.Shown)) action.Start(ToolAction.Dip); }
    public void OnDrank() { if (ToolActionRules.IsContainer(raise.Shown)) action.Start(ToolAction.Drink); }
    public void OnTapped() { if (ToolActionRules.IsSpile(raise.Shown)) action.Start(ToolAction.Tap); }
    public void OnPlaced() { if (ToolActionRules.IsTrap(raise.Shown)) action.Start(ToolAction.PlaceDown); }

    // A fist (ToolEvents, M0e-1): the hand throws a punch, or droops when too tired. Only with empty hands showing; with a tool in hand the tool owns the hands.
    public void OnPunched(bool rightHand, bool tired)
    {
        if (raise.Shown != null)
            return;
        if (tired)
            punch.StartTired(rightHand);
        else
            punch.Start(rightHand);
    }

    // A Knife blow (ToolEvents, M0e-2): the right hand slashes or stabs, or droops when too tired. Ignored unless the Knife is what the hands are showing.
    public void OnKnifed(bool stab, bool tired)
    {
        if (!KnifeRules.IsKnife(raise.Shown))
            return;
        if (tired)
            knife.StartTired();
        else
            knife.Start(stab);
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
        art = artIdle = prefab != null ? prefab : BuildPrimitiveHands(rig.transform);
        // Hands v3: a second pose with the fingers wrapped round a handle, shown whenever a tool is in hand. Only next to a real hands_player prefab; the primitives have one pose.
        artGrip = prefab != null ? ArtSwap.TryAttachHandsGrip(rig.transform, layer) : null;
        if (artGrip != null)
            artGrip.gameObject.SetActive(false);
        BindHands();
        Apply(null, 1f, 0f, 0f, HandMotion.Idle);
    }

    void BindHands()
    {
        handR = ArtSwap.HandRoot(art, HandSide.Right);
        handL = ArtSwap.HandRoot(art, HandSide.Left);
        anchorR = ArtSwap.HandAnchor(art, HandSide.Right);
        anchorL = ArtSwap.HandAnchor(art, HandSide.Left);
    }

    // Shows the gripping hands (a tool is in hand) or the thumb-up hands (empty hands). A no-op without a hands_player_grip prefab. Runs from ShowHeld before the tool is attached, so the tool
    // is always parented under the anchor of the pose being shown.
    void SelectPose(bool gripping)
    {
        if (artGrip == null || artIdle == null || (art == artGrip) == gripping)
            return;
        art = gripping ? artGrip : artIdle;
        artIdle.gameObject.SetActive(!gripping);
        artGrip.gameObject.SetActive(gripping);
        BindHands();
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
        lineAnchor = flameAnchor = stateLit = stateUnlit = flameShape = null;
        nockAnchor = null;
        stringHalves = null;
        bowStance = ToolActionRules.IsBow(toolId);
        hanging = ToolActionRules.IsHanging(toolId);
        shownFor = 0f;
        flameParticles = null;
        heldIsPrimitive = false;
        heldShown = toolId;
        action.Configure(toolId);
        // The tool in hand decides the ready pose: the Axes, Pick Axes and Hammer are held cocked (drawn back), everything else neutral. This also drops any swing in progress.
        swing.Configure(ToolWork.CockKind(toolId, out WorkKind cockKind), cockKind);
        punch.Stop(); // a fist never carries over into a tool
        knife.Configure(KnifeRules.IsKnife(toolId)); // the Knife is held cocked; anything else is not
        SelectPose(toolId != null);
        if (toolId == null || anchorR == null || anchorL == null)
            return;

        // THE swap point for tools: the prefab if there is one, else the primitive shape, for the eight tools M0b draws.
        ArtSwap.ToolSpec spec = ArtSwap.ToolSpecFor(toolId);
        if (spec != null)
        {
            // A bow is the left hand's: it hangs off the left palm and stays put; every other tool is the right hand's.
            Transform parent = bowStance ? anchorL : anchorR;
            Transform tool = ArtSwap.TryAttachTool(parent, toolId, layer);
            heldIsPrimitive = tool == null;
            if (tool == null)
                tool = ToolViewPrimitives.Build(toolId, parent, layer).transform;
            Transform gripR = ArtSwap.ToolAnchor(tool, spec, ArtSwap.AnchorGripR);
            gripL = ArtSwap.ToolAnchor(tool, spec, ArtSwap.AnchorGripL);
            ArtSwap.ToolAnchor(tool, spec, ArtSwap.AnchorTip);
            if (spec.Anchor(ArtSwap.AnchorLine) != null)
                lineAnchor = ArtSwap.ToolAnchor(tool, spec, ArtSwap.AnchorLine);
            if (spec.Anchor(ArtSwap.AnchorFlame) != null)
            {
                flameAnchor = ArtSwap.ToolAnchor(tool, spec, ArtSwap.AnchorFlame);
                List<Transform> lit = ArtSwap.Named(tool, ArtSwap.StateLit), unlit = ArtSwap.Named(tool, ArtSwap.StateUnlit);
                stateLit = lit.Count > 0 ? lit[0] : null;
                stateUnlit = unlit.Count > 0 ? unlit[0] : null;
                flameParticles = flameAnchor.GetComponentsInChildren<ParticleSystem>(true);
                if (heldIsPrimitive && stateLit != null)
                {
                    flameShape = stateLit.Find("Flame");
                    flameShapeScale = flameShape != null ? flameShape.localScale : Vector3.one;
                }
            }
            if (bowStance)
            {
                nockAnchor = ArtSwap.ToolAnchor(tool, spec, ArtSwap.AnchorNock);
                if (heldIsPrimitive)
                    stringHalves = new[] { tool.Find(ToolViewPrimitives.StringUpper), tool.Find(ToolViewPrimitives.StringLower) };
            }
            heldGripR = bowStance ? gripL : gripR; heldPitch = spec.holdPitch; heldRoll = spec.holdRoll;
            AlignToGrip(tool, heldGripR, heldPitch, heldRoll);
            held = tool.gameObject;
            heldRoot = tool;
            heldTwoHanded = spec.twoHanded && !bowStance; // a bow's left hand holds the bow itself, it does not follow a second grip
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

    // A hanging item (a bucket, pot, trap, canteen, pouch): its grip sits on the palm and the body hangs straight down in the view whatever the hand does (the rig's own orientation, not the hand's), then
    // swings by pitch (forward and back) and roll (side to side) degrees about the grip.
    void AlignHanging(Transform tool, Transform grip, Transform palm, float pitch, float roll, float yaw = 0f)
    {
        Quaternion gripRotation = Quaternion.Inverse(tool.rotation) * grip.rotation;
        tool.rotation = rig.transform.rotation * Quaternion.Euler(pitch, yaw, roll) * Quaternion.Inverse(gripRotation);
        tool.position += palm.position - grip.position;
    }

    // The primitive bow's string: two halves from the limb tips to wherever the right hand is holding it, so the string draws back with the hand. A prefab's string is the prefab's own.
    void UpdateString()
    {
        if (stringHalves == null || stringHalves[0] == null || stringHalves[1] == null || heldRoot == null)
            return;
        Vector3 limbTip = ToolViewPrimitives.BowTip(heldShown == "recurve_bow");
        Vector3 hand = anchorR.position;
        for (int i = 0; i < 2; i++)
        {
            Vector3 tip = heldRoot.TransformPoint(new Vector3(0f, i == 0 ? limbTip.y : -limbTip.y, limbTip.z));
            Transform segment = stringHalves[i];
            Vector3 along = hand - tip;
            float length = along.magnitude;
            if (length < 1e-5f)
                continue;
            segment.position = (tip + hand) * 0.5f;
            segment.rotation = Quaternion.FromToRotation(Vector3.up, along / length);
            float scale = segment.parent.lossyScale.y;
            segment.localScale = new Vector3(segment.localScale.x, length * 0.5f / scale, segment.localScale.z);
        }
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
        // No tool can be used while pushing the Hand Cart (both hands on its handles) or knocked down by the creek: the hands drop what they hold (the cart pose is a fixed two-handed push).
        pushing = ForcePushing || CartHandler.Pushing;
        string effective = HandPoseRules.EffectiveTool(wanted, pushing, CreekCrossing.InputLocked);
        raise.Tick(dt, effective);
        action.SetAim(scope);
        if (!HoldSwing)
        {
            swing.Tick(dt);
            action.Tick(dt);
            punch.Tick(dt);
            knife.Tick(dt);
        }
        breathTime += dt;
        shownFor += dt;

        bool moving = body != null && new Vector2(body.velocity.x, body.velocity.z).sqrMagnitude > 0.09f && body.isGrounded;
        HandMotion motion = HandPoseRules.MotionFor(moving, player != null && player.IsSprinting, player != null && player.IsCrouching);
        stepPhase += HandPoseRules.StepRate(motion) * dt;
        if (ForceMotion.HasValue)
        {
            motion = ForceMotion.Value;
            stepPhase = ForceStepPhase;
        }
        Apply(raise.Shown, raise.Raise * HandPoseRules.ScopeLowering(scope), breathTime, stepPhase, motion);
        UpdateLight(shown);
    }

    // Where PortableLight makes its point light, relative to the camera: low and right of the view. The light goes back here whenever the flame in hand is not carrying it.
    public static readonly Vector3 HeldLightHome = new Vector3(0.45f, -0.3f, 0.35f);

    // The light sits this far ahead of the flame along the view, in metres: right at the flame it would blow the hand holding it out to white.
    public const float FlameLightAhead = 0.18f;

    // The held light goes to the flame while a lit light is in hand and showing, and back home otherwise. Intensity, range and colour are PortableLight's and are not touched.
    public static void PlaceHeldLight(Transform heldLight, Transform flame, bool onFlame)
    {
        if (heldLight == null)
            return;
        if (onFlame && flame != null)
            heldLight.position = flame.position + (heldLight.parent != null ? heldLight.parent.forward * FlameLightAhead : Vector3.zero);
        else
            heldLight.localPosition = HeldLightHome;
    }

    // The lit or unlit state of a Torch or Lantern in hand: the matching State_ child, the flame's flicker, a prefab's particles, and the point light's place.
    public void UpdateLight(bool handsShown)
    {
        bool isLight = ToolActionRules.IsLight(heldShown) && flameAnchor != null;
        bool lit = isLight && LitQuery != null && LitQuery(heldShown);
        if (stateLit != null)
            stateLit.gameObject.SetActive(lit);
        if (stateUnlit != null)
            stateUnlit.gameObject.SetActive(isLight && !lit);
        if (flameParticles != null)
        {
            foreach (ParticleSystem particles in flameParticles)
            {
                if (lit && !particles.isPlaying)
                    particles.Play();
                else if (!lit && particles.isPlaying)
                    particles.Stop();
            }
        }
        if (flameShape != null && lit)
        {
            // A cheap flicker: the primitive flame breathes in height and width, two sines out of step.
            float t = Time.time;
            float height = 1f + 0.18f * Mathf.Sin(t * 17f) + 0.10f * Mathf.Sin(t * 29f + 1f);
            float width = 1f + 0.08f * Mathf.Sin(t * 23f + 2f);
            flameShape.localScale = new Vector3(flameShapeScale.x * width, flameShapeScale.y * height, flameShapeScale.z * width);
        }

        Transform heldLight = player != null && player.CameraTransform != null ? player.CameraTransform.Find("Held Light") : null;
        bool onFlame = handsShown && lit;
        if (heldLight != null && (onFlame || lightMoved))
            PlaceHeldLight(heldLight, flameAnchor, onFlame);
        lightMoved = onFlame;
    }

    // Puts the hands where the rules say, for the tool shown (null = empty hands) and how far up it is. Public so a test can pose them without a player.
    public void Apply(string shownTool, float raiseAmount, float breath, float steps, HandMotion motion)
    {
        if (!IsBuilt)
            return;
        ShowHeld(shownTool);

        bool rightHolds = shownTool != null;
        bool leftShown = HeldTools.LeftHandShown(shownTool);
        bool push = pushing && shownTool == null; // both hands on the cart handles
        if (bowStance && shownTool != null && heldRoot != null)
        {
            ApplyBow(raiseAmount, breath, steps, motion);
            return;
        }
        HandPose right = push ? HandPoseRules.PushPose(HandSide.Right, raiseAmount, breath, steps, motion) : HandPoseRules.Pose(HandSide.Right, rightHolds, raiseAmount, breath, steps, motion);
        if (rightHolds)
        {
            // The ready pose (cocked or neutral), a blow in progress and, for a rod or a light, its own ready pose and the cast, bite, reel or flicker: the hand turns about the elbow by only a share of
            // the pitch (so the sleeve stays out of the view); the tool takes the rest about its grip.
            HandPose offset = swing.Offset;
            if (ToolActionRules.Handles(shownTool))
                offset = SwingRules.Sum(offset, action.Offset(LitQuery != null && LitQuery(shownTool)));
            else if (KnifeRules.IsKnife(shownTool))
                offset = knife.Offset; // the cocked ready pose, and the slash or stab in progress
            float toolPitch = offset.Pitch * (1f - SwingRules.HandPitchShare);
            offset.Pitch *= SwingRules.HandPitchShare;
            PlaceHandSwinging(handR, right, offset);
            if (heldRoot != null && heldGripR != null)
            {
                if (hanging)
                {
                    // Gravity: the body hangs straight down; the step swings it a little and a fresh equip settles it. The action's own pitch (the dip, the drink, the set-down) still tips it.
                    HandPose sway = ToolActionRules.HangSway(steps, HandPoseRules.StepAmount(motion));
                    float settle = ToolActionRules.HangSettle(shownFor - HandPoseRules.RaiseSeconds);
                    AlignHanging(heldRoot, heldGripR, anchorR, toolPitch + sway.Pitch + settle, sway.Roll);
                }
                else if (ToolActionRules.IsRifle(shownTool))
                    AlignHanging(heldRoot, heldGripR, anchorR, ToolActionRules.RiflePitch + toolPitch, ToolActionRules.RifleRoll, ToolActionRules.RifleYaw); // barrel along the view, not the hand
                else
                    AlignToGrip(heldRoot, heldGripR, heldPitch + toolPitch, heldRoll);
            }
        }
        else if (push)
            PlaceHand(handR, right);
        else
            PlaceHandSwinging(handR, right, punch.Offset(true)); // empty hands: the right fist (nothing at rest)
        HandPose leftPose = push ? HandPoseRules.PushPose(HandSide.Left, raiseAmount, breath, steps, motion) : HandPoseRules.Pose(HandSide.Left, HeldTools.LeftHandHolds(shownTool), raiseAmount, breath, steps, motion);
        if (shownTool == null && !push)
            PlaceHandSwinging(handL, leftPose, punch.Offset(false)); // and the left fist
        else
            PlaceHand(handL, leftPose);
        handR.gameObject.SetActive(true);
        handL.gameObject.SetActive(leftShown);
        if (leftShown && heldTwoHanded && gripL != null && heldRoot != null)
            PlaceLeftOnGrip(); // on a two-handed tool the left hand follows the tool's left grip
    }

    // A bow in hand (M0c-2 fix): the LEFT hand holds the bow and it does not move with the draw or the release. The RIGHT hand goes to the nock on the string, pulls back toward the face as the draw
    // builds, trembles faintly at full draw, and springs forward on the release. Hands, ready pose, equip, sway and hiding are the usual ones.
    void ApplyBow(float raiseAmount, float breath, float steps, HandMotion motion)
    {
        HandPose left = ToolActionRules.Sum(HandPoseRules.Pose(HandSide.Left, true, raiseAmount, breath, steps, motion), action.BowHandOffset());
        PlaceHand(handL, left);
        AlignHanging(heldRoot, heldGripR, anchorL, ToolActionRules.BowPitch, ToolActionRules.BowRoll, ToolActionRules.BowYaw); // upright in the view whatever angle the left forearm comes in at

        HandPose right = HandPoseRules.Pose(HandSide.Right, true, raiseAmount, breath, steps, motion);
        HandPose pull = action.Offset(false);
        right.Pitch += pull.Pitch; right.Yaw += pull.Yaw; right.Roll += pull.Roll;
        PlaceHand(handR, right);
        if (nockAnchor != null)
            handR.position += nockAnchor.position - anchorR.position; // the palm on the string's middle
        handR.localPosition += new Vector3(pull.X * WidthScale, pull.Y, pull.Z);
        handR.gameObject.SetActive(true);
        handL.gameObject.SetActive(true);
        UpdateString();
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
