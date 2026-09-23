using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct InventorySaveData
{
    public InventoryContainerData player;
    public string equippedItemId;
    public List<InventoryContainerData> storage;
}

// Player inventory, storage containers, equipment (Unity_Architecture.md, Inventory_System.md).
// Storage contents live here rather than on the building objects so they persist across scenes and saves;
// a storage structure looks up its container with GetOrCreateStorage.
public class InventoryManager : MonoBehaviour, ISaveable
{
    const string PlayerContainerId = "player";

    public static InventoryManager Instance { get; private set; }

    // Inventory_System.md: exact limits are a balancing pass — these are placeholder defaults.
    [Header("Carrying (kg)")]
    [Tooltip("Carried weight above this slows the player (First_Person_Controller.md).")]
    [SerializeField, Min(0f)] float encumberedWeightKg = 25f;
    [Tooltip("Nothing more can be picked up past this weight.")]
    [SerializeField, Min(0f)] float maxCarryWeightKg = 40f;

    readonly Dictionary<string, InventoryContainer> storage = new Dictionary<string, InventoryContainer>();
    ItemDefinition equippedTool;

    public event Action<ItemDefinition> EquippedToolChanged;

    public InventoryContainer Player { get; private set; }
    public IEnumerable<InventoryContainer> Storage => storage.Values;
    public ItemDefinition EquippedTool => equippedTool;

    public float CarriedWeightKg => Player.WeightKg;
    public float EncumberedWeightKg => encumberedWeightKg;
    public float MaxCarryWeightKg => maxCarryWeightKg;
    public bool IsEncumbered => CarriedWeightKg > encumberedWeightKg;

    // 0 at or below the encumbered weight, rising to 1 at max carry weight — for the movement speed penalty.
    public float Encumbrance =>
        Mathf.Clamp01(Mathf.InverseLerp(encumberedWeightKg, maxCarryWeightKg, CarriedWeightKg));

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
            Player.MaxWeightKg = maxCarryWeightKg;
    }

    // Picks up into the player's inventory. Returns how many fit; the rest should stay where they were.
    public int AddToPlayer(string itemId, int quantity) => AddTo(Player, itemId, quantity);

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

    // Empties everything — used when starting a new game.
    public void ResetInventory()
    {
        Player.Clear();
        storage.Clear();
        SetEquipped(null);
    }

    void OnPlayerInventoryChanged()
    {
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

        return data;
    }

    // Restores silently apart from containers' Changed events.
    public void RestoreState(InventorySaveData data)
    {
        if (data.player != null)
            Player.LoadData(data.player);
        else
            Player.Clear();

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
