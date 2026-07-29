using System.Collections.Generic;
using UnityEngine;

public class PileView : MonoBehaviour
{
    [SerializeField] private CardView cardTooltipPrefab;

    private readonly List<CardView> views = new();

    public void Show(IReadOnlyList<CardDefinition> cardDefinitions)
    {
        Clear();

        foreach (var def in cardDefinitions)
        {
            CardView tooltip = Instantiate(cardTooltipPrefab, transform);
            tooltip.Initialize(def);
            views.Add(tooltip);
        }
    }

    private void Clear()
    {
        foreach (var view in views)
            Destroy(view.gameObject);

        views.Clear();
    }
}