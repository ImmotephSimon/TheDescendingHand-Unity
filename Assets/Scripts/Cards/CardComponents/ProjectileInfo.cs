using UnityEngine;

public readonly struct ProjectileInfo
{
    public readonly Vector3 Position;
    public readonly Quaternion Rotation;
    public readonly Vector3 Offset;

    public readonly GameObject Visual;
    public ProjectileInfo(Transform owner, Vector3 offset, GameObject visual)
    {
        Position = owner.position + owner.rotation * offset;
        Rotation = owner.rotation;
        Offset = offset;
        Visual = visual;
    }
}