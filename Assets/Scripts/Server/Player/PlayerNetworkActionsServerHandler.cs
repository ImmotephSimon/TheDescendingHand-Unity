using FishNet.Object;

public class PlayerNetworkActionsServerHandler
    : NetworkBehaviour, IPlayerNetworkActionsServerHandler
{
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
        var player = GetComponent<Player>();
        var interactable = player.FindNearbyInteractable();
        interactable?.Interact(player);
    }

    private void SpawnWorldDrop(IInventoryItem item)
    {
        if (!ServerPrefabRegistry.Instance.TryInstantiate(
                item.DropPrefabTag, transform.position, out var obj))
            return;

        ServerManager.Spawn(obj);
        obj.GetComponent<WorldDrop>().Restore(item);
    }
}