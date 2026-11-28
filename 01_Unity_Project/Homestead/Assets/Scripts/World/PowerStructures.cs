using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Slice E1 (Mike, 2026-11-28): the Power Shed, the Solar Panel Rack and the Generator. A PlacedStructure of one of these kinds hands its look, prompts,
// E and R to this class (as the watering fittings do). PowerManager runs the hour; PowerRules has the numbers and the words.
//   Power Shed: a small shed with the inverter on its wall and a battery rack. R stores Battery Modules (up to 8); with none to store, R starts or stops
//     laying cable from it; E opens its panel. R with the Hammer takes it down.
//   Solar Panel Rack: a tilted frame for four 400 W panels, facing south. R puts Solar Panels on it; E says what it is making.
//   Generator: a 5,000 W gasoline generator. R loads Gasoline (up to 5 cans); E says whether it is running and its fuel. It starts by itself.
// THE SWAP POINTS: Resources/Art/structure_powershed, structure_solarrack (with children Panel_1..Panel_4) and structure_generator; the colliders stay the code's.
public static class PowerStructures
{
    public static bool IsPowerKind(PileKind kind) => kind == PileKind.PowerShed || kind == PileKind.SolarRack || kind == PileKind.Generator;
    static InventoryManager Inventory => InventoryManager.Instance;
    static int Carried(string id) => Inventory != null ? Inventory.Player.Count(id) : 0;

    // --- Prompts ---

    public static string Prompt(WoodPileState s)
    {
        PowerManager power = PowerManager.Instance;
        switch (s.kind)
        {
            case PileKind.PowerShed:
                return $"Power Shed  (batteries {PowerRules.Percent(power != null ? power.State.chargeWh : 0f, s.Count(PowerRules.BatteryId))}, {s.Count(PowerRules.BatteryId)} of {PowerRules.BatteriesPerShed} modules)\nE: open the panel";
            case PileKind.SolarRack:
                return $"Solar Panel Rack  ({s.Count(PowerRules.PanelId)} of {PowerRules.PanelsPerRack} panels{ShadeWords(s)})";
            case PileKind.Generator:
                return $"Generator  ({(power != null && power.State.generatorOn ? "running" : "off - starts by itself when the batteries run low")}, {s.Count(WateringRules.GasolineId)} cans of Gasoline)";
        }
        return "";
    }

    static string ShadeWords(WoodPileState s)
    {
        int trees = WoodManager.Instance != null ? WoodManager.Instance.CountTrees(s.position, PowerRules.ShadeReachMetres) : 0;
        return trees > 0 ? $", {PowerRules.Shade(trees) * 100f:0}% shaded by {trees} tree{(trees == 1 ? "" : "s")}" : "";
    }

    public static string SecondaryPrompt(WoodPileState s)
    {
        if (WoodManager.HammerEquipped)
            return $"Take the {WoodManager.PileName(s.kind)} down  (what is in it comes back)";
        switch (s.kind)
        {
            case PileKind.PowerShed:
                if (Carried(PowerRules.BatteryId) > 0 && s.Count(PowerRules.BatteryId) < PowerRules.BatteriesPerShed)
                    return $"Install Battery Modules  ({Carried(PowerRules.BatteryId)} carried)";
                return PowerManager.WiringFrom == s ? "Stop laying cable" : $"Lay cable from here  ({Carried(PowerRules.CableId)} ft of Buried Cable carried)";
            case PileKind.SolarRack:
                return Carried(PowerRules.PanelId) > 0 && s.Count(PowerRules.PanelId) < PowerRules.PanelsPerRack ? $"Mount Solar Panels  ({Carried(PowerRules.PanelId)} carried)" : "";
            case PileKind.Generator:
                return Carried(WateringRules.GasolineId) > 0 && s.Count(WateringRules.GasolineId) < PowerRules.GasolineCapacity ? $"Load Gasoline  ({Carried(WateringRules.GasolineId)} carried)" : "";
        }
        return "";
    }

    // --- E and R ---

