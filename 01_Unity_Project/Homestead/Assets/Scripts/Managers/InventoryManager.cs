using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct InventorySaveData
{
    public InventoryContainerData player;
    public string equippedItemId;
    public List<InventoryContainerData> storage;
    public List<string> hotkeys; // item id per slot, "" for empty; null in saves from before hotkeys
    public List<string> worn;    // item id per clothing slot (ClothingRules.Slot order), "" for empty; null in saves from before clothing
}

// Player inventory, storage containers, equipment (Unity_Architecture.md, Inventory_System.md).
// Storage contents live here rather than on the building objects so they persist across scenes and saves;
// a storage structure looks up its container with GetOrCreateStorage.
public class InventoryManager : MonoBehaviour, ISaveable
{
    const string PlayerContainerId = "player";

    public static InventoryManager Instance { get; private set; }

    // Inventory_System.md's first confirmed balancing pass (2026-09-23): 30 kg encumbered, 45 kg hard cap.
    [Header("Carrying (kg)")]
    [Tooltip("Carried weight above this slows the player (First_Person_Controller.md).")]
    [SerializeField, Min(0f)] float encumberedWeightKg = 30f;
    [Tooltip("Nothing more can be picked up past this weight.")]
    [SerializeField, Min(0f)] float maxCarryWeightKg = 45f;
    [Tooltip("Carrying a Pouch (Primitive_Storage_System.md) raises both limits by this much: room for more to bring home.")]
    [SerializeField, Min(0f)] float pouchBonusKg = 10f;

    public const string PouchId = "pouch";

    // Inventory_System.md's Tool Hotkeys: ten slots, keys 1-9 then 0, each holding the id of a Tool (or empty).
    public const int HotkeySlots = 10;
    public static readonly UnityEngine.InputSystem.Key[] HotkeyKeys =
    {
        UnityEngine.InputSystem.Key.Digit1, UnityEngine.InputSystem.Key.Digit2, UnityEngine.InputSystem.Key.Digit3,
        UnityEngine.InputSystem.Key.Digit4, UnityEngine.InputSystem.Key.Digit5, UnityEngine.InputSystem.Key.Digit6,
        UnityEngine.InputSystem.Key.Digit7, UnityEngine.InputSystem.Key.Digit8, UnityEngine.InputSystem.Key.Digit9,
        UnityEngine.InputSystem.Key.Digit0,
    };

    readonly Dictionary<string, InventoryContainer> storage = new Dictionary<string, InventoryContainer>();
    readonly string[] hotkeys = new string[HotkeySlots];
    // What is worn in each clothing slot (tanning slice T5a): an item id per ClothingRules.Slot, or null. A worn piece stays in the pack, so its weight counts and it is
    // saved with the pack; this only says which piece is on.
    readonly string[] worn = new string[ClothingRules.SlotCount];
    ItemDefinition equippedTool;

    public event Action<ItemDefinition> EquippedToolChanged;
    public event Action HotkeysChanged;
    public event Action WornChanged;

    public InventoryContainer Player { get; private set; }
    public IEnumerable<InventoryContainer> Storage => storage.Values;
    public ItemDefinition EquippedTool => equippedTool;

    public float CarriedWeightKg => Player.WeightKg;
    // The Pouch's or the Leather Pack's extra room (tanning slice T5b): the bigger one counts, they do not add up.
    float CarryBonusKg => Player != null ? ClothingRules.CarryBonusKg(Player.Has(PouchId), pouchBonusKg, Player.Has(ClothingRules.PackId)) : 0f;
    public float EncumberedWeightKg => encumberedWeightKg + CarryBonusKg;
    public float MaxCarryWeightKg => maxCarryWeightKg + CarryBonusKg;
    public bool IsEncumbered => CarriedWeightKg > EncumberedWeightKg;

