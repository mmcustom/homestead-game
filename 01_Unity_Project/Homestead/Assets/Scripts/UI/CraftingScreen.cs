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
//     (WoodManager.Buildable), and the Small Cabin site, which becomes "Complete Small Cabin" once a site is in reach.
//     Costs, the Hammer requirement, placement and the CanBuildPile / CanCompleteCabin reasons are unchanged — this
//     screen only asks them. A successful Build closes the screen, since the new structure is placed in front of the
//     player and they should see it.
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
    string lastSignature;
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

        list = UiKit.ScrollList(left, "List");
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

            var trigger = row.gameObject.AddComponent<EventTrigger>();
            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(_ => { hoverKey = captured.key; UpdateDetail(); });
            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => { if (hoverKey == captured.key) { hoverKey = null; UpdateDetail(); } });
            trigger.triggers.Add(enter);
            trigger.triggers.Add(exit);
            rows[entry.key] = row;
        }
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
        if (message == null && chosen != null && mode == Mode.Build && !chosen.can && !string.IsNullOrEmpty(chosen.reason))
            message = $"<color=#E67350>{chosen.reason}</color>";
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
                info = "Placed just in front of you.",
                cost = $"{fires.FirewoodToBuild} Firewood",
                actionLabel = "Build",
                failHeading = "Can't build a Campfire",
            };
            e.can = fires.CanBuild(player, out _, out e.reason);
            e.sets.Add(new List<Req> { new Req { label = ItemName(FireManager.FirewoodId), have = inventory != null ? inventory.Player.Count(FireManager.FirewoodId) : 0, need = fires.FirewoodToBuild } });
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
            info = "Placed just in front of you.",
            cost = wood.CostText(kind),
            actionLabel = "Build",
            failHeading = $"Can't build {WoodManager.PileName(kind)}",
        };

        // The Small Cabin site's entry does double duty: place an (empty, free) site normally, or — with an unfinished
        // site in reach — complete it into the real thing. While a site is in reach it's the completion entry even when
        // it can't be used yet, so the screen says what's still missing; "you already have a site" is only for one that's
        // out of reach.
        WoodPileState site = kind == PileKind.CabinSite ? wood.SiteInReach(player) : null;
        if (site != null)
        {
            e.name = "Complete Small Cabin";
            e.actionLabel = "Complete Small Cabin";
            e.failHeading = "Can't complete Small Cabin";
            e.info = "Finishes the site you are standing by.";
            e.can = wood.CanCompleteCabin(player, out _, out e.reason);
            e.cost = e.can ? "fully stocked" : "still needs " + (wood.SiteNeeds(site) ?? "the Hammer");
            var reqs = new List<Req>();
            foreach (string itemId in WoodManager.CabinMaterialIds)
                reqs.Add(new Req { label = ItemName(itemId), have = site.Count(itemId), need = wood.CabinRequired(itemId) });
            e.sets.Add(reqs);
            e.notes.Add(("Deposited at the site (R); the figures above are what it holds.", 0));
            bool hammer = WoodManager.HammerEquipped;
            e.notes.Add((hammer ? "Hammer equipped" : "Equip the Hammer to build it", hammer ? 1 : 2));
            e.act = () => DoCompleteCabin(wood);
            return e;
        }

        e.can = wood.CanBuildPile(kind, player, out _, out e.reason);
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
        else
        {
            var reqs = new List<Req>();
            foreach (WoodStack c in wood.CostOf(kind))
                reqs.Add(new Req { label = ItemName(c.itemId), have = inventory != null ? inventory.Player.Count(c.itemId) : 0, need = c.count });
            if (reqs.Count > 0)
                e.sets.Add(reqs);
        }
        e.act = () => DoBuildPile(wood, kind);
        return e;
    }

    void DoBuildCampfire(FireManager fires)
    {
        if (!fires.CanBuild(player, out _, out string why))
        {
            banner.NotifyCant("Can't build a Campfire", why);
            return;
        }
        if (!fires.Build(player))
            return;
        banner.Hide();
        // (Using up the Firewood already plays the item-drop sound.) Back to the world, so the player sees what they built.
        Close();
    }

    void DoBuildPile(WoodManager wood, PileKind kind)
    {
        if (!wood.CanBuildPile(kind, player, out _, out string why))
        {
            banner.NotifyCant($"Can't build {WoodManager.PileName(kind)}", why);
            return;
        }
        if (!wood.BuildPile(kind, player))
            return;
        banner.Hide();
        ToolStatus.Flash($"{WoodManager.PileName(kind)} built — {PileHelp(kind)}");
        Close();
    }

    void DoCompleteCabin(WoodManager wood)
    {
        if (!wood.CanCompleteCabin(player, out _, out string why))
        {
            banner.NotifyCant("Can't complete Small Cabin", why);
            return;
        }
        if (!wood.CompleteCabin(player))
            return;
        banner.Hide();
        ToolStatus.Flash("Small Cabin built — you can sleep in it, and its hearth is ready for Firewood.");
        Close();
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

    const string CampfireHelp = "builds just in front of you. Light it with Flint and Steel, or a Bow Drill.";

    static string PileHelp(PileKind kind)
    {
        switch (kind)
        {
            case PileKind.RockStorage: return "store Stone (R), take it back (E).";
            case PileKind.WaterBarrel: return "pour water in from the Bucket (R), fill the Bucket from it (E). Holds 40 L.";
            case PileKind.FoodCache: return "store food (R), take it back (E).";
            case PileKind.StorageBin: return "store Cordage, hides, furs, arrows and the like (R), take them back (E).";
            case PileKind.ToolRack: return "store Tools you're not carrying for a trip (R), take them back (E).";
            case PileKind.Tent: return "sleep in it (E), pack it up again (R). Keeps off rain, wind and much of the cold.";
            case PileKind.TarpShelter: return "sleep under it (E), take it down for the Tarp and half the Sticks back (R). Keeps off most rain and half the wind, and a little cold. No Hammer needed.";
            case PileKind.LeanTo: return "sleep in it (E), take it down (R). Keeps off most rain and wind, and some cold.";
            case PileKind.Cabin: return "sleep in it (E) — permanent, the best shelter yet. Comes with a hearth to warm up and cook at.";
            case PileKind.CabinSite: return "an empty building site — deposit Logs, Branches, Tall Grass, Stone and Clay into it (R) over however many trips it takes, take any of it back any time (E). Fully stocked, come back here and Complete Small Cabin (needs the Hammer equipped). Placed one by mistake? E on it when it's empty, or R with nothing to store, takes it down — no Hammer needed.";
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
