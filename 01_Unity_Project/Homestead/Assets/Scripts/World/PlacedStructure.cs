using System.Collections.Generic;
using UnityEngine;

// What a Well Site, a Well or an Outhouse looks like. Pure meshes from primitives with no colliders and no components, so the same
// builder serves the real structure (PlacedStructure) and Placement Mode's ghost (PlacementGhost.ForPile) without either being
// registered with anything.
public static class StructureVisuals
{
    static readonly Color WaterColor = new Color(0.12f, 0.2f, 0.26f);
    static readonly Color SoilColor = new Color(0.2f, 0.14f, 0.09f);

    // Builds the pieces under parent (appending each to pieces) and reports the interaction box to use.
    public static void Build(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        var random = new System.Random(state.id * 7919);
        switch (state.kind)
        {
            case PileKind.Well: BuildWell(parent, state, bark, pieces, random, out boxCenter, out boxSize); return;
            case PileKind.Outhouse: BuildOuthouse(parent, state, bark, pieces, out boxCenter, out boxSize); return;
            case PileKind.OuthouseSite: BuildOuthouseSite(parent, state, bark, pieces, random, 0.6f, out boxCenter, out boxSize); return;
            case PileKind.WaterBarrelSite:
            case PileKind.RainBarrelSite: BuildOuthouseSite(parent, state, bark, pieces, random, 0.5f, out boxCenter, out boxSize); return;
            case PileKind.FoodCacheSite: BuildOuthouseSite(parent, state, bark, pieces, random, 0.55f, out boxCenter, out boxSize); return;
            case PileKind.WoodShedSite: BuildOuthouseSite(parent, state, bark, pieces, random, 0.9f, out boxCenter, out boxSize); return;
            case PileKind.KitchenSite:
            case PileKind.KitchenRoofSite: BuildKitchenSite(parent, state, bark, pieces, random, out boxCenter, out boxSize); return;
            case PileKind.Kitchen: BuildKitchen(parent, state, bark, pieces, random, out boxCenter, out boxSize); return;
            default: BuildSite(parent, state, bark, pieces, random, out boxCenter, out boxSize); return;
        }
    }

    // A marked-out plot: four stakes, a dark shaft that deepens as it is dug, the lining Stone as a ring, the cap timber to one side.
    static void BuildSite(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, System.Random random, out Vector3 boxCenter, out Vector3 boxSize)
    {
        foreach (Vector2 corner in new[] { new Vector2(-1f, -1f), new Vector2(1f, -1f), new Vector2(-1f, 1f), new Vector2(1f, 1f) })
            Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(corner.x, 0.25f, corner.y), Vector3.zero, new Vector3(0.04f, 0.25f, 0.04f), bark);

