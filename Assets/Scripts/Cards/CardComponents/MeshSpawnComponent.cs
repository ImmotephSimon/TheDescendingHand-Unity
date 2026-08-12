using System;
using UnityEngine;

public class MeshSpawnComponent : CardComponent
{
    private readonly GameObject _prefab;
    private readonly Action<GameObject> _onSpawn;
    private readonly Vector3 _offset;
    private readonly bool _useGravity;

    public event Action<GameObject> OnSpawned;

    public MeshSpawnComponent(
        GameObject prefab,
        Action<GameObject> spawnNetworkObject,
        Vector3 offset = default,
        bool useGravity = false)
    {
        _prefab = prefab;
        _onSpawn = spawnNetworkObject;
        _offset = offset;
        _useGravity = useGravity;
    }

    protected override void OnBegin()
    {
        Vector3 position = Card.TargetLocation + _offset;

        GameObject spawned = UnityEngine.Object.Instantiate(
            _prefab,
            position,
            Owner.Transform.rotation);

        spawned.layer = Owner.AttackLayer;

        if (spawned.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = !_useGravity;
            rb.useGravity = _useGravity;
        }

        _onSpawn?.Invoke(spawned);
        OnSpawned?.Invoke(spawned);
    }

}