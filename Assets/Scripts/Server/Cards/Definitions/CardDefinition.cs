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


public readonly struct CardInitContext
{
    public readonly Guid InstanceId;
    public readonly IEntity Owner;

    public bool IsTriggered { get; }

    public CardInitContext(Guid instanceId, IEntity owner, bool isTriggered = false)
    {
        InstanceId = instanceId;
        Owner = owner;
        IsTriggered = isTriggered;
    }

    public GameObject ServerSpawn(GameObject go) =>
        GameNetwork.Instance.SpawnCapped(go);

    public Action ClientSpawn(CardDefinition def, VfxSpawnParams p) =>
        GameNetwork.Instance.SpawnClientVfx(Owner, def, p);
}

[Serializable]
public class DeckOverrides
{
    [SerializeField] private int addedDraws = 0;
    [SerializeField] private int addedDiscards = 0;

    public int AddedDraws => addedDraws;
    public int AddedDiscards => addedDiscards;
}