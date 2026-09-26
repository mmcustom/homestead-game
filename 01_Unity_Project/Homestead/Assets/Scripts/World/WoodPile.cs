using System.Collections.Generic;
using UnityEngine;

// A pile of wood or stone in the World (WoodManager owns the state): what a felled tree left at its base, or a Wood
// Pile or Rock Pile the player built for storage (Wood_Gathering_System.md's Primitive Storage). Looking at it shows
// what's in it. The main interaction carries off whatever fits — the light things first, Logs last, since they're
// 8 kg each — so a big load can take a few trips. The second (R) stores everything the pile takes that the player is
// carrying: wood-family Materials in a wood pile, Stone in a rock pile. With the Axe, swinging at a wood pile splits
// its Logs (then its Branches) into Firewood where they lie (AxeTool). Drawn from simple shapes that follow the
// contents; a built pile shows its frame or base stones even when empty.
public class WoodPile : MonoBehaviour, IInteractable, ISecondaryInteractable
{
    // What the pile gives up first when carried off.
    static readonly string[] TakeOrder = { FireManager.FirewoodId, WoodManager.SticksId, WoodManager.BranchesId, WoodManager.LogsId, WoodManager.StoneId };

    WoodPileState state;
    Material bark;
    readonly List<GameObject> pieces = new List<GameObject>();
    BoxCollider box;

    public WoodPileState State => state;

    public void Bind(WoodPileState pileState, Material barkMaterial)
    {
        state = pileState;
        bark = barkMaterial;
        box = gameObject.AddComponent<BoxCollider>();
        Rebuild();
    }

    string Name => state.IsRock ? "Rock Pile" : "Wood Pile";

    public string InteractionPrompt
    {
        get
        {
            if (state == null)
                return "";
            if (state.IsEmpty)
                return $"{Name}  (empty)";
            string contents = Describe(state);
            string verb = state.IsRock ? "Take Stone" : "Take Wood";
            return AnythingFits() ? $"{verb}  ({contents})" : $"{Name}  ({contents}) — no room to carry more";
        }
    }

    // Always true so the pile's contents show when looked at.
    public bool CanInteract(PlayerController player) => state != null && InventoryManager.Instance != null;

    public void Interact(PlayerController player)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (state == null || inventory == null || state.IsEmpty)
            return;

