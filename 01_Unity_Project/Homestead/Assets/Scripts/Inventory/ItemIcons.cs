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
}
