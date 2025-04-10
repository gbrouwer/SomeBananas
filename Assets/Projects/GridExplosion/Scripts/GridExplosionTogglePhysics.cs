//using Unity.Entities;
//using UnityEngine;

//namespace GridExplosionRendering
//{
//    public class PhysicsToggleButton : MonoBehaviour
//    {
//        private World defaultWorld;
//        private SystemHandle enablePhysicsSystemHandle;
//        private bool systemFound = false;

//        void Start()
//        {
//            defaultWorld = World.DefaultGameObjectInjectionWorld;

//            if (!defaultWorld.IsCreated)
//            {
//                Debug.LogError("❌ Default World is not created yet!");
//                return;
//            }

//            if (defaultWorld.GetExistingSystem<GridExplosionPhysicsSystem>() == SystemHandle.Null)
//            {
//                Debug.LogError("❌ EnablePhysicsSystem not found in the Default World!");
//                return;
//            }

//            enablePhysicsSystemHandle = defaultWorld.GetExistingSystem<GridExplosionPhysicsSystem>();
//            systemFound = true;
//        }

//        public void EnablePhysics() // ✅ Make sure this is public!
//        {
//            if (!systemFound)
//            {
//                Debug.LogError("❌ EnablePhysicsSystem not available. Cannot enable physics!");
//                return;
//            }

//            Debug.Log("🔘 UI Button Clicked!");

//            var unmanagedWorld = defaultWorld.Unmanaged;

//            // ✅ Get a reference to the system
//            ref var systemRef = ref unmanagedWorld.GetUnsafeSystemRef<GridExplosionPhysicsSystem>(enablePhysicsSystemHandle);

//            systemRef.EnablePhysics = true; // ✅ Modify the actual system value

//            Debug.Log($"🟢 EnablePhysics is now: {systemRef.EnablePhysics}");
//        }
//    }
//}
