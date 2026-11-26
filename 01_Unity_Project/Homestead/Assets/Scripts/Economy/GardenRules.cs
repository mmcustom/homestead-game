using System;

// Gardening basics, slice GB1 (Gardening_Basics.md). Mike, 2026-11-25: nine crops (potatoes, field corn, sweet corn, beans, tomatoes, lettuce, carrots, squash and kale), dug beds,
// seed bought at the Trading Post, crops growing in real time, and the Hoe a new tool needed to plant. Deer and rabbits wait for the fence slice.
//
// A Garden Bed is one 4 by 4 ft bed square (1.2 m), placed free from the Build tab and dug with the Shovel (DigBlows). The saved pile holds it with the fields it already has plus
// four appended ones: digProgress (0 to 1, dug at 1), jobItem (the crop id growing, empty when bare), jobHours (the growing days it has had), soilMoisture (0 to 1), cropDamage
// (0 to 1, dead at 1), cropFlags (Dead, DryBeans) and picksTaken. Each game hour the crop grows by its temperature and the soil's moisture, frost and drought damage it, and rain
// wets the soil while warm air dries it. Pure and engine-free so edit-mode tests cover it; the greenhouses (Greenhouses.md) will pass their own inside temperature to Hour.
public static class GardenRules
{
    public const string HoeId = "hoe";
    public const string WateringCanId = "watering_can";
    public const int WateringCanLitres = 8;

    public const int DigBlows = 12;               // the Shovel's blows to turn a bed (an iron Shovel needs 0.75 of them, as for every dig)
    public const float BedSize = 1.2f;            // one bed square, 4 ft
    public const float BedClearance = 1.25f;      // beds may stand side by side: only the bed itself must not overlap
    public const float TreeClearance = 1.5f;      // no tree trunk on or right beside a bed
    public const float BoxHeight = 0.3f;     // low enough to step up onto

    public const long HoeStoreCents = 1000;
    public const long WateringCanStoreCents = 800;
    public const long SeedCents = 300;            // a packet of seed plants one square
    public const long TomatoPlantCents = 200;
    public const long PotatoStoreCents = 60;
    public const int ProduceDropBp = 400;          // 4% off per unit sold in one go, recovering over time
    public const int ProduceFloorBp = 2500;      // potatoes on the shelf, for seed or eating; the store pays PotatoCents for yours

    // Flags in the pile's cropFlags. Dead and DryBeans belong to the crop and go with it; Raised and Covered belong to the bed and stay (KeptFlags).
    public const int FlagDead = 1, FlagDryBeans = 2, FlagRaised = 4, FlagCovered = 8;
    public const int KeptFlags = FlagRaised | FlagCovered;

    // --- Slice GB2 (Mike, 2026-11-25: Gardening_Basics.md section 9b) ---

    // Soil fertility, kept as how SPENT the soil is (the pile's soilSpent, 0 rich to 1 spent, so a bed from before GB2 loads rich). Each crop taken to its
    // end uses a third (Mike: "Gentler: 3 crops"); beans put a little back instead. A spent bed still gives 75%. One Fertilizer (the Compost Bin's, or the
    // Trading Post's) puts back a quarter, so 4 bring a spent bed back to rich.
    public const float SpentPerCrop = 1f / 3f;
    public const float BeansGiveBack = 0.1f;
    public const float SpentYieldCut = 0.25f;
    public const float FertilizerPerUnit = 0.25f;
    public const string FertilizerId = CompostRules.FertilizerId;

    public static float AfterCrop(Crop crop, float spent) =>
        crop != null && crop.id == BeansCropId ? Clamp01(spent - BeansGiveBack) : Clamp01(spent + SpentPerCrop);

    public static float AfterFertilizer(float spent) => Clamp01(spent - FertilizerPerUnit);

    public static float YieldFactor(float spent) => 1f - SpentYieldCut * Clamp01(spent);

    // Fertilizer units that would bring the soil back to rich (rounded up).
    public static int FertilizerToRich(float spent) => (int)Math.Ceiling(Clamp01(spent) / FertilizerPerUnit - 0.0001f);

