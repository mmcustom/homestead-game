using System;

// Vehicles_and_Hauling.md, Step 1, Phase 1 (Mike, 2026-10-30): the Hand Cart as a parked, loaded object. Pure and engine-free so edit-mode tests
// cover it. A Wooden Wheel is a Craft recipe; the cart is a staged site (the wheels, Logs, Branches and Cordage are delivered over trips and the
// Hammer finishes it, like the Leach Hopper) that becomes a pile holding up to CapacityKg of bulk goods. Pushing it is Phase 2 and is not here.
//
// Every number is a named constant. Nothing here touches the pack, a pile, a fire or the clock: weights and counts come in as plain numbers.
public static class CartRules
{
    // --- The Wooden Wheel (Craft tab): 1 Log and 1 Cordage, instant, no tool. The item asset holds its 3 kg; WheelWeightKg is the figure it must match. ---

    public const string WheelId = "wooden_wheel";
    public const string LogsId = "logs";
    public const string BranchesId = "branches";
    public const string CordageId = "cordage";
    public const string FirewoodId = "firewood";
    public const string StoneId = "stone";
    public const string ClayId = "clay";
    public const string TallGrassId = "tall_grass";

    public const int WheelLogs = 1;
    public const int WheelCordage = 1;
    public const float WheelWeightKg = 3f;

    // --- The Hand Cart (Build tab, staged site): what the finished cart takes in total, delivered over trips, and the blows that finish it ---

    public const int CartWheels = 2;
    public const int CartLogs = 3;
    public const int CartBranches = 6;
    public const int CartCordage = 3;
    public const int BuildBlows = 12;

    // --- The load ---

    // The most a cart holds, in kilograms of what it carries (not counted against the pack's Encumbered line).
    public const float CapacityKg = 100f;

    // What a parked cart or its site keeps clear of other piles (the 2 m building spacing does not apply to it, so it can stand beside a site).
    public const float SpacingMetres = 1.2f;

    // Bulk building goods only: never food, water or tools.
    static readonly string[] Carried = { LogsId, BranchesId, FirewoodId, StoneId, ClayId, TallGrassId };

    public static bool Carries(string itemId) => Array.IndexOf(Carried, itemId) >= 0;

    // Shown on the transfer screen when nothing carried fits, and as the refusal for an item the cart does not take.
    public const string HoldsText = "A Hand Cart holds Logs, Branches, Firewood, Stone, Clay and Tall Grass, up to 100 kg - not food, water or tools.";

    // The one-line version for the transfer screen's status line.
    public const string HoldsShort = "Holds up to 100 kg of Logs, Branches, Firewood, Stone, Clay and Tall Grass.";

    public static string DoesntHold(string itemName) => $"the Hand Cart doesn't take {itemName} (Logs, Branches, Firewood, Stone, Clay and Tall Grass only)";

    // How many whole units of unitKg still fit in a cart already holding loadKg. A tiny tolerance keeps an exact fit (12 Logs = 96 kg, then 4 Branches)
    // from being lost to float error. A weightless or unknown item is limited by nothing but the caller's own count (int.MaxValue).
    public static int UnitsThatFit(float loadKg, float unitKg)
    {
        float room = RoomKg(loadKg);
        if (unitKg <= 0f)
            return room > 0f ? int.MaxValue : 0;
        return (int)Math.Floor((room + 0.0001f) / unitKg);
    }

    public static float RoomKg(float loadKg) => Math.Max(0f, CapacityKg - Math.Max(0f, loadKg));

    public static bool IsFull(float loadKg) => RoomKg(loadKg) < 0.0001f;

    // 0 for an empty cart, 1 for a full one (Phase 2's push speed reads this).
    public static float LoadFraction(float loadKg) => Math.Min(1f, Math.Max(0f, loadKg) / CapacityKg);

    // "12.5 / 100 kg" for the transfer screen's structure panel.
    public static string KgText(float loadKg) => $"{Math.Max(0f, loadKg):0.#} / {CapacityKg:0} kg";

