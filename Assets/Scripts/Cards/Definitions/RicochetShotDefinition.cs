using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Ricochet Shot")]
public class RicochetShotDefinition : CardDefinition
{
    [SerializeField] private ProjectileInfo projectileInfo;
    [SerializeField] private float effectiveness;
    [SerializeField] private GameTag damageConversion;

    public override void Construct(CardInitContext context, Card card)
    {

        var damage = new DirectDamageComponent(
            effectiveness,
            damageConversion);

        var projectile = new ProjectileComponent(
            projectileInfo,
            context.ServerSpawn);

        card.AddComponent(damage);
        card.AddComponent(projectile);
    }
}