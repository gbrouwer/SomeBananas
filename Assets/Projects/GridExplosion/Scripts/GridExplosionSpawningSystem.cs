//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Mathematics;
//using Unity.Transforms;
//using Unity.Rendering;
//using JetBrains.Annotations;
//using UnityEngine.LowLevelPhysics;
//using Unity.Physics;
//using UnityEngine;

//namespace GridExplosionRendering
//{
//    public struct GridExplosionSpawned : IComponentData { }

//    [BurstCompile]
//    public partial struct GridExplosionSpawningSystem : ISystem
//    {
//        private EntityQuery _query;

//        public void OnCreate(ref SystemState state)
//        {
//            UnityEngine.Debug.Log($"GridExplosionSpawnSystem running in world: {state.World.Name}");
//            if (state.World.Name != "Default World") // Assign system only to GridWorld
//            {
//                state.Enabled = false;
//                return;
//            }

//            _query = new EntityQueryBuilder(Allocator.Temp)
//                .WithAll<GridExplosionConfig>()
//                .WithNone<GridExplosionSpawned>()
//                .Build(ref state);
//        }

//        public void OnUpdate(ref SystemState state)
//        {
//            var ecb = new EntityCommandBuffer(Allocator.Temp);

//            // ✅ Exit early if cubes have already been spawned
//            if (SystemAPI.HasSingleton<GridExplosionSpawned>())
//            {
//                state.Enabled = false; // 🔥 Disable the system after first run
//                return;
//            }

//            foreach (var (gridConfig, entity) in SystemAPI.Query<RefRO<GridExplosionConfig>>().WithEntityAccess())
//            {
//                if (state.EntityManager.HasComponent<GridExplosionSpawned>(entity))
//                    continue;

//                var resolution = gridConfig.ValueRO.Resolution;
//                var gridExtent = gridConfig.ValueRO.GridExtent;
//                var prefab = gridConfig.ValueRO.CubePrefab;

//                float3 spacing = gridExtent / new float3(resolution.x, resolution.y, resolution.z);
//                float3 cubeScale = spacing;

//                for (int x = 0; x < resolution.x; x++)
//                    for (int y = 0; y < resolution.y; y++)
//                        for (int z = 0; z < resolution.z; z++)
//                        {
//                            float3 position = (new float3(x, y, z) * spacing) - (new float3(gridExtent) * 0.5f);

//                            int iterations = MandelbulbIterations(position, 2, 5, 10);

//                            float angleDegrees = 45; // Adjust this for different rotations
//                            float radians = math.radians(angleDegrees);

//                            //// ✅ Step 2: Create a rotation matrix (change axis if needed)
//                            float3x3 rotationMatrix = float3x3.EulerXYZ(0, 0, radians); // Rotate around Z-axis

//                            //// ✅ Step 3: Apply rotation to the position
//                            position = math.mul(rotationMatrix, position); // Rotate the position

//                            // ✅ Step 4: Create a quaternion rotation for the cube itself
//                            quaternion cubeRotation = quaternion.EulerXYZ(0, 0, radians); // Apply same rotation



//                            if (!IsOnSurface(position, 5))
//                                continue; // Skip cubes that are deep inside the Mandelbulb

//                            var instance = ecb.Instantiate(prefab);



//                            float alpha = math.saturate((float)iterations / 10);
//                            float4 color = iterations >= 2
//                                ? new float4(1, 1, 1, 1)
//                                : new float4(1, 1, 1, 0.5f);

//                            float cubeUniformScale = math.cmin(cubeScale);
//                            ecb.AddComponent(instance, new URPMaterialPropertyBaseColor { Value = color });


//                            ecb.SetComponent(instance, LocalTransform.FromPositionRotationScale(position, cubeRotation, cubeUniformScale));

//                            var boxGeometry = new Unity.Physics.BoxGeometry
//                            {
//                                Center = float3.zero,
//                                Orientation = quaternion.identity,
//                                Size = new float3(1.0f,1.0f,1.0f),// cubeUniformScale*3.0f, cubeUniformScale*3.0f, 3.0f*cubeUniformScale), // ✅ Match scaling
//                                BevelRadius = 0.1f
//                            };


//                            var collider = Unity.Physics.BoxCollider.Create(boxGeometry);
//                            ecb.AddComponent(instance, new PhysicsCollider { Value = collider });
//                            ecb.AddComponent(instance, PhysicsMass.CreateDynamic(collider.Value.MassProperties, 1f));
//                            ecb.AddComponent(instance, new PhysicsVelocity { Linear = float3.zero, Angular = float3.zero });

//                            // ✅ New: Start as kinematic (no gravity)
//                            ecb.AddComponent(instance, PhysicsMass.CreateKinematic(collider.Value.MassProperties));

//                            // ✅ Also store a tag so we can identify physics-disabled cubes later
//                            ecb.AddComponent(instance, new PhysicsDisabledTag());

//                        }

//                ecb.AddComponent(entity, new GridExplosionSpawned());
//            }

//            // ✅ Create and register the singleton
//            if (!SystemAPI.HasSingleton<GridExplosionSpawned>())
//            {
//                var spawnedEntity = state.EntityManager.CreateEntity();
//                state.EntityManager.AddComponent<GridExplosionSpawned>(spawnedEntity);
//            }



//            ecb.Playback(state.EntityManager);
//            ecb.Dispose();

//            // ✅ Disable system after first execution
//            state.Enabled = false;
//        }

//        private bool IsOnSurface(float3 pos, int threshold)
//        {
//            int iterations = MandelbulbIterations(pos, 2, 5, 10);

//            float3[] offsets = new float3[]
//            {
//        new float3(1, 0, 0), new float3(-1, 0, 0),
//        new float3(0, 1, 0), new float3(0, -1, 0),
//        new float3(0, 0, 1), new float3(0, 0, -1)
//            };

//            foreach (float3 offset in offsets)
//            {
//                float3 neighborPos = pos + offset * 0.1f; // Small step to check surface detail
//                int neighborIterations = MandelbulbIterations(neighborPos, 2, 5, 10);

//                if ((iterations < threshold && neighborIterations >= threshold) ||
//                    (iterations >= threshold && neighborIterations < threshold))
//                    return true; // This position is on the Mandelbulb surface
//            }
//            return false; // Inside the set, no need to spawn
//        }


//        private int MandelbulbIterations(float3 pos, int n, int m, int o)
//        {
//            float3 z = pos;
//            int iterations = 0;
//            float power = 8.0f;
//            float bailout = 4.0f;

//            while (math.lengthsq(z) < bailout && iterations < o)
//            {
//                float r = math.length(z);
//                float theta = math.acos(z.y / r) * power;
//                float phi = math.atan2(z.z, z.x) * power;

//                float sinTheta = math.sin(theta);
//                z = r * new float3(sinTheta * math.cos(phi), math.cos(theta), sinTheta * math.sin(phi)) + pos;

//                iterations++;
//            }
//            return iterations;
//        }
//    }
//}