    // Why a row can't take more: the cart is full, or the next one would not fit.
    public static string NoRoomReason(float loadKg) =>
        IsFull(loadKg) ? $"the cart is full ({KgText(loadKg)})" : $"won't fit - only {RoomKg(loadKg):0.#} kg of room left ({KgText(loadKg)})";

    // The 100 kg cap in the unit people think in: how many of an item a full, empty cart takes.
    public static int FullCartOf(float unitKg) => UnitsThatFit(0f, unitKg);

    // What dismantling gives back: half of each material, rounded down (the same rule as the other storage piles). The contents come back whole.
    public static int RefundOf(int cost) => Math.Max(0, cost) / 2;

    // =====================================================================================================================================
    // Phase 2 (Mike, 2026-10-31): grabbing the cart and pushing it. The follower, the sweeps and the controller hooks are CartHandler's; what is
    // pure lives here. Every number is a default that CartHandler exposes as an inspector field, so tuning never touches this file.
    // =====================================================================================================================================

    // --- Speed: a full cart pushes at three quarters of walking pace, an empty one at the full pace ---

    public const float EmptyPushSpeedFactor = 1f;
    public const float FullPushSpeedFactor = 0.75f;

    // 1 for an empty cart down to 0.75 for a full one, in a straight line between (so a lighter load is always faster).
    public static float PushSpeedFactor(float loadFraction) =>
        EmptyPushSpeedFactor + (FullPushSpeedFactor - EmptyPushSpeedFactor) * Clamp01(loadFraction);

    // The speed with the cart: the speed the player would have without it (walking, already slowed by the pack's encumbrance) times the load's factor.
    public static float PushSpeed(float speedWithoutCart, float loadFraction) => speedWithoutCart * PushSpeedFactor(loadFraction);

    // --- Where the cart goes: a follower pivoting on the player ---

    // The cart's centre is this far ahead of the player along the cart's own facing; the handles end about 0.45 m in front of the player.
    public const float FollowMetres = 1.7f;

    // How fast the cart swings round to the way the player faces, in degrees a second.
    public const float TurnDegreesPerSecond = 90f;

    // How far from the handles the player can grab (the interaction ray's own range also applies).
    public const float GrabReachMetres = 2.2f;

    public static bool CanReach(float distanceToHandles) => distanceToHandles >= 0f && distanceToHandles <= GrabReachMetres;

    // The cart's centre on the ground for a player at (playerX, playerZ) and a cart facing yawDegrees (0 is +Z, 90 is +X, as in Unity).
    public static void CartCentreAhead(float playerX, float playerZ, float yawDegrees, out float x, out float z, float followMetres = FollowMetres)
    {
        double yaw = yawDegrees * Math.PI / 180.0;
        x = playerX + (float)Math.Sin(yaw) * followMetres;
        z = playerZ + (float)Math.Cos(yaw) * followMetres;
    }

    // The shortest signed turn from one heading to another, -180 to 180 degrees.
    public static float DeltaAngle(float fromDegrees, float toDegrees)
    {
        float delta = (toDegrees - fromDegrees) % 360f;
        if (delta > 180f) delta -= 360f;
        if (delta < -180f) delta += 360f;
        return delta;
    }

    // One frame of swinging: the cart turns toward the player's heading by at most maxDegreesPerSecond * dt, the short way round.
    public static float NextYaw(float cartYaw, float playerYaw, float dt, float maxDegreesPerSecond = TurnDegreesPerSecond)
    {
        float step = Math.Max(0f, maxDegreesPerSecond) * Math.Max(0f, dt);
        float delta = DeltaAngle(cartYaw, playerYaw);
        float turn = Math.Max(-step, Math.Min(step, delta));
        float next = (cartYaw + turn) % 360f;
        return next < 0f ? next + 360f : next;
    }

    // The ground points the cart covers, for the tree test: 3 across (-0.6, 0, 0.6 m) and 5 along (-1.1 handles to 0.85 front), turned to yawDegrees
    // about the cart's centre. Fills xs and zs (15 each) and returns how many.
    public const int SampleCount = 15;
    static readonly float[] AcrossMetres = { -0.6f, 0f, 0.6f };
    static readonly float[] AlongMetres = { -1.1f, -0.55f, 0f, 0.45f, 0.85f };

