using System;

// The Cooking Pot redesign (Mike, 2026-10-27): the Pot stops being a carried requirement and HANGS on a Tripod that stands over a fire. The
// Potash Kettle is a second vessel for the same tripod. Pure and engine-free so edit-mode tests cover it; FireManager (the saved state and the
// actions), Tripod (the view and its prompts), Cooking and CampfireCooking (the boil gate) call these. Nothing here touches a fire, the pack
// or the clock: what is hung, what is carried and whether the fire is lit come in as plain values.
//
// One tripod per fire, one vessel on it at a time. The vessel is a requirement, not a capacity: boiling still counts litres and batches the
// way it did, and the tripod holds no liquid.
public static class TripodRules
{
    public const string TripodId = "tripod";
    public const string PotId = "cooking_pot";
    public const string KettleId = "potash_kettle";

    // The craft recipe (Crafting.cs): three poles lashed with cordage. A tripod is placed, not used up, so it comes back whole.
    public const int Branches = 3, Cordage = 1;

    // The Build tab's ghost snaps to the nearest fire this close (metres) to where the player aims.
    public const float SnapMetres = 3f;

    public static bool IsVessel(string itemId) => itemId == PotId || itemId == KettleId;

    // --- Setting a tripod up on a fire ---

    public enum SetUpResult { Ok, GrillFire, AlreadyHasTripod, NoTripod }

    // Checked in this order: the Grill's firebox is too small for one, a fire holds one tripod, and the item must be carried.
    public static SetUpResult CanSetUp(bool hasTripodItem, bool fireHasTripod, bool isGrillFire)
    {
        if (isGrillFire)
            return SetUpResult.GrillFire;
        if (fireHasTripod)
            return SetUpResult.AlreadyHasTripod;
        if (!hasTripodItem)
            return SetUpResult.NoTripod;
        return SetUpResult.Ok;
    }

    public static string SetUpMessage(SetUpResult result)
    {
        switch (result)
        {
            case SetUpResult.GrillFire: return "The Grill's firebox is too small for a tripod.";
            case SetUpResult.AlreadyHasTripod: return "This fire already has a tripod.";
            case SetUpResult.NoTripod: return "You need a Tripod (3 Branches and 1 Cordage).";
            default: return "";
        }
    }

    // --- E on the tripod: hang, swap or take down ---

    public enum StepKind { None, Hang, Swap, TakeDown }

    public struct Step
    {
        public StepKind kind;
        public string hang;      // the vessel that goes on (Hang, Swap)
        public string takeDown;  // the vessel that comes off (Swap, TakeDown)
    }

    // What E does. Nothing hung: hang the Pot if one is carried, else the Kettle. The Pot hung and a Kettle carried (or the other way round):
    // swap them. Otherwise the vessel comes down. An id that is not a known vessel (a bad save) just comes down.
    public static Step NextStep(string hung, bool carriesPot, bool carriesKettle)
    {
        if (string.IsNullOrEmpty(hung))
        {
            if (carriesPot)
                return new Step { kind = StepKind.Hang, hang = PotId };
            if (carriesKettle)
                return new Step { kind = StepKind.Hang, hang = KettleId };
            return new Step { kind = StepKind.None };
        }

        if (hung == PotId && carriesKettle)
            return new Step { kind = StepKind.Swap, hang = KettleId, takeDown = PotId };
        if (hung == KettleId && carriesPot)
            return new Step { kind = StepKind.Swap, hang = PotId, takeDown = KettleId };
        return new Step { kind = StepKind.TakeDown, takeDown = hung };
    }

    // The look prompt: nameOf turns an item id into its display name.
    public static string StepPrompt(Step step, Func<string, string> nameOf)
    {
        switch (step.kind)
        {
            case StepKind.Hang: return $"Hang the {nameOf(step.hang)} on the tripod";
            case StepKind.Swap: return $"Swap the {nameOf(step.takeDown)} for the {nameOf(step.hang)}";
            case StepKind.TakeDown: return $"Take the {nameOf(step.takeDown)} down";
            default: return "Tripod  (empty, carry a Cooking Pot to hang on it)";
        }
    }

    // The short message after the step is done.
    public static string StepDone(Step step, Func<string, string> nameOf)
    {
        switch (step.kind)
        {
            case StepKind.Hang: return $"Hung the {nameOf(step.hang)} on the tripod.";
            case StepKind.Swap: return $"Took the {nameOf(step.takeDown)} down and hung the {nameOf(step.hang)}.";
            case StepKind.TakeDown: return $"Took the {nameOf(step.takeDown)} down.";
            default: return "You have no Cooking Pot to hang on the tripod.";
        }
    }

    // --- Dismantling the tripod (R with the Hammer, fire out) ---

    public enum DismantleResult { Ok, FireLit, NoHammer }

    public static DismantleResult CanDismantle(bool fireLit, bool hammerEquipped)
    {
        if (fireLit)
            return DismantleResult.FireLit;
        if (!hammerEquipped)
            return DismantleResult.NoHammer;
        return DismantleResult.Ok;
    }

    public static string DismantleMessage(DismantleResult result)
    {
        switch (result)
        {
            case DismantleResult.FireLit: return "Put the fire out first.";
            case DismantleResult.NoHammer: return "Equip a Hammer to dismantle.";
            default: return "";
        }
    }

    // --- Boiling: is the vessel a job needs hung at a lit fire? ---

    public enum Gate { Ok, NoFire, NoTripod, NoVessel, WrongVessel }

    // The nearest lit fire the player is standing by: whether it has a tripod, and what hangs on it. needed is the vessel the job uses.
    public static Gate BoilGate(bool litFireInReach, bool fireHasTripod, string hung, string needed)
    {
        if (!litFireInReach)
            return Gate.NoFire;
        if (!fireHasTripod)
            return Gate.NoTripod;
        if (string.IsNullOrEmpty(hung))
            return Gate.NoVessel;
        if (hung != needed)
            return Gate.WrongVessel;
        return Gate.Ok;
    }

    // The flash when a boil can't start: doing is "Boiling water" or "Boiling sap", vessel is the vessel's display name.
    public static string GateMessage(Gate gate, string doing, string vessel)
    {
        switch (gate)
        {
            case Gate.NoFire: return $"{doing} needs a burning campfire.";
            case Gate.NoTripod: return $"{doing} needs a tripod with the {vessel} hung on it.";
            case Gate.NoVessel: return $"{doing} needs the {vessel} hung on the tripod.";
            case Gate.WrongVessel: return $"{doing} needs the {vessel} hung on the tripod (another vessel is on it).";
            default: return "";
        }
    }

    // The Inventory row's note while a boil can't start.
    public static string RowInfo(Gate gate, string vessel)
    {
        switch (gate)
        {
            case Gate.NoFire: return "needs a burning campfire";
            case Gate.NoTripod: return $"needs a tripod with the {vessel} hung";
            case Gate.NoVessel: return $"hang the {vessel} on the tripod";
            case Gate.WrongVessel: return $"hang the {vessel} (another vessel is on the tripod)";
            default: return "";
        }
    }

    // The HUD line while a queued boil waits for its vessel (verb is "Boiling").
    public static string PausedMessage(string verb, string vessel) => $"{verb} paused — hang the {vessel} on a tripod at this fire";
}
