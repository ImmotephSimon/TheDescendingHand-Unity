
using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileComponent : CardComponent
{
    private ProjectileInfo _info;
    private Func<GameObject, GameObject> _onSpawnProjectile;
    private const float MultiProjectileAngle = 15f;

    public event Action<HitInfo> OnProjectileHit;
    public event Action<ProjectileController> OnSpawned;

    public void Configure(
        ProjectileInfo info,
        Func<GameObject,GameObject> spawnNetworkObject)
    {
        _info = info;
        _onSpawnProjectile = spawnNetworkObject;
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

        ServerPrefabRegistry.Instance.TryGetPrefab(GameTags.PrefabProjectile, out GameObject prefab);

        var projectile = UnityEngine.Object.Instantiate(
            prefab,
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