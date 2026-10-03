using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class DroppedItemData
{
    public string itemId;
    public int quantity;
    public int acquiredDay;
    public Vector3 position;
    public float yaw;
}

[Serializable]
public class DroppedItemsSaveData
{
    public List<DroppedItemData> items = new List<DroppedItemData>();
}

// Inventory_System.md's Dropping Items (2026-10-03): any carried item, anywhere, set down as a real ItemPickup just in
// front of the player and left there until picked up — no despawn timer. This is the drop action (Drop) and the save
// block that remembers the piles across saves, so nothing the player sets down vanishes on reload. Created on startup
// rather than placed in a scene, like ItemDatabase.
public class DroppedItems : MonoBehaviour, ISaveable
{
    const float DropDistance = 1.2f;
    const float MergeRadius = 0.6f;

    public static DroppedItems Instance { get; private set; }

    bool registered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null)
            return;
        new GameObject("Dropped Items").AddComponent<DroppedItems>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // SaveManager may not exist yet on the first frame, so keep trying until it does.
    void Update()
    {
        if (registered || SaveManager.Instance == null)
            return;
        SaveManager.Instance.Register(this);
        registered = true;
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;
        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
        Instance = null;
    }

    // Drops up to quantity of an item from the player's pack. Returns how many were dropped; reason says why none were.
    public static int Drop(PlayerController player, string itemId, int quantity, out string reason)
    {
        reason = "";
        InventoryManager inventory = InventoryManager.Instance;
        ItemDefinition item = ItemDatabase.Get(itemId);
        if (inventory == null || item == null || player == null)
        {
            reason = "Can't drop that here.";
            return 0;
        }

        if (!TryFindSpot(player, out Vector3 spot, out reason))
            return 0;

        // One pile per batch, so a dropped perishable keeps the age it had in the pack.
        var batches = new List<(int day, int count)>();
        int removed = inventory.Player.RemoveBatches(itemId, quantity, (day, count) => batches.Add((day, count)));
        float yaw = player.transform.eulerAngles.y;
        for (int i = 0; i < batches.Count; i++)
        {
            Vector3 position = ClearOfOtherPiles(spot + player.transform.right * (0.3f * i), player.transform.right,
                                                 item, batches[i].day);
            Spawn(item, batches[i].count, batches[i].day, position, yaw, true);
        }

        if (removed > 0 && AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.ItemDrop);
        return removed;
    }

    // A spot on the ground a short way in front of the player, pulled in if something solid is in the way.
    static bool TryFindSpot(PlayerController player, out Vector3 spot, out string reason)
    {
        spot = default;
        reason = "";
        Transform body = player.transform;
        Vector3 forward = Vector3.ProjectOnPlane(body.forward, Vector3.up).normalized;
        Vector3 chest = body.position + Vector3.up * 1f;

        float distance = DropDistance;
        RaycastHit[] ahead = Physics.SphereCastAll(chest, 0.2f, forward, DropDistance, ~0, QueryTriggerInteraction.Ignore);
        float nearest = float.MaxValue;
        foreach (RaycastHit hit in ahead)
        {
            if (!hit.collider.transform.IsChildOf(body) && hit.collider.GetComponentInParent<ItemPickup>() == null)
                nearest = Mathf.Min(nearest, hit.distance);
        }
        if (nearest < DropDistance)
            distance = Mathf.Max(0.4f, nearest - 0.15f);

        Vector3 probe = body.position + forward * distance + Vector3.up * 2f;
        RaycastHit[] below = Physics.RaycastAll(probe, Vector3.down, 6f, ~0, QueryTriggerInteraction.Ignore);
        Array.Sort(below, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in below)
        {
            if (hit.collider.transform.IsChildOf(body))
                continue;
            if (hit.collider.GetComponentInParent<WaterSource>() != null)
            {
                reason = "Can't leave that in the water.";
                return false;
            }
            spot = hit.point;
            return true;
        }

        reason = "No ground in front of you to put it on.";
        return false;
    }

    // Slides a spot sideways, away from the player's right, until it isn't sitting on a different dropped pile — a pile
    // of the same batch is fine, Spawn merges into it. The ground height is kept; this only moves it along the ground.
    static Vector3 ClearOfOtherPiles(Vector3 spot, Vector3 right, ItemDefinition item, int day)
    {
        ItemPickup[] piles = FindObjectsByType<ItemPickup>(FindObjectsSortMode.None);
        for (int attempt = 0; attempt < 8; attempt++)
        {
            bool blocked = false;
            foreach (ItemPickup pile in piles)
            {
                bool same = pile.Item == item && pile.AcquiredDay == day;
                Vector3 flat = pile.transform.position - spot;
                flat.y = 0f;
                if (pile.Dropped && !same && flat.sqrMagnitude < 0.4f * 0.4f)
                    blocked = true;
            }
            if (!blocked)
                break;
            spot += Vector3.ProjectOnPlane(right, Vector3.up).normalized * 0.4f;
        }
        return spot;
    }

    // Makes the pickup object, or adds to a pile of the same batch already lying right there.
    static ItemPickup Spawn(ItemDefinition item, int quantity, int day, Vector3 position, float yaw, bool merge)
    {
        if (merge)
        {
            foreach (ItemPickup existing in FindObjectsByType<ItemPickup>(FindObjectsSortMode.None))
            {
                if (existing.Dropped && existing.Item == item && existing.AcquiredDay == day &&
                    (existing.transform.position - position).sqrMagnitude < MergeRadius * MergeRadius)
                {
                    existing.AddQuantity(quantity);
                    return existing;
                }
            }
        }

        // A plain cube, tinted by category and sized a little by weight — there are no per-item models yet.
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = $"Dropped {item.DisplayName}";
        float size = Mathf.Clamp(0.2f + 0.1f * Mathf.Sqrt(item.WeightKg), 0.2f, 0.5f);
        go.transform.localScale = new Vector3(size, size * 0.7f, size);
        go.transform.SetPositionAndRotation(position + Vector3.up * (size * 0.35f), Quaternion.Euler(0f, yaw, 0f));

        var block = new MaterialPropertyBlock();
        Color tint = Tint(item.Category);
        block.SetColor("_BaseColor", tint);
        block.SetColor("_Color", tint);
        go.GetComponent<Renderer>().SetPropertyBlock(block);

        Scene world = SceneManager.GetSceneByName(GameManager.WorldScene);
        if (world.IsValid() && world.isLoaded)
            SceneManager.MoveGameObjectToScene(go, world);

        var pickup = go.AddComponent<ItemPickup>();
        pickup.SetDropped(item, quantity, day);
        return pickup;
    }

    static Color Tint(ItemCategory category)
    {
        switch (category)
        {
            case ItemCategory.Tool: return new Color(0.45f, 0.5f, 0.58f);
            case ItemCategory.Consumable: return new Color(0.5f, 0.66f, 0.38f);
            case ItemCategory.Resource: return new Color(0.62f, 0.42f, 0.3f);
            default: return new Color(0.72f, 0.62f, 0.42f);
        }
    }

    // --- Saving ---

    public DroppedItemsSaveData CaptureState()
    {
        var data = new DroppedItemsSaveData();
        foreach (ItemPickup pickup in FindObjectsByType<ItemPickup>(FindObjectsSortMode.None))
        {
            if (!pickup.Dropped || pickup.Item == null)
                continue;
            data.items.Add(new DroppedItemData
            {
                itemId = pickup.Item.Id,
                quantity = pickup.Quantity,
                acquiredDay = pickup.AcquiredDay,
                position = pickup.transform.position,
                yaw = pickup.transform.eulerAngles.y,
            });
        }
        return data;
    }

    public void RestoreState(DroppedItemsSaveData data)
    {
        foreach (ItemPickup pickup in FindObjectsByType<ItemPickup>(FindObjectsSortMode.None))
        {
            if (pickup.Dropped)
                Destroy(pickup.gameObject);
        }

        if (data == null || data.items == null)
            return;
        foreach (DroppedItemData saved in data.items)
        {
            ItemDefinition item = ItemDatabase.Get(saved.itemId);
            if (item == null || saved.quantity <= 0)
                continue;
            // Saved positions are the pile's centre, already lifted off the ground, so lower it back to the ground.
            float size = Mathf.Clamp(0.2f + 0.1f * Mathf.Sqrt(item.WeightKg), 0.2f, 0.5f);
            Spawn(item, saved.quantity, saved.acquiredDay, saved.position - Vector3.up * (size * 0.35f), saved.yaw, false);
        }
    }

    string ISaveable.SaveFile => "world";
    string ISaveable.SaveKey => "dropped";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<DroppedItemsSaveData>(json));
}
