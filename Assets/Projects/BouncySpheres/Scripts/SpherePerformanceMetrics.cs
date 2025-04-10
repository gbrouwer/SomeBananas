using Unity.Entities;
using UnityEngine;
using TMPro;

public class PerformanceMonitor : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    public TextMeshProUGUI entityCountText;
    public TextMeshProUGUI physicsTimeText;

    private float deltaTime = 0.0f;

    void Update()
    {
        // ✅ Calculate FPS
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = $"FPS: {fps:0.}";

        // ✅ Count Active Entities
        int entityCount = World.DefaultGameObjectInjectionWorld.EntityManager.UniversalQuery.CalculateEntityCount();
        entityCountText.text = $"Entities: {entityCount}";

        // ✅ Show Physics Step Time (Per Frame)
        float physicsTime = Time.smoothDeltaTime;
        physicsTimeText.text = $"Physics Time: {physicsTime * 1000f:0.0}ms";
    }
}
