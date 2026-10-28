using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

public class DecalVfxController : MonoBehaviour, IVfx
{
    [SerializeField] private DecalProjector _decal;
    [SerializeField] private VisualEffect _vfx;
    [SerializeField] private float _lightningInterval = 1f;
    [SerializeField] private float _creepSpeed = 2f;

    private static readonly int Pos1Id = Shader.PropertyToID("Pos1");
    private static readonly int Pos4Id = Shader.PropertyToID("Pos4");

    private float _nextLightningTime;
    private Vector3 _currentStart;
    private Vector3 _currentEnd;
    private Vector3 _targetEnd;
    private bool _isInitialized;

    public void Initialize(VfxSpawnParams spawnParams, Transform target = null)
    {
        if (_decal == null || _vfx == null)
            throw new InvalidOperationException("DecalVfxController requires a decal and VFX.");

        _nextLightningTime = 0f;
        _isInitialized = false;
    }

    public void Stop()
    {
        _vfx.Stop();
        _isInitialized = false;
    }

    public void UpdateTarget(Transform target)
    {
        throw new NotImplementedException();
    }

    private void Update()
    {
        if (Time.time >= _nextLightningTime)
        {
            _nextLightningTime = Time.time + _lightningInterval;
            OnIntervalTick();
            _vfx.Play();
        }

        if (_isInitialized)
        {
            // Start moves toward the current end destination
            _currentStart = Vector3.MoveTowards(_currentStart, _currentEnd, _creepSpeed * Time.deltaTime);

            // If it reaches the target, it can immediately start creeping toward the next targetEnd
            if (_currentStart == _currentEnd)
            {
                _currentEnd = _targetEnd;
            }

            _vfx.SetVector3(Pos1Id, _currentStart);
            _vfx.SetVector3(Pos4Id, _targetEnd);
        }
    }

    private void OnIntervalTick()
    {
        Vector3 newPoint = GetRandomBoundaryPoint();

        if (!_isInitialized)
        {
            // Initial setup: set Start ONCE, pick an End target
            _currentStart = newPoint;
            _currentEnd = GetRandomBoundaryPoint();
            _targetEnd = _currentEnd;
            _isInitialized = true;
        }
        else
        {
            // Start is NOT changed here. Only advance the destination target.
            _targetEnd = newPoint;
        }
    }

    private Vector3 GetRandomBoundaryPoint()
    {
        Vector3 decalSize = _decal.size;
        Vector2 halfSize = new Vector2(decalSize.x, decalSize.y) * 0.5f;

        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        Vector2 p = new Vector2(Mathf.Cos(angle) * halfSize.x, Mathf.Sin(angle) * halfSize.y);

        return _decal.transform.TransformPoint(new Vector3(p.x, p.y, 0f));
    }
}