using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AttackAction : EnemyActionBase
{
    private readonly IAttackAbility attack;
    private float endTime;
    private bool canBeInterrupted = false;
    public override bool CanBeInterrupted => canBeInterrupted;


    public AttackAction(Enemy owner, IAttackAbility attack) : base(owner)
    {
        this.attack = attack;
        if (attack == null)
            Debug.LogError($"{owner.name}: Missing IAttackAbility");
    }
    

    public override bool IsAvailable()
    {
        if (!perception.HasTarget)
            return false;

        if (!abilityManager.Ready(attack))
            return false;

        return attack.CanHit(perception.Target);
    }

    public override float GetPriority()
    {
        return 100;
    }

    public override void StartAction()
    {
        if (!perception.HasTarget)
            return;

        canBeInterrupted = false;

        animationHandler.SetAnimationState(CharacterAnimationState.Attack);
        animationHandler.PlayAnimation(
            attack.AttackAnimation,
            FinishAttack
        );

        abilityManager.StartCooldown(attack);
        attack.Execute(perception.Target);
    }
    private void FinishAttack()
    {
        canBeInterrupted = true;
    }

    public override void UpdateAction()
    {
    }

    public override void StopAction()
    {
    }
}