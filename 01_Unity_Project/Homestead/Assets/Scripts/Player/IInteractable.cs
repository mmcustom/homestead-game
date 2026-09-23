// Something the player can interact with through the context-sensitive prompt (First_Person_Controller.md):
// Harvest, Check Trap, Draw Water, Feed Animal, Open. Implement on a MonoBehaviour with a non-trigger collider
// on the same object or a child. The controller decides how the player interacts; the implementing system
// decides what the interaction produces.
public interface IInteractable
{
    // Shown to the player while looking at it, e.g. "Harvest Blackberries" or "Open Root Cellar".
    string InteractionPrompt { get; }

    // False hides the prompt, e.g. a patch with nothing left to harvest.
    bool CanInteract(PlayerController player);

    void Interact(PlayerController player);
}
