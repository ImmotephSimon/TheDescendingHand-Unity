using System;

public class SequenceComponent : CardComponent
{
    private readonly CardComponent _step;
    private readonly int _steps;
    private readonly float _delay;

    private int _currentStep;
    private bool _running;
    private ServerScheduler _scheduler;

    public override bool IsTicking => false;

    public SequenceComponent(CardComponent step, int steps, float delay)
    {
        _step = step ?? throw new ArgumentNullException(nameof(step));
        _steps = steps;
        _delay = delay;
    }

    public override void Initialize(Card card, IEntity owner)
    {
        base.Initialize(card, owner);
        _scheduler = GameWorld.Instance.Scheduler;
    }

    protected override void OnActivate()
    {
        _currentStep = 1;
        _running = true;

        ScheduleNextStep();
    }

    private void ExecuteNextStep()
    {
        if (!_running || _currentStep >= _steps)
        {
            _running = false;
            return;
        }

        _currentStep++;

        _step.ExecuteBegin();
        _step.Activate();

        if (_currentStep < _steps)
            ScheduleNextStep();
        else
            _running = false;
    }

    private void ScheduleNextStep()
    {
        _scheduler.Schedule(_delay, ExecuteNextStep);
    }
}