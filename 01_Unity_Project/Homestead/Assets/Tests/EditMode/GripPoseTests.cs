using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// Tool grips (Hands v6, 2026-11-22): PlayerHands swaps the grip hand's right meshes for a posed copy while some tools are held (the rifle, the Flint and Steel).
// The game types are in Assembly-CSharp, so they are reached by reflection, as the other hands tests do.
public class GripPoseTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).FirstOrDefault(t => t != null);
    static string PoseFor(string toolId) => (string)Find("PlayerHands").GetMethod("GripPoseFor").Invoke(null, new object[] { toolId });

    [Test]
    public void TheRifleAndTheFlintAndSteelHaveTheirOwnGripAndOtherToolsKeepTheFist()
    {
        Assert.That(PoseFor("bolt_action_rifle"), Is.EqualTo("hands_grip_rifle"));
        Assert.That(PoseFor("flint_and_steel"), Is.EqualTo("hands_grip_flint"));
        foreach (string id in new[] { "axe", "hammer", "knife", "bucket", "clay_pot", "spile", "tarp" })
            Assert.That(PoseFor(id), Is.Null, id);
        Assert.That(PoseFor(null), Is.Null);
    }

    [Test]
    public void TheHandsAndToolsDoNotReceiveDecals()
    {
        // Mike, 2026-11-22: footprints showed on the hands. HDRP decals land in a screen-space buffer that any Lit material receiving
        // decals samples, so the viewmodel's materials (drawn over the world by ViewmodelPass) picked up the footprints behind them.
        int checkedCount = 0;
        foreach (Material m in Resources.LoadAll<Material>("Art"))
        {
            if (!(m.name.StartsWith("hands_") || m.name.StartsWith("tool_")) || !m.HasProperty("_SupportDecals"))
                continue;
            if (!UnityEditor.AssetDatabase.GetAssetPath(m).EndsWith(".mat"))
                continue; // a model's own imported material, never drawn (the prefabs use the .mat files)
            Assert.That(m.GetFloat("_SupportDecals"), Is.EqualTo(0f), m.name);
            Assert.That(m.IsKeywordEnabled("_DISABLE_DECALS"), Is.True, m.name);
            checkedCount++;
        }
        if (checkedCount == 0)
            Assert.Ignore("no hand or tool art in this project");
    }

    [Test]
    public void APosedMeshMatchesThePlainGripMeshItReplaces()
    {
        // Only where the art is in the project (Resources/Art is not committed): each posed part has exactly the vertices of the part it replaces, so the UVs and materials still fit.
        var grip = Resources.Load<GameObject>("Art/hands_player_grip");
        if (grip == null)
            Assert.Ignore("no hands_player_grip art in this project");
        foreach (string pose in new[] { "hands_grip_flint", "hands_grip_rifle" })
            foreach (var (part, suffix) in new[] { ("Skin_R", "skin_R"), ("Nails_R", "nails_R") })
            {
                Mesh posed = Resources.Load<Mesh>("Art/" + pose + "_" + suffix);
                if (posed == null)
                    continue;
                MeshFilter plain = grip.GetComponentsInChildren<MeshFilter>(true).First(f => f.name == part);
                Assert.That(posed.vertexCount, Is.EqualTo(plain.sharedMesh.vertexCount), pose + " " + part);
                Assert.That(posed.subMeshCount, Is.EqualTo(plain.sharedMesh.subMeshCount), pose + " " + part);
            }
    }
}
