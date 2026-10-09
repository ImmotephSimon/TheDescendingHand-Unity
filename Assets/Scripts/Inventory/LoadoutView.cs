using System.Collections.Generic;
using UnityEngine;
using static PlayerItemsSync;

public class LoadoutView : MonoBehaviour
{
    [SerializeField] private List<LoadoutSlotView> slots;

    private PlayerItemsSync _items;

    private void Awake()
    {
        foreach (var slot in slots)
        {
            slot.OnSlotRightClicked += HandleSlotRightClicked;
            slot.OnSlotHoverEnter += HandleSlotHoverEnter;
            slot.OnSlotHoverExit += HandleSlotHoverExit;
        }

    }

    private void HandleSlotHoverEnter(EquipmentType type)
    {
        foreach (var item in _items.EquippedItems)
        {
            if (item.EquipmentTypeId != type.ID) continue;

            _items.Server_RequestEquippedTooltip(type.ID);
            return;
        }
    }

    private void HandleSlotHoverExit()
    {
        TooltipController.Instance.Hide();
    }
    private void HandleSlotRightClicked(EquipmentType type)
    {
        _items.Server_RequestUnequip(type.ID);
    }

    public void Bind(PlayerItemsSync items)
    {
        if (_items != null)
            _items.LoadoutChanged -= RefreshLoadoutView;

        _items = items;

        if (_items == null)
            return;

        _items.LoadoutChanged += RefreshLoadoutView;
        RefreshLoadoutView();
    }

    private void OnDestroy()
    {
        if (_items != null)
            _items.LoadoutChanged -= RefreshLoadoutView;
    }

    private void RefreshLoadoutView()
    {
        foreach (var slot in slots)
        {
            bool found = false;
            EquippedItemDto equipped = default;

            foreach (var item in _items.EquippedItems)
            {
                if (item.EquipmentTypeId != slot.SlotType.ID) continue;
                equipped = item;
                found = true;
                break;
            }

            if (!found)
            {
                slot.UpdateSlot(null);
                continue;
            }

            if (!ItemRegistry.Instance.TryGetDefinition(equipped.BaseTypeId, out ItemDefinition def))
            {
                Debug.LogError($"Unknown item {equipped.BaseTypeId}");
                slot.UpdateSlot(null);
                continue;
            }

            slot.UpdateSlot(def.Appearance.Icon);
        }
    }

    private void ClearView()
    {
        foreach (var slot in slots)
            slot.UpdateSlot(null);
    }
}