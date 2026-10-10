using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Guarantees exactly one active AudioListener, always. Bootstrap and Loading have none (only MainMenu.unity and the
// Main Camera prefab carry one, and the player's camera gets its from that prefab once the World has spawned the
// Player), so Unity warned "There are no audio listeners in the scene" whenever one of those scenes was all there was.
// This object lives for the whole session with a listener of its own, which it keeps switched on only while no
// scene camera has one; when a scene's listener exists it takes over, preferring the one on the main camera, and any
// further listener is switched off (Unity warns about two as well). AudioListener.pause and AudioManager's use of
// Camera.main are unaffected.
public class AudioListenerGuard : MonoBehaviour
{
    const float FullCheckSeconds = 0.5f;

    AudioListener own;
    AudioListener active;
    float nextFullCheck;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Create()
    {
        if (FindAnyObjectByType<AudioListenerGuard>() != null)
            return;

        var go = new GameObject("Audio Listener Guard");
        DontDestroyOnLoad(go);
        go.AddComponent<AudioListenerGuard>();
    }

    void Awake()
    {
        own = gameObject.AddComponent<AudioListener>();
        active = own;
        SceneManager.sceneLoaded += OnSceneChanged;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneChanged;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void OnSceneChanged(Scene scene, LoadSceneMode mode) => Evaluate();

    void OnSceneUnloaded(Scene scene) => nextFullCheck = 0f;

    // Every frame, a cheap look at the listener in use; every half second, or when it goes, a full count.
    void LateUpdate()
    {
        bool inUse = active != null && active.isActiveAndEnabled;
        if (!inUse || Time.unscaledTime >= nextFullCheck)
            Evaluate();
    }

    void Evaluate()
    {
        nextFullCheck = Time.unscaledTime + FullCheckSeconds;

        // Every listener on an active object, including ones this guard switched off earlier.
        var candidates = new List<AudioListener>();
        foreach (AudioListener listener in FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (listener != own && listener.gameObject.activeInHierarchy)
                candidates.Add(listener);

        Camera main = Camera.main;
        AudioListener chosen = own;
        foreach (AudioListener listener in candidates)
        {
            if (main != null && listener.gameObject == main.gameObject)
            {
                chosen = listener;
                break;
            }
            if (chosen == own)
                chosen = listener;
        }

        // Switch the chosen one on first, so there is never a moment with none.
        if (!chosen.enabled)
            chosen.enabled = true;
        foreach (AudioListener listener in candidates)
            if (listener != chosen && listener.enabled)
                listener.enabled = false;
        if (own != chosen && own.enabled)
            own.enabled = false;
        active = chosen;
    }
}
