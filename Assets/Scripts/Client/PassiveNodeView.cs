
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PassiveNodeView : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _image;
    [SerializeField] private GameObject _selectionHighlight;
    [SerializeField] private GameObject _allocatedHighlight;

    [Header("Visual States")]
    [SerializeField] private Color _allocatedColor = Color.white;
    [SerializeField]
    private Color _unallocatedColor =
        new Color(0.3f, 0.3f, 0.3f, 1f);

    public PassiveNode NodeData { get; private set; }
    public PassiveNodeTypeDto NodeType { get; private set; }
    public bool IsAllocated { get; private set; }

    public event Action<PassiveNodeView> HoverEnter;
    public event Action<PassiveNodeView> HoverExit;

    private Action<PassiveNodeView, PointerEventData.InputButton> _onClicked;

    public void SetNode(
        PassiveNode node,
        PassiveNodeTypeDto type,
        Action<PassiveNodeView, PointerEventData.InputButton> onClick)
    {
        NodeData = node;
        NodeType = type;
        _onClicked = onClick;
        SpriteCatalog.Instance.TryGetSprite(type.Sprite, out Sprite sprite);
        _image.sprite = sprite;

        ((RectTransform)transform).anchoredPosition = node.Position;

        SetHighlight(false);
        SetAllocated(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _onClicked?.Invoke(this, eventData.button);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        HoverEnter?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HoverExit?.Invoke(this);
    }

    public void SetHighlight(bool isSelected)
    {
        if (_selectionHighlight != null)
            _selectionHighlight.SetActive(isSelected);
    }

    public void SetAllocated(bool isAllocated)
    {
        IsAllocated = isAllocated;

        if (_allocatedHighlight != null)
            _allocatedHighlight.SetActive(isAllocated);

        if (_image != null)
            _image.color = isAllocated
                ? _allocatedColor
                : _unallocatedColor;
    }
}
