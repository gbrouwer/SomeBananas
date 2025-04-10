//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Mathematics;
//using Unity.Transforms;
//using Unity.Rendering;

//namespace GridRendering
//{
//    public struct GridSpawned : IComponentData { }

//    [BurstCompile]
//    public partial struct GridSpawnSystem : ISystem
//    {
//        private EntityQuery _query;

//        public void OnCreate(ref SystemState state)
//        {
//            _query = new EntityQueryBuilder(Allocator.Temp)
//                .WithAll<GridConfig>()
//                .WithNone<GridSpawned>()
//                .Build(ref state);
//        }

//        public void OnUpdate(ref SystemState state)
//        {
//            var ecb = new EntityCommandBuffer(Allocator.Temp);

//            foreach (var (gridConfig, entity) in SystemAPI.Query<RefRO<GridConfig>>().WithEntityAccess())
//            {
//                if (state.EntityManager.HasComponent<GridSpawned>(entity))
//                    continue; // Skip if already spawned

//                var gridSize = gridConfig.ValueRO.GridSize;
//                var gridExtent = gridConfig.ValueRO.GridExtent;
//                var prefab = gridConfig.ValueRO.CubePrefab;

//                float3 spacing = gridExtent / (new float3(gridSize.x - 1, gridSize.y - 1, gridSize.z - 1));

//                for (int x = 0; x < gridSize.x; x++)
//                    for (int y = 0; y < gridSize.y; y++)
//                        for (int z = 0; z < gridSize.z; z++)
//                        {
//                            float3 position = (new float3(x, y, z) * spacing) - (new float3(gridExtent) * 0.5f);
//                            var instance = ecb.Instantiate(prefab);
//                            ecb.SetComponent(instance, LocalTransform.FromPosition(position));

//                            // Generate a unique color based on position
//                            float3 color = new float3((float)x / gridSize.x, (float)y / gridSize.y, (float)z / gridSize.z);
//                            ecb.AddComponent(instance, new URPMaterialPropertyBaseColor { Value = new float4(color, 1.0f) });
//                        }

//                ecb.AddComponent(entity, new GridSpawned()); // Mark as spawned
//            }

//            ecb.Playback(state.EntityManager);
//            ecb.Dispose();
//        }
//    }
//}
