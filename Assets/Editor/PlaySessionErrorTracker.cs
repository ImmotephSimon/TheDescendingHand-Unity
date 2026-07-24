#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PlaySessionErrorTracker
{
    private static readonly HashSet<string> UniqueErrors = new();
    private static readonly HashSet<string> UniqueWarnings = new();

    static PlaySessionErrorTracker()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            UniqueErrors.Clear();
            UniqueWarnings.Clear();
            Application.logMessageReceived += CountLogs;
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Application.logMessageReceived -= CountLogs;

            if (UniqueErrors.Count > 0)
            {
                EditorUtility.DisplayDialog(
                    "Play Session Ended",
                    $"⚠️ {UniqueErrors.Count} Unique Error(s) and {UniqueWarnings.Count} Unique Warning(s) occurred.\n\nCheck the Console tab for details.",
                    "OK"
                );
            }
        }
    }

    private static void CountLogs(string logString, string stackTrace, LogType type)
    {
        // Key on stackTrace (or logString if stackTrace is empty) to isolate unique code sites
        string key = string.IsNullOrEmpty(stackTrace) ? logString : stackTrace;

        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            UniqueErrors.Add(key);
        else if (type == LogType.Warning)
            UniqueWarnings.Add(key);
    }
}
#endif