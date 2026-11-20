using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// The ground wood (Mike, 2026-11-19): GroundWoodPlacer's stick bundles, fallen branches and Ground Wood.mat carry the Blender
// pieces (Resources/Art/world_stick_bundle_*, world_fallen_branch_*, world_ground_wood), so every piece in the scene shows them.
public class GroundWoodArtTests
{
    [TestCase("Stick Bundle A")]
    [TestCase("Stick Bundle B")]
    [TestCase("Stick Bundle C")]
    [TestCase("Fallen Branch A")]
    [TestCase("Fallen Branch B")]
    [TestCase("Fallen Branch C")]
    public void EachPieceIsTheBlenderWoodOnTheGround(string name)
    {
        var art = Resources.Load<GameObject>("Art/world_" + name.ToLowerInvariant().Replace(' ', '_') + "_model");
        Assert.That(art, Is.Not.Null);
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Art/Wood/" + name + ".asset");
        Assert.That(mesh.vertexCount, Is.EqualTo(art.GetComponentInChildren<MeshFilter>().sharedMesh.vertexCount), "applied (Homestead > Art > Apply Ground Wood Art)");
        Assert.That(mesh.uv.Length, Is.EqualTo(mesh.vertexCount), "textured");
        Assert.That(mesh.bounds.min.y, Is.InRange(-0.02f, 0.02f), "lying on the ground");
        Assert.That(mesh.bounds.max.y, Is.InRange(0.1f, 0.3f), "low");
        if (name.StartsWith("Fallen"))
            Assert.That(mesh.bounds.size.x, Is.GreaterThan(mesh.bounds.size.z * 2f), "a branch lies along X, where its pickup box runs");
    }

    [Test]
    public void TheGroundWoodMaterialIsTheBakedWood()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Wood/Ground Wood.mat");
        Assert.That(material.GetTexture("_BaseColorMap").name, Is.EqualTo("world_ground_wood_basecolor"));
        Assert.That(material.GetTexture("_NormalMap"), Is.Not.Null);
        Assert.That(material.enableInstancing, Is.True);
    }
}
