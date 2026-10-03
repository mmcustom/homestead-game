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
    ItemDefinition equippedTool;

    public event Action<ItemDefinition> EquippedToolChanged;
    public event Action HotkeysChanged;

    public InventoryContainer Player { get; private set; }
    public IEnumerable<InventoryContainer> Storage => storage.Values;
    public ItemDefinition EquippedTool => equippedTool;

    public float CarriedWeightKg => Player.WeightKg;
    float CarryBonusKg => Player != null && Player.Has(PouchId) ? pouchBonusKg : 0f;
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

        return container.Add(item, quantity, Today);
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
    public bool Equip(string itemId)
    {
        ItemDefinition item = ItemDatabase.Get(itemId);
        if (item == null || item.Category != ItemCategory.Tool || !Player.Has(itemId))
            return false;

        SetEquipped(item);
        return true;
    }

    public void Unequip() => SetEquipped(null);

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
            if (item == null || item.Category != ItemCategory.Tool)
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
    }

    void OnPlayerInventoryChanged()
    {
        // The Pouch's extra room comes and goes with it.
        Player.MaxWeightKg = MaxCarryWeightKg;

        // A tool that's been dropped, sold or stored can't stay equipped.
        if (equippedTool != null && !Player.Has(equippedTool.Id))
            SetEquipped(null);
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
                hotkeys[i] = item != null && item.Category == ItemCategory.Tool ? item.Id : null;
            }
        }
        HotkeysChanged?.Invoke();

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
