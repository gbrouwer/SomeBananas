
using Unity.Entities;
using Unity.Mathematics; // ✅ Needed for float4
using Unity.Rendering;   // ✅ Needed for material properties
using UnityEngine;

public class SphereAuthoring : MonoBehaviour
{
    class Baker : Baker<SphereAuthoring>
    {
        public override void Bake(SphereAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SphereComponent { Bounciness = 0.5f });
            AddComponent(entity, new Prefab()); // ✅ Mark as a prefab

        }
    }
}

// ✅ Ensure the CustomMaterialProperty struct exists



public struct SphereComponent : IComponentData
{
    public float Bounciness;
}
