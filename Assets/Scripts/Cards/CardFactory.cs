using System;
using UnityEngine;

public class CardFactory
{
    private readonly CardRegistry _registry;
    private readonly Action<GameObject> _onNetworkSpawn;

    public CardFactory(
        CardRegistry registry,
        Action<GameObject> onNetworkSpawn)
    {
        _registry = registry;
        _onNetworkSpawn = onNetworkSpawn;
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
                _onNetworkSpawn));
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