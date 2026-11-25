using UnityEngine;

// The Sawmill's water wheel (Saw_Pit_Sawmill_and_Salt_Barrel.md 2). On WheelSide, the group the primitive mill (StructureVisuals.BuildSawmill) hangs the wheel from:
// when the mill appears it turns WheelSide, in quarter turns, so the wheel sits on the wall nearest the creek, whichever way the mill was placed; then it spins the
// wheel about its axle while the mill is sawing (it holds 2 Logs or more and the creek is not in flash flood), and leaves it still otherwise.
public class SawmillWheel : MonoBehaviour
{
    public const float DegreesPerSecond = 40f;

    WoodPileState state;
    Transform wheel;

    public void Bind(WoodPileState pile, Transform wheelTransform)
    {
        state = pile;
        wheel = wheelTransform;
    }

    void Start()
    {
        if (transform.parent == null)
            return;
        Vector3 local = transform.parent.InverseTransformDirection(WellsAndOuthouses.TowardCreek(transform.position));
        float yaw = Mathf.Atan2(-local.z, local.x) * Mathf.Rad2Deg;   // the local +X wall turned toward the creek
        transform.localRotation = Quaternion.Euler(0f, Mathf.Round(yaw / 90f) * 90f, 0f);
    }

    public static bool Turning(int logs, bool flashFlood) => SawmillRules.StatusOf(logs, flashFlood) == SawmillRules.Status.Running;

    void Update()
    {
        if (wheel == null || state == null)
            return;
        if (Turning(state.Count(SawmillRules.LogsId), StreamManager.State == StreamState.FlashFlood))
            wheel.Rotate(DegreesPerSecond * Time.deltaTime, 0f, 0f, Space.Self);
    }
}
