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
            slot.OnSlotRightClicked += HandleSlotRightClicked;
    }

    private void HandleSlotRightClicked(EquipmentType type)
    {
        _items.RequestUnequip(type.ID);
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

            if (dto.Equals(default(EquippedItemDto)))
            {
                slot.UpdateSlot(null, emptySlotSprite);
                continue;
            }

            var item = _items.ReconstructItem(dto);
            slot.UpdateSlot(item, emptySlotSprite);
        }
    }

    private void ClearView()
    {
        foreach (var slot in slots)
            slot.UpdateSlot(null, emptySlotSprite);
    }
}