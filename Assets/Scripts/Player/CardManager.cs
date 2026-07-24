using System;
using System.Collections.Generic;
using System.Linq;

public class CardManager : ICardContainer
{
    private readonly Card[] _hand;
    private readonly List<Card> _drawPile = new();
    private readonly List<Card> _discard = new();
    private readonly Random _random = new();

    public CardManager(IEnumerable<Card> startingCards, int handSize = 5)
    {
        _hand = new Card[handSize];

        _drawPile.AddRange(startingCards);
        ShuffleDrawPile();

        DrawHand();
    }

    public IReadOnlyList<Card> Hand => _hand;

    public void DrawHand()
    {
        for (int i = 0; i < _hand.Length; i++)
        {
            if (_drawPile.Count == 0)
                break;

            DrawCard();
        }
    }

    public void AddToDrawPile(Card card) => _drawPile.Add(card);

    public bool AddToHand(Card card)
    {
        for (int i = 0; i < _hand.Length; i++)
        {
            if (_hand[i] == null)
            {
                _hand[i] = card;
                return true;
            }
        }
        return false;
    }

    public Card DrawCard()
    {
        int emptySlot = FindEmptyHandSlot();

        if (emptySlot == -1)
            throw new InvalidOperationException("Cannot draw card: hand is full.");

        if (_drawPile.Count == 0)
            throw new InvalidOperationException("Cannot draw card: no cards available.");

        Card drawnCard = _drawPile[0];
        _drawPile.RemoveAt(0);

        _hand[emptySlot] = drawnCard;

        return drawnCard;
    }

    public bool IsHandFull => _hand.All(card => card != null);

    public bool IsHandEmpty => _hand.All(card => card == null);

    private int FindEmptyHandSlot()
    {
        return Array.FindIndex(_hand, card => card == null);
    }

    public void DiscardCardInHand(int index)
    {
        if (_hand[index] == null)
            throw new InvalidOperationException("No card to discard.");

        _discard.Add(_hand[index]);
        _hand[index] = null;

        if (IsHandEmpty)
            ResetHand();
    }

    private void ResetHand()
    {
        _drawPile.AddRange(_discard);
        _discard.Clear();

        ShuffleDrawPile();
        DrawHand();
    }

    public bool TryGetCardAtIndex(int index, out Card card)
    {
        if (index >= 0 && index < _hand.Length && _hand[index] != null)
        {
            card = _hand[index];
            return true;
        }

        card = null;
        return false;
    }

    private void ShuffleDrawPile()
    {
        for (int i = _drawPile.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);

            (_drawPile[i], _drawPile[j]) = (_drawPile[j], _drawPile[i]);
        }
    }
}