        if (state.digProgress > 0f)
        {
            float r = 0.35f + 0.3f * Mathf.Clamp01(state.digProgress);
            Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.012f, 0f), Vector3.zero, new Vector3(r * 2f, 0.01f, r * 2f), null), SoilColor);
        }
        // Spoil heap beside the shaft, growing with the dig.
        int heap = Mathf.CeilToInt(state.digProgress * 6f);
        for (int i = 0; i < heap; i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Sphere, new Vector3(1.2f + (i % 3) * 0.18f, 0.08f + (i / 3) * 0.1f, -0.2f + (i % 2) * 0.2f), Vector3.zero,
                         new Vector3(0.3f, 0.16f, 0.3f), null), SoilColor);

        int stones = Mathf.Min(state.Count(WoodManager.StoneId), 12);
        for (int i = 0; i < stones; i++)
        {
            float a = i / 12f * Mathf.PI * 2f;
            Stone(parent, pieces, new Vector3(Mathf.Cos(a) * 0.62f, 0.1f, Mathf.Sin(a) * 0.62f), 0.26f, random);
        }
        int logs = Mathf.Min(state.Count(WoodManager.LogsId), 4);
        for (int i = 0; i < logs; i++)
            Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(-1.5f, 0.17f + i * 0.3f, -0.3f + (i % 2) * 0.2f), new Vector3(90f, 0f, 0f), new Vector3(0.32f, 0.85f, 0.32f), bark);
        int branches = Mathf.Min(state.Count(WoodManager.BranchesId), 8);
        for (int i = 0; i < branches; i++)
            Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(-1.2f + (float)random.NextDouble() * 0.3f, 0.05f + i * 0.035f, 0.6f + (float)random.NextDouble() * 0.3f),
                  new Vector3(90f, (float)random.NextDouble() * 40f - 20f, 0f), new Vector3(0.09f, 0.75f, 0.09f), bark);

        boxCenter = new Vector3(0f, 0.3f, 0f);
        boxSize = new Vector3(2.4f, 0.7f, 2.4f);
    }

    // A marked-out plot for the shack, or for a barrel or cache (half = half the plot's side): four stakes round a square of bare earth,
    // with the delivered Logs, Branches and Tall Grass lying beside it.
    static void BuildOuthouseSite(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, System.Random random, float half, out Vector3 boxCenter, out Vector3 boxSize)
    {
        foreach (Vector2 corner in new[] { new Vector2(-half, -half), new Vector2(half, -half), new Vector2(-half, half), new Vector2(half, half) })
            Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(corner.x, 0.25f, corner.y), Vector3.zero, new Vector3(0.04f, 0.25f, 0.04f), bark);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.01f, 0f), Vector3.zero, new Vector3(half * 1.85f, 0.01f, half * 1.85f), null), SoilColor);

        int logs = Mathf.Min(state.Count(WoodManager.LogsId), 4);
        for (int i = 0; i < logs; i++)
            Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(-1.4f, 0.17f + i * 0.3f, -0.3f + (i % 2) * 0.2f), new Vector3(90f, 0f, 0f), new Vector3(0.32f, 0.85f, 0.32f), bark);
        int branches = Mathf.Min(state.Count(WoodManager.BranchesId), 8);
        for (int i = 0; i < branches; i++)
            Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(-1.1f + (float)random.NextDouble() * 0.3f, 0.05f + i * 0.035f, 0.6f + (float)random.NextDouble() * 0.3f),
                  new Vector3(90f, (float)random.NextDouble() * 40f - 20f, 0f), new Vector3(0.09f, 0.75f, 0.09f), bark);
        int grass = Mathf.Min(state.Count(WoodManager.TallGrassId), 3);
        for (int i = 0; i < grass; i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Sphere, new Vector3(1.2f + i * 0.2f, 0.07f, 0.1f), Vector3.zero, new Vector3(0.3f, 0.14f, 0.3f), null), new Color(0.52f, 0.46f, 0.24f));

        boxCenter = new Vector3(0f, 0.4f, 0f);
        boxSize = new Vector3(1.6f, 0.9f, 1.6f);
    }

    static readonly Color ClayColor = new Color(0.52f, 0.34f, 0.22f);
    static readonly Color SlabColor = new Color(0.42f, 0.41f, 0.39f);

    // Marks a piece as solid: PlacedStructure gives it a collider (the ghost never does), so a roof can shelter and a post can be walked into.
    static GameObject Solid(GameObject piece)
    {
        piece.name = "Solid " + piece.name;
        return piece;
    }

    // The Covered Outdoor Kitchen's hearth: a flat stone slab with a clay-and-stone backing wall behind where the fire sits (the fire itself
    // is a Campfire, FireManager). Shown from the hearth stage on.
    static void BuildKitchenHearth(Transform parent, List<GameObject> pieces, System.Random random)
    {
        Vector3 at = Setbacks.KitchenLayout.HearthLocal;
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(at.x, at.y / 2f, at.z), Vector3.zero, new Vector3(1.5f, at.y, 1.5f), null), SlabColor);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(at.x, at.y + 0.38f, at.z - 0.72f), Vector3.zero, new Vector3(1.5f, 0.76f, 0.14f), null), ClayColor);
        for (int i = 0; i < 5; i++)
            Stone(parent, pieces, new Vector3(at.x - 0.62f + i * 0.31f, at.y + 0.08f, at.z + 0.7f), 0.26f, random);
        for (int i = 0; i < 4; i++)
            Stone(parent, pieces, new Vector3(at.x - 0.55f + i * 0.37f, at.y + 0.84f, at.z - 0.72f), 0.22f, random);
    }

    // A hearth-stage or roof-stage site: a marked-out plot the size of the roof, four stakes, bare earth, the delivered Stone and Clay beside it;
    // once the hearth is built (the roof stage) the hearth stands on its slab and the Logs, Branches and Tall Grass for the roof lie at the side.
    static void BuildKitchenSite(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, System.Random random, out Vector3 boxCenter, out Vector3 boxSize)
    {
        const float x = Setbacks.KitchenLayout.HalfWidth - 0.1f, z = Setbacks.KitchenLayout.HalfDepth - 0.1f;
        foreach (Vector2 corner in new[] { new Vector2(-x, -z), new Vector2(x, -z), new Vector2(-x, z), new Vector2(x, z) })
            Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(corner.x, 0.25f, corner.y), Vector3.zero, new Vector3(0.04f, 0.25f, 0.04f), bark);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.01f, 0f), Vector3.zero, new Vector3(x * 1.85f, 0.01f, z * 1.85f), null), SoilColor);

        bool roofStage = state.kind == PileKind.KitchenRoofSite;
        if (roofStage)
            BuildKitchenHearth(parent, pieces, random);

        // Delivered hearth materials lie beside the plot until the first blow; the roof stage shows the roof timber.
        int stones = Mathf.Min(state.Count(WoodManager.StoneId), 4);
        for (int i = 0; i < stones; i++)
            Stone(parent, pieces, new Vector3(-1.1f + i * 0.34f, 0.12f, 1.0f), 0.34f, random);
        int clay = Mathf.Min(state.Count(WoodManager.ClayId), 2);
        for (int i = 0; i < clay; i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Sphere, new Vector3(0.9f + i * 0.4f, 0.1f, 1.0f), Vector3.zero, new Vector3(0.36f, 0.2f, 0.3f), null), ClayColor);
        int logs = Mathf.Min(state.Count(WoodManager.LogsId), 4);
        for (int i = 0; i < logs; i++)
            Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(1.3f, 0.17f + i * 0.3f, -0.4f + (i % 2) * 0.2f), new Vector3(90f, 0f, 0f), new Vector3(0.32f, 0.85f, 0.32f), bark);
        int branches = Mathf.Min(state.Count(WoodManager.BranchesId), 8);
        for (int i = 0; i < branches; i++)
            Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(1.0f + (float)random.NextDouble() * 0.3f, 0.05f + i * 0.035f, -1.1f + (float)random.NextDouble() * 0.3f),
                  new Vector3(90f, (float)random.NextDouble() * 40f - 20f, 0f), new Vector3(0.09f, 0.75f, 0.09f), bark);
        int grass = Mathf.Min(state.Count(WoodManager.TallGrassId), 4);
        for (int i = 0; i < grass; i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Sphere, new Vector3(-1.3f, 0.07f, -0.9f + i * 0.2f), Vector3.zero, new Vector3(0.3f, 0.14f, 0.3f), null), new Color(0.52f, 0.46f, 0.24f));

        boxCenter = new Vector3(0f, 0.125f, 0f);
        boxSize = new Vector3(x * 1.8f, 0.25f, z * 1.8f);
    }

    // The finished kitchen: four posts, a plank roof set well above the fire (higher at the open front so rain runs off the back), a prep table at
    // the back, and the hearth on its slab. Posts, roof and table are Solid, so the roof shelters like the Tent's and the posts are walked round.
    static void BuildKitchen(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, System.Random random, out Vector3 boxCenter, out Vector3 boxSize)
    {
        const float postX = Setbacks.KitchenLayout.HalfWidth - 0.15f, postZ = Setbacks.KitchenLayout.HalfDepth - 0.15f, top = Setbacks.KitchenLayout.RoofHeight;
        var wood = new Color(0.33f, 0.25f, 0.17f);

        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.01f, 0f), Vector3.zero, new Vector3(postX * 2f, 0.01f, postZ * 2f), null), SoilColor);
        BuildKitchenHearth(parent, pieces, random);

        foreach (Vector2 corner in new[] { new Vector2(-postX, -postZ), new Vector2(postX, -postZ), new Vector2(-postX, postZ), new Vector2(postX, postZ) })
            Solid(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(corner.x, top / 2f, corner.y), Vector3.zero, new Vector3(0.2f, top / 2f, 0.2f), bark));

        // Roof: overlapping planks laid side by side front to back on a gentle slope, with a beam along each edge.
        const int planks = 8;
        float span = Setbacks.KitchenLayout.HalfDepth * 2f + 0.3f;
        for (int i = 0; i < planks; i++)
        {
            float t = i / (float)(planks - 1);
            float z = Mathf.Lerp(-span / 2f, span / 2f, t);
            float y = top + 0.12f + Mathf.Lerp(-0.1f, 0.1f, t);
            float tone = 0.85f + (float)random.NextDouble() * 0.3f;
            Solid(Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, y, z), new Vector3(-3f, 0f, 0f), new Vector3(Setbacks.KitchenLayout.HalfWidth * 2f + 0.4f, 0.07f, span / planks + 0.14f), bark),
                         new Color(0.36f * tone, 0.27f * tone, 0.18f * tone)));
        }
        foreach (float z in new[] { -postZ, postZ })
            Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, top + (z > 0f ? 0.1f : -0.02f), z), new Vector3(0f, 0f, 90f), new Vector3(0.16f, postX, 0.16f), bark);

        // Prep table at the back right, clear of the fire.
        Vector3 table = new Vector3(0.95f, 0f, -postZ + 0.35f);
        Solid(Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(table.x, 0.85f, table.z), Vector3.zero, new Vector3(1.1f, 0.07f, 0.6f), bark), wood));
        foreach (Vector2 leg in new[] { new Vector2(-0.48f, -0.24f), new Vector2(0.48f, -0.24f), new Vector2(-0.48f, 0.24f), new Vector2(0.48f, 0.24f) })
            Solid(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(table.x + leg.x, 0.41f, table.z + leg.y), Vector3.zero, new Vector3(0.07f, 0.82f, 0.07f), bark));

        boxCenter = new Vector3(0f, top / 2f, 0f);
        boxSize = new Vector3(0.1f, 0.1f, 0.1f); // unused: the finished kitchen's own pieces are what is aimed at and bumped into
    }

    // A stone-lined shaft with the water showing at its level, two posts, a windlass beam and a small plank roof.
    static void BuildWell(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, System.Random random, out Vector3 boxCenter, out Vector3 boxSize)
    {
        for (int layer = 0; layer < 2; layer++)
            for (int i = 0; i < 12; i++)
            {
                float a = (i + layer * 0.5f) / 12f * Mathf.PI * 2f;
                Stone(parent, pieces, new Vector3(Mathf.Cos(a) * 0.62f, 0.12f + layer * 0.22f, Mathf.Sin(a) * 0.62f), 0.3f, random);
            }

        int buffer = WellsAndOuthouses.Instance != null ? WellsAndOuthouses.Instance.wellBufferLitres : 150;
        float fill = Mathf.Clamp01(state.litres / (float)Mathf.Max(1, buffer));
        float contaminated = state.contaminatedHours > 0f ? 1f : 0f;
        Color water = Color.Lerp(WaterColor, new Color(0.3f, 0.22f, 0.12f), contaminated);
        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.04f + 0.14f * fill, 0f), Vector3.zero, new Vector3(1f, 0.01f, 1f), null), water);

        foreach (float x in new[] { -0.8f, 0.8f })
            Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(x, 0.75f, 0f), Vector3.zero, new Vector3(0.1f, 0.75f, 0.1f), bark);
        Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 1.2f, 0f), new Vector3(0f, 0f, 90f), new Vector3(0.09f, 0.8f, 0.09f), bark); // windlass beam
        Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 1.62f, 0f), new Vector3(0f, 0f, 0f), new Vector3(1.9f, 0.05f, 1.3f), bark);       // plank roof
        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0.2f, 0.95f, 0f), Vector3.zero, new Vector3(0.18f, 0.12f, 0.18f), null),
               new Color(0.35f, 0.25f, 0.15f)); // the bucket on its rope

        boxCenter = new Vector3(0f, 0.6f, 0f);
        boxSize = new Vector3(1.9f, 1.2f, 1.7f);
    }

    // A small shack, door toward +Z (the way the ghost is turned), darker as the pit fills.
    static void BuildOuthouse(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        float fill = WellsAndOuthouses.Instance != null ? WellsAndOuthouses.Instance.PitFill(state) : 0f;
        float shade = Mathf.Lerp(1f, 0.55f, fill);
        var wood = new Color(0.3f * shade, 0.22f * shade, 0.15f * shade);

        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.03f, 0f), Vector3.zero, new Vector3(1.2f, 0.06f, 1.2f), bark), wood);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 1f, -0.55f), Vector3.zero, new Vector3(1.1f, 1.9f, 0.08f), bark), wood);   // back
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(-0.55f, 1f, 0f), Vector3.zero, new Vector3(0.08f, 1.9f, 1.1f), bark), wood);   // left
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0.55f, 1f, 0f), Vector3.zero, new Vector3(0.08f, 1.9f, 1.1f), bark), wood);    // right
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0.12f, 0.95f, 0.55f), new Vector3(0f, -35f, 0f), new Vector3(0.7f, 1.8f, 0.06f), bark), wood); // door, ajar
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 2.0f, 0f), new Vector3(8f, 0f, 0f), new Vector3(1.4f, 0.08f, 1.5f), bark),
               new Color(0.52f, 0.46f, 0.24f)); // thatch roof

        boxCenter = new Vector3(0f, 1f, 0f);
        boxSize = new Vector3(1.3f, 2.1f, 1.3f);
    }

    static void Stone(Transform parent, List<GameObject> pieces, Vector3 position, float size, System.Random random)
    {
        GameObject stone = Piece(parent, pieces, PrimitiveType.Sphere, position,
                                 new Vector3((float)random.NextDouble() * 360f, (float)random.NextDouble() * 360f, 0f),
                                 new Vector3(size * (0.8f + (float)random.NextDouble() * 0.5f), size * 0.6f, size), null);
        float g = 0.38f + (float)random.NextDouble() * 0.12f;
        Tinted(stone, new Color(g, g * 0.98f, g * 0.95f));
    }

    static GameObject Tinted(GameObject piece, Color color)
    {
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        piece.GetComponent<MeshRenderer>().SetPropertyBlock(block);
        return piece;
    }

    static GameObject Piece(Transform parent, List<GameObject> pieces, PrimitiveType shape, Vector3 position, Vector3 euler, Vector3 scale, Material material)
    {
        GameObject piece = GameObject.CreatePrimitive(shape);
        Object.DestroyImmediate(piece.GetComponent<Collider>());
        piece.transform.SetParent(parent, false);
        piece.transform.localPosition = position;
        piece.transform.localRotation = Quaternion.Euler(euler);
        piece.transform.localScale = scale;
        if (material != null)
            piece.GetComponent<MeshRenderer>().sharedMaterial = material;
        pieces?.Add(piece);
        return piece;
    }
}

