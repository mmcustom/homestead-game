using UnityEngine;

// A placed Tent, Lean-To, Tarp Shelter or Small Cabin (Building_Housing_System.md's Sleep System and Small Cabin; WoodManager owns
// the state). The main interaction sleeps in it (SleepManager) — till morning at night, a short rest by day — with the
// shelter keeping off rain, wind and some of the cold. The second (R) packs a Tent back up into the Inventory, or takes
// a Lean-To down for half its Branches back. A Small Cabin is permanent by default, so R does nothing for it unless the
// Hammer is equipped: then it dismantles it (twice-pressed, hearth out first) for half its materials back.
// Drawn from simple shapes: a canvas A-frame tent, a lean-to of branches on a ridge pole, a small stacked-log cabin
// with a thatched roof (its hearth is a separate, ordinary Campfire — see WoodManager.BuildPile).
public class Shelter : MonoBehaviour, IInteractable, ISecondaryInteractable
{
    // The cabin's footprint (its own local space), so WoodManager can place the hearth just clear of its walls.
    public const float CabinWidth = 4f, CabinDepth = 5f;

    WoodPileState state;
    Material bark;

    public WoodPileState State => state;
    bool IsTent => state.kind == PileKind.Tent;
    bool IsCabin => state.kind == PileKind.Cabin;
    bool IsTarp => state.kind == PileKind.TarpShelter;
    string Name => WoodManager.PileName(state.kind);

    public void Bind(WoodPileState shelterState, Material barkMaterial)
    {
        state = shelterState;
        bark = barkMaterial;
        if (IsTent)
            BuildTent();
        else if (IsCabin)
            BuildCabin();
        else if (IsTarp)
            BuildTarp();
        else
            BuildLeanTo();
    }

    public string InteractionPrompt
    {
        get
        {
            TimeManager time = TimeManager.Instance;
            bool night = time == null || time.HourOfDay >= 19f || time.HourOfDay < 6f;
            string bag = SleepManager.CarryingBag ? "" : ", no Sleeping Bag";
            return night ? $"Sleep in the {Name}  (until morning{bag})" : $"Rest in the {Name}  (3 hours{bag})";
        }
    }

    public bool CanInteract(PlayerController player) => state != null && SleepManager.Instance != null;

    public void Interact(PlayerController player)
    {
        if (SleepManager.Instance != null)
            SleepManager.Instance.Sleep(state);
    }

    // A Small Cabin is dismantled with the Hammer (WoodManager.DismantleCabin), and — the most invested structure in the
    // game so far — only on a second press of R within a few seconds, so it can't happen by accident.
    const float ConfirmSeconds = 5f;
    float confirmUntil;
    bool Confirming => Time.unscaledTime < confirmUntil;

    public string SecondaryPrompt
    {
        get
        {
            if (state == null)
                return "";
            if (IsCabin)
            {
                // Shown only with the Hammer in hand, so a cabin you're just walking past stays uncluttered.
                WoodManager wood = WoodManager.Instance;
                if (wood == null || !WoodManager.HammerEquipped)
                    return "";
                if (!wood.CanDismantleCabin(state, out string why))
                    return $"Dismantle Small Cabin  — {why}";
                return Confirming ? "Press R again to dismantle the Small Cabin" : "Dismantle Small Cabin  (gives back about half its materials)";
            }
            if (IsTarp)
                return $"Take Down Tarp Shelter  (gives back the Tarp and {WoodManager.TarpShelterSticks / 2} Sticks)";
            return IsTent ? "Pack Up Tent" : "Take Down Lean-To  (keeps 4 Branches)";
        }
    }

