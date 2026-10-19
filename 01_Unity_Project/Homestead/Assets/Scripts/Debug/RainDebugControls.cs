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
//   Numpad 7            show the nearest Rain Barrel's litres, today's weather, the dry-day count, the nearest pond's level and state,
//                       and the creek's stage, state and murk hours
//   Numpad 8            step the creek's Stream Stage: Normal, High water, Flash flood, Normal...
//   Numpad 0            the nearest Well (within 8 m) to 0 L; Shift fills it, Ctrl makes it Unsafe (flood water)
//   Numpad .            the nearest Outhouse's pit to full; Shift empties it
//   Numpad 9            show the creek's stage, state and murk hours, and whether you are in the creek, its flow direction and your speed factor
//   Numpad +            mark the wood in the nearest Wood Shed (within 8 m) wet; with none in reach, the wood you carry. Shift marks it dry,
//                       Ctrl always means the wood you carry
//   Numpad -            advance seasoning by one season: every pile, shed and the wood you carry (wet wood becomes dry)
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
            ToolStatus.Flash($"{barrelText}  |  {weatherText}  |  {PondText()}  |  {StreamText()}", 6f);
        }
        else if (keyboard.numpad8Key.wasPressedThisFrame && StreamManager.Instance != null)
        {
            // 0 mm, then High water, then a flash flood, then calm again (a rise announces itself and sweeps creek traps, as an hour would).
            float stage = StreamManager.Instance.StageMm;
            float next = stage >= StreamManager.Instance.flashFloodAtMm ? 0f
                       : stage >= StreamManager.Instance.highWaterAtMm ? StreamManager.Instance.flashFloodAtMm + FloodMargin
                       : StreamManager.Instance.highWaterAtMm + FloodMargin;
            StreamManager.Instance.DebugSetStage(next);
            ToolStatus.Flash(StreamText(), 3f);
        }
        else if (keyboard.numpad9Key.wasPressedThisFrame)
        {
            ToolStatus.Flash(StreamText() + "  |  " + CreekCrossing.DebugText(), 5f);
        }
        else if (keyboard.numpad0Key.wasPressedThisFrame)
        {
            DebugWell(shift, keyboard.ctrlKey.isPressed);
        }
        else if (keyboard.numpadPeriodKey.wasPressedThisFrame)
        {
            DebugPit(shift);
        }
        else if (keyboard.numpadPlusKey.wasPressedThisFrame)
        {
            DebugWet(shift, keyboard.ctrlKey.isPressed);
        }
        else if (keyboard.numpadMinusKey.wasPressedThisFrame)
        {
            DebugSeason();
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

    // Numpad 0: the nearest Well (within 8 m) to 0 L; Shift fills it; Ctrl contaminates it (Unsafe for the flood hours).
    // Numpad . : the nearest Outhouse's pit to full; Shift empties it (a fresh pit).
    const float StructureReach = 8f;

    static void DebugWell(bool fill, bool contaminate)
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        WellsAndOuthouses rules = WellsAndOuthouses.Instance;
        WoodPileState well = player != null && rules != null ? rules.DebugNearest(PileKind.Well, player.transform.position, StructureReach) : null;
        if (well == null)
        {
            ToolStatus.Flash($"No finished Well within {StructureReach:0} m", 2.5f);
            return;
        }
        rules.DebugSetWell(well, contaminate ? 2 : fill ? 1 : 0);
        ToolStatus.Flash($"Well: {well.litres} of {rules.wellBufferLitres} L, {rules.WellQuality(well, out _)} water" +
                         (well.contaminatedHours > 0f ? $" ({well.contaminatedHours:0} h of flood water)" : ""), 3f);
    }

    static void DebugPit(bool empty)
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        WellsAndOuthouses rules = WellsAndOuthouses.Instance;
        WoodPileState outhouse = player != null && rules != null ? rules.DebugNearest(PileKind.Outhouse, player.transform.position, StructureReach) : null;
        if (outhouse == null)
        {
            ToolStatus.Flash($"No Outhouse within {StructureReach:0} m", 2.5f);
            return;
        }
        rules.DebugSetPit(outhouse, !empty);
        ToolStatus.Flash($"Outhouse pit {rules.PitFill(outhouse) * 100f:0}% full; sanitary homestead: {(rules.IsSanitary() ? "yes" : "no")}", 3f);
    }

    // Numpad +: wet (or, with Shift, dry) the wood in the nearest Wood Shed, or the wood carried (WoodMoisture).
    static void DebugWet(bool dry, bool carriedOnly)
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        WellsAndOuthouses rules = WellsAndOuthouses.Instance;
        InventoryManager inventory = InventoryManager.Instance;
        int stamp = dry ? 0 : WoodMoisture.WetStamp;
        WoodPileState shed = !carriedOnly && player != null && rules != null ? rules.DebugNearest(PileKind.WoodShed, player.transform.position, StructureReach) : null;
        if (shed != null)
        {
            shed.RestampWood(stamp);
            ToolStatus.Flash($"Wood Shed wood marked {(dry ? "dry" : "wet")}: {MoistureText(shed)}", 3f);
            return;
        }
        if (inventory == null)
            return;
        inventory.Player.MapBatchDays(WoodMoisture.Applies, _ => stamp);
        ToolStatus.Flash($"Carried wood marked {(dry ? "dry" : "wet")}: {CarriedMoistureText(inventory)}", 3f);
    }

    // Numpad -: one season on for all wood (piles, sheds, the pack).
    static void DebugSeason()
    {
        WoodManager wood = WoodManager.Instance;
        InventoryManager inventory = InventoryManager.Instance;
        int days = WoodMoisture.SeasoningDays;
        if (wood != null)
            foreach (WoodPileState pile in wood.Piles)
                pile.AgeWood(days);
        if (inventory != null)
            inventory.Player.MapBatchDays(WoodMoisture.Applies, day => WoodMoisture.Aged(day, days));
        ToolStatus.Flash($"Seasoned all wood forward {days} days (a season). Carried: {(inventory != null ? CarriedMoistureText(inventory) : "-")}", 4f);
    }

    static string MoistureText(WoodPileState pile)
    {
        var parts = new System.Collections.Generic.List<string>();
        foreach (string id in WoodManager.WoodIds)
        {
            pile.CountMoisture(id, WoodMoisture.Today, WoodMoisture.SeasoningDays, out int wet, out int dry);
            if (wet + dry > 0)
                parts.Add($"{ItemDatabase.Get(id)?.DisplayName ?? id} {WoodMoisture.Label(wet, dry)}");
        }
        return parts.Count > 0 ? string.Join(", ", parts) : "empty";
    }

    static string CarriedMoistureText(InventoryManager inventory)
    {
        var parts = new System.Collections.Generic.List<string>();
        foreach (string id in WoodManager.WoodIds)
        {
            int wet = 0, dry = 0;
            foreach ((int day, int quantity) in inventory.Player.Batches(id))
            {
                if (WoodMoisture.IsDry(day))
                    dry += quantity;
                else
                    wet += quantity;
            }
            if (wet + dry > 0)
                parts.Add($"{ItemDatabase.Get(id)?.DisplayName ?? id} {WoodMoisture.Label(wet, dry)}");
        }
        return parts.Count > 0 ? string.Join(", ", parts) : "no wood";
    }

    const float FloodMargin = 3f; // Numpad 8 sets the stage this far above a threshold

    static string StreamText() => StreamManager.Instance != null ? StreamManager.Instance.DebugText() : "no stream";

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
