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
    private void CardActivatedObserversRpc(CardImpactVisual visual)
    {
        ClientBridge.Instance.VFXView.PlayCardImpact(visual);
    }

    [ServerRpc]
    public void RequestUseAbility(int cardIndex)
    {
        if (_cardProvider.TryGetCardAtIndex(cardIndex, out Card card))
        {
            Server_StartCast(card);
        }
    }

    private void Server_StartCast(Card card)
    {
        CardStartedObserversRpc(card.Definition.Visuals.CastAnimation);

        StartCoroutine(Server_CastTimeRoutine(card));
    }


    [ObserversRpc]
    private void CardStartedObserversRpc(CardCastAnimation castAnimation)
    {
        ClientBridge.Instance.VFXView.PlayCardCastAnimation(castAnimation);
    }

    private void Server_ExecuteCard(Card card)
    {
        card.ExecuteBegin();
        card.ExecuteCastTimeDone();

        CardActivatedObserversRpc(card.Definition.Visuals.Impact);

    }
    public void Server_CancelCard(Card card)
    {
        if (_pendingCasts.TryGetValue(card, out var routine))
        {
            StopCoroutine(routine);
            _pendingCasts.Remove(card);
        }

        card.ExecuteCancelled();
    }

    private IEnumerator Server_CastTimeRoutine(Card card)
    {
        yield return new WaitForSeconds(card.CastTime);
        _pendingCasts.Remove(card);
        Server_ExecuteCard(card);
    }
}