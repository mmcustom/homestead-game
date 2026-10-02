using System.Collections.Generic;
using UnityEngine;

// A pile or container in the World (WoodManager owns the state): what a felled tree left at its base, or storage the
// player built — Wood_Gathering_System.md's Wood Pile and Rock Pile, Primitive_Storage_System.md's Water Barrel, Food
// Cache, Storage Bin and Tool Rack. Looking at it shows what's in it. The main interaction takes back whatever fits, lightest
// things first (Logs last — they're 8 kg each), so a big load can take a few trips; the Water Barrel instead fills the
// carried Buckets. The second (R) stores everything this kind of storage takes that the player is carrying — pouring
// water into the barrel from the Bucket. Perishable food keeps the day it was acquired while stored. With the Axe,
// swinging at a wood pile splits its Logs (then its Branches) into Firewood where they lie (AxeTool). Drawn from
// simple shapes; built storage shows its frame even when empty.
public class WoodPile : MonoBehaviour, IInteractable, ISecondaryInteractable
{
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

    string Name => WoodManager.PileName(state.kind);
    bool IsBarrel => state.kind == PileKind.WaterBarrel;
    static InventoryManager Inventory => InventoryManager.Instance;

    // --- Taking (E) ---

    public string InteractionPrompt
    {
        get
        {
            if (state == null)
                return "";
            if (state.IsEmpty)
                return $"{Name}  (empty)";
            string contents = Describe(state);
            if (IsBarrel)
            {
                int room = BucketRoom();
                return room > 0 ? $"Fill Bucket  ({contents})"
                     : Inventory != null && WaterQualities.Capacity(Inventory.Player) > 0 ? $"{Name}  ({contents}) — your water containers are full"
                     : $"{Name}  ({contents}) — needs a Bucket or Canteen to carry water";
            }
            string verb = state.kind == PileKind.RockStorage ? "Take Stone"
                        : state.kind == PileKind.FoodCache ? "Take Food"
                        : state.kind == PileKind.StorageBin ? "Take Things"
                        : state.kind == PileKind.ToolRack ? "Take Tools"
                        : state.kind == PileKind.CabinSite ? "Take Materials" : "Take Wood";
            return AnythingFits() ? $"{verb}  ({contents})" : $"{Name}  ({contents}) — no room to carry more";
        }
    }

    // Always true so the contents show when looked at.
    public bool CanInteract(PlayerController player) => state != null && Inventory != null;

    public void Interact(PlayerController player)
    {
        if (state == null || Inventory == null || state.IsEmpty)
            return;

        int taken = IsBarrel ? DrawWater() : TakeAll();
        if (taken == 0)
        {
            if (!IsBarrel)
                ToolStatus.Flash("Too heavy — no room to carry any of it");
            return;
        }
        if (!state.IsEmpty && !IsBarrel)
            ToolStatus.Flash($"Left behind: {Describe(state)}");
        if (WoodManager.Instance != null)
            WoodManager.Instance.NotifyChanged(state);
    }

    // Everything that fits, lightest first, each batch keeping its day.
    int TakeAll()
    {
        var ids = new List<string>();
        foreach (WoodStack stack in state.contents)
            if (!ids.Contains(stack.itemId))
                ids.Add(stack.itemId);
        ids.Sort((a, b) => WeightOf(a).CompareTo(WeightOf(b)));

        int taken = 0;
        foreach (string itemId in ids)
            taken += TakeInto(itemId, state.Count(itemId));
        return taken;
    }

    int TakeInto(string itemId, int count)
    {
        ItemDefinition item = ItemDatabase.Get(itemId);
        if (item == null)
            return 0;
        int fits = Mathf.Min(count, Inventory.Player.SpaceFor(item));
        int added = 0;
        foreach (WoodStack batch in state.Take(itemId, fits))
            added += item.IsPerishable ? Inventory.AddToPlayer(itemId, batch.count, batch.day) : Inventory.AddToPlayer(itemId, batch.count);
        return added;
    }

    // The Water Barrel fills the carried Buckets: purified water first, then the best raw water.
    int DrawWater()
    {
        int room = BucketRoom();
        if (room <= 0)
            return 0;
        int drawn = 0;
        string[] order = { Cooking.PurifiedWaterId, "water_excellent", "water_good", "water_questionable", "water_unsafe" };
        foreach (string itemId in order)
        {
            if (room <= 0)
                break;
            int n = TakeInto(itemId, Mathf.Min(room, state.Count(itemId)));
            room -= n;
            drawn += n;
        }
        if (drawn > 0)
            ToolStatus.Flash($"Filled your Bucket with {drawn} L");
        return drawn;
    }

