using System;
using UnityEngine;

public class ClientEnemy : MonoBehaviour
{
    [SerializeField] private Transform visualAnchor;
    [SerializeField] private GameObject healthBarPrefab;

    private EnemyStatsNetwork _network;
    private GameObject _model;

    private void Awake()
    {
        _network = GetComponent<EnemyStatsNetwork>();
        _network.WorldModelChanged += BuildModel;

        Instantiate(healthBarPrefab, transform);
    }

    private void OnDestroy()
    {
        if (_network != null)
            _network.WorldModelChanged -= BuildModel;
    }

    private void BuildModel(Guid id)
    {
        if (!WorldModelCatalog.Instance.TryGetPrefab(id, out var prefab))
        {
            Debug.LogWarning($"{name}: no world model in catalog for {id}", this);
            return;
        }

        _model = Instantiate(prefab, visualAnchor);
    }
}