    public static string FertilityText(float spent) =>
        spent < 0.05f ? "rich soil" : spent < 0.5f ? "good soil" : spent < 0.95f ? "worn soil - feed it" : "spent soil (75% harvest) - feed it";

    // Raised beds (Mike: "Stage it, any bed"): a frame of 2 Lumber, marked out and stocked like a site, hammered up in 10 blows, then filled with the Shovel
    // (the bed's dig). Or R with the Hammer upgrades a bed already dug, for 2 Lumber. The soil is warmer (about a week ahead in spring) and drains, so it
    // never waterlogs; a dug bed soaked past WaterloggedAbove grows slower.
    public const int RaisedBedLumber = 2;
    public const int RaisedBedBuildBlows = 10;
    public const float RaisedWarmthC = 1.5f;
    public const float WaterloggedAbove = 0.9f;
    public const float WaterloggedFactor = 0.6f;
    public const float RaisedDrainAbove = 0.8f;
    public const float RaisedDrainPerHour = 0.01f;

    // Row Cover (Mike: "Lay on with R, reusable"): a light cloth laid over a bed. It keeps about 3 C of frost off the crop, rain still comes through, and
    // left on in hot weather the crop under it grows a little slower.
    public const string RowCoverId = "row_cover";
    public const long RowCoverStoreCents = 600;
    public const float CoverProtectionC = 3f;
    public const float CoverHeatC = 27f;
    public const float CoverHeatFactor = 0.8f;

    // The frost alert (Mike: "Yes, a frost alert"): at AlertHour, if the night's expected low comes within FrostMarginC of hurting a bed's crop (the cover
    // counted), a message says which beds. Ground frost comes before the air reaches freezing, so the margin warns early.
    public const int AlertHour = 18;
    public const float FrostMarginC = 1.5f;

    public static bool FrostRisk(Crop crop, bool ready, bool covered, float lowC) =>
        crop != null && FrostDamage(crop, lowC + (covered ? CoverProtectionC : 0f) - FrostMarginC, ready) > 0f;

    public static string FrostAlert(int uncovered, int covered, float lowC)
    {
        string low = $"about {Math.Round(lowC):0} C";
        if (uncovered > 0)
            return (uncovered == 1 ? "Frost likely tonight (" + low + "): a garden bed's crop is at risk" : $"Frost likely tonight ({low}): {uncovered} garden beds' crops are at risk")
                   + (uncovered == 1 ? " - lay a Row Cover over it (R) or harvest what is ready" : " - lay Row Covers over them (R) or harvest what is ready");
        return covered == 1 ? $"Hard frost likely tonight ({low}): even under its Row Cover a bed's crop is at risk"
                            : $"Hard frost likely tonight ({low}): even under Row Covers {covered} beds' crops are at risk";
    }

    // How hard a frost a crop takes before it is hurt (air at or below this, in C).
    public enum Hardiness { Tender, SemiHardy, Tolerant, Hardy }

    public sealed class Crop
    {
        public string id, name;
        public string seedId;          // what is planted (a seed packet, a tomato plant, or potatoes)
        public int seedsPerSquare;
        public string produceId;       // what the harvest gives
        public int yieldPerSquare;     // units for a healthy square (spread over the picks of a picked crop)
        public float days;             // growing days to ready (Gardening_Basics.md section 3)
        public float baseC, fullC;     // no growth at or below baseC, full speed at or above fullC
        public float heatC;            // above this the crop stalls (tomatoes drop their flowers, lettuce bolts)
        public Hardiness hardiness;
        public int picks;              // 1: harvested once; more: picked that many times
        public float pickEveryDays;
        public bool frostProofWhenReady; // potatoes under the ground, dried field corn: a frost no longer hurts once ready
        public float boltAfterDays;    // lettuce: days past ready before it bolts and is spent (0: never)
        public long produceCents;      // what the Trading Post pays for one
        public float produceKg;
    }

