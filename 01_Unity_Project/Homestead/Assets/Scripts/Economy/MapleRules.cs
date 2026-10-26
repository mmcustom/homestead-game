using System;

// Early_Income_Design.md section 3 and Maple_Sugaring.md: which trees can be tapped, when the sap runs, how a Sap Bucket fills and sours, and
// what boiling turns it into. Pure and engine-free so edit-mode tests cover it; the taps in the world (Maple B) and the boil at a fire
// (Maple C) call these. Nothing here touches a manager, a tree or the clock: temperature, season and tree size come in as plain numbers.
//
// Every number is a named constant (Claude Code's first pass, accepted by Mike 2026-10-26). Seasons use the Season enum's order, the same as
// MarketRules: 0 Spring, 1 Summer, 2 Fall, 3 Winter.
public static class MapleRules
{
    public const string SapId = "maple_sap";
    public const string SyrupId = "maple_syrup";
    public const string SpileId = "spile";
    public const string SapBucketId = "sap_bucket";

    public const int Spring = 0, Winter = 3;

    // --- Which trees are Maples ---

    // WoodManager's prototype numbers (this assembly cannot see them): 0 Hardwood Broad, 1 Hardwood Tall, 2 Understory Shrub.
    public const int BroadHardwood = 0, TallHardwood = 1;

    // A "large" hardwood has a height scale of at least this (the terrain's trees run 0.8 to 1.35, so about 28% of hardwoods).
    public const float LargeHeightScale = 1.2f;

    // About 1 in this many large hardwoods is a Maple (Mike, 2026-10-26: about 290 on the map; 1 in 10 gave only 204 and few near the start, so 1 in 7).
    public const int OneMapleIn = 7;

    public static bool IsHardwood(int prototype) => prototype == BroadHardwood || prototype == TallHardwood;

    // A mixed index: the terrain's trees are listed in a sweep across the ground, so index % 7 would line Maples up in stripes.
    public static int Mix(int index) => (int)(((uint)index * 2654435761u) >> 16);

    // Whether the tree at this index of the terrain's tree list is a tappable Maple. Pure of the index, the prototype and the size, so it
    // is the same in every save and nothing about it needs saving. Shrubs and small hardwoods are never Maples.
    public static bool IsMaple(int index, int prototype, float heightScale) =>
        index >= 0 && IsHardwood(prototype) && heightScale >= LargeHeightScale && Mix(index) % OneMapleIn == 0;

    // --- When the sap runs ---

    // A flow hour is warmer than this and comes within FreezeMemoryHours of a freeze at or below FreezeC (Celsius: about 34 F and 30 F).
    public const float ThawC = 1f;
    public const float FreezeC = -1f;
    public const float FreezeMemoryHours = 24f;

    // The memory is capped so a long warm spell cannot grow it without bound (and the saved float stays small).
    public const float FreezeMemoryCap = 48f;

    // Sap runs only in Winter and Spring: the temperature rule alone would also flow on cold nights in late Fall.
    public static bool InSapSeason(int season) => season == Winter || season == Spring;

    // Hours since the temperature was last at or below FreezeC, after one more hour at tempC: 0 on a freezing hour, otherwise one more.
    public static float NextHoursSinceFreeze(float hoursSinceFreeze, float tempC)
    {
        if (tempC <= FreezeC)
            return 0f;
        return Math.Min(FreezeMemoryCap, Math.Max(0f, hoursSinceFreeze) + 1f);
    }

    // Whether this hour is a flow hour. hoursSinceFreeze is the value AFTER NextHoursSinceFreeze for this hour. Freezing hours do not flow
    // (the tap is frozen); the thaw after a freeze does, until the memory runs out.
    public static bool IsFlowHour(int season, float tempC, float hoursSinceFreeze) =>
        InSapSeason(season) && tempC > ThawC && hoursSinceFreeze < FreezeMemoryHours;

    // --- A tap and its bucket ---

    public const float SapPerFlowHour = 0.7f; // litres a tap gives in one flow hour at an average tree (about 3.9 L a day at 5.5 flow hours)
    public const float SmallestTreeFactor = 0.85f, LargestTreeFactor = 1.2f;
    public const float SmallestHeightScale = 0.8f, LargestHeightScale = 1.35f;

