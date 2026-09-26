using UnityEngine;

// A placed Tent or Lean-To (Building_Housing_System.md's Sleep System; WoodManager owns the state). The main interaction
// sleeps in it (SleepManager) — till morning at night, a short rest by day — with the shelter keeping off rain, wind
// and some of the cold. The second (R) packs a Tent back up into the Inventory, or takes a Lean-To down for half its
// Branches back. Drawn from simple shapes: a canvas A-frame tent, a lean-to of branches on a ridge pole.
public class Shelter : MonoBehaviour, IInteractable, ISecondaryInteractable
{
    WoodPileState state;
    Material bark;

    public WoodPileState State => state;
    bool IsTent => state.kind == PileKind.Tent;
    string Name => WoodManager.PileName(state.kind);

    public void Bind(WoodPileState shelterState, Material barkMaterial)
    {
        state = shelterState;
        bark = barkMaterial;
        if (IsTent)
            BuildTent();
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

    public string SecondaryPrompt => state == null ? "" : IsTent ? "Pack Up Tent" : "Take Down Lean-To  (keeps 4 Branches)";

    public void SecondaryInteract(PlayerController player)
    {
        InventoryManager inventory = InventoryManager.Instance;
        WoodManager wood = WoodManager.Instance;
        if (state == null || inventory == null || wood == null)
            return;

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
