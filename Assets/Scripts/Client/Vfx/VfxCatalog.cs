using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(menuName = "Registries/Vfx")]
public class VfxCatalog : ScriptableObject
{
    [SerializeField] private List<VfxEntry> entries = new();

    private Dictionary<GameObject, VfxType> _prefabToId;
    private Dictionary<VfxType, GameObject> _idToPrefab;
    private static VfxCatalog _instance;
    private static bool _loadAttempted;

    public static VfxCatalog Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            if (_loadAttempted)
                return null;

            _loadAttempted = true;
            _instance = Resources.Load<VfxCatalog>("VfxCatalog");

            if (_instance == null)
                Debug.LogError("VfxCatalog not found at Resources/VfxCatalog.asset.");

            return _instance;
        }
    }

    private void EnsureInit()
    {
        if (_prefabToId != null)
            return;

        _prefabToId = new();
        _idToPrefab = new();

        foreach (VfxEntry entry in entries)
        {
            if (entry.Prefab == null)
            {
                Debug.LogError("[VfxCatalog] Entry has a missing prefab.", this);
                continue;
            }

            if (entry.Id == VfxType.None)
            {
                Debug.LogError($"[VfxCatalog] Entry for prefab '{entry.Prefab.name}' has a missing or None ID.", this);
                continue;
            }

            _prefabToId[entry.Prefab] = entry.Id;
            _idToPrefab[entry.Id] = entry.Prefab;
        }
    }

    public bool TryGetId(GameObject prefab, out VfxType id)
    {
        EnsureInit();

        if (_prefabToId.TryGetValue(prefab, out id))
            return true;

        Debug.LogError($"[VfxRegistry] No ID registered for prefab '{prefab?.name}'.");
        return false;
    }

    public bool TryGetPrefab(
        VfxType id,
        out GameObject prefab,
        UnityEngine.Object context = null,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        prefab = null;

        if (id == VfxType.None)
        {
            Debug.LogError(
                $"[VfxCatalog] VFX ID is None (nothing assigned). {Describe(context, caller, file, line)}",
                context);
            return false;
        }

        EnsureInit();

        if (_idToPrefab.TryGetValue(id, out prefab))
            return true;

        prefab = null;
        Debug.LogError(
            $"[VfxCatalog] No prefab registered for ID '{id}'. " +
            $"Registered: [{string.Join(", ", _idToPrefab.Keys)}]. " +
            $"{Describe(context, caller, file, line)}",
            context);
        return false;
    }

    private static string Describe(UnityEngine.Object context, string caller, string file, int line)
    {
        string objectName = context != null ? context.name : "unknown";
        return $"Object: '{objectName}', at {caller} ({Path.GetFileName(file)}:{line})";
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _prefabToId = null;
        _idToPrefab = null;
    }

    public bool SetGeneratedEntries(List<VfxEntry> newEntries)
    {
        if (entries.Count == newEntries.Count)
        {
            bool same = true;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Id != newEntries[i].Id || entries[i].Prefab != newEntries[i].Prefab)
                {
                    same = false;
                    break;
                }
            }
            if (same) return false;
        }

        entries = new List<VfxEntry>(newEntries);
        _prefabToId = null;
        _idToPrefab = null;
        return true;
    }
#endif
}

[Serializable]
public struct VfxEntry
{
    [SerializeField] private VfxType id;
    [SerializeField] private GameObject prefab;

    public readonly VfxType Id => id;
    public readonly GameObject Prefab => prefab;

    public VfxEntry(VfxType id, GameObject prefab)
    {
        this.id = id;
        this.prefab = prefab;
    }
}