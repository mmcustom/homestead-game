using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

// Save, load, autosave (Unity_Architecture.md, Save_System.md).
// Each registered ISaveable owns one section of one JSON file; SaveManager never looks inside a section.
//
// Save slots (Save_System.md's Save Slots, build step 1): ten manual slots, slot1 to slot10, plus a separate autosave
// slot, each a folder under Saves/. slot1 is the original single-slot folder, unchanged. Every autosave (end of day,
// sleep, back to the main menu, quit) writes ONLY the autosave slot; a manual save writes the slot the player picked and
// makes it the active slot. Each slot folder also holds slotinfo.json, a small separate record (name, in-game date and
// time, difficulty, saved-at) for the slot list; it is not part of any game section. Saves/slots.json remembers the
// active slot.
public class SaveManager : MonoBehaviour
{
    const int SaveVersion = 1;
    const string MetaFile = "meta";
    const string SlotInfoFile = "slotinfo";
    const string ActiveSlotFile = "slots.json";

    public const int ManualSlotCount = SlotSwap.ManualSlotCount;
    public const string AutosaveId = SlotSwap.AutosaveId;

    [Serializable]
    class SaveSection
    {
        public string key;
        public string data;
    }

    [Serializable]
    class SaveFileData
    {
        public List<SaveSection> sections = new List<SaveSection>();
    }

    [Serializable]
    class SaveMeta
    {
        public int version;
        public string savedAtUtc;
    }

    // The slot list's record for one slot. Kept apart from the game sections so the save format is untouched.
    [Serializable]
    class SlotInfoData
    {
        public string name;
        public string inGameDate;
        public int hour;
        public int minute;
        public int year;
        public string season;
        public string difficulty;
        public string savedAtUtc;
        public string lastSlot; // the current slot when this was saved (SlotRules); empty on a save from before it existed
    }

    [Serializable]
    class ActiveSlotData
    {
        public string activeSlot;
    }

    // What the slot panel shows for one slot.
    public class SlotInfo
    {
        public string Id;
        public string Name;
        public bool Exists;
        public string InGame = "";
        public string Difficulty = "";
        public string LastSlot = "";
        public DateTime SavedAtUtc = DateTime.MinValue;

        public bool IsAutosave => Id == AutosaveId;
        public string SavedAtLocal => SavedAtUtc == DateTime.MinValue ? "" : SavedAtUtc.ToLocalTime().ToString("MMM d, h:mm tt", CultureInfo.CurrentCulture);
    }

    public static SaveManager Instance { get; private set; }

    readonly Dictionary<string, ISaveable> saveables = new Dictionary<string, ISaveable>();
    readonly Queue<string> pendingSaves = new Queue<string>();
    Task currentWrite = Task.CompletedTask;
    bool isSaving;
    string activeSlot;

    public event Action SaveCompleted;
    public event Action<Exception> SaveFailed;

    // Cached in Awake: Application.persistentDataPath can't be read from the background write thread.
    public string SavesRoot { get; private set; }
    public bool IsSaving => isSaving;

    // The manual slot a plain Save writes to, or null (a new game not yet placed, or one loaded from the Autosave).
    public string ActiveSlot => activeSlot;

    // The current slot as the slot screen shows it: the active slot, only while it still holds a save (SlotRules).
    public string CurrentSlot => SlotRules.ResolveCurrent(activeSlot, SlotExists);

    // True when any slot (manual or autosave) holds a completed save.
    public bool HasSave => AllSlotIds().Any(SlotExists);

    static bool InGame =>
        GameManager.Instance != null && GameManager.Instance.InGame;

    // Money_System.md Phase C: a lost game is never written to any slot, the autosave included. One flag covers every path (end of day,
    // sleep, back to the main menu, quit, a manual save), and it is set the moment the third miss lands, even mid-sleep when the game state
    // is still a game screen. The loan is caught up first so a save can never slip in between the miss and the flag.
    static bool SavingBlocked
    {
        get
        {
            MoneyManager money = MoneyManager.Instance;
            if (money == null)
                return false;
            money.ProcessNow();
            return money.Lost;
        }
    }

    public static string SlotId(int number) => "slot" + number;

    public static IEnumerable<string> ManualSlotIds()
    {
        for (int i = 1; i <= ManualSlotCount; i++)
            yield return SlotId(i);
    }

    public static IEnumerable<string> AllSlotIds() => ManualSlotIds().Append(AutosaveId);

    // Guards every slot id that reaches a file path (slots.json is a file the player could edit).
    public static bool IsValidSlot(string id) => SlotSwap.IsValidSlot(id);

