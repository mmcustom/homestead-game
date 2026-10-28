using System;
using System.Collections.Generic;
using UnityEngine;

// Discovery_System.md's journal categories, plus History for its Historical Records and Notes for player notes.
public enum JournalSection { WaterSources, Plants, Wildlife, Fishing, PropertyFeatures, History, Notes, KnowHow }

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

// One unlocked Know-how entry. Only the id is kept: the text lives in KnowHowRules, so a wording fix reaches old saves too.
[Serializable]
public class KnowHowUnlock
{
    public string id;
    public int day;
    public bool read;
}

[Serializable]
public struct JournalSaveData
{
    public List<JournalEntry> entries;
    public int nextId;

    // Additive: a save from before Know-how has none.
    public List<KnowHowUnlock> knowHow;
}

// Journal entries, player notes, discovery records (Unity_Architecture.md).
// Discovery_System.md: every discovery creates a journal entry, and the journal becomes the player's field
// notebook. Entries are written from DiscoveryManager's events as they happen and kept as written; only the
// player's own notes can be edited or deleted, since important records stay permanent.
public class JournalManager : MonoBehaviour, ISaveable
{
    public static JournalManager Instance { get; private set; }

    readonly List<JournalEntry> entries = new List<JournalEntry>();
    readonly List<KnowHowUnlock> knowHow = new List<KnowHowUnlock>();
    int nextId = 1;
    bool subscribed;

    // Know-how unlocks wait for the HUD's flash to clear before they announce themselves, so they never hide a message that is on show.
    InventoryContainer watchedPack;
    readonly List<string> toAnnounce = new List<string>();
    bool backfillPending;

    public event Action<JournalEntry> EntryAdded;
    public event Action<JournalEntry> EntryChanged;
    public event Action<JournalEntry> EntryRemoved;
    public event Action KnowHowChanged;

    // Oldest first.
    public IReadOnlyList<JournalEntry> Entries => entries;
    public int UnreadCount => entries.FindAll(e => !e.isRead).Count;

    // Know-how, in the order unlocked.
    public IReadOnlyList<KnowHowUnlock> KnowHow => knowHow;
    public int KnowHowUnread => knowHow.FindAll(k => !k.read).Count;

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
            case JournalSection.KnowHow: return "Know-how";
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

    void Update()
    {
        if (Instance != this)
            return;
        WatchPack();
        // Waits for the world to be live, so the flash is not spent behind a loading screen or the main menu.
        bool live = GameManager.Instance == null || GameManager.Instance.State == GameState.Playing;
        if (backfillPending && live)
            Backfill();
        Announce();
    }

    // Subscribes to the player's pack once it exists, and again if the pack is ever replaced.
    void WatchPack()
    {
        InventoryManager inventory = InventoryManager.Instance;
        InventoryContainer pack = inventory != null ? inventory.Player : null;
        if (pack == watchedPack)
            return;
        if (watchedPack != null)
            watchedPack.ItemsAdded -= OnPackItemsAdded;
        watchedPack = pack;
        if (watchedPack != null)
            watchedPack.ItemsAdded += OnPackItemsAdded;
    }

    void OnPackItemsAdded(string itemId, int quantity) => Unlock(KnowHowRules.ForItem(itemId), announce: true);

    public bool HasKnowHow(string id) => knowHow.Exists(k => k.id == id);

    // Something happened in the world that opens an entry (a Pot hung, a hopper site placed).
    public void NoteHappening(KnowHowRules.Happening what) => Unlock(KnowHowRules.ForHappening(what), announce: true);

    // Adds the entries not unlocked yet. Returns how many were new; announce queues the "new know-how" flash.
    public int Unlock(IEnumerable<string> ids, bool announce)
    {
        List<string> fresh = KnowHowRules.NewUnlocks(knowHow.ConvertAll(k => k.id), ids);
        foreach (string id in fresh)
        {
            knowHow.Add(new KnowHowUnlock { id = id, day = Today, read = false });
            if (announce)
                toAnnounce.Add(id);
        }
        if (fresh.Count > 0)
            KnowHowChanged?.Invoke();
        return fresh.Count;
    }

    public void MarkKnowHowRead(string id)
    {
        KnowHowUnlock unlock = knowHow.Find(k => k.id == id);
        if (unlock == null || unlock.read)
            return;
        unlock.read = true;
        KnowHowChanged?.Invoke();
    }

    // After a load: unlock what the saved game already has (an old save from before Know-how), with one summary flash.
    void Backfill()
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
            return;
        backfillPending = false;

        bool hopper = false;
        if (WoodManager.Instance != null)
        {
            foreach (WoodPileState pile in WoodManager.Instance.Piles)
                hopper |= pile.kind == PileKind.LeachHopperSite || pile.kind == PileKind.LeachHopper;
        }
        bool tripod = false;
        if (FireManager.Instance != null)
        {
            foreach (CampfireState fire in FireManager.Instance.Fires)
                tripod |= fire.tripod || !string.IsNullOrEmpty(fire.vessel);
        }
        bool taps = TapManager.Instance != null && TapManager.Instance.Taps.Count > 0;

        List<string> earned = KnowHowRules.Backfill(id => inventory.Player.Has(id), hopper, tripod, taps);
        if (Unlock(earned, announce: false) > 0)
        {
            ToolStatus.Flash(KnowHowRules.BackfillFlash(earned), 4f);
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play(SoundCue.JournalUpdated);
        }
    }

    // The flash for entries unlocked just now, once nothing else is on the HUD.
    void Announce()
    {
        if (toAnnounce.Count == 0 || ToolStatus.FlashActive)
            return;
        ToolStatus.Flash(KnowHowRules.UnlockedMessage(toAnnounce), 4f);
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.JournalUpdated);
        toAnnounce.Clear();
    }

    void OnDestroy()
    {
        if (watchedPack != null)
            watchedPack.ItemsAdded -= OnPackItemsAdded;
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
        knowHow.Clear();
        toAnnounce.Clear();
        backfillPending = true; // a new game's starting kit (a Tripod) and a loaded game's contents both earn entries, once the world is live
        nextId = 1;
        KnowHowChanged?.Invoke();
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
        knowHow = knowHow.ConvertAll(k => new KnowHowUnlock { id = k.id, day = k.day, read = k.read }),
    };

    // Restores silently — no events fire, so listeners should re-read state after a load.
    public void RestoreState(JournalSaveData data)
    {
        ResetJournal();

        if (data.entries != null)
            entries.AddRange(data.entries.FindAll(e => e != null));

        if (data.knowHow != null)
        {
            var seen = new HashSet<string>();
            foreach (KnowHowUnlock unlock in data.knowHow)
            {
                // An id this build does not know (or one saved twice) is dropped, so a stale save can never break the section.
                if (unlock != null && !string.IsNullOrEmpty(unlock.id) && KnowHowRules.TryGet(unlock.id, out _) && seen.Add(unlock.id))
                    knowHow.Add(unlock);
            }
        }
        backfillPending = true; // the world is restored by the end of this frame; the backfill reads it then

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
