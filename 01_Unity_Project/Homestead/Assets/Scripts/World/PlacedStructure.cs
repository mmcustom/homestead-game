using System.Collections.Generic;
using UnityEngine;

// What a Well Site, a Well or an Outhouse looks like. Pure meshes from primitives with no colliders and no components, so the same
// builder serves the real structure (PlacedStructure) and Placement Mode's ghost (PlacementGhost.ForPile) without either being
// registered with anything.
public static class StructureVisuals
{
    static readonly Color WaterColor = new Color(0.12f, 0.2f, 0.26f);
    static readonly Color SoilColor = new Color(0.2f, 0.14f, 0.09f);

    // Builds the pieces under parent (appending each to pieces, which may be null) and reports the interaction box to use.
    // THE SWAP POINT: when Resources/Art/<id>.prefab exists for this kind (ArtSwap), the prefab is shown instead and the primitives below are not drawn.
    public static void Build(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        if (ArtSwap.TryBuild(parent, state, bark, pieces, out boxCenter, out boxSize))
            return;
        BuildPrimitive(parent, state, bark, pieces, out boxCenter, out boxSize);
    }

    // The primitive version of every kind: the default, and what the changing contents of a prefab structure are copied from (ArtSwap).
    internal static void BuildPrimitive(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
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
            case PileKind.HandCartSite: BuildHandCartSite(parent, state, bark, pieces, random, out boxCenter, out boxSize); return;
            case PileKind.WorkshopSite: BuildWorkshopSite(parent, state, bark, pieces, random, out boxCenter, out boxSize); return;
            case PileKind.Workshop: BuildWorkshop(parent, state, bark, pieces, random, out boxCenter, out boxSize); return;
            case PileKind.PotterSite: BuildPotterSite(parent, state, bark, pieces, random, out boxCenter, out boxSize); return;
            case PileKind.PotterShed: BuildPotterShed(parent, state, bark, pieces, random, out boxCenter, out boxSize); return;
            case PileKind.KilnSite: BuildKilnSite(parent, state, bark, pieces, random, out boxCenter, out boxSize); return;
            case PileKind.Kiln: BuildKiln(parent, state, bark, pieces, random, out boxCenter, out boxSize); return;
            case PileKind.CharcoalPitSite: BuildCharcoalPitSite(parent, state, bark, pieces, random, out boxCenter, out boxSize); return;
            case PileKind.CharcoalPit: BuildCharcoalPit(parent, state, bark, pieces, out boxCenter, out boxSize); return;
            case PileKind.BloomerySite: BuildBloomerySite(parent, state, bark, pieces, random, out boxCenter, out boxSize); return;
            case PileKind.Bloomery: BuildBloomery(parent, state, pieces, out boxCenter, out boxSize); return;
            case PileKind.SmithySite: BuildSmithySite(parent, state, bark, pieces, random, out boxCenter, out boxSize); return;
            case PileKind.Smithy: BuildSmithy(parent, state, bark, pieces, random, out boxCenter, out boxSize); return;
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

    // The Hand Cart's marked-out plot: the usual stakes and soil with the delivered Logs and Branches, plus the Wooden Wheels leaning upright beside them
    // (a delivered wheel is otherwise invisible). Cordage is too small to draw.
    static void BuildHandCartSite(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, System.Random random, out Vector3 boxCenter, out Vector3 boxSize)
    {
        BuildOuthouseSite(parent, state, bark, pieces, random, 0.7f, out boxCenter, out boxSize);
        int wheels = Mathf.Min(state.Count(CartRules.WheelId), CartRules.CartWheels);
        for (int i = 0; i < wheels; i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(1.3f, 0.3f, 0.5f - i * 0.15f), new Vector3(0f, 0f, 90f), new Vector3(0.6f, 0.05f, 0.6f), bark), new Color(0.3f, 0.22f, 0.15f));
    }

    // The Workshop's marked-out plot (3 m by 2.4 m: half = 1.2 is the usual plot's side, enlarged to the roof's footprint): the stakes, bare earth and the delivered
    // Logs, Branches and Tall Grass lying beside it, with a wider interaction box than the small plots.
    static void BuildWorkshopSite(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, System.Random random, out Vector3 boxCenter, out Vector3 boxSize)
    {
        BuildOuthouseSite(parent, state, bark, pieces, random, 1.2f, out boxCenter, out boxSize);
        boxCenter = new Vector3(0f, 0.4f, 0f);
        boxSize = new Vector3(2.8f, 0.9f, 2.4f);
    }

    static readonly Color ThatchColor = new Color(0.52f, 0.46f, 0.24f);
    static readonly Color BenchWoodColor = new Color(0.33f, 0.25f, 0.17f);

