using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Ricochet Shot")]
public class RicochetShotDefinition : CardDefinition
{
    [SerializeField] private ProjectileInfo projectileInfo;
    [SerializeField] private float effectiveness;
    [SerializeField] private GameTag damageConversion;

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        var damage = card.AddCardComponent<DirectDamageComponent>();
        damage.Configure(
            effectiveness,
            damageConversion);

        var projectile = card.AddCardComponent<ProjectileComponent>();
        projectile.Configure(
            projectileInfo,
            context.ServerSpawn);
    }
}