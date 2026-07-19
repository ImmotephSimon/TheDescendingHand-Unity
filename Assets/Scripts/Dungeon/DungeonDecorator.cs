using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonDecorator : MonoBehaviour
{
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject doorWallPrefab;
    private int roomUnit;
    private float roomHeight;
    private Dictionary<Vector2Int, Room> dungeonMap;

    private void OnValidate()
    {
        if (wallPrefab == null) Debug.LogError("DungeonDecorator: wallPrefab is not assigned.", this);
        if (doorWallPrefab == null) Debug.LogError("DungeonDecorator: doorWallPrefab is not assigned.", this);
    }

    public void Decorate(Dictionary<Vector2Int, Room> dungeonMap, int roomUnit)
    {
        this.roomUnit = roomUnit;
        this.dungeonMap = dungeonMap;

        HashSet<string> processedConnections = new();

        foreach (Room room in dungeonMap.Values.Distinct())
        {
            foreach (DoorSocket door in room.GetConnectedDoors())
            {
                if (door == null || door.ConnectedDoor == null) continue;

                // Generate a unique, order-independent ID string for the pair
                string idA = door.GetInstanceID().ToString();
                string idB = door.ConnectedDoor.GetInstanceID().ToString();
                string connectionKey = string.Compare(idA, idB) < 0 ? $"{idA}_{idB}" : $"{idB}_{idA}";

                if (!processedConnections.Contains(connectionKey))
                {
                    processedConnections.Add(connectionKey);
                    CreateDoorFrame(
                        room,
                        door.ConnectedDoor.GetComponentInParent<Room>(),
                        door
                    );
                }
            }
        }

        foreach (Vector2Int cell in dungeonMap.Keys)
        {
            CheckWall(cell, Vector2Int.up);
            CheckWall(cell, Vector2Int.down);
            CheckWall(cell, Vector2Int.left);
            CheckWall(cell, Vector2Int.right);
        }
    }

    private void CreateDoorFrame(Room roomA, Room roomB, DoorSocket door)
    {
        GameObject obj = Instantiate(
            doorWallPrefab,
            door.transform.position,
            door.transform.rotation,
            roomA.transform
        );

        DoorController controller = obj.GetComponentInChildren<DoorController>();

        roomA.RegisterDoor(controller);
        roomB.RegisterDoor(controller);
    }

    private void CheckWall(Vector2Int cell, Vector2Int direction)
    {
        Room room = dungeonMap[cell];

        bool isBoundary =
            !dungeonMap.TryGetValue(cell + direction, out Room neighbour) ||
            neighbour != room;

        if (!isBoundary)
            return;

        float roomBottom = -room.RoomExtent.y / 2 + roomUnit / 2;

        for (float y = roomBottom; y < room.RoomExtent.y / 2; y += roomUnit)
        {
            if (HasDoorOnEdge(cell, direction, room, y))
                continue;

            CreateWallForEdge(cell, direction, room, y);
        }
    }
    private bool HasDoorOnEdge(Vector2Int cell, Vector2Int direction, Room room, float y)
    {
        if (!dungeonMap.TryGetValue(cell + direction, out Room neighbour))
            return false;

        foreach (DoorSocket door in room.GetConnectedDoors())
        {
            Debug.Log($"door width {door.Width}");
            if (door.ConnectedDoor == null)
                continue;

            if (!ReferenceEquals(door.ConnectedDoor.GetComponentInParent<Room>(), neighbour))
            {
                Debug.Log($"Mismatch socket room {door.ConnectedDoor.GetComponentInParent<Room>().name} vs neighbour {neighbour.name}");
                continue;
            }

            Vector3 wallPosition = GridToWorldPosition(cell, direction, y);

            float distance = direction.x != 0
                ? Mathf.Abs(door.transform.position.z - wallPosition.z)
                : Mathf.Abs(door.transform.position.x - wallPosition.x);
            if (distance <= door.Width / 2f)
                return true;
        }

        return false;
    }

    private void CreateWallForEdge(Vector2Int cell, Vector2Int direction, Room room, float y)
    {
        Vector3 pos = GridToWorldPosition(cell, direction, y);

        Quaternion rotation = direction.x != 0
            ? Quaternion.identity
            : Quaternion.Euler(0, 90, 0);

        Instantiate(wallPrefab, pos, rotation, room.transform);
    }


    private Vector3 GridToWorldPosition(Vector2Int cell, Vector2Int direction, float y)
    {
        float half = roomUnit / 2f;

        Vector3 pos = new Vector3(
            cell.x * roomUnit + half,
            y,
            cell.y * roomUnit + half
        );

        

        if (direction == Vector2Int.right)
            pos.x += half;
        else if (direction == Vector2Int.left)
            pos.x -= half;
        else if (direction == Vector2Int.up)
            pos.z += half;
        else if (direction == Vector2Int.down)
            pos.z -= half;

        return pos;
    }
}