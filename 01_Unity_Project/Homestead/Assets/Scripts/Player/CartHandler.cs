using UnityEngine;

// Vehicles_and_Hauling.md, Hand Cart Phase 2 (Mike, 2026-10-31): grabbing the parked cart, pushing it while walking and parking it again.
//
// The cart is a follower. Its centre sits FollowMetres ahead of the player along its own facing, and that facing swings round to the player's at a
// limited rate, so it trails and swings like a cart on a pivot. Only its Transform moves (the player is a CharacterController and nothing here has a
// Rigidbody), and WoodPileState.position and yaw are written every frame, so a save made mid-push stores the live pose and the cart is simply parked
// there after a load. Whether the cart is held is NOT saved: this component adds no saved field to anything.
//
// While it is held every collider on the cart is off (WoodPile.SetCollidersOn), so the player's CharacterController never collides with the cart it is
// pushing. The obstacle test is the cart's own: PlayerController asks Constrain before each Move, and Constrain tries the full step, then a slide that
// drops only the part of the step that runs into the obstacle, then each axis alone, then no step at all. A step away from an obstacle is always clear,
// and turning is never blocked (the cart just stops swinging until its way is clear), so the player can always back up or turn out of a corner. A cart
// that already overlaps something is not constrained at all, so it can never trap the player.
//
// Trees are inside the terrain collider (RuntimeTerrain swaps the tree list), so they are found by WoodManager.FindTree at points across the cart's
// footprint, not by a collider. Everything else solid is found by an OverlapBox. Terrain and the Water layer are ignored by the box.
//
// Every number is an inspector field here, defaulted from CartRules, where the pure rules (speed, turn, slope, water, words) live and are tested.
[DefaultExecutionOrder(110)]
public class CartHandler : MonoBehaviour
{
    [Header("Following")]
    [Tooltip("Metres from the player to the cart's centre (the handles end about 0.45 m in front of the player).")]
    [SerializeField, Min(0.5f)] float followMetres = CartRules.FollowMetres;
    [Tooltip("How fast the cart swings round to the way the player faces (degrees a second).")]
    [SerializeField, Min(0f)] float turnDegreesPerSecond = CartRules.TurnDegreesPerSecond;

    [Header("Limits (first-pass tuning)")]
    [SerializeField, Range(0f, 45f)] float emptySlopeDegrees = CartRules.EmptySlopeDegrees;
    [SerializeField, Range(0f, 45f)] float loadedSlopeDegrees = CartRules.LoadedSlopeDegrees;
    [Tooltip("Water deeper than this at any wheel stops the cart (metres).")]
    [SerializeField, Min(0f)] float waterLimitMetres = CartRules.WaterLimitMetres;
    [Tooltip("The cart must be in the way this long before it says so.")]
    [SerializeField, Min(0f)] float stuckMessageSeconds = CartRules.StuckMessageSeconds;

    [Header("Footprint, in the cart's own space (body and handles; the bottom 0.27 m is left clear so small things under the wheels do not stop it)")]
    [SerializeField] Vector3 footprintCentre = new Vector3(0f, 0.65f, -0.2f);
    [SerializeField] Vector3 footprintHalfExtents = new Vector3(0.62f, 0.38f, 1.1f);
    [Tooltip("A tree counts as in the way when a footprint point is within this of its trunk (metres).")]
    [SerializeField, Min(0f)] float treeReach = 0.1f;
    [Tooltip("The player in creek water deeper than this lets go of the cart (metres).")]
    [SerializeField, Min(0f)] float playerWaterLimitMetres = 0.15f;

    enum Block { None, Obstacle, Steep, Deep }

    public static CartHandler Instance { get; private set; }

    // True while a cart is held. A cart whose view was destroyed (a load rebuilds every pile) is not held.
    public static bool Pushing => Instance != null && Instance.pile != null;

    // How full the held cart is, 0 to 1 (0 when none is held). PlayerController reads it for the push speed.
    public static float LoadFraction => Instance != null && Instance.pile != null ? Instance.loadFraction : 0f;

    PlayerController player;
    WoodPile pile;
    WoodPileState state;
    float loadKg, loadFraction;
    float cartYaw, pendingYaw;
    float stuckTime;
    Block stuckReason;
    bool wasInFlood;

    readonly Collider[] overlaps = new Collider[24];
    readonly RaycastHit[] hits = new RaycastHit[24];
    readonly float[] sampleX = new float[CartRules.SampleCount];
    readonly float[] sampleZ = new float[CartRules.SampleCount];
    int waterMask = ~0;
    int solidMask = ~0;

