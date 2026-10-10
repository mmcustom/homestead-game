using UnityEngine;

// A patch of wet clay on a creek or pond bank (Building_Housing_System.md's Clay). Placed by Homestead > Place Clay
// Banks as a marker, because Mike couldn't find any Clay by looking for a bank (2026-10-09). The Primitive Shovel digs
// Clay on a patch or anywhere else on the bank within reach of the water (AxeTool.IsCreekBank); the patch only shows
// where to dig. It never runs out, so it keeps no state and has nothing to save.
public class ClayBank : MonoBehaviour, IInteractable
{
    [SerializeField] int id;

    public int Id => id;

    public void Setup(int value) => id = value;

    public string InteractionPrompt
    {
        get
        {
            InventoryManager inventory = InventoryManager.Instance;
            if (inventory == null)
                return "";
            if (inventory.EquippedTool != null && inventory.EquippedTool.Id == AxeTool.ShovelId)
                return ""; // the Shovel's own HUD line takes over
            if (inventory.Player.Has(AxeTool.ShovelId))
                return "Clay bank - equip a Shovel to dig";
            return $"Clay bank - a Primitive Shovel digs this ({ShovelCost})";
        }
    }

    public bool CanInteract(PlayerController player) => InteractionPrompt.Length > 0;

    public void Interact(PlayerController player) { }

    // "3 Sticks, 1 Cordage, 1 Stone", straight from the recipe so the two can't drift apart.
    static string ShovelCost
    {
        get
        {
            foreach (Crafting.Recipe recipe in Crafting.Recipes)
                if (recipe.outputId == AxeTool.ShovelId)
                    return Crafting.Cost(recipe);
            return "Sticks, Cordage, Stone";
        }
    }
}