    static Crop C(string id, string name, string seedId, int seeds, string produceId, int yield, float days, float baseC, float fullC, float heatC, Hardiness hardiness,
                  int picks, float pickEvery, long cents, float kg, bool frostProof = false, float bolt = 0f) =>
        new Crop { id = id, name = name, seedId = seedId, seedsPerSquare = seeds, produceId = produceId, yieldPerSquare = yield, days = days, baseC = baseC, fullC = fullC, heatC = heatC,
                   hardiness = hardiness, picks = picks, pickEveryDays = pickEvery, produceCents = cents, produceKg = kg, frostProofWhenReady = frostProof, boltAfterDays = bolt };

    public const string PotatoesId = "potatoes";
    public const string BeansCropId = "beans";
    public const string GreenBeansId = "green_beans";
    public const string DryBeansId = "dry_beans";
    public const float DryBeanDays = 90f;
    public const int DryBeanYield = 7;
    public const long DryBeanCents = 50;
    public const float DryBeanKg = 0.1f;

    // The nine crops, in the order the seed list cycles. Warm crops grow from 10 C and at full speed from 18 C; cool ones from 4 C and full from 13 C (so a typical Ohio summer day
    // and night average about full speed, and the "days to harvest" of the extension tables hold in an ordinary season).
    public static readonly Crop[] Crops =
    {
        C("potatoes", "Potatoes", PotatoesId, 8, PotatoesId, 36, 100f, 6f, 15f, 32f, Hardiness.SemiHardy, 1, 0f, 25, 0.25f, frostProof: true),
        C("field_corn", "Field Corn", "seed_field_corn", 1, "field_corn", 16, 95f, 10f, 18f, 35f, Hardiness.Tender, 1, 0f, 30, 0.15f, frostProof: true),
        C("sweet_corn", "Sweet Corn", "seed_sweet_corn", 1, "sweet_corn", 16, 78f, 10f, 18f, 35f, Hardiness.Tender, 1, 0f, 40, 0.3f),
        C(BeansCropId, "Beans", "seed_beans", 1, GreenBeansId, 8, 55f, 12f, 18f, 33f, Hardiness.Tender, 4, 5f, 75, 0.25f),
        C("tomatoes", "Tomatoes", "tomato_plant", 4, "tomato", 70, 75f, 10f, 18f, 32f, Hardiness.Tender, 7, 5f, 50, 0.2f),
        C("lettuce", "Lettuce", "seed_lettuce", 1, "lettuce", 16, 50f, 4f, 13f, 27f, Hardiness.SemiHardy, 1, 0f, 100, 0.25f, bolt: 20f),
        C("carrots", "Carrots", "seed_carrots", 1, "carrot", 35, 70f, 4f, 13f, 30f, Hardiness.Tolerant, 1, 0f, 20, 0.1f, frostProof: true),
        C("squash", "Squash", "seed_squash", 1, "squash", 6, 100f, 12f, 18f, 35f, Hardiness.Tender, 1, 0f, 200, 1.5f),
        C("kale", "Kale", "seed_kale", 1, "kale", 9, 60f, 2f, 13f, 30f, Hardiness.Hardy, 9, 10f, 100, 0.3f),
    };

    public static Crop Find(string cropId)
    {
        if (string.IsNullOrEmpty(cropId))
            return null;
        foreach (Crop crop in Crops)
            if (crop.id == cropId)
                return crop;
        return null;
    }

    // The crop a seed item plants, or null.
    public static Crop ForSeed(string seedId)
    {
        foreach (Crop crop in Crops)
            if (crop.seedId == seedId)
                return crop;
        return null;
    }

    public static bool IsSeed(string itemId) => ForSeed(itemId) != null;

    // The seed packets and plants (not the potatoes, which are food too).
    public static readonly string[] SeedItemIds = { "seed_field_corn", "seed_sweet_corn", "seed_beans", "tomato_plant", "seed_lettuce", "seed_carrots", "seed_squash", "seed_kale" };

