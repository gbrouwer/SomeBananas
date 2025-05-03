using Unity.Entities;
using UnityEngine.SceneManagement;

public class ConditionalWorldBootstrap : ICustomBootstrap
{
    public bool Initialize(string defaultWorldName)
    {
        // Only allow ECS world in specific scenes
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "YourECSScene")
        {
            // Debug.Log("Creating ECS world for scene: " + sceneName);
            DefaultWorldInitialization.Initialize(defaultWorldName);
            return true;
        }

        // Debug.Log("Skipping ECS world creation for scene: " + sceneName);
        return false;
    }
}