using UnityEngine;

// How a forage patch looks in the world, built from simple shapes by ForageVisuals.
public enum ForageLook { BerryBush, FruitTree, NutTree, GroundGreens, Cattail, GroundMushroom, LogMushroom }

// Regrowth after a patch is picked (Plants docs): Annual comes back the same season next year (Berries, Fruit, Nuts,
// Morel); Days comes back after a few in-game days within the same season (Dandelion, Wild Onion, Cattail, and the
// two wood-growing mushrooms).
public enum ForageRegrowth { Annual, Days }

// One forageable species' numbers, from its sheet in 05_Documentation/Plants. One asset per species under
// Assets/Resources/Forage; ForageNode looks them up by item id.
[CreateAssetMenu(menuName = "Homestead/Forage Species", fileName = "NewForageSpecies")]
public class ForageSpecies : ScriptableObject
{
    [Tooltip("The item harvested, e.g. blackberries.")]
    public string itemId;
    [Tooltip("What a patch is called when discovered, e.g. Blackberry Patch.")]
    public string patchName;
    [Tooltip("Map icon: berry, fruit, nut, greens or mushroom.")]
    public string markerIcon = "berry";
    public ForageLook look;

    [Header("Season")]
    public Season season = Season.Summer;
    [Tooltip("Part of the season the patch bears, 0-1 (Morel: a narrow early-to-mid Spring window).")]
    [Range(0f, 1f)] public float windowStart = 0f;
    [Range(0f, 1f)] public float windowEnd = 1f;

    [Header("Harvest")]
    [Min(1)] public int yield = 3;
    public ForageRegrowth regrowth = ForageRegrowth.Annual;
    [Tooltip("For Days regrowth: in-game days before a picked patch bears again.")]
    [Min(1)] public int regrowDays = 4;

    public string SeasonLabel => windowStart > 0.01f || windowEnd < 0.99f
        ? (windowEnd <= 0.7f ? $"Early {season}" : $"Late {season}")
        : season.ToString();

    public bool InWindow(TimeManager time) =>
        time != null && time.CurrentSeason == season &&
        time.SeasonProgress >= windowStart && time.SeasonProgress <= windowEnd;
}
