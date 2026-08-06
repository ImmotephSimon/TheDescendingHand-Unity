using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class LoadoutSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private EquipmentType slotType;
    [SerializeField] private Image icon;

    private ItemInstance _currentItem;
    public event Action<EquipmentType> OnSlotRightClicked;
    public EquipmentType SlotType => slotType;

    public void UpdateSlot(ItemInstance item, Sprite emptySprite)
    {
        _currentItem = item;
        bool hasItem = item != null;

        if (icon != null)
        {
            icon.sprite = hasItem ? item.BaseType.Appearance.Icon : emptySprite;
            icon.enabled = icon.sprite != null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnSlotRightClicked?.Invoke(slotType);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_currentItem == null) return;
        TooltipController.Instance.ShowItem(_currentItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.Instance.Hide();
    }
}