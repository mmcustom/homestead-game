using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// The world rocks (Mike, 2026-11-19): StonePlacer's Rock A, B, C meshes and Stone.mat carry the Blender rocks (Resources/Art/world_rock_*),
// so every loose stone and outcrop boulder in the scene shows them, and re-running Place Stone keeps them.
public class RockArtTests
{
    [TestCase("Rock A", "a")]
    [TestCase("Rock B", "b")]
    [TestCase("Rock C", "c")]
    public void EachRockMeshIsTheBlenderRockAtTheOldSize(string name, string letter)
    {
        var art = Resources.Load<GameObject>("Art/world_rock_" + letter + "_model");
        Assert.That(art, Is.Not.Null);
        Mesh source = art.GetComponentInChildren<MeshFilter>().sharedMesh;
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Art/Rocks/" + name + ".asset");
        Assert.That(mesh, Is.Not.Null);
        Assert.That(mesh.vertexCount, Is.EqualTo(source.vertexCount), "the art's mesh, applied (Homestead > Art > Apply Rock Art)");
        Assert.That(mesh.uv.Length, Is.EqualTo(mesh.vertexCount), "textured");
        // the size the placer expects: about 1 across, a flat bottom near -0.17, the top near 0.5 (boulders are sunk 30% of their size)
        Assert.That(mesh.bounds.size.x, Is.InRange(0.8f, 1.15f));
        Assert.That(mesh.bounds.min.y, Is.InRange(-0.2f, -0.14f));
        Assert.That(mesh.bounds.max.y, Is.InRange(0.42f, 0.6f));
    }

    [Test]
    public void TheStoneMaterialIsTheBakedLimestone()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Rocks/Stone.mat");
        Assert.That(material.GetTexture("_BaseColorMap"), Is.Not.Null);
        Assert.That(material.GetTexture("_BaseColorMap").name, Is.EqualTo("world_rock_stone_basecolor"));
        Assert.That(material.GetTexture("_NormalMap"), Is.Not.Null);
        Assert.That(material.enableInstancing, Is.True, "hundreds of stones share it");
    }
}
