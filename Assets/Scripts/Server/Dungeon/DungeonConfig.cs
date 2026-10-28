using UnityEngine;

[CreateAssetMenu(fileName = "DungeonConfig", menuName = "Dungeon/Dungeon Config")]
public class DungeonConfig : ScriptableObject
{
    public Room[] startRooms;
    public Room[] standardRooms;
    public Room[] bossRooms;
    public Room deadEndRoom;
}