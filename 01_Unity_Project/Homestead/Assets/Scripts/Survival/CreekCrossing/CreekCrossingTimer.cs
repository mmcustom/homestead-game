using UnityEngine;

public enum CrossingMode { None, HighWater, FlashFlood }

// The numbers the timer needs, copied from the CreekCrossing component's public tunables each tick.
public struct CrossingTiming
{
    public float knockDownAfterSeconds;
    public float lockoutSeconds;
    public float immunitySeconds;
    public float leaveGraceSeconds;
}

// What one tick of the timer decided.
public struct CrossingTick
{
    public bool knockedDown;     // the player was just knocked off their feet
    public CrossingMode announce; // the warning to show now (None for nothing)
}

// Flash Floods phase 5: the transient clocks of a creek crossing, with no scene access so the tests can drive it. Nothing here
// is saved. In a flash flood, staying in the creek for knockDownAfterSeconds knocks the player down: input locked for
// lockoutSeconds, then immune to another knock-down for immunitySeconds so they can scramble out. Each stay in the creek
// announces each level once (a stay ends when the player has been out for leaveGraceSeconds, so a bank-edge flicker doesn't repeat it).
public class CrossingTimer
{
    float exposure;
    float lockLeft;
    float immuneLeft;
    float outTime;
    bool inStay;
    CrossingMode announced;

    public bool Locked => lockLeft > 0f;
    public bool Immune => immuneLeft > 0f;
    public float Exposure => exposure;
    public float LockLeft => lockLeft;
    public float ImmuneLeft => immuneLeft;

    // Respawn, load, new game, or any moment the crossing must not carry on.
    public void Reset()
    {
        exposure = lockLeft = immuneLeft = outTime = 0f;
        inStay = false;
        announced = CrossingMode.None;
    }

    public CrossingTick Step(float dt, CrossingMode mode, bool inCreek, CrossingTiming timing)
    {
        var result = new CrossingTick();
        dt = Mathf.Max(0f, dt);

        if (lockLeft > 0f)
        {
            lockLeft -= dt;
            if (lockLeft <= 0f)
            {
                lockLeft = 0f;
                immuneLeft = timing.immunitySeconds; // the scramble-out window starts when the player can move again
            }
        }
        else if (immuneLeft > 0f)
        {
            immuneLeft = Mathf.Max(0f, immuneLeft - dt);
        }

        if (inCreek)
        {
            outTime = 0f;
            inStay = true;
            if (mode > announced)
            {
                announced = mode;
                if (mode != CrossingMode.None)
                    result.announce = mode;
            }
        }
        else if (inStay)
        {
            outTime += dt;
            if (outTime >= timing.leaveGraceSeconds)
            {
                inStay = false;
                announced = CrossingMode.None;
                exposure = 0f;
            }
        }

        if (inCreek && mode == CrossingMode.FlashFlood && lockLeft <= 0f && immuneLeft <= 0f)
        {
            exposure += dt;
            if (exposure >= timing.knockDownAfterSeconds)
            {
                exposure = 0f;
                lockLeft = Mathf.Max(0.01f, timing.lockoutSeconds);
                result.knockedDown = true;
            }
        }
        else if (mode != CrossingMode.FlashFlood)
        {
            exposure = 0f;
        }

        return result;
    }
}

// Health and stamina arithmetic for a crossing, pure so it can be tested.
public static class CrossingMath
{
    // Health after an injury of `amount` with a floor: never below the floor, and unchanged if already at or below it.
    public static float HealthAfterInjury(float health, float amount, float floor)
    {
        if (amount <= 0f || health <= floor)
            return health;
        return Mathf.Max(floor, health - amount);
    }

    public static float StaminaAfterLoss(float stamina, float loss) => Mathf.Max(0f, stamina - Mathf.Max(0f, loss));
}
