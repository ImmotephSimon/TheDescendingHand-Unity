using System;
using UnityEngine;


public class CardFactory
{
    private readonly CardRegistry _registry;
    private readonly Action<GameObject> _serverNetworkSpawn;
    private readonly Action<CardDefinition, Vector3, Quaternion> _clientNetworkSpawn;
    

    public CardFactory(
        CardRegistry registry,
        Action<GameObject> serverNetworkSpawn,
        Action<CardDefinition, Vector3, Quaternion> clientNetworkSpawn)
    {
        _registry = registry;
        _serverNetworkSpawn = serverNetworkSpawn;
        _clientNetworkSpawn = clientNetworkSpawn;
    }

    public Card CreateFromDefinition(CardDefinition definition, IEntity owner)
    {
        if (definition == null)
        {
            Debug.LogError("Cannot create card from a null CardDefinition.");
            return null;
        }

        return definition.Create(
            new CardInitContext(
                Guid.NewGuid(), // Runtime instance identity
                owner,
                _serverNetworkSpawn,
                _clientNetworkSpawn));
    }

    public Card CreateFromNetworkId(string definitionId, IEntity owner)
    {
        if (!_registry.TryGet(definitionId, out CardDefinition definition))
        {
            Debug.LogError($"Missing CardDefinition for ID: {definitionId}");
            return null;
        }

        return CreateFromDefinition(definition, owner);
    }
}