public enum AffixSlot
{
    Prefix,
    Suffix,
    Implicit
}
[System.Serializable]
public class ModifierPoolEntry
{
    public AffixDefinition Definition;
    public float Weight = 100f;

    public ModifierPoolEntry(AffixDefinition definition, float weight = 100f)
    {
        Definition = definition;
        Weight = weight;
    }
}