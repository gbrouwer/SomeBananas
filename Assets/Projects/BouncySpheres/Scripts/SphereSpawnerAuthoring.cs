using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SphereSpawnerAuthoring : MonoBehaviour
{
    public GameObject SpherePrefab; // Assign in Unity Inspector
    public int MaxSpheres = 100;    // ✅ Make MaxSpheres a public variable

    class Baker : Baker<SphereSpawnerAuthoring>
    {
        public override void Bake(SphereSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new SphereSpawner
            {
                Prefab = GetEntity(authoring.SpherePrefab, TransformUsageFlags.Dynamic),
                MaxSpheres = authoring.MaxSpheres // ✅ Pass MaxSpheres directly

            });
        }
    }
}
