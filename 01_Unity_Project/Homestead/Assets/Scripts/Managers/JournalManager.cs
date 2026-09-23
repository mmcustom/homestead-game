using System;
using System.Collections.Generic;
using UnityEngine;

// Discovery_System.md's journal categories, plus History for its Historical Records and Notes for player notes.
public enum JournalSection { WaterSources, Plants, Wildlife, Fishing, PropertyFeatures, History, Notes }

public enum JournalEntryType { Discovery, Observation, Milestone, PlayerNote }

[Serializable]
public class JournalEntry
{
    public int id;
    public JournalEntryType type;
    public JournalSection section;
    public int day;
    public string title;
    public string body;

    // Links the entry to a discovered site (its location comes from DiscoveryManager). Empty if none.
    public string siteId;
    public bool isRead;

    public bool IsPlayerNote => type == JournalEntryType.PlayerNote;
}

[Serializable]
public struct JournalSaveData
{
    public List<JournalEntry> entries;
    public int nextId;
}

// Journal entries, player notes, discovery records (Unity_Architecture.md).
// Discovery_System.md: every discovery creates a journal entry, and the journal becomes the player's field
// notebook. Entries are written from DiscoveryManager's events as they happen and kept as written; only the
// player's own notes can be edited or deleted, since important records stay permanent.
public class JournalManager : MonoBehaviour, ISaveable
{
    public static JournalManager Instance { get; private set; }

    readonly List<JournalEntry> entries = new List<JournalEntry>();
    int nextId = 1;
    bool subscribed;

    public event Action<JournalEntry> EntryAdded;
    public event Action<JournalEntry> EntryChanged;
    public event Action<JournalEntry> EntryRemoved;

    // Oldest first.
    public IReadOnlyList<JournalEntry> Entries => entries;
    public int UnreadCount => entries.FindAll(e => !e.isRead).Count;

    static int Today => TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0;

    public static string SectionName(JournalSection section)
    {
        switch (section)
        {
            case JournalSection.WaterSources: return "Water Sources";
            case JournalSection.Plants: return "Plants";
            case JournalSection.Wildlife: return "Wildlife";
            case JournalSection.Fishing: return "Fishing";
            case JournalSection.PropertyFeatures: return "Property Features";
            case JournalSection.History: return "History";
            case JournalSection.Notes: return "Notes";
            default: return section.ToString();
        }
    }

    public static JournalSection SectionFor(DiscoveryCategory category)
    {
        switch (category)
        {
            case DiscoveryCategory.WaterSource: return JournalSection.WaterSources;
            case DiscoveryCategory.Plant: return JournalSection.Plants;
            case DiscoveryCategory.Wildlife: return JournalSection.Wildlife;
            case DiscoveryCategory.Fishing: return JournalSection.Fishing;
            default: return JournalSection.PropertyFeatures;
        }
    }

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
        if (Instance != this)
            return;

        DiscoveryManager discovery = DiscoveryManager.Instance;
        if (discovery != null)
        {
            discovery.Discovered += OnDiscovered;
            discovery.ObservationAdded += OnObservationAdded;
            discovery.MilestoneReached += OnMilestoneReached;
            subscribed = true;
        }

        if (SaveManager.Instance != null)
            SaveManager.Instance.Register(this);
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;

