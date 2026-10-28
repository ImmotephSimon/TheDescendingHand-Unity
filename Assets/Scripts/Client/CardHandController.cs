using UnityEngine;

public class CardHandController : MonoBehaviour
{
    [SerializeField] private CardHandView handView;
    [SerializeField] private GameObject physicalCardPrefab;
    [SerializeField] private int maxHandSize = 10;

    private GameObject[] cards;

    private void Awake() => cards = new GameObject[maxHandSize];

    public void OnCardAdded(int index, CardVisuals visuals)
    {
        if ((uint)index >= cards.Length) return;

        if (cards[index] != null)          // server replaced a slot, don't leak the old one
            OnCardRemoved(index);

        GameObject cardObject = Instantiate(physicalCardPrefab, handView.transform);
        CardView view = cardObject.GetComponentInChildren<CardView>();
        view.InitializePhysicalCard(visuals, cardObject.transform, $"{index + 1}");

        cards[index] = cardObject;
        handView.AddCard(cardObject);
    }

    public void OnCardRemoved(int index)
    {
        if ((uint)index >= cards.Length || cards[index] == null) return;

        CardView view = cards[index].GetComponentInChildren<CardView>();
        handView.RemoveCard(view);
        cards[index] = null;
    }
}