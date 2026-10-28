using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Sigil Invocation")]
public class SigilInvocationDefinition : CardDefinition
{
    [Header("Damage")]
    [SerializeField] private float effectiveness = 2;
    [SerializeField] private GameTag damageConversion;

    [Header("Sigil")]
    [SerializeField] private float minSize = 1f;
    [SerializeField] private float area = 2f;

    [Header("Channel")]
    [SerializeField] private float tickInterval = 0.1f;
    [SerializeField] private float totalDuration = 5f;
    [SerializeField] private float minDuration = 0.25f;
    private Action vfxCancelHandle;


    public override float CastMoveSpeed => 0.5f;

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        var trace = card.AddCardComponent<ShapeTraceComponent>();
        var channel = card.AddCardComponent<ChannelingComponent>();
        var damage = card.AddCardComponent<DirectDamageComponent>();
        damage.Configure(effectiveness, damageConversion);

        trace.Configure(TracedShape.Triangle, minSize, TraceMatchMode.Lenient);
        channel.Configure(
            tickInterval,
            totalDuration,
            ChannelInputMode.Automatic,
            minDuration);

        card.OnActivated += () =>
        {
            vfxCancelHandle = context.ClientSpawn(
                this,
                new VfxSpawnParams(card.TargetLocation, vfxIndex: 0, attach: true)
            );
        };

        channel.OnCompleted += () =>
        {
            Debug.Log($"[{name}] Channel completed. Evaluating sigil.");
            trace.EndTrace();
            vfxCancelHandle?.Invoke();
            vfxCancelHandle = null;
        };

        trace.OnTraceEvaluated += (ShapeTraceResult res) =>
        {
            if (res.Matched)
            {
                context.ClientSpawn(
                    this,
                    new VfxSpawnParams(res.Center, vfxIndex: 1)
                    {
                        Scale = Vector3.one * area
                    });
            }
            int targetMask = 1 << card.Owner.HostileLayer;
            foreach (var collider in Physics.OverlapBox(res.Center, res.HalfExtents, Quaternion.identity, targetMask))
            {
                if (!res.Contains(collider.bounds.center)) continue;

                if (collider.TryGetComponent(out IEntity target))
                    card.OnHit.Invoke(new HitInfo(target, card.Owner, collider.bounds.center));
            }
        };
    }
}