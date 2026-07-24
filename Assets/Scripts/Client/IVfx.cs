using UnityEngine;

internal interface IVfx
{
    void Initialize(Vector3 start, Transform target);
    void UpdateTarget(Transform target);
}