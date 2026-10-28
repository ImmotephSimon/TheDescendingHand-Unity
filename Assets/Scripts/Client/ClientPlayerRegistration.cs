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
            GetComponentInChildren<IPlayerAnimationHandler>();

        var cardNetwork =
            GetComponent<CardNetwork>();

        cardNetwork.InitializeClientObservers(animHandler);

        if (IsOwner)
        {
            ClientRoot.Instance.RegisterLocalPlayer(_client);
            cardNetwork.NotifyClientReadyServerRpc();
        }


    }
}