// A Well Site, a Well or an Outhouse standing in the World (WoodManager owns the saved state; this draws it and is what the player
// looks at). Structures Slice 1, phase A.
//   Well Site  Shovel (hold click) digs the shaft; R stores the lining Stone then the cap Logs and Branches and E takes them back, both
//              free of the Hammer. Fully stocked, the Hammer (hold click, AxeTool.UpdateHammering) pounds it into the Well; the Hammer
//              also fills in an empty site. (Building and destroying are the Hammer's; digging is the Shovel's.)
//   Well       E fills a Bucket or Canteen, or drinks, from the buffer. R with the Hammer dismantles it.
//   Outhouse Site  The same staged mechanism with no dig stage (WellsAndOuthouses.IsStagedSite/DeliveryOrder/FinishedCost): R stores
//              2 Logs, 6 Branches and 3 Tall Grass over any number of trips, E takes them back (until the Hammer begins), the Hammer pounds it into the Outhouse. The barrel and cache sites work the same.
//   Outhouse   The Shovel (hold click, real time only) digs a new pit and moves the shack once the pit is full. R with the Hammer dismantles it.
public class PlacedStructure : MonoBehaviour, IInteractable, ISecondaryInteractable
{
    WoodPileState state;
    Material bark;
    BoxCollider box;
    readonly List<GameObject> pieces = new List<GameObject>();

