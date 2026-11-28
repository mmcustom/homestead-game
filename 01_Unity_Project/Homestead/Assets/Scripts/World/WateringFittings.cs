using System.Collections.Generic;
using UnityEngine;

// The watering upgrades (Greenhouses.md, slice G5; WateringRules): the Hose Bib in a greenhouse, the Hose Reel outdoors, the Hand Pump and the Fuel Pump.
// A PlacedStructure of one of these kinds hands its look, prompts, E and R to this class; GardenBeds' game hour calls Hour, which runs the drip timers at
// WateringRules.DripHour, before the beds grow.
// Saved on the pile: a fitting's Drip Kit in cropFlags (Drip); a Hand Pump's tank in litres; a Fuel Pump's Gasoline cans in contents and the litres left of the
// can it is burning in litres.
public static class WateringFittings
{
    public const int Drip = 1;
    static InventoryManager Inventory => InventoryManager.Instance;
    static readonly string[] WaterIds = { "water_unsafe", "water_questionable", "water_good", "water_excellent", Cooking.PurifiedWaterId };

    public static bool IsFitting(PileKind kind) => kind == PileKind.HoseBib || kind == PileKind.HoseReel;
    public static bool IsPump(PileKind kind) => kind == PileKind.HandPump || kind == PileKind.FuelPump;
    public static bool IsWaterKind(PileKind kind) => IsFitting(kind) || IsPump(kind);
    static bool HasDrip(WoodPileState s) => (s.cropFlags & Drip) != 0;

    // --- Beds and sources ---

    // The beds a fitting waters: a bib its greenhouse's, a reel the garden beds within reach that are not under cover. Dug beds only.
    public static List<WoodPileState> BedsOf(WoodManager wood, WoodPileState fitting)
    {
        var beds = new List<WoodPileState>();
        if (fitting.kind == PileKind.HoseBib)
        {
            WoodPileState house = wood.HouseAt(fitting.position + Quaternion.Euler(0f, fitting.yaw, 0f) * Vector3.forward * 0.3f);
            if (house != null)
                foreach (WoodPileState bed in wood.BedsInFrame(house))
                    if (bed.digProgress >= 1f)
                        beds.Add(bed);
            return beds;
        }
        foreach (WoodPileState bed in wood.Piles)
            if (bed.kind == PileKind.GardenBed && bed.digProgress >= 1f && (bed.cropFlags & GardenRules.FlagInFrame) == 0 &&
                Vector3.Distance(bed.position, fitting.position) <= WateringRules.ReelReachMetres)
                beds.Add(bed);
        return beds;
    }

    static int Need(List<WoodPileState> beds)
    {
        int need = 0;
        foreach (WoodPileState bed in beds)
            need += GardenRules.LitresToSoak(bed.soilMoisture);
        return need;
    }

    static bool IsSource(PileKind kind) => kind == PileKind.WaterBarrel || kind == PileKind.RainBarrel || IsPump(kind);

    // What a source can give now.
    public static int Available(WoodPileState source)
    {
        switch (source.kind)
        {
            case PileKind.HandPump: return source.litres;
            case PileKind.FuelPump: return WateringRules.FuelPumpLitres(source.Count(WateringRules.GasolineId), source.litres);
            default:
                int n = 0;
                foreach (string id in WaterIds)
                    n += source.Count(id);
                return n;
        }
    }

    static int Draw(WoodPileState source, int litres)
    {
        int given = 0;
        switch (source.kind)
        {
            case PileKind.HandPump:
                given = Mathf.Min(litres, source.litres);
                source.litres -= given;
                return given;
            case PileKind.FuelPump:
                while (source.litres < litres && source.Count(WateringRules.GasolineId) > 0)
                {
                    source.Take(WateringRules.GasolineId, 1);
                    source.litres += WateringRules.LitresPerCan;
                }
                given = Mathf.Min(litres, source.litres);
                source.litres -= given;
                return given;
            default:
                foreach (string id in WaterIds)
                {
                    if (given >= litres)
                        break;
                    int take = Mathf.Min(litres - given, source.Count(id));
                    if (take > 0)
                    {
                        source.Take(id, take);
                        given += take;
                    }
                }
                return given;
        }
    }

