using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpoilageSaveData
{
    public int version = 1;
    public double outdoor;
    public double cache;
    public double cellar;
    // Slice 3 (appended, so older saves load 0 and start the rack clock at the outdoor one): the Drying Rack's clock.
    public double rack;
    // Slice T1 (appended, so older saves load false): whether hides carry freshness stamps yet. A save without it gets its hides stamped half-fresh.
    public bool hidesStamped;
}

// Food spoilage (Food_Spoilage_and_Preservation.md, Mike 2026-11-22). Runs the three freshness clocks (SpoilageRules) once
// every game hour, by the air temperature (WeatherManager) and the difficulty (DifficultyManager.SpoilageMultiplier), then
// sweeps every place food can be: the pack, storage containers, piles and structures (WoodManager) and dropped piles. A
// batch past its life becomes Rot, one for each. Created on startup rather than placed in a scene, like DroppedItems.
//
// A save from before spoilage has no section for this; once everything has loaded, its perishable batches' acquired days are
// turned into stamps that keep their age (at most half their life, so nothing spoils the moment it loads).
public class SpoilageManager : MonoBehaviour, ISaveable
{
    public static SpoilageManager Instance { get; private set; }

    double outdoor, cache, cellar, rack;
    DayNightCycle dayNight;
    bool registered, hooked, restoredThisLoad;
    bool hidesStamped = true; // a new game's hides are stamped as they are picked up (Tanning_Hides_and_Furs.md section 2)

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null)
            return;
        new GameObject("Spoilage").AddComponent<SpoilageManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // SaveManager and TimeManager may not exist yet on the first frame, so keep trying until they do.
    void Update()
    {
        if (!registered && SaveManager.Instance != null)
        {
            SaveManager.Instance.Register(this);
            SaveManager.Instance.Loaded += OnLoaded;
            registered = true;
        }
        if (!hooked && TimeManager.Instance != null)
        {
            TimeManager.Instance.HourChanged += OnHour;
            hooked = true;
            if (outdoor <= 0)
                outdoor = cache = cellar = rack = TimeManager.Instance.TotalHours; // a fresh start: the clocks begin at the game's own hour
        }
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Unregister(this);
            SaveManager.Instance.Loaded -= OnLoaded;
        }
        if (TimeManager.Instance != null)
            TimeManager.Instance.HourChanged -= OnHour;
        Instance = null;
    }

    // --- The clocks ---

    public double ClockHours(FreshClock clock) => clock == FreshClock.Cellar ? cellar : clock == FreshClock.Cache ? cache : clock == FreshClock.Rack ? rack : outdoor;

    static double AirC => WeatherManager.Instance != null ? WeatherManager.Instance.TemperatureC : 15.0;
    static double Difficulty => DifficultyManager.SpoilageMultiplier;

    // Clock hours per game hour right now.
    public static double RateNow(FreshClock clock) => SpoilageRules.ClockRate(clock, AirC, Difficulty);

    // The stamp for food made fresh now on a clock (the outdoor clock when there is no manager, e.g. in tests: the game hour).
    public static int FreshStamp(FreshClock clock = FreshClock.Outdoor)
    {
        if (Instance != null)
            return SpoilageRules.Stamp(Instance.ClockHours(clock));
        return SpoilageRules.Stamp(TimeManager.Instance != null ? TimeManager.Instance.TotalHours : 0.0);
    }

    // Fresh now, less some hours of life (meat from a carcass that waited before field dressing).
    public static int AgedStamp(double ageHours, FreshClock clock = FreshClock.Outdoor) =>
        SpoilageRules.StampAged(Instance != null ? Instance.ClockHours(clock) : (TimeManager.Instance != null ? TimeManager.Instance.TotalHours : 0.0), ageHours);

    // Moves a perishable batch's stamp from one clock to another, keeping its age.
    public static int Convert(int stamp, FreshClock from, FreshClock to)
    {
        if (from == to || Instance == null)
            return stamp;
        return SpoilageRules.Convert(stamp, Instance.ClockHours(from), Instance.ClockHours(to));
    }

    // Which clock a pile or structure keeps its food on: the Food Cache its own; everything else the outdoors.
    public static FreshClock ClockOf(PileKind kind) => kind == PileKind.RootCellar ? FreshClock.Cellar : kind == PileKind.FoodCache ? FreshClock.Cache : kind == PileKind.DryingRack ? FreshClock.Rack : FreshClock.Outdoor;

    public static double FractionUsed(ItemDefinition item, int stamp, FreshClock clock = FreshClock.Outdoor)
    {
        if (item == null || !item.IsPerishable || Instance == null)
            return 0.0;
        return SpoilageRules.FractionUsed(stamp, Instance.ClockHours(clock), item.ShelfLifeDays);
    }

    // "2 days left" for the batch nearest to spoiling, coloured by how far gone it is (normal, amber, red). Null for non-perishables.
    public static string FreshnessTag(ItemDefinition item, int stamp, FreshClock clock = FreshClock.Outdoor, int size = 20)
    {
        if (item == null || !item.IsPerishable || Instance == null)
            return null;
        double clockNow = Instance.ClockHours(clock), rate = RateNow(clock);
        double used = SpoilageRules.FractionUsed(stamp, clockNow, item.ShelfLifeDays);
        string text = SpoilageRules.LeftText(SpoilageRules.DaysLeft(stamp, clockNow, item.ShelfLifeDays, rate), rate);
        string colour = SpoilageRules.StageOf(used) switch
        {
            Freshness.GoingOff => "#AD2E14",
            Freshness.Aging => "#94520E",
            _ => "#2E6121",
        };
        return $"<size={size}><color={colour}>{text}</color></size>";
    }

    void OnHour(int hour)
    {
        double outdoorStep = SpoilageRules.ClockRate(FreshClock.Outdoor, AirC, Difficulty);
        outdoor += outdoorStep;
        cache += SpoilageRules.ClockRate(FreshClock.Cache, AirC, Difficulty);
        cellar += SpoilageRules.ClockRate(FreshClock.Cellar, AirC, Difficulty);
        rack += SpoilageRules.ClockRate(FreshClock.Rack, AirC, Difficulty);
        SmokingTick(outdoorStep);
        Sweep();
        CompostTick();
        DryingTick();
    }

    // --- The Compost Bin (CompostRules) ---

    // Each game hour: an idle bin with at least one batch's worth starts a job; a running job counts down by the compost rate at today's
    // temperature and, when done, turns its batches' Rot and Tall Grass into Fertilizer in the bin. The job's inputs stay in the bin until
    // then (so taking the bin down returns them); if some were taken out meanwhile, only the batches still there are made.
    void CompostTick()
    {
        WoodManager wood = WoodManager.Instance;
        if (wood == null)
            return;
        double rate = CompostRules.RateAt(AirC);
        foreach (WoodPileState pile in wood.Piles)
        {
            if (pile.kind != PileKind.CompostBin)
                continue;
            bool changed = false;
            if (pile.jobItem != CompostRules.Marker)
            {
                int batches = CompostRules.Batches(pile.Count(CompostRules.RotId), pile.Count(CompostRules.GrassId));
                if (batches > 0)
                {
                    pile.jobItem = CompostRules.Marker;
                    pile.jobCount = batches;
                    pile.jobHours = (float)CompostRules.HoursPerJob;
                    changed = true;
                }
            }
            else
            {
                // A job started under a longer compost time (14 days before 2026-11-22) finishes on today's.
                pile.jobHours = Mathf.Min(pile.jobHours, (float)CompostRules.HoursPerJob) - (float)rate;
                if (pile.jobHours <= 0f)
                {
                    int made = Math.Min(pile.jobCount, CompostRules.Batches(pile.Count(CompostRules.RotId), pile.Count(CompostRules.GrassId)));
                    if (made > 0)
                    {
                        pile.Take(CompostRules.RotId, made * CompostRules.RotPerBatch);
                        pile.Take(CompostRules.GrassId, made * CompostRules.GrassPerBatch);
                        pile.Add(CompostRules.FertilizerId, made, 0);
                    }
                    pile.jobItem = null;
                    pile.jobCount = 0;
                    pile.jobHours = 0f;
                }
                changed = true;
            }
            if (changed)
                wood.NotifyChanged(pile);
        }
    }

    // "composting 2 batches, about 9 days left" while a job runs; when idle, what it still needs ("needs 1 more Rot to start").
    public static string CompostStatus(WoodPileState pile)
    {
        if (pile == null || pile.kind != PileKind.CompostBin)
            return null;
        if (pile.jobItem != CompostRules.Marker)
            return CompostRules.IdleStatus(pile.Count(CompostRules.RotId), pile.Count(CompostRules.GrassId));
        return CompostRules.Status(pile.jobCount, pile.jobHours, CompostRules.RateAt(AirC));
    }

    // --- The Drying Rack (DryingRules) ---

    // Daylight: the sun above the horizon (6 in the morning to 6 at night when there is no sky to ask).
    bool Daylight()
    {
        if (dayNight == null)
            dayNight = FindAnyObjectByType<DayNightCycle>();
        if (dayNight != null)
            return dayNight.SunElevation > 0f;
        int hour = TimeManager.Instance != null ? (int)(TimeManager.Instance.TotalHours % 24.0) : 12;
        return hour >= 6 && hour < 18;
    }

    static bool Raining => WeatherManager.Instance != null && WeatherManager.Instance.IsRaining;
    static bool Snowing => WeatherManager.Instance != null && WeatherManager.Instance.IsSnowing;

    // Each game hour that is light and dry, every fresh batch on a rack dries one hour more; a batch that has dried long enough becomes its
    // dried form, fresh on the rack's clock. Night, rain and snow pause it (the food still ages meanwhile, at the rack's slower rate).
    void DryingTick()
    {
        WoodManager wood = WoodManager.Instance;
        if (wood == null || !DryingRules.DriesNow(Daylight(), Raining || Snowing))
            return;
        int fresh = FreshStamp(FreshClock.Rack);
        foreach (WoodPileState pile in wood.Piles)
        {
            if (pile.kind != PileKind.DryingRack)
                continue;
            bool changed = false;
            var done = new List<(string driedId, int count)>();
            for (int i = pile.contents.Count - 1; i >= 0; i--)
            {
                WoodStack stack = pile.contents[i];
                if (stack.count <= 0 || !DryingRules.TryFind(stack.itemId, out string driedId, out int hours))
                    continue;
                stack.dried += 1f;
                changed = true;
                if (stack.dried < hours)
                    continue;
                ItemDefinition item = ItemDatabase.Get(stack.itemId);
                done.Add((driedId, stack.count * DryingRules.YieldOf(stack.itemId, item != null ? item.WeightKg : 0.0)));
                pile.contents.RemoveAt(i);
            }
            foreach ((string driedId, int count) in done)
                pile.Add(driedId, count, fresh);
            if (changed)
                wood.NotifyChanged(pile);
        }
    }

    // How many fresh pieces a rack is drying.
    public static int DryingCount(WoodPileState pile)
    {
        int n = 0;
        if (pile != null)
            foreach (WoodStack stack in pile.contents)
                if (DryingRules.Dries(stack.itemId))
                    n += stack.count;
        return n;
    }

    // "drying 6 pieces", "drying paused for the night" and so on; null when the rack is drying nothing.
    public static string DryingStatus(WoodPileState pile)
    {
        if (pile == null || pile.kind != PileKind.DryingRack)
            return null;
        bool day = Instance != null ? Instance.Daylight() : true;
        return DryingRules.Status(DryingCount(pile), day, Raining, Snowing);
    }

    // --- The Smokehouse (SmokeRules) ---

    // Each game hour, every lit smokehouse burns: a new Firewood goes on when the last has burned through; with none left the hour is lost and the
    // meat ages like any raw meat outdoors. An hour that burns moves each raw batch's stamp on by the hour's outdoor clock, so the meat does not age
    // while it smokes (the part-hours are kept in WoodStack.dried, which a raw batch in a smokehouse has no other use for). When the hours run out,
    // every raw piece becomes its smoked pieces, fresh.
    void SmokingTick(double outdoorStep)
    {
        WoodManager wood = WoodManager.Instance;
        if (wood == null)
            return;
        foreach (WoodPileState house in wood.Piles)
        {
            if (house.kind != PileKind.Smokehouse || !house.SmokeRunning)
                continue;
            BurnHour(house, outdoorStep, FreshStamp());
            wood.NotifyChanged(house);
        }
    }

    // One hour of a lit smokehouse (public static for tests). freshStamp: the outdoor clock's stamp for food made now. Returns true on the hour the load finishes.
    public static bool BurnHour(WoodPileState house, double outdoorStep, int freshStamp)
    {
        SmokeRules.Hour hour = SmokeRules.AfterHour(house.jobHours, house.jobCount, house.SmokeFirewood);
        if (hour.firewoodUsed > 0)
            house.Take(SmokeRules.FirewoodId, hour.firewoodUsed);
        house.jobCount = hour.fuelHours;
        house.jobHours = hour.hoursLeft;
        if (hour.smoked)
        {
            foreach (WoodStack stack in house.contents)
            {
                if (!SmokeRules.Smokes(stack.itemId) || stack.day <= 0)
                    continue;
                stack.dried += (float)outdoorStep;
                int whole = (int)Math.Floor(stack.dried);
                if (whole > 0)
                {
                    stack.day += whole;
                    stack.dried -= whole;
                }
            }
        }
        if (!SmokeRules.IsDone(house.jobHours))
            return false;
        var made = new Dictionary<string, int>();
        for (int i = house.contents.Count - 1; i >= 0; i--)
        {
            WoodStack stack = house.contents[i];
            if (!SmokeRules.TryFind(stack.itemId, out string smokedId))
                continue;
            made[smokedId] = (made.TryGetValue(smokedId, out int n) ? n : 0) + stack.count * SmokeRules.YieldOf(stack.itemId);
            house.contents.RemoveAt(i);
        }
        foreach (KeyValuePair<string, int> pair in made)
            house.Add(pair.Key, pair.Value, freshStamp);
        house.jobItem = "";
        house.jobHours = 0f;
        house.jobCount = 0;
        return true;
    }

    // --- The sweep: spoiled food becomes Rot wherever it is ---

    public void Sweep()
    {
        ItemDefinition rot = ItemDatabase.Get(SpoilageRules.RotId);
        if (rot == null)
            return;

        InventoryManager inventory = InventoryManager.Instance;
        if (inventory != null)
        {
            var lost = inventory.Player.ReplaceSpoiled((item, stamp) => Spoiled(item, stamp, FreshClock.Outdoor), rot);
            if (lost.Count > 0)
                ToolStatus.Flash($"{Describe(lost)} spoiled and turned to Rot", 5f);
            foreach (InventoryContainer container in inventory.Storage)
                container.ReplaceSpoiled((item, stamp) => Spoiled(item, stamp, FreshClock.Outdoor), rot);
        }

        WoodManager wood = WoodManager.Instance;
        if (wood != null)
        {
            foreach (WoodPileState pile in wood.Piles)
            {
                FreshClock clock = ClockOf(pile.kind);
                int rotCount = 0;
                for (int i = pile.contents.Count - 1; i >= 0; i--)
                {
                    WoodStack stack = pile.contents[i];
                    ItemDefinition item = ItemDatabase.Get(stack.itemId);
                    if (item == null || !item.IsPerishable || stack.count <= 0 || stack.day <= 0 || !Spoiled(item, stack.day, clock))
                        continue;
                    rotCount += stack.count;
                    pile.contents.RemoveAt(i);
                }
                if (rotCount > 0)
                {
                    pile.Add(SpoilageRules.RotId, rotCount, 0);
                    wood.NotifyChanged(pile);
                }
            }
        }

        foreach (ItemPickup pickup in FindObjectsByType<ItemPickup>(FindObjectsSortMode.None))
        {
            if (pickup == null || !pickup.Dropped || pickup.Item == null || !pickup.Item.IsPerishable || pickup.AcquiredDay < 0)
                continue;
            if (Spoiled(pickup.Item, pickup.AcquiredDay, FreshClock.Outdoor))
                DroppedItems.TurnTo(pickup, rot);
        }
    }

    bool Spoiled(ItemDefinition item, int stamp, FreshClock clock) =>
        SpoilageRules.IsSpoiled(stamp, ClockHours(clock), item.ShelfLifeDays);

    static string Describe(List<(string itemId, int quantity)> lost)
    {
        var totals = new Dictionary<string, int>();
        foreach ((string id, int n) in lost)
            totals[id] = (totals.TryGetValue(id, out int t) ? t : 0) + n;
        var parts = new List<string>();
        foreach (KeyValuePair<string, int> pair in totals)
            parts.Add($"{pair.Value} {ItemDatabase.Get(pair.Key)?.DisplayName ?? pair.Key}");
        return string.Join(", ", parts);
    }

    // --- Saving ---

    public SpoilageSaveData CaptureState() => new SpoilageSaveData { outdoor = outdoor, cache = cache, cellar = cellar, rack = rack, hidesStamped = true };

    public void RestoreState(SpoilageSaveData data)
    {
        outdoor = data.outdoor;
        cache = data.cache;
        cellar = data.cellar;
        rack = data.rack > 0 ? data.rack : data.outdoor; // a save from before the Drying Rack
        hidesStamped = data.hidesStamped;
        restoredThisLoad = true;
    }

    // After every section has loaded: a save from before spoilage gets its acquired days turned into freshness stamps.
    void OnLoaded()
    {
        if (!restoredThisLoad)
            MigrateFromDays(); // turns the hides' days into stamps along with the food's
        else if (!hidesStamped)
            StampOldHides();
        hidesStamped = true;
        restoredThisLoad = false;
    }

    // A save from before slice T1: its hides kept the day they were picked up (or nothing, in a pile), not a freshness stamp. Each one is
    // stamped half-fresh, as food was when spoilage first came in, so nothing turns to Rot the moment the save loads.
    public void StampOldHides()
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory != null)
        {
            Func<ItemDefinition, int, int> map = (item, stamp) => HideRules.IsRawHide(item.Id)
                ? SpoilageRules.StampAged(outdoor, HideRules.HalfFreshAgeHours(item.ShelfLifeDays)) : stamp;
            inventory.Player.RestampPerishables(map);
            foreach (InventoryContainer container in inventory.Storage)
                container.RestampPerishables(map);
        }

        WoodManager wood = WoodManager.Instance;
        if (wood != null)
        {
            foreach (WoodPileState pile in wood.Piles)
            {
                bool changed = false;
                foreach (WoodStack stack in pile.contents)
                {
                    ItemDefinition item = ItemDatabase.Get(stack.itemId);
                    if (item == null || !HideRules.IsRawHide(item.Id))
                        continue;
                    stack.day = SpoilageRules.StampAged(ClockHours(ClockOf(pile.kind)), HideRules.HalfFreshAgeHours(item.ShelfLifeDays));
                    changed = true;
                }
                if (changed)
                {
                    pile.MergeSameDay();
                    wood.NotifyChanged(pile);
                }
            }
        }

        foreach (ItemPickup pickup in FindObjectsByType<ItemPickup>(FindObjectsSortMode.None))
            if (pickup != null && pickup.Dropped && pickup.Item != null && HideRules.IsRawHide(pickup.Item.Id))
                pickup.Restamp(SpoilageRules.StampAged(outdoor, HideRules.HalfFreshAgeHours(pickup.Item.ShelfLifeDays)));
    }

    public void MigrateFromDays()
    {
        TimeManager time = TimeManager.Instance;
        int today = time != null ? time.TotalDays : 0;
        outdoor = cache = cellar = rack = time != null ? time.TotalHours : 0.0;
        double now = outdoor;

        InventoryManager inventory = InventoryManager.Instance;
        if (inventory != null)
        {
            Func<ItemDefinition, int, int> map = (item, day) => SpoilageRules.MigrateDayStamp(day, today, now, item.ShelfLifeDays);
            inventory.Player.RestampPerishables(map);
            foreach (InventoryContainer container in inventory.Storage)
                container.RestampPerishables(map);
        }

        WoodManager wood = WoodManager.Instance;
        if (wood != null)
        {
            foreach (WoodPileState pile in wood.Piles)
            {
                bool changed = false;
                foreach (WoodStack stack in pile.contents)
                {
                    ItemDefinition item = ItemDatabase.Get(stack.itemId);
                    if (item == null || !item.IsPerishable)
                        continue;
                    // A pile's day 0 meant "no stamp" (food from before batch days existed): treat it as acquired today.
                    stack.day = SpoilageRules.MigrateDayStamp(stack.day > 0 ? stack.day : today, today, now, item.ShelfLifeDays);
                    changed = true;
                }
                if (changed)
                {
                    pile.MergeSameDay();
                    wood.NotifyChanged(pile);
                }
            }
        }

        foreach (ItemPickup pickup in FindObjectsByType<ItemPickup>(FindObjectsSortMode.None))
            if (pickup != null && pickup.Dropped && pickup.Item != null && pickup.Item.IsPerishable && pickup.AcquiredDay >= 0)
                pickup.Restamp(SpoilageRules.MigrateDayStamp(pickup.AcquiredDay, today, now, pickup.Item.ShelfLifeDays));
    }

    string ISaveable.SaveFile => "world";
    string ISaveable.SaveKey => "spoilage";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<SpoilageSaveData>(json));
}
