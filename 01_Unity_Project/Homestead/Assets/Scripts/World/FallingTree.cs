using UnityEngine;

// A felled tree going over: a copy of its terrain tree (same shape, size, turn and season's leaves) that tips away
// from the axe, slowly at first and then fast, settles on the ground, lies there a moment, then sinks away — its wood
// is in the pile by then. Hardwoods creak and crash (sfx_tree_fall): the sound starts as the tree begins to go, cued so
// its crash lands as the tree hits the ground. Looks only: no collider, nothing saved.
public class FallingTree : MonoBehaviour
{
    Vector3 axis;
    Vector3 pivot;
    Quaternion upright;
    float fallSeconds, lieSeconds, elapsed;
    float maxAngle;

    const float SinkSeconds = 1.5f, SinkDepth = 1.5f;

    public static void Spawn(GameObject prefab, Vector3 basePosition, TreeInstance instance, Vector3 away, float fallSeconds, float lieSeconds,
                             bool crash = true)
    {
        var go = Instantiate(prefab, basePosition, Quaternion.Euler(0f, instance.rotation * Mathf.Rad2Deg, 0f));
        go.name = "Falling Tree";
        go.transform.localScale = new Vector3(instance.widthScale, instance.heightScale, instance.widthScale);
        foreach (Collider c in go.GetComponentsInChildren<Collider>())
            Destroy(c);
        foreach (LODGroup lod in go.GetComponentsInChildren<LODGroup>())
            Destroy(lod);

        var falling = go.AddComponent<FallingTree>();
        falling.pivot = basePosition;
        falling.upright = go.transform.rotation;
        falling.axis = Vector3.Cross(Vector3.up, away).normalized;
        falling.fallSeconds = fallSeconds;
        falling.lieSeconds = lieSeconds;
        falling.maxAngle = 86f; // resting on its branches, not quite flat
        if (crash && AudioManager.Instance != null)
        {
            // Heard from where the trunk comes down, partway along the fall line.
            Vector3 at = basePosition + away * (instance.heightScale * 3f) + Vector3.up * 0.5f;
            AudioManager.Instance.PlayTreeFall(at, fallSeconds);
        }
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        // Accelerating fall, with a small bounce as it lands.
        float t = Mathf.Clamp01(elapsed / fallSeconds);
        float angle = maxAngle * t * t;
        if (elapsed > fallSeconds)
        {
            float after = elapsed - fallSeconds;
            angle = maxAngle - 4f * Mathf.Exp(-after * 6f) * Mathf.Abs(Mathf.Sin(after * 14f));
        }

        Quaternion tilt = Quaternion.AngleAxis(angle, axis);
        transform.rotation = tilt * upright;
        float sink = Mathf.Clamp01((elapsed - fallSeconds - lieSeconds) / SinkSeconds) * SinkDepth;
        transform.position = pivot + Vector3.down * sink;

        if (elapsed > fallSeconds + lieSeconds + SinkSeconds)
            Destroy(gameObject);
    }
}
