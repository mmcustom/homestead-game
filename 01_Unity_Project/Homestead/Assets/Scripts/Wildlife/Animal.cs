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
    enum State { Idle, Walk, Flee, Dead }

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

    void Awake()
    {
        terrain = Terrain.activeTerrain;
        waterMask = 1 << LayerMask.NameToLayer("Water");
        home = transform.position;
        if (antlers != null)
            antlers.SetActive(Random.value < 0.5f);
        EnterIdle();
    }

    void Update()
    {
        if (state == State.Dead)
            return;

        if (Time.time >= nextSense)
        {
            nextSense = Time.time + 0.25f;
            if (state != State.Flee)
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
        }
    }

    // --- Senses ---

    void Sense()
    {
        PlayerController player = WildlifeManager.Player;
        if (player == null)
            return;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        float reach = hearing * player.DetectionMultiplier * WindFactor(player.transform.position);
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
        if (state != State.Flee)
            runLeft = Random.Range(35f, 70f);
        state = State.Flee;
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

    void Die(bool reducedHarvest)
    {
        state = State.Dead;
        if (model != null)
            model.localRotation = Quaternion.Euler(0f, 0f, 85f) * model.localRotation; // fallen on its side

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
