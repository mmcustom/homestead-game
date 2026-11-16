using UnityEngine;

// A tap in the World: a Spile in the trunk with a Sap Bucket hung under it (TapManager owns the saved state; this draws it and is what the
// player looks at). Looking at it shows what the bucket holds; E collects the sap the pack can carry, and R takes the tap down. Built in code from
// the Spile and Sap Bucket art (or three primitives without it), so there is no tap prefab; it redraws only when its state changes, never per frame.
public class Tap : MonoBehaviour, IInteractable, ISecondaryInteractable
{
    static readonly Color SpileColor = new Color(0.22f, 0.18f, 0.13f);
    static readonly Color BucketColor = new Color(0.46f, 0.33f, 0.2f);
    // Sap is a thin, watery amber, darker than the pale bucket wood so it shows inside it; glossy like water.
    static readonly Color SapColor = new Color(0.50f, 0.43f, 0.27f);
    static readonly Color SourColor = new Color(0.33f, 0.30f, 0.13f);
    const float SapSmoothness = 0.9f;

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

    // +Z is away from the trunk. The origin is 2 cm out from the trunk's capsule radius (TapManager), but the trunk mesh is an octagon that
    // tapers, so at tap height its bark is about 4 to 7 cm behind the origin (measured on Hardwood Broad and Tall, 2026-11-15): the art is set
    // back by BarkInset so the Spile goes into the bark instead of floating in front of it. The Spile is driven into the bark tip first, sloping a little down and out; the
    // Sap Bucket hangs from it by its bail, with its lid off so the sap shows; and a pale disc inside the bucket rises with the sap (browner once
    // it has soured). The Spile and the Sap Bucket are the hand-held art (Art/tool_spile, Art/tool_sap_bucket, Mike 2026-11-15); without that
    // art it falls back to the primitives.
    public const string SpileArt = "Art/tool_spile", BucketArt = "Art/tool_sap_bucket", BucketLidName = "Sap_Lid";
    public const float BarkInset = 0.06f;
    public static readonly Vector3 SpilePosition = new Vector3(0f, 0f, 0.105f - BarkInset); // the Spile's grip end: its tip is 0.155 in, about 5 cm into the bark
    public static readonly Vector3 SpileEuler = new Vector3(-8f, 180f, 0f);        // tip into the trunk, the trough end sloping down so the sap runs off it
    public static readonly Vector3 BucketPosition = new Vector3(0f, 0.008f, 0.12f - BarkInset); // the bail's grip resting on the Spile, the bucket clear of the bark
    // Inside the Sap Bucket, from its grip: the floor and just under the rim, and the inside width there.
    public const float SapFloorY = -0.140f, SapTopY = -0.035f, SapFloorWidth = 0.106f, SapTopWidth = 0.116f;

    bool art;

    void Build()
    {
        GameObject spileArt = Resources.Load<GameObject>(SpileArt);
        GameObject bucketArt = Resources.Load<GameObject>(BucketArt);
        art = spileArt != null && bucketArt != null;
        if (art)
        {
            Art(spileArt, SpilePosition, SpileEuler);
            GameObject bucket = Art(bucketArt, BucketPosition, Vector3.zero);
            Transform lid = bucket.transform.Find(BucketLidName);
            if (lid != null)
                lid.gameObject.SetActive(false); // lid off on the tree, on in the hand
            sap = Piece(PrimitiveType.Cylinder, BucketPosition + new Vector3(0f, SapFloorY, 0f), Vector3.zero, new Vector3(SapFloorWidth, 0.002f, SapFloorWidth), SapColor);
        }
        else
        {
            Piece(PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.07f), new Vector3(90f, 0f, 0f), new Vector3(0.035f, 0.07f, 0.035f), SpileColor);
            Piece(PrimitiveType.Cylinder, new Vector3(0f, -0.16f, 0.22f), Vector3.zero, new Vector3(0.28f, 0.13f, 0.28f), BucketColor);
            sap = Piece(PrimitiveType.Cylinder, new Vector3(0f, -0.028f, 0.22f), Vector3.zero, new Vector3(0.23f, 0.003f, 0.23f), SapColor);
        }
        sapRenderer = sap.GetComponent<MeshRenderer>();

        var box = gameObject.AddComponent<BoxCollider>();
        box.center = art ? new Vector3(0f, -0.07f, 0.07f) : new Vector3(0f, -0.12f, 0.15f);
        box.size = art ? new Vector3(0.22f, 0.24f, 0.22f) : new Vector3(0.34f, 0.36f, 0.34f);
    }

    GameObject Art(GameObject prefab, Vector3 position, Vector3 euler)
    {
        GameObject piece = Instantiate(prefab, transform, false);
        piece.transform.localPosition = position;
        piece.transform.localRotation = Quaternion.Euler(euler);
        foreach (Collider c in piece.GetComponentsInChildren<Collider>(true))
            Destroy(c);
        return piece;
    }

    // How far up the bucket the sap stands, 0 (floor) to 1 (just under the rim).
    public static float SapLevel(float litres) => Mathf.Clamp01(litres / MapleRules.BucketCapacityLitres);

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

    void Tint(MeshRenderer renderer, Color color, float smoothness = 0.5f)
    {
        block ??= new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        block.SetFloat("_Smoothness", smoothness);
        renderer.SetPropertyBlock(block);
    }

    public void Refresh()
    {
        if (state == null || sap == null)
            return;
        sap.SetActive(state.sour || state.litres > 0.05f);
        Tint(sapRenderer, state.sour ? SourColor : SapColor, SapSmoothness);
        if (art)
        {
            float level = SapLevel(state.litres);
            float width = Mathf.Lerp(SapFloorWidth, SapTopWidth, level);
            sap.transform.localPosition = BucketPosition + new Vector3(0f, Mathf.Lerp(SapFloorY, SapTopY, level), 0f);
            sap.transform.localScale = new Vector3(width, 0.002f, width);
        }
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
            {
                InventoryManager inventory = InventoryManager.Instance;
                int carried = inventory != null ? inventory.Player.Count(MapleRules.SapId) : 0;
                return $"Collect sap  ({Status}){KnowHowRules.TapBoilHint(carried)}";
            }
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
