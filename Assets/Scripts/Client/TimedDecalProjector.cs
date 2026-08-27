using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TimedDecalProjector : MonoBehaviour, IVfx
{
    [SerializeField] private DecalProjector decalProjector;

    private float fadeInMid = 0.8f;
    private float fadeInEnd = 1f;
    private float fadeOutDuration = 0.3f;

    private float _duration;
    private float _elapsed;
    private bool _isRunning;

    private void Awake()
    {
        if (decalProjector == null)
            TryGetComponent(out decalProjector);
    }

    public virtual void Initialize(VfxSpawnParams spawnParams, Transform target = null)
    {
        transform.position = spawnParams.Position;
        transform.rotation = spawnParams.Rotation;

        // Decals use size (Vector3) rather than scale for projection volume
        if (decalProjector != null && spawnParams.Scale != Vector3.zero)
        {
            decalProjector.size = spawnParams.Scale;
        }

        _duration = spawnParams.Duration;
        _elapsed = 0f;
        _isRunning = true;

        SetProgress(0f);
    }



    private void Update()
    {
        if (!_isRunning)
            return;

        _elapsed += Time.deltaTime;
        SetProgress(_elapsed / _duration);
    }



    private void SetProgress(float progress)
    {
        if (decalProjector == null)
            return;

        float fade;

        if (progress < fadeInMid)
        {
            fade = Mathf.Lerp(0.1f, 0.2f, progress / fadeInMid);
        }
        else if (progress < fadeInEnd)
        {
            float t = Mathf.InverseLerp(fadeInMid, fadeInEnd, progress);
            fade = Mathf.Lerp(0.2f, 1f, t);
        }
        else
        {
            float t = (progress - fadeInEnd) / fadeOutDuration;
            fade = Mathf.Lerp(1f, 0f, t);

            if (t >= 1f)
            {
                Stop();
                return;
            }
        }

        decalProjector.fadeFactor = fade;
    }

    public virtual void Stop()
    {
        _isRunning = false;
        Destroy(gameObject);
    }

    public virtual void UpdateTarget(Transform target) { }
}