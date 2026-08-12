using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyDefinition", menuName = "Enemies/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    [Header("Visuals")]
    public GameObject ModelPrefab;

    [Header("Attacks")]
    public List<EnemyAttackDefinition> Attacks;
}