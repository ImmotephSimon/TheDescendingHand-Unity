using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovementController : MonoBehaviour, IEnemyMovement
{
    private float angularSpeed = 300f;
    float stoppingDistance = 0.5f;
    private NavMeshAgent agent;
    private Transform ownerTransform;
    private Vector3 lastDestination;
    private Transform _lockTarget;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null ) Debug.LogError($"NavMeshAgent missing on {name}!");
        ownerTransform = transform;

        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = true;
        agent.acceleration = 100f;
        agent.angularSpeed = angularSpeed;

        enabled = false; // Guarantees it starts off until OnStartServer enables it
    }

    public void MoveTo(Vector3 position)
    {
        if ((lastDestination - position).sqrMagnitude < 0.25f)
            return;

        lastDestination = position;

        agent.isStopped = false;
        agent.SetDestination(position);
    }

    public void StopMovement()
    {
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    public bool HasReachedDestination()
    {
        if (agent.pathPending)
            return false;

        if (agent.remainingDistance > agent.stoppingDistance)
            return false;

        return !agent.hasPath || agent.velocity.sqrMagnitude == 0f;
    }

    public bool IsWithinStoppingDistance(Vector3 position)
    {
        return Vector3.Distance(ownerTransform.position, position) <= agent.stoppingDistance;
    }

    public void RotateTowardsTarget(Transform target)
    {

        Transform focus = _lockTarget != null ? _lockTarget : target ;
        //if (_lockTarget == null) return;
        //Transform focus = target;

        Vector3 direction = focus.position - ownerTransform.position;
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

    public void LockRotation(Transform target)
    {
        _lockTarget = target;
    }

    public void UnlockRotation()
    {
        _lockTarget = null;
    }
}