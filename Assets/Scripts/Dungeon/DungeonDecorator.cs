using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonDecorator : MonoBehaviour
{
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject doorFramePrefab;
    [SerializeField] private float wallThickness = 0.2f;
    private int roomUnit;
    private float roomHeight;
    private Dictionary<Vector2Int, Room> dungeonMap;

    public void Decorate(Dictionary<Vector2Int, Room> dungeonMap, int roomUnit)
    {
        this.roomUnit = roomUnit;
        this.dungeonMap = dungeonMap;

        foreach (Room room in dungeonMap.Values.Distinct())
        {
            foreach (DoorSocket door in room.GetConnectedDoors())
            {
                if (door == null)
                {
                    Debug.LogError($"{room.name} has a destroyed DoorSocket reference.");
                    continue;
                }
                if (door.GetEntityId() < door.ConnectedDoor.GetEntityId())
                {
                    CreateDoorFrame(door);
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

    private void CreateDoorFrame(DoorSocket door)
    {
        Instantiate(
            doorFramePrefab,
            door.transform.position,
            door.transform.rotation,
            transform
        );
    }

    private void CheckWall(Vector2Int cell, Vector2Int direction)
    {
        Room room = dungeonMap[cell];

        if (!dungeonMap.TryGetValue(cell + direction, out Room neighbour))
        {
            CreateWallForEdge(cell, direction, room);
        }
        else if (neighbour != room && !HasDoorBetween(room, neighbour))
        {
            CreateWallForEdge(cell, direction, room);
        }
    }

    private bool HasDoorBetween(Room a, Room b)
    {
        foreach (DoorSocket door in a.GetConnectedDoors())
        {
            if (door.ConnectedDoor != null &&
                door.ConnectedDoor.GetComponentInParent<Room>() == b)
            {
                return true;
            }
        }

        return false;
    }

    private void CreateWallSegment(Vector3 pos, Vector2Int direction, Room room, float length)
    {
        GameObject wall = Instantiate(
            wallPrefab,
            pos,
            Quaternion.identity,
            transform
        );

        Vector3 scale = GetWallScale(direction, room);

        if (direction.x != 0)
            scale.z *= length;
        else
            scale.x *= length;

        wall.transform.localScale = scale;
    }

    private void CreateWallForEdge(Vector2Int cell, Vector2Int direction, Room room)
    {
        Vector3 pos = GridToWorldEdge(cell, direction, room);
        pos += new Vector3(1.5f, 0, 1.5f);
        GameObject wall = Instantiate(
            wallPrefab,
            pos,
            Quaternion.identity,
            transform
        );
        wall.transform.localScale = GetWallScale(direction, room);
    }


    private Vector3 GetWallScale(Vector2Int direction, Room room)
    {
        if (direction.x != 0)
        {
            return new Vector3(wallThickness, room.RoomExtent.y, roomUnit);
        }
        else
        {
            return new Vector3(roomUnit, room.RoomExtent.y, wallThickness);
        }
    }

    private Vector3 GridToWorldEdge(Vector2Int cell, Vector2Int direction, Room room)
    {
        Vector3 pos = new Vector3(
            cell.x * roomUnit,
            -room.RoomExtent.y / 2f,
            cell.y * roomUnit
        );

        float half = roomUnit / 2f;

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