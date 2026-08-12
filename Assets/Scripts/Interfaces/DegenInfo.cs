using System.Collections.Generic;
using UnityEngine;

public class DegenInfo
{
    public Dictionary<GameTag, float> Damage { get; }
    public IEntity Source { get; }
    public Vector3 Position { get; }
    public float Duration { get; }
    public float TickInterval { get; }
    public int MaxStacks { get; }

    public DegenInfo(
        Dictionary<GameTag, float> damage,
        IEntity source,
        Vector3 position,
        float duration,
        float tickInterval,
        int maxStacks)
    {
        Damage = damage;
        Source = source;
        Position = position;
        Duration = duration;
        TickInterval = tickInterval;
        MaxStacks = maxStacks;
    }
}