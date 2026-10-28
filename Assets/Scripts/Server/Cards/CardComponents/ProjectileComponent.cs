
using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileComponent : CardComponent
{
    private ProjectileInfo _info;
    private CardInitContext _context;
    private const float MultiProjectileAngle = 15f;

    public event Action<HitInfo> OnProjectileHit;
    public event Action<ProjectileController> OnSpawned;

    public void Configure(ProjectileInfo info, CardInitContext context)
    {
        _info = info;
        _context = context;
    }


    protected override void OnActivate()
    {
        Transform frame = _context.IsTriggered ? Card.transform : Owner.Transform;

        for (int i = 0; i < _info.Count; i++)
        {
            float angle = GetSpreadAngle(i, _info.Count);
            Vector3 pos = _info.GetSpawnPosition(frame);
            Quaternion rot = frame.rotation * Quaternion.Euler(0, angle, 0);

            SpawnProjectile(pos, rot, frame);
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

    private ProjectileController SpawnProjectile(Vector3 position, Quaternion rotation, Transform frame = null)
    {
        ServerPrefabRegistry.Instance.TryGetPrefab(GameTags.PrefabProjectile, out GameObject prefab);

        var projectile = UnityEngine.Object.Instantiate(prefab, position, rotation);

        var controller = projectile.GetComponentInChildren<ProjectileController>();
        controller.OnHit += hit =>
        {
            OnProjectileHit?.Invoke(hit);
            Card.OnHit(hit);
        };
        controller.Initialize(_info.Clone(), Owner, frame);

        _context.ServerSpawn(projectile);
        OnSpawned?.Invoke(controller);
        return controller;
    }

}