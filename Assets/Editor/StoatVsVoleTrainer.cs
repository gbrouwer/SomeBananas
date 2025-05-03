#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Diagnostics;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Collections.Generic;

namespace StoatVsVole {

public class StoatVsVoleTrainer : EditorWindow
{
    private PPOConfig config = new PPOConfig();
    private string yamlFilePath;
    private static Process trainingProcess;
    private static bool hasScheduledPlayMode = false;
    private Vector2 scrollPos;

    [MenuItem("Training/StoatVsVole %#t")] // Ctrl+Shift+T
    public static void ShowWindow()
    {
        GetWindow<TrainingLauncherWindow>("Training Launcher");
    }

    private void OnEnable()
    {
        // Path to your TrafficJam.yaml
        yamlFilePath = @"C:\Core\SomeBananas\ml-agents\config\ppo\StoatVsVole.yaml";

        if (File.Exists(yamlFilePath))
        {
            LoadYamlConfig();
        }
        else
        {
            UnityEngine.Debug.LogError($"YAML file not found: {yamlFilePath}");
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("ML-Agents Training Launcher", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (config == null)
        {
            GUILayout.Label("No config loaded.", EditorStyles.helpBox);
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        config.batch_size = EditorGUILayout.IntField("Batch Size", config.batch_size);
        config.buffer_size = EditorGUILayout.IntField("Buffer Size", config.buffer_size);
        config.learning_rate = EditorGUILayout.FloatField("Learning Rate", config.learning_rate);
        config.beta = EditorGUILayout.FloatField("Beta", config.beta);
        config.epsilon = EditorGUILayout.FloatField("Epsilon", config.epsilon);
        config.entropy_coefficient = EditorGUILayout.FloatField("Entropy Coefficient", config.entropy_coefficient);

        EditorGUILayout.EndScrollView();

        GUILayout.Space(20);

        if (GUILayout.Button("START TRAINING", GUILayout.Height(50)))
        {
            LaunchTraining();
        }

        GUILayout.Space(10);

        if (trainingProcess != null && !trainingProcess.HasExited)
        {
            if (GUILayout.Button("STOP TRAINING", GUILayout.Height(30)))
            {
                StopTraining();
            }

            GUILayout.Label("Status: Training Running...");
        }
        else
        {
            GUILayout.Label("Status: Idle");
        }
    }

    private void LoadYamlConfig()
    {
        var input = new StreamReader(yamlFilePath);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var yamlObject = deserializer.Deserialize<Dictionary<string, object>>(input);
        input.Close();

        if (yamlObject.TryGetValue("behaviors", out object behaviorsObj))
        {
            if (behaviorsObj is Dictionary<object, object> behaviorsDict)
            {
                foreach (var kvp in behaviorsDict)
                {
                    if (kvp.Value is Dictionary<object, object> innerDict)
                    {
                        if (innerDict.ContainsKey("hyperparameters"))
                        {
                            if (innerDict["hyperparameters"] is Dictionary<object, object> hyperDict)
                            {
                                config.batch_size = GetValue<int>(hyperDict, "batch_size");
                                config.buffer_size = GetValue<int>(hyperDict, "buffer_size");
                                config.learning_rate = GetValue<float>(hyperDict, "learning_rate");
                                config.beta = GetValue<float>(hyperDict, "beta");
                                config.epsilon = GetValue<float>(hyperDict, "epsilon");
                                config.entropy_coefficient = GetValue<float>(hyperDict, "entropy_coefficient");
                            }
                        }
                    }
                }
            }
            else
            {
                UnityEngine.Debug.LogError("[TrainingLauncher] 'behaviors' block is not formatted correctly.");
            }
        }
        else
        {
            UnityEngine.Debug.LogError("[TrainingLauncher] No 'behaviors' block found in YAML!");
        }
    }

    private void LaunchTraining()
    {
        string unityProjectPath = @"C:\Core\SomeBananas";
        string venvPythonPath = @"C:\Core\SomeBananas\.venv\Scripts\python.exe";
        string wrapperScriptPath = @"C:\Core\SomeBananas\trafficjam.py";

        UnityEngine.Debug.Log("[TrainingLauncher] Starting training...");

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = venvPythonPath,
            Arguments = $"\"{wrapperScriptPath}\"",
            WorkingDirectory = unityProjectPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        trainingProcess = new Process
        {
            StartInfo = startInfo
        };

        hasScheduledPlayMode = false;

        trainingProcess.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                UnityEngine.Debug.Log(e.Data);

                if (!hasScheduledPlayMode && e.Data.Contains("Listening on port"))
                {
                    hasScheduledPlayMode = true;
                    UnityEditor.EditorApplication.update += WaitForMainThreadToStartPlaymode;
                }
            }
        };

        trainingProcess.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                UnityEngine.Debug.LogError(e.Data);
            }
        };

        trainingProcess.Start();
        trainingProcess.BeginOutputReadLine();
        trainingProcess.BeginErrorReadLine();
    }

    private static void WaitForMainThreadToStartPlaymode()
    {
        UnityEditor.EditorApplication.update -= WaitForMainThreadToStartPlaymode;
        UnityEngine.Debug.Log("[TrainingLauncher] ML-Agents is ready! Forcing PlayMode start.");
        EditorApplication.isPlaying = true;
    }

    private static void StopTraining()
    {
        if (trainingProcess != null && !trainingProcess.HasExited)
        {
            UnityEngine.Debug.Log("[TrainingLauncher] Stopping training process...");

            try
            {
                trainingProcess.Kill();
                trainingProcess.Dispose();
                trainingProcess = null;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[TrainingLauncher] Error stopping training: {ex.Message}");
            }
        }

        if (EditorApplication.isPlaying)
        {
            UnityEngine.Debug.Log("[TrainingLauncher] Stopping Play Mode...");
            EditorApplication.isPlaying = false;
        }
    }

    private T GetValue<T>(Dictionary<object, object> dict, string key)
    {
        if (dict.ContainsKey(key))
        {
            try
            {
                return (T)System.Convert.ChangeType(dict[key], typeof(T));
            }
            catch
            {
                UnityEngine.Debug.LogWarning($"Could not convert {key} to type {typeof(T)}.");
            }
        }
        return default;
    }
}

[System.Serializable]
public class PPOConfig
{
    public int batch_size;
    public int buffer_size;
    public float learning_rate;
    public float beta;
    public float epsilon;
    public float entropy_coefficient;
}
}