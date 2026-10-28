using System.Collections.Generic;
using UnityEngine;

public class LoadoutView : MonoBehaviour
{
    [SerializeField] private List<LoadoutSlotView> slots;

    private PlayerItemsNetwork _items;
    private EquipmentSlot? _hovered;

    private void Awake()
    {
        foreach (var slot in slots)
        {
            slot.OnSlotRightClicked += HandleSlotRightClicked;
            slot.OnSlotHoverEnter += HandleSlotHoverEnter;
            slot.OnSlotHoverExit += HandleSlotHoverExit;
        }
    }

    private void OnDestroy()
    {
        foreach (var slot in slots)
        {
            slot.OnSlotRightClicked -= HandleSlotRightClicked;
            slot.OnSlotHoverEnter -= HandleSlotHoverEnter;
            slot.OnSlotHoverExit -= HandleSlotHoverExit;
        }

        Unsubscribe();
    }

    public void Bind(PlayerItemsNetwork items)
    {
        Unsubscribe();
        _items = items;

        if (_items == null)
        {
            ClearView();
            return;
        }

        _items.LoadoutChanged += RefreshLoadoutView;
        _items.EquippedTooltipReceived += OnEquippedTooltipReceived;
        RefreshLoadoutView();
    }

    private void Unsubscribe()
    {
        if (_items == null) return;
        _items.LoadoutChanged -= RefreshLoadoutView;
        _items.EquippedTooltipReceived -= OnEquippedTooltipReceived;
    }

    private void HandleSlotHoverEnter(EquipmentSlot slot)
    {
        if (_items == null || !_items.TryGetEquipped(slot, out _)) return;

        _hovered = slot;
        _items.RequestEquippedTooltip(slot);
    }

    private void OnEquippedTooltipReceived(EquipmentSlot slot, TooltipDto dto)
    {
        if (_hovered != slot) return; // mouse left or moved, discard stale response
        TooltipController.Instance.Show(dto);
    }

    private void HandleSlotHoverExit()
    {
        _hovered = null;
        TooltipController.Instance.Hide();
    }

    private void HandleSlotRightClicked(EquipmentSlot slot)
        => _items?.RequestUnequip(slot); // intent only, wait for server

    private void RefreshLoadoutView()
    {
        foreach (var slot in slots)
        {
            if (_items.TryGetEquipped(slot.SlotType, out var equipped))
            {
                SpriteCatalog.Instance.TryGetSprite(equipped.Icon, out var sprite);
                slot.UpdateSlot(sprite);
            }
            else
            {
                slot.UpdateSlot(null);
            }
        }
    }

    private void ClearView()
    {
        foreach (var slot in slots)
            slot.UpdateSlot(null);
    }
}