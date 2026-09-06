using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PlayerItemsSync;

public class LoadoutView : MonoBehaviour
{
    [SerializeField] private List<LoadoutSlotView> slots;
    [SerializeField] private Sprite emptySlotSprite;

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
        var dto = _items.EquippedItems.FirstOrDefault(x => x.EquipmentTypeId == type.ID);
        if (dto.Equals(default(ItemTooltipDto))) return;

        TooltipController.Instance.ShowItem(dto);
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
            var dto = _items.EquippedItems
                .FirstOrDefault(x => x.EquipmentTypeId == slot.SlotType.ID);

            if (dto.Equals(default(ItemTooltipDto)))
            {
                slot.UpdateSlot(null, emptySlotSprite);
                continue;
            }
            ItemRegistry.Instance.TryGetDefinition(dto.BaseTypeId, out ItemDefinition def);

            slot.UpdateSlot(def.Appearance.Icon, emptySlotSprite);
        }
    }

    private void ClearView()
    {
        foreach (var slot in slots)
            slot.UpdateSlot(null, emptySlotSprite);
    }
}