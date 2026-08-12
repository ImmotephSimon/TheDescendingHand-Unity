using FishNet;
using FishNet.Object;
using System;
using System.Collections;
using UnityEngine;

public class Player : Entity
{
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private float interactRange = 2f;

    private PlayerMovementController playerMovement;
    private CardController _cardController;
    public CardController CardController => _cardController;
    public float InteractRange => interactRange;

    public override Vector3 CursorPosition => playerMovement.CursorPosition;

    protected override void Awake()
    {
        base.Awake();

        playerMovement = GetComponent<PlayerMovementController>();
        animationHandler = GetComponentInChildren<IAnimationHandler>();
        _cardController = GetComponentInChildren<CardController>();
        if (animationHandler == null) Debug.LogError($"{name} missing IAnimationHandler", this);

        stats.AddModifier(new StatModifier (
            GameTags.ModStatMana,
            MathOp.Added,
            20)
        );
        stats.AddModifier(new StatModifier(
            GameTags.ModOffenseDamage,
            MathOp.Added,
            3,
            new TagContainer(GameTags.RestrictionPhysical))
        );
    }

    public void InitializeLocalPlayer()
    {
        ClientBridge.Instance.HUD.Bind(this);
        ClientBridge.Instance.GlobePositioner.Initialize(this);
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
        // If the victim targets our team layer, it's an enemy
        if (victim.HostileLayer == TeamLayer)
        {
            Debug.Log($"Provide exp from {victim} death");
        }
    }

    public void TryInteract()
    {
        IInteractable interact = FindNearbyInteractable();

        if (interact == null)
            return;

        interact.Interact(this);
    }
    private IInteractable FindNearbyInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            interactRange,
            interactLayer);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IInteractable>(out var interactable))
                return interactable;
        }

        return null;
    }
}