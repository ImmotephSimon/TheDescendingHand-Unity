#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

internal static class SpriteCatalogBuilder
{
    private const string ResourcesFolder = "Assets/Resources";
    private const string CatalogPath = ResourcesFolder + "/" + SpriteCatalog.ResourcesAssetName + ".asset";

    [MenuItem("Tools/Sprites/Rebuild Generated Sprite Catalog")]
    private static void RebuildFromMenu()
    {
        int count = RebuildCatalog();
        Debug.Log($"Sprite catalog rebuilt. Found {count} sprites.");
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

        var entriesById = new SortedDictionary<string, SpriteCatalog.Entry>(StringComparer.Ordinal);
        var paths = new HashSet<string>(StringComparer.Ordinal);

        string[] assetGuids =
            AssetDatabase.FindAssets("t:Texture2D")
                .Concat(AssetDatabase.FindAssets("t:Sprite"))
                .ToArray();

        try
        {
            for (int i = 0; i < assetGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                if (string.IsNullOrEmpty(path) || !paths.Add(path))
                    continue;

                if (i % 50 == 0)
                    EditorUtility.DisplayProgressBar("Rebuilding Sprite Catalog", path, (float)i / assetGuids.Length);

                foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (!(asset is Sprite sprite))
                        continue;

                    if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                            sprite, out string guid, out long localId))
                        continue;

                    string id = guid + ":" + localId.ToString(CultureInfo.InvariantCulture);
                    if (!entriesById.ContainsKey(id))
                    {
                        entriesById.Add(id, new SpriteCatalog.Entry
                        {
                            Id = new SpriteString(id),
                            Sprite = sprite
                        });
                    }
                }
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