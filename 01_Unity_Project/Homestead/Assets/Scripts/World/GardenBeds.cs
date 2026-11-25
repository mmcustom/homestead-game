using System.Collections.Generic;
using UnityEngine;

// The Garden Bed in the World (Gardening_Basics.md, slice GB1; GardenRules has every number). A PlacedStructure of kind GardenBed hands its look, prompts, E and R to this class,
// and SpoilageManager's game hour calls Hour, so the crops grow while the player is away or asleep as the Compost Bin and Drying Rack do.
//   Not dug      the Shovel (hold click, AxeTool.UpdateDigging) turns it; R with a Shovel takes the marked-out bed away.
//   Bare         with the Hoe in hand, E plants the chosen seed and R picks the next seed carried. R with a Shovel fills the bed in.
//   Growing      E with the Bucket or Watering Can in hand pours water on it; beans: R switches between picking green and leaving them to dry.
//   Ready        E harvests (what does not fit in the pack is left on the ground); picked crops (tomatoes, beans, kale) give again every few days.
//   Dead         E clears the dead plants.
// THE SWAP POINT for the plants: Resources/Art/crop_<crop id>_<stage>.prefab (stage: sprouting, growing, maturing, ready, dead), placed at each plant's spot, replaces that crop's
// primitive plant for that stage; Resources/Art/structure_gardenbed.prefab replaces the dug soil and Resources/Art/structure_gardenbed_marked.prefab the stakes of an undug bed.
public static class GardenBeds
{
    static readonly Color SoilDry = new Color(0.33f, 0.24f, 0.16f);
    static readonly Color SoilWet = new Color(0.13f, 0.09f, 0.06f);
    static readonly Color StakeColor = new Color(0.58f, 0.46f, 0.30f);
    static readonly Color StringColor = new Color(0.85f, 0.80f, 0.68f);
    static readonly Color Leaf = new Color(0.22f, 0.44f, 0.15f);
    static readonly Color PaleLeaf = new Color(0.48f, 0.66f, 0.26f);
    static readonly Color BlueLeaf = new Color(0.20f, 0.36f, 0.30f);
    static readonly Color Straw = new Color(0.72f, 0.62f, 0.36f);
    static readonly Color DeadBrown = new Color(0.36f, 0.27f, 0.16f);
    static readonly Color TomatoRed = new Color(0.78f, 0.12f, 0.07f);
    static readonly Color SquashOrange = new Color(0.86f, 0.48f, 0.10f);
    static readonly Color CornYellow = new Color(0.90f, 0.78f, 0.30f);
    static readonly Color CarrotOrange = new Color(0.92f, 0.45f, 0.10f);

    // The seed the Hoe plants next (R picks it). An item id; null picks the first seed carried.
    public static string SelectedSeed;

    static InventoryManager Inventory => InventoryManager.Instance;
    static string EquippedId => Inventory != null && Inventory.EquippedTool != null ? Inventory.EquippedTool.Id : null;
    static bool HoeEquipped => EquippedId == GardenRules.HoeId;
    static bool ShovelEquipped => EquippedId != null && ToolRules.IsShovel(EquippedId);
    static bool WateringEquipped => EquippedId == GardenRules.WateringCanId || EquippedId == WaterSource.BucketItemId;

    public static bool IsDug(WoodPileState s) => s.digProgress >= 1f;
    public static GardenRules.Crop CropOf(WoodPileState s) => GardenRules.Find(s.jobItem);
    public static bool DryBeans(WoodPileState s) => (s.cropFlags & GardenRules.FlagDryBeans) != 0;
    public static bool Dead(WoodPileState s) => (s.cropFlags & GardenRules.FlagDead) != 0;
    public static GardenRules.Stage StageOf(WoodPileState s) => GardenRules.StageOf(CropOf(s), DryBeans(s), s.jobHours, s.picksTaken, Dead(s));
    static int PicksReady(WoodPileState s) => CropOf(s) == null || Dead(s) ? 0 : GardenRules.PicksReady(CropOf(s), DryBeans(s), s.jobHours, s.picksTaken);

