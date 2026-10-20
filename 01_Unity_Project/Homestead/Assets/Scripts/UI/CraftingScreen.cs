using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Crafting_System.md's Craft and Build screen (approved 2026-10-09, key B; also a button on the Inventory screen). Two
// top tabs share one layout: category tabs and a scrolling list on the left, the selected entry on the right with its
// description, what it makes or needs, a have / need line per ingredient (green when enough, red when short) and an
// action button.
//   Craft — every recipe in Crafting.Recipes, grouped into six categories, the ones makeable right now listed first.
//     Arrows and Cordage have a x1 / x5 / Max quantity. Crafting stays instant, with no timer, skill or station.
//   Build — what the Inventory screen's Build area held: the Campfire (FireManager), the shelters and storage piles
//     (WoodManager.Buildable), and the Small Cabin site, which you finish by pounding it with the Hammer at the site.
//     Costs, the Hammer requirement, placement and the CanBuildPile / CanCompleteCabin reasons are unchanged — this
//     screen only asks them. Build closes the screen and starts Placement Mode (PlacementMode), which shows a ghost where the
//     structure will go and builds it only when the player confirms.
// Clicking an action that can't go ahead shows the Inventory screen's missing-materials banner (ScreenBanner). Hovering
// a list row shows its detail in the right panel in place of the selected entry's. The lists refresh live as the pack
// changes and a few times a second for things that move (reach to a cabin site, the equipped Hammer).
public class CraftingScreen : GameScreen
{
    public override string Title => "Craft and Build";

    enum Mode { Craft, Build }

    static readonly string[] CraftCategories =
        { "Tools", "Weapons and Ammo", "Fire and Light", "Traps and Fishing", "Containers and Storage", "Materials" };
    static readonly string[] BuildCategories = { "Shelter", "Storage and Piles", "Other" };

    const float RowHeight = 56f;
    const float RefreshSeconds = 0.25f;
    static readonly Color Good = new Color(0.56f, 0.82f, 0.42f);
    static readonly Color Bad = new Color(0.9f, 0.45f, 0.3f);

    struct Req
    {
        public string label;
        public int have, need;
    }

    // One row of either tab, rebuilt from the game state on every refresh.
    class Entry
    {
        public string key, name, category, help, info, cost, actionLabel, failHeading;
        public bool can, multi;
        public string reason;                              // why not, for Build (Craft's is its have / need lines)
        public List<List<Req>> sets = new List<List<Req>>(); // alternatives; one set for most
        public List<(string text, int state)> notes = new List<(string, int)>(); // state: 0 plain, 1 ok, 2 short
        public Crafting.Recipe recipe;
        public int maxCrafts;
        public System.Action act;
    }

    ScreenBanner banner = new ScreenBanner();
    Button[] modeButtons = new Button[2];
    Button[] categoryButtons = new Button[6];
    Text[] categoryLabels = new Text[6];
    RectTransform list;
    Text nameLabel, helpLabel, infoLabel, reqHeading, reqLabel, reasonLabel;
    Button[] quantityButtons = new Button[3];
    Button actionButton;
    Text actionLabel;

    Mode mode = Mode.Craft;
    readonly int[] categoryIndex = new int[2];
    readonly string[] selectedKey = new string[2];
    int quantity; // 0 = x1, 1 = x5, 2 = Max
    string hoverKey;
    string lastSignature, lastVisibleMarker;
    List<Entry> allEntries = new List<Entry>();
    List<Entry> shown = new List<Entry>();
    readonly Dictionary<string, Button> rows = new Dictionary<string, Button>();
    PlayerController player;
    InventoryManager watched;
    float nextRefresh;
    string notice;
    float noticeUntil;

    // --- Building the layout ---