    public WoodPileState State => state;

    static InventoryManager Inventory => InventoryManager.Instance;
    static WellsAndOuthouses Rules => WellsAndOuthouses.Instance;
    string Name => WoodManager.PileName(state.kind);

    public void Bind(WoodPileState pileState, Material barkMaterial)
    {
        state = pileState;
        bark = barkMaterial;
        box = gameObject.AddComponent<BoxCollider>();
        Rebuild();
    }

    public void Rebuild()
    {
        foreach (GameObject piece in pieces)
            if (piece != null)
                Destroy(piece);
        pieces.Clear();
        if (state == null)
            return;

        StructureVisuals.Build(transform, state, bark, pieces, out Vector3 center, out Vector3 size);
        box.center = center;
        box.size = size;
        // The finished kitchen has no interaction box (it would be a solid block to walk into): its roof, posts and table are the colliders,
        // so OverheadCover finds the roof and the prompt shows when any of them is aimed at.
        bool kitchen = state.kind == PileKind.Kitchen;
        box.enabled = !kitchen;
        if (!kitchen)
            return;
        foreach (GameObject piece in pieces)
        {
            if (piece == null || !piece.name.StartsWith("Solid"))
                continue;
            BoxCollider solid = piece.AddComponent<BoxCollider>();
            MeshFilter filter = piece.GetComponent<MeshFilter>();
            if (filter != null && filter.sharedMesh != null && filter.sharedMesh.name.StartsWith("Cylinder"))
                solid.size = new Vector3(1f, 2f, 1f); // the primitive cylinder is two units tall
        }
    }

