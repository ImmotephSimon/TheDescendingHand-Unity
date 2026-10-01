using UnityEngine;

public class Trail : MonoBehaviour, IVfx
{
    [SerializeField] private TrailRenderer _trail;
    private Transform _target;

    public void Initialize(VfxSpawnParams spawnParams, Transform target = null)
    {
        _target = target;
        if (_target) transform.position = _target.position;
        _trail.Clear();
        _trail.emitting = true;
    }

    public void Stop()
    {
        _target = null;
        _trail.emitting = false; // existing trail fades out on its own
    }

    public void UpdateTarget(Transform target) => _target = target;

}