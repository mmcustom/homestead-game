using UnityEngine;
using UnityEngine.UI;

// Weather_System.md's Weather Information and Instruments, Layer 2: the free Weather tab (K). A short list of labelled rows in plain
// words. It reads the same player-facing name and feels-like text as the pause date line, and never names a Heat Wave: the
// player works it out from the temperature, the feels-like line and the strong sun. Nothing changes while it is open (the
// clock is paused), so the rows are filled once per OnShow.
public class WeatherScreen : GameScreen
{
    const float RowHeight = 62f;

    Text conditions, precipitation, temperature, wind, sunlight, date, daylight;

    public override string Title => "Weather";

    public override void Build(RectTransform area)
    {
        RectTransform column = UiKit.Rect("Rows", area);
        column.anchorMin = new Vector2(0.5f, 1f);
        column.anchorMax = new Vector2(0.5f, 1f);
        column.pivot = new Vector2(0.5f, 1f);
        column.sizeDelta = new Vector2(1000f, 7 * RowHeight + 6 * 6f);
        column.anchoredPosition = new Vector2(0f, -24f);
        var layout = column.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 6f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        conditions = Row(column, "Conditions");
        precipitation = Row(column, "Precipitation");
        temperature = Row(column, "Temperature");
        wind = Row(column, "Wind");
        sunlight = Row(column, "Sunlight");
        date = Row(column, "Date");
        daylight = Row(column, "Daylight");
    }

    static Text Row(RectTransform column, string label)
    {
        Image row = UiKit.Image(column, label + " Row", new Color(0f, 0f, 0f, 0.2f));
        UiKit.Height(row, RowHeight);

        Text name = UiKit.Text(row.transform, "Label", label, 24, UiKit.Muted, TextAnchor.MiddleLeft);
        name.rectTransform.anchorMin = Vector2.zero;
        name.rectTransform.anchorMax = new Vector2(0.28f, 1f);
        name.rectTransform.offsetMin = new Vector2(20f, 0f);
        name.rectTransform.offsetMax = Vector2.zero;

        Text value = UiKit.Text(row.transform, "Value", "", 28, UiKit.Cream, TextAnchor.MiddleLeft);
        value.rectTransform.anchorMin = new Vector2(0.28f, 0f);
        value.rectTransform.anchorMax = Vector2.one;
        value.rectTransform.offsetMin = Vector2.zero;
        value.rectTransform.offsetMax = new Vector2(-20f, 0f);
        return value;
    }

    public override void OnShow()
    {
        WeatherManager weather = WeatherManager.Instance;
        TimeManager time = TimeManager.Instance;
        SurvivalManager survival = SurvivalManager.Instance;

        if (weather != null)
        {
            conditions.text = weather.PlayerWeatherName();
            precipitation.text = weather.PrecipitationWord;

            string feels = weather.FeelsLikeText();
            temperature.text = $"{weather.TemperatureF:0}°F" + (feels.Length > 0 ? ", " + feels : "");

            wind.text = weather.WindWord + (weather.WindWord == "Calm" ? "" : ", from the " + weather.WindFromWords);
        }
        else
        {
            conditions.text = precipitation.text = temperature.text = wind.text = "—";
        }

        sunlight.text = survival != null ? SunWord(survival.SunStrength01) : "—";

        if (time != null)
        {
            date.text = time.FormatDate(time.TotalDays);
            daylight.text = $"Sunrise {Clock(time.SunriseHour)}   ·   Sunset {Clock(time.SunsetHour)}";
        }
        else
        {
            date.text = daylight.text = "—";
        }
    }

    // None at night (or under a roof), then Dim under 0.25, Moderate under 0.55, Strong under 0.80, Intense above.
    static string SunWord(float strength) =>
        strength <= 0.001f ? "None" : strength < 0.25f ? "Dim" : strength < 0.55f ? "Moderate" : strength < 0.80f ? "Strong" : "Intense";

    static string Clock(float hour)
    {
        int minutes = Mathf.RoundToInt(hour * 60f);
        return $"{minutes / 60 % 24:00}:{minutes % 60:00}";
    }
}
