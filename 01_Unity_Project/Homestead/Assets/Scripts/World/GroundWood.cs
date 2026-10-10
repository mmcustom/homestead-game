using UnityEngine;

// Wood lying loose on the ground (Wood_Gathering_System.md's Ground Sticks and Fallen Branches): a bundle of Sticks or a
// single Branch, picked up by hand with no tool, so a Pioneer with no Axe can still start a fire and craft the Primitive
// Axe. Placed by Homestead > Place Sticks and Branches with a stable id; GroundWoodManager brings a taken one back after
// a few in-game days.
public class GroundWood : MonoBehaviour, IInteractable
{
    public enum Kind { Sticks, Branch }

    public const int SticksPerBundle = 3;
    public const int BranchesPerPiece = 1;

    [SerializeField] int id;
    [SerializeField] Kind kind;

    public int Id => id;
    public Kind WoodKind => kind;

    public void Setup(int value, Kind woodKind)
    {
        id = value;
        kind = woodKind;
    }

    string ItemId => kind == Kind.Sticks ? WoodManager.SticksId : WoodManager.BranchesId;
    int Yield => kind == Kind.Sticks ? SticksPerBundle : BranchesPerPiece;

    public string InteractionPrompt
    {
        get
        {
            InventoryManager inventory = InventoryManager.Instance;
            ItemDefinition item = ItemDatabase.Get(ItemId);
            bool room = inventory != null && item != null && inventory.Player.SpaceFor(item) > 0;
            string name = kind == Kind.Sticks ? "Sticks" : "Fallen branch";
            if (room)
                return kind == Kind.Sticks ? "Pick up Sticks" : "Pick up Branch";
            return $"{name}  — no room to carry more";
        }
    }

    public bool CanInteract(PlayerController player) => InventoryManager.Instance != null && ItemDatabase.Get(ItemId) != null;

    public void Interact(PlayerController player)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
            return;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayOnNextAdd(SoundCue.Forage);
        if (inventory.AddToPlayer(ItemId, Yield) < 1)
            return;
        if (GroundWoodManager.Instance != null)
            GroundWoodManager.Instance.MarkTaken(this);
        else
            gameObject.SetActive(false);
    }
}
