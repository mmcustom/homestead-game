using System.Collections.Generic;
using UnityEngine;

// What a Pole Barn and its site look like (slice B1a; BarnRules). Centred on the ground, long along local Z, the big doorway in the front gable at -Z.
//   The site: corner stakes and string lines, the kit's parts stacked on it, and the work as far as it has got: the posts standing, the trusses up, then
//   the roof and walls going on (the crew's work shows the same way).
//   The barn: red steel walls on wooden posts, a light steel roof, a gravel floor, a sliding door slid open beside the doorway, and shelves along the
//   back (what E aims at, from inside). The walls and roof are solid colliders, with the doorway left open.
// THE SWAP POINT: Resources/Art/structure_polebarnsmall.prefab (and _medium, _large), same frame; the colliders stay the code's.
public static class PoleBarnVisuals
{
    static readonly Color Steel = new Color(0.52f, 0.13f, 0.1f);      // barn-red wall steel
    static readonly Color Trim = new Color(0.9f, 0.89f, 0.85f);       // white trim
    static readonly Color RoofSteel = new Color(0.58f, 0.6f, 0.62f);  // galvalume roof
    static readonly Color Wood = new Color(0.5f, 0.38f, 0.24f);       // treated posts and trusses
    static readonly Color Gravel = new Color(0.47f, 0.45f, 0.42f);

    public static string ArtId(BarnRules.Size size) => "structure_" + WoodManager.BarnOf(size).ToString().ToLowerInvariant();

    // --- The site ---

