using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Proxy attached to the falling object at runtime
public class PhysicsCollisionNotifier : MonoBehaviour
{
    public event Action<Vector3> OnCollision;
    public event Action<Vector3> OnCollisionEnd;

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 point = collision.contactCount > 0 ? collision.contacts[0].point : transform.position;
        OnCollision?.Invoke(point);
    }
    private void OnCollisionExit(Collision collision)
    {
        OnCollisionEnd?.Invoke(transform.position);
    }
}

public class AreaOverlapComponent : CardComponent
{
    private readonly float _radius;
    private readonly LayerMask? _targetLayers;

    private Transform _trackedTransform;
    private Vector3? _staticCenter;

    private readonly HashSet<IEntity> _currentOccupants = new();

    public HashSet<IEntity> CurrentOccupants => _currentOccupants;

    public event Action<IEntity> OnEntityEntered;
    public event Action<IEntity> OnEntityExited;
    public override bool IsTicking => (_trackedTransform != null || _staticCenter != null);

    public AreaOverlapComponent(float radius, LayerMask? targetLayers = null)
    {
        _radius = radius;
        _targetLayers = targetLayers;
    }

    public void ToggleTick(Transform targetTransform)
    {
        _trackedTransform = targetTransform;
    }

    public void ToggleTick(Vector3? position)
    {
        _staticCenter = position;
    }

    public override void Tick(float deltaTime)
    {
        if (_trackedTransform == null && _staticCenter == null)
            return;

        Vector3 center = _trackedTransform != null ? _trackedTransform.position : _staticCenter.Value;

        LayerMask mask = _targetLayers ?? (1 << Owner.HostileLayer);
        Collider[] colliders = Physics.OverlapSphere(center, _radius, mask);

        HashSet<IEntity> frameHits = new();

        foreach (var col in colliders)
        {
            if (!col.TryGetComponent<IEntity>(out var target) || target == Owner || target.IsDead)
                continue;

            frameHits.Add(target);

            // First frame entering radius
            if (_currentOccupants.Add(target))
            {
                OnEntityEntered?.Invoke(target);
            }
        }

        // Detect entities that exited radius or died
        _currentOccupants.RemoveWhere(target =>
        {
            if (!frameHits.Contains(target) || target.IsDead)
            {
                OnEntityExited?.Invoke(target);
                return true;
            }
            return false;
        });
    }


    public void TriggerAt(Vector3 center)
    {
        LayerMask mask = _targetLayers ?? (1 << Owner.HostileLayer);
        Collider[] colliders = Physics.OverlapSphere(center, _radius, mask);

        foreach (var col in colliders)
        {
            if (!col.TryGetComponent<IEntity>(out var target) ||
                target == Owner ||
                target.IsDead)
                continue;

            var hit = new HitInfo(target, Owner, col.ClosestPoint(center));
            Card.OnHit?.Invoke(hit);
        }
    }

    protected override void OnCancel()
    {
        foreach (var target in _currentOccupants)
        {
            OnEntityExited?.Invoke(target);
        }
        _currentOccupants.Clear();
    }
}