    // Every item a bed can give (dry beans as well as green).
    public static readonly string[] ProduceIds = { PotatoesId, "field_corn", "sweet_corn", GreenBeansId, DryBeansId, "tomato", "lettuce", "carrot", "squash", "kale" };

    // --- Frost ---

    // The air temperature at or below which a crop of this hardiness is hurt: tender at freezing, semi-hardy at -2, tolerant at -6, hardy at -12 C.
    public static float KillC(Hardiness hardiness)
    {
        switch (hardiness)
        {
            case Hardiness.Tender: return 0f;
            case Hardiness.SemiHardy: return -2f;
            case Hardiness.Tolerant: return -6f;
            default: return -12f;
        }
    }

    public static string HardinessText(Hardiness hardiness)
    {
        switch (hardiness)
        {
            case Hardiness.Tender: return "tender: any frost kills it";
            case Hardiness.SemiHardy: return "takes a light frost";
            case Hardiness.Tolerant: return "takes a hard frost";
            default: return "hardy: grows through winter";
        }
    }

    // Damage one hour of frost does: a little at the edge of the crop's tolerance, more the colder it gets, so a short light frost nips it and a long hard one kills it.
    public static float FrostDamage(Crop crop, float airC, bool ready)
    {
        if (crop == null || (ready && crop.frostProofWhenReady))
            return 0f;
        float kill = KillC(crop.hardiness);
        if (airC > kill)
            return 0f;
        return 0.15f + 0.05f * (kill - airC);
    }

    // --- Growth ---

    // The share of a growing day one hour at this temperature gives (0 to 1): nothing at or below baseC, full at or above fullC, and a stall (0.3) above heatC.
    public static float GrowthRate(Crop crop, float airC)
    {
        if (crop == null || airC <= crop.baseC)
            return 0f;
        if (airC > crop.heatC)
            return 0.3f;
        if (airC >= crop.fullC)
            return 1f;
        return (airC - crop.baseC) / (crop.fullC - crop.baseC);
    }

    // How much the soil's moisture lets the crop grow: full from 0.3, nothing below 0.1.
    public static float WaterFactor(float moisture)
    {
        if (moisture >= 0.3f)
            return 1f;
        if (moisture <= 0.1f)
            return 0f;
        return 0.3f + 0.7f * (moisture - 0.1f) / 0.2f;
    }

    public const float DroughtDamagePerHour = 0.01f;  // bone-dry soil (below 0.1): a crop dies in about four days
    public const float BoltDamagePerHour = 0.01f;     // lettuce in heat above its heatC

    // --- Soil moisture (0 dry, 1 soaked; 25 mm of rain soaks it) ---

    public const float MoisturePerRainMm = 0.04f;
    public const float MoisturePerLitre = 0.04f;    // 25 L poured on the square soaks it; a Bucket gives 0.4, a Watering Can 0.32
    public const float StartMoisture = 0.5f;        // a freshly dug bed

    // What warm air dries out of the soil in an hour: a little always above freezing, more the warmer it is (about 0.13 a day at 25 C, so a week of summer without rain dries a soaked bed).
    public static float DryingPerHour(float airC)
    {
        if (airC <= 0f)
            return 0f;
        return 0.001f + 0.0003f * Math.Max(0f, airC - 10f);
    }

    public static float AfterRain(float moisture, float rainMm) => Clamp01(moisture + rainMm * MoisturePerRainMm);

    public static float AfterWatering(float moisture, int litres) => Clamp01(moisture + litres * MoisturePerLitre);

    // Litres it takes to bring the soil up to soaked (rounded up).
    public static int LitresToSoak(float moisture) => (int)Math.Ceiling(Math.Max(0f, 1f - moisture) / MoisturePerLitre - 0.0001f);

    public static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

    // --- One hour for a planted bed ---

    public struct HourResult
    {
        public float growingDays, damage, moisture;
        public bool died, bolted;
    }

