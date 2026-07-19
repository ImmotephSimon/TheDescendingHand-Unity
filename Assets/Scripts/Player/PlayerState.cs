using UnityEngine.XR;

public class PlayerState
{
    private CardManager _cardManager;

    public Deck Deck { get; }
    public Hand Hand { get; }
    public ICardContainer CardManager => _cardManager;

    public PlayerState(CardFactory factory)
    {
        Deck = new Deck();
        Hand = new Hand();
        _cardManager = new CardManager(factory);
    }
}