using UnityEngine;
using UnityEngine.UI;

// Shows ToolStatus on the HUD: a small crosshair while an aimed tool is equipped, the tool's status line and
// progress bar under the centre of the screen, and brief messages (a catch, a miss) above them.
public class ToolHud : MonoBehaviour
{
    [SerializeField] Graphic crosshair;
    [SerializeField] Text statusLabel;
    [SerializeField] RectTransform bar;
    [SerializeField] RectTransform barFill;
    [SerializeField] CanvasGroup flashGroup;
    [SerializeField] Text flashLabel;

    void LateUpdate()
    {
        bool playing = GameManager.Instance == null || GameManager.Instance.State == GameState.Playing;

        crosshair.enabled = playing && ToolStatus.Crosshair;

        string line = playing ? ToolStatus.Line : null;
        statusLabel.enabled = !string.IsNullOrEmpty(line);
        if (statusLabel.enabled)
            statusLabel.text = line;

        float progress = playing ? ToolStatus.Progress : -1f;
        bar.gameObject.SetActive(progress >= 0f);
        if (progress >= 0f)
            barFill.anchorMax = new Vector2(Mathf.Clamp01(progress), 1f);

        string flash = ToolStatus.FlashMessage;
        flashGroup.alpha = Mathf.MoveTowards(flashGroup.alpha, string.IsNullOrEmpty(flash) ? 0f : 1f, Time.unscaledDeltaTime * 4f);
        if (!string.IsNullOrEmpty(flash))
            flashLabel.text = flash;
    }
}
