using Unity.Entities;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Authoring;

namespace GridExplosionRendering
{
    public class GridExplosionAuthoring : MonoBehaviour
    {
        public GameObject CubePrefab;
        public Vector3Int Resolution = new Vector3Int(10, 10, 10);
        public Vector3 GridExtent = new Vector3(10f, 10f, 10f);

        class Baker : Baker<GridExplosionAuthoring>
        {
            public override void Bake(GridExplosionAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                AddComponent(entity, new GridExplosionConfig
                {
                    CubePrefab = GetEntity(authoring.CubePrefab, TransformUsageFlags.Renderable),
                    Resolution = authoring.Resolution,
                    GridExtent = authoring.GridExtent
                });
            }
        }
    }
}
