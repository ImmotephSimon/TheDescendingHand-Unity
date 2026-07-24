using System;
using System.Collections.Generic;
using UnityEngine;

public class Card : IHitReceiver
{
    public IEntity Owner => _owner;
    public Guid Id { get; }
    public float CastTime => Definition.CastTime;
    public CardDefinition Definition { get; }

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
        _components.Add(component);
    }

    public void OnHit(HitInfo info)
    {
        foreach (var component in _components)
            component.OnHit(info);
    }

    public void Tick(float deltaTime)
    {
        Debug.LogError($"Tick is currently not being called.");

        foreach (var component in _components)
            component.Tick(deltaTime);
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