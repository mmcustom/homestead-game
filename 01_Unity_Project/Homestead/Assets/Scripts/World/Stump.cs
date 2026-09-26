using UnityEngine;

// What a felled hardwood leaves (WoodManager spawns them). The Primitive Shovel digs one out (AxeTool); looking at it
// without the Shovel says what it would take.
public class Stump : MonoBehaviour, IInteractable
{
    public int TreeIndex { get; set; }

    public string InteractionPrompt =>
        InventoryManager.Instance != null && InventoryManager.Instance.EquippedTool != null &&
        InventoryManager.Instance.EquippedTool.Id == AxeTool.ShovelId
            ? "" // the Shovel's own HUD line takes over
            : "Stump  — dig it out with a Shovel";

    public bool CanInteract(PlayerController player) => InteractionPrompt.Length > 0;

    public void Interact(PlayerController player) { }
}
