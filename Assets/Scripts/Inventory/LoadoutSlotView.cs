using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class LoadoutSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private EquipmentType slotType;
    private Image icon;
    [SerializeField] private Sprite emptySprite;

    public event Action<EquipmentType> OnSlotRightClicked;
    public event Action<EquipmentType> OnSlotHoverEnter;
    public event Action OnSlotHoverExit;

    public EquipmentType SlotType => slotType;

    private void Awake()
    {
        icon = GetComponentInChildren<Image>();
    }

    public void UpdateSlot(Sprite itemIcon)
    {
        if (itemIcon != null)
        {
            icon.sprite = itemIcon;
            icon.color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            icon.sprite = emptySprite;
            icon.color = new Color(0.3f, 0.3f, 0.3f, 0.7f); // darkened
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
        OnSlotHoverEnter?.Invoke(slotType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnSlotHoverExit?.Invoke();
    }
}