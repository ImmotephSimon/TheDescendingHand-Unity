using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Registry")]
public class CardRegistry : ScriptableObject
{
    [SerializeField] private List<CardDefinition> cards = new();

    private Dictionary<string, CardDefinition> _lookup;

    private void OnEnable()
    {
        _lookup = null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _lookup = null;
    }
#endif

    public bool TryGet(string definitionId, out CardDefinition definition)
    {
        if (string.IsNullOrEmpty(definitionId))
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

        _lookup = new Dictionary<string, CardDefinition>();

        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];

            if (card == null)
            {
                Debug.LogWarning($"[{name}] Null entry found in Cards list at index {i}.");
                continue;
            }

            if (string.IsNullOrEmpty(card.Id))
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
        return cards[Random.Range(0, cards.Count)];
    }
}