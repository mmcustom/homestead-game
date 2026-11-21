using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

// Wave 8 fire glow at the hot structures (2026-11-20): while a Kiln fires, a Charcoal Pit burns, a Bloomery smelts or a Smithy works, a warm
// flickering light sits in the mouth and embers drift up out of it. The Bloomery and Smithy breathe with their bellows: every few seconds the glow
// swells and a puff of sparks comes out. Before this the mouths only glowed as a dim tinted mesh (no light, no flicker).
// A persistent child of the structure, like KilnSmoke: PlacedStructure.Rebuild only switches it (Drive), so the embers are not restarted every hour.
// It sits at the art's Anchor_Mouth when the structure has art, otherwise at the same spot the primitive build uses.
public class StructureGlow : MonoBehaviour
{
    public const string ChildName = "Fire Glow";

    PileKind kind;
    Light glow;
    ParticleSystem embers;
    bool on;
    float level, seed, nextBellows, bellows;

    public bool On => on;
    public Light GlowLight => glow;

    // Switches the glow on a structure, making it the first time.
    public static StructureGlow Drive(Transform structure, PileKind kind, bool on)
    {
        Transform child = structure.Find(ChildName);
        StructureGlow g = child != null ? child.GetComponent<StructureGlow>() : null;
        if (g == null)
        {
            if (!on)
                return null; // nothing to switch off
            var go = new GameObject(ChildName);
            go.transform.SetParent(structure, false);
            g = go.AddComponent<StructureGlow>();
            g.Build(kind);
        }
        g.Place(structure);
        g.on = on;
        if (g.embers != null)
        {
            ParticleSystem.EmissionModule emission = g.embers.emission;
            emission.enabled = on;
            if (on && !g.embers.isPlaying)
                g.embers.Play();
        }
        return g;
    }

    // The mouth, in the structure's own space, for a build without art.
    public static Vector3 DefaultMouth(PileKind kind)
    {
        switch (kind)
        {
            case PileKind.Kiln: return new Vector3(0f, 0.45f, 0.95f);
            case PileKind.Bloomery: return new Vector3(0f, 0.28f, 0.5f);
            case PileKind.Smithy: return new Vector3(-0.8f, 0.3f, -0.27f);
            default: return new Vector3(0f, 0.4f, 0.38f); // the Charcoal Pit's draught hole
        }
    }

    // Candela at full glow, and light range.
    public static float Brightness(PileKind kind) =>
        kind == PileKind.Kiln ? 90f : kind == PileKind.Bloomery ? 120f : kind == PileKind.Smithy ? 110f : 30f; // the Campfire is 150

    static bool HasBellows(PileKind kind) => kind == PileKind.Bloomery || kind == PileKind.Smithy;

    void Place(Transform structure)
    {
        Transform mouth = FindDeep(structure, "Anchor_Mouth");
        transform.localPosition = mouth != null ? structure.InverseTransformPoint(mouth.position) : DefaultMouth(kind);
    }

    static Transform FindDeep(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name)
                return t;
        return null;
    }

    void Build(PileKind pileKind)
    {
        kind = pileKind;
        seed = Random.value * 100f;
        nextBellows = Time.time + Random.Range(2f, 5f);

        glow = gameObject.AddComponent<Light>();
        if (GetComponent<HDAdditionalLightData>() == null)
            gameObject.AddComponent<HDAdditionalLightData>();
        glow.type = LightType.Point;
        glow.color = new Color(1f, 0.52f, 0.22f);
        glow.range = kind == PileKind.CharcoalPit ? 3.5f : 6f;
        glow.shadows = LightShadows.None;
        glow.intensity = 0f;

        var go = new GameObject("Embers");
        go.transform.SetParent(transform, false);
        go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        embers = go.AddComponent<ParticleSystem>();
        embers.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule main = embers.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.04f);
        main.maxParticles = 60;
        main.gravityModifier = -0.05f;
        ParticleSystem.EmissionModule emission = embers.emission;
        emission.rateOverTime = kind == PileKind.CharcoalPit ? 1.2f : kind == PileKind.Kiln ? 2.5f : 4f;
        ParticleSystem.ShapeModule shape = embers.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.08f;
        ParticleSystem.NoiseModule noise = embers.noise;
        noise.enabled = true;
        noise.strength = 0.4f;
        noise.frequency = 0.8f;
        noise.quality = ParticleSystemNoiseQuality.Low;
        var curve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.7f, 0.8f), new Keyframe(1f, 0f));
        ParticleSystem.SizeOverLifetimeModule size = embers.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, curve);

        var r = go.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Billboard;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        Material template = Resources.Load<Material>("KilnSmoke");
        if (template != null)
        {
            var m = new Material(template) { name = "Embers" };
            Texture2D spark = Resources.Load<Texture2D>("Art/fx_spark");
            if (spark != null)
            {
                m.SetTexture("_UnlitColorMap", spark);
                m.SetTexture("_MainTex", spark);
            }
            m.SetColor("_UnlitColor", WorldFx.SparkColor);
            m.SetColor("_Color", WorldFx.SparkColor);
            r.sharedMaterial = m;
        }
    }

    void Update()
    {
        level = Mathf.MoveTowards(level, on ? 1f : 0f, Time.deltaTime * (on ? 0.5f : 0.25f));
        if (on && HasBellows(kind) && Time.time >= nextBellows)
        {
            nextBellows = Time.time + Random.Range(3.5f, 7f);
            bellows = 1f;
            if (embers != null)
                embers.Emit(Random.Range(8, 15));
        }
        bellows = Mathf.MoveTowards(bellows, 0f, Time.deltaTime * 0.8f);
        if (glow == null)
            return;
        float t = Time.time * 6f + seed;
        float flicker = 0.82f + 0.12f * Mathf.PerlinNoise(t, 0.3f) + 0.06f * Mathf.Sin(t * 2.1f);
        glow.intensity = Brightness(kind) * level * flicker * (1f + 0.8f * bellows);
        glow.enabled = level > 0.001f;
    }
}
