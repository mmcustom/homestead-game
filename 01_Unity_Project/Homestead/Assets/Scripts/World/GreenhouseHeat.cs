using System.Collections.Generic;
using UnityEngine;

// A greenhouse's Wood Stove, Propane Heater and Water Barrels (Greenhouses.md, slice G4; GreenhouseHeatRules). A PlacedStructure of one of these kinds hands its
// look, prompts, E and R to this class. GardenBeds' game hour calls Hour (before the beds), which decides whether each stove or heater burns this hour and takes
// its fuel; GardenBeds.FrameInside calls Apply, which adds the barrels' and a burning heater's warmth to the house's inside temperature.
// Saved on the pile: lit or on in cropFlags (On), burning this hour (Burning); the Firewood or full Propane Tanks it holds in contents; for the heater, the hours
// of full output left in the tank fitted, in jobHours.
public static class GreenhouseHeat
{
    public const int On = 1, Burning = 2;
    static InventoryManager Inventory => InventoryManager.Instance;

    static bool IsOn(WoodPileState s) => (s.cropFlags & On) != 0;
    static bool IsBurning(WoodPileState s) => (s.cropFlags & Burning) != 0;
    static int SizeOf(WoodPileState house) => WoodManager.HouseSizeIndex(house.kind);

    // --- The warmth ---

    // The barrels' and a burning heater's warmth, added to a house's inside temperature.
    public static float Apply(WoodPileState house, float insideC, float sun01)
    {
        WoodManager wood = WoodManager.Instance;
        if (wood == null || !WoodManager.IsWalkIn(house.kind) || (WoodManager.IsHoopHouse(house.kind) && WoodManager.HoopIsTorn(house)))
            return insideC;
        List<WoodPileState> heat = wood.HeatersIn(house);
        foreach (WoodPileState pile in heat)
            if (pile.kind == PileKind.WaterBarrels)
                insideC += GreenhouseHeatRules.BarrelEffectC(sun01);
        foreach (WoodPileState pile in heat)
        {
            if (!IsBurning(pile))
                continue;
            if (pile.kind == PileKind.GreenhouseStove)
                insideC += GreenhouseHeatRules.StoveGainC(SizeOf(house));
            else if (pile.kind == PileKind.PropaneHeater)
                insideC += GreenhouseHeatRules.PropaneGainC(insideC, SizeOf(house));
        }
        return insideC;
    }

    // Tonight's low inside, for the frost alert: the barrels at night, and a stove or heater that is on with fuel doing what it can.
    public static float NightEstimate(WoodPileState cover, float insideC)
    {
        WoodManager wood = WoodManager.Instance;
        if (wood == null || !WoodManager.IsWalkIn(cover.kind))
            return insideC;
        int size = SizeOf(cover);
        List<WoodPileState> heat = wood.HeatersIn(cover);
        foreach (WoodPileState pile in heat)
            if (pile.kind == PileKind.WaterBarrels)
                insideC += GreenhouseHeatRules.BarrelNightC;
        foreach (WoodPileState pile in heat)
        {
            if (!IsOn(pile))
                continue;
            if (pile.kind == PileKind.GreenhouseStove && pile.Count(GreenhouseHeatRules.FirewoodId) > 0 && insideC < GreenhouseHeatRules.StoveTargetC)
                insideC += GreenhouseHeatRules.StoveGainC(size);
            else if (pile.kind == PileKind.PropaneHeater && (pile.jobHours > 0f || pile.Count(GreenhouseHeatRules.TankId) > 0))
                insideC += GreenhouseHeatRules.PropaneGainC(insideC, size);
        }
        return insideC;
    }

