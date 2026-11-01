// ───────── Shared asmdef ─────────
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Stairs : NetworkBehaviour
{
    [SerializeField] private Transform dungeonAnchor;
    [SerializeField] private TriggerDetector walkDownTrigger;
    [SerializeField] private TriggerDetector walkUpTrigger;
    [SerializeField] private List<GameObject> walls = new();

    // Server-side registry so DungeonManager can hook in regardless of start order.
    public static event Action<Stairs> OnDescend;
    public static event Action<Stairs> OnAscend;


    private enum State { Idle, TraversingDown, TraversingUp }
    private State _state = State.Idle;


    private readonly SyncVar<bool> _wallsActive = new();

    public Transform DungeonAnchor => dungeonAnchor;

    // ── Networking lifecycle ──

    public override void OnStartNetwork()
    {
        _wallsActive.OnChange += OnWallsChanged;
    }

    public override void OnStopNetwork()
    {
        _wallsActive.OnChange -= OnWallsChanged;
    }

    public override void OnStartClient()
    {
        // Pure clients don't run trigger logic at all.
        if (!IsServerInitialized)
        {
            if (walkDownTrigger != null) walkDownTrigger.enabled = false;
            if (walkUpTrigger != null) walkUpTrigger.enabled = false;
        }

        ApplyWalls(_wallsActive.Value);
    }

    public override void OnStartServer()
    {
        walkDownTrigger.OnEntered += OnTopEnter;
        walkDownTrigger.OnExited += OnTopExit;
        walkUpTrigger.OnEntered += OnBottomEnter;
        walkUpTrigger.OnExited += OnBottomExit;
    }

    public override void OnStopServer()
    {
        walkDownTrigger.OnEntered -= OnTopEnter;
        walkDownTrigger.OnExited -= OnTopExit;
        walkUpTrigger.OnEntered -= OnBottomEnter;
        walkUpTrigger.OnExited -= OnBottomExit;

    }

    // ── Walls (runs on server and clients via SyncVar) ──

    private void OnWallsChanged(bool prev, bool next, bool asServer) => ApplyWalls(next);

    private void ApplyWalls(bool active)
    {
        for (int i = 0; i < walls.Count; i++)
            if (walls[i] != null) walls[i].SetActive(active);
    }


    // ── Server-only logic ──
    [Server]
    private void OnTopEnter(NetworkObject nob)
    {
        if (_state != State.Idle) return;
        BeginTraversal(State.TraversingDown, OnDescend);
    }

    [Server]
    private void OnBottomEnter(NetworkObject nob)
    {
        if (_state != State.Idle) return;
        BeginTraversal(State.TraversingUp, OnAscend);
    }

    [Server]
    private void OnBottomExit(NetworkObject nob)
    {
        if (_state == State.TraversingDown) EndTraversal();
    }

    [Server]
    private void OnTopExit(NetworkObject nob)
    {
        if (_state == State.TraversingUp) EndTraversal();
    }

    [Server]
    private void BeginTraversal(State next, Action<Stairs> request)
    {
        _state = next;
        _wallsActive.Value = true;

        try
        {
            request?.Invoke(this);
        }
        catch (Exception e)
        {
            // Don't leave the stairs stuck if the dungeon side fails.
            Debug.LogException(e);
            EndTraversal();
        }
    }

    [Server]
    private void EndTraversal()
    {
        _state = State.Idle;
        _wallsActive.Value = false;
    }

}