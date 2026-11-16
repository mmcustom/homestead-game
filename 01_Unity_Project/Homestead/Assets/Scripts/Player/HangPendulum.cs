using UnityEngine;

// Hanging items in hand (Mike, 2026-11-14): a bucket, pot, canteen, pouch or trap hangs by GRAVITY from the hand, not straight down in the view, so looking up or down
// does not tilt it, and it swings a little when the hand moves or the view turns quickly, then settles. A simple damped pendulum in world space: the direction from the
// grip to the body is pulled toward world-down by gravity and pushed by the hand's own acceleration (inertia), with damping so it comes to rest. Pure maths, no Unity objects,
// so the tests can step it. PlayerHands feeds it the grip's world position each frame and turns the item to the direction it returns.
public class HangPendulum
{
    public const float Gravity = 9.81f;
    public const float Length = 0.15f;        // metres from the grip to the body's centre (a bucket's bail): sets how fast it swings (about 1.3 swings a second)
    public const float Damping = 4.5f;        // per second: a fast look settles in about a second
    public const float Inertia = 1f;          // how much the hand's acceleration throws the body (1 = a real pendulum)
    public const float MaxAccel = 40f;        // m/s2: clamps teleports and frame spikes so they cannot fling it
    public const float MaxAngle = 45f;        // degrees from straight down it may swing

    Vector3 dir = Vector3.down;               // world direction from the grip to the body
    Vector3 vel;                              // rate of change of dir
    Vector3 lastPivot, lastPivotVel;
    bool started;

    public Vector3 Direction => dir;

    // Start again hanging straight down (a fresh equip, or after a jump such as loading or teleporting).
    public void Reset()
    {
        dir = Vector3.down;
        vel = Vector3.zero;
        lastPivotVel = Vector3.zero;
        started = false;
    }

    // One step: pivot is the grip's world position this frame. Returns the hanging direction (unit, world). dt of zero (paused) changes nothing.
    public Vector3 Step(Vector3 pivot, float dt)
    {
        if (!started)
        {
            lastPivot = pivot;
            lastPivotVel = Vector3.zero;
            started = true;
            return dir;
        }
        if (dt <= 0f)
            return dir;

        Vector3 pivotVel = (pivot - lastPivot) / dt;
        Vector3 pivotAcc = Vector3.ClampMagnitude((pivotVel - lastPivotVel) / dt, MaxAccel);
        lastPivot = pivot;
        lastPivotVel = pivotVel;

        // Gravity pulls the body down; the hand accelerating one way throws the body the other way.
        Vector3 pull = Vector3.down * Gravity - pivotAcc * Inertia;
        Vector3 tangent = pull - Vector3.Dot(pull, dir) * dir;
        vel += tangent / Length * dt;
        vel -= vel * Mathf.Min(1f, Damping * dt);
        vel -= Vector3.Dot(vel, dir) * dir;
        dir = (dir + vel * dt).normalized;

        // Never past MaxAngle from straight down.
        float angle = Vector3.Angle(Vector3.down, dir);
        if (angle > MaxAngle)
        {
            dir = Vector3.Slerp(Vector3.down, dir, MaxAngle / angle).normalized;
            vel -= Vector3.Dot(vel, dir) * dir;
            vel *= 0.5f;
        }
        return dir;
    }
}
