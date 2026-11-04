using UnityEngine;

// Hands and tools M0b (Mike, 2026-11-05): the primitive shape of each tool the hands hold, the default behind the tool swap point (ArtSwap.TryAttachTool). One builder, one head colour per
// tool: stone grey for the stone tools, dark steel for the iron ones and the steel Axe and Saw, wood for handles. Shapes are in metres at the tool's own size (the hands rig scales everything toward
// the eye, so a prefab is authored the same way).
//
// The tool's own space, which a prefab follows too: the origin is where the right hand grips (Anchor_Grip_R), +Z runs along the handle toward the working end, +Y is the poll or back of the head
// and -Y the cutting side (the side that leads when the tool swings forward and down), +X is the tool's right. PlayerHands tilts the whole tool up by holdPitch so it leans away from the hand.
public static class ToolViewPrimitives
{
    public static readonly Color StoneGrey = new Color(0.52f, 0.52f, 0.50f);
    public static readonly Color Steel = new Color(0.13f, 0.14f, 0.17f);
    public static readonly Color Wood = new Color(0.50f, 0.34f, 0.18f);
    public static readonly Color DarkWood = new Color(0.34f, 0.22f, 0.11f);

    // Where each tool is held and worked, in the tool's own space. The same numbers are the fallbacks for a prefab's missing anchors (ArtSwap.ToolSpecs reads them).
    public struct Layout
    {
        public bool twoHanded;
        public float holdPitch;   // degrees the tool leans up from the hand's forward
        public float holdRoll;    // degrees it is turned about its own handle, so a head or blade is seen three-quarter and not edge-on
        public Vector3 gripLeft;  // where the left hand steadies it (two-handed tools)
        public Vector3 tip;       // the working point
    }

    public static Layout LayoutFor(string toolId)
    {
        switch (toolId)
        {
            case "axe": return new Layout { twoHanded = true, holdPitch = -14f, holdRoll = -28f, gripLeft = new Vector3(0f, 0f, 0.20f), tip = new Vector3(0f, -0.14f, 0.56f) };
            case "primitive_axe": return new Layout { twoHanded = true, holdPitch = -14f, holdRoll = -28f, gripLeft = new Vector3(0f, 0f, 0.18f), tip = new Vector3(0f, -0.08f, 0.47f) };
            case "wood_saw": return new Layout { twoHanded = true, holdPitch = -8f, holdRoll = -55f, gripLeft = new Vector3(0f, 0f, 0.30f), tip = new Vector3(0f, -0.07f, 0.62f) };
            case "hammer": return new Layout { twoHanded = false, holdPitch = -24f, gripLeft = new Vector3(0f, 0f, 0.16f), tip = new Vector3(0f, 0f, 0.32f) };
            case "shovel":
            case "iron_shovel": return new Layout { twoHanded = true, holdPitch = -14f, gripLeft = new Vector3(0f, 0f, 0.42f), tip = new Vector3(0f, 0f, 0.92f) };
            case "stone_pick_axe":
            case "iron_pick_axe": return new Layout { twoHanded = true, holdPitch = -14f, holdRoll = -24f, gripLeft = new Vector3(0f, 0f, 0.22f), tip = new Vector3(0f, -0.24f, 0.68f) };
            default: return new Layout { twoHanded = false, holdPitch = -12f, gripLeft = Vector3.zero, tip = Vector3.zero };
        }
    }

    // The head, blade or striking face colour: stone for the stone tools, steel for the iron ones (and the steel Axe and Saw), dark wood for the wooden mallet.
    public static Color HeadColour(string toolId)
    {
        switch (toolId)
        {
            case "primitive_axe":
            case "shovel":
            case "stone_pick_axe": return StoneGrey;
            case "hammer": return DarkWood;
            default: return Steel;
        }
    }