    // One game hour for a bed: the soil wets or dries, then a living crop grows (by temperature and water), and frost, drought or bolting hurt it.
    public static HourResult Hour(Crop crop, float growingDays, float damage, float moisture, bool dryBeans, bool dead, float airC, float rainMm) =>
        Hour(crop, growingDays, damage, moisture, dryBeans, dead, airC, rainMm, false, false);

    // The same for a raised bed (warmer soil, drains, never waterlogged) and under a Row Cover (frost kept off, a little slower in the heat) (GB2).
    public static HourResult Hour(Crop crop, float growingDays, float damage, float moisture, bool dryBeans, bool dead, float airC, float rainMm, bool raised, bool covered)
    {
        var r = new HourResult { growingDays = growingDays, damage = damage, moisture = moisture };
        float drain = raised && moisture > RaisedDrainAbove ? RaisedDrainPerHour : 0f;
        r.moisture = Clamp01(AfterRain(moisture, rainMm) - DryingPerHour(airC) - drain);
        if (crop == null || dead)
            return r;

        float days = DaysToReady(crop, dryBeans);
        bool ready = growingDays >= days;
        float rate = GrowthRate(crop, airC + (raised ? RaisedWarmthC : 0f)) * WaterFactor(r.moisture);
        if (!raised && r.moisture > WaterloggedAbove)
            rate *= WaterloggedFactor;
        if (covered && airC > CoverHeatC)
            rate *= CoverHeatFactor;
        r.growingDays = growingDays + rate / 24f;
        float hurt = FrostDamage(crop, airC + (covered ? CoverProtectionC : 0f), ready);
        if (r.moisture < 0.1f)
            hurt += DroughtDamagePerHour;
        if (crop.boltAfterDays > 0f && airC > crop.heatC)
            hurt += BoltDamagePerHour;
        r.damage = Clamp01(damage + hurt);
        if (crop.boltAfterDays > 0f && r.growingDays >= days + crop.boltAfterDays)
            r.bolted = true;
        r.died = r.damage >= 1f || r.bolted;
        return r;
    }

    // --- Ready, picks and yield ---

    public static float DaysToReady(Crop crop, bool dryBeans) => crop.id == BeansCropId && dryBeans ? DryBeanDays : crop.days;

    public static int Picks(Crop crop, bool dryBeans) => crop.id == BeansCropId && dryBeans ? 1 : crop.picks;

    public static string ProduceOf(Crop crop, bool dryBeans) => crop.id == BeansCropId && dryBeans ? DryBeansId : crop.produceId;

    public static int YieldOf(Crop crop, bool dryBeans) => crop.id == BeansCropId && dryBeans ? DryBeanYield : crop.yieldPerSquare;

    // How many picks are ready now and not yet taken. A once-only crop has its one pick at ready; a picked crop adds one every pickEveryDays after.
    public static int PicksReady(Crop crop, bool dryBeans, float growingDays, int picksTaken)
    {
        float days = DaysToReady(crop, dryBeans);
        if (crop == null || growingDays < days)
            return 0;
        int picks = Picks(crop, dryBeans);
        int due = picks <= 1 ? 1 : Math.Min(picks, 1 + (int)Math.Floor((growingDays - days) / crop.pickEveryDays));
        return Math.Max(0, due - picksTaken);
    }

    // All the picks have been taken: the plants are spent and the bed is cleared with E.
    public static bool Spent(Crop crop, bool dryBeans, int picksTaken) => crop != null && picksTaken >= Picks(crop, dryBeans);

    // Units one pick gives: the square's yield shared over its picks, cut by the damage the crop has taken (a crop at half damage gives half). At least 1 while it lives.
    // GB2: a worn bed cuts it too (YieldFactor: 75% when spent).
    public static int PickYield(Crop crop, bool dryBeans, float damage, float spent = 0f)
    {
        int picks = Math.Max(1, Picks(crop, dryBeans));
        float each = (float)YieldOf(crop, dryBeans) / picks * (1f - Clamp01(damage)) * YieldFactor(spent);
        return Math.Max(1, (int)Math.Round(each));
    }

