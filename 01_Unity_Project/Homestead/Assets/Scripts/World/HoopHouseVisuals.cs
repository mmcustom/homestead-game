using System.Collections.Generic;
using UnityEngine;

// What a Hoop House and its site look like (Greenhouses.md, slice G2; HoopHouseRules). A tunnel along local Z, its door in the front end wall at local -Z
// (toward the player who placed the site), steel hoops every 4 ft, plank end walls, and plastic over the top. The beds inside are the house's own Garden Beds,
// drawn as beds.
//   Sides down: the plastic reaches the ground. Sides up: the lower plastic is rolled up to hip height on both sides.
//   Torn or worn out: the plastic is gone but for a few tatters.   Snow: a white layer on the top, thicker as it builds.
// What E aims at is the front wall beside the door (the box); the end walls and the long sides are solid (pieces named Solid, given colliders by PlacedStructure),
// so the way in is the door.
// THE SWAP POINT: Resources/Art/structure_hoophousesmall.prefab (and _medium, _large): centred on the ground, the door end to Unity -Z, with children named
// Sides_Down, Sides_Up, Plastic, Torn and Snow that are shown or hidden by state.
public static class HoopHouseVisuals
{
    static readonly Color Steel = new Color(0.62f, 0.64f, 0.66f);
    static readonly Color Plank = new Color(0.55f, 0.42f, 0.28f);
    static readonly Color Snowy = new Color(0.95f, 0.96f, 0.98f);
    static readonly Color Film = new Color(0.86f, 0.9f, 0.9f);
    const int ArcSegments = 10;
    const float SideTop = 0.35f; // the lower 35 degrees of each side are the roll-up sides

    public static string ArtId(PileKind kind) => "structure_" + kind.ToString().ToLowerInvariant();

