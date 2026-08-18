using FishNet;
using FishNet.Object;
using System;
using UnityEditor;
using UnityEngine;

public class PlayerNetworkComponent : NetworkBehaviour
{
    [SerializeField] private CardRegistry cardRegistry;

    private Player player;

    protected override void OnValidate()
    {
        Debug.Assert(cardRegistry != null, $"[{name}] Card Registry field is unassigned in the inspector.");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        player = GetComponent<Player>();

        if (GameWorld.Instance == null)
            throw new System.NullReferenceException("GameWorld missing.");

        GameWorld.Instance.RegisterEntity(player);

        var cardController = GetComponentInChildren<CardController>();
        if (cardController == null)
            throw new System.NullReferenceException(
                $"[{name}] Missing CardController.");

        CardFactory cardFactory = CreateCardFactory(cardRegistry);
        cardController.InitializeServer(player, cardFactory, cardRegistry);
    }
    
    private CardFactory CreateCardFactory(CardRegistry registry)
    {
        return new CardFactory(registry, SpawnNetworkObject, SpawnClientVfx);
    }

    private void SpawnNetworkObject(GameObject go)
    {
        ServerManager.Spawn(go);
    }
    private void SpawnClientVfx(CardDefinition cardDefinition, Vector3 position, Quaternion rotation)
    {
        SpawnClientVfxObserversRpc(cardDefinition.Id, position, rotation);
    }

    [ObserversRpc]
    private void SpawnClientVfxObserversRpc(
        string vfxId,
        Vector3 position,
        Quaternion rotation)
    {
        if (ClientBridge.Instance.CardRegistry.TryGet(vfxId, out CardDefinition def))
        {
            Instantiate(def.Visuals.Impact, position, rotation);
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        player = GetComponent<Player>();
        var animHandler = GetComponentInChildren<IAnimationHandler>();
        var cardController = GetComponentInChildren<CardController>();
        cardController.InitializeClientObservers(animHandler);
        if (IsOwner)
        {
            ClientBridge.Instance.RegisterLocalPlayer(player);
        }
        
    }

    [ServerRpc]
    internal void RequestDrop(string itemId)
    {
        if (!Guid.TryParse(itemId, out Guid id))
            return;

        IInventory inventory = player.GetComponent<IInventory>();

        if (!inventory.TryGet(id, out IInventoryItem inventoryItem))
            return;

        if (inventoryItem is not ItemInstance item)
        {
            Debug.LogWarning($"Dropping non-equipment not supported.");
            return;
        }

        if (!inventory.TryRemove(item))
            return;

        

        GameObject obj = Instantiate(ItemDatabase.Instance.ItemPrefab, player.transform.position, Quaternion.identity);

        ItemDrop drop = obj.GetComponent<ItemDrop>();
        drop.Initialize(item.BaseType, item.Rarity);
        drop.Instance = item;
        ServerManager.Spawn(obj);
    }
}