    // --- Stages, for the prompt and the look of the plants ---

    public enum Stage { Bare, Sprouting, Growing, Maturing, Ready, Spent, Dead }

    public static Stage StageOf(Crop crop, bool dryBeans, float growingDays, int picksTaken, bool dead)
    {
        if (crop == null)
            return Stage.Bare;
        if (dead)
            return Stage.Dead;
        if (Spent(crop, dryBeans, picksTaken))
            return Stage.Spent;
        float share = growingDays / DaysToReady(crop, dryBeans);
        if (share >= 1f)
            return Stage.Ready;
        if (share < 0.15f)
            return Stage.Sprouting;
        return share < 0.5f ? Stage.Growing : Stage.Maturing;
    }

    // How big the plants are, 0.15 (just up) to 1 (full grown), for the drawing.
    public static float SizeOf(Crop crop, bool dryBeans, float growingDays)
    {
        if (crop == null)
            return 0f;
        return 0.15f + 0.85f * Clamp01(growingDays / DaysToReady(crop, dryBeans));
    }

    // --- Words ---

    public static string MoistureText(float moisture) =>
        moisture >= 0.7f ? "soil wet" : moisture >= 0.3f ? "soil moist" : moisture >= 0.1f ? "soil drying - water it" : "soil dry - water it now";

    public static string DamageText(float damage) =>
        damage <= 0.02f ? "" : damage < 0.35f ? ", a little damaged" : damage < 0.7f ? ", badly damaged" : ", nearly dead";

    public static string StageText(Stage stage)
    {
        switch (stage)
        {
            case Stage.Sprouting: return "sprouting";
            case Stage.Growing: return "growing";
            case Stage.Maturing: return "nearly ready";
            case Stage.Ready: return "ready";
            case Stage.Spent: return "picked out";
            case Stage.Dead: return "dead";
            default: return "bare";
        }
    }

    public static string DaysLeftText(Crop crop, bool dryBeans, float growingDays)
    {
        float left = DaysToReady(crop, dryBeans) - growingDays;
        if (left <= 0f)
            return "";
        int d = (int)Math.Ceiling(left);
        return d == 1 ? ", about 1 growing day to go" : $", about {d} growing days to go";
    }

    public const string Undug = "Garden Bed - not dug yet\nEquip the Shovel and hold click on it to dig it";
    public const string RaisedUnfilled = "Raised Bed - empty frame\nEquip the Shovel and hold click on it to fill it with soil";
    public const string RaisedFilledBanner = "The raised bed is filled - equip the Hoe and press E on it to plant.";
    public static readonly string RaisedHelp = $"a 4 by 4 ft garden bed in a board frame: the soil warms sooner in spring (about a week ahead) and drains, so heavy rain cannot waterlog it. Mark it out (free), deliver {RaisedBedLumber} Lumber with R, equip the Hammer and hold click until the frame is built, then fill it with soil using the Shovel (hold click). A dug Garden Bed can be raised too: R with the Hammer and {RaisedBedLumber} Lumber.";
    public const string DugBanner = "The bed is dug - equip the Hoe and press E on it to plant.";
    public const string NeedHoe = "Equip the Hoe to plant";
    public const string NoSeed = "No seed to plant - buy seed packets, potatoes or (in Spring) tomato plants at the Trading Post";

    // Tomato Plants are on the Trading Post shelf only from mid-April to early June, after the last frost, since a tomato dies in the first frost and needs
    // about 70 growing days (Mike, 2026-11-25: "Sell them only in Spring"). A plant bought then goes straight into a bed: it keeps only 7 days.
    public static bool TomatoPlantsInSeason(GameCalendar calendar, int day)
    {
        if (calendar == null || !calendar.HasRealMonths)
            return calendar != null && calendar.SeasonOfYear(day) == 0;
        string month = calendar.MonthName(calendar.MonthOfYear(day));
        int date = calendar.DayOfMonth(day);
        return (month == "Apr" && date >= 15) || month == "May" || (month == "Jun" && date <= 10);
    }

