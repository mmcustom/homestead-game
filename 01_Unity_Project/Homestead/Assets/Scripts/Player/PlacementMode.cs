using UnityEngine;
using UnityEngine.InputSystem;

// Building_Housing_System.md's Build Placement Preview. Build on the Craft and Build screen closes the screen and starts
// this: a green or red ghost follows where the player is aiming (a ray from the camera to the ground, no further than
// reach), and nothing is spent until the player confirms.
//   Left click        build it there (materials are spent only now)
//   Esc, right click  cancel; nothing is spent
//   Mouse wheel       turn the ghost in rotationStepDegrees steps, for structures where facing matters
// The game keeps running. While placing, PlayerController.CanUseTools is false (so a click never swings the Axe or fires a
// tool), E and R interact with nothing, and Esc cancels instead of pausing. It cancels itself the moment the game leaves
// Playing (another screen, the pause menu, a load or a slot switch), the player sleeps or collapses, or the World goes away.
//
// The rules are WoodManager.CanBuildPileAt and FireManager.CanBuildAt — the same ones the build itself asks — evaluated at the
// ghost, so the red reason and the real refusal can never differ. The Rain Barrel ghost snaps to the nearest eave spot.
// Creates itself at startup, so nothing has to be added to a scene.
public class PlacementMode : MonoBehaviour
{
    [Header("Placement Mode")]
    [Tooltip("The furthest the ghost can be from the player (metres). Beyond it the ghost turns red: Too far.")]
    public float reach = 6f;
    [Tooltip("Degrees one notch of the mouse wheel turns a structure where facing matters.")]
    public float rotationStepDegrees = 15f;
    [Tooltip("A Rain Barrel ghost snaps to an eave spot this close (metres) to where you aim.")]
    public float eaveSnapRange = 3f;
    [Tooltip("A Kitchen add-on ghost snaps to a slot this close (metres) to where you aim.")]
    public float slotSnapRange = 3f;
    [Tooltip("How far the aim ray looks for ground, so a spot beyond reach can say Too far instead of nothing.")]
    public float aimRange = 40f;

    static PlacementMode instance;

    // True while placing, and for one frame after it ends, so the click or Esc that ended it can't also do its usual job.
    public static bool Active => instance != null && (instance.placing || Time.frameCount <= instance.endedFrame + 1);

