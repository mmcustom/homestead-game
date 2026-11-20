using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// The runtime ground layers' art (Mike, 2026-11-19): snow and leaf litter (SeasonalGround), wet clay and creek sand
// (CreekBanks) take tileable colour, normal and mask maps from Resources/Art/ground_<id>_* through GroundLayerArt.
public class GroundLayerArtTests
{
    static readonly string[] Ids = { "snow", "leaf_litter", "wet_clay", "creek_sand" };

    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).FirstOrDefault(t => t != null);

    static bool Apply(TerrainLayer layer, string id) =>
        (bool)Find("GroundLayerArt").GetMethod("Apply").Invoke(null, new object[] { layer, id });

    [Test]
    public void EveryGroundLayerGetsItsColourNormalAndMaskMaps()
    {
        foreach (string id in Ids)
        {
            var layer = new TerrainLayer { name = "test", tileSize = new Vector2(3f, 3f) };
            try
            {
                Assert.That(Apply(layer, id), Is.True, id);
                Assert.That(layer.diffuseTexture, Is.Not.Null, id);
                Assert.That(layer.normalMapTexture, Is.Not.Null, id);
                Assert.That(layer.maskMapTexture, Is.Not.Null, id);
                Assert.That(layer.tileSize, Is.EqualTo(new Vector2(3f, 3f)), "the caller's tile size stays");
                Assert.That(layer.name, Is.EqualTo("test"), "the caller's name stays (the minimap and save code look layers up by name)");
                Assert.That(layer.maskMapRemapMax, Is.EqualTo(Vector4.one));
                Assert.That((bool)Find("GroundLayerArt").GetMethod("IsArt").Invoke(null, new object[] { layer.diffuseTexture }), Is.True,
                            "so SeasonalGround and CreekBanks never destroy it");
            }
            finally { UnityEngine.Object.DestroyImmediate(layer); }
        }
    }

    [Test]
    public void WithoutArtTheLayerIsLeftAlone()
    {
        var layer = new TerrainLayer { name = "test" };
        try
        {
            Assert.That(Apply(layer, "no_such_ground"), Is.False);
            Assert.That(layer.diffuseTexture, Is.Null);
        }
        finally { UnityEngine.Object.DestroyImmediate(layer); }
    }

    static float MeanHeight(string id)
    {
        // read the PNG itself (the imported texture is not CPU readable)
        string path = $"Assets/Resources/Art/ground_{id}_mask.png";
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
        try
        {
            Assert.That(texture.LoadImage(System.IO.File.ReadAllBytes(path)), Is.True, path);
            Color32[] pixels = texture.GetPixels32();
            double sum = 0;
            for (int i = 0; i < pixels.Length; i += 37)
                sum += pixels[i].b;
            return (float)(sum / Math.Ceiling(pixels.Length / 37.0) / 255.0);
        }
        finally { UnityEngine.Object.DestroyImmediate(texture); }
    }

    [Test]
    public void SnowIsTheTallestLayerSoItWinsTheTerrainsHeightBlend()
    {
        float snow = MeanHeight("snow");
        foreach (string id in new[] { "leaf_litter", "wet_clay", "creek_sand" })
            Assert.That(snow, Is.GreaterThan(MeanHeight(id) + 0.1f), id);
        foreach (string layer in new[] { "Grass", "Dirt", "Gravel" })
        {
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TerrainLayer>($"Assets/Art/Terrain/{layer}.terrainlayer");
            if (asset == null || asset.maskMapTexture == null) continue;
            string png = UnityEditor.AssetDatabase.GetAssetPath(asset.maskMapTexture);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            texture.LoadImage(System.IO.File.ReadAllBytes(png));
            Color32[] pixels = texture.GetPixels32();
            double sum = 0; int n = 0;
            for (int i = 0; i < pixels.Length; i += 37) { sum += pixels[i].b; n++; }
            UnityEngine.Object.DestroyImmediate(texture);
            Assert.That(snow, Is.GreaterThan((float)(sum / n / 255.0)), "snow sits over " + layer);
        }
    }

    [Test]
    public void TheCallersUseTheArtAndNeverDestroyIt()
    {
        string ground = System.IO.File.ReadAllText("Assets/Scripts/World/SeasonalGround.cs");
        Assert.That(ground, Does.Contain("GroundLayerArt.LeafLitter").And.Contain("GroundLayerArt.Snow"));
        Assert.That(ground, Does.Contain("if (!GroundLayerArt.IsArt(layer.diffuseTexture))"));
        string banks = System.IO.File.ReadAllText("Assets/Scripts/World/CreekBanks.cs");
        Assert.That(banks, Does.Contain("GroundLayerArt.WetClay").And.Contain("GroundLayerArt.CreekSand"));
        Assert.That(banks.Split(new[] { "if (!GroundLayerArt.IsArt(layer.diffuseTexture))" }, StringSplitOptions.None).Length - 1, Is.EqualTo(2),
                    "both places CreekBanks discards a texture check first");
    }
}
