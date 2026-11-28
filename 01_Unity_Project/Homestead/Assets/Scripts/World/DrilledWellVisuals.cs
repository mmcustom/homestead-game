using System.Collections.Generic;
using UnityEngine;

// What a Drilled Well and its site look like (slice P2; DrillRules). Centred on the ground; the pump's spout faces the front (-Z), where the player stands.
//   The site: four stakes with orange flagging round a marked spot; once the rig comes (the job's buildProgress above 0), a drill rig beside it: a truck
//   bed with its mast raised over the spot, and a pile of drill cuttings growing as it drills.
//   The well: a round concrete pad, the steel casing standing out of it with its cap, and a cast-iron pitcher pump on top with its handle and spout.
// THE SWAP POINT: Resources/Art/structure_drilledwell.prefab, same frame; the collider stays the code's.
public static class DrilledWellVisuals
{
    static readonly Color Stake = new Color(0.55f, 0.42f, 0.27f);
    static readonly Color Flag = new Color(1f, 0.45f, 0.08f);
    static readonly Color Truck = new Color(0.85f, 0.72f, 0.12f);
    static readonly Color Steel = new Color(0.42f, 0.44f, 0.46f);
    static readonly Color Iron = new Color(0.16f, 0.18f, 0.2f);
    static readonly Color Concrete = new Color(0.62f, 0.61f, 0.58f);
    static readonly Color Dirt = new Color(0.36f, 0.27f, 0.18f);

    public const string ArtId = "structure_drilledwell";

    public static void BuildSite(Transform parent, WoodPileState state, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        boxCenter = new Vector3(0f, 0.4f, 0f);
        boxSize = new Vector3(1.4f, 0.8f, 1.4f);
        foreach (Vector2 c in new[] { new Vector2(-0.6f, -0.6f), new Vector2(0.6f, -0.6f), new Vector2(-0.6f, 0.6f), new Vector2(0.6f, 0.6f) })
        {
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(c.x, 0.35f, c.y), Vector3.zero, new Vector3(0.04f, 0.7f, 0.04f)), Stake);
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(c.x + 0.06f, 0.62f, c.y), Vector3.zero, new Vector3(0.12f, 0.06f, 0.005f)), Flag);
        }
        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.01f, 0f), Vector3.zero, new Vector3(0.3f, 0.01f, 0.3f)), Flag);
        float p = Mathf.Clamp01(state.buildProgress);
        if (p <= 0f)
            return;
        // The rig: a truck bed to the right, the mast raised over the hole.
        boxCenter = new Vector3(1.4f, 2f, 0f);
        boxSize = new Vector3(4.2f, 4f, 2.6f);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(2.2f, 0.75f, 0f), Vector3.zero, new Vector3(3.2f, 0.9f, 2.2f)), Truck);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(3.5f, 1.4f, 0f), Vector3.zero, new Vector3(1.2f, 1.4f, 2.2f)), Truck);
        foreach (float z in new[] { -0.8f, 0.8f })
            foreach (float x in new[] { 1.2f, 3.4f })
                Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(x, 0.45f, z * 1.25f), new Vector3(90f, 0f, 0f), new Vector3(0.9f, 0.15f, 0.9f)), Iron);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0.15f, 3.6f, 0f), Vector3.zero, new Vector3(0.3f, 7f, 0.3f)), Steel);
        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.6f + 2.2f * (1f - p), 0f), Vector3.zero, new Vector3(0.12f, 0.6f, 0.12f)), Iron);
        Tinted(Piece(parent, pieces, PrimitiveType.Sphere, new Vector3(-0.7f, 0f, 0.4f), Vector3.zero, new Vector3(0.9f, 0.25f + 0.4f * p, 0.9f) * (0.6f + 0.6f * p)), Dirt);
    }

    public static void Build(Transform parent, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        boxCenter = new Vector3(0f, 0.55f, 0f);
        boxSize = new Vector3(1.1f, 1.1f, 1.1f);
        GameObject prefab = Resources.Load<GameObject>("Art/" + ArtId);
        if (prefab != null)
        {
            GameObject art = Object.Instantiate(prefab, parent, false);
            art.name = prefab.name;
            foreach (Collider c in art.GetComponentsInChildren<Collider>())
                Object.Destroy(c);
            pieces.Add(art);
            return;
        }
        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.05f, 0f), Vector3.zero, new Vector3(1.1f, 0.05f, 1.1f)), Concrete);
        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.35f, 0f), Vector3.zero, new Vector3(0.17f, 0.25f, 0.17f)), Steel);
        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.85f, 0f), Vector3.zero, new Vector3(0.14f, 0.25f, 0.14f)), Iron);
        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.92f, -0.12f), new Vector3(90f, 0f, 0f), new Vector3(0.05f, 0.08f, 0.05f)), Iron);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 1.16f, 0.22f), new Vector3(-20f, 0f, 0f), new Vector3(0.035f, 0.035f, 0.5f)), Iron);
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
