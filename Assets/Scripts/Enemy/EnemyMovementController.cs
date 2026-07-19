using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovementController : MonoBehaviour, IEnemyMovement
{
    [SerializeField] private float angularSpeed = 720f;
    [SerializeField] float stoppingDistance = 0.6f;
    private NavMeshAgent agent;
    private Transform ownerTransform;
    private Vector3 lastDestination;

    private bool serverInitialized;

    public void Initialize(NavMeshAgent agent, Transform ownerTransform)
    {
        this.agent = agent;
        this.ownerTransform = ownerTransform;

        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = true;
        agent.acceleration = 100f;
        agent.angularSpeed = angularSpeed;
        serverInitialized = true;
    }

    public void MoveTo(Vector3 position)
    {
        if (!serverInitialized)
            return;

        if ((lastDestination - position).sqrMagnitude < 0.25f)
            return;

        lastDestination = position;

        agent.isStopped = false;
        agent.SetDestination(position);
    }

    public void StopMovement()
    {
        if (!serverInitialized)
            return;

        agent.isStopped = true;
        agent.ResetPath();
    }

    public bool HasReachedDestination()
    {
        if (!serverInitialized)
            return false;

        if (agent.pathPending)
            return false;

        if (agent.remainingDistance > agent.stoppingDistance)
            return false;

        return !agent.hasPath || agent.velocity.sqrMagnitude == 0f;
    }

    public bool IsWithinStoppingDistance(Vector3 position)
    {
        if (!serverInitialized)
            return false;

        return Vector3.Distance(ownerTransform.position, position) <= agent.stoppingDistance;
    }

    public void RotateTowardsTarget(Transform target)
    {
        Vector3 direction = target.position - ownerTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        float angle = Vector3.Angle(ownerTransform.forward, direction);

        float speedMultiplier = Mathf.Lerp(
            1f,
            1.5f,
            angle / 180f
        );

        ownerTransform.rotation = Quaternion.RotateTowards(
            ownerTransform.rotation,
            Quaternion.LookRotation(direction),
            angularSpeed * speedMultiplier * Time.deltaTime
        );
    }

    public float GetFacingAngle(Transform target)
    {
        Vector3 direction = target.position - ownerTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return 0f;

        return Vector3.Angle(
            ownerTransform.forward,
            direction.normalized
        );
    }
}