    public const string TomatoPlantSeason = "mid-April to early June";

    // The key line for planting (the HUD adds the key, so the text does not name it), with a warning under it when frost will likely spoil it (TimingWarning).
    public static string PlantPrompt(Crop crop, int carried, string warning = null) =>
        $"Plant {crop.name}  (uses {SeedWord(crop, crop.seedsPerSquare)}, you carry {carried}; {HardinessText(crop.hardiness)}, about {(int)crop.days} growing days)"
        + (string.IsNullOrEmpty(warning) ? "" : "\n" + warning);

    // Planting at the wrong time (Mike, 2026-11-25). A crop that frost hurts (tender or semi-hardy) planted so late that its growing days run past the usual
    // first fall frost (about mid-October in the game's Ohio), or a tender one planted before the usual last spring frost (about early May). The real growing
    // days stretch in cool weather, so this is a fair warning, not a promise. Null when the timing is fine.
    public const int LastFrostMonth = 2, LastFrostDate = 5;     // May 5 (months count from March: 0 Mar, 1 Apr, 2 May)
    public const int FirstFrostMonth = 7, FirstFrostDate = 15;  // October 15

    public static int DayOfYearOf(GameCalendar calendar, int monthOfYear, int date)
    {
        int d = 0;
        for (int m = 0; m < monthOfYear; m++)
            d += calendar.DaysInMonth(m);
        return d + date - 1;
    }

    public static string TimingWarning(Crop crop, GameCalendar calendar, int day)
    {
        if (crop == null || calendar == null || !calendar.HasRealMonths || crop.hardiness == Hardiness.Tolerant || crop.hardiness == Hardiness.Hardy)
            return null;
        int today = calendar.DayOfYear(day);
        int lastFrost = DayOfYearOf(calendar, LastFrostMonth, LastFrostDate);
        int firstFrost = DayOfYearOf(calendar, FirstFrostMonth, FirstFrostDate);
        if (crop.hardiness == Hardiness.Tender && today < lastFrost)
            return "<color=#AD2E14>Early: a late spring frost (until about early May) can still kill it</color>";
        if (today + crop.days > firstFrost)
            return today >= firstFrost ? "<color=#AD2E14>Too late in the year: frost will likely kill it before it is ready</color>"
                                       : "<color=#AD2E14>Late: it likely won't be ready before the first frost (about mid-October)</color>";
        return null;
    }

    // "1 packet", "4 plants", "8 potatoes".
    public static string SeedWord(Crop crop, int n)
    {
        if (crop.seedId == PotatoesId)
            return n == 1 ? "1 potato" : $"{n} potatoes";
        if (crop.seedId == "tomato_plant")
            return n == 1 ? "1 plant" : $"{n} plants";
        return n == 1 ? "1 packet" : $"{n} packets";
    }

    public static string Planted(Crop crop) => $"Planted {crop.name}. Keep the soil moist; it is {HardinessText(crop.hardiness)}.";

    public static string Harvested(int units, string name, int picksLeft) =>
        picksLeft > 0 ? $"Picked {name} x{units} - more to come ({picksLeft} more picking{(picksLeft == 1 ? "" : "s")})" : $"Harvested {name} x{units}";

    public const string Help = "a 4 by 4 ft garden bed. Mark it out anywhere open, then dig it with the Shovel (hold click). With the Hoe in hand, E plants the seed you carry (R picks which: seed packets, potatoes, or tomato plants, which the Trading Post sells from mid-April to early June). Crops grow day by day with the warmth and the soil's moisture: rain waters them, and in a dry spell pour water on with the Watering Can or Bucket (E). Frost kills tender crops: a Row Cover (Trading Post) laid on with R keeps a light frost off. E harvests when ready. Each crop wears the soil a little: E spreads Fertilizer to feed it. R with the Hammer and 2 Lumber makes it a raised bed (warmer, drains); a Shovel's R fills an empty bed in.";

    public const string KnowHowTitle = "The Kitchen Garden";
}
