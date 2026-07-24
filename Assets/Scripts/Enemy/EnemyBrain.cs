using System;
using System.Collections.Generic;
using UnityEngine;

public enum BrainState
{
    Active,
    Stunned,
    Dead
}

public class EnemyBrain : MonoBehaviour, ISpawnable
{


    private List<IEnemyAction> actions = new();
    private IEnemyAction currentAction;
    private bool IsStopped;
    private BrainState _state = BrainState.Active;

    public void AddAction(IEnemyAction action)
    {
        actions.Add(action);
    }

    void Update()
    {
        // Dead or stunned
        if (_state != BrainState.Active) return;

        if (currentAction != null && !currentAction.CanBeInterrupted)
        {
            currentAction.UpdateAction();
            return;
        }
        IEnemyAction bestAction = null;
        float bestScore = float.MinValue;

        foreach (var action in actions)
        {
            if (!action.IsAvailable())
                continue;

            float score = action.GetPriority();

            if (score > bestScore)
            {
                bestScore = score;
                bestAction = action;
            }
        }

        if (bestAction != currentAction)
        {
            currentAction?.StopAction();

            currentAction = bestAction;

            currentAction?.StartAction();
        }

        currentAction?.UpdateAction();
    }
    public void OnSpawnComplete()
    {
        // Optional initialization after spawning
    }

    internal void SetState(BrainState state)
    {
        _state = state;
    }
}