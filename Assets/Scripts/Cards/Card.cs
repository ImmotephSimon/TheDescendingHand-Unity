using System;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour 
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

    internal void AddComponent(CardComponent component)
    {
        component.Initialize(this, _owner);
        Tags.AddRange(component.GetTags());
        _components.Add(component);
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


    public void Tick(float deltaTime)
    {
        foreach (var component in _components)
        {
            if (component.IsTicking)
                component.Tick(deltaTime);
        }
    }

    public void ExecuteCastTimeDone()
    {
        OnActivated?.Invoke();
        foreach (var comp in _components)
            comp.Activate();
    }

    public void ExecuteCancelled()
    {
        foreach (var comp in _components)
            comp.Cancel();
    }
}