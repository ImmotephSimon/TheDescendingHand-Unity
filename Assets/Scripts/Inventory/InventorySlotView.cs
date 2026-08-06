using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI debugText;

    private int _row;
    private int _column;

    public int Row => _row;
    public int Column => _column;

    public event Action<InventorySlotView, PointerEventData> OnSlotRightClicked;
    public event Action<InventorySlotView, PointerEventData> OnSlotClicked;
    public event Action<InventorySlotView> OnSlotHovered;
    public event Action<InventorySlotView> OnSlotUnhovered;

    public void Initialize(int row, int column)
    {
        _row = row;
        _column = column;
    }


    public void SetHighlight(Color color)
    {
        if (backgroundImage != null)
            backgroundImage.color = color;
    }

    public void ClearHighlight()
    {
        if (backgroundImage != null)
            backgroundImage.color = Color.white;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnSlotRightClicked?.Invoke(this, eventData);
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnSlotClicked?.Invoke(this, eventData);
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        OnSlotHovered?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnSlotUnhovered?.Invoke(this);
    }
}