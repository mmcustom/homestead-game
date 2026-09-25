using UnityEngine;
using UnityEngine.InputSystem;

// Active fishing (Fishing_System.md's Rod and Reel and Cane Pole). With either equipped, aim at water and click
// (Attack) to cast. Wait for a bite — the bobber dips with a plop — and click within the moment to set the hook.
// Then hold to reel it in; when the fish makes a run, ease off or the line snaps. Landing it puts the catch
// (its species' yield in units) in the pack. Click while waiting to reel in and recast. FishingManager decides
// which fish bites, from where the line is, the time of day, season and weather.
public class FishingRod : MonoBehaviour
{
    const string RodId = "fishing_rod";
    const string CaneId = "cane_pole";

    enum State { Idle, Waiting, Bite, Reeling }

    [SerializeField] PlayerController player;
    [Tooltip("The bobber, with a LineRenderer for the line (Prefabs/Bobber).")]
    [SerializeField] GameObject bobberPrefab;

    [Header("Casting (metres)")]
    [SerializeField, Min(1f)] float rodRange = 18f;
    [SerializeField, Min(1f)] float caneRange = 9f;
    [Tooltip("Walking further than this from the bobber reels the line in.")]
    [SerializeField, Min(1f)] float maxLineLength = 26f;

    [Header("Timing (real seconds)")]
    [Tooltip("How long a bite lasts before the fish lets go.")]
    [SerializeField, Min(0.2f)] float biteWindow = 1.2f;
    [SerializeField, Min(0.2f)] float caneBiteWindow = 1f;
    [Tooltip("A fish makes a run about this often while being reeled in; reeling through it builds strain.")]
    [SerializeField, Min(0.3f)] float runInterval = 1.6f;
    [SerializeField, Min(0.1f)] float runSeconds = 0.6f;
    [Tooltip("Seconds of reeling during runs before the line snaps.")]
    [SerializeField, Min(0.1f)] float snapStrain = 0.5f;

    InputAction attack;
    State state;
    GameObject bobber;
    LineRenderer line;
    Vector3 bobberRest;
    WaterSource water;
    FishWater waterKind;
    bool nearCover;
    float stateUntil;
    FishingManager.Species hooked;
    float reelProgress, strain, nextRun, runUntil;
    bool cane;
    AudioSource reelSource;

    void Awake()
    {
        if (player == null)
            player = GetComponent<PlayerController>();
        attack = InputSystem.actions != null ? InputSystem.actions.FindAction("Player/Attack") : null;
    }

    void OnDisable() => EndCast();

    void Update()
    {
        InventoryManager inventory = InventoryManager.Instance;
        string equipped = inventory != null && inventory.EquippedTool != null ? inventory.EquippedTool.Id : null;
        if (equipped != RodId && equipped != CaneId)
        {
            EndCast();
            return;
        }

        cane = equipped == CaneId;
        if (!player.CanUseTools)
            return; // paused or in a menu: everything holds

        bool pressed = attack != null && attack.WasPressedThisFrame();
        bool held = attack != null && attack.IsPressed();

        switch (state)
        {
            case State.Idle: UpdateIdle(pressed); break;
            case State.Waiting: UpdateWaiting(pressed); break;
            case State.Bite: UpdateBite(pressed); break;
            case State.Reeling: UpdateReeling(held); break;
        }

        if (line != null && bobber != null)
        {
            line.SetPosition(0, RodTip);
            line.SetPosition(1, bobber.transform.position);
        }
    }

    Vector3 RodTip
    {
        get
        {
            Transform cam = player.CameraTransform;
            return cam.position + cam.forward * 0.9f + cam.right * 0.35f + Vector3.up * 0.25f;
        }
    }