    void Awake()
    {
        player = GetComponent<PlayerController>();
        int water = LayerMask.NameToLayer("Water");
        if (water >= 0)
        {
            waterMask = 1 << water;
            solidMask = ~waterMask;
        }
    }

    void OnEnable() => Instance = this;

    void OnDisable()
    {
        Release(null);
        if (Instance == this)
            Instance = null;
    }

    // --- Grabbing and letting go ---

    // E at the handles. The cart stays facing the way it was parked and is pulled to its follow place in front of the player; if that place is not clear
    // the grab is refused and nothing moves.
    public bool TryGrab(WoodPile target, Vector3 handlePosition)
    {
        if (Pushing || target == null || target.State == null || target.State.kind != PileKind.HandCart)
            return false;
        Vector3 flat = handlePosition - transform.position;
        flat.y = 0f;
        if (!CartRules.CanReach(flat.magnitude))
            return false;
        if (player != null && player.IsCrouching)
        {
            ToolStatus.Flash(CartRules.StandUpText);
            return false;
        }

        float yaw = target.transform.eulerAngles.y;
        pile = target; // so the cart's own colliders are not mistaken for obstacles in the test below
        if (!Clear(CartOrigin(transform.position, yaw), yaw))
        {
            pile = null;
            ToolStatus.Flash(CartRules.NoRoomToGrabText);
            return false;
        }

        state = target.State;
        loadKg = WoodPile.CartLoadKg(state);
        loadFraction = CartRules.LoadFraction(loadKg);
        cartYaw = pendingYaw = yaw;
        stuckTime = 0f;
        stuckReason = Block.None;
        wasInFlood = false;
        pile.SetCollidersOn(false);
        Place(CartOrigin(transform.position, cartYaw), cartYaw);
        return true;
    }

    // E while pushing: the cart stays exactly where it is.
    public void Park() => Release(CartRules.ParkedText);

    // Everything else that lets go (sleep, a knock-down, the creek, a teleport, a load, the menu): the same, with its own words.
    public static void LetGo()
    {
        if (Instance != null)
            Instance.Release(CartRules.LetGoText);
    }

    void Release(string message)
    {
        if (pile == null)
            return;
        WoodPile released = pile;
        pile = null;
        WoodPileState releasedState = state;
        state = null;
        if (released != null) // not destroyed by a load in the meantime
        {
            released.SetCollidersOn(true);
            Physics.SyncTransforms(); // the colliders are back at the cart's new place before the next look ray
            if (WoodManager.Instance != null && releasedState != null)
                WoodManager.Instance.NotifyChanged(releasedState);
        }
        if (!string.IsNullOrEmpty(message))
            ToolStatus.Flash(message, 2.5f);
    }

    bool ShouldLetGo()
    {
        GameManager game = GameManager.Instance;
        if (game != null && !game.InGame)
            return true;
        if (SleepManager.Instance != null && SleepManager.Instance.IsSleeping)
            return true;
        if (SurvivalManager.Instance != null && SurvivalManager.Instance.IsCollapsed)
            return true;
        if (CreekCrossing.InputLocked || PlacementMode.Active)
            return true;
        // The player wading into the creek (the cart itself never goes in: Constrain stops it first).
        Vector3 feet = transform.position;
        return CreekFlow.InChannel(new Vector2(feet.x, feet.z), 1f, out _) &&
               Animal.IsOverWater(feet, out float surface) && surface - feet.y > playerWaterLimitMetres;
    }

    // --- Each frame ---

    void Update()
    {
        if (pile == null)
        {
            if (state != null) // the view was destroyed under us (a load): nothing to restore
                state = null;
            return;
        }
        if (ShouldLetGo())
        {
            Release(CartRules.LetGoText);
            return;
        }
        ToolStatus.Report(CartRules.PushingLine(loadKg));
    }

