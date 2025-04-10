using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GridExplosionRendering
{
    public class GridExplosionPlaneSpawnerAuthoring : MonoBehaviour
    {
        public GameObject GridExplosionPlanePrefab; // Assign this in the Inspector

        class Baker : Baker<GridExplosionPlaneSpawnerAuthoring>
        {
            public override void Bake(GridExplosionPlaneSpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new GridExplosionPlaneSpawner
                {
                    Prefab = GetEntity(authoring.GridExplosionPlanePrefab, TransformUsageFlags.Renderable)
                });
            }
        }
    }

    // ✅ Component to store the plane prefab reference
    public struct GridExplosionPlaneSpawner : IComponentData
    {
        public Entity Prefab;
    }
}

