using UnityEngine;

public interface IEnemyMovement
{
    bool HasReachedDestination();
    bool IsWithinStoppingDistance(Vector3 position);
    void MoveTo(Vector3 position);
    void StopMovement();
    void RotateTowardsTarget(Transform target);
    float GetFacingAngle(Transform target);
    void LockRotation(Transform target);
    void UnlockRotation();
}

