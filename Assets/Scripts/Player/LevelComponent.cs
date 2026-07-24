using UnityEngine;

public class LevelComponent : MonoBehaviour
{
    [SerializeField] private int level = 1;
    [SerializeField] private float baseHealth = 20;
    [SerializeField] private float healthPerLevel = 2;
    [SerializeField] private GameTag healthTag;

    private IStatContainer stats;
    

    private void Awake()
    {
        stats = GetComponent<IStatContainer>();
        ApplyLevelStats();
    }

    public void LevelUp()
    {
        level++;
        ApplyLevelStats();
    }

    private void ApplyLevelStats()
    {
        stats.AddModifier(new StatModifier(
            healthTag,
            ModifierType.Flat,
            baseHealth + (level - 1) * healthPerLevel
        ));
    }
}