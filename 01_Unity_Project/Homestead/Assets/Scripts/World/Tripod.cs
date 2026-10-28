using UnityEngine;

// A tripod standing over a fire, with the Cooking Pot or the Potash Kettle hung from it (FireManager owns the saved state on the fire; Campfire
// creates this object as its child and the rest is drawn here). Three lashed poles, a chain and the hung vessel are primitives built in code, like the
// taps, so there is no prefab; the vessel is redrawn only when it changes. It has its own colliders, so looking at the poles or the vessel targets
// the tripod and looking at the ring targets the fire. E hangs the Pot (else the Kettle), swaps one for the other or takes the vessel down; R with
// the Hammer, fire out, dismantles the tripod and everything on it comes back whole.
public class Tripod : MonoBehaviour, IInteractable, ISecondaryInteractable
{
    static readonly Color PoleColor = new Color(0.36f, 0.25f, 0.14f);
    static readonly Color LashColor = new Color(0.55f, 0.47f, 0.3f);
    static readonly Color ChainColor = new Color(0.16f, 0.16f, 0.17f);
    static readonly Color PotColor = new Color(0.13f, 0.13f, 0.15f);
    static readonly Color KettleColor = new Color(0.55f, 0.32f, 0.16f);

    const float PoleHeight = 1.6f, PoleSpread = 0.6f, VesselHeight = 0.8f;

    CampfireState state;
    GameObject vesselRoot;
    string drawnVessel = "\u0001"; // nothing is ever this, so the first Refresh always draws
    MaterialPropertyBlock block;

    public CampfireState State => state;

    public void Bind(CampfireState fireState)
    {
        bool first = state == null;
        state = fireState;
        if (first)
            BuildFrame();
        Refresh();
    }

    // Three poles leaning in to a lashing at the top, and a chain down from it for the vessel to hang on.
    void BuildFrame()
    {
        Vector3 top = new Vector3(0f, PoleHeight, 0f);
        for (int i = 0; i < 3; i++)
        {
            float angle = i * 120f * Mathf.Deg2Rad;
            Vector3 bottom = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * PoleSpread;
            Vector3 along = top - bottom;
            GameObject pole = Piece(transform, PrimitiveType.Cylinder, (top + bottom) * 0.5f, Quaternion.FromToRotation(Vector3.up, along),
                                    new Vector3(0.07f, along.magnitude * 0.5f, 0.07f), PoleColor, true);
            var capsule = pole.GetComponent<CapsuleCollider>();
            if (capsule != null)
                capsule.radius = 1.4f; // about 10 cm across in the world: easy to aim at without a solid wall around the fire
        }

        Piece(transform, PrimitiveType.Cylinder, top + Vector3.down * 0.04f, Quaternion.identity, new Vector3(0.15f, 0.06f, 0.15f), LashColor, false);
        Piece(transform, PrimitiveType.Cylinder, new Vector3(0f, PoleHeight - 0.32f, 0f), Quaternion.identity, new Vector3(0.012f, 0.3f, 0.012f), ChainColor, false);
    }

    // Redraws the hung vessel when it has changed.
    public void Refresh()
    {
        if (state == null)
            return;
        string hung = state.vessel ?? "";
        if (hung == drawnVessel)
            return;
        drawnVessel = hung;

        if (vesselRoot != null)
            Destroy(vesselRoot);
        vesselRoot = null;
        if (hung.Length == 0)
            return;

        vesselRoot = new GameObject("Vessel " + hung);
        vesselRoot.transform.SetParent(transform, false);
        vesselRoot.transform.localPosition = new Vector3(0f, VesselHeight, 0f);
        if (hung == TripodRules.KettleId)
            BuildKettle(vesselRoot.transform);
        else
            BuildPot(vesselRoot.transform);
    }

