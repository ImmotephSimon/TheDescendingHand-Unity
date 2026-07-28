using UnityEngine;

public class PatrolAction : IEnemyAction
{
    private enum PatrolState
    {
        Moving,
        Waiting
    }

    [SerializeField] private float idleDuration = 3f;

    private readonly IEnemyMovement movementHandler;
    private readonly Vector3 anchorPosition;
    private readonly float radius;
    private float waitTimer;
    private Vector3 currentDestination;
    private PatrolState patrolState;

    public bool CanBeInterrupted => true;

    public PatrolAction(Enemy owner, float radius)
    {
        movementHandler = owner.GetComponent<IEnemyMovement>();
        anchorPosition = owner.transform.position;
        this.radius = radius;
    }

    public bool IsAvailable() => true;

    public float GetPriority() => 10f;

    public void StartAction()
    {
        MoveToNextPoint();
    }

    public void UpdateAction()
    {
        if (patrolState == PatrolState.Waiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
                MoveToNextPoint();

            return;
        }

        if (movementHandler.HasReachedDestination())
        {
            movementHandler.StopMovement();

            patrolState = PatrolState.Waiting;
            waitTimer = idleDuration;
        }
    }

    public void StopAction()
    {
        movementHandler.StopMovement();
    }

    private void MoveToNextPoint()
    {
        Vector3 randomPoint = anchorPosition + Random.insideUnitSphere * radius;

        // sample onto NavMesh here
        movementHandler.MoveTo(randomPoint);
    }
}