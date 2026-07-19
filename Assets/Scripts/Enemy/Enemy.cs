using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyBrain))]
[RequireComponent(typeof(AbilityManager))]
[RequireComponent(typeof(Perception))]
public class Enemy : MonoBehaviour
{
    private EnemyBrain brain;
    private AbilityManager abilityManager;
    private Perception perception;
    [SerializeField] private float patrolRadius = 10f;
    private void Awake()
    {
        brain = GetComponent<EnemyBrain>();
        abilityManager = GetComponent<AbilityManager>();
        perception = GetComponent<Perception>();

        RegisterActions();
    }
    private void RegisterActions()
    {
        brain.AddAction(new ChaseAction(this));
        brain.AddAction(new PatrolAction(this, patrolRadius));
        foreach (var ability in GetComponents<IAttackAbility>())
        {
            brain.AddAction(new AttackAction(this, ability));
        }
    }
    private void OnEnable()
    {
        Debug.Log($"[Enemy Enable] {name}", this);
    }

    private void OnDisable()
    {
        Debug.Log($"[Enemy Disable] {name}", this);
    }
}