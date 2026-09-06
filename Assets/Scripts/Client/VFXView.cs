using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class VFXView : MonoBehaviour
{
    private readonly Dictionary<Transform, List<GameObject>> _activeVisuals = new();

    public void AttachAbilityVisual(GameObject prefab, Transform target)
    {
        if (prefab == null)
        {
            Debug.LogWarning("No VFX prefab provided.");
            return;
        }

        var instance = Instantiate(prefab, target);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        if (instance.TryGetComponent<IVfx>(out var vfx))
        {
            var spawnParams = new VfxSpawnParams(target.position)
            {
                Rotation = target.rotation
            };

            vfx.Initialize(spawnParams, target);
        }

        if (!_activeVisuals.TryGetValue(target, out var list))
        {
            list = new List<GameObject>();
            _activeVisuals[target] = list;
        }
        list.Add(instance);
    }

    public void DetachAbilityVisual(GameObject prefab, Transform target)
    {
        if (!_activeVisuals.TryGetValue(target, out var list)) return;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            var instance = list[i];
            if (instance != null)
            {
                if (instance.TryGetComponent<IVfx>(out var vfx))
                {
                    Debug.Log($"Client {prefab.name} stop folliwng: {target.name}");
                    vfx.Stop();
                }
                else
                {
                    Destroy(instance);
                }
            }
        }

        _activeVisuals.Remove(target);
    }
}