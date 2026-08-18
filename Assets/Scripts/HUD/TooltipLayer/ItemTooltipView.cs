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

        var equipComp = definition.Components
            .OfType<EquipComponentDefinition>()
            .FirstOrDefault();

        if (equipComp != null)
            SetTextSection(baseTypeText, equipComp.EquipmentType.name);

        SetTextSection(implicitsText, BuildImplicits(item));
        SetTextSection(explicitsText, BuildAffixes(item.Explicits));

        bool hasLore = !string.IsNullOrEmpty(definition.Lore);

        if (bottomSection != null)
            bottomSection.SetActive(hasLore);

        SetTextSection(loreText, definition.Lore);

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    private void SetTextSection(TextMeshProUGUI field, string content)
    {
        if (field == null)
            return;

        bool hasContent = !string.IsNullOrEmpty(content);
        field.gameObject.SetActive(hasContent);

        if (hasContent)
            field.text = content;
    }

    private void SetTextSection(TextMeshProUGUI field, string content, Color color)
    {
        if (field == null)
            return;

        bool hasContent = !string.IsNullOrEmpty(content);
        field.gameObject.SetActive(hasContent);

        if (hasContent)
        {
            field.text = content;
            field.color = color;
        }
    }

    private string BuildImplicits(ItemInstance item)
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

    private string GetRawImplicitText(ItemInstance item)
    {
        if (item?.BaseType?.Implicits == null)
            return string.Empty;

        var lines = new List<string>();

        foreach (var modifier in item.BaseType.Implicits)
        {
            if (!IsDisplayedAsWeaponStat(modifier.Stat))
                lines.Add(modifier.ToString());
        }

        return string.Join("\n", lines);
    }

    private bool IsDamageStat(GameTag stat)
    {
        if (stat == null)
            return false;

        if (stat == GameTags.ModOffenseDamage ||
            stat == GameTags.ModOffenseDamageMin ||
            stat == GameTags.ModOffenseDamageMax)
            return true;

        foreach (var damageType in GameTags.DamageTypes)
        {
            if (stat == damageType)
                return true;
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

    private string GetDamageText(ItemInstance item)
    {
        var ranges = new Dictionary<GameTag, (float Min, float Max)>();

        void AddModifier(GameTag stat, MathOp op, TagRequirement tags, float value)
        {
            Debug.Log($"Damage modifier: {stat} / {op} / {value} / damage={IsDamageStat(stat)}");

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

        if (item?.BaseType?.Implicits != null)
        {
            foreach (var modifier in item.BaseType.Implicits)
            {
                AddModifier(
                    modifier.Stat,
                    modifier.Op,
                    modifier.RequiredTags,
                    modifier.Value);
            }
        }

        if (item?.Explicits != null)
        {
            foreach (var affix in item.Explicits)
            {
                if (affix?.Definition == null)
                    continue;

                AddModifier(
                    affix.Definition.Modifier,
                    affix.Definition.MathOp,
                    affix.Definition.TagRequirement,
                    affix.Value);
            }
        }

        void ApplyMultiplier(GameTag stat, TagRequirement tags, float value)
        {
            if (!IsDamageStat(stat))
                return;

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

        if (item?.BaseType?.Implicits != null)
        {
            foreach (var modifier in item.BaseType.Implicits)
            {
                if (modifier.Op == MathOp.Multiplicative)
                {
                    ApplyMultiplier(
                        modifier.Stat,
                        modifier.RequiredTags,
                        modifier.Value);
                }
            }
        }

        if (item?.Explicits != null)
        {
            foreach (var affix in item.Explicits)
            {
                if (affix?.Definition == null ||
                    affix.Definition.MathOp != MathOp.Multiplicative)
                    continue;

                ApplyMultiplier(
                    affix.Definition.Modifier,
                    affix.Definition.TagRequirement,
                    affix.Value);
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
                lines.Add(
                    $"{GetDamageLabel(element)}: " +
                    $"<color={GetDamageColorHex(element)}>{min}-{max}</color>");
            }
        }

        return string.Join("\n", lines);
    }



    private string GetCritText(ItemInstance item)
    {
        float baseCrit = 0f;
        float critMultiplier = 1f;

        if (item?.BaseType?.Implicits != null)
        {
            foreach (var modifier in item.BaseType.Implicits)
            {
                if (modifier.Stat != GameTags.ModOffenseCritical)
                    continue;

                if (modifier.Op == MathOp.Added)
                    baseCrit += modifier.Value;
                else if (modifier.Op == MathOp.Multiplicative)
                    critMultiplier *= 1f + NormalizePercent(modifier.Value);
            }
        }

        if (item?.Explicits != null)
        {
            foreach (var affix in item.Explicits)
            {
                if (affix?.Definition == null ||
                    affix.Definition.Modifier != GameTags.ModOffenseCritical)
                    continue;

                if (affix.Definition.MathOp == MathOp.Added)
                    baseCrit += affix.Value;
                else if (affix.Definition.MathOp == MathOp.Multiplicative)
                    critMultiplier *= 1f + NormalizePercent(affix.Value);
            }
        }

        if (baseCrit <= 0f)
            return string.Empty;

        float finalCrit = baseCrit * critMultiplier;

        return $"Critical: {finalCrit:F2}%";
    }

    private string GetSpeedText(ItemInstance item)
    {
        float castSpeed = 0f;
        bool found = false;

        if (item?.BaseType?.Implicits != null)
        {
            foreach (var modifier in item.BaseType.Implicits)
            {
                if (modifier.Stat != GameTags.ModOffenseCastSpeed)
                    continue;

                castSpeed = modifier.Value;
                found = true;
                break;
            }
        }

        if (!found)
            return string.Empty;

        if (item.Explicits != null)
        {
            foreach (var affix in item.Explicits)
            {
                if (affix?.Definition == null ||
                    affix.Definition.Modifier != GameTags.ModOffenseCastSpeed)
                    continue;

                castSpeed *= 1f + NormalizePercent(affix.Value);
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
        if (element == GameTags.RestrictionElementFire)
            return "Fire Damage";

        if (element == GameTags.RestrictionElementCold)
            return "Cold Damage";

        if (element == GameTags.RestrictionElementLightning)
            return "Lightning Damage";

        if (element == GameTags.RestrictionChaos)
            return "Chaos Damage";

        return "Damage";
    }

    private string GetDamageColorHex(GameTag element)
    {
        if (element == GameTags.RestrictionElementFire)
            return "#FF5555";

        if (element == GameTags.RestrictionElementCold)
            return "#55FFFF";

        if (element == GameTags.RestrictionElementLightning)
            return "#FFFF55";

        if (element == GameTags.RestrictionChaos)
            return "#D355FF";

        return "#FFFFFF";
    }

    private string BuildAffixes(List<AffixInstance> affixes)
    {
        if (affixes == null || affixes.Count == 0)
            return string.Empty;

        var sorted = affixes
        .Where(a => a != null)
        .OrderBy(a => a.Definition.Slot);

        var lines = new List<string>();

        foreach (var affix in affixes)
        {
            var modifier = affix?.ToStatModifier();

            if (modifier != null)
                lines.Add(modifier.ToString());
        }

        return string.Join("\n", lines);
    }
}