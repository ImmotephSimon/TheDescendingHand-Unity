using System;
using System.Collections.Generic;
using UnityEngine;

public class Card : IHitReceiver
{
    public IEntity Owner => _owner;
    public Guid Id { get; }
    public float CastTime => Definition.CastTime;
    public bool SpawnAtCursor => Definition.SpawnAtCursor;
    public Vector3 TargetLocation => SpawnAtCursor ? _owner.CursorPosition : _owner.Transform.position;
    public TagContainer Tags { get; } = new();

    public CardDefinition Definition { get; }
    public bool IsTicking {
        get { 
            foreach (var component in _components)
                if (component.IsTicking)
                    return true;
            return false; 
        }}


    private readonly IEntity _owner;
    private readonly List<CardComponent> _components = new();

    public Card(Guid id, CardDefinition definition, IEntity owner)
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

    // Direct peer lookup added right here to avoid messy component flow
    public T GetComponent<T>() where T : CardComponent 
    { 
        for (int i = 0; i < _components.Count; i++) 
        { 
            if (_components[i] is T typed) return typed; 
        } 
        return null; 
    }

    public void OnHit(HitInfo info)
    {
        foreach (var component in _components)
            component.OnHit(info);
    }

    public void Tick(float deltaTime)
    {
        foreach (var component in _components)
        {
            if (component.IsTicking)
                component.Tick(deltaTime);
        }
    }

    public void ExecuteBegin()
    {
        foreach (var comp in _components)
            comp.ExecuteBegin();
    }

    public void ExecuteCastTimeDone()
    {
        foreach (var comp in _components)
            comp.Activate();
    }

    public void ExecuteCancelled()
    {
        foreach (var comp in _components)
            comp.Cancel();
    }
}