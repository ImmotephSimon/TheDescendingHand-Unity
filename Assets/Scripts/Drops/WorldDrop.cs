using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using static UnityEditor.Progress;

public abstract class WorldDrop : NetworkBehaviour, IInteractable
{
    private ItemDefinition _item;
    private Rarity _rarity;

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
    public virtual void Initialize(ItemDefinition item, Rarity rarity)
    {
        _item = item;
        _rarity = rarity;
    }

    protected virtual bool TryPickup(Player player)
    {
        return false;
    }
}