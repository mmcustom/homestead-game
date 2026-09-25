using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Discovery_System.md's Journal Screen (confirmed 2026-09-25): JournalManager's entries browsed by its seven sections,
// newest first. Section tabs on the left show how many entries each holds and whether any are unread; the middle lists
// the section's entries; the right shows the selected one, which marks it read. Discovery, observation and milestone
// entries are read-only records. The player's own notes can be written, edited in place and deleted.
public class JournalScreen : GameScreen
{
    static readonly JournalSection[] Sections =
    {
        JournalSection.WaterSources, JournalSection.Plants, JournalSection.Wildlife, JournalSection.Fishing,
        JournalSection.PropertyFeatures, JournalSection.History, JournalSection.Notes,
    };

    readonly Button[] sectionTabs = new Button[Sections.Length];
    RectTransform entryList;
    RectTransform detail;
    JournalSection section = JournalSection.WaterSources;
    int selectedId = -1;

    // The note being edited, if the selected entry is a player note.
    InputField noteTitle, noteBody;
    Button deleteButton;
    bool confirmingDelete;

    public override string Title => "Field Journal";

    public override void Build(RectTransform area)
    {
        RectTransform tabs = UiKit.Rect("Sections", area).Region(0f, 0f, 0.2f, 1f);
        var layout = tabs.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 6f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        for (int i = 0; i < Sections.Length; i++)
        {
            JournalSection s = Sections[i];
            sectionTabs[i] = UiKit.Height(UiKit.Button(tabs, s.ToString(), "", 21, () => SelectSection(s), TextAnchor.MiddleLeft), 50f);
        }

        entryList = UiKit.ScrollList(area, "Entries");
        ((RectTransform)entryList.parent).Region(0.21f, 0f, 0.5f, 1f);

        detail = UiKit.Rect("Entry", area).Region(0.52f, 0f, 1f, 1f);
    }

    public override void OnShow()
    {
        JournalManager journal = JournalManager.Instance;
        if (journal != null)
            journal.EntryAdded += OnEntryAdded;
        Refresh();
    }

    public override void OnHide()
    {
        CommitNote();
        if (JournalManager.Instance != null)
            JournalManager.Instance.EntryAdded -= OnEntryAdded;
    }

    void OnDestroy()
    {
        if (JournalManager.Instance != null)
            JournalManager.Instance.EntryAdded -= OnEntryAdded;
    }

    void OnEntryAdded(JournalEntry entry) => Refresh();

    void SelectSection(JournalSection newSection)
    {
        CommitNote();
        section = newSection;
        selectedId = -1;
        Refresh();
    }

    void Select(int entryId)
    {
        CommitNote();
        selectedId = entryId;
        if (JournalManager.Instance != null)
            JournalManager.Instance.MarkRead(entryId);
        Refresh();
    }

    void Refresh()
    {
        RefreshTabs();
        RefreshEntries();
        RefreshDetail();
    }

    void RefreshTabs()
    {
        JournalManager journal = JournalManager.Instance;
        for (int i = 0; i < Sections.Length; i++)
        {
            int count = 0, unread = 0;
            if (journal != null)
            {
                foreach (JournalEntry entry in journal.InSection(Sections[i]))
                {
                    count++;
                    if (!entry.isRead)
                        unread++;
                }
            }

            string label = JournalManager.SectionName(Sections[i]);
            string counts = count > 0 ? $"  <color=#EDE3C799>{count}</color>" : "";
            string dot = unread > 0 ? "  <color=#C7D68C>●</color>" : "";
            sectionTabs[i].GetComponentInChildren<Text>().text = label + counts + dot;
            UiKit.SetSelected(sectionTabs[i], Sections[i] == section);
        }
    }

    // This section's entries, newest first.
    List<JournalEntry> SectionEntries()
    {
        var list = new List<JournalEntry>();
        if (JournalManager.Instance != null)
            list.AddRange(JournalManager.Instance.InSection(section));
        list.Sort((a, b) => a.day != b.day ? b.day.CompareTo(a.day) : b.id.CompareTo(a.id));
        return list;
    }

    void RefreshEntries()
    {
        UiKit.Clear(entryList);

        if (section == JournalSection.Notes)
            UiKit.Height(UiKit.Button(entryList, "New Note", "+  New Note", 21, NewNote), 48f);

        List<JournalEntry> entries = SectionEntries();
        if (entries.Count == 0)
        {
            string empty = section == JournalSection.Notes ? "No notes yet." : "Nothing recorded here yet.";
            UiKit.Height(UiKit.Text(entryList, "Empty", empty, 20, UiKit.Muted), 48f);
        }

        foreach (JournalEntry entry in entries)
        {
            int id = entry.id;
            string title = string.IsNullOrEmpty(entry.title) ? "Untitled" : entry.title;
            string unread = entry.isRead ? "" : "<color=#C7D68C>●</color> ";
            string label = $"{unread}{title}\n<size=16><color=#EDE3C799>{FormatDay(entry.day)}</color></size>";
            Button row = UiKit.Height(UiKit.Button(entryList, title, label, 20, () => Select(id), TextAnchor.MiddleLeft), 62f);
            UiKit.SetSelected(row, id == selectedId);
        }
    }

