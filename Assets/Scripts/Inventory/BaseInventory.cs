using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseInventory : MonoBehaviour, IInventory
{
    public virtual int Rows => 4;
    public virtual int Columns => 4;

    private ItemInstance[,] occupancyGrid;

    // Fast lookup for item top-left origin points
    private readonly Dictionary<ItemInstance, Vector2Int> itemPositions = new();

    public event Action OnChanged;

    protected virtual void Awake()
    {
        occupancyGrid = new ItemInstance[Rows, Columns];
    }

    public IReadOnlyDictionary<ItemInstance, Vector2Int> GetPlacedItems()
    {
        return itemPositions;
    }

    public bool TryGet(int row, int column, out ItemInstance item)
    {
        if (!IsWithinGrid(row, column))
        {
            item = null;
            return false;
        }

        item = occupancyGrid[row, column];
        return item != null;
    }
    public bool TryAdd(ItemInstance item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item), "Cannot add null ItemInstance to inventory.");

        if (item.BaseType == null)
            throw new InvalidOperationException($"[{nameof(TryAdd)}] ItemInstance missing BaseType definition.");

        if (itemPositions.ContainsKey(item))
        {
            Debug.LogWarning($"[{nameof(TryAdd)}] Item '{item.BaseType.name}' is already present in inventory.");
            return false;
        }

        if (!TryFindFreeSlot(item.BaseType.InventorySize, out Vector2Int origin))
            return false; // Grid full / no space

        OccupyCells(item, origin);
        OnChanged?.Invoke();
        return true;
    }
    public bool TryAddAt(ItemInstance item, int row, int column)
    {
        if (item?.BaseType == null) return false;
        Vector2Int size = item.BaseType.InventorySize;

        // Check grid bounds and availability
        if (row < 0 || column < 0 || row + size.y > Rows || column + size.x > Columns)
            return false;

        if (!CanFitAt(row, column, size))
            return false;

        OccupyCells(item, new Vector2Int(column, row));
        OnChanged?.Invoke();
        return true;
    }

    public bool TryRemove(ItemInstance item)
    {
        if (item?.BaseType == null || !itemPositions.TryGetValue(item, out Vector2Int origin))
            return false;

        ClearCells(item, origin);
        itemPositions.Remove(item);
        OnChanged?.Invoke();
        return true;
    }

    public bool CanAdd(Vector2Int itemSize)
    {
        return TryFindFreeSlot(itemSize, out _);
    }

    private bool TryFindFreeSlot(Vector2Int size, out Vector2Int origin)
    {
        for (int r = 0; r <= Rows - size.y; r++)
        {
            for (int c = 0; c <= Columns - size.x; c++)
            {
                if (CanFitAt(r, c, size))
                {
                    origin = new Vector2Int(c, r);
                    return true;
                }
            }
        }

        origin = Vector2Int.zero;
        return false;
    }

    private bool CanFitAt(int startRow, int startCol, Vector2Int size)
    {
        for (int r = 0; r < size.y; r++)
        {
            for (int c = 0; c < size.x; c++)
            {
                if (occupancyGrid[startRow + r, startCol + c] != null)
                    return false;
            }
        }
        return true;
    }

    private void OccupyCells(ItemInstance item, Vector2Int origin)
    {
        Vector2Int size = item.BaseType.InventorySize;
        for (int r = 0; r < size.y; r++)
        {
            for (int c = 0; c < size.x; c++)
            {
                occupancyGrid[origin.y + r, origin.x + c] = item;
            }
        }
        itemPositions[item] = origin;
    }

    private void ClearCells(ItemInstance item, Vector2Int origin)
    {
        Vector2Int size = item.BaseType.InventorySize;
        for (int r = 0; r < size.y; r++)
        {
            for (int c = 0; c < size.x; c++)
            {
                occupancyGrid[origin.y + r, origin.x + c] = null;
            }
        }
    }

    private bool IsWithinGrid(int r, int c) => r >= 0 && r < Rows && c >= 0 && c < Columns;

    public virtual void SlotClicked(int row, int column, PointerEventData eventData)
    {
    }

    public virtual void SlotRightClicked(int row, int column, PointerEventData eventData)
    {   

    }
    public virtual void SlotHovered(int row, int column)
    {   
    }

    public virtual void SlotUnhovered(int row, int column)
    {   
    }
}