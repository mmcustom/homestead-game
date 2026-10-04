using System.Collections.Generic;
using UnityEngine;

// What comes back to the player when a structure is taken down (WoodPile.RemoveStructure, WoodManager.DismantleCabin):
// each item goes into the pack as far as it fits, and what doesn't is left as pickup piles in a ring around the spot
// the structure stood (DroppedItems.Place), so nothing is ever lost to a full pack. Describe says what happened.
public class StructureRefund
{
    readonly Vector3 at;
    readonly Transform ignore;
    readonly List<string> back = new List<string>();
    readonly List<string> left = new List<string>();
    int spot;

    // ignore is the structure's own object, so piles land on the ground where it stood rather than on top of it.
    public StructureRefund(Vector3 at, Transform ignore)
    {
        this.at = at;
        this.ignore = ignore;
    }

    public void Return(string itemId, int count, int day = 0)
    {
        InventoryManager inventory = InventoryManager.Instance;
        ItemDefinition item = ItemDatabase.Get(itemId);
        if (inventory == null || item == null || count <= 0)
            return;

        int added = item.IsPerishable ? inventory.AddToPlayer(itemId, count, day) : inventory.AddToPlayer(itemId, count);
        if (added > 0)
            back.Add($"{added} {item.DisplayName}");
        if (added < count)
        {
            float angle = spot++ * 72f * Mathf.Deg2Rad;
            DroppedItems.Place(itemId, count - added, at + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 1.2f, ignore);
            left.Add($"{count - added} {item.DisplayName}");
        }
    }

    // e.g. "Took down the Tool Rack — 2 Branches, 2 Sticks back in your pack; 4 Stone left on the ground (no room to carry it)."
    public string Describe(string what)
    {
        string message = $"Took down the {what}";
        if (back.Count > 0)
            message += $" — {string.Join(", ", back)} back in your pack";
        if (left.Count > 0)
            message += $"; {string.Join(", ", left)} left on the ground (no room to carry it)";
        return message + ".";
    }
}
