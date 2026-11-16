using UnityEngine;

// A placed Rabbit Snare, Box Trap or Fish Trap (TrapManager owns its state). Looking at it shows what it is, how good
// the spot is and its condition; E checks it — taking any catch — or baits an empty Box Trap, and R repairs a worn
// trap or picks it back up. A caught animal or fish shows in the trap until it's checked.
public class Trap : MonoBehaviour, IInteractable, ISecondaryInteractable
{
    [Tooltip("Shown while there's a catch waiting.")]
    [SerializeField] GameObject catchVisual;
    [Tooltip("Shown while a Box Trap is baited.")]
    [SerializeField] GameObject baitVisual;
    [Tooltip("Tilted over when the trap is broken.")]
    [SerializeField] Transform body;

    TrapState state;
    Quaternion bodyRest = Quaternion.identity;

    // The art (Mike, 2026-11-15): a prefab at Resources/Art/trap_<item id> replaces the primitive Body. Its children, found by name, take over:
    // "Body" (tilted when broken), "Catch" (shown with a catch), "Bait" (shown while baited) and "State_Set" (shown while set, hidden with a
    // catch: the Box Trap's raised door). Any of them the art lacks stays the prefab's own. Without the art nothing changes.
    public const string ArtPrefix = "Art/trap_";
    public const string ArtBodyName = "Body", ArtCatchName = "Catch", ArtBaitName = "Bait", ArtSetName = "State_Set";
    public const float ColliderPadding = 0.02f, ColliderMinSize = 0.25f;
    GameObject setVisual;
    bool artChecked;

    public TrapState State => state;

    void Awake()
    {
        if (body != null)
            bodyRest = body.localRotation;
    }

    public void Bind(TrapState trapState)
    {
        state = trapState;
        if (!artChecked)
            UseArt();
        Refresh();
    }

    void UseArt()
    {
        artChecked = true;
        GameObject prefab = Resources.Load<GameObject>(ArtPrefix + TrapManager.ItemFor(state.type));
        if (prefab == null)
            return;

        GameObject art = Instantiate(prefab, transform, false);
        art.name = "Art";
        foreach (Collider c in art.GetComponentsInChildren<Collider>(true))
            Destroy(c);

        Transform artCatch = FindDeep(art.transform, ArtCatchName);
        if (artCatch != null)
        {
            if (catchVisual != null)
                catchVisual.SetActive(false);
            catchVisual = artCatch.gameObject;
        }
        Transform artBait = FindDeep(art.transform, ArtBaitName);
        if (artBait != null)
        {
            if (baitVisual != null)
                baitVisual.SetActive(false);
            baitVisual = artBait.gameObject;
        }
        if (body != null)
            body.gameObject.SetActive(false); // the primitives
        Transform artBody = art.transform.Find(ArtBodyName);
        if (artBody != null)
        {
            body = artBody;
            bodyRest = body.localRotation;
        }
        Transform set = FindDeep(art.transform, ArtSetName);
        setVisual = set != null ? set.gameObject : null;

        FitCollider(art.transform);
    }

    static Transform FindDeep(Transform root, string childName)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t != root && t.name == childName)
                return t;
        return null;
    }

    // The interaction box fitted round the art (the prefab's own box was sized for the bigger primitives).
    void FitCollider(Transform art)
    {
        var box = GetComponent<BoxCollider>();
        if (box == null)
            return;
        bool any = false;
        Bounds local = default;
        foreach (MeshFilter mf in art.GetComponentsInChildren<MeshFilter>(true))
        {
            if (mf.sharedMesh == null)
                continue;
            Bounds b = mf.sharedMesh.bounds;
            for (int i = 0; i < 8; i++)
            {
                var corner = new Vector3((i & 1) == 0 ? b.min.x : b.max.x, (i & 2) == 0 ? b.min.y : b.max.y, (i & 4) == 0 ? b.min.z : b.max.z);
                Vector3 p = transform.InverseTransformPoint(mf.transform.TransformPoint(corner));
                if (!any) { local = new Bounds(p, Vector3.zero); any = true; }
                else local.Encapsulate(p);
            }
        }
        if (!any)
            return;
        Vector3 size = local.size + Vector3.one * (2f * ColliderPadding);
        box.center = local.center;
        box.size = Vector3.Max(size, Vector3.one * ColliderMinSize);
    }

    public void Refresh()
    {
        if (state == null)
            return;

        bool caught = state.type == TrapType.FishTrap ? state.catchCount > 0
            : state.result == TrapResult.SuccessfulCatch || state.result == TrapResult.SmallCatch;
        if (catchVisual != null)
            catchVisual.SetActive(caught);
        if (setVisual != null)
            setVisual.SetActive(!caught);
        if (baitVisual != null)
            baitVisual.SetActive(!string.IsNullOrEmpty(state.bait));
        if (body != null)
            body.localRotation = state.condition <= 0f ? bodyRest * Quaternion.Euler(0f, 0f, 25f) : bodyRest;
    }

    string Name => TrapManager.NameOf(state.type);

    string Details => state.condition <= 0f
        ? "broken"
        : $"{TrapManager.PlacementWord(state.placement)} spot, {Mathf.RoundToInt(state.condition * 100f)}% condition";

    public string InteractionPrompt
    {
        get
        {
            TrapManager traps = TrapManager.Instance;
            if (state == null || traps == null)
                return "";

            if (state.type == TrapType.FishTrap && state.catchCount > 0)
                return $"Check {Name}  — fish in it!";
            if (state.result == TrapResult.SuccessfulCatch || state.result == TrapResult.SmallCatch)
                return $"Check {Name}  — something's caught!";

            if (traps.NeedsBait(state))
            {
                string bait = TrapManager.CarriedBait();
                if (bait != null)
                    return $"Bait {Name}  with {ItemDatabase.Get(bait)?.DisplayName ?? bait}";
                return $"Check {Name}  (unbaited — carry fruit or greens to bait it; {Details})";
            }

            string baitNote = state.type == TrapType.BoxTrap ? $", baited with {ItemDatabase.Get(state.bait)?.DisplayName ?? state.bait}" : "";
            return $"Check {Name}  ({Details}{baitNote})";
        }
    }

    public bool CanInteract(PlayerController player) => state != null && TrapManager.Instance != null;

    public void Interact(PlayerController player)
    {
        TrapManager traps = TrapManager.Instance;
        if (state == null || traps == null)
            return;

        bool caught = state.catchCount > 0 || state.result == TrapResult.SuccessfulCatch || state.result == TrapResult.SmallCatch;
        if (!caught && traps.NeedsBait(state) && traps.Bait(state))
        {
            ToolStatus.Flash($"Baited the {Name}");
            return;
        }

        ToolStatus.Flash(traps.Check(state));
    }

    public string SecondaryPrompt
    {
        get
        {
            TrapManager traps = TrapManager.Instance;
            if (state == null || traps == null)
                return "";
            if (state.condition < 0.7f && traps.CanRepair(state))
                return $"Repair  (1 {ItemDatabase.Get(TrapManager.RepairItem(state.type))?.DisplayName})";
            return $"Pick Up {Name}";
        }
    }

    public void SecondaryInteract(PlayerController player)
    {
        TrapManager traps = TrapManager.Instance;
        if (state == null || traps == null)
            return;

        if (state.condition < 0.7f && traps.Repair(state))
            ToolStatus.Flash($"Repaired the {Name}");
        else if (traps.PickUp(state))
            ToolStatus.Flash($"Picked up the {Name}");
        else
            ToolStatus.Flash("No room to carry it");
    }
}
