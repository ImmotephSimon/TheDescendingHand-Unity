using UnityEngine;

[CreateAssetMenu(fileName = "NewAffixDefinition", menuName = "Items/Affix Definition")]
public class AffixDefinition : ScriptableObject
{
    [SerializeField, HideInInspector]
    private string id;
    public string DisplayName;
    public float BaseValue;
    public GameTag Modifier;
    public MathOp MathOp;
    [SerializeReference, SerializeReferenceDropdown]
    public Restriction Restriction;

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