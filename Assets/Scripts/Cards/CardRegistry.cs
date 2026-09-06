using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Registries/Cards")]
public class CardRegistry : ScriptableObject
{
    [SerializeField] private List<CardDefinition> cards = new();

    private Dictionary<Guid, CardDefinition> _lookup;

    public List<CardDefinition> Cards => cards;

    private static CardRegistry _instance;
    public static CardRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<CardRegistry>("CardRegistry");
                if (_instance == null)
                {
                    Debug.LogError("No CardRegistry asset found in Resources folder!");
                }
            }
            return _instance;
        }
    }

    [ContextMenu("Log Card IDs")]
    private void LogCardIds()
    {
        foreach (var card in cards)
        {
            if (card != null)
                Debug.Log($"{card.name} = {card.Id}");
        }
    }

    private void OnEnable() => _lookup = null;

#if UNITY_EDITOR
    private void OnValidate() => _lookup = null;
#endif

    public bool TryGet(Guid definitionId, out CardDefinition definition)
    {
        if (definitionId == Guid.Empty)
        {
            definition = null;
            return false;
        }

        InitializeIfNeeded();
        return _lookup.TryGetValue(definitionId, out definition);
    }

    private void InitializeIfNeeded()
    {
        if (_lookup != null) return;

        _lookup = new Dictionary<Guid, CardDefinition>();

        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];

            if (card == null)
            {
                Debug.LogWarning($"[{name}] Null entry found in Cards list at index {i}.");
                continue;
            }

            if (card.Id == Guid.Empty)
            {
                Debug.LogError($"[{name}] CardDefinition '{card.name}' has no assigned Id.");
                continue;
            }

            if (!_lookup.TryAdd(card.Id, card))
            {
                Debug.LogError($"[{name}] Duplicate CardDefinition Id '{card.Id}' on asset '{card.name}'. Collision with '{_lookup[card.Id].name}'.");
            }
        }
    }

    public CardDefinition GetRandomCard()
    {
        if (cards == null || cards.Count == 0) return null;
        return cards[UnityEngine.Random.Range(0, cards.Count)];
    }
}