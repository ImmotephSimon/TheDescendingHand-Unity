using System;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMPro.TextMeshProUGUI debugText;
    private int _row;
    private int _column;
    public void Initialize(int row, int column)
    {
        _row = row;
        _column = column;
        //debugText.SetText($"{_row}, {_column}");
    }
    public void SetHighlight(Color color)
    {
        backgroundImage.color = color;
    }

    public void ClearHighlight()
    {
        backgroundImage.color = Color.white;
    }


}