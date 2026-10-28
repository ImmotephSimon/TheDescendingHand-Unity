// CardNetwork.cs
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CardNetwork : NetworkBehaviour
{
    public const int MaxHandSize = 10;

    [SerializeField] private AnimationClip standardCastAnimation;

    private readonly SyncList<CardVisuals> _hand = new();


    private IPlayerAnimationHandler _animationHandler;
    private Action _cancelAnimation;

    public event Action<int, CardVisuals> CardAdded;
    public event Action<int> CardRemoved;
    public Action<bool, IReadOnlyList<CardVisuals>> OnPileReceived { get; set; }

    // ---------- Lifecycle ----------

    private ICardServerHandler _server;

    public override void OnStartServer()
    {
        base.OnStartServer();
        _server = GetComponent<ICardServerHandler>();
        if (_server == null)
            throw new InvalidOperationException($"{nameof(ICardServerHandler)} missing on {name}.");

        _server.Initialize(this);
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        _hand.OnChange += OnHandChanged;

        // Late joiners don't get Add events for existing items.
        for (int i = 0; i < _hand.Count; i++)
            CardAdded?.Invoke(i, _hand[i]);
    }

    public override void OnStopClient()
    {
        _hand.OnChange -= OnHandChanged;
        base.OnStopClient();
    }

    public void InitializeClientObservers(IPlayerAnimationHandler animationHandler)
        => _animationHandler = animationHandler;

    // ---------- Server -> client ----------

    [Server]
    public void ServerAddCard(int index, CardVisuals visuals)
    {
        if (index >= 0 && index < _hand.Count) _hand.Insert(index, visuals);
        else _hand.Add(visuals);
    }

    [Server]
    public void ServerRemoveCard(int index)
    {
        if (index >= 0 && index < _hand.Count) _hand.RemoveAt(index);
    }

    [Server]
    public void ServerSendPile(bool drawPile, CardVisuals[] cards)
    {
        if (Owner != null)
            PileTargetRpc(Owner, drawPile, cards);
    }

    [Server]
    public void ServerCardStarted(AnimationString animationString, float duration)
    {
        CardStartedObserversRpc(animationString, duration);
    }

    [Server]
    public void ServerCardEnded() => CardEndedObserversRpc();

    [TargetRpc]
    private void PileTargetRpc(NetworkConnection conn, bool drawPile, CardVisuals[] cards)
        => OnPileReceived?.Invoke(drawPile, cards);

    [ObserversRpc]
    private void CardStartedObserversRpc(AnimationString animationString, float duration)
    {
        // TODO: resolve key through a client-side animation catalog.
        AnimationClip clip = ResolveClip(animationString) ?? standardCastAnimation;
        _cancelAnimation?.Invoke();
        _cancelAnimation = _animationHandler?.PlayAnimation(clip, duration: duration);
    }

    [ObserversRpc]
    private void CardEndedObserversRpc()
    {
        _cancelAnimation?.Invoke();
        _cancelAnimation = null;
    }

    private AnimationClip ResolveClip(AnimationString key)
    {
        AnimationRegistry.Instance.TryGetClip(key, out AnimationClip clip);
        return clip;
    }

    // ---------- Hand change handling ----------

    private void OnHandChanged(SyncListOperation op, int index,
        CardVisuals oldItem, CardVisuals newItem, bool asServer)
    {
        if (asServer) return;

        switch (op)
        {
            case SyncListOperation.Add:
            case SyncListOperation.Insert:
                CardAdded?.Invoke(index, newItem);
                break;
            case SyncListOperation.RemoveAt:
                CardRemoved?.Invoke(index);
                break;
            case SyncListOperation.Set:
                CardRemoved?.Invoke(index);
                CardAdded?.Invoke(index, newItem);
                break;
            case SyncListOperation.Clear:
                for (int i = 0; i < MaxHandSize; i++) CardRemoved?.Invoke(i);
                break;
        }
    }

    // ---------- Intents (client -> server) ----------

    [ServerRpc]
    public void RequestUseAbility(int index)
    {
        if (!IsValidIndex(index)) return;
        _server.HandleUseAbility(index);
    }

    [ServerRpc]
    public void RequestCancelAbility(int index)
    {
        if (!IsValidIndex(index)) return;
        _server.HandleCancelAbility(index);
    }

    [ServerRpc] public void RequestCancelCurrentCast() => _server.HandleCancelCurrentCast();
    [ServerRpc] public void RequestPile(bool v) => _server.HandleRequestPile(v);
    [ServerRpc] public void NotifyClientReadyServerRpc() => _server.HandleClientReady();

    private static bool IsValidIndex(int i) => i >= 0 && i < MaxHandSize;
}