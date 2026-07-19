using System;
using UnityEngine;

public class CardFactory
{
    private readonly CardRegistry _registry;
    private readonly Action<GameObject> _onNetworkSpawn;
    private readonly Transform _owner;

    
    public CardFactory(CardRegistry registry, Transform owner, Action<GameObject> onNetworkSpawn)
    {
        _registry = registry;
        _owner = owner;
        // Pass the network spawn method from network controller
        _onNetworkSpawn = onNetworkSpawn;
    }

    public Card Create(string cardId)
    {
        if (!_registry.TryGet(cardId, out CardRegistry.Entry entry))
        {
            Debug.LogError($"CardRegistry missing card: {cardId}");
            return null;
        }

        Type cardType = Type.GetType($"{cardId}Card, Assembly-CSharp");
        if (cardType == null)
        {
            Debug.LogError($"Could not find card type: {cardId}Card");
            return null;
        }

        Type[] signature =
        {
        typeof(Guid),
        typeof(float),
        typeof(Transform),
        typeof(GameObject),
        typeof(Action<GameObject>)
    };

        var constructor = cardType.GetConstructor(signature);
        if (constructor == null)
        {
            Debug.LogError($"Missing expected constructor on {cardType.Name}");
            return null;
        }

        object[] parameters =
        {
        Guid.NewGuid(),
        0f,
        _owner,
        entry.ProjectilePrefab,
        _onNetworkSpawn
    };

        return (Card)constructor.Invoke(parameters);
    }
}