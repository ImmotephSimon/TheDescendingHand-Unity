using UnityEngine;

public class PlayerState
{
    private readonly CardManager _cardManager;

    public ICardContainer CardManager => _cardManager;

    public PlayerState(IEntity owner,
        CardFactory factory,
        CardRegistry registry
        )
    {
        
        CardDefinition definition = registry.GetRandomCard();
        Card card = factory.CreateFromDefinition(definition, owner);
        
        _cardManager = new CardManager(new Card[] { card }, handSize: 5);
    }
}