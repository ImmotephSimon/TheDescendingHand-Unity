#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

internal static class AnimationRegistryBuilder
{
    private const string ResourcesFolder = "Assets/Resources";
    private const string RegistryPath = ResourcesFolder + "/AnimationRegistry.asset";
    private static readonly string[] SearchFolders = { "Assets/Animations" };

    [MenuItem("Tools/Animations/Rebuild Animation Registry")]
    private static void RebuildFromMenu()
    {
        int count = RebuildRegistry();
        Debug.Log($"Animation registry rebuilt. Found {count} clips.");
    }

    internal static int RebuildRegistry()
    {
        if (!AssetDatabase.IsValidFolder(ResourcesFolder))
            AssetDatabase.CreateFolder("Assets", "Resources");

        AnimationRegistry registry = AssetDatabase.LoadAssetAtPath<AnimationRegistry>(RegistryPath);
        if (registry == null)
        {
            registry = ScriptableObject.CreateInstance<AnimationRegistry>();
            AssetDatabase.CreateAsset(registry, RegistryPath);
        }

        var entriesById = new SortedDictionary<string, AnimationRegistry.Entry>(StringComparer.Ordinal);
        var paths = new HashSet<string>(StringComparer.Ordinal);

        foreach (string assetGuid in AssetDatabase.FindAssets("t:AnimationClip", SearchFolders))
        {
            string path = AssetDatabase.GUIDToAssetPath(assetGuid);
            if (string.IsNullOrEmpty(path) || !paths.Add(path))
                continue;

            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (!(asset is AnimationClip clip))
                    continue;

                // Unity auto-generates these for every FBX with animation.
                if (clip.name.StartsWith("__preview__", StringComparison.Ordinal))
                    continue;

                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                        clip, out string guid, out long localId))
                    continue;

                string id = guid + ":" + localId.ToString(CultureInfo.InvariantCulture);
                if (!entriesById.ContainsKey(id))
                    entriesById.Add(id, new AnimationRegistry.Entry(id, clip));
            }
        }

        var entries = new List<AnimationRegistry.Entry>(entriesById.Values);
        if (registry.SetGeneratedEntries(entries))
        {
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
        }

        return entries.Count;
    }
}

internal sealed class AnimationRegistryAssetWatcher : AssetPostprocessor
{
    private static bool _rebuildQueued;

    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".anim", ".fbx", ".controller"
    };

    public static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (!HasRelevant(importedAssets) && !HasRelevant(deletedAssets) &&
            !HasRelevant(movedAssets) && !HasRelevant(movedFromAssetPaths))
            return;

        if (_rebuildQueued)
            return;

        _rebuildQueued = true;
        EditorApplication.delayCall += () =>
        {
            _rebuildQueued = false;
            AnimationRegistryBuilder.RebuildRegistry();
        };
    }

    private static bool HasRelevant(string[] paths)
    {
        foreach (string path in paths)
            if (Extensions.Contains(System.IO.Path.GetExtension(path)))
                return true;
        return false;
    }
}

internal sealed class AnimationRegistryBuildPreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder => -10000;

    public void OnPreprocessBuild(BuildReport report) =>
        AnimationRegistryBuilder.RebuildRegistry();
}
#endif