using System.Collections.Generic;
using UnityEngine;

// Looks up ItemDefinitions by id. Loads every definition under Resources/Items on first use.
public static class ItemDatabase
{
    const string ResourcesFolder = "Items";

    static Dictionary<string, ItemDefinition> items;

    public static IEnumerable<ItemDefinition> All
    {
        get
        {
            EnsureLoaded();
            return items.Values;
        }
    }

    public static ItemDefinition Get(string id)
    {
        EnsureLoaded();
        return id != null && items.TryGetValue(id, out ItemDefinition item) ? item : null;
    }

    // Adds a definition that isn't under Resources/Items (e.g. one built in code).
    public static void Register(ItemDefinition item)
    {
        EnsureLoaded();
        items[item.Id] = item;
    }

    static void EnsureLoaded()
    {
        if (items != null)
            return;

        items = new Dictionary<string, ItemDefinition>();
        foreach (ItemDefinition item in Resources.LoadAll<ItemDefinition>(ResourcesFolder))
        {
            if (string.IsNullOrEmpty(item.Id))
                Debug.LogError($"[ItemDatabase] Item asset '{item.name}' has no id.", item);
            else if (items.ContainsKey(item.Id))
                Debug.LogError($"[ItemDatabase] Duplicate item id '{item.Id}' on '{item.name}'.", item);
            else
                items.Add(item.Id, item);
        }
    }

    // Domain reload may be disabled in Play Mode settings, so clear the cache explicitly.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetCache() => items = null;
}