    // The nearest source within reach that has water (or, if none has, the nearest at all, so the prompt can say it is empty).
    public static WoodPileState SourceOf(WoodManager wood, WoodPileState fitting, out float metres)
    {
        WoodPileState best = null, bestDry = null;
        float bestD = float.MaxValue, bestDryD = float.MaxValue;
        foreach (WoodPileState pile in wood.Piles)
        {
            if (!IsSource(pile.kind))
                continue;
            float d = Vector3.Distance(pile.position, fitting.position);
            if (d > WateringRules.SourceReachMetres)
                continue;
            if (Available(pile) > 0 && d < bestD) { best = pile; bestD = d; }
            if (d < bestDryD) { bestDry = pile; bestDryD = d; }
        }
        metres = best != null ? bestD : bestDryD;
        return best ?? bestDry;
    }

    // Waters a fitting's beds to soaked from its source. Returns the litres poured; why says what was short, if anything.
    public static int WaterBeds(WoodManager wood, WoodPileState fitting, out int need, out string why)
    {
        why = null;
        List<WoodPileState> beds = BedsOf(wood, fitting);
        need = Need(beds);
        if (need <= 0)
            return 0;
        WoodPileState source = SourceOf(wood, fitting, out _);
        if (source == null)
        {
            why = WateringRules.NoSource;
            return 0;
        }
        int got = Draw(source, need);
        if (got < need)
            why = WateringRules.Empty(WoodManager.PileName(source.kind));
        int left = got;
        foreach (WoodPileState bed in beds)
        {
            int pour = Mathf.Min(left, GardenRules.LitresToSoak(bed.soilMoisture));
            if (pour <= 0)
                continue;
            bed.soilMoisture = GardenRules.AfterWatering(bed.soilMoisture, pour);
            left -= pour;
            wood.NotifyChanged(bed);
        }
        wood.NotifyChanged(source);
        return got;
    }

    // --- The hour: the drip timers, each morning ---

    public static void Hour(WoodManager wood)
    {
        if (TimeManager.Instance == null || TimeManager.Instance.Hour != WateringRules.DripHour)
            return;
        string fail = null;
        foreach (WoodPileState fitting in new List<WoodPileState>(wood.Piles))
        {
            if (!IsFitting(fitting.kind) || !HasDrip(fitting))
                continue;
            WaterBeds(wood, fitting, out _, out string why);
            if (why != null && fail == null)
                fail = WateringRules.DripFailBanner(fitting.kind == PileKind.HoseBib ? "a greenhouse" : "a hose reel", why.Replace("No water: ", ""));
        }
        if (fail != null)
            ToolStatus.Banner(fail, 10f);
    }

    // --- Prompts ---

    static int Carried(string id) => Inventory != null ? Inventory.Player.Count(id) : 0;

    static string SourceText(WoodManager wood, WoodPileState fitting)
    {
        WoodPileState source = SourceOf(wood, fitting, out float metres);
        if (source == null)
            return null;
        return WateringRules.SourceName(WoodManager.PileName(source.kind), metres) + $" ({Available(source)} L)";
    }

    public static string Header(WoodPileState s)
    {
        WoodManager wood = WoodManager.Instance;
        if (wood == null)
            return null;
        switch (s.kind)
        {
            case PileKind.HandPump: return WateringRules.PumpHeader(s.litres);
            case PileKind.FuelPump: return WateringRules.FuelPumpHeader(s.Count(WateringRules.GasolineId), Available(s));
            default: return WateringRules.FittingHeader(WoodManager.PileName(s.kind), SourceText(wood, s), HasDrip(s));
        }
    }