    // Litres the player's Buckets and Canteens can still take.
    static int BucketRoom()
    {
        if (Inventory == null)
            return 0;
        return Mathf.Max(0, WaterQualities.Capacity(Inventory.Player) - WaterQualities.LitresCarried(Inventory.Player));
    }

    static float WeightOf(string itemId) => ItemDatabase.Get(itemId)?.WeightKg ?? 0f;

    // --- Storing (R) ---

    public string SecondaryPrompt
    {
        get
        {
            if (state == null)
                return "";
            int carried = CarriedStorable(out string what);
            if (carried <= 0)
                return "";
            if (IsBarrel)
            {
                int room = WoodManager.Instance != null ? WoodManager.Instance.BarrelLitres - state.Total : 0;
                return room > 0 ? $"Pour In Water  ({Mathf.Min(room, carried)} L)" : "";
            }
            return $"Store {what}";
        }
    }

    public void SecondaryInteract(PlayerController player)
    {
        if (state == null || Inventory == null)
            return;

        // The barrel's litres are one shared budget across every water type; a CabinSite's cap is per material
        // instead (each stops accepting more once it's got what Small Cabin needs — see WoodPileState.Accepts), so it
        // doesn't share a running total the way the barrel's room does.
        bool isCabinSite = state.kind == PileKind.CabinSite;
        int room = IsBarrel && WoodManager.Instance != null ? WoodManager.Instance.BarrelLitres - state.Total : int.MaxValue;
        int stored = 0;
        var ids = new List<string>();
        foreach (ItemStack stack in Inventory.Player.Stacks)
            if (state.Accepts(stack.itemId) && !ids.Contains(stack.itemId))
                ids.Add(stack.itemId);

        bool first = true;
        foreach (string itemId in ids)
        {
            if (room <= 0)
                break;
            if (Inventory.EquippedTool != null && Inventory.EquippedTool.Id == itemId)
                continue;

            int cap = isCabinSite && WoodManager.Instance != null
                ? Mathf.Max(0, WoodManager.Instance.CabinRequired(itemId) - state.Count(itemId))
                : room;
            if (cap <= 0)
                continue;

            // Note each batch's day before it leaves the inventory, oldest first as Remove takes them.
            var batches = new List<ItemStack>();
            foreach (ItemStack stack in Inventory.Player.Stacks)
                if (stack.itemId == itemId)
                    batches.Add(new ItemStack { itemId = itemId, quantity = stack.quantity, acquiredDay = stack.acquiredDay });
            batches.Sort((a, b) => a.acquiredDay.CompareTo(b.acquiredDay));

            ItemDefinition item = ItemDatabase.Get(itemId);
            int want = Mathf.Min(cap, Inventory.Player.Count(itemId));
            // Storing isn't dropping: the barrel plays one pour, everything else stays quiet.
            if (AudioManager.Instance != null)
            {
                if (IsBarrel && first)
                    AudioManager.Instance.PlayOnConsume(SoundCue.Pour);
                else
                    AudioManager.Instance.SilenceNextRemoval();
            }
            first = false;
            int removed = Inventory.RemoveFromPlayer(itemId, want);
            int left = removed;
            foreach (ItemStack batch in batches)
            {
                if (left <= 0)
                    break;
                int n = Mathf.Min(left, batch.quantity);
                state.Add(itemId, n, item != null && item.IsPerishable ? batch.acquiredDay : 0);
                left -= n;
            }
            stored += removed;
            if (!isCabinSite)
                room -= removed;
        }
        if (stored <= 0)
            return;
        ToolStatus.Flash(IsBarrel ? $"Poured in {stored} L — the barrel holds {state.Total} L"
                                  : $"Stored — it now holds {Describe(state)}");
        if (WoodManager.Instance != null)
            WoodManager.Instance.NotifyChanged(state);
    }

