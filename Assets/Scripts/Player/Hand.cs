using System;
using System.Collections.Generic;

public class Hand : ICardContainer
{
    private readonly List<Card> _cards = new();

    public void AddCard(Card card)
    {
        _cards.Add(card);
    }

    public bool TryGetCardAtIndex(int cardIndex, out Card card)
    {
        if (cardIndex < 0 || cardIndex >= _cards.Count)
        {
            card = null;
            return false;
        }

        card = _cards[cardIndex];
        return true;
    }
}