    public static void BuildSite(Transform parent, WoodPileState state, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        BarnRules.Size size = WoodManager.BarnSizeOf(state.kind);
        float w = BarnRules.Width(size), l = BarnRules.Length(size), eave = BarnRules.EaveHeight(size);
        int stage = Mathf.Clamp(state.jobCount, 0, BarnRules.Stages - 1);
        float p = Mathf.Clamp01(state.buildProgress);
        boxCenter = new Vector3(0f, 0.4f, 0f);
        boxSize = new Vector3(w, 0.8f, l);

        // Corner stakes and the string lines between them.
        foreach (Vector2 c in new[] { new Vector2(-1f, -1f), new Vector2(1f, -1f), new Vector2(-1f, 1f), new Vector2(1f, 1f) })
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(c.x * w / 2f, 0.3f, c.y * l / 2f), Vector3.zero, new Vector3(0.05f, 0.6f, 0.05f)), Wood);
        foreach (float s in new[] { -1f, 1f })
        {
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(s * w / 2f, 0.5f, 0f), Vector3.zero, new Vector3(0.01f, 0.01f, l)), Trim);
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.5f, s * l / 2f), Vector3.zero, new Vector3(w, 0.01f, 0.01f)), Trim);
        }

        // The parts waiting on the site, stacked inside its front corner.
        int posts = state.Count(BarnRules.PostId), trusses = state.Count(BarnRules.TrussId), sheets = state.Count(BarnRules.SheetId);
        if (posts > 0)
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(-w / 4f, 0.1f + 0.03f * Mathf.Min(posts, 16), -l / 2f + 1.2f), Vector3.zero,
                         new Vector3(1.2f, 0.2f + 0.06f * Mathf.Min(posts, 16), 4.2f)), Wood);
        if (trusses > 0 && stage < 2)
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(w / 4f, 0.15f + 0.03f * Mathf.Min(trusses, 16), -l / 2f + 1.8f), Vector3.zero,
                         new Vector3(w * 0.45f, 0.3f + 0.06f * Mathf.Min(trusses, 16), 1.2f)), Wood);
        if (sheets > 0)
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.1f + 0.004f * sheets, 0f), Vector3.zero, new Vector3(1f, 0.2f + 0.008f * sheets, 3.6f)), Steel);

        // The work so far: the posts (stage 0), the trusses (stage 1), the roof and walls (stage 2), and everything before the stage in hand.
        int postsUp = stage >= 1 ? BarnRules.Posts(size) : Mathf.FloorToInt(BarnRules.Posts(size) * p);
        for (int i = 0; i < postsUp; i++)
        {
            int perSide = BarnRules.Posts(size) / 2;
            float x = (i % 2 == 0 ? -1f : 1f) * w / 2f, z = -l / 2f + l * (i / 2) / Mathf.Max(1, perSide - 1);
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(x, eave / 2f, z), Vector3.zero, new Vector3(0.15f, eave, 0.15f)), Wood);
        }
        int trussesUp = stage >= 2 ? BarnRules.Trusses(size) : stage == 1 ? Mathf.FloorToInt(BarnRules.Trusses(size) * p) : 0;
        for (int i = 0; i < trussesUp; i++)
            Truss(parent, pieces, size, -l / 2f + l * i / Mathf.Max(1, BarnRules.Trusses(size) - 1));
        if (stage == 2 && p > 0f)
            Cladding(parent, pieces, size, p);
    }

    // --- The finished barn (WoodPile draws it; it passes its own pieces list) ---

    public static void Build(Transform parent, BarnRules.Size size, List<GameObject> pieces)
    {
        Solids(parent, pieces, size);
        GameObject prefab = Resources.Load<GameObject>("Art/" + ArtId(size));
        if (prefab != null)
        {
            GameObject art = Object.Instantiate(prefab, parent, false);
            art.name = prefab.name;
            foreach (Collider c in art.GetComponentsInChildren<Collider>())
                Object.Destroy(c);
            pieces.Add(art);
            return;
        }
        float w = BarnRules.Width(size), l = BarnRules.Length(size), eave = BarnRules.EaveHeight(size);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.02f, 0f), Vector3.zero, new Vector3(w, 0.04f, l)), Gravel);
        int perSide = BarnRules.Posts(size) / 2;
        for (int i = 0; i < perSide; i++)
            foreach (float s in new[] { -1f, 1f })
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(s * (w / 2f - 0.1f), eave / 2f, -l / 2f + l * i / Mathf.Max(1, perSide - 1)), Vector3.zero,
                             new Vector3(0.15f, eave, 0.15f)), Wood);
        for (int i = 0; i < BarnRules.Trusses(size); i++)
            Truss(parent, pieces, size, -l / 2f + l * i / Mathf.Max(1, BarnRules.Trusses(size) - 1));
        Cladding(parent, pieces, size, 1f);
        // The sliding door, slid open along the front beside the doorway.
        float door = BarnRules.DoorWidth(size), doorH = BarnRules.DoorHeight(size);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(-door, doorH / 2f, -l / 2f - 0.08f), Vector3.zero, new Vector3(door, doorH, 0.05f)), Steel);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(-door / 2f, doorH + 0.08f, -l / 2f - 0.1f), Vector3.zero, new Vector3(door * 2.1f, 0.08f, 0.08f)), Trim);
        // The shelves along the back.
        float shelfW = w * 0.8f, shelfZ = l / 2f - 0.55f;
        foreach (float s in new[] { -1f, 0f, 1f })
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(s * shelfW / 2f, 1.1f, shelfZ), Vector3.zero, new Vector3(0.08f, 2.2f, 0.7f)), Wood);
        foreach (float y in new[] { 0.3f, 1.1f, 1.9f })
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, y, shelfZ), Vector3.zero, new Vector3(shelfW, 0.05f, 0.7f)), Wood);
    }

    // The walls (the doorway open), the back and the roof, as colliders with nothing drawn.
    static void Solids(Transform parent, List<GameObject> pieces, BarnRules.Size size)
    {
        float w = BarnRules.Width(size), l = BarnRules.Length(size), eave = BarnRules.EaveHeight(size), rise = BarnRules.RidgeRise(size);
        float door = BarnRules.DoorWidth(size), doorH = BarnRules.DoorHeight(size);
        foreach (float s in new[] { -1f, 1f })
            Solid(parent, pieces, new Vector3(s * w / 2f, eave / 2f, 0f), new Vector3(0.12f, eave, l));
        Solid(parent, pieces, new Vector3(0f, eave / 2f, l / 2f), new Vector3(w, eave, 0.12f));
        float leaf = (w - door) / 2f;
        foreach (float s in new[] { -1f, 1f })
            Solid(parent, pieces, new Vector3(s * (door / 2f + leaf / 2f), eave / 2f, -l / 2f), new Vector3(leaf, eave, 0.12f));
        Solid(parent, pieces, new Vector3(0f, (doorH + eave) / 2f, -l / 2f), new Vector3(door, eave - doorH, 0.12f));
        Solid(parent, pieces, new Vector3(0f, eave + rise / 2f, 0f), new Vector3(w + 0.6f, rise + 0.1f, l + 0.4f));
    }

    // One truss across the barn at z: a bottom chord at the eaves and two rafters up to the ridge.
    static void Truss(Transform parent, List<GameObject> pieces, BarnRules.Size size, float z)
    {
        float w = BarnRules.Width(size), eave = BarnRules.EaveHeight(size), rise = BarnRules.RidgeRise(size);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, eave, z), Vector3.zero, new Vector3(w, 0.1f, 0.06f)), Wood);
        float rafter = Mathf.Sqrt(w * w / 4f + rise * rise), angle = Mathf.Atan2(rise, w / 2f) * Mathf.Rad2Deg;
        foreach (float s in new[] { -1f, 1f })
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(s * w / 4f, eave + rise / 2f, z), new Vector3(0f, 0f, -s * angle), new Vector3(rafter, 0.1f, 0.06f)), Wood);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, eave + rise / 2f, z), Vector3.zero, new Vector3(0.06f, rise, 0.06f)), Wood);
    }

    // The steel: the walls and the roof, front to back as far as `done` of it (0..1).
    static void Cladding(Transform parent, List<GameObject> pieces, BarnRules.Size size, float done)
    {
        float w = BarnRules.Width(size), l = BarnRules.Length(size), eave = BarnRules.EaveHeight(size), rise = BarnRules.RidgeRise(size);
        float door = BarnRules.DoorWidth(size), doorH = BarnRules.DoorHeight(size);
        float len = l * done, mid = -l / 2f + len / 2f;
        foreach (float s in new[] { -1f, 1f })
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(s * w / 2f, eave / 2f, mid), Vector3.zero, new Vector3(0.04f, eave, len)), Steel);
        float rafter = Mathf.Sqrt(w * w / 4f + rise * rise) + 0.35f, angle = Mathf.Atan2(rise, w / 2f) * Mathf.Rad2Deg;
        foreach (float s in new[] { -1f, 1f })
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(s * w / 4f, eave + rise / 2f + 0.07f, mid), new Vector3(0f, 0f, -s * angle), new Vector3(rafter, 0.04f, len + 0.3f)), RoofSteel);
        // The front: either side of the doorway, over it, and the gable (a squashed diamond whose top half shows above the eaves).
        float leaf = (w - door) / 2f;
        foreach (float s in new[] { -1f, 1f })
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(s * (door / 2f + leaf / 2f), eave / 2f, -l / 2f), Vector3.zero, new Vector3(leaf, eave, 0.04f)), Steel);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, (doorH + eave) / 2f, -l / 2f), Vector3.zero, new Vector3(door, eave - doorH, 0.04f)), Steel);
        Gable(parent, pieces, w, rise, eave, -l / 2f);
        if (done >= 1f)
        {
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, eave / 2f, l / 2f), Vector3.zero, new Vector3(w, eave, 0.04f)), Steel);
            Gable(parent, pieces, w, rise, eave, l / 2f);
            foreach (float s in new[] { -1f, 1f })
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(s * w / 2f, 0.15f, 0f), Vector3.zero, new Vector3(0.06f, 0.3f, l + 0.06f)), Trim);
        }
    }

    static void Gable(Transform parent, List<GameObject> pieces, float w, float rise, float eave, float z)
    {
        var holder = new GameObject("Gable");
        holder.transform.SetParent(parent, false);
        holder.transform.localPosition = new Vector3(0f, eave, z);
        holder.transform.localScale = new Vector3(1f, 2f * rise / w, 1f);
        pieces.Add(holder);
        GameObject diamond = Piece(holder.transform, null, PrimitiveType.Cube, Vector3.zero, new Vector3(0f, 0f, 45f), new Vector3(w / Mathf.Sqrt(2f), w / Mathf.Sqrt(2f), 0.04f));
        Tinted(diamond, Steel);
    }

    static void Solid(Transform parent, List<GameObject> pieces, Vector3 center, Vector3 size)
    {
        var go = new GameObject("Solid");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = center;
        go.AddComponent<BoxCollider>().size = size;
        pieces.Add(go);
    }

    static GameObject Piece(Transform parent, List<GameObject> pieces, PrimitiveType shape, Vector3 position, Vector3 euler, Vector3 scale)
    {
        GameObject piece = GameObject.CreatePrimitive(shape);
        Object.DestroyImmediate(piece.GetComponent<Collider>());
        piece.transform.SetParent(parent, false);
        piece.transform.localPosition = position;
        piece.transform.localEulerAngles = euler;
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
