using UnityEngine;

public class TargetDummy : Entity
{
    private EnemyHealthBar _healthBar;

    protected override void Awake()
    {
        base.Awake();

        stats.AddModifier(
            new StatModifier(
                GameTags.ModStatHealth,
                MathOp.Added,
                20f));

    }

    
    public override void TakeDamage(DamageInfo info)
    {
        base.TakeDamage(info);

        if (_healthBar == null) 
        { 
            _healthBar = GetComponentInChildren<EnemyHealthBar>();
            _healthBar.Bind(GetComponent<IHealth>());
        }

        GetComponent<DropsComponent>().DropFromEnemy();
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
        GetComponent<IHealth>().AdjustHealth(stats.GetStat(GameTags.ModStatHealth), this);
    }
}