    // --- The game hour ---

    // Every dug bed: rain and warmth change its soil, and a living crop grows, or is hurt by frost, drought or heat. Called from SpoilageManager's hour.
    public static void Hour(float airC, float rainMm)
    {
        WoodManager wood = WoodManager.Instance;
        if (wood == null)
            return;
        int frostKilled = 0, droughtKilled = 0, bolted = 0;
        foreach (WoodPileState bed in wood.Piles)
        {
            if (bed.kind != PileKind.GardenBed || !IsDug(bed))
                continue;
            GardenRules.Crop crop = CropOf(bed);
            string before = LookKey(bed);
            bool ready = crop != null && bed.jobHours >= GardenRules.DaysToReady(crop, DryBeans(bed));
            bool frost = crop != null && GardenRules.FrostDamage(crop, airC, ready) > 0f;
            GardenRules.HourResult r = GardenRules.Hour(crop, bed.jobHours, bed.cropDamage, bed.soilMoisture, DryBeans(bed), Dead(bed), airC, rainMm);
            bed.soilMoisture = r.moisture;
            bed.jobHours = r.growingDays;
            bed.cropDamage = r.damage;
            if (r.died && !Dead(bed))
            {
                bed.cropFlags |= GardenRules.FlagDead;
                if (r.bolted)
                    bolted++;
                else if (frost)
                    frostKilled++;
                else
                    droughtKilled++;
            }
            if (LookKey(bed) != before)
                wood.NotifyChanged(bed);
        }
        if (frostKilled > 0)
            ToolStatus.Banner(frostKilled == 1 ? "Frost killed the crop in a garden bed" : $"Frost killed the crops in {frostKilled} garden beds", 8f);
        if (droughtKilled > 0)
            ToolStatus.Banner(droughtKilled == 1 ? "A garden bed's crop died - the soil dried out" : $"{droughtKilled} garden beds' crops died - the soil dried out", 8f);
        if (bolted > 0)
            ToolStatus.Banner("Lettuce bolted in the heat and went to seed", 6f);
    }

    // What the bed looks like, coarsely: redrawn only when this changes (stage, plant size in tenths, soil wet or dry, a pick waiting).
    static string LookKey(WoodPileState s) =>
        $"{StageOf(s)}|{Mathf.FloorToInt(GardenRules.SizeOf(CropOf(s), DryBeans(s), s.jobHours) * 10f)}|{(s.soilMoisture >= 0.3f ? 1 : 0)}|{(PicksReady(s) > 0 ? 1 : 0)}";

    // --- Prompts ---

    // The bed's status, shown above the key lines with no key (IPromptHeader); the key lines (ActionE, SecondaryPrompt) say only what each key does
    // (Mike, 2026-11-25: no "E ..." inside a line the HUD already starts with [E]).
    public static string Header(WoodPileState s)
    {
        if (!IsDug(s))
            return ShovelEquipped ? $"Garden Bed - not dug yet\nHold click to dig it ({s.digProgress * 100f:0}%)" : GardenRules.Undug;
        GardenRules.Crop crop = CropOf(s);
        string soil = GardenRules.MoistureText(s.soilMoisture);
        if (crop == null)
        {
            string hint = !HoeEquipped ? "\n" + GardenRules.NeedHoe : SeedToPlant() == null ? "\n" + GardenRules.NoSeed : "";
            return $"Garden Bed  (bare, {soil}){hint}";
        }
        bool dry = DryBeans(s);
        GardenRules.Stage stage = StageOf(s);
        string name = crop.id == GardenRules.BeansCropId ? (dry ? "Beans (left to dry)" : "Beans (picked green)") : crop.name;
        if (stage == GardenRules.Stage.Dead)
            return $"Garden Bed  ({name}, dead)";
        string status = GardenRules.StageText(stage) + GardenRules.DaysLeftText(crop, dry, s.jobHours) + GardenRules.DamageText(s.cropDamage);
        return $"Garden Bed  ({name}: {status}; {soil})";
    }

