using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MovementState { Idle, Walking, Sprinting, Crouching, Jumping }

[Serializable]
public struct PlayerSaveData
{
    public Vector3 position;
    public float yaw;
    public float pitch;
    public float stamina;
    public bool crouched;
}

// First_Person_Controller.md: walk, sprint (stamina + fatigue cost, less stealthy), crouch (slower, less visible),
// jump (a small grounded hop), context-sensitive interaction, encumbrance slowdown, first-person camera only, and
// discovery by proximity (DiscoverySite triggers) and line of sight. Swimming isn't in Alpha 0.1 scope.
//
// Jump (confirmed 2026-09-25): a functional hop for clearing logs, rocks, low fences and creek edges — never a
// platforming jump. About half a metre, a short snappy arc under the controller's strong gravity, and the take-off
// momentum is kept with only slight steering in the air. Costs stamina like a burst of sprinting; not while crouched,
// lower when heavily loaded, and impossible at the carry limit.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour, ISaveable
{
    [Header("References")]
    [Tooltip("The first-person camera, a child of this object.")]
    [SerializeField] Transform cameraTransform;
    [Tooltip("Optional. Defaults to the project-wide input actions.")]
    [SerializeField] InputActionAsset inputActions;

    // All values below are tuning defaults — the design docs describe the states, not the numbers.
    [Header("Movement (m/s)")]
    [SerializeField, Min(0f)] float walkSpeed = 2.5f;
    [SerializeField, Min(0f)] float sprintSpeed = 5f;
    [SerializeField, Min(0f)] float crouchSpeed = 1.3f;
    [Tooltip("How quickly speed changes, in m/s². Lower feels heavier.")]
    [SerializeField, Min(0.1f)] float acceleration = 12f;
    [SerializeField, Min(0f)] float gravity = 20f;
    [Tooltip("Speed multiplier at full encumbrance (InventoryManager.Encumbrance = 1). Sprinting is also impossible then.")]
    [SerializeField, Range(0.1f, 1f)] float fullyEncumberedSpeedMultiplier = 0.5f;

    [Header("Crouch")]
    [SerializeField, Min(0.5f)] float standingHeight = 1.8f;
    [SerializeField, Min(0.5f)] float crouchingHeight = 1.1f;
    [SerializeField, Min(0.1f)] float standingEyeHeight = 1.65f;
    [SerializeField, Min(0.1f)] float crouchingEyeHeight = 1f;
    [SerializeField, Min(0.1f)] float crouchTransitionSpeed = 6f;

    [Header("Stamina")]
    [SerializeField, Min(1f)] float maxStamina = 100f;
    [SerializeField, Min(0f)] float sprintStaminaPerSecond = 15f;
    [SerializeField, Min(0f)] float staminaRecoveryPerSecond = 25f;
    [Tooltip("Seconds after sprinting before stamina starts recovering.")]
    [SerializeField, Min(0f)] float staminaRecoveryDelay = 1f;
    [Tooltip("Stamina needed to start a sprint, so an empty bar can't be tapped back into a sprint.")]
    [SerializeField, Min(0f)] float minStaminaToSprint = 15f;

    [Header("Jump")]
    [Tooltip("Height of the hop in metres — enough for logs, rocks and creek banks.")]
    [SerializeField, Min(0f)] float jumpHeight = 0.5f;
    [Tooltip("Share of normal steering while in the air; the take-off momentum does most of the work.")]
    [SerializeField, Range(0f, 1f)] float airControl = 0.2f;
    [SerializeField, Min(0f)] float jumpStaminaCost = 10f;
    [Tooltip("Jump height at the encumbered limit, as a share of normal (no jump at all at the carry limit).")]
    [SerializeField, Range(0f, 1f)] float encumberedJumpScale = 0.6f;
    [Tooltip("Grace for a jump pressed just after stepping off an edge or just before landing (seconds).")]
    [SerializeField, Min(0f)] float jumpGrace = 0.1f;

    [Header("Look")]
    [Tooltip("Degrees per pixel of mouse movement.")]
    [SerializeField, Min(0f)] float mouseSensitivity = 0.1f;
    [Tooltip("Degrees per second at full gamepad stick.")]
    [SerializeField, Min(0f)] float stickSensitivity = 120f;
    [SerializeField, Range(0f, 90f)] float maxPitch = 85f;

    [Header("Stealth (wildlife detection multipliers)")]
    [SerializeField, Min(0f)] float idleDetection = 0.7f;
    [SerializeField, Min(0f)] float walkDetection = 1f;
    [SerializeField, Min(0f)] float sprintDetection = 2f;
    [SerializeField, Min(0f)] float crouchDetection = 0.4f;
    [Tooltip("A jump and its landing are louder than walking.")]
    [SerializeField, Min(0f)] float jumpDetection = 1.5f;

    [Header("Interaction and Discovery")]
    [SerializeField, Min(0f)] float interactRange = 2.5f;
    [SerializeField] LayerMask interactionMask = ~0;
    [Tooltip("How far away a DiscoverySite can be spotted by looking at it (First_Person_Controller.md: line of sight).")]
    [SerializeField, Min(0f)] float sightRange = 40f;
    [SerializeField, Min(0.02f)] float sightCheckInterval = 0.25f;

    readonly RaycastHit[] sightHits = new RaycastHit[16];

    CharacterController controller;
    InputAction moveAction, lookAction, sprintAction, crouchAction, jumpAction, interactAction, interactAltAction, pauseAction;
    Vector3 horizontalVelocity;
    float verticalVelocity;
    float yaw, pitch;
    float stamina;
    float lastSprintTime = float.NegativeInfinity;
    float sprintExertion;
    float nextSightCheck;
    bool crouched;
    bool jumping;
    float lastGroundedTime = float.NegativeInfinity;
    float jumpPressedTime = float.NegativeInfinity;
    bool subscribed;
    IInteractable focus;

    public event Action<IInteractable> FocusChanged;

    public MovementState State { get; private set; }
    public float Stamina => stamina;
    public float MaxStamina => maxStamina;

    // Core_Survival_System.md: low Hydration and Hunger reduce stamina — both the ceiling and how fast it refills.
    static float SurvivalStamina => SurvivalManager.Instance != null ? SurvivalManager.Instance.StaminaMultiplier : 1f;
    public float EffectiveMaxStamina => maxStamina * SurvivalStamina;
    public bool IsCrouching => crouched;
    public bool IsSprinting => State == MovementState.Sprinting;
    public bool IsGrounded => controller != null && controller.isGrounded;
    public float HorizontalSpeed => horizontalVelocity.magnitude;
    public Transform CameraTransform => cameraTransform;

    // Scales mouse and stick look — lowered while a scope is zoomed in, so aiming stays steady.
    public float LookScale { get; set; } = 1f;
    // Equipped tools (rod, bow, rifle, traps) only work during gameplay, not in menus or while paused.
    public bool CanUseTools => CanAct;

    // For Wildlife/Hunting: how noticeable the player currently is (First_Person_Controller.md, Design Rule 2).
    public float DetectionMultiplier
    {
        get
        {
            switch (State)
            {
                case MovementState.Jumping: return jumpDetection;
                case MovementState.Crouching: return crouchDetection;
                case MovementState.Sprinting: return sprintDetection;
                case MovementState.Walking: return walkDetection;
                default: return crouched ? crouchDetection : idleDetection;
            }
        }
    }

    // What the player is looking at and can interact with, or null. Its InteractionPrompt is the on-screen prompt.
    public IInteractable Focus => focus;

    // Without a GameManager (e.g. pressing Play directly in World), the controller runs so movement can be tested.
    static bool CanAct => GameManager.Instance == null || GameManager.Instance.State == GameState.Playing;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        stamina = maxStamina;

        // Water surfaces carry a collider only so they can be looked at (WaterSource's Drink); the player wades through.
        int waterLayer = LayerMask.NameToLayer("Water");
        if (waterLayer >= 0)
            controller.excludeLayers |= 1 << waterLayer;
        yaw = transform.eulerAngles.y;
        ApplyHeight(standingHeight, standingEyeHeight);

        InputActionAsset actions = inputActions != null ? inputActions : InputSystem.actions;
        if (actions == null)
        {
            Debug.LogError("[PlayerController] No input actions assigned and no project-wide actions set.", this);
            return;
        }

        moveAction = actions.FindAction("Player/Move", true);
        lookAction = actions.FindAction("Player/Look", true);
        sprintAction = actions.FindAction("Player/Sprint", true);
        jumpAction = actions.FindAction("Player/Jump", false);
        crouchAction = actions.FindAction("Player/Crouch", true);
        interactAction = actions.FindAction("Player/Interact", true);
        interactAltAction = actions.FindAction("Player/InteractAlt", false);
        pauseAction = actions.FindAction("UI/Cancel", true);
    }

    // Registered in OnEnable, not Start: GameManager.ContinueGame loads the save as soon as the World scene
    // finishes loading, before Start has run on scene objects.
    void OnEnable()
    {
        moveAction?.actionMap.Enable();
        pauseAction?.actionMap.Enable();

        if (SaveManager.Instance != null)
            SaveManager.Instance.Register(this);

        if (GameManager.Instance != null && !subscribed)
        {
            GameManager.Instance.StateChanged += OnGameStateChanged;
            subscribed = true;
        }

        UpdateCursor();
    }

    void OnDisable()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);

        if (subscribed && GameManager.Instance != null)
            GameManager.Instance.StateChanged -= OnGameStateChanged;
        subscribed = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // Esc while typing a journal note belongs to the text field, not the pause menu.
        if (pauseAction != null && pauseAction.WasPressedThisFrame() && GameManager.Instance != null && !GameScreens.IsTyping)
            GameManager.Instance.TogglePause();

        if (!CanAct || moveAction == null)
        {
            State = MovementState.Idle;
            return;
        }

        float dt = Time.deltaTime;
        Look(dt);

        if (crouchAction.WasPressedThisFrame())
            SetCrouched(!crouched);

        Move(dt);
        UpdateHeight(dt);
        UpdateFocus();

        if (focus != null && interactAction.WasPressedThisFrame())
            focus.Interact(this);
        else if (focus is ISecondaryInteractable secondary && interactAltAction != null &&
                 interactAltAction.WasPressedThisFrame() && !string.IsNullOrEmpty(secondary.SecondaryPrompt))
            secondary.SecondaryInteract(this);

        if (Time.time >= nextSightCheck)
        {
            nextSightCheck = Time.time + sightCheckInterval;
            CheckSight();
        }
    }

    // Seconds spent sprinting since the last call. Core_Survival_System.md's Fatigue (not yet implemented)
    // should consume this to apply sprinting's fatigue cost.
    public float ConsumeSprintExertion()
    {
        float exertion = sprintExertion;
        sprintExertion = 0f;
        return exertion;
    }

    // Moves the player without the CharacterController fighting the teleport.
    public void Teleport(Vector3 position, float newYaw)
    {
        controller.enabled = false;
        transform.position = position;
        controller.enabled = true;

        yaw = newYaw;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        horizontalVelocity = Vector3.zero;
        verticalVelocity = 0f;
    }

    void Look(float dt)
    {
        Vector2 look = lookAction.ReadValue<Vector2>();
        bool fromPointer = lookAction.activeControl != null && lookAction.activeControl.device is Pointer;
        Vector2 delta = (fromPointer ? look * mouseSensitivity : look * (stickSensitivity * dt)) * LookScale;

        yaw += delta.x;
        pitch = Mathf.Clamp(pitch - delta.y, -maxPitch, maxPitch);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void Move(float dt)
    {
        Vector2 input = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
        bool moving = input.sqrMagnitude > 0.01f;

        float encumbrance = InventoryManager.Instance != null ? InventoryManager.Instance.Encumbrance : 0f;
        bool grounded = controller.isGrounded;
        if (grounded && verticalVelocity <= 0f)
            jumping = false; // landed
        // The controller's grounded flag flickers off for a frame or two on uneven terrain; a short probe underneath
        // backs it up so a jump pressed at that moment isn't lost.
        if (grounded || (!jumping && verticalVelocity <= 0f && GroundJustBelow()))
            lastGroundedTime = Time.time;

        bool wantsSprint = sprintAction.IsPressed() && moving && input.y > 0.1f && !crouched && encumbrance < 1f;
        bool sprinting = !jumping && wantsSprint && stamina > 0f &&
                         (State == MovementState.Sprinting || stamina >= minStaminaToSprint);

        UpdateStamina(sprinting, dt);
        TryJump(encumbrance);

        State = jumping ? MovementState.Jumping
              : !moving ? MovementState.Idle
              : crouched ? MovementState.Crouching
              : sprinting ? MovementState.Sprinting
              : MovementState.Walking;

        float speed = crouched ? crouchSpeed : sprinting ? sprintSpeed : walkSpeed;
        speed *= Mathf.Lerp(1f, fullyEncumberedSpeedMultiplier, encumbrance);

        // In the air the take-off momentum carries on; steering is only slight.
        Vector3 desired = (transform.right * input.x + transform.forward * input.y) * speed;
        float control = jumping || !grounded ? airControl : 1f;
        if (!jumping || moving)
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, desired, acceleration * control * dt);

        if (grounded && verticalVelocity < 0f)
            verticalVelocity = -2f; // keep a slight downward push so the controller stays grounded on slopes
        verticalVelocity -= gravity * dt;

        controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * dt);
    }

    bool GroundJustBelow()
    {
        float radius = controller.radius * 0.9f;
        Vector3 origin = transform.position + Vector3.up * (radius + 0.05f);
        int mask = ~(1 << LayerMask.NameToLayer("Water"));
        return Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, 0.25f, mask, QueryTriggerInteraction.Ignore) &&
               !hit.collider.transform.IsChildOf(transform);
    }

    // A small hop. Pressing just before landing, or just after walking off an edge, still counts (jumpGrace).
    void TryJump(float encumbrance)
    {
        if (jumpAction != null && jumpAction.WasPressedThisFrame())
            jumpPressedTime = Time.time;

        bool buffered = Time.time - jumpPressedTime <= jumpGrace;
        bool canLeaveGround = !jumping && Time.time - lastGroundedTime <= jumpGrace;
        if (!buffered || !canLeaveGround || crouched || encumbrance >= 1f || stamina < jumpStaminaCost)
            return;

        float height = jumpHeight * Mathf.Lerp(1f, encumberedJumpScale, encumbrance);
        verticalVelocity = Mathf.Sqrt(2f * gravity * height);
        jumping = true;
        jumpPressedTime = float.NegativeInfinity;

        stamina = Mathf.Max(0f, stamina - jumpStaminaCost);
        lastSprintTime = Time.time; // recovery waits after a jump as it does after sprinting
        sprintExertion += jumpStaminaCost / Mathf.Max(0.01f, sprintStaminaPerSecond); // the same effort as that much sprinting
    }

    void UpdateStamina(bool sprinting, float dt)
    {
        if (sprinting)
        {
            stamina = Mathf.Max(0f, stamina - sprintStaminaPerSecond * dt);
            sprintExertion += dt;
            lastSprintTime = Time.time;
        }
        else if (Time.time - lastSprintTime >= staminaRecoveryDelay)
        {
            stamina = Mathf.Min(EffectiveMaxStamina, stamina + staminaRecoveryPerSecond * SurvivalStamina * dt);
        }

        // Falling into a worse tier lowers the ceiling right away.
        stamina = Mathf.Min(stamina, EffectiveMaxStamina);
    }

    void SetCrouched(bool value)
    {
        if (!value && !HasHeadroom())
            return;

        crouched = value;
    }

    // Standing up needs clear space above the crouched capsule.
    bool HasHeadroom()
    {
        float radius = controller.radius * 0.95f;
        Vector3 origin = transform.position + Vector3.up * (controller.height - radius);
        float distance = standingHeight - controller.height;
        return distance <= 0f ||
               !Physics.SphereCast(origin, radius, Vector3.up, out _, distance, interactionMask, QueryTriggerInteraction.Ignore);
    }

    void UpdateHeight(float dt)
    {
        float targetHeight = crouched ? crouchingHeight : standingHeight;
        float targetEye = crouched ? crouchingEyeHeight : standingEyeHeight;
        float step = crouchTransitionSpeed * dt;

        float height = Mathf.MoveTowards(controller.height, targetHeight, step);
        float eye = cameraTransform != null
            ? Mathf.MoveTowards(cameraTransform.localPosition.y, targetEye, step)
            : targetEye;

        ApplyHeight(height, eye);
    }

    // The transform pivot is at the feet, so the capsule's center sits at half its height.
    void ApplyHeight(float height, float eyeHeight)
    {
        controller.height = height;
        controller.center = Vector3.up * (height * 0.5f);

        if (cameraTransform != null)
            cameraTransform.localPosition = new Vector3(0f, eyeHeight, 0f);
    }

    void UpdateFocus()
    {
        IInteractable found = null;
        if (cameraTransform != null)
        {
            // Looking steeply down, the ray can report the player's own capsule first, so skip it.
            int count = Physics.RaycastNonAlloc(cameraTransform.position, cameraTransform.forward, sightHits,
                                                interactRange, interactionMask, QueryTriggerInteraction.Ignore);
            Array.Sort(sightHits, 0, count, RaycastHitDistanceComparer.Instance);

            for (int i = 0; i < count; i++)
            {
                if (sightHits[i].collider.transform.IsChildOf(transform))
                    continue;

                IInteractable candidate = sightHits[i].collider.GetComponentInParent<IInteractable>();
                if (candidate != null && candidate.CanInteract(this))
                    found = candidate;
                break; // only the nearest solid thing counts
            }
        }

        if (found == focus)
            return;

        focus = found;
        FocusChanged?.Invoke(focus);
    }

    // Raycast along the camera's aim, ignoring the player's own colliders and triggers — for aimed tools (rod, bow,
    // rifle, trap placement). direction defaults to straight ahead; tools with spread pass their own.
    public bool AimRaycast(float range, out RaycastHit hit, Vector3? direction = null)
    {
        hit = default;
        if (cameraTransform == null)
            return false;

        int count = Physics.RaycastNonAlloc(cameraTransform.position, direction ?? cameraTransform.forward, sightHits, range,
                                            ~0, QueryTriggerInteraction.Ignore);
        Array.Sort(sightHits, 0, count, RaycastHitDistanceComparer.Instance);
        for (int i = 0; i < count; i++)
        {
            if (sightHits[i].collider.transform.IsChildOf(transform))
                continue;
            hit = sightHits[i];
            return true;
        }
        return false;
    }

    // Discovers a DiscoverySite the player looks at from a distance, as long as nothing solid is in the way.
    void CheckSight()
    {
        if (cameraTransform == null || DiscoveryManager.Instance == null)
            return;

        int count = Physics.RaycastNonAlloc(cameraTransform.position, cameraTransform.forward, sightHits,
                                            sightRange, ~0, QueryTriggerInteraction.Collide);
        Array.Sort(sightHits, 0, count, RaycastHitDistanceComparer.Instance);

        for (int i = 0; i < count; i++)
        {
            Collider hitCollider = sightHits[i].collider;
            if (hitCollider.transform.IsChildOf(transform))
                continue;

            DiscoverySite site = hitCollider.GetComponentInParent<DiscoverySite>();
            if (site != null && site.DiscoverBySight)
            {
                site.Discover();
                return;
            }

            if (!hitCollider.isTrigger)
                return; // line of sight blocked
        }
    }

    void OnGameStateChanged(GameState state) => UpdateCursor();

    void UpdateCursor()
    {
        bool locked = CanAct;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    public PlayerSaveData CaptureState() => new PlayerSaveData
    {
        position = transform.position,
        yaw = yaw,
        pitch = pitch,
        stamina = stamina,
        crouched = crouched,
    };

    public void RestoreState(PlayerSaveData data)
    {
        // Disabled first so the ground probe can't hit the player's own collider.
        controller.enabled = false;
        Teleport(AboveGround(data.position), data.yaw);
        pitch = Mathf.Clamp(data.pitch, -maxPitch, maxPitch);
        stamina = Mathf.Clamp(data.stamina, 0f, maxStamina);
        crouched = data.crouched;

        ApplyHeight(crouched ? crouchingHeight : standingHeight, crouched ? crouchingEyeHeight : standingEyeHeight);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    // A save made before the terrain was built or reshaped can hold a position that's now underground.
    static Vector3 AboveGround(Vector3 position)
    {
        const float ProbeHeight = 500f;
        if (Physics.Raycast(position + Vector3.up * ProbeHeight, Vector3.down, out RaycastHit hit, ProbeHeight * 2f,
                            ~0, QueryTriggerInteraction.Ignore) && hit.point.y > position.y)
            return hit.point;
        return position;
    }

    // Save_Data_Model.md's Player Block: position (SurvivalManager writes the survival stats to the same file).
    string ISaveable.SaveFile => "player";
    string ISaveable.SaveKey => "controller";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<PlayerSaveData>(json));

    sealed class RaycastHitDistanceComparer : System.Collections.Generic.IComparer<RaycastHit>
    {
        public static readonly RaycastHitDistanceComparer Instance = new RaycastHitDistanceComparer();
        public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
    }
}
