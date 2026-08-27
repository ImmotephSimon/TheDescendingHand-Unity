using System.Collections.Generic;

public class StatusEffectComponent : CardComponent
{
    private readonly GameTag _Statustag;
    private readonly float _duration;
    private readonly Dictionary<IEntity, ModifierHandle> _activeStatuses = new();

    public override bool IsTicking => false;

    public StatusEffectComponent(GameTag statusTag, float duration)
    {
        _Statustag = statusTag;
        _duration = duration;
    }

    public override void Initialize(Card card, IEntity owner)
    {
        base.Initialize(card, owner);
        Card.OnHit += HandleOnHit;
    }

    public void HandleOnHit(HitInfo info)
    {
        if (info.Target == null)
            return;

        var target = info.Target;


        var statusHandle = target.Stats.AddModifier(
            new StatModifier(
                _Statustag,
                MathOp.Added,
                1f),
            _duration);

        _activeStatuses[target] = statusHandle;
    }

    protected override void OnCancel()
    {
        foreach (var kvp in _activeStatuses)
            kvp.Key.Stats.RemoveModifier(kvp.Value, true);

        _activeStatuses.Clear();
    }
}