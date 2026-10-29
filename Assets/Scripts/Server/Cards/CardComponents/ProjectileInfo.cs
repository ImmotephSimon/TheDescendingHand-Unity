using System;
using UnityEngine;

[Serializable]
public class ProjectileInfo
{

    [Range(2f, 20f)]
    public float Speed = 5;

    public LaunchDirection Direction;
    public VfxType Visual = VfxType.None;

    [Range(1, 10)]
    public int Count = 1;

    [Range(0, 10)]
    public int Pierce = 0;

    [Range(0, 10)]
    public int Chains = 0;

    [Range(0, 10)]
    public int Bounces = 0;

    public Vector3 GetSpawnPosition(Transform frame)
        => frame.position + Vector3.up * 1.5f;

    public Quaternion GetSpawnRotation(Transform frame)
        => Quaternion.LookRotation(GetLaunchVelocity(frame));

    public Vector3 GetLaunchVelocity(Transform frame)
        => Direction.GetDirection(frame).normalized * Speed;

    public ProjectileInfo Clone()
    {
        return (ProjectileInfo)MemberwiseClone();
    }
}