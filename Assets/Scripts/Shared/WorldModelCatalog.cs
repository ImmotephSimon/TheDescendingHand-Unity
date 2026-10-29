using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Catalogs/World Model Catalog")]
public class WorldModelCatalog : ScriptableObject
{
    public const string ResourcesAssetName = "WorldModelCatalog";

    [Serializable]
    public class Entry
    {
        public string ItemId;
        public GameObject Prefab;
    }

    [SerializeField] private List<Entry> entries = new();

    private Dictionary<Guid, GameObject> _lookup;

    private static WorldModelCatalog _instance;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => _instance = null;

    public static WorldModelCatalog Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<WorldModelCatalog>(
                    ResourcesAssetName);

                _instance?.Initialize();
            }

            return _instance;
        }
    }

    public void ReplaceEntries(List<Entry> newEntries)
    {
        entries = newEntries ?? new List<Entry>();
        _lookup = null;
    }

    private void Initialize()
    {
        if (_lookup != null)
            return;

        _lookup = new Dictionary<Guid, GameObject>();

        foreach (var entry in entries)
        {
            if (entry == null || entry.Prefab == null ||
                !Guid.TryParse(entry.ItemId, out var id))
                continue;

            if (!_lookup.TryAdd(id, entry.Prefab))
            {
                Debug.LogError(
                    $"[WorldModelCatalog] Duplicate item ID '{id}'.", this);
            }
        }
    }

    public bool TryGetPrefab(Guid id, out GameObject prefab)
    {
        Initialize();

        if (_lookup.TryGetValue(id, out prefab) && prefab != null)
            return true;

        prefab = null;
        return false;
    }
}