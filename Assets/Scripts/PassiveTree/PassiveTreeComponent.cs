using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PassiveTreeComponent : NetworkBehaviour
{
    [SerializeField] private PassiveTreeData _data;
    private IStatContainer _stats;

    private readonly SyncHashSet<string> _allocatedNodes = new();
    private readonly Dictionary<string, List<ModifierHandle>> _nodeModifiers = new();

    // Dynamically tracks spent stat points per attribute string ("Strength", "Dexterity", etc.)
    private readonly Dictionary<GameTag, int> _allocatedStats = new();

    public IReadOnlyCollection<string> AllocatedNodes => _allocatedNodes.Collection;
    public event Action<string, bool> OnNodeStateChanged;

    private void Awake()
    {
        _allocatedNodes.OnChange += OnAllocatedNodesChanged;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _stats = GetComponent<IStatContainer>();
        Debug.Assert(_stats != null, "Invalid stats container.");
    }

    private void OnAllocatedNodesChanged(SyncHashSetOperation op, string item, bool asServer)
    {
        if (op == SyncHashSetOperation.Add)
            OnNodeStateChanged?.Invoke(item, true);
        else if (op == SyncHashSetOperation.Remove)
            OnNodeStateChanged?.Invoke(item, false);
    }

    [ServerRpc]
    public void AllocateNodeServerRpc(string nodeName) => TryAllocateNode(nodeName);

    [ServerRpc]
    public void RefundNodeServerRpc(string nodeName) => TryRefundNode(nodeName);

    private bool TryAllocateNode(string nodeName)
    {
        if (_allocatedNodes.Contains(nodeName)) return false;

        PassiveNode node = FindNode(nodeName);
        if (node == null || !CanAllocate(node)) return false;

        if (!HasSufficientStats(node)) return false;

        DeductStats(node);
        _allocatedNodes.Add(node.Name);
        ApplyModifiers(node);
        return true;
    }

    private bool TryRefundNode(string nodeName)
    {
        if (!_allocatedNodes.Contains(nodeName)) return false;

        PassiveNode node = FindNode(nodeName);
        if (node == null || node.Role == PassiveNodeRole.Starter) return false;

        _allocatedNodes.Remove(node.Name);
        RemoveModifiers(node);
        RestoreStats(node);
        return true;
    }

    private bool HasSufficientStats(PassiveNode node)
    {
        // 1. Minimum Threshold Check (Requirement)
        foreach (PassiveNodeCost req in node.Requirement)
        {
            if (_stats.GetStat(req.Attribute) < req.Amount)
                return false;
        }

        // 2. Unspent Stat Pool Check (Cost)
        foreach (PassiveNodeCost cost in node.Cost)
        {
            int allocated = GetAllocatedStat(cost.Attribute);
            float totalStat = _stats.GetStat(cost.Attribute);

            if (totalStat - allocated < cost.Amount)
                return false;
        }

        return true;
    }

    private void DeductStats(PassiveNode node)
    {
        foreach (PassiveNodeCost cost in node.Cost)
            _allocatedStats[cost.Attribute] = GetAllocatedStat(cost.Attribute) + cost.Amount;
    }

    private void RestoreStats(PassiveNode node)
    {
        foreach (PassiveNodeCost cost in node.Cost)
            _allocatedStats[cost.Attribute] = GetAllocatedStat(cost.Attribute) - cost.Amount;
    }

    private int GetAllocatedStat(GameTag attribute) =>
        _allocatedStats.TryGetValue(attribute, out int val) ? val : 0;

    private bool CanAllocate(PassiveNode node)
    {
        if (node.Role == PassiveNodeRole.Starter) return true;

        foreach (PassiveNodeConnection connection in _data.Connections)
        {
            bool connected =
                (connection.A == node.Name && _allocatedNodes.Contains(connection.B)) ||
                (connection.B == node.Name && _allocatedNodes.Contains(connection.A));

            if (connected) return true;
        }

        return false;
    }

    private void ApplyModifiers(PassiveNode node)
    {
        PassiveNodeType type = FindType(node.Type);
        if (type == null) return;

        List<ModifierHandle> handles = new();
        foreach (StatModifier passiveModifier in type.Modifiers)
        {
            ModifierHandle handle = _stats.AddModifier(passiveModifier);
            handles.Add(handle);
        }

        _nodeModifiers[node.Name] = handles;
    }

    private void RemoveModifiers(PassiveNode node)
    {
        if (!_nodeModifiers.Remove(node.Name, out List<ModifierHandle> handles)) return;

        foreach (ModifierHandle handle in handles)
            _stats.RemoveModifier(handle);
    }

    private PassiveNode FindNode(string nodeName)
    {
        foreach (PassiveNode node in _data.Nodes)
            if (node.Name == nodeName) return node;
        return null;
    }

    private PassiveNodeType FindType(string typeName)
    {
        foreach (PassiveNodeType type in _data.NodeTypes)
            if (type.Name == typeName) return type;
        return null;
    }
}