using System;
using FishNet;
using UnityEngine;

public class EnemyCardAttack : EnemyAttack
{
    private Card _card;

    public override float CooldownDuration => 7f;
    public override AttackAnimation AttackAnimation => AttackAnimation.Special;

    public override void Initialize(EnemyAttackDefinition def)
    {
        base.Initialize(def);

        if (def is not EnemyCardAttackDefinition cardDef)
        {
            Debug.LogError($"[{gameObject.name}] EnemyCardAttack initialized with invalid definition type: {def?.GetType().Name ?? "null"}", this);
            return;
        }

        if (cardDef.CardDefinition == null)
        {
            Debug.LogError($"[{gameObject.name}] EnemyCardAttackDefinition '{cardDef.name}' is missing its CardDefinition assignment!", cardDef);
            return;
        }
        var owner = GetComponent<IEntity>() ?? GetComponentInParent<IEntity>();

        var context = new CardInitContext(
            Guid.NewGuid(),
            owner,
            serverNetworkSpawn: go => InstanceFinder.ServerManager.Spawn(go),
            clientNetworkSpawn: null
        );

        _card = cardDef.CardDefinition.Create(context);
    }

    public override void Execute(Transform target)
    {
        // base.Execute(target); Don't toggle hit collider

        _card.SetTargetLocation(target.position);
    }

    public override void OnAnimationFinish()
    {
        _card.ExecuteCastTimeDone();
        _card.ExecuteBegin();
    }

    private void Update()
    {
        if (_card != null && _card.IsTicking)
        {
            _card.Tick(Time.deltaTime);
        }
    }


    public override void Stop()
    {
        // base.Stop(); Don't toggle hit collider
        _card?.ExecuteCancelled();
    }
}