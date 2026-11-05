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
    public static readonly Color Cork = new Color(0.62f, 0.46f, 0.28f);
    public static readonly Color RodBlank = new Color(0.16f, 0.14f, 0.12f);
    public static readonly Color PaleWood = new Color(0.66f, 0.54f, 0.30f);
    public static readonly Color Cloth = new Color(0.80f, 0.76f, 0.66f);
    public static readonly Color CharredCloth = new Color(0.30f, 0.22f, 0.16f);
    public static readonly Color Terracotta = new Color(0.65f, 0.34f, 0.20f);
    public static readonly Color Leather = new Color(0.40f, 0.27f, 0.16f);
    public static readonly Color TarpGreen = new Color(0.30f, 0.40f, 0.28f);
    public static readonly Color IronGrey = new Color(0.36f, 0.37f, 0.40f); // the pots and the kettle: iron, lit enough to read against the grass
    public static readonly Color Rope = new Color(0.72f, 0.62f, 0.42f);

    // The bows' limb tips, in the bow's own space (the grip at the origin, +Y up the upper limb, -Z toward the archer, where the string is): the upper tip. The lower tip mirrors it in Y. The string runs
    // tip to tip, so Anchor_Nock (its middle at rest) is at the tips' Z, behind the grip by the brace height.
    // Round 4: the bow's limb tips, in the bow's own space (the grip at the origin, +Y up the upper limb, -Z toward the archer, where the string is): the upper tip. The lower tip mirrors it in Y. The two
    // tips sit only a few centimetres behind the grip (the brace), so that with the bow seen from its side the tips are directly above and below the grip. The limbs bulge forward by BowBulge in the
    // middle (a gentle, symmetric bend) and come back to the tips; the Recurve's last stretch also flicks forward by BowFlick, so ITS real tip is BowBaseZ + BowFlick. The string runs cap to cap and
    // Anchor_Nock is on it, BowNockY below the grip, where the right hand rests.
    public const float BowFlick = 0.06f, BowNockY = -0.07f;
    public const float BowLimbWidth = 0.036f, BowLimbDepth = 0.012f; // a limb's cross-section: wide across the view, thin in depth
    public static float BowBaseZ(bool recurve) => recurve ? -0.095f : -0.03f;
    public static float BowBulge(bool recurve) => recurve ? 0.05f : 0.06f;
    public static Vector3 BowTip(bool recurve) => new Vector3(0f, recurve ? 0.38f : 0.37f, BowBaseZ(recurve) + (recurve ? BowFlick : 0f));

    // The bows' colours: pale wood for the Primitive Bow, dark for the Recurve, a leather-wrapped grip on both, and a pale tip cap so the ends read against the grass.
    public static readonly Color BowPale = new Color(0.95f, 0.80f, 0.50f);
    public static readonly Color BowDark = new Color(0.74f, 0.36f, 0.19f); // a medium reddish brown: clearly not the Primitive Bow's pale wood, and it reads on grass

    public const string StringUpper = "String Upper", StringLower = "String Lower"; // the primitive bow's string halves

    // Where each tool is held and worked, in the tool's own space. The same numbers are the fallbacks for a prefab's missing anchors (ArtSwap.ToolSpecs reads them).
    public struct Layout
    {
        public bool twoHanded;
        public float holdPitch;   // degrees the tool leans up from the hand's forward
        public float holdRoll;    // degrees it is turned about its own handle, so a head or blade is seen three-quarter and not edge-on
        public Vector3 gripLeft;  // where the left hand steadies it (two-handed tools)
        public Vector3 tip;       // the working point
        public bool hasLine; public Vector3 line;    // Anchor_Line: where the fishing line starts (the rods)
        public bool hasFlame; public Vector3 flame;  // Anchor_Flame: where the flame and its light sit (the Torch and Lantern)
        public bool hasNock; public Vector3 nock;    // Anchor_Nock: where an arrow is nocked on the string (the bows)
        public bool hasMuzzle; public Vector3 muzzle; // Anchor_Muzzle: the end of the barrel (the rifle)
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
            case "fishing_rod": return new Layout { twoHanded = true, holdPitch = -34f, gripLeft = new Vector3(0f, 0f, 0.30f), tip = new Vector3(0f, 0f, 1.45f), hasLine = true, line = new Vector3(0f, 0f, 1.47f) };
            case "cane_pole": return new Layout { twoHanded = true, holdPitch = -34f, gripLeft = new Vector3(0f, 0f, 0.32f), tip = new Vector3(0f, 0f, 1.55f), hasLine = true, line = new Vector3(0f, 0f, 1.56f) };
            case "torch": return new Layout { twoHanded = false, holdPitch = -22f, tip = new Vector3(0f, 0f, 0.52f), hasFlame = true, flame = new Vector3(0f, 0f, 0.58f) };
            case "lantern": return new Layout { twoHanded = false, holdPitch = 0f, tip = new Vector3(0f, -0.17f, 0f), hasFlame = true, flame = new Vector3(0f, -0.17f, 0f) };
            // M0c-2: the rest of the tools. Bows stand upright (limbs along Y, the string toward you) and are held by the LEFT hand at Anchor_Grip_L, the handle; the right hand goes to Anchor_Nock on the string.
            // The rifle runs along Z. Everything that hangs (a bucket, a pot, a trap, the pouch) has holdPitch 0: it hangs straight down from the grip.
            case "primitive_bow": return new Layout { twoHanded = true, holdPitch = 0f, gripLeft = Vector3.zero, tip = BowTip(false), hasNock = true, nock = new Vector3(0f, BowNockY, BowTip(false).z) };
            case "recurve_bow": return new Layout { twoHanded = true, holdPitch = 0f, gripLeft = Vector3.zero, tip = BowTip(true), hasNock = true, nock = new Vector3(0f, BowNockY, BowTip(true).z) };
            case "bolt_action_rifle": return new Layout { twoHanded = true, holdPitch = -20f, gripLeft = new Vector3(0f, -0.02f, 0.40f), tip = new Vector3(0f, 0.01f, 0.78f), hasMuzzle = true, muzzle = new Vector3(0f, 0.01f, 0.79f) };
            case "bow_drill": return new Layout { twoHanded = false, holdPitch = -14f, tip = new Vector3(0f, 0f, 0.32f) };
            case "bucket": return new Layout { twoHanded = false, holdPitch = 0f, tip = new Vector3(0f, -0.22f, 0f) };
            case "sap_bucket": return new Layout { twoHanded = false, holdPitch = 0f, tip = new Vector3(0f, -0.16f, 0f) };
            case "canteen": return new Layout { twoHanded = false, holdPitch = 0f, tip = new Vector3(0f, -0.10f, 0f) };
            case "spile": return new Layout { twoHanded = false, holdPitch = -20f, tip = new Vector3(0f, 0f, 0.16f) };
            case "box_trap": return new Layout { twoHanded = false, holdPitch = 0f, tip = new Vector3(0f, -0.14f, 0f) };
            case "fish_trap": return new Layout { twoHanded = false, holdPitch = 0f, tip = new Vector3(0f, -0.20f, 0f) };
            case "rabbit_snare": return new Layout { twoHanded = false, holdPitch = -14f, tip = new Vector3(0f, 0f, 0.32f) };
            case "knife": return new Layout { twoHanded = false, holdPitch = -22f, tip = new Vector3(0f, 0f, 0.24f) };
            case "flint_and_steel": return new Layout { twoHanded = false, holdPitch = -10f, tip = new Vector3(0f, 0f, 0.07f) };
            case "pouch": return new Layout { twoHanded = false, holdPitch = 0f, tip = new Vector3(0f, -0.12f, 0f) };
            case "clay_pot": return new Layout { twoHanded = false, holdPitch = -6f, tip = new Vector3(0f, 0.04f, 0.10f) };
            case "cooking_pot": return new Layout { twoHanded = false, holdPitch = 0f, tip = new Vector3(0f, -0.14f, 0f) };
            case "potash_kettle": return new Layout { twoHanded = false, holdPitch = 0f, tip = new Vector3(0f, -0.16f, 0f) };
            case "tarp": return new Layout { twoHanded = false, holdPitch = -8f, tip = new Vector3(0f, 0f, 0.14f) };
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

    // Builds the tool's shape as a child of parent, named tool_<id>, on the layer, with no colliders and no shadows. Null for a tool outside the twelve (the caller draws the generic placeholder).
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
            case "fishing_rod":
                // A cork grip, a reel on the side, and a blank in three thinning sections with line guides along it.
                Part(root.transform, PrimitiveType.Cylinder, "Handle", new Vector3(0f, 0f, 0.03f), new Vector3(0.034f, 0.15f, 0.034f), Cork, new Vector3(90f, 0f, 0f));
                Part(root.transform, PrimitiveType.Cylinder, "Reel", new Vector3(0.035f, -0.03f, 0.10f), new Vector3(0.050f, 0.012f, 0.050f), Steel, new Vector3(0f, 0f, 90f));
                Part(root.transform, PrimitiveType.Cylinder, "Blank 1", new Vector3(0f, 0f, 0.47f), new Vector3(0.016f, 0.29f, 0.016f), RodBlank, new Vector3(90f, 0f, 0f));
                Part(root.transform, PrimitiveType.Cylinder, "Blank 2", new Vector3(0f, 0f, 0.95f), new Vector3(0.011f, 0.20f, 0.011f), RodBlank, new Vector3(90f, 0f, 0f));
                Part(root.transform, PrimitiveType.Cylinder, "Blank 3", new Vector3(0f, 0f, 1.30f), new Vector3(0.007f, 0.16f, 0.007f), RodBlank, new Vector3(90f, 0f, 0f));
                foreach (float z in new[] { 0.40f, 0.72f, 1.02f, 1.30f, 1.45f })
                    Part(root.transform, PrimitiveType.Cube, "Guide", new Vector3(0f, -0.014f, z), new Vector3(0.012f, 0.012f, 0.012f), Steel);
                break;
            case "cane_pole":
                // A plain pole, thinner toward the tip, with a short line hanging from it.
                Part(root.transform, PrimitiveType.Cylinder, "Handle", new Vector3(0f, 0f, 0.68f), new Vector3(0.024f, 0.84f, 0.024f), PaleWood, new Vector3(90f, 0f, 0f));
                Part(root.transform, PrimitiveType.Cylinder, "Blank", new Vector3(0f, 0f, 1.40f), new Vector3(0.014f, 0.32f, 0.014f), PaleWood, new Vector3(90f, 0f, 0f));
                Part(root.transform, PrimitiveType.Cylinder, "Line", new Vector3(0f, -0.15f, 1.56f), new Vector3(0.003f, 0.15f, 0.003f), Cloth);
                break;
            case "torch":
                // A stick with a wrapped head; the flame (State_Lit) sits at its top.
                Handle(root.transform, -0.10f, 0.46f, 0.036f);
                Part(root.transform, PrimitiveType.Cylinder, "Head", new Vector3(0f, 0f, 0.50f), new Vector3(0.070f, 0.06f, 0.070f), CharredCloth, new Vector3(90f, 0f, 0f));
                Part(root.transform, PrimitiveType.Cylinder, "Binding", new Vector3(0f, 0f, 0.45f), new Vector3(0.048f, 0.012f, 0.048f), DarkWood, new Vector3(90f, 0f, 0f));
                break;
            case "lantern":
                // A bar to hold it by, two wires down to a small iron body, with the flame (State_Lit) glowing inside.
                Part(root.transform, PrimitiveType.Cube, "Handle", Vector3.zero, new Vector3(0.11f, 0.014f, 0.014f), Steel);
                Part(root.transform, PrimitiveType.Cube, "Wire R", new Vector3(0.045f, -0.05f, 0f), new Vector3(0.008f, 0.10f, 0.008f), Steel);
                Part(root.transform, PrimitiveType.Cube, "Wire L", new Vector3(-0.045f, -0.05f, 0f), new Vector3(0.008f, 0.10f, 0.008f), Steel);
                Part(root.transform, PrimitiveType.Cube, "Cap", new Vector3(0f, -0.105f, 0f), new Vector3(0.10f, 0.016f, 0.10f), Steel);
                Part(root.transform, PrimitiveType.Cylinder, "Body", new Vector3(0f, -0.17f, 0f), new Vector3(0.085f, 0.05f, 0.085f), Color.Lerp(Steel, Color.white, 0.12f));
                Part(root.transform, PrimitiveType.Cube, "Base", new Vector3(0f, -0.235f, 0f), new Vector3(0.10f, 0.016f, 0.10f), Steel);
                break;

            // ---- M0c-2: the rest of the tools. Every one has a Handle or a Body so it reads as something; colours follow the material (wood, iron, leather, cloth, clay). ----
            case "primitive_bow":
            case "recurve_bow":
            {
                // Round 4: a bow that STANDS STRAIGHT UP AND DOWN. Both limbs are mirror images about the grip: the same curve above and below, and the two tips (and so the string between them) sit the same
                // few centimetres behind the grip (BowTip.z, the brace), so seen side-on the tip-to-tip line is vertical and directly above and below the grip. The limbs bulge gently FORWARD in the middle
                // (BowBulge) and come back to the tips, so they read as a bow and not a stick. A leather-wrapped grip at the origin; each limb is short tapering segments chained from the grip to a tip cap.
                // The string (two halves, PlayerHands.UpdateString) runs cap to cap and is drawn to the right hand, which rests on it just below the grip (Anchor_Nock).
                bool recurve = toolId == "recurve_bow";
                Color limb = recurve ? BowDark : BowPale;
                Vector3 tipPoint = BowTip(recurve);
                // Round 5: full size again (limbs 0.38 / 0.39 each side of the grip), seen nearly straight-on from the archer's place, so the limbs are FLAT AND WIDE (about 3.5 cm across the view, 1.2 cm
                // thin in depth) as real bow limbs are: they read as a bow and not a thin dark stick even though the bend is in depth. A clear leather grip with a lighter wrap, and a square tip cap.
                Part(root.transform, PrimitiveType.Cylinder, "Handle", Vector3.zero, new Vector3(0.044f, 0.075f, 0.044f), Leather);
                Part(root.transform, PrimitiveType.Cylinder, "Grip Wrap", new Vector3(0f, 0.012f, 0f), new Vector3(0.050f, 0.040f, 0.050f), Rope);
                foreach (float sign in new[] { 1f, -1f })
                {
                    const int segments = 6;
                    Vector3 previous = new Vector3(0f, sign * 0.07f, 0f);
                    for (int i = 1; i <= segments; i++)
                    {
                        float u = i / (float)segments;
                        float y = Mathf.Lerp(0.07f, tipPoint.y, u);
                        float z = BowBaseZ(recurve) * u + BowBulge(recurve) * Mathf.Sin(Mathf.PI * u);
                        if (recurve && u > 0.72f)
                            z += BowFlick * Mathf.SmoothStep(0f, 1f, (u - 0.72f) / 0.28f); // the tips flick forward (by BowFlick at the very end)
                        Vector3 point = new Vector3(0f, sign * y, z);
                        Vector3 along = point - previous;
                        float width = Mathf.Lerp(BowLimbWidth, BowLimbWidth * 0.55f, u);
                        GameObject piece = Part(root.transform, PrimitiveType.Cube, "Limb", (previous + point) * 0.5f, new Vector3(width, along.magnitude + 0.006f, BowLimbDepth), limb);
                        piece.transform.localRotation = Quaternion.FromToRotation(Vector3.up, along.normalized);
                        previous = point;
                    }
                    Part(root.transform, PrimitiveType.Cube, "Limb Tip", previous, new Vector3(BowLimbWidth * 0.8f, 0.02f, BowLimbDepth * 1.6f), recurve ? BowDark : BowPale);
                }
                // the string, two halves from the tip caps to the nock (at rest a straight line from cap to cap, the nock on it just below the grip)
                Vector3 nockPoint = new Vector3(0f, BowNockY, tipPoint.z);
                foreach (float sign in new[] { 1f, -1f })
                {
                    Vector3 end = new Vector3(0f, sign * tipPoint.y, tipPoint.z), along = nockPoint - end;
                    GameObject half = Part(root.transform, PrimitiveType.Cylinder, sign > 0f ? StringUpper : StringLower, (end + nockPoint) * 0.5f, new Vector3(0.008f, along.magnitude * 0.5f, 0.008f), Cloth);
                    half.transform.localRotation = Quaternion.FromToRotation(Vector3.up, along.normalized);
                }
                break;
            }
            case "bolt_action_rifle":
                Part(root.transform, PrimitiveType.Cube, "Stock", new Vector3(0f, -0.02f, -0.12f), new Vector3(0.045f, 0.09f, 0.34f), Wood);
                Part(root.transform, PrimitiveType.Cube, "Fore-end", new Vector3(0f, -0.015f, 0.30f), new Vector3(0.04f, 0.05f, 0.34f), Wood);
                Part(root.transform, PrimitiveType.Cylinder, "Barrel", new Vector3(0f, 0.012f, 0.50f), new Vector3(0.022f, 0.30f, 0.022f), Steel, new Vector3(90f, 0f, 0f));
                Part(root.transform, PrimitiveType.Cube, "Receiver", new Vector3(0f, 0.02f, 0.04f), new Vector3(0.04f, 0.05f, 0.14f), Steel);
                Part(root.transform, PrimitiveType.Cylinder, "Bolt", new Vector3(0.03f, 0.03f, 0.06f), new Vector3(0.012f, 0.02f, 0.012f), Steel, new Vector3(0f, 0f, 90f));
                break;
            case "bow_drill":
                // A spindle with a hearth board at its foot, and the bow (a bent stick and its cord) crossing it.
                Part(root.transform, PrimitiveType.Cylinder, "Spindle", new Vector3(0f, 0f, 0.16f), new Vector3(0.022f, 0.16f, 0.022f), Wood, new Vector3(90f, 0f, 0f));
                Part(root.transform, PrimitiveType.Cube, "Hearth Board", new Vector3(0f, -0.012f, 0.33f), new Vector3(0.12f, 0.02f, 0.07f), DarkWood);
                Part(root.transform, PrimitiveType.Cylinder, "Bow", new Vector3(0f, 0f, 0.12f), new Vector3(0.014f, 0.24f, 0.014f), Wood, new Vector3(0f, 0f, 90f));
                Part(root.transform, PrimitiveType.Cylinder, "Cord", new Vector3(0f, 0f, 0.10f), new Vector3(0.004f, 0.24f, 0.004f), Cloth, new Vector3(0f, 0f, 90f));
                break;
            case "bucket":
                BuildVessel(root.transform, 0.085f, 0.075f, 0.17f, Wood, true);
                break;
            case "sap_bucket":
                BuildVessel(root.transform, 0.065f, 0.055f, 0.13f, Wood, true);
                Part(root.transform, PrimitiveType.Cylinder, "Lid", new Vector3(0f, -0.075f, 0f), new Vector3(0.13f, 0.008f, 0.13f), DarkWood);
                break;
            case "canteen":
                Part(root.transform, PrimitiveType.Cylinder, "Body", new Vector3(0f, -0.10f, 0f), new Vector3(0.14f, 0.026f, 0.14f), Leather, new Vector3(90f, 0f, 0f));
                Part(root.transform, PrimitiveType.Cylinder, "Neck", new Vector3(0f, -0.03f, 0f), new Vector3(0.03f, 0.03f, 0.03f), Wood);
                Part(root.transform, PrimitiveType.Cube, "Strap", new Vector3(0f, -0.005f, 0f), new Vector3(0.10f, 0.012f, 0.012f), Rope);
                break;
            case "spile":
                Part(root.transform, PrimitiveType.Cylinder, "Body", new Vector3(0f, 0f, 0.06f), new Vector3(0.028f, 0.09f, 0.028f), Wood, new Vector3(90f, 0f, 0f));
                Part(root.transform, PrimitiveType.Cylinder, "Spout", new Vector3(0f, -0.02f, 0.015f), new Vector3(0.012f, 0.03f, 0.012f), DarkWood, new Vector3(0f, 0f, 0f));
                Part(root.transform, PrimitiveType.Cylinder, "Point", new Vector3(0f, 0f, 0.15f), new Vector3(0.016f, 0.02f, 0.016f), Wood, new Vector3(90f, 0f, 0f));
                break;
            case "box_trap":
                Part(root.transform, PrimitiveType.Cube, "Body", new Vector3(0f, -0.075f, 0f), new Vector3(0.20f, 0.10f, 0.15f), Wood);
                Part(root.transform, PrimitiveType.Cube, "Slats", new Vector3(0f, -0.075f, 0.077f), new Vector3(0.16f, 0.08f, 0.006f), DarkWood);
                Part(root.transform, PrimitiveType.Cube, "Handle", new Vector3(0f, -0.01f, 0f), new Vector3(0.09f, 0.012f, 0.012f), Rope);
                break;
            case "fish_trap":
                // A wicker cylinder with a cone at its mouth, carried hanging by a cord.
                Part(root.transform, PrimitiveType.Cylinder, "Body", new Vector3(0f, -0.115f, 0f), new Vector3(0.10f, 0.085f, 0.10f), PaleWood);
                Part(root.transform, PrimitiveType.Cylinder, "Mouth", new Vector3(0f, -0.20f, 0f), new Vector3(0.06f, 0.012f, 0.06f), DarkWood);
                Part(root.transform, PrimitiveType.Cylinder, "Cord", new Vector3(0f, -0.02f, 0f), new Vector3(0.006f, 0.02f, 0.006f), Rope);
                break;
            case "rabbit_snare":
                // A stake with a noose of cord at the end of a short line.
                Part(root.transform, PrimitiveType.Cylinder, "Stake", new Vector3(0f, 0f, 0.10f), new Vector3(0.026f, 0.10f, 0.026f), Wood, new Vector3(90f, 0f, 0f));
                Part(root.transform, PrimitiveType.Cylinder, "Line", new Vector3(0f, -0.02f, 0.23f), new Vector3(0.004f, 0.07f, 0.004f), Rope, new Vector3(90f, 0f, 0f));
                foreach (Vector3 side in new[] { new Vector3(0.05f, 0f, 0.32f), new Vector3(-0.05f, 0f, 0.32f) })
                    Part(root.transform, PrimitiveType.Cylinder, "Noose", side, new Vector3(0.004f, 0.05f, 0.004f), Rope);
                Part(root.transform, PrimitiveType.Cylinder, "Noose Top", new Vector3(0f, 0.05f, 0.32f), new Vector3(0.004f, 0.05f, 0.004f), Rope, new Vector3(0f, 0f, 90f));
                break;
            case "knife":
                Part(root.transform, PrimitiveType.Cylinder, "Handle", new Vector3(0f, 0f, -0.01f), new Vector3(0.03f, 0.06f, 0.03f), Wood, new Vector3(90f, 0f, 0f));
                Part(root.transform, PrimitiveType.Cube, "Lashing", new Vector3(0f, 0f, 0.055f), new Vector3(0.034f, 0.034f, 0.025f), Rope);
                Part(root.transform, PrimitiveType.Cube, "Blade", new Vector3(0f, 0f, 0.155f), new Vector3(0.012f, 0.04f, 0.17f), StoneGrey);
                Part(root.transform, PrimitiveType.Cube, "Edge", new Vector3(0f, -0.02f, 0.155f), new Vector3(0.006f, 0.008f, 0.17f), Color.Lerp(StoneGrey, Color.white, 0.5f));
                break;
            case "flint_and_steel":
                Part(root.transform, PrimitiveType.Cube, "Flint", new Vector3(0f, 0f, 0.04f), new Vector3(0.045f, 0.03f, 0.05f), StoneGrey);
                Part(root.transform, PrimitiveType.Cube, "Steel Bar", new Vector3(0f, -0.03f, 0.06f), new Vector3(0.095f, 0.014f, 0.016f), Steel);
                Part(root.transform, PrimitiveType.Cube, "Steel Loop", new Vector3(0.05f, -0.015f, 0.06f), new Vector3(0.012f, 0.04f, 0.016f), Steel);
                break;
            case "pouch":
                Part(root.transform, PrimitiveType.Sphere, "Body", new Vector3(0f, -0.09f, 0f), new Vector3(0.12f, 0.14f, 0.10f), Leather);
                Part(root.transform, PrimitiveType.Cylinder, "Tie", new Vector3(0f, -0.02f, 0f), new Vector3(0.05f, 0.012f, 0.05f), Rope);
                break;
            case "clay_pot":
                Part(root.transform, PrimitiveType.Cylinder, "Body", new Vector3(0f, 0.02f, 0.09f), new Vector3(0.13f, 0.065f, 0.13f), Terracotta);
                Part(root.transform, PrimitiveType.Cylinder, "Rim", new Vector3(0f, 0.085f, 0.09f), new Vector3(0.14f, 0.01f, 0.14f), Color.Lerp(Terracotta, Color.black, 0.2f));
                break;
            case "cooking_pot":
                BuildVessel(root.transform, 0.09f, 0.075f, 0.15f, IronGrey, true);
                break;
            case "potash_kettle":
                BuildVessel(root.transform, 0.11f, 0.085f, 0.17f, IronGrey, true);
                Part(root.transform, PrimitiveType.Cube, "Spout", new Vector3(0f, -0.10f, 0.12f), new Vector3(0.03f, 0.03f, 0.09f), Steel, new Vector3(-20f, 0f, 0f));
                break;
            case "tarp":
                Part(root.transform, PrimitiveType.Cube, "Bundle", new Vector3(0f, 0f, 0.10f), new Vector3(0.20f, 0.07f, 0.15f), TarpGreen);
                Part(root.transform, PrimitiveType.Cube, "Band", new Vector3(0f, 0f, 0.10f), new Vector3(0.205f, 0.075f, 0.022f), Rope);
                Part(root.transform, PrimitiveType.Cube, "Band 2", new Vector3(0f, 0f, 0.16f), new Vector3(0.205f, 0.075f, 0.022f), Rope);
                break;
        }

        // The anchors the code attaches to (and a prefab would carry): the right grip at the origin, the left grip on two-handed tools, and the working tip.
        Anchor(root.transform, ArtSwap.AnchorGripR, Vector3.zero);
        if (layout.twoHanded)
            Anchor(root.transform, ArtSwap.AnchorGripL, layout.gripLeft);
        Anchor(root.transform, ArtSwap.AnchorTip, layout.tip);
        if (layout.hasNock)
            Anchor(root.transform, ArtSwap.AnchorNock, layout.nock);
        if (layout.hasMuzzle)
            Anchor(root.transform, ArtSwap.AnchorMuzzle, layout.muzzle);
        if (layout.hasLine)
            Anchor(root.transform, ArtSwap.AnchorLine, layout.line);
        if (layout.hasFlame)
        {
            Transform flame = Anchor(root.transform, ArtSwap.AnchorFlame, layout.flame);
            BuildFlameStates(root.transform, flame.localPosition, toolId == "lantern");
        }

        ArtSwap.SetViewLayer(root, layer);
        return root;
    }

    // A hanging vessel (bucket, pot, kettle): a bail to hold it by at the origin, two wires, and a cylinder body below. radiusTop and radiusBottom are in metres, height the body's.
    static void BuildVessel(Transform root, float radiusTop, float radiusBottom, float height, Color color, bool bail)
    {
        float mid = -(0.035f + height * 0.5f);
        Part(root, PrimitiveType.Cylinder, "Body", new Vector3(0f, mid, 0f), new Vector3((radiusTop + radiusBottom), height * 0.5f, (radiusTop + radiusBottom)), color);
        Part(root, PrimitiveType.Cylinder, "Rim", new Vector3(0f, mid + height * 0.5f, 0f), new Vector3(radiusTop * 2.1f, 0.008f, radiusTop * 2.1f), Color.Lerp(color, Color.black, 0.25f));
        if (!bail)
            return;
        Part(root, PrimitiveType.Cube, "Handle", Vector3.zero, new Vector3(radiusTop * 1.6f, 0.012f, 0.012f), Steel);
        Part(root, PrimitiveType.Cube, "Bail R", new Vector3(radiusTop * 0.8f, -0.018f, 0f), new Vector3(0.008f, 0.04f, 0.008f), Steel);
        Part(root, PrimitiveType.Cube, "Bail L", new Vector3(-radiusTop * 0.8f, -0.018f, 0f), new Vector3(0.008f, 0.04f, 0.008f), Steel);
    }

    // A round handle along +Z from z0 to z1.
    static void Handle(Transform root, float z0, float z1, float diameter)
    {
        float length = z1 - z0;
        Part(root, PrimitiveType.Cylinder, "Handle", new Vector3(0f, 0f, (z0 + z1) * 0.5f), new Vector3(diameter, length * 0.5f, diameter), Wood, new Vector3(90f, 0f, 0f));
    }

    static Transform Anchor(Transform root, string name, Vector3 position)
    {
        var anchor = new GameObject(name).transform;
        anchor.SetParent(root, false);
        anchor.localPosition = position;
        return anchor;
    }

    // The two state children of a light: State_Unlit (a charred tip for the torch, nothing for the lantern) and State_Lit (the flame and its glow, small unlit shapes at the flame anchor).
    // ArtSwap's state names; PlayerHands shows the one that matches PortableLight.IsLit and flickers the lit one's flame.
    static void BuildFlameStates(Transform root, Vector3 at, bool lantern)
    {
        var unlit = new GameObject(ArtSwap.StateUnlit).transform;
        unlit.SetParent(root, false);
        var lit = new GameObject(ArtSwap.StateLit).transform;
        lit.SetParent(root, false);
        if (!lantern)
            Part(unlit, PrimitiveType.Cylinder, "Char", at - new Vector3(0f, 0f, 0.03f), new Vector3(0.05f, 0.012f, 0.05f), new Color(0.06f, 0.05f, 0.05f), new Vector3(90f, 0f, 0f));

        float size = lantern ? 0.06f : 0.05f;
        Glow(lit, "Flame", at, new Vector3(size, size * 1.8f, size), new Color(1f, 0.55f, 0.12f), lantern ? Vector3.zero : new Vector3(90f, 0f, 0f));
        Glow(lit, "Core", at, new Vector3(size * 0.55f, size, size * 0.55f), new Color(1f, 0.9f, 0.55f), lantern ? Vector3.zero : new Vector3(90f, 0f, 0f));
    }

    // A small emissive-looking shape: an unlit material (HDRP/Unlit), so it shines at night the same as by day. Falls back to a bright lit material when that shader is not there.
    static GameObject Glow(Transform parent, string name, Vector3 position, Vector3 scale, Color color, Vector3 euler)
    {
        GameObject go = Part(parent, PrimitiveType.Capsule, name, position, scale, color, euler);
        Shader unlit = Shader.Find("HDRP/Unlit");
        if (unlit != null)
        {
            var material = new Material(unlit);
            material.SetColor("_UnlitColor", color);
            go.GetComponent<Renderer>().SetPropertyBlock(null);
            go.GetComponent<Renderer>().sharedMaterial = material;
        }
        return go;
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
