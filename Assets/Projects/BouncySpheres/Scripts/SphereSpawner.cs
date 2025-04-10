using Unity.Entities;

public struct SphereSpawner : IComponentData
{
    public Entity Prefab;
    public int MaxSpheres;  // ✅ Now MaxSpheres is stored in ECS!
}
