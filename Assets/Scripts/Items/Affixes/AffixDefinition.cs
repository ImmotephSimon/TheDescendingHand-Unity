using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAffixDefinition", menuName = "Items/Affix Definition")]
public class AffixDefinition : ScriptableObject
{
    [SerializeField, HideInInspector]
    private string id;
    public string NameOverride;
    public List<StatModifier> Mods;
    [SerializeField]
    public TagRequirement ContextRequirement;
    public List<GameTag> Prerequisites;
    public List<GameTag> Tags;
    public AffixSlot Slot { get; set; }

    public string Id => id;


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
            id = System.Guid.NewGuid().ToString();
    }
#endif
}