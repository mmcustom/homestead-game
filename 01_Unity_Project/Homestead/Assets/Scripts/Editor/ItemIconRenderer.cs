using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;

// Wave 4 item icons (2026-11-19): one 128 px transparent PNG per item, rendered from the same art a dropped pile shows (Resources/Art/item_<id>,
// or the tool_<id> art for a Tool with no item art, unlit and empty), so the icons always match the world. Written to Resources/Icons/item_<id>.png
// as Sprites; ItemIcons.For(id) loads them at run time. Re-run from the menu whenever item art changes.
//
// How: a preview scene (Mike's open scene is never touched) with a fixed-exposure volume on its own layer (no tonemapping, bloom, vignette or fog,
// so the colours stay linear), three lights that turn with the camera, and an orthographic camera fitted to the model. Each icon is rendered
// twice, on black and on white, and the alpha is the difference between the two (HDRP's colour buffer does not keep alpha). Rendered at 4x and
// box-filtered down.
public static class ItemIconRenderer
{
    public const string Folder = "Assets/Resources/Icons";
    public const int Size = 128;
    const int Super = 4;
    const int VolumeLayer = 29; // an unused layer (31 is Unity's own preview layer)
    const float Margin = 1.08f;
    public static float ExposureEV = -1.2f;
    public static float LightScale = 1f;
    public static float LastMedian; // the median brightness of the model's own pixels in the last render on black
    public static float LastPeak; // the brightest linear value in the last render on black (to check the exposure)

    [MenuItem("Homestead/Art/Render Item Icons")]
    public static void RenderAllFromMenu()
    {
        int made = RenderAll(out string report);
        int built = RenderBuild(out string buildReport);
        Debug.Log($"Icons: {made} items and {built} Build entries rendered to {Folder}.\n{report}{buildReport}");
    }

    // Renders every item in the database that has art; returns how many were written.
    public static int RenderAll(out string report)
    {
        var ids = new List<string>();
        foreach (ItemDefinition item in ItemDatabase.All)
            ids.Add(item.Id);
        return Render(ids, out report);
    }

    struct Job
    {
        public string file;
        public GameObject prefab;
        public bool tool;
        public float pitch; // 0: the usual view for its shape
    }

    // Item icons, Icons/item_<id>: the item's own art, or its tool art (unlit, empty) for a Tool with no item art.
    public static int Render(IEnumerable<string> ids, out string report)
    {
        var log = new System.Text.StringBuilder();
        var jobs = new List<Job>();
        foreach (string id in ids)
        {
            GameObject prefab = Resources.Load<GameObject>("Art/item_" + id);
            bool tool = prefab == null;
            if (tool)
                prefab = Resources.Load<GameObject>("Art/tool_" + id);
            if (prefab == null)
                log.Append($"{id}: no art, no icon\n");
            else
                jobs.Add(new Job { file = "item_" + id, prefab = prefab, tool = tool });
        }
        return RenderJobs(jobs, log, out report);
    }

    // Build tab icons (Mike, 2026-11-19), Icons/build_<key>: every structure kind from its art (a ... Site from the building it becomes, since that
    // is what is being marked out), the Campfire from its art and the Tripod standing. ItemIcons.ForBuild(entry key) finds them.
    public static int RenderBuild(out string report)
    {
        var log = new System.Text.StringBuilder();
        var jobs = new List<Job>();
        void Add(string key, string artId)
        {
            GameObject prefab = Resources.Load<GameObject>("Art/" + artId);
            if (prefab == null)
                log.Append($"{key}: no art ({artId}), no icon\n");
            else
                jobs.Add(new Job { file = ItemIcons.BuildPrefix.Substring("Icons/".Length) + key, prefab = prefab, tool = true, pitch = SteepRoof(artId) ? 58f : 0f });
        }
        foreach (PileKind kind in System.Enum.GetValues(typeof(PileKind)))
        {
            if (kind == PileKind.Felled)
                continue;
            // A Raised Bed's site becomes a Garden Bed with a frame: its icon is the frame (Gardening GB2).
            Add(kind.ToString().ToLowerInvariant(), kind == PileKind.RaisedBedSite ? "structure_gardenbed_raised" : ArtSwap.StructureId(BuiltFrom(kind)));
        }
        Add("campfire", "effect_campfire");
        Add("tripod", "structure_tripod");
        return RenderJobs(jobs, log, out report);
    }

