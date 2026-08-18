using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CursorItemController : MonoBehaviour
{
    [SerializeField] private CursorItem prefab;
    [SerializeField] private RectTransform tooltipLayer;
    public static CursorItemController Instance { get; private set; }

    public event Action<IInventoryItem> OnDropRequested;

    public CursorItem CursorItem { get; private set; }

    private void Awake()
    {
        Instance = this;

        CursorItem = Instantiate(prefab, tooltipLayer);
        CursorItem.gameObject.SetActive(false);
    }

    public void TryDropHeldItem()
    {
        if (CursorItem == null || !CursorItem.HasItem)
            return;

        if (!EventSystem.current.IsPointerOverGameObject())
            OnDropRequested?.Invoke(CursorItem.HeldItem);
    }
}