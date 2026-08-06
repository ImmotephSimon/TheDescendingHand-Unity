using UnityEngine;

public class CursorItemController : MonoBehaviour
{
    [SerializeField] private CursorItem prefab;
    [SerializeField] private RectTransform tooltipLayer;
    public static CursorItemController Instance { get; private set; }

    public CursorItem CursorItem { get; private set; }

    private void Awake()
    {
        Instance = this;

        CursorItem = Instantiate(prefab, tooltipLayer);
        CursorItem.gameObject.SetActive(false);
    }
}