    static bool Equipped(string itemId) => Inventory != null && Inventory.EquippedTool != null && Inventory.EquippedTool.Id == itemId;

    // --- Prompts ---

    public bool CanInteract(PlayerController player) => state != null && Inventory != null && Rules != null;

    public string InteractionPrompt
    {
        get
        {
            if (state == null || Rules == null)
                return "";
            if (WellsAndOuthouses.IsStagedSite(state.kind))
                return SitePrompt();
            switch (state.kind)
            {
                case PileKind.Well: return WellPrompt();
                case PileKind.Kitchen: return KitchenPrompt();
                default: return OuthousePrompt();
            }
        }
    }

    string SitePrompt()
    {
        string stage = StageNeeds();
        if (!state.IsEmpty)
            return (WellsAndOuthouses.MaterialsLocked(state) ? Name + "  (" + Describe() + ")" : "Take Materials  (" + Describe() + ")") + "\n" + stage;
        // Taking an empty site down is destroying a built structure: the Hammer's job, like Dismantle.
        string removal = !WoodManager.HammerEquipped ? "Equip the Hammer to take the site down"
                       : state.digProgress > 0f ? "E fills the site in (the digging is lost)" : "E removes the site";
        if (state.kind == PileKind.KitchenRoofSite)
            removal = WoodManager.HammerEquipped ? "R dismantles the hearth (fire out)" : "Equip the Hammer to dismantle the hearth (R)";
        // (Name is "Well Site" / "Outhouse Site".)
        return Name + "\n" + stage + "\n" + removal;
    }

    // What the site needs next.
    string StageNeeds()
    {
        WellsAndOuthouses w = Rules;
        if (WellsAndOuthouses.SiteNeeds(state) == null)
        {
            // Fully stocked: the Hammer finishes it (hold click). Once it has begun the materials are built in.
            string percent = $"{state.buildProgress * 100f:0}%";
            string locked = WellsAndOuthouses.MaterialsLocked(state) ? "\nThe materials are built in - E no longer takes them back" : "";
            string what = state.kind == PileKind.KitchenSite ? " the hearth" : state.kind == PileKind.KitchenRoofSite ? " the roof" : "";
            return (WoodManager.HammerEquipped ? $"Hold click with the Hammer to build{what} ({percent})" : $"Fully stocked - equip the Hammer to build{what} ({percent})") + locked;
        }
        if (WellsAndOuthouses.RequiresDig(state.kind) && state.digProgress < 1f)
            return Equipped(AxeTool.ShovelId) ? $"Dig the shaft: hold click ({state.digProgress * 100f:0}%)" : $"Dig the shaft: equip the Shovel, hold click ({state.digProgress * 100f:0}%)";
        if (state.kind == PileKind.WellSite)
        {
            if (state.Count(WoodManager.StoneId) < w.wellLiningStone)
                return $"Line it with Stone: {state.Count(WoodManager.StoneId)} of {w.wellLiningStone} (R stores Stone you carry)";
            return $"Cap and windlass: {state.Count(WoodManager.LogsId)} of {w.wellCapLogs} Logs, {state.Count(WoodManager.BranchesId)} of {w.wellCapBranches} Branches (R stores them)";
        }
        // Any other staged site: what it holds against what it needs, material by material.
        var parts = new List<string>();
        foreach (WoodStack c in WellsAndOuthouses.FinishedCost(state.kind))
            parts.Add($"{state.Count(c.itemId)} of {c.count} {ItemDatabase.Get(c.itemId)?.DisplayName ?? c.itemId}");
        string stageName = state.kind == PileKind.KitchenSite ? "Hearth stage: " : state.kind == PileKind.KitchenRoofSite ? "Hearth built. Roof stage: " : "";
        return stageName + "Needs " + string.Join(", ", parts) + " (R stores what you carry)";
    }

