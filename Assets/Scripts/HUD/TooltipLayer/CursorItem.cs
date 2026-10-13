using UnityEngine;
using UnityEngine.InputSystem;

public class CursorItem : MonoBehaviour
{
    [SerializeField] private ItemIconView icon;

    private RectTransform rectTransform;
    private RectTransform parentRect;

    public bool HasItem { get; private set; }

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



    public void Hold(Sprite iconSprite, Vector2Int size)   // adjust type to whatever icon.Render takes
    {
        HasItem = true;
        gameObject.SetActive(true);

        icon.Render(iconSprite, Vector2.zero, new Vector2(size.x * 64f, size.y * 64f));
        UpdatePosition();
    }

    public void Clear()
    {
        HasItem = false;
        gameObject.SetActive(false);
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