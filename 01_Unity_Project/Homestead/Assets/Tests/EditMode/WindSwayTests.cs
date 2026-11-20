using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// Plants that sway in the wind (Mike, 2026-11-19): the Homestead/HomesteadWindLit shader graph on the grass and tree materials,
// fed by WindShaderDriver from the weather's wind.
public class WindSwayTests
{
    const string ShaderPath = "Assets/Shaders/HomesteadWindLit.shadergraph";

    static Type Find(string name)
    {
        foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type t = a.GetType(name);
            if (t != null)
                return t;
        }
        throw new Exception(name + " not found");
    }

    [Test]
    public void TheWindShaderCompilesWithTheLitNamesTheSeasonsUse()
    {
        var shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
        Assert.That(shader, Is.Not.Null);
        foreach (ShaderMessage m in ShaderUtil.GetShaderMessages(shader))
            Assert.That(m.severity, Is.Not.EqualTo(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error), m.message);
        foreach (string p in new[] { "_BaseColor", "_BaseColorMap", "_NormalMap", "_MaskMap", "_AlphaCutoff", "_Smoothness", "_WindSway", "_WindHeight", "_WindFlutter" })
            Assert.That(shader.FindPropertyIndex(p), Is.GreaterThanOrEqualTo(0), p + " (SeasonalTrees sets _BaseColor and _AlphaCutoff)");
    }

    [TestCase("Assets/Art/Grass/Tall Grass.mat")]
    [TestCase("Assets/Resources/Art/world_tall_grass.mat")]
    [TestCase("Assets/Art/Terrain/Bark.mat")]
    [TestCase("Assets/Art/Terrain/Leaves.mat")]
    [TestCase("Assets/Art/Terrain/Shrub Leaves.mat")]
    [TestCase("Assets/Art/Terrain/Tree Snow.mat")]
    public void EachPlantMaterialSways(string path)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Assert.That(material.shader, Is.EqualTo(AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath)));
        Assert.That(material.GetFloat("_WindSway"), Is.GreaterThan(0f));
    }

    [Test]
    public void TheTreePartsBendTogetherSoLeavesStayOnTheBranches()
    {
        var bark = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Terrain/Bark.mat");
        foreach (string name in new[] { "Leaves", "Shrub Leaves", "Tree Snow" })
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Terrain/" + name + ".mat");
            Assert.That(m.GetFloat("_WindHeight"), Is.EqualTo(bark.GetFloat("_WindHeight")), name);
            Assert.That(m.GetFloat("_WindSway"), Is.EqualTo(bark.GetFloat("_WindSway")), name);
            Assert.That(m.GetFloat("_AlphaCutoffEnable"), Is.EqualTo(1f), name + ": the seasons thin leaves and snow by alpha cutoff");
        }
        Assert.That(bark.GetFloat("_WindFlutter"), Is.EqualTo(0f), "wood doesn't flutter");
    }

    [Test]
    public void TheWindBlowsAwayFromItsBearingAndEasesIn()
    {
        Type wind = Find("WindShader");
        MethodInfo toward = wind.GetMethod("Toward"), pack = wind.GetMethod("Pack"), ease = wind.GetMethod("Ease");
        var west = (Vector2)toward.Invoke(null, new object[] { 270f });
        Assert.That(west.x, Is.EqualTo(1f).Within(1e-4f), "a west wind blows toward the east (+X)");
        var north = (Vector2)toward.Invoke(null, new object[] { 0f });
        Assert.That(north.y, Is.EqualTo(-1f).Within(1e-4f), "a north wind blows toward the south (-Z)");
        var calm = (Vector4)pack.Invoke(null, new object[] { 270f, 0.2f });
        var gale = (Vector4)pack.Invoke(null, new object[] { 0f, 0.9f });
        var step = (Vector4)ease.Invoke(null, new object[] { calm, gale, 1f });
        Assert.That(step.z, Is.InRange(0.2f, 0.9f), "a change of weather builds up, not at once");
        Assert.That(new Vector2(step.x, step.y).magnitude, Is.EqualTo(1f).Within(1e-3f));
        Vector4 v = calm;
        for (int i = 0; i < 60; i++) v = (Vector4)ease.Invoke(null, new object[] { v, gale, 1f });
        Assert.That(v.z, Is.EqualTo(0.9f).Within(0.01f), "and gets there");
    }
}