    public void SecondaryInteract(PlayerController player)
    {
        InventoryManager inventory = InventoryManager.Instance;
        WoodManager wood = WoodManager.Instance;
        if (state == null || inventory == null || wood == null)
            return;

        if (IsCabin)
        {
            DismantleCabin(wood);
            return;
        }

        if (IsTarp)
        {
            // The Tarp and half the Sticks; what the pack can't carry is left on the ground, so nothing is lost.
            var refund = new StructureRefund(state.position, transform);
            refund.Return(SleepManager.TarpId, 1);
            refund.Return(WoodManager.SticksId, WoodManager.TarpShelterSticks / 2);
            ToolStatus.Flash(refund.Describe("Tarp Shelter"), 4f);
            wood.RemovePile(state);
            return;
        }

        if (IsTent)
        {
            if (inventory.AddToPlayer(WoodManager.TentId, 1) < 1)
            {
                ToolStatus.Flash("No room to carry the Tent");
                return;
            }
            ToolStatus.Flash("Packed up the Tent");
        }
        else
        {
            int kept = inventory.AddToPlayer(WoodManager.BranchesId, 4);
            if (kept < 4)
                wood.DropPile(transform.position, WoodManager.BranchesId, 4 - kept);
            ToolStatus.Flash("Took down the Lean-To");
        }
        wood.RemovePile(state);
    }

    // First press asks; a second within ConfirmSeconds takes the cabin down.
    void DismantleCabin(WoodManager wood)
    {
        if (!wood.CanDismantleCabin(state, out string reason))
        {
            ToolStatus.Flash($"Can't dismantle the Small Cabin — {reason}.", 3.5f);
            return;
        }

        if (!Confirming)
        {
            confirmUntil = Time.unscaledTime + ConfirmSeconds;
            ToolStatus.Flash("Dismantle the Small Cabin? Press R again to confirm — you'll get about half its materials back.", ConfirmSeconds);
            return;
        }

        ToolStatus.Flash(wood.DismantleCabin(state, transform), 6f);
    }

    // --- Looks ---

