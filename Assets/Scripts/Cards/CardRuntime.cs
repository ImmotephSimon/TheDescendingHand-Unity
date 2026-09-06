using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardRuntime : MonoBehaviour 
{
    public IEntity Owner => _owner;
    public Guid Id { get; private set; }
    public CardDefinition Definition { get; private set; }
    public float CastTime => Definition.CastTime;
    public bool SpawnAtCursor => Definition.SpawnAtCursor;
    public Vector3 TargetLocation { get; private set; }
    public TagContainer Tags { get; } = new();

    public bool IsTicking {
        get { 
            foreach (var component in _components)
                if (component.IsTicking)
                    return true;
            return false; 
        }}

    private IEntity _owner;

    private readonly List<CardComponent> _components = new();
    public event Action OnActivated;
    public Action<HitInfo> OnHit; // Components bind in Initialize().

    public void Initialize(Guid id, CardDefinition definition, IEntity owner)
    {
        Id = id;
        Definition = definition;
        _owner = owner;
    }

    public T AddCardComponent<T>() where T : CardComponent
    {
        var component = gameObject.AddComponent<T>();
        component.Initialize(this, _owner);

        Tags.AddRange(component.GetTags());
        _components.Add(component);

        return component;
    }

    public void SetTargetLocation(Vector3 location)
    {
        TargetLocation = location;
    }

    public T GetCardComponent<T>() where T : CardComponent 
    { 
        for (int i = 0; i < _components.Count; i++) 
        { 
            if (_components[i] is T typed) return typed; 
        } 
        return null; 
    }


    public void ExecuteCastTimeDone()
    {
        transform.position = TargetLocation;
        transform.rotation = GetLookRotation(TargetLocation);

        OnActivated?.Invoke();
        foreach (var comp in _components)
            comp.Activate();

    }

    public void ExecuteCancelled()
    {
        foreach (var comp in _components)
            comp.Cancel();
    }

    private Quaternion GetLookRotation(Vector3 target)
    {
        Vector3 direction = target - _owner.Transform.position;
        direction.y = 0f; 

        if (direction.sqrMagnitude < 0.0001f)
        {
            return Quaternion.identity;
        }

        return Quaternion.LookRotation(direction);
    }

    public AreaOverlapComponent AddSphereOverlap(float radius)
    {
        var collider = gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = radius;
        collider.gameObject.layer = Owner.AttackLayer;

        var overlap = AddCardComponent<AreaOverlapComponent>();
        overlap.Configure(collider);
        return overlap;
    }

    public AreaOverlapComponent AddBoxOverlap(Vector3 size)
    {
        var collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = size;
        collider.gameObject.layer = Owner.AttackLayer;

        var overlap = AddCardComponent<AreaOverlapComponent>();    
        overlap.Configure(collider);
        return overlap;
    }

    //public AreaOverlapComponent AddBoxOverlap(Vector3 size, int? overrideLayer = null)
    //{
    //    if (overrideLayer.HasValue)
    //    {
    //        gameObject.layer = overrideLayer.Value;
    //    }

    //    var collider = gameObject.AddComponent<BoxCollider>();
    //    collider.isTrigger = true;
    //    collider.size = size;

    //    collider.includeLayers = 1 << Owner.HostileLayer;

    //    var overlap = AddCardComponent<AreaOverlapComponent>();
    //    overlap.Configure(collider);
    //    return overlap;
    //}
}