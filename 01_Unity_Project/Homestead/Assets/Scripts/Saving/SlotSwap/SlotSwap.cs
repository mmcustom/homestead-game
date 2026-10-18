using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

// Whole-slot atomic writes (Save_System.md Decision 5). A slot is a folder under the saves root. A save, a copy or a delete
// never edits a slot in place: the new slot is built in "<id>.tmp" and swapped in with folder renames, so a crash or power
// cut leaves either the complete old slot or the complete new slot, never a mix.
//
//   save/copy:  recover <id>; build <id>.tmp; rename <id> -> <id>.old; rename <id>.tmp -> <id>; delete <id>.old
//   delete:     recover <id>; rename <id> -> <id>.del; delete <id>.del
//
// Recovery (run at start-up and at the start of every operation on the id):
//   <id> exists            -> delete <id>.tmp, <id>.old and <id>.del
//   <id> missing, .old     -> rename <id>.old back to <id>, delete <id>.tmp and <id>.del
//   otherwise              -> delete <id>.tmp and <id>.del
// A leftover .tmp is always discarded, never promoted.
//
// Path safety: every folder touched is Path.Combine(root, id) plus a constant suffix, and the id must pass IsValidSlot (a
// whitelist, so nothing like ".." or a rooted path can reach a path). No Unity API in here: it runs on a worker thread.
public static class SlotSwap
{
    public const int ManualSlotCount = 10;
    public const string AutosaveId = "autosave";

    public const string TempSuffix = ".tmp";
    public const string OldSuffix = ".old";
    public const string DelSuffix = ".del";

    // Step 1's copy folder, no longer used; start-up recovery removes a leftover one.
    const string LegacyCopyTemp = ".copy_tmp";

    // The file name (no extension) written last, so its presence marks a completed save.
    const string MetaKey = "meta";

    // Crash points a test can ask the swap to die at.
    public const string CrashMidTemp = "MidTemp";
    public const string CrashAfterTemp = "AfterTemp";
    public const string CrashAfterOld = "AfterOldRenamed";
    public const string CrashAfterNew = "AfterNewRenamed";
    public const string CrashAfterDel = "AfterDelRenamed";

    // Renames retry because antivirus or an indexer can hold a freshly written folder for a moment.
    public static int RenameAttempts = 5;
    public static int RenameDelayMs = 150;

    // Thrown by a test's crash hook to simulate a power cut. The swap's own error handling deliberately does not catch it,
    // so the folders are left exactly as they were at that point.
    public class SimulatedCrash : Exception
    {
        public SimulatedCrash(string point) : base("Simulated crash at " + point) { }
    }

    public static IEnumerable<string> ManualSlotIds()
    {
        for (int i = 1; i <= ManualSlotCount; i++)
            yield return "slot" + i;
    }

    public static bool IsValidSlot(string id) => !string.IsNullOrEmpty(id) && (id == AutosaveId || ManualSlotIds().Contains(id));

    // --- Operations ---

    // Writes `files` (name without extension -> contents) as the whole new slot. meta is written last.
    public static void Save(string root, string id, IReadOnlyDictionary<string, string> files, Action<string> crashPoint = null)
    {
        RequireValid(id);
        if (!files.ContainsKey(MetaKey))
            throw new ArgumentException("A save needs a meta file.", nameof(files));

        Recover(root, id);

        string temp = Folder(root, id, TempSuffix);
        try
        {
            Directory.CreateDirectory(temp);

            bool first = true;
            foreach (KeyValuePair<string, string> file in files.Where(f => f.Key != MetaKey))
            {
                WriteFlushed(Path.Combine(temp, file.Key + ".json"), file.Value);
                if (first)
                {
                    crashPoint?.Invoke(CrashMidTemp);
                    first = false;
                }
            }
            WriteFlushed(Path.Combine(temp, MetaKey + ".json"), files[MetaKey]);
            crashPoint?.Invoke(CrashAfterTemp);
        }
        catch (Exception e) when (!(e is SimulatedCrash))
        {
            TryDelete(temp);
            throw;
        }

        Commit(root, id, crashPoint);
    }

    // Replaces slot `toId` with a copy of slot `fromId`. `adjust` receives the temp folder to change files in before the
    // swap (use WriteFlushed). The target is whole-old until the swap completes.
    public static void Copy(string root, string fromId, string toId, Action<string> adjust = null, Action<string> crashPoint = null)
    {
        RequireValid(fromId);
        RequireValid(toId);
        if (fromId == toId)
            throw new ArgumentException("Can't copy a slot onto itself.");

        Recover(root, fromId);
        Recover(root, toId);

        string source = Path.Combine(root, fromId);
        if (!Directory.Exists(source))
            throw new DirectoryNotFoundException("There is nothing in that slot to copy.");

        string temp = Folder(root, toId, TempSuffix);
        try
        {
            Directory.CreateDirectory(temp);

            bool first = true;
            // meta last, like a save.
            foreach (string path in Directory.GetFiles(source, "*.json").OrderBy(p => Path.GetFileNameWithoutExtension(p) == MetaKey))
            {
                string dest = Path.Combine(temp, Path.GetFileName(path));
                File.Copy(path, dest);
                Flush(dest);
                if (first)
                {
                    crashPoint?.Invoke(CrashMidTemp);
                    first = false;
                }
            }
            adjust?.Invoke(temp);
            crashPoint?.Invoke(CrashAfterTemp);
        }
        catch (Exception e) when (!(e is SimulatedCrash))
        {
            TryDelete(temp);
            throw;
        }

        Commit(root, toId, crashPoint);
    }

