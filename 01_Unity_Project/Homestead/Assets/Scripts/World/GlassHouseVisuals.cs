using System.Collections.Generic;
using UnityEngine;

// What a Glass Greenhouse and its site look like (Greenhouses.md, slice G3; GlassHouseRules). A timber frame on a low stone footing, glazed walls WallHeight high
// and a pitched glass roof along local Z, the door in the front gable at local -Z. The beds and seedling benches inside are their own piles.
//   Vents shut: the roof vents lie flat.   Vents open: they are propped up along the ridge, and the door stands open.
// What E aims at is the front wall beside the door (the box); the walls and roof are solid (pieces named Solid, given colliders by PlacedStructure), so the way in
// is the door.
// THE SWAP POINT: Resources/Art/structure_glasshousesmall.prefab (and _medium, _large): centred on the ground, the door end to Unity -Z, with children named
// Vents_Shut and Vents_Open shown by state.
public static class GlassHouseVisuals
{
    static readonly Color Timber = new Color(0.82f, 0.8f, 0.74f);   // white-painted frame
    static readonly Color StoneGrey = new Color(0.5f, 0.49f, 0.46f);
    static readonly Color Pane = new Color(0.8f, 0.88f, 0.9f);

    public static string ArtId(PileKind kind) => "structure_" + kind.ToString().ToLowerInvariant();

    public static void Build(Transform parent, WoodPileState state, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        GlassHouseRules.Size size = WoodManager.GlassSizeOf(state.kind);
        float w = GlassHouseRules.Width(size), l = GlassHouseRules.Length(size), wall = GlassHouseRules.WallHeight, ridge = GlassHouseRules.RidgeHeight(size);
        bool open = WoodManager.GlassVentsAreOpen(state);
        float side = w / 2f - GlassHouseRules.DoorWidth / 2f;
        boxCenter = new Vector3(-(GlassHouseRules.DoorWidth / 2f + side / 2f), 0.9f, -l / 2f);
        boxSize = new Vector3(side, 1.8f, 0.12f);
        Solids(parent, pieces, w, l, wall, ridge, side);

        GameObject prefab = Resources.Load<GameObject>("Art/" + ArtId(state.kind));
        if (prefab != null)
        {
            GameObject art = Object.Instantiate(prefab, parent, false);
            art.name = prefab.name;
            art.transform.localPosition = Vector3.zero;
            art.transform.localRotation = Quaternion.identity;
            foreach (Collider c in art.GetComponentsInChildren<Collider>())
                Object.DestroyImmediate(c);
            foreach (Transform t in art.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Vents_Open") t.gameObject.SetActive(open);
                else if (t.name == "Vents_Shut") t.gameObject.SetActive(!open);
            }
            pieces?.Add(art);
            return;
        }

        Material glass = Resources.Load<Material>("Art/tool_lantern_glass");
        // The stone footing.
        foreach (float x in new[] { -w / 2f, w / 2f })
            Tinted(Piece(parent, pieces, "Footing", PrimitiveType.Cube, new Vector3(x, 0.15f, 0f), Vector3.zero, new Vector3(0.25f, 0.3f, l + 0.25f)), StoneGrey);
        foreach (float z in new[] { -l / 2f, l / 2f })
            Tinted(Piece(parent, pieces, "Footing", PrimitiveType.Cube, new Vector3(0f, 0.15f, z), Vector3.zero, new Vector3(w, 0.3f, 0.25f)), StoneGrey);
        // Corner and side posts, and the wall plates.
        int bays = Mathf.Max(2, Mathf.RoundToInt(l / 1.2f));
        for (int i = 0; i <= bays; i++)
        {
            float z = -l / 2f + l * i / bays;
            foreach (float x in new[] { -w / 2f, w / 2f })
                Tinted(Piece(parent, pieces, "Post", PrimitiveType.Cube, new Vector3(x, (0.3f + wall) / 2f, z), Vector3.zero, new Vector3(0.07f, wall - 0.3f, 0.07f)), Timber);
            // the rafters up to the ridge
            float run = Mathf.Sqrt(w * w / 4f + (ridge - wall) * (ridge - wall)), tilt = Mathf.Atan2(ridge - wall, w / 2f) * Mathf.Rad2Deg;
            foreach (float s in new[] { -1f, 1f })
                Tinted(Piece(parent, pieces, "Rafter", PrimitiveType.Cube, new Vector3(s * w / 4f, (wall + ridge) / 2f, z), new Vector3(0f, 0f, -s * tilt), new Vector3(run, 0.06f, 0.06f)), Timber);
        }
        Tinted(Piece(parent, pieces, "Ridge", PrimitiveType.Cube, new Vector3(0f, ridge, 0f), Vector3.zero, new Vector3(0.08f, 0.08f, l)), Timber);
        // The glass: both long walls, the back gable wall and the front either side of the door, and both roof slopes.
        foreach (float x in new[] { -w / 2f, w / 2f })
            Glass(parent, pieces, glass, new Vector3(x, (0.3f + wall) / 2f, 0f), Vector3.zero, new Vector3(0.01f, wall - 0.3f, l));
        Glass(parent, pieces, glass, new Vector3(0f, (0.3f + wall) / 2f, l / 2f), Vector3.zero, new Vector3(w, wall - 0.3f, 0.01f));
        foreach (float s in new[] { -1f, 1f })
            Glass(parent, pieces, glass, new Vector3(s * (GlassHouseRules.DoorWidth / 2f + side / 2f), (0.3f + wall) / 2f, -l / 2f), Vector3.zero, new Vector3(side, wall - 0.3f, 0.01f));
        float roofRun = Mathf.Sqrt(w * w / 4f + (ridge - wall) * (ridge - wall)), roofTilt = Mathf.Atan2(ridge - wall, w / 2f) * Mathf.Rad2Deg;
        foreach (float s in new[] { -1f, 1f })
            Glass(parent, pieces, glass, new Vector3(s * w / 4f, (wall + ridge) / 2f + 0.04f, 0f), new Vector3(0f, 0f, -s * roofTilt), new Vector3(roofRun, 0.01f, l));
        // The roof vents along the ridge, propped open or lying shut.
        for (int i = 0; i < Mathf.Max(1, bays / 2); i++)
        {
            float z = -l / 2f + l * (i + 0.5f) / Mathf.Max(1, bays / 2);
            Tinted(Piece(parent, pieces, "Vent", PrimitiveType.Cube, new Vector3(0.35f, ridge + 0.03f + (open ? 0.15f : 0f), z), new Vector3(0f, 0f, open ? -35f : -roofTilt), new Vector3(0.7f, 0.04f, 0.9f)), Timber);
        }
    }

