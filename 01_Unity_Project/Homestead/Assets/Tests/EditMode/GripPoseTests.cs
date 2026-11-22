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
