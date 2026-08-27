using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity, IPlayerCollection
{
    [SerializeField] private StatModifierData startingStats;
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private TriggerDetector enemyDetector;

    private PlayerMovementController playerMovement;
    private CardController _cardController;
    private ModifierHandle _sprintHandle = ModifierHandle.Invalid;
    private ModifierHandle _movementLockHandle;

    public CardController CardController => _cardController;
    public float InteractRange => interactRange;

    public override Vector3 CursorPosition => playerMovement.CursorPosition;

    protected override void Awake()
    {
        base.Awake();

        playerMovement = GetComponent<PlayerMovementController>();
        _animationHandler = GetComponentInChildren<IAnimationHandler>();
        _cardController = GetComponent<CardController>();
        Debug.Assert(_animationHandler != null, $"{name} missing IAnimationHandler");
        Debug.Assert(_cardController != null, $"{name} missing CardController");

        enemyDetector.OnEntered += _ => ToggleSprint(false);
        enemyDetector.OnExited += _ => ToggleSprint(enemyDetector.Count == 0);
        ToggleSprint(true);

        startingStats.ApplyTo(_stats);
    }

    private void ToggleSprint(bool isEnabled)
    {
        if (isEnabled)
        {
            if (_sprintHandle.IsValid)
                return;

            _sprintHandle = _stats.AddModifier(
                new StatModifier(GameTags.ModStatMovement, MathOp.Multiplicative, 1.8f));
        }
        else if (_sprintHandle.IsValid)
        {
            _stats.RemoveModifier(_sprintHandle);
            GetComponent<PlayerNetworkActions>().ShowSpeechBubble("What was that?");
        }
    }

    protected override void OnDeath(IEntity killer)
    {
        _movementLockHandle = Stats.AddModifier(new StatModifier(GameTags.ModStatMovement, MathOp.Multiplicative, 0));
        StartCoroutine(ScheduleRevival());
    }

    private IEnumerator ScheduleRevival()
    {
        yield return new WaitForSeconds(4f);

        var healthHandler = GetComponent<IHealth>();
        healthHandler.AdjustHealth(healthHandler.MaxHealth, this);

        IsDead = false;
        Stats.RemoveModifier(_movementLockHandle);
        _animationHandler.SetAnimationState(CharacterAnimationState.Locomotion);

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

    
    public IInteractable FindNearbyInteractable()
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



    private void OnDestroy()
    {
        startingStats.RemoveFrom(_stats);
    }

    public void AddGold(int amount)
    {
        // TODO
    }
}