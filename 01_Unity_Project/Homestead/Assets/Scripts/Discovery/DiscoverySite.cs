using System.Collections.Generic;
using UnityEngine;

// Marks a discoverable place in the world — a spring, berry patch, deer trail, fishing hole or cabin site.
// Discovery_System.md: "Players must physically locate resources", so the site is discovered when an object
// tagged Player enters its trigger collider. Other systems can also call Discover() directly (e.g. spotting deer).
public class DiscoverySite : MonoBehaviour
{
    const string PlayerTag = "Player";

    static readonly Dictionary<string, DiscoverySite> activeSites = new Dictionary<string, DiscoverySite>();

    [Tooltip("Stable id written to save files. Must be unique across the world and never change once saves exist.")]
    [SerializeField] string siteId;
    [SerializeField] string displayName;
    [SerializeField] DiscoveryCategory category;

    [Tooltip("What discovering this site teaches the player, e.g. Water Quality = Excellent.")]
    [SerializeField] List<DiscoveryFact> facts = new List<DiscoveryFact>();

    public string SiteId => siteId;
    public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
    public DiscoveryCategory Category => category;
    public IReadOnlyList<DiscoveryFact> Facts => facts;

    public bool IsDiscovered => DiscoveryManager.Instance != null && DiscoveryManager.Instance.IsDiscovered(siteId);

    void OnEnable()
    {
        if (string.IsNullOrEmpty(siteId))
        {
            Debug.LogError($"[DiscoverySite] '{name}' has no site id and can't be discovered.", this);
            return;
        }

        if (activeSites.TryGetValue(siteId, out DiscoverySite other) && other != this)
            Debug.LogError($"[DiscoverySite] '{name}' and '{other.name}' share site id '{siteId}'.", this);
        else
            activeSites[siteId] = this;
    }

    void OnDisable()
    {
        if (siteId != null && activeSites.TryGetValue(siteId, out DiscoverySite current) && current == this)
            activeSites.Remove(siteId);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(PlayerTag))
            Discover();
    }

    // Returns true if this was a new discovery.
    public bool Discover() =>
        !string.IsNullOrEmpty(siteId) && DiscoveryManager.Instance != null && DiscoveryManager.Instance.Discover(this);

    // New sites get a unique id and a trigger to walk into.
    void Reset()
    {
        siteId = System.Guid.NewGuid().ToString("N");

        if (GetComponent<Collider>() == null)
        {
            var trigger = gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 5f;
        }
    }

    void OnValidate()
    {
        var trigger = GetComponent<Collider>();
        if (trigger != null && !trigger.isTrigger)
            Debug.LogWarning($"[DiscoverySite] '{name}' collider isn't a trigger, so walking into it won't discover the site.", this);
    }

    // Domain reload may be disabled in Play Mode settings, so clear the registry explicitly.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetRegistry() => activeSites.Clear();

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
