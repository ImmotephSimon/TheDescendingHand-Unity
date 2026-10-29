#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

internal static class WorldModelCatalogBuilder
{
    private const string ResourcesFolder = "Assets/Resources";

    private const string CatalogPath =
        ResourcesFolder + "/" +
        WorldModelCatalog.ResourcesAssetName + ".asset";

    [MenuItem("Tools/Items/Rebuild World Model Catalog")]
    private static void RebuildFromMenu()
    {
        int count = RebuildCatalog();
        Debug.Log($"World model catalog rebuilt: {count} entries.");
    }

    internal static int RebuildCatalog()
    {
        if (!AssetDatabase.IsValidFolder(ResourcesFolder))
            AssetDatabase.CreateFolder("Assets", "Resources");

        var catalog =
            AssetDatabase.LoadAssetAtPath<WorldModelCatalog>(CatalogPath);

        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<WorldModelCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }

        var entries = new SortedDictionary<string, WorldModelCatalog.Entry>(
            StringComparer.Ordinal);

        foreach (string assetGuid in AssetDatabase.FindAssets("t:ItemDefinition"))
        {
            string path = AssetDatabase.GUIDToAssetPath(assetGuid);
            var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);

            if (item == null || item.Id == Guid.Empty)
                continue;

            // Items without world models are intentionally omitted.
            if (item.Appearance.WorldModel == null)
                continue;

            string id = item.Id.ToString("D");

            if (entries.ContainsKey(id))
            {
                Debug.LogError(
                    $"Duplicate item ID '{id}' on '{item.name}'.", item);
                continue;
            }

            entries.Add(id, new WorldModelCatalog.Entry
            {
                ItemId = id,
                Prefab = item.Appearance.WorldModel
            });
        }

        catalog.ReplaceEntries(entries.Values.ToList());

        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();

        return entries.Count;
    }
}

internal sealed class WorldModelCatalogBuildPreprocessor
    : IPreprocessBuildWithReport
{
    public int callbackOrder => -10000;

    public void OnPreprocessBuild(BuildReport report)
    {
        WorldModelCatalogBuilder.RebuildCatalog();
    }
}
#endif