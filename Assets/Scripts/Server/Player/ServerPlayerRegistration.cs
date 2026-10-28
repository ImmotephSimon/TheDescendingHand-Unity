using FishNet.Object;

public class ServerPlayerRegistration : NetworkBehaviour
{

    private Player player;

    public override void OnStartServer()
    {
        base.OnStartServer();

        player = GetComponent<Player>();

        if (GameWorld.Instance == null)
            throw new System.NullReferenceException("GameWorld missing.");

        GameWorld.Instance.RegisterEntity(player);

        var cardController = GetComponent<CardController>();
        if (cardController == null)
            throw new System.NullReferenceException(
                $"[{name}] Missing CardController.");

        var networkActions = GetComponent<PlayerNetworkActions>();

        cardController.InitializeServer(player);
    }

}