using System;
using FishNet;
using UnityEngine;

public static class CardFactory
{
    private static CardRegistry _registry;
    private static GameObject _cardPrefab;
    private static Func<GameObject, GameObject> _serverNetworkSpawn;
    private static Func<CardDefinition, VfxSpawnParams, Action> _clientNetworkSpawn;
    

    public static void Initialize(
            CardRegistry registry,
            GameObject cardPrefab,
            Func<GameObject, GameObject> serverNetworkSpawn,
            Func<CardDefinition, VfxSpawnParams, Action> clientNetworkSpawn)
    {
        if (!InstanceFinder.IsServerStarted) return;
        
        _registry = registry;
        _cardPrefab = cardPrefab;
        _serverNetworkSpawn = serverNetworkSpawn;
        _clientNetworkSpawn = clientNetworkSpawn;

    }

    public static Card CreateFromDefinition(CardDefinition definition, IEntity owner)
    {
        if (!InstanceFinder.IsServerStarted)
        {
            Debug.LogError("CardFactory.CreateFromDefinition can only be called on the server.");
            return null;
        }

        if (definition == null)
        {
            Debug.LogError("Cannot create card from a null CardDefinition.");
            return null;
        }

        if (_cardPrefab == null)
        {
            Debug.LogError("CardFactory has not been initialized with a card prefab.");
            return null;
        }

        GameObject cardObject = UnityEngine.Object.Instantiate(_cardPrefab);

        if (!cardObject.TryGetComponent(out Card card))
        {
            Debug.LogError($"Card prefab '{_cardPrefab.name}' is missing a Card component.");
            UnityEngine.Object.Destroy(cardObject);
            return null;
        }

        card.Initialize(Guid.NewGuid(), definition, owner);


        definition.Construct(
                new CardInitContext(
                    Guid.NewGuid(),
                    owner,
                    _serverNetworkSpawn,
                    _clientNetworkSpawn),
                card);

        return card;
    }

    public static Card CreateFromNetworkId(string definitionId, IEntity owner)
    {
        if (_registry == null)
        {
            Debug.LogError("CardFactory has not been initialized with a CardRegistry.");
            return null;
        }

        if (!_registry.TryGet(definitionId, out CardDefinition definition))
        {
            Debug.LogError($"Missing CardDefinition for ID: {definitionId}");
            return null;
        }

        return CreateFromDefinition(definition, owner);
    }
}