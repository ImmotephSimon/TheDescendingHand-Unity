using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class LoadoutSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private EquipmentType slotType;
    [SerializeField] private Image icon;

    public event Action<EquipmentType> OnSlotRightClicked;
    public event Action<EquipmentType> OnSlotHoverEnter;
    public event Action OnSlotHoverExit;

    public EquipmentType SlotType => slotType;

    public void UpdateSlot(Sprite itemIcon, Sprite emptySprite)
    {
        icon.sprite = itemIcon != null ? itemIcon : emptySprite;
        icon.enabled = icon.sprite != null;
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
        OnSlotHoverEnter?.Invoke(slotType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnSlotHoverExit?.Invoke();
    }
}