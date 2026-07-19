public class SequenceComponent : CardComponent
{
    private readonly CardComponent[] _steps;
    private readonly float _delay;

    private int _currentStep;
    private float _timer;
    private bool _running;

    public SequenceComponent(CardComponent[] steps, float delay)
    {
        _steps = steps;
        _delay = delay;
    }

    protected override void OnCastTimeDone()
    {
        _running = true;
        _currentStep = 0;
        _timer = 0f;
    }

    public override void Tick(float deltaTime)
    {
        if (!_running)
            return;

        _timer += deltaTime;

        if (_timer < _delay)
            return;

        _timer = 0f;

        var step = _steps[_currentStep];

        step.ExecuteBegin();
        step.ExecuteCastTimeDone();

        _currentStep++;

        if (_currentStep >= _steps.Length)
            _running = false;
    }
}