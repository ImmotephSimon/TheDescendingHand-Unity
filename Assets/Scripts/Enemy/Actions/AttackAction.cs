using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AttackAction : EnemyActionBase
{
    private readonly IEnemyAttack _attack;
    private float endTime;
    private bool _canBeInterrupted = false;
    public override bool CanBeInterrupted => _canBeInterrupted;


    public AttackAction(Enemy owner, IEnemyAttack attack) : base(owner)
    {
        _attack = attack;
        if (attack == null) Debug.LogError($"{owner.name}: Missing IAttackAbility");
    }
    

    public override bool IsAvailable()
    {
        if (!perception.HasTarget)
            return false;

        if (!abilityManager.Ready(_attack))
            return false;

        return _attack.CanHit(perception.Target);
    }

    public override float GetPriority()
    {
        return 100;
    }

    public override void StartAction()
    {
        if (!perception.HasTarget)
            return;

        _canBeInterrupted = false;
        _attack.OnHit += HandleHit;

        animationHandler.SetAnimationState(CharacterAnimationState.Attack);
        animationHandler.PlayAnimation(
            _attack.AttackAnimation,
            FinishAttack
        );

        abilityManager.StartCooldown(_attack);
        _attack.Execute(perception.Target);
    }

    private void HandleHit(IEntity entity)
    {
        Debug.Log($"{entity} being hit");

        if (entity.Transform.TryGetComponent<IDamageable>(out var target))
        {
            target.TakeDamage(new DamageInfo { Amount = 2,  HitPosition = entity.Transform.position, Source = _owner});
        }
    }

    private void FinishAttack()
    {
        _canBeInterrupted = true;
    }

    public override void UpdateAction()
    {
    }

    public override void StopAction()
    {
        _attack.OnHit -= HandleHit;
    }
}