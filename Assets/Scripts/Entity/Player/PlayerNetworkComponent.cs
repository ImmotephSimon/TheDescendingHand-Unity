using FishNet;
using FishNet.Object;
using UnityEngine;

public class PlayerNetworkComponent : NetworkBehaviour
{
    private Player player;

    public override void OnStartServer()
    {
        base.OnStartServer();

        player = GetComponent<Player>();

        if (GameWorld.Instance == null)
            throw new System.NullReferenceException("GameWorld missing.");

        GameWorld.Instance.RegisterEntity(player);

        if (player.CardRegistry == null)
        {
            throw new System.NullReferenceException(
                $"[{name}] Card Registry asset is missing.");
        }

        var cardController = GetComponentInChildren<CardController>();
        if (cardController == null)
        {
            throw new System.NullReferenceException(
                $"[{name}] Missing CardController.");
        }

        CardFactory factory = CreateCardFactory(player.CardRegistry);

        player.InitializeServer(factory);

        cardController.SetCardProvider(player.CardProvider);

    }
    
    private CardFactory CreateCardFactory(CardRegistry registry)
    {
        return new CardFactory(registry, SpawnCard);
    }

    private void SpawnCard(GameObject go)
    {
        ServerManager.Spawn(go);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner) ClientBridge.Instance.RegisterPlayer(GetComponent<Player>());
    }

}