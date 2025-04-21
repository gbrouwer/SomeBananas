using System.IO;
using UnityEngine;

public static class SecretLoader
{
    private static string cachedKey;

    public static string LoadOpenAIKey()
    {
        if (!string.IsNullOrEmpty(cachedKey)) return cachedKey;

        string path = Path.Combine(Application.dataPath, "../.config/openai");

        if (File.Exists(path))
        {
            cachedKey = File.ReadAllText(path).Trim();
            return cachedKey;
        }

        Debug.LogError("OpenAI key file not found! Expected at: " + path);
        return null;
    }
}