    public static void Interact(WoodPileState s)
    {
        if (s.kind == PileKind.PowerShed)
        {
            PowerPanel.Toggle(s);
            return;
        }
        PowerManager power = PowerManager.Instance;
        if (s.kind == PileKind.SolarRack)
        {
            float wh = power != null ? SolarNow(s) : 0f;
            ToolStatus.Flash(s.Count(PowerRules.PanelId) <= 0 ? "No panels yet - R mounts Solar Panels from your pack" : $"Making {wh:0} W right now", 3f);
            return;
        }
        if (s.kind == PileKind.Generator)
            ToolStatus.Flash(power != null && power.State.generatorOn ? $"Running - {power.State.lastGeneratorWh:0} W; {s.Count(WateringRules.GasolineId)} cans left" :
                             $"Off - it starts by itself below {PowerRules.GeneratorOnBelow * 100f:0}% charge and stops at {PowerRules.GeneratorOffAbove * 100f:0}%", 3.5f);
    }

    static float SolarNow(WoodPileState rack)
    {
        TimeManager time = TimeManager.Instance;
        if (time == null)
            return 0f;
        WeatherManager weather = WeatherManager.Instance;
        float clear = DayNightCycle.SunlightFor(weather != null ? weather.Current : WeatherType.Clear);
        float height = ColdFrameRules.Sun(time.HourOfDay, time.SunriseHour, time.SunsetHour, ColdFrameRules.NoonSin((int)time.CurrentSeason), 1f);
        int trees = WoodManager.Instance != null ? WoodManager.Instance.CountTrees(rack.position, PowerRules.ShadeReachMetres) : 0;
        return PowerRules.SolarWh(rack.Count(PowerRules.PanelId), height, clear, PowerRules.Shade(trees));
    }

    public static void Secondary(WoodPileState s)
    {
        WoodManager wood = WoodManager.Instance;
        if (wood == null || Inventory == null)
            return;
        string id = s.kind == PileKind.PowerShed ? PowerRules.BatteryId : s.kind == PileKind.SolarRack ? PowerRules.PanelId : WateringRules.GasolineId;
        int cap = s.kind == PileKind.PowerShed ? PowerRules.BatteriesPerShed : s.kind == PileKind.SolarRack ? PowerRules.PanelsPerRack : PowerRules.GasolineCapacity;
        int n = Mathf.Min(Carried(id), cap - s.Count(id));
        if (n > 0)
        {
            int removed = Inventory.Player.Remove(id, n);
            s.Add(id, removed);
            wood.NotifyChanged(s);
            ToolStatus.Flash($"{(s.kind == PileKind.PowerShed ? "Installed" : s.kind == PileKind.SolarRack ? "Mounted" : "Loaded")} {removed} {ItemDatabase.Get(id)?.DisplayName ?? id} ({s.Count(id)}/{cap})", 2.5f);
            return;
        }
        if (s.kind == PileKind.PowerShed && PowerManager.Instance != null)
        {
            if (PowerManager.WiringFrom == s)
            {
                PowerManager.StopWiring();
                ToolStatus.Flash("Stopped laying cable", 2f);
            }
            else
                PowerManager.Instance.StartWiring(s);
        }
    }

    // --- The look ---

    static readonly Color Wood = new Color(0.48f, 0.36f, 0.22f), Metal = new Color(0.42f, 0.44f, 0.46f), Dark = new Color(0.12f, 0.13f, 0.15f);
    static readonly Color Cell = new Color(0.06f, 0.1f, 0.22f), Gen = new Color(0.82f, 0.62f, 0.12f), Roof = new Color(0.4f, 0.42f, 0.44f);

