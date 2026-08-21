using FishNet.Object;
using System;
using UnityEngine;

public class PlayerNetworkActions : NetworkBehaviour
{
    private void SpawnNetworkObject(GameObject go)
    {
        ServerManager.Spawn(go);
    }

    private void SpawnClientVfx(
        CardDefinition cardDefinition,
        Vector3 position,
        Quaternion rotation)
    {
        SpawnClientVfxObserversRpc(
            cardDefinition.Id,
            position,
            rotation);
    }

    [ObserversRpc]
    private void SpawnClientVfxObserversRpc(
        string vfxId,
        Vector3 position,
        Quaternion rotation)
    {
        if (ClientBridge.Instance.CardRegistry.TryGet(vfxId, out CardDefinition def))
            Instantiate(def.Visuals.Impact, position, rotation);
    }

    [Server]
    public CardFactory CreateCardFactory(CardRegistry registry)
    {
        return new CardFactory(
            registry,
            SpawnNetworkObject,
            SpawnClientVfx);
    }

    [ServerRpc]
    public void RequestDrop(string itemId)
    {
        if (!Guid.TryParse(itemId, out Guid id))
            return;

        var player = GetComponent<Player>();
        var inventory = player.GetComponent<IInventory>();

        if (!inventory.TryGet(id, out IInventoryItem inventoryItem))
            return;

        if (inventoryItem is not ItemInstance item)
        {
            Debug.LogWarning("Dropping non-equipment not supported.");
            return;
        }

        if (!inventory.TryRemove(item))
            return;

        GameObject obj = Instantiate(
            ItemDatabase.Instance.ItemPrefab,
            player.transform.position,
            Quaternion.identity);

        ItemDrop drop = obj.GetComponent<ItemDrop>();
        drop.Initialize(item.BaseType, item.Rarity);
        drop.Instance = item;

        ServerManager.Spawn(obj);
    }

    [ServerRpc]
    public void RequestInteract()
    {
        var player = GetComponent<Player>();
        var interactable = player.FindNearbyInteractable();

        if (interactable == null)
            return;

        interactable.Interact(player);
    }
}