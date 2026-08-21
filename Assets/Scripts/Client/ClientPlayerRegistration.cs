using FishNet.Object;
using UnityEngine;

public class ClientPlayerRegistration : NetworkBehaviour
{
    private ClientPlayer _client;

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"CLIENT REGISTRATION: {name}, IsOwner={IsOwner}");
        _client = GetComponent<ClientPlayer>();

        var animHandler =
            GetComponentInChildren<IAnimationHandler>();

        var cardController =
            GetComponent<CardController>();

        cardController.InitializeClientObservers(animHandler);

        if (IsOwner)
        {
            ClientBridge.Instance.RegisterLocalPlayer(_client);
            cardController.NotifyClientReadyServerRpc();
        }
            
        
    }
}