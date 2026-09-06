using FishNet.Connection;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardController : NetworkBehaviour, IAbilitySystem
{
    [SerializeField] private AnimationClip standardCastAnimation;

    public ICardContainer CardProvider => _cardManager;
    public ICardPiles CardPiles => _cardManager;

    public event Action<bool, IReadOnlyList<CardDefinition>> OnPileReceived;

    private CardRuntime _activeCard;
    private int _activeCardSlotIndex = -1;
    private bool _isInputHeld = false;

    private PlayerMovementController playerMovement;
    private bool isCasting = false;
    private ModifierHandle _movementLockHandle;
    private IStatContainer stats;
    private CardManager _cardManager;
    private IEntity _owner;
    private CardRegistry _registry;
    private (CardRuntime Card, int HandIndex, Coroutine Handle)? _pendingCast;

    private Action _onActiveCardCompleted;
    private Action _onActiveCardInterrupted;
    private IAnimationHandler _animationHandler;

    public void InitializeClientObservers(IAnimationHandler animationHandler)
    {
        _animationHandler = animationHandler;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        playerMovement = GetComponentInParent<PlayerMovementController>();
        Debug.Assert(playerMovement != null, "CardController requires a PlayerMovementController");

        stats = GetComponentInParent<IStatContainer>();
        Debug.Assert(stats != null, "CardController requires an IStatContainer");
    }

    public void InitializeServer(IEntity owner, CardRegistry registry)
    {
        _owner = owner;
        _registry = registry;

        List<CardInstance> cards = new();

        foreach (CardDefinition definition in _registry.Cards)
        {
            cards.Add(new CardInstance(
                Guid.NewGuid(),
                definition,
                _owner));
        }

        _cardManager = new CardManager(cards);
        _cardManager.OnCardAdded += OnCardAdded;
        _cardManager.OnCardRemoved += OnCardRemoved;
    }

    private void OnCardRemoved(int index, CardInstance card)
    {
        RemoveCardTargetRpc(Owner, index);
    }

    [TargetRpc]
    private void RemoveCardTargetRpc(NetworkConnection owner, int index)
    {
        ClientBridge.Instance.CardHandController.OnCardRemoved(index);
    }

    private void OnCardAdded(int index, CardInstance card)
    {
        AddCardTargetRpc(Owner, index, card.Definition.Id);
    }

    [TargetRpc]
    private void AddCardTargetRpc(NetworkConnection conn, int index, Guid cardDefinitionId)
    {
        ClientBridge.Instance.CardHandController.OnCardAdded(index, cardDefinitionId);
    }

    [ObserversRpc]
    private void CardStartedObserversRpc(Guid cardId, float animationDuration)
    {
        if (!ClientBridge.Instance.CardRegistry.TryGet(cardId, out var definition))
            return;

        var clip = definition.Visuals.AnimationOverride != null
            ? definition.Visuals.AnimationOverride
            : standardCastAnimation;

        Debug.Assert(
            clip != null,
            $"[CardStartedObserversRpc] No animation clip handled for {clip}.");

        _animationHandler?.PlayAnimation(clip, duration: animationDuration);
    }


    [ServerRpc]
    public void RequestUseAbility(int cardIndex)
    {
        if (isCasting || _activeCard != null)
            return;

        if (_cardManager.TryGetCardAtIndex(cardIndex, out CardInstance card))
        {
            _activeCardSlotIndex = cardIndex;
            _isInputHeld = true;
            Server_StartCast(card, cardIndex);
        }
    }

    [ServerRpc]
    public void RequestCancelAbility(int cardIndex)
    {
        if (_activeCardSlotIndex != cardIndex)
            return;

        _isInputHeld = false;

        if (_activeCard != null)
        {
            var channel = _activeCard.GetCardComponent<ChannelingComponent>();
            channel?.SetInputHeld(false);
        }
    }

    private void Server_StartCast(CardInstance card, int handIndex)
    {
        isCasting = true;

        _movementLockHandle = stats.AddModifier(
            new StatModifier(GameTags.ModStatMovement, MathOp.Multiplicative, 0));

        CardRuntime runtime = CardFactory.CreateRuntime(card);

        runtime.SetTargetLocation(
            runtime.SpawnAtCursor
                ? _owner.CursorPosition
                : _owner.Transform.position);

        var castTime = stats.GetStat(
                GameTags.ModOffenseCastSpeed,
                runtime.Tags,
                runtime.CastTime);

        CardStartedObserversRpc(runtime.Definition.Id, castTime);

        var castHandle = StartCoroutine(Server_CastTimeRoutine(runtime, castTime));
        _pendingCast = (runtime, handIndex, castHandle);
    }

    private void Server_ExecuteCard(CardRuntime card)
    {
        var channel = card.GetCardComponent<ChannelingComponent>();

        if (channel != null)
        {
            _activeCard = card;

            _onActiveCardCompleted = () => Server_EndChannel(card);
            _onActiveCardInterrupted = () => Server_EndChannel(card);

            channel.OnCompleted += _onActiveCardCompleted;
            channel.OnInterrupted += _onActiveCardInterrupted;
        }

        card.ExecuteCastTimeDone();

        if (channel != null)
            channel.SetInputHeld(_isInputHeld);
        else
            Server_EndCard(card);
    }

    private void Server_EndChannel(CardRuntime card)
    {
        if (_activeCard != card)
            return;

        var channel = card.GetCardComponent<ChannelingComponent>();

        if (channel != null)
        {
            channel.OnCompleted -= _onActiveCardCompleted;
            channel.OnInterrupted -= _onActiveCardInterrupted;
            _onActiveCardCompleted = null;
            _onActiveCardInterrupted = null;
        }

        _activeCard = null;
        _activeCardSlotIndex = -1;
        _isInputHeld = false;

        Server_EndCard(card);
    }

    private void Server_EndCard(CardRuntime card)
    {
        _owner.Stats.RemoveModifier(_movementLockHandle);
        _movementLockHandle = default;

        // Animation shouldn't be handled here. 
    }

    private IEnumerator Server_CastTimeRoutine(CardRuntime card, float castTime)
    {
        yield return new WaitForSeconds(castTime);

        _cardManager.DiscardPlayedCard();

        _pendingCast = null;
        isCasting = false;

        Server_ExecuteCard(card);
    }

    [ServerRpc]
    public void RequestPile(bool drawPile)
    {
        var pile = drawPile
            ? _cardManager.DrawPile
            : _cardManager.DiscardPile;

        Guid[] ids = pile
            .Select(card => card.Id)
            .ToArray();

        SendPileTargetRpc(Owner, drawPile, ids);
    }

    [TargetRpc]
    private void SendPileTargetRpc(
        NetworkConnection conn,
        bool drawPile,
        Guid[] ids)
    {
        var cards = new List<CardDefinition>();

        foreach (var id in ids)
        {
            if (_registry.TryGet(id, out var definition))
                cards.Add(definition);
            else
                Debug.LogError($"Unknown CardDefinition ID '{id}'.");
        }

        OnPileReceived?.Invoke(drawPile, cards);
    }

    [ServerRpc]
    public void NotifyClientReadyServerRpc()
    {
        _cardManager.DrawHand();
    }

    [ServerRpc]
    public void RequestCancelCurrentCast()
    {
        if (_pendingCast == null)
            return;

        var pending = _pendingCast.Value;

        StopCoroutine(pending.Handle);
        pending.Card.ExecuteCancelled();

        _pendingCast = null;
        isCasting = false;
        _activeCardSlotIndex = -1;
        _isInputHeld = false;

        Server_EndCard(pending.Card);
    }
}
