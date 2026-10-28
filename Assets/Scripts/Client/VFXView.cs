using FishNet.Object;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Client-assembly home for all VFX instantiation.
///
/// Two entry points:
///  1. Server-driven effects: listens to the Shared VfxNet relay, tracked by Guid.
///  2. Attached visuals (e.g. projectile trails): called directly by client views, tracked by Transform.
///
/// Takes ready prefabs. Resolving IDs to prefabs (VfxRegistry / future VfxString)
/// is the caller's job, so this class has no registry dependency.
/// </summary>
public class VFXView : MonoBehaviour
{
    private readonly Dictionary<Guid, GameObject> _byId = new();
    private readonly Dictionary<Transform, List<GameObject>> _attached = new();

    private VfxNet _net;

    // Start, not OnEnable: VfxNet.Instance is assigned in its Awake.
    private void Start()
    {
        ClientRoot.Instance.OnClientPlayerReady += HandleClientPlayerReady;
    }

    private void HandleClientPlayerReady(ClientPlayer player)
    {
        _net = VfxNet.Instance;

        if (_net == null)
        {
            Debug.LogError("VFXView: no VfxNet found when client became ready.", this);
            return;
        }

        _net.SpawnRequested += OnSpawnRequested;
        _net.StopRequested += OnStopRequested;
    }

    private void OnDestroy()
    {
        if (ClientRoot.Instance != null)
            ClientRoot.Instance.OnClientPlayerReady -= HandleClientPlayerReady;

        if (_net == null) return;

        _net.SpawnRequested -= OnSpawnRequested;
        _net.StopRequested -= OnStopRequested;
    }

    // ---------------------------------------------------------------- server-driven

    private void OnSpawnRequested(VfxString vfxString, VfxSpawnParams p, NetworkObject attachTarget)
    {
        if (!VfxCatalog.Instance.TryGetPrefab(vfxString, out GameObject prefab))
            return;

        GameObject instance = p.Attach && attachTarget != null
            ? Instantiate(prefab, attachTarget.transform, false)
            : Instantiate(prefab, p.Position, p.Rotation);

        instance.transform.localScale = p.Scale;
        instance.GetComponentInChildren<IVfx>()?.Initialize(p);

        _byId[p.InstanceId] = instance;
    }

    private void OnStopRequested(Guid instanceId)
    {
        if (_byId.Remove(instanceId, out GameObject instance))
            Release(instance);
    }

    // ---------------------------------------------------------------- attached visuals

    public GameObject AttachAbilityVisual(GameObject prefab, Transform target)
    {
        if (prefab == null || target == null)
        {
            Debug.LogWarning("VFXView: missing prefab or target for attach.");
            return null;
        }

        GameObject instance = Instantiate(prefab, target);
        instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        var spawnParams = new VfxSpawnParams(target.position) { Rotation = target.rotation };
        instance.GetComponentInChildren<IVfx>()?.Initialize(spawnParams, target);

        if (!_attached.TryGetValue(target, out var list))
        {
            list = new List<GameObject>();
            _attached[target] = list;
        }
        list.Add(instance);

        return instance;
    }

    /// <summary>Stops and forgets every visual attached to this target.</summary>
    public void DetachAbilityVisual(Transform target)
    {
        // `is null` skips Unity's overloaded ==, so a destroyed (but non-null) key still gets cleaned up.
        if (target is null) return;

        if (!_attached.Remove(target, out var list)) return;

        for (int i = list.Count - 1; i >= 0; i--)
            Release(list[i]);
    }

    // ---------------------------------------------------------------- shared

    private static void Release(GameObject instance)
    {
        if (instance == null) return;

        IVfx vfx = instance.GetComponentInChildren<IVfx>();
        if (vfx != null)
            vfx.Stop();     // assumes Stop() handles its own cleanup, as the old code did
        else
            Destroy(instance);
    }
}