using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public abstract class WorldDrop : NetworkBehaviour, IInteractable
{
    public int DropLevel { get; set; } = 1;

    public void Interact(Player player)
    {
        if (IsServerStarted) ExecutePickup(player);
        else RequestPickupServerRpc();
    }

    [ServerRpc]
    private void RequestPickupServerRpc(NetworkConnection sender = null)
    {
        Player player = sender.FirstObject.GetComponent<Player>();

        if (Vector3.Distance(player.transform.position, transform.position) > player.InteractRange)
            return;

        ExecutePickup(player);
    }

    private void ExecutePickup(Player player)
    {
        if (TryPickup(player))
        {
            Despawn(gameObject);
        }
    }

    protected virtual bool TryPickup(Player player)
    {
        return false;
    }
}