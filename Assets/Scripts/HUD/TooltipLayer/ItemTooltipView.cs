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

    public void SetItem(ItemTooltipDto item)
    {
        ItemRegistry.Instance.TryGetDefinition(item.BaseTypeId, out ItemDefinition definition);
        ItemRegistry.Instance.TryGetRarity(item.RarityId, out Rarity rarity);

        SetTextSection(nameText, definition.DisplayName, rarity.DisplayColor);

        var equipComp = GetEquipComponent(definition);
        if (equipComp != null)
        {
            SetTextSection(baseTypeText, equipComp.EquipmentType.name);
        }

        SetTextSection(implicitsText, BuildImplicits(item));
        SetTextSection(explicitsText, AffixesToString(item.Explicits));

        bool hasLore = !string.IsNullOrEmpty(definition.Lore);
        if (bottomSection != null)
        {
            bottomSection.SetActive(hasLore);
        }

        SetTextSection(loreText, definition.Lore);

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public EquipComponentDefinition GetEquipComponent(ItemDefinition definition)
    {
        return definition.Components
            .OfType<EquipComponentDefinition>()
            .FirstOrDefault();
    }

    private void SetTextSection(TextMeshProUGUI field, string content)
    {
        if (field == null) return;

        bool hasContent = !string.IsNullOrEmpty(content);
        field.gameObject.SetActive(hasContent);

        if (hasContent)
            field.text = content;
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

    private string BuildImplicits(ItemTooltipDto item)
    {
        var lines = new List<string>();

        string damage = GetDamageText(item);
        if (!string.IsNullOrEmpty(damage))
            lines.Add(damage);

        string rawImplicits = GetRawImplicitText(item);
        if (!string.IsNullOrEmpty(rawImplicits))
            lines.Add(rawImplicits);

        string crit = GetCritText(item);
        if (!string.IsNullOrEmpty(crit))
            lines.Add(crit);

        string speed = GetSpeedText(item);
        if (!string.IsNullOrEmpty(speed))
            lines.Add(speed);

        return string.Join("\n", lines);
    }

    private string GetRawImplicitText(ItemTooltipDto item)
    {
        if (item.Implicits == null)
            return string.Empty;

        var lines = new List<string>();

        foreach (var affix in item.Implicits)
        {
            if (!IsDisplayedAsWeaponStat(affix.Modifier))
                lines.Add(new StatModifier(affix.Modifier, affix.MathOp, affix.RolledValue, affix.TagRequirement).ToString());
        }

        return string.Join("\n", lines);
    }

    private bool IsDamageStat(GameTag stat)
    {
        if (stat == null) return false;

        if (stat == GameTags.ModOffenseDamage ||
            stat == GameTags.ModOffenseDamageMin ||
            stat == GameTags.ModOffenseDamageMax)
            return true;

        foreach (var damageType in GameTags.DamageTypes)
        {
            if (stat == damageType) return true;
        }

        return false;
    }

    private bool IsDisplayedAsWeaponStat(GameTag stat)
    {
        return IsDamageStat(stat) ||
               stat == GameTags.ModOffenseCritical ||
               stat == GameTags.ModOffenseCastSpeed;
    }

    private List<GameTag> GetElementTags(TagRequirement requirement, GameTag stat)
    {
        var elements = new List<GameTag>();

        foreach (var element in GameTags.DamageTypes)
        {
            if (requirement.Tags != null && requirement.Tags.HasTag(element))
                elements.Add(element);
            else if (stat == element)
                elements.Add(element);
        }

        return elements;
    }

    private string GetDamageText(ItemTooltipDto item)
    {
        var ranges = new Dictionary<GameTag, (float Min, float Max)>();

        void AddModifier(GameTag stat, MathOp op, TagRequirement tags, float value)
        {
            if (!IsDamageStat(stat) || op != MathOp.Added)
                return;

            var elements = GetElementTags(tags, stat);
            if (elements.Count == 0)
                elements.Add(GameTags.RestrictionPhysical);

            foreach (var element in elements)
            {
                var range = ranges.GetValueOrDefault(element);

                if (stat == GameTags.ModOffenseDamageMin)
                    range.Min += value;
                else if (stat == GameTags.ModOffenseDamageMax)
                    range.Max += value;
                else
                {
                    range.Min += value;
                    range.Max += value;
                }

                ranges[element] = range;
            }
        }

        if (item.Implicits != null)
        {
            foreach (var affix in item.Implicits)
                AddModifier(affix.Modifier, affix.MathOp, affix.TagRequirement, affix.RolledValue);
        }

        if (item.Explicits != null)
        {
            foreach (var affix in item.Explicits)
                AddModifier(affix.Modifier, affix.MathOp, affix.TagRequirement, affix.RolledValue);
        }

        void ApplyMultiplier(GameTag stat, TagRequirement tags, float value)
        {
            if (!IsDamageStat(stat)) return;

            var elements = GetElementTags(tags, stat);
            if (elements.Count == 0)
                elements.Add(GameTags.RestrictionPhysical);

            float multiplier = 1f + NormalizePercent(value);

            foreach (var element in elements)
            {
                if (!ranges.TryGetValue(element, out var range))
                    continue;

                range.Min *= multiplier;
                range.Max *= multiplier;
                ranges[element] = range;
            }
        }

        if (item.Implicits != null)
        {
            foreach (var affix in item.Implicits)
            {
                if (affix.MathOp == MathOp.Multiplicative)
                    ApplyMultiplier(affix.Modifier, affix.TagRequirement, affix.RolledValue);
            }
        }

        if (item.Explicits != null)
        {
            foreach (var affix in item.Explicits)
            {
                if (affix.MathOp == MathOp.Multiplicative)
                    ApplyMultiplier(affix.Modifier, affix.TagRequirement, affix.RolledValue);
            }
        }

        var lines = new List<string>();

        foreach (var (element, range) in ranges)
        {
            int min = Mathf.RoundToInt(range.Min);
            int max = Mathf.RoundToInt(range.Max);

            if (element == GameTags.RestrictionPhysical)
            {
                lines.Add($"Damage: {min}-{max}");
            }
            else
            {
                lines.Add($"{GetDamageLabel(element)}: <color={GetDamageColorHex(element)}>{min}-{max}</color>");
            }
        }

        return string.Join("\n", lines);
    }

    private string GetCritText(ItemTooltipDto item)
    {
        float baseCrit = 0f;
        float critMultiplier = 1f;

        void ProcessAffix(GameTag stat, MathOp op, float value)
        {
            if (stat != GameTags.ModOffenseCritical) return;

            if (op == MathOp.Added)
                baseCrit += value;
            else if (op == MathOp.Multiplicative)
                critMultiplier *= 1f + NormalizePercent(value);
        }

        if (item.Implicits != null)
        {
            foreach (var affix in item.Implicits)
                ProcessAffix(affix.Modifier, affix.MathOp, affix.RolledValue);
        }

        if (item.Explicits != null)
        {
            foreach (var affix in item.Explicits)
                ProcessAffix(affix.Modifier, affix.MathOp, affix.RolledValue);
        }

        if (baseCrit <= 0f) return string.Empty;

        float finalCrit = baseCrit * critMultiplier;
        return $"Critical: {finalCrit:F2}%";
    }

    private string GetSpeedText(ItemTooltipDto item)
    {
        float castSpeed = 0f;
        bool found = false;

        if (item.Implicits != null)
        {
            foreach (var affix in item.Implicits)
            {
                if (affix.Modifier != GameTags.ModOffenseCastSpeed) continue;

                castSpeed = affix.RolledValue;
                found = true;
                break;
            }
        }

        if (!found) return string.Empty;

        if (item.Explicits != null)
        {
            foreach (var affix in item.Explicits)
            {
                if (affix.Modifier != GameTags.ModOffenseCastSpeed) continue;

                castSpeed *= 1f + NormalizePercent(affix.RolledValue);
            }
        }

        return $"Cast Speed: {castSpeed:F2}";
    }

    private float NormalizePercent(float value)
    {
        return value > 1f ? value / 100f : value;
    }

    private string GetDamageLabel(GameTag element)
    {
        if (element == GameTags.RestrictionElementFire) return "Fire Damage";
        if (element == GameTags.RestrictionElementCold) return "Cold Damage";
        if (element == GameTags.RestrictionElementLightning) return "Lightning Damage";
        if (element == GameTags.RestrictionChaos) return "Chaos Damage";
        return "Damage";
    }

    private string GetDamageColorHex(GameTag element)
    {
        if (element == GameTags.RestrictionElementFire) return "#FF5555";
        if (element == GameTags.RestrictionElementCold) return "#55FFFF";
        if (element == GameTags.RestrictionElementLightning) return "#FFFF55";
        if (element == GameTags.RestrictionChaos) return "#D355FF";
        return "#FFFFFF";
    }

    private string AffixesToString(List<AffixState> states)
    {
        if (states == null || states.Count == 0) return string.Empty;
        return string.Join("\n", states.Select(s =>
            new StatModifier(s.Modifier, s.MathOp, s.RolledValue, s.TagRequirement).ToString()));
    }
}