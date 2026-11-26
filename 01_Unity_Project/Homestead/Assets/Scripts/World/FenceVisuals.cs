using System.Collections.Generic;
using UnityEngine;

// What a garden fence panel or gate looks like (slice GB4a, FenceRules). The panel runs along its local X, centred, as long as FenceRules.LengthOf says (a panel cut to
// fit a ring is shorter), and stands up from the ground at its middle. Pure meshes with no colliders, so the same builder serves the real panel (PlacedStructure,
// whose interaction box is the panel itself and so stops the player) and Placement Mode's ghost. A Gate's frame is drawn here; its leaf is a SwingDoor that
// PlacedStructure hangs in it (FenceVisuals.GateLeaf), so it swings open as the player walks into it.
// THE SWAP POINTS: Resources/Art/structure_wattlefence.prefab, structure_deerfence.prefab and structure_fencegate.prefab (the gate's posts and top bar only, not
// the leaf), each made at the full length along +X and centred, replace the primitives below; a shorter panel squeezes the prefab along X.
public static class FenceVisuals
{
    static readonly Color Withy = new Color(0.45f, 0.36f, 0.24f);
    static readonly Color Stake = new Color(0.40f, 0.31f, 0.21f);
    static readonly Color Wire = new Color(0.30f, 0.31f, 0.31f);
    static readonly Color Post = new Color(0.36f, 0.29f, 0.20f);
    static readonly Color Sawn = new Color(0.62f, 0.50f, 0.34f);

    public static string ArtId(PileKind kind) => "structure_" + kind.ToString().ToLowerInvariant();

    public const float GateLeafHeight = 1.9f;
    public const float BoxThickness = 0.14f;

    public static void Build(Transform parent, WoodPileState state, Material bark, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        FenceRules.Type type = WoodManager.FenceTypeOf(state.kind);
        float length = FenceRules.LengthOf(type, state.jobHours);
        float height = FenceRules.Height(type);
        // The panel is the box (solid: it stops the player); a Gate's box is only its hinge post, so the player walks through the opening.
        boxCenter = type == FenceRules.Type.Gate ? new Vector3(-length / 2f, height / 2f, 0f) : new Vector3(0f, height / 2f, 0f);
        boxSize = type == FenceRules.Type.Gate ? new Vector3(0.12f, height, 0.12f) : new Vector3(length, height, BoxThickness);

        GameObject prefab = Resources.Load<GameObject>("Art/" + ArtId(state.kind));
        if (prefab != null)
        {
            GameObject art = Object.Instantiate(prefab, parent, false);
            art.name = prefab.name;
            art.transform.localPosition = Vector3.zero;
            art.transform.localRotation = Quaternion.identity;
            art.transform.localScale = new Vector3(length / FenceRules.FullLength(type), 1f, 1f);
            foreach (Collider c in art.GetComponentsInChildren<Collider>())
                Object.DestroyImmediate(c);
            pieces?.Add(art);
            return;
        }

        switch (type)
        {
            case FenceRules.Type.Wattle: Wattle(parent, pieces, length, bark, state.id); break;
            case FenceRules.Type.Deer: Deer(parent, pieces, length, bark); break;
            default: GateFrame(parent, pieces, length); break;
        }
    }

    // Stakes about every 0.6 m with withies woven between them, in and out.
    static void Wattle(Transform parent, List<GameObject> pieces, float length, Material bark, int seed)
    {
        var random = new System.Random(seed * 31 + 7);
        int stakes = Mathf.Max(2, Mathf.RoundToInt(length / 0.6f) + 1);
        for (int i = 0; i < stakes; i++)
        {
            float x = -length / 2f + length * i / (stakes - 1);
            float top = FenceRules.WattleHeight + 0.05f + (float)random.NextDouble() * 0.08f;
            Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(x, (top - 0.2f) / 2f, 0f), Vector3.zero, new Vector3(0.05f, (top + 0.2f) / 2f, 0.05f), bark), Stake, bark);
        }
        const int rows = 9;
        for (int r = 0; r < rows; r++)
        {
            float y = 0.1f + r * (FenceRules.WattleHeight - 0.15f) / (rows - 1);
            float z = (r % 2 == 0 ? 1f : -1f) * 0.025f;
            Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, y, z), new Vector3(0f, 0f, 90f), new Vector3(0.045f, length / 2f, 0.045f), bark), Withy, bark);
        }
    }

    // A post at each end (and one in the middle of a full panel), wire mesh between them: horizontal strands every 0.25 m, verticals every 0.3 m.
    static void Deer(Transform parent, List<GameObject> pieces, float length, Material bark)
    {
        float h = FenceRules.DeerHeight;
        var posts = new List<float> { -length / 2f, length / 2f };
        if (length > 1.6f)
            posts.Add(0f);
        foreach (float x in posts)
            Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(x, (h + 0.05f - 0.3f) / 2f, 0f), Vector3.zero, new Vector3(0.11f, (h + 0.05f + 0.3f) / 2f, 0.11f), bark), Post, bark);
        for (float y = 0.05f; y <= h - 0.04f; y += 0.25f)
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, y, 0.06f), Vector3.zero, new Vector3(length, 0.008f, 0.008f), null), Wire, null);
        int verticals = Mathf.Max(2, Mathf.RoundToInt(length / 0.3f));
        for (int i = 0; i <= verticals; i++)
        {
            float x = -length / 2f + length * i / verticals;
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(x, h / 2f, 0.06f), Vector3.zero, new Vector3(0.008f, h - 0.1f, 0.008f), null), Wire, null);
        }
    }

    // Two sawn posts and a bar across the top; the leaf hangs from the left post.
    static void GateFrame(Transform parent, List<GameObject> pieces, float length)
    {
        float h = FenceRules.DeerHeight;
        foreach (float x in new[] { -length / 2f, length / 2f })
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(x, (h + 0.1f - 0.3f) / 2f, 0f), Vector3.zero, new Vector3(0.1f, h + 0.1f + 0.3f, 0.1f), null), Sawn, null);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, h + 0.05f, 0f), Vector3.zero, new Vector3(length + 0.1f, 0.09f, 0.09f), null), Sawn, null);
    }

    // The gate's leaf: hung on the left post, the opening's width less the posts, swinging as the player walks into it.
    public static SwingDoor GateLeaf(Transform parent, WoodPileState state)
    {
        float length = FenceRules.LengthOf(FenceRules.Type.Gate, state.jobHours);
        SwingDoor door = SwingDoor.Create(parent, new Vector3(-length / 2f + 0.06f, 0.08f, 0f), length - 0.12f, GateLeafHeight, 0.04f,
                                          Resources.Load<Material>("Art/structure_fencegate_planks"), "Gate Leaf");
        door.openDistance = 1.2f;
        return door;
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

    // Bark where there is bark; a plain colour otherwise (the wire, the sawn gate).
    static GameObject Tinted(GameObject piece, Color color, Material bark)
    {
        if (bark != null)
            return piece;
        var renderer = piece.GetComponent<MeshRenderer>();
        var block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);
        block.SetColor("_BaseColor", color);
        block.SetColor("_Color", color);
        renderer.SetPropertyBlock(block);
        return piece;
    }
}
