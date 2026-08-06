using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IInventory
{
    int Rows { get; }
    int Columns { get; }

    IReadOnlyDictionary<ItemInstance, Vector2Int> GetPlacedItems();
    bool TryGet(int row, int column, out ItemInstance item);

    bool TryAdd(ItemInstance item);
    bool TryRemove(ItemInstance item);

    void SlotClicked(int row, int column, PointerEventData eventData);
    void SlotRightClicked(int row, int column, PointerEventData eventData);
    void SlotHovered(int row, int column);
    void SlotUnhovered(int row, int column);

    event Action OnChanged;
}