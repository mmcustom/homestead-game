using UnityEngine;
using UnityEngine.UI;

// Weather_System.md's Weather Information and Instruments, Layer 1: one small, semi-transparent line under the minimap
// (top-right; the top-left belongs to the F3 debug readout), "14:30 · 54°F". 24-hour time and the outdoor temperature
// in Fahrenheit, from the same shifted value the rest of the game uses. MinimapHud adds this to its own object, so the line
// sits in the minimap's rect and fades with the minimap; nothing in the HUD prefab changes.
public class WeatherHudLine : MonoBehaviour
{
    static readonly Color TextColor = new Color(UiKit.Cream.r, UiKit.Cream.g, UiKit.Cream.b, 0.75f);

    Text label;
    string shown = "";

    void Awake()
    {
        label = UiKit.Text(transform, "Weather Line", "", 22, TextColor, TextAnchor.MiddleCenter);
        RectTransform rt = label.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(250f, 30f);
        rt.anchoredPosition = new Vector2(0f, -6f);
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        var shadow = label.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);
    }

    void Update()
    {
        TimeManager time = TimeManager.Instance;
        WeatherManager weather = WeatherManager.Instance;
        string text = time != null && weather != null ? $"{time.Hour:00}:{time.Minute:00} · {weather.TemperatureF:0}°F" : "";
        if (text != shown)
        {
            shown = text;
            label.text = text;
        }
    }
}
