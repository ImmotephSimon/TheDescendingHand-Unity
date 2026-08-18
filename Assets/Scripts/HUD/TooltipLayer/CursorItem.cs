using UnityEngine;
using UnityEngine.InputSystem;

public class CursorItem : MonoBehaviour
{
    [SerializeField] private ItemIconView icon;

    private RectTransform rectTransform;
    private RectTransform parentRect;

    public IInventoryItem HeldItem { get; private set; }
    public bool HasItem => HeldItem != null;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRect = transform.parent as RectTransform;
    }

    private void Update()
    {
        if (HasItem)
        {
            UpdatePosition();
        }
    }

    public void Hold(IInventoryItem item)
    {
        HeldItem = item;

        gameObject.SetActive(true);

        Vector2Int gridDimensions = item.Size;
        Vector2 pixelSize = new Vector2(gridDimensions.x * 64f, gridDimensions.y * 64f);

        icon.Render(
            item.Icon,
            Vector2.zero,
            pixelSize
        );

        UpdatePosition();
    }

    public IInventoryItem Clear()
    {
        IInventoryItem item = HeldItem;
        HeldItem = null;
        gameObject.SetActive(false);
        return item;
    }

    private void UpdatePosition()
    {
        if (Mouse.current == null)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            mousePos,
            null,
            out Vector2 localPoint);

        rectTransform.anchoredPosition = localPoint;
    }
}