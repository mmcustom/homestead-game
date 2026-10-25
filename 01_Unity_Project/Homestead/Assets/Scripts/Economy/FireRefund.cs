using System;

// What comes back when a plain Campfire is dismantled (Hammer, fire out), pure and engine-free so edit-mode tests cover it. Half of the
// build materials back, rounded down (3 Firewood gives 1; 6 Sticks gives 3), the unspent fuel as Firewood the way the hearth refund does,
// and the whole Ash waiting in the ring (the fraction of an Ash is lost). The Firewood from the build and from the fuel are one amount.
public struct FireRefundResult
{
    public int firewood;
    public int sticks;
    public int ash;

    public bool IsEmpty => firewood <= 0 && sticks <= 0 && ash <= 0;
}

public static class FireRefund
{
    // A whole amount of Firewood from the hours of fuel left, with the slack a sum of small burns needs.
    public const float HoursTolerance = 0.0001f;

    // Half of what was paid, rounded down: a single piece refunds nothing.
    public static int HalfBack(int paid) => paid > 1 ? paid / 2 : 0;

    // The Firewood the unspent fuel is worth. 0 for no fuel or a fuel that gives no hours.
    public static int FuelToFirewood(float fuelHours, float hoursPerFirewood)
    {
        if (fuelHours <= 0f || hoursPerFirewood <= 0f)
            return 0;
        return (int)Math.Floor(fuelHours / hoursPerFirewood + HoursTolerance);
    }

    public static FireRefundResult Of(bool builtWithSticks, int firewoodToBuild, int sticksToBuild, float fuelHours, float hoursPerFirewood, float ashMade)
    {
        return new FireRefundResult
        {
            firewood = (builtWithSticks ? 0 : HalfBack(firewoodToBuild)) + FuelToFirewood(fuelHours, hoursPerFirewood),
            sticks = builtWithSticks ? HalfBack(sticksToBuild) : 0,
            ash = AshRules.Whole(ashMade),
        };
    }
}
