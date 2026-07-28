using System;
using Unity.AI.Assistant.Agents;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using static UnityEngine.GraphicsBuffer;

public class AttackAction : EnemyActionBase
{
    private readonly IEnemyAttack _attack;
    private readonly IStatContainer _stats;
    private bool _canBeInterrupted = false;
    private bool _isAttacking = false;

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
        return 100;
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

            target.TakeDamage(new DamageInfo { Amount = damage,  HitPosition = entity.Transform.position, Source = _owner});
        }
    }

    private void FinishAttack()
    {
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
            _isAttacking = true;
            animationHandler.SetAnimationState(CharacterAnimationState.Attack);
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
    }
}