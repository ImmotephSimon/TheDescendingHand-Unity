#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

internal static class SpriteCatalogBuilder
{
    private const string ResourcesFolder = "Assets/Resources";
    private const string CatalogPath = ResourcesFolder + "/" + SpriteCatalog.ResourcesAssetName + ".asset";

    // Matches "<32-hex guid>:<local file id>", the format SpriteString IDs are stored in.
    private static readonly Regex IdRegex =
        new Regex(@"[0-9a-fA-F]{32}:-?\d+", RegexOptions.Compiled);

    [MenuItem("Tools/Sprites/Rebuild Generated Sprite Catalog")]
    private static void RebuildFromMenu()
    {
        int count = RebuildCatalog();
        Debug.Log($"Sprite catalog rebuilt. {count} referenced sprites.");
    }

    internal static int RebuildCatalog()
    {
        EnsureResourcesFolder();

        SpriteCatalog catalog = AssetDatabase.LoadAssetAtPath<SpriteCatalog>(CatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<SpriteCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }

        HashSet<string> referencedIds = ScanReferencedIds();
        var entriesById = new SortedDictionary<string, SpriteCatalog.Entry>(StringComparer.Ordinal);

        // Group by GUID so each texture file is only loaded once.
        var idsByGuid = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (string id in referencedIds)
        {
            string guid = id.Substring(0, id.IndexOf(':'));
            if (!idsByGuid.TryGetValue(guid, out var list))
                idsByGuid[guid] = list = new List<string>();
            list.Add(id);
        }

        try
        {
            int i = 0;
            foreach (var kvp in idsByGuid)
            {
                string path = AssetDatabase.GUIDToAssetPath(kvp.Key);
                EditorUtility.DisplayProgressBar("Rebuilding Sprite Catalog", path, (float)i++ / idsByGuid.Count);

                if (string.IsNullOrEmpty(path))
                {
                    foreach (string id in kvp.Value)
                        Debug.LogWarning($"SpriteString references missing asset: {id}");
                    continue;
                }

                var wanted = new HashSet<string>(kvp.Value, StringComparer.Ordinal);

                foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (!(asset is Sprite sprite))
                        continue;

                    if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                            sprite, out string guid, out long localId))
                        continue;

                    string id = guid + ":" + localId.ToString(CultureInfo.InvariantCulture);
                    if (!wanted.Remove(id))
                        continue;

                    entriesById[id] = new SpriteCatalog.Entry
                    {
                        Id = new SpriteString(id),
                        Sprite = sprite
                    };
                }

                foreach (string missing in wanted)
                    Debug.LogWarning($"SpriteString references a sprite that no longer exists: {missing} ({path})");
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        catalog.Entries.Clear();
        catalog.Entries.AddRange(entriesById.Values);
        catalog.InvalidateLookup();
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssetIfDirty(catalog);
        return catalog.Entries.Count;
    }

    private static HashSet<string> ScanReferencedIds()
    {
        if (EditorSettings.serializationMode != SerializationMode.ForceText)
            Debug.LogWarning("Sprite catalog scan reads serialized YAML. Set Asset Serialization to Force Text, " +
                             "otherwise references will be missed.");

        var ids = new HashSet<string>(StringComparer.Ordinal);
        string[] allPaths = AssetDatabase.GetAllAssetPaths();

        try
        {
            for (int i = 0; i < allPaths.Length; i++)
            {
                string path = allPaths[i];
                if (!path.StartsWith("Assets/", StringComparison.Ordinal) || path == CatalogPath)
                    continue; // skip packages and the catalog itself (it holds every ID it lists)

                if (!(path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) ||
                      path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase) ||
                      path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)))
                    continue;

                if (i % 100 == 0)
                    EditorUtility.DisplayProgressBar("Scanning SpriteString references", path, (float)i / allPaths.Length);

                string text;
                try { text = File.ReadAllText(path); }
                catch (IOException) { continue; }

                foreach (Match m in IdRegex.Matches(text))
                    ids.Add(m.Value.ToLowerInvariant());
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        return ids;
    }

    private static void EnsureResourcesFolder()
    {
        if (!AssetDatabase.IsValidFolder(ResourcesFolder))
            AssetDatabase.CreateFolder("Assets", "Resources");
    }
}

internal sealed class SpriteCatalogBuildPreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder => -10000;

    public void OnPreprocessBuild(BuildReport report)
    {
        SpriteCatalogBuilder.RebuildCatalog();
    }
}
#endif