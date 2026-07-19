using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private DungeonConfig config;
    [SerializeField] private int roomUnit = 3;

    private const int MaxGenerationAttempts = 10;
    private const int BossDepth = 5;
    List<Room> generatedRooms = new();

    private Dictionary<Vector2Int, Room> dungeonMap = new();
    private DungeonDecorator decorator;

    void Start()
    {
        decorator = GetComponentInChildren<DungeonDecorator>();
        StartGenerating();
    }
    void StartGenerating()
    {
        for (int attempt = 1; attempt <= MaxGenerationAttempts; attempt++)
        {
            ClearDungeon();

            if (TryGenerateDungeon())
            {
                Debug.Log($"[DungeonGenerator] Dungeon generated successfully on attempt {attempt}");
                FinishDungeon();
                return;
            }
            Debug.Log($"[DungeonGenerator] Dungeon generation failed, retrying ({attempt}/{MaxGenerationAttempts})");
        }

        Debug.LogError($"[DungeonGenerator] Dungeon generation failed after all ({MaxGenerationAttempts}) attempts");
    }

    bool TryGenerateDungeon()
    {
        Room startRoom = Instantiate(
            config.startRooms[Random.Range(0, config.startRooms.Length)],
            Vector3.zero,
            Quaternion.identity
        );

        AddStartRoom(startRoom);

        bool hasBranch = false;

        foreach (DoorSocket door in startRoom.GetUnconnectedDoors())
        {
            List<Room> branchRooms = new();

            if (GenerateBranch(door, 1, branchRooms))
            {
                hasBranch = true;
            }
            else
            {
                CreateDeadEnd(door);
            }
        }

        return hasBranch;
    }

    private void AddStartRoom(Room room)
    {
        generatedRooms.Add(room);
        RegisterRoom(room, Vector2Int.zero);
    }

    void ClearDungeon()
    {
        foreach (Room room in generatedRooms)
        {
            if (room != null)
                DestroyImmediate(room.gameObject);
        }

        generatedRooms.Clear();
        dungeonMap.Clear();
    }

    void Rollback(List<Room> rooms)
    {
        foreach (Room room in rooms)
        {

            if (room != null)
            {
                foreach (var pair in dungeonMap.Where(p => p.Value == room).ToList())
                {
                    dungeonMap.Remove(pair.Key);
                }
                generatedRooms.Remove(room);
                room.GetEntranceDoor().Disconnect();
                DestroyImmediate(room.gameObject);
            }
        }
        rooms.Clear();
    }

    bool GenerateBranch(
       DoorSocket parentDoor,
       int depth,
       List<Room> createdRooms)
    {
        if (depth >= BossDepth)
        {
            if (TrySpawnRoom(
                parentDoor,
                config.bossRooms,
                out Room boss))
            {
                createdRooms.Add(boss);
                return true;
            }

            return false;
        }

        if (!TrySpawnRoom(
            parentDoor,
            config.standardRooms,
            out Room room))
        {
            return false;
        }

        createdRooms.Add(room);

        bool hasSuccessfulChild = false;

        foreach (DoorSocket door in room.GetUnconnectedDoors())
        {
            List<Room> childRooms = new();

            if (GenerateBranch(
                door,
                depth + 1,
                childRooms))
            {
                createdRooms.AddRange(childRooms);
                hasSuccessfulChild = true;
            }
            else 
            {
                CreateDeadEnd(door);
            }
        }

        if (!hasSuccessfulChild)
        {
            Rollback(createdRooms);
            return false;
        }

        return true;
    }
    bool TrySpawnRoom(DoorSocket parentDoor, Room[] options, out Room room)
    {
        Room[] shuffled = options.OrderBy(x => Random.value).ToArray();

        foreach (Room prefab in shuffled)
        {
            // Double check the engine hasn't lost the asset reference
            if (prefab == null) continue;

            Room spawnedRoom = Instantiate(prefab, Vector3.zero, Quaternion.identity);

            Vector2Int gridPosition = SnapRoom(
                parentDoor,
                spawnedRoom,
                spawnedRoom.GetEntranceDoor()
            );

            if (CanPlaceRoom(spawnedRoom, gridPosition))
            {
                AcceptRoom(parentDoor, spawnedRoom, gridPosition);
                room = spawnedRoom; // Only assign to the out parameter on success
                return true;
            }

            // Use DestroyImmediate during generation loops to guarantee 
            // the native pointer is wiped cleanly without bleeding into the next iteration
            DestroyImmediate(spawnedRoom.gameObject);
        }

        room = null;
        Debug.Log($"[DungeonGenerator] No valid room found from door {parentDoor.name}");
        return false;
    }

    private void AcceptRoom(
        DoorSocket parentDoor,
        Room room,
        Vector2Int gridPosition)
    {
        Debug.Log($"[DungeonGenerator] Accepted room {room.name} at {room.transform.position}");

        generatedRooms.Add(room);
        RegisterRoom(room, gridPosition);

        parentDoor.Connect(room.GetEntranceDoor());
        room.GetEntranceDoor().Connect(parentDoor);
    }

    void CreateDeadEnd(DoorSocket parentDoor)
    {
        Room room = Instantiate(
            config.deadEndRoom,
            Vector3.zero,
            Quaternion.identity
        );

        Vector2Int gridPosition = SnapRoom(
            parentDoor,
            room,
            room.GetEntranceDoor()
        );

        if (CanPlaceRoom(room, gridPosition))
        {
            AcceptRoom(parentDoor, room, gridPosition);
            Debug.Log($"[DungeonGenerator] Spawned DEAD END at {room.transform.position}");
        }
        else
        {
            Destroy(room.gameObject);
            parentDoor.Disconnect();
            Debug.Log("[DungeonGenerator] Dead end rejected due to overlap");
        }
    }

    bool CanPlaceRoom(Room room, Vector2Int origin)
    {
        foreach (Vector2Int cell in GetOccupiedCells(room))
        {
            Vector2Int position = origin + cell;

            if (dungeonMap.ContainsKey(position))
                return false;
        }

        return true;
    }

    void RegisterRoom(Room room, Vector2Int origin)
    {
        Debug.Log($"{room.name} origin {origin} size {room.RoomExtent}");
        foreach (Vector2Int cell in GetOccupiedCells(room))
        {
            Vector2Int position = origin + cell;
            dungeonMap[position] = room;
        }
    }

    List<Vector2Int> GetOccupiedCells(Room room)
    {
        Vector3 worldExtent = room.transform.TransformVector(room.RoomExtent);

        Vector2Int size = new(
            Mathf.RoundToInt(Mathf.Abs(worldExtent.x) / roomUnit),
            Mathf.RoundToInt(Mathf.Abs(worldExtent.z) / roomUnit)
        );

        List<Vector2Int> cells = new();

        int offsetX = size.x / 2;
        int offsetZ = size.y / 2;

        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                cells.Add(new Vector2Int(
                    x - offsetX,
                    z - offsetZ
                ));
            }
        }

        return cells;
    }

    Vector2Int SnapRoom(
        DoorSocket parentDoor,
        Room room,
        DoorSocket childDoor)
    {
        Transform childRoom = room.transform;
        Quaternion rotation = Quaternion.LookRotation(
            -parentDoor.transform.forward,
            Vector3.up
        );
        childRoom.rotation = rotation * Quaternion.Inverse(childDoor.transform.rotation);

        Vector3 offset = childDoor.transform.position - parentDoor.transform.position;
        childRoom.position -= offset;

        return GetRoomGridPosition(room);
    }

    Vector2Int GetRoomGridPosition(Room room)
    {
        return new Vector2Int(
            Mathf.RoundToInt(room.transform.position.x / roomUnit),
            Mathf.RoundToInt(room.transform.position.z / roomUnit)
        );
    }

    void FinishDungeon()
    {
        decorator.Decorate(dungeonMap, roomUnit);
        var surfaces = FindObjectsByType<NavMeshSurface>(FindObjectsSortMode.None);

        Debug.Log($"Found {surfaces.Length} surfaces");

        foreach (NavMeshSurface surface in surfaces)
        {
            surface.BuildNavMesh();

            if (surface.navMeshData == null) Debug.LogError($"Failed to bake: {surface.gameObject.name}");
        }
    }
}