        DiscoveryManager discovery = DiscoveryManager.Instance;
        if (subscribed && discovery != null)
        {
            discovery.Discovered -= OnDiscovered;
            discovery.ObservationAdded -= OnObservationAdded;
            discovery.MilestoneReached -= OnMilestoneReached;
        }

        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
        Instance = null;
    }

    public JournalEntry Get(int entryId) => entries.Find(e => e.id == entryId);

    public IEnumerable<JournalEntry> InSection(JournalSection section)
    {
        foreach (JournalEntry entry in entries)
        {
            if (entry.section == section)
                yield return entry;
        }
    }

    public IEnumerable<JournalEntry> ForSite(string siteId)
    {
        foreach (JournalEntry entry in entries)
        {
            if (!string.IsNullOrEmpty(siteId) && entry.siteId == siteId)
                yield return entry;
        }
    }

    // A note written by the player, optionally about a discovered site (filed under that site's section).
    public JournalEntry AddNote(string title, string body, string siteId = null)
    {
        DiscoveryRecord site = DiscoveryManager.Instance != null ? DiscoveryManager.Instance.Get(siteId) : null;
        return AddEntry(JournalEntryType.PlayerNote,
                        site != null ? SectionFor(site.category) : JournalSection.Notes,
                        title, body, site != null ? siteId : null, isRead: true);
    }

    public bool EditNote(int entryId, string title, string body)
    {
        JournalEntry entry = Get(entryId);
        if (entry == null || !entry.IsPlayerNote)
            return false;

        entry.title = title;
        entry.body = body;
        EntryChanged?.Invoke(entry);
        return true;
    }

    public bool DeleteNote(int entryId)
    {
        JournalEntry entry = Get(entryId);
        if (entry == null || !entry.IsPlayerNote)
            return false;

        entries.Remove(entry);
        EntryRemoved?.Invoke(entry);
        return true;
    }

    public void MarkRead(int entryId)
    {
        JournalEntry entry = Get(entryId);
        if (entry == null || entry.isRead)
            return;

        entry.isRead = true;
        EntryChanged?.Invoke(entry);
    }

    public void MarkAllRead()
    {
        foreach (JournalEntry entry in entries)
        {
            if (entry.isRead)
                continue;

            entry.isRead = true;
            EntryChanged?.Invoke(entry);
        }
    }

    // Empties the journal — used when starting a new game.
    public void ResetJournal()
    {
        entries.Clear();
        nextId = 1;
    }

    // e.g. "Natural Spring" / "Water Quality: Excellent" (Water_System.md's example discovery).
    void OnDiscovered(DiscoveryRecord record)
    {
        // "\n" rather than AppendLine so saved text doesn't depend on the platform's line ending.
        var lines = new List<string>();
        foreach (DiscoveryFact fact in record.facts)
            lines.Add($"{fact.label}: {fact.value}");

        AddEntry(JournalEntryType.Discovery, SectionFor(record.category), record.displayName,
                 string.Join("\n", lines), record.siteId, isRead: false);
    }

    // e.g. "Bass Hole — Slower After Cold Fronts" (Weather_System.md's example journal note).
    void OnObservationAdded(DiscoveryRecord record, DiscoveryObservation observation)
    {
        AddEntry(JournalEntryType.Observation, SectionFor(record.category), $"{record.displayName} — {observation.text}",
                 "", record.siteId, isRead: false);
    }

    void OnMilestoneReached(DiscoveryMilestone milestone)
    {
        AddEntry(JournalEntryType.Milestone, JournalSection.History, milestone.text, "", null, isRead: false);
    }

    JournalEntry AddEntry(JournalEntryType type, JournalSection section, string title, string body, string siteId, bool isRead)
    {
        var entry = new JournalEntry
        {
            id = nextId++,
            type = type,
            section = section,
            day = Today,
            title = title ?? "",
            body = body ?? "",
            siteId = siteId ?? "",
            isRead = isRead,
        };

        entries.Add(entry);
        EntryAdded?.Invoke(entry);
        return entry;
    }

    public JournalSaveData CaptureState() => new JournalSaveData
    {
        entries = new List<JournalEntry>(entries),
        nextId = nextId,
    };

    // Restores silently — no events fire, so listeners should re-read state after a load.
    public void RestoreState(JournalSaveData data)
    {
        ResetJournal();

        if (data.entries != null)
            entries.AddRange(data.entries.FindAll(e => e != null));

        // Never reuse an id, even if the save's counter is behind its entries.
        nextId = Mathf.Max(data.nextId, 1);
        foreach (JournalEntry entry in entries)
            nextId = Mathf.Max(nextId, entry.id + 1);
    }

    // Save_Data_Model.md's World Block: journal entries, categorized per Discovery_System.md.
    string ISaveable.SaveFile => "journal";
    string ISaveable.SaveKey => "journal";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<JournalSaveData>(json));
}
