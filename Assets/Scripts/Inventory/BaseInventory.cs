using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseInventory : MonoBehaviour, IInventory
{
    public virtual int Rows => 4;
    public virtual int Columns => 4;

    private IInventoryItem[,] occupancyGrid;

    // Fast lookup for item top-left origin points
    private readonly Dictionary<IInventoryItem, Vector2Int> itemPositions = new();

    public event Action OnChanged;

    protected virtual void Awake()
    {
        occupancyGrid = new IInventoryItem[Rows, Columns];
    }

    public IReadOnlyDictionary<IInventoryItem, Vector2Int> GetItemPositions()
    {
        return itemPositions;
    }

    public bool TryGet(int row, int column, out IInventoryItem item)
    {
        if (!IsWithinGrid(row, column))
        {
            item = null;
            return false;
        }

        item = occupancyGrid[row, column];
        return item != null;
    }
    public bool TryAdd(IInventoryItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item), "Cannot add null ItemInstance to inventory.");

        if (itemPositions.ContainsKey(item))
        {
            Debug.LogWarning($"[{nameof(TryAdd)}] Item '{item}' is already present in inventory.");
            return false;
        }

        if (!TryFindFreeSlot(item.Size, out Vector2Int origin))
            return false; // Grid full / no space

        OccupyCells(item, origin);
        OnChanged?.Invoke();
        return true;
    }
    public bool TryAddAt(IInventoryItem item, int row, int column)
    {
        Vector2Int size = item.Size;

        // Check grid bounds and availability
        if (row < 0 || column < 0 || row + size.y > Rows || column + size.x > Columns)
            return false;

        if (!CanFitAt(row, column, size))
            return false;

        OccupyCells(item, new Vector2Int(column, row));
        OnChanged?.Invoke();
        return true;
    }

    public bool TryRemove(IInventoryItem item)
    {
        if (!itemPositions.TryGetValue(item, out Vector2Int origin))
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

    private void OccupyCells(IInventoryItem item, Vector2Int origin)
    {
        Vector2Int size = item.Size;
        for (int r = 0; r < size.y; r++)
        {
            for (int c = 0; c < size.x; c++)
            {
                occupancyGrid[origin.y + r, origin.x + c] = item;
            }
        }
        itemPositions[item] = origin;
    }

    private void ClearCells(IInventoryItem item, Vector2Int origin)
    {
        Vector2Int size = item.Size;
        for (int r = 0; r < size.y; r++)
        {
            for (int c = 0; c < size.x; c++)
            {
                occupancyGrid[origin.y + r, origin.x + c] = null;
            }
        }
    }

    private bool IsWithinGrid(int r, int c) => r >= 0 && r < Rows && c >= 0 && c < Columns;

    public virtual void SlotLeftClicked(int row, int column)
    {
    }

    public virtual void SlotRightClicked(int row, int column)
    {   

    }

    public bool TryGet(Guid id, out IInventoryItem item)
    {
        foreach (var entry in itemPositions)
        {
            if (entry.Key.InventoryId == id)
            {
                item = entry.Key;
                return true;
            }
        }

        item = null;
        return false;
    }
}