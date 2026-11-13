using System;
using UnityEngine;

public class DropView : MonoBehaviour
{
    private DropNetwork _network;

    private void Awake()
    {
        _network = GetComponent<DropNetwork>();
        _network.OnIdChanged += OnIdChanged;
    }

    private void OnDestroy()
    {
        if (_network != null) _network.OnIdChanged -= OnIdChanged;
    }

    private void OnIdChanged(Guid id)
    {
        if (id == Guid.Empty) return;

        if (!WorldModelCatalog.Instance.TryGetPrefab(id, out var prefab))
        {
            Debug.LogError($"No world model for item '{id}'.");
            return;
        }

        var mesh = Instantiate(prefab, transform);

        uint mask = (1u << 0) | (1u << 1);
        foreach (var r in mesh.GetComponentsInChildren<Renderer>())
            r.renderingLayerMask = mask;

    }
}