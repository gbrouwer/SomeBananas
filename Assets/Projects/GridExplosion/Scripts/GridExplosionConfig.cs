using Unity.Entities;
using UnityEngine;

namespace GridExplosionRendering
{

    public struct GridExplosionConfig : IComponentData
    {
        public Entity CubePrefab;
        public Vector3Int Resolution;
        public Vector3 GridExtent;
    }
}
