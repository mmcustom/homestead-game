using UnityEngine;

public enum GameTier { Small, Medium, Large }

// One wild animal (Wildlife_System.md, 05_Documentation/Wildlife). It grazes and wanders near where it spawned, and
// bolts when it notices the player: by hearing — scaled by the player's movement (First_Person_Controller.md's
// detection multipliers: crouching is quiet, sprinting loud) — and by scent, which carries downwind (Hunting_System.md:
// hunting against the wind improves approach success). Rain dampens sound. Gunshots alert everything in earshot.
// A shot animal dies (becoming a Carcass to field dress), or on a wounding hit runs off and is usually lost.
// Waterfowl sit on the pond and fly off; rabbits hop.
public class Animal : MonoBehaviour
{
    enum State { Idle, Walk, Flee, Dead, Stunned }

    [SerializeField] string speciesId = "deer";
    [SerializeField] GameTier tier = GameTier.Large;
    [SerializeField, Min(0.1f)] float walkSpeed = 1.2f;
    [SerializeField, Min(0.1f)] float runSpeed = 9f;
    [Tooltip("How far away it hears a walking player (metres); crouching, standing still and sprinting scale this.")]
    [SerializeField, Min(1f)] float hearing = 28f;
    [Tooltip("On water rather than land (Waterfowl), and flies off when startled.")]
    [SerializeField] bool swims;
    [Tooltip("Hops as it moves (rabbits).")]
    [SerializeField] bool hops;
    [Tooltip("Shown on a buck; hidden on does. Also decides whether antlers can be harvested.")]
    [SerializeField] GameObject antlers;
    [Tooltip("The model root, tipped over when the animal dies.")]
    [SerializeField] Transform model;
    [Tooltip("Heart and lungs: a shot whose line passes within the radius of this point is a vitals hit.")]
    [SerializeField] Transform vitals;
    [SerializeField, Min(0.01f)] float vitalsRadius = 0.2f;

    State state = State.Idle;
    Vector3 home, target, fleeFrom;
    float stateUntil, nextSense;
    bool wounded, willDie;
    float runLeft;
    Terrain terrain;
    int waterMask;

    public string SpeciesId => speciesId;
    public GameTier Tier => tier;
    public bool IsDead => state == State.Dead;
    public bool IsBuck => antlers != null && antlers.activeSelf;
    public bool IsMale { get; private set; }
    public bool Swims => swims;

    // Whether a shot along this line passes through the vitals (they sit inside the body, so the body collider is
    // what the shot actually hits).
    public bool IsVitalsHit(Vector3 origin, Vector3 direction)
    {
        if (vitals == null)
            return false;
        Vector3 toVitals = vitals.position - origin;
        float along = Vector3.Dot(toVitals, direction.normalized);
        if (along < 0f)
            return false;
        Vector3 closest = origin + direction.normalized * along;
        return Vector3.Distance(closest, vitals.position) <= vitalsRadius;
    }

    // Wave 5 animal art (2026-11-19), the same for every species so more can be added for other regions and climates without new code:
    // Resources/Art/animal_<speciesId> holds Pose_Alive (standing) and Pose_Dead (the carcass), a Vitals empty and optional parts such as
    // Antlers inside each pose. When it exists the primitive renderers under Model stop drawing, the art goes under Model, Vitals moves to the
    // art's, the body capsule is fitted to the standing pose, and the Antlers in the standing pose become the ones a buck shows.
    public const string ArtPrefix = "Art/animal_";
    Transform artAlive, artDead;

    void UseArt()
    {
        if (model == null)
            return;
        GameObject prefab = Resources.Load<GameObject>(ArtPrefix + speciesId);
        if (prefab == null)
            return;
        Transform alive = prefab.transform.Find("Pose_Alive");
        if (alive == null)
            return;
        foreach (Renderer r in model.GetComponentsInChildren<Renderer>(true))
            r.enabled = false;
        GameObject art = Instantiate(prefab, model, false);
        art.name = "Art";
        foreach (Collider c in art.GetComponentsInChildren<Collider>(true))
            Destroy(c);
        artAlive = art.transform.Find("Pose_Alive");
        artDead = art.transform.Find("Pose_Dead");
        if (artDead != null)
            artDead.gameObject.SetActive(false);
        Transform artVitals = art.transform.Find("Vitals");
        if (vitals != null && artVitals != null)
            vitals.position = artVitals.position;
        Transform artAntlers = PartOf(artAlive, "Antlers");
        if (artAntlers != null)
            antlers = artAntlers.gameObject;
        FitBody(artAlive);
    }

