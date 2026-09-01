using System;
using System.Collections.Generic;

public class DamageAccComponent : CardComponent
{
    private readonly List<HitInfo> _accumulatedHits = new();
    private readonly float _storedPct;

    public event Action<IReadOnlyList<HitInfo>, float> OnReleased;

    public DamageAccComponent(float storedPct)
    {
        _storedPct = storedPct;
    }



    public void HandleHit(HitInfo info)
    {
        _accumulatedHits.Add(info);
    }

    public void Release()
    {
        if (_accumulatedHits.Count > 0)
        {
            float totalScalar = _storedPct * _accumulatedHits.Count;
            OnReleased?.Invoke(_accumulatedHits, totalScalar);
        }
        _accumulatedHits.Clear();
    }

    protected override void OnActivate()
    {
        _accumulatedHits.Clear();
        Card.OnHit += HandleHit;
    }
}