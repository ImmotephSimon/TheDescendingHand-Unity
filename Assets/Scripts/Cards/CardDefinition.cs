using System;
using UnityEngine;

public abstract class CardDefinition : ScriptableObject
{
    [SerializeField, HideInInspector]
    private string id;

    [SerializeField]
    private float castTime = 1f;

    [SerializeField]
    public CardVisuals visuals = new();
    public string Id => id;
    public float CastTime => castTime;

    public CardVisuals Visuals => visuals; 

    public abstract Card Create(CardInitContext context);

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Stable asset GUID used as the type identifier for network/save RPCs
        if (string.IsNullOrEmpty(id))
            id = Guid.NewGuid().ToString();
    }
#endif
}

public readonly struct CardInitContext
{
    public readonly Guid InstanceId;
    public readonly IEntity Owner;
    public readonly Action<GameObject> NetworkSpawn;

    public CardInitContext(
        Guid instanceId,
        IEntity owner,
        Action<GameObject> networkSpawn)
    {
        InstanceId = instanceId;
        Owner = owner;
        NetworkSpawn = networkSpawn;
    }
}