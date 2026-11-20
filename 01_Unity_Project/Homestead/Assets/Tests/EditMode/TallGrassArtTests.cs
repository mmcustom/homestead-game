using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// The tall grass (Mike, 2026-11-19): GrassPlacer's clump mesh and Tall Grass.mat carry the Blender clump (Resources/Art/world_tall_grass*).
public class TallGrassArtTests
{
    [Test]
    public void TheClumpIsTheBlenderGrassAtTheOldSize()
    {
        var art = Resources.Load<GameObject>("Art/world_tall_grass_model");
        Assert.That(art, Is.Not.Null);
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Art/Grass/Tall Grass Clump.asset");
        Assert.That(mesh.vertexCount, Is.EqualTo(art.GetComponentInChildren<MeshFilter>().sharedMesh.vertexCount), "applied (Homestead > Art > Apply Tall Grass Art)");
        Assert.That(mesh.uv.Length, Is.EqualTo(mesh.vertexCount), "textured");
        Assert.That(mesh.bounds.min.y, Is.InRange(-0.01f, 0.01f), "rooted on the ground");
        Assert.That(mesh.bounds.max.y, Is.InRange(1.0f, 1.45f), "about the height of its 1.1 m pickup capsule");
    }

    [Test]
    public void TheGrassMaterialIsDoubleSidedSoEachRibbonShowsFromBothSides()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Grass/Tall Grass.mat");
        Assert.That(material.GetTexture("_BaseColorMap").name, Is.EqualTo("world_tall_grass_basecolor"));
        Assert.That(material.GetFloat("_DoubleSidedEnable"), Is.EqualTo(1f));
        Assert.That(material.enableInstancing, Is.True);
    }
}
