using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

// Wave 8 effects (2026-11-20, Mike: "all of Wave 8 in order"). One hidden object in the World scene, made by PlayerAudio, holds:
//  - action particles: wood chips from the axes, stone chips, dust and the odd spark from the pick, dirt from the shovel, a flash and dust from the
//    hammer (AxeTool.Swing calls Blow at the point each blow lands);
//  - steam over a boiling pot and a wisp over cooking (CampfireCooking calls Steam every frame);
//  - water: a splash and ripples when wading, at the bobber, and the occasional fish jumping near the player;
//  - cold breath in front of the face below about 4 C;
//  - footprints in snow and in mud (PlayerAudio calls Footstep each stride), as fading HDRP decals.
// The screen effects for the body (cold, heat, tired, waking weakened) are BodyStateFx on the same object; the kiln, bloomery, smithy and charcoal pit
// glow is StructureGlow.
// Particles that should look like solid bits (chips, stones, clods) are small HDRP/Lit cubes, so they are lit like the world. Soft ones (dust, steam,
// breath, splashes, ripples) use the Kiln Smoke unlit material, darkened at night as Precipitation does. HDRP Unlit ignores a particle's own colour and
// alpha, so each soft kind has its own material and its particles shrink away rather than fade.
public enum FxKind { WoodChips, StoneChips, Dirt, Dust, StoneDust, Sparks, Flash, Steam, Splash, Ripple, Breath }

public class WorldFx : MonoBehaviour
{
    static WorldFx instance;
    public static WorldFx Instance => instance;

    // Made once by PlayerAudio in the World scene; it goes with the scene. Edit-mode calls (tests) do nothing.
    public static WorldFx Ensure()
    {
        if (instance != null)
            return instance;
        if (!Application.isPlaying)
            return null;
        var go = new GameObject("World FX");
        instance = go.AddComponent<WorldFx>();
        go.AddComponent<BodyStateFx>();
        return instance;
    }

    // ---------------------------------------------------------------- colours (exposed for tests and tuning)
    public static readonly Color WoodChipColor = new Color(0.66f, 0.52f, 0.33f);
    public static readonly Color StoneChipColor = new Color(0.52f, 0.51f, 0.48f);
    public static readonly Color DirtColor = new Color(0.30f, 0.22f, 0.14f);
    public static readonly Color DustTint = new Color(0.62f, 0.55f, 0.44f, 0.35f);
    public static readonly Color StoneDustTint = new Color(0.66f, 0.65f, 0.62f, 0.35f);
    public static readonly Color SteamTint = new Color(0.92f, 0.93f, 0.95f, 0.30f);
    public static readonly Color BreathTint = new Color(0.90f, 0.92f, 0.95f, 0.16f);
    public static readonly Color WaterTint = new Color(0.78f, 0.84f, 0.88f, 0.55f);
    public static readonly Color SparkColor = new Color(6f, 2.6f, 0.7f, 1f);   // HDR: bright at night, visible by day
    public static readonly Color FlashColor = new Color(5f, 4.2f, 3f, 0.9f);

    public const float BreathBelowC = 4f;

    readonly Dictionary<FxKind, ParticleSystem> systems = new Dictionary<FxKind, ParticleSystem>();
    readonly List<Material> nightDimmed = new List<Material>();
    readonly List<Color> nightBase = new List<Color>();
    DayNightCycle dayNight;
    SeasonalGround ground;
    Material template;
    Mesh cube;

