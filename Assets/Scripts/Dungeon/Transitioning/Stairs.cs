using System.Collections.Generic;
using UnityEngine;

public class Stairs : MonoBehaviour
{
    private enum State { Idle, TraversingDown, TraversingUp }
    private State currentState = State.Idle;

    [SerializeField] private Transform dungeonAnchor;
    [SerializeField] private StairsTrigger walkDownTrigger;
    [SerializeField] private StairsTrigger walkUpTrigger;
    [SerializeField] private List<GameObject> walls = new();

    private void OnEnable()
    {
        if (walkDownTrigger != null)
        {
            walkDownTrigger.OnPlayerEnter += OnTopEnter;
            walkDownTrigger.OnPlayerExit += OnTopExit;
        }

        if (walkUpTrigger != null)
        {
            walkUpTrigger.OnPlayerEnter += OnBottomEnter;
            walkUpTrigger.OnPlayerExit += OnBottomExit;
        }
    }

    private void OnDisable()
    {
        if (walkDownTrigger != null)
        {
            walkDownTrigger.OnPlayerEnter -= OnTopEnter;
            walkDownTrigger.OnPlayerExit -= OnTopExit;
        }

        if (walkUpTrigger != null)
        {
            walkUpTrigger.OnPlayerEnter -= OnBottomEnter;
            walkUpTrigger.OnPlayerExit -= OnBottomExit;
        }
    }

    private void OnTopEnter()
    {
        if (currentState != State.Idle) return;

        currentState = State.TraversingDown;
        SetWallsActive(true);
        DungeonManager.Instance.EnterDungeon(dungeonAnchor);
    }

    private void OnBottomExit()
    {
        if (currentState == State.TraversingDown)
        {
            currentState = State.Idle;
            SetWallsActive(false);
        }
    }

    private void OnBottomEnter()
    {
        if (currentState != State.Idle) return;

        currentState = State.TraversingUp;
        SetWallsActive(true);
        DungeonManager.Instance.LeaveDungeon();
    }

    private void OnTopExit()
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