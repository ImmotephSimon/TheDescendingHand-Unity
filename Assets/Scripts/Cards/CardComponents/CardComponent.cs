using UnityEngine;

public abstract class CardComponent
{
    protected IStatContainer Stats { get; private set; }
    protected Card Card { get; private set; }
    protected IEntity Owner { get; private set; }
    public virtual void OnHit(HitInfo info) { }

    public void ExecuteBegin()
    {
        OnBegin();
    }

    public void Activate()
    {
        OnActivate();
    }

    public void Cancel()
    {
        OnCancel();
    }
    internal void Initialize(Card card, IEntity owner)
    {
        Card = card;
        Owner = owner;
    }
    protected abstract void OnBegin();
    protected abstract void OnActivate();
    protected abstract void OnCancel();
    public virtual void Tick(float deltaTime) { }
}