    public static string Prompt(WoodPileState s)
    {
        WoodManager wood = WoodManager.Instance;
        if (wood == null)
            return null;
        switch (s.kind)
        {
            case PileKind.HandPump: return WateringRules.PumpPrompt(s.litres);
            case PileKind.FuelPump: return null;
        }
        if (!HasDrip(s) && Carried(WateringRules.DripKitId) > 0)
            return WateringRules.FitDripPrompt;
        List<WoodPileState> beds = BedsOf(wood, s);
        int need = Need(beds);
        WoodPileState source = SourceOf(wood, s, out float metres);
        if (need > 0 && source == null)
            return WateringRules.NoSource;
        if (need > 0 && Available(source) == 0)
            return WateringRules.Empty(WoodManager.PileName(source.kind));
        return WateringRules.WaterPrompt(beds.Count, need, source != null ? WateringRules.SourceName(WoodManager.PileName(source.kind), metres) : "");
    }

    public static string SecondaryPrompt(WoodPileState s)
    {
        if (WoodManager.HammerEquipped)
            return $"Take the {WoodManager.PileName(s.kind)} down  (half its materials back)";
        return s.kind == PileKind.FuelPump ? WateringRules.FuelLoadPrompt(Carried(WateringRules.GasolineId), s.Count(WateringRules.GasolineId)) : "";
    }

    // --- E and R ---

    public static void Interact(WoodPileState s)
    {
        WoodManager wood = WoodManager.Instance;
        if (wood == null || Inventory == null)
            return;
        if (s.kind == PileKind.HandPump)
        {
            int added = WateringRules.Pumped(s.litres);
            if (added <= 0)
                return;
            s.litres += added;
            wood.NotifyChanged(s);
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play(SoundCue.UiClick);
            ToolStatus.Flash($"Pumped {added} L - {s.litres}/{WateringRules.PumpTankLitres} L in the tank", 2f);
            return;
        }
        if (s.kind == PileKind.FuelPump)
            return;
        if (!HasDrip(s) && Carried(WateringRules.DripKitId) > 0)
        {
            Inventory.Player.Remove(WateringRules.DripKitId, 1);
            s.cropFlags |= Drip;
            wood.NotifyChanged(s);
            ToolStatus.Flash(WateringRules.DripFitted, 3f);
            return;
        }
        int poured = WaterBeds(wood, s, out int need, out string why);
        if (need <= 0)
        {
            ToolStatus.Flash("The beds are soaked", 2f);
            return;
        }
        if (poured > 0 && AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
        ToolStatus.Flash(poured <= 0 ? why : why != null ? $"Watered with {poured} L - then ran dry ({why.Replace("No water: ", "")})" : $"Watered the beds with {poured} L", 3f);
    }

    public static void Secondary(WoodPileState s)
    {
        if (s.kind != PileKind.FuelPump || Inventory == null || WoodManager.Instance == null)
            return;
        int n = Mathf.Min(Carried(WateringRules.GasolineId), WateringRules.GasolineCap - s.Count(WateringRules.GasolineId));
        if (n <= 0)
            return;
        int removed = Inventory.Player.Remove(WateringRules.GasolineId, n);
        s.Add(WateringRules.GasolineId, removed);
        WoodManager.Instance.NotifyChanged(s);
        ToolStatus.Flash($"Loaded {removed} Gasoline ({s.Count(WateringRules.GasolineId)}/{WateringRules.GasolineCap})", 2.5f);
    }

    // --- Where a pump may stand ---

    public static bool NearOpenWater(Vector3 point, float metres)
    {
        for (int ring = 1; ring <= 3; ring++)
        {
            float r = metres * ring / 3f;
            for (int i = 0; i < 16; i++)
            {
                float a = i * Mathf.PI / 8f;
                if (Animal.IsOverWater(point + new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r), out _))
                    return true;
            }
        }
        return false;
    }

    public static bool PumpSiteOk(PileKind kind, Vector3 point)
    {
        if (NearOpenWater(point, WateringRules.PumpWaterMetres))
            return true;
        if (kind != PileKind.HandPump || WoodManager.Instance == null)
            return false;
        foreach (WoodPileState pile in WoodManager.Instance.Piles)
            if ((pile.kind == PileKind.Well || pile.kind == PileKind.DrilledWell) && Vector3.Distance(pile.position, point) <= WateringRules.PumpWellMetres + 1f)
                return true;
        return false;
    }

