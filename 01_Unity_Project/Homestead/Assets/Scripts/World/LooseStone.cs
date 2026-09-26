using UnityEngine;

// A loose rock lying on the ground (Stone_Gathering_System.md): picked up by hand for one Stone, no tool needed.
// Placed by Homestead > Place Stone with a stable id, so StoneManager can keep it gone once it's been taken.
public class LooseStone : MonoBehaviour, IInteractable
{
    [SerializeField] int id;

    public int Id => id;

    public void SetId(int value) => id = value;

    static ItemDefinition Stone => ItemDatabase.Get(WoodManager.StoneId);

    public string InteractionPrompt
    {
        get
        {
            InventoryManager inventory = InventoryManager.Instance;
            bool room = inventory != null && Stone != null && inventory.Player.SpaceFor(Stone) > 0;
            return room ? "Pick up Stone" : "Stone  — too heavy to carry more";
        }
    }

    public bool CanInteract(PlayerController player) => InventoryManager.Instance != null && Stone != null;

    public void Interact(PlayerController player)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null || inventory.AddToPlayer(WoodManager.StoneId, 1) < 1)
            return;
        if (StoneManager.Instance != null)
            StoneManager.Instance.MarkGathered(this);
        else
            gameObject.SetActive(false);
    }
}
