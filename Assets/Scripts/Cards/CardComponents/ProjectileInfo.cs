using System;
using UnityEngine;

[Serializable]
public class ProjectileInfo
{
    public GameObject Prefab;

    [Range(2f, 20f)]
    public float Speed = 5;

    public LaunchDirection Direction;
    public GameObject Visual;

    [Range(1, 10)]
    public int Count = 1;

    [Range(0, 10)]
    public int Pierce = 0;

    [Range(0, 10)]
    public int Chains = 0;

    [Range(0, 10)]
    public int Bounces = 0;
    public bool IsEmpty => Prefab == null;

    public Vector3 GetSpawnPosition(IEntity owner)
        => owner.Transform.position + owner.Transform.up * 1.5f;

    public Quaternion GetSpawnRotation(IEntity owner)
        => Quaternion.LookRotation(GetLaunchVelocity(owner));

    public Vector3 GetLaunchVelocity(IEntity owner)
        => Direction.GetDirection(owner).normalized * Speed;

    public ProjectileInfo Clone()
    {
        return (ProjectileInfo)MemberwiseClone();
    }
}