    // How many storable items the player carries (not the equipped tool), and a short description.
    int CarriedStorable(out string what)
    {
        what = "";
        if (Inventory == null)
            return 0;
        var counts = new Dictionary<string, int>();
        foreach (ItemStack stack in Inventory.Player.Stacks)
        {
            if (!state.Accepts(stack.itemId) || (Inventory.EquippedTool != null && Inventory.EquippedTool.Id == stack.itemId))
                continue;
            counts[stack.itemId] = (counts.TryGetValue(stack.itemId, out int n) ? n : 0) + stack.quantity;
        }
        int total = 0;
        var parts = new List<string>();
        foreach (KeyValuePair<string, int> pair in counts)
        {
            total += pair.Value;
            parts.Add($"{pair.Value} {ItemDatabase.Get(pair.Key)?.DisplayName ?? pair.Key}");
        }
        what = Shorten(parts);
        return total;
    }

    bool AnythingFits()
    {
        foreach (WoodStack stack in state.contents)
        {
            ItemDefinition item = ItemDatabase.Get(stack.itemId);
            if (item != null && Inventory.Player.SpaceFor(item) > 0)
                return true;
        }
        return false;
    }

    // e.g. "2 Logs, 4 Branches, 5 Sticks" — water as litres, and long lists cut short.
    public static string Describe(WoodPileState pile)
    {
        if (pile.kind == PileKind.WaterBarrel)
            return pile.IsEmpty ? "empty" : $"{pile.Total} L of water";

        var counts = new List<KeyValuePair<string, int>>();
        foreach (WoodStack stack in pile.contents)
        {
            int i = counts.FindIndex(p => p.Key == stack.itemId);
            if (i < 0)
                counts.Add(new KeyValuePair<string, int>(stack.itemId, stack.count));
            else
                counts[i] = new KeyValuePair<string, int>(stack.itemId, counts[i].Value + stack.count);
        }
        // Wood reads biggest first, as before; everything else in the order it went in.
        string[] woodOrder = { WoodManager.LogsId, WoodManager.BranchesId, WoodManager.SticksId, FireManager.FirewoodId };
        counts.Sort((a, b) => Order(a.Key, woodOrder).CompareTo(Order(b.Key, woodOrder)));
        var parts = new List<string>();
        foreach (KeyValuePair<string, int> pair in counts)
            parts.Add($"{pair.Value} {ItemDatabase.Get(pair.Key)?.DisplayName ?? pair.Key}");
        return parts.Count > 0 ? Shorten(parts) : "empty";
    }

    static int Order(string itemId, string[] order)
    {
        int i = System.Array.IndexOf(order, itemId);
        return i < 0 ? order.Length : i;
    }

    static string Shorten(List<string> parts) =>
        parts.Count <= 4 ? string.Join(", ", parts) : string.Join(", ", parts.GetRange(0, 3)) + $" and {parts.Count - 3} more";

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
        switch (state.kind)
        {
            case PileKind.RockStorage: BuildRocks(random); return;
            case PileKind.WaterBarrel: BuildBarrel(); return;
            case PileKind.FoodCache: BuildBox(new Vector3(1f, 0.6f, 0.7f), 0.8f, true); return;
            case PileKind.StorageBin: BuildBox(new Vector3(1.1f, 0.55f, 0.75f), 1.15f, false); return;
            case PileKind.ToolRack: BuildRack(); return;
            case PileKind.CabinSite: BuildCabinSite(random); return;
        }

        int logs = Mathf.Min(state.Count(WoodManager.LogsId), 6);
        int branches = Mathf.Min(state.Count(WoodManager.BranchesId), 8);
        int sticks = Mathf.Min(state.Count(WoodManager.SticksId), 10);
        int firewood = Mathf.Min(state.Count(FireManager.FirewoodId), 16);

        // A built pile's frame: two poles on the ground that the wood sits across.
        if (state.IsBuilt)
            foreach (float x in new[] { -0.55f, 0.55f })
                Piece(PrimitiveType.Cylinder, new Vector3(x, 0.03f, 0f), new Vector3(90f, 0f, 0f), new Vector3(0.06f, 0.8f, 0.06f), bark);

