
using System;
using UnityEngine;

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