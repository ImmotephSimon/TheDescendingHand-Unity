using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;

public class PassiveTreeNetwork : NetworkBehaviour
{
    private readonly SyncHashSet<string> _allocatedNodes = new();

    private IPassiveTreeAllocationService _allocationService;

    public IReadOnlyCollection<string> AllocatedNodes =>
        _allocatedNodes.Collection;

    public PassiveTreeDto CurrentPassiveTreeDto { get; private set; }

    public event Action<PassiveTreeDto> PassiveTreeDtoReceived;

    public event Action<string, bool> OnNodeStateChanged;

    private void Awake()
    {
        _allocatedNodes.OnChange += OnAllocatedNodesChanged;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _allocationService = GetComponent<IPassiveTreeAllocationService>();

        if (_allocationService == null)
        {
            throw new InvalidOperationException(
                $"{nameof(IPassiveTreeAllocationService)} is missing on {gameObject.name}.");
        }
    }

    public override void OnStopServer()
    {
        _allocationService?.ShutdownServer();
        _allocationService = null;

        base.OnStopServer();
    }



    public override void OnStartClient()
    {
        base.OnStartClient();

        ClientReadyServerRpc();
    }

    [ServerRpc]
    private void ClientReadyServerRpc()
    {
        _allocationService = GetComponent<IPassiveTreeAllocationService>();
        _allocationService.InitializeServer();
    }

    private void OnDestroy()
    {
        _allocatedNodes.OnChange -= OnAllocatedNodesChanged;
    }

    private void OnAllocatedNodesChanged(
        SyncHashSetOperation op, string item, bool asServer)
    {
        if (asServer)
            return;

        if (op == SyncHashSetOperation.Add)
            OnNodeStateChanged?.Invoke(item, true);
        else if (op == SyncHashSetOperation.Remove)
            OnNodeStateChanged?.Invoke(item, false);
    }

    [ServerRpc]
    public void AllocateNodeServerRpc(string nodeName)
    {
        _allocationService?.TryAllocateNode(nodeName);
    }

    [ServerRpc]
    public void RefundNodeServerRpc(string nodeName)
    {
        _allocationService?.TryRefundNode(nodeName);
    }

    [Server]
    public bool ServerAddAllocatedNode(string nodeName)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            return false;

        return _allocatedNodes.Add(nodeName);
    }

    [Server]
    public bool ServerRemoveAllocatedNode(string nodeName)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            return false;

        return _allocatedNodes.Remove(nodeName);
    }

    [Server]
    public void SendPassiveTreeDto(PassiveTreeDto value)
    {
        TargetReceivePassiveTreeDto(Owner, value);
    }

    [TargetRpc]
    private void TargetReceivePassiveTreeDto(
        NetworkConnection connection,
        PassiveTreeDto value)
    {
        CurrentPassiveTreeDto = value;
        PassiveTreeDtoReceived?.Invoke(value);
    }
}