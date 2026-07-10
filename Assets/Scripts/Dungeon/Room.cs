using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : MonoBehaviour
{
    private DoorSocket[] doors;
    private BoxCollider bounds;
    private DoorSocket entranceDoor;

    public Vector3 RoomExtent => bounds.size;
    [SerializeField] private float roomUnit = 3f;


    private void OnValidate()
    {
        UpdateDoors();
    }

    public void UpdateDoors()
    {
        bounds = GetComponentInChildren<BoxCollider>();
        doors = GetComponentsInChildren<DoorSocket>();

        foreach (DoorSocket door in doors)
        {
            door.UpdatePosition(bounds, roomUnit);
        }
    }

    private void Awake()
    {
        doors = GetComponentsInChildren<DoorSocket>();
        bounds = GetComponentInChildren<BoxCollider>();
    }


    public DoorSocket GetEntranceDoor()
    {
        if (entranceDoor == null)
        {
            List<DoorSocket> availableDoors = GetUnconnectedDoors();

            entranceDoor = availableDoors
                .Where(d => d.Type == DoorSocket.DoorType.Entrance)
                .OrderBy(x => Random.value)
                .FirstOrDefault()
                ?? availableDoors.FirstOrDefault();
        }
        return entranceDoor;
    }

    public List<DoorSocket> GetUnconnectedDoors()
    {
        return doors.Where(d => !d.IsConnected).ToList();
    }

    public List<DoorSocket> GetConnectedDoors()
    {
        return doors.Where(d => d.IsConnected).ToList();
    }


}