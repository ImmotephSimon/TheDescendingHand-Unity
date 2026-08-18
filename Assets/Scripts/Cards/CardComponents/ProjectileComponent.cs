
using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileComponent : CardComponent
{
    private readonly ProjectileInfo _info;
    private readonly Action<GameObject> _onSpawnProjectile;
    private const float MultiProjectileAngle = 15f;

    public event Action<HitInfo> OnProjectileHit;
    public event Action<ProjectileController> OnSpawned;

    public ProjectileComponent(
        ProjectileInfo info,
        Action<GameObject> spawnNetworkObject)
        : base(GameTags.TypeProjectile)
    {
        _info = info;
        _onSpawnProjectile = spawnNetworkObject;
        Debug.Assert(!info.IsEmpty, $"[{nameof(ProjectileComponent)}] Construction failed: Prefab is unassigned in ProjectileInfo.");
    }


    protected override void OnActivate()
    {
        for (int i = 0; i < _info.Count; i++)
        {
            float angle = GetSpreadAngle(i, _info.Count);

            Quaternion rotation =
                Owner.Transform.rotation * Quaternion.Euler(0, angle, 0);

            SpawnProjectile(rotation);
        }
    }

    

    private float GetSpreadAngle(int index, int count)
    {
        if (count == 1)
            return 0f;

        float t = (float)index / (count - 1);

        return Mathf.Lerp(
            -MultiProjectileAngle,
            MultiProjectileAngle,
            t);
    }

    private void SpawnProjectile(Quaternion rotation)
    {
        var projectile = UnityEngine.Object.Instantiate(
            _info.Prefab,
            _info.GetSpawnPosition(Owner),
            rotation);

        var controller = projectile.GetComponentInChildren<ProjectileController>();
        controller.OnHit += hit =>
        {
            OnProjectileHit?.Invoke(hit);
            Card.OnHit(hit);
        };
        controller.Initialize(_info.Clone(), Owner);

        _onSpawnProjectile?.Invoke(projectile);
        OnSpawned?.Invoke(controller);
    }

}