    // Shelters with one sloped roof read as bare poles from the usual angle (the roof is seen edge-on), so they are seen from higher up.
    static bool SteepRoof(string artId) => artId == "structure_leanto" || artId == "structure_tarpshelter";

    // The kind a site becomes (the Kitchen's roof stage is still the Kitchen); a finished kind is itself.
    static PileKind BuiltFrom(PileKind kind)
    {
        string name = kind.ToString();
        if (name == "KitchenRoofSite") return PileKind.Kitchen;
        if (name == "PotterSite") return PileKind.PotterShed;
        if (WoodManager.IsWalkInSite(kind) || WoodManager.IsBarnSite(kind)) return WellsAndOuthouses.FinishedKind(kind); // and PoleBarnSiteSmall becomes PoleBarnSmall (B1) // HoopHouseSiteSmall becomes HoopHouseSmall (Greenhouses G2), GlassHouseSiteSmall GlassHouseSmall (G3)
        if (name.EndsWith("Site") && System.Enum.TryParse(name.Substring(0, name.Length - 4), out PileKind built))
            return built;
        return kind;
    }

    static int RenderJobs(List<Job> jobs, System.Text.StringBuilder log, out string report)
    {
        Directory.CreateDirectory(Folder);
        Scene scene = EditorSceneManager.NewPreviewScene();
        var written = new List<string>();
        RenderTexture rt = null;
        VolumeProfile profile = null;
        try
        {
            profile = BuildProfile();
            var volumeGo = new GameObject("Icon Volume") { layer = VolumeLayer };
            SceneManager.MoveGameObjectToScene(volumeGo, scene);
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1000f;
            volume.sharedProfile = profile;

            var camGo = new GameObject("Icon Camera");
            SceneManager.MoveGameObjectToScene(camGo, scene);
            var cam = camGo.AddComponent<Camera>();
            cam.scene = scene;
            cam.orthographic = true;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 50f;
            var hd = camGo.GetComponent<HDAdditionalCameraData>() ?? camGo.AddComponent<HDAdditionalCameraData>();
            hd.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
            hd.volumeLayerMask = 1 << VolumeLayer;
            hd.antialiasing = HDAdditionalCameraData.AntialiasingMode.None;

            // Key from the upper left, fill from the right, rim from behind: all relative to the camera, so every icon is lit alike.
            AddLight(camGo.transform, scene, "Key", new Vector3(38f, -40f, 0f), 3.2f);
            AddLight(camGo.transform, scene, "Fill", new Vector3(10f, 55f, 0f), 1.1f);
            AddLight(camGo.transform, scene, "Rim", new Vector3(20f, 170f, 0f), 1.6f);

            int px = Size * Super;
            rt = new RenderTexture(px, px, 24, RenderTextureFormat.ARGBHalf) { antiAliasing = 1 };
            cam.targetTexture = rt;

            foreach (Job job in jobs)
            {
                GameObject prefab = job.prefab;
                bool tool = job.tool;

                var model = (GameObject)Object.Instantiate(prefab);
                model.name = prefab.name;
                SceneManager.MoveGameObjectToScene(model, scene);
                if (tool)
                {
                    Show(model.transform, ArtSwap.StateLit, false);
                    Show(model.transform, ArtSwap.StateUnlit, true);
                    Show(model.transform, "Fill", false);
                }
                Pose(model, cam, job.pitch);
                Color[] onBlack = Shoot(cam, hd, rt, Color.black);
                Color[] onWhite = Shoot(cam, hd, rt, Color.white);
                Object.DestroyImmediate(model);

                string path = $"{Folder}/{job.file}.png";
                File.WriteAllBytes(path, Matte(onBlack, onWhite, px).EncodeToPNG());
                written.Add(path);
            }
        }
        finally
        {
            if (rt != null)
                rt.Release();
            EditorSceneManager.ClosePreviewScene(scene);
            if (profile != null)
                Object.DestroyImmediate(profile);
        }

        AssetDatabase.Refresh();
        foreach (string path in written)
        {
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            if (ti == null)
                continue;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = false;
            ti.maxTextureSize = Size;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
        }
        report = log.ToString();
        return written.Count;
    }

