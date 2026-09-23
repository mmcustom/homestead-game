using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DiscoverySaveData
{
    public List<DiscoveryRecord> records;
    public List<DiscoveryMilestone> milestones;
}

// Discoveries, knowledge tracking, map markers (Unity_Architecture.md, Discovery_System.md).
// "The map should be learned, not revealed": nothing is known until a DiscoverySite is found. The discovered
// records double as the map marker list — each has a position and category for the minimap/world map to draw.
// Journal entries and the discovery notification come from JournalManager and UI listening to Discovered.
public class DiscoveryManager : MonoBehaviour, ISaveable
{
    public static DiscoveryManager Instance { get; private set; }

    readonly List<DiscoveryRecord> records = new List<DiscoveryRecord>();
    readonly Dictionary<string, DiscoveryRecord> recordsById = new Dictionary<string, DiscoveryRecord>();
    readonly List<DiscoveryMilestone> milestones = new List<DiscoveryMilestone>();

    public event Action<DiscoveryRecord> Discovered;
    public event Action<DiscoveryRecord, DiscoveryObservation> ObservationAdded;
    public event Action<DiscoveryMilestone> MilestoneReached;

    // In discovery order.
    public IReadOnlyList<DiscoveryRecord> Records => records;
    public IReadOnlyList<DiscoveryMilestone> Milestones => milestones;

    static int Today => TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0;

    static bool InGame =>
        GameManager.Instance == null ||
        GameManager.Instance.State == GameState.Playing || GameManager.Instance.State == GameState.Paused;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (Instance == this && SaveManager.Instance != null)
            SaveManager.Instance.Register(this);
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;

        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
        Instance = null;
    }

    public bool IsDiscovered(string siteId) => siteId != null && recordsById.ContainsKey(siteId);

    public DiscoveryRecord Get(string siteId) =>
        siteId != null && recordsById.TryGetValue(siteId, out DiscoveryRecord record) ? record : null;

    public IEnumerable<DiscoveryRecord> InCategory(DiscoveryCategory category)
    {
        foreach (DiscoveryRecord record in records)
        {
            if (record.category == category)
                yield return record;
        }
    }

    public bool HasMilestone(string milestoneId) => milestones.Exists(m => m.id == milestoneId);

    public bool Discover(DiscoverySite site) =>
        Discover(site.SiteId, site.DisplayName, site.Category, site.transform.position, site.Facts);

    // Records a new discovery. Returns false if it was already known, or if not currently in gameplay
    // (e.g. a trigger firing while the World scene loads).
    public bool Discover(string siteId, string displayName, DiscoveryCategory category, Vector3 position,
                         IEnumerable<DiscoveryFact> facts = null)
    {
        if (string.IsNullOrEmpty(siteId) || IsDiscovered(siteId) || !InGame)
            return false;

        var record = new DiscoveryRecord
        {
            siteId = siteId,
            displayName = displayName,
            category = category,
            position = position,
            dayDiscovered = Today,
        };

        if (facts != null)
        {
            foreach (DiscoveryFact fact in facts)
                record.facts.Add(new DiscoveryFact { label = fact.label, value = fact.value });
        }

        records.Add(record);
        recordsById.Add(siteId, record);
        Discovered?.Invoke(record);

        RecordMilestone("first_" + category, $"First {DiscoveryCategoryNames.Of(category)} found: {displayName}");
        return true;
    }

    // Adds something learned about an already-discovered site (Knowledge Progression: the player should know
    // more about the property each year). Repeating an existing observation is ignored.
    public bool AddObservation(string siteId, string text)
    {
        DiscoveryRecord record = Get(siteId);
        if (record == null || string.IsNullOrEmpty(text) || record.observations.Exists(o => o.text == text))
            return false;

        var observation = new DiscoveryObservation { day = Today, text = text };
        record.observations.Add(observation);
        ObservationAdded?.Invoke(record, observation);
        return true;
    }

    // Records a permanent "first", e.g. "first_deer_harvest" from the Hunting system. Only the first call counts.
    public bool RecordMilestone(string milestoneId, string text)
    {
        if (string.IsNullOrEmpty(milestoneId) || HasMilestone(milestoneId))
            return false;

        var milestone = new DiscoveryMilestone { id = milestoneId, text = text, day = Today };
        milestones.Add(milestone);
        MilestoneReached?.Invoke(milestone);
        return true;
    }

    // Forgets everything — used when starting a new game.
    public void ResetDiscoveries()
    {
        records.Clear();
        recordsById.Clear();
        milestones.Clear();
    }

    public DiscoverySaveData CaptureState() => new DiscoverySaveData
    {
        records = new List<DiscoveryRecord>(records),
        milestones = new List<DiscoveryMilestone>(milestones),
    };

    // Restores silently — no events fire, so listeners should re-read state after a load.
    public void RestoreState(DiscoverySaveData data)
    {
        ResetDiscoveries();

        if (data.records != null)
        {
            foreach (DiscoveryRecord record in data.records)
            {
                if (record == null || string.IsNullOrEmpty(record.siteId) || recordsById.ContainsKey(record.siteId))
                    continue;

                records.Add(record);
                recordsById.Add(record.siteId, record);
            }
        }

        if (data.milestones != null)
            milestones.AddRange(data.milestones);
    }

    // Save_Data_Model.md's World Block: discovered locations and minimap reveal state.
    string ISaveable.SaveFile => "discovery";
    string ISaveable.SaveKey => "discovery";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<DiscoverySaveData>(json));
}