    // What E does now, in Interact's order (water, plant, clear, harvest), or null when E does nothing.
    public static string ActionE(WoodPileState s)
    {
        if (!IsDug(s))
            return null;
        GardenRules.Crop crop = CropOf(s);
        bool dead = crop != null && StageOf(s) == GardenRules.Stage.Dead;
        if (WateringEquipped && WaterCarried() > 0 && s.soilMoisture < 0.98f && !dead)
            return "Pour water on it";
        if (crop == null)
        {
            GardenRules.Crop next = HoeEquipped ? SeedToPlant() : null;
            return next != null ? GardenRules.PlantPrompt(next, Inventory.Player.Count(next.seedId), GardenRules.TimingWarning(next, CalendarRules.Current, TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0)) : null;
        }
        if (dead)
            return "Clear the dead plants";
        int ready = PicksReady(s);
        return ready > 0 ? $"Harvest {ProduceName(crop, DryBeans(s))} x{GardenRules.PickYield(crop, DryBeans(s), s.cropDamage) * ready}" : null;
    }

    public static string Prompt(WoodPileState s) => Header(s) + (ActionE(s) is string e ? "\n" + e : "");

    public static string SecondaryPrompt(WoodPileState s)
    {
        GardenRules.Crop crop = CropOf(s);
        if (ShovelEquipped && (crop == null || !IsDug(s)))
            return IsDug(s) ? "Fill the bed in" : "Take the marked-out bed away";
        if (crop == null && IsDug(s) && HoeEquipped && CarriedCrops().Count > 1)
            return "Pick another seed";
        if (crop != null && crop.id == GardenRules.BeansCropId && !Dead(s) && s.picksTaken == 0 && PicksReady(s) == 0)
            return DryBeans(s) ? "Pick these beans green instead" : "Leave these beans to dry";
        return "";
    }

    static string ProduceName(GardenRules.Crop crop, bool dry) => ItemDatabase.Get(GardenRules.ProduceOf(crop, dry))?.DisplayName ?? crop.name;

    // --- E ---

    public static void Interact(WoodPileState s, Transform view)
    {
        WoodManager wood = WoodManager.Instance;
        if (Inventory == null || wood == null)
            return;
        if (!IsDug(s))
        {
            ToolStatus.Flash(ShovelEquipped ? "Hold click with the Shovel to dig the bed" : "Equip the Shovel and hold click on the bed to dig it", 3f);
            return;
        }
        GardenRules.Crop crop = CropOf(s);
        if (WateringEquipped && s.soilMoisture < 0.98f && WaterCarried() > 0 && (crop == null || !Dead(s)))
        {
            Water(s);
            return;
        }
        if (crop == null)
        {
            Plant(s);
            return;
        }
        if (Dead(s))
        {
            ClearCrop(s);
            wood.NotifyChanged(s);
            ToolStatus.Flash("Cleared the dead plants - the bed is ready to plant again", 3f);
            return;
        }
        int ready = PicksReady(s);
        if (ready > 0)
        {
            Harvest(s, crop, ready, view);
            return;
        }
        ToolStatus.Flash($"{crop.name}: {GardenRules.StageText(StageOf(s))}{GardenRules.DaysLeftText(crop, DryBeans(s), s.jobHours)} ({GardenRules.HardinessText(crop.hardiness)}).", 4f);
    }

    // Every litre of water carried, raw or purified.
    static int WaterCarried() => Inventory == null ? 0 : WaterQualities.LitresCarried(Inventory.Player);

    // The worst water is poured first, so the good water stays for drinking.
    static readonly string[] PourOrder = { "water_unsafe", "water_questionable", "water_good", "water_excellent", Cooking.PurifiedWaterId };

