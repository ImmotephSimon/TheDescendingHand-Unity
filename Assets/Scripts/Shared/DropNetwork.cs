using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UnityEngine;

public class DropNetwork : NetworkBehaviour
{
    private readonly SyncVar<Guid> _id = new SyncVar<Guid>();
    private readonly SyncVar<Color> _color = new SyncVar<Color>();
    private readonly SyncVar<float> _intensity = new SyncVar<float>();
    private readonly SyncVar<float> _range = new SyncVar<float>();

    public event Action<Guid> OnIdChanged;

    public override void OnStartClient()
    {
        _id.OnChange += OnIdIdChanged;
        _color.OnChange += OnLightChanged;
        _intensity.OnChange += OnLightChanged;
        _range.OnChange += OnLightChanged;
    }

    public override void OnStopClient()
    {
        _id.OnChange -= OnIdIdChanged;
        _color.OnChange -= OnLightChanged;
        _intensity.OnChange -= OnLightChanged;
        _range.OnChange -= OnLightChanged;
    }


    private void OnIdIdChanged(Guid prev, Guid next, bool asServer)
    {
        OnIdChanged?.Invoke(next);
    }

    private void OnLightChanged<T>(T prev, T next, bool asServer)
    {
        ApplyLight();
    }

    // Server only
    public void SetId(Guid id) => _id.Value = id;

    public void ApplyRarityVisuals(Color color, float intensity, float range)
    {
        _color.Value = color;
        _intensity.Value = intensity;
        _range.Value = range;
    }

    private void ApplyLight()
    {
        var light = GetComponentInChildren<Light>();
        if (light == null) return;

        light.color = _color.Value;
        light.intensity = _intensity.Value;
        light.range = _range.Value;
        light.renderingLayerMask = 1 << 1;
    }
}