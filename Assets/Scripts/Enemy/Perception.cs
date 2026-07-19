using UnityEngine;

public class Perception : MonoBehaviour, IPerception
{
    [SerializeField] private float sightRange = 15f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float scanInterval = 0.1f;
    [SerializeField] private Transform eyePoint;
    [SerializeField] private int maxDetectedColliders = 32;
    [SerializeField] private float forgetDuration = 3f;

    private Transform currentTarget;
    private float nextScanTime;
    private Collider[] hitsBuffer;
    private float lastTimeTargetSeen;

    public bool HasTarget => currentTarget != null;
    public Transform Target => currentTarget;

    private void Awake()
    {
        hitsBuffer = new Collider[maxDetectedColliders];
        if (eyePoint == null)
        {
            eyePoint = transform;
        }
    }

    void Update()
    {
        if (Time.time >= nextScanTime)
        {
            nextScanTime = Time.time + scanInterval;
            FindTarget();
        }
    }

    private void FindTarget()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            eyePoint.position,
            sightRange,
            hitsBuffer,
            targetMask
        );
        float closestDistanceSqr = float.MaxValue;
        Transform bestTarget = null;

        for (int i = 0; i < hitCount; i++)
        {
            var hit = hitsBuffer[i];
            if (hit == null) continue;

            float distanceSqr = (eyePoint.position - hit.transform.position).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                if (Physics.Linecast(eyePoint.position, hit.transform.position, obstacleMask))
                {
                    continue;
                }

                closestDistanceSqr = distanceSqr;
                bestTarget = hit.transform;
            }
        }

        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            lastTimeTargetSeen = Time.time;
        }
        else if (currentTarget != null)
        {
            if (Time.time - lastTimeTargetSeen >= forgetDuration)
            {
                currentTarget = null;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 origin = eyePoint != null ? eyePoint.position : transform.position;
        Gizmos.DrawWireSphere(origin, sightRange);
    }
}