using System;
using System.Collections.Generic;
using UnityEngine;

public class DegenComponent : MonoBehaviour
{
    private const float TickInterval = 0.2f;
    private float _tickTimer;
    private IHealth _healthHandler;
    private readonly List<ActiveDegen> _activeDegens = new();

    private class ActiveDegen
    {
        public DegenInfo Info { get; }
        public float ExpirationTime { get; }

        public ActiveDegen(DegenInfo info)
        {
            Info = info;
            ExpirationTime = Time.time + info.Duration;
        }
    }

    private void Awake()
    {
        _healthHandler = GetComponent<IHealth>();
        Debug.Assert(
            _healthHandler != null,
            $"[{nameof(DegenComponent)}] Missing IHealth implementation on {gameObject.name}.");
    }

    public void Apply(DegenInfo degenInfo)
    {
        int count = 0;
        ActiveDegen oldest = null;

        for (int i = 0; i < _activeDegens.Count; i++)
        {
            if (_activeDegens[i].Info.Id != degenInfo.Id)
                continue;

            count++;

            if (oldest == null || _activeDegens[i].ExpirationTime < oldest.ExpirationTime)
                oldest = _activeDegens[i];
        }

        if (count < degenInfo.MaxStacks)
        {
            _activeDegens.Add(new ActiveDegen(degenInfo));
        }
        else if (oldest != null)
        {
            _activeDegens.Remove(oldest);
            _activeDegens.Add(new ActiveDegen(degenInfo));
        }
    }

    public void RemoveDegen(Guid id)
    {
        _activeDegens.RemoveAll(d => d.Info.Id == id);
    }

    private void Update()
    {
        if (_activeDegens.Count == 0)
            return;

        _tickTimer += Time.deltaTime;

        if (_tickTimer < TickInterval)
            return;

        float tick = _tickTimer;
        _tickTimer = 0f;

        var activeDegens = _activeDegens.ToArray();

        foreach (var degen in activeDegens)
        {
            foreach (var (tag, amount) in degen.Info.Damage)
            {
                _healthHandler.AdjustHealth(
                    -amount * TickInterval,
                    degen.Info.Source,
                    false);
            }

            if (Time.time >= degen.ExpirationTime)
                _activeDegens.Remove(degen);
        }
    }
}