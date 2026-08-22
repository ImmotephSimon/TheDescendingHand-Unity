using System.Collections.Generic;
using UnityEngine;

public class Stairs : MonoBehaviour
{
    private enum State { Idle, TraversingDown, TraversingUp }
    private State currentState = State.Idle;

    [SerializeField] private Transform dungeonAnchor;
    [SerializeField] private TriggerDetector walkDownTrigger;
    [SerializeField] private TriggerDetector walkUpTrigger;
    [SerializeField] private List<GameObject> walls = new();

    private void OnEnable()
    {
        if (walkDownTrigger != null)
        {
            walkDownTrigger.OnEntered += OnTopEnter;
            walkDownTrigger.OnExited += OnTopExit;
        }

        if (walkUpTrigger != null)
        {
            walkUpTrigger.OnEntered += OnBottomEnter;
            walkUpTrigger.OnExited += OnBottomExit;
        }
    }

    private void OnDisable()
    {
        if (walkDownTrigger != null)
        {
            walkDownTrigger.OnEntered -= OnTopEnter;
            walkDownTrigger.OnExited -= OnTopExit;
        }

        if (walkUpTrigger != null)
        {
            walkUpTrigger.OnEntered -= OnBottomEnter;
            walkUpTrigger.OnExited -= OnBottomExit;
        }
    }

    private void OnTopEnter(IEntity other)
    {
        if (currentState != State.Idle) return;

        currentState = State.TraversingDown;
        SetWallsActive(true);
        DungeonManager.Instance.EnterDungeon(dungeonAnchor);
    }

    private void OnBottomExit(IEntity other)
    {
        if (currentState == State.TraversingDown)
        {
            currentState = State.Idle;
            SetWallsActive(false);
        }
    }

    private void OnBottomEnter(IEntity other)
    {
        if (currentState != State.Idle) return;

        currentState = State.TraversingUp;
        SetWallsActive(true);
        DungeonManager.Instance.LeaveDungeon();
    }

    private void OnTopExit(IEntity other)
    {
        if (currentState == State.TraversingUp)
        {
            currentState = State.Idle;
            SetWallsActive(false);
        }
    }

    private void SetWallsActive(bool active)
    {
        for (int i = 0; i < walls.Count; i++)
        {
            if (walls[i] != null) walls[i].SetActive(active);
        }
    }
}