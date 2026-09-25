using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Shows the context-sensitive prompt for whatever the player is looking at (First_Person_Controller.md), e.g.
// "[E] Drink" or "[E] Pick up Stone", just below the centre of the screen. An object's second action
// (ISecondaryInteractable) shows as a second line, e.g. "[R]  Put Out Fire".
public class InteractionPromptHud : MonoBehaviour
{
    [SerializeField] CanvasGroup prompt;
    [SerializeField] Text label;

    PlayerController player;
    InputAction interactAction, interactAltAction;
    float nextPlayerSearch;

    void Update()
    {
        if (player == null && Time.unscaledTime >= nextPlayerSearch)
        {
            nextPlayerSearch = Time.unscaledTime + 1f;
            player = FindAnyObjectByType<PlayerController>();
            interactAction ??= InputSystem.actions != null ? InputSystem.actions.FindAction("Player/Interact") : null;
            interactAltAction ??= InputSystem.actions != null ? InputSystem.actions.FindAction("Player/InteractAlt") : null;
        }

        bool playing = GameManager.Instance == null || GameManager.Instance.State == GameState.Playing;
        IInteractable focus = player != null && playing ? player.Focus : null;
        string text = focus != null ? focus.InteractionPrompt : null;

        if (string.IsNullOrEmpty(text))
        {
            prompt.alpha = 0f;
            return;
        }

        label.text = WithKey(interactAction, text);
        string second = focus is ISecondaryInteractable secondary ? secondary.SecondaryPrompt : null;
        if (!string.IsNullOrEmpty(second))
            label.text += "\n" + WithKey(interactAltAction, second);
        prompt.alpha = 1f;

        // Size the backing panel to the prompt, since item names vary a lot in length.
        var panel = (RectTransform)prompt.transform;
        panel.sizeDelta = new Vector2(label.preferredWidth + 48f, label.preferredHeight + 16f);
    }

    static string WithKey(InputAction action, string text)
    {
        string key = action != null ? action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse")) : "";
        return string.IsNullOrEmpty(key) ? text : $"[{key}]  {text}";
    }
}
