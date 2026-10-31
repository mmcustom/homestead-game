using UnityEngine;

// The Hand Cart's handles (Vehicles_and_Hauling.md, Phase 2). A collider of its own, a little behind the bed (WoodPile.AddCartHandle), so the look ray
// resolves to this instead of the pile when the player looks at the handles: E grabs the cart (CartHandler). Looking at the bed still opens its load.
// Nothing here is saved: whether the cart is held is CartHandler's, and a held cart is saved only as the pile it always was.
public class CartHandle : MonoBehaviour, IInteractable
{
    WoodPile cart;

    public void Bind(WoodPile pile) => cart = pile;

    public string InteractionPrompt
    {
        get
        {
            WoodPileState state = cart != null ? cart.State : null;
            return state == null ? "" : CartRules.GrabPrompt(WoodPile.CartLoadKg(state));
        }
    }

    // Not while another cart is held; the handles of the held one are switched off anyway.
    public bool CanInteract(PlayerController player) =>
        cart != null && cart.State != null && cart.State.kind == PileKind.HandCart && !CartHandler.Pushing;

    public void Interact(PlayerController player)
    {
        if (cart == null || CartHandler.Instance == null)
            return;
        CartHandler.Instance.TryGrab(cart, transform.position);
    }
}
