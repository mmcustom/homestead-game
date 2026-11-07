using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for the see-through sleeve fix (Mike, 2026-11-05). The sleeves showed the wall of the Small Cabin through them because HDRP's forward Lit shader multiplies in the screen-space
// ambient occlusion texture (computed from the WORLD's depth) at every pixel the hands cover. ViewmodelPass now gives the hands a white one while it draws them. The pass itself needs a render to
// run, so these tests pin what can be pinned: the texture name, the line and where it sits in the pass, that every viewmodel renderer (hands, tools, a prefab) is on the layer the pass draws, and
// that no HDRP asset, Volume profile or ProjectSettings file was touched. What they cannot show: the frame itself (frames were taken in the real cabin in Play Mode).
public class ViewmodelAmbientOcclusionTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static Type Art => Find("ArtSwap");
    static Type HandsType => Find("PlayerHands");
    static Type PassType => Find("ViewmodelPass");
    static int ViewLayer => LayerMask.NameToLayer("Viewmodel");

    readonly List<GameObject> made = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject go in made)
            if (go != null)
                UnityEngine.Object.DestroyImmediate(go);
        made.Clear();
        Art.GetMethod("ResetForTests").Invoke(null, null);
    }

    GameObject Make(string name)
    {
        var go = new GameObject(name);
        made.Add(go);
        return go;
    }

    static string Source(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative));

    [Test]
    public void ThePassNamesHDRPsAmbientOcclusionTextureAndHandsItAWhiteOneBeforeDrawingTheHands()
    {
        Assert.That(PassType.GetField("AmbientOcclusionTexture").GetRawConstantValue(), Is.EqualTo("_AmbientOcclusionTexture"), "the global texture HDRP's Lit shader reads");
        string pass = Source("Scripts/Player/ViewmodelPass.cs").Replace("\r\n", "\n");
        int white = pass.IndexOf("ctx.cmd.SetGlobalTexture(AmbientOcclusionTexture, TextureXR.GetWhiteTexture());", StringComparison.Ordinal);
        int draw = pass.IndexOf("CustomPassUtils.RenderFromCamera(", StringComparison.Ordinal);
        Assert.That(white, Is.GreaterThan(0), "the white texture is set");
        Assert.That(draw, Is.GreaterThan(white), "and before the hands are drawn, not after");
        Assert.That(pass.IndexOf("if (!draw || viewCamera == null", StringComparison.Ordinal), Is.LessThan(white), "only when the hands are actually being drawn: the world's frames are never touched");
    }

    [Test]
    public void TheFixIsInThePassSoEveryRendererOnTheViewmodelLayerIsCoveredWhateverItIs()
    {
        Art.GetMethod("ResetForTests").Invoke(null, null);
        GameObject prefab = Make("tool_stand_in");
        prefab.SetActive(false);
        GameObject.CreatePrimitive(PrimitiveType.Cube).transform.SetParent(prefab.transform, false);
        Art.GetField("Loader").SetValue(null, (Func<string, GameObject>)(path => path == "Art/tool_axe" ? prefab : null));
        Camera cam = Make("Test Camera").AddComponent<Camera>();
        cam.cullingMask = -1;
        Component hands = Make("Test Player").AddComponent(HandsType);
        HandsType.GetMethod("Build").Invoke(hands, new object[] { cam });
        var rig = (GameObject)HandsType.GetProperty("Rig").GetValue(hands);
        var pass = HandsType.GetProperty("Pass").GetValue(hands);
        Assert.That(pass, Is.Not.Null);
        Assert.That(pass.GetType(), Is.EqualTo(PassType));
        var layerMask = (LayerMask)PassType.GetField("layerMask").GetValue(pass);
        Assert.That(layerMask.value, Is.EqualTo(1 << ViewLayer), "the pass draws exactly the Viewmodel layer");
        foreach (string id in new[] { null, "axe", "recurve_bow", "bucket", "bolt_action_rifle", "torch" })
        {
            HandsType.GetMethod("Apply").Invoke(hands, new object[] { id, 1f, 0f, 0f, HandMotion.Idle });
            foreach (Renderer r in rig.GetComponentsInChildren<Renderer>(true))
                Assert.That(r.gameObject.layer, Is.EqualTo(ViewLayer), (id ?? "empty") + "/" + r.name + ": drawn by the pass, so given the white texture");
        }
    }

    [Test]
    public void NoHdrpAssetVolumeProfileOrProjectSettingWasChangedAndLitShaderModeIsStillDeferredOnly()
    {
        foreach (string file in new[] { "HDRP Balanced.asset", "HDRP High Fidelity.asset", "HDRP Performant.asset" })
            StringAssert.Contains("supportedLitShaderMode: 2", File.ReadAllText(Path.Combine(Application.dataPath, "Settings", file)), file);
        Assert.That(ArtFolderGuard.LeftoverTestArt(), Is.Empty, "no generated test prefab or test material folder is left under Resources/Art");
        string pass = Source("Scripts/Player/ViewmodelPass.cs");
        StringAssert.DoesNotContain("FrameSettings", pass);
        StringAssert.DoesNotContain("VolumeProfile", pass);
        StringAssert.DoesNotContain("AmbientOcclusion>", pass, "the world's occlusion setting is never read or changed");
    }
}
