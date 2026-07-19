public enum ActionState
{
    Ready,
    Running
}
public interface IEnemyAction
{
    float GetPriority();

    void StartAction();
    void UpdateAction();
    void StopAction();
    bool IsAvailable();
    bool CanBeInterrupted { get; }
}