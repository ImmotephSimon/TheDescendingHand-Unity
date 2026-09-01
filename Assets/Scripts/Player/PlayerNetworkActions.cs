using FishNet.Connection;
using FishNet.Object;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetworkActions : NetworkActionBase
{

    [ServerRpc]
    public void RequestDrop(string itemId)
    {
        if (!Guid.TryParse(itemId, out Guid id))
            return;

        var player = GetComponent<Player>();
        var inventory = player.GetComponent<IInventory>();

        if (!inventory.TryGet(id, out IInventoryItem inventoryItem) || inventoryItem is not ItemInstance item)
            return;

        if (!inventory.TryRemove(item))
            return;

        GameObject obj = Instantiate(ItemRegistry.Instance.ItemPrefab, player.transform.position, Quaternion.identity);
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
        interactable?.Interact(player);
    }

    internal void ShowSpeechBubble(string text, float duration = -1f)
    {
        ShowSpeechBubbleTargetRpc(Owner, text, duration);
    }

    [TargetRpc]
    private void ShowSpeechBubbleTargetRpc(NetworkConnection connection, string text, float duration)
    {
        ClientBridge.Instance.PlayerHUD.HUD.ShowSpeechBubble(text, duration);
    }
}