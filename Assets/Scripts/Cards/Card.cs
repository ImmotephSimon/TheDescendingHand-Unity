using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Card : IHitReceiver
{
    protected readonly Transform Owner;
    public Guid Id { get; }
    public float CastTime { get; }

    private readonly List<CardComponent> _components = new();

    protected Card(Guid id, float castTime, Transform owner)
    {
        Id = id;
        CastTime = castTime;
        Owner = owner;
    }

    protected void AddComponent(CardComponent component)
    {
        component.Initialize(Owner);
        _components.Add(component);
    }

    public void OnHit(HitInfo info)
    {
        foreach (var component in _components)
            component.OnHit(info);
    }

    public void Tick(float deltaTime)
    {
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
            comp.ExecuteCastTimeDone();
    }

    internal void ExecuteCancelled()
    {
        throw new NotImplementedException();
    }
}