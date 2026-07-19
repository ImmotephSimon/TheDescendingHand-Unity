using System;

public class CardManager : ICardContainer
{
    
    private readonly Hand _hand = new();
    private readonly Deck _deck = new();
    private CardFactory _factory;

    public CardManager(CardFactory factory)
    {
        _factory = factory;
        InitializeCards();
    }
    private void InitializeCards()
    {
        _hand.AddCard(
            _factory.Create("Electrocute")
        );
    }

    public bool TryGetCardAtIndex(int cardIndex, out Card card)
    {
        return _hand.TryGetCardAtIndex(cardIndex, out card);
    }
}