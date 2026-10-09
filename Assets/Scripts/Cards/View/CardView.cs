using System.Collections.Generic;
using UnityEngine;

public class CardView : MonoBehaviour
{
    [SerializeField] private CardTooltipView tooltip;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private CardDefinition definition;
    private Transform root;

    public void SetHandTransform(Vector3 position, Quaternion rotation)
    {
        targetPosition = position;
        targetRotation = rotation;
    }

    public void MoveTowardsHandTransform(float speed)
    {
        float t = speed * Time.deltaTime;

        root.localPosition = Vector3.Lerp(root.localPosition, targetPosition, t);
        root.localRotation = Quaternion.Lerp(root.localRotation, targetRotation, t);
    }

    public void InitializeTooltipCard(CardDefinition definition)
    {
        this.definition = definition;
        tooltip.Set(BuildDto(definition, null));
    }

    public void InitializePhysicalCard(CardDefinition definition, Transform root, string keybindText)
    {
        this.definition = definition;
        this.root = root;
        tooltip.Set(BuildDto(definition, keybindText));
    }

    private static TooltipDto BuildDto(CardDefinition definition, string keybindText)
    {
        CardVisuals visuals = definition.Visuals;

        return new TooltipDto(
            layout: TooltipLayout.Card,
            title: visuals.Name,
            id: definition.Id,
            header: string.IsNullOrEmpty(keybindText)
                ? null
                : new List<string> { keybindText },
            body: string.IsNullOrEmpty(visuals.Description)
                ? null
                : new List<string> { visuals.Description }
        );
    }

    public void UpdateParent(Transform parent)
    {
        root.SetParent(parent, true);
        root.localScale = Vector3.one * 0.2f;
    }

    public void DestroyCard()
    {
        Destroy(root.gameObject);
    }

    // hover, selection, highlight, etc. stay here
}