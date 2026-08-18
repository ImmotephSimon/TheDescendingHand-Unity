using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IInventory
{
    int Rows { get; }
    int Columns { get; }
    bool TryGet(Guid id, out IInventoryItem item);
    IReadOnlyDictionary<IInventoryItem, Vector2Int> GetPlacedItems();
    bool TryGet(int row, int column, out IInventoryItem item);

    bool TryAdd(IInventoryItem item);
    bool TryRemove(IInventoryItem item);

    void SlotClicked(int row, int column, PointerEventData eventData);
    void SlotRightClicked(int row, int column, PointerEventData eventData);
    void SlotHovered(int row, int column);
    void SlotUnhovered(int row, int column);

    event Action OnChanged;
}