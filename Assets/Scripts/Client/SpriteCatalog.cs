using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Generated catalog mapping stable Sprite IDs to local Sprite references.
/// The asset is placed in Resources so clients can load it without a scene
/// reference. The editor generator populates Entries automatically.
/// </summary>
public sealed class SpriteCatalog : ScriptableObject
{
    public const string ResourcesAssetName = "GeneratedSpriteCatalog";

    [Serializable]
    public sealed class Entry
    {
        public SpriteString Id;
        public Sprite Sprite;
    }

    [SerializeField] private List<Entry> _entries = new List<Entry>();

    // Used by the Editor generator. Do not edit this list manually.
    public List<Entry> Entries => _entries;

    [NonSerialized] private Dictionary<string, Sprite> _lookup;
    private static SpriteCatalog _instance;

    private static bool _loadAttempted;

    public static SpriteCatalog Instance
    {
        get
        {
            if (_instance == null && !_loadAttempted)
            {
                _loadAttempted = true;
                _instance = Resources.Load<SpriteCatalog>(ResourcesAssetName);

                if (_instance == null)
                    Debug.LogError(
                        $"SpriteCatalog: Failed to load Resources/{ResourcesAssetName}.asset. " +
                        "Ensure the generated catalog exists in a Resources folder.");
            }

            return _instance;
        }
    }

    public bool TryGetSprite(
        SpriteString id,
        out Sprite sprite,
        [CallerMemberName] string callerMember = "",
        [CallerFilePath] string callerFile = "",
        [CallerLineNumber] int callerLine = 0)
    {
        EnsureLookup();

        sprite = null;
        if (id.IsEmpty)
        {
            Debug.LogError($"[SpriteCatalog] TryGetSprite called with an empty SpriteString ID. " +
                           $"Caller: {callerMember} ({Path.GetFileName(callerFile)}:{callerLine})");
            return false;
        }

        if (_lookup.TryGetValue(id.Id, out sprite))
            return true;

        Debug.LogError($"[SpriteCatalog] No sprite registered for ID '{id.Id}'. " +
                       $"Caller: {callerMember} ({Path.GetFileName(callerFile)}:{callerLine})");
        return false;
    }

    /// <summary>Call after regenerating Entries in the Editor. Safe at runtime too.</summary>
    public void InvalidateLookup()
    {
        _lookup = null;
    }

    private void EnsureLookup()
    {
        if (_lookup != null)
            return;

        _lookup = new Dictionary<string, Sprite>(StringComparer.Ordinal);

        foreach (Entry entry in _entries)
        {
            if (entry == null)
            {
                Debug.LogError("[SpriteCatalog] Entry is null.", this);
                continue;
            }

            if (entry.Sprite == null)
            {
                Debug.LogError($"[SpriteCatalog] Entry '{entry.Id.Id}' has a missing sprite.", this);
                continue;
            }

            if (entry.Id.IsEmpty)
            {
                Debug.LogError($"[SpriteCatalog] Entry for sprite '{entry.Sprite.name}' has a missing ID.", this);
                continue;
            }

            if (!_lookup.TryAdd(entry.Id.Id, entry.Sprite))
                Debug.LogError($"[SpriteCatalog] Duplicate ID '{entry.Id.Id}'.", this);
        }
    }

    private void OnEnable()
    {
        _lookup = null;
    }
}