    public override void Build(RectTransform area)
    {
        // The two top tabs.
        string[] modeNames = { "Craft", "Build" };
        for (int i = 0; i < 2; i++)
        {
            var chosen = (Mode)i;
            Button tab = UiKit.Button(area, modeNames[i] + " Mode", modeNames[i], 26, () => SetMode(chosen));
            var rt = (RectTransform)tab.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(190f, 48f);
            rt.anchoredPosition = new Vector2(i * 200f, 0f);
            modeButtons[i] = tab;
        }

        RectTransform left = UiKit.Rect("Left", area);
        left.anchorMin = Vector2.zero;
        left.anchorMax = new Vector2(0.46f, 1f);
        left.offsetMin = Vector2.zero;
        left.offsetMax = new Vector2(0f, -60f);

        RectTransform right = UiKit.Rect("Right", area);
        right.anchorMin = new Vector2(0.48f, 0f);
        right.anchorMax = Vector2.one;
        right.offsetMin = Vector2.zero;
        right.offsetMax = new Vector2(0f, -60f);

        // Category tabs: three to a row, two rows (the Build tab only uses the first few).
        for (int i = 0; i < categoryButtons.Length; i++)
        {
            int index = i;
            Button button = UiKit.Button(left, "Category " + i, "", 16, () => SetCategory(index));
            var rt = (RectTransform)button.transform;
            int column = i % 3, row = i / 3;
            rt.anchorMin = new Vector2(column / 3f, 1f);
            rt.anchorMax = new Vector2((column + 1) / 3f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(column == 0 ? 0f : 2f, -row * 50f - 46f);
            rt.offsetMax = new Vector2(column == 2 ? 0f : -2f, -row * 50f);
            categoryLabels[i] = button.GetComponentInChildren<Text>();
            categoryLabels[i].rectTransform.Fill(4f, 0f, 4f, 0f);
            categoryLabels[i].lineSpacing = 0.9f;
            categoryButtons[i] = button;
        }

        list = UiKit.ScrollList(left, "List", withScrollbar: true);
        ((RectTransform)list.parent).Fill(0f, 0f, 0f, 108f);

        // The selected entry.
        nameLabel = UiKit.Text(right, "Name", "", 36, UiKit.Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        Top(nameLabel.rectTransform, 0f, 46f);
        helpLabel = UiKit.Text(right, "Description", "", 20, UiKit.Cream, TextAnchor.UpperLeft);
        Top(helpLabel.rectTransform, 50f, 84f);
        infoLabel = UiKit.Text(right, "Info", "", 19, UiKit.Muted, TextAnchor.MiddleLeft);
        Top(infoLabel.rectTransform, 136f, 28f);
        reqHeading = UiKit.Text(right, "Needs Heading", "", 24, UiKit.Accent, TextAnchor.MiddleLeft, FontStyle.Italic);
        Top(reqHeading.rectTransform, 172f, 32f);
        reqLabel = UiKit.Text(right, "Needs", "", 23, UiKit.Cream, TextAnchor.UpperLeft);
        reqLabel.lineSpacing = 1.1f;
        Top(reqLabel.rectTransform, 208f, 300f);

        // Reason or notice, the quantity row and the action button, from the bottom.
        reasonLabel = UiKit.Text(right, "Reason", "", 19, UiKit.Muted, TextAnchor.LowerLeft);
        reasonLabel.rectTransform.anchorMin = Vector2.zero;
        reasonLabel.rectTransform.anchorMax = new Vector2(1f, 0f);
        reasonLabel.rectTransform.pivot = new Vector2(0.5f, 0f);
        reasonLabel.rectTransform.offsetMin = new Vector2(0f, 116f);
        reasonLabel.rectTransform.offsetMax = new Vector2(0f, 172f);

        string[] quantityNames = { "x1", "x5", "Max" };
        for (int i = 0; i < quantityButtons.Length; i++)
        {
            int index = i;
            Button q = UiKit.Button(right, "Quantity " + quantityNames[i], quantityNames[i], 22, () => { quantity = index; Refresh(); });
            var rt = (RectTransform)q.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 0f);
            rt.sizeDelta = new Vector2(120f, 44f);
            rt.anchoredPosition = new Vector2(i * 128f, 66f);
            quantityButtons[i] = q;
        }

        actionButton = UiKit.Button(right, "Action", "", 28, OnAction);
        var actionRt = (RectTransform)actionButton.transform;
        actionRt.anchorMin = Vector2.zero;
        actionRt.anchorMax = new Vector2(1f, 0f);
        actionRt.pivot = new Vector2(0.5f, 0f);
        actionRt.offsetMin = Vector2.zero;
        actionRt.offsetMax = new Vector2(0f, 56f);
        actionLabel = actionButton.GetComponentInChildren<Text>();

        banner.Build(area); // last, so it draws over everything else
    }

    // A full-width element a fixed distance from the parent's top.
    static void Top(RectTransform rt, float top, float height)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.offsetMin = new Vector2(0f, -top - height);
        rt.offsetMax = new Vector2(0f, -top);
    }

    // --- Showing and refreshing ---

    public override void OnShow()
    {
        player = FindAnyObjectByType<PlayerController>();
        watched = InventoryManager.Instance;
        if (watched != null)
        {
            watched.Player.Changed += Refresh;
            watched.EquippedToolChanged += OnEquippedChanged;
        }
        hoverKey = null;
        notice = null;
        lastSignature = null;
        Refresh();
    }

    public override void OnHide()
    {
        banner.Hide();
        Unsubscribe();
    }

    void OnDestroy() => Unsubscribe();

    void Unsubscribe()
    {
        if (watched == null)
            return;
        watched.Player.Changed -= Refresh;
        watched.EquippedToolChanged -= OnEquippedChanged;
        watched = null;
    }

    void OnEquippedChanged(ItemDefinition tool) => Refresh();

