using System;

public interface ICardContainer
{
    bool TryGetCardAtIndex(int cardIndex, out Card card);
}