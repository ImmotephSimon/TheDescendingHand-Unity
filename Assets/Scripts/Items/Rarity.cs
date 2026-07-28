using UnityEngine;

[CreateAssetMenu(menuName = "Items/Rarity")]
public class Rarity : ScriptableObject
{
    public string DisplayName;

    public float DropWeight;

    public Color DisplayColor;

    public int MaxAffixes;

    public int MinAffixes;

    public bool CanHaveUniqueMods;
}