    // 0 at or below the encumbered weight, rising to 1 at max carry weight — for the movement speed penalty.
    public float Encumbrance =>
        Mathf.Clamp01(Mathf.InverseLerp(EncumberedWeightKg, MaxCarryWeightKg, CarriedWeightKg));

    static int Today => TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Player = new InventoryContainer(PlayerContainerId, maxCarryWeightKg);
        Player.Changed += OnPlayerInventoryChanged;
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

    void OnValidate()
    {
        maxCarryWeightKg = Mathf.Max(maxCarryWeightKg, encumberedWeightKg);
        if (Player != null)
            Player.MaxWeightKg = MaxCarryWeightKg;
    }

    // Picks up into the player's inventory. Returns how many fit; the rest should stay where they were.
    public int AddToPlayer(string itemId, int quantity) => AddTo(Player, itemId, quantity);

    // As AddToPlayer, but dated as if acquired on another day — e.g. meat from a carcass that waited before
    // field dressing, which has less shelf life left (Hunting_System.md: delayed dressing accelerates spoilage).
    public int AddToPlayer(string itemId, int quantity, int acquiredDay)
    {
        ItemDefinition item = ItemDatabase.Get(itemId);
        if (item == null)
        {
            Debug.LogWarning($"[Inventory] Unknown item '{itemId}'.");
            return 0;
        }

        return Player.Add(item, quantity, acquiredDay);
    }

    public int RemoveFromPlayer(string itemId, int quantity) => Player.Remove(itemId, quantity);

    public int AddTo(InventoryContainer container, string itemId, int quantity)
    {
        ItemDefinition item = ItemDatabase.Get(itemId);
        if (item == null)
        {
            Debug.LogWarning($"[Inventory] Unknown item '{itemId}'.");
            return 0;
        }

        // Perishable food is stamped on the freshness clock (SpoilageManager); everything else keeps the day, as before.
        return container.Add(item, quantity, item.IsPerishable ? SpoilageManager.FreshStamp() : Today);
    }

    public InventoryContainer GetStorage(string storageId) =>
        storageId != null && storage.TryGetValue(storageId, out InventoryContainer container) ? container : null;

    // Called by a Home Storage structure (Root Cellar, Storage Shed, Barn — Building_Housing_System.md).
    // Inventory_System.md Design Rule 2: storage capacity should always exceed what the player can carry.
    public InventoryContainer GetOrCreateStorage(string storageId, float maxWeightKg)
    {
        if (maxWeightKg <= maxCarryWeightKg)
            Debug.LogWarning($"[Inventory] Storage '{storageId}' holds {maxWeightKg} kg, not more than the player's {maxCarryWeightKg} kg.");

        InventoryContainer container = GetStorage(storageId);
        if (container == null)
        {
            container = new InventoryContainer(storageId, maxWeightKg);
            storage.Add(storageId, container);
        }
        else
        {
            container.MaxWeightKg = maxWeightKg;
        }

        return container;
    }

    // Removes a storage structure's container, e.g. when the building is demolished. Returns false if unknown.
    public bool RemoveStorage(string storageId) => storageId != null && storage.Remove(storageId);

    // Only tools the player is carrying can be equipped.
    // Tools by category that are carried, not held: they never go in hand or on a hotkey (the Inventory screen shows no
    // Key slot for them). The category alone can't say so, since the Sleeping Bag, Tent and Weather Station are all Tools.
    public static bool IsCarriedOnly(string itemId) =>
        itemId == SleepManager.SleepingBagId || itemId == WoodManager.TentId || itemId == WeatherScreen.WeatherStationId || itemId == TanningVatRules.BarkSpudId // the Bark Spud is used from the Inventory
        || itemId == ClothingRules.PackId; // the Leather Pack works by being carried

    public bool Equip(string itemId)
    {
        ItemDefinition item = ItemDatabase.Get(itemId);
        if (item == null || item.Category != ItemCategory.Tool || IsCarriedOnly(itemId) || !Player.Has(itemId))
            return false;

        SetEquipped(item);
        return true;
    }

