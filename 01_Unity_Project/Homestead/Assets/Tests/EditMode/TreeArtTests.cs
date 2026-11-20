using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// The tree art (Mike, 2026-11-19): Hardwood Broad is the Blender sugar maple (world_tree_maple), Hardwood Tall the tulip
// poplar (world_tree_poplar) and Understory Shrub a viburnum (world_tree_viburnum), three detail levels on
// LOD children, each with SeasonalTrees' four material slots (0 bark, 1 leaves, 2 crown snow, 3 branch snow). Each leaf
// card carries its own drop value in UV1.x, which the wind shader reads when _UseFaceValue is on.
public class TreeArtTests
{
        static string Prefab(string name) => $"Assets/Prefabs/Environment/{name}.prefab";

    static GameObject Load(string path)
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        Assert.That(go, Is.Not.Null, path);
        return go;
    }

    [TestCase("Hardwood Broad", "maple")]
    [TestCase("Hardwood Tall", "poplar")]
    [TestCase("Understory Shrub", "viburnum")]
    public void TheHardwoodsAreTheArtTreesWithThreeDetailLevels(string name, string species)
    {
        var tree = Load(Prefab(name));
        var group = tree.GetComponent<LODGroup>();
        Assert.That(group, Is.Not.Null);
        LOD[] lods = group.GetLODs();
        Assert.That(lods.Length, Is.EqualTo(3));
        int previous = int.MaxValue;
        for (int i = 0; i < lods.Length; i++)
        {
            Assert.That(lods[i].renderers.Length, Is.EqualTo(1));
            var renderer = (MeshRenderer)lods[i].renderers[0];
            Mesh mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
            int tris = mesh.triangles.Length / 3;
            Assert.That(tris, Is.LessThan(previous), "each detail level is lighter than the one before");
            Assert.That(tris, Is.LessThanOrEqualTo(i == 0 ? 25000 : 5000), i == 0 ? "the art list's tree budget" : "the middle and far levels stay light: thousands of them are drawn");
            previous = tris;
            Assert.That(mesh.subMeshCount, Is.EqualTo(4));
            Material[] mats = renderer.sharedMaterials;
            Assert.That(mats.Length, Is.EqualTo(4));
            Assert.That(mats[0].name, Is.EqualTo($"world_tree_{species}_bark"));
            Assert.That(mats[1].name, Is.EqualTo($"world_tree_{species}_leaves"));
            Assert.That(mats[2].name, Is.EqualTo($"world_tree_{species}_snow_leaves"));
            Assert.That(mats[3].name, Is.EqualTo("Tree Snow"));
        }
        Assert.That(lods[2].renderers[0].GetComponent<MeshFilter>().sharedMesh.triangles.Length / 3, Is.LessThan(1500),
                    "the far level stays close to the old trees' cost (8,800 trees)");
        Assert.That(tree.GetComponentInChildren<MeshRenderer>().sharedMaterials.Length, Is.GreaterThanOrEqualTo(4),
                    "SeasonalTrees finds the slots on the first renderer in the tree");
    }

    [TestCase("Hardwood Broad", 6.5f)]
    [TestCase("Hardwood Tall", 9.5f)]
    public void TheArtTreesKeepTheirTrunkColliderAndSeasonColours(string name, float trunkHeight)
    {
        var tree = Load(Prefab(name));
        var trunk = tree.GetComponent<CapsuleCollider>();
        Assert.That(trunk, Is.Not.Null, "the terrain's tree colliders and felling use it");
        Assert.That(trunk.height, Is.EqualTo(trunkHeight).Within(0.01f));
        var foliage = tree.GetComponent("SeasonalFoliage");
        Assert.That(foliage, Is.Not.Null);
        foreach (string field in new[] { "springColor", "summerColor", "fallColor" })
        {
            var colour = (Color)foliage.GetType().GetField(field).GetValue(foliage);
            Assert.That(colour.a, Is.EqualTo(1f), field + ": the leaves' alpha is their clip value, so only the colour is brightened");
        }
        var leaves = tree.GetComponentInChildren<MeshRenderer>().sharedMaterials[1];
        Assert.That(leaves.GetColor("_BaseColor").a, Is.EqualTo(1f));
    }

    [TestCase("Hardwood Broad")]
    [TestCase("Hardwood Tall")]
    [TestCase("Understory Shrub")]
    public void EveryLeafCardHasOneDropValueAndTheShaderReadsIt(string name)
    {
        var tree = Load(Prefab(name));
        Mesh mesh = tree.transform.Find("LOD0").GetComponent<MeshFilter>().sharedMesh;
        var face = new List<Vector2>(); mesh.GetUVs(1, face);
        Assert.That(face.Count, Is.EqualTo(mesh.vertexCount));
        int[] leaves = mesh.GetTriangles(1);
        Assert.That(leaves.Length / 6, Is.GreaterThan(150), "plenty of leaf cards");
        float low = 1f, high = 0f;
        for (int i = 0; i < leaves.Length; i += 3)
        {
            float a = face[leaves[i]].x, b = face[leaves[i + 1]].x, c = face[leaves[i + 2]].x;
            Assert.That(b, Is.EqualTo(a).Within(1e-4f)); Assert.That(c, Is.EqualTo(a).Within(1e-4f));
            Assert.That(a, Is.InRange(0.01f, 0.99f), "0 cutoff shows them all, 1 hides them all");
            low = Mathf.Min(low, a); high = Mathf.Max(high, a);
        }
        Assert.That(high - low, Is.GreaterThan(0.8f), "values spread, so leaves drop a few at a time");

        var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/HomesteadWindLit.shadergraph");
        foreach (ShaderMessage m in ShaderUtil.GetShaderMessages(shader))
            Assert.That(m.severity, Is.Not.EqualTo(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error), m.message);
        Assert.That(shader.FindPropertyIndex("_UseFaceValue"), Is.GreaterThanOrEqualTo(0));
        var leafMat = tree.GetComponentInChildren<MeshRenderer>().sharedMaterials[1];
        Assert.That(leafMat.GetFloat("_UseFaceValue"), Is.EqualTo(1f));
        Assert.That(leafMat.GetTexture("_BaseColorMap"), Is.Not.Null);
        // grass and the old trees' leaves keep reading their texture's alpha
        var bark = tree.GetComponentInChildren<MeshRenderer>().sharedMaterials[0];
        Assert.That(bark.GetFloat("_UseFaceValue"), Is.EqualTo(0f));
        foreach (string path in new[] { "Assets/Art/Terrain/Tall Grass.mat", "Assets/Art/Terrain/Leaves.mat" })
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null) Assert.That(m.GetFloat("_UseFaceValue"), Is.EqualTo(0f), path);
        }
    }

    [Test]
    public void TheShrubCanStillBeWalkedThroughAndKeepsItsRedFall()
    {
        var shrub = Load(Prefab("Understory Shrub"));
        Assert.That(shrub.GetComponentInChildren<Collider>(), Is.Null, "shrubs have no collider");
        var foliage = shrub.GetComponent("SeasonalFoliage");
        var fall = (Color)foliage.GetType().GetField("fallColor").GetValue(foliage);
        Assert.That(fall.r, Is.GreaterThan(fall.g * 2f), "red in fall");
        Assert.That(fall.a, Is.EqualTo(1f));
    }
}
