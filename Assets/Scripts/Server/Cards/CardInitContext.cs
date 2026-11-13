using FishNet.Object;
using System;
using UnityEngine;

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
        ServerObjectSpawner.Instance.SpawnCapped(go);

    public Action ClientSpawn(CardDefinition def, VfxSpawnParams p) =>
    VfxNet.Instance.Spawn(
        def.Visuals.Vfx[p.VfxIndex],
        p,
        Owner.Transform.GetComponent<NetworkObject>());
}