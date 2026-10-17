using UnityEngine;
using UnityEngine.UI;

// Weather_System.md's Weather Information and Instruments, Layer 2: the free Weather tab (K). A short list of labelled rows in plain
// words. It reads the same player-facing name and feels-like text as the pause date line, and never names a Heat Wave: the
// player works it out from the temperature, the feels-like line and the strong sun. Nothing changes while it is open (the
// clock is paused), so the rows are filled once per OnShow. Phase 2: while a Weather Station is in the player's inventory
// (the backpack container; hotbar slots only point at items in it) five numeric rows follow the free ones, and they come
// and go as the inventory changes. They show data only, so a Heat Wave still never gets a name.
public class WeatherScreen : GameScreen
{
    public const string WeatherStationId = "weather_station";

    // The content area is 740 high; twelve rows with a Weather Station must sit inside it with a margin, so the rows are
    // 52 high with 4 between (668 plus the top gap), and the free seven simply leave more room.
    const float RowHeight = 52f;
    const float RowSpacing = 4f;

    Text conditions, precipitation, temperature, wind, sunlight, date, daylight;
    Text humidity, dewPoint, windSpeed, windBearing, sunValue;
    GameObject[] stationRows;
    InventoryContainer watched;

    public override string Title => "Weather";

    public override void Build(RectTransform area)
    {
        RectTransform column = UiKit.Rect("Rows", area);
        column.anchorMin = new Vector2(0.5f, 1f);
        column.anchorMax = new Vector2(0.5f, 1f);
        column.pivot = new Vector2(0.5f, 1f);
        column.sizeDelta = new Vector2(1000f, 7 * RowHeight + 6 * RowSpacing);
        var fitter = column.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        column.anchoredPosition = new Vector2(0f, -16f);
        var layout = column.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = RowSpacing;
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

        humidity = Row(column, "Humidity");
        dewPoint = Row(column, "Dew point");
        windSpeed = Row(column, "Wind speed");
        windBearing = Row(column, "Wind bearing");
        sunValue = Row(column, "Sunlight value");
        stationRows = new[] { humidity.transform.parent.gameObject, dewPoint.transform.parent.gameObject, windSpeed.transform.parent.gameObject,
            windBearing.transform.parent.gameObject, sunValue.transform.parent.gameObject };
        foreach (GameObject row in stationRows)
            row.SetActive(false);
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
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory != null && watched != inventory.Player)
        {
            Unwatch();
            watched = inventory.Player;
            watched.Changed += RefreshStation;
        }

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

        RefreshStation();
    }

    public override void OnHide() => Unwatch();

    void Unwatch()
    {
        if (watched != null)
            watched.Changed -= RefreshStation;
        watched = null;
    }

    // Shows or hides the Weather Station rows to match the inventory, and fills them from the live weather.
    void RefreshStation()
    {
        bool owned = watched != null && watched.Has(WeatherStationId);
        foreach (GameObject row in stationRows)
            row.SetActive(owned);
        if (!owned)
            return;

        WeatherManager weather = WeatherManager.Instance;
        if (weather == null)
            return;

        humidity.text = $"{weather.RelativeHumidity:0}%";
        // The raw dew point (the one humidity and the body model use), never shown above the air temperature.
        float shownDewC = Mathf.Min(weather.DewPointC, weather.TemperatureC);
        dewPoint.text = $"{shownDewC * 9f / 5f + 32f:0}°F";
        windSpeed.text = $"{weather.WindSpeedMph:0} mph";
        windBearing.text = $"{Mathf.Repeat(weather.WindDirection, 360f):0}° ({weather.WindFromWords})";
        SurvivalManager survival = SurvivalManager.Instance;
        sunValue.text = survival != null ? $"{survival.SunStrength01 * 100f:0}%" : "—";
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