    public static void Build(Transform parent, WoodPileState state, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        string artId = state.kind == PileKind.PowerShed ? "structure_powershed" : state.kind == PileKind.SolarRack ? "structure_solarrack" : "structure_generator";
        GameObject prefab = Resources.Load<GameObject>("Art/" + artId);
        switch (state.kind)
        {
            case PileKind.PowerShed:
                boxCenter = new Vector3(0f, 1.1f, 0f);
                boxSize = new Vector3(2.1f, 2.2f, 1.7f);
                if (Art(prefab, parent, pieces))
                    return;
                Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 1f, 0f), Vector3.zero, new Vector3(2f, 2f, 1.6f), Wood);
                Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 2.1f, 0f), new Vector3(8f, 0f, 0f), new Vector3(2.3f, 0.06f, 1.9f), Roof);
                Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0.45f, 0.95f, -0.81f), Vector3.zero, new Vector3(0.8f, 1.8f, 0.04f), Dark);
                Piece(parent, pieces, PrimitiveType.Cube, new Vector3(-0.5f, 1.3f, -0.82f), Vector3.zero, new Vector3(0.5f, 0.6f, 0.06f), Metal);
                return;
            case PileKind.SolarRack:
                boxCenter = new Vector3(0f, 0.9f, 0f);
                boxSize = new Vector3(4.6f, 1.8f, 2.2f);
                int panels = state.Count(PowerRules.PanelId);
                if (Art(prefab, parent, pieces))
                {
                    GameObject art = pieces[pieces.Count - 1];
                    for (int i = 1; i <= PowerRules.PanelsPerRack; i++)
                    {
                        Transform p = art.transform.Find("Panel_" + i);
                        if (p != null)
                            p.gameObject.SetActive(i <= panels);
                    }
                    return;
                }
                foreach (float x in new[] { -2f, 0f, 2f })
                {
                    Piece(parent, pieces, PrimitiveType.Cube, new Vector3(x, 0.45f, -0.7f), Vector3.zero, new Vector3(0.08f, 0.9f, 0.08f), Metal);
                    Piece(parent, pieces, PrimitiveType.Cube, new Vector3(x, 1.2f, 0.7f), Vector3.zero, new Vector3(0.08f, 2.4f, 0.08f), Metal);
                }
                for (int i = 0; i < panels; i++)
                    Piece(parent, pieces, PrimitiveType.Cube, new Vector3(-1.69f + i * 1.13f, 1.15f, 0f), new Vector3(-35f, 0f, 0f), new Vector3(1.1f, 0.04f, 1.75f), Cell);
                return;
            default:
                boxCenter = new Vector3(0f, 0.35f, 0f);
                boxSize = new Vector3(1f, 0.7f, 0.7f);
                if (Art(prefab, parent, pieces))
                    return;
                Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.38f, 0f), Vector3.zero, new Vector3(0.9f, 0.5f, 0.6f), Gen);
                Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.16f, 0f), Vector3.zero, new Vector3(1f, 0.06f, 0.66f), Dark);
                Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0.2f, 0.68f, 0f), Vector3.zero, new Vector3(0.22f, 0.05f, 0.22f), Dark);
                return;
        }
    }

    static bool Art(GameObject prefab, Transform parent, List<GameObject> pieces)
    {
        if (prefab == null)
            return false;
        GameObject art = Object.Instantiate(prefab, parent, false);
        art.name = prefab.name;
        foreach (Collider c in art.GetComponentsInChildren<Collider>())
            Object.Destroy(c);
        pieces.Add(art);
        return true;
    }

    static void Piece(Transform parent, List<GameObject> pieces, PrimitiveType shape, Vector3 position, Vector3 euler, Vector3 scale, Color color)
    {
        GameObject piece = GameObject.CreatePrimitive(shape);
        Object.DestroyImmediate(piece.GetComponent<Collider>());
        piece.transform.SetParent(parent, false);
        piece.transform.localPosition = position;
        piece.transform.localEulerAngles = euler;
        piece.transform.localScale = scale;
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        piece.GetComponent<MeshRenderer>().SetPropertyBlock(block);
        pieces.Add(piece);
    }

    // --- What the panel and the phone show ---

    public static List<(string label, string value)> Summary()
    {
        var rows = new List<(string, string)>();
        PowerManager power = PowerManager.Instance;
        WoodManager wood = WoodManager.Instance;
        WoodPileState shed = PowerManager.Shed;
        if (power == null || wood == null || shed == null)
        {
            rows.Add(("Power", "No Power Shed yet - build one (Build tab, Other), then add Battery Modules and Solar Panel Racks"));
            return rows;
        }
        PowerState s = power.State;
        int batteries = shed.Count(PowerRules.BatteryId);
        rows.Add(("Status", s.powered ? "Powered" : batteries <= 0 ? "No batteries - R installs Battery Modules at the shed" : "Off - the batteries are empty"));
        rows.Add(("Batteries", batteries <= 0 ? "none" : $"{PowerRules.Percent(s.chargeWh, batteries)} of {PowerRules.Kwh(PowerRules.CapacityWh(batteries))} ({batteries} module{(batteries == 1 ? "" : "s")})"));
        int racks = 0, panels = 0;
        foreach (WoodPileState p in wood.Piles)
            if (p.kind == PileKind.SolarRack)
            {
                racks++;
                panels += p.Count(PowerRules.PanelId);
            }
        rows.Add(("Solar", $"{panels} panel{(panels == 1 ? "" : "s")} on {racks} rack{(racks == 1 ? "" : "s")} ({panels * PowerRules.PanelWatts} W) - making {power.SolarWhNow():0} W now"));
        rows.Add(("Using", $"{power.LoadWattsNow():0} W now"));
        WoodPileState gen = null;
        foreach (WoodPileState p in wood.Piles)
            if (p.kind == PileKind.Generator)
                gen = p;
        rows.Add(("Generator", gen == null ? "none" : $"{(s.generatorOn ? "running" : "off (automatic)")} - {gen.Count(WateringRules.GasolineId)} cans + {s.fuelLeft:0.0} gal"));
        var wired = new List<string>();
        foreach (WireRun w in s.wires)
            foreach (WoodPileState p in wood.Piles)
                if (p.id == w.targetId)
                    wired.Add(WoodManager.PileName(p.kind));
        rows.Add(("Wired", wired.Count == 0 ? "nothing yet - R at the shed lays cable" : string.Join(", ", wired)));
        return rows;
    }
}

