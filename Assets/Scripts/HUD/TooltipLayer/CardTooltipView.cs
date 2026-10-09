using UnityEngine;
using UnityEngine.UI;

public class CardTooltipView : TooltipView
{
    [SerializeField] private Image iconImage;

    public override void Set(TooltipDto dto)
    {
        base.Set(dto);

        ItemRegistry.Instance.TryGetIcon(dto.Id, out Sprite icon);

        iconImage.gameObject.SetActive(icon != null);
        iconImage.sprite = icon;
    }
}