    // The walls and roof you bump into, as invisible boxes (PlacedStructure gives every piece named Solid a collider): the front either side of the door and above it,
    // the back, both long sides, and the roof (so it counts as overhead cover).
    static void Solids(Transform parent, List<GameObject> pieces, float w, float l, float wall, float ridge, float side)
    {
        float gap = GlassHouseRules.DoorWidth / 2f;
        Solid(parent, pieces, new Vector3(gap + side / 2f, wall / 2f, -l / 2f), new Vector3(side, wall, 0.12f));
        Solid(parent, pieces, new Vector3(-(gap + side / 2f), wall / 2f, -l / 2f), new Vector3(side, wall, 0.12f));
        Solid(parent, pieces, new Vector3(0f, (GlassHouseRules.DoorHeight + ridge) / 2f, -l / 2f), new Vector3(gap * 2f, ridge - GlassHouseRules.DoorHeight, 0.12f));
        Solid(parent, pieces, new Vector3(0f, ridge / 2f, l / 2f), new Vector3(w, ridge, 0.12f));
        foreach (float s in new[] { -1f, 1f })
            Solid(parent, pieces, new Vector3(s * (w / 2f - 0.05f), wall / 2f, 0f), new Vector3(0.1f, wall, l));
        Solid(parent, pieces, new Vector3(0f, ridge + 0.05f, 0f), new Vector3(w * 0.6f, 0.1f, l));
    }

    static void Solid(Transform parent, List<GameObject> pieces, Vector3 at, Vector3 scale)
    {
        GameObject piece = Piece(parent, pieces, "Solid", PrimitiveType.Cube, at, Vector3.zero, scale);
        piece.GetComponent<MeshRenderer>().enabled = false;
    }

    static void Glass(Transform parent, List<GameObject> pieces, Material glass, Vector3 at, Vector3 euler, Vector3 scale)
    {
        GameObject pane = Piece(parent, pieces, "Glass", PrimitiveType.Cube, at, euler, scale);
        if (glass != null) pane.GetComponent<MeshRenderer>().sharedMaterial = glass;
        else Tinted(pane, Pane);
    }

