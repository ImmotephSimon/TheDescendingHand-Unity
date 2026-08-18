using UnityEngine;

public class CardHandController : MonoBehaviour
{
    [SerializeField] private CardHandView handView;
    [SerializeField] private GameObject physicalCardPrefab;

    private GameObject[] cards = new GameObject[10];

    public void OnCardAdded(int index, string cardDefinitionId)
    {
        GameObject cardObject = Instantiate(physicalCardPrefab, handView.transform);

        CardView view = cardObject.GetComponentInChildren<CardView>();
        ClientBridge.Instance.CardRegistry.TryGet(cardDefinitionId, out CardDefinition def);
        view.InitializePhysicalCard(def, cardObject.transform, $"{index + 1}");

        cards[index] = cardObject;

        handView.AddCard(cardObject);
    }

    public void OnCardRemoved(int index)
    {
        CardView view = cards[index].GetComponentInChildren<CardView>();
        handView.RemoveCard(view);
        cards[index] = null;
    }
}