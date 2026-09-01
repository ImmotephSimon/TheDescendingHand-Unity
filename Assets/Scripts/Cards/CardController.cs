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

    private Card _activeCard;
    private int _activeCardSlotIndex = -1;
    private bool _isInputHeld = false;

    private PlayerMovementController playerMovement;
    private bool isCasting = false;
    private ModifierHandle _movementLockHandle;
    private IStatContainer stats;
    private CardManager _cardManager;
    private IEntity _owner; 
    private CardRegistry _registry;
    private (Card Card, int HandIndex, Coroutine Handle)? _pendingCast;

    private Action _onActiveCardCompleted;
    private Action _onActiveCardInterrupted;
    private Coroutine _tickingCardsRoutine;
    private IAnimationHandler _animationHandler;
    private readonly List<Card> _tickingCards = new();

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
        //CardDefinition definition = _registry.GetRandomCard();
        List<Card> cards = new();
        foreach (CardDefinition def in _registry.Cards)
        {
            cards.Add(CardFactory.CreateFromDefinition(def, _owner));
        }
        
        
        _cardManager = new CardManager(cards);
        _cardManager.OnCardAdded += OnCardAdded;
        _cardManager.OnCardRemoved += OnCardRemoved;
    }



    private void OnCardRemoved(int index, Card card)
    {
        RemoveCardTargetRpc(Owner, index);
    }

    [TargetRpc]
    private void RemoveCardTargetRpc(NetworkConnection owner, int index)
    {
        ClientBridge.Instance.CardHandController.OnCardRemoved(index);
    }

    private void OnCardAdded(int index, Card card)
    {
        AddCardTargetRpc(Owner, index, card.Definition.Id);
    }

    [TargetRpc]
    private void AddCardTargetRpc(NetworkConnection conn, int index, string cardDefinitionId)
    {
        ClientBridge.Instance.CardHandController.OnCardAdded(index, cardDefinitionId);
    }



    [ObserversRpc]
    private void CardStartedObserversRpc(string cardId)
    {
        if (!ClientBridge.Instance.CardRegistry.TryGet(cardId, out var definition))
            return;



        var clip = definition.Visuals.AnimationOverride != null
           ? definition.Visuals.AnimationOverride
           : standardCastAnimation;

        Debug.Assert(clip != null, $"[CardStartedObserversRpc] No animation clip handled for {clip}.");

        _animationHandler?.PlayAnimation(clip, duration: clip.length);
    }

    [ObserversRpc]
    private void CardInterruptedObserversRpc()
    {
        _animationHandler?.StopCurrentAnimation();
    }

    [ServerRpc]
    public void RequestUseAbility(int cardIndex)
    {
        if (isCasting || _activeCard != null) return;

        if (_cardManager.TryGetCardAtIndex(cardIndex, out Card card))
        {
            _activeCardSlotIndex = cardIndex;
            _isInputHeld = true;
            Server_StartCast(card, cardIndex);
            Camera.main.GetComponent<CardHandView>();
        }
    }

    [ServerRpc]
    public void RequestCancelAbility(int cardIndex)
    {
        if (_activeCardSlotIndex != cardIndex) return;

        _isInputHeld = false;

        if (_activeCard != null)
        {
            var channel = _activeCard.GetCardComponent<ChannelingComponent>();
            channel?.SetInputHeld(false);
        }
    }

    private void Server_StartCast(Card card, int handIndex)
    {
        isCasting = true;
        _movementLockHandle = stats.AddModifier(new StatModifier(GameTags.ModStatMovement, MathOp.Multiplicative, 0));

        card.SetTargetLocation(card.SpawnAtCursor ? _owner.CursorPosition : _owner.Transform.position);

        CardStartedObserversRpc(card.Definition.Id);
        var castHandle = StartCoroutine(Server_CastTimeRoutine(card));
        _pendingCast = (card, handIndex, castHandle);
    }

    private void Server_ExecuteCard(Card card)
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

        if (!_tickingCards.Contains(card))
            _tickingCards.Add(card);
        if (_tickingCardsRoutine == null)
            _tickingCardsRoutine = StartCoroutine(TickCardsRoutine());

        if (channel != null)
        {
            channel.SetInputHeld(_isInputHeld);
        }
        else
        {
            Server_EndCard(card);
        }
    }

    private void Server_EndChannel(Card card)
    {
        if (_activeCard != card) return;

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

    private void Server_EndCard(Card card)
    {
        _owner.Stats.RemoveModifier(_movementLockHandle);
        CardInterruptedObserversRpc();
    }


    private IEnumerator TickCardsRoutine()
    {
        while (_tickingCards.Count > 0)
        {
            for (int i = _tickingCards.Count - 1; i >= 0; i--)
            {
                var card = _tickingCards[i];
                card.Tick(Time.deltaTime);

                if (!card.IsTicking)
                    _tickingCards.RemoveAt(i);
            }

            yield return null;
        }

        _tickingCardsRoutine = null;
    }

    private IEnumerator Server_CastTimeRoutine(Card card)
    {
        yield return new WaitForSeconds(stats.GetStat(GameTags.ModOffenseCastSpeed, card.Tags, card.CastTime));
        _cardManager.DiscardCardInHand(_pendingCast.Value.HandIndex);
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

        string[] ids = pile
            .Select(card => card.Id)
            .ToArray();

        SendPileTargetRpc(Owner, drawPile, ids);
    }

    [TargetRpc]
    private void SendPileTargetRpc(
        NetworkConnection conn,
        bool drawPile,
        string[] ids)
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