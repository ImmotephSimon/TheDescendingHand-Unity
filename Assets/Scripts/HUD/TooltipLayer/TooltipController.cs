using UnityEngine;
using UnityEngine.InputSystem;

public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance { get; private set; }

    [SerializeField] private ItemTooltipView tooltipPrefab;
    [SerializeField] private RectTransform tooltipLayer;
    [SerializeField] private RectTransform fixedTooltipAnchor;

    private ItemTooltipView instanceInstance;
    private RectTransform instanceRect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        instanceInstance = Instantiate(tooltipPrefab, tooltipLayer);
        instanceRect = instanceInstance.GetComponent<RectTransform>();
        instanceInstance.gameObject.SetActive(false);
    }

    public void ShowItem(ItemInstance item)
    {
        instanceInstance.SetItem(item);
        instanceRect.position = fixedTooltipAnchor.position;
        instanceInstance.gameObject.SetActive(true);
    }

    public void Hide()
    {
        instanceInstance.gameObject.SetActive(false);
    }

}