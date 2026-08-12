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

public readonly struct CardInitContext
{
    public readonly Guid InstanceId;
    public readonly IEntity Owner;
    public readonly Action<GameObject> ServerSpawn;
    public readonly Action<CardDefinition, Vector3, Quaternion> ClientSpawn;

    public CardInitContext(
        Guid instanceId,
        IEntity owner,
        Action<GameObject> serverNetworkSpawn,
        Action<CardDefinition, Vector3, Quaternion> clientNetworkSpawn)
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