        for (int i = 0; i < logs; i++)
        {
            float x = ((i % 3) - 1) * 0.36f, y = 0.17f + (i / 3) * 0.3f;
            Piece(PrimitiveType.Cylinder, new Vector3(x, y, 0f), new Vector3(90f, 0f, 0f), new Vector3(0.32f, 0.8f, 0.32f), bark);
        }
        for (int i = 0; i < branches; i++)
            Piece(PrimitiveType.Cylinder, new Vector3(0.8f + (float)random.NextDouble() * 0.4f, 0.05f + i * 0.04f, (float)random.NextDouble() * 1.2f - 0.6f),
                  new Vector3(90f, (float)random.NextDouble() * 60f - 30f, 0f), new Vector3(0.09f, 0.9f, 0.09f), bark);
        for (int i = 0; i < sticks; i++)
            Piece(PrimitiveType.Cylinder, new Vector3(-0.8f + (float)random.NextDouble() * 0.3f, 0.03f + i * 0.02f, (float)random.NextDouble() * 0.6f - 0.3f),
                  new Vector3(90f, (float)random.NextDouble() * 180f, 0f), new Vector3(0.035f, 0.35f, 0.035f), bark);
        for (int i = 0; i < firewood; i++)
            Piece(PrimitiveType.Cylinder, new Vector3(((i % 4) - 1.5f) * 0.2f, 0.09f + (i / 4) * 0.17f, -0.85f),
                  new Vector3(90f, 0f, 0f), new Vector3(0.17f, 0.2f, 0.17f), bark);

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

    // A Small Cabin build site: four corner stakes mark the plot even empty, then each of the five materials stacks
    // up in its own corner as it's deposited — visibly growing over however many trips it takes, same as any other
    // pile (Building_Housing_System.md's staged-deposit build).
    void BuildCabinSite(System.Random random)
    {
        var clayColor = new Color(0.45f, 0.3f, 0.2f);
        var grassColor = new Color(0.62f, 0.55f, 0.28f);

        foreach (Vector2 corner in new[] { new Vector2(-1.5f, -1.5f), new Vector2(1.5f, -1.5f), new Vector2(-1.5f, 1.5f), new Vector2(1.5f, 1.5f) })
            Piece(PrimitiveType.Cylinder, new Vector3(corner.x, 0.25f, corner.y), Vector3.zero, new Vector3(0.04f, 0.25f, 0.04f), bark);

        int logs = Mathf.Min(state.Count(WoodManager.LogsId), 20);
        int branches = Mathf.Min(state.Count(WoodManager.BranchesId), 14);
        int grass = Mathf.Min(state.Count(WoodManager.TallGrassId), 15);
        int stone = Mathf.Min(state.Count(WoodManager.StoneId), 16);
        int clay = Mathf.Min(state.Count(WoodManager.ClayId), 10);

        for (int i = 0; i < logs; i++)
        {
            float x = -1.3f + (i % 5) * 0.3f, y = 0.17f + (i / 5) * 0.3f;
            Piece(PrimitiveType.Cylinder, new Vector3(x, y, -1.1f), new Vector3(90f, 0f, 0f), new Vector3(0.32f, 0.85f, 0.32f), bark);
        }
        for (int i = 0; i < branches; i++)
            Piece(PrimitiveType.Cylinder, new Vector3(1.1f + (float)random.NextDouble() * 0.5f, 0.05f + i * 0.035f, -1.3f + (float)random.NextDouble() * 0.7f),
                  new Vector3(90f, (float)random.NextDouble() * 40f - 20f, 0f), new Vector3(0.09f, 0.75f, 0.09f), bark);
        for (int i = 0; i < stone; i++)
        {
            float a = (float)random.NextDouble() * Mathf.PI * 2f, r = (float)random.NextDouble() * 0.4f;
            Stone(new Vector3(-1.2f + Mathf.Cos(a) * r, 0.1f + i * 0.02f, 1.2f + Mathf.Sin(a) * r), 0.24f, random);
        }
        for (int i = 0; i < clay; i++)
            Tinted(Piece(PrimitiveType.Sphere, new Vector3(-0.4f + (i % 3) * 0.3f, 0.08f, 1.3f + (i / 3) * 0.25f), Vector3.zero,
                        new Vector3(0.22f, 0.13f, 0.22f), null), clayColor);
        for (int i = 0; i < grass; i++)
            Tinted(Piece(PrimitiveType.Cylinder, new Vector3(0.9f + (float)random.NextDouble() * 0.5f, 0.02f + i * 0.015f, 0.9f + (float)random.NextDouble() * 0.5f),
                        new Vector3((float)random.NextDouble() * 20f, (float)random.NextDouble() * 360f, 80f), new Vector3(0.02f, 0.35f, 0.02f), null), grassColor);

        box.center = new Vector3(0f, 0.35f, 0f);
        box.size = new Vector3(3.4f, 0.9f, 3.4f);
    }

