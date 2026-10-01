using System;
using UnityEngine;

public abstract class CardDefinition : ScriptableObject
{
    [SerializeField, HideInInspector] private string idString;
    public Guid Id => Guid.Parse(idString);

    [SerializeField]
    private float castTime = 1f;
    [SerializeField] private bool spawnAtCursor = false;
    [SerializeField]
    public CardVisuals visuals = new();
    public float CastTime => castTime;
    public bool SpawnAtCursor => spawnAtCursor;
    public virtual float CastMoveSpeed => 0f;
    public CardVisuals Visuals => visuals; 
    public DeckOverrides DeckOverrides; 
    public abstract void Construct(CardInitContext context, CardRuntime card);

#if UNITY_EDITOR
    private void OnValidate()
    {
        string path = UnityEditor.AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrEmpty(path)) return;

        string hex = UnityEditor.AssetDatabase.AssetPathToGUID(path);
        string value = Guid.Parse(hex).ToString();
        if (idString != value)
        {
            idString = value;
            UnityEditor.EditorUtility.SetDirty(this);
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
    public bool Attach;

    public VfxSpawnParams(Vector3 position, int vfxIndex = 0, float duration = 0f, bool attach = false)
    {
        InstanceId = Guid.NewGuid();
        VfxIndex = vfxIndex;
        Position = position;
        Duration = duration;
        Rotation = Quaternion.identity;
        Scale = Vector3.one;
        Attach = attach;
    }
}

public readonly struct CardInitContext
{
    public readonly Guid InstanceId;
    public readonly IEntity Owner;

    public CardInitContext(Guid instanceId, IEntity owner)
    {
        InstanceId = instanceId;
        Owner = owner;
    }

    public GameObject ServerSpawn(GameObject go) =>
        NetworkManager.Instance.SpawnCapped(go);

    public Action ClientSpawn(CardDefinition def, VfxSpawnParams p) =>
        NetworkManager.Instance.SpawnClientVfx(Owner, def, p);
}

[Serializable]
public class DeckOverrides
{
    [SerializeField] private int addedDraws = 0;
    [SerializeField] private int addedDiscards = 0;

    public int AddedDraws => addedDraws;
    public int AddedDiscards => addedDiscards;
}