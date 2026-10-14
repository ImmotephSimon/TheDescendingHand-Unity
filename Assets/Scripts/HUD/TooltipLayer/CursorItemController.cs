using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorItemController : MonoBehaviour
{
    [SerializeField] private CursorItem prefab;
    [SerializeField] private RectTransform tooltipLayer;
    private PlayerItemsSync _items;

    public static CursorItemController Instance { get; private set; }

    public CursorItem CursorItem { get; private set; }

    private static readonly List<RaycastResult> _hits = new();

    private void Awake()
    {
        Instance = this;
        ClientBridge.Instance.OnClientPlayerReady += BindTo;
        CursorItem = Instantiate(prefab, tooltipLayer);
        CursorItem.gameObject.SetActive(false);
    }


    private void BindTo(ClientPlayer player)
    {
        _items = ClientBridge.Instance.ItemsSync;
        _items.HeldItemChanged += Refresh;
    }

    private void Refresh()
    {
        var held = _items.HeldItem;
        if (held.HasItem && ItemRegistry.Instance.TryGetIcon(held.ItemId, out Sprite icon))
            CursorItem.Hold(icon, held.Size);
        else
            CursorItem.Clear();
    }

    private void OnDestroy()
    {
        if (ClientBridge.Instance != null)
            ClientBridge.Instance.OnClientPlayerReady -= BindTo;
    }

    public void ShowPredicted(Guid iconId, Vector2Int size)
    {
        if (ItemRegistry.Instance.TryGetIcon(iconId, out Sprite icon))
            CursorItem.Hold(icon, size);
    }

    public void TryDropHeldItem()
    {
        if (_items == null || CursorItem == null || !CursorItem.HasItem)
            return;

        if (!IsPointerOverUI())
            _items.RequestDropHeld();
    }

    private static bool IsPointerOverUI()
    {
        var data = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };
        _hits.Clear();
        EventSystem.current.RaycastAll(data, _hits);

        foreach (var hit in _hits)
            if (hit.module is GraphicRaycaster)
                return true;
        return false;
    }
}