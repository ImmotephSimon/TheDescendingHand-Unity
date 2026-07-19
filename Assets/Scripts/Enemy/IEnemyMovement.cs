using UnityEngine;

public interface IEnemyMovement
{
    bool HasReachedDestination();
    public bool IsWithinStoppingDistance(Vector3 position);
    void MoveTo(Vector3 position);
    void StopMovement();
    void RotateTowardsTarget(Transform target);
    float GetFacingAngle(Transform target);
}