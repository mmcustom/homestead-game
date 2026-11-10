using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// One-off dev tool (real-calendar Phase 1): converts ONE save from the old calendar into a NEW slot on the real one. It only ever READS the source slot and writes a slot that does not exist
// yet; it refuses to run in Play Mode, on a source that is not an old-calendar save, or when no manual slot is free. The rules are CalendarConversion (pure, tested); this does the files.
// Homestead > Dev > Convert Slot 4 to a New-Calendar Slot.
public static class CalendarConversionTool
{
    const string SourceSlot = "slot4";
    static readonly string[] Files = { "world", "player", "inventory", "journal", "discovery" };

    [Serializable]
    class Section
    {
        public string key;
        public string data;
    }

    [Serializable]
    class FileData
    {
        public List<Section> sections = new List<Section>();
    }

    [Serializable]
    class Meta
    {
        public int version;
        public string savedAtUtc;
        public int calendarVersion;
    }

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
        public string lastSlot;
        public int calendarVersion;
    }

    [MenuItem("Homestead/Dev/Convert Slot 4 to a New-Calendar Slot")]
    static void ConvertSlot4Menu()
    {
        string report = Convert(SourceSlot);
        Debug.Log("[CalendarConversion] " + report);
        EditorUtility.DisplayDialog("Calendar conversion", report, "OK");
    }

    // Converts the source slot into the first free manual slot and returns a report (what landed where, and what was converted). Begins with "FAILED" when nothing was written.
    public static string Convert(string sourceSlot)
    {
        if (EditorApplication.isPlaying)
            return "FAILED: not in Play Mode.";
        string root = Path.Combine(Application.persistentDataPath, "Saves");
        string source = Path.Combine(root, sourceSlot);
        if (!Directory.Exists(source))
            return $"FAILED: {sourceSlot} does not exist.";

        Meta sourceMeta = JsonUtility.FromJson<Meta>(File.ReadAllText(Path.Combine(source, "meta.json")));
        if (!CalendarSaveRules.IsLegacy(sourceMeta.calendarVersion))
            return $"FAILED: {sourceSlot} is not an old-calendar save (calendarVersion {sourceMeta.calendarVersion}).";

        string targetSlot = null;
        for (int i = 1; i <= 10 && targetSlot == null; i++)
            if (!Directory.Exists(Path.Combine(root, "slot" + i)) && !Directory.Exists(Path.Combine(root, "slot" + i + ".tmp")))
                targetSlot = "slot" + i;
        if (targetSlot == null)
            return "FAILED: no free manual slot to write to.";
        string target = Path.Combine(root, targetSlot);

        var log = new List<string>();
        var converted = new Dictionary<string, string>();
        try
        {
            // The clock first: every other section is converted against the saved game's own day and hour.
            FileData world = JsonUtility.FromJson<FileData>(File.ReadAllText(Path.Combine(source, "world.json")));
            string timeJson = world.sections.Find(s => s.key == "time").data;
            TimeSaveData time = JsonUtility.FromJson<TimeSaveData>(timeJson);
            int oldToday = time.totalDays;
            int hourOfDay = (int)(time.minuteOfDay / 60f);
            int newToday = CalendarConversion.MapDay(oldToday);
            log.Add($"day {oldToday} ({CalendarRules.Legacy.FormatDate(oldToday)}) -> day {newToday} ({CalendarRules.Real.FormatDate(newToday)})");

            foreach (string file in Files)
            {
                FileData data = JsonUtility.FromJson<FileData>(File.ReadAllText(Path.Combine(source, file + ".json")));
                foreach (Section section in data.sections)
                    section.data = CalendarConversion.ConvertSection(file, section.key, section.data, oldToday, hourOfDay, log);
                converted[file] = JsonUtility.ToJson(data, true);
            }

            SlotInfoData info = JsonUtility.FromJson<SlotInfoData>(File.ReadAllText(Path.Combine(source, "slotinfo.json")));
            info.name = $"Slot {SlotNumber(targetSlot)} (from {info.name})";
            info.inGameDate = CalendarRules.Real.FormatDate(newToday);
            info.year = CalendarRules.Real.YearOfDay(newToday);
            info.season = GameCalendar.SeasonName(CalendarRules.Real.SeasonOfYear(newToday));
            info.lastSlot = targetSlot;
            info.calendarVersion = CalendarRules.SaveVersion;
            sourceMeta.calendarVersion = CalendarRules.SaveVersion;

            // Written only now that everything converted; into a folder that did not exist. Anything that goes wrong removes what was started.
            Directory.CreateDirectory(target);
            foreach (KeyValuePair<string, string> file in converted)
                File.WriteAllText(Path.Combine(target, file.Key + ".json"), file.Value);
            File.WriteAllText(Path.Combine(target, "meta.json"), JsonUtility.ToJson(sourceMeta, true));
            File.WriteAllText(Path.Combine(target, "slotinfo.json"), JsonUtility.ToJson(info, true));
        }
        catch (Exception e)
        {
            if (Directory.Exists(target))
                Directory.Delete(target, true);
            return "FAILED: " + e.Message;
        }

        return $"{sourceSlot} (unchanged) -> {targetSlot}:\n" + string.Join("\n", log);
    }

    static string SlotNumber(string slot) => slot.Substring(4);
}
