using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Registries/Server Prefabs")]
public class ServerPrefabRegistry : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public GameTag Tag;
        public GameObject Prefab;
    }

    private static ServerPrefabRegistry _instance;

    public static ServerPrefabRegistry Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<ServerPrefabRegistry>("ServerPrefabRegistry");
            return _instance;
        }
    }

    [SerializeField] private List<Entry> _entries = new();
    private Dictionary<GameTag, GameObject> _lookup;

    private void OnEnable() => _lookup = null;

    public bool TryGetPrefab(GameTag tag, out GameObject prefab)
    {
        if (_lookup == null)
        {
            CreateLookup();
        }

        if (_lookup.TryGetValue(tag, out prefab))
            return true;

        Debug.LogError($"ServerPrefabRegistry: No prefab registered for tag '{tag}'.");
        return false;
    }

    private void CreateLookup()
    {
        _lookup = new Dictionary<GameTag, GameObject>(_entries.Count);

        foreach (var entry in _entries)
        {
            if (entry.Prefab != null)
                _lookup[entry.Tag] = entry.Prefab;
        }
    }
}