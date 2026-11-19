#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

internal sealed class WorldModelCatalogBuilder : ICatalogBuilder
{
    private const string ResourcesFolder = "Assets/Resources";

    private const string CatalogPath =
        ResourcesFolder + "/" +
        WorldModelCatalog.ResourcesAssetName + ".asset";

    public bool Rebuild(CatalogAssetIndex index)
    {
        RebuildCatalog();
        return true;
    }

    internal int RebuildCatalog()
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

        var entries = new SortedDictionary<string, WorldModelCatalog.Entry>(StringComparer.Ordinal);
        var seenPaths = new HashSet<string>();

        foreach (var type in TypeCache.GetTypesDerivedFrom<IWorldModelSource>())
        {
            if (type.IsAbstract || !typeof(ScriptableObject).IsAssignableFrom(type))
                continue;

            foreach (string guid in AssetDatabase.FindAssets($"t:{type.Name}"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!seenPaths.Add(path)) continue; // t:Base also matches subclasses

                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset is not IWorldModelSource src) continue;
                if (src.Id == Guid.Empty || src.WorldModel == null) continue;

                string key = src.Id.ToString();

                if (!entries.TryAdd(key, new WorldModelCatalog.Entry
                { Id = key, Prefab = src.WorldModel }))
                {
                    Debug.LogError($"Duplicate catalog ID '{key}' on '{asset.name}'.", asset);
                }
            }
        }

        catalog.ReplaceEntries(entries.Values.ToList());

        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();

        return entries.Count;
    }
}
#endif