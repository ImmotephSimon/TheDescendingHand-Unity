using UnityEngine;
using UnityEngine.UI;

public class CardTooltipView : TooltipView
{
    [SerializeField] private Image iconImage;
    [SerializeField] private SpriteCatalog sprites;

    public override void Set(TooltipDto dto)
    {
        base.Set(dto);

        SpriteCatalog.Instance.TryGetSprite(dto.Icon, out Sprite icon);

        iconImage.gameObject.SetActive(icon != null);
        iconImage.sprite = icon;
    }
}