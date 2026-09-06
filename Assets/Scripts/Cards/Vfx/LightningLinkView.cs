using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class ChainLightningView : MonoBehaviour, IVfx
{
    [SerializeField] private VisualEffect visualEffect;
    



    private enum LightningState
    {
        Grow,
        Fixed,
        Shrink
    }
    private float minShrinkTime = 1f;
    private float maxLength = 10f;
    private Vector3 previousEnd;
    private float shrinkSpeed;
    private LightningState state = LightningState.Grow;
    private Vector3 start;
    private Vector3 end;
    private float maxSpeed;
    private Vector3 originalStart;

    private Transform target;
    private float shrinkTimer;

    public void Initialize(VfxSpawnParams spawnParams, Transform target = null)
    {
        originalStart = spawnParams.Position;
        start = spawnParams.Position;
        this.target = target;
        end = target != null ? target.position : spawnParams.Position;
        previousEnd = end;
        visualEffect.SetVector3("Pos1", start);
        visualEffect.SetVector3("Pos4", end);
        Debug.Log($"[VFX Init] Start: {start}, End: {end}, TargetNull: {target == null}, TargetPos: {target?.position}");
    }

    public void UpdateTarget(Transform target)
    {
        this.target = target;
    }

    private void Awake()
    {
        visualEffect = GetComponent<VisualEffect>();

        if (visualEffect == null)
        {
            Debug.LogError("Missing VisualEffect component.", this);
        }
    }

    private void Update()
    {
        if (target == null) state = LightningState.Shrink;
        else UpdateSpeed();

        switch (state)
        {

            case LightningState.Grow:
                Grow();
                break;

            case LightningState.Fixed:
                start = Vector3.MoveTowards(start, end, maxSpeed * Time.deltaTime);
                break;

            case LightningState.Shrink:
                Shrink();
                break;
        }

        visualEffect.SetVector3("Pos1", start);
        visualEffect.SetVector3("Pos4", end);
    }

    private void UpdateSpeed()
    {
        if (Time.deltaTime > 0)
        {
            end = target.position;
            float currentSpeed = Vector3.Distance(previousEnd, end) / Time.deltaTime;
            maxSpeed = Mathf.Max(maxSpeed, currentSpeed);
            previousEnd = end;
        }
    }


    private void Grow()
    {
        if (Vector3.Distance(start, end) < 0.1f || Vector3.Distance(originalStart, end) >= maxLength)
        {
            state = LightningState.Fixed;
        }
    }
    private void Shrink()
    {
        //start = Vector3.MoveTowards(
        //    start,
        //    end,
        //    maxSpeed * Time.deltaTime
        //);

        start = Vector3.MoveTowards(start, end, maxSpeed * Time.deltaTime);
        shrinkTimer -= Time.deltaTime;

        // Only destroy after start hits end AND minShrinkTime has elapsed
        if (start == end && shrinkTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public void Stop()
    {
        Debug.Log($"stopping vfx {gameObject.name}");
        if (target != null)
        {
            end = target.position;
            target = null;
        }

        transform.SetParent(null);

        // Calculate how long maxSpeed would naturally take to collapse the distance
        float distance = Vector3.Distance(start, end);
        float naturalTime = maxSpeed > 0 ? distance / maxSpeed : 0f;

        // Ensure the shrink state lasts at least minShrinkTime
        shrinkTimer = Mathf.Max(minShrinkTime, naturalTime);

        state = LightningState.Shrink;
    }


}