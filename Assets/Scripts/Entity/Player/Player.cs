using FishNet;
using FishNet.Object;
using System.Collections;
using UnityEngine;

public class Player : Entity
{
    [SerializeField] private CardRegistry cardRegistry;

    private PlayerMovementController playerMovement;
    private CardManager _cardManager;
    public CardRegistry CardRegistry => cardRegistry;
    public ICardContainer CardProvider => _cardManager;


    protected override void Awake()
    {
        base.Awake();

        playerMovement = GetComponent<PlayerMovementController>();
        animationHandler = GetComponentInChildren<IAnimationHandler>();
        stats = GetComponent<IStatContainer>();
        if (animationHandler == null) Debug.LogError($"{name} missing IAnimationHandler", this);
        if (cardRegistry == null) Debug.LogWarning($"[{name}] Card Registry field is unassigned in the inspector.", this);
    }

    public void InitializeServer(CardFactory factory)
    {
        CardDefinition definition = cardRegistry.GetRandomCard();
        Card card = factory.CreateFromDefinition(definition, this);

        _cardManager = new CardManager(new Card[] { card }, handSize: 5);
    }

    protected override void OnDeath(IEntity killer)
    {
        playerMovement.LockMovement();
        StartCoroutine(ScheduleRevival());
    }

    private IEnumerator ScheduleRevival()
    {
        yield return new WaitForSeconds(4f);

        var healthHandler = GetComponent<IHealth>();
        healthHandler.AdjustHealth(healthHandler.MaxHealth, this);

        IsDead = false;
        playerMovement.UnlockMovement();
        animationHandler.SetAnimationState(CharacterAnimationState.Locomotion);

        GameWorld.Instance.NotifyRevive(this);
    }

    protected override void OnEntityDied(IEntity victim, IEntity killer)
    {
        if (victim.TeamLayer != TeamLayer)
        {
            // grant experience from victim
            Debug.Log($"Provide exp from {victim} death");
        }
    }
}