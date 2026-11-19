
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonWallsNetwork : NetworkBehaviour
{
    public readonly SyncList<WallData> Walls = new();
    public readonly SyncList<WallData> DoorFrames = new();

    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject doorFramePrefab;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        Walls.OnChange += OnWallsChanged;
        DoorFrames.OnChange += OnDoorFramesChanged;

        if (!IsServerInitialized)
        {
            foreach (WallData wall in Walls)
                BuildDecoration(wall, wallPrefab);

            foreach (WallData frame in DoorFrames)
                BuildDecoration(frame, wallPrefab);
        }
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        Walls.OnChange -= OnWallsChanged;
        DoorFrames.OnChange -= OnDoorFramesChanged;
    }

    private void OnWallsChanged(
        SyncListOperation op, int index,
        WallData oldItem, WallData newItem, bool asServer)
    {
        if (!asServer && op == SyncListOperation.Add)
            BuildDecoration(newItem, wallPrefab);
    }

    private void OnDoorFramesChanged(
        SyncListOperation op, int index,
        WallData oldItem, WallData newItem, bool asServer)
    {
        if (!asServer && op == SyncListOperation.Add)
            BuildDecoration(newItem, doorFramePrefab);
    }

    private void BuildDecoration(WallData data, GameObject prefab)
    {
        Instantiate(prefab, data.Position, data.Rotation, transform);
    }

    public void SetWalls(IEnumerable<Transform> walls)
    {
        Walls.AddRange(walls.Select(w => new WallData
        {
            Position = w.position,
            Rotation = w.rotation
        }));
    }

    public void SetDoorFrames(IEnumerable<Transform> frames)
    {
        DoorFrames.AddRange(frames.Select(f => new WallData
        {
            Position = f.position,
            Rotation = f.rotation
        }));
    }
}

public struct WallData
{
    public Vector3 Position;
    public Quaternion Rotation;
}
