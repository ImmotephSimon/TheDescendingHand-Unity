using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Zones")]
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private TextMeshProUGUI footerText;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public virtual void Set(TooltipDto dto)
    {
        SetText(titleText, dto.Title);
        SetZone(headerText, dto.Header);
        SetZone(bodyText, dto.Body);
        SetZone(footerText, dto.Footer);

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    private static void SetZone(TextMeshProUGUI field, List<string> lines)
    {
        SetText(field, lines == null ? null : string.Join("\n", lines));
    }

    private static void SetText(TextMeshProUGUI field, string content)
    {
        if (field == null) return;

        bool hasContent = !string.IsNullOrEmpty(content);
        field.gameObject.SetActive(hasContent);

        if (hasContent)
            field.text = content;
    }
}