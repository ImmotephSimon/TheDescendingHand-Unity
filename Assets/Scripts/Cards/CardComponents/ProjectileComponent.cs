using System;
using UnityEngine;

public class ProjectileComponent : CardComponent
{
    private readonly ProjectileInfo _settings;
    private readonly Action<GameObject> _networkSpawnMethod;

    public ProjectileComponent(ProjectileInfo settings, Action<GameObject> networkSpawnMethod)
    {
        _settings = settings;
        _networkSpawnMethod = networkSpawnMethod;
    }

    protected override void OnCastTimeDone()
    {
        SpawnProjectile();
    }

    private void SpawnProjectile()
    {
        var projectile = UnityEngine.Object.Instantiate(_settings.Visual, _settings.Position, _settings.Rotation);

        _networkSpawnMethod?.Invoke(projectile);
    }
}