using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class PassiveTreeView : MonoBehaviour, IDragHandler, IScrollHandler
{
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
    private readonly Dictionary<string, PassiveNodeTypeDto> _nodeTypes = new();
    private readonly Dictionary<string, PassiveNodeView> _nodeViews = new();

    private PassiveTreeDto _treeData;
    private PassiveTreeNetwork _targetComponent;
    private PassiveNodeView _selectedNodeView;
    private PassiveTooltipView _tooltipInstance;
    private bool _built;

    public PassiveNode SelectedNode =>
        _selectedNodeView != null ? _selectedNodeView.NodeData : null;

    public bool IsVisible => _window.gameObject.activeSelf;

    private void Awake()
    {
        if (ClientRoot.Instance != null)
            ClientRoot.Instance.OnClientPlayerReady += HandlePlayerReady;

        _tooltipInstance = Instantiate(_tooltipPrefab, _tooltipLayer);
        _tooltipInstance.Hide();
    }

    private void OnDestroy()
    {
        if (ClientRoot.Instance != null)
            ClientRoot.Instance.OnClientPlayerReady -= HandlePlayerReady;

        if (_targetComponent != null)
        {
            _targetComponent.OnNodeStateChanged -= HandleNodeStateChanged;
            _targetComponent.PassiveTreeDtoReceived -= HandlePassiveTreeDtoReceived;
        }
    }

    public void Initialize(PassiveTreeNetwork targetComponent)
    {
        if (_targetComponent != null)
        {
            _targetComponent.OnNodeStateChanged -= HandleNodeStateChanged;
            _targetComponent.PassiveTreeDtoReceived -= HandlePassiveTreeDtoReceived;
        }

        _targetComponent = targetComponent;

        if (_targetComponent != null)
        {
            _targetComponent.OnNodeStateChanged += HandleNodeStateChanged;
            _targetComponent.PassiveTreeDtoReceived += HandlePassiveTreeDtoReceived;

            // The DTO may have arrived before this view subscribed.
            if (_targetComponent.CurrentPassiveTreeDto != null)
                HandlePassiveTreeDtoReceived(
                    _targetComponent.CurrentPassiveTreeDto);
        }

        SetVisible(false);
    }

    private void HandlePlayerReady(ClientPlayer player)
    {
        if (!player.TryGetComponent(out PassiveTreeNetwork passiveTree))
        {
            Debug.LogError(
                "[PassiveTreeView] No passive tree component.", this);
            return;
        }

        Initialize(passiveTree);
    }

    private void HandlePassiveTreeDtoReceived(PassiveTreeDto dto)
    {
        if (dto == null)
        {
            Debug.LogError(
                "[PassiveTreeView] Received null tree data.", this);
            return;
        }

        _treeData = dto;
        Build();
    }

    private void Build()
    {
        if (_treeData == null || _built)
            return;

        _built = true;

        foreach (PassiveNodeTypeDto type in _treeData.NodeTypes)
        {
            if (type == null || string.IsNullOrEmpty(type.Name))
                continue;

            if (!_nodeTypes.TryAdd(type.Name, type))
                Debug.LogError(
                    $"Duplicate passive node type: {type.Name}", this);
        }

        foreach (PassiveNode node in _treeData.Nodes)
        {
            if (!_nodeTypes.TryGetValue(node.Type, out PassiveNodeTypeDto type))
            {
                Debug.LogError(
                    $"No node-type DTO found for '{node.Type}' " +
                    $"on node '{node.Name}'.",
                    this);
                continue;
            }

            _nodes[node.Name] = node;

            PassiveNodeView instance = Instantiate(_nodePrefab, _container);
            instance.SetNode(node, type, OnNodeClicked);

            instance.HoverEnter += nodeView =>
                _tooltipInstance.Show(
                    BuildTooltipData(nodeView.NodeData, nodeView.NodeType),
                    nodeView.transform as RectTransform);

            instance.HoverExit += _ => _tooltipInstance.Hide();

            _nodeViews[node.Name] = instance;
        }

        foreach (PassiveNodeConnection connection in _treeData.Connections)
        {
            if (_nodes.ContainsKey(connection.A) &&
                _nodes.ContainsKey(connection.B))
            {
                CreateConnection(connection);
            }
            else
            {
                Debug.LogWarning(
                    $"Skipping connection '{connection.A}' - '{connection.B}': " +
                    "one or both nodes are missing.",
                    this);
            }
        }

        RefreshAllNodeStates();
    }

    public void RequestAllocateSelected()
    {
        if (_targetComponent == null || _selectedNodeView == null)
            return;

        _targetComponent.AllocateNodeServerRpc(
            _selectedNodeView.NodeData.Name);
    }

    public void RequestRefundSelected()
    {
        if (_targetComponent == null || _selectedNodeView == null)
            return;

        _targetComponent.RefundNodeServerRpc(
            _selectedNodeView.NodeData.Name);
    }

    private void HandleNodeStateChanged(string nodeName, bool isAllocated)
    {
        if (_nodeViews.TryGetValue(nodeName, out PassiveNodeView view))
            view.SetAllocated(isAllocated);
    }

    private void RefreshAllNodeStates()
    {
        if (_targetComponent == null)
            return;

        foreach (var entry in _nodeViews)
        {
            bool isAllocated =
                _targetComponent.AllocatedNodes.Contains(entry.Key);

            entry.Value.SetAllocated(isAllocated);
        }
    }

    private void OnNodeClicked(
        PassiveNodeView nodeView,
        PointerEventData.InputButton button)
    {
        if (_selectedNodeView != null)
            _selectedNodeView.SetHighlight(false);

        _selectedNodeView = nodeView;
        _selectedNodeView.SetHighlight(true);

        RequestAllocateSelected();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position =
            _container.anchoredPosition + eventData.delta;

        _container.anchoredPosition = ClampPosition(position);
    }

    private Vector2 ClampPosition(Vector2 targetPosition)
    {
        float scale = _container.localScale.x;
        Vector2 containerSize = _container.rect.size * scale;
        Vector2 windowSize = _window.rect.size;

        float maxX = Mathf.Max(
            0f, (containerSize.x - windowSize.x) * 0.5f);

        float maxY = Mathf.Max(
            0f, (containerSize.y - windowSize.y) * 0.5f);

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
        line.sizeDelta = new Vector2(
            direction.magnitude, line.sizeDelta.y);

        line.localRotation = Quaternion.Euler(
            0f, 0f,
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    public void OnScroll(PointerEventData eventData)
    {
        float zoom = Mathf.Clamp(
            _container.localScale.x +
            eventData.scrollDelta.y * _zoomSpeed,
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

    private PassiveTooltipData BuildTooltipData(
        PassiveNode node, PassiveNodeTypeDto type)
    {
        var cost = new StringBuilder();

        if (node.Cost.Count > 0)
        {
            cost.AppendLine("Cost:");

            foreach (var entry in node.Cost)
                cost.AppendLine($"  {entry.Amount} {entry.Attribute}");
        }

        var requirement = new StringBuilder();

        if (node.Requirement.Count > 0)
        {
            requirement.AppendLine("Requires:");

            foreach (var entry in node.Requirement)
                requirement.AppendLine(
                    $"  {entry.Amount} {entry.Attribute}");
        }

        return new PassiveTooltipData
        {
            Title = node.Role == PassiveNodeRole.Keystone
                ? node.Name
                : (!string.IsNullOrWhiteSpace(type.DisplayTitle)
                    ? type.DisplayTitle
                    : type.Name),

            RoleText = node.Role == PassiveNodeRole.Keystone
                ? "Keystone"
                : "",

            DescriptionText = type.Description ?? "",
            CostText = cost.ToString().TrimEnd(),
            RequirementText = requirement.ToString().TrimEnd()
        };
    }
}