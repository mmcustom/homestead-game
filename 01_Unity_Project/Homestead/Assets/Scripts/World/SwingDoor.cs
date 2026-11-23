using UnityEngine;

// A plank door on a hinge (Mike, 2026-11-23: "doors that swing open when we walk into them ... it's okay if it swings in and out"). It swings away from the
// player as they come up to the doorway, inward from outside and outward from inside, stays open while they are in the doorway, and swings shut once
// they have gone through. Nothing is saved: a door is always shut when it is built or loaded.
//
// The door object sits at the hinge, with the doorway's plane along its local X (the leaf runs from the hinge to +X, width long) and its outside to +Z.
// Its leaf is boards and two battens drawn from cubes; the leaf's collider shuts the doorway only while the door is shut, so it never shoves the player.
public class SwingDoor : MonoBehaviour
{
    public float width = 1f, height = 2f;
    public float openAngle = 100f;     // how far it swings, degrees
    public float openDistance = 1.4f;  // how near the doorway's plane the player must be for it to open
    public float swingSpeed = 240f;    // degrees a second

    Transform leaf;
    BoxCollider leafCollider;
    float angle, side;
    static PlayerController player;

    // A door of the given size hung at hingeLocal (in parent's space), swinging about the parent's up axis. thickness: the boards'. material: the planks'
    // (null: a plain weathered wood tint).
    public static SwingDoor Create(Transform parent, Vector3 hingeLocal, float width, float height, float thickness, Material material, string name = "Door")
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = hingeLocal;
        go.transform.localRotation = Quaternion.identity;
        var door = go.AddComponent<SwingDoor>();
        door.width = width;
        door.height = height;
        door.Build(thickness, material);
        return door;
    }

    static readonly Color Weathered = new Color(0.36f, 0.29f, 0.21f);

    void Build(float thickness, Material material)
    {
        leaf = new GameObject("Leaf").transform;
        leaf.SetParent(transform, false);
        int boards = Mathf.Max(3, Mathf.RoundToInt(width / 0.18f));
        float boardWidth = width / boards;
        for (int i = 0; i < boards; i++)
        {
            float h = height - (i % 2) * 0.015f; // a little uneven along the top
            Board(new Vector3(boardWidth * (i + 0.5f), h / 2f, 0f), new Vector3(boardWidth - 0.006f, h, thickness), material);
        }
        // Two battens across the inside face, and a pull at the free edge on both faces.
        foreach (float y in new[] { height * 0.22f, height * 0.78f })
            Board(new Vector3(width / 2f, y, -thickness), new Vector3(width - 0.08f, 0.1f, thickness), material);
        foreach (float z in new[] { thickness, -thickness * 2f })
            Board(new Vector3(width - 0.12f, height * 0.5f, z), new Vector3(0.04f, 0.16f, 0.03f), material);
        leafCollider = leaf.gameObject.AddComponent<BoxCollider>();
        leafCollider.center = new Vector3(width / 2f, height / 2f, 0f);
        leafCollider.size = new Vector3(width, height, thickness);
    }

    void Board(Vector3 position, Vector3 scale, Material material)
    {
        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(board.GetComponent<Collider>());
        board.transform.SetParent(leaf, false);
        board.transform.localPosition = position;
        board.transform.localScale = scale;
        var renderer = board.GetComponent<MeshRenderer>();
        if (material != null)
            renderer.sharedMaterial = material;
        else
            renderer.material.color = Weathered;
    }

    // Where the door wants to be for a player at this point in its own space: shut (0), or swung away from them (+ swings the free edge inward, to -Z).
    public static float TargetAngle(Vector3 local, float width, float openDistance, float openAngle, float side)
    {
        bool near = Mathf.Abs(local.z) < openDistance && local.x > -0.5f && local.x < width + 0.5f && local.y > -1f && local.y < 2.5f;
        if (!near)
            return 0f;
        return side != 0f ? side * openAngle : (local.z >= 0f ? openAngle : -openAngle);
    }

    void Update()
    {
        if (player == null)
            player = FindAnyObjectByType<PlayerController>();
        float target = 0f;
        if (player != null)
        {
            Vector3 local = transform.InverseTransformPoint(player.transform.position);
            // The way it swings is chosen when it starts to open and kept while it is open, so walking through never flips it back at you.
            if (Mathf.Abs(angle) < 1f)
                side = 0f;
            target = TargetAngle(local, width, openDistance, openAngle, side);
            if (side == 0f && target != 0f)
                side = Mathf.Sign(target);
        }
        angle = Mathf.MoveTowards(angle, target, swingSpeed * Time.deltaTime);
        leaf.localRotation = Quaternion.Euler(0f, angle, 0f);
        leafCollider.enabled = Mathf.Abs(angle) < 3f;
    }
}
