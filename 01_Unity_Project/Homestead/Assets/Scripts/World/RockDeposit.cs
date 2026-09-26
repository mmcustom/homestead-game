using UnityEngine;

// South Ridge's rock outcrop (Stone_Gathering_System.md, Property_Layout.md): exposed rock rather than loose fieldstone,
// so it only gives up Stone to the Stone Pick Axe — swung like the Axe (AxeTool), a Stone every few blows until it's
// worked out (StoneManager). Looking at it without the Pick Axe says what it takes.
public class RockDeposit : MonoBehaviour, IInteractable
{
    public string InteractionPrompt
    {
        get
        {
            InventoryManager inventory = InventoryManager.Instance;
            bool pick = inventory != null && inventory.EquippedTool != null && inventory.EquippedTool.Id == AxeTool.PickAxeId;
            if (pick)
                return ""; // the Pick Axe's own HUD line takes over
            StoneManager stone = StoneManager.Instance;
            if (stone != null && stone.DepositRemaining <= 0)
                return "Rock Outcrop  — worked out";
            return "Rock Outcrop  — mine it with a Stone Pick Axe";
        }
    }

    public bool CanInteract(PlayerController player) => InteractionPrompt.Length > 0;

    public void Interact(PlayerController player) { }
}