    // A hollowed-log barrel with the water showing at its level.
    void BuildBarrel()
    {
        const float height = 0.95f;
        Piece(PrimitiveType.Cylinder, new Vector3(0f, height / 2f, 0f), Vector3.zero, new Vector3(0.7f, height / 2f, 0.7f), bark);
        float fill = WoodManager.Instance != null ? Mathf.Clamp01(state.Total / (float)WoodManager.Instance.BarrelLitres) : 0f;
        if (fill > 0f)
            Tinted(Piece(PrimitiveType.Cylinder, new Vector3(0f, 0.08f + (height - 0.1f) * fill, 0f), Vector3.zero, new Vector3(0.62f, 0.005f, 0.62f), null),
                   new Color(0.22f, 0.3f, 0.34f));
        box.center = new Vector3(0f, height / 2f, 0f);
        box.size = new Vector3(0.75f, height, 0.75f);
    }

    // A standing frame with a peg for each kind of Tool stored (up to 6), filling left to right, top to bottom.
    void BuildRack()
    {
        const float height = 1.1f, width = 0.9f;
        Piece(PrimitiveType.Cube, new Vector3(0f, height / 2f, 0f), Vector3.zero, new Vector3(width, height, 0.06f), bark);
        foreach (Vector3 post in new[] { new Vector3(-width / 2f, 0f, 0f), new Vector3(width / 2f, 0f, 0f) })
            Piece(PrimitiveType.Cylinder, post + new Vector3(0f, 0.02f, 0.03f), Vector3.zero, new Vector3(0.05f, 0.02f, 0.05f), bark);

        int kinds = 0;
        foreach (WoodStack stack in state.contents)
            if (stack.count > 0)
                kinds++;
        kinds = Mathf.Min(kinds, 6);
        for (int i = 0; i < kinds; i++)
        {
            float x = -width / 2f + 0.18f + (i % 3) * (width - 0.36f) / 2f;
            float y = height - 0.22f - (i / 3) * 0.4f;
            Piece(PrimitiveType.Cylinder, new Vector3(x, y, 0.09f), new Vector3(90f, 0f, 0f), new Vector3(0.035f, 0.16f, 0.035f), bark);
        }
        box.center = new Vector3(0f, height / 2f, 0f);
        box.size = new Vector3(width + 0.3f, height + 0.1f, 0.5f);
    }

    // A lidded box of split wood (the Food Cache) or an open, lighter-coloured bin (the Storage Bin).
    void BuildBox(Vector3 size, float tint, bool lid)
    {
        GameObject body = Piece(PrimitiveType.Cube, new Vector3(0f, size.y / 2f, 0f), Vector3.zero, size, bark);
        if (tint != 1f)
            Tinted(body, new Color(0.27f * tint, 0.2f * tint, 0.14f * tint));
        if (lid)
            Piece(PrimitiveType.Cube, new Vector3(0f, size.y + 0.03f, 0f), new Vector3(0f, 0f, 2f), new Vector3(size.x + 0.08f, 0.06f, size.z + 0.08f), bark);
        else
            Tinted(Piece(PrimitiveType.Cube, new Vector3(0f, size.y - 0.01f, 0f), Vector3.zero, new Vector3(size.x - 0.1f, 0.02f, size.z - 0.1f), null),
                   new Color(0.08f, 0.06f, 0.05f)); // the dark inside of an open bin
        box.center = new Vector3(0f, size.y / 2f, 0f);
        box.size = new Vector3(size.x + 0.1f, size.y + 0.1f, size.z + 0.1f);
    }

    void Stone(Vector3 position, float size, System.Random random)
    {
        GameObject stone = Piece(PrimitiveType.Sphere, position,
                                 new Vector3((float)random.NextDouble() * 360f, (float)random.NextDouble() * 360f, 0f),
                                 new Vector3(size * (0.8f + (float)random.NextDouble() * 0.5f), size * 0.6f, size), null);
        float g = 0.38f + (float)random.NextDouble() * 0.12f;
        Tinted(stone, new Color(g, g * 0.98f, g * 0.95f));
    }

    static void Tinted(GameObject piece, Color color)
    {
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        piece.GetComponent<MeshRenderer>().SetPropertyBlock(block);
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
