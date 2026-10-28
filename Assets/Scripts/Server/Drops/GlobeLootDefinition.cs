[System.Serializable]
public class GlobeLootDefinition : LootDefinition
{
    public float HealPercentage;

    public override void Initialize(WorldDrop drop, Rarity rarity)
    {
        if (drop is GlobeDrop globeDrop)
            globeDrop.Initialize(HealPercentage);
    }
}