using FishNet;
using FishNet.Object;
using UnityEngine;

public class Player : NetworkBehaviour, IEntity
{
    [SerializeField] private CardRegistry cardRegistry;

    private PlayerState _state;
    private IAnimationHandler animationHandler;
    private IStatContainer stats;

    public PlayerState State => _state;
    public ICardContainer CardProvider => _state.CardManager;

    public Transform Transform => transform;

    public int TeamLayer => gameObject.layer;

    public bool IsDead { get; private set; }

    public IStatContainer Stats => stats;

    protected override void OnValidate()
    {
        base.OnValidate();

        if (cardRegistry == null)
        {
            Debug.LogWarning($"[{name}] Card Registry field is unassigned in the inspector.", this);
        }
    }
    private void Awake()
    {
        animationHandler = GetComponentInChildren<IAnimationHandler>();
        stats = GetComponent<IStatContainer>();
        if (animationHandler == null) Debug.LogError($"{name} missing IAnimationHandler", this);
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

        
        CardFactory factory = new CardFactory(cardRegistry, (go) => ServerManager.Spawn(go));
        _state = new PlayerState(this, factory, cardRegistry);
        cardController.SetCardProvider(CardProvider);
        
        
    }

    public void Die()
    {
        if (IsDead) return;
        IsDead = true;
        animationHandler.SetAnimationState(CharacterAnimationState.Dead);
    }
}