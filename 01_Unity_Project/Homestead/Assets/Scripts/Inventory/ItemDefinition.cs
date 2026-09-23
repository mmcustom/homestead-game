using UnityEngine;

// Inventory_System.md's item categories.
public enum ItemCategory { Resource, Tool, Consumable, Material }

// Static data for one kind of item. Create via Assets > Create > Homestead > Item and place under
// Assets/Resources/Items so ItemDatabase can find it. Per Inventory_System.md, this holds only what
// Inventory needs to store and carry the item — what the item does belongs to the system that uses it.
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

    public string Id => id;
    public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
    public ItemCategory Category => category;
    public float WeightKg => weightKg;
    public float ShelfLifeDays => shelfLifeDays;
    public bool IsPerishable => shelfLifeDays > 0f;

    // For items built in code (tests, tools).
    public static ItemDefinition Create(string id, ItemCategory category, float weightKg, float shelfLifeDays = 0f, string displayName = null)
    {
        var item = CreateInstance<ItemDefinition>();
        item.id = id;
        item.name = id;
        item.displayName = displayName;
        item.category = category;
        item.weightKg = weightKg;
        item.shelfLifeDays = shelfLifeDays;
        return item;
    }

    void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
            id = name;
    }
}
