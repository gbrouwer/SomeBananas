//using Unity.Entities;
//using Unity.Physics;
//using UnityEngine;

//namespace GridExplosionRendering
//{

//    public partial struct GridExplosionPhysicsSystem : ISystem
//    {
//        public bool EnablePhysics; // Controlled by UI button

//        public void OnCreate(ref SystemState state)
//        {
//            // ✅ Log when the system is first created
//            UnityEngine.Debug.Log("🟢 EnablePhysicsSystem has been created!");
//        }

//        public void OnUpdate(ref SystemState state)
//        {
//            // ✅ Log to check if the system is running at all
//            //UnityEngine.Debug.Log("🔄 EnablePhysicsSystem is running...");

//            if (!EnablePhysics)
//            {
//                //UnityEngine.Debug.Log("⏸ EnablePhysics is still false. Waiting...");
//                return;
//            }

//            //UnityEngine.Debug.Log("✅ EnablePhysicsSystem is now applying physics!");

//            var entityManager = state.EntityManager;
//            var query = entityManager.CreateEntityQuery(typeof(PhysicsDisabledTag), typeof(PhysicsCollider), typeof(PhysicsMass));

//            foreach (var entity in query.ToEntityArray(Unity.Collections.Allocator.Temp))
//            {
//                UnityEngine.Debug.Log($"🔄 Updating physics for entity {entity.Index}");

//                var collider = entityManager.GetComponentData<PhysicsCollider>(entity);
//                if (!collider.Value.IsCreated)
//                {
//                    UnityEngine.Debug.Log("❌ Collider not created!");
//                    continue;
//                }

//                var massProperties = collider.Value.Value.MassProperties;

//                var oldMass = entityManager.GetComponentData<PhysicsMass>(entity);
//                UnityEngine.Debug.Log($"🔍 Before: MassType = {oldMass.InverseMass}");

//                entityManager.SetComponentData(entity, PhysicsMass.CreateDynamic(massProperties, 1f));

//                var newMass = entityManager.GetComponentData<PhysicsMass>(entity);
//                UnityEngine.Debug.Log($"✅ After: MassType = {newMass.InverseMass}");

//                entityManager.RemoveComponent<PhysicsDisabledTag>(entity);
//            }

//            // ✅ Disable system after applying physics
//            state.Enabled = false;
//        }
//    }
//    // ✅ Tag to mark entities with physics disabled
//    public struct PhysicsDisabledTag : IComponentData { }



//}


