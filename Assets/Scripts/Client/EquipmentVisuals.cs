using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentVisuals : MonoBehaviour
{
    [Serializable]
    private struct SlotBinding
    {
        public EquipmentSlot Slot;
        public Transform BoneAnchor;
    }

    [SerializeField] private List<SlotBinding> slots;
    private PlayerItemsNetwork _itemsNetwork;

    private readonly Dictionary<EquipmentSlot, Transform> _boneAnchors = new();
    private readonly Dictionary<EquipmentSlot, GameObject> _activeVisuals = new();
    private readonly Dictionary<EquipmentSlot, EquippedItemDto> _applied = new();
    private readonly HashSet<EquipmentSlot> _present = new();
    private readonly List<EquipmentSlot> _toClear = new();

    private ClientRoot _clientRoot;
    private bool _subscribedToLoadout;

    private void Awake()
    {
        ClientRoot.Instance.OnClientPlayerReady += HandleClientPlayerReady;

        foreach (var binding in slots)
        {
            if (binding.BoneAnchor != null)
                _boneAnchors[binding.Slot] = binding.BoneAnchor;
        }
    }

    private void HandleClientPlayerReady(ClientPlayer client)
    {
        _itemsNetwork = client.GetComponent<PlayerItemsNetwork>();

        if (_itemsNetwork == null)
        {
            Debug.LogError("EquipmentVisuals: PlayerItemsNetwork reference is missing.", this);
            return;
        }

        if (!_subscribedToLoadout)
        {
            _itemsNetwork.LoadoutChanged += OnLoadoutChanged;
            _subscribedToLoadout = true;
        }

        OnLoadoutChanged();
    }

    private void OnDisable()
    {
        if (_clientRoot != null)
            _clientRoot.OnClientPlayerReady -= HandleClientPlayerReady;

        if (_subscribedToLoadout && _itemsNetwork != null)
        {
            _itemsNetwork.LoadoutChanged -= OnLoadoutChanged;
            _subscribedToLoadout = false;
        }

        _clientRoot = null;
    }

    private void OnLoadoutChanged()
    {
        _present.Clear();

        foreach (EquippedItemDto dto in _itemsNetwork.EquippedItems)
        {
            _present.Add(dto.Slot);

            if (_applied.TryGetValue(dto.Slot, out EquippedItemDto current) &&
                current.Equals(dto))
                continue;

            SetEquipment(dto.Slot, ResolveWorldModel(dto));
            _applied[dto.Slot] = dto;
        }

        _toClear.Clear();

        foreach (var slot in _applied.Keys)
        {
            if (!_present.Contains(slot))
                _toClear.Add(slot);
        }

        foreach (var slot in _toClear)
        {
            ClearEquipment(slot);
            _applied.Remove(slot);
        }
    }

    private GameObject ResolveWorldModel(EquippedItemDto dto)
    {
        var catalog = WorldModelCatalog.Instance;

        if (catalog == null ||
            !catalog.TryGetPrefab(dto.BaseTypeId, out GameObject prefab))
        {
            Debug.LogWarning(
                $"No world model found for item {dto.BaseTypeId}.", this);
            return null;
        }

        return prefab;
    }

    public void SetEquipment(EquipmentSlot slot, GameObject worldModel)
    {
        if (worldModel == null)
        {
            ClearEquipment(slot);
            return;
        }

        if (!_boneAnchors.TryGetValue(slot, out Transform anchor))
        {
            Debug.LogWarning($"No bone anchor assigned for {slot}", this);
            return;
        }

        ClearEquipment(slot);

        GameObject visual = Instantiate(worldModel, anchor, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        _activeVisuals[slot] = visual;
    }

    public void ClearEquipment(EquipmentSlot slot)
    {
        if (!_activeVisuals.TryGetValue(slot, out GameObject current))
            return;

        if (current != null)
            Destroy(current);

        _activeVisuals.Remove(slot);
    }
}