    static VolumeProfile BuildProfile()
    {
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        var exposure = profile.Add<Exposure>(true);
        exposure.mode.value = ExposureMode.Fixed;
        exposure.fixedExposure.value = ExposureEV;
        profile.Add<Tonemapping>(true).mode.value = TonemappingMode.None;
        profile.Add<Bloom>(true).intensity.value = 0f;
        profile.Add<Vignette>(true).intensity.value = 0f;
        profile.Add<Fog>(true).enabled.value = false;
        profile.Add<ColorAdjustments>(true);
        // No sky: otherwise the open scene's sky lights the model (its ambient is far brighter than these lights) and icons change with its time of day.
        var env = profile.Add<VisualEnvironment>(true);
        env.skyType.value = 0;
        env.skyAmbientMode.value = SkyAmbientMode.Dynamic;
        return profile;
    }

    static void AddLight(Transform cam, Scene scene, string name, Vector3 euler, float lux)
    {
        var go = new GameObject(name);
        SceneManager.MoveGameObjectToScene(go, scene);
        go.transform.SetParent(cam, false);
        go.transform.localRotation = Quaternion.Euler(euler);
        var light = go.AddComponent<Light>();
        light.type = LightType.Directional;
        // HDRP's own light data is not added automatically in a preview scene; without it the intensity is not read as lux.
        if (go.GetComponent<HDAdditionalLightData>() == null)
            go.AddComponent<HDAdditionalLightData>();
        light.lightUnit = LightUnit.Lux;
        light.intensity = lux * LightScale;
        light.shadows = LightShadows.Soft;
    }

    // Long things (tools, arrows, poles, rolls) lie across the icon on a diagonal with their broad side to the camera; everything else is
    // seen from a three-quarter view above, steeper for flat piles. The camera is then fitted to the model's mesh bounds.
    static void Pose(GameObject model, Camera cam, float pitchOverride = 0f)
    {
        model.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        Bounds local = MeshBounds(model.transform, Matrix4x4.identity);
        Vector3 s = local.size;
        int[] order = { 0, 1, 2 };
        System.Array.Sort(order, (a, b) => s[b].CompareTo(s[a]));
        bool elongated = s[order[0]] > 2.6f * s[order[1]];

        if (elongated)
        {
            Vector3 L = Axis(order[0]), M = Axis(order[1]), S = Vector3.Cross(L, M); // right-handed, so the result is a proper rotation
            // Local longest -> the screen diagonal, middle -> across it, shortest -> along the view (world Z), then a small tilt to show depth.
            var from = new Matrix4x4(); from.SetColumn(0, L); from.SetColumn(1, M); from.SetColumn(2, S); from.SetColumn(3, new Vector4(0, 0, 0, 1));
            var to = new Matrix4x4(); to.SetColumn(0, new Vector3(1, 0, 0)); to.SetColumn(1, new Vector3(0, 1, 0)); to.SetColumn(2, new Vector3(0, 0, 1)); to.SetColumn(3, new Vector4(0, 0, 0, 1));
            Quaternion align = (to * from.transpose).rotation;
            model.transform.rotation = Quaternion.Euler(0f, 0f, 38f) * Quaternion.AngleAxis(-22f, Vector3.right) * align;
            cam.transform.rotation = Quaternion.identity; // looking along +Z
        }
        else
        {
            // Art faces Unity +Z, so the camera looks back at it from the front-left. Open containers of liquid are seen steeply so the liquid shows
            // (it is all that tells the water grades and the sap apart); flat piles a little less; everything else at 34 degrees.
            bool flat = s.y < 0.35f * Mathf.Max(s.x, s.z);
            float pitch = pitchOverride > 0f ? pitchOverride : ShowsLiquid(model.name) ? 58f : flat ? 50f : 34f;
            cam.transform.rotation = Quaternion.Euler(pitch, 148f, 0f);
        }

        Matrix4x4 toCam = cam.transform.worldToLocalMatrix;
        Bounds view = MeshBounds(model.transform, toCam);
        float half = Mathf.Max(view.extents.x, view.extents.y) * Margin;
        cam.orthographicSize = Mathf.Max(half, 0.005f);
        Vector3 centre = cam.transform.TransformPoint(new Vector3(view.center.x, view.center.y, view.min.z - 2f));
        cam.transform.position = centre;
        cam.farClipPlane = view.size.z + 4f;
    }