    public const float BucketCapacityLitres = 8f;
    public const float SourAfterHours = 48f; // sap that has sat this long since the bucket's first sap has turned
    public const int MaxTaps = 12;           // soft cap on taps at once (Maple B enforces it)

    // Bigger trees give more: a straight line from SmallestTreeFactor at the smallest tree to LargestTreeFactor at the largest.
    public static float TreeFactor(float heightScale)
    {
        float t = (heightScale - SmallestHeightScale) / (LargestHeightScale - SmallestHeightScale);
        t = Math.Max(0f, Math.Min(1f, t));
        return SmallestTreeFactor + (LargestTreeFactor - SmallestTreeFactor) * t;
    }

    // Litres one tap gives in an hour: 0 when the hour is not a flow hour.
    public static float SapThisHour(bool flowHour, float heightScale) => flowHour ? SapPerFlowHour * TreeFactor(heightScale) : 0f;

    // A bucket's contents. ageHours counts from the first sap in it (0 while it is empty); once sour it holds nothing useful and takes no more.
    public struct Bucket
    {
        public float litres;
        public float ageHours;
        public bool sour;

        public bool IsEmpty => litres <= 0f && !sour;
        public bool IsFull => litres >= BucketCapacityLitres - Epsilon;
    }

    public const float Epsilon = 0.0001f;

    // One hour passes with this many litres running into the bucket. Sap that would not fit is lost; sap that has sat for SourAfterHours turns
    // the whole bucket sour (the sap in it is lost when it is emptied).
    public static Bucket Hour(Bucket bucket, float litresFlowing)
    {
        if (bucket.sour)
            return bucket;

        bool hadSap = bucket.litres > 0f;
        float litres = Math.Min(BucketCapacityLitres, bucket.litres + Math.Max(0f, litresFlowing));
        float age = hadSap ? bucket.ageHours + 1f : 0f;
        if (litres > 0f && age >= SourAfterHours - Epsilon)
            return new Bucket { litres = litres, ageHours = age, sour = true };
        return new Bucket { litres = litres, ageHours = age, sour = false };
    }

    public struct Collected
    {
        public int litres;       // whole litres of good sap taken
        public int spoiled;      // whole litres thrown out because the bucket had gone sour
        public Bucket bucket;    // what is left hanging
    }

    // Empties the bucket: a fresh one gives its whole litres (a fraction stays and starts fresh), a sour one is poured out and gives nothing.
    public static Collected Collect(Bucket bucket)
    {
        if (bucket.sour)
            return new Collected { spoiled = Whole(bucket.litres), bucket = default(Bucket) };

        int whole = Whole(bucket.litres);
        float left = bucket.litres - whole;
        if (left < Epsilon)
            left = 0f;
        return new Collected { litres = whole, bucket = new Bucket { litres = left, ageHours = 0f, sour = false } };
    }

    public static int Whole(float litres) => litres <= 0f ? 0 : (int)Math.Floor(litres + Epsilon);

    // Whether another tap may be put up with this many already out.
    public static bool CanAddTap(int tapsOut) => tapsOut < MaxTaps;

    // --- Boiling at a fire ---

    public const int SapPerSyrup = 10;            // litres of sap boiled down into one Syrup
    public const float SecondsPerLitre = 1.2f;    // real seconds at a lit fire, per litre
    public const float SecondsPerSyrup = SapPerSyrup * SecondsPerLitre;

    public static int SyrupFrom(int litres) => litres <= 0 ? 0 : litres / SapPerSyrup;

    // Litres of sap that do not make a whole Syrup and stay in the pack.
    public static int LitresLeftOver(int litres) => litres <= 0 ? 0 : litres % SapPerSyrup;

    // --- What the tap kit costs to make (the recipes themselves arrive with Maple B) ---

    public const int SpileSticks = 3;
    public const int BucketLogs = 1, BucketBranches = 3, BucketCordage = 1;

    // --- The market: Syrup is its own group, $10, 5% off per unit, floor 0.25 of normal, the same price all year ---

    public const long SyrupBaseCents = 1000;
    public const int DropBp = 500;
    public const int FloorBp = 2500;
}
