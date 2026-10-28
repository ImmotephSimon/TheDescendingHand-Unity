using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum CatalogAssetKind
{
    None = 0,
    Sprite = 1,
    VfxPrefab = 2,
    AnimationClip = 4
}

[Serializable]
public sealed class CatalogAssetRecord
{
    [SerializeField] private string _id;
    [SerializeField] private string _assetPath;
    [SerializeField] private CatalogAssetKind _kind;
    [SerializeField] private UnityEngine.Object _asset;

    public string Id => _id;
    public string AssetPath => _assetPath;
    public CatalogAssetKind Kind => _kind;
    public UnityEngine.Object Asset => _asset;

    public CatalogAssetRecord(string id, string assetPath, CatalogAssetKind kind, UnityEngine.Object asset)
    {
        _id = id;
        _assetPath = assetPath;
        _kind = kind;
        _asset = asset;
    }

    internal void AddKind(CatalogAssetKind kind)
    {
        _kind |= kind;
    }
}

public sealed class CatalogAssetIndex : ScriptableObject
{
    [SerializeField] private List<CatalogAssetRecord> _entries = new();

    public IReadOnlyList<CatalogAssetRecord> Entries => _entries;

    public bool SetEntries(List<CatalogAssetRecord> entries)
    {
        if (_entries.Count == entries.Count)
        {
            bool identical = true;

            for (int i = 0; i < _entries.Count; i++)
            {
                CatalogAssetRecord current = _entries[i];
                CatalogAssetRecord next = entries[i];

                if (current.Id != next.Id ||
                    current.AssetPath != next.AssetPath ||
                    current.Kind != next.Kind ||
                    current.Asset != next.Asset)
                {
                    identical = false;
                    break;
                }
            }

            if (identical)
                return false;
        }

        _entries = entries;
        return true;
    }
}
