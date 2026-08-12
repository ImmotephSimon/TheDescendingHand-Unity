using System;

public abstract class EnemyActionBase : IEnemyAction
{
    protected readonly Enemy _owner;
    protected readonly IEnemyMovement movementHandler;
    protected readonly IPerception perception;
    protected IAnimationHandler animationHandler => _owner.GetComponentInChildren<IAnimationHandler>();
    protected readonly IAbilityManager abilityManager;

    public virtual bool CanBeInterrupted => true;
    public virtual bool CanBeUpdated => true;

    protected EnemyActionBase(Enemy owner)
    {
        if (owner == null)
            throw new ArgumentNullException(nameof(owner));
        _owner = owner;

        movementHandler = owner.GetComponent<IEnemyMovement>()
            ?? throw new InvalidOperationException($"{owner.name} requires IEnemyMovement");

        perception = owner.GetComponent<IPerception>()
            ?? throw new InvalidOperationException($"{owner.name} requires IPerception");

        abilityManager = owner.GetComponent<IAbilityManager>()
            ?? throw new InvalidOperationException($"{owner.name} requires IAbilityManager");

    }

    public abstract bool IsAvailable();
    public abstract float GetPriority();
    public abstract void StartAction();
    public abstract void UpdateAction();
    public abstract void StopAction();
}