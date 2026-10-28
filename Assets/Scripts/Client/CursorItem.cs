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

    public void Hold(SpriteString icon, Vector2Int size)
    {
        HasItem = true;
        gameObject.SetActive(true);

        SpriteCatalog.Instance.TryGetSprite(icon, out Sprite sprite);
        this.icon.Render(sprite, Vector2.zero, new Vector2(size.x * 64f, size.y * 64f));
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