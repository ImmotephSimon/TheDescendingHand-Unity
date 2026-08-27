using UnityEngine;

public interface IVfx
{
    void Initialize(VfxSpawnParams spawnParams, Transform target = null);
    void Stop();
    void UpdateTarget(Transform target);
}