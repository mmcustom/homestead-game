using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

// Save, load, autosave (Unity_Architecture.md, Save_System.md).
// Each registered ISaveable owns one section of one JSON file; SaveManager never looks inside a section.
public class SaveManager : MonoBehaviour
{
    const int SaveVersion = 1;
    const string MetaFile = "meta";

    // Save_System.md: a single slot is sufficient for Alpha 0.1.
    const string SlotName = "slot1";

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

    public static SaveManager Instance { get; private set; }

    readonly Dictionary<string, ISaveable> saveables = new Dictionary<string, ISaveable>();
    Task currentWrite = Task.CompletedTask;
    bool isSaving;
    bool saveQueued;

    public event Action SaveCompleted;
    public event Action<Exception> SaveFailed;

    // Cached in Awake: Application.persistentDataPath can't be read from the background write thread.
    public string SlotPath { get; private set; }
    public bool HasSave => File.Exists(FilePath(MetaFile));
    public bool IsSaving => isSaving;

    static bool InGame =>
        GameManager.Instance != null &&
        (GameManager.Instance.State == GameState.Playing || GameManager.Instance.State == GameState.Paused);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SlotPath = Path.Combine(Application.persistentDataPath, "Saves", SlotName);
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

    // Save_System.md: never lose progress to an accidental quit. Written synchronously since the app is closing.
    void OnApplicationQuit()
    {
        if (!InGame)
            return;

        try
        {
            WaitForPendingWrite();
            WriteFiles(CaptureFiles());
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Save on quit failed: {e}");
        }
    }

    void OnDayChanged() => RequestAutoSave("End of day");

    public void Register(ISaveable saveable) => saveables[BlockId(saveable)] = saveable;

    public void Unregister(ISaveable saveable)
    {
        string id = BlockId(saveable);
        if (saveables.TryGetValue(id, out ISaveable current) && current == saveable)
            saveables.Remove(id);
    }

    // Save_System.md's auto-save checkpoints (sleep, building entry/exit, end of day). Ignored outside gameplay.
    public void RequestAutoSave(string reason)
    {
        if (!InGame)
            return;

        Debug.Log($"[SaveManager] Auto-save: {reason}");
        SaveGame();
    }

    // State is captured on the main thread; files are written on a worker thread so saving doesn't hitch gameplay.
    // A save requested while one is in flight runs once the current one finishes.
    [ContextMenu("Save Now")]
    public async void SaveGame()
    {
        if (isSaving)
        {
            saveQueued = true;
            return;
        }

        isSaving = true;
        try
        {
            do
            {
                saveQueued = false;
                Dictionary<string, string> files = CaptureFiles();
                currentWrite = Task.Run(() => WriteFiles(files));
                await currentWrite;
                SaveCompleted?.Invoke();
            }
            while (saveQueued);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Save failed: {e}");
            SaveFailed?.Invoke(e);
        }
        finally
        {
            isSaving = false;
        }
    }

    // Restores every registered system that has a section in the save. Systems without one keep their current state,
    // so a system added after the save was made starts fresh instead of breaking the load.
    public bool LoadGame()
    {
        // A background write may still hold the files open.
        WaitForPendingWrite();

        if (!HasSave)
            return false;

        try
        {
            SaveMeta meta = JsonUtility.FromJson<SaveMeta>(File.ReadAllText(FilePath(MetaFile)));
            if (meta.version != SaveVersion)
                Debug.LogWarning($"[SaveManager] Save version {meta.version} differs from current {SaveVersion}.");

            foreach (IGrouping<string, ISaveable> file in saveables.Values.ToList().GroupBy(s => s.SaveFile))
            {
                string path = FilePath(file.Key);
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

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Load failed: {e}");
            return false;
        }
    }

    [ContextMenu("Delete Save")]
    public void DeleteSave()
    {
        WaitForPendingWrite();
        if (Directory.Exists(SlotPath))
            Directory.Delete(SlotPath, true);
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

    Dictionary<string, string> CaptureFiles()
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

        var json = files.ToDictionary(pair => pair.Key, pair => JsonUtility.ToJson(pair.Value, true));
        json[MetaFile] = JsonUtility.ToJson(new SaveMeta
        {
            version = SaveVersion,
            savedAtUtc = DateTime.UtcNow.ToString("o"),
        }, true);
        return json;
    }

    // Runs off the main thread — no Unity API calls in here.
    void WriteFiles(Dictionary<string, string> files)
    {
        Directory.CreateDirectory(SlotPath);

        foreach (KeyValuePair<string, string> file in files)
        {
            if (file.Key != MetaFile)
                WriteAtomic(FilePath(file.Key), file.Value);
        }

        // Meta last: its presence marks a completed save.
        WriteAtomic(FilePath(MetaFile), files[MetaFile]);

        // Drop files from an earlier save that no system wrote this time, so a load never mixes old and new state.
        foreach (string path in Directory.GetFiles(SlotPath, "*.json"))
        {
            if (!files.ContainsKey(Path.GetFileNameWithoutExtension(path)))
                File.Delete(path);
        }
    }

    // Write to a temp file then swap it in, so a crash mid-write can't leave a half-written file.
    static void WriteAtomic(string path, string contents)
    {
        string temp = path + ".tmp";
        File.WriteAllText(temp, contents);

        if (File.Exists(path))
            File.Replace(temp, path, null);
        else
            File.Move(temp, path);
    }

    string FilePath(string fileName) => Path.Combine(SlotPath, fileName + ".json");

    static string BlockId(ISaveable saveable) => saveable.SaveFile + "/" + saveable.SaveKey;
}
