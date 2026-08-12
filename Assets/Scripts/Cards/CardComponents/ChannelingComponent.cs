using System;
using UnityEngine;

public enum ChannelInputMode
{
    HoldToChannel,
    Automatic
}

public class ChannelingComponent : CardComponent
{
    private readonly float _tickInterval;
    private readonly float _totalDuration;
    private readonly ChannelInputMode _inputMode;
    private float _minDuration;

    private float _elapsedTime;
    private float _tickTimer;
    private bool _isInputHeld = true;
    private bool _isChanneling = false;

    public event Action OnTick;
    public event Action OnCompleted;
    public event Action OnInterrupted;

    public ChannelingComponent(
        float tickInterval,
        float totalDuration,
        ChannelInputMode inputMode = ChannelInputMode.HoldToChannel,
        float minDuration = 0f)
        : base(GameTags.TypeChannelling)
    {
        _tickInterval = tickInterval;
        _totalDuration = totalDuration;
        _inputMode = inputMode;
        _minDuration = minDuration;
    }

    protected override void OnBegin()
    {
        _elapsedTime = 0f;
        _tickTimer = 0f;
        _isInputHeld = true;
        _isChanneling = true;

    }

    protected override void OnActivate() { }


    public void SetInputHeld(bool held)
    {
        _isInputHeld = held;
    }

    public override void Tick(float deltaTime)
    {
        if (!_isChanneling) return;

        _elapsedTime += deltaTime;
        _tickTimer += deltaTime;

        // Fixed: Properly stop channeling and notify listeners on key release
        if (_inputMode == ChannelInputMode.HoldToChannel && !_isInputHeld && _elapsedTime >= _minDuration)
        {
            _isChanneling = false;
            OnInterrupted?.Invoke();
            return;
        }

        if (_totalDuration > 0f && _elapsedTime >= _totalDuration)
        {
            _isChanneling = false;
            OnCompleted?.Invoke();
            return;
        }

        if (_tickInterval > 0f && _tickTimer >= _tickInterval)
        {
            _tickTimer -= _tickInterval;
            OnTick?.Invoke();
        }
    }

    protected override void OnCancel()
    {
        if (!_isChanneling) return;

        _isChanneling = false;
        OnInterrupted?.Invoke();
    }
}