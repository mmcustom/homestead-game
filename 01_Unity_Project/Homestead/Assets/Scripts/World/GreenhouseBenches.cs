using System.Collections.Generic;
using UnityEngine;

// A Glass Greenhouse's seedling bench (Greenhouses.md, slice G3; GlassHouseRules). A waist-high table along the back wall that holds one tray. A PlacedStructure
// of kind SeedlingBench hands its look, prompts, E and R to this class, and GardenBeds' game hour calls Hour, so the seedlings grow while the player is away.
//   Empty        E sows a tray of the chosen seed (1 seed from the pack); R picks the next seed carried.
//   Growing      they grow at the greenhouse's inside temperature (GardenRules.GrowthRate), and take frost and heat as a crop does; no watering.
//   Ready        E takes up the transplants into the pack (what does not fit stays on the bench for the next E).
//   Dead         E clears the dead seedlings.
// Saved on the bench: the tray's crop in jobItem, its growing days in jobHours, its damage in cropDamage, dead in cropFlags (GardenRules.FlagDead), the transplants
// already taken in picksTaken, and the seed chosen while empty in jobCount (an index into GlassHouseRules.Trays).
public static class GreenhouseBenches
{
    static InventoryManager Inventory => InventoryManager.Instance;
    static readonly Color Wood = new Color(0.5f, 0.38f, 0.24f);
    static readonly Color Soil = new Color(0.22f, 0.16f, 0.1f);
    static readonly Color Leaf = new Color(0.3f, 0.6f, 0.22f);
    static readonly Color Brown = new Color(0.45f, 0.35f, 0.2f);

    static bool Dead(WoodPileState s) => (s.cropFlags & GardenRules.FlagDead) != 0;
    static bool HasTray(WoodPileState s) => !string.IsNullOrEmpty(s.jobItem) && GlassHouseRules.TryTray(s.jobItem, out _);

    static GlassHouseRules.Tray TrayOf(WoodPileState s)
    {
        GlassHouseRules.TryTray(s.jobItem, out GlassHouseRules.Tray tray);
        return tray;
    }

    static bool Ready(WoodPileState s) => HasTray(s) && !Dead(s) && s.jobHours >= TrayOf(s).days;

    static float InsideNow(WoodPileState bench)
    {
        WoodManager wood = WoodManager.Instance;
        WoodPileState house = wood != null ? wood.HouseAt(bench.position) : null;
        float air = WeatherManager.Instance != null ? WeatherManager.Instance.TemperatureC : 15f;
        return house != null ? GardenBeds.FrameInsideShown(house) : air;
    }

    // --- The hour ---

    public static void Hour(WoodManager wood, float airC)
    {
        int died = 0;
        foreach (WoodPileState bench in wood.Piles)
        {
            if (bench.kind != PileKind.SeedlingBench || !HasTray(bench) || Dead(bench) || Ready(bench))
                continue;
            GardenRules.Crop crop = GardenRules.Find(bench.jobItem);
            if (crop == null)
                continue;
            WoodPileState house = wood.HouseAt(bench.position);
            float inside = house != null ? GardenBeds.FrameInside(house, airC) : airC;
            int stageBefore = Stage(bench);
            bench.jobHours += GardenRules.GrowthRate(crop, inside) / 24f;
            float harm = GardenRules.FrostDamage(crop, inside, false) + ColdFrameRules.HeatDamage(crop.heatC, inside);
            if (harm > 0f)
            {
                bench.cropDamage = GardenRules.Clamp01(bench.cropDamage + harm);
                if (bench.cropDamage >= 1f)
                {
                    bench.cropFlags |= GardenRules.FlagDead;
                    died++;
                }
            }
            if (Stage(bench) != stageBefore || Dead(bench))
                wood.NotifyChanged(bench);
        }
        if (died > 0)
            ToolStatus.Banner(GlassHouseRules.TrayDiedBanner, 8f);
    }

    // How far on the seedlings look: 0 (just sown) to 4 (ready).
    static int Stage(WoodPileState s) => HasTray(s) ? Mathf.Clamp(Mathf.FloorToInt(4f * s.jobHours / Mathf.Max(1f, TrayOf(s).days)), 0, 4) : 0;

    // --- Prompts ---

    // The trays the player can sow now: those whose seed is in the pack.
    static List<int> Carried()
    {
        var list = new List<int>();
        if (Inventory == null)
            return list;
        for (int i = 0; i < GlassHouseRules.Trays.Length; i++)
            if (Inventory.Player.Count(GlassHouseRules.Trays[i].seedId) >= 1)
                list.Add(i);
        return list;
    }

    static int Chosen(WoodPileState s)
    {
        List<int> carried = Carried();
        if (carried.Count == 0)
            return -1;
        return carried.Contains(s.jobCount) ? s.jobCount : carried[0];
    }

    public static string Header(WoodPileState s)
    {
        if (!HasTray(s))
            return GlassHouseRules.BenchHeader(null, 0f, 0f, InsideNow(s));
        GlassHouseRules.Tray tray = TrayOf(s);
        if (Dead(s))
            return $"Seedling bench - the {tray.transplantName} died";
        return GlassHouseRules.BenchHeader(tray.transplantName, s.jobHours, tray.days, InsideNow(s));
    }

    public static string Prompt(WoodPileState s)
    {
        if (HasTray(s))
        {
            if (Dead(s))
                return GlassHouseRules.ClearPrompt;
            return Ready(s) ? GlassHouseRules.TakePrompt(TrayOf(s)) : null;
        }
        int chosen = Chosen(s);
        return chosen < 0 ? GlassHouseRules.NoSeedPrompt : GlassHouseRules.SowPrompt(GlassHouseRules.Trays[chosen]);
    }