    void Awake()
    {
        instance = this;
        template = Resources.Load<Material>("KilnSmoke");
        cube = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        BuildSystems();
        nextFishJump = Time.time + Random.Range(15f, 40f);
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    // ---------------------------------------------------------------- building
    void BuildSystems()
    {
        systems[FxKind.WoodChips] = Solid("Wood Chips", WoodChipColor, 60);
        systems[FxKind.StoneChips] = Solid("Stone Chips", StoneChipColor, 60);
        systems[FxKind.Dirt] = Solid("Dirt", DirtColor, 80);
        systems[FxKind.Dust] = Soft("Dust", "Art/fx_smoke_puff", DustTint, 40, ParticleSystemRenderMode.Billboard, -0.02f, true, true);
        systems[FxKind.StoneDust] = Soft("Stone Dust", "Art/fx_smoke_puff", StoneDustTint, 40, ParticleSystemRenderMode.Billboard, -0.02f, true, true);
        systems[FxKind.Steam] = Soft("Steam", "Art/fx_smoke_puff", SteamTint, 60, ParticleSystemRenderMode.Billboard, -0.05f, true, true);
        systems[FxKind.Breath] = Soft("Breath", "Art/fx_smoke_puff", BreathTint, 24, ParticleSystemRenderMode.Billboard, -0.01f, true, true);
        systems[FxKind.Splash] = Soft("Splash", "Art/fx_droplet", WaterTint, 80, ParticleSystemRenderMode.Stretch, 1f, false, true);
        systems[FxKind.Ripple] = Soft("Ripples", "Art/fx_ripple", WaterTint, 40, ParticleSystemRenderMode.HorizontalBillboard, 0f, true, true);
        systems[FxKind.Sparks] = Soft("Sparks", "Art/fx_spark", SparkColor, 60, ParticleSystemRenderMode.Stretch, 0.6f, false, false);
        systems[FxKind.Flash] = Soft("Flash", "Art/fx_spark", FlashColor, 8, ParticleSystemRenderMode.Billboard, 0f, false, false);

        // Ripples spread out flat and then go; steam, dust and breath swell as they drift.
        SizeCurve(systems[FxKind.Ripple], new Keyframe(0f, 0.15f), new Keyframe(0.85f, 1f), new Keyframe(1f, 0f));
        foreach (FxKind kind in new[] { FxKind.Dust, FxKind.StoneDust, FxKind.Steam, FxKind.Breath })
            SizeCurve(systems[kind], new Keyframe(0f, 0.35f), new Keyframe(0.6f, 1f), new Keyframe(1f, 0f));
        SizeCurve(systems[FxKind.Flash], new Keyframe(0f, 1f), new Keyframe(1f, 0f));
    }

    ParticleSystem Base(string name, int max, float gravity)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = max;
        main.gravityModifier = gravity;
        main.startLifetime = 1f;
        main.startSpeed = 0f;
        ParticleSystem.EmissionModule emission = ps.emission;
        emission.enabled = false; // everything here is emitted by hand
        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = false;
        var r = go.GetComponent<ParticleSystemRenderer>();
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        ps.Play();
        return ps;
    }

    // Chips, stones and clods: small lit cubes that tumble, bounce once on the ground and shrink away.
    ParticleSystem Solid(string name, Color colour, int max)
    {
        ParticleSystem ps = Base(name, max, 1f);
        ParticleSystem.MainModule main = ps.main;
        main.startSize3D = true;
        main.startRotation3D = true;
        var r = ps.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Mesh;
        r.mesh = cube;
        r.alignment = ParticleSystemRenderSpace.Local;
        var lit = new Material(Shader.Find("HDRP/Lit")) { name = name };
        lit.SetColor("_BaseColor", colour);
        lit.SetFloat("_Smoothness", 0.15f);
        r.sharedMaterial = lit;

        ParticleSystem.CollisionModule collision = ps.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision3D;
        collision.quality = ParticleSystemCollisionQuality.Low;
        collision.dampen = 0.55f;
        collision.bounce = 0.2f;
        collision.lifetimeLoss = 0f;
        collision.radiusScale = 0.5f;
        collision.collidesWith = ~((1 << 2) | (1 << 5) | (1 << 6)); // not Ignore Raycast, UI or the Viewmodel

        SizeCurve(ps, new Keyframe(0f, 1f), new Keyframe(0.75f, 1f), new Keyframe(1f, 0f));
        return ps;
    }

