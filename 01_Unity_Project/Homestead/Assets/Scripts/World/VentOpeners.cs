using System.Collections.Generic;
using UnityEngine;

// The Vent Opener (Greenhouses.md, slice G6; VentRules): a wax cylinder fitted inside a Cold Frame's back wall or beside a greenhouse's door, that opens the
// lid, the sides or the vents when the inside gets warm and shuts them when it cools. A PlacedStructure of this kind hands its look, prompts and E to this class;
// GardenBeds' game hour calls Hour (before the heat and the beds). It keeps nothing of its own: the cover's own flag says open or shut.
public static class VentOpeners
{
    // How high it is mounted: under a Cold Frame's lid, or at chest height on a greenhouse's front wall.
    public const float HouseMountHeight = 1.5f;
    static float MountHeight(WoodPileState host) => host != null && WoodManager.IsColdFrame(host.kind) ? ColdFrameRules.BackHeight - 0.07f : HouseMountHeight;

    // The cover's kind, for VentRules' words.
    public static string CoverKind(WoodPileState host) =>
        host == null || WoodManager.IsColdFrame(host.kind) ? "ColdFrame" : WoodManager.IsHoopHouse(host.kind) ? "HoopHouse" : "GlassHouse";

    static int OpenFlag(WoodPileState host) =>
        WoodManager.IsColdFrame(host.kind) ? WoodManager.FrameLidOpen : WoodManager.IsHoopHouse(host.kind) ? WoodManager.HoopSidesUp : WoodManager.GlassVentsOpen;

    // --- The hour ---

    public static void Hour(WoodManager wood, float airC)
    {
        foreach (WoodPileState pile in new List<WoodPileState>(wood.Piles))
        {
            if (pile.kind != PileKind.VentOpener)
                continue;
            WoodPileState host = wood.OpenerHost(pile);
            if (host == null || (WoodManager.IsHoopHouse(host.kind) && WoodManager.HoopIsTorn(host)))
                continue; // nothing to work, or no plastic to roll
            bool open = GardenBeds.CoverOpen(host);
            bool after = VentRules.OpenAfter(open, GardenBeds.InsideIf(host, airC, false));
            if (after == open)
                continue;
            if (after)
                host.cropFlags |= OpenFlag(host);
            else
                host.cropFlags &= ~OpenFlag(host);
            host.jobHours = 0f; // the inside is worked out again for the new cover
            wood.NotifyChanged(host);
            wood.NotifyChanged(pile);
        }
    }

    // --- Prompts ---

    public static string Header(WoodPileState s)
    {
        WoodPileState host = WoodManager.Instance != null ? WoodManager.Instance.OpenerHost(s) : null;
        return host == null ? VentRules.Name : VentRules.Header(CoverKind(host), GardenBeds.CoverOpen(host));
    }

    public static string Prompt(WoodPileState s) => VentRules.Prompt;

    public static string SecondaryPrompt(WoodPileState s) =>
        WoodManager.HammerEquipped ? $"Take the {VentRules.Name} off  (half its materials back, rounded down)" : "";

    // E: says what it is doing.
    public static void Interact(WoodPileState s) => ToolStatus.Flash(Header(s), 3f);

    // --- The look ---

    public static void Build(Transform parent, WoodPileState state, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        WoodManager wood = WoodManager.Instance;
        WoodPileState host = wood != null ? wood.OpenerHost(state) : null;
        bool open = host != null && GardenBeds.CoverOpen(host);
        float h = MountHeight(host);
        boxCenter = new Vector3(0f, h, 0.08f);
        boxSize = new Vector3(0.5f, 0.3f, 0.2f);
        GameObject prefab = Resources.Load<GameObject>("Art/structure_ventopener");
        if (prefab != null)
        {
            GameObject art = Object.Instantiate(prefab, parent, false);
            art.name = prefab.name;
            art.transform.localPosition = new Vector3(0f, h, 0f);
            foreach (Collider c in art.GetComponentsInChildren<Collider>())
                Object.DestroyImmediate(c);
            foreach (Transform t in art.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "State_Open") t.gameObject.SetActive(open);
                if (t.name == "State_Shut") t.gameObject.SetActive(!open);
            }
            pieces?.Add(art);
            return;
        }
        Color bracket = new Color(0.2f, 0.2f, 0.22f), brass = new Color(0.72f, 0.58f, 0.3f), steel = new Color(0.7f, 0.71f, 0.73f);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, h, 0.01f), Vector3.zero, new Vector3(0.12f, 0.1f, 0.02f)), bracket);
        Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0.06f, h, 0.07f), new Vector3(0f, 0f, 90f), new Vector3(0.05f, 0.14f, 0.05f)), brass);
        Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(-0.12f, h + (open ? 0.12f : 0.03f), 0.07f), new Vector3(0f, 0f, open ? 55f : 10f), new Vector3(0.3f, 0.015f, 0.02f)), steel);
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
