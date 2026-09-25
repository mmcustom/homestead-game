using UnityEngine;

// A forage patch in the world (Foraging_System.md): a berry bush, fruit or nut tree, patch of greens or cattails, or a
// mushroom spot. Looking at it tells the player what it is and when it bears — "Knowledge Is Power" — and in season
// hold E harvests what fits in the pack. The patch is a DiscoverySite found by harvesting it for the first time
// (Foraging_System.md: resources must be discovered), with its own map icon. What's been picked is kept by
// ForagingManager; ripe fruit, nuts, flowers or caps are only shown while there's something to pick.
[RequireComponent(typeof(DiscoverySite))]
public class ForageNode : MonoBehaviour, IInteractable
{
    [SerializeField] ForageSpecies species;
    [Tooltip("The ripe fruit, nuts, flowers or caps — shown only while there's something to pick (ForagePlacer builds it).")]
    [SerializeField] GameObject crop;

    DiscoverySite site;
    float nextRefresh;
    bool cropShown = true;

    public ForageSpecies Species => species;

    // Set up by ForagePlacer.
    public void Configure(ForageSpecies forageSpecies, GameObject ripeCrop)
    {
        species = forageSpecies;
        crop = ripeCrop;
    }

    string NodeId => site != null ? site.SiteId : name;

    void Awake()
    {
        site = GetComponent<DiscoverySite>();
    }

    void Update()
    {
        if (UnityEngine.Time.unscaledTime < nextRefresh)
            return;
        nextRefresh = UnityEngine.Time.unscaledTime + 1f;
        RefreshCrop();
    }

    void RefreshCrop()
    {
        bool show = Available > 0;
        if (crop != null && show != cropShown)
            crop.SetActive(show);
        cropShown = show;
    }

    int Available => ForagingManager.Instance != null ? ForagingManager.Instance.Available(NodeId, species) : 0;

    string ItemName
    {
        get
        {
            ItemDefinition item = species != null ? ItemDatabase.Get(species.itemId) : null;
            return item != null ? item.DisplayName : species != null ? species.itemId : "";
        }
    }

    public string InteractionPrompt
    {
        get
        {
            if (species == null || ForagingManager.Instance == null)
                return "";

            int available = Available;
            if (available > 0)
            {
                ItemDefinition item = ItemDatabase.Get(species.itemId);
                bool room = item != null && InventoryManager.Instance != null && InventoryManager.Instance.Player.SpaceFor(item) > 0;
                return room ? $"Harvest {ItemName}  ({available})" : $"Harvest {ItemName}  — no room to carry more";
            }

            if (ForagingManager.Instance.IsPickedClean(NodeId, species))
                return species.regrowth == ForageRegrowth.Days
                    ? $"{species.patchName}  — picked, grows back in a few days"
                    : $"{species.patchName}  — picked, bears again next {species.season}";

            return $"{species.patchName}  — bears in {species.SeasonLabel}";
        }
    }

    // Always true, so the patch can be identified and its season learned even when there's nothing to pick.
    public bool CanInteract(PlayerController player) => species != null && ForagingManager.Instance != null;

    public void Interact(PlayerController player)
    {
        ForagingManager foraging = ForagingManager.Instance;
        InventoryManager inventory = InventoryManager.Instance;
        ItemDefinition item = species != null ? ItemDatabase.Get(species.itemId) : null;
        if (foraging == null || inventory == null || item == null)
            return;

        int fits = Mathf.Min(Available, inventory.Player.SpaceFor(item));
        int taken = foraging.Take(NodeId, species, fits);
        if (taken <= 0)
            return;

        inventory.AddToPlayer(species.itemId, taken);
        if (site != null)
            site.Discover(); // first harvest finds the patch: notification, journal and map marker
        RefreshCrop();
    }
}
