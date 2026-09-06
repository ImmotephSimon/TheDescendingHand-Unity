using System.Collections.Generic;
using UnityEngine;
using System;

public class EquipmentVisuals : MonoBehaviour
{
    [Serializable]
    private struct SlotBinding
    {
        public EquipmentType Type;
        public Transform BoneAnchor;
    }

    [SerializeField] private List<SlotBinding> slots;

    private readonly Dictionary<EquipmentType, Transform> _boneAnchors = new();
    private readonly Dictionary<EquipmentType, GameObject> _activeVisuals = new();

    private void Awake()
    {
        foreach (var slot in slots)
        {
            if (slot.BoneAnchor != null)
                _boneAnchors[slot.Type] = slot.BoneAnchor;
        }
    }

    public void SetEquipment(EquipmentType type, ItemDefinition item)
    {
        if (item == null)
        {
            ClearEquipment(type);
            return;
        }

        if (item.Appearance?.WorldModel == null)
        {
            Debug.LogWarning($"Item '{item.Id}' has no EquippedPrefab assigned for {type}.");
            return;
        }

        ClearEquipment(type);

        if (!_boneAnchors.TryGetValue(type, out Transform anchor))
        {
            Debug.LogWarning($"No bone anchor assigned for {type}");
            return;
        }

        GameObject visual = Instantiate(item.Appearance.WorldModel, anchor, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        _activeVisuals[type] = visual;
    }

    public void ClearEquipment(EquipmentType type)
    {
        if (_activeVisuals.TryGetValue(type, out GameObject currentVisual))
        {
            if (currentVisual != null)
                Destroy(currentVisual);

            _activeVisuals.Remove(type);
        }
    }
}