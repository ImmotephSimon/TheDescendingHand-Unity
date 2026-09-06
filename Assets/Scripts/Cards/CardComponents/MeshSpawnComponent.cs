using System;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshSpawnComponent : CardComponent
{
    private GameObject _prefab;
    private Func<GameObject, GameObject> _serverSpawn;
    private Vector3 _offset;
    private bool _useGravity;
    private float _scale;

    public event Action<GameObject> OnSpawned;

    public void Configure(
        GameObject prefab,
        Func<GameObject, GameObject> spawnNetworkObject,
        Vector3 offset = default,
        bool useGravity = false,
        float scale = 1f)
    {
        _prefab = prefab;
        _serverSpawn = spawnNetworkObject;
        _offset = offset;
        _useGravity = useGravity;
        _scale = scale;
    }

    protected override void OnActivate()
    {
        Vector3 position = Card.TargetLocation + _offset;

        GameObject spawned = UnityEngine.Object.Instantiate(
            _prefab,
            position,
            Owner.Transform.rotation);
        spawned.transform.localScale = Vector3.one * _scale;
        spawned.layer = Owner.AttackLayer;

        Rigidbody rb = spawned.GetComponent<Rigidbody>();
        if (rb == null)
            rb = spawned.AddComponent<Rigidbody>();

        rb.isKinematic = !_useGravity;
        rb.useGravity = _useGravity;


        if (_serverSpawn != null)
            spawned = _serverSpawn(spawned);
        OnSpawned?.Invoke(spawned);
    }

}