using UnityEngine;

// Art (tool_torch, 2026-11-14): the flame of a held light, drawn as a few upright cards that step through a flipbook (4 x 4 frames).
// Particle systems do not work on the Viewmodel layer: the main camera never sees that layer, so Unity stops simulating and redrawing them and the
// flame froze where it was lit. Mesh cards are drawn by ViewmodelPass like the hands, so this component animates them instead.
// Put it on an object under State_Lit (so it only shows while the light burns) with one MeshRenderer child per card; each card keeps its own
// frame phase, turns to face the camera while standing upright in the world, and flickers a little in height and width.
[DefaultExecutionOrder(1000)] // after PlayerHands has posed the tool, so the cards face the camera this frame
public class ViewFlame : MonoBehaviour
{
    public int columns = 4;
    public int rows = 4;
    public float framesPerSecond = 18f;
    [Range(0f, 0.4f)] public float flicker = 0.12f;

    static readonly int UnlitST = Shader.PropertyToID("_UnlitColorMap_ST");
    static readonly int EmissiveST = Shader.PropertyToID("_EmissiveColorMap_ST");

    Renderer[] cards;
    Vector3[] baseScale;
    float[] phase;
    MaterialPropertyBlock block;

    void Awake()
    {
        cards = GetComponentsInChildren<Renderer>(true);
        baseScale = new Vector3[cards.Length];
        phase = new float[cards.Length];
        float shared = Random.value * 3f;
        for (int i = 0; i < cards.Length; i++)
        {
            baseScale[i] = cards[i].transform.localScale;
            phase[i] = shared + i * (1.5f / Mathf.Max(1f, framesPerSecond)); // a frame or two apart, so the cards layer into one flame
        }
        block = new MaterialPropertyBlock();
    }

    void LateUpdate()
    {
        if (cards == null)
            return;
        Camera cam = Camera.main;
        int frames = Mathf.Max(1, columns * rows);
        float sx = 1f / Mathf.Max(1, columns), sy = 1f / Mathf.Max(1, rows);
        float t = Time.time;

        for (int i = 0; i < cards.Length; i++)
        {
            Renderer card = cards[i];
            if (card == null)
                continue;

            // Which frame, and whether this pass through the sheet is mirrored (more variety from the same 16 frames).
            float step = (t + phase[i]) * framesPerSecond;
            int whole = Mathf.FloorToInt(step);
            int frame = ((whole % frames) + frames) % frames;
            bool flip = (Mathf.FloorToInt((t + phase[0]) * framesPerSecond) / frames) % 2 == 1; // every card mirrors together
            int col = frame % columns, row = frame / columns;
            // Texture rows run from the top of the sheet; UV v runs from the bottom.
            float v = 1f - (row + 1) * sy;
            Vector4 st = flip ? new Vector4(-sx, sy, (col + 1) * sx, v) : new Vector4(sx, sy, col * sx, v);
            card.GetPropertyBlock(block);
            block.SetVector(UnlitST, st);
            block.SetVector(EmissiveST, st);
            card.SetPropertyBlock(block);

            // Upright in the world, turned toward the camera.
            if (cam != null)
            {
                Vector3 to = card.transform.position - cam.transform.position;
                to.y = 0f;
                if (to.sqrMagnitude > 1e-8f)
                    card.transform.rotation = Quaternion.LookRotation(to, Vector3.up);
            }

            // A gentle flicker: taller and thinner, then shorter and wider, out of step between the cards.
            float n = 1f + flicker * (0.6f * Mathf.Sin(t * 13f + phase[i] * 7f) + 0.4f * Mathf.Sin(t * 23f + phase[i] * 3f));
            Vector3 s = baseScale[i];
            card.transform.localScale = new Vector3(s.x * (2f - n), s.y * n, s.z);
        }
    }
}
