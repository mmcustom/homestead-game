using System.Collections.Generic;
using UnityEngine;

// What a Cold Frame looks like (Greenhouses.md, slice G1; ColdFrameRules). A knee-high box of boards round its bed squares, its long axis along local X, the low front
// wall at local -Z (toward the player who placed it) and the taller back wall at +Z, with two window sashes per square for a lid, hinged along the back. Shut, the
// sashes lie on the sloping walls; propped open, they are swung up OpenDegrees about the back edge. The soil and the crops are the frame's own Garden Beds, drawn
// as beds. The frame's box (what E and R aim at) is its front board only, so the beds behind it can be aimed at too.
// THE SWAP POINT: Resources/Art/structure_coldframesmall.prefab (and _medium, _large): the box and its sashes shut, centred, the front to Unity -Z; each sash under
// its own child named Sash_0, Sash_1 ... whose pivot is its hinge on the back edge and which is swung about its local X to prop it open.
public static class ColdFrameVisuals
{
    public const float OpenDegrees = 60f;
    static readonly Color Board = new Color(0.45f, 0.35f, 0.23f);
    static readonly Color SashWood = new Color(0.78f, 0.74f, 0.64f);

    public static string ArtId(PileKind kind) => "structure_" + kind.ToString().ToLowerInvariant();

    // The shut sash's own tilt: it lies from the back wall's top down to the front wall's.
    static float ShutTilt => -Mathf.Atan2(ColdFrameRules.BackHeight - ColdFrameRules.FrontHeight, ColdFrameRules.Depth) * Mathf.Rad2Deg;

    public static void Build(Transform parent, WoodPileState state, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        ColdFrameRules.Size size = WoodManager.ColdFrameSizeOf(state.kind);
        float length = ColdFrameRules.Squares(size) * ColdFrameRules.SquareSize;
        float halfD = ColdFrameRules.Depth / 2f;
        bool open = WoodManager.FrameLidIsOpen(state);
        boxCenter = new Vector3(0f, ColdFrameRules.FrontHeight / 2f, -halfD);
        boxSize = new Vector3(length + 0.08f, ColdFrameRules.FrontHeight + 0.02f, 0.1f);

        GameObject prefab = Resources.Load<GameObject>("Art/" + ArtId(state.kind));
        if (prefab != null)
        {
            GameObject art = Object.Instantiate(prefab, parent, false);
            art.name = prefab.name;
            art.transform.localPosition = Vector3.zero;
            art.transform.localRotation = Quaternion.identity;
            foreach (Collider c in art.GetComponentsInChildren<Collider>())
                Object.DestroyImmediate(c);
            foreach (Transform t in art.GetComponentsInChildren<Transform>())
                if (t.name.StartsWith("Sash_"))
                    t.localRotation = Quaternion.Euler(open ? OpenDegrees : 0f, 0f, 0f);
            pieces?.Add(art);
            return;
        }

        // The box: the low front, the tall back, and the two sloping sides.
        float midH = (ColdFrameRules.FrontHeight + ColdFrameRules.BackHeight) / 2f;
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, ColdFrameRules.FrontHeight / 2f, -halfD), Vector3.zero, new Vector3(length + 0.08f, ColdFrameRules.FrontHeight, 0.04f)), Board);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, ColdFrameRules.BackHeight / 2f, halfD), Vector3.zero, new Vector3(length + 0.08f, ColdFrameRules.BackHeight, 0.04f)), Board);
        foreach (float x in new[] { -length / 2f, length / 2f })
            Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(x, midH / 2f, 0f), Vector3.zero, new Vector3(0.04f, midH, ColdFrameRules.Depth)), Board);

        // The sashes: two a square, each hung on the back edge.
        int sashes = ColdFrameRules.Sashes(size);
        float width = length / sashes;
        float run = Mathf.Sqrt(ColdFrameRules.Depth * ColdFrameRules.Depth + (ColdFrameRules.BackHeight - ColdFrameRules.FrontHeight) * (ColdFrameRules.BackHeight - ColdFrameRules.FrontHeight));
        Material glass = Resources.Load<Material>("Art/tool_lantern_glass");
        for (int i = 0; i < sashes; i++)
        {
            var hinge = new GameObject("Sash_" + i);
            hinge.transform.SetParent(parent, false);
            hinge.transform.localPosition = new Vector3(-length / 2f + width * (i + 0.5f), ColdFrameRules.BackHeight + 0.02f, halfD);
            hinge.transform.localRotation = Quaternion.Euler(ShutTilt + (open ? OpenDegrees : 0f), 0f, 0f);
            pieces?.Add(hinge);
            Transform h = hinge.transform;
            // The sash's wooden frame (two stiles and two rails) and its pane, running from the hinge toward the front (-Z).
            foreach (float x in new[] { -width / 2f + 0.025f, width / 2f - 0.025f })
                Tinted(Piece(h, null, PrimitiveType.Cube, new Vector3(x, 0f, -run / 2f), Vector3.zero, new Vector3(0.05f, 0.035f, run)), SashWood);
            foreach (float z in new[] { -0.025f, -run + 0.025f })
                Tinted(Piece(h, null, PrimitiveType.Cube, new Vector3(0f, 0f, z), Vector3.zero, new Vector3(width - 0.02f, 0.035f, 0.05f)), SashWood);
            GameObject pane = Piece(h, null, PrimitiveType.Cube, new Vector3(0f, 0f, -run / 2f), Vector3.zero, new Vector3(width - 0.09f, 0.006f, run - 0.09f));
            if (glass != null)
                pane.GetComponent<MeshRenderer>().sharedMaterial = glass;
            else
                Tinted(pane, new Color(0.75f, 0.85f, 0.88f));
        }
    }

    static GameObject Piece(Transform parent, List<GameObject> pieces, PrimitiveType shape, Vector3 position, Vector3 euler, Vector3 scale)
    {
        GameObject piece = GameObject.CreatePrimitive(shape);
        Object.DestroyImmediate(piece.GetComponent<Collider>());
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
