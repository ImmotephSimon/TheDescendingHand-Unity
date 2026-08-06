using UnityEngine;
using UnityEngine.UI;

public class ItemIconView : MonoBehaviour
{
    [SerializeField] private Image image;

    public void Render(Sprite icon, Vector2 position, Vector2 size)
    {
        RectTransform rect = transform as RectTransform;

        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        image.sprite = icon;
    }
}