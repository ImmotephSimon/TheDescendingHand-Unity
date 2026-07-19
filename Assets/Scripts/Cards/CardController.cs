using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.XR;

public class CardController : NetworkBehaviour, IAbilitySystem
{
    public static ClientBridge Instance { get; private set; }
    private readonly List<Card> _activeCards = new();

    private ICardContainer _cardProvider;
    private readonly Dictionary<Card, Coroutine> _pendingCasts = new();

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public void SetCardProvider(ICardContainer provider)
    {
        _cardProvider = provider;
    }

    [ObserversRpc]
    private void CardExecutedObserversRpc(Guid cardId)
    {
        if (IsServerStarted)
            return;

        ClientBridge.Instance.VFXView.PlayCardAnimation(cardId);
    }

    [ServerRpc]
    public void RequestUseAbility(int cardIndex)
    {
        if (_cardProvider.TryGetCardAtIndex(cardIndex, out Card card))
        {
            StartCast(card);
        }
    }

    private void StartCast(Card card)
    {
        
        _activeCards.Add(card);

        CardStartedObserversRpc(card.Id);

        StartCoroutine(CastRoutine(card));
    }

    private void ExecuteCard(Card card)
    {
        card.ExecuteBegin();
        card.ExecuteCastTimeDone();

        CardExecutedObserversRpc(card.Id);
    }
    public void CancelCard(Card card)
    {
        if (_pendingCasts.TryGetValue(card, out var routine))
        {
            StopCoroutine(routine);
            _pendingCasts.Remove(card);
        }

        card.ExecuteCancelled();
        _activeCards.Remove(card);
    }

    private void CardStartedObserversRpc(Guid id)
    {
        Debug.Log($"Started casting id {id}");
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        foreach (var card in _activeCards)
            card.Tick(dt);
    }
    private IEnumerator CastRoutine(Card card)
    {
        yield return new WaitForSeconds(card.CastTime);
        _pendingCasts.Remove(card);
        ExecuteCard(card);
    }
}