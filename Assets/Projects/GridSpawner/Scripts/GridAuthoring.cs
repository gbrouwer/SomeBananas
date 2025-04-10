//using Unity.Entities;
//using UnityEngine;

//namespace GridRendering
//{
//    public class GridAuthoring : MonoBehaviour
//    {
//        public GameObject CubePrefab;
//        public Vector3Int GridSize = new Vector3Int(10, 10, 10);
//        public Vector3 GridExtent = new Vector3(10f, 10f, 10f);

//        class Baker : Baker<GridAuthoring>
//        {
//            public override void Bake(GridAuthoring authoring)
//            {
//                var entity = GetEntity(TransformUsageFlags.Renderable);
//                Debug.Log($"Baking GridConfig Entity: {entity.Index}");

//                AddComponent(entity, new GridConfig
//                {
//                    CubePrefab = GetEntity(authoring.CubePrefab, TransformUsageFlags.Renderable),
//                    GridSize = authoring.GridSize,
//                    GridExtent = authoring.GridExtent
//                });
//            }
//        }
//    }

//}