    string Describe()
    {
        var parts = new List<string>();
        foreach (string id in WellsAndOuthouses.DeliveryIds(state.kind))
        {
            int n = state.Count(id);
            if (n > 0)
                parts.Add($"{n} {ItemDatabase.Get(id)?.DisplayName ?? id}");
        }
        return string.Join(", ", parts);
    }

    string KitchenFire()
    {
        CampfireState hearth = WoodManager.Instance != null ? WoodManager.Instance.KitchenHearthOf(state) : null;
        if (hearth == null)
            return "no hearth fire";
        return hearth.lit ? $"hearth lit, {hearth.fuelHours:0.#} h of fuel" : hearth.fuelHours > 0f ? $"hearth out, {hearth.fuelHours:0.#} h of fuel" : "hearth out";
    }

    string KitchenPrompt() => $"{Name}  ({KitchenFire()})\nCook and boil water at the fire; a meal eaten under the roof is a sheltered meal";

    string WellPrompt()
    {
        WellsAndOuthouses w = Rules;
        WaterQuality quality = w.WellQuality(state, out string note);
        string detail = note.Length > 0 ? $", {note}" : "";
        if (state.litres <= 0)
            return $"Well  (dry for now - it recovers {w.RecoveryPerHour():0.#} L per hour){(state.contaminatedHours > 0f ? ", flood water got in" : "")}";
        return $"Draw water  ({quality} water{detail}, {state.litres} of {w.wellBufferLitres} L)";
    }

    string OuthousePrompt()
    {
        WellsAndOuthouses w = Rules;
        float fill = w.PitFill(state);
        if (fill >= 1f)
            {
            string dug = state.digProgress > 0f ? $" ({state.digProgress * 100f:0}%)" : "";
            return Equipped(AxeTool.ShovelId) ? $"Outhouse  (the pit is full)\nHold click with the Shovel to dig a new pit{dug}"
                                              : $"Outhouse  (the pit is full)\nEquip the Shovel and hold click to dig a new pit{dug}";
        }
        return $"Outhouse  (pit {fill * 100f:0}% full{(w.IsSanitary() ? ", the homestead is sanitary" : "")})";
    }

    public string SecondaryPrompt
    {
        get
        {
            if (state == null || Rules == null)
                return "";
            if (WellsAndOuthouses.IsStagedSite(state.kind))
            {
                if (CarriedStorable(out string what) > 0)
                    return $"Store {what}"; // delivering is free; the Hammer finishes the Well
                return state.kind == PileKind.KitchenRoofSite && state.IsEmpty
                    ? (WoodManager.HammerEquipped ? "Dismantle the hearth  (half the Stone and Clay back, rounded down; the fire must be out)" : "Dismantle the hearth  (needs the Hammer)")
                    : "";
            }
            return WoodManager.HammerEquipped ? $"Dismantle the {Name}  ({RefundText()})"
                                              : $"Dismantle the {Name}  (needs the Hammer; half the materials back, rounded down)";
        }
    }

    // e.g. "half the materials back, rounded down: 5 Stone, 1 Log, 2 Branches - a single item refunds nothing".
    string RefundText()
    {
        var parts = new List<string>();
        foreach (WoodStack cost in WoodManager.Instance.CostOf(state.kind))
            if (cost.count / 2 > 0)
                parts.Add($"{cost.count / 2} {ItemDatabase.Get(cost.itemId)?.DisplayName ?? cost.itemId}");
        return "half back, rounded down, so a single item refunds nothing: " + (parts.Count > 0 ? string.Join(", ", parts) : "nothing");
    }

    // --- E ---

    public void Interact(PlayerController player)
    {
        if (state == null || Inventory == null || Rules == null)
            return;
        if (WellsAndOuthouses.IsStagedSite(state.kind))
        {
            InteractSite();
            return;
        }
        switch (state.kind)
        {
            case PileKind.Well: DrawWater(player); break;
            case PileKind.Kitchen: ToolStatus.Flash($"{Name}: {KitchenFire()}. Light it, feed it and cook at the fire itself.", 3.5f); break;
            default: DigNewPit(player); break;
        }
    }