    void BuildTent()
    {
        var canvas = new Color(0.52f, 0.5f, 0.38f);
        const float width = 1.6f, length = 2.2f, height = 1.25f;
        float slope = Mathf.Atan2(height, width / 2f) * Mathf.Rad2Deg;
        float side = Mathf.Sqrt(height * height + width * width / 4f);
        foreach (float dir in new[] { -1f, 1f })
        {
            GameObject panel = Piece(PrimitiveType.Cube, new Vector3(dir * width / 4f, height / 2f, 0f),
                                     new Vector3(0f, 0f, dir * (90f - slope)), new Vector3(0.03f, side, length), null);
            Tint(panel, canvas);
        }
        // Ridge pole and the two end poles.
        Piece(PrimitiveType.Cylinder, new Vector3(0f, height, 0f), new Vector3(90f, 0f, 0f), new Vector3(0.04f, length / 2f + 0.05f, 0.04f), bark);
        foreach (float z in new[] { -length / 2f, length / 2f })
            Piece(PrimitiveType.Cylinder, new Vector3(0f, height / 2f, z), Vector3.zero, new Vector3(0.035f, height / 2f, 0.035f), bark);
        // A darker triangular back wall, so the open end reads as the front.
        var back = new GameObject("Back");
        back.transform.SetParent(transform, false);
        back.transform.localPosition = new Vector3(0f, 0f, -length / 2f + 0.02f);
        back.AddComponent<MeshFilter>().sharedMesh = Gable(width - 0.06f, height - 0.04f);
        var backRenderer = back.AddComponent<MeshRenderer>();
        MeshRenderer panelRenderer = GetComponentInChildren<MeshRenderer>();
        backRenderer.sharedMaterial = panelRenderer != null ? panelRenderer.sharedMaterial : null;
        Tint(back, canvas * 0.75f);

        var box = gameObject.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, height / 2f, 0f);
        box.size = new Vector3(width, height, length);
    }

    // A pitched tarp: a blue sheet strung from a ridgeline between two poles at the front, sloping down to two stakes at
    // the back. A crouching player fits under the high side; the sheet's collider is what OverheadCover.Roofed sees, so it
    // keeps the rain off while awake, and what the sleep prompt looks at. Smaller than the Lean-To (2.0 x 1.6 m, not 2.4 x 1.8).
    void BuildTarp()
    {
        const float width = 2.0f, depth = 1.6f, high = 2.0f, low = 0.5f, thickness = 0.04f;
        var blue = new Color(0.2f, 0.42f, 0.58f);
        float length = Mathf.Sqrt((high - low) * (high - low) + depth * depth);
        float pitch = Mathf.Atan2(high - low, depth) * Mathf.Rad2Deg;

        GameObject sheet = Piece(PrimitiveType.Cube, new Vector3(0f, (high + low) / 2f, 0f), new Vector3(-pitch, 0f, 0f),
                                 new Vector3(width, thickness, length), null);
        Tint(sheet, blue);
        sheet.AddComponent<BoxCollider>().size = Vector3.one; // the cube mesh is 1 m a side; the scale does the rest

        // Poles at the high front corners, and the ridgeline across them.
        foreach (float x in new[] { -width / 2f, width / 2f })
        {
            Piece(PrimitiveType.Cylinder, new Vector3(x, high / 2f, depth / 2f), Vector3.zero, new Vector3(0.05f, high / 2f, 0.05f), bark);
            Piece(PrimitiveType.Cylinder, new Vector3(x, low / 2f, -depth / 2f), Vector3.zero, new Vector3(0.03f, low / 2f, 0.03f), bark);
        }
        GameObject ridge = Piece(PrimitiveType.Cylinder, new Vector3(0f, high, depth / 2f), new Vector3(0f, 0f, 90f), new Vector3(0.015f, width / 2f, 0.015f), null);
        Tint(ridge, new Color(0.75f, 0.68f, 0.5f));
    }

    void BuildLeanTo()
    {
        const float width = 2.4f, depth = 1.8f, height = 1.5f;
        // Two forked uprights at the front and a ridge pole across them.
        foreach (float x in new[] { -width / 2f, width / 2f })
            Piece(PrimitiveType.Cylinder, new Vector3(x, height / 2f, depth / 2f), Vector3.zero, new Vector3(0.07f, height / 2f, 0.07f), bark);
        Piece(PrimitiveType.Cylinder, new Vector3(0f, height, depth / 2f), new Vector3(0f, 0f, 90f), new Vector3(0.06f, width / 2f + 0.1f, 0.06f), bark);
        // Branches laid from the ridge pole down to the ground behind, side by side.
        float roofLength = Mathf.Sqrt(height * height + depth * depth);
        float angle = Mathf.Atan2(height, depth) * Mathf.Rad2Deg;
        var random = new System.Random(state.id * 131);
        for (int i = 0; i < 12; i++)
        {
            float x = -width / 2f + (i + 0.5f) * width / 12f;
            float jitter = (float)random.NextDouble() * 0.1f;
            Piece(PrimitiveType.Cylinder, new Vector3(x, height / 2f + jitter * 0.3f, 0f), new Vector3(90f - angle, 0f, 0f),
                  new Vector3(0.09f, roofLength / 2f + jitter, 0.09f), bark);
        }

        var box = gameObject.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, height / 2f, 0f);
        box.size = new Vector3(width + 0.2f, height, depth + 0.2f);
    }

    // A small one-room log cabin: a stone-and-clay foundation, stacked horizontal logs, and a grass-thatched, peaked
    // roof — the first permanent residence, so unlike the Tent and Lean-To it's one solid shell (no take-down).
    // The front (+Z) wall has a real doorway (DoorWidth x about DoorHeight, a lintel log across the top). Colliders are
    // separate children — wall slabs that leave the door gap, the foundation as the floor, a step at the door and a
    // collider per roof panel — all under this Shelter, so the sleep prompt resolves from inside and out, and
    // OverheadCover.Roofed sees the roof. Built the same way for a cabin saved before this was changed (it's rebuilt on load).
    public const float DoorWidth = 1.1f, DoorHeight = 2.0f;

    void BuildCabin()
    {
        var stoneColor = new Color(0.58f, 0.55f, 0.5f);
        var thatch = new Color(0.62f, 0.55f, 0.28f);
        const float width = CabinWidth, depth = CabinDepth, wallHeight = 2.2f, ridgeHeight = 3.4f, logRadius = 0.14f;
        const float foundationHeight = 0.3f, logDiameter = logRadius * 2f;

        GameObject foundation = Piece(PrimitiveType.Cube, new Vector3(0f, foundationHeight / 2f, 0f), Vector3.zero,
                                      new Vector3(width + 0.2f, foundationHeight, depth + 0.2f), null);
        Tint(foundation, stoneColor);
        Solid("Floor", new Vector3(0f, foundationHeight / 2f, 0f), new Vector3(width + 0.2f, foundationHeight, depth + 0.2f), Quaternion.identity);

        // A low stone step at the doorway (the player steps 0.4 m, the slab is 0.3, but it reads better and is safer).
        float stepDepth = 0.5f;
        GameObject step = Piece(PrimitiveType.Cube, new Vector3(0f, 0.075f, depth / 2f + 0.1f + stepDepth / 2f), Vector3.zero,
                                new Vector3(DoorWidth + 0.4f, 0.15f, stepDepth), null);
        Tint(step, stoneColor * 0.9f);
        Solid("Step", step.transform.localPosition, step.transform.localScale, Quaternion.identity);

        // Stacked horizontal logs for each of the four walls. A Cylinder primitive is 1 m across at scale 1, so the
        // diameter is logDiameter and a course is exactly one log high (no gaps). The side walls run along Z (90, 0, 0),
        // the front and back along X (90, 90, 0). The front wall leaves the doorway open below the lintel course.
        int courses = Mathf.Max(4, Mathf.RoundToInt(wallHeight / logDiameter));
        int doorCourses = Mathf.Min(courses - 1, Mathf.FloorToInt(DoorHeight / logDiameter));
        float doorTop = foundationHeight + doorCourses * logDiameter;
        float segment = width / 2f - DoorWidth / 2f; // each piece of front wall beside the door
        for (int i = 0; i < courses; i++)
        {
            float y = foundationHeight + logRadius + i * logDiameter;
            foreach (float side in new[] { -1f, 1f })
            {
                Piece(PrimitiveType.Cylinder, new Vector3(side * width / 2f, y, 0f), new Vector3(90f, 0f, 0f),
                      new Vector3(logDiameter, depth / 2f + logRadius, logDiameter), bark);
            }
            Piece(PrimitiveType.Cylinder, new Vector3(0f, y, -depth / 2f), new Vector3(90f, 90f, 0f),
                  new Vector3(logDiameter, width / 2f, logDiameter), bark);
            if (i >= doorCourses) // the lintel course (and anything above it) runs right across
            {
                Piece(PrimitiveType.Cylinder, new Vector3(0f, y, depth / 2f), new Vector3(90f, 90f, 0f),
                      new Vector3(logDiameter, width / 2f, logDiameter), bark);
                continue;
            }
            foreach (float side in new[] { -1f, 1f })
                Piece(PrimitiveType.Cylinder, new Vector3(side * (DoorWidth / 2f + segment / 2f), y, depth / 2f), new Vector3(90f, 90f, 0f),
                      new Vector3(logDiameter, segment / 2f, logDiameter), bark);
        }
        // Door jambs: a post each side, so the opening reads as framed (the dark slab that used to fill it is gone).
        foreach (float side in new[] { -1f, 1f })
            Piece(PrimitiveType.Cylinder, new Vector3(side * DoorWidth / 2f, foundationHeight + (doorTop - foundationHeight) / 2f, depth / 2f),
                  Vector3.zero, new Vector3(logRadius, (doorTop - foundationHeight) / 2f, logRadius), bark);

        // A peaked, thatched roof over a ridge pole.
        float roofY = foundationHeight + courses * logDiameter;
        float rise = ridgeHeight - roofY;
        float slope = Mathf.Atan2(rise, width / 2f) * Mathf.Rad2Deg;
        float roofSide = Mathf.Sqrt(rise * rise + width * width / 4f);
        foreach (float dir in new[] { -1f, 1f })
        {
            GameObject panel = Piece(PrimitiveType.Cube, new Vector3(dir * width / 4f, roofY + rise / 2f, 0f),
                                     new Vector3(0f, 0f, dir * (90f - slope)), new Vector3(0.08f, roofSide + 0.3f, depth + 0.4f), null);
            Tint(panel, thatch);
            panel.AddComponent<BoxCollider>().size = Vector3.one; // the cube mesh is 1 m a side; the scale does the rest
        }
        Piece(PrimitiveType.Cylinder, new Vector3(0f, ridgeHeight, 0f), new Vector3(90f, 0f, 0f), new Vector3(0.06f, depth / 2f + 0.15f, 0.06f), bark);

        // Wall colliders: one slab per wall, the front as two segments with a lintel over the gap. They stop at the
        // top log course, so the roof panels (their own colliders) close the shell above.
        float wallTop = roofY, wallBase = foundationHeight;
        float slabHeight = wallTop - wallBase, slabY = wallBase + slabHeight / 2f;
        float outerWidth = width + logDiameter, outerDepth = depth + logDiameter;
        Solid("Wall Left", new Vector3(-width / 2f, slabY, 0f), new Vector3(logDiameter, slabHeight, outerDepth), Quaternion.identity);
        Solid("Wall Right", new Vector3(width / 2f, slabY, 0f), new Vector3(logDiameter, slabHeight, outerDepth), Quaternion.identity);
        Solid("Wall Back", new Vector3(0f, slabY, -depth / 2f), new Vector3(outerWidth, slabHeight, logDiameter), Quaternion.identity);
        float frontLength = segment + logRadius; // out to the corner log
        foreach (float side in new[] { -1f, 1f })
            Solid("Wall Front", new Vector3(side * (DoorWidth / 2f + frontLength / 2f), slabY, depth / 2f), new Vector3(frontLength, slabHeight, logDiameter), Quaternion.identity);
        float lintelHeight = wallTop - doorTop;
        Solid("Lintel", new Vector3(0f, doorTop + lintelHeight / 2f, depth / 2f), new Vector3(DoorWidth, lintelHeight, logDiameter), Quaternion.identity);
    }

    // An invisible box collider on a child, in this shelter's local space. Child colliders still resolve to this Shelter
    // (the player's interaction ray uses GetComponentInParent), so the sleep prompt works wherever the ray lands.
    void Solid(string name, Vector3 center, Vector3 size, Quaternion rotation)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = center;
        go.transform.localRotation = rotation;
        go.AddComponent<BoxCollider>().size = size;
    }

    // A flat triangle, base on the ground and apex up, faced both ways.
    static Mesh Gable(float width, float height)
    {
        var mesh = new Mesh { name = "Tent Gable" };
        mesh.vertices = new[]
        {
            new Vector3(-width / 2f, 0f, 0f), new Vector3(0f, height, 0f), new Vector3(width / 2f, 0f, 0f),
            new Vector3(-width / 2f, 0f, 0f), new Vector3(0f, height, 0f), new Vector3(width / 2f, 0f, 0f),
        };
        mesh.triangles = new[] { 0, 1, 2, 5, 4, 3 };
        mesh.normals = new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.forward, Vector3.forward, Vector3.forward };
        mesh.RecalculateBounds();
        return mesh;
    }

    static void Tint(GameObject piece, Color color)
    {
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        piece.GetComponent<MeshRenderer>().SetPropertyBlock(block);
    }

    GameObject Piece(PrimitiveType shape, Vector3 position, Vector3 euler, Vector3 scale, Material material)
    {
        GameObject piece = GameObject.CreatePrimitive(shape);
        Destroy(piece.GetComponent<Collider>());
        piece.transform.SetParent(transform, false);
        piece.transform.localPosition = position;
        piece.transform.localRotation = Quaternion.Euler(euler);
        piece.transform.localScale = scale;
        if (material != null)
            piece.GetComponent<MeshRenderer>().sharedMaterial = material;
        return piece;
    }
}
