#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.InputSystem;

// Testing only: numpad hotkeys for Rainwater Collection (Water_System.md). The whole file is compiled out of release builds,
// and it creates itself at startup, so nothing is added to any scene. Works only while playing (not paused, not in menus).
// Numpad keys because nothing else uses them (F2 to F12 are TimeDebugControls').
//
//   Numpad 1            force the next weather in the cycle for 12 hours: Clear, Light Rain, Heavy Rain, Thunderstorm, Clear...
//   Numpad 2            skip 1 game hour (Shift: 6 hours), so a barrel's hourly fill can be watched without waiting
//   Numpad 3            fill the Rain Barrel nearest you (within 6 m) to the top; Shift+Numpad 3 empties it
//   Numpad 4            lower the nearest pond's level by 10 points; Numpad 5 raises it by 10
//   Numpad 6            set the nearest pond's level to 20 percent (below the Bass Hole floor, to test Very Low)
//   Numpad 7            show the nearest Rain Barrel's litres, today's weather, the dry-day count, and the nearest pond's level and state
public class RainDebugControls : MonoBehaviour
{
    static readonly WeatherType[] Cycle = { WeatherType.Clear, WeatherType.LightRain, WeatherType.HeavyRain, WeatherType.Thunderstorm };
    const float Reach = 6f;
    const int ForcedHours = 12;

    int cycleIndex;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        var go = new GameObject("Rain Debug Controls");
        go.hideFlags = HideFlags.DontSave;
        DontDestroyOnLoad(go);
        go.AddComponent<RainDebugControls>();
    }

    void Update()
    {
        GameManager game = GameManager.Instance;
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || game == null || game.State != GameState.Playing)
            return;

        bool shift = keyboard.shiftKey.isPressed;
        WeatherManager weather = WeatherManager.Instance;
        TimeManager time = TimeManager.Instance;
        WoodManager wood = WoodManager.Instance;

        if (keyboard.numpad1Key.wasPressedThisFrame && weather != null)
        {
            cycleIndex = (cycleIndex + 1) % Cycle.Length;
            weather.DebugForceWeather(Cycle[cycleIndex], ForcedHours);
            ToolStatus.Flash($"Weather forced: {weather.Current} for {ForcedHours} h  ({weather.RainMmPerHour(weather.PrecipitationWeather):0.#} mm/h)", 3f);
        }
        else if (keyboard.numpad2Key.wasPressedThisFrame && time != null)
        {
            int hours = shift ? 6 : 1;
            time.AdvanceMinutes(hours * 60f);
            ToolStatus.Flash($"Skipped {hours} h", 2f);
        }
        else if (keyboard.numpad3Key.wasPressedThisFrame && wood != null)
        {
            WoodPileState barrel = NearestBarrel(wood);
            if (barrel == null)
            {
                ToolStatus.Flash($"No Rain Barrel within {Reach:0} m", 2.5f);
                return;
            }
            wood.DebugSetBarrelLitres(barrel, shift ? 0 : wood.CapacityOf(barrel.kind));
            ToolStatus.Flash($"Rain Barrel {(shift ? "emptied" : "filled")}: {barrel.Total} / {wood.CapacityOf(barrel.kind)} L", 2.5f);
        }
        else if (keyboard.numpad7Key.wasPressedThisFrame)
        {
            string barrelText = "no Rain Barrel within " + Reach.ToString("0") + " m";
            if (wood != null)
            {
                WoodPileState barrel = NearestBarrel(wood);
                if (barrel != null)
                {
                    bool under = wood.AtAnEave(barrel.position, out _, out int side);
                    barrelText = $"Rain Barrel {barrel.Total} / {wood.CapacityOf(barrel.kind)} L, " +
                                 (under ? $"under the {RainCatchment.SlopeName(side)}" : "NOT under a roof slope");
                }
            }
            string weatherText = weather != null
                ? $"{weather.Current} ({weather.RainMmPerHour(weather.PrecipitationWeather):0.#} mm/h), dry days {weather.DryDays}"
                : "no weather";
            ToolStatus.Flash($"{barrelText}  |  {weatherText}  |  {PondText()}", 6f);
        }
        else if (keyboard.numpad4Key.wasPressedThisFrame)
        {
            ChangePond(-PondStep, null);
        }
        else if (keyboard.numpad5Key.wasPressedThisFrame)
        {
            ChangePond(PondStep, null);
        }
        else if (keyboard.numpad6Key.wasPressedThisFrame)
        {
            ChangePond(0f, PondVeryLowTest);
        }
    }

    // Numpad 4 / 5 move the nearest pond by 10 points; numpad 6 sets it to 0.2, below the Bass Hole floor on purpose so the
    // very-low effects can be tested (the next daily tick brings it back up to the floor).
    const float PondStep = 0.1f;
    const float PondVeryLowTest = 0.2f;

    static void ChangePond(float delta, float? setTo)
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        PondManager ponds = PondManager.Instance;
        string pond = player != null ? PondManager.NearestPond(player.transform.position) : null;
        if (ponds == null || pond == null)
        {
            ToolStatus.Flash("No pond on this property", 2.5f);
            return;
        }
        ponds.SetLevel(pond, setTo ?? ponds.LevelOf(pond) + delta);
        ToolStatus.Flash(PondText(), 3f);
    }

    static string PondText()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        PondManager ponds = PondManager.Instance;
        string pond = player != null ? PondManager.NearestPond(player.transform.position) : null;
        if (ponds == null || pond == null)
            return "no pond";
        float level = ponds.LevelOf(pond);
        return $"{pond} {level * 100f:0}% ({ponds.StateOfLevel(level)}, floor {ponds.FloorOf(pond) * 100f:0}%)";
    }

    static WoodPileState NearestBarrel(WoodManager wood)
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        return player == null ? null : wood.DebugNearestRainBarrel(player.transform.position, Reach);
    }
}
#endif
