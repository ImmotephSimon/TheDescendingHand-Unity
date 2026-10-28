
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PassiveTreeNetwork))]
public class PassiveTreeAllocationService : MonoBehaviour, IPassiveTreeAllocationService
{
    [SerializeField] private PassiveTreeData _data;

    private PassiveTreeNetwork _tree;
    private IStatContainer _stats;

    private readonly Dictionary<string, List<ModifierHandle>> _nodeModifiers = new();
    private readonly Dictionary<GameTag, int> _allocatedStats = new();

    public void InitializeServer()
    {
        _tree = GetComponent<PassiveTreeNetwork>();
        _stats = GetComponent<IStatContainer>();

        Debug.Assert(_tree != null, "Missing PassiveTreeComponent.");
        Debug.Assert(_stats != null, "Missing IStatContainer.");
        Debug.Assert(_data != null, "Missing PassiveTreeData.");

        _tree.SendPassiveTreeDto(BuildPassiveTreeDto());
    }

    public void ShutdownServer()
    {
        RemoveAllModifiers();
        _allocatedStats.Clear();

        _stats = null;
        _tree = null;

        // Keep _data: it's the serialized authoring asset.
    }

    public bool TryAllocateNode(string nodeName)
    {
        if (_tree == null || _stats == null || _data == null)
            return false;

        if (_tree.AllocatedNodes.Contains(nodeName))
            return false;

        PassiveNode node = FindNode(nodeName);

        if (node == null || !CanAllocate(node) || !HasSufficientStats(node))
            return false;

        if (!_tree.ServerAddAllocatedNode(node.Name))
            return false;

        DeductStats(node);
        ApplyModifiers(node);
        return true;
    }

    public bool TryRefundNode(string nodeName)
    {
        if (_tree == null || _stats == null || _data == null)
            return false;

        if (!_tree.AllocatedNodes.Contains(nodeName))
            return false;

        PassiveNode node = FindNode(nodeName);

        if (node == null || node.Role == PassiveNodeRole.Starter)
            return false;

        if (!_tree.ServerRemoveAllocatedNode(node.Name))
            return false;

        RemoveModifiers(node);
        RestoreStats(node);
        return true;
    }

    private bool HasSufficientStats(PassiveNode node)
    {
        foreach (PassiveNodeCost requirement in node.Requirement)
        {
            if (_stats.GetStat(requirement.Attribute) < requirement.Amount)
                return false;
        }

        foreach (PassiveNodeCost cost in node.Cost)
        {
            float available = _stats.GetStat(cost.Attribute)
                              - GetAllocatedStat(cost.Attribute);

            if (available < cost.Amount)
                return false;
        }

        return true;
    }

    private void DeductStats(PassiveNode node)
    {
        foreach (PassiveNodeCost cost in node.Cost)
        {
            _allocatedStats[cost.Attribute] =
                GetAllocatedStat(cost.Attribute) + cost.Amount;
        }
    }

    private void RestoreStats(PassiveNode node)
    {
        foreach (PassiveNodeCost cost in node.Cost)
        {
            _allocatedStats[cost.Attribute] =
                GetAllocatedStat(cost.Attribute) - cost.Amount;
        }
    }

    private int GetAllocatedStat(GameTag attribute)
    {
        return _allocatedStats.TryGetValue(attribute, out int value)
            ? value
            : 0;
    }

    private bool CanAllocate(PassiveNode node)
    {
        if (node.Role == PassiveNodeRole.Starter)
            return true;

        foreach (PassiveNodeConnection connection in _data.Connections)
        {
            if ((connection.A == node.Name &&
                 _tree.AllocatedNodes.Contains(connection.B)) ||
                (connection.B == node.Name &&
                 _tree.AllocatedNodes.Contains(connection.A)))
                return true;
        }

        return false;
    }

    private void ApplyModifiers(PassiveNode node)
    {
        PassiveNodeType type = FindNodeType(node.Type);

        if (type == null)
            return;

        var handles = new List<ModifierHandle>();

        foreach (StatModifier modifier in type.Modifiers)
            handles.Add(_stats.AddModifier(modifier));

        _nodeModifiers[node.Name] = handles;
    }

    private void RemoveModifiers(PassiveNode node)
    {
        if (!_nodeModifiers.Remove(node.Name, out var handles))
            return;

        foreach (ModifierHandle handle in handles)
            _stats.RemoveModifier(handle);
    }

    private void RemoveAllModifiers()
    {
        if (_stats != null)
        {
            foreach (var handles in _nodeModifiers.Values)
            {
                foreach (ModifierHandle handle in handles)
                    _stats.RemoveModifier(handle);
            }
        }

        _nodeModifiers.Clear();
    }

    private PassiveNode FindNode(string nodeName)
    {
        foreach (PassiveNode node in _data.Nodes)
        {
            if (node.Name == nodeName)
                return node;
        }

        return null;
    }

    private PassiveNodeType FindNodeType(string typeName)
    {
        foreach (PassiveNodeType type in _data.NodeTypes)
        {
            if (type.Name == typeName)
                return type;
        }

        return null;
    }

    private PassiveTreeDto BuildPassiveTreeDto()
    {
        var dto = new PassiveTreeDto();

        dto.Nodes.AddRange(_data.Nodes);
        dto.Connections.AddRange(_data.Connections);

        foreach (PassiveNodeType type in _data.NodeTypes)
        {
            string description = type.DescriptionOverride;

            if (string.IsNullOrWhiteSpace(description))
            {
                description = string.Join(
                    "\n",
                    type.Modifiers.Select(modifier => modifier.ToDescription()));
            }

            string displayTitle = type.Modifiers.Count > 0
                ? string.Join(" & ", type.Modifiers.Select(modifier => modifier.Stat))
                : type.Name;

            dto.NodeTypes.Add(new PassiveNodeTypeDto
            {
                Name = type.Name,
                DisplayTitle = displayTitle,
                Description = description
            });
        }

        return dto;
    }
}
