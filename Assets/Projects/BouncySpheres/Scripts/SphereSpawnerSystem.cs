using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;  // ✅ Required for material properties
using UnityEngine;

[BurstCompile]
public partial struct SphereSpawnerSystem : ISystem
{
    private Entity spherePrefab;
    private Unity.Mathematics.Random random;
    private bool hasSpawned;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SphereSpawner>();
        random = new Unity.Mathematics.Random(1);
        hasSpawned = false;
    }

    public void OnUpdate(ref SystemState state)
    {
        if (hasSpawned) return;

        Debug.Log("🚀 Spawning spheres with random colors...");

        if (spherePrefab == Entity.Null)
        {
            spherePrefab = SystemAPI.GetSingleton<SphereSpawner>().Prefab;
            return;
        }

        int maxSpheres = SystemAPI.GetSingleton<SphereSpawner>().MaxSpheres;
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        for (int i = 0; i < maxSpheres; i++)
        {
            float3 position = new float3(
                random.NextFloat(-5f, 5f),
                random.NextFloat(5f, 10f) + (i * 0.1f),
                random.NextFloat(-5f, 5f));

            Entity sphere = ecb.Instantiate(spherePrefab); // ✅ Instantiate first

            // ✅ Immediately set the required components
            ecb.SetComponent(sphere, LocalTransform.FromPosition(position));

            // ✅ Always add `CustomMaterialProperty` (avoiding HasComponent check)
            float4 randomColor = new float4(
                random.NextFloat(0f, 1f),
                random.NextFloat(0f, 1f),
                random.NextFloat(0f, 1f),
                1f);

        }

        // ✅ Play back the EntityCommandBuffer (registers all created entities)
        ecb.Playback(state.EntityManager);
        ecb.Dispose();

        hasSpawned = true;
    }
}
