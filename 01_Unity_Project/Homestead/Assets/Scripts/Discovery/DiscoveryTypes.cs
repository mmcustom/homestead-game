using System;
using System.Collections.Generic;
using UnityEngine;

// Discovery_System.md's discovery categories (also its journal categories).
public enum DiscoveryCategory { WaterSource, Plant, Wildlife, Fishing, PropertyFeature }

// One piece of recorded information, e.g. "Water Quality" = "Excellent" or "Harvest Season" = "Summer".
// Discovery_System.md lists what each category records; the site's author fills these in.
[Serializable]
public class DiscoveryFact
{
    public string label;
    public string value;
}

// Something learned about a known site after discovering it, e.g. "Slower after cold fronts" (Weather_System.md).
[Serializable]
public class DiscoveryObservation
{
    public int day;
    public string text;
}

// Everything the player knows about one discovered site. Stored in the save so the journal and maps
// can show it even when the World scene (and the site object) isn't loaded.
[Serializable]
public class DiscoveryRecord
{
    public string siteId;
    public string displayName;
    public DiscoveryCategory category;
    public Vector3 position;
    public int dayDiscovered;
    // Optional map icon within the category, e.g. "nut" or "mushroom" for a Plant (Foraging_System.md's 🌰🍄).
    public string markerIcon = "";
    public List<DiscoveryFact> facts = new List<DiscoveryFact>();
    public List<DiscoveryObservation> observations = new List<DiscoveryObservation>();
}

// Discovery_System.md's Historical Records: permanent "firsts" such as First Spring Found.
[Serializable]
public class DiscoveryMilestone
{
    public string id;
    public string text;
    public int day;
}

public static class DiscoveryCategoryNames
{
    public static string Of(DiscoveryCategory category)
    {
        switch (category)
        {
            case DiscoveryCategory.WaterSource: return "Water Source";
            case DiscoveryCategory.Plant: return "Plant";
            case DiscoveryCategory.Wildlife: return "Wildlife Area";
            case DiscoveryCategory.Fishing: return "Fishing Location";
            case DiscoveryCategory.PropertyFeature: return "Property Feature";
            default: return category.ToString();
        }
    }

    // The fact shown in the discovery notification: the first entry in Discovery_System.md's "Information Recorded"
    // list for the category that describes the site itself (Location and Discovery Date are recorded separately).
    public static string HeadlineFactLabel(DiscoveryCategory category)
    {
        switch (category)
        {
            case DiscoveryCategory.WaterSource: return "Water Quality";
            case DiscoveryCategory.Plant: return "Harvest Season";
            case DiscoveryCategory.Wildlife: return "Activity Periods";
            case DiscoveryCategory.Fishing: return "Species Found";
            case DiscoveryCategory.PropertyFeature: return "Buildability";
            default: return null;
        }
    }

    // e.g. "Spring Hollow — Water Quality: Excellent". Just the name if the site doesn't record its headline fact.
    public static string Summary(DiscoveryRecord record)
    {
        string label = HeadlineFactLabel(record.category);
        DiscoveryFact fact = record.facts.Find(f => f.label == label);
        return fact != null ? $"{record.displayName} — {fact.label}: {fact.value}" : record.displayName;
    }

    // Text for the automatic first-of-category milestone, e.g. "First Water Source found".
    // No site name (Mike, 2026-09-23) — the site's own journal entry already records it.
    public static string FirstFound(DiscoveryCategory category) => $"First {Of(category)} found";
}
