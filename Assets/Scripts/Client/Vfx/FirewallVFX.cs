using System.Collections;
using UnityEngine;

public class FirewallVFX : MonoBehaviour, IVfx
{
    [SerializeField] private ParticleSystem leftWaveVfx;
    [SerializeField] private ParticleSystem leftTrailVfx;
    [SerializeField] private ParticleSystem rightWaveVfx;
    [SerializeField] private ParticleSystem rightTrailVfx;

    [SerializeField] private GameObject flamePrefab;
    [SerializeField] private int flamesPerSide = 3;
    [SerializeField] private float distanceBetweenFlames = 1.5f;
    [SerializeField] private float initialDelay = 0f;

    private float distanceOverTime = 2f;
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        rightWaveVfx.transform.localRotation = Quaternion.Euler(0, -90, 0);
        leftWaveVfx.transform.localRotation = Quaternion.Euler(0, 90, 0);

        SetTrailDistance(leftTrailVfx);
        SetTrailDistance(rightTrailVfx);

        SetMainVelocity(leftWaveVfx);
        SetMainVelocity(rightWaveVfx);
    }

    public void Initialize(VfxSpawnParams spawnParams, Transform target = null)
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        transform.position = spawnParams.Position;
        transform.rotation = spawnParams.Rotation;
        transform.localScale = spawnParams.Scale;

        float stepDelay = leftWaveVfx.main.startLifetime.constant / flamesPerSide;

        spawnCoroutine = StartCoroutine(SpawnFirewall(stepDelay, spawnParams.Duration));
    }

    public void Stop()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        Destroy(gameObject);
    }

    public void UpdateTarget(Transform target)
    {
    }

    private IEnumerator SpawnFirewall(float stepDelay, float duration)
    {
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        Instantiate(flamePrefab, transform.position, transform.rotation, transform);

        for (int i = 1; i <= flamesPerSide; i++)
        {
            if (stepDelay > 0f)
                yield return new WaitForSeconds(stepDelay);

            float offset = i * distanceBetweenFlames;
            Instantiate(flamePrefab, transform.position + (transform.right * offset), transform.rotation, transform);
            Instantiate(flamePrefab, transform.position - (transform.right * offset), transform.rotation, transform);
        }

        // Wait out the remainder of the duration before destroying
        float elapsedSpawnTime = (flamesPerSide + 1) * stepDelay + initialDelay;
        float remainingDuration = duration - elapsedSpawnTime;

        if (remainingDuration > 0f)
            yield return new WaitForSeconds(remainingDuration);

        Stop();
    }

    private void SetTrailDistance(ParticleSystem vfx)
    {
        var velocity = vfx.velocityOverLifetime;
        velocity.enabled = true;
        velocity.orbitalY = distanceOverTime;
    }

    private void SetMainVelocity(ParticleSystem vfx)
    {
        var main = vfx.main;
        main.startLifetime = 1f;
        var velocity = vfx.velocityOverLifetime;
        velocity.enabled = true;
        velocity.z = 4f;
    }
}