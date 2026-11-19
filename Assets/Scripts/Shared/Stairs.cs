using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Stairs : NetworkBehaviour
{
    [SerializeField] private Transform dungeonAnchor;
    [SerializeField] private List<GameObject> walls = new();
    [SerializeField] private TriggerDetector staircaseVolume;
    [SerializeField] private TriggerDetector topLanding;
    [SerializeField] private TriggerDetector bottomLanding;

    public static event Action<Stairs, NetworkObject> OnDescend;
    public static event Action<Stairs, NetworkObject> OnAscend;

    private enum State
    {
        Idle,
        GoingDown,
        GoingUp
    }

    private enum Side
    {
        None,
        Top,
        Bottom
    }

    private State _state = State.Idle;
    private NetworkObject _traveller;

    // Idle only: the landing a player was last standing on.
    private Side _origin = Side.None;
    private NetworkObject _originPlayer;

    // Travelling only: true once he has been in the volume clear of both landings.
    private bool _clearOfLandings;

    // Last physics time a Stay was seen on each landing.
    private float _topSeen = float.NegativeInfinity;
    private float _bottomSeen = float.NegativeInfinity;

    private readonly SyncVar<bool> _wallsActive = new();

    public Transform DungeonAnchor => dungeonAnchor;

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
        ApplyWalls(_wallsActive.Value);

        // Only the server's physics drives the state machine.
        if (!IsServerInitialized)
        {
            staircaseVolume.enabled = false;
            topLanding.enabled = false;
            bottomLanding.enabled = false;
        }
    }

    public override void OnStartServer()
    {
        staircaseVolume.OnStay += OnVolumeStay;
        topLanding.OnStay += OnTopLandingStay;
        bottomLanding.OnStay += OnBottomLandingStay;
    }

    public override void OnStopServer()
    {
        staircaseVolume.OnStay -= OnVolumeStay;
        topLanding.OnStay -= OnTopLandingStay;
        bottomLanding.OnStay -= OnBottomLandingStay;
    }

    private void OnWallsChanged(bool prev, bool next, bool asServer)
    {
        ApplyWalls(next);
    }

    private void ApplyWalls(bool active)
    {
        for (int i = 0; i < walls.Count; i++)
        {
            if (walls[i] != null)
            {
                walls[i].SetActive(active);
            }
        }
    }

    // ---------------------------------------------------------------
    // Volume
    // Idle: a player who came off a landing and is now clear of it starts a trip.
    // Travelling: being here clear of both landings arms the landings.
    // ---------------------------------------------------------------
    [Server]
    private void OnVolumeStay(NetworkObject player)
    {
        if (!IsPlayer(player))
            return;

        RecoverIfTravellerGone();

        if (_state == State.Idle)
        {
            TryStartTrip(player);
            return;
        }

        if (player == _traveller && !OnTop() && !OnBottom())
            _clearOfLandings = true;
    }

    [Server]
    private void TryStartTrip(NetworkObject player)
    {
        if (player != _originPlayer)
            return;

        // Still standing on the landing he came from: not started yet.
        if (_origin == Side.Top && !OnTop())
            StartGoingDown(player);
        else if (_origin == Side.Bottom && !OnBottom())
            StartGoingUp(player);
    }

    [Server]
    private void StartGoingDown(NetworkObject player)
    {
        BeginTrip(State.GoingDown, player);
        OnDescend?.Invoke(this, player);
    }

    [Server]
    private void StartGoingUp(NetworkObject player)
    {
        BeginTrip(State.GoingUp, player);
        OnAscend?.Invoke(this, player);
    }

    [Server]
    private void BeginTrip(State state, NetworkObject player)
    {
        _origin = Side.None;
        _originPlayer = null;
        _state = state;
        _traveller = player;
        _clearOfLandings = false;
        _wallsActive.Value = true;
    }

    // ---------------------------------------------------------------
    // Landings
    // Idle: remember which landing he is on.
    // Travelling: once armed, either he arrived or he turned back.
    // ---------------------------------------------------------------
    [Server]
    private void OnTopLandingStay(NetworkObject player)
    {
        if (!IsPlayer(player))
            return;

        _topSeen = Time.fixedTime;
        RecoverIfTravellerGone();

        if (_state == State.Idle)
        {
            Remember(Side.Top, player);
            return;
        }

        if (player != _traveller || !_clearOfLandings)
            return;

        if (_state == State.GoingUp)
            FinishAscent();
        else
            CancelDescent();
    }

    [Server]
    private void OnBottomLandingStay(NetworkObject player)
    {
        if (!IsPlayer(player))
            return;

        _bottomSeen = Time.fixedTime;
        RecoverIfTravellerGone();

        if (_state == State.Idle)
        {
            Remember(Side.Bottom, player);
            return;
        }

        if (player != _traveller || !_clearOfLandings)
            return;

        if (_state == State.GoingDown)
            FinishDescent();
        else
            CancelAscent();
    }

    // Arrived: the start event already fired, so just release.
    [Server] private void FinishAscent() => ReturnToIdle();
    [Server] private void FinishDescent() => ReturnToIdle();

    // Turned back: undo the start event, then release.
    [Server]
    private void CancelDescent()
    {
        NetworkObject player = _traveller;
        OnAscend?.Invoke(this, player);
        ReturnToIdle();
    }

    [Server]
    private void CancelAscent()
    {
        NetworkObject player = _traveller;
        OnDescend?.Invoke(this, player);
        ReturnToIdle();
    }

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------
    private bool OnTop() => IsFresh(_topSeen);
    private bool OnBottom() => IsFresh(_bottomSeen);

    // A landing counts as occupied if it saw a Stay within about 1.5 physics steps.
    private static bool IsFresh(float seenAt)
    {
        return Time.fixedTime - seenAt <= Time.fixedDeltaTime * 1.5f;
    }

    private void Remember(Side side, NetworkObject player)
    {
        _origin = side;
        _originPlayer = player;
    }

    [Server]
    private void ReturnToIdle()
    {
        _state = State.Idle;
        _traveller = null;
        _clearOfLandings = false;
        _wallsActive.Value = false;
    }

    [Server]
    private void RecoverIfTravellerGone()
    {
        if (_state != State.Idle && (_traveller == null || !_traveller.IsSpawned))
            ReturnToIdle();
    }

    private static bool IsPlayer(NetworkObject nob)
    {
        return nob != null && nob.Owner.IsValid && nob.Owner.FirstObject == nob;
    }
}