    // Removes a slot. The rename is the delete as far as the slot is concerned; the files go afterwards.
    public static void Delete(string root, string id, Action<string> crashPoint = null)
    {
        RequireValid(id);
        Recover(root, id);

        string slot = Path.Combine(root, id);
        if (!Directory.Exists(slot))
            return;

        string del = Folder(root, id, DelSuffix);
        MoveWithRetry(slot, del);
        crashPoint?.Invoke(CrashAfterDel);
        TryDelete(del);
    }

    // The temp folder is complete: swap it in.
    static void Commit(string root, string id, Action<string> crashPoint)
    {
        string slot = Path.Combine(root, id);
        string temp = Folder(root, id, TempSuffix);
        string old = Folder(root, id, OldSuffix);

        if (Directory.Exists(slot))
        {
            try
            {
                MoveWithRetry(slot, old);
            }
            catch (Exception e) when (!(e is SimulatedCrash))
            {
                TryDelete(temp); // the old slot was not touched
                throw;
            }
        }
        crashPoint?.Invoke(CrashAfterOld);

        try
        {
            MoveWithRetry(temp, slot);
        }
        catch (Exception e) when (!(e is SimulatedCrash))
        {
            // Put the old slot back; if even that fails, start-up recovery restores it from .old.
            if (!Directory.Exists(slot) && Directory.Exists(old))
            {
                try { MoveWithRetry(old, slot); } catch (Exception) { }
            }
            TryDelete(temp);
            throw;
        }
        crashPoint?.Invoke(CrashAfterNew);

        TryDelete(old); // best effort: recovery removes it next start if something holds it
    }

    // --- Recovery ---

    // Recovers one slot id; returns what it did or what went wrong.
    public static List<string> Recover(string root, string id)
    {
        RequireValid(id);
        var log = new List<string>();

        string slot = Path.Combine(root, id);
        string temp = Folder(root, id, TempSuffix);
        string old = Folder(root, id, OldSuffix);
        string del = Folder(root, id, DelSuffix);

        if (!Directory.Exists(slot) && Directory.Exists(old))
        {
            try
            {
                MoveWithRetry(old, slot);
                log.Add($"Restored {id} from {id}{OldSuffix}.");
            }
            catch (Exception e)
            {
                log.Add($"Couldn't restore {id} from {id}{OldSuffix}: {e.Message}");
            }
        }

        foreach (string leftover in new[] { temp, del, old })
        {
            if (leftover == old && !Directory.Exists(slot))
                continue; // the slot is missing and the restore failed: keep the only copy

            if (!Directory.Exists(leftover))
                continue;
            try
            {
                Directory.Delete(leftover, true);
                log.Add($"Removed leftover {Path.GetFileName(leftover)}.");
            }
            catch (Exception e)
            {
                log.Add($"Couldn't remove {Path.GetFileName(leftover)}: {e.Message}");
            }
        }
        return log;
    }

    // Start-up scan: acts only on folders ending .tmp, .old or .del whose base name is a valid slot id (plus step 1's
    // .copy_tmp). Anything else in the saves root is left alone.
    public static List<string> RecoverAll(string root)
    {
        var log = new List<string>();
        if (!Directory.Exists(root))
            return log;

        string legacy = Path.Combine(root, LegacyCopyTemp);
        if (Directory.Exists(legacy))
        {
            try
            {
                Directory.Delete(legacy, true);
                log.Add($"Removed leftover {LegacyCopyTemp}.");
            }
            catch (Exception e)
            {
                log.Add($"Couldn't remove {LegacyCopyTemp}: {e.Message}");
            }
        }

        var ids = new SortedSet<string>(StringComparer.Ordinal);
        foreach (string dir in Directory.GetDirectories(root))
        {
            string name = Path.GetFileName(dir);
            foreach (string suffix in new[] { TempSuffix, OldSuffix, DelSuffix })
            {
                if (name.EndsWith(suffix, StringComparison.Ordinal))
                {
                    string id = name.Substring(0, name.Length - suffix.Length);
                    if (IsValidSlot(id))
                        ids.Add(id);
                }
            }
        }

        foreach (string id in ids)
            log.AddRange(Recover(root, id));
        return log;
    }

    // --- File helpers ---

    // Writes a file and forces it to disk before returning (a power cut can't leave it empty or short).
    public static void WriteFlushed(string path, string contents)
    {
        byte[] bytes = new UTF8Encoding(false).GetBytes(contents);
        using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush(true);
        }
    }

    // One file replaced atomically (for small standalone files such as slots.json).
    public static void WriteFileAtomic(string path, string contents)
    {
        string temp = path + TempSuffix;
        WriteFlushed(temp, contents);

        if (File.Exists(path))
            File.Replace(temp, path, null);
        else
            File.Move(temp, path);
    }

    static void Flush(string path)
    {
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.None))
            stream.Flush(true);
    }

    // --- Internals ---

    static void RequireValid(string id)
    {
        if (!IsValidSlot(id))
            throw new ArgumentException($"Not a save slot: '{id}'.", nameof(id));
    }

    // id has passed IsValidSlot, suffix is one of the constants.
    static string Folder(string root, string id, string suffix) => Path.Combine(root, id + suffix);

    static void MoveWithRetry(string from, string to)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                Directory.Move(from, to);
                return;
            }
            catch (Exception e) when ((e is IOException || e is UnauthorizedAccessException) && attempt < RenameAttempts)
            {
                Thread.Sleep(RenameDelayMs);
            }
        }
    }

    static void TryDelete(string folder)
    {
        try
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }
        catch (Exception)
        {
        }
    }
}
