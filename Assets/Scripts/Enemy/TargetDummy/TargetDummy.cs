using UnityEngine;

public class TargetDummy : Entity
{
    private EnemyHealthBar _healthBar;

    protected override void Awake()
    {
        base.Awake();

        _stats.AddModifier(
            new StatModifier(
                GameTags.ModStatHealth,
                MathOp.Added,
                20f));

    }

    protected override void Start()
    {
        base.Start();

        if (_healthBar == null)
        {
            _healthBar = GetComponentInChildren<EnemyHealthBar>();
            _healthBar.Bind(GetComponent<IHealth>());
        }
    }


    public override void TakeDamage(DamageInfo info)
    {
        base.TakeDamage(info);
        HandleTargetDummyHit();
    }

    private void HandleTargetDummyHit()
    {
        GetComponent<DropsComponent>().DropFromEnemy(transform.position);
    }

    public override void ApplyDegen(DegenInfo degenInfo)
    {
        base.ApplyDegen(degenInfo);
        HandleTargetDummyHit();
    }


    protected override void OnDeath(IEntity killer)
    {
        GameWorld.Instance.NotifyRevive(this);
        
    }

    protected override void OnEntityDied(IEntity victim, IEntity killer)
    {
    }
    protected override void OnEntityRevived(IEntity entity)
    {
        IsDead = false;
        GetComponent<IHealth>().AdjustHealth(_stats.GetStat(GameTags.ModStatHealth), this);
    }
}