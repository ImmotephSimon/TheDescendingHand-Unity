using FishNet;
using FishNet.Object;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private CardRegistry cardRegistry;

    private PlayerState _state;

    public PlayerState State => _state;
    public ICardContainer CardProvider => _state.CardManager;
    protected override void OnValidate()
    {
        base.OnValidate();

        if (cardRegistry == null)
        {
            Debug.LogWarning($"[{name}] Card Registry field is unassigned in the inspector.", this);
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner)
            return;

        ClientBridge.Instance.RegisterPlayer(this);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (cardRegistry == null)
        {
            throw new System.NullReferenceException($"[{name}] Card Registry asset is missing from the Player inspector slot.");
        }

        var cardController = GetComponentInChildren<CardController>();
        if (cardController == null)
        {
            throw new System.NullReferenceException($"[{name}] Missing expected CardController component on the Player GameObject.");
        }

        if (InstanceFinder.ServerManager == null)
        {
            throw new System.NullReferenceException($"[{name}] FishNet InstanceFinder.ServerManager is null during OnStartServer execution.");
        }

        
        
        cardRegistry.Initialize();
        CardFactory factory = new CardFactory(cardRegistry, transform, (go) => ServerManager.Spawn(go));
        _state = new PlayerState(factory);
        cardController.SetCardProvider(CardProvider);
    }
}