    void InteractSite()
    {
        WoodManager wood = WoodManager.Instance;
        if (state.IsEmpty && state.kind == PileKind.KitchenRoofSite)
        {
            // The hearth stands on the site now: it is not a bare plot to take down, it is dismantled (R).
            ToolStatus.Flash("The hearth is built. Deliver the roof materials (R), or dismantle the hearth (R with the Hammer, fire out).", 4f);
            return;
        }
        if (state.IsEmpty)
        {
            if (!WoodManager.HammerEquipped)
            {
                ToolStatus.Flash("Equip the Hammer to take the site down.", 3f);
                return;
            }
            wood.RemovePile(state);
            ToolStatus.Flash(state.kind == PileKind.WellSite ? "Filled in the well site" : $"Took down the {Name.ToLower()}");
            return;
        }

        if (WellsAndOuthouses.MaterialsLocked(state))
        {
            ToolStatus.Flash("The materials are built in now - keep hammering to finish it.", 3f);
            return;
        }

        int taken = 0;
        foreach (string id in WellsAndOuthouses.DeliveryIds(state.kind))
        {
            ItemDefinition item = ItemDatabase.Get(id);
            int fits = item != null ? Mathf.Min(state.Count(id), Inventory.Player.SpaceFor(item)) : 0;
            foreach (WoodStack batch in state.Take(id, fits))
                taken += WoodManager.IsWood(id) ? Inventory.AddToPlayer(id, batch.count, batch.day) : Inventory.AddToPlayer(id, batch.count); // wood keeps its wet or dry day
        }
        if (taken == 0)
            ToolStatus.Flash("Too heavy - no room to carry any of it");
        else if (!state.IsEmpty)
            ToolStatus.Flash($"Left behind: {Describe()}");
        wood.NotifyChanged(state);
    }

    void DrawWater(PlayerController player)
    {
        WellsAndOuthouses w = Rules;
        if (state.litres <= 0)
        {
            ToolStatus.Flash($"The well is dry for now - it recovers {w.RecoveryPerHour():0.#} L per hour", 3.5f);
            return;
        }

        WaterQuality quality = w.WellQuality(state, out _);
        string itemId = WaterQualities.ItemId(quality);
        string equipped = Inventory.EquippedTool != null ? Inventory.EquippedTool.Id : null;
        if (equipped == WaterSource.BucketItemId || equipped == WaterQualities.CanteenId)
        {
            ItemDefinition water = ItemDatabase.Get(itemId);
            int room = WaterQualities.Capacity(Inventory.Player) - WaterQualities.LitresCarried(Inventory.Player);
            int fill = water != null ? Mathf.Min(Mathf.Min(room, state.litres), Inventory.Player.SpaceFor(water)) : 0;
            if (fill <= 0)
            {
                ToolStatus.Flash("Your water containers are full", 2.5f);
                return;
            }
            fill = Inventory.AddToPlayer(itemId, fill);
            state.litres -= fill;
            WoodManager.Instance.NotifyChanged(state);
            ToolStatus.Flash($"Filled {fill} L of {quality.ToString().ToLower()} water  ({state.litres} L left in the well)", 3f);
            return;
        }

        SurvivalManager survival = SurvivalManager.Instance;
        if (survival == null || survival.Hydration >= SurvivalManager.MaxValue)
        {
            ToolStatus.Flash("You are not thirsty - equip the Bucket or a Canteen to carry water", 3f);
            return;
        }
        if (state.litres < w.wellLitresPerDrink)
        {
            ToolStatus.Flash("The well is dry for now", 2.5f);
            return;
        }

        state.litres -= w.wellLitresPerDrink;
        survival.Drink(w.wellHydrationPerDrink);
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.Drink);
        WoodManager.Instance.NotifyChanged(state);