    public static void Build(Transform parent, WoodPileState state, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        HoopHouseRules.Size size = WoodManager.HoopSizeOf(state.kind);
        float w = HoopHouseRules.Width(size), l = HoopHouseRules.Length(size), h = HoopHouseRules.Height(size);
        bool up = WoodManager.HoopSidesAreUp(state), torn = WoodManager.HoopIsTorn(state);
        float snow = torn ? 0f : state.soilMoisture;

        // E aims at the front wall beside the door (left of it, seen from outside).
        float side = (w / 2f - HoopHouseRules.DoorWidth / 2f);
        boxCenter = new Vector3(-(HoopHouseRules.DoorWidth / 2f + side / 2f), 0.9f, -l / 2f);
        boxSize = new Vector3(side, 1.8f, 0.12f);
        Solids(parent, pieces, w, l, h, side);

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
                if (t.name == "Sides_Down") t.gameObject.SetActive(!torn && !up);
                else if (t.name == "Sides_Up") t.gameObject.SetActive(!torn && up);
                else if (t.name == "Plastic") t.gameObject.SetActive(!torn);
                else if (t.name == "Torn") t.gameObject.SetActive(torn);
                else if (t.name == "Snow") t.gameObject.SetActive(snow >= HoopHouseRules.SweepMinCm);
            }
            pieces?.Add(art);
            return;
        }

        Material film = Resources.Load<Material>("Art/tool_lantern_glass");
        // The hoops, every 4 ft along the length.
        int hoops = HoopHouseRules.Hoops(size);
        for (int i = 0; i < hoops; i++)
        {
            float z = -l / 2f + l * i / Mathf.Max(1, hoops - 1);
            for (int s = 0; s < ArcSegments; s++)
            {
                Arc(w, h, s, out Vector3 mid, out float angle, out float run);
                Tinted(Piece(parent, pieces, "Hoop", PrimitiveType.Cube, new Vector3(mid.x, mid.y, z), new Vector3(0f, 0f, angle), new Vector3(run + 0.02f, 0.04f, 0.04f)), Steel);
            }
        }
        // Base boards along both sides.
        foreach (float x in new[] { -w / 2f, w / 2f })
            Tinted(Piece(parent, pieces, "Base", PrimitiveType.Cube, new Vector3(x, 0.1f, 0f), Vector3.zero, new Vector3(0.05f, 0.2f, l)), Plank);
        // The end walls: rows of planks following the arch, the door left open in the front one.
        EndWall(parent, pieces, w, h, -l / 2f, true);
        EndWall(parent, pieces, w, h, l / 2f, false);

        if (!torn)
        {
            // The plastic, strip by strip along the arch; the lowest strips each side are the roll-up sides.
            for (int s = 0; s < ArcSegments; s++)
            {
                bool lower = s < ArcSegments * SideTop || s >= ArcSegments * (1f - SideTop);
                if (lower && up)
                    continue;
                Arc(w, h, s, out Vector3 mid, out float angle, out float run);
                GameObject strip = Piece(parent, pieces, "Plastic", PrimitiveType.Cube, new Vector3(mid.x, mid.y + 0.03f, 0f), new Vector3(0f, 0f, angle), new Vector3(run + 0.03f, 0.01f, l + 0.04f));
                if (film != null) strip.GetComponent<MeshRenderer>().sharedMaterial = film;
                else Tinted(strip, Film);
            }
            if (up)
            {
                // The rolled sides, a tube along each side at hip height.
                float a = Mathf.PI * SideTop;
                foreach (float sign in new[] { -1f, 1f })
                {
                    var at = new Vector3(sign * w / 2f * Mathf.Cos(a) * 1.02f, h * Mathf.Sin(a) * 1.02f, 0f);
                    GameObject roll = Piece(parent, pieces, "Roll", PrimitiveType.Cylinder, at, new Vector3(90f, 0f, 0f), new Vector3(0.12f, l / 2f, 0.12f));
                    Tinted(roll, Film);
                }
            }
            if (snow >= HoopHouseRules.SweepMinCm)
            {
                float depth = Mathf.Clamp(snow / 100f, 0.02f, 0.3f);
                for (int s = 3; s < ArcSegments - 3; s++)
                {
                    Arc(w, h, s, out Vector3 mid, out float angle, out float run);
                    Tinted(Piece(parent, pieces, "Snow", PrimitiveType.Cube, new Vector3(mid.x, mid.y + 0.04f + depth / 2f, 0f), new Vector3(0f, 0f, angle), new Vector3(run + 0.05f, depth, l)), Snowy);
                }
            }
        }
        else
        {
            // A few tatters hanging from the top.
            for (int i = 0; i < 4; i++)
            {
                float x = (i - 1.5f) * w / 5f, z = (i % 2 == 0 ? -1f : 1f) * l / 5f;
                Tinted(Piece(parent, pieces, "Torn", PrimitiveType.Cube, new Vector3(x, h * 0.8f, z), new Vector3(0f, 0f, 8f * (i - 1.5f)), new Vector3(0.3f, 0.6f, 0.01f)), Film);
            }
        }
    }

    // Segment s of the arch (an ellipse W wide and H high): its middle, its tilt (degrees about Z) and its length.
    static void Arc(float w, float h, int s, out Vector3 mid, out float angle, out float run)
    {
        float a0 = Mathf.PI * s / ArcSegments, a1 = Mathf.PI * (s + 1) / ArcSegments;
        var p0 = new Vector3(w / 2f * Mathf.Cos(a0), h * Mathf.Sin(a0), 0f);
        var p1 = new Vector3(w / 2f * Mathf.Cos(a1), h * Mathf.Sin(a1), 0f);
        mid = (p0 + p1) / 2f;
        Vector3 d = p1 - p0;
        angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        run = d.magnitude;
    }

    // The width of the arch at a height.
    static float HalfWidthAt(float w, float h, float y) => w / 2f * Mathf.Sqrt(Mathf.Max(0f, 1f - (y / h) * (y / h)));

    static void EndWall(Transform parent, List<GameObject> pieces, float w, float h, float z, bool door)
    {
        const float row = 0.25f;
        for (float y = row / 2f; y < h - 0.05f; y += row)
        {
            float half = HalfWidthAt(w, h, y + row / 2f);
            if (half < 0.1f)
                break;
            if (door && y < HoopHouseRules.DoorHeight)
            {
                float gap = HoopHouseRules.DoorWidth / 2f;
                if (half > gap + 0.05f)
                    foreach (float sign in new[] { -1f, 1f })
                        Tinted(Piece(parent, pieces, "Wall", PrimitiveType.Cube, new Vector3(sign * (gap + half) / 2f, y, z), Vector3.zero, new Vector3(half - gap, row - 0.02f, 0.03f)), Plank);
                continue;
            }
            Tinted(Piece(parent, pieces, "Wall", PrimitiveType.Cube, new Vector3(0f, y, z), Vector3.zero, new Vector3(half * 2f, row - 0.02f, 0.03f)), Plank);
        }
    }

    // The walls you bump into: the front either side of the door and above it, the back, and both long sides. Invisible boxes (PlacedStructure gives
    // every piece named Solid a collider).
    static void Solids(Transform parent, List<GameObject> pieces, float w, float l, float h, float side)
    {
        float gap = HoopHouseRules.DoorWidth / 2f;
        Solid(parent, pieces, new Vector3(gap + side / 2f, h / 2f, -l / 2f), new Vector3(side, h, 0.12f));
        Solid(parent, pieces, new Vector3(-(gap + side / 2f), h / 2f, -l / 2f), new Vector3(side, h, 0.12f));
        Solid(parent, pieces, new Vector3(0f, (HoopHouseRules.DoorHeight + h) / 2f, -l / 2f), new Vector3(gap * 2f, h - HoopHouseRules.DoorHeight, 0.12f));
        Solid(parent, pieces, new Vector3(0f, h / 2f, l / 2f), new Vector3(w, h, 0.12f));
        foreach (float sign in new[] { -1f, 1f })
            Solid(parent, pieces, new Vector3(sign * (w / 2f - 0.05f), h * 0.4f, 0f), new Vector3(0.1f, h * 0.8f, l));
        Solid(parent, pieces, new Vector3(0f, h + 0.05f, 0f), new Vector3(w * 0.6f, 0.1f, l)); // the roof, so it counts as overhead cover
    }

    static void Solid(Transform parent, List<GameObject> pieces, Vector3 at, Vector3 scale)
    {
        GameObject piece = Piece(parent, pieces, "Solid", PrimitiveType.Cube, at, Vector3.zero, scale);
        piece.GetComponent<MeshRenderer>().enabled = false;
    }

    // --- The site: corner stakes and string lines round the footprint, and the delivered materials stacked just outside the door end. ---

    public static void BuildSite(Transform parent, WoodPileState state, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        HoopHouseRules.Size size = WoodManager.HoopSizeOf(state.kind);
        float w = HoopHouseRules.Width(size), l = HoopHouseRules.Length(size);
        float hx = w / 2f, hz = l / 2f;
        foreach (var c in new[] { new Vector2(-hx, -hz), new Vector2(hx, -hz), new Vector2(hx, hz), new Vector2(-hx, hz) })
            Tinted(Piece(parent, pieces, "Stake", PrimitiveType.Cylinder, new Vector3(c.x, 0.3f, c.y), Vector3.zero, new Vector3(0.05f, 0.3f, 0.05f)), Plank);
        Color twine = new Color(0.85f, 0.8f, 0.6f);
        Tinted(Piece(parent, pieces, "Line", PrimitiveType.Cube, new Vector3(0f, 0.5f, -hz), Vector3.zero, new Vector3(w, 0.01f, 0.01f)), twine);
        Tinted(Piece(parent, pieces, "Line", PrimitiveType.Cube, new Vector3(0f, 0.5f, hz), Vector3.zero, new Vector3(w, 0.01f, 0.01f)), twine);
        Tinted(Piece(parent, pieces, "Line", PrimitiveType.Cube, new Vector3(-hx, 0.5f, 0f), Vector3.zero, new Vector3(0.01f, 0.01f, l)), twine);
        Tinted(Piece(parent, pieces, "Line", PrimitiveType.Cube, new Vector3(hx, 0.5f, 0f), Vector3.zero, new Vector3(0.01f, 0.01f, l)), twine);

        // The materials, outside the door end: the box E and R and the Hammer aim at.
        float z = -hz - 0.8f;
        boxCenter = new Vector3(0f, 0.35f, z);
        boxSize = new Vector3(2.4f, 0.7f, 1.0f);
        Tinted(Piece(parent, pieces, "Pallet", PrimitiveType.Cube, new Vector3(0f, 0.04f, z), Vector3.zero, new Vector3(2.4f, 0.08f, 1.0f)), Plank);
        int lumber = state.Count(HoopHouseRules.LumberId), hoops = state.Count(HoopHouseRules.HoopId), sheets = state.Count(HoopHouseRules.PlasticId);
        for (int i = 0; i < Mathf.Min(lumber, 10); i++)
            Tinted(Piece(parent, pieces, "Lumber", PrimitiveType.Cube, new Vector3(-0.7f + (i % 2) * 0.14f, 0.12f + (i / 2) * 0.06f, z), Vector3.zero, new Vector3(0.12f, 0.05f, 0.95f)), Plank);
        for (int i = 0; i < Mathf.Min(hoops, 8); i++)
            Tinted(Piece(parent, pieces, "Hoops", PrimitiveType.Cylinder, new Vector3(0.1f, 0.1f + i * 0.03f, z), Vector3.zero, new Vector3(0.6f, 0.012f, 0.6f)), Steel);
        for (int i = 0; i < Mathf.Min(sheets, 4); i++)
            Tinted(Piece(parent, pieces, "Rolls", PrimitiveType.Cylinder, new Vector3(0.85f, 0.16f + i * 0.17f, z), new Vector3(90f, 0f, 0f), new Vector3(0.16f, 0.45f, 0.16f)), Film);
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