    // --- The look ---

    public static void Build(Transform parent, WoodPileState state, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        switch (state.kind)
        {
            case PileKind.HoseBib: boxCenter = new Vector3(0f, 0.9f, 0.05f); boxSize = new Vector3(0.4f, 0.6f, 0.15f); break;
            case PileKind.HoseReel: boxCenter = new Vector3(0f, 0.55f, 0f); boxSize = new Vector3(0.6f, 1.1f, 0.5f); break;
            case PileKind.HandPump: boxCenter = new Vector3(0f, 0.6f, 0f); boxSize = new Vector3(0.9f, 1.2f, 0.6f); break;
            default: boxCenter = new Vector3(0f, 0.35f, 0f); boxSize = new Vector3(0.9f, 0.7f, 0.6f); break;
        }
        GameObject prefab = Resources.Load<GameObject>("Art/structure_" + state.kind.ToString().ToLowerInvariant());
        if (prefab != null)
        {
            GameObject art = Object.Instantiate(prefab, parent, false);
            art.name = prefab.name;
            foreach (Collider c in art.GetComponentsInChildren<Collider>())
                Object.DestroyImmediate(c);
            foreach (Transform t in art.GetComponentsInChildren<Transform>(true))
                if (t.name == "Drip") t.gameObject.SetActive(HasDrip(state));
            pieces?.Add(art);
            return;
        }
        Color hose = new Color(0.15f, 0.35f, 0.15f), brass = new Color(0.7f, 0.55f, 0.25f), iron = new Color(0.18f, 0.18f, 0.19f), wood = new Color(0.45f, 0.33f, 0.2f), red = new Color(0.6f, 0.12f, 0.08f);
        switch (state.kind)
        {
            case PileKind.HoseBib:
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 1.0f, 0.02f), new Vector3(0.08f, 0.08f, 0.08f)), brass);
                Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.8f, 0.08f), new Vector3(0.35f, 0.03f, 0.35f), new Vector3(90f, 0f, 0f)), hose);
                break;
            case PileKind.HoseReel:
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.5f, -0.1f), new Vector3(0.08f, 1.0f, 0.08f)), wood);
                Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.7f, 0.05f), new Vector3(0.5f, 0.12f, 0.5f), new Vector3(90f, 0f, 0f)), hose);
                break;
            case PileKind.HandPump:
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.05f, 0f), new Vector3(0.6f, 0.1f, 0.6f)), wood);
                Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.55f, 0f), new Vector3(0.14f, 0.45f, 0.14f)), iron);
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0.2f, 1.0f, 0f), new Vector3(0.45f, 0.04f, 0.04f), new Vector3(0f, 0f, -15f)), iron);
                Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0.35f, 0.25f, 0f), new Vector3(0.4f, 0.25f, 0.4f)), wood);
                break;
            default:
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.05f, 0f), new Vector3(0.8f, 0.1f, 0.5f)), wood);
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(-0.15f, 0.3f, 0f), new Vector3(0.35f, 0.4f, 0.35f)), red);
                Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0.22f, 0.25f, 0f), new Vector3(0.25f, 0.15f, 0.25f), new Vector3(0f, 0f, 90f)), iron);
                break;
        }
    }

    static GameObject Piece(Transform parent, List<GameObject> pieces, PrimitiveType shape, Vector3 position, Vector3 scale, Vector3 euler = default)
    {
        GameObject piece = GameObject.CreatePrimitive(shape);
        Object.DestroyImmediate(piece.GetComponent<Collider>());
        piece.transform.SetParent(parent, false);
        piece.transform.localPosition = position;
        piece.transform.localRotation = Quaternion.Euler(euler);
        piece.transform.localScale = scale;
        pieces?.Add(piece);
        return piece;
    }

    static GameObject Tinted(GameObject piece, Color color)
    {
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        piece.GetComponent<MeshRenderer>().SetPropertyBlock(block);
        return piece;
    }
}
