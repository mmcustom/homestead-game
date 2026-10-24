// The all-or-nothing carry check behind the felled tree pile's take screen: a choice goes into the pack whole or not at all. Pure and
// engine-free so edit-mode tests cover it. The pack's only hard limit is its maximum carry weight (InventoryManager.MaxCarryWeightKg; the
// lower Encumbered line only slows the player, it never refuses a pickup), so that is the one rule checked here, with the same small
// tolerance InventoryContainer.SpaceFor uses so an exact fit is not refused by float error.
public static class CarryRules
{
    const float Tolerance = 0.0001f;

    // Whether addKg more fits in freeKg of room. Weightless things always fit.
    public static bool Fits(float freeKg, float addKg) => addKg <= 0f || addKg <= freeKg + Tolerance;

    // Why a choice was refused, naming the problem in plain words. what is the choice ("3 Logs", "everything in the pile"). Weights are
    // rounded the careful way (room down, load up) so the numbers never claim it should have fit.
    public static string Refusal(string what, float freeKg, float addKg, float maxKg)
    {
        double room = System.Math.Floor(freeKg * 10.0) / 10.0;
        if (room < 0.1)
            return $"No room: you are already carrying the most you can ({maxKg:0.#} kg). Nothing was taken.";
        double load = System.Math.Ceiling(addKg * 10.0) / 10.0;
        return $"Too heavy: {what} weighs {load:0.#} kg and you can carry only {room:0.#} kg more (limit {maxKg:0.#} kg). Nothing was taken.";
    }
}
