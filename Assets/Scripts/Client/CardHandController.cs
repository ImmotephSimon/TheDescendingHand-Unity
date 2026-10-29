using UnityEngine;

public class CardHandController : MonoBehaviour
{
    [SerializeField] private CardHandView handView;
    [SerializeField] private GameObject physicalCardPrefab;
    [SerializeField] private int maxHandSize = 10;

    private GameObject[] cards;
    private CardNetwork _network;

    private void Awake()
    {
        cards = new GameObject[maxHandSize];

        // Camera.main predates ClientRoot.Instance
        ClientRoot.OnInstanceReady += Bind;
    }


    private void Bind(ClientRoot root)
    {
        ClientRoot.Instance.OnClientPlayerReady += Initialize;
    }

    private void Initialize(ClientPlayer client)
    {
        _network = client.GetComponent<CardNetwork>();
        Debug.Assert(_network != null, $"[{name}] CardHandController requires a CardNetwork", this);

        _network.OnCardSlotChanged += OnCardSlotChanged;
    }

    private void OnDisable()
    {
        ClientRoot.OnInstanceReady -= Bind;
        _network.OnCardSlotChanged -= OnCardSlotChanged;
    }

    public void OnCardSlotChanged(int index, CardVisuals visuals)
    {
        if ((uint)index >= cards.Length) return;

        // Clean up existing card if present
        if (cards[index] != null)
        {
            CardView view = cards[index].GetComponentInChildren<CardView>();
            handView.RemoveCard(view);
            cards[index] = null;
        }

        // If the new state is empty, we're done
        if (visuals.IsEmpty) return;

        // Otherwise, instantiate and setup the new card
        GameObject cardObject = Instantiate(physicalCardPrefab, handView.transform);
        CardView newView = cardObject.GetComponentInChildren<CardView>();
        newView.InitializePhysicalCard(visuals, cardObject.transform, $"{index + 1}");

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