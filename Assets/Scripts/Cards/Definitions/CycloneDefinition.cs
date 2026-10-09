using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Cyclone")]
public class CycloneDefinition : CardDefinition
{
    [Header("Damage")]
    [SerializeField] private float effectiveness = 1.5f;
    [SerializeField] private GameTag damageConversion;

    [Header("Trace")]
    [SerializeField] private float minSize = 1f;
    [SerializeField] private int requiredMatches = 2;

    [Header("Tornado")]
    [SerializeField] private float cycloneScale = 3f;

    [Header("Channel")]
    [SerializeField] private float tickInterval = 0.1f;
    [SerializeField] private float totalDuration = 3f;
    [SerializeField] private float minDuration = 0.25f;


    public override float CastMoveSpeed => 1.3f;

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        var trace = card.AddCardComponent<ShapeTraceComponent>();
        var channel = card.AddCardComponent<ChannelingComponent>();
        var damage = card.AddCardComponent<DirectDamageComponent>();
        damage.Configure(effectiveness, damageConversion);

        trace.Configure(TracedShape.Circle, minSize, TraceMatchMode.Exact);
        channel.Configure(tickInterval, totalDuration, ChannelInputMode.Automatic, minDuration);

        // Local instead of a field: see note 2 below.
        Action vfxCancelHandle = null;

        card.OnActivated += () =>
        {
            vfxCancelHandle = context.ClientSpawn(
                this,
                new VfxSpawnParams(card.TargetLocation, vfxIndex: 0, attach: true));
        };

        void ApplyResult(ShapeTraceResult res)
        {
            context.ClientSpawn(
                this,
                new VfxSpawnParams(res.Center, vfxIndex: 1)
                {
                    Scale = Vector3.one * cycloneScale
                });

            int targetMask = 1 << card.Owner.HostileLayer;
            foreach (var collider in Physics.OverlapBox(res.Center, res.HalfExtents, Quaternion.identity, targetMask))
            {
                if (!res.Contains(collider.bounds.center)) continue;

                if (collider.TryGetComponent(out IEntity target))
                    card.OnHit.Invoke(new HitInfo(target, card.Owner, collider.bounds.center));
            }
        }
        int matches = 0;

        channel.OnTick += () =>
        {
            var res = trace.Evaluate();
            if (!res.Matched) return;

            matches++;
            trace.BeginTrace();

            if (matches < requiredMatches) return;

            ApplyResult(res);
            channel.Complete();
        };

        channel.OnCompleted += () =>
        {
            vfxCancelHandle?.Invoke();
            vfxCancelHandle = null;
        };
    }


}