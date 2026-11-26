using System.Collections.Generic;
using UnityEngine;

// The Garden Bed in the World (Gardening_Basics.md, slice GB1; GardenRules has every number). A PlacedStructure of kind GardenBed hands its look, prompts, E and R to this class,
// and SpoilageManager's game hour calls Hour, so the crops grow while the player is away or asleep as the Compost Bin and Drying Rack do.
//   Not dug      the Shovel (hold click, AxeTool.UpdateDigging) turns it; R with a Shovel takes the marked-out bed away.
//   Bare         with the Hoe in hand, E plants the chosen seed and R picks the next seed carried. R with a Shovel fills the bed in.
//   Growing      E with the Bucket or Watering Can in hand pours water on it; beans: R switches between picking green and leaving them to dry.
//   Ready        E harvests (what does not fit in the pack is left on the ground); picked crops (tomatoes, beans, kale) give again every few days.
//   Dead         E clears the dead plants.
// Slice GB2 (Mike, 2026-11-25, Gardening_Basics.md 9b): each crop taken to its end wears the soil (soilSpent), E spreads Fertilizer when nothing else is to do;
// a bed is raised (built as a Raised Bed Site, or R with the Hammer and 2 Lumber), and R lays a Row Cover over it or takes it off. At 6 pm the frost alert
// says which beds tonight's expected low would hurt.
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
    public static bool Raised(WoodPileState s) => (s.cropFlags & GardenRules.FlagRaised) != 0;
    public static bool Covered(WoodPileState s) => (s.cropFlags & GardenRules.FlagCovered) != 0;
    static bool HammerEquipped => EquippedId == WoodManager.HammerId;
    static int Carried(string itemId) => Inventory != null ? Inventory.Player.Count(itemId) : 0;
    static string BedName(WoodPileState s) => Raised(s) ? "Raised Bed" : "Garden Bed";
    const float RaisedLift = 0.15f;   // a raised bed's soil and plants sit this much higher, inside its frame
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
        // The frost alert (GB2): once, at the alert hour, against the night's expected low.
        bool alertHour = TimeManager.Instance != null && TimeManager.Instance.Hour == GardenRules.AlertHour && WeatherManager.Instance != null;
        float lowTonight = alertHour ? WeatherManager.Instance.ExpectedNightLowC() : 99f;
        int atRiskUncovered = 0, atRiskCovered = 0;
        foreach (WoodPileState bed in wood.Piles)
        {
            if (bed.kind != PileKind.GardenBed || !IsDug(bed))
                continue;
            GardenRules.Crop crop = CropOf(bed);
            string before = LookKey(bed);
            bool ready = crop != null && bed.jobHours >= GardenRules.DaysToReady(crop, DryBeans(bed));
            bool covered = Covered(bed);
            bool frost = crop != null && GardenRules.FrostDamage(crop, airC + (covered ? GardenRules.CoverProtectionC : 0f), ready) > 0f;
            if (alertHour && crop != null && !Dead(bed) && GardenRules.FrostRisk(crop, ready, covered, lowTonight))
            {
                if (covered) atRiskCovered++;
                else atRiskUncovered++;
            }
            GardenRules.HourResult r = GardenRules.Hour(crop, bed.jobHours, bed.cropDamage, bed.soilMoisture, DryBeans(bed), Dead(bed), airC, rainMm, Raised(bed), covered);
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
        if (atRiskUncovered + atRiskCovered > 0)
            ToolStatus.Banner(GardenRules.FrostAlert(atRiskUncovered, atRiskCovered, lowTonight), 12f);
    }

    // What the bed looks like, coarsely: redrawn only when this changes (stage, plant size in tenths, soil wet or dry, a pick waiting).
    static string LookKey(WoodPileState s) =>
        $"{s.cropFlags}|{StageOf(s)}|{Mathf.FloorToInt(GardenRules.SizeOf(CropOf(s), DryBeans(s), s.jobHours) * 10f)}|{(s.soilMoisture >= 0.3f ? 1 : 0)}|{(PicksReady(s) > 0 ? 1 : 0)}";

    // --- Prompts ---

    // The bed's status, shown above the key lines with no key (IPromptHeader); the key lines (ActionE, SecondaryPrompt) say only what each key does
    // (Mike, 2026-11-25: no "E ..." inside a line the HUD already starts with [E]).
    public static string Header(WoodPileState s)
    {
        if (!IsDug(s))
        {
            if (Raised(s))
                return ShovelEquipped ? $"Raised Bed - empty frame\nHold click to fill it with soil ({s.digProgress * 100f:0}%)" : GardenRules.RaisedUnfilled;
            return ShovelEquipped ? $"Garden Bed - not dug yet\nHold click to dig it ({s.digProgress * 100f:0}%)" : GardenRules.Undug;
        }
        GardenRules.Crop crop = CropOf(s);
        string soil = GardenRules.MoistureText(s.soilMoisture);
        // The second line: the soil's fertility and the Row Cover (GB2).
        string bed = GardenRules.FertilityText(s.soilSpent) + (Covered(s) ? "; under a Row Cover" : "");
        if (crop == null)
        {
            string hint = !HoeEquipped ? "\n" + GardenRules.NeedHoe : SeedToPlant() == null ? "\n" + GardenRules.NoSeed : "";
            return $"{BedName(s)}  (bare, {soil})\n{bed}{hint}";
        }
        bool dry = DryBeans(s);
        GardenRules.Stage stage = StageOf(s);
        string name = crop.id == GardenRules.BeansCropId ? (dry ? "Beans (left to dry)" : "Beans (picked green)") : crop.name;
        if (stage == GardenRules.Stage.Dead)
            return $"{BedName(s)}  ({name}, dead)\n{bed}";
        string status = GardenRules.StageText(stage) + GardenRules.DaysLeftText(crop, dry, s.jobHours) + GardenRules.DamageText(s.cropDamage);
        return $"{BedName(s)}  ({name}: {status}; {soil})\n{bed}";
    }

    // Fertilizer goes on when the soil is worn and nothing else is to be done with E (GB2).
    static bool CanFertilize(WoodPileState s) => IsDug(s) && s.soilSpent > 0.01f && Carried(GardenRules.FertilizerId) > 0 && !Dead(s);

    static string FertilizePrompt(WoodPileState s) =>
        $"Spread Fertilizer  ({GardenRules.FertilizerToRich(s.soilSpent)} to make it rich; you carry {Carried(GardenRules.FertilizerId)})";

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
            if (next != null)
                return GardenRules.PlantPrompt(next, Inventory.Player.Count(next.seedId), GardenRules.TimingWarning(next, CalendarRules.Current, TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0));
            return CanFertilize(s) ? FertilizePrompt(s) : null;
        }
        if (dead)
            return "Clear the dead plants";
        int ready = PicksReady(s);
        if (ready > 0)
            return $"Harvest {ProduceName(crop, DryBeans(s))} x{GardenRules.PickYield(crop, DryBeans(s), s.cropDamage, s.soilSpent) * ready}";
        return CanFertilize(s) ? FertilizePrompt(s) : null;
    }

    public static string Prompt(WoodPileState s) => Header(s) + (ActionE(s) is string e ? "\n" + e : "");

    // R, in Secondary's order: the Shovel takes the bed away; the Hammer raises it; the Hoe picks the seed; beans switch green or dry; then the Row Cover.
    public static string SecondaryPrompt(WoodPileState s)
    {
        GardenRules.Crop crop = CropOf(s);
        if (ShovelEquipped && (crop == null || !IsDug(s)))
            return Raised(s) ? $"Take the raised bed apart  ({GardenRules.RaisedBedLumber} Lumber back)" : IsDug(s) ? "Fill the bed in" : "Take the marked-out bed away";
        if (HammerEquipped && !Raised(s))
            return Carried(SawPitRules.LumberId) >= GardenRules.RaisedBedLumber ? $"Make it a raised bed  (uses {GardenRules.RaisedBedLumber} Lumber)"
                                                                                : $"Make it a raised bed  (needs {GardenRules.RaisedBedLumber} Lumber)";
        if (crop == null && IsDug(s) && HoeEquipped && CarriedCrops().Count > 1)
            return "Pick another seed";
        if (crop != null && crop.id == GardenRules.BeansCropId && !Dead(s) && s.picksTaken == 0 && PicksReady(s) == 0)
            return DryBeans(s) ? "Pick these beans green instead" : "Leave these beans to dry";
        if (Covered(s))
            return "Take the Row Cover off";
        if (IsDug(s) && Carried(GardenRules.RowCoverId) > 0)
            return "Lay a Row Cover over it  (keeps about 3 C of frost off)";
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
            if (!(HoeEquipped && SeedToPlant() != null) && CanFertilize(s))
                Fertilize(s);
            else
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
        if (CanFertilize(s))
        {
            Fertilize(s);
            return;
        }
        ToolStatus.Flash($"{crop.name}: {GardenRules.StageText(StageOf(s))}{GardenRules.DaysLeftText(crop, DryBeans(s), s.jobHours)} ({GardenRules.HardinessText(crop.hardiness)}).", 4f);
    }

    // One Fertilizer spread on the bed puts back a quarter of a spent soil (GB2).
    static void Fertilize(WoodPileState s)
    {
        if (Inventory.RemoveFromPlayer(GardenRules.FertilizerId, 1) < 1)
            return;
        s.soilSpent = GardenRules.AfterFertilizer(s.soilSpent);
        WoodManager.Instance.NotifyChanged(s);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDig(s.position);
        ToolStatus.Flash($"Spread Fertilizer on the bed - {GardenRules.FertilityText(s.soilSpent)}", 3f);
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
        s.cropFlags &= GardenRules.KeptFlags; // the bed's own flags (raised, covered) stay; the crop's go with it
        s.picksTaken = 0;
    }

    static void Harvest(WoodPileState s, GardenRules.Crop crop, int picks, Transform view)
    {
        bool dry = DryBeans(s);
        string produce = GardenRules.ProduceOf(crop, dry);
        int units = GardenRules.PickYield(crop, dry, s.cropDamage, s.soilSpent) * picks;
        ItemDefinition item = ItemDatabase.Get(produce);
        int fits = item != null ? Mathf.Min(units, Inventory.Player.SpaceFor(item)) : 0;
        int took = fits > 0 ? Inventory.AddToPlayer(produce, fits) : 0;
        if (units - took > 0)
            DroppedItems.Place(produce, units - took, s.position, view);
        s.picksTaken += picks;
        int left = GardenRules.Picks(crop, dry) - s.picksTaken;
        string name = item != null ? item.DisplayName : produce;
        if (GardenRules.Spent(crop, dry, s.picksTaken))
        {
            s.soilSpent = GardenRules.AfterCrop(crop, s.soilSpent); // a crop taken to its end wears the soil (beans put a little back)
            ClearCrop(s);
        }
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
            bool dug = IsDug(s), raised = Raised(s);
            // A raised bed's frame comes back as its Lumber, and a Row Cover on the bed comes back too (GB2).
            if (raised)
                Inventory.AddToPlayer(SawPitRules.LumberId, GardenRules.RaisedBedLumber);
            if (Covered(s))
                Inventory.AddToPlayer(GardenRules.RowCoverId, 1);
            wood.RemovePile(s);
            ToolStatus.Flash(raised ? $"Took the raised bed apart - {GardenRules.RaisedBedLumber} Lumber back" : dug ? "Filled the garden bed in" : "Took the marked-out bed away", 3f);
            return;
        }
        if (HammerEquipped && !Raised(s))
        {
            if (Inventory.Player.Count(SawPitRules.LumberId) < GardenRules.RaisedBedLumber)
            {
                ToolStatus.Flash($"A raised bed's frame takes {GardenRules.RaisedBedLumber} Lumber (saw it at the Saw Pit, or buy it)", 3f);
                return;
            }
            Inventory.RemoveFromPlayer(SawPitRules.LumberId, GardenRules.RaisedBedLumber);
            s.cropFlags |= GardenRules.FlagRaised;
            wood.NotifyChanged(s);
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayDig(s.position);
            ToolStatus.Flash("Built a frame round the bed: it is a raised bed now (warmer soil, and it drains)", 4f);
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
        if (Covered(s))
        {
            s.cropFlags &= ~GardenRules.FlagCovered;
            Inventory.AddToPlayer(GardenRules.RowCoverId, 1);
            wood.NotifyChanged(s);
            ToolStatus.Flash("Took the Row Cover off the bed", 2.5f);
            return;
        }
        if (IsDug(s) && Inventory.RemoveFromPlayer(GardenRules.RowCoverId, 1) > 0)
        {
            s.cropFlags |= GardenRules.FlagCovered;
            wood.NotifyChanged(s);
            ToolStatus.Flash("Laid a Row Cover over the bed: it keeps about 3 C of frost off (take it off in hot weather)", 4f);
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
        bool raised = Raised(s);
        float lift = raised ? RaisedLift : 0f;
        if (raised)
        {
            RaisedFrame(parent, pieces, h);
            boxCenter = new Vector3(0f, 0.2f, 0f);
            boxSize = new Vector3(GardenRules.BedSize + 0.1f, 0.4f, GardenRules.BedSize + 0.1f);
        }
        if (Covered(s))
            Cover(parent, pieces, h, lift);
        if (!IsDug(s) && raised)
        {
            // An empty frame: the soil rises in it as the Shovel fills it.
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.01f + 0.1f * s.digProgress, 0f), Vector3.zero, new Vector3(h * 2f - 0.04f, 0.02f + 0.2f * s.digProgress, h * 2f - 0.04f)), SoilDry);
            return;
        }
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

        if (Placed(parent, pieces, Art("structure_gardenbed"), Vector3.up * lift))
            WetLook(parent.GetChild(parent.childCount - 1), s.soilMoisture);
        else
        {
            Color soil = Color.Lerp(SoilDry, SoilWet, Mathf.Clamp01(s.soilMoisture / 0.7f));
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.04f + lift, 0f), Vector3.zero, new Vector3(h * 2f, 0.09f, h * 2f)), soil);
            for (int i = -1; i <= 1; i++) // three low ridges of turned soil
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(i * h * 0.62f, 0.09f + lift, 0f), Vector3.zero, new Vector3(0.22f, 0.04f, h * 2f - 0.08f)), soil * 0.92f);
        }
        if (raised) // the soil under a raised bed's lifted top, filling the frame down to the ground
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, lift / 2f, 0f), Vector3.zero, new Vector3(h * 2f - 0.04f, lift, h * 2f - 0.04f)), SoilWet);

        GardenRules.Crop crop = CropOf(s);
        if (crop == null)
            return;
        bool dry = DryBeans(s);
        GardenRules.Stage stage = StageOf(s);
        float size = GardenRules.SizeOf(crop, dry, s.jobHours);
        bool ready = stage == GardenRules.Stage.Ready;
        bool dead = stage == GardenRules.Stage.Dead;
        GameObject art = Art($"crop_{crop.id}_{stage.ToString().ToLowerInvariant()}");
        foreach (Vector3 flat in Spots(crop, h))
        {
            Vector3 spot = flat + Vector3.up * lift;
            if (Placed(parent, pieces, art, spot))
                continue;
            Plant(parent, pieces, crop, spot, size, ready, dead, dry, PicksReady(s) > 0);
        }
    }

    static readonly Color PlankColor = new Color(0.62f, 0.47f, 0.29f);
    static readonly Color CoverColor = new Color(0.90f, 0.90f, 0.86f);

    // A raised bed's frame: four boards round the square, 30 cm high (GB2). THE SWAP POINT: Resources/Art/structure_gardenbed_raised.prefab.
    static void RaisedFrame(Transform parent, List<GameObject> pieces, float h)
    {
        if (Placed(parent, pieces, Art("structure_gardenbed_raised"), Vector3.zero))
            return;
        foreach (float side in new[] { -1f, 1f })
        {
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.15f, side * (h + 0.025f)), Vector3.zero, new Vector3(h * 2f + 0.1f, 0.3f, 0.05f)), PlankColor);
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(side * (h + 0.025f), 0.15f, 0f), Vector3.zero, new Vector3(0.05f, 0.3f, h * 2f)), PlankColor);
        }
    }

    // A Row Cover: pale cloth over low hoops, draped to the ground at the sides (GB2). THE SWAP POINT: Resources/Art/structure_gardenbed_cover.prefab.
    static void Cover(Transform parent, List<GameObject> pieces, float h, float lift)
    {
        if (Placed(parent, pieces, Art("structure_gardenbed_cover"), Vector3.up * lift))
            return;
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, lift + 0.55f, 0f), Vector3.zero, new Vector3(h * 2f, 0.01f, h * 2f)), CoverColor);
        foreach (float side in new[] { -1f, 1f })
        {
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, lift + 0.275f, side * h), Vector3.zero, new Vector3(h * 2f, 0.55f, 0.01f)), CoverColor);
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(side * h, lift + 0.275f, 0f), Vector3.zero, new Vector3(0.01f, 0.55f, h * 2f)), CoverColor);
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
