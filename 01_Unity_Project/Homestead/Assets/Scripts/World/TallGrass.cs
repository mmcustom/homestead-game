using UnityEngine;

// A clump of tall meadow grass (Trapping_System.md's Cordage sourcing): cut by hand for 3 Tall Grass, which twists into
// Cordage. Grows back after a few days (GrassManager). Placed by Homestead > Place Tall Grass with a stable id.
public class TallGrass : MonoBehaviour, IInteractable
{
    public const string ItemId = "tall_grass";
    const int Yield = 3;

    [SerializeField] int id;

    public int Id => id;

    public void SetId(int value) => id = value;

    public string InteractionPrompt
    {
        get
        {
            InventoryManager inventory = InventoryManager.Instance;
            ItemDefinition item = ItemDatabase.Get(ItemId);
            bool room = inventory != null && item != null && inventory.Player.SpaceFor(item) > 0;
            return room ? "Cut Tall Grass" : "Tall Grass  — no room to carry more";
        }
    }

    public bool CanInteract(PlayerController player) => InventoryManager.Instance != null && ItemDatabase.Get(ItemId) != null;

    public void Interact(PlayerController player)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
            return;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayOnNextAdd(SoundCue.Forage); // a rustle, like the other gathering
        if (inventory.AddToPlayer(ItemId, Yield) < 1)
            return;
        if (GrassManager.Instance != null)
            GrassManager.Instance.MarkCut(this);
        else
            gameObject.SetActive(false);
    }
}
