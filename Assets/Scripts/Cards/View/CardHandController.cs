using GameKit.Dependencies.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CardHandController : MonoBehaviour
{
    [SerializeField] private CardHandView handView;
    [SerializeField] private GameObject physicalCardPrefab;

    private GameObject[] cards;
    

    private void Start()
    {
        if (ClientBridge.Instance.Player != null)
            Bind(ClientBridge.Instance.Player);
        else
            ClientBridge.Instance.OnLocalPlayerRegistered += Bind;
    }

    private void Bind(Player player)
    {
        ICardContainer container = player.CardProvider;
        cards = new GameObject[container.Capacity];

        container.OnCardAdded += OnCardAdded;
        container.OnCardRemoved += OnCardRemoved;
    }

    private void OnCardAdded(int index, Card card)
    {
        GameObject cardObject = Instantiate(physicalCardPrefab, handView.transform);

        CardView view = cardObject.GetComponentInChildren<CardView>();
        view.Initialize(card.Definition, cardObject.transform);

        cards[index] = cardObject;

        handView.AddCard(cardObject);
    }

    private void OnCardRemoved(int index, Card card)
    {
        CardView view = cards[index].GetComponentInChildren<CardView>();
        handView.RemoveCard(view);
        cards[index] = null;
    }
    private void OnDestroy()
    {
        if (ClientBridge.Instance != null)
            ClientBridge.Instance.OnLocalPlayerRegistered -= Bind;
    }
}