using System.Collections.Generic;
using UnityEngine;

public class AreaDegenComponent : CardComponent
{
    private readonly AreaOverlapComponent _overlap;
    private readonly DurationDamageComponent _degen;
    public override bool IsTicking => !_stopped;

    private readonly HashSet<IEntity> _targets = new();
    private bool _stopped = false;

    public AreaDegenComponent(
        float radius,
        GameTag damageConversion,
        float effectiveness,
        float duration,
        int maxStacks)
    {
        _overlap = new AreaOverlapComponent(radius);
        _degen = new DurationDamageComponent(
            damageConversion,
            effectiveness,
            duration,
            maxStacks);

        _overlap.OnEntityEntered += OnEntered;
        _overlap.OnEntityExited += OnExited;
    }

    public override void Initialize(Card card, IEntity owner)
    {
        base.Initialize(card, owner);

        _overlap.Initialize(card, owner);
        _degen.Initialize(card, owner);
    }

    protected override void OnActivate()
    {
        _stopped = false;
        _overlap.Activate();
        _degen.Activate();
    }

    public void TrackTransform(Transform transform)
    {
        _overlap.ToggleTick(transform);
    }

    private void OnEntered(IEntity target)
    {
        _targets.Add(target);
    }

    private void OnExited(IEntity target)
    {
        _targets.Remove(target);
        _degen.StopDegen(target);
    }


    public override void Tick(float deltaTime)
    {
        _overlap.Tick(deltaTime);

        foreach (var target in _targets) 
        {
            _degen.ApplyDegen(target);
        }
    }

    public void Stop()
    {
        foreach (var target in _targets) 
        {
            _degen.StopDegen(target);
        }
        _targets.Clear();
        _stopped = true;
    }
}