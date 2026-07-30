using System.Collections.Generic;
using UnityEngine;

public class PileView : MonoBehaviour
{
    [SerializeField] private CardView cardTooltipPrefab;
    private Camera cam;
    private readonly List<CardView> views = new();

    private void Awake()
    {
        cam = Camera.main;
    }
    public void Show(IReadOnlyList<CardDefinition> cardDefinitions)
    {
        Clear();

        for (int i = 0; i < cardDefinitions.Count; i++)
        {
            CardView tooltip = Instantiate(cardTooltipPrefab, cam.transform, false);

            float xOffset = (i - (cardDefinitions.Count - 1) * 0.5f) * 1.2f;

            // Place directly in front of the camera's local viewport (X offset, Y offset, Z distance)
            tooltip.transform.localPosition = new Vector3(xOffset, 0f, 2f);
            tooltip.transform.localRotation = Quaternion.identity;

            tooltip.Initialize(cardDefinitions[i], tooltip.transform);
            views.Add(tooltip);
        }
    }

    public void Clear()
    {
        foreach (var view in views)
            Destroy(view.gameObject);

        views.Clear();
    }
}