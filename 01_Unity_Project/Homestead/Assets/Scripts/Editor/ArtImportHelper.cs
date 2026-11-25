using UnityEditor;
using UnityEngine;

// Claude's art pipeline helper (homestead-blender-art skill, step 7): imports one Blender piece from Art/Blender_Work/<id>/ into Resources/Art as <id>.prefab, the way every
// piece so far was imported by hand: copies the FBX and the basecolor, normal and mask maps, sets the importers, makes one HDRP/Lit material per part from tool_axe_wood.mat,
// and builds the prefab (the FBX's children under a root named <id>, materials matched by name). Editor only; nothing in the game calls it.
public static class ArtImportHelper
{
    const string BlenderWork = @"C:\Users\mmcus\OneDrive\Documents\Homestead Game\Art\Blender_Work\";

    public static string Import(string id, string[] parts, bool overwrite = false)
    {
        if (EditorApplication.isPlaying)
            return "PLAYING";
        string src = BlenderWork + id + "\\";
        string dst = "Assets/Resources/Art/";
        if (System.IO.File.Exists(dst + id + ".prefab") && !overwrite) return "prefab exists already";
        System.IO.File.Copy(src + id + "_model.fbx", dst + id + "_model.fbx", true);
        foreach (var p in parts)
            foreach (var k in new[] { "basecolor", "normal", "mask" })
                System.IO.File.Copy(src + "textures\\" + id + "_" + p + "_" + k + ".png", dst + id + "_" + p + "_" + k + ".png", true);
        AssetDatabase.Refresh();
        foreach (var p in parts)
        {
            var n = (TextureImporter)AssetImporter.GetAtPath(dst + id + "_" + p + "_normal.png");
            n.textureType = TextureImporterType.NormalMap; n.SaveAndReimport();
            var m = (TextureImporter)AssetImporter.GetAtPath(dst + id + "_" + p + "_mask.png");
            m.sRGBTexture = false; m.alphaSource = TextureImporterAlphaSource.FromInput; m.SaveAndReimport();
            AssetDatabase.ImportAsset(dst + id + "_" + p + "_basecolor.png", ImportAssetOptions.ForceUpdate);
        }
        var mi = (ModelImporter)AssetImporter.GetAtPath(dst + id + "_model.fbx");
        mi.importAnimation = false; mi.animationType = ModelImporterAnimationType.None; mi.importCameras = false; mi.importLights = false; mi.SaveAndReimport();
        var template = AssetDatabase.LoadAssetAtPath<Material>(dst + "tool_axe_wood.mat");
        System.Reflection.MethodInfo reset = null;
        foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType("UnityEditor.Rendering.HighDefinition.HDShaderUtils");
            if (t == null) continue;
            foreach (var mm in t.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
                if (mm.Name == "ResetMaterialKeywords" && mm.GetParameters().Length == 1) reset = mm;
        }
        var mats = new System.Collections.Generic.Dictionary<string, Material>();
        foreach (var p in parts)
        {
            string path = dst + id + "_" + p + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool fresh = mat == null;
            if (fresh) mat = new Material(template);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetTexture("_BaseColorMap", AssetDatabase.LoadAssetAtPath<Texture2D>(dst + id + "_" + p + "_basecolor.png"));
            mat.SetTexture("_NormalMap", AssetDatabase.LoadAssetAtPath<Texture2D>(dst + id + "_" + p + "_normal.png"));
            mat.SetTexture("_MaskMap", AssetDatabase.LoadAssetAtPath<Texture2D>(dst + id + "_" + p + "_mask.png"));
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_SmoothnessRemapMin", 0f); mat.SetFloat("_SmoothnessRemapMax", 1f);
            mat.SetFloat("_AORemapMin", 0f); mat.SetFloat("_AORemapMax", 1f);
            if (reset != null) reset.Invoke(null, new object[] { mat });
            if (fresh) AssetDatabase.CreateAsset(mat, path); else EditorUtility.SetDirty(mat);
            mats[id + "_" + p] = mat;
        }
        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(dst + id + "_model.fbx");
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
        var inst = (GameObject)UnityEngine.Object.Instantiate(fbx);
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(inst, scene);
        var root = new GameObject(id);
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, scene);
        var kids = new System.Collections.Generic.List<Transform>();
        foreach (Transform c in inst.transform) kids.Add(c);
        if (kids.Count == 0)
        {
            // An FBX with a single mesh and nothing else imports as that mesh itself: it becomes the Body.
            inst.name = "Body";
            inst.transform.SetParent(root.transform, false);
            inst.transform.localPosition = Vector3.zero;
            inst.transform.localRotation = Quaternion.identity;
        }
        else
        {
            foreach (var c in kids) c.SetParent(root.transform, false);
            UnityEngine.Object.DestroyImmediate(inst);
        }
        var report = new System.Text.StringBuilder();
        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
        {
            var shared = r.sharedMaterials;
            for (int i = 0; i < shared.Length; i++)
            {
                string nm = shared[i] != null ? shared[i].name : "";
                if (mats.ContainsKey(nm)) shared[i] = mats[nm]; else report.Append("UNMATCHED " + nm + "; ");
            }
            r.sharedMaterials = shared;
        }
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            var mf = t.GetComponent<MeshFilter>();
            report.Append(t.name + " p" + t.localPosition.ToString("F3") + " r" + t.localEulerAngles.ToString("F0") + (mf != null ? " b" + mf.sharedMesh.bounds.size.ToString("F3") + " c" + mf.sharedMesh.bounds.center.ToString("F3") : "") + " | ");
        }
        PrefabUtility.SaveAsPrefabAsset(root, dst + id + ".prefab");
        UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);
        AssetDatabase.SaveAssets();
        ArtSwap.ResetCache();
        return "reset=" + (reset != null) + " :: " + report.ToString();
    }
}
