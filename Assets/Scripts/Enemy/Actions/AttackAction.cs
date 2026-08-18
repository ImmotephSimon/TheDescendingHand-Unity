using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackAction : EnemyActionBase
{
    private readonly IEnemyAttack _attack;
    private readonly IStatContainer _stats;
    private bool _canBeInterrupted = false;
    private bool _isAttacking = false;
    private bool _canBeUpdated = true;

    public override bool CanBeUpdated => _canBeUpdated;
    public override bool CanBeInterrupted => _canBeInterrupted;


    public AttackAction(Enemy owner, IEnemyAttack attack) : base(owner)
    {
        _attack = attack;
        _stats = owner.GetComponent<IStatContainer>();
        if (attack == null) Debug.LogError($"{owner.name}: Missing IAttackAbility");
    }
    

    public override bool IsAvailable()
    {
        if (!perception.HasTarget) return false;
        if (!abilityManager.Ready(_attack)) return false;

        return _attack.CanHit(perception.Target);
    }

    public override float GetPriority()
    {
        return _attack.GetPriority();
    }

    public override void StartAction()
    {
        if (!perception.HasTarget)
            return;

        movementHandler.StopMovement();
        movementHandler.LockRotation(perception.Target);

        _canBeInterrupted = false;
        _attack.OnHit += HandleHit;


        abilityManager.StartCooldown(_attack);
    }

    private void HandleHit(IEntity entity)
    {
        Debug.Log($"{entity} being hit");

        if (entity.Transform.TryGetComponent<IDamageable>(out var target))
        {
            float damage = _stats.GetStat(
                GameTags.ModOffenseDamage) * _attack.Effectiveness;

            target.TakeDamage(new DamageInfo(
                damageMap: new Dictionary<GameTag, float> { { GameTags.RestrictionPhysical, damage } }, 
                source: _owner, 
                hitPosition: entity.Transform.position)
            );
        }
    }

    private void FinishAttack()
    {
        _attack.OnAnimationFinish();
        StopAction();
    }

    public override void UpdateAction()
    {
        if (!perception.HasTarget)
        {
            StopAction();
            return;
        }

        if (_isAttacking) return;

        movementHandler.RotateTowardsTarget(perception.Target);

        if (movementHandler.GetFacingAngle(perception.Target) < 5f)
        {
            _canBeUpdated = false;
            _isAttacking = true;
            animationHandler.PlayAnimation(
                _attack.AttackAnimation,
                FinishAttack
            );

            _attack.Execute(perception.Target);
        }
    }

    public override void StopAction()
    {
        _attack.Stop();
        ClearAttack();
    }

    private void ClearAttack()
    {
        _isAttacking = false;
        _canBeInterrupted = true;
        _attack.OnHit -= HandleHit;
        movementHandler.UnlockRotation();
        _canBeUpdated = true;
    }
}