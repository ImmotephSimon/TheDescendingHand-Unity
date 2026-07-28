using UnityEngine;

[CreateAssetMenu(fileName = "NewAffixDefinition", menuName = "Items/Affix Definition")]
public class AffixDefinition : ScriptableObject
{
    [SerializeField, HideInInspector]
    private string id;

    public GameTag Modifier;
    public MathOp MathOp;
    public Restriction Restriction;

    public string Id => id;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
            id = System.Guid.NewGuid().ToString();
    }
#endif
}