    void UpdateIdle(bool pressed)
    {
        float range = cane ? caneRange : rodRange;
        string rodName = cane ? "Cane Pole" : "Rod";

        bool hit = player.AimRaycast(range, out RaycastHit hitInfo);
        WaterSource source = hit ? hitInfo.collider.GetComponentInParent<WaterSource>() : null;
        FishWater kind = FishingManager.WaterAt(source, hitInfo.point);

        if (source == null)
        {
            ToolStatus.Report($"{rodName} — aim at water within {range:0} m and click to cast");
            return;
        }
        if (kind == FishWater.None)
        {
            ToolStatus.Report("Too shallow for fish here");
            return;
        }

        ToolStatus.Report($"Click to cast  ({(kind == FishWater.Pond ? "pond" : "creek")})");
        if (!pressed)
            return;

        water = source;
        waterKind = kind;
        nearCover = FishingManager.Instance != null && FishingManager.Instance.NearCover(hitInfo.point);
        bobberRest = hitInfo.point;
        if (bobberPrefab != null)
        {
            bobber = Instantiate(bobberPrefab, bobberRest, Quaternion.identity);
            line = bobber.GetComponentInChildren<LineRenderer>();
        }
        PlaySound(a => a.Splash, bobberRest);
        StartWaiting();
    }

    void StartWaiting()
    {
        state = State.Waiting;
        float wait = FishingManager.Instance != null ? FishingManager.Instance.BiteWait(cane) : 10f;
        stateUntil = Time.time + wait;
    }

    void UpdateWaiting(bool pressed)
    {
        Bob(0.015f, 1.5f);
        if (TooFar())
        {
            ToolStatus.Flash("Walked too far — reeled the line in");
            EndCast();
            return;
        }

        ToolStatus.Report("Waiting for a bite…  (click to reel in)", -1f);
        if (pressed)
        {
            EndCast();
            return;
        }

        if (Time.time < stateUntil)
            return;

        hooked = FishingManager.Instance != null ? FishingManager.Instance.PickSpecies(waterKind, nearCover, false) : null;
        if (hooked == null)
        {
            StartWaiting();
            return;
        }

        state = State.Bite;
        stateUntil = Time.time + (cane ? caneBiteWindow : biteWindow);
        PlaySound(a => a.FishBite, bobberRest);
    }

    void UpdateBite(bool pressed)
    {
        // The bobber ducks under while the fish has it.
        if (bobber != null)
            bobber.transform.position = bobberRest + Vector3.down * 0.08f + Vector3.up * Mathf.Sin(Time.time * 30f) * 0.02f;
        ToolStatus.Report("Bite!  Click to set the hook");

        if (pressed)
        {
            float chance = cane ? hooked.hookCane : hooked.hookRod;
            if (Random.value <= chance)
            {
                state = State.Reeling;
                reelProgress = 0f;
                strain = 0f;
                nextRun = Time.time + Random.Range(0.4f, runInterval);
                runUntil = 0f;
            }
            else
            {
                ToolStatus.Flash("Missed the hookset — it spat the hook");
                StartWaiting();
            }
            return;
        }

        if (Time.time >= stateUntil)
        {
            ToolStatus.Flash("Too slow — it let go");
            StartWaiting();
        }
    }

    void UpdateReeling(bool held)
    {
        bool running = Time.time < runUntil;
        if (!running && Time.time >= nextRun)
        {
            runUntil = Time.time + runSeconds;
            nextRun = runUntil + Random.Range(runInterval * 0.6f, runInterval * 1.4f);
            running = true;
        }

        SetReeling(held && !running);
        if (held)
        {
            if (running)
                strain += Time.deltaTime;
            else
                reelProgress += Time.deltaTime / hooked.reelSeconds;
        }
        else
        {
            strain = Mathf.Max(0f, strain - Time.deltaTime * 0.5f);
        }

        if (strain >= snapStrain)
        {
            ToolStatus.Flash("The line snapped — it got away");
            EndCast();
            return;
        }

        // Draw the bobber in toward the bank as the fish comes in, thrashing while it runs.
        if (bobber != null)
        {
            Vector3 toward = Vector3.Lerp(bobberRest, new Vector3(player.transform.position.x, bobberRest.y, player.transform.position.z), reelProgress * 0.7f);
            bobber.transform.position = toward + (running ? Random.insideUnitSphere * 0.08f : Vector3.zero);
        }

        ToolStatus.Report(running ? "It's running — ease off!" : "Hold to reel in", reelProgress, true);
        if (reelProgress >= 1f)
            Land();
    }

