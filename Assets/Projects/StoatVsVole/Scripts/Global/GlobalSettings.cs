using UnityEngine;

namespace StoatVsVole
{
    /// <summary>
    /// Stores global, simulation-wide parameters accessible by multiple systems.
    /// Primarily defines the world size, which determines terrain and wall boundaries.
    /// </summary>
    public class GlobalSettings : MonoBehaviour
    {
        [Header("World Settings")]
        [Tooltip("Length of one side of the square world (units). Affects ground, wall, and grid scaling.")]
        public float worldSize = 10f;
    }
}
