using UnityEngine;

// Plants that sway in the wind (Mike, 2026-11-19). The tall grass, the trees' bark, leaves and snow, and the understory
// shrubs use the Homestead/HomesteadWindLit shader graph, which bends each plant downwind (more with height, so the base
// stays planted) in slow gusting waves, with a small flutter on leaves. This feeds it the weather: every frame it sets the
// global _HomesteadWind to (x, z of the direction the wind blows toward, strength 0-1, unused) from
// WeatherManager.WindDirection and WindStrength, eased so a change of weather builds up over a few seconds.
// Without a WeatherManager (menus, the editor) it is a light breeze from the west.
public static class WindShader
{
    public static readonly int WindId = Shader.PropertyToID("_HomesteadWind");
    public const float CalmBearing = 270f;   // from the west
    public const float CalmStrength = 0.25f;
    public const float EaseSeconds = 4f;

    // WeatherManager's bearing is where the wind blows FROM (0 = north, +Z; 90 = east, +X); the plants lean the other way.
    public static Vector2 Toward(float bearingFrom)
    {
        float r = bearingFrom * Mathf.Deg2Rad;
        return new Vector2(-Mathf.Sin(r), -Mathf.Cos(r));
    }

    public static Vector4 Pack(float bearingFrom, float strength)
    {
        Vector2 d = Toward(bearingFrom);
        return new Vector4(d.x, d.y, Mathf.Clamp01(strength), 0f);
    }

    public static Vector4 Calm => Pack(CalmBearing, CalmStrength);

    // One step of the easing toward the weather's wind (direction re-normalised, so a swing round doesn't go still).
    public static Vector4 Ease(Vector4 current, Vector4 target, float deltaTime)
    {
        float k = 1f - Mathf.Exp(-Mathf.Max(0f, deltaTime) / EaseSeconds);
        Vector4 v = Vector4.Lerp(current, target, k);
        var d = new Vector2(v.x, v.y);
        d = d.sqrMagnitude > 1e-6f ? d.normalized : new Vector2(target.x, target.y);
        return new Vector4(d.x, d.y, v.z, 0f);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void StartDriver()
    {
        Shader.SetGlobalVector(WindId, Calm);
        var go = new GameObject("Wind Shader Driver") { hideFlags = HideFlags.HideAndDontSave };
        Object.DontDestroyOnLoad(go);
        go.AddComponent<WindShaderDriver>();
    }

#if UNITY_EDITOR
    // A light breeze in the editor too, so the scene view shows the plants as they'll look.
    [UnityEditor.InitializeOnLoadMethod]
    static void EditorBreeze() => Shader.SetGlobalVector(WindId, Calm);
#endif
}

public class WindShaderDriver : MonoBehaviour
{
    Vector4 current = WindShader.Calm;

    void Update()
    {
        WeatherManager weather = WeatherManager.Instance;
        Vector4 target = weather != null ? WindShader.Pack(weather.WindDirection, weather.WindStrength) : WindShader.Calm;
        current = WindShader.Ease(current, target, Time.deltaTime);
        Shader.SetGlobalVector(WindShader.WindId, current);
    }
}