    public void Unequip() => SetEquipped(null);

    // --- Clothing (tanning slice T5a) ---

    // The piece worn in a slot, or null.
    public string WornIn(ClothingRules.Slot slot) => worn[(int)slot];

    public bool IsWorn(string itemId) => !string.IsNullOrEmpty(itemId) && Array.IndexOf(worn, itemId) >= 0;

    // The warmth (clo) of everything worn, added to the everyday clothes' warmth (SurvivalManager).
    public float WornClo => ClothingRules.WornClo(worn);

    public bool MoccasinsWorn => IsWorn(ClothingRules.MoccasinsId);

    // Puts a carried piece of clothing on, in its own slot (whatever was there comes off). False if it is not clothing or not carried.
    public bool Wear(string itemId)
    {
        int slot = ClothingRules.SlotOf(itemId);
        if (slot < 0 || !Player.Has(itemId))
            return false;
        if (worn[slot] == itemId)
            return true;
        worn[slot] = itemId;
        WornChanged?.Invoke();
        return true;
    }

    // Takes a piece off (it stays in the pack). False if it was not worn.
    public bool TakeOff(string itemId)
    {
        int slot = Array.IndexOf(worn, itemId);
        if (string.IsNullOrEmpty(itemId) || slot < 0)
            return false;
        worn[slot] = null;
        WornChanged?.Invoke();
        return true;
    }

    // --- Tool hotkeys ---

    // The label for a slot: keys 1-9, then 0 for the tenth.
    public static string HotkeyLabel(int slot) => slot == HotkeySlots - 1 ? "0" : (slot + 1).ToString();

    public string HotkeyItemId(int slot) =>
        slot >= 0 && slot < HotkeySlots && !string.IsNullOrEmpty(hotkeys[slot]) ? hotkeys[slot] : null;

    // The slot a Tool is on, or -1.
    public int HotkeySlotOf(string itemId)
    {
        for (int i = 0; i < HotkeySlots; i++)
        {
            if (itemId != null && hotkeys[i] == itemId)
                return i;
        }
        return -1;
    }

    // Puts a Tool on a slot (itemId null or empty clears it). A Tool lives on one slot at a time, so assigning it
    // moves it off its old one, and whatever was on the target slot is replaced. Tools only.
    public void SetHotkey(int slot, string itemId)
    {
        if (slot < 0 || slot >= HotkeySlots)
            return;

        if (!string.IsNullOrEmpty(itemId))
        {
            ItemDefinition item = ItemDatabase.Get(itemId);
            if (item == null || item.Category != ItemCategory.Tool || IsCarriedOnly(itemId))
                return;
            int old = HotkeySlotOf(itemId);
            if (old >= 0)
                hotkeys[old] = null;
            hotkeys[slot] = itemId;
        }
        else
        {
            hotkeys[slot] = null;
        }

        HotkeysChanged?.Invoke();
    }

    // A number key: equips that slot's Tool, or puts it away if it's already in hand. Says why when it can't.
    public void PressHotkey(int slot)
    {
        string id = HotkeyItemId(slot);
        if (id == null)
        {
            ToolStatus.Flash($"Nothing on key {HotkeyLabel(slot)} — assign a Tool from the Inventory screen (I).", 2f);
            return;
        }

        ItemDefinition tool = ItemDatabase.Get(id);
        string name = tool != null ? tool.DisplayName : id;
        if (!Player.Has(id))
        {
            ToolStatus.Flash($"Not carrying the {name}.", 2f);
            return;
        }

        if (equippedTool != null && equippedTool.Id == id)
            Unequip();
        else
            Equip(id);
    }

    // Number keys work whenever the world is live — not in a menu, with the Inventory open, or paused.
    void Update()
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard == null || GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
            return;

