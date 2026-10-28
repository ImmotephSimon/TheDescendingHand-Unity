using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDetector : MonoBehaviour
{
    private readonly Dictionary<IEntity, HashSet<Collider>> _inside = new();

    public event Action<IEntity> OnEntered;
    public event Action<IEntity> OnExited;

    public int Count => _inside.Count;

    private void OnTriggerEnter(Collider other)
    {
        var entity = other.GetComponentInParent<IEntity>();
        if (entity == null)
            return;

        if (!_inside.TryGetValue(entity, out var colliders))
        {
            colliders = new HashSet<Collider>();
            _inside.Add(entity, colliders);

            entity.Died += OnEntityDied;
            OnEntered?.Invoke(entity);
        }

        colliders.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        var entity = other.GetComponentInParent<IEntity>();
        if (entity == null)
            return;

        if (!_inside.TryGetValue(entity, out var colliders))
            return;

        colliders.Remove(other);

        if (colliders.Count == 0)
        {
            _inside.Remove(entity);
            entity.Died -= OnEntityDied;
            OnExited?.Invoke(entity);
        }
    }

    private void OnEntityDied(IEntity entity)
    {
        if (!_inside.Remove(entity))
            return;

        entity.Died -= OnEntityDied;
        OnExited?.Invoke(entity);
    }

    private void OnDestroy()
    {
        foreach (var entity in _inside.Keys)
            entity.Died -= OnEntityDied;
    }
}