using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// The water (Mike, 2026-11-19): Homestead/HomesteadWater on Water.mat, see-through and moving with the flow stored in the
// water mesh's UV1 (x, z direction, z speed m/s).
public class WaterArtTests
{
    [Test]
    public void TheWaterShaderCompilesAndIsOnTheWaterAsSeeThroughWater()
    {
        var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/HomesteadWater.shadergraph");
        Assert.That(shader, Is.Not.Null);
        foreach (ShaderMessage m in ShaderUtil.GetShaderMessages(shader))
            Assert.That(m.severity, Is.Not.EqualTo(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error), m.message);
        var water = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Terrain/Water.mat");
        Assert.That(water.shader, Is.EqualTo(shader));
        Assert.That(water.GetFloat("_SurfaceType"), Is.EqualTo(1f), "transparent");
        Assert.That(water.renderQueue, Is.GreaterThan(2500), "drawn after the opaque world, so it can see the creek bed");
        Assert.That(water.GetTexture("_RippleMap"), Is.Not.Null);
        Assert.That(water.GetTexture("_FoamMap"), Is.Not.Null);
    }

    [Test]
    public void TheCreekFlowsDownstreamAndThePondIsAlmostStill()
    {
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Art/Terrain/PropertyWater.asset");
        var flow = new List<Vector4>(); mesh.GetUVs(1, flow);
        Assert.That(flow.Count, Is.EqualTo(mesh.vertexCount));
        Vector3[] v = mesh.vertices;
        Vector3 spring = v[0], pond = v[65];
        Assert.That(flow[66].z, Is.LessThan(0.1f), "the pond barely drifts");
        // along the creek, the flow points the way the creek goes on toward the pond
        int first = 130, pairs = (v.Length - first) / 2, agree = 0, checkedPairs = 0;
        for (int k = 1; k < pairs - 1; k++)
        {
            Vector3 here = (v[first + k * 2] + v[first + k * 2 + 1]) * 0.5f, next = (v[first + (k + 1) * 2] + v[first + (k + 1) * 2 + 1]) * 0.5f;
            var along = new Vector2(next.x - here.x, next.z - here.z);
            if (along.sqrMagnitude < 1e-4f) continue;
            checkedPairs++;
            if (Vector2.Dot(along.normalized, new Vector2(flow[first + k * 2].x, flow[first + k * 2].y)) > 0.5f) agree++;
            Assert.That(flow[first + k * 2].z, Is.GreaterThan(0.1f), "the creek runs");
        }
        Assert.That(checkedPairs, Is.GreaterThan(20));
        Assert.That(agree, Is.EqualTo(checkedPairs), "every stretch flows on downstream");
        Vector3 last = (v[v.Length - 2] + v[v.Length - 1]) * 0.5f;
        Assert.That(Vector2.Distance(new Vector2(last.x, last.z), new Vector2(pond.x, pond.z)), Is.LessThan(Vector2.Distance(new Vector2(last.x, last.z), new Vector2(spring.x, spring.z))), "the creek ends at the pond");
    }
}