    // The hour: each stove or heater looks at the house without its own heat and burns if it is needed.
    public static void Hour(WoodManager wood, float airC)
    {
        bool stoveOut = false, propaneOut = false;
        foreach (WoodPileState pile in new List<WoodPileState>(wood.Piles))
        {
            if (pile.kind != PileKind.GreenhouseStove && pile.kind != PileKind.PropaneHeater)
                continue;
            bool wasBurning = IsBurning(pile);
            pile.cropFlags &= ~Burning;
            WoodPileState house = wood.HouseAt(pile.position);
            if (house == null || !IsOn(pile))
            {
                if (wasBurning)
                    wood.NotifyChanged(pile);
                continue;
            }
            float inside = GardenBeds.FrameInside(house, airC); // the barrels, but not this heater
            int size = SizeOf(house);
            if (pile.kind == PileKind.GreenhouseStove)
            {
                int wood_ = pile.Count(GreenhouseHeatRules.FirewoodId);
                if (GreenhouseHeatRules.StoveBurns(true, wood_, inside))
                {
                    pile.Take(GreenhouseHeatRules.FirewoodId, 1);
                    pile.cropFlags |= Burning;
                }
                else if (wood_ <= 0)
                {
                    pile.cropFlags &= ~On;
                    stoveOut = true;
                }
            }
            else
            {
                float burn = GreenhouseHeatRules.PropaneBurn(inside, size);
                if (burn > 0f)
                {
                    if (pile.jobHours <= 0f && pile.Count(GreenhouseHeatRules.TankId) > 0)
                    {
                        pile.Take(GreenhouseHeatRules.TankId, 1);
                        pile.jobHours += GreenhouseHeatRules.TankHours(size);
                    }
                    if (pile.jobHours > 0f)
                    {
                        pile.jobHours = Mathf.Max(0f, pile.jobHours - burn);
                        pile.cropFlags |= Burning;
                    }
                    else
                    {
                        pile.cropFlags &= ~On;
                        propaneOut = true;
                    }
                }
            }
            if (IsBurning(pile) != wasBurning || !IsOn(pile) || pile.kind == PileKind.GreenhouseStove)
                wood.NotifyChanged(pile);
            // the house's own temperature now includes this heat (GardenRules works it out again for the beds)
            house.jobHours = 0f;
        }
        if (stoveOut)
            ToolStatus.Banner(GreenhouseHeatRules.StoveOutBanner, 8f);
        if (propaneOut)
            ToolStatus.Banner(GreenhouseHeatRules.PropaneOutBanner, 8f);
    }

    // --- Prompts ---

    static int Carried(string itemId) => Inventory != null ? Inventory.Player.Count(itemId) : 0;

    public static string Header(WoodPileState s)
    {
        switch (s.kind)
        {
            case PileKind.GreenhouseStove: return GreenhouseHeatRules.StoveHeader(IsOn(s), IsBurning(s), s.Count(GreenhouseHeatRules.FirewoodId));
            case PileKind.PropaneHeater: return GreenhouseHeatRules.HeaterHeader(IsOn(s), IsBurning(s), s.Count(GreenhouseHeatRules.TankId), s.jobHours);
            default: return GreenhouseHeatRules.BarrelsHeader;
        }
    }

    public static string Prompt(WoodPileState s)
    {
        switch (s.kind)
        {
            case PileKind.GreenhouseStove: return GreenhouseHeatRules.StovePrompt(IsOn(s), s.Count(GreenhouseHeatRules.FirewoodId));
            case PileKind.PropaneHeater: return GreenhouseHeatRules.HeaterPrompt(IsOn(s), s.jobHours > 0f || s.Count(GreenhouseHeatRules.TankId) > 0);
            default: return null;
        }
    }

    public static string SecondaryPrompt(WoodPileState s)
    {
        if (WoodManager.HammerEquipped)
            return $"Take the {WoodManager.PileName(s.kind)} out  (half its materials back, and its fuel)";
        switch (s.kind)
        {
            case PileKind.GreenhouseStove: return GreenhouseHeatRules.StoveLoadPrompt(Carried(GreenhouseHeatRules.FirewoodId), s.Count(GreenhouseHeatRules.FirewoodId));
            case PileKind.PropaneHeater: return GreenhouseHeatRules.HeaterLoadPrompt(Carried(GreenhouseHeatRules.TankId), s.Count(GreenhouseHeatRules.TankId));
            default: return "";
        }
    }

    // --- E and R ---

    public static void Interact(WoodPileState s)
    {
        WoodManager wood = WoodManager.Instance;
        if (wood == null || s.kind == PileKind.WaterBarrels)
            return;
        bool fuel = s.kind == PileKind.GreenhouseStove ? s.Count(GreenhouseHeatRules.FirewoodId) > 0 : s.jobHours > 0f || s.Count(GreenhouseHeatRules.TankId) > 0;
        if (!IsOn(s) && !fuel)
        {
            ToolStatus.Flash(Prompt(s), 2.5f);
            return;
        }
        s.cropFlags ^= On;
        if (!IsOn(s))
            s.cropFlags &= ~Burning;
        WoodPileState house = wood.HouseAt(s.position);
        if (house != null)
            house.jobHours = 0f;
        wood.NotifyChanged(s);
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
        ToolStatus.Flash(s.kind == PileKind.GreenhouseStove
            ? (IsOn(s) ? $"Lit the stove - it burns while the greenhouse is below {GreenhouseHeatRules.StoveTargetC:0} C" : "Let the stove go out")
            : (IsOn(s) ? $"Turned the heater on - it keeps the greenhouse at {GreenhouseHeatRules.PropaneHoldC:0} C or above" : "Turned the heater off"), 3f);
    }

