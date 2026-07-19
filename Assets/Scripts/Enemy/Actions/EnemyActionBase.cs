using System;

public abstract class EnemyActionBase : IEnemyAction
{
    protected readonly IEnemyMovement movementHandler;
    protected readonly IPerception perception;
    protected readonly IAnimationHandler animationHandler;
    protected readonly IAbilityManager abilityManager;

    public virtual bool CanBeInterrupted => true;

    protected EnemyActionBase(Enemy owner)
    {
        if (owner == null)
            throw new ArgumentNullException(nameof(owner));

        movementHandler = owner.GetComponent<IEnemyMovement>()
            ?? throw new InvalidOperationException($"{owner.name} requires IEnemyMovement");

        perception = owner.GetComponent<IPerception>()
            ?? throw new InvalidOperationException($"{owner.name} requires IPerception");

        animationHandler = owner.GetComponent<IAnimationHandler>()
            ?? throw new InvalidOperationException($"{owner.name} requires IAnimationHandler");
        abilityManager = owner.GetComponent<IAbilityManager>()
            ?? throw new InvalidOperationException($"{owner.name} requires IAbilityManager");

    }

    public abstract bool IsAvailable();
    public abstract float GetPriority();
    public abstract void StartAction();
    public abstract void UpdateAction();
    public abstract void StopAction();
}