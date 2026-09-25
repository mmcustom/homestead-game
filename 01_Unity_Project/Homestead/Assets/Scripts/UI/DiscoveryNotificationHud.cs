using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Discovery_System.md's Discovery Process, steps 1-3: a "Discovery Unlocked" popup naming the site and its headline
// fact, followed by a "Journal Updated · Map Updated" confirmation (the minimap marker appears at the same moment,
// via MinimapHud). Step 4, the World Map, waits for that system.
// Discoveries made in quick succession queue up and show one at a time. Runs on scaled time, so it holds while paused.
public class DiscoveryNotificationHud : MonoBehaviour
{
    [SerializeField] CanvasGroup popup;
    [SerializeField] Text detailLabel;
    [SerializeField] CanvasGroup journalConfirmation;

    [Header("Timing (seconds)")]
    [SerializeField, Min(0f)] float fadeSeconds = 0.4f;
    [SerializeField, Min(0f)] float popupHoldSeconds = 3.5f;
    [SerializeField, Min(0f)] float journalHoldSeconds = 2.5f;

    readonly Queue<DiscoveryRecord> pending = new Queue<DiscoveryRecord>();
    Coroutine showing;
    bool subscribed;

    void OnEnable()
    {
        popup.alpha = 0f;
        journalConfirmation.alpha = 0f;

        if (DiscoveryManager.Instance != null)
        {
            DiscoveryManager.Instance.Discovered += OnDiscovered;
            subscribed = true;
        }
    }

    void OnDisable()
    {
        if (subscribed && DiscoveryManager.Instance != null)
            DiscoveryManager.Instance.Discovered -= OnDiscovered;
        subscribed = false;

        pending.Clear();
        showing = null;
    }

    void OnDiscovered(DiscoveryRecord record)
    {
        pending.Enqueue(record);
        if (showing == null)
            showing = StartCoroutine(ShowQueue());
    }

    IEnumerator ShowQueue()
    {
        while (pending.Count > 0)
        {
            DiscoveryRecord record = pending.Dequeue();
            detailLabel.text = DiscoveryCategoryNames.Summary(record);

            yield return Fade(popup, 1f);
            yield return new WaitForSeconds(popupHoldSeconds);

            // Only confirm the journal if it actually recorded this discovery.
            if (JournalHasEntryFor(record))
            {
                journalConfirmation.alpha = 0f;
                // Audio_System.md: sfx_journal_updated pairs with this line.
                if (AudioManager.Instance != null)
                    AudioManager.Instance.Play(SoundCue.JournalUpdated);
                yield return CrossFade(popup, journalConfirmation);
                yield return new WaitForSeconds(journalHoldSeconds);
                yield return Fade(journalConfirmation, 0f);
            }
            else
            {
                yield return Fade(popup, 0f);
            }
        }

        showing = null;
    }

    static bool JournalHasEntryFor(DiscoveryRecord record)
    {
        if (JournalManager.Instance == null)
            return false;

        foreach (JournalEntry entry in JournalManager.Instance.ForSite(record.siteId))
        {
            if (entry.type == JournalEntryType.Discovery)
                return true;
        }
        return false;
    }

    IEnumerator Fade(CanvasGroup group, float target)
    {
        float start = group.alpha;
        for (float t = 0f; t < fadeSeconds; t += Time.deltaTime)
        {
            group.alpha = Mathf.Lerp(start, target, t / fadeSeconds);
            yield return null;
        }
        group.alpha = target;
    }

    IEnumerator CrossFade(CanvasGroup from, CanvasGroup to)
    {
        for (float t = 0f; t < fadeSeconds; t += Time.deltaTime)
        {
            float k = t / fadeSeconds;
            from.alpha = 1f - k;
            to.alpha = k;
            yield return null;
        }
        from.alpha = 0f;
        to.alpha = 1f;
    }
}
