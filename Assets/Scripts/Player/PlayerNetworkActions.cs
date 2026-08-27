using FishNet.Connection;
using FishNet.Object;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerNetworkActions : NetworkBehaviour
{
    private const int MaxSpawnedObjects = 50;
    private readonly Queue<NetworkObject> _spawnedObjects = new();

    private readonly Dictionary<Guid, GameObject> _activeClientVfx = new();

    private void SpawnNetworkObjectCapped(GameObject go)
    {
        ServerManager.Spawn(go);

        if (!go.TryGetComponent(out NetworkObject nob))
            return;

        _spawnedObjects.Enqueue(nob);

        while (_spawnedObjects.Count > MaxSpawnedObjects)
        {
            var oldest = _spawnedObjects.Dequeue();

            if (oldest != null && oldest.IsSpawned)
            { 
                ServerManager.Despawn(oldest);
            }
        }
    }

    private void SpawnClientVfx(CardDefinition cardDefinition, VfxSpawnParams vfxSpawnParams)
    {
        SpawnClientVfxObserversRpc(
            cardDefinition.Id,
            vfxSpawnParams);
    }

    [ObserversRpc]
    private void SpawnClientVfxObserversRpc(string vfxId, VfxSpawnParams vfxParams)
    {
        if (!ClientBridge.Instance.CardRegistry.TryGet(vfxId, out CardDefinition def))
            return;

        if (def.Visuals.Impact == null)
            return;

        GameObject instance = Instantiate(def.Visuals.Impact, vfxParams.Position, vfxParams.Rotation);
        instance.transform.localScale = vfxParams.Scale;
        var vfx = instance.GetComponentInChildren<IVfx>();
        vfx?.Initialize(vfxParams);
        _activeClientVfx[vfxParams.InstanceId] = instance;
    }

    public void StopClientVfx(Guid instanceId)
    {
        StopClientVfxObserversRpc(instanceId);
    }

    [ObserversRpc]
    private void StopClientVfxObserversRpc(Guid instanceId)
    {
        if (!_activeClientVfx.Remove(instanceId, out GameObject instance))
            return;

        var vfx = instance.GetComponentInChildren<IVfx>();
        vfx?.Stop();
    }


    [Server]
    public CardFactory CreateCardFactory(CardRegistry registry)
    {
        return new CardFactory(
            registry,
            SpawnNetworkObjectCapped,
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