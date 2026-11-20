using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// The rest of Wave 6 (Mike, 2026-11-19): weather and fire sprites, forage shapes, the clay patch decal, the night sky
// (stars and a phased moon) and the trees' fall variety. See Art_Needed_List.md: world_fx, world_forage, world_sky.
public class Wave6FinishTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).FirstOrDefault(t => t != null);

    static T Load<T>(string path) where T : UnityEngine.Object
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        Assert.That(asset, Is.Not.Null, path);
        return asset;
    }

    // --- weather and fire ---

    [Test]
    public void RainSnowAndTheBoltUseTheNewSprites()
    {
        var rain = Load<Texture2D>("Assets/Art/Weather/Raindrop.png");
        Assert.That(rain.height, Is.GreaterThanOrEqualTo(128), "the longer, sharper streak");
        Assert.That(Load<Material>("Assets/Art/Weather/Rain.mat").GetTexture("_UnlitColorMap"), Is.EqualTo(rain));
        Assert.That(Load<Material>("Assets/Art/Weather/Snow.mat").GetTexture("_UnlitColorMap"), Is.EqualTo(Load<Texture2D>("Assets/Art/Weather/Snowflake.png")));
        var bolt = Load<Material>("Assets/Art/Weather/Lightning Bolt.mat");
        Assert.That(bolt.GetTexture("_UnlitColorMap"), Is.EqualTo(Load<Texture2D>("Assets/Art/Weather/LightningBolt.png")));
        Assert.That(bolt.GetFloat("_SurfaceType"), Is.EqualTo(1f), "see-through, so the glow fades at its edges");
    }

    [Test]
    public void TheCampfireFlamesPlayTheFlameSheetWithoutSquares()
    {
        var sheet = Load<Texture2D>("Assets/Art/Fire/Flame Sheet.png");
        var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Art/Fire/Flame Sheet.png");
        Assert.That(importer.alphaIsTransparency, Is.False, "premultiplied: colour bled into the clear parts would show each sprite's square");
        foreach (string name in new[] { "Flame Outer", "Flame Core" })
        {
            var material = Load<Material>($"Assets/Art/Fire/{name}.mat");
            Assert.That(material.GetTexture("_UnlitColorMap"), Is.EqualTo(sheet), name);
            Assert.That(material.GetTexture("_EmissiveColorMap"), Is.EqualTo(sheet), name + ": the old round glow as emission drew a square on every flame");
        }
        var campfire = Load<GameObject>("Assets/Prefabs/Campfire.prefab");
        foreach (ParticleSystem system in campfire.GetComponentsInChildren<ParticleSystem>(true))
        {
            if (system.name != "Flames" && system.name != "Core")
                continue;
            ParticleSystem.TextureSheetAnimationModule frames = system.textureSheetAnimation;
            Assert.That(frames.enabled && frames.numTilesX == 4 && frames.numTilesY == 4, Is.True, system.name);
        }
    }

    [Test]
    public void KilnSmokeUsesTheBillowyPuff()
    {
        var puff = Resources.Load<Texture2D>("Art/fx_smoke_puff");
        Assert.That(puff, Is.Not.Null);
        MethodInfo make = Find("KilnSmoke").GetMethod("PuffTexture", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(make.Invoke(null, null), Is.EqualTo(puff));
    }

    // --- forage and clay ---

    [Test]
    public void EveryForageCropHasAShapeThatFitsItsPrimitive()
    {
        var meshes = AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/Art/forage_crop_model.fbx").OfType<Mesh>().ToDictionary(m => m.name);
        string[] spheres = { "blackberries", "raspberries", "wild_apples", "pawpaw", "hickory_nuts", "walnuts", "chestnuts",
                             "dandelion", "wild_onion", "morel", "dryads_saddle", "oyster_mushroom", "Dandelion Leaves" };
        foreach (string name in spheres)
        {
            Assert.That(meshes.ContainsKey(name), Is.True, name);
            Assert.That(meshes[name].bounds.size.x, Is.EqualTo(1f).Within(0.02f), name);
            Assert.That(meshes[name].bounds.size.y, Is.EqualTo(1f).Within(0.02f), name);
        }
        foreach (string name in new[] { "cattail", "Fallen Log" })
            Assert.That(meshes[name].bounds.size.y, Is.EqualTo(2f).Within(0.02f), name + " replaces a 2 m tall cylinder");
        MethodInfo art = Find("ForagePlacer").GetMethod("ArtMesh");
        Assert.That(art.Invoke(null, new object[] { "blackberries" }), Is.EqualTo(meshes["blackberries"]));
        Assert.That(art.Invoke(null, new object[] { "Cattail Stem" }), Is.Null, "thin stems stay primitives");
    }

    [Test]
    public void TheClayPatchIsADecalReadyToPlace()
    {
        var prefab = Load<GameObject>("Assets/Resources/Art/world_clay_patch.prefab");
        Component projector = prefab.GetComponent("DecalProjector");   // HDRP's, reached by name (the test assembly has no HDRP reference)
        Assert.That(projector, Is.Not.Null);
        var material = (Material)projector.GetType().GetProperty("material").GetValue(projector);
        Assert.That(material.shader.name, Is.EqualTo("HDRP/Decal"));
        Assert.That(material.GetTexture("_BaseColorMap"), Is.Not.Null);
        Assert.That(Vector3.Dot(prefab.transform.forward, Vector3.down), Is.GreaterThan(0.99f), "projects straight down");
    }

    // --- the sky ---

    [Test]
    public void TheNightSkyHasStars()
    {
        var profile = Load<ScriptableObject>("Assets/Settings/WorldSkyAndFog.asset");
        var components = (System.Collections.IEnumerable)profile.GetType().GetField("components").GetValue(profile);
        object sky = components.Cast<object>().FirstOrDefault(c => c.GetType().Name == "PhysicallyBasedSky");
        Assert.That(sky, Is.Not.Null);
        object parameter = sky.GetType().GetField("spaceEmissionTexture").GetValue(sky);
        Assert.That((bool)parameter.GetType().GetProperty("overrideState").GetValue(parameter), Is.True);
        Assert.That(parameter.GetType().GetProperty("value").GetValue(parameter), Is.EqualTo(Load<Cubemap>("Assets/Art/Sky/sky_stars.png")));
    }

    [Test]
    public void TheMoonIsDrawnAndWaxesAndWanesWithTheCalendar()
    {
        MethodInfo phase = Find("DayNightCycle").GetMethod("MoonPhase");
        Func<float, float> at = days => (float)phase.Invoke(null, new object[] { days, 29.5f });
        Assert.That(at(0f), Is.EqualTo(0.5f).Within(1e-4f), "a new game starts on a full moon");
        Assert.That(Mathf.Min(at(14.75f), 1f - at(14.75f)), Is.LessThan(1e-3f), "new moon half a cycle later");
        Assert.That(at(29.5f), Is.EqualTo(0.5f).Within(1e-4f), "and full again");
        // the scene's Moon light is a textured celestial body with manual phase (checked in the saved scene file)
        string guid = AssetDatabase.AssetPathToGUID("Assets/Art/Sky/sky_moon_surface.png");
        string world = System.IO.File.ReadAllText("Assets/Scenes/World.unity");
        Assert.That(world, Does.Contain(guid), "the Moon's surface texture");
        string source = System.IO.File.ReadAllText("Assets/Scripts/World/DayNightCycle.cs");
        Assert.That(source, Does.Contain("moonData.moonPhase = MoonPhase("));
    }

    // --- the trees' fall variety ---

    [Test]
    public void APartOfEachTreeKindTurnsASecondFallColour()
    {
        var shader = Load<Shader>("Assets/Shaders/HomesteadWindLit.shadergraph");
        Assert.That(shader.FindPropertyIndex("_FallAltColor"), Is.GreaterThanOrEqualTo(0));
        Assert.That(shader.FindPropertyIndex("_FallMix"), Is.GreaterThanOrEqualTo(0));
        foreach (string name in new[] { "Hardwood Broad", "Hardwood Tall", "Understory Shrub" })
        {
            var tree = Load<GameObject>($"Assets/Prefabs/Environment/{name}.prefab");
            Component foliage = tree.GetComponent("SeasonalFoliage");
            Color fall = (Color)foliage.GetType().GetField("fallColor").GetValue(foliage);
            Color alt = (Color)foliage.GetType().GetField("fallAltColor").GetValue(foliage);
            Assert.That(alt.a, Is.EqualTo(1f), name);
            Assert.That(Mathf.Abs(alt.r - fall.r) + Mathf.Abs(alt.g - fall.g) + Mathf.Abs(alt.b - fall.b), Is.GreaterThan(0.15f), name + ": a visibly different colour");
            var leaves = tree.GetComponentInChildren<MeshRenderer>().sharedMaterials[1];
            Assert.That(leaves.GetFloat("_FallMix"), Is.EqualTo(0f), name + ": no variety outside Fall (SeasonalTrees raises it in play)");
        }
        Assert.That(System.IO.File.ReadAllText("Assets/Scripts/World/SeasonalTrees.cs"), Does.Contain("SetFloat(\"_FallMix\""));
    }
}
