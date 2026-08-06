using System;

public class AffixInstance
{
    public AffixDefinition Definition;
    public float Roll = 1f;

    public StatModifier ToStatModifier()
    {
        var tags = Definition.Restriction is TagRestriction tagRestriction
                ? tagRestriction.Tags
                : TagContainer.Empty;

        return new StatModifier(
            Definition.Modifier,
            Definition.MathOp,
            Definition.BaseValue * Roll,
            tags,
            ModifierSource.Explicit);
    }
}