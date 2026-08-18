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
        public float ElapsedTime { get; set; }
        

        public ActiveDegen(DegenInfo info)
        {
            Info = info;
        }
    }

    private void Awake()
    {
        _healthHandler = GetComponent<IHealth>();
        Debug.Assert(_healthHandler != null, $"[{nameof(DegenComponent)}] Missing IHealth implementation on {gameObject.name}.");
    }

    public void Apply(DegenInfo degenInfo)
    {
        int count = 0;
        ActiveDegen oldest = null;

        for (int i = 0; i < _activeDegens.Count; i++)
        {
            if (_activeDegens[i].Info.Id == degenInfo.Id)
            {
                count++;
                if (oldest == null || _activeDegens[i].ElapsedTime > oldest.ElapsedTime)
                {
                    oldest = _activeDegens[i];
                }
            }
        }

        if (count < degenInfo.MaxStacks)
        {
            _activeDegens.Add(new ActiveDegen(degenInfo));
        }
        else if (oldest != null)
        {
            oldest.ElapsedTime = 0f;
        }
    }

    public void RemoveDegen(Guid id)
    {
        _activeDegens.RemoveAll(d => d.Info.Id == id);
    }

    private void Update()
    {
        if (_activeDegens.Count == 0) return;

        _tickTimer += Time.deltaTime;

        if (_tickTimer < TickInterval) return;

        float tick = _tickTimer;
        _tickTimer = 0f;

        for (int i = _activeDegens.Count - 1; i >= 0; i--)
        {
            var degen = _activeDegens[i];

            foreach (var (tag, amount) in degen.Info.Damage)
            {
                _healthHandler.AdjustHealth(-amount * TickInterval, degen.Info.Source, false);
            }

            degen.ElapsedTime += tick;

            if (degen.ElapsedTime >= degen.Info.Duration)
                _activeDegens.RemoveAt(i);
        }
    }
}