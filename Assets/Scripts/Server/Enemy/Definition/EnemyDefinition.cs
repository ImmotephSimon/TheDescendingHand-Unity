using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyDefinition", menuName = "Enemies/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    [SerializeField, HideInInspector] private string id;
    public Guid Id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty;

    public EnemyType EnemyType = EnemyType.Normal;

    [Header("Visuals")]
    public GameObject ModelPrefab;

    [Header("Attacks")]
    public List<EnemyAttackDefinition> Attacks;

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        string path = UnityEditor.AssetDatabase.GetAssetPath(this);
        if (!string.IsNullOrEmpty(path))
            id = UnityEditor.AssetDatabase.AssetPathToGUID(path);
    }
#endif

}

public enum EnemyType { Normal, Magic, Rare, Unique }