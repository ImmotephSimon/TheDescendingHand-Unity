using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Registries/Vfx")]
public class VfxRegistry : ScriptableObject
{
    [SerializeField] private List<VfxEntry> entries = new();

    private Dictionary<GameObject, string> _prefabToId;
    private Dictionary<string, GameObject> _idToPrefab;
    private static VfxRegistry _instance;
    private static bool _loadAttempted;

    public static VfxRegistry Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            if (_loadAttempted)
                return null;

            _loadAttempted = true;
            _instance = Resources.Load<VfxRegistry>("VfxRegistry");

            if (_instance == null)
                Debug.LogError("VfxRegistry not found in Resources/VfxRegistry.asset.");

            return _instance;
        }
    }

    private void EnsureInit()
    {
        if (_prefabToId != null)
            return;

        _prefabToId = new Dictionary<GameObject, string>();
        _idToPrefab = new Dictionary<string, GameObject>();

        foreach (var entry in entries)
        {
            if (entry.Prefab == null || string.IsNullOrEmpty(entry.Id))
                continue;

            _prefabToId[entry.Prefab] = entry.Id;
            _idToPrefab[entry.Id] = entry.Prefab;
        }
    }

    public bool TryGetId(GameObject prefab, out string id)
    {
        EnsureInit();

        if (_prefabToId.TryGetValue(prefab, out id))
            return true;

        Debug.LogError(
            $"[VfxRegistry] No ID registered for prefab '{prefab?.name}'.");
        return false;
    }

    public bool TryGetPrefab(string id, out GameObject prefab)
    {
        EnsureInit();

        if (_idToPrefab.TryGetValue(id, out prefab))
            return true;

        Debug.LogError(
            $"[VfxRegistry] No prefab registered for ID '{id}'.");
        return false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        bool changed = false;
        HashSet<string> seenIds = new();
        HashSet<GameObject> seenPrefabs = new();

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];

            // New entry or otherwise missing an ID.
            if (string.IsNullOrEmpty(entry.Id))
            {
                entry.SetId(Guid.NewGuid().ToString());
                entries[i] = entry;
                changed = true;
            }

            // Unity can copy the previous struct's serialized ID
            // when adding a new list element.
            if (!seenIds.Add(entry.Id))
            {
                entry.SetId(Guid.NewGuid().ToString());
                entries[i] = entry;
                seenIds.Add(entry.Id);
                changed = true;
            }

            if (entry.Prefab != null && !seenPrefabs.Add(entry.Prefab))
            {
                Debug.LogError(
                    $"[VfxRegistry] Duplicate Prefab assigned: {entry.Prefab.name}",
                    this);
            }
        }

        if (changed)
            UnityEditor.EditorUtility.SetDirty(this);

        _prefabToId = null;
        _idToPrefab = null;
    }
#endif
}

[Serializable]
public struct VfxEntry
{
    [SerializeField, HideInInspector] private string id;
    [SerializeField] private GameObject prefab;

    public readonly string Id => id;
    public readonly GameObject Prefab => prefab;

    public void SetId(string value) => id = value;
}