    static void Water(WoodPileState s)
    {
        int want = Mathf.Max(1, GardenRules.LitresToSoak(s.soilMoisture));
        int poured = 0;
        foreach (string id in PourOrder)
        {
            if (poured >= want)
                break;
            poured += Inventory.RemoveFromPlayer(id, Mathf.Min(want - poured, Inventory.Player.Count(id)));
        }
        if (poured <= 0)
            return;
        s.soilMoisture = GardenRules.AfterWatering(s.soilMoisture, poured);
        WoodManager.Instance.NotifyChanged(s);
        ToolEvents.RaiseFilled(); // the hands tip the vessel
        ToolStatus.Flash($"Poured {poured} L on the bed - {GardenRules.MoistureText(s.soilMoisture)}", 3f);
    }

    // The crops whose seed is carried in the amount one square takes, in GardenRules' order.
    static List<GardenRules.Crop> CarriedCrops()
    {
        var list = new List<GardenRules.Crop>();
        if (Inventory == null)
            return list;
        foreach (GardenRules.Crop crop in GardenRules.Crops)
            if (Inventory.Player.Count(crop.seedId) >= crop.seedsPerSquare)
                list.Add(crop);
        return list;
    }

    // The chosen seed if it is carried, otherwise the first one carried.
    static GardenRules.Crop SeedToPlant()
    {
        List<GardenRules.Crop> carried = CarriedCrops();
        foreach (GardenRules.Crop crop in carried)
            if (crop.seedId == SelectedSeed)
                return crop;
        return carried.Count > 0 ? carried[0] : null;
    }

    static void Plant(WoodPileState s)
    {
        if (!HoeEquipped)
        {
            ToolStatus.Flash(GardenRules.NeedHoe, 3f);
            return;
        }
        GardenRules.Crop crop = SeedToPlant();
        if (crop == null)
        {
            ToolStatus.Flash(GardenRules.NoSeed, 4f);
            return;
        }
        if (Inventory.RemoveFromPlayer(crop.seedId, crop.seedsPerSquare) < crop.seedsPerSquare)
            return;
        ClearCrop(s);
        s.jobItem = crop.id;
        SelectedSeed = crop.seedId;
        WoodManager.Instance.NotifyChanged(s);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDig(s.position);
        ToolStatus.Flash(GardenRules.Planted(crop), 4f);
    }

    static void ClearCrop(WoodPileState s)
    {
        s.jobItem = null;
        s.jobHours = 0f;
        s.cropDamage = 0f;
        s.cropFlags = 0;
        s.picksTaken = 0;
    }

    static void Harvest(WoodPileState s, GardenRules.Crop crop, int picks, Transform view)
    {
        bool dry = DryBeans(s);
        string produce = GardenRules.ProduceOf(crop, dry);
        int units = GardenRules.PickYield(crop, dry, s.cropDamage) * picks;
        ItemDefinition item = ItemDatabase.Get(produce);
        int fits = item != null ? Mathf.Min(units, Inventory.Player.SpaceFor(item)) : 0;
        int took = fits > 0 ? Inventory.AddToPlayer(produce, fits) : 0;
        if (units - took > 0)
            DroppedItems.Place(produce, units - took, s.position, view);
        s.picksTaken += picks;
        int left = GardenRules.Picks(crop, dry) - s.picksTaken;
        string name = item != null ? item.DisplayName : produce;
        if (GardenRules.Spent(crop, dry, s.picksTaken))
            ClearCrop(s);
        WoodManager.Instance.NotifyChanged(s);
        string message = GardenRules.Harvested(units, name, left);
        if (units - took > 0)
            message += $" ({units - took} left on the ground - no room in the pack)";
        ToolStatus.Flash(message, 4f);
    }

    // --- R ---

