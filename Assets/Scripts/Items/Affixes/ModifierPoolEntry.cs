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
    public AffixSlot Slot;
    public GameTag Modifier;
    public TagRequirement TagRequirement;

    public ModifierPoolEntry()
    {
        Weight = 100f;
        Slot = AffixSlot.Prefix;
        Modifier = GameTag.Empty;
    }
}