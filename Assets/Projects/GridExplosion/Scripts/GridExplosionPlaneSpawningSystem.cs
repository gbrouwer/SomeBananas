//using Unity.Burst;
//using Unity.Entities;
//using Unity.Mathematics;
//using Unity.Transforms;
//using Unity.Rendering;
//using Unity.Physics;
//using UnityEngine;
//namespace GridExplosionRendering
//{
//    [BurstCompile]

//    public partial struct GridExplosionPlaneSpawnerSystem : ISystem
//    {
//        private bool hasSpawned;

//        public void OnCreate(ref SystemState state)
//        {
//            state.RequireForUpdate<GridExplosionPlaneSpawner>();
//            hasSpawned = false;
//        }

//        public void OnUpdate(ref SystemState state)
//        {
//            if (hasSpawned) return;

//            var planeSpawner = SystemAPI.GetSingleton<GridExplosionPlaneSpawner>();
//            if (planeSpawner.Prefab == Entity.Null) return; // Ensure prefab exists

//            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
//            var plane = ecb.Instantiate(planeSpawner.Prefab);

//            // ✅ Set the plane's position under the cubes
//            ecb.SetComponent(plane, LocalTransform.FromPositionRotationScale(new float3(0, -3f, 0), quaternion.identity, 5f));

//            ecb.Playback(state.EntityManager);
//            ecb.Dispose();

//            hasSpawned = true;
//            state.Enabled = false; // ✅ Disable system after spawning the plane
//        }
//    }
//}
