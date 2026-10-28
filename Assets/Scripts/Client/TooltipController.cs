using System.Collections.Generic;
using UnityEngine;

public class TooltipController : MonoBehaviour, ITooltipPresenter
{
    public static TooltipController Instance { get; private set; }

    [System.Serializable]
    private struct LayoutEntry
    {
        public TooltipLayout layout;
        public TooltipView prefab;
    }

    [SerializeField] private LayoutEntry[] layouts;

    private readonly Dictionary<TooltipLayout, TooltipView> _views = new();
    private TooltipView _active;

    [SerializeField] private RectTransform tooltipLayer;
    [SerializeField] private RectTransform fixedTooltipAnchor;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        TooltipPresenter.Current = this;

        foreach (var entry in layouts)
        {
            var view = Instantiate(entry.prefab, tooltipLayer);
            view.transform.localScale = Vector3.one;
            ((RectTransform)view.transform).pivot = new Vector2(1f, 1f);
            view.gameObject.SetActive(false);
            _views[entry.layout] = view;
        }
    }

    public void Show(TooltipDto dto)
    {
        Hide();

        if (!_views.TryGetValue(dto.Layout, out var view))
        {
            Debug.LogError($"No tooltip view registered for layout {dto.Layout}");
            return;
        }

        view.gameObject.SetActive(true);
        view.Set(dto);
        _active = view;

        var rect = view.GetComponent<RectTransform>();
        rect.position = fixedTooltipAnchor.position;
    }

    public void Hide()
    {
        if (_active != null)
            _active.gameObject.SetActive(false);
        _active = null;
    }
}