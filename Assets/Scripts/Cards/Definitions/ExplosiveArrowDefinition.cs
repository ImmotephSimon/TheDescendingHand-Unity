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

    public override void Construct(CardInitContext context, Card card)
    {

        var damage = new DirectDamageComponent(
            effectiveness,
            damageConversion);

        var projectile = new ProjectileComponent(
            projectileInfo,
            context.ServerSpawn);

        var channel = new ChannelingComponent(
            channelTickInterval,
            maxChannelDuration);

        var delay = new DelayedComponent(detonationDelay);
        var damageAcc = new DamageAccComponent(effectiveness);

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

        // Register components
        card.AddComponent(channel);
        card.AddComponent(projectile);
        card.AddComponent(damage);
        card.AddComponent(damageAcc);
        card.AddComponent(delay);
    }
}