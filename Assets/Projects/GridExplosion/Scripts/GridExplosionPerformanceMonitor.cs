using UnityEngine;
using TMPro;
using Unity.Entities;
using UnityEngine.Rendering;
using System.Text;

namespace GridExplosionRendering
{
    public class PerformanceMonitor : MonoBehaviour
    {
        public TMP_Text fpsText;
        public TMP_Text entityCountText;
        public TMP_Text verticesText;
        public TMP_Text edgesText;
        public TMP_Text facesText;

        private float deltaTime = 0.0f;
        private float updateInterval = 0.5f; // 🔥 Update stats every 0.5 seconds
        private float timeSinceLastUpdate = 0f;
        private StringBuilder sb = new StringBuilder(); // 🔥 Use StringBuilder to reduce GC

        private void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            timeSinceLastUpdate += Time.unscaledDeltaTime;

            if (timeSinceLastUpdate >= updateInterval)
            {
                timeSinceLastUpdate = 0f;

                // ✅ Calculate FPS
                float fps = 1.0f / deltaTime;
                fpsText.text = $"FPS: {fps:F1}";

                // ✅ Get Entity Count
                var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                int entityCount = entityManager.GetAllEntities().Length;
                entityCountText.text = $"Entities: {entityCount}";

                // ✅ Get Rendering Stats
                int totalVertices = 0, totalEdges = 0, totalFaces = 0;
                foreach (var mesh in Resources.FindObjectsOfTypeAll<Mesh>())
                {
                    if (!mesh.isReadable) continue; // 🔥 Skip unreadable meshes

                    totalVertices += mesh.vertexCount;
                    totalEdges += mesh.triangles.Length;  // Each triangle has 3 edges
                    totalFaces += mesh.triangles.Length / 3;
                }

                // 🔥 Use StringBuilder to reduce memory allocation
                sb.Clear();
                sb.Append("Vertices: ").Append(totalVertices);
                verticesText.text = sb.ToString();

                sb.Clear();
                sb.Append("Edges: ").Append(totalEdges);
                edgesText.text = sb.ToString();

                sb.Clear();
                sb.Append("Faces: ").Append(totalFaces);
                facesText.text = sb.ToString();
            }
        }
    }
}
