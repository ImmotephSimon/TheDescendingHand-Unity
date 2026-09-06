using System;
using UnityEngine;

public class EnemyCardAttack : EnemyAttack
{
    private CardInstance _card;
    private CardRuntime _runtime;

    public override float CooldownDuration => 7f;
    public override AttackAnimation AttackAnimation => AttackAnimation.Special;

    public override void Initialize(EnemyAttackDefinition def)
    {
        base.Initialize(def);

        if (def is not EnemyCardAttackDefinition cardDef)
        {
            Debug.LogError(
                $"[{gameObject.name}] EnemyCardAttack initialized with invalid definition type: " +
                $"{def?.GetType().Name ?? "null"}",
                this);

            return;
        }

        if (cardDef.CardDefinition == null)
        {
            Debug.LogError(
                $"[{gameObject.name}] EnemyCardAttackDefinition '{cardDef.name}' is missing its CardDefinition assignment!",
                cardDef);

            return;
        }

        var owner = GetComponent<IEntity>() ?? GetComponentInParent<IEntity>();

        _card = CardFactory.CreateCardInstance(
            cardDef.CardDefinition,
            owner);
    }

    public override void Execute(Transform target)
    {
        _runtime = CardFactory.CreateRuntime(_card);

        _runtime.SetTargetLocation(target.position);
    }

    public override void OnAnimationFinish()
    {
        _runtime?.ExecuteCastTimeDone();
    }


    public override void Stop()
    {
        _runtime?.ExecuteCancelled();
    }
}