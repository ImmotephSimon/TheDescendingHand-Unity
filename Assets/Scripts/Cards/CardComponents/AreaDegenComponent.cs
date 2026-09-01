using UnityEngine;

public class AreaDegenComponent : CardComponent
{
    public AreaOverlapComponent Overlap { get; }

    private readonly DurationDamageComponent _degen;

    public override bool IsTicking => _degen.IsTicking;

    public AreaDegenComponent(
        float radius,
        GameTag damageConversion,
        float effectiveness,
        float duration,
        int maxStacks)
    {
        Overlap = new AreaOverlapComponent(radius);

        _degen = new DurationDamageComponent(
            damageConversion,
            effectiveness,
            duration,
            maxStacks);

        Overlap.OnEntityEntered += OnEntered;
        Overlap.OnEntityExited += OnExited;
    }

    public override void Initialize(Card card, IEntity owner)
    {
        base.Initialize(card, owner);
        Overlap.Initialize(card, owner);
        _degen.Initialize(card, owner);
    }

    protected override void OnActivate()
    {
        Overlap.Activate();
        _degen.Activate();
    }

    private void OnEntered(IEntity target)
    {
        _degen.ApplyDegen(target);
    }

    private void OnExited(IEntity target)
    {
        _degen.StopDegen(target);
    }

    public override void Tick(float deltaTime)
    {
        _degen.Tick(deltaTime);
    }

    protected override void OnCancel()
    {
        Overlap.Cancel();
        _degen.Cancel();
    }
}