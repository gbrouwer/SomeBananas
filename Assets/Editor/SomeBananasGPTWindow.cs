using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class SomeBananasGPTWindow : EditorWindow
{
    private string userInput = "";
    private Vector2 scrollPos;
    private List<ChatEntry> chatHistory = new();
    private List<ScriptFile> attachedScripts = new();

    private int totalInputTokens = 0;
    private int totalOutputTokens = 0;
    private float totalCost = 0f;

    private string[] modelOptions = { "gpt-3.5-turbo", "gpt-4.1", "gpt-4.1-mini", "gpt-4.1-nano" };
    private int selectedModelIndex = 0;
    private string currentModel => modelOptions[selectedModelIndex];
    private float inputRatePer1k = 0.0010f;
    private float outputRatePer1k = 0.0010f;

    private bool showFullScripts = false;

    [MenuItem("Window/SomeBananasGPT Chat")]
    public static void ShowWindow() => GetWindow<SomeBananasGPTWindow>("SomeBananasGPT");

    private void OnGUI()
    {
        float fontSize = 12f;

        GUILayout.BeginVertical();



        GUILayout.BeginHorizontal();
        GUILayout.Label("Model:", GUILayout.Width(50));
        int newSelected = EditorGUILayout.Popup(selectedModelIndex, modelOptions);
        if (newSelected != selectedModelIndex)
        {
            selectedModelIndex = newSelected;
            UpdateModelRates();
        }
        if (GUILayout.Button("Clear Chat", GUILayout.Width(100)))
        {
            if (EditorUtility.DisplayDialog("Clear Chat?", "Are you sure you want to clear this chat?", "Yes", "No"))
            {
                chatHistory.Clear();
                totalInputTokens = 0;
                totalOutputTokens = 0;
                totalCost = 0f;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Label($"Tokens In: {totalInputTokens} | Out: {totalOutputTokens} | Cost: ${totalCost:F4}", EditorStyles.helpBox);



        DrawChatHistory(fontSize);

        if (attachedScripts.Count > 0)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Attached Scripts:", EditorStyles.boldLabel);
            showFullScripts = GUILayout.Toggle(showFullScripts, showFullScripts ? "Hide Full" : "Show Full");
            GUILayout.EndHorizontal();

            foreach (var script in attachedScripts)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label(script.fileName);
                string preview = showFullScripts ? script.content : GetPreview(script.content, 20);
                GUILayout.TextArea(preview, GUILayout.Height(showFullScripts ? 160 : 80));
                if (GUILayout.Button("📋 Copy", GUILayout.Width(70)))
                    EditorGUIUtility.systemCopyBuffer = script.content;
                GUILayout.EndVertical();
            }
        }

        Rect dropArea = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag .cs files here", EditorStyles.helpBox);
        Event evt = Event.current;
        if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) && dropArea.Contains(evt.mousePosition))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (string path in DragAndDrop.paths)
                {
                    if (path.EndsWith(".cs"))
                        attachedScripts.Add(new ScriptFile(Path.GetFileName(path), File.ReadAllText(path)));
                }
            }
            Event.current.Use();
        }

        GUILayout.Label("Your Prompt:", EditorStyles.boldLabel);
        userInput = EditorGUILayout.TextArea(userInput, GUILayout.MinHeight(60));

        if (GUILayout.Button("Send", GUILayout.Height(30)))
        {
            if (!string.IsNullOrWhiteSpace(userInput))
            {
                string fullPrompt = userInput;
                foreach (var script in attachedScripts)
                    fullPrompt += $"\n\n// {script.fileName}\n{script.content}\n";

                int inputTokens = EstimateTokens(fullPrompt);
                totalInputTokens += inputTokens;

                chatHistory.Add(new ChatEntry("user", userInput));

                OpenAIRequestHandler.SendChatWithModel(fullPrompt, currentModel, (response) =>
                {
                    int outputTokens = EstimateTokens(response);
                    totalOutputTokens += outputTokens;
                    totalCost += (inputTokens * inputRatePer1k / 1000f) + (outputTokens * outputRatePer1k / 1000f);
                    chatHistory.Add(new ChatEntry("gpt", response));
                    Repaint();
                });

                userInput = "";
            }
        }

        GUILayout.EndVertical();
    }

private string GetPreview(string content, int lineCount)
{
    var lines = content.Split('\n');
    return string.Join("\n", lines, 0, Mathf.Min(lines.Length, lineCount)) + (lines.Length > lineCount ? "\n..." : "");
}    

private void UpdateModelRates()
{
    switch (currentModel)
    {
        case "gpt-3.5-turbo":
            inputRatePer1k = 0.0010f;
            outputRatePer1k = 0.0010f;
            break;
        case "gpt-4.1":
            inputRatePer1k = 0.0020f;
            outputRatePer1k = 0.0080f;
            break;
        case "gpt-4.1-mini":
            inputRatePer1k = 0.0004f;
            outputRatePer1k = 0.0016f;
            break;
        case "gpt-4.1-nano":
            inputRatePer1k = 0.0001f;
            outputRatePer1k = 0.0004f;
            break;
    }
}

private const string ChatHistoryKey = "SomeBananasGPT_ChatHistory";

private void OnEnable()
{
    if (EditorPrefs.HasKey(ChatHistoryKey))
    {
        string saved = EditorPrefs.GetString(ChatHistoryKey);
        chatHistory = JsonUtility.FromJson<ChatLogWrapper>(saved)?.entries ?? new List<ChatEntry>();
    }
}

