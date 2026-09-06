using System;
using FishNet;
using UnityEngine;

public static class CardFactory
{
    private static CardRegistry _registry;
    private static GameObject _cardRuntimePrefab;
    private static Func<GameObject, GameObject> _serverNetworkSpawn;
    private static Func<CardDefinition, VfxSpawnParams, Action> _clientNetworkSpawn;

    public static void Initialize(
        CardRegistry registry,
        GameObject cardRuntimePrefab,
        Func<GameObject, GameObject> serverNetworkSpawn,
        Func<CardDefinition, VfxSpawnParams, Action> clientNetworkSpawn)
    {
        if (!InstanceFinder.IsServerStarted)
            return;

        _registry = registry;
        _cardRuntimePrefab = cardRuntimePrefab;
        _serverNetworkSpawn = serverNetworkSpawn;
        _clientNetworkSpawn = clientNetworkSpawn;
    }

    public static CardInstance CreateCardInstance(
        CardDefinition definition,
        IEntity owner)
    {
        return new CardInstance(
            Guid.NewGuid(),
            definition,
            owner);
    }

    public static CardRuntime CreateRuntime(CardInstance card)
    {
        if (!InstanceFinder.IsServerStarted)
        {
            Debug.LogError("CardFactory.CreateRuntime can only be called on the server.");
            return null;
        }

        if (_cardRuntimePrefab == null)
        {
            Debug.LogError("CardFactory has not been initialized with a card runtime prefab.");
            return null;
        }

        GameObject cardObject = UnityEngine.Object.Instantiate(_cardRuntimePrefab);

        if (!cardObject.TryGetComponent(out CardRuntime runtime))
        {
            Debug.LogError(
                $"Card runtime prefab '{_cardRuntimePrefab.name}' is missing a CardRuntime component.");

            UnityEngine.Object.Destroy(cardObject);
            return null;
        }

        runtime.Initialize(
            Guid.NewGuid(),
            card.Definition,
            card.Owner);

        card.Definition.Construct(
            new CardInitContext(
                Guid.NewGuid(),
                card.Owner,
                _serverNetworkSpawn,
                _clientNetworkSpawn),
            runtime);

        return runtime;
    }
}