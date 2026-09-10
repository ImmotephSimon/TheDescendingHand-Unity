using TMPro;
using UnityEngine;

public class PassiveTooltipView : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _bodyText;

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
        _titleText.text = data.Title;
        _bodyText.text = data.Body;
        gameObject.SetActive(true);
        PositionNear(anchor);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void PositionNear(RectTransform anchor)
    {
        Camera uiCamera = _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : _rootCanvas.worldCamera;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, anchor.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_layerBounds, screenPoint, uiCamera, out Vector2 localPoint);

        localPoint += new Vector2(20f,-20f);

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
    public string Body;
}