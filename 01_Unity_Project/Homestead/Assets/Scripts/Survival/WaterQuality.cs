// Water_System.md's Water Quality Levels. Collected water keeps its source's quality as a separate item per level.
// Each level's illness chance per litre lives on its water item (ItemDefinition.IllnessChance) and applies to drinking
// straight from the source too; boiling at a campfire turns any of them into Purified Water (Cooking).
public enum WaterQuality { Excellent, Good, Questionable, Unsafe }

public static class WaterQualities
{
    // One litre of raw water from a source of this quality; 1 kg each, so a full 10 L bucket weighs 10 kg.
    public static string ItemId(WaterQuality quality)
    {
        switch (quality)
        {
            case WaterQuality.Excellent: return "water_excellent";
            case WaterQuality.Good: return "water_good";
            case WaterQuality.Questionable: return "water_questionable";
            default: return "water_unsafe";
        }
    }

    // Hydration from drinking one litre, matching a drink taken straight from the source.
    public const float HydrationPerLitre = 20f;

    public static readonly string[] AllItemIds = { "water_excellent", "water_good", "water_questionable", "water_unsafe" };

    public static bool IsRawWater(string itemId) => System.Array.IndexOf(AllItemIds, itemId) >= 0;

    // Chance of falling ill from drinking one litre of this quality.
    public static float IllnessChance(WaterQuality quality)
    {
        ItemDefinition water = ItemDatabase.Get(ItemId(quality));
        return water != null ? water.IllnessChance : 0f;
    }

    // The Canteen (Difficulty_System.md's kit): a small carried water container, lighter than the Bucket.
    public const string CanteenId = "canteen";
    public const int LitresPerCanteen = 2;

    // Litres the player's water containers hold between them: every Bucket and Canteen carried.
    public static int Capacity(InventoryContainer container) =>
        container.Count(WaterSource.BucketItemId) * WaterSource.LitresPerBucket + container.Count(CanteenId) * LitresPerCanteen;

    // Litres of water the player is carrying in their buckets and canteens, across every quality and purified.
    public static int LitresCarried(InventoryContainer container)
    {
        int total = container.Count(Cooking.PurifiedWaterId);
        foreach (string id in AllItemIds)
            total += container.Count(id);
        return total;
    }
}