    void Land()
    {
        InventoryManager inventory = InventoryManager.Instance;
        ItemDefinition fish = ItemDatabase.Get(hooked.itemId);
        int added = inventory != null && fish != null ? inventory.AddToPlayer(hooked.itemId, hooked.yield) : 0;
        string name = fish != null ? fish.DisplayName : hooked.itemId;

        if (added <= 0)
            ToolStatus.Flash($"Landed a {name}, but there's no room to carry it — let it go");
        else if (added < hooked.yield)
            ToolStatus.Flash($"Caught a {name} — kept {added} of {hooked.yield}, no room for the rest");
        else
            ToolStatus.Flash($"Caught a {name}!  (+{added})");

        RecordCatch(name);
        EndCast();
    }

    // Knowledge Progression: a catch near a discovered fishing spot is noted in the journal.
    void RecordCatch(string fishName)
    {
        DiscoveryManager discovery = DiscoveryManager.Instance;
        TimeManager time = TimeManager.Instance;
        if (discovery == null || time == null)
            return;

        foreach (DiscoveryRecord record in discovery.Records)
        {
            if (record.category == DiscoveryCategory.Fishing && Vector3.Distance(record.position, bobberRest) < 40f)
                discovery.AddObservation(record.siteId, $"{fishName} caught here — {time.CurrentSeason}, {PhaseWord(time.Phase)}");
        }
    }

    static string PhaseWord(DayPhase phase) =>
        phase == DayPhase.Day ? "daytime" : phase == DayPhase.Night ? "night" : phase.ToString().ToLower();

    void Bob(float height, float speed)
    {
        if (bobber != null)
            bobber.transform.position = bobberRest + Vector3.up * Mathf.Sin(Time.time * speed) * height;
    }

    bool TooFar() =>
        Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), new Vector2(bobberRest.x, bobberRest.z)) > maxLineLength;

    void EndCast()
    {
        SetReeling(false);
        state = State.Idle;
        hooked = null;
        if (bobber != null)
            Destroy(bobber);
        bobber = null;
        line = null;
    }

    static void PlaySound(System.Func<AudioManager, Sound> sound, Vector3 at)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayAt(sound(AudioManager.Instance), at);
    }

    // sfx_fishing_reel, looped while actually winding in — silent while the fish runs or the button's released.
    void SetReeling(bool on)
    {
        if (on && reelSource == null)
        {
            Sound reel = AudioManager.Instance != null ? AudioManager.Instance.FishingReel : null;
            if (reel == null || reel.clip == null)
                return;
            reelSource = gameObject.AddComponent<AudioSource>();
            reelSource.clip = reel.clip;
            reelSource.volume = reel.volume;
            reelSource.loop = true;
            reelSource.playOnAwake = false;
            reelSource.spatialBlend = 0f; // the player's own reel
            reelSource.outputAudioMixerGroup = AudioManager.Instance.GetGroup(AudioChannel.Sfx);
        }
        if (reelSource == null)
            return;

        Sound sound = AudioManager.Instance.FishingReel;
        if (on && !reelSource.isPlaying)
        {
            if (reelSource.time < sound.startTime)
                reelSource.time = sound.startTime;
            reelSource.Play();
        }
        else if (!on && reelSource.isPlaying)
        {
            reelSource.Pause();
        }

        // Loop only the audible stretch of the recording, skipping its silent ends.
        if (reelSource.isPlaying && sound.loopEnd > sound.startTime && reelSource.time >= sound.loopEnd)
            reelSource.time = sound.startTime;
    }
}