    public static int FootprintSamples(float centreX, float centreZ, float yawDegrees, float[] xs, float[] zs)
    {
        double yaw = yawDegrees * Math.PI / 180.0;
        float sin = (float)Math.Sin(yaw), cos = (float)Math.Cos(yaw);
        int n = 0;
        foreach (float along in AlongMetres)
        {
            foreach (float across in AcrossMetres)
            {
                // forward is (sin, cos); right of it is (cos, -sin)
                xs[n] = centreX + along * sin + across * cos;
                zs[n] = centreZ + along * cos - across * sin;
                n++;
            }
        }
        return n;
    }

    // --- Slope: only uphill is limited, and a load makes the limit lower ---

    public const float EmptySlopeDegrees = 25f;
    public const float LoadedSlopeDegrees = 18f;

    public static float MaxPushSlopeDegrees(float loadFraction, float emptyDegrees = EmptySlopeDegrees, float loadedDegrees = LoadedSlopeDegrees) =>
        emptyDegrees + (loadedDegrees - emptyDegrees) * Clamp01(loadFraction);

    // Rise over run, in metres, along the way the cart is going (rise above zero is uphill). Downhill and level ground never block.
    public static bool SlopeBlocksPush(float riseMetres, float runMetres, float loadFraction, float emptyDegrees = EmptySlopeDegrees, float loadedDegrees = LoadedSlopeDegrees)
    {
        if (riseMetres <= 0f || runMetres <= 0f)
            return false;
        double degrees = Math.Atan2(riseMetres, runMetres) * 180.0 / Math.PI;
        return degrees > MaxPushSlopeDegrees(loadFraction, emptyDegrees, loadedDegrees);
    }

    // --- Water: the wheels keep out of anything deeper than an ankle ---

    public const float WaterLimitMetres = 0.25f;

    public static bool WaterBlocksPush(float depthMetres, float limitMetres = WaterLimitMetres) => depthMetres > limitMetres;

    // A step into deeper water is refused, but a step that does not make the deepest wheel any deeper is allowed, so a cart already standing at the
    // edge can always be backed out.
    public static bool WaterStepBlocked(float depthNow, float depthAfter, float limitMetres = WaterLimitMetres) =>
        WaterBlocksPush(depthAfter, limitMetres) && depthAfter > depthNow;

    // --- The stuck message waits this long, so a brush against a tree is not nagged about ---

    public const float StuckMessageSeconds = 0.6f;

    // A seconds-so-far counter that grows while the player is trying to move and the cart is not letting them, and resets when they are free.
    public static float StuckTime(float stuckSoFar, bool wantsToMove, bool isMoving, float dt) =>
        wantsToMove && !isMoving ? stuckSoFar + Math.Max(0f, dt) : 0f;

    public static bool ShowStuckMessage(float stuckSoFar) => stuckSoFar >= StuckMessageSeconds;

    // --- The words (never an [E] in them: the prompt HUD adds the key itself) ---

    // At the handles, before grabbing.
    public static string GrabPrompt(float loadKg) => $"Push Hand Cart  ({KgText(loadKg)})";

    // The status line while pushing.
    public static string PushingLine(float loadKg) => $"Pushing the Hand Cart  ({KgText(loadKg)})  -  E to park";

    public const string ParkedText = "Parked the Hand Cart.";
    public const string LetGoText = "You let go of the cart.";
    public const string NoRoomToGrabText = "No room to pull the cart here.";
    public const string StandUpText = "Stand up to push the cart.";
    public const string StuckText = "The cart is stuck - back up or turn.";
    public const string TooSteepText = "Too steep to push the cart.";
    public const string TooDeepText = "Too deep to push the cart through.";
    public const string FloodText = "The cart is in the flood zone - this ground floods. Park it higher up.";

    static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
}
