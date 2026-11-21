#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Wave 10 store art and teaser (2026-11-20). Editor-only helper for Play Mode: sets the season, hour, weather and snow for a shot, flies a
// camera of its own (the HUD hidden, the player's hands not drawn) and saves stills or a frame sequence to Temp/Marketing. The shot list for
// the teaser is Teaser below, so the teaser can be re-recorded the same way when the world art improves (MarketingCamera.RecordTeaser).
// Nothing here touches a save on purpose, but the game autosaves on its own: back the saves up first and put them back after.
public class MarketingCamera : MonoBehaviour
{
    public class Shot
    {
        public string name;
        public float seconds;
        public int day;            // day of the year (0 = Spring 1)
        public float hour;
        public string weather;     // WeatherType name
        public float snow;         // 0-1 snow cover
        public Vector3 fromPos, fromLook, toPos, toLook;
        public float fov = 50f;
        public string action;      // "chop": wood chips at the look point; "glow": hot structures working
    }

    static MarketingCamera instance;
    Camera cam;
    public static MarketingCamera Instance => instance;

    public static MarketingCamera Ensure()
    {
        if (instance != null)
            return instance;
        var go = new GameObject("Marketing Camera");
        instance = go.AddComponent<MarketingCamera>();
        instance.cam = go.AddComponent<Camera>();
        go.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
        instance.cam.depth = 10;
        instance.cam.nearClipPlane = 0.05f;
        instance.cam.farClipPlane = 2000f;
        instance.cam.cullingMask = ~(1 << 6); // not the Viewmodel
        foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            if (canvas.isRootCanvas)
                canvas.enabled = false;
        foreach (Renderer r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            if (r.name.StartsWith("Marker (Placeholder)")) // discovery markers only; the water surface is also called "(Placeholder)"
                r.enabled = false;
        return instance;
    }

    public void Set(Shot s, float t)
    {
        float e = Mathf.SmoothStep(0f, 1f, t);
        Vector3 pos = Vector3.Lerp(s.fromPos, s.toPos, e);
        Vector3 look = Vector3.Lerp(s.fromLook, s.toLook, e);
        transform.position = pos;
        transform.rotation = Quaternion.LookRotation(look - pos);
        cam.fieldOfView = s.fov;
    }

    // Season, hour, weather and snow for a shot. The clock is held so the light does not move during it.
    public static void World(Shot s)
    {
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        TimeManager time = TimeManager.Instance;
        if (time != null)
        {
            TimeSaveData data = time.CaptureState();
            data.totalDays = s.day;
            data.minuteOfDay = s.hour * 60f;
            time.RestoreState(data);
            time.Pause();
        }
        WeatherManager weather = WeatherManager.Instance;
        if (weather != null && !string.IsNullOrEmpty(s.weather))
            weather.DebugForceWeather((WeatherType)System.Enum.Parse(typeof(WeatherType), s.weather), 48);
        SeasonalGround ground = FindFirstObjectByType<SeasonalGround>();
        if (ground != null)
        {
            ground.DebugSetSnow(s.snow, 96f);
            typeof(SeasonalGround).GetMethod("Repaint", flags).Invoke(ground, new object[] { true });
        }
        SeasonalTrees trees = FindFirstObjectByType<SeasonalTrees>();
        if (trees != null)
            typeof(SeasonalTrees).GetMethod("Apply", flags).Invoke(trees, null);
        bool glow = s.action == "glow";
        foreach (StructureGlow g in FindObjectsByType<StructureGlow>(FindObjectsSortMode.None))
            ForceGlow(g, glow);
        if (glow)
            foreach (PlacedStructure p in FindObjectsByType<PlacedStructure>(FindObjectsSortMode.None))
            {
                PileKind kind = p.State != null ? p.State.kind : PileKind.Kiln;
                if (p.State == null || !(kind == PileKind.Kiln || kind == PileKind.Smithy || kind == PileKind.Bloomery || kind == PileKind.CharcoalPit))
                    continue;
                ForceGlow(StructureGlow.Drive(p.transform, kind, true), true);
                foreach (KilnSmoke smoke in p.GetComponentsInChildren<KilnSmoke>(true))
                    smoke.SetFiring(true);
            }
    }

    static void ForceGlow(StructureGlow g, bool on)
    {
        if (g == null)
            return;
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        typeof(StructureGlow).GetField("on", flags).SetValue(g, on);
        typeof(StructureGlow).GetField("level", flags).SetValue(g, on ? 1f : 0f);
    }

    // A still: warms the shot up for a second (exposure, particles), then saves it at 4K.
    public static void Still(Shot s, string file)
    {
        Busy = true;
        Ensure().StartCoroutine(instance.StillRoutine(s, file));
    }

    IEnumerator StillRoutine(Shot s, string file)
    {
        World(s);
        Set(s, 0.5f);
        for (float t = 0f; t < 3f; t += Time.unscaledDeltaTime)
        {
            Action(s);
            yield return null;
        }
        Directory.CreateDirectory(Path.GetDirectoryName(file));
        ScreenCapture.CaptureScreenshot(file, 2);
        Busy = false;
    }

    public static bool Busy;

    float nextAction;

    void Action(Shot s)
    {
        if (s.action == "chop" && Time.time >= nextAction)
        {
            nextAction = Time.time + 0.9f;
            WorldFx.Blow("axe", s.fromLook, transform.position);
        }
    }

    // A frame sequence for each shot (30 fps, fixed time steps, the game view's size) to <folder>/<shot>.mjpeg.
    public static void Record(List<Shot> shots, string folder)
    {
        Busy = true;
        Ensure().StartCoroutine(instance.RecordRoutine(shots, folder));
    }

    IEnumerator RecordRoutine(List<Shot> shots, string folder)
    {
        Directory.CreateDirectory(folder);
        foreach (Shot s in shots)
        {
            // One file per shot: the frames as JPEGs one after another (an MJPEG stream ffmpeg reads with -f mjpeg).
            string path = Path.Combine(folder, s.name + ".mjpeg");
            if (File.Exists(path))
                File.WriteAllBytes(path, new byte[0]);
            World(s);
            Set(s, 0f);
            Time.captureFramerate = 0;
            for (float t = 0f; t < 2.5f; t += Time.unscaledDeltaTime) // settle: exposure, snow, particles
            {
                Action(s);
                yield return null;
            }
            Time.captureFramerate = 30;
            int frames = Mathf.RoundToInt(s.seconds * 30f);
            for (int i = 0; i < frames; i++)
            {
                Set(s, i / (float)Mathf.Max(1, frames - 1));
                Action(s);
                yield return new WaitForEndOfFrame();
                Texture2D frame = ScreenCapture.CaptureScreenshotAsTexture();
                using (var stream = new FileStream(path, FileMode.Append))
                {
                    byte[] jpg = frame.EncodeToJPG(90);
                    stream.Write(jpg, 0, jpg.Length);
                }
                Destroy(frame);
            }
            Time.captureFramerate = 0;
        }
        Busy = false;
    }
}
#endif
