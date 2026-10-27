using System;
using System.Globalization;

// Maple B (Early_Income_Design.md section 3): the pure rules behind taps in the world, on top of MapleRules. Pure and engine-free so edit-mode
// tests cover them. TapManager (the saved taps, the hourly tick, placing and checking) and Tap (the view and its prompts) call these.
public static class TapRules
{
    // How far the Spile reaches to a trunk (metres), and how high up the trunk the tap goes.
    public const float ReachMetres = 3f;
    public const float TapHeightMetres = 1f;

    // The sap-ready message fires when a bucket first holds this much.
    public const float ReadyLitres = 4f;

    // A tap whose saved tree no longer matches the terrain (a regenerated terrain): the tree at the saved index must be within this of the
    // saved position (beyond the trunk), or a Maple is searched for within SearchMetres of the saved position, else the tap is refunded.
    public const float MatchMetres = 0.6f;
    public const float SearchMetres = 1.5f;

    // A tap taken down or lost with its tree gives back one of each.
    public const int SpilesBack = 1;
    public const int BucketsBack = 1;

    // The player must be this close (metres) to a felled tapped tree for the refund to go straight to the pack; further off (a storm) it is
    // left on the ground by the stump.
    public const float PackRefundMetres = 10f;

    // Whether a tap can be put up on a tree, and if not, why (checked in this order).
    public enum PlaceResult { Ok, NotAMaple, AlreadyTapped, TooManyTaps, NoSpile, NoBucket }

    public static PlaceResult CanPlace(bool isMaple, bool alreadyTapped, int tapsOut, bool hasSpile, bool hasBucket)
    {
        if (!isMaple)
            return PlaceResult.NotAMaple;
        if (alreadyTapped)
            return PlaceResult.AlreadyTapped;
        if (!MapleRules.CanAddTap(tapsOut))
            return PlaceResult.TooManyTaps;
        if (!hasSpile)
            return PlaceResult.NoSpile;
        if (!hasBucket)
            return PlaceResult.NoBucket;
        return PlaceResult.Ok;
    }

    public static string PlaceMessage(PlaceResult result)
    {
        switch (result)
        {
            case PlaceResult.NotAMaple: return "Not a maple you can tap.";
            case PlaceResult.AlreadyTapped: return "This maple is already tapped.";
            case PlaceResult.TooManyTaps: return $"You already have {MapleRules.MaxTaps} taps out.";
            case PlaceResult.NoSpile: return "You need a Spile.";
            case PlaceResult.NoBucket: return "You need a Sap Bucket to hang on the tap.";
            default: return "";
        }
    }

    // Takes sap from a bucket: a fresh one gives its whole litres up to maxLitres and keeps the rest (the leftover keeps its age, because it is
    // the same sap); a sour one is poured out whatever the pack can carry. maxLitres of 0 takes nothing from a fresh bucket.
    public static MapleRules.Collected CollectUpTo(MapleRules.Bucket bucket, int maxLitres)
    {
        if (bucket.sour)
            return MapleRules.Collect(bucket);

        int take = Math.Max(0, Math.Min(MapleRules.Whole(bucket.litres), maxLitres));
        float left = bucket.litres - take;
        if (left < MapleRules.Epsilon)
            left = 0f;
        var result = new MapleRules.Collected { litres = take };
        result.bucket = new MapleRules.Bucket { litres = left, ageHours = left > 0f ? bucket.ageHours : 0f, sour = false };
        return result;
    }

    // One hour at one tap: this hour's flow (or not) goes into its bucket, scaled by the tree's size.
    public static MapleRules.Bucket TapHour(MapleRules.Bucket bucket, bool flowHour, float heightScale) =>
        MapleRules.Hour(bucket, MapleRules.SapThisHour(flowHour, heightScale));

    // True the hour a bucket first reaches ReadyLitres.
    public static bool BecameReady(float litresBefore, float litresAfter) => litresBefore < ReadyLitres && litresAfter >= ReadyLitres;

    static string Litres(float litres) => litres.ToString("0.#", CultureInfo.InvariantCulture) + " L";

    // The bucket in a few words for the look prompt: "empty", "3.9 L, fresh", "6 L, souring in 10 h", "sour". flowing is whether sap is
    // running into it right now.
    public static string Status(MapleRules.Bucket bucket, bool flowing)
    {
        string flow = flowing ? "flowing" : "not flowing";
        if (bucket.sour)
            return "sour, empty it";
        if (bucket.litres <= 0f)
            return "empty, " + flow;

        string amount = bucket.IsFull ? "full, " + Litres(MapleRules.BucketCapacityLitres) : Litres(bucket.litres);
        float hoursLeft = MapleRules.SourAfterHours - bucket.ageHours;
        string freshness = bucket.ageHours >= SoursSoonAfterHours ? $"souring in {Math.Max(1, (int)Math.Ceiling(hoursLeft - MapleRules.Epsilon))} h" : "fresh";
        return $"{amount}, {freshness}, {flow}";
    }

    // The status turns from "fresh" to a countdown once the sap is this old (hours since the first sap).
    public const float SoursSoonAfterHours = 24f;

    // The short message after a check, in the order the cases come up.
    public static string CollectMessage(MapleRules.Bucket before, MapleRules.Collected taken)
    {
        if (before.sour)
            return taken.spoiled > 0 ? $"The sap had soured: poured out {taken.spoiled} L." : "The sap had soured: the bucket is emptied.";
        int whole = MapleRules.Whole(before.litres);
        if (whole <= 0)
            return "No sap to collect yet.";
        if (taken.litres <= 0)
            return "No room to carry any sap.";
        if (taken.litres < whole)
            return $"Took {taken.litres} L of sap: no room for the other {whole - taken.litres} L, still in the bucket.";
        return $"Collected {taken.litres} L of sap.";
    }

    // Whether a tap's saved position still sits at the tree at its saved index (horizontal distance from the tree's trunk to the tap).
    public static bool PositionMatches(float horizontalDistance, float trunkRadius) =>
        horizontalDistance <= trunkRadius + MatchMetres;
}
