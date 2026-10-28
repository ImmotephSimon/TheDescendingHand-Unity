using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Registries/Animation")]
public class AnimationRegistry : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        [SerializeField] private string _id;
        [SerializeField] private AnimationClip _clip;

        public string Id => _id;
        public AnimationClip Clip => _clip;

        public Entry(string id, AnimationClip clip)
        {
            _id = id;
            _clip = clip;
        }
    }

    [SerializeField] private List<Entry> _entries = new();

    private Dictionary<AnimationClip, string> _clipToId;
    private Dictionary<string, AnimationClip> _idToClip;

    private static AnimationRegistry _instance;
    private static bool _loadAttempted;

    public static AnimationRegistry Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            if (_loadAttempted)
                return null;

            _loadAttempted = true;
            _instance = Resources.Load<AnimationRegistry>("AnimationRegistry");

            if (_instance == null)
                Debug.LogError("AnimationRegistry not found in Resources/AnimationRegistry.asset.");

            return _instance;
        }
    }

    public IReadOnlyList<Entry> Entries => _entries;

    public bool SetGeneratedEntries(List<Entry> entries)
    {
        if (_entries.Count == entries.Count)
        {
            bool identical = true;

            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Id != entries[i].Id ||
                    _entries[i].Clip != entries[i].Clip)
                {
                    identical = false;
                    break;
                }
            }

            if (identical)
                return false;
        }

        _entries = entries;
        _clipToId = null;
        _idToClip = null;
        return true;
    }

    private void EnsureInit()
    {
        if (_clipToId != null)
            return;

        _clipToId = new Dictionary<AnimationClip, string>();
        _idToClip = new Dictionary<string, AnimationClip>();

        foreach (Entry entry in _entries)
        {
            if (entry.Clip == null || string.IsNullOrEmpty(entry.Id))
                continue;

            _clipToId[entry.Clip] = entry.Id;
            _idToClip[entry.Id] = entry.Clip;
        }
    }

    public bool TryGetId(AnimationClip clip, out string id)
    {
        EnsureInit();

        if (clip != null && _clipToId.TryGetValue(clip, out id))
            return true;

        id = null;
        Debug.LogError($"[AnimationRegistry] No ID registered for clip '{clip?.name}'.");
        return false;
    }

    public bool TryGetClip(AnimationString key, out AnimationClip clip)
    {
        EnsureInit();

        if (!string.IsNullOrEmpty(key.Id) && _idToClip.TryGetValue(key.Id, out clip))
            return true;

        clip = null;
        Debug.LogError($"[AnimationRegistry] No clip registered for ID '{key.Id}'.");
        return false;
    }
}