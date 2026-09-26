using UnityEngine;

// Inventory_System.md's item categories.
public enum ItemCategory { Resource, Tool, Consumable, Material }

// Static data for one kind of item. Create via Assets > Create > Homestead > Item and place under
// Assets/Resources/Items so ItemDatabase can find it. Per Inventory_System.md, this holds what Inventory needs to
// store and carry the item, plus its food effect (the Eating entry, 2026-09-25): what eating or drinking one restores
// and its chance of making the player sick — raw meat and fish, and water below Excellent. Anything with a food
// effect can be eaten or drunk from the Inventory screen.
[CreateAssetMenu(menuName = "Homestead/Item", fileName = "NewItem")]
public class ItemDefinition : ScriptableObject
{
    [Tooltip("Stable id written to save files. Don't change it once saves exist.")]
    [SerializeField] string id;
    [SerializeField] string displayName;
    [SerializeField] ItemCategory category;
    [SerializeField, Min(0f)] float weightKg = 0.5f;

    [Tooltip("0 = doesn't spoil. Stacks of perishable items keep the day they were acquired so the Spoilage System " +
             "(Core_Survival_System.md) can age them; the spoilage rules themselves live there, not in Inventory.")]
    [SerializeField, Min(0f)] float shelfLifeDays;

    [Header("Food (0 = not food)")]
    [Tooltip("Hunger restored by eating one.")]
    [SerializeField, Min(0f)] float hungerRestored;
    [Tooltip("Hydration restored by eating or drinking one.")]
    [SerializeField, Min(0f)] float hydrationRestored;
    [Tooltip("Chance (0-1) that eating or drinking one makes the player sick — raw meat and fish, unpurified water.")]
    [SerializeField, Range(0f, 1f)] float illnessChance;

    public string Id => id;
    public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
    public ItemCategory Category => category;
    public float WeightKg => weightKg;
    public float ShelfLifeDays => shelfLifeDays;
    public bool IsPerishable => shelfLifeDays > 0f;
    public float HungerRestored => hungerRestored;
    public float HydrationRestored => hydrationRestored;
    public float IllnessChance => illnessChance;
    public bool IsFood => hungerRestored > 0f || hydrationRestored > 0f;
    // Mostly water: drunk rather than eaten (sound and wording).
    public bool IsDrink => hydrationRestored > 0f && hungerRestored <= 0f;

    // For items built in code (tests, tools).
    public static ItemDefinition Create(string id, ItemCategory category, float weightKg, float shelfLifeDays = 0f, string displayName = null,
                                        float hunger = 0f, float hydration = 0f, float illness = 0f)
    {
        var item = CreateInstance<ItemDefinition>();
        item.id = id;
        item.name = id;
        item.displayName = displayName;
        item.category = category;
        item.weightKg = weightKg;
        item.shelfLifeDays = shelfLifeDays;
        item.hungerRestored = hunger;
        item.hydrationRestored = hydration;
        item.illnessChance = illness;
        return item;
    }

    void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
            id = name;
    }
}
