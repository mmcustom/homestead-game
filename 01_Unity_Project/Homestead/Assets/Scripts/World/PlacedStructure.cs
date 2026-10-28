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
            case PileKind.LeachHopperSite: BuildOuthouseSite(parent, state, bark, pieces, random, 0.7f, out boxCenter, out boxSize); return;
            case PileKind.LeachHopper: BuildLeachHopper(parent, state, bark, pieces, out boxCenter, out boxSize); return;
            case PileKind.KitchenSite:
            case PileKind.KitchenRoofSite: BuildKitchenSite(parent, state, bark, pieces, random, out boxCenter, out boxSize); return;
            case PileKind.Kitchen: BuildKitchen(parent, state, bark, pieces, random, out boxCenter, out boxSize); return;
            case PileKind.Countertop: BuildCountertop(parent, bark, pieces, out boxCenter, out boxSize); return;
            case PileKind.Grill: BuildGrill(parent, bark, pieces, random, out boxCenter, out boxSize); return;
            case PileKind.SlopSink: BuildSlopSink(parent, state, bark, pieces, out boxCenter, out boxSize); return;
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

    static readonly Color AshColor = new Color(0.55f, 0.55f, 0.52f);
    static readonly Color LyeColor = new Color(0.62f, 0.5f, 0.28f);

    // The Leach Hopper (Potash B): four legs, a wooden trough of Ash with water dripping through it into a clay pot beneath. The fill rises with what it
    // holds and is grey while the Ash is leaching, brown once it is Lye.
    static void BuildLeachHopper(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        foreach (Vector2 corner in new[] { new Vector2(-0.42f, -0.42f), new Vector2(0.42f, -0.42f), new Vector2(-0.42f, 0.42f), new Vector2(0.42f, 0.42f) })
            Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(corner.x, 0.45f, corner.y), Vector3.zero, new Vector3(0.1f, 0.45f, 0.1f), bark);
        Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.92f, 0f), Vector3.zero, new Vector3(1.05f, 0.1f, 1.05f), bark);
        foreach (Vector2 side in new[] { new Vector2(0f, -0.5f), new Vector2(0f, 0.5f) })
            Piece(parent, pieces, PrimitiveType.Cube, new Vector3(side.x, 1.15f, side.y), Vector3.zero, new Vector3(1.05f, 0.4f, 0.08f), bark);
        foreach (Vector2 side in new[] { new Vector2(-0.5f, 0f), new Vector2(0.5f, 0f) })
            Piece(parent, pieces, PrimitiveType.Cube, new Vector3(side.x, 1.15f, side.y), Vector3.zero, new Vector3(0.08f, 0.4f, 1.05f), bark);

        int held = state.Total;
        if (held > 0)
        {
            float level = 0.04f + 0.3f * Mathf.Clamp01(held / (float)PotashRules.HopperCapacity);
            Color fill = state.Count(PotashRules.LyeId) > 0 && state.leachHours <= 0f ? LyeColor : AshColor;
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.97f + level / 2f, 0f), Vector3.zero, new Vector3(0.95f, level, 0.95f), null), fill);
        }

        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.2f, 0f), Vector3.zero, new Vector3(0.5f, 0.2f, 0.5f), null), ClayColor);
        if (state.leachHours > 0f)
            Tinted(Piece(parent, pieces, PrimitiveType.Sphere, new Vector3(0f, 0.55f, 0f), Vector3.zero, new Vector3(0.06f, 0.3f, 0.06f), null), new Color(0.2f, 0.28f, 0.34f));

        boxCenter = new Vector3(0f, 0.7f, 0f);
        boxSize = new Vector3(1.2f, 1.4f, 1.2f);
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
        WorkTable(parent, pieces, bark, new Vector3(0.95f, 0f, -postZ + 0.35f), 1.1f, 0.6f, 0.85f, true);

        boxCenter = new Vector3(0f, top / 2f, 0f);
        boxSize = new Vector3(0.1f, 0.1f, 0.1f); // unused: the finished kitchen's own pieces are what is aimed at and bumped into
    }

    // A work table: a plank top on four legs, centred at (at.x, at.z) with its top at topY. The finished kitchen's prep table (solid: it is the
    // kitchen's own collider) and the Countertop (not solid: the Countertop has its own interaction box) are both drawn with it.
    static void WorkTable(Transform parent, List<GameObject> pieces, Material bark, Vector3 at, float width, float depth, float topY, bool solid)
    {
        var wood = new Color(0.33f, 0.25f, 0.17f);
        GameObject top = Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(at.x, topY, at.z), Vector3.zero, new Vector3(width, 0.07f, depth), bark), wood);
        if (solid)
            Solid(top);
        float legHeight = topY - 0.03f;
        foreach (Vector2 leg in new[] { new Vector2(-1f, -1f), new Vector2(1f, -1f), new Vector2(-1f, 1f), new Vector2(1f, 1f) })
        {
            GameObject post = Piece(parent, pieces, PrimitiveType.Cube,
                                    new Vector3(at.x + leg.x * (width / 2f - 0.07f), legHeight / 2f, at.z + leg.y * (depth / 2f - 0.06f)), Vector3.zero,
                                    new Vector3(0.07f, legHeight, 0.07f), bark);
            if (solid)
                Solid(post);
        }
    }

    // --- Kitchen add-ons, batch 1. Each is drawn about its slot's middle, facing local +Z (the middle of the kitchen), 0.88 m wide and
    // 0.5 m deep at most, and every collider stays under 1.5 m so it never reads as a roof to OverheadCover (shelter, shade, Bow Drill cover). ---

    // A work surface about 0.9 m high with a lower board, a back lip and a chopping board on top.
    static void BuildCountertop(Transform parent, Material bark, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        WorkTable(parent, pieces, bark, Vector3.zero, 0.88f, 0.5f, 0.9f, false);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.3f, 0f), Vector3.zero, new Vector3(0.78f, 0.04f, 0.42f), bark), new Color(0.3f, 0.23f, 0.15f));
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.97f, -0.23f), Vector3.zero, new Vector3(0.88f, 0.1f, 0.04f), bark), new Color(0.33f, 0.25f, 0.17f));
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(-0.15f, 0.945f, 0.02f), new Vector3(0f, 8f, 0f), new Vector3(0.36f, 0.025f, 0.24f), null), new Color(0.55f, 0.43f, 0.28f));

        boxCenter = new Vector3(0f, 0.5f, 0f);
        boxSize = new Vector3(0.88f, 1f, 0.5f);
    }

    // A stone-and-clay firebox with a flat stone top either side of the fire (the fire itself is a second Campfire, FireManager, standing on the
    // firebox at WoodManager.GrillFireSpot) and a rack of green sticks over it. The collider is the firebox only, so the fire above has its own.
    static void BuildGrill(Transform parent, Material bark, List<GameObject> pieces, System.Random random, out Vector3 boxCenter, out Vector3 boxSize)
    {
        const float height = WoodManager.GrillFireHeight;
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, height / 2f, 0f), Vector3.zero, new Vector3(0.88f, height, 0.5f), null), ClayColor);
        foreach (float x in new[] { -0.33f, 0.33f })
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(x, height + 0.015f, 0f), Vector3.zero, new Vector3(0.2f, 0.03f, 0.5f), null), SlabColor); // flat stone top
        foreach (float x in new[] { -0.44f, 0.44f })
            for (int i = 0; i < 2; i++)
                Stone(parent, pieces, new Vector3(x, 0.07f, -0.17f + i * 0.34f), 0.2f, random);

        // The rack: two forked posts either side of the fire and green sticks laid across them.
        var green = new Color(0.34f, 0.4f, 0.2f);
        foreach (float x in new[] { -0.4f, 0.4f })
            Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(x, height + 0.17f, 0.18f), Vector3.zero, new Vector3(0.04f, 0.17f, 0.04f), bark);
        for (int i = 0; i < 5; i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, height + 0.32f, -0.12f + i * 0.08f), new Vector3(0f, 0f, 90f), new Vector3(0.025f, 0.43f, 0.025f), bark), green);

        boxCenter = new Vector3(0f, height / 2f, 0f);
        boxSize = new Vector3(0.88f, height, 0.5f);
    }

    // A work-height table with a clay basin sunk in the top: a rim of boards, the water showing at its level, and a spout at the front.
    static void BuildSlopSink(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        WorkTable(parent, pieces, bark, Vector3.zero, 0.88f, 0.5f, 0.85f, false);
        var wood = new Color(0.33f, 0.25f, 0.17f);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.895f, 0f), Vector3.zero, new Vector3(0.64f, 0.03f, 0.36f), null), ClayColor);
        foreach (float z in new[] { -0.19f, 0.19f })
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.93f, z), Vector3.zero, new Vector3(0.7f, 0.1f, 0.04f), bark), wood);
        foreach (float x in new[] { -0.33f, 0.33f })
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(x, 0.93f, 0f), Vector3.zero, new Vector3(0.04f, 0.1f, 0.4f), bark), wood);

        int capacity = WellsAndOuthouses.Instance != null ? WellsAndOuthouses.Instance.slopSinkLitres : 20;
        float fill = Mathf.Clamp01(state.Total / (float)Mathf.Max(1, capacity));
        if (fill > 0f)
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.905f + 0.03f * fill, 0f), Vector3.zero, new Vector3(0.6f, 0.004f, 0.32f), null), WaterColor);
        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.8f, 0.28f), new Vector3(90f, 0f, 0f), new Vector3(0.05f, 0.05f, 0.05f), null), ClayColor); // drain spout

        boxCenter = new Vector3(0f, 0.5f, 0f);
        boxSize = new Vector3(0.88f, 1f, 0.5f);
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

        int buffer = WellsAndOuthouses.WellNumbers(state).bufferLitres; // this well's own (Shallow 200, Medium and old wells 150, Deep 120)
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
                case PileKind.Countertop: return Name + "\nA work surface under the kitchen roof - nothing to do at it yet";
                case PileKind.Grill: return GrillPrompt();
                case PileKind.SlopSink: return SinkPrompt();
                case PileKind.LeachHopper: return HopperPrompt();
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
            int lining = WellsAndOuthouses.NeedOf(state, WoodManager.StoneId); // this well's own: its depth to water sets it
            if (state.Count(WoodManager.StoneId) < lining)
                return $"Line it with Stone: {state.Count(WoodManager.StoneId)} of {lining} (R stores Stone you carry)";
            return $"Cap and windlass: {state.Count(WoodManager.LogsId)} of {w.wellCapLogs} Logs, {state.Count(WoodManager.BranchesId)} of {w.wellCapBranches} Branches (R stores them)";
        }
        // Any other staged site: what it holds against what it needs, material by material.
        var parts = new List<string>();
        foreach (WoodStack c in WellsAndOuthouses.FinishedCostFor(state))
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

    // --- Kitchen add-ons: the Wood-Fired Grill and the Slop Sink ---

    string GrillFire()
    {
        CampfireState fire = WoodManager.Instance != null ? WoodManager.Instance.GrillFireOf(state) : null;
        if (fire == null)
            return "no fire";
        return fire.lit ? $"lit, {fire.fuelHours:0.#} h of fuel" : fire.fuelHours > 0f ? $"out, {fire.fuelHours:0.#} h of fuel" : "out";
    }

    string GrillPrompt() => $"{Name}  ({GrillFire()})\nLight and feed the fire on top; cook meat and fish at it from the Inventory";

    // Water in the sink is kept by quality in the pile's contents, like a barrel's; best quality first when drawn.
    static readonly string[] WaterOrder = { Cooking.PurifiedWaterId, "water_excellent", "water_good", "water_questionable", "water_unsafe" };
    static float lastDrainTime = -10f;

    int SinkCapacity => WoodManager.Instance != null ? WoodManager.Instance.CapacityOf(PileKind.SlopSink) : 20;

    static int ContainerRoom() =>
        Inventory == null ? 0 : Mathf.Max(0, WaterQualities.Capacity(Inventory.Player) - WaterQualities.LitresCarried(Inventory.Player));

    string SinkPrompt()
    {
        int litres = state.Total;
        if (litres <= 0)
            return $"{Name}  (empty, holds {SinkCapacity} L)";
        return ContainerRoom() > 0 ? $"Draw water  ({litres} of {SinkCapacity} L in the {Name})" : $"{Name}  ({litres} of {SinkCapacity} L of water)";
    }

    string SinkSecondaryPrompt()
    {
        int carried = Inventory != null ? WaterQualities.LitresCarried(Inventory.Player) : 0;
        int room = SinkCapacity - state.Total;
        if (carried > 0 && room > 0)
            return $"Pour In Water  ({Mathf.Min(room, carried)} L)";
        if (state.Total > 0)
            return $"Drain the {Name}  (empties it onto the ground)";
        return WoodManager.HammerEquipped ? $"Dismantle the {Name}  ({RefundText()})"
                                          : $"Dismantle the {Name}  (needs the Hammer; half the materials back, rounded down)";
    }

    // E: fills the carried Buckets and Canteens, best quality first, as the Water Barrel does.
    void DrawFromSink()
    {
        if (state.Total <= 0)
        {
            ToolStatus.Flash($"The {Name} is empty.", 2.5f);
            return;
        }
        int room = ContainerRoom();
        if (room <= 0)
        {
            ToolStatus.Flash(WaterQualities.Capacity(Inventory.Player) > 0 ? "Your water containers are full." : "Needs a Bucket or Canteen to carry water.", 3f);
            return;
        }

        int drawn = 0;
        foreach (string id in WaterOrder)
        {
            if (room <= 0)
                break;
            ItemDefinition water = ItemDatabase.Get(id);
            int n = water != null ? Mathf.Min(Mathf.Min(room, state.Count(id)), Inventory.Player.SpaceFor(water)) : 0;
            if (n <= 0)
                continue;
            int added = Inventory.AddToPlayer(id, n);
            state.Take(id, added);
            room -= added;
            drawn += added;
        }
        if (drawn <= 0)
        {
            ToolStatus.Flash("Too heavy - no room to carry any of it", 3f);
            return;
        }
        WoodManager.Instance.NotifyChanged(state);
        ToolStatus.Flash($"Filled {drawn} L from the {Name}  ({state.Total} L left)", 3f);
    }

    // R: pours carried water in (up to the cap), else drains what is in it onto the ground, else (empty) dismantles it with the Hammer.
    void SinkSecondary()
    {
        int carried = WaterQualities.LitresCarried(Inventory.Player);
        int room = SinkCapacity - state.Total;
        if (carried > 0 && room > 0)
        {
            int poured = 0;
            foreach (string id in WaterOrder)
            {
                if (room <= 0)
                    break;
                int n = Mathf.Min(room, Inventory.Player.Count(id));
                if (n <= 0)
                    continue;
                int removed = Inventory.RemoveFromPlayer(id, n);
                state.Add(id, removed);
                room -= removed;
                poured += removed;
            }
            if (poured > 0)
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.Play(SoundCue.Pour);
                WoodManager.Instance.NotifyChanged(state);
                ToolStatus.Flash($"Poured in {poured} L - the {Name} holds {state.Total} L", 3f);
            }
            return;
        }

        if (state.Total > 0)
        {
            state.contents.Clear();
            lastDrainTime = Time.unscaledTime;
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play(SoundCue.Pour);
            WoodManager.Instance.NotifyChanged(state);
            ToolStatus.Flash($"Drained the {Name}.", 3f);
            return;
        }

        if (Time.unscaledTime - lastDrainTime < 1.5f)
            return; // the press that drained it must not also take the empty sink down
        Dismantle();
    }

    // --- The Leach Hopper (Potash B): R loads Ash and water, E collects the Lye, R with the Hammer dismantles ---

    bool Leaching => state.leachHours > 0f;

    int[] CarriedWater()
    {
        var litres = new int[PotashRules.WaterWorstFirst.Length];
        for (int i = 0; i < litres.Length; i++)
            litres[i] = Inventory.Player.Count(PotashRules.WaterWorstFirst[i]);
        return litres;
    }

    PotashRules.LoadPlan CarriedLoad() =>
        PotashRules.PlanLoad(Inventory.Player.Count(AshRules.ItemId), CarriedWater(), PotashRules.Room(state.Total));

    string HopperPrompt()
    {
        int ready = state.Count(PotashRules.LyeId), pending = state.Count(AshRules.ItemId);
        if (Leaching)
            return $"{Name}  (leaching {pending} Lye, {PotashRules.HoursText(state.leachHours)} to go{(ready > 0 ? $", {ready} ready" : "")})";
        if (ready > 0)
            return $"Collect Lye  ({ready} ready in the {Name})";
        return $"{Name}  (empty, holds {PotashRules.HopperCapacity} Lye: 1 Ash and 1 L of water each)";
    }

    string HopperSecondaryPrompt()
    {
        if (WoodManager.HammerEquipped)
            return $"Dismantle the {Name}  (half the Logs and Tall Grass back, rounded down; the Ash and Lye in it come back, the water is lost)";
        if (Leaching)
            return $"Still leaching - {PotashRules.HoursText(state.leachHours)} to go  (loading waits; equip the Hammer to dismantle)";
        PotashRules.LoadPlan plan = CarriedLoad();
        if (plan.lye > 0)
            return $"Load {plan.ash} Ash and {PotashRules.WaterFor(plan.lye)} L of water  ({plan.lye} Lye in {PotashRules.HoursText(PotashRules.LeachGameHours)})";
        return "Load Ash and water  (1 Ash and 1 L each; the worst water goes in first. Equip the Hammer to dismantle)";
    }

    // E: the Lye that has finished leaching, as much as the pack can carry.
    void CollectLye()
    {
        int ready = state.Count(PotashRules.LyeId);
        if (ready <= 0)
        {
            ToolStatus.Flash(Leaching ? $"Still leaching - {PotashRules.HoursText(state.leachHours)} to go." : $"The {Name} is empty. Load Ash and water with R.", 3f);
            return;
        }
        ItemDefinition lye = ItemDatabase.Get(PotashRules.LyeId);
        int fits = lye != null ? Mathf.Min(ready, Inventory.Player.SpaceFor(lye)) : 0;
        if (fits <= 0)
        {
            ToolStatus.Flash("Too heavy - no room to carry any of it", 3f);
            return;
        }
        int added = Inventory.AddToPlayer(PotashRules.LyeId, fits);
        state.Take(PotashRules.LyeId, added);
        WoodManager.Instance.NotifyChanged(state);
        ToolStatus.Flash(added < ready ? $"Took {added} Lye  ({ready - added} left in the {Name}, no room)" : $"Took {added} Lye", 3f);
    }

    // R: with the Hammer it dismantles; without, it loads Ash and water (worst water first), unless a batch is still leaching.
    void HopperSecondary()
    {
        if (WoodManager.HammerEquipped)
        {
            Dismantle();
            return;
        }
        if (Leaching)
        {
            ToolStatus.Flash(PotashRules.LoadRefusedWhileLeaching(state.leachHours), 3.5f);
            return;
        }

        PotashRules.LoadPlan plan = CarriedLoad();
        if (plan.lye <= 0)
        {
            int ash = Inventory.Player.Count(AshRules.ItemId), water = 0;
            foreach (int litres in CarriedWater())
                water += litres;
            ToolStatus.Flash(PotashRules.Room(state.Total) <= 0 ? $"The {Name} is full - collect the Lye (E)."
                           : ash <= 0 ? "You need Ash to load it (1 Ash and 1 L of water per Lye)."
                           : water <= 0 ? "You need water to load it (1 L per Lye; raw or boiled)." : "Nothing to load.", 3.5f);
            return;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval();
        Inventory.RemoveFromPlayer(AshRules.ItemId, plan.ash);
        int litresUsed = 0;
        for (int i = 0; i < plan.water.Length; i++)
        {
            if (plan.water[i] <= 0)
                continue;
            litresUsed += Inventory.RemoveFromPlayer(PotashRules.WaterWorstFirst[i], plan.water[i]);
        }
        state.Add(AshRules.ItemId, plan.ash);
        state.leachHours = PotashRules.LeachGameHours;
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.Pour);
        WoodManager.Instance.NotifyChanged(state);
        ToolStatus.Flash(PotashRules.LoadedMessage(plan.lye, plan.ash, litresUsed), 4f);
    }

    string WellPrompt()
    {
        WellsAndOuthouses w = Rules;
        WaterQuality quality = w.WellQuality(state, out string note);
        string detail = note.Length > 0 ? $", {note}" : "";
        if (state.litres <= 0)
            return $"Well  (dry for now - it recovers {w.RecoveryPerHour(state):0.#} L per hour){(state.contaminatedHours > 0f ? ", flood water got in" : "")}";
        return $"Draw water  ({quality} water{detail}, {state.litres} of {w.NumbersOf(state).bufferLitres} L)";
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
            if (state.kind == PileKind.SlopSink)
                return SinkSecondaryPrompt();
            if (state.kind == PileKind.LeachHopper)
                return HopperSecondaryPrompt();
            if (state.kind == PileKind.Grill)
            {
                CampfireState fire = WoodManager.Instance.GrillFireOf(state);
                if (fire != null && fire.lit)
                    return $"Dismantle the {Name}  (put its fire out first)";
            }
            return WoodManager.HammerEquipped ? $"Dismantle the {Name}  ({RefundText()})"
                                              : $"Dismantle the {Name}  (needs the Hammer; half the materials back, rounded down)";
        }
    }

    // e.g. "half the materials back, rounded down: 5 Stone, 1 Log, 2 Branches - a single item refunds nothing".
    string RefundText()
    {
        var parts = new List<string>();
        foreach (WoodStack cost in WoodManager.Instance.CostOf(state))
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
            case PileKind.Countertop: ToolStatus.Flash($"{Name}: a work surface under the roof. Nothing to do at it yet.", 3f); break;
            case PileKind.Grill: ToolStatus.Flash($"{Name}: {GrillFire()}. Light it, feed it and cook at the fire on top.", 3.5f); break;
            case PileKind.SlopSink: DrawFromSink(); break;
            case PileKind.LeachHopper: CollectLye(); break;
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
            ToolStatus.Flash($"The well is dry for now - it recovers {w.RecoveryPerHour(state):0.#} L per hour", 3.5f);
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
        else if (state.kind == PileKind.SlopSink)
        {
            SinkSecondary();
        }
        else if (state.kind == PileKind.LeachHopper)
        {
            HopperSecondary();
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
            int need = WellsAndOuthouses.NeedOf(state, id);
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
        if (state.IsKitchenAddon)
        {
            // Hammer in hand; a grill must be out and a sink empty. WoodManager says why not, or what came back.
            ToolStatus.Flash(WoodManager.Instance.DismantleAddon(state, transform), 5f);
            return;
        }
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
        foreach (WoodStack cost in WoodManager.Instance.CostOf(state)) // a Well gives back half of the lining its own depth took
            refund.Return(cost.itemId, cost.count / 2);
        string name = Name;
        // A Leach Hopper gives back the Ash still leaching and the Lye waiting in it, whole; the water is lost.
        if (state.kind == PileKind.LeachHopper)
        {
            refund.Return(AshRules.ItemId, state.Count(AshRules.ItemId));
            refund.Return(PotashRules.LyeId, state.Count(PotashRules.LyeId));
        }
        WoodManager.Instance.RemovePile(state);
        ToolStatus.Flash(refund.Describe(name), 5f);
    }
}
