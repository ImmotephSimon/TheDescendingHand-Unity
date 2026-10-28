using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] private float roomUnit = 3f;
    [SerializeField] private BoxCollider bounds;

    private DoorSocket[] doorSockets;
    private DoorSocket entranceDoor;
    private readonly List<HingeController> _doors = new();
    public List<HingeController> Doors => _doors;
    public Vector3 RoomExtent => bounds.size;
    
    public event Action Entered;
    public Collider Bounds => bounds;
    private bool entered = false;

    private void OnValidate()
    {
        UpdateDoors();

        if (bounds == null)
            return;

        CheckGridAlignment();
    }

    public void UpdateDoors()
    {
        doorSockets = GetComponentsInChildren<DoorSocket>();

        foreach (DoorSocket doorSocket in doorSockets)
        {
            doorSocket.UpdatePosition(bounds, roomUnit);
        }
    }

    private void Awake()
    {
        doorSockets = GetComponentsInChildren<DoorSocket>();
    }

    private void CheckGridAlignment()
    {
        float x = bounds.size.x / roomUnit;
        float z = bounds.size.z / roomUnit;

        bool invalidX = !Mathf.Approximately(x, Mathf.Round(x));
        bool invalidZ = !Mathf.Approximately(z, Mathf.Round(z));

        if (invalidX || invalidZ)
        {
            Debug.LogError(
                $"Room '{gameObject.name}' has invalid extent {bounds.size}. " +
                $"roomUnit={roomUnit}. " +
                $"X={x:F2} tiles, Z={z:F2} tiles. " +
                $"Collider='{bounds.name}'.",
                this);
        }
    }


    public DoorSocket GetEntranceDoor()
    {
        if (entranceDoor == null)
        {
            List<DoorSocket> availableDoors = GetUnconnectedDoors();

            entranceDoor = availableDoors.Count > 0
                ? availableDoors[UnityEngine.Random.Range(0, availableDoors.Count)]
                : availableDoors.FirstOrDefault();
        }
        return entranceDoor;
    }

    public List<DoorSocket> GetUnconnectedDoors()
    {
        return doorSockets.Where(d => !d.IsConnected).ToList();
    }

    public List<DoorSocket> GetConnectedDoors()
    {
        return doorSockets.Where(d => d.IsConnected).ToList();
    }

    private void OnDoorEntered()
    {
        if (entered)
            return;

        entered = true;
        foreach (HingeController door in _doors)
        {
            door.PlayerInteracted -= OnDoorEntered;
        }
        Entered?.Invoke();
    }

    

    public void RegisterDoor(HingeController door)
    {
        if (door == null)
            return;

        _doors.Add(door);
        door.PlayerInteracted += OnDoorEntered;
    }
    public Vector2Int GetRoomGridPosition()
    {

        return new Vector2Int(
            Mathf.RoundToInt(transform.position.x / roomUnit),
            Mathf.RoundToInt(transform.position.z / roomUnit)
        );
    }
}