    void RefreshDetail()
    {
        UiKit.Clear(detail);
        noteTitle = noteBody = null;
        deleteButton = null;
        confirmingDelete = false;

        JournalEntry entry = JournalManager.Instance != null ? JournalManager.Instance.Get(selectedId) : null;
        if (entry == null || entry.section != section)
        {
            string prompt = SectionEntries().Count > 0 ? "Select an entry to read it." : "";
            UiKit.Text(detail, "Prompt", prompt, 21, UiKit.Muted, TextAnchor.UpperLeft).rectTransform.Fill(8f, 0f, 0f, 8f);
            return;
        }

        Text meta = UiKit.Text(detail, "Meta", $"{FormatDay(entry.day)}  ·  {TypeName(entry.type)}", 18, UiKit.Muted);
        Top(meta.rectTransform, 0f, 28f);

        if (entry.IsPlayerNote)
        {
            BuildNoteEditor(entry);
            return;
        }

        Text title = UiKit.Text(detail, "Title", entry.title, 30, UiKit.Cream, TextAnchor.UpperLeft, FontStyle.Bold);
        Top(title.rectTransform, 32f, 80f);

        RectTransform body = UiKit.ScrollList(detail, "Body");
        ((RectTransform)body.parent).Fill(0f, 0f, 0f, 120f);
        string text = string.IsNullOrEmpty(entry.body) ? "<color=#EDE3C799>No further details.</color>" : entry.body;
        Text bodyText = UiKit.Text(body, "Text", text, 22, UiKit.Cream, TextAnchor.UpperLeft);
        bodyText.verticalOverflow = VerticalWrapMode.Overflow;
    }

    void BuildNoteEditor(JournalEntry entry)
    {
        noteTitle = UiKit.Input(detail, "Title", "Title", 26, false);
        noteTitle.text = entry.title;
        Top((RectTransform)noteTitle.transform, 34f, 52f);
        noteTitle.onEndEdit.AddListener(_ => CommitNote());

        noteBody = UiKit.Input(detail, "Body", "Write your note…", 22, true);
        noteBody.text = entry.body;
        ((RectTransform)noteBody.transform).Fill(0f, 64f, 0f, 98f);
        noteBody.onEndEdit.AddListener(_ => CommitNote());

        deleteButton = UiKit.Button(detail, "Delete", "Delete note", 20, DeleteNote);
        var rt = (RectTransform)deleteButton.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 0f);
        rt.sizeDelta = new Vector2(220f, 48f);
        rt.anchoredPosition = Vector2.zero;

        Text saved = UiKit.Text(detail, "Saved", "Saved when you click away or close the journal.", 18, UiKit.Muted);
        saved.rectTransform.anchorMin = new Vector2(0f, 0f);
        saved.rectTransform.anchorMax = new Vector2(0.6f, 0f);
        saved.rectTransform.offsetMin = Vector2.zero;
        saved.rectTransform.offsetMax = new Vector2(0f, 48f);
    }

    void NewNote()
    {
        if (JournalManager.Instance == null)
            return;

        CommitNote();
        JournalEntry note = JournalManager.Instance.AddNote("New note", "");
        section = note.section;
        selectedId = note.id;
        Refresh();
        if (noteBody != null)
            noteTitle.ActivateInputField();
    }

    // Writes the editor's text back to the note, if it changed.
    void CommitNote()
    {
        JournalManager journal = JournalManager.Instance;
        if (journal == null || noteTitle == null || noteBody == null)
            return;

        JournalEntry entry = journal.Get(selectedId);
        if (entry == null || !entry.IsPlayerNote || (entry.title == noteTitle.text && entry.body == noteBody.text))
            return;

        journal.EditNote(entry.id, noteTitle.text, noteBody.text);
        RefreshEntries(); // the title may have changed; the editor itself stays as it is
    }

    // Two clicks, so a note can't be lost to a stray click.
    void DeleteNote()
    {
        if (!confirmingDelete)
        {
            confirmingDelete = true;
            deleteButton.GetComponentInChildren<Text>().text = "Click again to delete";
            return;
        }

        if (JournalManager.Instance != null)
            JournalManager.Instance.DeleteNote(selectedId);
        noteTitle = noteBody = null;
        selectedId = -1;
        Refresh();
    }

    static string FormatDay(int day) => TimeManager.Instance != null ? TimeManager.Instance.FormatDate(day) : $"Day {day + 1}";

    static string TypeName(JournalEntryType type)
    {
        switch (type)
        {
            case JournalEntryType.Discovery: return "Discovery";
            case JournalEntryType.Observation: return "Observation";
            case JournalEntryType.Milestone: return "Milestone";
            default: return "Your note";
        }
    }

    static void Top(RectTransform rt, float top, float height)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.offsetMin = new Vector2(0f, -top - height);
        rt.offsetMax = new Vector2(0f, -top);
    }
}
