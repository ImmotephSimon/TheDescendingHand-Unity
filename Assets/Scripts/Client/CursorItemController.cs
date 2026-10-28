using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorItemController : MonoBehaviour
{
    [SerializeField] private CursorItem prefab;
    [SerializeField] private RectTransform tooltipLayer;

    private PlayerItemsNetwork _items;

    public static CursorItemController Instance { get; private set; }
    public CursorItem CursorItem { get; private set; }

    private static readonly List<RaycastResult> _hits = new();

    private void Awake()
    {
        Instance = this;
        CursorItem = Instantiate(prefab, tooltipLayer);
        CursorItem.gameObject.SetActive(false);
        ClientRoot.Instance.OnClientPlayerReady += BindTo;
    }

    private void OnDestroy()
    {
        if (ClientRoot.Instance != null)
            ClientRoot.Instance.OnClientPlayerReady -= BindTo;

        Unbind();
        if (Instance == this) Instance = null;
    }

    private void BindTo(ClientPlayer player)
    {
        Unbind();
        _items = ClientRoot.Instance.ItemsSync;
        if (_items == null) return;

        _items.HeldItemChanged += Refresh;
        Refresh();
    }

    private void Unbind()
    {
        if (_items != null)
            _items.HeldItemChanged -= Refresh;
        _items = null;
    }

    private void Refresh()
    {
        var held = _items.HeldItem;
        if (held.HasItem)
            CursorItem.Hold(held.Icon, held.Size);
        else
            CursorItem.Clear();
    }
    public void ShowPredicted(SpriteString icon, Vector2Int size)
    {
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