    // Shows the carcass pose (its antlers matching the standing ones) and fits the body capsule to it, so the carcass is what is aimed at
    // and dressed. False when this species has no art.
    bool ShowDeadPose()
    {
        if (artAlive == null || artDead == null)
            return false;
        bool buck = IsBuck;
        artAlive.gameObject.SetActive(false);
        artDead.gameObject.SetActive(true);
        ShowSex(artDead);
        Transform deadAntlers = PartOf(artDead, "Antlers");
        if (deadAntlers != null)
            deadAntlers.gameObject.SetActive(buck);
        FitBody(artDead);
        return true;
    }

    // A pose's part by name, anywhere under it and among the parts shown (a tom's Body, not the hidden hen's); the dead pose's copies are
    // exported as Dead_<name>.
    static Transform PartOf(Transform pose, string part)
    {
        if (pose == null)
            return null;
        Transform hidden = null;
        foreach (Transform t in pose.GetComponentsInChildren<Transform>(true))
        {
            if (t == pose || !(t.name == part || t.name.EndsWith("_" + part)))
                continue;
            if (t.gameObject.activeInHierarchy || !pose.gameObject.activeInHierarchy && t.gameObject.activeSelf && (t.parent == pose || t.parent.gameObject.activeSelf))
                return t;
            hidden = hidden ?? t;
        }
        return hidden;
    }

    // A species drawn differently by sex (a tom's fan, a hen's folded tail) keeps each look in a Male and a Female group in each pose
    // (Dead_Male, Dead_Female in the carcass); only the animal's own is shown. Species without the groups are unchanged.
    void ShowSex(Transform pose)
    {
        if (pose == null)
            return;
        foreach (Transform t in pose)
        {
            if (t.name == "Male" || t.name.EndsWith("_Male"))
                t.gameObject.SetActive(IsMale);
            else if (t.name == "Female" || t.name.EndsWith("_Female"))
                t.gameObject.SetActive(!IsMale);
        }
    }

