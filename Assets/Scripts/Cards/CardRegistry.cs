using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Card Registry")]
public class CardRegistry : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string CardId;

        public GameObject ProjectilePrefab;
        public GameObject MinionPrefab;
        public GameObject EffectPrefab;
    }

    [SerializeField] private List<Entry> entries = new();

    private Dictionary<string, Entry> _lookup;

    public void Initialize()
    {
        _lookup = new();

        foreach (var entry in entries)
            _lookup[entry.CardId] = entry;
    }

    public bool TryGet(string id, out Entry entry)
    {
        return _lookup.TryGetValue(id, out entry);
    }
}