using UnityEngine;

// The smoke over a firing Kiln (Pottery P2 polish): soft, translucent puffs that rise slowly, grow, drift a little and thin out. It lives as one persistent child of the Kiln
// (PlacedStructure.Rebuild redraws the shapes every game hour, which would restart a particle system built with them) and is switched by SetFiring: emission runs only while the
// firing runs, and the last puffs finish rising after it ends.
//
// Look: the HDRP Unlit transparent material Resources/KilnSmoke.mat (the Snow material's settings, no texture) with a round soft-edged puff made here. No lit or glossy material.
// The project had no smoke to reuse (the campfire is flame and ember sprites). HDRP Unlit ignores particle vertex colour, so the thinning at the end of a puff is done with its size
// (a puff shrinks away) as well as the colour gradient; opacity is the material's alpha (opacity below).
// Every number is a public field on the "Kiln Smoke" child, adjustable in the Inspector while the game runs (the Editor re-applies them every frame).
public class KilnSmoke : MonoBehaviour
{
    [Header("Where")]
    [Tooltip("Metres above the Kiln's origin: the vent is at 1.0.")]
    public float height = 1.1f;
    [Tooltip("Radius of the circle the puffs start in.")]
    public float spreadRadius = 0.12f;

    [Header("How many, how long")]
    public float puffsPerSecond = 3f;
    public float lifetimeSeconds = 9f;
    public int maxPuffs = 40;

    [Header("Movement")]
    [Tooltip("Metres per second straight up.")]
    public float riseSpeed = 0.5f;
    [Tooltip("How far the wobble pushes a puff sideways (noise strength).")]
    public float driftStrength = 0.35f;
    [Tooltip("How quickly the wobble changes (noise frequency); small is slow and lazy.")]
    public float driftFrequency = 0.2f;

    [Header("Size")]
    public float startSizeMin = 0.45f;
    public float startSizeMax = 0.7f;
    [Tooltip("How many times its starting size a puff reaches before it thins away.")]
    public float growthMultiplier = 3.2f;

    [Header("Look")]
    [Tooltip("The material's alpha: 0.3 is a thin haze, 0.6 a thick grey column.")]
    [Range(0f, 1f)] public float opacity = 0.3f;
    public Color tint = new Color(0.62f, 0.62f, 0.6f);
    [Tooltip("The fraction of a puff's life spent fading in, and (from the end) fading out.")]
    [Range(0.05f, 0.5f)] public float fadeInFraction = 0.2f;
    [Range(0.05f, 0.6f)] public float fadeOutFraction = 0.45f;

    ParticleSystem system;
    Material material;
    bool firing;

    static Texture2D puffTexture;
    static Material template;

    // Creates the persistent child on a Kiln. Call once; later calls through SetFiring only switch the emission.
    public static KilnSmoke Create(Transform parent)
    {
        var go = new GameObject("Kiln Smoke");
        go.transform.SetParent(parent, false);
        var smoke = go.AddComponent<KilnSmoke>();
        smoke.Setup();
        return smoke;
    }

    // A white disc with a soft round edge: alpha falls from the middle to nothing at the rim, so a puff has no edge.
    static Texture2D PuffTexture()
    {
        if (puffTexture != null)
            return puffTexture;
        const int size = 64;
        puffTexture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Kiln Smoke Puff", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - size / 2f + 0.5f) * (x - size / 2f + 0.5f) + (y - size / 2f + 0.5f) * (y - size / 2f + 0.5f)) / (size / 2f);
                float a = Mathf.Clamp01(1f - d);
                a = a * a * (3f - 2f * a); // smoothstep: soft in the middle and at the rim
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
            }
        puffTexture.SetPixels32(pixels);
        puffTexture.Apply(false, true);
        return puffTexture;
    }

    void Awake() => Setup();

    // Builds the particle system once (Awake does it in play; Create calls it too so it also works where Awake does not run).
    public void Setup()
    {
        if (system != null)
            return;
        if (template == null)
            template = Resources.Load<Material>("KilnSmoke");

        var go = gameObject;
        go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f); // the cone points along +Z; turned so it points up
        system = go.AddComponent<ParticleSystem>();
        system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = system.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        if (template != null)
        {
            material = new Material(template); // an instance, so changing the opacity never writes into the asset
            renderer.sharedMaterial = material;
        }

        Apply();
    }

    // Pushes every field onto the particle system and the material.
    public void Apply()
    {
        if (system == null)
            return;

        transform.localPosition = new Vector3(0f, height, 0f);

        ParticleSystem.MainModule main = system.main;
        main.startLifetime = lifetimeSeconds;
        main.startSpeed = riseSpeed;
        main.startSize = new ParticleSystem.MinMaxCurve(startSizeMin, startSizeMax);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.maxParticles = maxPuffs;
        main.startColor = tint;

        ParticleSystem.EmissionModule emission = system.emission;
        emission.rateOverTime = puffsPerSecond;
        emission.enabled = firing;

        ParticleSystem.ShapeModule shape = system.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 4f;
        shape.radius = spreadRadius;

        ParticleSystem.NoiseModule noise = system.noise;
        noise.enabled = true;
        noise.strength = driftStrength;
        noise.frequency = driftFrequency;
        noise.quality = ParticleSystemNoiseQuality.Low;
        noise.damping = true;

        ParticleSystem.RotationOverLifetimeModule spin = system.rotationOverLifetime;
        spin.enabled = true;
        spin.z = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);

        // Grows as it rises, then thins away: the shrink at the end stands in for fading, because HDRP Unlit ignores the particle's own alpha.
        float peak = Mathf.Max(0.01f, growthMultiplier);
        float shrinkAt = Mathf.Clamp(1f - fadeOutFraction, 0.1f, 0.95f);
        var sizeCurve = new AnimationCurve(new Keyframe(0f, 0.2f), new Keyframe(Mathf.Clamp(fadeInFraction, 0.05f, 0.5f), peak * 0.35f), new Keyframe(shrinkAt, peak), new Keyframe(1f, 0f));
        for (int i = 0; i < sizeCurve.length; i++)
            sizeCurve.SmoothTangents(i, 0f);
        ParticleSystem.SizeOverLifetimeModule size = system.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var fade = new Gradient();
        fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                     new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, fadeInFraction), new GradientAlphaKey(1f, 1f - fadeOutFraction), new GradientAlphaKey(0f, 1f) });
        ParticleSystem.ColorOverLifetimeModule colour = system.colorOverLifetime;
        colour.enabled = true;
        colour.color = fade;

        if (material != null)
        {
            Color c = new Color(tint.r, tint.g, tint.b, opacity);
            material.SetColor("_UnlitColor", c);
            material.SetColor("_Color", c);
            material.SetTexture("_UnlitColorMap", PuffTexture());
            material.SetTexture("_MainTex", PuffTexture());
        }
    }

    // On while the firing runs; off when it ends (the puffs already up finish their rise).
    public void SetFiring(bool on)
    {
        firing = on;
        if (system == null)
            return;
        ParticleSystem.EmissionModule emission = system.emission;
        emission.enabled = on;
        if (on && !system.isPlaying)
            system.Play();
        else if (!on && system.isPlaying)
            system.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public bool IsFiring => firing;

    // The Editor re-applies the fields every frame so they can be tuned live; a build applies them once.
#if UNITY_EDITOR
    void Update() => Apply();
#endif
}
