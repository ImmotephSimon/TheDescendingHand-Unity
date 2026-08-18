using UnityEngine;

public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance { get; private set; }

    [SerializeField] private ItemTooltipView itemTooltipPrefab;
    [SerializeField] private CardView cardTooltipPrefab;
    [SerializeField] private RectTransform tooltipLayer;
    [SerializeField] private RectTransform fixedTooltipAnchor;

    private ItemTooltipView itemTooltip;
    private CardView cardTooltip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        itemTooltip = Instantiate(itemTooltipPrefab, tooltipLayer);
        itemTooltip.gameObject.SetActive(false);

        cardTooltip = Instantiate(cardTooltipPrefab, tooltipLayer);
        cardTooltip.gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        Debug.Assert(itemTooltipPrefab != null, $"{nameof(itemTooltipPrefab)} is not assigned.", this);
        Debug.Assert(cardTooltipPrefab != null, $"{nameof(cardTooltipPrefab)} is not assigned.", this);
        Debug.Assert(tooltipLayer != null, $"{nameof(tooltipLayer)} is not assigned.", this);
        Debug.Assert(fixedTooltipAnchor != null, $"{nameof(fixedTooltipAnchor)} is not assigned.", this);
    }
#endif

    public void Show(IInventoryItem item)
    {
        switch (item)
        {
            case ItemInstance itemInstance:
                ShowItem(itemInstance);
                break;

            case CardInstance cardInstance:
                ShowCard(cardInstance);
                break;
        }
    }

    public void ShowItem(ItemInstance item)
    {
        Hide();

        itemTooltip.SetItem(item);
        Show(itemTooltip.GetComponent<RectTransform>());
    }

    public void ShowCard(CardInstance card)
    {
        Hide();

        cardTooltip.InitializeTooltipCard(card.Definition);
        Show(cardTooltip.GetComponent<RectTransform>());
    }

    private void Show(RectTransform rect)
    {
        rect.position = fixedTooltipAnchor.position;
        rect.gameObject.SetActive(true);
    }

    public void Hide()
    {
        itemTooltip.gameObject.SetActive(false);
        cardTooltip.gameObject.SetActive(false);
    }
}