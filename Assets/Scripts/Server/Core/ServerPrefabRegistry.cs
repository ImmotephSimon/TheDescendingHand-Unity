using FishNet.Managing.Object;
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
    [SerializeField] private List<Entry> _entries = new();
    [SerializeField] private DefaultPrefabObjects _clientCollection;
    [SerializeField] private DefaultPrefabObjects _serverCollection;

    public DefaultPrefabObjects ClientCollection => _clientCollection;
    public DefaultPrefabObjects ServerCollection => _serverCollection;

    private static ServerPrefabRegistry _instance;
    public static ServerPrefabRegistry Instance => _instance;

    public static void SetInstance(ServerPrefabRegistry registry) => _instance = registry;

    private Dictionary<GameTag, GameObject> _lookup;

    private void OnEnable() => _lookup = null;

    public bool TryGetPrefab(GameTag tag, out GameObject prefab)
    {
        if (_lookup == null)
            CreateLookup();

        if (_lookup.TryGetValue(tag, out prefab))
            return true;

        Debug.LogError($"ServerPrefabRegistry: No prefab registered for tag '{tag.TagId}'.");
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

    public bool TryInstantiate(GameTag tag, Vector3 position, out GameObject obj)
    {
        obj = null;
        if (!TryGetPrefab(tag, out var prefab))
            return false;

        obj = Instantiate(prefab, position, Quaternion.identity);
        return true;
    }

}