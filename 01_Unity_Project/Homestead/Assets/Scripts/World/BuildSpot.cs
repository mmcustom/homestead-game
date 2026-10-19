using UnityEngine;

// A place something could be built: where, which way the ground faces, and what the aim ray (or the old fixed probe) hit
// there. Passed to the placement rules so they can be asked about any spot, not just the one in front of the player.
public struct BuildSpot
{
    public Vector3 point;
    public Vector3 normal;
    // The first thing under the spot is the terrain (not a wall, a roof, a tree or a rock).
    public bool onTerrain;
    // The first thing under the spot is water (what a campfire's rule needs).
    public bool inWaterSource;
}