    static bool ShowsLiquid(string prefabName) =>
        prefabName.StartsWith("item_water") && prefabName != "item_water" && !prefabName.StartsWith("item_water_crock") || prefabName.StartsWith("item_maple_sap") || prefabName.StartsWith("item_goat_milk");

    static Vector3 Axis(int i) => i == 0 ? Vector3.right : i == 1 ? Vector3.up : Vector3.forward;

    // The bounds, in the given space, of every shown mesh's own bounding box corners (meshes need not be readable).
    static Bounds MeshBounds(Transform root, Matrix4x4 space)
    {
        var b = new Bounds();
        bool any = false;
        foreach (MeshFilter mf in root.GetComponentsInChildren<MeshFilter>(false))
        {
            if (mf.sharedMesh == null)
                continue;
            Bounds mb = mf.sharedMesh.bounds;
            Matrix4x4 m = space * mf.transform.localToWorldMatrix;
            for (int i = 0; i < 8; i++)
            {
                Vector3 c = mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                Vector3 p = m.MultiplyPoint3x4(c);
                if (!any) { b = new Bounds(p, Vector3.zero); any = true; }
                else b.Encapsulate(p);
            }
        }
        return b;
    }

    static Color[] Shoot(Camera cam, HDAdditionalCameraData hd, RenderTexture rt, Color background)
    {
        hd.backgroundColorHDR = background;
        cam.Render();
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false, true);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        Color[] px = tex.GetPixels();
        if (background == Color.black)
        {
            LastPeak = 0f;
            var lit = new List<float>();
            foreach (Color c in px) { LastPeak = Mathf.Max(LastPeak, c.maxColorComponent); if (c.maxColorComponent > 0.002f) lit.Add(c.maxColorComponent); }
            lit.Sort();
            LastMedian = lit.Count > 0 ? lit[lit.Count / 2] : 0f;
        }
        Object.DestroyImmediate(tex);
        return px;
    }

    // Alpha from the difference between the black and white renders (scaled by what the two backgrounds actually came out as, read from a
    // corner), colour un-premultiplied from the black one, then box-filtered down by Super in premultiplied space and written as sRGB.
    static Texture2D Matte(Color[] black, Color[] white, int px)
    {
        Color bgB = black[0], bgW = white[0];
        float span = Mathf.Max(1e-4f, ((bgW.r - bgB.r) + (bgW.g - bgB.g) + (bgW.b - bgB.b)) / 3f);
        var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false, false);
        var outPx = new Color[Size * Size];
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                float r = 0, g = 0, bl = 0, a = 0;
                for (int sy = 0; sy < Super; sy++)
                {
                    for (int sx = 0; sx < Super; sx++)
                    {
                        int i = (y * Super + sy) * px + x * Super + sx;
                        Color k = black[i], w = white[i];
                        float diff = ((w.r - k.r) + (w.g - k.g) + (w.b - k.b)) / 3f;
                        float alpha = Mathf.Clamp01(1f - diff / span);
                        // premultiplied colour: what the black render shows minus the background let through
                        r += Mathf.Max(0f, k.r - (1f - alpha) * bgB.r);
                        g += Mathf.Max(0f, k.g - (1f - alpha) * bgB.g);
                        bl += Mathf.Max(0f, k.b - (1f - alpha) * bgB.b);
                        a += alpha;
                    }
                }
                float n = Super * Super;
                a /= n;
                Color c = a > 1e-4f ? new Color(r / n / a, g / n / a, bl / n / a, 1f) : new Color(0, 0, 0, 1f);
                c = new Color(Mathf.Clamp01(c.r), Mathf.Clamp01(c.g), Mathf.Clamp01(c.b)).gamma;
                c.a = a;
                outPx[y * Size + x] = c;
            }
        }
        tex.SetPixels(outPx);
        tex.Apply();
        return tex;
    }

    static void Show(Transform root, string childName, bool on)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == childName)
                t.gameObject.SetActive(on);
    }
}