// The Power Shed's panel (E): the same rows the phone's Power app shows, on a small window that stays while the player is near the shed.
public class PowerPanel : MonoBehaviour
{
    static PowerPanel instance;
    WoodPileState shed;
    Text body;
    float refreshAt;

    public static void Toggle(WoodPileState shed)
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
            instance = null;
            return;
        }
        var go = new GameObject("Power Panel");
        instance = go.AddComponent<PowerPanel>();
        instance.shed = shed;
        instance.Build();
    }

    void Build()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 35;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        var rt = (RectTransform)panel.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(720f, 520f);
        rt.anchoredPosition = new Vector2(-40f, 0f);
        panel.GetComponent<Image>().color = new Color(0.05f, 0.07f, 0.09f, 0.9f);
        var text = new GameObject("Text", typeof(RectTransform), typeof(Text));
        text.transform.SetParent(panel.transform, false);
        var trt = (RectTransform)text.transform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(24f, 20f);
        trt.offsetMax = new Vector2(-24f, -20f);
        body = text.GetComponent<Text>();
        body.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        body.fontSize = 26;
        body.color = Color.white;
        body.supportRichText = true;
        body.horizontalOverflow = HorizontalWrapMode.Wrap;
        body.verticalOverflow = VerticalWrapMode.Overflow;
        Refresh();
    }

    void Refresh()
    {
        var sb = new System.Text.StringBuilder("<b><size=32>Power Shed</size></b>\n\n");
        foreach (var row in PowerStructures.Summary())
            sb.Append($"<color=#9FC8FF>{row.label}</color>   {row.value}\n");
        sb.Append("\n<size=20><color=#BBBBBB>E closes this. R lays cable or installs Battery Modules.</color></size>");
        body.text = sb.ToString();
    }

    void Update()
    {
        Vector3? at = WoodManager.PlayerSpot;
        bool far = shed == null || at == null || Vector3.Distance(at.Value, shed.position) > 6f;
        if (far || GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
        {
            Destroy(gameObject);
            instance = null;
            return;
        }
        if (Time.unscaledTime >= refreshAt)
        {
            refreshAt = Time.unscaledTime + 1f;
            Refresh();
        }
    }
}
