using System;

public class DelayedComponent : CardComponent
{
    private readonly float _delay;

    private float _timer;
    private bool _active;
    private bool _paused;

    public event Action OnCompleted;

    public DelayedComponent(float delay)
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

    public override void Tick(float deltaTime)
    {
        if (!_active || _paused)
            return;

        _timer += deltaTime;

        if (_timer < _delay)
            return;

        _active = false;
        OnCompleted?.Invoke();
    }

    protected override void OnBegin()
    {
        _timer = 0f;
        _active = true;
        _paused = false;
    }

}