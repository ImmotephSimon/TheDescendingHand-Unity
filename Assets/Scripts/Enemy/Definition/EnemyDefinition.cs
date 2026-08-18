using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyDefinition", menuName = "Enemies/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    public EnemyType EnemyType = EnemyType.Normal;

    [Header("Visuals")]
    public GameObject ModelPrefab;

    [Header("Attacks")]
    public List<EnemyAttackDefinition> Attacks;
}

public enum EnemyType { Normal, Magic, Rare, Unique }