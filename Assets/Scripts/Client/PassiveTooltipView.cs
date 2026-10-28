using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PassiveTooltipView : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;

    [Header("Text Fields")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _roleText;
    [SerializeField] private TMP_Text _modifiersText;
    [SerializeField] private TMP_Text _costText;
    [SerializeField] private TMP_Text _requirementText;

    private RectTransform _layerBounds;
    private Canvas _rootCanvas;

    public RectTransform RectTransform => _rectTransform;

    private void Awake()
    {
        _layerBounds = transform.parent as RectTransform;
        _rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
    }

    public void Show(PassiveTooltipData data, RectTransform anchor)
    {
        gameObject.SetActive(true);

        _titleText.text = data.Title;
        _roleText.text = data.RoleText;
        _modifiersText.text = data.DescriptionText;
        _costText.text = data.CostText;
        _requirementText.text = data.RequirementText;

        // Force layout to recalculate before we read _rectTransform.rect.size in PositionNear.
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);

        PositionNear(anchor);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private static void SetRow(TMP_Text field, string value)
    {
        if (field == null) return;

        bool hasContent = !string.IsNullOrEmpty(value);
        field.gameObject.SetActive(hasContent);

        if (hasContent)
        {
            field.text = value;
        }
    }

    private void PositionNear(RectTransform anchor)
    {
        Camera uiCamera = _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : _rootCanvas.worldCamera;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, anchor.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_layerBounds, screenPoint, uiCamera, out Vector2 localPoint);

        localPoint += new Vector2(20f, -20f);

        Vector2 layerSize = _layerBounds.rect.size;
        Vector2 tooltipSize = _rectTransform.rect.size;

        float clampedX = Mathf.Clamp(localPoint.x, -layerSize.x / 2f, layerSize.x / 2f - tooltipSize.x);
        float clampedY = Mathf.Clamp(localPoint.y, -layerSize.y / 2f + tooltipSize.y, layerSize.y / 2f);

        _rectTransform.anchoredPosition = new Vector2(clampedX, clampedY);
    }
}

public struct PassiveTooltipData
{
    public string Title;
    public string RoleText;
    public string DescriptionText;
    public string CostText;
    public string RequirementText;
}