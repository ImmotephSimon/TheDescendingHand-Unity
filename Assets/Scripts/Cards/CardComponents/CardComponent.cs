using UnityEngine;

public abstract class CardComponent
{
    protected Transform Owner { get; private set; }
    public virtual void OnHit(HitInfo info) { }

    public void ExecuteBegin()
    {
        OnBeginCard();
    }

    public void ExecuteCastTimeDone()
    {
        OnCastTimeDone();
    }

    public void ExecuteCancelled()
    {
        OnCancelled();
    }
    public void Initialize(Transform owner)
    {
        Owner = owner;
    }
    protected virtual void OnBeginCard() { }
    protected virtual void OnCastTimeDone() { }
    protected virtual void OnCancelled() { }
    public virtual void Tick(float deltaTime) { }
}