    public static void Secondary(WoodPileState s)
    {
        WoodManager wood = WoodManager.Instance;
        if (Inventory == null || wood == null)
            return;
        GardenRules.Crop crop = CropOf(s);
        if (ShovelEquipped)
        {
            if (crop != null && IsDug(s))
            {
                ToolStatus.Flash("Harvest or clear the crop first (E)", 3f);
                return;
            }
            bool dug = IsDug(s);
            wood.RemovePile(s);
            ToolStatus.Flash(dug ? "Filled the garden bed in" : "Took the marked-out bed away", 3f);
            return;
        }
        if (crop == null && IsDug(s) && HoeEquipped)
        {
            List<GardenRules.Crop> carried = CarriedCrops();
            if (carried.Count == 0)
            {
                ToolStatus.Flash(GardenRules.NoSeed, 4f);
                return;
            }
            int at = carried.IndexOf(SeedToPlant());
            GardenRules.Crop next = carried[(at + 1) % carried.Count];
            SelectedSeed = next.seedId;
            ToolStatus.Flash($"Next to plant: {next.name}", 2.5f);
            return;
        }
        if (crop != null && crop.id == GardenRules.BeansCropId && !Dead(s) && s.picksTaken == 0 && PicksReady(s) == 0)
        {
            s.cropFlags ^= GardenRules.FlagDryBeans;
            wood.NotifyChanged(s);
            ToolStatus.Flash(DryBeans(s) ? $"These beans will be left to dry on the plant (about {(int)GardenRules.DryBeanDays} growing days); dry beans keep a year"
                                         : $"These beans will be picked green (about {(int)crop.days} growing days, picked {crop.picks} times)", 4f);
            return;
        }
        ToolStatus.Flash(crop == null ? "Equip the Hoe to plant, or a Shovel to fill the bed in" : "Nothing to do here with R", 2.5f);
    }

    // --- The look (primitives, no colliders; the same builder serves the placement ghost) ---

    static readonly Dictionary<string, GameObject> ArtCache = new Dictionary<string, GameObject>();

