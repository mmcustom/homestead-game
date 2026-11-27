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
// Slice GB3a (Mike, 2026-11-25, Gardening_Basics.md 9c): with the Hoe in hand, R on a growing square marks it "save for seed"; it is left past ready until the seed is ripe and E
// collects seed for 4 squares and a little food. Corn needs 2 squares of its kind saved together (checked as it flowers). Seed keeps the day it was bought or saved and old seed
// sprouts less; Tomato Seed saved from your own tomatoes is sown straight into the bed.
// Slice GB4a (Mike, 2026-11-25, Gardening_Basics.md 9d): at dawn and at dusk a rabbit or a deer may nibble an open bed's crop (FenceRules: light nibbling); a closed ring of
// fence round the bed keeps them out (wattle keeps rabbits out, deer fence both). The bed's second line says how it is fenced.
// Slice GB4b (2026-11-26): a raid on a bed within FenceRules.VisibleRange of the player, awake, is a real deer or rabbit (WildlifeManager.SendRaider) that walks in,
// eats, and only then takes its bite (RaiderAte); chased off first, it takes nothing.
// Greenhouse G1 (2026-11-26, Greenhouses.md 9): a bed square in a Cold Frame (FlagInFrame) lives at the frame's inside temperature (ColdFrameRules.Inside), gets rain only
// through a propped lid, is harmed past its crop's heat limit, is out of the raiders' reach, takes only low crops, and is taken down with its frame.
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
    public static bool Saving(WoodPileState s) => (s.cropFlags & GardenRules.FlagSeed) != 0;
    public static bool Pollinated(WoodPileState s) => (s.cropFlags & GardenRules.FlagPollinated) != 0;
    public static bool Mulched(WoodPileState s) => (s.cropFlags & GardenRules.FlagMulched) != 0;
    static bool SeedRipe(WoodPileState s) => Saving(s) && !Dead(s) && GardenRules.SeedRipe(CropOf(s), s.jobHours, s.chillDays);
    // GB3b: a carrot or kale square saved for seed, not yet mulched, with the Tall Grass to mulch it carried.
    static bool CanMulch(WoodPileState s) => Saving(s) && !Dead(s) && GardenRules.NeedsWinter(CropOf(s)) && !Mulched(s) && Carried(GardenRules.MulchId) >= GardenRules.MulchGrass;
    public static bool InFrame(WoodPileState s) => (s.cropFlags & GardenRules.FlagInFrame) != 0;
    static int Today => TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0;
    static int DaysPerYear => CalendarRules.Current != null ? CalendarRules.Current.DaysPerYear : 365;
    public static GardenRules.Stage StageOf(WoodPileState s) => GardenRules.StageOf(CropOf(s), DryBeans(s), s.jobHours, s.picksTaken, Dead(s));
    // A square saved for seed is not picked: it is left until the seed is ripe.
    static int PicksReady(WoodPileState s) => CropOf(s) == null || Dead(s) || Saving(s) ? 0 : GardenRules.PicksReady(CropOf(s), DryBeans(s), s.jobHours, s.picksTaken);

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
        // Raids (GB4a): at dawn and at dusk, against the fences standing now.
        int hourNow = TimeManager.Instance != null ? TimeManager.Instance.Hour : -1;
        bool raidHour = FenceRules.IsRaidHour(hourNow);
        List<FenceRules.Segment> fences = raidHour ? wood.FenceSegments() : null;
        int rabbitBeds = 0, deerBeds = 0;
        var lonelyCorn = new List<string>();
        var wintered = new List<string>();
        // Cold Frames (G1): each frame's inside temperature this hour, the frames past a crop's heat limit, and those left open on a frosty night.
        var frameAir = new Dictionary<int, float>();
        var overheated = new List<WoodPileState>();
        var openOnFrost = new List<int>();
        var housesOpenOnFrost = new List<int>();
        var glassOpenOnFrost = new List<int>();
        int heatKilled = 0;
        // Hoop Houses (G2): the snow on their plastic and its age, before the beds under it.
        HoopHouseHour(wood, airC);
        // The drip timers (G5): each morning, before the beds grow.
        WateringFittings.Hour(wood);
        // The vent openers (G6): each opens or shuts its cover for the hour, before the heat and the beds.
        VentOpeners.Hour(wood, airC);
        // Greenhouse stoves and heaters (G4): whether each burns this hour, before the beds and benches feel it.
        GreenhouseHeat.Hour(wood, airC);
        // Glass Greenhouses' seedling benches (G3).
        GreenhouseBenches.Hour(wood, airC);
        foreach (WoodPileState bed in wood.Piles)
        {
            if (bed.kind != PileKind.GardenBed || !IsDug(bed))
                continue;
            GardenRules.Crop crop = CropOf(bed);
            string before = LookKey(bed);
            float daysBefore = bed.jobHours;
            // A square in a Cold Frame lives at the frame's inside temperature, and gets the rain only through a propped lid.
            // A square in a Hoop House (G2) the same, through its sides; through torn plastic it is as outdoors.
            WoodPileState frame = InFrame(bed) ? wood.CoverOf(bed) : null;
            if (frame != null && WoodManager.IsHoopHouse(frame.kind) && WoodManager.HoopIsTorn(frame))
                frame = null;
            float air = airC, rain = rainMm;
            if (frame != null)
            {
                if (!frameAir.TryGetValue(frame.id, out air))
                {
                    air = FrameInside(frame, airC);
                    frameAir[frame.id] = air;
                }
                rain = rainMm * CoverRainShare(frame);
            }
            // A hotbed (G4) warms the soil while it lasts, then it is spent.
            if ((bed.cropFlags & GardenRules.FlagHotbed) != 0)
            {
                if (Hotbed(bed))
                    air += GreenhouseHeatRules.HotbedC;
                else
                {
                    bed.cropFlags &= ~GardenRules.FlagHotbed;
                    wood.NotifyChanged(bed);
                }
            }
            bool ready = crop != null && bed.jobHours >= GardenRules.DaysToReady(crop, DryBeans(bed));
            bool covered = Covered(bed), mulched = Mulched(bed), forSeed = Saving(bed);
            bool frost = crop != null && GardenRules.FrostDamage(crop, air + (covered ? GardenRules.CoverProtectionC : 0f) + (mulched ? GardenRules.MulchProtectionC : 0f),
                                                                 GardenRules.FrostProofNow(crop, ready, forSeed)) > 0f;
            float lowHere = frame != null ? CoverInsideAt(frame, lowTonight, CoverOpen(frame)) : lowTonight;
            if (alertHour && crop != null && !Dead(bed) && GardenRules.FrostRisk(crop, ready, covered, lowHere, forSeed, mulched))
            {
                // An open frame whose shut lid would keep tonight's frost off: told to close it, not to cover it.
                if (frame != null && CoverOpen(frame) && !GardenRules.FrostRisk(crop, ready, covered, CoverInsideAt(frame, lowTonight, false), forSeed, mulched))
                {
                    List<int> open = WoodManager.IsHoopHouse(frame.kind) ? housesOpenOnFrost : WoodManager.IsGlassHouse(frame.kind) ? glassOpenOnFrost : openOnFrost;
                    if (!open.Contains(frame.id))
                        open.Add(frame.id);
                }
                else if (covered) atRiskCovered++;
                else atRiskUncovered++;
            }
            GardenRules.HourResult r = GardenRules.Hour(crop, bed.jobHours, bed.cropDamage, bed.soilMoisture, DryBeans(bed), Dead(bed), air, rain, Raised(bed), covered, forSeed, mulched);
            bed.soilMoisture = r.moisture;
            bed.jobHours = r.growingDays;
            bed.cropDamage = r.damage;
            // Carrots and kale saved for seed (GB3b): once ready, cold hours count toward their winter. When the winter is done, growth starts again from ready
            // (the spring regrowth), and the seed ripens as it grows on.
            if (crop != null && forSeed && !Dead(bed) && !r.died && GardenRules.NeedsWinter(crop) && !GardenRules.Chilled(crop, bed.chillDays))
            {
                float readyDays = GardenRules.DaysToReady(crop, false);
                bed.chillDays = GardenRules.ChillAfterHour(bed.chillDays, air, bed.jobHours >= readyDays);
                if (GardenRules.Chilled(crop, bed.chillDays))
                {
                    bed.jobHours = Mathf.Min(bed.jobHours, readyDays);
                    if (!wintered.Contains(crop.name))
                        wintered.Add(crop.name);
                }
            }
            // Corn saved for seed sets good seed if, as it flowers (its ready day), enough squares of its kind saved for seed are standing (GB3a).
            if (crop != null && Saving(bed) && !Dead(bed) && GardenRules.PlanFor(crop).squares > 1 && !Pollinated(bed))
            {
                float flowers = GardenRules.DaysToReady(crop, false);
                if (daysBefore < flowers && bed.jobHours >= flowers)
                {
                    if (GardenRules.Pollinates(crop, SavedSquares(wood, crop, flowers * 0.5f)))
                        bed.cropFlags |= GardenRules.FlagPollinated;
                    else if (!lonelyCorn.Contains(crop.name))
                        lonelyCorn.Add(crop.name);
                }
            }
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
            // Too hot in a Cold Frame (G1): stalled past the crop's heat limit (GardenRules.GrowthRate), harmed past it by HeatHarmMarginC.
            if (frame != null && crop != null && !Dead(bed))
            {
                if (air > crop.heatC && !overheated.Contains(frame))
                    overheated.Add(frame);
                float harm = ColdFrameRules.HeatDamage(crop.heatC, air);
                if (harm > 0f)
                {
                    bed.cropDamage = GardenRules.Clamp01(bed.cropDamage + harm);
                    if (bed.cropDamage >= 1f)
                    {
                        bed.cropFlags |= GardenRules.FlagDead;
                        heatKilled++;
                    }
                }
            }
            // A Cold Frame keeps the raiders out (G1).
            if (raidHour && frame == null && crop != null && !Dead(bed) && Raid(bed, crop, fences, out bool rabbit, out bool deer))
            {
                if (rabbit) rabbitBeds++;
                if (deer) deerBeds++;
            }
            if (LookKey(bed) != before)
                wood.NotifyChanged(bed);
        }
        if (rabbitBeds + deerBeds > 0)
            ToolStatus.Banner(FenceRules.RaidBanner(rabbitBeds, deerBeds, hourNow == FenceRules.DawnHour), 8f);
        // Cold Frames (G1, Mike: yes, banners): too hot inside (at most every few hours a frame), or left open on a frosty night.
        if (heatKilled > 0)
            ToolStatus.Banner(heatKilled == 1 ? "The heat under cover killed a crop - open the lid or roll the sides up on sunny days" : $"The heat under cover killed {heatKilled} crops - open the lids or roll the sides up on sunny days", 8f);
        int stamp = TimeManager.Instance != null ? TimeManager.Instance.TotalDays * 24 + TimeManager.Instance.Hour : 0;
        foreach (WoodPileState frame in overheated)
        {
            if (LastHeatWarning.TryGetValue(frame.id, out int last) && stamp - last < ColdFrameRules.WarnEveryHours)
                continue;
            LastHeatWarning[frame.id] = stamp;
            ToolStatus.Banner(WoodManager.IsHoopHouse(frame.kind) ? HoopHouseRules.OverheatBanner(frameAir[frame.id]) : WoodManager.IsGlassHouse(frame.kind) ? GlassHouseRules.OverheatBanner(frameAir[frame.id]) : ColdFrameRules.OverheatBanner(frameAir[frame.id]), 8f);
            break; // one banner an hour is plenty
        }
        if (openOnFrost.Count > 0)
            ToolStatus.Banner(ColdFrameRules.CloseLidsBanner(openOnFrost.Count, lowTonight), 12f);
        if (housesOpenOnFrost.Count > 0)
            ToolStatus.Banner(HoopHouseRules.CloseSidesBanner(housesOpenOnFrost.Count, lowTonight), 12f);
        if (glassOpenOnFrost.Count > 0)
            ToolStatus.Banner(GlassHouseRules.CloseVentsBanner(glassOpenOnFrost.Count, lowTonight), 12f);
        if (frostKilled > 0)
            ToolStatus.Banner(frostKilled == 1 ? "Frost killed the crop in a garden bed" : $"Frost killed the crops in {frostKilled} garden beds", 8f);
        if (droughtKilled > 0)
            ToolStatus.Banner(droughtKilled == 1 ? "A garden bed's crop died - the soil dried out" : $"{droughtKilled} garden beds' crops died - the soil dried out", 8f);
        if (bolted > 0)
            ToolStatus.Banner("Lettuce bolted in the heat and went to seed", 6f);
        if (atRiskUncovered + atRiskCovered > 0)
            ToolStatus.Banner(GardenRules.FrostAlert(atRiskUncovered, atRiskCovered, lowTonight), 12f);
        foreach (string name in wintered)
            ToolStatus.Banner($"The {name} saved for seed has had its winter: it will flower and set seed as it grows on", 8f);
        foreach (string name in lonelyCorn)
            ToolStatus.Banner($"Your {name} saved for seed flowered without enough {name} beside it: it won't set good seed (save 2 or more squares together)", 10f);
    }

    // One raid hour for one bed (GB4a): a rabbit, a deer, both or neither come, if the fences let them and the crop is one they eat, and each nibbles its share.
    // GB4b: with the player near and awake, each one that comes is a real animal sent to the bed instead (its bite comes when it has eaten, RaiderAte); the
    // out flags then stay false, as nothing has been eaten yet.
    static bool Raid(WoodPileState bed, GardenRules.Crop crop, List<FenceRules.Segment> fences, out bool rabbit, out bool deer)
    {
        FenceRules.Protection fenced = FenceRules.ProtectionAt(fences, new FenceRules.Point(bed.position.x, bed.position.z));
        bool young = StageOf(bed) == GardenRules.Stage.Sprouting;
        rabbit = !fenced.rabbits && FenceRules.RabbitEats(crop.id, young) && Random.value < FenceRules.RabbitChance;
        deer = !fenced.deer && FenceRules.DeerEats(crop.id) && Random.value < FenceRules.DeerChance;
        if (!rabbit && !deer)
            return false;
        if (RaiderSeen(bed))
        {
            if (rabbit && WildlifeManager.Instance.SendRaider("rabbit", bed))
                rabbit = false;
            if (deer && WildlifeManager.Instance.SendRaider("deer", bed))
                deer = false;
            if (!rabbit && !deer)
                return false;
        }
        bed.cropDamage = GardenRules.Clamp01(bed.cropDamage + (rabbit ? FenceRules.RabbitBite : 0f) + (deer ? FenceRules.DeerBite : 0f));
        if (bed.cropDamage >= 1f)
            bed.cropFlags |= GardenRules.FlagDead;
        return true;
    }

    // Whether a raid on this bed comes as an animal the player can see: near and awake, with the wildlife running.
    static bool RaiderSeen(WoodPileState bed)
    {
        if (WildlifeManager.Instance == null)
            return false;
        PlayerController player = WildlifeManager.Player != null ? WildlifeManager.Player : Object.FindAnyObjectByType<PlayerController>();
        if (player == null)
            return false;
        bool asleep = SleepManager.Instance != null && SleepManager.Instance.IsSleeping;
        return FenceRules.Visible(Vector3.Distance(player.transform.position, bed.position), asleep);
    }

    // A raider has reached the bed and started eating (GB4b): the player is told, in time to chase it off.
    public static void RaiderArrived(WoodPileState bed, string animal)
    {
        GardenRules.Crop crop = CropOf(bed);
        if (crop == null || Dead(bed))
            return;
        ToolStatus.Banner(FenceRules.EatingBanner(animal, crop.name), 6f);
    }

    // Testing only (TimeDebugControls, the / key): sends a raider at once to the nearest planted bed it could get at and would eat. Returns what happened.
    public static string DebugSendRaider(string speciesId, Vector3 near)
    {
        WoodManager wood = WoodManager.Instance;
        if (wood == null || WildlifeManager.Instance == null)
            return "No wildlife here";
        WoodPileState best = null;
        float bestDistance = float.MaxValue;
        foreach (WoodPileState bed in wood.Piles)
        {
            GardenRules.Crop crop = bed.kind == PileKind.GardenBed && IsDug(bed) ? CropOf(bed) : null;
            if (crop == null || Dead(bed) || !FenceRules.Eats(speciesId, crop.id, StageOf(bed) == GardenRules.Stage.Sprouting))
                continue;
            FenceRules.Protection fenced = wood.FenceProtectionAt(bed.position);
            if (speciesId == "deer" ? fenced.deer : fenced.rabbits)
                continue;
            float d = Vector3.Distance(near, bed.position);
            if (d < bestDistance)
            {
                bestDistance = d;
                best = bed;
            }
        }
        if (best == null)
            return $"No planted, unfenced bed with a crop a {speciesId} eats";
        return WildlifeManager.Instance.SendRaider(speciesId, best)
            ? $"Sent a {speciesId} to the {CropOf(best).name} bed {bestDistance:0} m away"
            : "Found no spot for it to come from";
    }

    static bool StillStanding(WoodManager wood, WoodPileState bed)
    {
        foreach (WoodPileState pile in wood.Piles)
            if (pile == bed)
                return true;
        return false;
    }

    // A raider has eaten its fill (GB4b): its bite, if the crop it came for is still there, alive, and not fenced off against it meanwhile.
    public static void RaiderAte(WoodPileState bed, string speciesId, string cropId, string animal)
    {
        WoodManager wood = WoodManager.Instance;
        GardenRules.Crop crop = CropOf(bed);
        if (wood == null || crop == null || crop.id != cropId || Dead(bed) || !StillStanding(wood, bed))
            return;
        FenceRules.Protection fenced = wood.FenceProtectionAt(bed.position);
        if (speciesId == "deer" ? fenced.deer : fenced.rabbits)
            return;
        bed.cropDamage = GardenRules.Clamp01(bed.cropDamage + FenceRules.Bite(speciesId));
        if (bed.cropDamage >= 1f)
            bed.cropFlags |= GardenRules.FlagDead;
        wood.NotifyChanged(bed);
        ToolStatus.Banner(FenceRules.AteBanner(animal, crop.name), 6f);
    }

    static readonly Dictionary<int, int> LastHeatWarning = new Dictionary<int, int>();

    // The inside temperature to show: the hour's (kept on the frame), or worked out now if no hour has passed since it was built or its lid moved.
    public static float FrameInsideShown(WoodPileState frame) =>
        frame.jobHours != 0f ? frame.jobHours : FrameInside(frame, WeatherManager.Instance != null ? WeatherManager.Instance.TemperatureC : 15f);

    // A Cold Frame's inside temperature now (G1), from the outside air and the sun on its glass; kept on the frame (its jobHours) for its prompt.
    public static float FrameInside(WoodPileState frame, float airC)
    {
        float inside = InsideIf(frame, airC, CoverOpen(frame));
        frame.jobHours = inside;
        return inside;
    }

    // What a cover's inside reaches this hour with its lid, sides or vents as given (the sun and a greenhouse's heat included); nothing is kept. The vent
    // openers (G6) ask it what the inside would reach shut.
    public static float InsideIf(WoodPileState frame, float airC, bool open)
    {
        TimeManager time = TimeManager.Instance;
        float sun = 0f;
        if (time != null)
        {
            WeatherManager weather = WeatherManager.Instance;
            float clear = DayNightCycle.SunlightFor(weather != null ? weather.Current : WeatherType.Clear);
            sun = ColdFrameRules.Sun(time.HourOfDay, time.SunriseHour, time.SunsetHour, ColdFrameRules.NoonSin((int)time.CurrentSeason), clear);
        }
        float inside = WoodManager.IsGlassHouse(frame.kind) ? GlassHouseRules.InsideC(airC, sun, open)
            : WoodManager.IsHoopHouse(frame.kind)
            ? HoopHouseRules.InsideC(airC, sun, open, WoodManager.HoopIsTorn(frame))
            : ColdFrameRules.Inside(airC, sun, open);
        return GreenhouseHeat.Apply(frame, inside, sun); // a greenhouse's barrels and burning heater (G4)
    }

    // --- Covers: a Cold Frame (G1) or a Hoop House (G2) ---

    // Its lid propped or its sides rolled up.
    public static bool CoverOpen(WoodPileState cover) => WoodManager.IsHoopHouse(cover.kind) ? WoodManager.HoopSidesAreUp(cover) : WoodManager.IsGlassHouse(cover.kind) ? WoodManager.GlassVentsAreOpen(cover) : WoodManager.FrameLidIsOpen(cover);

    // Its inside at night (no sun), lid or sides as given: for the frost alert.
    static float CoverInsideAt(WoodPileState cover, float airC, bool open) =>
        GreenhouseHeat.NightEstimate(cover,
            WoodManager.IsHoopHouse(cover.kind) ? HoopHouseRules.InsideC(airC, 0f, open, false) : WoodManager.IsGlassHouse(cover.kind) ? GlassHouseRules.InsideC(airC, 0f, open) : ColdFrameRules.Inside(airC, 0f, open));

    // --- Hotbeds (G4): Fertilizer dug into a Cold Frame's bed ---

    static bool Hotbed(WoodPileState s) => (s.cropFlags & GardenRules.FlagHotbed) != 0 && GreenhouseHeatRules.HotbedActive(s.builtDay, Today);
    static bool InColdFrame(WoodPileState s) => InFrame(s) && WoodManager.Instance != null && WoodManager.Instance.ColdFrameOf(s) != null;
    static bool CanHotbed(WoodPileState s) => IsDug(s) && !Dead(s) && InColdFrame(s) && !Hotbed(s) && Carried(GardenRules.FertilizerId) >= GreenhouseHeatRules.HotbedFertilizer;

    static void MakeHotbed(WoodPileState s)
    {
        if (Inventory.RemoveFromPlayer(GardenRules.FertilizerId, GreenhouseHeatRules.HotbedFertilizer) < GreenhouseHeatRules.HotbedFertilizer)
            return;
        s.cropFlags |= GardenRules.FlagHotbed;
        s.builtDay = Today;
        for (int i = 0; i < GreenhouseHeatRules.HotbedFertilizer; i++)
            s.soilSpent = GardenRules.AfterFertilizer(s.soilSpent);
        WoodManager.Instance.NotifyChanged(s);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDig(s.position);
        ToolStatus.Flash($"Dug in a hotbed - the soil will stay warm for {GreenhouseHeatRules.HotbedDays} days", 3f);
    }

    static float CoverRainShare(WoodPileState cover) =>
        WoodManager.IsHoopHouse(cover.kind) ? HoopHouseRules.RainShare(WoodManager.HoopIsTorn(cover)) : WoodManager.IsGlassHouse(cover.kind) ? 0f : ColdFrameRules.RainShare(WoodManager.FrameLidIsOpen(cover));

    // A Hoop House's hour (G2): snow builds on its plastic while it snows and melts above freezing; too much tears it, and after WearYears it wears out.
    // Either way it is flagged torn and gives no cover until new plastic is fitted (PlacedStructure). Banners: the snow piling up, and the tear.
    static void HoopHouseHour(WoodManager wood, float airC)
    {
        TimeManager time = TimeManager.Instance;
        bool snowing = WeatherManager.Instance != null && WeatherManager.Instance.IsSnowing;
        int today = time != null ? time.TotalDays : 0;
        int daysPerYear = time != null ? time.DaysPerSeason * 4 : 0;
        foreach (WoodPileState house in wood.Piles)
        {
            if (!WoodManager.IsHoopHouse(house.kind) || WoodManager.HoopIsTorn(house))
                continue;
            float before = house.soilMoisture;
            house.soilMoisture = HoopHouseRules.SnowAfterHour(before, snowing, airC);
            if (HoopHouseRules.Tears(house.soilMoisture))
            {
                house.cropFlags |= WoodManager.HoopTorn;
                house.soilMoisture = 0f; // it falls through
                ToolStatus.Banner(HoopHouseRules.TornBanner, 12f);
            }
            else if (HoopHouseRules.WornOut(house.builtDay, today, daysPerYear))
            {
                house.cropFlags |= WoodManager.HoopTorn;
                ToolStatus.Banner(HoopHouseRules.WornBanner, 12f);
            }
            else if (before < HoopHouseRules.WarnCm && house.soilMoisture >= HoopHouseRules.WarnCm)
                ToolStatus.Banner(HoopHouseRules.SnowBanner(house.soilMoisture), 10f);
            if (WoodManager.HoopIsTorn(house) || Mathf.Round(before) != Mathf.Round(house.soilMoisture))
                wood.NotifyChanged(house);
        }
    }

    // Living squares of this crop saved for seed and grown at least this far (corn that is flowering, or near it).
    static int SavedSquares(WoodManager wood, GardenRules.Crop crop, float atLeastDays)
    {
        int n = 0;
        foreach (WoodPileState bed in wood.Piles)
            if (bed.kind == PileKind.GardenBed && IsDug(bed) && bed.jobItem == crop.id && Saving(bed) && !Dead(bed) && bed.jobHours >= atLeastDays)
                n++;
        return n;
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
        string bed = GardenRules.FertilityText(s.soilSpent) + (Covered(s) ? "; under a Row Cover" : "") + (Mulched(s) ? "; mulched for winter" : "");
        // In a Cold Frame (G1): the lid and the inside temperature. Otherwise how it is fenced (GB4a), while something grows in it.
        // In a Hoop House (G2): its sides and the inside temperature, or that its plastic is torn.
        WoodPileState frame = InFrame(s) && WoodManager.Instance != null ? WoodManager.Instance.CoverOf(s) : null;
        if (frame != null && WoodManager.IsGlassHouse(frame.kind))
            bed += "\n" + GlassHouseRules.BedLine(WoodManager.GlassVentsAreOpen(frame), FrameInsideShown(frame));
        else if (frame != null && WoodManager.IsHoopHouse(frame.kind))
            bed += "\n" + HoopHouseRules.BedLine(WoodManager.HoopSidesAreUp(frame), WoodManager.HoopIsTorn(frame), FrameInsideShown(frame));
        else if (frame != null)
            bed += "\n" + ColdFrameRules.BedLine(WoodManager.FrameLidIsOpen(frame), FrameInsideShown(frame));
        if (Hotbed(s))
            bed += "; " + GreenhouseHeatRules.HotbedLine(GreenhouseHeatRules.HotbedDaysLeft(s.builtDay, Today));
        else if (CropOf(s) != null && !Dead(s) && WoodManager.Instance != null)
            bed += "\n" + FenceRules.ProtectionText(WoodManager.Instance.FenceProtectionAt(s.position));
        if (crop == null)
        {
            string hint = !HoeEquipped ? "\n" + GardenRules.NeedHoe : !SeedToPlant(s).HasValue ? "\n" + GardenRules.NoSeed : "";
            return $"{BedName(s)}  (bare, {soil})\n{bed}{hint}";
        }
        bool dry = DryBeans(s);
        GardenRules.Stage stage = StageOf(s);
        string name = Saving(s) ? $"{crop.name}, saved for seed" : crop.id == GardenRules.BeansCropId ? (dry ? "Beans (left to dry)" : "Beans (picked green)") : crop.name;
        if (stage == GardenRules.Stage.Dead)
            return $"{BedName(s)}  ({name}, dead)\n{bed}";
        string status = Saving(s) ? GardenRules.SeedStatusText(crop, s.jobHours, s.chillDays) + GardenRules.DamageText(s.cropDamage)
                                  : GardenRules.StageText(stage) + GardenRules.DaysLeftText(crop, dry, s.jobHours) + GardenRules.DamageText(s.cropDamage);
        return $"{BedName(s)}  ({name}: {status}; {soil})\n{bed}";
    }

    // Fertilizer goes on when the soil is worn and nothing else is to be done with E (GB2).
    static bool CanFertilize(WoodPileState s) => (IsDug(s) && s.soilSpent > 0.01f && Carried(GardenRules.FertilizerId) > 0 && !Dead(s)) || CanHotbed(s);

    static string FertilizePrompt(WoodPileState s) => CanHotbed(s) ? GreenhouseHeatRules.HotbedPrompt(Carried(GardenRules.FertilizerId)) :
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
            GardenRules.Sowing? next = HoeEquipped ? SeedToPlant(s) : null;
            if (next.HasValue)
            {
                GardenRules.Sowing sow = next.Value;
                string timing = GardenRules.TimingWarning(sow.crop, CalendarRules.Current, Today);
                string age = GardenRules.AgeWarning(sow.crop, NextSeedAge(sow));
                string warning = age != null && timing != null ? age + "\n" + timing : age ?? timing;
                return GardenRules.PlantPrompt(sow, Inventory.Player.Count(sow.seedId), warning);
            }
            return CanFertilize(s) ? FertilizePrompt(s) : null;
        }
        if (dead)
            return "Clear the dead plants";
        if (SeedRipe(s))
            return CollectSeedPrompt(s, crop);
        int ready = PicksReady(s);
        if (ready > 0)
            return $"Harvest {ProduceName(crop, DryBeans(s))} x{GardenRules.PickYield(crop, DryBeans(s), s.cropDamage, s.soilSpent) * ready}";
        if (CanMulch(s))
            return GardenRules.MulchPrompt(Carried(GardenRules.MulchId));
        return CanFertilize(s) ? FertilizePrompt(s) : null;
    }

    public static string Prompt(WoodPileState s) => Header(s) + (ActionE(s) is string e ? "\n" + e : "");

    static string ItemName(string itemId) => ItemDatabase.Get(itemId)?.DisplayName ?? itemId;

    static string CollectSeedPrompt(WoodPileState s, GardenRules.Crop crop)
    {
        int seed = GardenRules.SeedYield(crop, s.cropDamage, Pollinated(s));
        int food = GardenRules.SeedFood(crop, s.cropDamage);
        string what = seed > 0 ? $"{ItemName(GardenRules.PlanFor(crop).savedSeedId)} x{seed}" : "no good seed";
        return $"Collect the seed  ({what}" + (food > 0 ? $", {ItemName(GardenRules.SeedFoodId(crop))} x{food}" : "") + ")";
    }

    // The age in years of the seed the next planting takes (the oldest carried, as planting takes it); 0 for what does not age as seed.
    static int NextSeedAge(GardenRules.Sowing sowing)
    {
        if (Inventory == null || !GardenRules.AgesAsSeed(sowing.seedId))
            return 0;
        var batches = Inventory.Player.Batches(sowing.seedId);
        return batches.Count == 0 ? 0 : GardenRules.AgeYears(batches[0].day, Today, DaysPerYear);
    }

    // Whether R with the Hoe marks or unmarks this square for seed (GB3a): a one-year crop, alive, not yet picked, its seed not yet ripe.
    static bool SeedToggle(WoodPileState s, GardenRules.Crop crop) =>
        crop != null && HoeEquipped && IsDug(s) && !Dead(s) && GardenRules.CanSaveSeed(crop) && !SeedRipe(s) && (Saving(s) || s.picksTaken == 0);

    // R, in Secondary's order: the Shovel takes the bed away; the Hammer raises it; the Hoe picks the seed, or marks a growing square for seed; beans switch green or dry;
    // then the Row Cover.
    public static string SecondaryPrompt(WoodPileState s)
    {
        GardenRules.Crop crop = CropOf(s);
        // A Cold Frame's square (G1) is not filled in or raised on its own: the frame comes down as one, with the Hammer at its front.
        if (InFrame(s) && (ShovelEquipped || HammerEquipped))
            return "";
        if (ShovelEquipped && (crop == null || !IsDug(s)))
            return Raised(s) ? $"Take the raised bed apart  ({GardenRules.RaisedBedLumber} Lumber back)" : IsDug(s) ? "Fill the bed in" : "Take the marked-out bed away";
        if (HammerEquipped && !Raised(s))
            return Carried(SawPitRules.LumberId) >= GardenRules.RaisedBedLumber ? $"Make it a raised bed  (uses {GardenRules.RaisedBedLumber} Lumber)"
                                                                                : $"Make it a raised bed  (needs {GardenRules.RaisedBedLumber} Lumber)";
        if (crop == null && IsDug(s) && HoeEquipped && CarriedSowings(s).Count > 1)
            return "Pick another seed";
        if (SeedToggle(s, crop))
            return Saving(s) ? GardenRules.StopSavingPrompt : GardenRules.SaveSeedPrompt(crop);
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
            if (!(HoeEquipped && SeedToPlant(s) != null) && CanFertilize(s))
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
        if (SeedRipe(s))
        {
            CollectSeed(s, crop, view);
            return;
        }
        int ready = PicksReady(s);
        if (ready > 0)
        {
            Harvest(s, crop, ready, view);
            return;
        }
        if (CanMulch(s))
        {
            Inventory.RemoveFromPlayer(GardenRules.MulchId, GardenRules.MulchGrass);
            s.cropFlags |= GardenRules.FlagMulched;
            wood.NotifyChanged(s);
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayDig(s.position);
            ToolStatus.Flash(GardenRules.Mulched, 4f);
            return;
        }
        if (CanFertilize(s))
        {
            Fertilize(s);
            return;
        }
        string status = Saving(s) ? GardenRules.SeedStatusText(crop, s.jobHours, s.chillDays) : GardenRules.StageText(StageOf(s)) + GardenRules.DaysLeftText(crop, DryBeans(s), s.jobHours);
        ToolStatus.Flash($"{crop.name}: {status} ({GardenRules.HardinessText(crop.hardiness)}).", 4f);
    }

    // E on a square whose seed is ripe (GB3a): the saved seed, dated today so it ages from now, and the little food that comes with it; the crop is over, so the
    // soil is worn as after any crop. What does not fit in the pack is left on the ground.
    static void CollectSeed(WoodPileState s, GardenRules.Crop crop, Transform view)
    {
        int seed = GardenRules.SeedYield(crop, s.cropDamage, Pollinated(s));
        int food = GardenRules.SeedFood(crop, s.cropDamage);
        string seedId = GardenRules.PlanFor(crop).savedSeedId, foodId = GardenRules.SeedFoodId(crop);
        int onGround = Give(seedId, seed, s, view) + Give(foodId, food, s, view);
        s.soilSpent = GardenRules.AfterCrop(crop, s.soilSpent);
        ClearCrop(s);
        WoodManager.Instance.NotifyChanged(s);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDig(s.position);
        string message = GardenRules.SeedCollected(crop, seed, ItemName(seedId), food, ItemName(foodId));
        if (onGround > 0)
            message += $" ({onGround} left on the ground - no room in the pack)";
        ToolStatus.Flash(message, 5f);
    }

    // Into the pack, the rest on the ground beside the bed. Returns how many went on the ground.
    static int Give(string itemId, int count, WoodPileState s, Transform view)
    {
        if (count <= 0)
            return 0;
        ItemDefinition item = ItemDatabase.Get(itemId);
        int fits = item != null ? Mathf.Min(count, Inventory.Player.SpaceFor(item)) : 0;
        int took = fits > 0 ? Inventory.AddToPlayer(itemId, fits) : 0;
        if (count - took > 0)
            DroppedItems.Place(itemId, count - took, s.position, view);
        return count - took;
    }

    // One Fertilizer spread on the bed puts back a quarter of a spent soil (GB2).
    static void Fertilize(WoodPileState s)
    {
        if (CanHotbed(s))
        {
            MakeHotbed(s);
            return;
        }
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

    // The ways to plant a square whose seed is carried in the amount one square takes, in GardenRules.Sowings' order (Tomato Seed after the Tomato Plants).
    // A Cold Frame's square (G1) offers only the low crops that fit under its lid.
    static List<GardenRules.Sowing> CarriedSowings(WoodPileState s = null)
    {
        var list = new List<GardenRules.Sowing>();
        if (Inventory == null)
            return list;
        // A Hoop House's squares (G2) take any crop.
        WoodPileState cover = s != null && InFrame(s) && WoodManager.Instance != null ? WoodManager.Instance.CoverOf(s) : null;
        bool framed = cover != null && WoodManager.IsColdFrame(cover.kind);
        foreach (GardenRules.Sowing sowing in GardenRules.Sowings)
            if (Inventory.Player.Count(sowing.seedId) >= sowing.seeds && (!framed || ColdFrameRules.Fits(sowing.crop.id)))
                list.Add(sowing);
        return list;
    }

    // The chosen seed if it is carried, otherwise the first one carried.
    static GardenRules.Sowing? SeedToPlant(WoodPileState s = null)
    {
        List<GardenRules.Sowing> carried = CarriedSowings(s);
        foreach (GardenRules.Sowing sowing in carried)
            if (sowing.seedId == SelectedSeed)
                return sowing;
        return carried.Count > 0 ? carried[0] : (GardenRules.Sowing?)null;
    }

    static void Plant(WoodPileState s)
    {
        if (!HoeEquipped)
        {
            ToolStatus.Flash(GardenRules.NeedHoe, 3f);
            return;
        }
        GardenRules.Sowing? next = SeedToPlant(s);
        if (!next.HasValue)
        {
            ToolStatus.Flash(GardenRules.NoSeed, 4f);
            return;
        }
        GardenRules.Sowing sowing = next.Value;
        GardenRules.Crop crop = sowing.crop;
        // The oldest seed carried goes in first (GB3a); old seed may not come up at all.
        int oldest = int.MaxValue;
        if (Inventory.Player.RemoveBatches(sowing.seedId, sowing.seeds, (day, n) => oldest = Mathf.Min(oldest, day)) < sowing.seeds)
            return;
        SelectedSeed = sowing.seedId;
        int age = GardenRules.AgesAsSeed(sowing.seedId) && oldest != int.MaxValue ? GardenRules.AgeYears(oldest, Today, DaysPerYear) : 0;
        float sprouts = GardenRules.AgesAsSeed(sowing.seedId) ? GardenRules.Sprouts(crop, age) : 1f;
        if (sprouts < 1f && Random.value >= sprouts)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayDig(s.position);
            ToolStatus.Flash(GardenRules.FailedToSprout(crop, age), 5f);
            return;
        }
        ClearCrop(s);
        s.jobItem = crop.id;
        s.jobHours = sowing.startDays; // Tomato Seed starts behind a bought plant
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
        s.chillDays = 0f;
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
        if (InFrame(s) && (ShovelEquipped || HammerEquipped))
        {
            ToolStatus.Flash("This bed is part of the cold frame - to take it down, use the Hammer's R on the frame's front", 3.5f);
            return;
        }
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
            List<GardenRules.Sowing> carried = CarriedSowings(s);
            if (carried.Count == 0)
            {
                ToolStatus.Flash(GardenRules.NoSeed, 4f);
                return;
            }
            GardenRules.Sowing? now = SeedToPlant(s);
            int at = now.HasValue ? carried.FindIndex(x => x.seedId == now.Value.seedId) : -1;
            GardenRules.Sowing next = carried[(at + 1) % carried.Count];
            SelectedSeed = next.seedId;
            ToolStatus.Flash(next.startDays < 0f ? $"Next to plant: {next.crop.name} from seed" : $"Next to plant: {next.crop.name}", 2.5f);
            return;
        }
        if (SeedToggle(s, crop))
        {
            if (Saving(s))
            {
                s.cropFlags &= ~(GardenRules.FlagSeed | GardenRules.FlagPollinated);
                ToolStatus.Flash($"Stopped saving seed: this {crop.name} will be harvested as usual", 3f);
            }
            else
            {
                s.cropFlags |= GardenRules.FlagSeed;
                if (crop.id == GardenRules.BeansCropId)
                    s.cropFlags |= GardenRules.FlagDryBeans; // bean seed is the dry bean in its pod
                ToolStatus.Flash(GardenRules.SavingStarted(crop), 5f);
            }
            wood.NotifyChanged(s);
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
