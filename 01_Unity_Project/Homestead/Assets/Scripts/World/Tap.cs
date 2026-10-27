using UnityEngine;

// A tap in the World: a Spile in the trunk with a Sap Bucket hung under it (TapManager owns the saved state; this draws it and is what the
// player looks at). Looking at it shows what the bucket holds; E collects the sap the pack can carry, and R takes the tap down. Built from three
// primitives in code, like the structures, so there is no prefab; it redraws only when its state changes, never per frame.
public class Tap : MonoBehaviour, IInteractable, ISecondaryInteractable
{
    static readonly Color SpileColor = new Color(0.22f, 0.18f, 0.13f);
    static readonly Color BucketColor = new Color(0.46f, 0.33f, 0.2f);
    static readonly Color SapColor = new Color(0.86f, 0.82f, 0.62f);
    static readonly Color SourColor = new Color(0.42f, 0.38f, 0.2f);

    TapState state;
    GameObject sap;
    MeshRenderer sapRenderer;
    MaterialPropertyBlock block;

    public TapState State => state;

    public void Bind(TapState tapState)
    {
        state = tapState;
        if (sap == null)
            Build();
        Refresh();
    }

    // +Z is away from the trunk. The Spile is a short peg in the bark, the Bucket hangs under its end, and a pale disc on the Bucket's top
    // shows that it holds sap (browner once it has soured).
    void Build()
    {
        Piece(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.07f), new Vector3(90f, 0f, 0f), new Vector3(0.035f, 0.07f, 0.035f), SpileColor);
        Piece(PrimitiveType.Cylinder, new Vector3(0f, -0.16f, 0.22f), Vector3.zero, new Vector3(0.28f, 0.13f, 0.28f), BucketColor);
        sap = Piece(PrimitiveType.Cylinder, new Vector3(0f, -0.028f, 0.22f), Vector3.zero, new Vector3(0.23f, 0.003f, 0.23f), SapColor);
        sapRenderer = sap.GetComponent<MeshRenderer>();

        var box = gameObject.AddComponent<BoxCollider>();
        box.center = new Vector3(0f, -0.12f, 0.15f);
        box.size = new Vector3(0.34f, 0.36f, 0.34f);
    }

    GameObject Piece(PrimitiveType shape, Vector3 position, Vector3 euler, Vector3 scale, Color color)
    {
        GameObject piece = GameObject.CreatePrimitive(shape);
        Destroy(piece.GetComponent<Collider>());
        piece.transform.SetParent(transform, false);
        piece.transform.localPosition = position;
        piece.transform.localRotation = Quaternion.Euler(euler);
        piece.transform.localScale = scale;
        Tint(piece.GetComponent<MeshRenderer>(), color);
        return piece;
    }

    void Tint(MeshRenderer renderer, Color color)
    {
        block ??= new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        renderer.SetPropertyBlock(block);
    }

    public void Refresh()
    {
        if (state == null || sap == null)
            return;
        sap.SetActive(state.sour || state.litres > 0.05f);
        Tint(sapRenderer, state.sour ? SourColor : SapColor);
    }

    string Status => TapRules.Status(state.Bucket, TapManager.Instance != null && TapManager.Instance.FlowingNow);

    public string InteractionPrompt
    {
        get
        {
            if (state == null || TapManager.Instance == null)
                return "";
            if (state.sour)
                return $"Empty the Sap Bucket  ({Status})";
            if (MapleRules.Whole(state.litres) >= 1)
                return $"Collect sap  ({Status})";
            return $"Check the tap  ({Status})";
        }
    }

    public bool CanInteract(PlayerController player) => state != null && TapManager.Instance != null;

    public void Interact(PlayerController player)
    {
        if (state == null || TapManager.Instance == null)
            return;
        ToolStatus.Flash(TapManager.Instance.Collect(state));
    }

    public string SecondaryPrompt
    {
        get
        {
            if (state == null || TapManager.Instance == null)
                return "";
            bool loses = !state.sour && MapleRules.Whole(state.litres) > 0;
            return loses ? "Take down the tap  (back: Spile and Sap Bucket; the sap in the bucket is lost)"
                         : "Take down the tap  (back: Spile and Sap Bucket)";
        }
    }

    public void SecondaryInteract(PlayerController player)
    {
        if (state == null || TapManager.Instance == null)
            return;
        ToolStatus.Flash(TapManager.Instance.TakeDown(state), 5f);
    }
}
