using System;
using UnityEngine;

public abstract class CardDefinition : ScriptableObject
{
    [SerializeField, HideInInspector]
    private Guid id;

    [SerializeField]
    private float castTime = 1f;
    [SerializeField] private bool spawnAtCursor = false;
    [SerializeField]
    public CardVisuals visuals = new();
    public Guid Id => id;
    public float CastTime => castTime;
    public bool SpawnAtCursor => spawnAtCursor;

    public CardVisuals Visuals => visuals; 
    public DeckOverrides DeckOverrides; 
    public abstract void Construct(CardInitContext context, CardRuntime card);

#if UNITY_EDITOR
    private void OnValidate()
    {
        string path = UnityEditor.AssetDatabase.GetAssetPath(this);
        if (!string.IsNullOrEmpty(path))
        {
            string hex = UnityEditor.AssetDatabase.AssetPathToGUID(path);
            id = System.Guid.Parse(hex);
        }
    }
#endif
}

public struct VfxSpawnParams
{
    public Guid InstanceId;
    public int VfxIndex;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public float Duration;

    

    public VfxSpawnParams(Vector3 position, int vfxIndex = 0, float duration = 0f)
    {
        InstanceId = Guid.NewGuid();
        VfxIndex = vfxIndex;
        Position = position;
        Duration = duration;
        Rotation = Quaternion.identity;
        Scale = Vector3.one;
    }
}

public readonly struct CardInitContext
{
    public readonly Guid InstanceId;
    public readonly IEntity Owner;
    public readonly Func<GameObject, GameObject> ServerSpawn;
    public readonly Func<CardDefinition, VfxSpawnParams, Action> ClientSpawn;

    public CardInitContext(
        Guid instanceId,
        IEntity owner,
        Func<GameObject, GameObject> serverNetworkSpawn,
        Func<CardDefinition, VfxSpawnParams, Action> clientNetworkSpawn)
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