    bool placing;
    int beganFrame = -10, endedFrame = -10;
    bool campfire;
    bool tripod;                // the Tripod's ghost snaps to the nearest fire (FireManager.SetUpTripod)
    CampfireState tripodFire;   // the fire the ghost is on right now
    PileKind kind;
    float yaw;
    PlayerController player;
    PlacementGhost ghost;
    // The Well Site's preview for the spot under the ghost (WellsAndOuthouses.PreviewWell): extra HUD lines, and whether the ghost is amber.
    string previewInfo;
    bool previewWarn;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        instance = null;
        Application.quitting -= OnQuitting; // once only, however many times the statics are reset
        Application.quitting += OnQuitting;
    }

    // Play Mode is ending (or the game is quitting) while a placement is on: take the ghost down at once. The ghost is a HideFlags.DontSave
    // object, so in the editor it would otherwise outlive Play Mode with its materials destroyed and draw solid magenta; a plain Destroy
    // does not run while Play Mode tears down.
    static void OnQuitting()
    {
        if (instance == null)
            return;
        instance.placing = false;
        instance.ghost?.DestroyNow();
        instance.ghost = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create() => Ensure();

    static PlacementMode Ensure()
    {
        if (instance != null)
            return instance;
        var go = new GameObject("Placement Mode");
        go.hideFlags = HideFlags.DontSave;
        DontDestroyOnLoad(go);
        instance = go.AddComponent<PlacementMode>();
        return instance;
    }

    public static void Begin(PileKind pileKind) => Ensure().StartPlacing(pileKind, false);

    public static void BeginCampfire() => Ensure().StartPlacing(PileKind.Felled, true);

    public static void BeginTripod() => Ensure().StartPlacing(PileKind.Felled, false, true);
    public static void Cancel()
    {
        if (instance != null && instance.placing)
            instance.End(null);
    }

    // Structures with a front, a door or a long side: the mouse wheel turns these. Round things and piles ignore it.
    static bool FacingMatters(PileKind pileKind)
    {
        switch (pileKind)
        {
            case PileKind.LeanTo:
            case PileKind.Tent:
            case PileKind.TarpShelter:
            case PileKind.ToolRack:
            case PileKind.StorageBin:
            case PileKind.FoodCacheSite:
            case PileKind.WoodShedSite:
            case PileKind.KitchenSite:
            case PileKind.CabinSite:
            case PileKind.OuthouseSite:
                return true;
            default:
                return false;
        }
    }

    string Name => tripod ? "Tripod" : campfire ? "Campfire" : WoodManager.PileName(kind);
    bool Rotatable => !campfire && FacingMatters(kind);

    void StartPlacing(PileKind pileKind, bool isCampfire, bool isTripod = false)
    {
        if (placing)
            End(null);
        player = FindAnyObjectByType<PlayerController>();
        if (player == null)
            return;

        campfire = isCampfire;
        tripod = isTripod;
        tripodFire = null;
        kind = pileKind;
        yaw = player.transform.eulerAngles.y;
        if (!isCampfire && pileKind == PileKind.WoodShedSite)
            yaw = WellsAndOuthouses.DownwindYaw(); // the open front starts facing away from the wind; the wheel turns it
        ghost = tripod ? PlacementGhost.ForTripod() : campfire ? PlacementGhost.ForCampfire() : PlacementGhost.ForPile(kind);
        ghost.SetVisible(false);
        placing = true;
        beganFrame = Time.frameCount;
    }

    void OnDestroy()
    {
        ghost?.Destroy();
        if (instance == this)
            instance = null;
    }

    // After the player's own Update (which turns the camera), so the ghost follows this frame's aim.
    void LateUpdate()
    {
        if (!placing)
            return;

        if (!StillPlacing())
        {
            End(null);
            return;
        }

        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        bool settled = Time.frameCount > beganFrame + 1; // the click that pressed Build doesn't also confirm or cancel
        if (settled && ((keyboard != null && keyboard.escapeKey.wasPressedThisFrame) || (mouse != null && mouse.rightButton.wasPressedThisFrame)))
        {
            End("Placement cancelled — nothing was spent.");
            return;
        }

        if (Rotatable)
        {
            float scroll = mouse != null ? mouse.scroll.ReadValue().y : 0f;
            if (scroll != 0f)
                yaw = Mathf.Repeat(yaw + Mathf.Sign(scroll) * rotationStepDegrees, 360f);
        }
        else
        {
            yaw = player.transform.eulerAngles.y; // as before: it faces the way the player does
        }

        Evaluate(out BuildSpot spot, out bool haveSpot, out bool valid, out string reason, out string note);

        if (haveSpot)
        {
            ghost.SetPose(spot.point, yaw);
            ghost.SetValid(valid, previewWarn);
        }
        ghost.SetVisible(haveSpot);

        string keys = tripod ? "Left click: set it up    Esc / Right click: cancel"
                    : Rotatable ? "Left click: build    Mouse wheel: turn    Esc / Right click: cancel"
                                : "Left click: build    Esc / Right click: cancel";
        string head = valid ? $"Place {Name} here" + (note != null ? $" — {note}" : "") : reason;
        if (valid && !string.IsNullOrEmpty(previewInfo))
            head += "\n" + previewInfo;
        ToolStatus.Report(head + "\n" + keys, -1f, true, !string.IsNullOrEmpty(previewInfo));

        if (settled && haveSpot && mouse != null && mouse.leftButton.wasPressedThisFrame)
            Confirm(spot, valid, reason);
    }

    // The game is still in a state where placing makes sense; anything else (a screen, the pause menu, a load, a sleep) ends it.
    bool StillPlacing()
    {
        GameManager game = GameManager.Instance;
        if (game != null && game.State != GameState.Playing)
            return false;
        if (player == null || WoodManager.Instance == null || !WoodManager.Instance.InWorld)
            return false;
        if ((campfire || tripod) && FireManager.Instance == null)
            return false;
        SurvivalManager survival = SurvivalManager.Instance;
        if (survival != null && survival.IsCollapsed)
            return false;
        SleepManager sleep = SleepManager.Instance;
        return sleep == null || !sleep.IsSleeping;
    }

    void Confirm(BuildSpot spot, bool valid, string reason)
    {
        if (!valid)
        {
            ToolStatus.Flash(reason, 2.5f); // stays in Placement Mode: aim somewhere else
            return;
        }

        string builtMessage;
        bool built;
        if (tripod)
        {
            built = FireManager.Instance.SetUpTripod(tripodFire);
            builtMessage = "Tripod set up — aim at it and press E to hang the Cooking Pot.";
        }
        else if (campfire)
        {
            built = FireManager.Instance.Build(player, spot, yaw);
            builtMessage = "Campfire built — light it with Flint and Steel, or a Bow Drill.";
        }
        else
        {
            built = WoodManager.Instance.BuildPile(kind, player, spot, yaw);
            builtMessage = $"{Name} built — {CraftingScreen.PileHelp(kind)}";
        }

        if (built)
            End(builtMessage);
        else
            ToolStatus.Flash("Can't build there.", 2.5f); // the rules said yes a moment ago and now say no (the pack changed, say)
    }

    void End(string message)
    {
        placing = false;
        endedFrame = Time.frameCount;
        ghost?.Destroy();
        ghost = null;
        if (!string.IsNullOrEmpty(message))
            ToolStatus.Flash(message, 3.5f);
    }

    // --- Where the ghost goes and whether it may ---

    void Evaluate(out BuildSpot spot, out bool haveSpot, out bool valid, out string reason, out string note)
    {
        spot = default;
        haveSpot = false;
        valid = false;
        reason = "";
        note = null;
        previewInfo = null;
        previewWarn = false;

        bool hit = player.AimRaycast(aimRange, out RaycastHit ray);
        BuildSpot aim = default;
        if (hit)
        {
            aim = new BuildSpot
            {
                point = ray.point,
                normal = ray.normal,
                // A campfire takes any solid surface (its old rule), everything else only the terrain.
                onTerrain = campfire || ray.collider is TerrainCollider,
                inWaterSource = ray.collider.GetComponentInParent<WaterSource>() != null,
            };
        }

        // The Tripod's ghost jumps to the nearest fire within snap range of where the player aims (or stands, with no ground under the aim).
        if (tripod)
        {
            SnapToFire(hit ? aim.point : player.transform.position, out spot, out haveSpot, out valid, out reason);
            return;
        }

        // A Countertop, Shelving, Grill or Slop Sink goes only in a free slot of a finished Covered Outdoor Kitchen.
        if (!campfire && WoodManager.IsKitchenAddon(kind))
        {
            SnapToSlot(hit ? aim.point : player.transform.position, out spot, out haveSpot, out valid, out reason, out note);
            return;
        }

        if (!campfire && WoodManager.IsRainBarrel(kind) && TrySnapToEave(hit ? aim.point : player.transform.position, out spot, out valid, out reason, out note))
        {
            haveSpot = true;
            return;
        }

        if (!hit)
        {
            reason = "Aim at the ground nearby.";
            return;
        }

        spot = aim;
        haveSpot = true;
        if (TooFar(spot.point))
        {
            reason = TooFarText;
            return;
        }
        if (!campfire && WoodManager.IsRainBarrel(kind))
        {
            reason = WoodManager.NotAtAnEaveReason; // nothing snapped: no cabin eave within reach of the aim
            return;
        }

        valid = campfire ? FireManager.Instance.CanBuildAt(player, spot, true, out reason)
                         : WoodManager.Instance.CanBuildPileAt(kind, player, spot, yaw, true, out reason);
        if (!campfire && kind == PileKind.WoodShedSite)
            note = WellsAndOuthouses.ShedOpeningNote(yaw);

        // A Well Site also says how deep the water is, what is near, and whether floods can reach it (amber when they can).
        if (!campfire && kind == PileKind.WellSite && WellsAndOuthouses.Instance != null)
        {
            WellPreviewResult preview = WellsAndOuthouses.Instance.PreviewWell(spot.point, valid, reason);
            if (valid)
            {
                previewInfo = preview.text;
                previewWarn = preview.level == PreviewLevel.Warning;
            }
        }
    }

    string TooFarText => $"Too far — come within {reach:0} m.";

    bool TooFar(Vector3 point) => Vector3.Distance(player.transform.position, point) > reach;

    // The Tripod's ghost sits on the nearest fire within TripodRules.SnapMetres of where the player aims, facing the way the fire does; it is
    // red for a fire that has a tripod already or for the Grill's fire (the reason says which).
    void SnapToFire(Vector3 around, out BuildSpot spot, out bool haveSpot, out bool valid, out string reason)
    {
        spot = default;
        haveSpot = false;
        valid = false;
        reason = "Aim at a fire within 3 m of you.";
        tripodFire = null;

        FireManager fires = FireManager.Instance;
        CampfireState best = null;
        float bestDistance = TripodRules.SnapMetres;
        foreach (CampfireState fire in fires.Fires)
        {
            float distance = Vector3.Distance(fire.position, around);
            if (distance <= bestDistance)
            {
                best = fire;
                bestDistance = distance;
            }
        }
        if (best == null)
            return;

        tripodFire = best;
        spot = new BuildSpot { point = best.position, normal = Vector3.up, onTerrain = true };
        haveSpot = true;
        yaw = best.yaw;
        if (TooFar(best.position))
        {
            reason = TooFarText;
            return;
        }
        valid = fires.CanSetUpTripod(best, out reason);
    }

    // The Rain Barrel (and Rain Barrel Site) ghost jumps to the nearest eave spot along a finished cabin's long walls, preferring one that is allowed.
    bool TrySnapToEave(Vector3 around, out BuildSpot spot, out bool valid, out string reason, out string note)
    {
        spot = default;
        valid = false;
        reason = "";
        note = null;

        bool found = false;
        float bestDistance = float.MaxValue;
        foreach ((Vector3 candidate, int side, WoodPileState cabin) in WoodManager.Instance.EaveSpots(around, eaveSnapRange))
        {
            if (!GroundAt(candidate, out BuildSpot ground))
                continue;

            bool ok;
            string why;
            if (TooFar(ground.point))
            {
                ok = false;
                why = TooFarText;
            }
            else
            {
                ok = WoodManager.Instance.CanBuildPileAt(kind, player, ground, yaw, true, out why);
            }

            float distance = Vector3.Distance(candidate, around);
            bool better = !found || (ok && !valid) || (ok == valid && distance < bestDistance);
            if (!better)
                continue;

            found = true;
            bestDistance = distance;
            spot = ground;
            valid = ok;
            reason = why;
            note = $"{RainCatchment.SlopeName(side)} of the cabin ({Shelter.RoofSlopePlanArea:0.#} m² of roof)";
        }
        return found;
    }

    // The add-on ghost jumps to the nearest free slot of the nearest finished kitchen within snap range (a taken one only if none is free, so the
    // refusal can say so). The slot sets the ghost's facing too: toward the middle of the kitchen.
    void SnapToSlot(Vector3 around, out BuildSpot spot, out bool haveSpot, out bool valid, out string reason, out string note)
    {
        spot = default;
        haveSpot = false;
        valid = false;
        note = null;
        reason = WoodManager.Instance.AnyFinishedKitchen ? "Aim at a slot inside the Covered Outdoor Kitchen." : WoodManager.NeedsKitchenReason;

        bool found = false, bestFree = false;
        float bestDistance = float.MaxValue, bestYaw = yaw;
        foreach ((Vector3 world, float slotYaw, WoodPileState kitchen, int index, bool taken) in WoodManager.Instance.AddonSlots(around, slotSnapRange))
        {
            if (!GroundAt(world, out BuildSpot ground))
                continue;

            bool free = !taken;
            float distance = Vector3.Distance(world, around);
            if (found && !(free && !bestFree) && !(free == bestFree && distance < bestDistance))
                continue;

            found = true;
            bestFree = free;
            bestDistance = distance;
            bestYaw = slotYaw;
            spot = ground;
            note = Setbacks.KitchenLayout.SlotLabel(index);
            if (TooFar(ground.point))
            {
                valid = false;
                reason = TooFarText;
            }
            else
            {
                valid = WoodManager.Instance.CanBuildPileAt(kind, player, ground, slotYaw, true, out reason);
            }
        }
        if (found)
            yaw = bestYaw;
        haveSpot = found;
    }

    // The terrain under a point, ignoring cabins, piles and trees above it.
    static bool GroundAt(Vector3 point, out BuildSpot ground)
    {
        ground = default;
        RaycastHit[] hits = Physics.RaycastAll(point + Vector3.up * 50f, Vector3.down, 120f, ~0, QueryTriggerInteraction.Ignore);
        float best = float.MaxValue;
        bool found = false;
        foreach (RaycastHit h in hits)
        {
            if (!(h.collider is TerrainCollider) || h.distance >= best)
                continue;
            best = h.distance;
            found = true;
            ground = new BuildSpot { point = h.point, normal = h.normal, onTerrain = true };
        }
        return found;
    }
}