    // R without the Hammer: load the fuel carried, as much as fits.
    public static void Secondary(WoodPileState s)
    {
        if (Inventory == null || WoodManager.Instance == null)
            return;
        string id = s.kind == PileKind.GreenhouseStove ? GreenhouseHeatRules.FirewoodId : s.kind == PileKind.PropaneHeater ? GreenhouseHeatRules.TankId : null;
        if (id == null)
            return;
        int cap = s.kind == PileKind.GreenhouseStove ? GreenhouseHeatRules.StoveFirewoodCap : GreenhouseHeatRules.TankCap;
        int n = Mathf.Min(Carried(id), cap - s.Count(id));
        if (n <= 0)
            return;
        int removed = Inventory.Player.Remove(id, n);
        s.Add(id, removed);
        WoodManager.Instance.NotifyChanged(s);
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
        ToolStatus.Flash($"Loaded {removed} {ItemDatabase.Get(id)?.DisplayName ?? id} ({s.Count(id)}/{cap})", 2.5f);
    }

    // --- The look ---

    public static void Build(Transform parent, WoodPileState state, List<GameObject> pieces, out Vector3 boxCenter, out Vector3 boxSize)
    {
        WoodManager wood = WoodManager.Instance;
        WoodPileState house = wood != null ? wood.HouseAt(state.position) : null;
        float roof = house != null ? (WoodManager.IsGlassHouse(house.kind) ? GlassHouseRules.RidgeHeight(WoodManager.GlassSizeOf(house.kind)) : HoopHouseRules.Height(WoodManager.HoopSizeOf(house.kind))) : 2.5f;
        if (state.kind == PileKind.WaterBarrels)
        {
            boxCenter = new Vector3(0f, 0.45f, 0f);
            boxSize = new Vector3(1.2f, 0.9f, 0.6f);
        }
        else
        {
            boxCenter = new Vector3(0f, 0.4f, 0f);
            boxSize = new Vector3(0.6f, 0.8f, 0.6f);
        }
        GameObject prefab = Resources.Load<GameObject>("Art/structure_" + state.kind.ToString().ToLowerInvariant());
        if (prefab != null)
        {
            GameObject art = Object.Instantiate(prefab, parent, false);
            art.name = prefab.name;
            foreach (Collider c in art.GetComponentsInChildren<Collider>())
                Object.DestroyImmediate(c);
            foreach (Transform t in art.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "State_Burning") t.gameObject.SetActive(IsBurning(state));
                // the stovepipe reaches the roof of the house it stands in
                // (it is saved hidden, so the Build tab's icon shows the stove itself)
                if (t.name == "Pipe")
                {
                    t.gameObject.SetActive(true);
                    t.localScale = new Vector3(1f, Mathf.Max(0.5f, roof - 0.75f) / 2f, 1f);
                }
            }
            pieces?.Add(art);
            return;
        }
        Color iron = new Color(0.16f, 0.16f, 0.17f), stoneGrey = new Color(0.5f, 0.49f, 0.46f), wooden = new Color(0.45f, 0.33f, 0.2f), steel = new Color(0.75f, 0.76f, 0.78f);
        switch (state.kind)
        {
            case PileKind.GreenhouseStove:
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.04f, 0f), new Vector3(0.7f, 0.08f, 0.7f)), stoneGrey);
                Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.38f, 0f), new Vector3(0.45f, 0.55f, 0.5f)), iron);
                Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.65f + (roof - 0.65f) / 2f, 0.12f), new Vector3(0.12f, (roof - 0.65f) / 2f, 0.12f)), iron);
                if (IsBurning(state))
                    Tinted(Piece(parent, pieces, PrimitiveType.Cube, new Vector3(0f, 0.3f, -0.255f), new Vector3(0.2f, 0.12f, 0.01f)), new Color(1f, 0.45f, 0.1f));
                break;
            case PileKind.PropaneHeater:
                Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.24f, 0f), new Vector3(0.3f, 0.24f, 0.3f)), steel);
                Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.56f, 0f), new Vector3(0.42f, 0.04f, 0.42f)), iron);
                if (IsBurning(state))
                    Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(0f, 0.6f, 0f), new Vector3(0.36f, 0.01f, 0.36f)), new Color(1f, 0.4f, 0.1f));
                break;
            default:
                foreach (float x in new[] { -0.3f, 0.3f })
                    Tinted(Piece(parent, pieces, PrimitiveType.Cylinder, new Vector3(x, 0.43f, 0f), new Vector3(0.55f, 0.43f, 0.55f)), wooden);
                break;
        }
    }

    static GameObject Piece(Transform parent, List<GameObject> pieces, PrimitiveType shape, Vector3 position, Vector3 scale)
    {
        GameObject piece = GameObject.CreatePrimitive(shape);
        Object.DestroyImmediate(piece.GetComponent<Collider>());
        piece.transform.SetParent(parent, false);
        piece.transform.localPosition = position;
        piece.transform.localScale = scale;
        pieces?.Add(piece);
        return piece;
    }

    static GameObject Tinted(GameObject piece, Color color)
    {
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        piece.GetComponent<MeshRenderer>().SetPropertyBlock(block);
        return piece;
    }
}