    // The finished Workshop (Workshop W1(a1)): a roofed shed about 3 m wide and 2.4 m deep, open at the front (+Z), with a Woodworking bench against the back wall.
    // Posts, back wall and roof are Solid, so the roof shelters like the Tent's and the posts are walked round; the bench is what is aimed at (the interaction box).
    // The stock lies beside the shed (Logs on the left, Branches on the right); the bench shows a Log while a piece is being made and the finished stool when it waits.
    static void BuildWorkshop(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, System.Random random, out Vector3 boxCenter, out Vector3 boxSize)
    {
        const float halfW = 1.5f, halfD = 1.2f, top = 2.2f, postX = halfW - 0.12f, postZ = halfD - 0.12f;
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.01f, 0f), Vector3.zero, new Vector3(postX * 2f, 0.01f, postZ * 2f), null), SoilColor);

        foreach (Vector2 corner in new[] { new Vector2(-postX, -postZ), new Vector2(postX, -postZ), new Vector2(-postX, postZ), new Vector2(postX, postZ) })
            Solid(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(corner.x, top / 2f, corner.y), Vector3.zero, new Vector3(0.18f, top / 2f, 0.18f), bark));
        Solid(Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 1.1f, -postZ), Vector3.zero, new Vector3(postX * 2f, 2.1f, 0.08f), bark), BenchWoodColor));

        // Thatch roof: overlapping bundles laid front to back on a gentle slope (higher at the open front so rain runs off the back).
        const int bundles = 7;
        float span = halfD * 2f + 0.4f;
        for (int i = 0; i < bundles; i++)
        {
            float t = i / (float)(bundles - 1);
            float tone = 0.85f + (float)random.NextDouble() * 0.3f;
            Solid(Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, top + 0.1f + Mathf.Lerp(-0.1f, 0.1f, t), Mathf.Lerp(-span / 2f, span / 2f, t)),
                               new Vector3(-3f, 0f, 0f), new Vector3(halfW * 2f + 0.5f, 0.09f, span / bundles + 0.16f), null),
                         new Color(ThatchColor.r * tone, ThatchColor.g * tone, ThatchColor.b * tone)));
        }

        // The bench: a plank top on four legs against the back wall.
        Vector3 bench = new Vector3(0f, 0f, -postZ + 0.4f);
        WorkTable(parent, pieces, bark, bench, 2.0f, 0.6f, 0.85f, false);

        bool working = !string.IsNullOrEmpty(state.jobItem) && state.jobHours > 0f;
        if (working)
            Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(-0.4f, 1.02f, bench.z), new Vector3(0f, 0f, 90f), new Vector3(0.26f, 0.45f, 0.26f), bark);
        foreach (WorkshopRules.Recipe recipe in WorkshopRules.Recipes)
        {
            if (state.Count(recipe.itemId) > 0)
            {
                FinishedPiece(parent, pieces, bark, recipe.itemId, new Vector3(0.4f, 0.85f, bench.z));
                break;
            }
        }

        int logs = Mathf.Min(state.Count(WorkshopRules.LogsId), 4);
        for (int i = 0; i < logs; i++)
            Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(-halfW - 0.45f, 0.17f + i * 0.3f, -0.3f + (i % 2) * 0.2f), new Vector3(90f, 0f, 0f), new Vector3(0.32f, 0.85f, 0.32f), bark);
        int branches = Mathf.Min(state.Count(WorkshopRules.BranchesId), 8);
        for (int i = 0; i < branches; i++)
            Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(halfW + 0.35f + (float)random.NextDouble() * 0.2f, 0.05f + i * 0.035f, -0.2f + (float)random.NextDouble() * 0.3f),
                  new Vector3(90f, (float)random.NextDouble() * 40f - 20f, 0f), new Vector3(0.09f, 0.75f, 0.09f), bark);

        boxCenter = new Vector3(0f, 0.55f, bench.z);
        boxSize = new Vector3(2.0f, 1.1f, 0.7f);
    }

    // The Potter's Shed's marked-out plot (about 2.4 m by 2.0 m: half = 1.0): the stakes, bare earth and the delivered Logs, Branches and Tall Grass beside it.
    static void BuildPotterSite(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, System.Random random, out Vector3 boxCenter, out Vector3 boxSize)
    {
        BuildOuthouseSite(parent, state, bark, pieces, random, 1.0f, out boxCenter, out boxSize);
        boxCenter = new Vector3(0f, 0.4f, 0f);
        boxSize = new Vector3(2.4f, 0.9f, 2.0f);
    }

    static readonly Color WetClayColor = new Color(0.36f, 0.25f, 0.19f);
    static readonly Color DryClayColor = new Color(0.74f, 0.58f, 0.44f);
    static readonly Color SandColor = new Color(0.82f, 0.73f, 0.5f);

    // The finished Potter's Shed (Pottery P1): a roofed shed about 2.4 m wide and 2.0 m deep, open at the front (+Z), with a shaping table on the left and a two-board shelf
    // against the back wall on the right. Pieces on the shelf are dark while wet and pale once dry; a lump of clay on the table shows a job in progress. The stock lies beside
    // the shed (Clay on the left, Sand on the right). Posts, back wall and roof are Solid like the Workshop's; the back wall is what is aimed at (the interaction box).
    static void BuildPotterShed(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, System.Random random, out Vector3 boxCenter, out Vector3 boxSize)
    {
        const float halfW = 1.2f, halfD = 1.0f, top = 2.1f, postX = halfW - 0.12f, postZ = halfD - 0.12f;
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.01f, 0f), Vector3.zero, new Vector3(postX * 2f, 0.01f, postZ * 2f), null), SoilColor);

        foreach (Vector2 corner in new[] { new Vector2(-postX, -postZ), new Vector2(postX, -postZ), new Vector2(-postX, postZ), new Vector2(postX, postZ) })
            Solid(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(corner.x, top / 2f, corner.y), Vector3.zero, new Vector3(0.16f, top / 2f, 0.16f), bark));
        Solid(Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 1.05f, -postZ), Vector3.zero, new Vector3(postX * 2f, 2.0f, 0.08f), bark), BenchWoodColor));

        const int bundles = 6;
        float span = halfD * 2f + 0.4f;
        for (int i = 0; i < bundles; i++)
        {
            float t = i / (float)(bundles - 1);
            float tone = 0.85f + (float)random.NextDouble() * 0.3f;
            Solid(Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, top + 0.1f + Mathf.Lerp(-0.1f, 0.1f, t), Mathf.Lerp(-span / 2f, span / 2f, t)),
                               new Vector3(-3f, 0f, 0f), new Vector3(halfW * 2f + 0.5f, 0.09f, span / bundles + 0.16f), null),
                         new Color(ThatchColor.r * tone, ThatchColor.g * tone, ThatchColor.b * tone)));
        }

        // The shaping table on the left, and the two shelf boards on the right (low and high) against the back wall.
        WorkTable(parent, pieces, bark, new Vector3(-0.55f, 0f, -postZ + 0.35f), 0.95f, 0.5f, 0.8f, false);
        float shelfX = 0.5f, shelfZ = -postZ + 0.22f;
        foreach (float shelfY in new[] { 0.55f, 1.15f })
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(shelfX, shelfY, shelfZ), Vector3.zero, new Vector3(0.95f, 0.04f, 0.38f), bark), BenchWoodColor);

        bool working = !string.IsNullOrEmpty(state.jobItem) && state.jobHours > 0f;
        if (working)
            Tinted(Piece(parent, pieces, PrimitiveType.Sphere, new Vector3(-0.55f, 0.9f, -postZ + 0.35f), Vector3.zero, new Vector3(0.3f, 0.2f, 0.3f), null), WetClayColor);

        // The shelf, up to eight pieces drawn (a stack of any size shows as at most four).
        int now = WellsAndOuthouses.PotteryNow();
        int slot = 0;
        foreach (PotteryRules.ShelfEntry entry in WellsAndOuthouses.ShelfOf(state))
        {
            Color tone = PotteryRules.IsDry(entry.ready, now) ? DryClayColor : WetClayColor;
            for (int i = 0; i < Mathf.Min(entry.count, 4) && slot < 8; i++, slot++)
            {
                float y = slot < 4 ? 0.57f : 1.17f;
                Vector3 at = new Vector3(shelfX - 0.36f + (slot % 4) * 0.24f, y, shelfZ);
                switch (entry.itemId)
                {
                    case PotteryRules.DryCupId: Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, at + new Vector3(0f, 0.06f, 0f), Vector3.zero, new Vector3(0.09f, 0.06f, 0.09f), null), tone); break;
                    case PotteryRules.DryBowlId: Tinted(Piece(parent, pieces, PrimitiveType.Sphere, at + new Vector3(0f, 0.05f, 0f), Vector3.zero, new Vector3(0.2f, 0.1f, 0.2f), null), tone); break;
                    case PotteryRules.DryPotId: Tinted(Piece(parent, pieces, PrimitiveType.Sphere, at + new Vector3(0f, 0.1f, 0f), Vector3.zero, new Vector3(0.22f, 0.2f, 0.22f), null), tone); break;
                    case PotteryRules.DryCrockId: Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, at + new Vector3(0f, 0.14f, 0f), Vector3.zero, new Vector3(0.2f, 0.14f, 0.2f), null), tone); break;
                    default: Tinted(Piece(parent, pieces, PrimitiveType.Cube, at + new Vector3(0f, 0.07f, 0f), Vector3.zero, new Vector3(0.22f, 0.14f, 0.18f), null), tone); break;
                }
            }
        }

        int clay = Mathf.Min(state.Count(PotteryRules.ClayId), 5);
        for (int i = 0; i < clay; i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Sphere, new Vector3(-halfW - 0.4f + (i % 2) * 0.22f, 0.1f + (i / 2) * 0.16f, -0.3f + (i % 3) * 0.2f), Vector3.zero, new Vector3(0.26f, 0.2f, 0.26f), null), WetClayColor);
        int sand = Mathf.Min(state.Count(PotteryRules.SandId), 4);
        for (int i = 0; i < sand; i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Sphere, new Vector3(halfW + 0.4f, 0.09f + i * 0.1f, -0.3f + (i % 2) * 0.24f), Vector3.zero, new Vector3(0.34f, 0.18f, 0.3f), null), SandColor);

        boxCenter = new Vector3(0f, 0.7f, -postZ + 0.35f);
        boxSize = new Vector3(2.2f, 1.4f, 0.7f);
    }

    static readonly Color SootColor = new Color(0.16f, 0.13f, 0.12f);
    static readonly Color FiredClayColor = new Color(0.72f, 0.38f, 0.22f);
    static readonly Color EmberColor = new Color(1f, 0.45f, 0.1f);

    // The Kiln's marked-out plot (about 2.4 m square: half = 1.2): the stakes, bare earth, and the delivered Stone and Clay beside it.
    static void BuildKilnSite(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, System.Random random, out Vector3 boxCenter, out Vector3 boxSize)
    {
        BuildOuthouseSite(parent, state, bark, pieces, random, 1.2f, out boxCenter, out boxSize);
        int stone = Mathf.Min(state.Count(WoodManager.StoneId), PotteryRules.KilnStone);
        for (int i = 0; i < stone; i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(-1.6f + (i % 3) * 0.3f, 0.1f + (i / 3) * 0.2f, -0.5f + (i % 2) * 0.1f), new Vector3(0f, i * 23f, 0f), new Vector3(0.28f, 0.18f, 0.24f), null), SlabColor);
        int clay = Mathf.Min(state.Count(WoodManager.ClayId), PotteryRules.KilnClay);
        for (int i = 0; i < clay; i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Sphere, new Vector3(1.6f, 0.1f + (i / 2) * 0.16f, -0.5f + (i % 2) * 0.28f), Vector3.zero, new Vector3(0.26f, 0.2f, 0.26f), null), ClayColor);
        boxCenter = new Vector3(0f, 0.4f, 0f);
        boxSize = new Vector3(2.4f, 0.9f, 2.4f);
    }

    // The finished Kiln (Pottery P2): a stone pad under a clay dome about 2 m across with a mouth at the front (+Z), open to the sky. It reads at a glance: bare when empty; Firewood
    // stacked and a few dry pieces at the mouth when loaded; a sooty dome, glowing mouth and smoke while it fires; pale, ash-dusted dome, a heap of Ash and the fired pieces (terracotta)
    // set out at the mouth when they are waiting. The shapes are plain pieces coloured here; the smoke is KilnSmoke, a persistent child that Rebuild only switches on and off.
    static void BuildKiln(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, System.Random random, out Vector3 boxCenter, out Vector3 boxSize)
    {
        PotteryRules.KilnPhase phase = WellsAndOuthouses.KilnPhaseOf(state);
        bool firing = phase == PotteryRules.KilnPhase.Firing, done = phase == PotteryRules.KilnPhase.Done;

        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.07f, 0f), Vector3.zero, new Vector3(2.4f, 0.07f, 2.4f), null), SlabColor);
        Color dome = firing ? SootColor : done ? Color.Lerp(ClayColor, AshColor, 0.45f) : ClayColor;
        Tinted(Piece(parent, pieces, PrimitiveType.Sphere, new Vector3(0f, 0.15f, 0f), Vector3.zero, new Vector3(2.0f, 1.7f, 2.0f), null), dome);
        for (int i = 0; i < 5; i++) // a ring of stones bedded in the dome's foot
        {
            float angle = (i - 2) * 0.55f + Mathf.PI / 2f;
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(Mathf.Cos(angle) * 0.92f, 0.25f, Mathf.Sin(angle) * 0.92f), new Vector3(0f, -angle * Mathf.Rad2Deg, 0f),
                         new Vector3(0.3f, 0.26f, 0.3f), null), SlabColor);
        }
        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 1.0f, 0f), Vector3.zero, new Vector3(0.28f, 0.1f, 0.28f), null), firing ? SootColor : SlabColor); // the vent

        // The mouth: dark and empty, a glowing ember while it fires. Its front face stands at z 1.10, 3 cm proud of the front ring stone's (1.07): at 0.92 the two faces were
        // coplanar and the stone's tan and the ember's orange fought over the same pixels, so they flashed as the view moved.
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.45f, 0.95f), Vector3.zero, new Vector3(0.62f, 0.5f, 0.3f), null), firing ? EmberColor : SootColor);

        int wood = Mathf.Min(state.Count(PotteryRules.FirewoodId), PotteryRules.KilnFirewood);
        if (!firing && !done)
            for (int i = 0; i < wood; i++)
                Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(-1.45f + (i % 5) * 0.02f, 0.1f + (i / 2) * 0.12f, -0.2f + (i % 2) * 0.35f), new Vector3(90f, 12f * (i % 3), 0f), new Vector3(0.1f, 0.55f, 0.1f), bark);

        if (firing)
        {
            // The smoke is not a piece: KilnSmoke is switched on by PlacedStructure.Rebuild.
        }
        else if (!done)
        {
            // Dry pieces in the load, up to six at the mouth (one small shape for each kind, pale as on the shed's shelf).
            int slot = 0;
            foreach (PotteryRules.ShelfEntry entry in WellsAndOuthouses.KilnLoadOf(state))
                for (int i = 0; i < Mathf.Min(entry.count, 3) && slot < 6; i++, slot++)
                    KilnPiece(parent, pieces, entry.itemId, new Vector3(1.4f - (slot % 3) * 0.25f, 0.06f + (slot / 3) * 0.2f, 0.7f), DryClayColor);
        }
        else
        {
            int slot = 0;
            foreach (PotteryRules.Recipe recipe in PotteryRules.Recipes)
                for (int i = 0; i < Mathf.Min(state.Count(recipe.firedId), 3) && slot < 6; i++, slot++)
                    KilnPiece(parent, pieces, recipe.itemId, new Vector3(1.4f - (slot % 3) * 0.25f, 0.06f + (slot / 3) * 0.2f, 0.7f), FiredClayColor);
            if (state.Count(PotteryRules.AshId) > 0)
                Tinted(Piece(parent, pieces, PrimitiveType.Sphere, new Vector3(-0.5f, 0.1f, 1.25f), Vector3.zero, new Vector3(0.5f, 0.2f, 0.4f), null), AshColor);
        }

        boxCenter = new Vector3(0f, 0.75f, 0f);
        boxSize = new Vector3(2.2f, 1.5f, 2.2f);
    }

    static readonly Color EarthColor = new Color(0.34f, 0.25f, 0.16f);
    static readonly Color CharcoalColor = new Color(0.1f, 0.1f, 0.11f);

    // The Charcoal Pit's marked-out plot (about 1.8 m square: half = 0.9): the stakes, bare earth, the delivered Clay to one side and the Tall Grass to the other.
    static void BuildCharcoalPitSite(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, System.Random random, out Vector3 boxCenter, out Vector3 boxSize)
    {
        BuildOuthouseSite(parent, state, bark, pieces, random, 0.9f, out boxCenter, out boxSize);
        int clay = Mathf.Min(state.Count(CharcoalRules.ClayId), CharcoalRules.SiteClay);
        for (int i = 0; i < clay; i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Sphere, new Vector3(1.3f, 0.1f + (i / 2) * 0.16f, -0.4f + (i % 2) * 0.28f), Vector3.zero, new Vector3(0.26f, 0.2f, 0.26f), null), ClayColor);
        // The plot already shows the first three Tall Grass as thatch lumps (BuildOuthouseSite); the rest lie as bundles beside them.
        int grass = Mathf.Min(state.Count(CharcoalRules.TallGrassId), CharcoalRules.SiteTallGrass);
        for (int i = 3; i < grass; i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(-1.3f + ((i - 3) % 3) * 0.12f, 0.06f, -0.3f + ((i - 3) % 3) * 0.05f),
                         new Vector3(90f, 0f, 20f * ((i - 3) % 3)), new Vector3(0.06f, 0.35f, 0.06f), null), ThatchColor);
        boxCenter = new Vector3(0f, 0.4f, 0f);
        boxSize = new Vector3(1.8f, 0.9f, 1.8f);
    }

    // The finished Charcoal Pit (Metalworking M1): a low clay-coloured earth mound about 1.5 m across and 0.8 m high on a ring of trodden earth, with a small vent on top and a little
    // draught hole at the front (+Z). Logs lie beside it when it is loaded; it darkens and the hole glows dimly (no light, no flicker) while it burns; the Charcoal waiting lies in a
    // small heap at the front. The shapes are plain pieces coloured here; the smoke is KilnSmoke, a persistent child that Rebuild only switches on and off. No two faces share a plane:
    // the draught hole's front face stands at z 0.82, 8 cm clear of the mound's surface there (about 0.74), so nothing z-fights as the view moves.
    static void BuildCharcoalPit(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        CharcoalRules.Phase phase = WellsAndOuthouses.PitPhaseOf(state);
        bool burning = phase == CharcoalRules.Phase.Burning, done = phase == CharcoalRules.Phase.Done;

        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.03f, 0f), Vector3.zero, new Vector3(1.9f, 0.03f, 1.9f), null), EarthColor);
        Color mound = burning ? Color.Lerp(ClayColor, SootColor, 0.55f) : done ? Color.Lerp(ClayColor, AshColor, 0.25f) : ClayColor;
        Tinted(Piece(parent, pieces, PrimitiveType.Sphere, Vector3.zero, Vector3.zero, new Vector3(1.5f, 1.6f, 1.5f), null), mound);
        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.8f, 0f), Vector3.zero, new Vector3(0.16f, 0.07f, 0.16f), null), burning ? SootColor : SlabColor); // the vent
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.14f, 0.72f), Vector3.zero, new Vector3(0.24f, 0.2f, 0.2f), null),
               burning ? Color.Lerp(SootColor, EmberColor, 0.55f) : SootColor); // the draught hole: a dim ember while it burns

        if (!burning && !done)
        {
            int logs = Mathf.Min(state.PitLogs, CharcoalRules.LogsCap);
            for (int i = 0; i < logs; i++)
                Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(-1.45f + (i % 3) * 0.3f, 0.14f + (i / 3) * 0.26f, 0f), new Vector3(90f, 0f, 0f), new Vector3(0.14f, 0.5f, 0.14f), bark);
        }
        if (done)
        {
            int heap = Mathf.Min(state.PitCharcoal, 6);
            for (int i = 0; i < heap; i++)
                Tinted(Piece(parent, pieces, PrimitiveType.Sphere, new Vector3(0.55f + (i % 3) * 0.2f, 0.07f + (i / 3) * 0.1f, 0.95f), Vector3.zero, new Vector3(0.2f, 0.12f, 0.16f), null), CharcoalColor);
        }

        boxCenter = new Vector3(0f, 0.4f, 0f);
        boxSize = new Vector3(1.7f, 0.9f, 1.7f);
    }

    static readonly Color ForgeStoneColor = new Color(0.27f, 0.265f, 0.26f);
    static readonly Color AnvilColor = new Color(0.27f, 0.28f, 0.31f);

    // The Smithy's marked-out plot (3 m by 2.4 m: half = 1.2, as the Workshop's): the stakes, bare earth, the delivered Logs, Branches and Tall Grass lying beside it, and the Stone and
    // the Clay in front.
    static void BuildSmithySite(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, System.Random random, out Vector3 boxCenter, out Vector3 boxSize)
    {
        BuildOuthouseSite(parent, state, bark, pieces, random, 1.2f, out boxCenter, out boxSize);
        int stone = Mathf.Min(state.Count(SmithyRules.StoneId), SmithyRules.SiteStone);
        for (int i = 0; i < stone; i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(-1.9f + (i % 3) * 0.3f, 0.1f + (i / 3) * 0.2f, 0.7f + (i % 2) * 0.1f), new Vector3(0f, i * 23f, 0f), new Vector3(0.28f, 0.18f, 0.24f), null), SlabColor);
        int clay = Mathf.Min(state.Count(SmithyRules.ClayId), SmithyRules.SiteClay);
        for (int i = 0; i < clay; i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Sphere, new Vector3(1.9f, 0.1f + (i / 2) * 0.16f, 0.7f + (i % 2) * 0.28f), Vector3.zero, new Vector3(0.26f, 0.2f, 0.26f), null), ClayColor);
        boxCenter = new Vector3(0f, 0.4f, 0f);
        boxSize = new Vector3(2.8f, 0.9f, 2.4f);
    }

    // A finished good lying on the anvil (a small plain shape for each): the hinges' two leaves, the hooks standing in a row, the box of nails.
    static void ForgeGood(Transform parent, List<GameObject> pieces, string itemId, Vector3 at)
    {
        switch (itemId)
        {
            case SmithyRules.HingesId:
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, at + new Vector3(-0.08f, 0.02f, 0f), Vector3.zero, new Vector3(0.12f, 0.03f, 0.2f), null), IronBarColor);
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, at + new Vector3(0.08f, 0.02f, 0f), new Vector3(0f, 12f, 0f), new Vector3(0.12f, 0.03f, 0.2f), null), IronBarColor);
                break;
            case SmithyRules.HooksId:
                for (int i = 0; i < 3; i++)
                    Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, at + new Vector3(-0.1f + i * 0.1f, 0.06f, 0f), Vector3.zero, new Vector3(0.025f, 0.06f, 0.025f), null), IronBarColor);
                break;
            case ToolRules.IronShovelId:
                // A shovel lying along the anvil: a wooden haft and a flat grey blade at one end.
                Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, at + new Vector3(0f, 0.03f, 0f), new Vector3(0f, 0f, 90f), new Vector3(0.03f, 0.25f, 0.03f), null), BenchWoodColor);
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, at + new Vector3(0.26f, 0.025f, 0f), Vector3.zero, new Vector3(0.14f, 0.025f, 0.12f), null), IronBarColor);
                break;
            case ToolRules.IronPickAxeId:
                // A pick axe lying along the anvil: a wooden haft and a grey head across one end.
                Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, at + new Vector3(0f, 0.03f, 0f), new Vector3(0f, 0f, 90f), new Vector3(0.03f, 0.25f, 0.03f), null), BenchWoodColor);
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, at + new Vector3(0.24f, 0.04f, 0f), Vector3.zero, new Vector3(0.05f, 0.04f, 0.3f), null), IronBarColor);
                break;
            default:
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, at + new Vector3(0f, 0.055f, 0f), Vector3.zero, new Vector3(0.22f, 0.1f, 0.14f), null), BenchWoodColor);
                break;
        }
    }

    // The finished Smithy (Metalworking M3): a roofed shed about 3 m wide and 2.4 m deep, open at the front (+Z), like the Workshop but darker and sootier. A stone forge hearth stands
    // against the back wall on the left with a chimney rising through the roof beside it; an anvil on a log stump stands in front, a little right of the middle. Posts, back wall and roof
    // are Solid, so the roof shelters like the Workshop's; the forge and anvil are what is aimed at (the interaction box). Iron Bars (grey) lie outside on the left and Charcoal (black)
    // on the right; a good waiting to be taken lies on the anvil; while a job runs the fire bed and the hearth's mouth glow a dim ember (no light, no flicker) and a hot piece lies on the
    // anvil. The smoke is KilnSmoke, a persistent child at the chimney that Rebuild only switches on and off. No two faces share a plane: the hearth's back is 7 cm off the back wall,
    // its mouth stands 8 cm proud of its front, the chimney starts inside the hearth, the anvil's parts overlap instead of touching, and the horn runs 3 cm into the face plate.
    static void BuildSmithy(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, System.Random random, out Vector3 boxCenter, out Vector3 boxSize)
    {
        const float halfW = 1.5f, halfD = 1.2f, top = 2.2f, postX = halfW - 0.12f, postZ = halfD - 0.12f;
        bool working = !string.IsNullOrEmpty(state.jobItem) && state.jobHours > 0f;
        Color glow = Color.Lerp(SootColor, EmberColor, 0.55f);

        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.01f, 0f), Vector3.zero, new Vector3(postX * 2f, 0.01f, postZ * 2f), null), SoilColor);

        foreach (Vector2 corner in new[] { new Vector2(-postX, -postZ), new Vector2(postX, -postZ), new Vector2(-postX, postZ), new Vector2(postX, postZ) })
            Solid(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(corner.x, top / 2f, corner.y), Vector3.zero, new Vector3(0.18f, top / 2f, 0.18f), bark));
        Solid(Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 1.1f, -postZ), Vector3.zero, new Vector3(postX * 2f, 2.1f, 0.08f), bark), Color.Lerp(SootColor, BenchWoodColor, 0.3f)));

        // Thatch roof, sooty: the Workshop's bundles in a much darker tone.
        const int bundles = 7;
        float span = halfD * 2f + 0.4f;
        for (int i = 0; i < bundles; i++)
        {
            float t = i / (float)(bundles - 1);
            float tone = (0.85f + (float)random.NextDouble() * 0.3f) * 0.55f;
            Solid(Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, top + 0.1f + Mathf.Lerp(-0.1f, 0.1f, t), Mathf.Lerp(-span / 2f, span / 2f, t)),
                               new Vector3(-3f, 0f, 0f), new Vector3(halfW * 2f + 0.5f, 0.09f, span / bundles + 0.16f), null),
                         new Color(ThatchColor.r * tone, ThatchColor.g * tone, ThatchColor.b * tone)));
        }

        // The forge: a stone hearth 0.9 m wide, 0.8 m high and 0.7 m deep against the back wall on the left (z -0.97 to -0.27), a dark fire bed on top, a mouth in front, and the chimney.
        var hearth = new Vector3(-0.8f, 0f, -0.62f);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, hearth + new Vector3(0f, 0.4f, 0f), Vector3.zero, new Vector3(0.9f, 0.8f, 0.7f), null), ForgeStoneColor);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, hearth + new Vector3(0f, 0.83f, 0f), Vector3.zero, new Vector3(0.6f, 0.05f, 0.4f), null), working ? glow : SootColor);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, hearth + new Vector3(0f, 0.3f, 0.35f), Vector3.zero, new Vector3(0.5f, 0.3f, 0.16f), null), working ? glow : SootColor);
        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(SmithyRules.ChimneyX, 1.5f, SmithyRules.ChimneyZ), Vector3.zero, new Vector3(0.34f, 1.5f, 0.34f), null), ForgeStoneColor);

        // The anvil on its log stump, in front and a little right of the middle.
        Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0.55f, 0.2f, 0.15f), Vector3.zero, new Vector3(0.4f, 0.2f, 0.4f), bark);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0.55f, 0.46f, 0.15f), Vector3.zero, new Vector3(0.2f, 0.14f, 0.16f), null), AnvilColor);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0.55f, 0.56f, 0.15f), Vector3.zero, new Vector3(0.5f, 0.07f, 0.16f), null), AnvilColor);
        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0.92f, 0.55f, 0.15f), new Vector3(0f, 0f, 90f), new Vector3(0.07f, 0.15f, 0.07f), null), AnvilColor);

        if (working)
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0.45f, 0.62f, 0.15f), Vector3.zero, new Vector3(0.2f, 0.035f, 0.06f), null), EmberColor);
        else
        {
            foreach (SmithyRules.Recipe recipe in SmithyRules.Recipes)
            {
                if (state.Count(recipe.itemId) > 0)
                {
                    ForgeGood(parent, pieces, recipe.itemId, new Vector3(0.6f, 0.6f, 0.15f));
                    break;
                }
            }
        }

        // The stock lies outside: Iron Bars on the left, Charcoal on the right.
        for (int i = 0; i < Mathf.Min(state.SmithyBars, 4); i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(-halfW - 0.4f, 0.06f + (i / 2) * 0.09f, -0.25f + (i % 2) * 0.3f), Vector3.zero, new Vector3(0.14f, 0.07f, 0.4f), null), IronBarColor);
        for (int i = 0; i < Mathf.Min(state.SmithyCharcoal, 6); i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Sphere, new Vector3(halfW + 0.35f + (i % 3) * 0.17f, 0.07f + (i / 3) * 0.11f, -0.2f + (i % 2) * 0.2f), Vector3.zero, new Vector3(0.2f, 0.12f, 0.16f), null), CharcoalColor);

        boxCenter = new Vector3(-0.07f, 0.55f, -0.31f);
        boxSize = new Vector3(2.4f, 1.1f, 1.3f);
    }

    static readonly Color OreColor = new Color(0.45f, 0.24f, 0.16f);
    static readonly Color IronBarColor = new Color(0.42f, 0.43f, 0.46f);

    // The Bloomery's marked-out plot (about 1.6 m square: half = 0.8): the stakes, bare earth, the delivered Stone to one side and the Clay to the other.
    static void BuildBloomerySite(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, System.Random random, out Vector3 boxCenter, out Vector3 boxSize)
    {
        BuildOuthouseSite(parent, state, bark, pieces, random, 0.8f, out boxCenter, out boxSize);
        int stone = Mathf.Min(state.Count(BloomeryRules.StoneId), BloomeryRules.SiteStone);
        for (int i = 0; i < stone; i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(-1.4f + (i % 3) * 0.3f, 0.1f + (i / 3) * 0.2f, -0.5f + (i % 2) * 0.1f), new Vector3(0f, i * 23f, 0f), new Vector3(0.28f, 0.18f, 0.24f), null), SlabColor);
        int clay = Mathf.Min(state.Count(BloomeryRules.ClayId), BloomeryRules.SiteClay);
        for (int i = 0; i < clay; i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Sphere, new Vector3(1.4f, 0.1f + (i / 2) * 0.16f, -0.5f + (i % 2) * 0.28f), Vector3.zero, new Vector3(0.26f, 0.2f, 0.26f), null), ClayColor);
        boxCenter = new Vector3(0f, 0.4f, 0f);
        boxSize = new Vector3(1.6f, 0.9f, 1.6f);
    }

    // The finished Bloomery (Metalworking M2): a stone-and-clay furnace stack about 1.2 m tall and 0.9 m across, tapering, on a flat stone footing, with a short chimney on top and a mouth at
    // the front (+Z). Four footing stones are bedded round its foot, none in front of the mouth. Ore (rust lumps) lies to the left and Charcoal (black lumps) to the right when loaded; Iron Bars (grey) lie at
    // the front when they wait; while it smelts the clay darkens and the mouth glows a dim ember (no light, no flicker). The smoke is KilnSmoke, a persistent child that Rebuild only
    // switches on and off. No two faces share a plane: the mouth's front face stands at z 0.62, 17 cm clear of the stack's surface there (radius 0.45), and the chimney and the upper stack
    // overlap the pieces below them instead of touching.
    static void BuildBloomery(Transform parent, WoodPileState state, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        BloomeryRules.Phase phase = WellsAndOuthouses.BloomeryPhaseOf(state);
        bool smelting = phase == BloomeryRules.Phase.Smelting;

        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.04f, 0f), Vector3.zero, new Vector3(1.4f, 0.04f, 1.4f), null), SlabColor);              // the footing: top at 0.08
        Color lower = smelting ? Color.Lerp(ClayColor, SootColor, 0.5f) : ClayColor;
        Color upper = smelting ? Color.Lerp(FiredClayColor, SootColor, 0.5f) : Color.Lerp(ClayColor, SlabColor, 0.35f);
        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.4f, 0f), Vector3.zero, new Vector3(0.9f, 0.32f, 0.9f), null), lower);                    // 0.08 to 0.72
        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.9f, 0f), Vector3.zero, new Vector3(0.64f, 0.22f, 0.64f), null), upper);                 // 0.68 to 1.12, narrower
        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 1.12f, 0f), Vector3.zero, new Vector3(0.3f, 0.1f, 0.3f), null), smelting ? SootColor : SlabColor); // the chimney, top at 1.22
        foreach (float around in new[] { -1.5f, -0.9f, 0.9f, 1.5f }) // footing stones round the foot, none in front of the mouth
        {
            float angle = Mathf.PI / 2f + around;
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(Mathf.Cos(angle) * 0.5f, 0.16f, Mathf.Sin(angle) * 0.5f), new Vector3(0f, -angle * Mathf.Rad2Deg, 0f), new Vector3(0.22f, 0.2f, 0.22f), null), SlabColor);
        }
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.28f, 0.5f), Vector3.zero, new Vector3(0.34f, 0.26f, 0.24f), null),
               smelting ? Color.Lerp(SootColor, EmberColor, 0.55f) : SootColor);                                                                                              // the mouth

        for (int i = 0; i < Mathf.Min(state.BloomeryOre, BloomeryRules.OreCap); i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Sphere, new Vector3(-0.85f + (i % 2) * 0.2f, 0.09f + (i / 2) * 0.14f, 0.1f + (i % 2) * 0.08f), Vector3.zero, new Vector3(0.22f, 0.16f, 0.2f), null), OreColor);
        for (int i = 0; i < Mathf.Min(state.BloomeryCharcoal, BloomeryRules.CharcoalCap); i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Sphere, new Vector3(0.85f - (i % 2) * 0.2f, 0.09f + (i / 2) * 0.14f, 0.1f + (i % 2) * 0.08f), Vector3.zero, new Vector3(0.2f, 0.12f, 0.16f), null), CharcoalColor);
        for (int i = 0; i < Mathf.Min(state.BloomeryBars, 4); i++)
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(-0.3f + i * 0.2f, 0.06f, 0.95f), new Vector3(0f, 8f * i, 0f), new Vector3(0.14f, 0.07f, 0.4f), null), IronBarColor);

        boxCenter = new Vector3(0f, 0.65f, 0f);
        boxSize = new Vector3(1.2f, 1.4f, 1.2f);
    }

    // One small piece, by its dry id's shape (the same shapes as on the Potter's Shed's shelf).
    static void KilnPiece(Transform parent, List<GameObject> pieces, string dryId, Vector3 at, Color tone)
    {
        switch (dryId)
        {
            case PotteryRules.DryCupId: Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, at + new Vector3(0f, 0.06f, 0f), Vector3.zero, new Vector3(0.09f, 0.06f, 0.09f), null), tone); break;
            case PotteryRules.DryBowlId: Tinted(Piece(parent, pieces, PrimitiveType.Sphere, at + new Vector3(0f, 0.05f, 0f), Vector3.zero, new Vector3(0.2f, 0.1f, 0.2f), null), tone); break;
            case PotteryRules.DryPotId: Tinted(Piece(parent, pieces, PrimitiveType.Sphere, at + new Vector3(0f, 0.1f, 0f), Vector3.zero, new Vector3(0.22f, 0.2f, 0.22f), null), tone); break;
            case PotteryRules.DryCrockId: Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, at + new Vector3(0f, 0.14f, 0f), Vector3.zero, new Vector3(0.2f, 0.14f, 0.2f), null), tone); break;
            default: Tinted(Piece(parent, pieces, PrimitiveType.Cube, at + new Vector3(0f, 0.07f, 0f), Vector3.zero, new Vector3(0.22f, 0.14f, 0.18f), null), tone); break;
        }
    }

    // A finished Workshop piece standing on the bench (a2-1), a small plain shape for each: the stool's round seat on three legs, the End Table's square top on four legs,
    // the Chair's seat with a back, the Table's long top on four legs. `at` is the bench top where it stands.
    static void FinishedPiece(Transform parent, List<GameObject> pieces, Material bark, string itemId, Vector3 at)
    {
        void Plank(Vector3 offset, Vector3 size) => Tinted(Piece(parent, pieces, PrimitiveType.Cube, at + offset, Vector3.zero, size, bark), BenchWoodColor);
        void Leg(float x, float z, float height) => Piece(parent, pieces, PrimitiveType.Cylinder, at + new Vector3(x, height / 2f, z), Vector3.zero, new Vector3(0.05f, height / 2f, 0.05f), bark);

        switch (itemId)
        {
            case WorkshopRules.EndTableId:
                Plank(new Vector3(0f, 0.34f, 0f), new Vector3(0.42f, 0.04f, 0.42f));
                foreach (Vector2 leg in new[] { new Vector2(-0.17f, -0.17f), new Vector2(0.17f, -0.17f), new Vector2(-0.17f, 0.17f), new Vector2(0.17f, 0.17f) })
                    Leg(leg.x, leg.y, 0.32f);
                Plank(new Vector3(0f, 0.14f, 0f), new Vector3(0.34f, 0.03f, 0.34f)); // the low shelf
                break;
            case WorkshopRules.ChairId:
                Plank(new Vector3(0f, 0.24f, 0f), new Vector3(0.38f, 0.04f, 0.38f));
                foreach (Vector2 leg in new[] { new Vector2(-0.15f, -0.15f), new Vector2(0.15f, -0.15f), new Vector2(-0.15f, 0.15f), new Vector2(0.15f, 0.15f) })
                    Leg(leg.x, leg.y, 0.22f);
                Plank(new Vector3(0f, 0.5f, -0.17f), new Vector3(0.38f, 0.48f, 0.04f)); // the back
                break;
            case WorkshopRules.TableId:
                Plank(new Vector3(0.05f, 0.4f, 0f), new Vector3(0.9f, 0.05f, 0.5f));
                foreach (Vector2 leg in new[] { new Vector2(-0.35f, -0.2f), new Vector2(0.45f, -0.2f), new Vector2(-0.35f, 0.2f), new Vector2(0.45f, 0.2f) })
                    Leg(leg.x, leg.y, 0.38f);
                break;
            default: // the Rough Stool
                Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, at + new Vector3(0f, 0.17f, 0f), Vector3.zero, new Vector3(0.34f, 0.025f, 0.34f), bark), BenchWoodColor);
                foreach (Vector2 leg in new[] { new Vector2(-0.1f, -0.1f), new Vector2(0.1f, -0.1f), new Vector2(0f, 0.12f) })
                    Piece(parent, pieces, PrimitiveType.Cylinder, at + new Vector3(leg.x, 0.08f, leg.y), Vector3.zero, new Vector3(0.05f, 0.08f, 0.05f), bark);
                break;
        }
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

        if (state.hopperWater > 0)
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.975f, 0f), Vector3.zero,
                         new Vector3(0.95f, 0.02f + 0.04f * Mathf.Clamp01(state.hopperWater / (float)PotashRules.HopperWaterCapacity), 0.95f), null), new Color(0.2f, 0.28f, 0.34f));
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
    KilnSmoke kilnSmoke; // a Kiln's smoke, kept across Rebuild so its particles are not restarted every game hour
    KilnSmoke pitSmoke;  // a Charcoal Pit's, the same smoke made smaller
    KilnSmoke bloomerySmoke; // a Bloomery's, likewise
    KilnSmoke smithySmoke;   // a Smithy's, from its chimney

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
        if (state.kind == PileKind.Kiln && ArtSwap.DriveSmoke(transform, state, ref kilnSmoke))
        {
            // A prefab Kiln: its Anchor_Smoke carries the smoke (ArtSwap), so nothing here.
        }
        else if (state.kind == PileKind.Kiln)
        {
            if (kilnSmoke == null)
                kilnSmoke = KilnSmoke.Create(transform);
            kilnSmoke.SetFiring(WellsAndOuthouses.KilnPhaseOf(state) == PotteryRules.KilnPhase.Firing);
        }
        else if (state.kind == PileKind.CharcoalPit)
        {
            if (pitSmoke == null)
            {
                pitSmoke = KilnSmoke.Create(transform);
                pitSmoke.gameObject.name = "Pit Smoke";
                pitSmoke.height = CharcoalRules.SmokeHeight;
                pitSmoke.spreadRadius = CharcoalRules.SmokeSpread;
                pitSmoke.startSizeMin = CharcoalRules.SmokeSizeMin;
                pitSmoke.startSizeMax = CharcoalRules.SmokeSizeMax;
                pitSmoke.puffsPerSecond = CharcoalRules.SmokePuffsPerSecond;
                pitSmoke.riseSpeed = CharcoalRules.SmokeRiseSpeed;
                pitSmoke.maxPuffs = CharcoalRules.SmokeMaxPuffs;
                pitSmoke.Apply();
            }
            pitSmoke.SetFiring(state.PitBurning);
        }
        else if (state.kind == PileKind.Bloomery)
        {
            if (bloomerySmoke == null)
            {
                bloomerySmoke = KilnSmoke.Create(transform);
                bloomerySmoke.gameObject.name = "Bloomery Smoke";
                bloomerySmoke.height = BloomeryRules.SmokeHeight;
                bloomerySmoke.spreadRadius = BloomeryRules.SmokeSpread;
                bloomerySmoke.startSizeMin = BloomeryRules.SmokeSizeMin;
                bloomerySmoke.startSizeMax = BloomeryRules.SmokeSizeMax;
                bloomerySmoke.puffsPerSecond = BloomeryRules.SmokePuffsPerSecond;
                bloomerySmoke.riseSpeed = BloomeryRules.SmokeRiseSpeed;
                bloomerySmoke.maxPuffs = BloomeryRules.SmokeMaxPuffs;
                bloomerySmoke.Apply();
            }
            bloomerySmoke.SetFiring(state.SmeltRunning);
        }
        else if (state.kind == PileKind.Smithy)
        {
            if (smithySmoke == null)
            {
                var chimney = new GameObject("Chimney");
                chimney.transform.SetParent(transform, false);
                chimney.transform.localPosition = new Vector3(SmithyRules.ChimneyX, 0f, SmithyRules.ChimneyZ);
                smithySmoke = KilnSmoke.Create(chimney.transform);
                smithySmoke.gameObject.name = "Smithy Smoke";
                smithySmoke.height = SmithyRules.SmokeHeight;
                smithySmoke.spreadRadius = SmithyRules.SmokeSpread;
                smithySmoke.startSizeMin = SmithyRules.SmokeSizeMin;
                smithySmoke.startSizeMax = SmithyRules.SmokeSizeMax;
                smithySmoke.puffsPerSecond = SmithyRules.SmokePuffsPerSecond;
                smithySmoke.riseSpeed = SmithyRules.SmokeRiseSpeed;
                smithySmoke.maxPuffs = SmithyRules.SmokeMaxPuffs;
                smithySmoke.Apply();
            }
            smithySmoke.SetFiring(SmithyWorking);
        }
        box.center = center;
        box.size = size;
        // The finished kitchen has no interaction box (it would be a solid block to walk into): its roof, posts and table are the colliders,
        // so OverheadCover finds the roof and the prompt shows when any of them is aimed at.
        bool kitchen = state.kind == PileKind.Kitchen;
        box.enabled = !kitchen;
        // The Workshop keeps its interaction box (the bench) and also has solid posts, back wall and roof.
        if (!kitchen && state.kind != PileKind.Workshop && state.kind != PileKind.PotterShed && state.kind != PileKind.Smithy)
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

    // Any shovel in hand, stone or iron (ToolRules is the one place that lists them).
    static bool EquippedShovel => Inventory != null && Inventory.EquippedTool != null && ToolRules.IsShovel(Inventory.EquippedTool.Id);

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
                case PileKind.Workshop: return WorkshopPrompt();
                case PileKind.PotterShed: return PotteryPrompt();
                case PileKind.Kiln: return KilnPromptText();
                case PileKind.CharcoalPit: return PitPromptText();
                case PileKind.Bloomery: return BloomeryPromptText();
                case PileKind.Smithy: return SmithyPromptText();
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
    // compact: one line for a flash (items joined with "; "), otherwise one line per material for the look prompt.
    string StageNeeds(bool compact = false)
    {
        WellsAndOuthouses w = Rules;
        string sep = compact ? "; " : "\n";
        int PackCount(string id) => Inventory != null ? Inventory.Player.Count(id) : 0;
        string Line(string id, int need) => MaterialCheckRules.SiteProgress(ItemDatabase.Get(id)?.DisplayName ?? id, state.Count(id), need, PackCount(id));
        if (WellsAndOuthouses.SiteNeeds(state) == null)
        {
            // Fully stocked: the Hammer finishes it (hold click). Once it has begun the materials are built in.
            string percent = $"{state.buildProgress * 100f:0}%";
            string locked = WellsAndOuthouses.MaterialsLocked(state) ? "\nThe materials are built in - E no longer takes them back" : "";
            string what = state.kind == PileKind.KitchenSite ? " the hearth" : state.kind == PileKind.KitchenRoofSite ? " the roof" : "";
            return (WoodManager.HammerEquipped ? $"Hold click with the Hammer to build{what} ({percent})" : $"Fully stocked - equip the Hammer to build{what} ({percent})") + locked;
        }
        if (WellsAndOuthouses.RequiresDig(state.kind) && state.digProgress < 1f)
            return EquippedShovel ? $"Dig the shaft: hold click ({state.digProgress * 100f:0}%)" : $"Dig the shaft: equip the Shovel, hold click ({state.digProgress * 100f:0}%)";
        if (state.kind == PileKind.WellSite)
        {
            int lining = WellsAndOuthouses.NeedOf(state, WoodManager.StoneId); // this well's own: its depth to water sets it
            if (state.Count(WoodManager.StoneId) < lining)
                return "Line it with Stone (R stores Stone you carry)" + (compact ? " - " : sep) + Line(WoodManager.StoneId, lining);
            return "Cap and windlass (R stores them)" + (compact ? " - " : sep)
                   + string.Join(sep, Line(WoodManager.LogsId, w.wellCapLogs), Line(WoodManager.BranchesId, w.wellCapBranches));
        }
        // Any other staged site: what it holds against what it needs, material by material, with what the pack could add.
        var parts = new List<string>();
        foreach (WoodStack c in WellsAndOuthouses.FinishedCostFor(state))
            parts.Add(Line(c.itemId, c.count));
        string stageName = state.kind == PileKind.KitchenSite ? "Hearth stage" : state.kind == PileKind.KitchenRoofSite ? "Hearth built. Roof stage" : "Needs";
        return stageName + " (R stores what you carry)" + (compact ? " - " : sep) + string.Join(sep, parts);
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

    // --- The Leach Hopper (Potash B): R stocks Ash and water (separately, any time), E collects the Lye or starts a batch, R with the Hammer dismantles ---

    bool Leaching => state.leachHours > 0f;

    // The Ash in the running batch, and the Ash waiting for the next one (all of it when nothing leaches).
    int HopperBatchAsh => Leaching ? PotashRules.RunningBatch(state.Count(AshRules.ItemId), state.hopperBatch) : 0;
    int HopperAshStock => PotashRules.AshStock(state.Count(AshRules.ItemId), Leaching, state.hopperBatch);
    int HopperAshRoom => PotashRules.Room(state.Total);

    int[] CarriedWater()
    {
        var litres = new int[PotashRules.WaterWorstFirst.Length];
        for (int i = 0; i < litres.Length; i++)
            litres[i] = Inventory.Player.Count(PotashRules.WaterWorstFirst[i]);
        return litres;
    }

    int CarriedWaterLitres()
    {
        int total = 0;
        foreach (int litres in CarriedWater())
            total += litres;
        return total;
    }

    PotashRules.StockPlan CarriedStock() =>
        PotashRules.PlanStock(Inventory.Player.Count(AshRules.ItemId), CarriedWater(), HopperAshRoom, PotashRules.WaterRoom(state.hopperWater));

    string HopperPrompt()
    {
        int ready = state.Count(PotashRules.LyeId), stock = HopperAshStock, water = state.hopperWater;
        if (Leaching)
            return PotashRules.LeachingPrompt(Name, HopperBatchAsh, state.leachHours, ready, stock, water);
        if (ready > 0)
            return $"Collect Lye  ({ready} ready in the {Name})";
        if (stock <= 0 && water <= 0)
            return KnowHowRules.HopperEmptyPrompt(Name, WoodManager.HammerEquipped);
        string line = PotashRules.StockedPrompt(Name, stock, water);
        return WoodManager.HammerEquipped ? line + ". Put the Hammer away to load more" : line;
    }

    string HopperSecondaryPrompt()
    {
        if (WoodManager.HammerEquipped)
            return $"Dismantle the {Name}  (half the Logs and Tall Grass back, rounded down; the Ash and Lye in it come back, the water is lost)";
        PotashRules.StockPlan plan = CarriedStock();
        if (plan.ash <= 0 && plan.litres <= 0 && Inventory.Player.Count(AshRules.ItemId) <= 0 && CarriedWaterLitres() <= 0)
            return KnowHowRules.HopperLoadHelp();
        return PotashRules.LoadPrompt(plan, Inventory.Player.Count(AshRules.ItemId), CarriedWaterLitres(), HopperAshRoom, PotashRules.WaterRoom(state.hopperWater));
    }

    // E: Lye that is ready comes first (even while a batch leaches or stock waits); otherwise it starts a batch when there is Ash and water, or says what is missing.
    void HopperInteract()
    {
        PotashRules.HopperAction action = PotashRules.ActionFor(state.Count(PotashRules.LyeId), Leaching, HopperAshStock, state.hopperWater);
        switch (action)
        {
            case PotashRules.HopperAction.CollectLye: CollectLye(); break;
            case PotashRules.HopperAction.StillLeaching: ToolStatus.Flash(PotashRules.StillLeachingMessage(state.leachHours), 3f); break;
            case PotashRules.HopperAction.Start: HopperStart(); break;
            default: ToolStatus.Flash(PotashRules.NeedToStartMessage(action, HopperAshStock, state.hopperWater), 3.5f); break;
        }
    }

    // Starts a batch of min(Ash stock, stored litres) Lye. Its Ash stays in the hopper (marked as the batch), its water is used up, and the rest stays as stock for the next batch.
    void HopperStart()
    {
        int stock = HopperAshStock;
        int batch = PotashRules.NextBatch(stock, state.hopperWater);
        if (batch <= 0)
            return;
        state.hopperBatch = batch;
        state.hopperWater -= PotashRules.WaterFor(batch);
        state.leachHours = PotashRules.LeachGameHours;
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.Pour);
        WoodManager.Instance.NotifyChanged(state);
        ToolStatus.Flash(PotashRules.StartedMessage(batch, stock - PotashRules.AshFor(batch), state.hopperWater), 5f);
    }

    // E: the Lye that has finished leaching, as much as the pack can carry.
    void CollectLye()
    {
        int ready = state.Count(PotashRules.LyeId);
        if (ready <= 0)
        {
            ToolStatus.Flash(Leaching ? PotashRules.StillLeachingMessage(state.leachHours) : $"The {Name} has no Lye ready.", 3f);
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
        ToolStatus.Flash(added < ready ? $"Took {added} Lye  ({ready - added} left in the {Name}, no room). " + KnowHowRules.LyeTaken
                                       : $"Took {added} Lye. " + KnowHowRules.LyeTaken, 5f);
    }

    // R: with the Hammer it dismantles; without, it stocks Ash and water from the pack, each up to its cap, at any time (a batch that runs keeps going; the new stock waits for the next).
    void HopperSecondary()
    {
        if (WoodManager.HammerEquipped)
        {
            Dismantle();
            return;
        }

        int ashCarried = Inventory.Player.Count(AshRules.ItemId), litresCarried = CarriedWaterLitres();
        int ashRoom = HopperAshRoom, waterRoom = PotashRules.WaterRoom(state.hopperWater);
        PotashRules.StockPlan plan = PotashRules.PlanStock(ashCarried, CarriedWater(), ashRoom, waterRoom);
        if (plan.ash <= 0 && plan.litres <= 0)
        {
            ToolStatus.Flash(PotashRules.LoadRefusedMessage(ashCarried, litresCarried, ashRoom, waterRoom), 4f);
            return;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval();
        int ashTaken = plan.ash > 0 ? Inventory.RemoveFromPlayer(AshRules.ItemId, plan.ash) : 0;
        int litresTaken = 0;
        for (int i = 0; i < plan.water.Length; i++)
        {
            if (plan.water[i] <= 0)
                continue;
            litresTaken += Inventory.RemoveFromPlayer(PotashRules.WaterWorstFirst[i], plan.water[i]);
        }
        state.Add(AshRules.ItemId, ashTaken);
        state.hopperWater += litresTaken;
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.Pour);
        WoodManager.Instance.NotifyChanged(state);
        var done = new PotashRules.StockPlan { ash = ashTaken, litres = litresTaken, water = plan.water };
        ToolStatus.Flash(PotashRules.LoadedMessage(done, state.Count(AshRules.ItemId), state.hopperWater, ashCarried, litresCarried), 5f);
    }

    // --- The Workshop (Workshop W1(a1)): R stocks the bench, E starts a piece or collects the finished one, R with the Hammer dismantles ---

    bool WorkshopWorking => !string.IsNullOrEmpty(state.jobItem) && state.jobHours > 0f;

    // The piece being made. False when the bench is idle, or when the job names a piece this build does not know.
    bool WorkshopMade(out WorkshopRules.Recipe made)
    {
        made = default;
        return WorkshopWorking && WorkshopRules.TryFind(state.jobItem, out made);
    }

    // The Workshop screen (WorkshopScreen) reads the bench through these: the running piece, a waiting piece, and why a piece can or cannot start now.
    public bool WorkshopRunning(out WorkshopRules.Recipe made) => WorkshopMade(out made);
    public bool WorkshopWaiting(out WorkshopRules.Recipe piece) => WorkshopProduct(out piece, out _);
    public WorkshopRules.Gate WorkshopGateFor(WorkshopRules.Recipe recipe) => WorkshopGate(recipe);

    // A finished piece waiting to be collected.
    bool WorkshopProduct(out WorkshopRules.Recipe piece, out int count)
    {
        foreach (WorkshopRules.Recipe recipe in WorkshopRules.Recipes)
        {
            int n = state.Count(recipe.itemId);
            if (n > 0)
            {
                piece = recipe;
                count = n;
                return true;
            }
        }
        piece = default;
        count = 0;
        return false;
    }

    // Any felling tool in the pack opens the bench (checked when a job starts, never while it runs).
    static bool CarriesFellingTool() =>
        Inventory != null && AxeRules.FastestCarried(Inventory.Player.Has(AxeRules.AxeId), Inventory.Player.Has(AxeRules.PrimitiveAxeId), Inventory.Player.Has(AxeRules.SawId),
                                                       AxeRules.DefaultPrimitiveFactor, AxeRules.DefaultSawFactor) > 0f;

    WorkshopRules.Gate WorkshopGate(WorkshopRules.Recipe recipe) =>
        WorkshopRules.CanStart(recipe, CarriesFellingTool(), WorkshopWorking, WorkshopProduct(out _, out _),
                               state.Count(WorkshopRules.LogsId), state.Count(WorkshopRules.BranchesId), state.Count(WorkshopRules.CordageId));

    string WorkshopPrompt()
    {
        if (WorkshopProduct(out WorkshopRules.Recipe piece, out _))
            return WorkshopRules.DonePrompt(piece);
        if (WorkshopMade(out WorkshopRules.Recipe made))
            return WorkshopRules.WorkingPrompt(made, state.jobHours);
        return WorkshopRules.OpenPrompt;
    }

    // The stock materials the pack carries that the bench still has room for.
    int WorkshopCarried(out string what)
    {
        what = "";
        int total = 0;
        var names = new List<string>();
        foreach (string id in WorkshopRules.StockIds)
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

    string WorkshopSecondaryPrompt()
    {
        if (WoodManager.HammerEquipped)
            return WorkshopRules.DismantlePrompt(WorkshopWorking, state.jobHours);
        return WorkshopCarried(out string what) > 0 ? WorkshopRules.StorePrompt(what) : WorkshopRules.DeliverHelp;
    }

    // E: collect a finished piece, or else open the Workshop screen (a job starts there, through TryStartJob).
    void WorkshopInteract()
    {
        if (WorkshopProduct(out WorkshopRules.Recipe piece, out int waiting))
        {
            ItemDefinition item = ItemDatabase.Get(piece.itemId);
            int fits = item != null ? Mathf.Min(waiting, Inventory.Player.SpaceFor(item)) : 0;
            if (fits <= 0)
            {
                ToolStatus.Flash("Too heavy - no room to carry it", 3f);
                return;
            }
            int added = Inventory.AddToPlayer(piece.itemId, fits);
            state.Take(piece.itemId, added);
            WoodManager.Instance.NotifyChanged(state);
            ToolStatus.Flash(added < waiting ? $"Took {added} {piece.name} ({waiting - added} left in the Workshop, no room)." : $"Took the {piece.name}. Sell it at the Trading Post, or keep it.", 4f);
            return;
        }

        WorkshopScreen.Open(this);
    }

    // The one way a job starts (the Workshop screen's Start button calls it): the gate is worked out again on the bench as it is now, so a stale click does nothing. On
    // success it takes the stock, sets the job and returns true with the "Started" message; otherwise false with the reason.
    public bool TryStartJob(WorkshopRules.Recipe recipe, out string message)
    {
        message = "";
        if (state == null || state.kind != PileKind.Workshop || Inventory == null || WoodManager.Instance == null)
            return false;
        int logs = state.Count(WorkshopRules.LogsId), branches = state.Count(WorkshopRules.BranchesId), cordage = state.Count(WorkshopRules.CordageId);
        WorkshopRules.Gate gate = WorkshopGate(recipe);
        if (gate != WorkshopRules.Gate.Ok)
        {
            WorkshopMade(out WorkshopRules.Recipe made);
            message = WorkshopRules.ReasonText(gate, recipe, made, logs, branches, cordage, state.jobHours);
            return false;
        }

        state.Take(WorkshopRules.LogsId, recipe.logs);
        state.Take(WorkshopRules.BranchesId, recipe.branches);
        if (recipe.cordage > 0)
            state.Take(WorkshopRules.CordageId, recipe.cordage);
        state.jobItem = recipe.itemId;
        state.jobHours = recipe.hours;
        WoodManager.Instance.NotifyChanged(state);
        message = WorkshopRules.StartedMessage(recipe);
        return true;
    }

    // R: with the Hammer it dismantles; without, it stocks the bench from the pack (StockFromPack, which the Workshop screen's button and R key share).
    void WorkshopSecondary()
    {
        if (WoodManager.HammerEquipped)
        {
            Dismantle();
            return;
        }

        StockFromPack(out string message);
        ToolStatus.Flash(message, 4f);
    }

    // What the pack could put in the bench now (the pure rule decides: nothing carried, the bench full of what is carried, or something to move).
    public WorkshopRules.StockGate StockGateNow() =>
        state == null || Inventory == null ? WorkshopRules.StockGate.NothingCarried
            : WorkshopRules.StockState(Inventory.Player.Count(WorkshopRules.LogsId), Inventory.Player.Count(WorkshopRules.BranchesId), Inventory.Player.Count(WorkshopRules.CordageId),
                                       state.Count(WorkshopRules.LogsId), state.Count(WorkshopRules.BranchesId), state.Count(WorkshopRules.CordageId));

    // Puts what the pack carries of Logs, Branches and Cordage into the bench, up to the caps (wood keeps its wet or dry day). Works while a job runs. Returns whether anything
    // moved, with the message R flashes. Never dismantles: that is the Hammer's R in the world.
    public bool StockFromPack(out string message)
    {
        message = "Nothing you carry fits. " + WorkshopRules.DeliverHelp;
        if (state == null || state.kind != PileKind.Workshop || Inventory == null || WoodManager.Instance == null)
            return false;

        int stored = 0;
        var names = new List<string>();
        foreach (string id in WorkshopRules.StockIds)
        {
            if (!state.Accepts(id))
                continue;
            int n = Mathf.Min(WorkshopRules.CapOf(id) - state.Count(id), Inventory.Player.Count(id));
            if (n <= 0)
                continue;
            if (AudioManager.Instance != null)
                AudioManager.Instance.SilenceNextRemoval();
            var days = new List<(int day, int count)>();
            int removed = Inventory.Player.RemoveBatches(id, n, (day, count) => days.Add((day, count)));
            foreach ((int day, int count) in days)
                state.Add(id, count, WoodManager.IsWood(id) ? day : 0);
            if (removed > 0)
            {
                stored += removed;
                names.Add($"{removed} {ItemDatabase.Get(id)?.DisplayName ?? id}");
            }
        }

        if (stored <= 0)
            return false;
        WoodManager.Instance.NotifyChanged(state);
        message = $"Stored {string.Join(", ", names)}  (the bench holds {state.Count(WorkshopRules.LogsId)} Logs, {state.Count(WorkshopRules.BranchesId)} Branches, {state.Count(WorkshopRules.CordageId)} Cordage)";
        return true;
    }

    // --- The Potter's Shed (Pottery P1): R stocks Clay and Sand, E collects dry pieces or else opens the Potter screen, R with the Hammer dismantles ---

    bool PotteryWorking => !string.IsNullOrEmpty(state.jobItem) && state.jobHours > 0f;

    // The Potter screen (PotteryScreen) reads the shed through these: the running job, the shelf, and why a job can or cannot start now.
    public bool PotteryRunning(out PotteryRules.Recipe made, out int count)
    {
        count = PotteryRules.SavedCount(state.jobCount);
        made = default;
        return PotteryWorking && PotteryRules.TryFind(state.jobItem, out made);
    }

    public List<PotteryRules.ShelfEntry> PotteryShelf() => WellsAndOuthouses.ShelfOf(state);

    public PotteryRules.Gate PotteryGateFor(PotteryRules.Recipe recipe, int count) =>
        PotteryRules.CanStart(recipe, count, PotteryWorking, state.Count(PotteryRules.ClayId), state.Count(PotteryRules.SandId), PotteryRules.FreeSpace(PotteryShelf()));

    string PotteryPrompt()
    {
        int dry = PotteryRules.DryPieces(PotteryShelf(), WellsAndOuthouses.PotteryNow());
        if (dry > 0)
            return PotteryRules.CollectPrompt(dry);
        if (PotteryRunning(out PotteryRules.Recipe made, out int count))
            return PotteryRules.WorkingPrompt(made, count, state.jobHours);
        return PotteryRules.OpenPrompt;
    }

    // The Clay and Sand the pack carries that the shed still has room for.
    int PotteryCarried(out string what)
    {
        what = "";
        int total = 0;
        var names = new List<string>();
        foreach (string id in PotteryRules.StockIds)
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

    string PotterySecondaryPrompt()
    {
        if (WoodManager.HammerEquipped)
            return PotteryRules.DismantlePrompt(PotteryWorking, state.jobHours, PotteryRules.WetPieces(PotteryShelf(), WellsAndOuthouses.PotteryNow()));
        return PotteryCarried(out string what) > 0 ? PotteryRules.StorePrompt(what) : PotteryRules.DeliverHelp;
    }

    // E: if any piece on the shelf is dry, take them all (what does not fit stays); otherwise open the Potter screen (a job starts there, through PotteryStartJob).
    void PotteryInteract()
    {
        int now = WellsAndOuthouses.PotteryNow();
        if (PotteryRules.DryPieces(PotteryShelf(), now) <= 0)
        {
            PotteryScreen.Open(this);
            return;
        }

        int took = 0, stayed = 0;
        foreach (PotteryRules.Recipe recipe in PotteryRules.Recipes)
        {
            int dry = 0;
            foreach (PotteryRules.ShelfEntry entry in PotteryShelf())
                if (entry.itemId == recipe.itemId && PotteryRules.IsDry(entry.ready, now))
                    dry += entry.count;
            if (dry <= 0)
                continue;
            ItemDefinition item = ItemDatabase.Get(recipe.itemId);
            int fits = item != null ? Mathf.Min(dry, Inventory.Player.SpaceFor(item)) : 0;
            int added = fits > 0 ? Inventory.AddToPlayer(recipe.itemId, fits) : 0;
            state.Take(recipe.itemId, added); // the lowest day first, which is the dry stacks (their hour is already past)
            took += added;
            stayed += dry - added;
        }
        if (took <= 0)
        {
            ToolStatus.Flash("Too heavy - no room to carry them", 3f);
            return;
        }
        WoodManager.Instance.NotifyChanged(state);
        ToolStatus.Flash(PotteryRules.TookMessage(took, stayed), 4f);
    }

    // The one way a job starts (the Potter screen's Start button calls it): the gate is worked out again on the shed as it is now, so a stale click does nothing. On success it
    // takes the Clay and Sand, sets the job and returns true with the "Started" message; otherwise false with the reason.
    public bool PotteryStartJob(PotteryRules.Recipe recipe, int count, out string message)
    {
        message = "";
        if (state == null || state.kind != PileKind.PotterShed || Inventory == null || WoodManager.Instance == null)
            return false;
        count = PotteryRules.ClampCount(count);
        int clay = state.Count(PotteryRules.ClayId), sand = state.Count(PotteryRules.SandId);
        int free = PotteryRules.FreeSpace(PotteryShelf());
        PotteryRules.Gate gate = PotteryGateFor(recipe, count);
        if (gate != PotteryRules.Gate.Ok)
        {
            PotteryRunning(out PotteryRules.Recipe made, out int madeCount);
            message = PotteryRules.ReasonText(gate, recipe, count, clay, sand, free, state.jobHours, made, madeCount);
            return false;
        }

        state.Take(PotteryRules.ClayId, PotteryRules.ClayFor(recipe, count));
        state.Take(PotteryRules.SandId, PotteryRules.SandFor(recipe, count));
        state.jobItem = recipe.itemId;
        state.jobCount = count;
        state.jobHours = PotteryRules.HoursFor(recipe, count);
        WoodManager.Instance.NotifyChanged(state);
        message = PotteryRules.StartedMessage(recipe, count);
        return true;
    }

    // R: with the Hammer it dismantles; without, it stocks the shed from the pack (PotteryStockFromPack, which the Potter screen's button and R key share).
    void PotterySecondary()
    {
        if (WoodManager.HammerEquipped)
        {
            Dismantle();
            return;
        }

        PotteryStockFromPack(out string message);
        ToolStatus.Flash(message, 4f);
    }

    public PotteryRules.StockGate PotteryStockGateNow() =>
        state == null || Inventory == null ? PotteryRules.StockGate.NothingCarried
            : PotteryRules.StockState(Inventory.Player.Count(PotteryRules.ClayId), Inventory.Player.Count(PotteryRules.SandId),
                                      state.Count(PotteryRules.ClayId), state.Count(PotteryRules.SandId));

    // Puts what the pack carries of Clay and Sand into the shed, up to the caps. Works while a job runs. Returns whether anything moved, with the message R flashes.
    public bool PotteryStockFromPack(out string message)
    {
        message = "Nothing you carry fits. " + PotteryRules.DeliverHelp;
        if (state == null || state.kind != PileKind.PotterShed || Inventory == null || WoodManager.Instance == null)
            return false;

        int stored = 0;
        var names = new List<string>();
        foreach (string id in PotteryRules.StockIds)
        {
            if (!state.Accepts(id))
                continue;
            int n = Mathf.Min(PotteryRules.CapOf(id) - state.Count(id), Inventory.Player.Count(id));
            if (n <= 0)
                continue;
            if (AudioManager.Instance != null)
                AudioManager.Instance.SilenceNextRemoval();
            int removed = Inventory.Player.RemoveBatches(id, n, (day, count) => state.Add(id, count, 0));
            if (removed > 0)
            {
                stored += removed;
                names.Add($"{removed} {ItemDatabase.Get(id)?.DisplayName ?? id}");
            }
        }

        if (stored <= 0)
            return false;
        WoodManager.Instance.NotifyChanged(state);
        message = $"Stored {string.Join(", ", names)}  (the shed holds {state.Count(PotteryRules.ClayId)} Clay, {state.Count(PotteryRules.SandId)} Sand)";
        return true;
    }

    // --- The Kiln (Pottery P2): R loads dry pieces then Firewood from the pack, E lights it or collects what was fired, R with the Hammer takes it down. No screen: prompts only. ---

    PotteryRules.KilnPhase KilnPhaseNow => WellsAndOuthouses.KilnPhaseOf(state);

    string KilnPromptText() =>
        PotteryRules.KilnPrompt(KilnPhaseNow, WellsAndOuthouses.KilnPiecesIn(state), state.Count(PotteryRules.FirewoodId), KilnFiredPieces(), state.Count(PotteryRules.AshId), state.jobHours);

    int KilnFiredPieces()
    {
        int n = 0;
        foreach (PotteryRules.Recipe recipe in PotteryRules.Recipes)
            n += state.Count(recipe.firedId);
        return n;
    }

    // What the pack carries that the kiln would take now (dry pieces in the space left, Firewood to 10), and a short name for it.
    int KilnCarriedLoadable(out string what)
    {
        what = "";
        int total = 0;
        bool pieces = false, wood = false;
        int used = state.KilnSpaceUsed(), fire = state.Count(PotteryRules.FirewoodId);
        foreach (string id in PotteryRules.LoadOrder)
        {
            int n = Mathf.Min(PotteryRules.KilnRoomFor(id, used, fire), Inventory.Player.Count(id));
            if (n <= 0)
                continue;
            total += n;
            if (id == PotteryRules.FirewoodId)
                wood = true;
            else
                pieces = true;
        }
        what = pieces && wood ? "dry pieces and Firewood" : pieces ? "dry pieces" : "Firewood";
        return total;
    }

    string KilnSecondaryPrompt()
    {
        PotteryRules.KilnPhase phase = KilnPhaseNow;
        if (WoodManager.HammerEquipped)
            return PotteryRules.KilnDismantlePrompt(phase, state.jobHours);
        int loadable = KilnCarriedLoadable(out string what);
        return PotteryRules.KilnLoadPrompt(PotteryRules.LoadGate(phase, loadable), what);
    }

    // E: light it when it is ready, say what is missing when it is not, show the hours while it fires, and collect what was fired.
    void KilnInteract()
    {
        PotteryRules.KilnPhase phase = KilnPhaseNow;
        int pieces = WellsAndOuthouses.KilnPiecesIn(state), firewood = state.Count(PotteryRules.FirewoodId);
        switch (phase)
        {
            case PotteryRules.KilnPhase.Firing:
                ToolStatus.Flash(KilnPromptText(), 3f);
                return;
            case PotteryRules.KilnPhase.Done:
                KilnCollect();
                return;
            case PotteryRules.KilnPhase.Ready:
                KilnStartFiring(out string started);
                ToolStatus.Flash(started, 4f);
                return;
            default:
                ToolStatus.Flash(PotteryRules.KilnNotReady(pieces, firewood), 4f);
                return;
        }
    }

    // Lights the kiln: takes the fixed 10 Firewood and starts the 24 hours. The firing marker is jobItem; jobHours is the hours left.
    public bool KilnStartFiring(out string message)
    {
        message = "";
        if (state == null || state.kind != PileKind.Kiln || WoodManager.Instance == null || !PotteryRules.CanFire(KilnPhaseNow))
            return false;
        int pieces = WellsAndOuthouses.KilnPiecesIn(state);
        state.Take(PotteryRules.FirewoodId, PotteryRules.KilnFirewood);
        state.jobItem = PotteryRules.FiringMarker;
        state.jobCount = 0;
        state.jobHours = PotteryRules.KilnFireHours;
        WoodManager.Instance.NotifyChanged(state);
        message = PotteryRules.KilnStarted(pieces);
        return true;
    }

    // Everything fired and the Ash go to the pack; whatever does not fit stays in the kiln.
    void KilnCollect()
    {
        int took = 0, ashTook = 0, stayed = 0;
        var ids = new List<string>();
        foreach (PotteryRules.Recipe recipe in PotteryRules.Recipes)
            ids.Add(recipe.firedId);
        ids.Add(PotteryRules.AshId);
        foreach (string id in ids)
        {
            int n = state.Count(id);
            if (n <= 0)
                continue;
            ItemDefinition item = ItemDatabase.Get(id);
            int fits = item != null ? Mathf.Min(n, Inventory.Player.SpaceFor(item)) : 0;
            int added = fits > 0 ? Inventory.AddToPlayer(id, fits) : 0;
            state.Take(id, added);
            if (id == PotteryRules.AshId)
                ashTook += added;
            else
                took += added;
            stayed += n - added;
        }
        if (took + ashTook > 0)
            WoodManager.Instance.NotifyChanged(state);
        ToolStatus.Flash(PotteryRules.KilnTook(took, ashTook, stayed), 4f);
    }

    // R: with the Hammer it takes the kiln down; without, it loads from the pack (KilnLoadFromPack).
    void KilnSecondary()
    {
        if (WoodManager.HammerEquipped)
        {
            Dismantle();
            return;
        }
        PotteryRules.KilnGate gate = PotteryRules.LoadGate(KilnPhaseNow, KilnCarriedLoadable(out _));
        if (gate != PotteryRules.KilnGate.Ok)
        {
            ToolStatus.Flash(PotteryRules.KilnLoadRefusal(gate), 4f);
            return;
        }
        KilnLoadFromPack(out string message);
        ToolStatus.Flash(message, 4f);
    }

    // Puts the dry pieces first (in the recipe order, up to the 12 space units) and then the Firewood (up to 10) from the pack into the kiln. Returns whether anything moved.
    public bool KilnLoadFromPack(out string message)
    {
        message = PotteryRules.KilnLoadRefusal(PotteryRules.KilnGate.NothingFits);
        if (state == null || state.kind != PileKind.Kiln || Inventory == null || WoodManager.Instance == null || !state.KilnOpen)
            return false;

        int used = state.KilnSpaceUsed(), fire = state.Count(PotteryRules.FirewoodId);
        int moved = 0;
        var names = new List<string>();
        foreach (string id in PotteryRules.LoadOrder)
        {
            int n = Mathf.Min(PotteryRules.KilnRoomFor(id, used, fire), Inventory.Player.Count(id));
            if (n <= 0)
                continue;
            if (AudioManager.Instance != null)
                AudioManager.Instance.SilenceNextRemoval();
            int removed = Inventory.Player.RemoveBatches(id, n, (day, count) => state.Add(id, count, 0));
            if (removed <= 0)
                continue;
            moved += removed;
            names.Add($"{removed} {ItemDatabase.Get(id)?.DisplayName ?? id}");
            if (id == PotteryRules.FirewoodId)
                fire += removed;
            else if (PotteryRules.TryFind(id, out PotteryRules.Recipe recipe))
                used += recipe.space * removed;
        }

        if (moved <= 0)
            return false;
        WoodManager.Instance.NotifyChanged(state);
        message = PotteryRules.KilnLoaded(string.Join(", ", names), WellsAndOuthouses.KilnPiecesIn(state), state.Count(PotteryRules.FirewoodId));
        return true;
    }

    // --- The Smithy (Metalworking M3, as the Workshop): R stocks the forge, E collects a finished good or else opens the Smithy screen, R with the Hammer dismantles ---

    bool SmithyWorking => !string.IsNullOrEmpty(state.jobItem) && state.jobHours > 0f;

    // The good being forged. False when the forge is idle, or when the job names a good this build does not know.
    bool SmithyMade(out SmithyRules.Recipe made)
    {
        made = default;
        return SmithyWorking && SmithyRules.TryFind(state.jobItem, out made);
    }

    // The Smithy screen (SmithyScreen) reads the forge through these: the running good, a waiting good, and why a good can or cannot start now.
    public bool SmithyRunning(out SmithyRules.Recipe made) => SmithyMade(out made);
    public bool SmithyWaiting(out SmithyRules.Recipe good) => SmithyProduct(out good, out _);
    public SmithyRules.Gate SmithyGateFor(SmithyRules.Recipe recipe) => SmithyGate(recipe);

    // A finished good waiting to be collected.
    bool SmithyProduct(out SmithyRules.Recipe good, out int count)
    {
        foreach (SmithyRules.Recipe recipe in SmithyRules.Recipes)
        {
            int n = state.Count(recipe.itemId);
            if (n > 0)
            {
                good = recipe;
                count = n;
                return true;
            }
        }
        good = default;
        count = 0;
        return false;
    }

    SmithyRules.Gate SmithyGate(SmithyRules.Recipe recipe) =>
        SmithyRules.CanStart(recipe, SmithyWorking, SmithyProduct(out _, out _), state.Count(SmithyRules.BarId), state.Count(SmithyRules.CharcoalId),
                             state.Count(SmithyRules.SticksId), state.Count(SmithyRules.CordageId));

    string SmithyPromptText()
    {
        if (SmithyProduct(out SmithyRules.Recipe good, out _))
            return SmithyRules.DonePrompt(good);
        if (SmithyMade(out SmithyRules.Recipe made))
            return SmithyRules.WorkingPrompt(made, state.jobHours);
        return SmithyRules.OpenPrompt;
    }

    // The stock materials the pack carries that the forge still has room for.
    int SmithyCarried(out string what)
    {
        what = "";
        int total = 0;
        var names = new List<string>();
        foreach (string id in SmithyRules.StockIds)
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

    string SmithySecondaryPrompt()
    {
        if (WoodManager.HammerEquipped)
            return SmithyRules.DismantlePrompt(SmithyWorking, state.jobHours);
        return SmithyCarried(out string what) > 0 ? SmithyRules.StorePrompt(what) : SmithyRules.DeliverHelp;
    }

    // E: collect a finished good, or else open the Smithy screen (a job starts there, through TryStartForge).
    void SmithyInteract()
    {
        if (SmithyProduct(out SmithyRules.Recipe good, out int waiting))
        {
            ItemDefinition item = ItemDatabase.Get(good.itemId);
            int fits = item != null ? Mathf.Min(waiting, Inventory.Player.SpaceFor(item)) : 0;
            if (fits <= 0)
            {
                ToolStatus.Flash("Too heavy - no room to carry it", 3f);
                return;
            }
            int added = Inventory.AddToPlayer(good.itemId, fits);
            state.Take(good.itemId, added);
            WoodManager.Instance.NotifyChanged(state);
            ToolStatus.Flash(SmithyRules.TookMessage(good, added, waiting - added), 4f);
            return;
        }

        SmithyScreen.Open(this);
    }

    // The one way a job starts (the Smithy screen's Start button calls it): the gate is worked out again on the forge as it is now, so a stale click does nothing. On success it takes
    // the Bar and the Charcoal, sets the job and returns true with the "Started" message; otherwise false with the reason.
    public bool TryStartForge(SmithyRules.Recipe recipe, out string message)
    {
        message = "";
        if (state == null || state.kind != PileKind.Smithy || Inventory == null || WoodManager.Instance == null)
            return false;
        int bars = state.Count(SmithyRules.BarId), charcoal = state.Count(SmithyRules.CharcoalId);
        int sticks = state.Count(SmithyRules.SticksId), cordage = state.Count(SmithyRules.CordageId);
        SmithyRules.Gate gate = SmithyGate(recipe);
        if (gate != SmithyRules.Gate.Ok)
        {
            SmithyMade(out SmithyRules.Recipe made);
            message = SmithyRules.ReasonText(gate, recipe, made, bars, charcoal, state.jobHours, sticks, cordage);
            return false;
        }

        state.Take(SmithyRules.BarId, recipe.bars);
        state.Take(SmithyRules.CharcoalId, recipe.charcoal);
        state.Take(SmithyRules.SticksId, recipe.sticks);
        state.Take(SmithyRules.CordageId, recipe.cordage);
        state.jobItem = recipe.itemId;
        state.jobHours = recipe.hours;
        WoodManager.Instance.NotifyChanged(state);
        message = SmithyRules.StartedMessage(recipe);
        return true;
    }

    // R: with the Hammer it dismantles; without, it stocks the forge from the pack (ForgeStockFromPack, which the Smithy screen's button and R key share).
    void SmithySecondary()
    {
        if (WoodManager.HammerEquipped)
        {
            Dismantle();
            return;
        }

        ForgeStockFromPack(out string message);
        ToolStatus.Flash(message, 4f);
    }

    // What the pack could put in the forge now (the pure rule decides: nothing carried, the forge full of what is carried, or something to move).
    public SmithyRules.StockGate ForgeStockGateNow() =>
        state == null || Inventory == null ? SmithyRules.StockGate.NothingCarried
            : SmithyRules.StockState(Inventory.Player.Count(SmithyRules.BarId), Inventory.Player.Count(SmithyRules.CharcoalId), state.Count(SmithyRules.BarId), state.Count(SmithyRules.CharcoalId),
                                     Inventory.Player.Count(SmithyRules.SticksId), Inventory.Player.Count(SmithyRules.CordageId), state.Count(SmithyRules.SticksId), state.Count(SmithyRules.CordageId));

    // Puts what the pack carries of Iron Bars and Charcoal into the forge, up to the caps. Works while a job runs. Returns whether anything moved, with the message R flashes. Never
    // dismantles: that is the Hammer's R in the world.
    public bool ForgeStockFromPack(out string message)
    {
        message = "Nothing you carry fits. " + SmithyRules.DeliverHelp;
        if (state == null || state.kind != PileKind.Smithy || Inventory == null || WoodManager.Instance == null)
            return false;

        int stored = 0;
        var names = new List<string>();
        foreach (string id in SmithyRules.StockIds)
        {
            if (!state.Accepts(id))
                continue;
            int n = Mathf.Min(SmithyRules.CapOf(id) - state.Count(id), Inventory.Player.Count(id));
            if (n <= 0)
                continue;
            if (AudioManager.Instance != null)
                AudioManager.Instance.SilenceNextRemoval();
            int removed = Inventory.Player.RemoveBatches(id, n, (day, count) => state.Add(id, count, 0));
            if (removed > 0)
            {
                stored += removed;
                names.Add($"{removed} {ItemDatabase.Get(id)?.DisplayName ?? id}");
            }
        }

        if (stored <= 0)
            return false;
        WoodManager.Instance.NotifyChanged(state);
        message = SmithyRules.Stored(string.Join(", ", names), state.Count(SmithyRules.BarId), state.Count(SmithyRules.CharcoalId), state.Count(SmithyRules.SticksId), state.Count(SmithyRules.CordageId));
        return true;
    }

    // --- The Bloomery (Metalworking M2) ---

    BloomeryRules.Phase SmeltPhaseNow => WellsAndOuthouses.BloomeryPhaseOf(state);

    string BloomeryPromptText() =>
        BloomeryRules.Prompt(SmeltPhaseNow, state.BloomeryOre, state.BloomeryCharcoal, state.BloomeryBars, state.jobCount, state.jobHours);

    BloomeryRules.LoadPlan BloomeryPlan(out int oreCarried, out int charcoalCarried)
    {
        oreCarried = Inventory.Player.Count(BloomeryRules.OreId);
        charcoalCarried = Inventory.Player.Count(BloomeryRules.CharcoalId);
        return BloomeryRules.PlanLoad(state.BloomeryOre, state.BloomeryCharcoal, oreCarried, charcoalCarried);
    }

    string BloomerySecondaryPrompt()
    {
        if (WoodManager.HammerEquipped)
            return BloomeryRules.DismantlePrompt(SmeltPhaseNow, state.jobHours);
        BloomeryRules.LoadPlan plan = BloomeryPlan(out int oreCarried, out int charcoalCarried);
        return BloomeryRules.LoadPrompt(plan, state.BloomeryOre, state.BloomeryCharcoal, oreCarried, charcoalCarried);
    }

    // E: collect any finished Bars first; otherwise start a smelt when it holds at least 2 Ore and 2 Charcoal, show the hours while it smelts, and say what is missing when it is not ready.
    void BloomeryInteract()
    {
        switch (SmeltPhaseNow)
        {
            case BloomeryRules.Phase.Done:
                BloomeryCollect();
                return;
            case BloomeryRules.Phase.Smelting:
                ToolStatus.Flash(BloomeryPromptText(), 3f);
                return;
            case BloomeryRules.Phase.Ready:
                BloomeryStart(out string started);
                ToolStatus.Flash(started, 4f);
                return;
            default:
                ToolStatus.Flash(BloomeryRules.NotReady(state.BloomeryOre, state.BloomeryCharcoal), 4f);
                return;
        }
    }

    // Starts a smelt: the Ore and Charcoal it uses are taken out of the stock, the Bars it will make are fixed (jobCount) and the 10 hours start. The marker is jobItem; jobHours is the
    // hours left. What is left in contents is stock for the next smelt.
    public bool BloomeryStart(out string message)
    {
        message = "";
        if (state == null || state.kind != PileKind.Bloomery || WoodManager.Instance == null || !BloomeryRules.CanStart(SmeltPhaseNow))
            return false;
        int bars = BloomeryRules.BarsFor(state.BloomeryOre, state.BloomeryCharcoal);
        state.Take(BloomeryRules.OreId, BloomeryRules.OreUsed(bars));
        state.Take(BloomeryRules.CharcoalId, BloomeryRules.CharcoalUsed(bars));
        state.jobItem = BloomeryRules.SmeltMarker;
        state.jobCount = bars;
        state.jobHours = BloomeryRules.SmeltHours;
        WoodManager.Instance.NotifyChanged(state);
        message = BloomeryRules.Started(bars, state.BloomeryOre, state.BloomeryCharcoal);
        return true;
    }

    // The Iron Bars go to the pack, as many as it holds; the rest stay in the furnace.
    void BloomeryCollect()
    {
        int waiting = state.BloomeryBars;
        ItemDefinition item = ItemDatabase.Get(BloomeryRules.BarId);
        int fits = item != null ? Mathf.Min(waiting, Inventory.Player.SpaceFor(item)) : 0;
        int added = fits > 0 ? Inventory.AddToPlayer(BloomeryRules.BarId, fits) : 0;
        if (added > 0)
        {
            state.Take(BloomeryRules.BarId, added);
            WoodManager.Instance.NotifyChanged(state);
        }
        ToolStatus.Flash(BloomeryRules.Took(added, waiting - added), 4f);
    }

    // R: with the Hammer it takes the bloomery down; without, it loads Ore and Charcoal from the pack, at any time.
    void BloomerySecondary()
    {
        if (WoodManager.HammerEquipped)
        {
            Dismantle();
            return;
        }
        BloomeryRules.LoadPlan plan = BloomeryPlan(out int oreCarried, out int charcoalCarried);
        if (!plan.Any)
        {
            ToolStatus.Flash(BloomeryRules.LoadRefusal(state.BloomeryOre, state.BloomeryCharcoal, oreCarried, charcoalCarried), 4f);
            return;
        }
        BloomeryLoadFromPack(out string message);
        ToolStatus.Flash(message, 4f);
    }

    // Puts the Ore and the Charcoal carried into the furnace, each up to 4. Returns whether anything moved.
    public bool BloomeryLoadFromPack(out string message)
    {
        message = BloomeryRules.LoadRefusal(0, 0, 0, 0);
        if (state == null || state.kind != PileKind.Bloomery || Inventory == null || WoodManager.Instance == null)
            return false;
        BloomeryRules.LoadPlan plan = BloomeryPlan(out int oreCarried, out int charcoalCarried);
        if (!plan.Any)
            return false;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval();
        int ore = plan.ore > 0 ? Inventory.Player.RemoveBatches(BloomeryRules.OreId, plan.ore, (day, count) => state.Add(BloomeryRules.OreId, count, 0)) : 0;
        int charcoal = plan.charcoal > 0 ? Inventory.Player.RemoveBatches(BloomeryRules.CharcoalId, plan.charcoal, (day, count) => state.Add(BloomeryRules.CharcoalId, count, 0)) : 0;
        if (ore + charcoal <= 0)
            return false;
        WoodManager.Instance.NotifyChanged(state);
        var moved = new BloomeryRules.LoadPlan { ore = ore, charcoal = charcoal };
        message = BloomeryRules.Loaded(moved, state.BloomeryOre, state.BloomeryCharcoal, oreCarried, charcoalCarried);
        return true;
    }

    // --- The Charcoal Pit (Metalworking M1) ---

    CharcoalRules.Phase PitPhaseNow => WellsAndOuthouses.PitPhaseOf(state);

    // The Charcoal a lit pit would make now, the burn will make, or is waiting: whichever the phase is about.
    int PitCharcoalShown(CharcoalRules.Phase phase) =>
        phase == CharcoalRules.Phase.Ready ? CharcoalRules.Yield(state.PitLogs) : phase == CharcoalRules.Phase.Burning ? state.jobCount : state.PitCharcoal;

    string PitPromptText()
    {
        CharcoalRules.Phase phase = PitPhaseNow;
        return CharcoalRules.Prompt(phase, state.PitLogs, PitCharcoalShown(phase), state.jobHours);
    }

    string PitSecondaryPrompt()
    {
        CharcoalRules.Phase phase = PitPhaseNow;
        if (WoodManager.HammerEquipped)
            return CharcoalRules.DismantlePrompt(phase, state.jobHours);
        int carried = Inventory.Player.Count(CharcoalRules.LogsId);
        CharcoalRules.Gate gate = CharcoalRules.LoadGate(phase, state.PitLogs, carried);
        return CharcoalRules.LoadPrompt(gate, CharcoalRules.LoadAmount(state.PitLogs, carried));
    }

    // E: light it when it is ready, say what is missing when it is not, show the hours while it burns, and collect the Charcoal.
    void PitInteract()
    {
        CharcoalRules.Phase phase = PitPhaseNow;
        switch (phase)
        {
            case CharcoalRules.Phase.Burning:
                ToolStatus.Flash(PitPromptText(), 3f);
                return;
            case CharcoalRules.Phase.Done:
                PitCollect();
                return;
            case CharcoalRules.Phase.Ready:
                PitLight(out string started);
                ToolStatus.Flash(started, 4f);
                return;
            default:
                ToolStatus.Flash(CharcoalRules.NotReady(state.PitLogs), 4f);
                return;
        }
    }

    // Lights the pit: the Logs loaded are used up, the Charcoal they will make is fixed (jobCount) and the 24 hours start. The burn marker is jobItem; jobHours is the hours left.
    public bool PitLight(out string message)
    {
        message = "";
        if (state == null || state.kind != PileKind.CharcoalPit || WoodManager.Instance == null || !CharcoalRules.CanLight(PitPhaseNow))
            return false;
        int logs = state.PitLogs;
        int charcoal = CharcoalRules.Yield(logs);
        state.Take(CharcoalRules.LogsId, logs);
        state.jobItem = CharcoalRules.BurnMarker;
        state.jobCount = charcoal;
        state.jobHours = CharcoalRules.BurnHours;
        WoodManager.Instance.NotifyChanged(state);
        message = CharcoalRules.Started(logs, charcoal);
        return true;
    }

    // The Charcoal goes to the pack, as much as it holds; the rest stays in the pit.
    void PitCollect()
    {
        int waiting = state.PitCharcoal;
        ItemDefinition item = ItemDatabase.Get(CharcoalRules.CharcoalId);
        int fits = item != null ? Mathf.Min(waiting, Inventory.Player.SpaceFor(item)) : 0;
        int added = fits > 0 ? Inventory.AddToPlayer(CharcoalRules.CharcoalId, fits) : 0;
        if (added > 0)
        {
            state.Take(CharcoalRules.CharcoalId, added);
            WoodManager.Instance.NotifyChanged(state);
        }
        ToolStatus.Flash(CharcoalRules.Took(added, waiting - added), 4f);
    }

    // R: with the Hammer it takes the pit down; without, it loads the Logs from the pack.
    void PitSecondary()
    {
        if (WoodManager.HammerEquipped)
        {
            Dismantle();
            return;
        }
        int carried = Inventory.Player.Count(CharcoalRules.LogsId);
        CharcoalRules.Gate gate = CharcoalRules.LoadGate(PitPhaseNow, state.PitLogs, carried);
        if (gate != CharcoalRules.Gate.Ok)
        {
            ToolStatus.Flash(CharcoalRules.LoadRefusal(gate), 4f);
            return;
        }
        PitLoadFromPack(out string message);
        ToolStatus.Flash(message, 4f);
    }

    // Puts the Logs carried into the pit, up to its six. Returns whether anything moved.
    public bool PitLoadFromPack(out string message)
    {
        message = CharcoalRules.LoadRefusal(CharcoalRules.Gate.NothingCarried);
        if (state == null || state.kind != PileKind.CharcoalPit || Inventory == null || WoodManager.Instance == null || !state.PitOpen)
            return false;
        int amount = CharcoalRules.LoadAmount(state.PitLogs, Inventory.Player.Count(CharcoalRules.LogsId));
        if (amount <= 0)
            return false;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval();
        int removed = Inventory.Player.RemoveBatches(CharcoalRules.LogsId, amount, (day, count) => state.Add(CharcoalRules.LogsId, count, 0));
        if (removed <= 0)
            return false;
        WoodManager.Instance.NotifyChanged(state);
        message = CharcoalRules.Loaded(removed, state.PitLogs);
        return true;
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
            return EquippedShovel ? $"Outhouse  (the pit is full)\nHold click with the Shovel to dig a new pit{dug}"
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
            if (state.kind == PileKind.PotterShed)
                return PotterySecondaryPrompt();
            if (state.kind == PileKind.Kiln)
                return KilnSecondaryPrompt();
            if (state.kind == PileKind.CharcoalPit)
                return PitSecondaryPrompt();
            if (state.kind == PileKind.Bloomery)
                return BloomerySecondaryPrompt();
            if (state.kind == PileKind.Smithy)
                return SmithySecondaryPrompt();
            if (state.kind == PileKind.Workshop)
                return WorkshopSecondaryPrompt();
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
            case PileKind.LeachHopper: HopperInteract(); break;
            case PileKind.Workshop: WorkshopInteract(); break;
            case PileKind.PotterShed: PotteryInteract(); break;
            case PileKind.Kiln: KilnInteract(); break;
            case PileKind.CharcoalPit: PitInteract(); break;
            case PileKind.Bloomery: BloomeryInteract(); break;
            case PileKind.Smithy: SmithyInteract(); break;
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
        ToolStatus.Flash(EquippedShovel ? "Hold click with the Shovel on the outhouse to dig a new pit" : "Equip the Shovel, then hold click on the outhouse to dig a new pit", 3.5f);
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
        else if (state.kind == PileKind.Kiln)
        {
            KilnSecondary();
        }
        else if (state.kind == PileKind.CharcoalPit)
        {
            PitSecondary();
        }
        else if (state.kind == PileKind.Bloomery)
        {
            BloomerySecondary();
        }
        else if (state.kind == PileKind.Smithy)
        {
            SmithySecondary();
        }
        else if (state.kind == PileKind.PotterShed)
        {
            PotterySecondary();
        }
        else if (state.kind == PileKind.Workshop)
        {
            WorkshopSecondary();
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
            ToolStatus.Flash($"Nothing you carry fits yet - {StageNeeds(true)}", 4f);
            return;
        }

        wood.NotifyChanged(state);
        // The delivery that supplies the last missing material: a big notice, so the next step isn't missed. The Well is finished
        // from the Build tab with the Hammer (WellsAndOuthouses.FinishSite), as the Small Cabin is.
        if (!wasStocked && WellsAndOuthouses.SiteNeeds(state) == null)
            ToolStatus.Banner($"{Name} fully stocked - equip the Hammer and hold click on it to build it", 10f);
        else
            ToolStatus.Flash($"Stored - {StageNeeds(true)}", 4f);
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
        // A Workshop is not taken down while its bench is making a piece (the materials are already in the work).
        if (state.kind == PileKind.Workshop && WorkshopWorking)
        {
            ToolStatus.Flash(WorkshopRules.DismantlePrompt(true, state.jobHours), 4f);
            return;
        }
        // A Potter's Shed is not taken down while it is shaping or any piece is still wet on the shelf (the materials are in the work).
        if (state.kind == PileKind.PotterShed && (PotteryWorking || PotteryRules.WetPieces(PotteryShelf(), WellsAndOuthouses.PotteryNow()) > 0))
        {
            ToolStatus.Flash(PotteryRules.DismantlePrompt(PotteryWorking, state.jobHours, PotteryRules.WetPieces(PotteryShelf(), WellsAndOuthouses.PotteryNow())), 4f);
            return;
        }
        // A Kiln is taken down only when it is empty and not firing (what is in it would be lost).
        if (state.kind == PileKind.Kiln && !PotteryRules.CanDismantleKiln(KilnPhaseNow))
        {
            ToolStatus.Flash(PotteryRules.KilnDismantlePrompt(KilnPhaseNow, state.jobHours), 4f);
            return;
        }
        // A Charcoal Pit is not taken down while it burns (the Logs are already in the fire).
        if (state.kind == PileKind.CharcoalPit && !CharcoalRules.CanDismantle(PitPhaseNow))
        {
            ToolStatus.Flash(CharcoalRules.DismantlePrompt(PitPhaseNow, state.jobHours), 4f);
            return;
        }
        // A Smithy is not taken down while a good is being forged (the Bar and Charcoal are already in the work), as the Workshop is not while a piece is being made.
        if (state.kind == PileKind.Smithy && SmithyWorking)
        {
            ToolStatus.Flash(SmithyRules.DismantlePrompt(true, state.jobHours), 4f);
            return;
        }
        // A Bloomery is not taken down while it smelts (the Ore and Charcoal are already in the fire).
        if (state.kind == PileKind.Bloomery && !BloomeryRules.CanDismantle(SmeltPhaseNow))
        {
            ToolStatus.Flash(BloomeryRules.DismantlePrompt(SmeltPhaseNow, state.jobHours), 4f);
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
        // A Workshop gives back the stock in its bench and any finished piece whole, on top of half the materials.
        if (state.kind == PileKind.Workshop)
        {
            foreach (string stockId in WorkshopRules.StockIds)
                refund.Return(stockId, state.Count(stockId));
            foreach (WorkshopRules.Recipe piece in WorkshopRules.Recipes)
                refund.Return(piece.itemId, state.Count(piece.itemId));
        }
        // A Potter's Shed gives back the Clay and Sand in it and the dry pieces on its shelf whole, on top of half the materials.
        if (state.kind == PileKind.PotterShed)
        {
            foreach (string stockId in PotteryRules.StockIds)
                refund.Return(stockId, state.Count(stockId));
            foreach (PotteryRules.Recipe piece in PotteryRules.Recipes)
                refund.Return(piece.itemId, state.Count(piece.itemId));
        }
        // A Charcoal Pit gives back the Logs loaded and the Charcoal waiting whole, on top of half the Clay and Tall Grass.
        if (state.kind == PileKind.CharcoalPit)
        {
            refund.Return(CharcoalRules.LogsId, state.Count(CharcoalRules.LogsId));
            refund.Return(CharcoalRules.CharcoalId, state.Count(CharcoalRules.CharcoalId));
        }
        // A Smithy gives back the Iron Bars and Charcoal in its forge and any finished good whole, on top of half the materials.
        if (state.kind == PileKind.Smithy)
        {
            foreach (string stockId in SmithyRules.StockIds)
                refund.Return(stockId, state.Count(stockId));
            foreach (SmithyRules.Recipe good in SmithyRules.Recipes)
                refund.Return(good.itemId, state.Count(good.itemId));
        }
        // A Bloomery gives back the Ore and Charcoal stocked and any Iron Bars waiting whole, on top of half the Stone and Clay.
        if (state.kind == PileKind.Bloomery)
        {
            refund.Return(BloomeryRules.OreId, state.Count(BloomeryRules.OreId));
            refund.Return(BloomeryRules.CharcoalId, state.Count(BloomeryRules.CharcoalId));
            refund.Return(BloomeryRules.BarId, state.Count(BloomeryRules.BarId));
        }
        WoodManager.Instance.RemovePile(state);
        ToolStatus.Flash(refund.Describe(name), 5f);
    }
}