private void OnDisable()
{
    SaveChatHistory();
}

private void SaveChatHistory()
{
    ChatLogWrapper wrapper = new ChatLogWrapper { entries = chatHistory };
    string json = JsonUtility.ToJson(wrapper);
    EditorPrefs.SetString(ChatHistoryKey, json);
}

[Serializable]
private class ChatLogWrapper
{
    public List<ChatEntry> entries;
}

private void DrawChatHistory(float fontSize)
{
    scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
    foreach (var entry in chatHistory)
    {
        bool isUser = entry.role == "user";
        GUIStyle textStyle = GetStyledTextStyle(isUser, fontSize);
        GUIStyle bubbleStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = Texture2D.whiteTexture },
            padding = new RectOffset(12, 12, 8, 8),
            margin = new RectOffset(6, 6, 6, 6)
        };

        Color bubbleColor = isUser ? new Color(0.6f, 1f, 0.6f, 0.2f) : new Color(1f, 0.6f, 0.9f, 0.2f);
        Color originalBg = GUI.backgroundColor;
        GUI.backgroundColor = bubbleColor;

        GUILayout.BeginHorizontal();

        if (isUser)
            GUILayout.FlexibleSpace();

        GUILayout.BeginVertical(GUILayout.Width(position.width * 0.6f));

        GUILayout.BeginVertical(bubbleStyle);

        if (!isUser)
        {
            var blocks = ParseMarkdown(entry.message);
            foreach (var block in blocks)
            {
                if (block.isCode)
                {
                    GUI.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
                    GUILayout.BeginVertical("box");
                    GUILayout.Label(block.content, new GUIStyle(EditorStyles.textArea)
                    {
                        font = Font.CreateDynamicFontFromOSFont("Courier New", (int)fontSize),
                        fontSize = (int)fontSize,
                        wordWrap = true,
                        richText = false,
                        normal = { textColor = Color.white }
                    });
                    if (GUILayout.Button("📋 Copy Code", GUILayout.Width(90)))
                        EditorGUIUtility.systemCopyBuffer = block.content;
                    GUILayout.EndVertical();
                }
                else
                {
                    GUILayout.Label(block.content, textStyle);
                }
            }
        }
        else
        {
            GUILayout.Label(entry.message, textStyle);
        }

        GUILayout.EndVertical(); // end speech bubble
        GUILayout.EndVertical();

        if (!isUser)
            GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();
        GUILayout.Space(12);

        GUI.backgroundColor = originalBg;
    }
    GUILayout.EndScrollView();
}

    private string ConvertMarkdownToRichText(string markdown)
    {
        string text = markdown;
        text = Regex.Replace(text, @"^# (.*)", "<size=18><b>$1</b></size>", RegexOptions.Multiline);
        text = Regex.Replace(text, @"^## (.*)", "<size=16><b>$1</b></size>", RegexOptions.Multiline);
        text = Regex.Replace(text, @"\\*\\*([^*]+)\\*\\*", "<b>$1</b>");
        text = Regex.Replace(text, @"(?<!\\*)\\*([^*]+)\\*(?!\\*)", "<i>$1</i>");
        text = Regex.Replace(text, @"\\b\\w+\\(\\)", m => $"<b><color=#66ccff>{m.Value}</color></b>");
        text = Regex.Replace(text, @"^---$", "<color=#666666>────────────</color>", RegexOptions.Multiline);
        text = Regex.Replace(text, @"^- (.*)", "• $1", RegexOptions.Multiline);
        return text;
    }

    private List<MessageBlock> ParseMarkdown(string input)
    {
        var result = new List<MessageBlock>();
        string[] parts = input.Split(new string[] { "```" }, StringSplitOptions.None);
        for (int i = 0; i < parts.Length; i++)
        {
            bool isCode = i % 2 != 0;
            string block = parts[i];
            if (isCode)
            {
                int firstLine = block.IndexOf('\n');
                if (firstLine > -1)
                {
                    string first = block.Substring(0, firstLine).Trim().ToLower();
                    if (first.Length < 20 && Regex.IsMatch(first, "^[a-zA-Z]+$"))
                        block = block.Substring(firstLine + 1);
                }
            }
            else block = ConvertMarkdownToRichText(block);
            result.Add(new MessageBlock { isCode = isCode, content = block.Trim() });
        }
        return result;
    }

    private GUIStyle GetStyledTextStyle(bool isUser, float fontSize)
    {
        return new GUIStyle(EditorStyles.wordWrappedLabel)
        {
            richText = true,
            fontSize = (int)fontSize,
            wordWrap = true,
            normal = { textColor = isUser ? new Color(0.7f, 1f, 0.7f) : new Color(1f, 0.7f, 0.9f) },
            padding = new RectOffset(0, 0, 0, 0)
        };
    }

    private int EstimateTokens(string text) => Mathf.CeilToInt(text.Length / 4f);

    [Serializable] public class ChatEntry { public string role; public string message; public DateTime timestamp; public ChatEntry(string role, string message) { this.role = role; this.message = message; this.timestamp = DateTime.Now; } }
    [Serializable] public class ScriptFile { public string fileName; public string content; public ScriptFile(string fileName, string content) { this.fileName = fileName; this.content = content; } }
    private class MessageBlock { public bool isCode; public string content; }
}
