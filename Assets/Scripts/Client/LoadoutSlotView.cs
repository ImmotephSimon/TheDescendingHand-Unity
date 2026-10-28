using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoadoutSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private EquipmentSlot slotType;

    private Image icon;
    public event Action<EquipmentSlot> OnSlotRightClicked;
    public event Action<EquipmentSlot> OnSlotHoverEnter;
    public event Action OnSlotHoverExit;

    public EquipmentSlot SlotType => slotType;

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