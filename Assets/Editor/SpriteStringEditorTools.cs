#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;


/// <summary>
/// Regenerates the catalog from Sprite assets across the project. It scans
/// Texture2D assets as well as Sprite assets so sliced sprites (sub-assets)
/// are included too.
/// </summary>
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

        IEnumerable<string> assetGuids =
            AssetDatabase.FindAssets("t:Texture2D")
                .Concat(AssetDatabase.FindAssets("t:Sprite"));

        foreach (string assetGuid in assetGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(assetGuid);
            if (string.IsNullOrEmpty(path) || !paths.Add(path))
                continue;

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

        catalog.Entries.Clear();
        catalog.Entries.AddRange(entriesById.Values);
        catalog.InvalidateLookup();
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        return catalog.Entries.Count;
    }

    private static void EnsureResourcesFolder()
    {
        if (!AssetDatabase.IsValidFolder(ResourcesFolder))
            AssetDatabase.CreateFolder("Assets", "Resources");
    }
}

internal sealed class SpriteCatalogAssetWatcher : AssetPostprocessor
{
    private static bool _rebuildQueued;

    private static readonly HashSet<string> ImageExtensions = new(
        StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".psd",
        ".tga", ".tif", ".tiff", ".bmp",
        ".gif", ".exr", ".hdr", ".webp"
    };

    public static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (!HasSpriteChanges(importedAssets, false) &&
            !HasSpriteChanges(deletedAssets, true) &&
            !HasSpriteChanges(movedAssets, false) &&
            !HasSpriteChanges(movedFromAssetPaths, true))
            return;

        if (_rebuildQueued)
            return;

        _rebuildQueued = true;
        UnityEditor.EditorApplication.delayCall += Rebuild;
    }

    private static bool HasSpriteChanges(string[] paths, bool deleted)
    {
        foreach (string path in paths)
        {
            string extension = System.IO.Path.GetExtension(path);

            if (ImageExtensions.Contains(extension))
                return true;

            if (deleted && extension.Equals(
                    ".asset", StringComparison.OrdinalIgnoreCase))
                return true;

            if (!deleted)
            {
                foreach (var asset in
                         AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (asset is Sprite)
                        return true;
                }
            }
        }

        return false;
    }

    private static void Rebuild()
    {
        _rebuildQueued = false;
        SpriteCatalogBuilder.RebuildCatalog();
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
