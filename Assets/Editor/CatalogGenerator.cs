using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public interface ICatalogBuilder
{
    bool Rebuild(CatalogAssetIndex index);
}

public static class CatalogGenerator
{
    private const string IndexPath = "Assets/Editor/CatalogAssetIndex.asset";

    [MenuItem("Tools/Catalogs/Rebuild Index and Catalogs")]
    public static void RebuildIndexAndCatalogs()
    {
        CatalogAssetIndex index = GetOrCreateIndex(out bool created);
        List<CatalogAssetRecord> entries = ScanAssets();
        bool changed = index.SetEntries(entries) || created;

        if (changed)
            EditorUtility.SetDirty(index);

        foreach (Type type in TypeCache.GetTypesDerivedFrom<ICatalogBuilder>())
        {
            if (type.IsAbstract || type.IsInterface || type.ContainsGenericParameters)
                continue;

            try
            {
                if (Activator.CreateInstance(type) is ICatalogBuilder builder)
                    changed |= builder.Rebuild(index);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        if (changed)
            AssetDatabase.SaveAssets();

        Debug.Log($"Catalog generation complete. Indexed {index.Entries.Count} assets.");
    }

    private static CatalogAssetIndex GetOrCreateIndex(out bool created)
    {
        CatalogAssetIndex index = AssetDatabase.LoadAssetAtPath<CatalogAssetIndex>(IndexPath);
        created = index == null;

        if (!created)
            return index;

        if (!AssetDatabase.IsValidFolder("Assets/Editor"))
            AssetDatabase.CreateFolder("Assets", "Editor");

        index = ScriptableObject.CreateInstance<CatalogAssetIndex>();
        AssetDatabase.CreateAsset(index, IndexPath);
        EditorUtility.SetDirty(index);
        return index;
    }

    private static List<CatalogAssetRecord> ScanAssets()
    {
        var recordsById = new Dictionary<string, CatalogAssetRecord>(StringComparer.Ordinal);

        // Only search asset types that can contribute to the catalogs.
        AddPrefabAssets(recordsById, AssetDatabase.FindAssets("t:Prefab"));
        AddSpriteAssets(recordsById, AssetDatabase.FindAssets("t:Texture2D"));

        var animationGuids = new HashSet<string>(StringComparer.Ordinal);
        AddGuids(animationGuids, AssetDatabase.FindAssets("t:AnimationClip"));
        AddGuids(animationGuids, AssetDatabase.FindAssets("t:Model"));
        AddAnimationAssets(recordsById, animationGuids);

        var entries = new List<CatalogAssetRecord>(recordsById.Values);
        entries.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
        return entries;
    }

    private static void AddPrefabAssets(
        Dictionary<string, CatalogAssetRecord> recordsById,
        string[] guids)
    {
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null || !ContainsVfx(prefab))
                continue;

            AddAsset(recordsById, prefab, path, CatalogAssetKind.VfxPrefab);
        }
    }

    private static void AddSpriteAssets(
        Dictionary<string, CatalogAssetRecord> recordsById,
        string[] guids)
    {
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite sprite)
                    AddAsset(recordsById, sprite, path, CatalogAssetKind.Sprite);
            }
        }
    }

    private static void AddAnimationAssets(
        Dictionary<string, CatalogAssetRecord> recordsById,
        HashSet<string> guids)
    {
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is AnimationClip clip)
                    AddAsset(recordsById, clip, path, CatalogAssetKind.AnimationClip);
            }
        }
    }

    private static void AddGuids(HashSet<string> destination, string[] guids)
    {
        foreach (string guid in guids)
            destination.Add(guid);
    }

    public static bool ContainsVfx(GameObject prefab)
    {
        foreach (MonoBehaviour component in prefab.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (component is IVfx)
                return true;
        }

        return false;
    }

    private static void AddAsset(
        Dictionary<string, CatalogAssetRecord> recordsById,
        UnityEngine.Object asset,
        string path,
        CatalogAssetKind kind)
    {
        if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long localId))
            return;

        string id = $"{guid}:{localId}";

        if (recordsById.TryGetValue(id, out CatalogAssetRecord existing))
        {
            existing.AddKind(kind);
            return;
        }

        recordsById.Add(id, new CatalogAssetRecord(id, path, kind, asset));
    }
}
