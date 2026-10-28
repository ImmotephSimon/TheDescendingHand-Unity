using System.Collections.Generic;
using UnityEngine;

public class CardHandView : MonoBehaviour
{
    private List<CardView> cards = new();
    private float spacing = 0.15f;
    private float fanAngle = 30f;
    private float moveSpeed = 12f;

    private void Update()
    {
        UpdateLayout();
        MoveCards();
    }

    private void UpdateLayout()
    {
        if (cards.Count == 0) return;

        int count = cards.Count;

        for (int i = 0; i < count; i++)
        {
            float centerOffset = i - (count - 1) * 0.5f;

            Vector3 position = new(
                centerOffset * spacing,
                0,
                i * -0.02f
            );

            Quaternion rotation = Quaternion.Euler(
                0,
                0,
                centerOffset * -fanAngle
            );

            cards[i].SetHandTransform(position, rotation);
        }
    }

    private void MoveCards()
    {
        foreach (var card in cards)
            card.MoveTowardsHandTransform(moveSpeed);
    }

    public void AddCard(GameObject physicalCard)
    {
        CardView view = physicalCard.GetComponentInChildren<CardView>();
        cards.Add(view);
        view.UpdateParent(transform);
        UpdateLayout();
    }

    public void RemoveCard(CardView card)
    {
        cards.Remove(card);
        card.DestroyCard();
        UpdateLayout();
    }
}