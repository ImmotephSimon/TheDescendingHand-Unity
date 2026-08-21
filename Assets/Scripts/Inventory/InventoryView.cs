using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private InventorySlotView slotPrefab;
    [SerializeField] private ItemIconView itemIconPrefab;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private Vector2 cellSize = new(64f, 64f);

    private readonly Vector2 spacing = new(2f, 2f);
    private readonly List<ItemIconView> spawnedIcons = new();

    private PlayerItemsSync _items;
    private InventorySlotView[,] slots;

    public void Bind(PlayerItemsSync items)
    {
        if (_items != null)
            _items.InventoryChanged -= Refresh;

        _items = items;

        if (_items == null)
            return;

        _items.InventoryChanged += Refresh;
    }

    public void Initialize()
    {
        InitializeGrid();
        Refresh();
        ToggleVisibility();
    }

    private void OnDestroy()
    {
        if (_items != null)
            _items.InventoryChanged -= Refresh;
    }

    private void InitializeGrid()
    {
        if (slots != null)
        {
            foreach (var slot in slots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
        }

        int rows = _items.InventoryRows;
        int columns = _items.InventoryColumns;

        if (TryGetComponent<GridLayoutGroup>(out var gridLayout))
        {
            gridLayout.cellSize = cellSize;
            gridLayout.spacing = spacing;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = columns;
        }

        slots = new InventorySlotView[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                var slot = Instantiate(slotPrefab, transform);
                slot.Initialize(row, column);

                slot.OnSlotClicked += HandleSlotClicked;
                slot.OnSlotRightClicked += HandleSlotRightClicked;
                slot.OnSlotHovered += HandleSlotHovered;
                slot.OnSlotUnhovered += HandleSlotUnhovered;

                slots[row, column] = slot;
            }
        }
    }

    private void HandleSlotClicked(
        InventorySlotView slot,
        PointerEventData eventData)
    {
        _items.RequestInventorySlotClick(slot.Row, slot.Column);
    }

    private void HandleSlotRightClicked(
        InventorySlotView slot,
        PointerEventData eventData)
    {
        _items.RequestInventorySlotRightClick(slot.Row, slot.Column);
    }

    private void HandleSlotHovered(InventorySlotView slot)
    {
        if (_items.TryGetInventoryItem(slot.Row, slot.Column, out var item))
            TooltipController.Instance.Show(item);
    }

    private void HandleSlotUnhovered(InventorySlotView slot)
    {
        TooltipController.Instance.Hide();
    }

    private void Refresh()
    {
        foreach (var icon in spawnedIcons)
            Destroy(icon.gameObject);

        spawnedIcons.Clear();

        Transform parent = itemContainer != null
            ? itemContainer
            : transform;

        foreach (var entry in _items.GetInventoryItems())
        {
            IInventoryItem item = entry.Key;
            Vector2Int origin = entry.Value;

            ItemIconView icon = Instantiate(itemIconPrefab, parent);
            spawnedIcons.Add(icon);

            Vector2 position = GetLocalPosition(origin.x, origin.y);
            Vector2Int size = item.Size;

            Vector2 pixelSize = new(
                size.x * cellSize.x + (size.x - 1) * spacing.x,
                size.y * cellSize.y + (size.y - 1) * spacing.y
            );

            icon.Render(item.Icon, position, pixelSize);
        }
    }

    private Vector2 GetLocalPosition(int column, int row)
    {
        float x = column * (cellSize.x + spacing.x);
        float y = -row * (cellSize.y + spacing.y);

        return new Vector2(x, y);
    }

    internal void ToggleVisibility()
    {
        if (panelRoot.activeSelf)
        {
            panelRoot.SetActive(false);
            TooltipController.Instance.Hide();
        }
        else
        {
            panelRoot.SetActive(true);
        }
    }
}