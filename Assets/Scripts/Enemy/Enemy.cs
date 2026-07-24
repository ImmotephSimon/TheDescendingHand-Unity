using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyBrain))]
[RequireComponent(typeof(AbilityManager))]
[RequireComponent(typeof(Perception))]
public class Enemy : MonoBehaviour, IEntity
{
    private EnemyBrain brain;
    private AbilityManager abilityManager;
    private Perception perception;
    private IAnimationHandler animationHandler;
    private IStatContainer stats;
    [SerializeField] private float patrolRadius = 10f;

    public Transform Transform => transform;

    public int TeamLayer => gameObject.layer;

    public bool IsDead { get; private set; }

    public IStatContainer Stats => stats;

    private void Awake()
    {
        brain = GetComponent<EnemyBrain>();
        abilityManager = GetComponent<AbilityManager>();
        perception = GetComponent<Perception>();
        animationHandler = GetComponentInChildren<IAnimationHandler>();
        stats = GetComponent<IStatContainer>();
        if (animationHandler == null) Debug.LogError("Enemy has no animation handler");
        if (stats == null) Debug.LogError("Enemy has no stat container");
        RegisterActions();
    }
    private void RegisterActions()
    {
        brain.AddAction(new ChaseAction(this));
        brain.AddAction(new PatrolAction(this, patrolRadius));
        foreach (var ability in GetComponents<IEnemyAttack>())
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

    public void Die()
    {
        if (IsDead)
            return;

        IsDead = true;

        brain.SetState(BrainState.Dead);
        animationHandler.SetAnimationState(CharacterAnimationState.Dead);
    }
}