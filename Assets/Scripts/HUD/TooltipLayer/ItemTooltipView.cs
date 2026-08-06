using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemTooltipView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI baseTypeText;
    [SerializeField] private TextMeshProUGUI implicitsText;
    [SerializeField] private TextMeshProUGUI explicitsText;
    [SerializeField] private TextMeshProUGUI loreText;

    [Header("Optional Section Wrappers")]
    [SerializeField] private GameObject bottomSection;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetItem(ItemInstance item)
    {
        var definition = item.BaseType;

        SetTextSection(nameText, definition.DisplayName, item.Rarity.DisplayColor);
        var equipComp = definition.Components.OfType<EquipComponentDefinition>().FirstOrDefault();
        if (equipComp != null) SetTextSection(baseTypeText, equipComp.EquipmentType.name);
        SetTextSection(implicitsText, BuildImplicits(definition.Implicits));
        SetTextSection(explicitsText, BuildAffixes(item.Affixes));

        bool hasLore = !string.IsNullOrEmpty(definition.Lore);
        if (bottomSection != null)
        {
            bottomSection.SetActive(hasLore);
        }
        SetTextSection(loreText, definition.Lore);

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    private void SetTextSection(TextMeshProUGUI field, string content)
    {
        if (field == null) return;

        bool hasContent = !string.IsNullOrEmpty(content);
        field.gameObject.SetActive(hasContent);
        if (hasContent)
        {
            field.text = content;
        }
    }
    private void SetTextSection(TextMeshProUGUI field, string content, Color color)
    {
        if (field == null) return;

        bool hasContent = !string.IsNullOrEmpty(content);
        field.gameObject.SetActive(hasContent);
        if (hasContent)
        {
            field.text = content;
            field.color = color;
        }
    }

    private string BuildImplicits(List<StatModifier> implicits)
    {
        if (implicits == null || implicits.Count == 0) return string.Empty;

        var sb = new System.Text.StringBuilder();
        foreach (var mod in implicits)
        {
            sb.AppendLine(mod.ToString());
        }
        return sb.ToString().TrimEnd();
    }

    private string BuildAffixes(List<AffixInstance> affixes)
    {
        if (affixes == null || affixes.Count == 0) return string.Empty;

        var lines = new List<string>();
        foreach (var affix in affixes)
        {
            var mod = affix?.ToStatModifier();
            if (mod != null) lines.Add(mod.ToString());
        }

        return string.Join("\n", lines);
    }
}