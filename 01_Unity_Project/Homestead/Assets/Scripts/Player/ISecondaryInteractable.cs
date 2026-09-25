// An optional second action on an IInteractable, on its own key (Player/InteractAlt, R by default), shown as a second
// line under the main prompt — e.g. a campfire's "Put Out Fire" alongside "Add Firewood". Only offered while the
// object is the player's focus, so the object's CanInteract still decides whether it's looked at at all.
public interface ISecondaryInteractable
{
    // e.g. "Put Out Fire". Empty hides the second line.
    string SecondaryPrompt { get; }

    void SecondaryInteract(PlayerController player);
}