    public static string SecondaryPrompt(WoodPileState s) => !HasTray(s) && Carried().Count > 1 ? GlassHouseRules.PickPrompt : "";

    // --- E and R ---

    public static void Interact(WoodPileState s)
    {
        WoodManager wood = WoodManager.Instance;
        if (Inventory == null || wood == null)
            return;
        if (HasTray(s) && Dead(s))
        {
            Clear(s);
            wood.NotifyChanged(s);
            ToolStatus.Flash("Cleared the tray", 2f);
            return;
        }
        if (HasTray(s))
        {
            if (!Ready(s))
                return;
            GlassHouseRules.Tray tray = TrayOf(s);
            int left = tray.transplants - s.picksTaken;
            int took = Inventory.AddToPlayer(tray.transplantId, left);
            s.picksTaken += took;
            if (took <= 0)
            {
                ToolStatus.Flash("No room in your pack", 2.5f);
                return;
            }
            if (s.picksTaken >= tray.transplants)
                Clear(s);
            wood.NotifyChanged(s);
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play(SoundCue.UiClick);
            ToolStatus.Flash(took < left ? $"Took up {took} {tray.transplantName} - no room for the other {left - took}" : $"Took up {took} {tray.transplantName} - plant them in a garden bed with the Hoe", 3f);
            return;
        }
        int chosen = Chosen(s);
        if (chosen < 0)
        {
            ToolStatus.Flash(GlassHouseRules.NoSeedPrompt, 2.5f);
            return;
        }
        GlassHouseRules.Tray sow = GlassHouseRules.Trays[chosen];
        Inventory.Player.Remove(sow.seedId, 1);
        s.jobItem = sow.cropId;
        s.jobHours = 0f;
        s.cropDamage = 0f;
        s.cropFlags = 0;
        s.picksTaken = 0;
        s.jobCount = chosen;
        wood.NotifyChanged(s);
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
        ToolStatus.Flash($"Sowed a tray of {sow.transplantName} - ready in about {sow.days:0} days under glass", 3f);
    }

    public static void Secondary(WoodPileState s)
    {
        if (HasTray(s))
            return;
        List<int> carried = Carried();
        if (carried.Count < 2)
            return;
        int at = carried.IndexOf(Chosen(s));
        s.jobCount = carried[(at + 1) % carried.Count];
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
        ToolStatus.Flash(GlassHouseRules.SowPrompt(GlassHouseRules.Trays[s.jobCount]), 2f);
    }

    static void Clear(WoodPileState s)
    {
        s.jobItem = null;
        s.jobHours = 0f;
        s.cropDamage = 0f;
        s.cropFlags = 0;
        s.picksTaken = 0;
    }

    // --- The look: a plank table with a tray of soil, and the seedlings in it growing taller and greener as they come on. ---

    public static void Build(Transform parent, WoodPileState state, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        WoodManager wood = WoodManager.Instance;
        WoodPileState house = wood != null ? wood.HouseAt(state.position) : null;
        float width = house != null && WoodManager.IsGlassHouse(house.kind) ? GlassHouseRules.BenchWidth(WoodManager.GlassSizeOf(house.kind)) : 1.8f;
        float d = GlassHouseRules.BenchDepth, h = GlassHouseRules.BenchHeight;
        boxCenter = new Vector3(0f, h / 2f, 0f);
        boxSize = new Vector3(width, h, d);

        GameObject prefab = Resources.Load<GameObject>("Art/structure_seedlingbench");
        if (prefab != null)
        {
            GameObject art = Object.Instantiate(prefab, parent, false);
            art.name = prefab.name;
            art.transform.localScale = new Vector3(width / 1.8f, 1f, 1f); // the art is a 1.8 m bench
            foreach (Collider c in art.GetComponentsInChildren<Collider>())
                Object.DestroyImmediate(c);
            pieces?.Add(art);
        }
        else
        {
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, h - 0.02f, 0f), new Vector3(width, 0.04f, d)), Wood);
            foreach (float x in new[] { -width / 2f + 0.05f, width / 2f - 0.05f })
                foreach (float z in new[] { -d / 2f + 0.05f, d / 2f - 0.05f })
                    Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(x, (h - 0.04f) / 2f, z), new Vector3(0.05f, h - 0.04f, 0.05f)), Wood);
        }
        if (!HasTray(state))
            return;
        // The tray and its seedlings, in rows.
        float trayW = Mathf.Min(width - 0.2f, 1.2f), trayD = d - 0.15f;
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, h + 0.03f, 0f), new Vector3(trayW, 0.06f, trayD)), Soil);
        float grown = Mathf.Clamp01(state.jobHours / Mathf.Max(1f, TrayOf(state).days));
        Color leaf = Dead(state) ? Brown : Leaf;
        float tall = Mathf.Lerp(0.02f, 0.12f, grown), wide = Mathf.Lerp(0.015f, 0.06f, grown);
        for (int ix = 0; ix < 6; ix++)
            for (int iz = 0; iz < 3; iz++)
            {
                float x = -trayW / 2f + trayW * (ix + 0.5f) / 6f, z = -trayD / 2f + trayD * (iz + 0.5f) / 3f;
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(x, h + 0.06f + tall / 2f, z), new Vector3(wide, tall, wide)), leaf);
            }
    }

    static GameObject Piece(Transform parent, List<GameObject> pieces, PrimitiveType shape, Vector3 position, Vector3 scale)
    {
        GameObject piece = GameObject.CreatePrimitive(shape);
        Object.DestroyImmediate(piece.GetComponent<Collider>());
        piece.transform.SetParent(parent, false);
        piece.transform.localPosition = position;
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
