using System;
using System.Collections.Generic;
using UnityEngine;

public class LoadoutView : MonoBehaviour
{
    [SerializeField] private List<LoadoutSlotView> slots;
    [SerializeField] private Sprite emptySlotSprite;

    private Loadout _loadout;

    private void Awake()
    {
        foreach (var slot in slots)
        {
            slot.OnSlotRightClicked += HandleSlotRightClicked;
        }
    }

    private void HandleSlotRightClicked(EquipmentType type)
    {
        if (_loadout.GetEquipped(type) != null)
        {
            _loadout.Unequip(type);
        }
    }

    public void Bind(Loadout loadout)
    {
        _loadout = loadout;

        _loadout.OnLoadoutChanged += RefreshLoadoutView;
        RefreshLoadoutView();
    }

    private void OnDestroy()
    {
        if (_loadout != null)
        {
            _loadout.OnLoadoutChanged -= RefreshLoadoutView;
        }
    }

    public void RefreshLoadoutView()
    {
        foreach (var slot in slots)
        {
            ItemInstance item = _loadout.GetEquipped(slot.SlotType);
            slot.UpdateSlot(item, emptySlotSprite);
        }
    }

    private void ClearView()
    {
        foreach (var slot in slots)
        {
            slot.UpdateSlot(null, emptySlotSprite);
        }
    }
}