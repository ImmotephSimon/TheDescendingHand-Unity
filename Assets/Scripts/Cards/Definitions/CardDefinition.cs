using System;
using UnityEngine;

public abstract class CardDefinition : ScriptableObject
{
    [SerializeField, HideInInspector]
    private string id;

    [SerializeField]
    private float castTime = 1f;
    [SerializeField] private bool spawnAtCursor = false;
    [SerializeField]
    public CardVisuals visuals = new();
    public string Id => id;
    public float CastTime => castTime;
    public bool SpawnAtCursor => spawnAtCursor;

    public CardVisuals Visuals => visuals; 
    public DeckOverrides DeckOverrides; 
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

public struct VfxSpawnParams
{
    public Guid InstanceId;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public float Duration;

    public VfxSpawnParams(Vector3 position, float duration = 0f)
        : this(position, Quaternion.identity, duration)
    {
    }

    public VfxSpawnParams(Vector3 position, Quaternion rotation, float duration = 0f)
    {
        InstanceId = Guid.NewGuid();
        Position = position;
        Rotation = rotation;
        Scale = Vector3.one;
        Duration = duration;
    }
}

public readonly struct CardInitContext
{
    public readonly Guid InstanceId;
    public readonly IEntity Owner;
    public readonly Action<GameObject> ServerSpawn;
    public readonly Action<CardDefinition, VfxSpawnParams> ClientSpawn;

    public CardInitContext(
        Guid instanceId,
        IEntity owner,
        Action<GameObject> serverNetworkSpawn,
        Action<CardDefinition, VfxSpawnParams> clientNetworkSpawn)
    {
        InstanceId = instanceId;
        Owner = owner;
        ServerSpawn = serverNetworkSpawn;
        ClientSpawn = clientNetworkSpawn;
    }
}
[Serializable]
public class DeckOverrides
{
    [SerializeField] private int addedDraws = 0;
    [SerializeField] private int addedDiscards = 0;

    public int AddedDraws => addedDraws;
    public int AddedDiscards => addedDiscards;
}