using UnityEngine;

// A tree prototype's leaf colours through the year (Season_System.md: Visual Feedback). Sits on the tree prefabs
// PropertyTerrainBuilder generates; SeasonalTrees reads it to colour, drop and regrow that prototype's leaves.
public class SeasonalFoliage : MonoBehaviour
{
    [Tooltip("New leaves in early Spring.")]
    public Color springColor = new Color(0.28f, 0.42f, 0.14f);
    public Color summerColor = new Color(0.19f, 0.33f, 0.12f);
    [Tooltip("Peak Fall colour.")]
    public Color fallColor = new Color(0.55f, 0.22f, 0.06f);
    [Tooltip("Whether the leaves drop over late Fall and regrow in Spring.")]
    public bool deciduous = true;
}
