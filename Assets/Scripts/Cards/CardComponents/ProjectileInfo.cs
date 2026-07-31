using System;
using UnityEngine;

[Serializable]
public struct ProjectileInfo
{
    public GameObject Prefab;
    public float Speed;
    public LaunchDirection Direction;
    public AbilityVisual Visual;
    public int Count;
    public int Pierce;
    public int Chains;
    public int Bounces;

    public readonly bool IsEmpty => Prefab == null;

    public Vector3 GetSpawnPosition(IEntity owner)
    {
        return owner.Transform.position
            + owner.Transform.up * 1.5f;
    }

    public Quaternion GetSpawnRotation(IEntity owner)
    {
        return Quaternion.LookRotation(GetLaunchVelocity(owner));
    }

    public Vector3 GetLaunchVelocity(IEntity owner)
    {
        Vector3 direction = Direction.GetDirection(owner);

        return direction.normalized * Speed;
    }
}