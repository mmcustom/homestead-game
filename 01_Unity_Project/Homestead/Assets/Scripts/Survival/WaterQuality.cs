// Water_System.md's Water Quality Levels. Collected water keeps its source's quality as a separate item per level,
// so the Water Purification step can tell them apart (illness risk and boiling aren't modelled yet).
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

    // Litres of raw water the player is carrying, across every quality.
    public static int LitresCarried(InventoryContainer container)
    {
        int total = 0;
        foreach (string id in AllItemIds)
            total += container.Count(id);
        return total;
    }
}