    // Called by PlayerController just before its controller.Move: the step it wants (metres, y included) comes back with the part that would push the
    // cart into something taken out. dt is the frame's time.
    public Vector3 Constrain(Vector3 move, float dt)
    {
        pendingYaw = cartYaw;
        if (pile == null)
            return move;

        Vector3 here = transform.position;
        Vector3 flat = new Vector3(move.x, 0f, move.z);
        float nextYaw = CartRules.NextYaw(cartYaw, transform.eulerAngles.y, dt, turnDegreesPerSecond);
        Vector3 originNow = CartOrigin(here, cartYaw);

        // A cart that already overlaps something never constrains the player: nothing here can trap them.
        if (!Clear(originNow, cartYaw))
        {
            pendingYaw = nextYaw;
            stuckTime = 0f;
            return move;
        }

        float depthNow = WaterDepth(originNow, cartYaw);
        Block firstReason = Block.None;
        Vector3 chosen = Vector3.zero;
        bool found = false;

        if (TryStep(here, flat, nextYaw, depthNow, out float yawOut, out Block reason))
        {
            chosen = flat;
            pendingYaw = yawOut;
            found = true;
        }
        else
        {
            firstReason = reason;
            // Only the part of the step that runs into the obstacle is dropped: slide along it, then each axis on its own.
            Vector3 normal = BlockerNormal(here, flat);
            Vector3[] tries =
            {
                normal.sqrMagnitude > 0f ? flat - normal * Mathf.Min(0f, Vector3.Dot(flat, normal)) : flat,
                new Vector3(flat.x, 0f, 0f),
                new Vector3(0f, 0f, flat.z),
            };
            foreach (Vector3 candidate in tries)
            {
                if ((candidate - flat).sqrMagnitude < 1e-10f)
                    continue; // the full step again
                if (TryStep(here, candidate, nextYaw, depthNow, out yawOut, out _))
                {
                    chosen = candidate;
                    pendingYaw = yawOut;
                    found = true;
                    break;
                }
            }
        }

        if (!found)
        {
            // Stay put, but keep swinging if the way round is clear.
            chosen = Vector3.zero;
            pendingYaw = Allowed(here, nextYaw, Vector3.zero, depthNow, out _) ? nextYaw : cartYaw;
        }

        bool wants = flat.sqrMagnitude > (0.3f * dt) * (0.3f * dt);
        bool moving = chosen.sqrMagnitude > (0.1f * dt) * (0.1f * dt);
        stuckTime = CartRules.StuckTime(stuckTime, wants, moving, dt);
        if (stuckTime > 0f && firstReason != Block.None)
            stuckReason = firstReason;
        if (stuckTime >= stuckMessageSeconds)
        {
            ToolStatus.Flash(stuckReason == Block.Steep ? CartRules.TooSteepText : stuckReason == Block.Deep ? CartRules.TooDeepText : CartRules.StuckText, 2f);
            stuckTime = -2f; // not again for a couple of seconds
        }

        return new Vector3(chosen.x, move.y, chosen.z);
    }

    void LateUpdate()
    {
        if (pile == null)
            return;
        cartYaw = pendingYaw;
        Vector3 origin = CartOrigin(transform.position, cartYaw);
        Place(origin, cartYaw);

        // The cart is left untouched in the flood band (Phase 1's rule), but the player is told once as it goes in.
        bool inFlood = WellsAndOuthouses.Instance != null && WellsAndOuthouses.Instance.InFloodBand(origin);
        if (inFlood && !wasInFlood)
            ToolStatus.Flash(CartRules.FloodText, 4f);
        wasInFlood = inFlood;
    }

    // --- The cart's place and pose ---

    Vector3 CartOrigin(Vector3 playerPos, float yaw)
    {
        CartRules.CartCentreAhead(playerPos.x, playerPos.z, yaw, out float x, out float z, followMetres);
        return new Vector3(x, Ground(x, z, playerPos.y), z);
    }

    float Ground(float x, float z, float fallback)
    {
        Terrain terrain = Terrain.activeTerrain;
        return terrain != null ? terrain.SampleHeight(new Vector3(x, 0f, z)) + terrain.transform.position.y : fallback;
    }

    float Ground(Vector3 p) => Ground(p.x, p.z, p.y);

    // Sets the view's transform, tilted along the ground, and the saved pose (WoodPileState.position and yaw) with it.
    void Place(Vector3 origin, float yaw)
    {
        Vector3 forward = new Vector3(Mathf.Sin(yaw * Mathf.Deg2Rad), 0f, Mathf.Cos(yaw * Mathf.Deg2Rad));
        float front = Ground(origin + forward * 0.9f), rear = Ground(origin - forward * 0.9f);
        float pitch = Mathf.Atan2(front - rear, 1.8f) * Mathf.Rad2Deg;
        origin.y = (front + rear) * 0.5f;
        pile.transform.SetPositionAndRotation(origin, Quaternion.Euler(-pitch, yaw, 0f));
        if (state != null)
        {
            state.position = origin;
            state.yaw = yaw;
        }
    }

    // --- Is the way clear? ---

    bool TryStep(Vector3 here, Vector3 step, float nextYaw, float depthNow, out float yawOut, out Block reason)
    {
        if (Allowed(here + step, nextYaw, step, depthNow, out reason))
        {
            yawOut = nextYaw;
            return true;
        }
        Block keep = reason;
        // Swinging round would hit something: keep the cart's facing and just take the step.
        if (!Mathf.Approximately(nextYaw, cartYaw) && Allowed(here + step, cartYaw, step, depthNow, out _))
        {
            yawOut = cartYaw;
            reason = Block.None;
            return true;
        }
        yawOut = cartYaw;
        reason = keep;
        return false;
    }

