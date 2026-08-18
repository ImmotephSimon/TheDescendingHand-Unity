using System;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshSpawnComponent : CardComponent
{
    private readonly GameObject _prefab;
    private readonly Action<GameObject> _onSpawn;
    private readonly Vector3 _offset;
    private readonly bool _useGravity;
    private readonly float _scale;

    public event Action<GameObject> OnSpawned;

    public MeshSpawnComponent(
        GameObject prefab,
        Action<GameObject> spawnNetworkObject,
        Vector3 offset = default,
        bool useGravity = false,
        float scale = 1f)
    {
        _prefab = prefab;
        _onSpawn = spawnNetworkObject;
        _offset = offset;
        _useGravity = useGravity;
        _scale = scale;
    }

    protected override void OnBegin()
    {
        Vector3 position = Card.TargetLocation + _offset;

        GameObject spawned = UnityEngine.Object.Instantiate(
            _prefab,
            position,
            Owner.Transform.rotation);
        spawned.transform.localScale = Vector3.one * _scale;
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