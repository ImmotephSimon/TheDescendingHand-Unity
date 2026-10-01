using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private InventorySlotView slotPrefab;
    [SerializeField] private ItemIconView itemIconPrefab;
    [SerializeField] private RectTransform itemContainer; // the box that shrinks

    private readonly List<ItemIconView> spawnedIcons = new();

    private PlayerItemsSync _items;
    private InventorySlotView[,] slots;
    private RectTransform grid;
    private Vector2 lastBoxSize = new(-1f, -1f);

    internal void ToggleVisibility() => SetVisibility(!panelRoot.activeSelf);

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
        CreateGridRoot();
        InitializeGrid();
        Refresh();
        SetVisibility(false);
    }

    private void OnDestroy()
    {
        if (_items != null)
            _items.InventoryChanged -= Refresh;
    }

    // Slots and icons both live in this child, so they always line up.
    private void CreateGridRoot()
    {
        if (grid != null)
            return;

        var go = new GameObject("Grid", typeof(RectTransform));
        grid = (RectTransform)go.transform;
        grid.SetParent(itemContainer, false);
        grid.anchorMin = grid.anchorMax = grid.pivot = new Vector2(0.5f, 0.5f);
        grid.anchoredPosition = Vector2.zero;
    }

    private void LateUpdate()
    {
        if (_items == null || slots == null || grid == null)
            return;

        Vector2 size = itemContainer.rect.size;
        if (size == lastBoxSize)
            return;

        lastBoxSize = size;
        FitToBox(size);
    }

    // cell = min(width / columns, height / rows): the tighter axis caps the size.
    private void FitToBox(Vector2 box)
    {
        int rows = _items.InventoryRows;
        int columns = _items.InventoryColumns;

        float cell = Mathf.Max(0f, Mathf.Min(box.x / columns, box.y / rows));
        grid.sizeDelta = new Vector2(cell * columns, cell * rows);
    }

    // Places a rect at (column,row) spanning (w,h) cells, as fractions of its parent.
    private static void SetCellAnchors(RectTransform rt, int column, int row, int w, int h, int columns, int rows)
    {
        rt.anchorMin = new Vector2((float)column / columns, 1f - (float)(row + h) / rows);
        rt.anchorMax = new Vector2((float)(column + w) / columns, 1f - (float)row / rows);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
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

        slots = new InventorySlotView[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                var slot = Instantiate(slotPrefab, grid);
                slot.Initialize(row, column);
                SetCellAnchors((RectTransform)slot.transform, column, row, 1, 1, columns, rows);

                slot.OnSlotClicked += HandleSlotClicked;
                slot.OnSlotRightClicked += HandleSlotRightClicked;
                slot.OnSlotHovered += HandleSlotHovered;
                slot.OnSlotUnhovered += HandleSlotUnhovered;

                slots[row, column] = slot;
            }
        }

        lastBoxSize = new Vector2(-1f, -1f); // force a fit on the next LateUpdate
    }

    private void HandleSlotClicked(InventorySlotView slot, PointerEventData eventData)
    {
        _items.RequestInventorySlotClick(slot.Row, slot.Column);
    }

    private void HandleSlotRightClicked(InventorySlotView slot, PointerEventData eventData)
    {
        _items.RequestInventorySlotRightClick(slot.Row, slot.Column);
    }

    private void HandleSlotHovered(InventorySlotView slot)
    {
        _items.Server_RequestInventoryTooltip(slot.Row, slot.Column);
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

        int rows = _items.InventoryRows;
        int columns = _items.InventoryColumns;

        foreach (var entry in _items.InventoryItems)
        {
            if (!ItemRegistry.Instance.TryGetIcon(entry.ItemId, out Sprite icon))
                continue;

            ItemIconView iconView = Instantiate(itemIconPrefab, grid);
            spawnedIcons.Add(iconView);

            SetCellAnchors(
                (RectTransform)iconView.transform,
                entry.Position.x, entry.Position.y,
                entry.Size.x, entry.Size.y,
                columns, rows);

            iconView.GetComponentInChildren<Image>().sprite = icon;
        }
    }

    public void SetVisibility(bool visible)
    {
        panelRoot.SetActive(visible);

        if (!visible)
            TooltipController.Instance.Hide();
    }
}