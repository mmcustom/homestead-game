using UnityEngine;

// An item lying in the world that the player can pick up (First_Person_Controller.md: "pickup").
// Whatever doesn't fit under the carry limit stays behind. Needs a non-trigger collider to be looked at.
// Note: picked-up world objects aren't tracked by the save system yet, so they reappear when World reloads.
public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] ItemDefinition item;
    [SerializeField, Min(1)] int quantity = 1;

    public ItemDefinition Item => item;
    public int Quantity => quantity;

    public string InteractionPrompt =>
        item == null ? "" : quantity > 1 ? $"Pick up {item.DisplayName} ({quantity})" : $"Pick up {item.DisplayName}";

    public bool CanInteract(PlayerController player) =>
        item != null && InventoryManager.Instance != null && InventoryManager.Instance.Player.SpaceFor(item) > 0;

    public void Interact(PlayerController player)
    {
        if (!CanInteract(player))
            return;

        quantity -= InventoryManager.Instance.AddToPlayer(item.Id, quantity);
        if (quantity <= 0)
            Destroy(gameObject);
    }

    // For pickups spawned in code.
    public void Set(ItemDefinition newItem, int newQuantity)
    {
        item = newItem;
        quantity = Mathf.Max(1, newQuantity);
    }
}