    static GameObject Art(string id)
    {
        if (!ArtCache.TryGetValue(id, out GameObject prefab))
        {
            prefab = Resources.Load<GameObject>("Art/" + id);
            ArtCache[id] = prefab;
        }
        return prefab;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ClearCache() => ArtCache.Clear();

    public static void Build(Transform parent, WoodPileState s, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        float h = GardenRules.BedSize / 2f;
        boxCenter = new Vector3(0f, GardenRules.BoxHeight / 2f, 0f);
        boxSize = new Vector3(GardenRules.BedSize, GardenRules.BoxHeight, GardenRules.BedSize);
        if (!IsDug(s))
        {
            if (!Placed(parent, pieces, Art("structure_gardenbed_marked"), Vector3.zero))
            {
                foreach (Vector2 c in new[] { new Vector2(-h, -h), new Vector2(h, -h), new Vector2(h, h), new Vector2(-h, h) })
                    Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(c.x, 0.15f, c.y), Vector3.zero, new Vector3(0.04f, 0.3f, 0.04f)), StakeColor);
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.25f, -h), Vector3.zero, new Vector3(h * 2f, 0.008f, 0.008f)), StringColor);
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.25f, h), Vector3.zero, new Vector3(h * 2f, 0.008f, 0.008f)), StringColor);
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(-h, 0.25f, 0f), Vector3.zero, new Vector3(0.008f, 0.008f, h * 2f)), StringColor);
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(h, 0.25f, 0f), Vector3.zero, new Vector3(0.008f, 0.008f, h * 2f)), StringColor);
            }
            // A partly dug bed shows the turned soil growing in from one side.
            if (s.digProgress > 0f)
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(-h + h * s.digProgress, 0.02f, 0f), Vector3.zero, new Vector3(h * 2f * s.digProgress, 0.05f, h * 2f - 0.05f)), SoilDry);
            return;
        }

        if (Placed(parent, pieces, Art("structure_gardenbed"), Vector3.zero))
            WetLook(parent.GetChild(parent.childCount - 1), s.soilMoisture);
        else
        {
            Color soil = Color.Lerp(SoilDry, SoilWet, Mathf.Clamp01(s.soilMoisture / 0.7f));
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.04f, 0f), Vector3.zero, new Vector3(h * 2f, 0.09f, h * 2f)), soil);
            for (int i = -1; i <= 1; i++) // three low ridges of turned soil
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(i * h * 0.62f, 0.09f, 0f), Vector3.zero, new Vector3(0.22f, 0.04f, h * 2f - 0.08f)), soil * 0.92f);
        }

        GardenRules.Crop crop = CropOf(s);
        if (crop == null)
            return;
        bool dry = DryBeans(s);
        GardenRules.Stage stage = StageOf(s);
        float size = GardenRules.SizeOf(crop, dry, s.jobHours);
        bool ready = stage == GardenRules.Stage.Ready;
        bool dead = stage == GardenRules.Stage.Dead;
        GameObject art = Art($"crop_{crop.id}_{stage.ToString().ToLowerInvariant()}");
        foreach (Vector3 spot in Spots(crop, h))
        {
            if (Placed(parent, pieces, art, spot))
                continue;
            Plant(parent, pieces, crop, spot, size, ready, dead, dry, PicksReady(s) > 0);
        }
    }

    // Where a square's plants stand: in a grid sized for the crop.
    static IEnumerable<Vector3> Spots(GardenRules.Crop crop, float h)
    {
        int n;
        switch (crop.id)
        {
            case "squash": n = 1; break;
            case "potatoes":
            case "tomatoes":
            case "kale": n = 2; break;
            case "carrots": n = 4; break;
            default: n = 3; break;
        }
        float step = h * 2f / n;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                yield return new Vector3(-h + step * (i + 0.5f), 0.08f, -h + step * (j + 0.5f));
    }

    // One plant, drawn by crop: a bush, a stalk, a vine or a rosette, growing with size; ripe fruit shows on the ready ones.
    static void Plant(Transform parent, List<GameObject> pieces, GardenRules.Crop crop, Vector3 at, float size, bool ready, bool dead, bool dryBeans, bool pickWaiting)
    {
        Color leaf = dead ? DeadBrown : Leaf;
        switch (crop.id)
        {
            case "potatoes":
            {
                Color c = dead ? DeadBrown : ready ? Straw : leaf; // the vines die back when the potatoes are ready
                Tinted(Piece(parent, pieces, PrimitiveType.Sphere, at + Vector3.up * 0.18f * size, Vector3.zero, new Vector3(0.42f, 0.36f, 0.42f) * size), c);
                break;
            }
            case "field_corn":
            case "sweet_corn":
            {
                float tall = 2.0f * size;
                Color c = dead ? DeadBrown : ready && crop.id == "field_corn" ? Straw : leaf;
                Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, at + Vector3.up * tall / 2f, Vector3.zero, new Vector3(0.05f, tall / 2f, 0.05f)), c);
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, at + Vector3.up * tall * 0.55f, new Vector3(0f, 35f, 30f), new Vector3(0.5f * size, 0.01f, 0.07f)), c);
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, at + Vector3.up * tall * 0.35f, new Vector3(0f, -55f, -30f), new Vector3(0.5f * size, 0.01f, 0.07f)), c);
                if (size > 0.8f && !dead)
                    Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, at + new Vector3(0.05f, tall * 0.5f, 0f), new Vector3(0f, 0f, 20f), new Vector3(0.06f, 0.1f, 0.06f)), ready ? CornYellow : PaleLeaf);
                break;
            }
            case "beans":
            {
                Color c = dead ? DeadBrown : ready && dryBeans ? Straw : leaf;
                Tinted(Piece(parent, pieces, PrimitiveType.Sphere, at + Vector3.up * 0.15f * size, Vector3.zero, new Vector3(0.3f, 0.32f, 0.3f) * size), c);
                if (pickWaiting && !dryBeans)
                    for (int k = 0; k < 3; k++)
                        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, at + new Vector3(0.1f * (k - 1), 0.12f, 0.12f), new Vector3(0f, 0f, 75f), new Vector3(0.015f, 0.05f, 0.015f)), PaleLeaf);
                break;
            }
            case "tomatoes":
            {
                Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, at + Vector3.up * 0.6f, Vector3.zero, new Vector3(0.025f, 0.6f, 0.025f)), StakeColor); // the stake
                Tinted(Piece(parent, pieces, PrimitiveType.Sphere, at + Vector3.up * 0.55f * size, Vector3.zero, new Vector3(0.45f, 1.0f, 0.45f) * size), leaf);
                if ((ready || pickWaiting) && !dead)
                    for (int k = 0; k < 5; k++)
                        Tinted(Piece(parent, pieces, PrimitiveType.Sphere, at + new Vector3(Mathf.Cos(k * 1.3f) * 0.18f, 0.3f + 0.12f * k, Mathf.Sin(k * 1.3f) * 0.18f), Vector3.zero, Vector3.one * 0.07f), TomatoRed);
                break;
            }
            case "lettuce":
                Tinted(Piece(parent, pieces, PrimitiveType.Sphere, at + Vector3.up * 0.06f * size, Vector3.zero, new Vector3(0.32f, 0.18f, 0.32f) * size), dead ? DeadBrown : PaleLeaf);
                break;
            case "carrots":
                Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, at + Vector3.up * 0.12f * size, Vector3.zero, new Vector3(0.12f, 0.12f, 0.12f) * size), leaf);
                if (ready)
                    Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, at + Vector3.up * 0.01f, Vector3.zero, new Vector3(0.035f, 0.02f, 0.035f)), CarrotOrange);
                break;
            case "squash":
            {
                Tinted(Piece(parent, pieces, PrimitiveType.Sphere, at + Vector3.up * 0.15f * size, Vector3.zero, new Vector3(1.1f, 0.3f, 1.1f) * size), leaf);
                if (ready && !dead)
                    for (int k = 0; k < 3; k++)
                        Tinted(Piece(parent, pieces, PrimitiveType.Sphere, at + new Vector3(Mathf.Cos(k * 2.1f) * 0.35f, 0.12f, Mathf.Sin(k * 2.1f) * 0.35f), Vector3.zero, new Vector3(0.24f, 0.2f, 0.22f)), SquashOrange);
                break;
            }
            default: // kale
                Tinted(Piece(parent, pieces, PrimitiveType.Sphere, at + Vector3.up * 0.25f * size, Vector3.zero, new Vector3(0.42f, 0.5f, 0.42f) * size), dead ? DeadBrown : BlueLeaf);
                break;
        }
    }

    // The dug-bed art darkens as the soil gets wetter, as the primitive soil's tint does (2026-11-25).
    static void WetLook(Transform bed, float moisture)
    {
        float k = Mathf.Lerp(1f, 0.55f, Mathf.Clamp01(moisture / 0.7f));
        var block = new MaterialPropertyBlock();
        foreach (Renderer r in bed.GetComponentsInChildren<Renderer>())
        {
            r.GetPropertyBlock(block);
            block.SetColor("_BaseColor", new Color(k, k, k, 1f));
            r.SetPropertyBlock(block);
        }
    }

    static bool Placed(Transform parent, List<GameObject> pieces, GameObject prefab, Vector3 at)
    {
        if (prefab == null)
            return false;
        GameObject made = Object.Instantiate(prefab, parent, false);
        made.transform.localPosition = at;
        made.transform.localRotation = Quaternion.Euler(0f, Mathf.Abs(at.x * 997f + at.z * 613f) % 360f, 0f);
        foreach (Collider c in made.GetComponentsInChildren<Collider>(true))
            Object.DestroyImmediate(c);
        pieces?.Add(made);
        return true;
    }

    static GameObject Tinted(GameObject piece, Color color)
    {
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        piece.GetComponent<MeshRenderer>().SetPropertyBlock(block);
        return piece;
    }

    static GameObject Piece(Transform parent, List<GameObject> pieces, PrimitiveType shape, Vector3 position, Vector3 euler, Vector3 scale)
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
}