    // The capsule the shots and the interaction ray hit, around the pose's "Body" mesh (in the model's space; the long axis is Z).
    void FitBody(Transform pose)
    {
        Transform body = PartOf(pose, "Body");
        var capsule = model.GetComponentInChildren<CapsuleCollider>(true);
        var filter = body != null ? body.GetComponent<MeshFilter>() : null;
        if (capsule == null || filter == null || filter.sharedMesh == null)
            return;
        Bounds mb = filter.sharedMesh.bounds;
        Matrix4x4 toCapsule = capsule.transform.worldToLocalMatrix * body.localToWorldMatrix;
        var b = new Bounds(toCapsule.MultiplyPoint3x4(mb.center), Vector3.zero);
        for (int i = 0; i < 8; i++)
            b.Encapsulate(toCapsule.MultiplyPoint3x4(mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1))));
        capsule.direction = 2;
        capsule.center = b.center;
        capsule.radius = Mathf.Max(0.05f, Mathf.Min(b.size.x, b.size.y) * 0.5f);
        capsule.height = Mathf.Max(b.size.z, capsule.radius * 2f);
    }

    void Awake()
    {
        terrain = Terrain.activeTerrain;
        waterMask = 1 << LayerMask.NameToLayer("Water");
        home = transform.position;
        UseArt();
        if (antlers != null)
            antlers.SetActive(Random.value < 0.5f);
        // Male or female: a deer's antlers decide it (a buck); any other species is an even chance. The art shows its Male or Female parts.
        IsMale = antlers != null ? antlers.activeSelf : Random.value < 0.5f;
        ShowSex(artAlive);
        if (artAlive != null)
            FitBody(artAlive);
        EnterIdle();
    }

    void Update()
    {
        if (state == State.Dead)
            return;

        if (Time.time >= nextSense)
        {
            nextSense = Time.time + 0.25f;
            if (state != State.Flee && state != State.Stunned)
                Sense();
        }

        switch (state)
        {
            case State.Idle:
                if (Time.time >= stateUntil)
                    PickWanderTarget();
                break;
            case State.Walk:
                MoveToward(target, walkSpeed);
                if (Flat(transform.position - target).magnitude < 0.4f || Time.time >= stateUntil)
                    EnterIdle();
                break;
            case State.Flee:
                UpdateFlee();
                break;
            case State.Stunned:
                if (Time.time >= stateUntil)
                {
                    state = State.Idle;
                    Startle(fleeFrom); // the moment is over: it runs as it normally would
                }
                break;
        }
    }

    // --- Senses ---

    void Sense()
    {
        PlayerController player = WildlifeManager.Player;
        if (player == null)
            return;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        float reach = hearing * player.DetectionMultiplier * WindFactor(player.transform.position) *
                      DifficultyManager.WildlifeAlertness; // warier game on harder difficulties
        WeatherManager weather = WeatherManager.Instance;
        if (weather != null && weather.IsRaining)
            reach *= 0.8f; // rain dampens sound, aiding stalking

        if (distance < Mathf.Max(reach, 3f))
            Startle(player.transform.position);
    }

    // Scent carries downwind: much further if the wind blows from the player toward the animal, less if not.
    float WindFactor(Vector3 playerPosition)
    {
        WeatherManager weather = WeatherManager.Instance;
        if (weather == null || weather.WindStrength < 0.05f)
            return 1f;

        float towardBearing = weather.WindDirection + 180f; // the wind blows toward this bearing
        Vector3 downwind = Quaternion.Euler(0f, towardBearing, 0f) * Vector3.forward;
        Vector3 toAnimal = Flat(transform.position - playerPosition).normalized;
        float alignment = Vector3.Dot(downwind, toAnimal); // 1 = the animal is straight downwind of the player
        return Mathf.Lerp(1f, alignment > 0f ? 1.7f : 0.7f, Mathf.Abs(alignment) * Mathf.Clamp01(weather.WindStrength * 2f));
    }

    // Runs (or flies) away from a threat.
    public void Startle(Vector3 from)
    {
        if (state == State.Dead)
            return;
        fleeFrom = from;
        if (state == State.Stunned && Time.time < stateUntil)
            return; // stopped in its tracks: it runs from here when the moment is over
        if (state != State.Flee)
            runLeft = Random.Range(35f, 70f);
        state = State.Flee;
    }

    // Hands M0e-1: a fist hit stops a small animal for a moment (it neither senses nor moves), then it flees from where the blow came as it normally would. Never harms it: nothing here touches
    // wounded or dies, and a dead animal is left alone.
    public void Stun(float seconds, Vector3 from)
    {
        if (state == State.Dead)
            return;
        fleeFrom = from;
        state = State.Stunned;
        stateUntil = Time.time + Mathf.Max(0f, seconds);
    }

    // --- Movement ---

    void EnterIdle()
    {
        state = State.Idle;
        stateUntil = Time.time + Random.Range(2f, 7f); // grazing
    }

    void PickWanderTarget()
    {
        for (int i = 0; i < 6; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 12f;
            Vector3 candidate = home + new Vector3(offset.x, 0f, offset.y);
            if (swims == OnWater(candidate))
            {
                target = candidate;
                state = State.Walk;
                stateUntil = Time.time + 15f;
                return;
            }
        }
        EnterIdle();
    }

    void UpdateFlee()
    {
        Vector3 away = Flat(transform.position - fleeFrom).normalized;
        if (away.sqrMagnitude < 0.01f)
            away = transform.forward;

        if (swims)
        {
            // Waterfowl take off and fly low and away.
            transform.position += (away * runSpeed + Vector3.up * 3f) * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(away);
            runLeft -= runSpeed * Time.deltaTime;
            if (runLeft <= 0f)
                Destroy(gameObject);
            return;
        }

        // Steer around water rather than into it.
        Vector3 step = away;
        if (OnWater(transform.position + step * 3f))
            step = Quaternion.Euler(0f, 70f, 0f) * away;
        MoveToward(transform.position + step * 5f, runSpeed);
        runLeft -= runSpeed * Time.deltaTime;

        if (runLeft <= 0f)
        {
            if (wounded && willDie)
            {
                Die(true);
                ToolStatus.Flash($"The wounded {DisplayName.ToLower()} went down after a short run");
                return;
            }
            // Calmed down somewhere new, but wary: it may bolt again.
            home = transform.position;
            wounded = false;
            EnterIdle();
        }
    }

    void MoveToward(Vector3 goal, float speed)
    {
        Vector3 direction = Flat(goal - transform.position);
        if (direction.sqrMagnitude < 0.0001f)
            return;
        direction.Normalize();

        Vector3 next = transform.position + direction * speed * Time.deltaTime;
        next.y = swims ? WaterHeight(next, transform.position.y) : GroundHeight(next);
        if (hops && speed > 0f)
            next.y += Mathf.Abs(Mathf.Sin(Time.time * (speed > walkSpeed ? 14f : 8f))) * 0.12f;
        transform.position = next;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 6f);
    }

    float GroundHeight(Vector3 at) => terrain != null ? terrain.SampleHeight(at) + terrain.transform.position.y : at.y;

    float WaterHeight(Vector3 at, float fallback) =>
        Physics.Raycast(new Vector3(at.x, at.y + 20f, at.z), Vector3.down, out RaycastHit hit, 40f, waterMask, QueryTriggerInteraction.Ignore)
            ? hit.point.y : fallback;

    bool OnWater(Vector3 at) => IsOverWater(at, out _);

    // Whether there's water surface above the ground at this point (checked against the Water layer alone, so the
    // animal's own collider or anything else standing there doesn't get in the way).
    public static bool IsOverWater(Vector3 at, out float surface)
    {
        surface = at.y;
        Terrain terrain = Terrain.activeTerrain;
        int water = 1 << LayerMask.NameToLayer("Water");
        if (!Physics.Raycast(new Vector3(at.x, at.y + 30f, at.z), Vector3.down, out RaycastHit hit, 80f, water, QueryTriggerInteraction.Ignore))
            return false;
        surface = hit.point.y;
        float ground = terrain != null ? terrain.SampleHeight(hit.point) + terrain.transform.position.y : float.MinValue;
        return ground < surface - 0.05f;
    }

    static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

    public string DisplayName => WildlifeManager.Instance != null ? WildlifeManager.Instance.NameOf(speciesId) : speciesId;

    // --- Being shot ---

    // Hunting_System.md's Shot Placement & Outcomes. Returns the outcome message for the HUD.
    public string OnShot(bool vitals, float cleanKillChance, Vector3 shooter)
    {
        if (state == State.Dead)
            return "";

        // Small game drops from any solid hit; bigger animals need the vitals for a clean kill.
        if (tier == GameTier.Small || (vitals && Random.value <= cleanKillChance))
        {
            Die(false);
            return $"Clean kill — {DisplayName} down";
        }

        // Wounding hit (Alpha 0.1: no tracking — a reduced harvest at best, usually lost).
        wounded = true;
        willDie = Random.value < (vitals ? 0.6f : 0.3f);
        Startle(shooter);
        return willDie ? $"Wounding hit — the {DisplayName.ToLower()} ran" : $"Wounding hit — the {DisplayName.ToLower()} got away";
    }

    // Hands M0e-2: a Knife blow finishes the animal through the same kill path as a clean shot (Die(false): a Carcass, a full harvest). KnifeRules decides which animals it is used on; a dead one is
    // left alone.
    public void Slay()
    {
        if (state == State.Dead)
            return;
        Die(false);
    }

    void Die(bool reducedHarvest)
    {
        state = State.Dead;
        if (!ShowDeadPose() && model != null)
            model.localRotation = Quaternion.Euler(0f, 0f, 85f) * model.localRotation; // fallen on its side (a species with no art yet)

        if (swims)
        {
            // Downed waterfowl drop onto the water (sfx_splash) and drift to the bank so they can be reached.
            Vector3 p = transform.position;
            p.y = WaterHeight(p, p.y);
            transform.position = p;
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayAt(AudioManager.Instance.Splash, p);
        }
        else
        {
            Vector3 p = transform.position;
            p.y = GroundHeight(p);
            transform.position = p;
        }

        var carcass = GetComponent<Carcass>();
        if (carcass == null)
            carcass = gameObject.AddComponent<Carcass>();
        carcass.Begin(this, reducedHarvest);
    }
}
