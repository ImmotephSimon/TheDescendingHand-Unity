using System;
using UnityEngine;

[Serializable]
public class ProjectileInfo
{
    public GameObject Prefab;
    public float Speed = 10f;
    public LaunchDirection Direction = new(0,0,1);
    public AbilityVisual Visual;
    public int Count = 1;
    public int PierceCount = 0;

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