    // Builds the tool's shape as a child of parent, named tool_<id>, on the layer, with no colliders and no shadows. Null for a tool outside the eight (the caller draws the generic placeholder).
    public static GameObject Build(string toolId, Transform parent, int layer)
    {
        if (!ToolWork.IsViewTool(toolId))
            return null;

        var root = new GameObject("tool_" + toolId);
        root.transform.SetParent(parent, false);
        Color head = HeadColour(toolId);
        Layout layout = LayoutFor(toolId);

        switch (toolId)
        {
            case "axe":
                Handle(root.transform, -0.14f, 0.58f, 0.034f);
                Part(root.transform, PrimitiveType.Cube, "Head", new Vector3(0f, -0.04f, 0.56f), new Vector3(0.034f, 0.19f, 0.13f), head);
                Part(root.transform, PrimitiveType.Cube, "Edge", new Vector3(0f, -0.135f, 0.56f), new Vector3(0.012f, 0.03f, 0.135f), Color.Lerp(head, Color.white, 0.5f));
                Part(root.transform, PrimitiveType.Cube, "Poll", new Vector3(0f, 0.06f, 0.56f), new Vector3(0.036f, 0.045f, 0.05f), head);
                break;
            case "primitive_axe":
                Handle(root.transform, -0.12f, 0.48f, 0.034f);
                Part(root.transform, PrimitiveType.Cube, "Head", new Vector3(0f, -0.02f, 0.47f), new Vector3(0.050f, 0.12f, 0.085f), head);
                Part(root.transform, PrimitiveType.Cube, "Binding", new Vector3(0f, 0.04f, 0.47f), new Vector3(0.040f, 0.020f, 0.095f), DarkWood);
                break;
            case "wood_saw":
                Part(root.transform, PrimitiveType.Cube, "Handle", new Vector3(0f, 0f, 0.04f), new Vector3(0.032f, 0.05f, 0.16f), Wood);
                Part(root.transform, PrimitiveType.Cube, "Head", new Vector3(0f, -0.035f, 0.40f), new Vector3(0.005f, 0.075f, 0.50f), head);
                Part(root.transform, PrimitiveType.Cube, "Teeth", new Vector3(0f, -0.078f, 0.40f), new Vector3(0.008f, 0.012f, 0.50f), Color.Lerp(head, Color.white, 0.35f));
                break;
            case "hammer":
                Handle(root.transform, -0.06f, 0.30f, 0.032f);
                Part(root.transform, PrimitiveType.Cube, "Head", new Vector3(0f, 0f, 0.31f), new Vector3(0.16f, 0.075f, 0.085f), head);
                break;
            case "shovel":
            case "iron_shovel":
                Handle(root.transform, -0.16f, 0.72f, 0.030f);
                Part(root.transform, PrimitiveType.Cube, "Grip", new Vector3(0f, 0f, -0.16f), new Vector3(0.10f, 0.03f, 0.03f), Wood);
                Part(root.transform, PrimitiveType.Cube, "Head", new Vector3(0f, 0f, 0.84f), new Vector3(0.20f, 0.012f, 0.28f), head);
                break;
            case "stone_pick_axe":
            case "iron_pick_axe":
                Handle(root.transform, -0.14f, 0.68f, 0.032f);
                Part(root.transform, PrimitiveType.Cube, "Head", new Vector3(0f, 0f, 0.68f), new Vector3(0.050f, 0.34f, 0.060f), head);
                Part(root.transform, PrimitiveType.Cube, "Point", new Vector3(0f, -0.20f, 0.68f), new Vector3(0.022f, 0.08f, 0.026f), Color.Lerp(head, Color.white, 0.45f));
                Part(root.transform, PrimitiveType.Cube, "Point Back", new Vector3(0f, 0.20f, 0.68f), new Vector3(0.022f, 0.08f, 0.026f), head);
                break;
        }

        // The anchors the code attaches to (and a prefab would carry): the right grip at the origin, the left grip on two-handed tools, and the working tip.
        Anchor(root.transform, ArtSwap.AnchorGripR, Vector3.zero);
        if (layout.twoHanded)
            Anchor(root.transform, ArtSwap.AnchorGripL, layout.gripLeft);
        Anchor(root.transform, ArtSwap.AnchorTip, layout.tip);

        ArtSwap.SetViewLayer(root, layer);
        return root;
    }

    // A round handle along +Z from z0 to z1.
    static void Handle(Transform root, float z0, float z1, float diameter)
    {
        float length = z1 - z0;
        Part(root, PrimitiveType.Cylinder, "Handle", new Vector3(0f, 0f, (z0 + z1) * 0.5f), new Vector3(diameter, length * 0.5f, diameter), Wood, new Vector3(90f, 0f, 0f));
    }

    static void Anchor(Transform root, string name, Vector3 position)
    {
        var anchor = new GameObject(name).transform;
        anchor.SetParent(root, false);
        anchor.localPosition = position;
    }

    static GameObject Part(Transform parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Color color, Vector3 euler = default)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        Collider collider = go.GetComponent<Collider>();
        if (collider != null)
            Object.DestroyImmediate(collider);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localRotation = Quaternion.Euler(euler);
        go.transform.localScale = scale;
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        block.SetColor("_Color", color);
        block.SetFloat("_Smoothness", 0.12f); // matt: a flat blade facing the sun would otherwise blaze white
        go.GetComponent<Renderer>().SetPropertyBlock(block);
        return go;
    }
}
