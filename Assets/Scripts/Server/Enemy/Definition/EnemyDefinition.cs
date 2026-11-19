using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyDefinition", menuName = "Enemies/Enemy Definition")]
public class EnemyDefinition : ScriptableObject, IWorldModelSource
{
    [SerializeField, HideInInspector] private string id;
    public Guid Id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty;

    [Header("Visuals")]
    [SerializeField] private GameObject worldModel;
    [SerializeField] private HitBoxSampleData hitBoxData = new();


    public GameObject WorldModel => worldModel;
    public HitBoxSampleData HitBoxSampleData => hitBoxData;

    [Header("Attacks")]
    public List<EnemyAttackDefinition> Attacks;

    public EnemyType EnemyType = EnemyType.Normal;

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        string path = UnityEditor.AssetDatabase.GetAssetPath(this);
        if (!string.IsNullOrEmpty(path))
            id = UnityEditor.AssetDatabase.AssetPathToGUID(path);
    }
#endif

}

