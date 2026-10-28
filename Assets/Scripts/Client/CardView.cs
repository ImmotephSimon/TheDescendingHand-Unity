using System.Collections.Generic;
using UnityEngine;

public class CardView : MonoBehaviour
{
    [SerializeField] private CardTooltipView tooltip;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
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

    public void InitializeTooltipCard(CardVisuals visuals)
    {
        tooltip.Set(BuildDto(visuals, null));
    }

    public void InitializePhysicalCard(CardVisuals visuals, Transform root, string keybindText)
    {
        this.root = root;
        tooltip.Set(BuildDto(visuals, keybindText));
    }

    private static TooltipDto BuildDto(CardVisuals visuals, string keybindText)
    {

        return new TooltipDto(
            layout: TooltipLayout.Card,
            title: visuals.Name,
            icon: visuals.Icon,
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