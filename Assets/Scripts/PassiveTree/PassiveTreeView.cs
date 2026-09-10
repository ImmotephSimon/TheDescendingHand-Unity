using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class PassiveTreeView : MonoBehaviour, IDragHandler, IScrollHandler
{
    [SerializeField] private PassiveTreeData _data;
    [SerializeField] private RectTransform _container;
    [SerializeField] private PassiveNodeView _nodePrefab;
    [SerializeField] private RectTransform _connectionPrefab;
    [SerializeField] private RectTransform _window;
    [SerializeField] private PassiveTooltipView _tooltipPrefab;
    [SerializeField] private RectTransform _tooltipLayer;

    [SerializeField] private float _minZoom = 0.5f;
    [SerializeField] private float _maxZoom = 2f;
    [SerializeField] private float _zoomSpeed = 0.1f;

    private readonly Dictionary<string, PassiveNode> _nodes = new();
    private readonly Dictionary<string, PassiveNodeType> _nodeTypes = new();
    private readonly Dictionary<string, PassiveNodeView> _nodeViews = new();

    private PassiveTreeComponent _targetComponent;
    private PassiveNodeView _selectedNodeView;
    private PassiveTooltipView _tooltipInstance;

    public PassiveTreeData Data => _data;
    public PassiveNode SelectedNode => _selectedNodeView != null ? _selectedNodeView.NodeData : null;
    public bool IsVisible => _window.gameObject.activeSelf;

    private void Awake()
    {
        ClientBridge.Instance.OnClientPlayerReady += HandlePlayerReady;

        _tooltipInstance = Instantiate(_tooltipPrefab, _tooltipLayer);
        _tooltipInstance.Hide();
    }

    private void Start()
    {
        Build();
    }

    public void Initialize(PassiveTreeComponent targetComponent)
    {
        if (_targetComponent != null)
            _targetComponent.OnNodeStateChanged -= HandleNodeStateChanged;

        _targetComponent = targetComponent;

        if (_targetComponent != null)
        {
            _targetComponent.OnNodeStateChanged += HandleNodeStateChanged;
            RefreshAllNodeStates();
        }
        SetVisible(false);
    }

    private void HandlePlayerReady(ClientPlayer player)
    {
        if (!player.TryGetComponent(out PassiveTreeComponent passiveTree))
        {
            Debug.LogError("[PassiveTreeView] No passive tree component.");
        }
        Initialize(passiveTree);
    }

    public void Build()
    {
        _nodeTypes.Clear();
        _nodes.Clear();
        _nodeViews.Clear();

        foreach (PassiveNodeType type in _data.NodeTypes)
            _nodeTypes.Add(type.Name, type);

        foreach (PassiveNode node in _data.Nodes)
        {
            _nodes.Add(node.Name, node);
            PassiveNodeView instance = Instantiate(_nodePrefab, _container);
            instance.SetNode(node, _nodeTypes[node.Type], OnNodeSelected);

            instance.HoverEnter += nodeView => _tooltipInstance.Show(
                BuildTooltipData(nodeView.NodeData, nodeView.NodeType),
                nodeView.transform as RectTransform);

            instance.HoverExit += _ => _tooltipInstance.Hide();

            _nodeViews.Add(node.Name, instance);
        }

        foreach (PassiveNodeConnection connection in _data.Connections)
            CreateConnection(connection);
    }

    // Call from UI Button: "Allocate"
    public void RequestAllocateSelected()
    {
        if (_targetComponent == null || _selectedNodeView == null) return;
        _targetComponent.AllocateNodeServerRpc(_selectedNodeView.NodeData.Name);
    }

    // Call from UI Button: "Refund"
    public void RequestRefundSelected()
    {
        if (_targetComponent == null || _selectedNodeView == null) return;
        _targetComponent.RefundNodeServerRpc(_selectedNodeView.NodeData.Name);
    }

    private void HandleNodeStateChanged(string nodeName, bool isAllocated)
    {
        if (_nodeViews.TryGetValue(nodeName, out PassiveNodeView view))
        {
            view.SetAllocated(isAllocated);
        }
    }

    private void RefreshAllNodeStates()
    {
        if (_targetComponent == null) return;

        foreach (var kvp in _nodeViews)
        {
            bool isAllocated = _targetComponent.AllocatedNodes.Contains(kvp.Key);
            kvp.Value.SetAllocated(isAllocated);
        }
    }

    private void OnNodeSelected(PassiveNodeView nodeView)
    {
        if (_selectedNodeView != null)
            _selectedNodeView.SetHighlight(false);

        _selectedNodeView = nodeView;
        _selectedNodeView.SetHighlight(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position = _container.anchoredPosition + eventData.delta;
        _container.anchoredPosition = ClampPosition(position);
    }

    private Vector2 ClampPosition(Vector2 targetPosition)
    {
        float currentScale = _container.localScale.x;
        Vector2 containerSize = _container.rect.size * currentScale;
        Vector2 windowSize = _window.rect.size;

        float maxX = Mathf.Max(0f, (containerSize.x - windowSize.x) * 0.5f);
        float maxY = Mathf.Max(0f, (containerSize.y - windowSize.y) * 0.5f);

        return new Vector2(
            Mathf.Clamp(targetPosition.x, -maxX, maxX),
            Mathf.Clamp(targetPosition.y, -maxY, maxY));
    }

    private void CreateConnection(PassiveNodeConnection connection)
    {
        PassiveNode a = _nodes[connection.A];
        PassiveNode b = _nodes[connection.B];

        RectTransform line = Instantiate(_connectionPrefab, _container);
        Vector2 start = a.Position;
        Vector2 end = b.Position;
        Vector2 direction = end - start;

        line.anchoredPosition = (start + end) * 0.5f;
        line.sizeDelta = new Vector2(direction.magnitude, line.sizeDelta.y);
        line.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    public void OnScroll(PointerEventData eventData)
    {
        float zoom = Mathf.Clamp(
            _container.localScale.x + eventData.scrollDelta.y * _zoomSpeed,
            _minZoom,
            _maxZoom);

        _container.localScale = Vector3.one * zoom;
    }

    public void SetVisible(bool visible)
    {
        _window.gameObject.SetActive(visible);

        if (visible)
            RefreshAllNodeStates();
    }

    private PassiveTooltipData BuildTooltipData(PassiveNode node, PassiveNodeType type)
    {
        var body = new StringBuilder();

        if (node.Role != PassiveNodeRole.Unset)
            body.AppendLine($"<{node.Role}>");

        if (type.Modifiers.Count > 0)
        {
            foreach (var mod in type.Modifiers)
                body.AppendLine(mod.ToString());
        }

        if (node.Cost.Count > 0)
        {
            body.AppendLine("Cost:");
            foreach (var c in node.Cost)
                body.AppendLine($"  {c.Amount} {c.Attribute}");
        }

        if (node.Requirement.Count > 0)
        {
            body.AppendLine("Requires:");
            foreach (var r in node.Requirement)
                body.AppendLine($"  {r.Amount} {r.Attribute}");
        }

        return new PassiveTooltipData
        {
            Title = node.Name,
            Body = body.ToString().TrimEnd()
        };
    }
}