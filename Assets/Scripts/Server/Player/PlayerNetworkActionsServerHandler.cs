using FishNet.Object;
using UnityEngine;

public class PlayerNetworkActionsServerHandler
    : NetworkBehaviour, IPlayerNetworkActionsServerHandler
{
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private float interactRange = 3f;

    public override void OnStartServer()
    {
        base.OnStartServer();
        GetComponent<PlayerInventory>().OnItemDropped += SpawnWorldDrop;
    }

    public override void OnStopServer()
    {
        GetComponent<PlayerInventory>().OnItemDropped -= SpawnWorldDrop;
        base.OnStopServer();
    }

    public void RequestInteract()
    {
        var player = GetComponent<ServerPlayer>();
        var interactable = FindNearbyInteractable();
        interactable?.Interact(player);
    }
    private void SpawnWorldDrop(IInventoryItem item)
    {
        if (!ServerPrefabRegistry.Instance.TryInstantiate(
                item.DropPrefabTag, transform.position, out var obj))
            return;

        ServerManager.Spawn(obj);

        var drop = obj.GetComponent<WorldDrop>();
        drop.Consumed += OnDropConsumed;
        drop.Restore(item);
    }

    private void OnDropConsumed(WorldDrop drop)
    {
        drop.Consumed -= OnDropConsumed;
        ServerManager.Despawn(drop.gameObject);
    }

    private IInteractable FindNearbyInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange, interactLayer);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IInteractable>(out var interactable))
                return interactable;
        }

        return null;
    }
}