        // The same chance a drink from any other source carries: the item's per-litre figure scaled to the Hydration given.
        float perLitre = WaterQualities.IllnessChance(quality);
        float chance = 1f - Mathf.Pow(1f - perLitre, w.wellHydrationPerDrink / WaterQualities.HydrationPerLitre);
        SurvivalManager.Sickness before = survival.SicknessLevel;
        if (survival.RollIllness(chance))
            ToolStatus.Flash($"The {quality.ToString().ToLower()} water - {Food.SickMessage(before, survival.SicknessLevel)}", 5f);
    }

    // A full pit: E only says what to do (the digging is the Shovel's, hold click: AxeTool.DigPit). A new pit is dug a couple of
    // metres to the side and the shack moves onto it, only where the Outhouse rules still allow it.
    void DigNewPit(PlayerController player)
    {
        WellsAndOuthouses w = Rules;
        if (!w.PitFull(state))
        {
            ToolStatus.Flash($"The pit still has room ({w.PitFill(state) * 100f:0}% full)", 2.5f);
            return;
        }
        ToolStatus.Flash(Equipped(AxeTool.ShovelId) ? "Hold click with the Shovel on the outhouse to dig a new pit" : "Equip the Shovel, then hold click on the outhouse to dig a new pit", 3.5f);
    }

    // Where a new pit could go: a couple of metres to either side, if the Outhouse rules and the ground allow it.
    public bool FindNewPitSpot(out Vector3 chosen)
    {
        WellsAndOuthouses w = Rules;
        chosen = Vector3.zero;
        foreach (float side in new[] { 1f, -1f })
        {
            Vector3 flat = state.position + transform.right * (side * w.outhouseMoveMetres);
            Terrain terrain = Terrain.activeTerrain;
            var candidate = new Vector3(flat.x, terrain != null ? terrain.SampleHeight(flat) + terrain.transform.position.y : flat.y, flat.z);
            if (!Animal.IsOverWater(candidate, out _) && w.CanPlace(PileKind.Outhouse, candidate, state, out _))
            {
                chosen = candidate;
                return true;
            }
        }
        return false;
    }

    // The last Shovel blow: the shack moves onto the fresh pit and the pit is empty again.
    public void MoveToNewPit(Vector3 chosen)
    {
        state.position = chosen;
        state.builtDay = WellsAndOuthouses.Today;
        state.digProgress = 0f;
        transform.position = chosen;
        WoodManager.Instance.NotifyChanged(state);
    }

    // --- R ---

    public void SecondaryInteract(PlayerController player)
    {
        if (state == null || Inventory == null || Rules == null)
            return;
        if (WellsAndOuthouses.IsStagedSite(state.kind))
        {
            if (state.kind == PileKind.KitchenRoofSite && CarriedStorable(out _) <= 0)
                Dismantle(); // a hearth-built site with nothing to deliver: R takes the hearth down
            else
                StoreMaterials();
        }
        else
        {
            Dismantle();
        }
    }

    // Everything carried that the site's current stage still takes, up to what the Well needs of each.
    int CarriedStorable(out string what)
    {
        what = "";
        int total = 0;
        var names = new List<string>();
        foreach (string id in WellsAndOuthouses.DeliveryIds(state.kind))
        {
            if (!state.Accepts(id))
                continue;
            int n = Inventory.Player.Count(id);
            if (n <= 0)
                continue;
            total += n;
            names.Add(ItemDatabase.Get(id)?.DisplayName ?? id);
        }
        what = string.Join(", ", names);
        return total;
    }

    void StoreMaterials()
    {
        WoodManager wood = WoodManager.Instance;
        bool wasStocked = WellsAndOuthouses.SiteNeeds(state) == null;
        int stored = 0;
        foreach (string id in WellsAndOuthouses.DeliveryIds(state.kind))
        {
            // Asked material by material in delivery order: the Well's lining fills first, which is what lets the cap timber in.
            if (!state.Accepts(id))
                continue;
            int need = WellsAndOuthouses.NeedOf(state.kind, id);
            int n = Mathf.Min(need - state.Count(id), Inventory.Player.Count(id));
            if (n <= 0)
                continue;
            if (AudioManager.Instance != null)
                AudioManager.Instance.SilenceNextRemoval();
            // Wood keeps the day it was last wet through the site (WoodMoisture), so taking it back does not make dry wood wet.
            var days = new List<(int day, int count)>();
            int removed = Inventory.Player.RemoveBatches(id, n, (day, count) => days.Add((day, count)));
            foreach ((int day, int count) in days)
                state.Add(id, count, WoodManager.IsWood(id) ? day : 0);
            stored += removed;
        }

        if (stored <= 0)
        {
            ToolStatus.Flash($"Nothing you carry fits yet - {StageNeeds()}", 4f);
            return;
        }

        wood.NotifyChanged(state);
        // The delivery that supplies the last missing material: a big notice, so the next step isn't missed. The Well is finished
        // from the Build tab with the Hammer (WellsAndOuthouses.FinishSite), as the Small Cabin is.
        if (!wasStocked && WellsAndOuthouses.SiteNeeds(state) == null)
            ToolStatus.Banner($"{Name} fully stocked - equip the Hammer and hold click on it to build it", 10f);
        else
            ToolStatus.Flash($"Stored - {StageNeeds()}", 4f);
    }

    // Hammer Dismantle for the Well and the Outhouse: half of each cost line back, rounded down (so a single item refunds nothing).
    void Dismantle()
    {
        if (state.IsKitchen)
        {
            // Hammer in hand, hearth out (and a hearth site's roof timber taken back): WoodManager says why not, or what came back.
            ToolStatus.Flash(WoodManager.Instance.DismantleKitchen(state, transform), 5f);
            return;
        }
        if (!WoodManager.HammerEquipped)
        {
            ToolStatus.Flash($"Can't take the {Name} down - equip the Hammer.", 3f);
            return;
        }
        var refund = new StructureRefund(state.position, transform);
        foreach (WoodStack cost in WoodManager.Instance.CostOf(state.kind))
            refund.Return(cost.itemId, cost.count / 2);
        string name = Name;
        WoodManager.Instance.RemovePile(state);
        ToolStatus.Flash(refund.Describe(name), 5f);
    }
}
