using System.Collections.Generic;
using UnityEngine;

// Wave 4 item icons (2026-11-19): the 128 px picture of each item, rendered from its dropped-pile art by the editor tool
// ItemIconRenderer (menu Homestead > Art > Render Item Icons) into Resources/Icons/item_<id>.png. Found by the item's id, so no item
// asset needs an icon field. An item with no icon gets null and the UI simply shows no picture.
public static class ItemIcons
{
    public const string Prefix = "Icons/item_";

    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static Sprite For(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return null;
        if (!cache.TryGetValue(itemId, out Sprite sprite))
        {
            sprite = Resources.Load<Sprite>(Prefix + itemId);
            cache[itemId] = sprite;
        }
        return sprite;
    }

    public static Sprite For(ItemDefinition item) => item != null ? For(item.Id) : null;

    // The Build tab's entries (Mike, 2026-11-19): "pile:<Kind>" for a structure or its site, "campfire" and "tripod", as Icons/build_<key>,
    // rendered by ItemIconRenderer.RenderBuild from the structure art. Null for anything else.
    public const string BuildPrefix = "Icons/build_";

    static readonly Dictionary<string, Sprite> buildCache = new Dictionary<string, Sprite>();

    public static Sprite ForBuild(string entryKey)
    {
        if (string.IsNullOrEmpty(entryKey))
            return null;
        if (!buildCache.TryGetValue(entryKey, out Sprite sprite))
        {
            string name = entryKey.StartsWith("pile:") ? entryKey.Substring(5) : entryKey;
            sprite = Resources.Load<Sprite>(BuildPrefix + name.ToLowerInvariant());
            buildCache[entryKey] = sprite;
        }
        return sprite;
    }
}
