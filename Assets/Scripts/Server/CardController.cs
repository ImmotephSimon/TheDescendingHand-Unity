using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardController : MonoBehaviour, ICardServerHandler
{
    public ICardContainer CardProvider => _cardManager;
    public ICardPiles CardPiles => _cardManager;

    private CardNetwork _network;
    private CardManager _cardManager;
    private IEntity _owner;
    private IStatContainer _stats;

    private CardRuntime _activeCard;
    private int _activeCardSlotIndex = -1;
    private bool _isInputHeld;
    private bool _isCasting;
    private ModifierHandle _movementLockHandle;
    private (CardRuntime Card, int HandIndex, Coroutine Handle)? _pendingCast;

    private Action _onActiveCardCompleted;
    private Action _onActiveCardInterrupted;

    public void Initialize(CardNetwork network)
    {
        _network = network;
        Debug.Assert(_network != null, $"[{name}] CardController requires a CardNetwork", this);
    }

    // Called by whatever spawns the player, once the owner exists.
    public void InitializeServer(IEntity owner)
    {
        _owner = owner;

        _stats = GetComponentInParent<IStatContainer>();
        Debug.Assert(_stats != null, $"[{name}] CardController requires an IStatContainer", this);

        var cards = new List<CardInstance>();
        foreach (CardDefinition definition in ItemRegistry.Instance.CardRegistry.Cards)
            cards.Add(new CardInstance(Guid.NewGuid(), definition, _owner));

        _cardManager = new CardManager(cards);
        _cardManager.OnCardAdded += (index, card) => _network.ServerAddCard(index, card.Definition.Visuals);
        _cardManager.OnCardRemoved += (index, _) => _network.ServerRemoveCard(index);
    }

    // ---------- ICardServerHandler ----------

    public void HandleClientReady() => _cardManager?.DrawHand();

    public void HandleUseAbility(int cardIndex)
    {
        if (_cardManager == null || _isCasting || _activeCard != null) return;

        if (_cardManager.TryGetCardAtIndex(cardIndex, out CardInstance card))
        {
            _activeCardSlotIndex = cardIndex;
            _isInputHeld = true;
            StartCast(card, cardIndex);
        }
    }

    public void HandleCancelAbility(int cardIndex)
    {
        if (_activeCardSlotIndex != cardIndex) return;

        _isInputHeld = false;
        _activeCard?.GetCardComponent<ChannelingComponent>()?.SetInputHeld(false);
    }

    public void HandleCancelCurrentCast()
    {
        if (_pendingCast == null) return;

        var pending = _pendingCast.Value;
        StopCoroutine(pending.Handle);
        pending.Card.ExecuteCancelled();

        _pendingCast = null;
        _isCasting = false;
        _activeCardSlotIndex = -1;
        _isInputHeld = false;

        EndCard();
    }

    public void HandleRequestPile(bool drawPile)
    {
        if (_cardManager == null) return;

        var pile = drawPile ? _cardManager.DrawPile : _cardManager.DiscardPile;
        CardVisuals[] visuals = pile.Select(c => c.Visuals).ToArray();
        // Draw pile: sort or shuffle here so the client can't read draw order.
        _network.ServerSendPile(drawPile, visuals);
    }

    // ---------- Casting ----------

    private void StartCast(CardInstance card, int handIndex)
    {
        _isCasting = true;

        _movementLockHandle = _stats.AddModifier(
            new StatModifier(GameTags.ModStatMovement, MathOp.Multiplicative, 0));

        CardRuntime runtime = CardFactory.CreateRuntime(card);
        runtime.SetTargetLocation(
            runtime.SpawnAtCursor ? _owner.CursorPosition : _owner.Transform.position);

        float castTime = _stats.GetStat(GameTags.ModOffenseCastSpeed, runtime.Tags, runtime.CastTime);
        var channel = runtime.GetCardComponent<ChannelingComponent>();
        float animationDuration = channel != null ? channel.TotalDuration : castTime;

        _network.ServerCardStarted(runtime.Definition.Visuals.AnimationOverride, animationDuration);

        var handle = StartCoroutine(CastTimeRoutine(runtime, castTime));
        _pendingCast = (runtime, handIndex, handle);
    }

    private IEnumerator CastTimeRoutine(CardRuntime card, float castTime)
    {
        yield return new WaitForSeconds(castTime);

        _cardManager.DiscardPlayedCard();
        _pendingCast = null;
        _isCasting = false;

        ExecuteCard(card);
    }

    private void ExecuteCard(CardRuntime card)
    {
        var channel = card.GetCardComponent<ChannelingComponent>();

        if (channel != null)
        {
            _activeCard = card;
            _onActiveCardCompleted = () => EndChannel(card);
            _onActiveCardInterrupted = () => EndChannel(card);
            channel.OnCompleted += _onActiveCardCompleted;
            channel.OnInterrupted += _onActiveCardInterrupted;

            _stats.RemoveModifier(_movementLockHandle);
            _movementLockHandle = _stats.AddModifier(
                new StatModifier(GameTags.ModStatMovement, MathOp.Multiplicative, card.Definition.CastMoveSpeed));
        }

        card.Cast();

        if (channel != null) channel.SetInputHeld(_isInputHeld);
        else EndCard();
    }

    private void EndChannel(CardRuntime card)
    {
        if (_activeCard != card) return;

        var channel = card.GetCardComponent<ChannelingComponent>();
        if (channel != null)
        {
            channel.OnCompleted -= _onActiveCardCompleted;
            channel.OnInterrupted -= _onActiveCardInterrupted;
        }
        _onActiveCardCompleted = null;
        _onActiveCardInterrupted = null;

        _activeCard = null;
        _activeCardSlotIndex = -1;
        _isInputHeld = false;

        EndCard();
    }

    private void EndCard()
    {
        _stats.RemoveModifier(_movementLockHandle);
        _movementLockHandle = default;
        _network.ServerCardEnded();
    }
}