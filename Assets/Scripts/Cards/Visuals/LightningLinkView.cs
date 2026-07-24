using System;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class ChainLightningView : MonoBehaviour, IVfx
{
    [SerializeField] private VisualEffect visualEffect;
    private float shrinkTime = 3f;
    private float maxLength = 10f;
    private Vector3 previousEnd;

    private enum LightningState
    {
        Grow,
        Fixed,
        Shrink
    }

    private LightningState state = LightningState.Grow;
    private Vector3 start;
    private Vector3 end;
    private float maxSpeed;
    private Vector3 originalStart;

    private bool reachedTarget;
    private Transform target;
    private float shrinkTimer;

    public void Initialize(Vector3 start, Transform target)
    {
        originalStart = start;
        this.start = start;
        this.target = target;
        end = target.position;
        previousEnd = end;
        visualEffect.SetVector3("Pos1", start);
        visualEffect.SetVector3("Pos4", end);
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
            maxSpeed = Math.Max(maxSpeed, currentSpeed);
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
        start = Vector3.MoveTowards(
            start,
            end,
            maxSpeed * Time.deltaTime
        );

        shrinkTimer += Time.deltaTime;
        if (shrinkTimer >= shrinkTime)
            Destroy(gameObject);
    }

}