    // A squat iron pot with a lighter rim; the chain's end is at its top.
    void BuildPot(Transform parent)
    {
        Piece(parent, PrimitiveType.Cylinder, Vector3.zero, Quaternion.identity, new Vector3(0.36f, 0.12f, 0.36f), PotColor, true);
        Piece(parent, PrimitiveType.Cylinder, new Vector3(0f, 0.12f, 0f), Quaternion.identity, new Vector3(0.4f, 0.014f, 0.4f), ChainColor, false);
    }

    // A round copper kettle with a lid knob and a spout.
    void BuildKettle(Transform parent)
    {
        Piece(parent, PrimitiveType.Sphere, Vector3.zero, Quaternion.identity, new Vector3(0.38f, 0.3f, 0.38f), KettleColor, true);
        Piece(parent, PrimitiveType.Sphere, new Vector3(0f, 0.14f, 0f), Quaternion.identity, new Vector3(0.12f, 0.07f, 0.12f), LashColor, false);
        Piece(parent, PrimitiveType.Cylinder, new Vector3(0.22f, 0.04f, 0f), Quaternion.Euler(0f, 0f, -60f), new Vector3(0.05f, 0.1f, 0.05f), KettleColor, false);
    }

    GameObject Piece(Transform parent, PrimitiveType shape, Vector3 position, Quaternion rotation, Vector3 scale, Color color, bool collider)
    {
        GameObject piece = GameObject.CreatePrimitive(shape);
        if (!collider)
            Destroy(piece.GetComponent<Collider>());
        piece.transform.SetParent(parent, false);
        piece.transform.localPosition = position;
        piece.transform.localRotation = rotation;
        piece.transform.localScale = scale;
        block ??= new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        piece.GetComponent<MeshRenderer>().SetPropertyBlock(block);
        return piece;
    }

    static string NameOf(string itemId) => ItemDatabase.Get(itemId)?.DisplayName ?? itemId;

    TripodRules.Step NextStep()
    {
        InventoryManager inventory = InventoryManager.Instance;
        bool pot = inventory != null && inventory.Player.Has(TripodRules.PotId);
        bool kettle = inventory != null && inventory.Player.Has(TripodRules.KettleId);
        return TripodRules.NextStep(state.vessel, pot, kettle);
    }

    public bool CanInteract(PlayerController player) => state != null && FireManager.Instance != null && InventoryManager.Instance != null;

    public string InteractionPrompt
    {
        get
        {
            if (!CanInteract(null))
                return "";
            string line = TripodRules.StepPrompt(NextStep(), NameOf);
            string job = CampfireCooking.Instance != null ? CampfireCooking.Instance.JobTextFor(state.vessel) : null;
            return job != null ? line + "\n" + NameOf(state.vessel) + " hung: " + job : line;
        }
    }

    public void Interact(PlayerController player)
    {
        if (!CanInteract(player))
            return;
        string message = FireManager.Instance.TripodStep(state);
        if (message.Length > 0)
            ToolStatus.Flash(message, 3f);
    }

    string BackText => string.IsNullOrEmpty(state.vessel) ? "Tripod" : "Tripod, " + NameOf(state.vessel);

    public string SecondaryPrompt
    {
        get
        {
            if (state == null)
                return "";
            switch (TripodRules.CanDismantle(state.lit, WoodManager.HammerEquipped))
            {
                case TripodRules.DismantleResult.FireLit: return $"Dismantle the Tripod  (put the fire out first; back: {BackText})";
                case TripodRules.DismantleResult.NoHammer: return $"Dismantle the Tripod  (needs the Hammer; back: {BackText})";
                default: return $"Dismantle the Tripod  (back: {BackText})";
            }
        }
    }

    public void SecondaryInteract(PlayerController player)
    {
        if (state == null || FireManager.Instance == null)
            return;
        string message = FireManager.Instance.DismantleTripod(state);
        if (message.Length > 0)
            ToolStatus.Flash(message, 5f);
    }
}
