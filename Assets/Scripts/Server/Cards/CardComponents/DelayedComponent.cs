using System;
using UnityEngine;

public class DelayedComponent : CardComponent
{
    private float _delay;

    private float _timer;
    private bool _active;
    private bool _paused;
    public override bool IsTicking => true;
    public event Action OnCompleted;

    public void Configure(float delay)
    {
        _delay = delay;
    }

    public void Pause()
    {
        _paused = true;
    }

    public void Resume()
    {
        _paused = false;
    }

    private void Update()
    {
        if (!_active || _paused)
            return;

        _timer += Time.deltaTime;

        if (_timer < _delay)
            return;

        _active = false;
        OnCompleted?.Invoke();
    }

    protected override void OnActivate()
    {
        _timer = 0f;
        _active = true;
        _paused = false;
    }

}