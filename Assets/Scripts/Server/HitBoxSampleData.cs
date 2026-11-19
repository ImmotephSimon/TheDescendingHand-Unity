using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HitBoxSampleData
{
    public List<HitBoxSampleEntry> entries = new();

    private Dictionary<AttackAnimation, List<HitBoxSample>> _lookup;

    public List<HitBoxSample> this[AttackAnimation animation]
        => (_lookup ??= BuildLookup())[animation];

    private Dictionary<AttackAnimation, List<HitBoxSample>> BuildLookup()
    {
        var dict = new Dictionary<AttackAnimation, List<HitBoxSample>>();
        foreach (var entry in entries)
            dict[entry.animation] = entry.samples;
        return dict;
    }

}

[System.Serializable]
public class HitBoxSampleEntry
{
    public AttackAnimation animation;
    public List<HitBoxSample> samples = new();
}

[System.Serializable]
public class HitBoxSample
{
    public float normalizedTime;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 size;
}