    // The player standing at playerPos with the cart facing yaw: nothing solid in the footprint, no deeper water under a wheel than it is allowed (or
    // than the cart already has), and no steeper uphill ahead than the load allows.
    bool Allowed(Vector3 playerPos, float yaw, Vector3 travel, float depthNow, out Block reason)
    {
        reason = Block.None;
        Vector3 origin = CartOrigin(playerPos, yaw);
        if (!Clear(origin, yaw))
        {
            reason = Block.Obstacle;
            return false;
        }
        if (CartRules.WaterStepBlocked(depthNow, WaterDepth(origin, yaw), waterLimitMetres))
        {
            reason = Block.Deep;
            return false;
        }
        if (travel.sqrMagnitude > 1e-8f)
        {
            Vector3 d = travel.normalized;
            float rise = Ground(origin + d * 0.9f) - Ground(origin - d * 0.9f);
            if (CartRules.SlopeBlocksPush(rise, 1.8f, loadFraction, emptySlopeDegrees, loadedSlopeDegrees))
            {
                reason = Block.Steep;
                return false;
            }
        }
        return true;
    }

    bool Clear(Vector3 origin, float yaw)
    {
        Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
        Vector3 centre = origin + rotation * footprintCentre;
        int count = Physics.OverlapBoxNonAlloc(centre, footprintHalfExtents, overlaps, rotation, solidMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            if (!Ignored(overlaps[i]))
                return false;
        }
        return TreeAt(origin, yaw) < 0;
    }

    // Terrain (and the trees inside it), the player, the cart itself and the water's look-at colliders are not obstacles for the box.
    bool Ignored(Collider c) =>
        c == null || c is TerrainCollider || c.transform.IsChildOf(transform) || (pile != null && c.transform.IsChildOf(pile.transform));

    // The index of a standing tree under the footprint, or -1.
    int TreeAt(Vector3 origin, float yaw)
    {
        WoodManager wood = WoodManager.Instance;
        if (wood == null || !wood.InWorld)
            return -1;
        int n = CartRules.FootprintSamples(origin.x, origin.z, yaw, sampleX, sampleZ);
        for (int i = 0; i < n; i++)
        {
            int tree = wood.FindTree(new Vector3(sampleX[i], Ground(sampleX[i], sampleZ[i], origin.y), sampleZ[i]), treeReach);
            if (tree >= 0)
                return tree;
        }
        return -1;
    }

    // The deepest water under the footprint, in metres (0 on dry ground).
    float WaterDepth(Vector3 origin, float yaw)
    {
        int n = CartRules.FootprintSamples(origin.x, origin.z, yaw, sampleX, sampleZ);
        float deepest = 0f;
        for (int i = 0; i < n; i++)
        {
            float ground = Ground(sampleX[i], sampleZ[i], origin.y);
            if (Animal.IsOverWater(new Vector3(sampleX[i], ground, sampleZ[i]), out float surface))
                deepest = Mathf.Max(deepest, surface - ground);
        }
        return deepest;
    }

    // Which way the thing in the cart's path pushes back (horizontal, unit, pointing from the thing toward the cart), or zero when it is not known:
    // the nearest solid collider along the step, or the trunk of a tree under the cart at the end of it.
    Vector3 BlockerNormal(Vector3 here, Vector3 flat)
    {
        if (flat.sqrMagnitude < 1e-10f)
            return Vector3.zero;
        Vector3 origin = CartOrigin(here, cartYaw);
        Quaternion rotation = Quaternion.Euler(0f, cartYaw, 0f);
        Vector3 centre = origin + rotation * footprintCentre;
        int count = Physics.BoxCastNonAlloc(centre, footprintHalfExtents, flat.normalized, hits, rotation, flat.magnitude + 0.05f, solidMask, QueryTriggerInteraction.Ignore);
        float best = float.MaxValue;
        Vector3 normal = Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            if (Ignored(hits[i].collider) || hits[i].distance >= best)
                continue;
            Vector3 n = hits[i].normal;
            n.y = 0f;
            if (n.sqrMagnitude < 1e-6f)
                continue;
            best = hits[i].distance;
            normal = n.normalized;
        }
        if (normal.sqrMagnitude > 0f)
            return normal;

        WoodManager wood = WoodManager.Instance;
        int tree = TreeAt(CartOrigin(here + flat, cartYaw), cartYaw);
        if (wood == null || tree < 0)
            return Vector3.zero;
        Vector3 away = CartOrigin(here, cartYaw) - wood.WorldPosition(tree);
        away.y = 0f;
        return away.sqrMagnitude > 1e-6f ? away.normalized : Vector3.zero;
    }
}
