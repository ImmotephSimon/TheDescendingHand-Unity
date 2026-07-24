
using System;
using UnityEngine;

public class ProjectileComponent : CardComponent
{
    private readonly ProjectileInfo _info;
    private readonly Action<GameObject> _networkSpawn;
    private const float MultiProjectileAngle = 15f;
    public ProjectileComponent(
        ProjectileInfo info,
        Action<GameObject> networkSpawn)
    {
        _info = info;
        _networkSpawn = networkSpawn;

        if (info == null) Debug.LogError($"[{nameof(ProjectileComponent)}] Construction failed: ProjectileInfo is null.");
        else if (info.Prefab == null) Debug.LogError($"[{nameof(ProjectileComponent)}] Construction failed: Prefab is unassigned in ProjectileInfo.");

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
        controller.OnHit += Card.OnHit;
        controller.Initialize(_info, Owner);

        _networkSpawn?.Invoke(projectile);
    }



    protected override void OnBegin()
    {
    }

    protected override void OnCancel()
    {
    }
}