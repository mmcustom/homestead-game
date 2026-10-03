using UnityEngine;

// An item lying in the world that the player can pick up (First_Person_Controller.md: "pickup").
// Whatever doesn't fit under the carry limit stays behind. Needs a non-trigger collider to be looked at.
// Note: picked-up world objects aren't tracked by the save system yet, so they reappear when World reloads.
// The exception is items the player dropped from the Inventory screen (Inventory_System.md's Dropping Items): those
// are Dropped, kept by DroppedItems across saves, and keep the day they were acquired so perishables keep ageing.
public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] ItemDefinition item;
    [SerializeField, Min(1)] int quantity = 1;

    // -1 = acquired today, as for any pickup placed in the scene.
    int acquiredDay = -1;

    public ItemDefinition Item => item;
    public int Quantity => quantity;
    public int AcquiredDay => acquiredDay;
    public bool Dropped { get; private set; }

    public string InteractionPrompt =>
        item == null ? "" : quantity > 1 ? $"Pick up {item.DisplayName} ({quantity})" : $"Pick up {item.DisplayName}";

    public bool CanInteract(PlayerController player) =>
        item != null && InventoryManager.Instance != null && InventoryManager.Instance.Player.SpaceFor(item) > 0;

    public void Interact(PlayerController player)
    {
        if (!CanInteract(player))
            return;

        int added = acquiredDay >= 0
            ? InventoryManager.Instance.AddToPlayer(item.Id, quantity, acquiredDay)
            : InventoryManager.Instance.AddToPlayer(item.Id, quantity);
        quantity -= added;
        if (quantity <= 0)
            Destroy(gameObject);
    }

    // For pickups spawned in code.
    public void Set(ItemDefinition newItem, int newQuantity)
    {
        item = newItem;
        quantity = Mathf.Max(1, newQuantity);
    }

    // A pickup the player dropped, from a batch acquired on the given day.
    public void SetDropped(ItemDefinition newItem, int newQuantity, int day)
    {
        Set(newItem, newQuantity);
        acquiredDay = day;
        Dropped = true;
    }

    // More of the same dropped batch landing on this pile.
    public void AddQuantity(int extra) => quantity += Mathf.Max(0, extra);
}
