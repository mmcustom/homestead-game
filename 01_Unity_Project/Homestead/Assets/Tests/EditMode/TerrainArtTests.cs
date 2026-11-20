using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// The terrain art (Mike, 2026-11-19): the Grass, Dirt and Gravel layers carry tileable colour, normal and mask maps, and the
// terrain material uses them (normal and mask maps on, height-based blending between layers).
public class TerrainArtTests
{
    [TestCase("Grass", "grass")]
    [TestCase("Dirt", "dirt")]
    [TestCase("Gravel", "gravel")]
    public void EachLayerHasItsArtMaps(string layer, string art)
    {
        var l = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Art/Terrain/" + layer + ".terrainlayer");
        Assert.That(l.diffuseTexture.name, Is.EqualTo("terrain_" + art + "_basecolor"));
        Assert.That(l.normalMapTexture.name, Is.EqualTo("terrain_" + art + "_normal"));
        Assert.That(l.maskMapTexture.name, Is.EqualTo("terrain_" + art + "_mask"));
        Assert.That(((TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(l.normalMapTexture))).textureType, Is.EqualTo(TextureImporterType.NormalMap));
        Assert.That(((TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(l.maskMapTexture))).sRGBTexture, Is.False, "the mask holds data, not colour");
        Assert.That(l.diffuseTexture.wrapMode, Is.EqualTo(TextureWrapMode.Repeat));
    }

    [Test]
    public void TheTerrainMaterialUsesTheNormalAndMaskMapsAndBlendsByHeight()
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Terrain/Property Terrain.mat");
        Assert.That(m.IsKeywordEnabled("_NORMALMAP"), Is.True);
        Assert.That(m.IsKeywordEnabled("_MASKMAP"), Is.True);
        Assert.That(m.IsKeywordEnabled("_TERRAIN_BLEND_HEIGHT"), Is.True);
        Assert.That(m.GetFloat("_EnableHeightBlend"), Is.EqualTo(1f));
    }
}
