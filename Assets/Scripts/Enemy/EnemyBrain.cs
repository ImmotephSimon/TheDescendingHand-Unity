using System.Collections.Generic;
using UnityEngine;

public class EnemyBrain : MonoBehaviour, ISpawnable
{
    private List<IEnemyAction> actions = new();
    private IEnemyAction currentAction;
    private bool IsStunned;

    public void AddAction(IEnemyAction action)
    {
        actions.Add(action);
    }

    void Update()
    {
        if (IsStunned) return;

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

}