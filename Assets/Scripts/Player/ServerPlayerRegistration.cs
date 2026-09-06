using FishNet;
using FishNet.Object;
using System.Xml.Linq;
using UnityEngine;

public class ServerPlayerRegistration : NetworkBehaviour
{
    [SerializeField] private CardRegistry cardRegistry;

    public CardRegistry CardRegistry => cardRegistry.Cards.Count > 0 ? cardRegistry : CardRegistry.Instance;

    private Player player;

    public override void OnStartServer()
    {
        base.OnStartServer();

        var networkManager = InstanceFinder.NetworkManager;

        player = GetComponent<Player>();

        if (GameWorld.Instance == null)
            throw new System.NullReferenceException("GameWorld missing.");

        GameWorld.Instance.RegisterEntity(player);

        var cardController = GetComponent<CardController>();
        if (cardController == null)
            throw new System.NullReferenceException(
                $"[{name}] Missing CardController.");

        var networkActions = GetComponent<PlayerNetworkActions>();

        cardController.InitializeServer(
            player,
            cardRegistry);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        ClientBridge.Instance.SetCardRegistry(CardRegistry);
    }
}