    // 1 to 10 for a manual slot, 0 for the autosave.
    public static int SlotNumber(string id) => id != null && id.StartsWith("slot") && int.TryParse(id.Substring(4), out int n) ? n : 0;

    static string DefaultName(string id) => id == AutosaveId ? "Autosave" : "Slot " + SlotNumber(id);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SavesRoot = Path.Combine(Application.persistentDataPath, "Saves");

        // Finish or discard any save, copy or delete a crash interrupted, before anything reads a slot.
        try
        {
            foreach (string line in SlotSwap.RecoverAll(SavesRoot))
                Debug.LogWarning($"[SaveManager] Slot recovery: {line}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Slot recovery failed: {e}");
        }

        activeSlot = ReadActiveSlot();
    }

    void Start()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.DayChanged += OnDayChanged;
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;

        if (TimeManager.Instance != null)
            TimeManager.Instance.DayChanged -= OnDayChanged;
        Instance = null;
    }

    // Save_System.md: never lose progress to an accidental quit. Written synchronously since the app is closing, to the
    // autosave slot only, so quitting (or stopping Play Mode) can never overwrite a manual save.
    void OnApplicationQuit()
    {
        if (!InGame || SavingBlocked)
            return;

        try
        {
            WaitForPendingWrite();
            SlotSwap.Save(SavesRoot, AutosaveId, CaptureFiles(AutosaveId));
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Save on quit failed: {e}");
        }
    }

    // The land loan, the Trading Post market and the account ledger are brought up to the new day BEFORE the autosave is requested (handler
    // order between managers isn't fixed, and TimeManager fires DayChanged before SeasonChanged), so the end-of-day save never holds any of
    // them behind the calendar: a season that just ended is already closed in it. Their CaptureState does the same for every other save.
    void OnDayChanged()
    {
        if (MoneyManager.Instance != null)
            MoneyManager.Instance.ProcessNow();
        if (MarketManager.Instance != null)
            MarketManager.Instance.ProcessNow();
        if (LedgerManager.Instance != null)
            LedgerManager.Instance.ProcessNow();
        RequestAutoSave("End of day");
    }

    public void Register(ISaveable saveable) => saveables[BlockId(saveable)] = saveable;

    public void Unregister(ISaveable saveable)
    {
        string id = BlockId(saveable);
        if (saveables.TryGetValue(id, out ISaveable current) && current == saveable)
            saveables.Remove(id);
    }

    // Save_System.md's auto-save checkpoints (sleep, end of day). Ignored outside gameplay. Always the autosave slot.
    public void RequestAutoSave(string reason)
    {
        if (!InGame || SavingBlocked)
            return;

        Debug.Log($"[SaveManager] Auto-save: {reason}");
        SaveGame(AutosaveId);
    }

    [ContextMenu("Save Now")]
    void SaveNow() => SaveGame(activeSlot ?? AutosaveId);

    // State is captured on the main thread; files are written on a worker thread so saving doesn't hitch gameplay.
    // The target slot's folder is fixed at capture time and handed to the writer, so switching slots mid-write can't mix
    // files. A save requested while one is in flight runs once the current one finishes.
    public async void SaveGame(string slotId)
    {
        if (!IsValidSlot(slotId))
        {
            Debug.LogError($"[SaveManager] Not a save slot: '{slotId}'.");
            return;
        }

        if (SavingBlocked)
        {
            pendingSaves.Clear();
            Debug.Log("[SaveManager] Save skipped: the game is lost.");
            return;
        }

        if (isSaving)
        {
            if (!pendingSaves.Contains(slotId))
                pendingSaves.Enqueue(slotId);
            return;
        }

        isSaving = true;
        try
        {
            string slot = slotId;
            while (slot != null)
            {
                string root = SavesRoot;
                string target = slot;
                Dictionary<string, string> files = CaptureFiles(slot);
                currentWrite = Task.Run(() => SlotSwap.Save(root, target, files));
                await currentWrite;

                // A manual save makes its slot the active one, unless a Delete ran while this write was finishing.
                if (slot != AutosaveId && SlotExists(slot))
                    SetActiveSlot(slot);
                SaveCompleted?.Invoke();
                if (SavingBlocked)
                    pendingSaves.Clear();
                slot = pendingSaves.Count > 0 ? pendingSaves.Dequeue() : null;
            }
        }
        catch (Exception e)
        {
            pendingSaves.Clear();
            Debug.LogError($"[SaveManager] Save failed: {e}");
            SaveFailed?.Invoke(e);
        }
        finally
        {
            isSaving = false;
        }
    }

    // Restores every registered system that has a section in the slot's save. Systems without one keep their current
    // state, so a system added after the save was made starts fresh instead of breaking the load.
    public bool LoadGame(string slotId)
    {
        // A background write may still hold the files open.
        WaitForPendingWrite();

        if (!IsValidSlot(slotId) || !SlotExists(slotId))
            return false;

        try
        {
            string folder = SlotFolder(slotId);
            SaveMeta meta = JsonUtility.FromJson<SaveMeta>(File.ReadAllText(FilePath(folder, MetaFile)));
            if (meta.version != SaveVersion)
                Debug.LogWarning($"[SaveManager] Save version {meta.version} differs from current {SaveVersion}.");

            foreach (IGrouping<string, ISaveable> file in saveables.Values.ToList().GroupBy(s => s.SaveFile))
            {
                string path = FilePath(folder, file.Key);
                if (!File.Exists(path))
                    continue;

                SaveFileData data = JsonUtility.FromJson<SaveFileData>(File.ReadAllText(path));
                foreach (ISaveable saveable in file)
                {
                    SaveSection section = data.sections.Find(s => s.key == saveable.SaveKey);
                    if (section != null)
                        saveable.RestoreState(section.data);
                }
            }

            // Loading a manual slot makes it current; loading the Autosave takes the slot it carried (SlotRules), so Continue
            // still knows the current slot. A save from before lastSlot existed has none.
            SetActiveSlot(SlotRules.CurrentAfterLoad(slotId, GetSlotInfo(slotId).LastSlot, SlotExists));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Load failed: {e}");
            return false;
        }
    }

    // Deletes one slot's folder. Deleting the active slot clears the active slot. No context-menu shortcut: it must
    // never delete a save by accident.
    public bool DeleteSave(string slotId)
    {
        if (!IsValidSlot(slotId))
            return false;

        DrainPendingSaves();
        try
        {
            SlotSwap.Delete(SavesRoot, slotId);
            if (activeSlot == slotId)
                SetActiveSlot(null);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Delete failed: {e}");
            return false;
        }
    }

    // Copies one slot onto a manual slot, replacing whatever was there (the caller asks first). The copy is built in a
    // temporary folder and swapped into place (SlotSwap), and its slotinfo takes the target's name.
    public bool CopySlot(string fromId, string toId, out string error)
    {
        error = null;
        if (!IsValidSlot(fromId) || !IsValidSlot(toId) || toId == AutosaveId || fromId == toId)
        {
            error = "That copy isn't allowed.";
            return false;
        }

        DrainPendingSaves();
        if (!SlotExists(fromId))
        {
            error = "There is nothing in that slot to copy.";
            return false;
        }

        try
        {
            SlotSwap.Copy(SavesRoot, fromId, toId, temp =>
            {
                string infoPath = FilePath(temp, SlotInfoFile);
                if (File.Exists(infoPath))
                {
                    SlotInfoData info = JsonUtility.FromJson<SlotInfoData>(File.ReadAllText(infoPath));
                    info.name = DefaultName(toId);
                    info.lastSlot = toId; // a manual slot's record names itself
                    SlotSwap.WriteFlushed(infoPath, JsonUtility.ToJson(info, true));
                }
            });
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Copy failed: {e}");
            error = "The copy failed. Details are in the log.";
            return false;
        }
    }

    // --- Slot information ---

    // Waits for a write in flight: during its swap the slot folder briefly doesn't exist.
    public bool SlotExists(string slotId)
    {
        if (!IsValidSlot(slotId))
            return false;

        WaitForPendingWrite();
        return File.Exists(FilePath(SlotFolder(slotId), MetaFile));
    }

    public SlotInfo GetSlotInfo(string slotId)
    {
        WaitForPendingWrite();
        var info = new SlotInfo { Id = slotId, Name = DefaultName(slotId) };
        if (!SlotExists(slotId))
            return info;

        info.Exists = true;
        string folder = SlotFolder(slotId);
        try
        {
            SaveMeta meta = JsonUtility.FromJson<SaveMeta>(File.ReadAllText(FilePath(folder, MetaFile)));
            if (DateTime.TryParse(meta.savedAtUtc, null, DateTimeStyles.RoundtripKind, out DateTime saved))
                info.SavedAtUtc = saved.ToUniversalTime();
        }
        catch (Exception)
        {
        }

        // slot1 from before slots has no slotinfo yet: it shows as "Slot 1" with just the saved-at time.
        string infoPath = FilePath(folder, SlotInfoFile);
        if (File.Exists(infoPath))
        {
            try
            {
                SlotInfoData data = JsonUtility.FromJson<SlotInfoData>(File.ReadAllText(infoPath));
                if (!string.IsNullOrEmpty(data.name))
                    info.Name = data.name;
                info.InGame = $"{data.inGameDate} · {data.hour:00}:{data.minute:00}";
                info.Difficulty = data.difficulty ?? "";
                info.LastSlot = data.lastSlot ?? "";
            }
            catch (Exception)
            {
            }
        }

        return info;
    }

    // The slot whose save is newest (by the time it was written), or null when no slot has a save. Continue loads this.
    public string NewestSlot()
    {
        WaitForPendingWrite();
        string newest = null;
        DateTime newestTime = DateTime.MinValue;
        foreach (string id in AllSlotIds())
        {
            if (!SlotExists(id))
                continue;
            DateTime time = GetSlotInfo(id).SavedAtUtc;
            if (newest == null || time > newestTime)
            {
                newest = id;
                newestTime = time;
            }
        }
        return newest;
    }

    // Remembers the manual slot a plain Save writes to (null or the Autosave clears it).
    public void SetActiveSlot(string slotId)
    {
        activeSlot = slotId != null && slotId != AutosaveId && IsValidSlot(slotId) ? slotId : null;
        try
        {
            Directory.CreateDirectory(SavesRoot);
            SlotSwap.WriteFileAtomic(Path.Combine(SavesRoot, ActiveSlotFile), JsonUtility.ToJson(new ActiveSlotData { activeSlot = activeSlot ?? "" }));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveManager] Couldn't remember the active slot: {e.Message}");
        }
    }

    // Missing or invalid means no active slot.
    string ReadActiveSlot()
    {
        try
        {
            string path = Path.Combine(SavesRoot, ActiveSlotFile);
            if (!File.Exists(path))
                return null;
            string id = JsonUtility.FromJson<ActiveSlotData>(File.ReadAllText(path)).activeSlot;
            return IsValidSlot(id) && id != AutosaveId ? id : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    // Blocks until any background write finishes. A failed write was already reported by SaveGame,
    // so it must not stop the next save from running.
    void WaitForPendingWrite()
    {
        try
        {
            currentWrite.Wait();
        }
        catch (AggregateException)
        {
        }
    }

    // Finishes the write in flight and then every save queued behind it, on the main thread, so a Delete or Copy that follows
    // can't have a queued save start afterwards and recreate the slot.
    void DrainPendingSaves()
    {
        WaitForPendingWrite();
        if (SavingBlocked)
            pendingSaves.Clear();
        while (pendingSaves.Count > 0)
        {
            string slot = pendingSaves.Dequeue();
            try
            {
                SlotSwap.Save(SavesRoot, slot, CaptureFiles(slot));
                if (slot != AutosaveId)
                    SetActiveSlot(slot);
                SaveCompleted?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Save failed: {e}");
                SaveFailed?.Invoke(e);
            }
        }
    }

    Dictionary<string, string> CaptureFiles(string slotId)
    {
        var files = new Dictionary<string, SaveFileData>();
        foreach (ISaveable saveable in saveables.Values)
        {
            if (!files.TryGetValue(saveable.SaveFile, out SaveFileData file))
                files[saveable.SaveFile] = file = new SaveFileData();

            file.sections.Add(new SaveSection
            {
                key = saveable.SaveKey,
                data = JsonUtility.ToJson(saveable.CaptureState()),
            });
        }

        string savedAt = DateTime.UtcNow.ToString("o");
        var json = files.ToDictionary(pair => pair.Key, pair => JsonUtility.ToJson(pair.Value, true));
        json[MetaFile] = JsonUtility.ToJson(new SaveMeta
        {
            version = SaveVersion,
            savedAtUtc = savedAt,
        }, true);
        json[SlotInfoFile] = JsonUtility.ToJson(BuildSlotInfo(slotId, savedAt, CurrentSlot), true);
        return json;
    }

    // Main thread only: reads the clock and difficulty.
    static SlotInfoData BuildSlotInfo(string slotId, string savedAt, string currentSlot)
    {
        var info = new SlotInfoData
        {
            name = DefaultName(slotId),
            difficulty = DifficultyManager.Tier.ToString(),
            savedAtUtc = savedAt,
            lastSlot = SlotRules.LastSlotToWrite(slotId, currentSlot) ?? "",
        };

        TimeManager time = TimeManager.Instance;
        if (time != null)
        {
            info.inGameDate = time.FormatDate(time.TotalDays);
            info.hour = time.Hour;
            info.minute = time.Minute;
            info.year = time.Year;
            info.season = time.CurrentSeason.ToString();
        }
        return info;
    }

    string SlotFolder(string slotId) => Path.Combine(SavesRoot, slotId);

    static string FilePath(string folder, string fileName) => Path.Combine(folder, fileName + ".json");

    static string BlockId(ISaveable saveable) => saveable.SaveFile + "/" + saveable.SaveKey;
}
