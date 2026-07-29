using System;

public interface ICardContainer
{
    bool TryGetCardAtIndex(int cardIndex, out Card card);
    int Capacity { get; }

    event Action<int, Card> OnCardAdded;
    event Action<int, Card> OnCardRemoved;
}