        int taken = 0;
        foreach (string itemId in TakeOrder)
        {
            int count = state.Count(itemId);
            if (count <= 0)
                continue;
            int added = inventory.AddToPlayer(itemId, count);
            state.Remove(itemId, added);
            taken += added;
        }
        if (taken == 0)
        {
            ToolStatus.Flash("Too heavy — no room to carry any of it");
            return;
        }
        if (!state.IsEmpty)
            ToolStatus.Flash($"Left in the pile: {Describe(state)}");
        if (WoodManager.Instance != null)
            WoodManager.Instance.NotifyChanged(state);
    }

    // --- Storing (R) ---

    public string SecondaryPrompt
    {
        get
        {
            if (state == null)
                return "";
            int carried = CarriedStorable(out string what);
            return carried > 0 ? $"Store {what}" : "";
        }
    }

    public void SecondaryInteract(PlayerController player)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (state == null || inventory == null)
            return;

        int stored = 0;
        var ids = new List<string>();
        foreach (ItemStack stack in inventory.Player.Stacks)
            if (state.Accepts(stack.itemId) && !ids.Contains(stack.itemId))
                ids.Add(stack.itemId);
        foreach (string itemId in ids)
        {
            if (inventory.EquippedTool != null && inventory.EquippedTool.Id == itemId)
                continue;
            int count = inventory.Player.Count(itemId);
            int removed = inventory.RemoveFromPlayer(itemId, count);
            state.Add(itemId, removed);
            stored += removed;
        }
        if (stored <= 0)
            return;
        ToolStatus.Flash($"Stored — the pile now holds {Describe(state)}");
        if (WoodManager.Instance != null)
            WoodManager.Instance.NotifyChanged(state);
    }

    // How many storable items the player carries, and a short description ("5 Firewood, 2 Logs").
    int CarriedStorable(out string what)
    {
        what = "";
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
            return 0;
        var parts = new List<string>();
        int total = 0;
        foreach (string itemId in state.IsRock ? new[] { WoodManager.StoneId } : WoodManager.WoodIds)
        {
            int count = inventory.Player.Count(itemId);
            if (count <= 0)
                continue;
            total += count;
            parts.Add($"{count} {ItemDatabase.Get(itemId)?.DisplayName ?? itemId}");
        }
        what = string.Join(", ", parts);
        return total;
    }

    bool AnythingFits()
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
            return false;
        foreach (WoodStack stack in state.contents)
        {
            ItemDefinition item = ItemDatabase.Get(stack.itemId);
            if (item != null && inventory.Player.SpaceFor(item) > 0)
                return true;
        }
        return false;
    }

    // e.g. "2 Logs, 4 Branches, 5 Sticks".
    public static string Describe(WoodPileState pile)
    {
        var parts = new List<string>();
        foreach (string itemId in new[] { WoodManager.LogsId, WoodManager.BranchesId, WoodManager.SticksId, FireManager.FirewoodId, WoodManager.StoneId })
        {
            int count = pile.Count(itemId);
            if (count > 0)
                parts.Add($"{count} {ItemDatabase.Get(itemId)?.DisplayName ?? itemId}");
        }
        return parts.Count > 0 ? string.Join(", ", parts) : "empty";
    }

    // --- Looks ---

    public void Rebuild()
    {
        foreach (GameObject piece in pieces)
            if (piece != null)
                Destroy(piece);
        pieces.Clear();
        if (state == null)
            return;

        var random = new System.Random(state.id * 7919);
        if (state.IsRock)
        {
            BuildRocks(random);
            return;
        }

        int logs = Mathf.Min(state.Count(WoodManager.LogsId), 6);
        int branches = Mathf.Min(state.Count(WoodManager.BranchesId), 8);
        int sticks = Mathf.Min(state.Count(WoodManager.SticksId), 10);
        int firewood = Mathf.Min(state.Count(FireManager.FirewoodId), 16);

        // A built pile's frame: two poles on the ground that the wood sits across.
        if (state.IsBuilt)
            foreach (float x in new[] { -0.55f, 0.55f })
                Piece(PrimitiveType.Cylinder, new Vector3(x, 0.03f, 0f), new Vector3(90f, 0f, 0f), new Vector3(0.06f, 0.8f, 0.06f), bark);

        // Logs side by side along the fall line, stacked two deep.
        for (int i = 0; i < logs; i++)
        {
            float x = ((i % 3) - 1) * 0.36f, y = 0.17f + (i / 3) * 0.3f;
            Piece(PrimitiveType.Cylinder, new Vector3(x, y, 0f), new Vector3(90f, 0f, 0f), new Vector3(0.32f, 0.8f, 0.32f), bark);
        }
        // Branches: long and thin, criss-crossed to one side.
        for (int i = 0; i < branches; i++)
            Piece(PrimitiveType.Cylinder, new Vector3(0.8f + (float)random.NextDouble() * 0.4f, 0.05f + i * 0.04f, (float)random.NextDouble() * 1.2f - 0.6f),
                  new Vector3(90f, (float)random.NextDouble() * 60f - 30f, 0f), new Vector3(0.09f, 0.9f, 0.09f), bark);
        // Sticks: a small heap on the other side.
        for (int i = 0; i < sticks; i++)
            Piece(PrimitiveType.Cylinder, new Vector3(-0.8f + (float)random.NextDouble() * 0.3f, 0.03f + i * 0.02f, (float)random.NextDouble() * 0.6f - 0.3f),
                  new Vector3(90f, (float)random.NextDouble() * 180f, 0f), new Vector3(0.035f, 0.35f, 0.035f), bark);
        // Split firewood: short chunks stacked in a row behind.
        for (int i = 0; i < firewood; i++)
            Piece(PrimitiveType.Cylinder, new Vector3(((i % 4) - 1.5f) * 0.2f, 0.09f + (i / 4) * 0.17f, -0.85f),
                  new Vector3(90f, 0f, 0f), new Vector3(0.17f, 0.2f, 0.17f), bark);

        // One box to look at and bump into, around whatever's there.
        bool anyWood = logs + branches + sticks + firewood > 0;
        box.center = new Vector3(0f, 0.25f, -0.1f);
        box.size = anyWood ? new Vector3(logs + branches > 0 ? 2.2f : 1.6f, logs > 3 || firewood > 8 ? 0.7f : 0.45f, 1.9f)
                           : new Vector3(1.3f, 0.3f, 1.7f);
    }

    // A heap of grey stones: a ring of base stones always, more piled on as Stone is stored.
    void BuildRocks(System.Random random)
    {
        int stones = Mathf.Min(state.Count(WoodManager.StoneId), 20);
        for (int i = 0; i < 6; i++)
        {
            float a = i / 6f * Mathf.PI * 2f;
            Stone(new Vector3(Mathf.Cos(a) * 0.55f, 0.06f, Mathf.Sin(a) * 0.55f), 0.18f, random);
        }
        for (int i = 0; i < stones; i++)
        {
            float a = (float)random.NextDouble() * Mathf.PI * 2f;
            float r = (float)random.NextDouble() * 0.4f * (1f - i / 24f);
            Stone(new Vector3(Mathf.Cos(a) * r, 0.1f + i * 0.025f, Mathf.Sin(a) * r), 0.26f, random);
        }
        box.center = new Vector3(0f, 0.2f, 0f);
        box.size = new Vector3(1.4f, stones > 8 ? 0.6f : 0.35f, 1.4f);
    }

    void Stone(Vector3 position, float size, System.Random random)
    {
        GameObject stone = Piece(PrimitiveType.Sphere, position,
                                 new Vector3((float)random.NextDouble() * 360f, (float)random.NextDouble() * 360f, 0f),
                                 new Vector3(size * (0.8f + (float)random.NextDouble() * 0.5f), size * 0.6f, size), null);
        var block = new MaterialPropertyBlock();
        float g = 0.38f + (float)random.NextDouble() * 0.12f;
        block.SetColor("_BaseColor", new Color(g, g * 0.98f, g * 0.95f));
        stone.GetComponent<MeshRenderer>().SetPropertyBlock(block);
    }

    GameObject Piece(PrimitiveType shape, Vector3 position, Vector3 euler, Vector3 scale, Material material)
    {
        GameObject piece = GameObject.CreatePrimitive(shape);
        Destroy(piece.GetComponent<Collider>());
        piece.transform.SetParent(transform, false);
        piece.transform.localPosition = position;
        piece.transform.localRotation = Quaternion.Euler(euler);
        piece.transform.localScale = scale;
        if (material != null)
            piece.GetComponent<MeshRenderer>().sharedMaterial = material;
        pieces.Add(piece);
        return piece;
    }
}
