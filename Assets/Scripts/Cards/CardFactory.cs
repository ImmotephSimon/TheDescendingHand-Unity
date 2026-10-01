using System;
using FishNet;
using UnityEngine;

public static class CardFactory
{
    public static CardInstance CreateCardInstance(CardDefinition definition, IEntity owner)
    {
        return new CardInstance(Guid.NewGuid(), definition, owner);
    }

    public static CardRuntime CreateRuntime(CardInstance card)
    {
        if (!InstanceFinder.IsServerStarted)
        {
            Debug.LogError("CardFactory.CreateRuntime can only be called on the server.");
            return null;
        }

        var prefab = NetworkManager.Instance.CardPrefab;
        if (prefab == null)
        {
            Debug.LogError("CardNetworkManager has no card runtime prefab assigned.");
            return null;
        }

        GameObject cardObject = UnityEngine.Object.Instantiate(prefab);

        if (!cardObject.TryGetComponent(out CardRuntime runtime))
        {
            Debug.LogError($"Card runtime prefab '{prefab.name}' is missing a CardRuntime component.");
            UnityEngine.Object.Destroy(cardObject);
            return null;
        }

        runtime.Initialize(Guid.NewGuid(), card.Definition, card.Owner);

        card.Definition.Construct(
            new CardInitContext(Guid.NewGuid(), card.Owner),
            runtime);

        return runtime;
    }
}