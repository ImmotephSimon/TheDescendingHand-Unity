using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ItemTooltipBuilder
{
    public static TooltipDto Build(ItemDropInstance item)
    {
        var equipComp = GetEquipComponent(item.BaseType);

        EquipmentTypeDefinition equipmentType = null;
        if (equipComp != null)
            ServerRoot.Instance.EquipmentTypes.TryGet(equipComp.Slot, out equipmentType);

        var data = new ItemTooltipDto
        {
            EquipmentSlot = equipComp.Slot,
            BaseTypeId = item.BaseType.Id,
            RarityId = item.Rarity.Id,

            Implicits = item.BaseType.Implicits.Select(x => x.Modifier)
                .Select(AffixState.FromModifier)
                .ToList(),

            Explicits = item.Explicits
                .Select(affix => AffixState.FromInstance(affix, item.BaseType.Implicits))
                .ToList()
        };

        // Title = name (rarity colored) + base type on a second line
        string colorHex = ColorUtility.ToHtmlStringRGB(item.Rarity.DisplayColor);
        string title = $"<color=#{colorHex}>{item.BaseType.DisplayName}</color>";

        if (equipmentType != null)
            title += $"\n<size=70%>{equipmentType.name}</size>";

        var header = new List<string>();
        AddIfNotEmpty(header, GetDamageText(data));
        AddIfNotEmpty(header, GetRawImplicitText(data));
        AddIfNotEmpty(header, GetCritText(data));
        AddIfNotEmpty(header, GetSpeedText(data));

        var body = new List<string>();
        AddIfNotEmpty(body, AffixesToString(data.Explicits));

        var footer = new List<string>();
        AddIfNotEmpty(footer, item.BaseType.Lore);

        return new TooltipDto(
            icon: item.Icon,
            layout: TooltipLayout.Equipment,
            title: title,
            header: header,
            body: body,
            footer: footer);
    }

    private static void AddIfNotEmpty(List<string> list, string text)
    {
        if (!string.IsNullOrEmpty(text))
            list.Add(text);
    }

    public static EquipComponentDefinition GetEquipComponent(ItemDefinition definition)
    {
        return definition.Components
            .OfType<EquipComponentDefinition>()
            .FirstOrDefault();
    }

    private static string GetRawImplicitText(ItemTooltipDto item)
    {
        if (item.Implicits == null)
            return string.Empty;

        var lines = new List<string>();

        foreach (var affix in item.Implicits)
        {
            if (affix.ResolvedMods == null) continue;

            foreach (var mod in affix.ResolvedMods)
            {
                if (!IsDisplayedAsWeaponStat(mod.Stat))
                    lines.Add(mod.ToString());
            }
        }

        return string.Join("\n", lines);
    }

    private static bool IsDamageStat(GameTag stat)
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

    private static bool IsDisplayedAsWeaponStat(GameTag stat)
    {
        return IsDamageStat(stat) ||
               stat == GameTags.ModOffenseCritical ||
               stat == GameTags.ModOffenseCastSpeed;
    }

    private static List<GameTag> GetElementTags(TagRequirement requirement, GameTag stat)
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

    private static string GetDamageText(ItemTooltipDto item)
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
            {
                if (affix.ResolvedMods == null) continue;
                foreach (var mod in affix.ResolvedMods)
                    AddModifier(mod.Stat, mod.Op, mod.RequiredTags, mod.Value);
            }
        }

        if (item.Explicits != null)
        {
            foreach (var affix in item.Explicits)
            {
                if (affix.ResolvedMods == null) continue;
                foreach (var mod in affix.ResolvedMods)
                    AddModifier(mod.Stat, mod.Op, mod.RequiredTags, mod.Value);
            }
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
                if (affix.ResolvedMods == null) continue;
                foreach (var mod in affix.ResolvedMods)
                {
                    if (mod.Op == MathOp.Multiplicative)
                        ApplyMultiplier(mod.Stat, mod.RequiredTags, mod.Value);
                }
            }
        }

        if (item.Explicits != null)
        {
            foreach (var affix in item.Explicits)
            {
                if (affix.ResolvedMods == null) continue;
                foreach (var mod in affix.ResolvedMods)
                {
                    if (mod.Op == MathOp.Multiplicative)
                        ApplyMultiplier(mod.Stat, mod.RequiredTags, mod.Value);
                }
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

    private static string GetCritText(ItemTooltipDto item)
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
            {
                if (affix.ResolvedMods == null) continue;
                foreach (var mod in affix.ResolvedMods)
                    ProcessAffix(mod.Stat, mod.Op, mod.Value);
            }
        }

        if (item.Explicits != null)
        {
            foreach (var affix in item.Explicits)
            {
                if (affix.ResolvedMods == null) continue;
                foreach (var mod in affix.ResolvedMods)
                    ProcessAffix(mod.Stat, mod.Op, mod.Value);
            }
        }

        if (baseCrit <= 0f) return string.Empty;

        float finalCrit = baseCrit * critMultiplier;
        return $"Critical: {finalCrit:F2}%";
    }

    private static string GetSpeedText(ItemTooltipDto item)
    {
        float castSpeed = 0f;
        bool found = false;

        if (item.Implicits != null)
        {
            foreach (var affix in item.Implicits)
            {
                if (affix.ResolvedMods == null) continue;

                foreach (var mod in affix.ResolvedMods)
                {
                    if (mod.Stat != GameTags.ModOffenseCastSpeed) continue;

                    castSpeed = mod.Value;
                    found = true;
                    break;
                }

                if (found) break;
            }
        }

        if (!found) return string.Empty;

        if (item.Explicits != null)
        {
            foreach (var affix in item.Explicits)
            {
                if (affix.ResolvedMods == null) continue;

                foreach (var mod in affix.ResolvedMods)
                {
                    if (mod.Stat != GameTags.ModOffenseCastSpeed) continue;

                    castSpeed *= 1f + NormalizePercent(mod.Value);
                }
            }
        }

        return $"Cast Speed: {castSpeed:F2}";
    }

    private static float NormalizePercent(float value)
    {
        return value > 1f ? value / 100f : value;
    }

    private static string GetDamageLabel(GameTag element)
    {
        if (element == GameTags.RestrictionElementFire) return "Fire Damage";
        if (element == GameTags.RestrictionElementCold) return "Cold Damage";
        if (element == GameTags.RestrictionElementLightning) return "Lightning Damage";
        if (element == GameTags.RestrictionChaos) return "Chaos Damage";
        return "Damage";
    }

    private static string GetDamageColorHex(GameTag element)
    {
        if (element == GameTags.RestrictionElementFire) return "#FF5555";
        if (element == GameTags.RestrictionElementCold) return "#55FFFF";
        if (element == GameTags.RestrictionElementLightning) return "#FFFF55";
        if (element == GameTags.RestrictionChaos) return "#D355FF";
        return "#FFFFFF";
    }

    private static string AffixesToString(List<AffixState> states)
    {
        if (states == null || states.Count == 0) return string.Empty;
        return string.Join("\n", states
            .Where(s => s.ResolvedMods != null)
            .SelectMany(s => s.ResolvedMods)
            .Select(m => m.ToString()));
    }
}