using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;

public static class OpenAIRequestHandler
{
    private const string apiUrl = "https://api.openai.com/v1/chat/completions";

    public static void SendChatWithModel(string prompt, string model, System.Action<string> callback)
    {
        string apiKey = SecretLoader.LoadOpenAIKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API key not found.");
            callback?.Invoke("Missing API key.");
            return;
        }

        EditorCoroutineUtility.StartCoroutineOwnerless(SendRequest(prompt, apiKey, model, callback));
    }

    private static IEnumerator SendRequest(string prompt, string apiKey, string model, System.Action<string> callback)
    {
        ChatRequest requestData = new ChatRequest(prompt, model);
        string json = JsonUtility.ToJson(requestData);

        var request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("GPT Request Error: " + request.error);
            callback?.Invoke("Error: " + request.downloadHandler.text);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            string content = ParseResponse(jsonResponse);
            callback?.Invoke(content);
        }
    }

    private static string ParseResponse(string json)
    {
        var wrapper = JsonUtility.FromJson<ChatWrapper>(json);
        return wrapper.choices[0].message.content.Trim();
    }

    [System.Serializable]
    public class ChatRequest
    {
        public string model;
        public List<Message> messages = new();

        public ChatRequest(string prompt, string model)
        {
            this.model = model;
            messages.Add(new Message("user", prompt));
        }
    }

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;

        public Message(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }

    [System.Serializable]
    private class ChatWrapper
    {
        public Choice[] choices;
    }

    [System.Serializable]
    private class Choice
    {
        public Message message;
    }
}