        for (int i = 0; i < HotkeySlots; i++)
        {
            if (keyboard[HotkeyKeys[i]].wasPressedThisFrame)
            {
                PressHotkey(i);
                break;
            }
        }
    }

    // Empties everything — used when starting a new game.
    public void ResetInventory()
    {
        Player.Clear();
        storage.Clear();
        SetEquipped(null);
        Array.Clear(hotkeys, 0, hotkeys.Length);
        HotkeysChanged?.Invoke();
        Array.Clear(worn, 0, worn.Length);
        WornChanged?.Invoke();
    }

    void OnPlayerInventoryChanged()
    {
        // The Pouch's extra room comes and goes with it.
        Player.MaxWeightKg = MaxCarryWeightKg;

        // A tool that's been dropped, sold or stored can't stay equipped.
        if (equippedTool != null && !Player.Has(equippedTool.Id))
            SetEquipped(null);

        // Nor can clothing that has left the pack stay worn.
        bool changed = false;
        for (int i = 0; i < worn.Length; i++)
        {
            if (worn[i] != null && !Player.Has(worn[i]))
            {
                worn[i] = null;
                changed = true;
            }
        }
        if (changed)
            WornChanged?.Invoke();
    }

    void SetEquipped(ItemDefinition item)
    {
        if (item == equippedTool)
            return;

        equippedTool = item;
        EquippedToolChanged?.Invoke(equippedTool);
    }

    public InventorySaveData CaptureState()
    {
        var data = new InventorySaveData
        {
            player = Player.ToData(),
            equippedItemId = equippedTool != null ? equippedTool.Id : "",
            storage = new List<InventoryContainerData>(),
        };

        foreach (InventoryContainer container in storage.Values)
            data.storage.Add(container.ToData());

        data.hotkeys = new List<string>();
        foreach (string id in hotkeys)
            data.hotkeys.Add(id ?? "");

        data.worn = new List<string>();
        foreach (string id in worn)
            data.worn.Add(id ?? "");

        return data;
    }

    // Restores silently apart from containers' Changed events.
    public void RestoreState(InventorySaveData data)
    {
        if (data.player != null)
            Player.LoadData(data.player);
        else
            Player.Clear();
        Player.MaxWeightKg = MaxCarryWeightKg; // the limit is the game's, not whatever the save recorded

        storage.Clear();
        if (data.storage != null)
        {
            foreach (InventoryContainerData containerData in data.storage)
            {
                var container = new InventoryContainer(containerData.id, containerData.maxWeightKg);
                container.LoadData(containerData);
                storage[containerData.id] = container;
            }
        }

        Array.Clear(hotkeys, 0, hotkeys.Length);
        if (data.hotkeys != null)
        {
            for (int i = 0; i < HotkeySlots && i < data.hotkeys.Count; i++)
            {
                ItemDefinition item = string.IsNullOrEmpty(data.hotkeys[i]) ? null : ItemDatabase.Get(data.hotkeys[i]);
                hotkeys[i] = item != null && item.Category == ItemCategory.Tool && !IsCarriedOnly(item.Id) ? item.Id : null;
            }
        }
        HotkeysChanged?.Invoke();

        // Each slot keeps only clothing for that slot that is still in the pack.
        Array.Clear(worn, 0, worn.Length);
        if (data.worn != null)
        {
            for (int i = 0; i < worn.Length && i < data.worn.Count; i++)
            {
                string id = data.worn[i];
                if (!string.IsNullOrEmpty(id) && ClothingRules.SlotOf(id) == i && Player.Has(id))
                    worn[i] = id;
            }
        }
        WornChanged?.Invoke();

        equippedTool = null;
        if (!string.IsNullOrEmpty(data.equippedItemId))
            Equip(data.equippedItemId);
    }

    // Save_Data_Model.md: Player Block inventory/equipped tools and Property Block home storage.
    string ISaveable.SaveFile => "inventory";
    string ISaveable.SaveKey => "inventory";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<InventorySaveData>(json));
}