    // Soft sprites on the Kiln Smoke material. dim: darkened at night (sparks and flashes are their own light, so they are not).
    ParticleSystem Soft(string name, string texture, Color colour, int max, ParticleSystemRenderMode mode, float gravity, bool spin, bool dim)
    {
        ParticleSystem ps = Base(name, max, gravity);
        var r = ps.GetComponent<ParticleSystemRenderer>();
        r.renderMode = mode;
        if (mode == ParticleSystemRenderMode.Stretch)
        {
            r.velocityScale = 0.05f;
            r.lengthScale = 1.2f;
        }
        if (template != null)
        {
            var m = new Material(template) { name = name };
            Texture2D tex = Resources.Load<Texture2D>(texture);
            if (tex != null)
            {
                m.SetTexture("_UnlitColorMap", tex);
                m.SetTexture("_MainTex", tex);
            }
            m.SetColor("_UnlitColor", colour);
            m.SetColor("_Color", colour);
            r.sharedMaterial = m;
            if (dim)
            {
                nightDimmed.Add(m);
                nightBase.Add(colour);
            }
        }
        if (spin)
        {
            ParticleSystem.MainModule main = ps.main;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        }
        return ps;
    }

    static void SizeCurve(ParticleSystem ps, params Keyframe[] keys)
    {
        var curve = new AnimationCurve(keys);
        ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, curve);
    }

    // ---------------------------------------------------------------- emitting
    void Emit(FxKind kind, Vector3 position, Vector3 velocity, float size, float life, Vector3 size3 = default)
    {
        if (!systems.TryGetValue(kind, out ParticleSystem ps))
            return;
        var p = new ParticleSystem.EmitParams
        {
            position = position,
            velocity = velocity,
            startLifetime = life,
            applyShapeToPosition = false,
        };
        if (size3 != default)
        {
            p.startSize3D = size3;
            p.rotation3D = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
            p.angularVelocity3D = Random.insideUnitSphere * 720f;
        }
        else
        {
            p.startSize = size;
        }
        ps.Emit(p, 1);
    }

    static Vector3 Spray(Vector3 away, float spread, float speedMin, float speedMax, float up)
    {
        Vector3 dir = (away + Random.insideUnitSphere * spread + Vector3.up * up).normalized;
        return dir * Random.Range(speedMin, speedMax);
    }

    // A blow from AxeTool.Swing landed at point. from: where the player stands (bits fly back out of the cut toward them).
    public static void Blow(string toolId, Vector3 point, Vector3 from)
    {
        if (instance == null || string.IsNullOrEmpty(toolId))
            return;
        Vector3 away = from - point;
        away.y = 0f;
        away = away.sqrMagnitude > 0.001f ? away.normalized : Vector3.up;
        Vector3 at = point + away * 0.05f;

        if (ToolRules.IsPickAxe(toolId))
        {
            for (int i = 0; i < 7; i++)
                instance.Emit(FxKind.StoneChips, at, Spray(away, 0.7f, 1.5f, 3.5f, 0.6f), 0f, Random.Range(0.9f, 1.6f), Chip(0.03f, 0.06f));
            for (int i = 0; i < 3; i++)
                instance.Emit(FxKind.StoneDust, at, Spray(away, 0.6f, 0.2f, 0.6f, 0.4f), Random.Range(0.25f, 0.45f), Random.Range(1.0f, 1.8f));
            if (Random.value < 0.5f) // iron or stone on rock now and then strikes sparks
            {
                instance.Emit(FxKind.Flash, at, Vector3.zero, 0.22f, 0.07f);
                for (int i = 0; i < 6; i++)
                    instance.Emit(FxKind.Sparks, at, Spray(away, 0.9f, 2f, 4.5f, 0.4f), 0.035f, Random.Range(0.2f, 0.45f));
            }
        }
        else if (ToolRules.IsDiggingTool(toolId))
        {
            for (int i = 0; i < 9; i++)
                instance.Emit(FxKind.Dirt, at, Spray(away, 0.6f, 1.2f, 2.6f, 1.0f), 0f, Random.Range(0.8f, 1.4f), Chip(0.04f, 0.08f, true));
            for (int i = 0; i < 3; i++)
                instance.Emit(FxKind.Dust, at, Spray(away, 0.6f, 0.2f, 0.5f, 0.5f), Random.Range(0.3f, 0.5f), Random.Range(1.0f, 1.8f));
        }
        else if (toolId == WoodManager.HammerId)
        {
            instance.Emit(FxKind.Flash, at, Vector3.zero, 0.16f, 0.06f);
            for (int i = 0; i < 3; i++)
                instance.Emit(FxKind.WoodChips, at, Spray(away, 0.8f, 1f, 2f, 0.6f), 0f, Random.Range(0.6f, 1.1f), Chip(0.015f, 0.03f));
            for (int i = 0; i < 2; i++)
                instance.Emit(FxKind.Dust, at, Spray(away, 0.6f, 0.15f, 0.4f, 0.4f), Random.Range(0.2f, 0.35f), Random.Range(0.8f, 1.4f));
        }
        else // a felling or splitting blow
        {
            for (int i = 0; i < 11; i++)
                instance.Emit(FxKind.WoodChips, at, Spray(away, 0.8f, 1.5f, 3.2f, 0.7f), 0f, Random.Range(1.0f, 1.8f), Chip(0.03f, 0.07f));
            instance.Emit(FxKind.Dust, at, Spray(away, 0.5f, 0.2f, 0.4f, 0.4f), Random.Range(0.2f, 0.35f), Random.Range(0.8f, 1.2f));
        }
    }

    // A flat chip (or a rounder clod) between the two sizes, in metres.
    static Vector3 Chip(float min, float max, bool clod = false)
    {
        float s = Random.Range(min, max);
        return clod ? new Vector3(s, s * Random.Range(0.6f, 1f), s * Random.Range(0.7f, 1f))
                    : new Vector3(s, s * Random.Range(0.15f, 0.3f), s * Random.Range(0.5f, 1.4f));
    }

    // ---------------------------------------------------------------- steam (CampfireCooking, every frame)
    bool steamOn, steamBoil;
    Vector3 steamAt;
    float steamDue;

    public static void Steam(bool on, Vector3 fireAt, bool boiling)
    {
        if (instance == null)
            return;
        instance.steamOn = on;
        instance.steamBoil = boiling;
        instance.steamAt = fireAt + Vector3.up * (boiling ? 0.62f : 0.32f);
    }

    void UpdateSteam()
    {
        if (!steamOn || Time.time < steamDue)
            return;
        steamDue = Time.time + (steamBoil ? 0.12f : 0.45f);
        Vector3 jitter = new Vector3(Random.Range(-0.1f, 0.1f), 0f, Random.Range(-0.1f, 0.1f));
        Emit(FxKind.Steam, steamAt + jitter, new Vector3(jitter.x, Random.Range(0.35f, 0.6f), jitter.z) + Drift() * 0.3f,
             Random.Range(0.18f, 0.32f) * (steamBoil ? 1f : 0.7f), Random.Range(2.0f, 3.2f));
    }

    static Vector3 Drift()
    {
        WeatherManager weather = WeatherManager.Instance;
        if (weather == null)
            return Vector3.zero;
        float bearing = weather.WindDirection * Mathf.Deg2Rad;
        return -new Vector3(Mathf.Sin(bearing), 0f, Mathf.Cos(bearing)) * weather.WindStrength;
    }

    // ---------------------------------------------------------------- water
    public static void Splash(Vector3 at, float strength)
    {
        if (instance == null)
            return;
        int drops = Mathf.RoundToInt(Mathf.Lerp(4f, 18f, strength));
        for (int i = 0; i < drops; i++)
        {
            Vector3 v = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) * Mathf.Lerp(0.5f, 1.4f, strength);
            v.y = Random.Range(1.2f, 2.8f) * Mathf.Lerp(0.6f, 1.2f, strength);
            instance.Emit(FxKind.Splash, at + Vector3.up * 0.02f, v, Random.Range(0.03f, 0.06f), Random.Range(0.4f, 0.8f));
        }
        Ripple(at, Mathf.Lerp(0.5f, 1.4f, strength));
    }

    public static void Ripple(Vector3 at, float size)
    {
        if (instance == null)
            return;
        instance.Emit(FxKind.Ripple, at + Vector3.up * 0.015f, Vector3.zero, size, Random.Range(1.1f, 1.6f));
    }

    float nextFishJump;

    // Now and then a fish jumps somewhere on the water near the player.
    void UpdateFishJumps(Vector3 player)
    {
        if (Time.time < nextFishJump)
            return;
        nextFishJump = Time.time + Random.Range(20f, 60f);
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
            return;
        for (int attempt = 0; attempt < 12; attempt++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized * Random.Range(6f, 25f);
            Vector3 p = player + new Vector3(dir.x, 0f, dir.y);
            if (!Animal.IsOverWater(p, out float surface))
                continue;
            float bed = terrain.SampleHeight(p) + terrain.transform.position.y;
            if (surface - bed < 0.35f)
                continue;
            p.y = surface;
            Splash(p, 0.7f);
            AudioManager audio = AudioManager.Instance;
            if (audio != null)
                audio.PlayAt(audio.Splash, p);
            return;
        }
    }

    // ---------------------------------------------------------------- footsteps (PlayerAudio, each stride)
    public const int MaxPrints = 48;
    public const float SnowPrintsFrom = 0.3f;
    readonly Queue<DecalProjector> prints = new Queue<DecalProjector>();
    readonly Dictionary<DecalProjector, float> printBorn = new Dictionary<DecalProjector, float>();
    Material snowPrint, mudPrint;
    bool leftFoot;

    public static void Footstep(Vector3 feet, float yaw, SurfaceType surface)
    {
        if (instance == null)
            return;
        instance.Step(feet, yaw, surface);
    }

    void Step(Vector3 feet, float yaw, SurfaceType surface)
    {
        leftFoot = !leftFoot;
        if (Animal.IsOverWater(feet, out float water) && water > feet.y + 0.03f)
        {
            Vector3 at = new Vector3(feet.x, water, feet.z);
            Splash(at, Mathf.Clamp01((water - feet.y) * 2f) * 0.6f);
            return;
        }

        if (ground == null)
            ground = FindFirstObjectByType<SeasonalGround>();
        bool snow = ground != null && ground.Amount >= SnowPrintsFrom;
        WeatherManager weather = WeatherManager.Instance;
        bool mud = !snow && surface == SurfaceType.Dirt && weather != null && (weather.IsRaining || weather.DryDays == 0);
        if (!snow && !mud)
            return;
        Print(feet, yaw, snow);
    }

    void Print(Vector3 feet, float yaw, bool snow)
    {
        Material material = snow ? (snowPrint != null ? snowPrint : snowPrint = PrintMaterial(new Color(0.58f, 0.64f, 0.74f, 0.6f)))
                                 : (mudPrint != null ? mudPrint : mudPrint = PrintMaterial(new Color(0.14f, 0.10f, 0.06f, 0.65f)));
        if (material == null)
            return;
        DecalProjector decal;
        if (prints.Count >= MaxPrints)
        {
            decal = prints.Dequeue();
            printBorn.Remove(decal);
        }
        else
        {
            var go = new GameObject("Footprint");
            go.transform.SetParent(transform, false);
            decal = go.AddComponent<DecalProjector>();
            decal.size = new Vector3(0.13f, 0.32f, 0.5f);
            decal.pivot = Vector3.zero;
            decal.drawDistance = 60f;
        }
        Quaternion facing = Quaternion.Euler(0f, yaw, 0f);
        Vector3 side = facing * Vector3.right * (leftFoot ? -0.11f : 0.11f);
        decal.transform.SetPositionAndRotation(feet + side + Vector3.up * 0.2f, facing * Quaternion.Euler(90f, 0f, 0f));
        decal.material = material;
        decal.uvScale = new Vector2(leftFoot ? -1f : 1f, 1f);
        decal.uvBias = new Vector2(leftFoot ? 1f : 0f, 0f);
        decal.fadeFactor = 1f;
        decal.gameObject.SetActive(true);
        prints.Enqueue(decal);
        printBorn[decal] = Time.time;
    }

    public const float PrintSeconds = 180f;

    void FadePrints()
    {
        foreach (DecalProjector decal in prints)
        {
            if (decal == null || !printBorn.TryGetValue(decal, out float born))
                continue;
            float age = (Time.time - born) / PrintSeconds;
            decal.fadeFactor = age < 0.6f ? 1f : Mathf.Clamp01(1f - (age - 0.6f) / 0.4f);
            if (age >= 1f && decal.gameObject.activeSelf)
                decal.gameObject.SetActive(false);
        }
    }

    static Material PrintMaterial(Color colour)
    {
        GameObject clay = Resources.Load<GameObject>("Art/world_clay_patch");
        DecalProjector source = clay != null ? clay.GetComponentInChildren<DecalProjector>(true) : null;
        Texture2D print = Resources.Load<Texture2D>("Art/fx_footprint");
        if (source == null || source.material == null || print == null)
            return null;
        var m = new Material(source.material) { name = "Footprint" };
        m.SetTexture("_BaseColorMap", print);
        m.SetTexture("_NormalMap", null);
        m.SetTexture("_MaskMap", null);
        m.SetColor("_BaseColor", colour);
        m.SetFloat("_AffectNormal", 0f);
        m.SetFloat("_AffectMetal", 0f);
        m.SetFloat("_AffectAO", 0f);
        m.SetFloat("_AffectSmoothness", 0f);
        m.SetFloat("_DecalBlend", 1f);
        HDMaterial.ValidateMaterial(m);
        return m;
    }

    // ---------------------------------------------------------------- breath
    float nextBreath;

    void UpdateBreath(Camera cam, PlayerController player)
    {
        WeatherManager weather = WeatherManager.Instance;
        if (weather == null || weather.TemperatureC >= BreathBelowC || Time.time < nextBreath)
            return;
        bool hard = player != null && (player.IsSprinting || player.Stamina < player.EffectiveMaxStamina * 0.3f);
        nextBreath = Time.time + (hard ? 1.5f : 3.4f) * Random.Range(0.85f, 1.15f);
        float cold = Mathf.InverseLerp(BreathBelowC, -12f, weather.TemperatureC);
        Transform t = cam.transform;
        Vector3 mouth = t.position + t.forward * 0.42f - t.up * 0.14f;
        int puffs = 3 + Mathf.RoundToInt(cold * 3f);
        for (int i = 0; i < puffs; i++)
            Emit(FxKind.Breath, mouth + Random.insideUnitSphere * 0.03f,
                 t.forward * Random.Range(0.25f, 0.5f) + Vector3.up * Random.Range(0.05f, 0.15f) + Random.insideUnitSphere * 0.05f + Drift() * 0.15f,
                 Random.Range(0.12f, 0.2f) * Mathf.Lerp(0.8f, 1.3f, cold), Random.Range(1.0f, 1.6f));
    }

    // ---------------------------------------------------------------- per frame
    PlayerController playerController;

    void Update()
    {
        if (dayNight == null)
            dayNight = FindFirstObjectByType<DayNightCycle>();
        float light = dayNight != null ? Mathf.Lerp(0.12f, 1f, Mathf.InverseLerp(-6f, 10f, dayNight.SunElevation)) : 1f;
        for (int i = 0; i < nightDimmed.Count; i++)
        {
            Color c = nightBase[i];
            nightDimmed[i].SetColor("_UnlitColor", new Color(c.r * light, c.g * light, c.b * light, c.a));
        }

        GameManager game = GameManager.Instance;
        bool playing = game == null || game.State == GameState.Playing || game.State == GameState.Menu;
        UpdateSteam();
        steamOn = false; // CampfireCooking says so again next frame while it lasts
        FadePrints();
        if (!playing)
            return;

        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();
        Camera cam = Camera.main;
        bool asleep = SleepManager.Instance != null && SleepManager.Instance.IsSleeping;
        if (cam != null && !asleep)
            UpdateBreath(cam, playerController);
        if (playerController != null)
            UpdateFishJumps(playerController.transform.position);
    }
}
