using System;
using UnityEngine;

// Proxy attached to the falling object at runtime
public class CollisionNotifier : MonoBehaviour
{
    public event Action<Vector3> OnImpact;

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 point = collision.contactCount > 0 ? collision.contacts[0].point : transform.position;
        OnImpact?.Invoke(point);
    }
}

public class AreaOverlapComponent : CardComponent
{
    private readonly float _radius;
    private readonly LayerMask? _targetLayers;

    public AreaOverlapComponent(float radius, LayerMask? targetLayers = null)
    {
        _radius = radius;
        _targetLayers = targetLayers;
    }

    public void TriggerAt(Vector3 center)
    {
        LayerMask mask = _targetLayers ?? (1 << Owner.HostileLayer);
        Collider[] colliders = Physics.OverlapSphere(center, _radius, mask);

        foreach (var col in colliders)
        {
            if (!col.TryGetComponent<IEntity>(out var target))
                continue;

            var hit = new HitInfo(target, Owner, col.ClosestPoint(center));
            Card.OnHit(hit);

        }
    }

}