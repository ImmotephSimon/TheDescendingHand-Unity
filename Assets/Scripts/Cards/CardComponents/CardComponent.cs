using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardComponent
{
    protected IStatContainer Stats { get; private set; }
    protected Card Card { get; private set; }
    protected IEntity Owner { get; private set; }
    public virtual bool IsTicking => true;
    public virtual void OnHit(HitInfo info) { }
    public IEnumerable<GameTag> GetTags() => _tags;
    private readonly GameTag[] _tags;

    protected CardComponent(params GameTag[] tags)
    {
        _tags = tags ?? Array.Empty<GameTag>();
    }

    public void ExecuteBegin()
    {
        OnBegin();
    }

    public void Activate()
    {
        OnActivate();
    }

    public virtual void Initialize(Card card, IEntity owner)
    {
        Card = card;
        Owner = owner;
    }
    protected virtual void OnBegin() { }
    protected virtual void OnActivate() { }
    protected virtual void OnCancel() { }
    public virtual void Tick(float deltaTime) { }

    public void Cancel() => OnCancel();
}