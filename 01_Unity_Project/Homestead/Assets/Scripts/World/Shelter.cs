using UnityEngine;

// A placed Tent, Lean-To or Small Cabin (Building_Housing_System.md's Sleep System and Small Cabin; WoodManager owns
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
    string Name => WoodManager.PileName(state.kind);

    public void Bind(WoodPileState shelterState, Material barkMaterial)
    {
        state = shelterState;
        bark = barkMaterial;
        if (IsTent)
            BuildTent();
        else if (IsCabin)
            BuildCabin();
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
    void BuildCabin()
    {
        var stoneColor = new Color(0.58f, 0.55f, 0.5f);
        var thatch = new Color(0.62f, 0.55f, 0.28f);
        var doorway = new Color(0.16f, 0.12f, 0.08f);
        const float width = CabinWidth, depth = CabinDepth, wallHeight = 2.2f, ridgeHeight = 3.4f, logRadius = 0.14f;

        GameObject foundation = Piece(PrimitiveType.Cube, new Vector3(0f, 0.15f, 0f), Vector3.zero, new Vector3(width + 0.2f, 0.3f, depth + 0.2f), null);
        Tint(foundation, stoneColor);

        // Stacked horizontal logs for each of the four walls.
        int courses = Mathf.Max(4, Mathf.RoundToInt(wallHeight / (logRadius * 2f)));
        for (int i = 0; i < courses; i++)
        {
            float y = 0.3f + logRadius + i * logRadius * 2f;
            foreach (float side in new[] { -1f, 1f })
                Piece(PrimitiveType.Cylinder, new Vector3(side * width / 2f, y, 0f), new Vector3(0f, 0f, 90f),
                      new Vector3(logRadius, depth / 2f, logRadius), bark);
            foreach (float side in new[] { -1f, 1f })
                Piece(PrimitiveType.Cylinder, new Vector3(0f, y, side * depth / 2f), new Vector3(90f, 90f, 0f),
                      new Vector3(logRadius, width / 2f, logRadius), bark);
        }

        // A peaked, thatched roof over a ridge pole.
        float roofY = 0.3f + courses * logRadius * 2f;
        float rise = ridgeHeight - roofY;
        float slope = Mathf.Atan2(rise, width / 2f) * Mathf.Rad2Deg;
        float roofSide = Mathf.Sqrt(rise * rise + width * width / 4f);
        foreach (float dir in new[] { -1f, 1f })
        {
            GameObject panel = Piece(PrimitiveType.Cube, new Vector3(dir * width / 4f, roofY + rise / 2f, 0f),
                                     new Vector3(0f, 0f, dir * (90f - slope)), new Vector3(0.08f, roofSide + 0.3f, depth + 0.4f), null);
            Tint(panel, thatch);
        }
        Piece(PrimitiveType.Cylinder, new Vector3(0f, ridgeHeight, 0f), new Vector3(90f, 0f, 0f), new Vector3(0.06f, depth / 2f + 0.15f, 0.06f), bark);

        // A plain dark doorway on the front wall.
        GameObject door = Piece(PrimitiveType.Cube, new Vector3(0f, 0.3f + 0.9f, depth / 2f - 0.02f), Vector3.zero, new Vector3(0.9f, 1.8f, 0.05f), null);
        Tint(door, doorway);

        var box = gameObject.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, roofY / 2f, 0f);
        box.size = new Vector3(width + 0.3f, roofY, depth + 0.3f);
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