    void Update()
    {
        banner.Tick();
        if (watched == null)
            return;
        if (notice != null && Time.unscaledTime >= noticeUntil)
        {
            notice = null;
            Refresh();
        }
        else if (Time.unscaledTime >= nextRefresh)
        {
            Refresh();
        }
    }

    void SetMode(Mode next)
    {
        if (mode == next)
            return;
        mode = next;
        hoverKey = null;
        lastSignature = null;
        banner.Hide();
        Refresh();
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
    }

    void SetCategory(int index)
    {
        categoryIndex[(int)mode] = index;
        hoverKey = null;
        lastSignature = null;
        Refresh();
    }

    string[] Categories => mode == Mode.Craft ? CraftCategories : BuildCategories;

    void Refresh()
    {
        nextRefresh = Time.unscaledTime + RefreshSeconds;
        if (nameLabel == null)
            return;

        allEntries = mode == Mode.Craft ? CraftEntries() : BuildEntries();
        string[] categories = Categories;
        int m = (int)mode;
        categoryIndex[m] = Mathf.Clamp(categoryIndex[m], 0, categories.Length - 1);

        for (int i = 0; i < categoryButtons.Length; i++)
        {
            bool used = i < categories.Length;
            categoryButtons[i].gameObject.SetActive(used);
            if (!used)
                continue;
            int ready = 0;
            foreach (Entry e in allEntries)
                if (e.category == categories[i] && e.can)
                    ready++;
            categoryLabels[i].text = ready > 0 ? $"{categories[i]}  <color=#C7D68C>({ready})</color>" : categories[i];
            UiKit.SetSelected(categoryButtons[i], i == categoryIndex[m]);
        }
        for (int i = 0; i < modeButtons.Length; i++)
            UiKit.SetSelected(modeButtons[i], i == m);

        // This category's entries, the ones that can be done right now first.
        shown = allEntries.FindAll(e => e.category == categories[categoryIndex[m]]);
        var ordered = new List<Entry>();
        ordered.AddRange(shown.FindAll(e => e.can));
        ordered.AddRange(shown.FindAll(e => !e.can));
        shown = ordered;

        if (!shown.Exists(e => e.key == selectedKey[m]))
            selectedKey[m] = shown.Count > 0 ? shown[0].key : null;

        string signature = string.Join("|", shown.ConvertAll(e => e.key + (e.can ? "+" : "-") + e.name + e.cost)) + "#" + m + categoryIndex[m];
        if (signature != lastSignature)
        {
            lastSignature = signature;
            RebuildRows();
        }
        foreach (Entry e in shown)
            if (rows.TryGetValue(e.key, out Button row))
                UiKit.SetSelected(row, e.key == selectedKey[m]);
        KeepSelectedVisible(m);

        UpdateDetail();
    }

    void RebuildRows()
    {
        UiKit.Clear(list);
        rows.Clear();
        hoverKey = null;
        if (shown.Count == 0)
            UiKit.Height(UiKit.Text(list, "Empty", "Nothing here.", 21, UiKit.Muted), 50f);
        foreach (Entry entry in shown)
        {
            Entry captured = entry;
            Button row = UiKit.Button(list, entry.key, "", 20, () => Select(captured.key));
            UiKit.Height(row, RowHeight);
            Destroy(row.GetComponentInChildren<Text>().gameObject);
            var rt = (RectTransform)row.transform;
            Text nameText = UiKit.Text(rt, "Name", entry.name, 22, UiKit.Cream, TextAnchor.MiddleLeft);
            nameText.horizontalOverflow = HorizontalWrapMode.Overflow;
            nameText.rectTransform.anchorMin = Vector2.zero;
            nameText.rectTransform.anchorMax = new Vector2(0.52f, 1f);
            nameText.rectTransform.offsetMin = new Vector2(12f, 0f);
            nameText.rectTransform.offsetMax = Vector2.zero;
            Text costText = UiKit.Text(rt, "Cost", entry.cost, 15, UiKit.Muted, TextAnchor.MiddleRight);
            costText.rectTransform.anchorMin = new Vector2(0.4f, 0f);
            costText.rectTransform.anchorMax = Vector2.one;
            costText.rectTransform.offsetMin = Vector2.zero;
            costText.rectTransform.offsetMax = new Vector2(-12f, 0f);
            SetAvailable(row, entry.can);

            // Hover only: an EventTrigger here would swallow the mouse wheel and drags before the ScrollRect saw them.
            row.gameObject.AddComponent<RowHover>().Bind(
                () => { hoverKey = captured.key; UpdateDetail(); },
                () => { if (hoverKey == captured.key) { hoverKey = null; UpdateDetail(); } });
            rows[entry.key] = row;
        }
    }

    // Reports the pointer entering and leaving a list row, and nothing else, so scrolling passes through to the ScrollRect.
    class RowHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        System.Action enter, exit;

        public void Bind(System.Action onEnter, System.Action onExit)
        {
            enter = onEnter;
            exit = onExit;
        }

