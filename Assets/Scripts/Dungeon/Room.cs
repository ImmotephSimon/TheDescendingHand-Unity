using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] private float roomUnit = 3f;

    private DoorSocket[] doorSockets;
    private BoxCollider bounds;
    private DoorSocket entranceDoor;

    public Vector3 RoomExtent => bounds.size;
    
    public event Action Entered;
    public Collider Bounds => bounds;
    private bool entered = false;

    private void OnValidate()
    {
        UpdateDoors();
    }

    public void UpdateDoors()
    {
        bounds = GetComponentInChildren<BoxCollider>();
        doorSockets = GetComponentsInChildren<DoorSocket>();

        foreach (DoorSocket doorSocket in doorSockets)
        {
            doorSocket.UpdatePosition(bounds, roomUnit);
        }

        
    }

    private void Awake()
    {
        doorSockets = GetComponentsInChildren<DoorSocket>();
        bounds = GetComponentInChildren<BoxCollider>();
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
        foreach (DoorController door in doors)
        {
            door.PlayerEntered -= OnDoorEntered;
        }
        Entered?.Invoke();
    }

    private readonly List<DoorController> doors = new();

    public void RegisterDoor(DoorController door)
    {
        if (door == null)
            return;

        doors.Add(door);
        door.PlayerEntered += OnDoorEntered;
    }
}