using FishNet;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Explosive Arrow")]
public class ExplosiveArrowDefinition : CardDefinition
{
    [SerializeField] private ProjectileInfo projectileInfo;
    [SerializeField] private float effectiveness;
    [SerializeField] private GameTag damageConversion;
    [SerializeField] private float detonationDelay;
    [SerializeField] private float channelTickInterval;
    [SerializeField] private float maxChannelDuration;

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

        var channel = card.AddCardComponent<ChannelingComponent>();
        channel.Configure(
            channelTickInterval,
            maxChannelDuration);

        var delay = card.AddCardComponent<DelayedComponent>();
        delay.Configure(detonationDelay);

        var damageAcc = card.AddCardComponent<DamageAccComponent>();
        damageAcc.Configure(effectiveness);

        // Wire events
        channel.OnTick += projectile.Activate;

        damageAcc.OnReleased += (hits, scalar) =>
        {
            foreach (var hit in hits)
            {
                damage.TriggerDamage(hit, scalar);

                Debug.Log(
                    $"ClientSpawn called | " +
                    $"Instance={context.InstanceId} | " +
                    $"Server={InstanceFinder.IsServerStarted} | " +
                    $"NetworkAction={GetInstanceID()}"
                );

                // Only spawns vfx on hit
                context.ClientSpawn(this, new VfxSpawnParams(hit.Position));
            }
        };

        delay.OnCompleted += () =>
        {
            damageAcc.Release();
        };
    }
}