        public void OnPointerEnter(PointerEventData eventData) => enter?.Invoke();

        public void OnPointerExit(PointerEventData eventData) => exit?.Invoke();
    }

    // Scrolls the list just far enough that the selected row is in view; only when the selection, category or mode changes, so
    // it never fights the player's own scrolling.
    void KeepSelectedVisible(int m)
    {
        string marker = mode + "|" + categoryIndex[m] + "|" + selectedKey[m];
        if (marker == lastVisibleMarker)
            return;

        int index = shown.FindIndex(e => e.key == selectedKey[m]);
        var viewport = (RectTransform)list.parent;
        if (index < 0 || viewport.rect.height <= 0f)
            return; // not laid out yet: try again next refresh
        lastVisibleMarker = marker;

        float top = 6f + index * (RowHeight + 4f);
        float bottom = top + RowHeight + 6f;
        float y = list.anchoredPosition.y;
        if (top < y)
            y = top - 6f;
        else if (bottom > y + viewport.rect.height)
            y = bottom - viewport.rect.height;
        float contentHeight = 12f + shown.Count * (RowHeight + 4f) - 4f;
        list.anchoredPosition = new Vector2(0f, Mathf.Clamp(y, 0f, Mathf.Max(0f, contentHeight - viewport.rect.height)));
    }

    void Select(string key)
    {
        selectedKey[(int)mode] = key;
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
        Refresh();
    }

    Entry Shown()
    {
        string key = hoverKey ?? selectedKey[(int)mode];
        return shown.Find(e => e.key == key);
    }

    Entry Selected() => shown.Find(e => e.key == selectedKey[(int)mode]);

    void UpdateDetail()
    {
        Entry e = Shown();
        Entry chosen = Selected();
        bool none = e == null;
        nameLabel.text = none ? "" : e.name;
        helpLabel.text = none ? "" : e.help;
        infoLabel.text = none ? "" : e.info;
        reqHeading.text = none ? "" : (e.sets.Count > 0 || e.notes.Count > 0 ? (mode == Mode.Craft ? "Needs  (have / need)" : "Needs") : "");
        reqLabel.text = none ? "" : Requirements(e);

        string message = Notice;
        if (message == null && e != null && mode == Mode.Build && !e.can && !string.IsNullOrEmpty(e.reason))
            message = $"<color=#E67350>{e.reason}</color>";
        reasonLabel.text = message ?? "";

        // The action works on the selected entry (hovering another only previews it).
        bool hasAction = chosen != null;
        actionButton.gameObject.SetActive(hasAction);
        bool showQuantity = hasAction && chosen.multi;
        for (int i = 0; i < quantityButtons.Length; i++)
        {
            quantityButtons[i].gameObject.SetActive(showQuantity);
            UiKit.SetSelected(quantityButtons[i], i == quantity);
            if (i == 2 && showQuantity)
                quantityButtons[i].GetComponentInChildren<Text>().text = chosen.maxCrafts > 1 ? $"Max ({chosen.maxCrafts})" : "Max";
        }
        if (hasAction)
        {
            SetAvailable(actionButton, chosen.can);
            actionLabel.text = chosen.multi ? $"{chosen.actionLabel}  x{CraftTimes(chosen)}" : chosen.actionLabel;
        }
    }

    // How many crafts the button will make: the chosen quantity, no more than the materials allow (at least one, so the
    // button still reads sensibly while it's greyed).
    int CraftTimes(Entry e)
    {
        int want = quantity == 0 ? 1 : quantity == 1 ? 5 : 99;
        return Mathf.Max(1, Mathf.Min(want, e.maxCrafts));
    }

    string Notice => notice != null && Time.unscaledTime < noticeUntil ? notice : null;

    void Tell(string message, float seconds = 4f)
    {
        notice = message;
        noticeUntil = Time.unscaledTime + seconds;
        UpdateDetail();
    }

    static string Requirements(Entry e)
    {
        var text = new System.Text.StringBuilder();
        for (int s = 0; s < e.sets.Count; s++)
        {
            if (e.sets.Count > 1)
                text.Append(s == 0 ? "<color=#EDE3C799>Either</color>\n" : "<color=#EDE3C799>or</color>\n");
            foreach (Req r in e.sets[s])
                text.Append($"<color={(r.have >= r.need ? "#8FD16A" : "#E67350")}>{r.label}   {r.have} / {r.need}</color>\n");
        }
        foreach ((string note, int state) in e.notes)
        {
            string color = state == 1 ? "#8FD16A" : state == 2 ? "#E67350" : "#EDE3C799";
            text.Append($"<color={color}>{note}</color>\n");
        }
        return text.ToString().TrimEnd('\n');
    }

    // Craft and Build buttons stay clickable when they can't be used — a click explains why — so "unavailable" is shown
    // by dimming rather than by Button.interactable, which would swallow the click.
    static void SetAvailable(Button button, bool available)
    {
        button.interactable = true;
        CanvasGroup group = button.GetComponent<CanvasGroup>();
        if (group == null)
            group = button.gameObject.AddComponent<CanvasGroup>();
        group.alpha = available ? 1f : 0.45f;
    }

    // --- Craft entries ---

    static string CraftCategory(string outputId)
    {
        switch (outputId)
        {
            case "stone_pick_axe":
            case "shovel":
            case "primitive_axe":
            case "knife":
            case "hammer": return CraftCategories[0];
            case "primitive_bow":
            case "arrows": return CraftCategories[1];
            case "bow_drill":
            case "torch":
            case "lantern": return CraftCategories[2];
            case "rabbit_snare":
            case "box_trap":
            case "fish_trap": return CraftCategories[3];
            case "pouch": return CraftCategories[4];
            default: return CraftCategories[5]; // cordage, and anything added later
        }
    }

    static string ItemName(string itemId) => ItemDatabase.Get(itemId)?.DisplayName ?? itemId;

    List<Entry> CraftEntries()
    {
        var entries = new List<Entry>();
        InventoryManager inventory = InventoryManager.Instance;
        foreach (Crafting.Recipe recipe in Crafting.Recipes)
        {
            Crafting.Recipe r = recipe;
            ItemDefinition item = ItemDatabase.Get(r.outputId);
            var e = new Entry
            {
                key = r.outputId,
                name = Crafting.OutputName(r),
                category = CraftCategory(r.outputId),
                help = Capitalize(CraftHelp(r.outputId)),
                info = item != null ? $"Makes {Mathf.Max(1, r.outputCount)}  ·  weighs {item.WeightKg * Mathf.Max(1, r.outputCount):0.0#} kg" : "",
                cost = Crafting.Cost(r),
                actionLabel = "Craft",
                failHeading = $"Can't make {Crafting.OutputName(r)}",
                recipe = r,
                multi = r.outputId == "arrows" || r.outputId == "cordage",
            };
            e.can = Crafting.CanCraft(r, out e.reason);
            e.maxCrafts = Crafting.MaxCrafts(r);
            foreach (Crafting.Ingredient[] set in Crafting.IngredientSets(r))
            {
                var reqs = new List<Req>();
                foreach (Crafting.Ingredient i in set)
                    reqs.Add(new Req { label = ItemName(i.itemId), have = inventory != null ? inventory.Player.Count(i.itemId) : 0, need = i.quantity });
                e.sets.Add(reqs);
            }
            e.act = () => DoCraft(e);
            entries.Add(e);
        }
        return entries;
    }

    void DoCraft(Entry e)
    {
        if (!e.can)
        {
            banner.NotifyCant(e.failHeading, e.reason);
            return;
        }

        banner.Hide();
        Crafting.LeftOnGround = 0;
        int made = Crafting.Craft(e.recipe, CraftTimes(e));
        if (made <= 0)
            return;
        int total = made * Mathf.Max(1, e.recipe.outputCount);
        string itemName = ItemName(e.recipe.outputId);
        string text = total == 1 ? $"Made a {itemName}." : $"Made {total} {itemName}.";
        if (Crafting.LeftOnGround > 0)
            text += $" {Crafting.LeftOnGround} left on the ground — no room in the pack.";
        Tell($"<color=#C7D68C>{text}</color>");
        Refresh();
    }

    static string Capitalize(string text) => string.IsNullOrEmpty(text) ? text : char.ToUpper(text[0]) + text.Substring(1);

    // --- Build entries ---

    static string BuildCategory(PileKind kind)
    {
        switch (kind)
        {
            case PileKind.TarpShelter:
            case PileKind.Tent:
            case PileKind.LeanTo:
            case PileKind.CabinSite:
            case PileKind.Cabin: return BuildCategories[0];
            default: return BuildCategories[1];
        }
    }

    List<Entry> BuildEntries()
    {
        var entries = new List<Entry>();
        WoodManager wood = WoodManager.Instance;
        FireManager fires = FireManager.Instance;
        InventoryManager inventory = InventoryManager.Instance;

        if (wood != null)
        {
            // Listed in WoodManager.Buildable's order within each category.
            foreach (PileKind kind in WoodManager.Buildable)
                entries.Add(PileEntry(wood, inventory, kind));
        }
        if (fires != null)
        {
            var e = new Entry
            {
                key = "campfire",
                name = "Campfire",
                category = BuildCategories[2],
                help = Capitalize(CampfireHelp),
                info = "You choose the spot: a ghost shows where it will go.",
                cost = $"{fires.FirewoodToBuild} Firewood / {fires.CampfireSticksCost} Sticks",
                actionLabel = "Build",
                failHeading = "Can't build a Campfire",
            };
            e.can = fires.CanStartBuild(player, out e.reason);
            e.sets.Add(new List<Req> { new Req { label = ItemName(FireManager.FirewoodId), have = inventory != null ? inventory.Player.Count(FireManager.FirewoodId) : 0, need = fires.FirewoodToBuild } });
            e.sets.Add(new List<Req> { new Req { label = ItemName(WoodManager.SticksId), have = inventory != null ? inventory.Player.Count(WoodManager.SticksId) : 0, need = fires.CampfireSticksCost } });
            e.act = () => DoBuildCampfire(fires);
            entries.Add(e);
        }
        return entries;
    }

    Entry PileEntry(WoodManager wood, InventoryManager inventory, PileKind kind)
    {
        var e = new Entry
        {
            key = "pile:" + kind,
            name = WoodManager.PileName(kind),
            category = BuildCategory(kind),
            help = Capitalize(PileHelp(kind)),
            info = "You choose the spot: a ghost shows where it will go.",
            cost = wood.CostText(kind),
            actionLabel = "Build",
            failHeading = $"Can't build {WoodManager.PileName(kind)}",
        };

        e.can = wood.CanStartBuild(kind, player, out e.reason);
        if (kind == PileKind.CabinSite)
        {
            e.actionLabel = "Place Site";
            e.info = "Free to place. Then deposit the materials at the site (R).";
            var required = new List<Req>();
            foreach (string itemId in WoodManager.CabinMaterialIds)
                required.Add(new Req { label = ItemName(itemId), have = 0, need = wood.CabinRequired(itemId) });
            // Shown as a plain list of what the finished cabin takes, not as a have / need check: nothing is spent now.
            foreach (Req r in required)
                e.notes.Add(($"{r.label}   {r.need}", 0));
            e.notes.Insert(0, ("The cabin needs, in total:", 0));
        }
        else if (WellsAndOuthouses.IsStagedSite(kind))
        {
            // Too heavy for one trip: free to place, then stocked over as many trips as it takes and pounded into shape with the Hammer.
            e.actionLabel = "Place Site";
            e.info = "Free to place, no Hammer. Then deliver the materials at the site (R), over as many trips as it takes.";
            PileKind finished = WellsAndOuthouses.FinalKind(kind);
            if (kind == PileKind.KitchenSite)
            {
                // Two stages, each delivered and then pounded in turn: the hearth, then the roof.
                e.notes.Add(("Stage 1, the hearth (Stone first, then Clay):", 0));
                foreach (WoodStack c in WellsAndOuthouses.KitchenHearthCost())
                    e.notes.Add(($"{ItemName(c.itemId)}   {c.count}", 0));
                e.notes.Add(("Stage 2, the roof (after the hearth is built):", 0));
                foreach (WoodStack c in WellsAndOuthouses.KitchenRoofCost())
                    e.notes.Add(($"{ItemName(c.itemId)}   {c.count}", 0));
                e.notes.Add(("Pound each stage with the Hammer (hold click) to build it.", 0));
            }
            else
            {
                e.notes.Add(($"The {WoodManager.PileName(finished)} needs, in total:", 0));
                foreach (WoodStack c in WellsAndOuthouses.FinishedCost(kind))
                    e.notes.Add(($"{ItemName(c.itemId)}   {c.count}", 0));
                e.notes.Add(("Then " + (WellsAndOuthouses.RequiresDig(kind) ? "dig the shaft (Shovel) and " : "") + $"pound it with the Hammer (hold click) to build the {WoodManager.PileName(finished)}.", 0));
            }
        }
        else
        {
            var reqs = new List<Req>();
            foreach (WoodStack c in wood.CostOf(kind))
                reqs.Add(new Req { label = ItemName(c.itemId), have = inventory != null ? inventory.Player.Count(c.itemId) : 0, need = c.count });
            if (reqs.Count > 0)
                e.sets.Add(reqs);
        }
        if (WoodManager.NeedsHammerToBuild(kind))
        {
            // The ghost shows either way; without the Hammer it stays red and the click builds nothing.
            bool hammer = WoodManager.HammerEquipped;
            e.notes.Add((hammer ? "Hammer equipped" : "Equip the Hammer to build it (the ghost shows, but stays red)", hammer ? 1 : 2));
        }
        e.act = () => DoBuildPile(wood, kind);
        return e;
    }

    // Build no longer places anything here: it closes the screen and starts Placement Mode, which shows a ghost where the
    // structure will go and builds it (spending the materials) only when the player confirms.
    void DoBuildCampfire(FireManager fires)
    {
        if (!fires.CanStartBuild(player, out string why))
        {
            banner.NotifyCant("Can't build a Campfire", why);
            return;
        }
        banner.Hide();
        Close();
        PlacementMode.BeginCampfire();
    }

    void DoBuildPile(WoodManager wood, PileKind kind)
    {
        if (!wood.CanStartBuild(kind, player, out string why))
        {
            banner.NotifyCant($"Can't build {WoodManager.PileName(kind)}", why);
            return;
        }
        banner.Hide();
        Close();
        PlacementMode.Begin(kind);
    }

    void OnAction()
    {
        Entry e = Selected();
        if (e == null || e.act == null)
            return;
        if (!e.can && mode == Mode.Craft)
        {
            banner.NotifyCant(e.failHeading, e.reason);
            return;
        }
        e.act();
        if (this != null && gameObject.activeInHierarchy)
            Refresh();
    }

    void Close()
    {
        GameScreens screens = GetComponentInParent<GameScreens>();
        if (screens != null)
            screens.Close();
    }

    // --- Descriptions ---

    public const string CampfireHelp = "builds where you aim. Light it with Flint and Steel, or a Bow Drill.";

    public static string PileHelp(PileKind kind)
    {
        switch (kind)
        {
            case PileKind.RockStorage: return "store Stone (R), take it back (E).";
            case PileKind.WaterBarrel: return "pour water in from the Bucket (R), fill the Bucket from it (E). Holds 40 L.";
            case PileKind.RainBarrel: return "stand it under a Small Cabin's eave; it fills from the roof in rain (E draws water, R pours in). Holds 80 L.";
            case PileKind.FoodCache: return "store food (R), take it back (E).";
            case PileKind.StorageBin: return "store Cordage, hides, furs, arrows and the like (R), take them back (E).";
            case PileKind.ToolRack: return "store Tools you're not carrying for a trip (R), take them back (E).";
            case PileKind.Tent: return "sleep in it (E), pack it up again (R). Keeps off rain, wind and much of the cold.";
            case PileKind.TarpShelter: return "sleep under it (E), take it down for the Tarp and half the Sticks back (R). Keeps off most rain and half the wind, and a little cold. No Hammer needed.";
            case PileKind.LeanTo: return "sleep in it (E), take it down (R). Keeps off most rain and wind, and some cold.";
            case PileKind.Cabin: return "sleep in it (E) — permanent, the best shelter yet. Comes with a hearth to warm up and cook at.";
            case PileKind.WellSite: return "a hand-dug well, built in stages. Mark the site out (free, no Hammer), dig the shaft with the Shovel (hold click; it takes game time), then deliver 10 Stone for the lining and 2 Logs and 4 Branches for the cap with R over as many trips as it takes (no Hammer). Fully stocked, equip the Hammer and hold click on the site until the Well is built. Gives a steady supply that recovers by the hour, Good water when clear of the outhouse. Needs high, dry ground 30 m from any outhouse.";
            case PileKind.WaterBarrelSite: return "a hollowed-log barrel, built in stages because its Logs are heavy. Mark the site out (free, no Hammer), deliver 3 Logs with R over as many trips as it takes (no Hammer; E takes them back), then equip the Hammer and hold click on the site until the barrel is built (once you start, the Logs are built in). Holds 40 L.";
            case PileKind.RainBarrelSite: return "a rain barrel, built in stages because its Logs are heavy. Mark the site out under a Small Cabin's eave (it snaps there; free, no Hammer), deliver 3 Logs and 2 Branches with R over as many trips as it takes (no Hammer), then equip the Hammer and hold click on the site until the barrel is built (once you start, the wood is built in). The finished barrel stands where the site is and fills from the roof in rain. Holds 80 L.";
            case PileKind.FoodCacheSite: return "a lidded food cache, built in stages because its Logs are heavy. Mark the site out (free, no Hammer; turn it with the wheel), deliver 2 Logs and 4 Branches with R over as many trips as it takes (no Hammer), then equip the Hammer and hold click on the site until the cache is built (once you start, the wood is built in).";
            case PileKind.WoodShedSite: return "a roofed firewood rack with an open front, built in stages because its wood is heavy. Mark the site out (free, no Hammer; turn it with the wheel so the open front faces away from the wind), deliver 2 Logs and 8 Branches with R over as many trips as it takes (no Hammer), then equip the Hammer and hold click on the site until the shed is built (once you start, the wood is built in). Stores Sticks, Branches, Logs and Firewood (R, E), holds as much as a Wood Pile, and keeps the rain off: fresh wet wood dries in it over about a season, and dry wood burns longer than wet. Needs dry ground at least 3 m from the cabin.";
            case PileKind.WoodShed: return "store Sticks, Branches, Logs and Firewood (R), take them back (E). The roof keeps rain off, so wet wood dries in here over about a season.";
            case PileKind.KitchenSite: return "a roofed outdoor cooking place, built in two stages because its materials are heavy. Mark the site out (free, no Hammer; turn it with the wheel), deliver 4 Stone and then 2 Clay with R over as many trips as it takes, and equip the Hammer and hold click on the site until the hearth is built. Then deliver 2 Logs, 6 Branches and 4 Tall Grass for the plank roof and pound that too (once you start a stage, its materials are built in). The hearth is an ordinary campfire: Cooking, boiling water, Flint and Steel and the Bow Drill all work, and under the roof the Bow Drill catches as well as in calm weather, even in rain or snow. The roof also keeps rain and snow off you, shades you from the sun, and a meal eaten under it is a sheltered meal: Health recovers faster for a few hours. Needs dry ground; the roof edge stays 3 m clear of every other structure and every tree.";
            case PileKind.KitchenRoofSite: return "the hearth is built; deliver the roof materials (R) and pound the roof (Hammer, hold click).";
            case PileKind.Kitchen: return "a roof on posts over a stone hearth. Light and feed the fire as any campfire; cook and boil water there. Rain and snow stay off you under the roof, it shades the sun, and a meal eaten under it is a sheltered meal. It has four fixed slots inside for add-ons (Kitchen-Countertop, Kitchen-Shelves, Kitchen-Grill, Kitchen-Sink). R with the Hammer dismantles it (fire out, add-ons taken down first).";
            case PileKind.Countertop: return "a work surface about 0.9 m high that stands in one of the four fixed slots inside a finished Covered Outdoor Kitchen (the ghost snaps to the nearest free one). Equip the Hammer to build it; nothing to do at it yet. R with the Hammer takes it down for half its materials back.";
            case PileKind.Shelving: return "open shelves that stand in a slot of a finished Covered Outdoor Kitchen, under its roof. Store food and dry goods (R or E opens the transfer screen); every batch keeps its day. Equip the Hammer to build it; take it down from the transfer screen's Remove button when it is empty.";
            case PileKind.Grill: return "a stone-and-clay firebox with a flat stone top and a rack of green sticks, in a slot of a finished Covered Outdoor Kitchen. It comes with its own fire, a second campfire: light it with Flint and Steel or the Bow Drill, feed it Firewood, and cook meat and fish at it from the Inventory. Under the roof the Bow Drill catches as well as in calm weather. Equip the Hammer to build it; R with the Hammer takes it down when its fire is out.";
            case PileKind.SlopSink: return "a clay basin on a table that holds up to 20 L of water of any quality, in a slot of a finished Covered Outdoor Kitchen. R pours carried water in, E fills your Bucket or Canteen from it, and R with nothing to pour drains it onto the ground. Washing has no effect yet. Equip the Hammer to build it; R with the Hammer takes it down when it is empty.";
            case PileKind.OuthouseSite: return "a pit privy, built in stages because its Logs are heavy. Mark the site out (free, no Hammer; turn it with the wheel so the door faces where you want), deliver 2 Logs, 6 Branches and 3 Tall Grass with R over as many trips as it takes (no Hammer), then equip the Hammer and hold click on the site until the shack is built (once you start, the wood is built in). A working outhouse within 60 m of where you sleep makes the homestead sanitary: a little less illness and a little better rest. The pit fills over about a season; then dig a new one with the Shovel (E). Must stand 30 m from water and any well, 7.5 m from buildings, below any well.";
            case PileKind.CabinSite: return "an empty building site — deposit Logs, Branches, Tall Grass, Stone and Clay into it (R) over however many trips it takes, take any of it back any time (E). Fully stocked, equip the Hammer and hold click on the site until the cabin is built (once you start, the materials are built in). Placed one by mistake? With the Hammer equipped, E on it when it's empty, or R with nothing to store, takes it down.";
            default: return "store wood (R), take it back (E).";
        }
    }

    static string CraftHelp(string itemId)
    {
        switch (itemId)
        {
            case "stone_pick_axe": return "mines Stone from the rock outcrop on South Ridge.";
            case "shovel": return "digs out stumps, and Clay from creek and pond banks.";
            case "primitive_axe": return "fells trees and splits wood like the Axe, just slower.";
            case "knife": return "carried, it lets you field dress kills and take game from traps.";
            case "pouch": return "carried, it lets you carry 10 kg more.";
            case "cordage": return "twisted from whichever fibre you have.";
            case "hammer": return "equip it to build a Small Cabin, or to dismantle a finished structure for half its materials.";
            case "torch": return "equip it and click to light it (needs Flint and Steel, or stand by a burning Campfire). Burns about 3 hours, then it's gone.";
            case "bow_drill": return "equip it, face a Campfire with fuel and hold click to start a fire without Flint and Steel. Doesn't always catch.";
            case "primitive_bow": return "equip it, hold click to draw and release to shoot. Shorter range and less accurate than a Recurve Bow, but it's made from a Branch.";
            case "arrows": return "shoot them from either bow; a killing arrow can often be recovered when field dressing.";
            case "lantern": return "equip it and click to light it. Burns Lamp Oil (Trading Post) — refill it in the Inventory with the Refill button.";
            default: return "equip it to set it.";
        }
    }
}
