using FishNet.Connection;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CardController : NetworkBehaviour, IAbilitySystem
{
    public static ClientBridge Instance { get; private set; }
    public ICardContainer CardProvider => _cardManager;
    public ICardPiles CardPiles => _cardManager;

    private Card _activeCard;
    private int _activeCardSlotIndex = -1;
    private bool _isInputHeld = false;

    private PlayerMovementController playerMovement;
    private bool isCasting = false;
    private IStatContainer stats;
    private CardManager _cardManager;
    private IEntity _owner;
    private CardFactory _factory;
    private CardRegistry _registry;
    private readonly Dictionary<Card, Coroutine> _pendingCastHandles = new();

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

        _cardManager.OnCardAdded += OnCardAdded;
        _cardManager.OnCardRemoved += OnCardRemoved;
    }

    public void InitializeServer(IEntity owner, CardFactory factory, CardRegistry registry)
    {
        _owner = owner;
        _factory = factory;
        _registry = registry;
        CardDefinition definition = _registry.GetRandomCard();
        Card card = factory.CreateFromDefinition(definition, _owner);
        Card card2 = factory.CreateFromDefinition(definition, _owner);
        _cardManager = new CardManager(new Card[] { card, card2 }, handSize: 5);
    }

    internal void OnLocalPlayerReady()
    {
        _cardManager.DrawHand();
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
    private void CardStartedObserversRpc(CardCastAnimation castAnimation)
    {
        _animationHandler?.PlayAnimation(castAnimation);
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
            Server_StartCast(card);
        }
    }

    [ServerRpc]
    public void RequestCancelAbility(int cardIndex)
    {
        if (_activeCardSlotIndex != cardIndex) return;

        _isInputHeld = false;

        if (_activeCard != null)
        {
            var channel = _activeCard.GetComponent<ChannelingComponent>();
            channel?.SetInputHeld(false);
        }
    }

    private void Server_StartCast(Card card)
    {
        isCasting = true;
        playerMovement.LockMovement();
        CardStartedObserversRpc(card.Definition.Visuals.CastAnimation);
        var castHandle = StartCoroutine(Server_CastTimeRoutine(card));
        _pendingCastHandles[card] = castHandle;
    }

    private void Server_ExecuteCard(Card card)
    {
        var channel = card.GetComponent<ChannelingComponent>();
        if (channel != null)
        {
            _activeCard = card;

            _onActiveCardCompleted = () => Server_EndChannel(card);
            _onActiveCardInterrupted = () => Server_EndChannel(card);

            channel.OnCompleted += _onActiveCardCompleted;
            channel.OnInterrupted += _onActiveCardInterrupted;
        }

        isCasting = false;

        card.ExecuteBegin();
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

        var channel = card.GetComponent<ChannelingComponent>();
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
        playerMovement.UnlockMovement();
        CardInterruptedObserversRpc();
    }


    public void Server_EndCast(Card card)
    {
        if (_pendingCastHandles.TryGetValue(card, out var handle))
        {
            StopCoroutine(handle);
            _pendingCastHandles.Remove(card);
            isCasting = false;
        }

        card.ExecuteCancelled();

        Server_EndChannel(card);
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
        _pendingCastHandles.Remove(card);
        Server_ExecuteCard(card);
    }

}