using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MovementState { Idle, Walking, Sprinting, Crouching }

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
// context-sensitive interaction, encumbrance slowdown, first-person camera only, and discovery by proximity
// (DiscoverySite triggers) and line of sight. Swimming and jumping aren't in Alpha 0.1 scope.
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

    [Header("Interaction and Discovery")]
    [SerializeField, Min(0f)] float interactRange = 2.5f;
    [SerializeField] LayerMask interactionMask = ~0;
    [Tooltip("How far away a DiscoverySite can be spotted by looking at it (First_Person_Controller.md: line of sight).")]
    [SerializeField, Min(0f)] float sightRange = 40f;
    [SerializeField, Min(0.02f)] float sightCheckInterval = 0.25f;

    readonly RaycastHit[] sightHits = new RaycastHit[16];

    CharacterController controller;
    InputAction moveAction, lookAction, sprintAction, crouchAction, interactAction, pauseAction;
    Vector3 horizontalVelocity;
    float verticalVelocity;
    float yaw, pitch;
    float stamina;
    float lastSprintTime = float.NegativeInfinity;
    float sprintExertion;
    float nextSightCheck;
    bool crouched;
    bool subscribed;
    IInteractable focus;

    public event Action<IInteractable> FocusChanged;

    public MovementState State { get; private set; }
    public float Stamina => stamina;
    public float MaxStamina => maxStamina;
    public bool IsCrouching => crouched;
    public bool IsSprinting => State == MovementState.Sprinting;

    // For Wildlife/Hunting: how noticeable the player currently is (First_Person_Controller.md, Design Rule 2).
    public float DetectionMultiplier
    {
        get
        {
            switch (State)
            {
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
        crouchAction = actions.FindAction("Player/Crouch", true);
        interactAction = actions.FindAction("Player/Interact", true);
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
        if (pauseAction != null && pauseAction.WasPressedThisFrame() && GameManager.Instance != null)
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
        Vector2 delta = fromPointer ? look * mouseSensitivity : look * (stickSensitivity * dt);

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
        bool wantsSprint = sprintAction.IsPressed() && moving && input.y > 0.1f && !crouched && encumbrance < 1f;
        bool sprinting = wantsSprint && stamina > 0f &&
                         (State == MovementState.Sprinting || stamina >= minStaminaToSprint);

        UpdateStamina(sprinting, dt);

        State = !moving ? MovementState.Idle
              : crouched ? MovementState.Crouching
              : sprinting ? MovementState.Sprinting
              : MovementState.Walking;

        float speed = crouched ? crouchSpeed : sprinting ? sprintSpeed : walkSpeed;
        speed *= Mathf.Lerp(1f, fullyEncumberedSpeedMultiplier, encumbrance);

        Vector3 desired = (transform.right * input.x + transform.forward * input.y) * speed;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, desired, acceleration * dt);

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f; // keep a slight downward push so the controller stays grounded on slopes
        verticalVelocity -= gravity * dt;

        controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * dt);
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
            stamina = Mathf.Min(maxStamina, stamina + staminaRecoveryPerSecond * dt);
        }
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
        if (cameraTransform != null &&
            Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit,
                            interactRange, interactionMask, QueryTriggerInteraction.Ignore))
        {
            IInteractable candidate = hit.collider.GetComponentInParent<IInteractable>();
            if (candidate != null && candidate.CanInteract(this))
                found = candidate;
        }

        if (found == focus)
            return;

        focus = found;
        FocusChanged?.Invoke(focus);
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
            if (site != null)
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
        Teleport(data.position, data.yaw);
        pitch = Mathf.Clamp(data.pitch, -maxPitch, maxPitch);
        stamina = Mathf.Clamp(data.stamina, 0f, maxStamina);
        crouched = data.crouched;

        ApplyHeight(crouched ? crouchingHeight : standingHeight, crouched ? crouchingEyeHeight : standingEyeHeight);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    // Save_Data_Model.md's Player Block: position (survival stats will join this file when implemented).
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