    // --- The site: corner stakes and string lines round the footprint, and the delivered materials stacked outside the door end. ---

    public static void BuildSite(Transform parent, WoodPileState state, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        GlassHouseRules.Size size = WoodManager.GlassSizeOf(state.kind);
        float w = GlassHouseRules.Width(size), l = GlassHouseRules.Length(size);
        float hx = w / 2f, hz = l / 2f;
        foreach (var c in new[] { new Vector2(-hx, -hz), new Vector2(hx, -hz), new Vector2(hx, hz), new Vector2(-hx, hz) })
            Tinted(Piece(parent, pieces, "Stake", PrimitiveType.Cylinder, new Vector3(c.x, 0.3f, c.y), Vector3.zero, new Vector3(0.05f, 0.3f, 0.05f)), Timber);
        Color twine = new Color(0.85f, 0.8f, 0.6f);
        Tinted(Piece(parent, pieces, "Line", PrimitiveType.Cube, new Vector3(0f, 0.5f, -hz), Vector3.zero, new Vector3(w, 0.01f, 0.01f)), twine);
        Tinted(Piece(parent, pieces, "Line", PrimitiveType.Cube, new Vector3(0f, 0.5f, hz), Vector3.zero, new Vector3(w, 0.01f, 0.01f)), twine);
        Tinted(Piece(parent, pieces, "Line", PrimitiveType.Cube, new Vector3(-hx, 0.5f, 0f), Vector3.zero, new Vector3(0.01f, 0.01f, l)), twine);
        Tinted(Piece(parent, pieces, "Line", PrimitiveType.Cube, new Vector3(hx, 0.5f, 0f), Vector3.zero, new Vector3(0.01f, 0.01f, l)), twine);

        float z = -hz - 0.8f;
        boxCenter = new Vector3(0f, 0.35f, z);
        boxSize = new Vector3(2.4f, 0.7f, 1.0f);
        Tinted(Piece(parent, pieces, "Pallet", PrimitiveType.Cube, new Vector3(0f, 0.04f, z), Vector3.zero, new Vector3(2.4f, 0.08f, 1.0f)), new Color(0.5f, 0.38f, 0.24f));
        int lumber = state.Count(GlassHouseRules.LumberId), stone = state.Count(GlassHouseRules.StoneId), panes = state.Count(GlassHouseRules.PaneId);
        for (int i = 0; i < Mathf.Min(lumber, 10); i++)
            Tinted(Piece(parent, pieces, "Lumber", PrimitiveType.Cube, new Vector3(-0.8f + (i % 2) * 0.14f, 0.12f + (i / 2) * 0.06f, z), Vector3.zero, new Vector3(0.12f, 0.05f, 0.95f)), new Color(0.55f, 0.42f, 0.28f));
        for (int i = 0; i < Mathf.Min(stone, 8); i++)
            Tinted(Piece(parent, pieces, "Stone", PrimitiveType.Cube, new Vector3(-0.1f + (i % 3) * 0.2f, 0.16f + (i / 3) * 0.16f, z + ((i % 2) - 0.5f) * 0.2f), new Vector3(0f, i * 23f, 0f), new Vector3(0.18f, 0.15f, 0.2f)), StoneGrey);
        for (int i = 0; i < Mathf.Min(panes, 12); i++)
        {
            GameObject pane = Piece(parent, pieces, "Panes", PrimitiveType.Cube, new Vector3(0.85f + i * 0.02f, 0.4f, z), new Vector3(0f, 0f, -8f), new Vector3(0.012f, 0.6f, 0.9f));
            Tinted(pane, Pane);
        }
    }

    static GameObject Piece(Transform parent, List<GameObject> pieces, string name, PrimitiveType shape, Vector3 position, Vector3 euler, Vector3 scale)
    {
        GameObject piece = GameObject.CreatePrimitive(shape);
        Object.DestroyImmediate(piece.GetComponent<Collider>());
        piece.name = name;
        piece.transform.SetParent(parent, false);
        piece.transform.localPosition = position;
        piece.transform.localRotation = Quaternion.Euler(euler);
        piece.transform.localScale = scale;
        pieces?.Add(piece);
        return piece;
    }

    static GameObject Tinted(GameObject piece, Color color)
    {
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        piece.GetComponent<MeshRenderer>().SetPropertyBlock(block);
        return piece;
    }
}
