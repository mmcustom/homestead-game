using UnityEngine;

// Audio_System.md's footstep surfaces.
public enum SurfaceType { Grass, Dirt, Gravel }

// Marks what a ground collider is made of, for footstep sounds. Put it on the collider's object or a parent.
// Ground without one uses PlayerAudio's default surface. Real terrain will need its texture layers mapped to
// surfaces instead — this covers placed objects and the placeholder ground until then.
public class GroundSurface : MonoBehaviour
{
    [SerializeField] SurfaceType surface = SurfaceType.Grass;

    public SurfaceType Surface => surface;
}
